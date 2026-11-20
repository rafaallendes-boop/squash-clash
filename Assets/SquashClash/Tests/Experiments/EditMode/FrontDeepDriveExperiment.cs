using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Experiments.EditMode
{
    // Front deep drive (20 nov). Explicit experiment, separate from regression.
    // Replays the PO's applied human strokes of 20 nov (build fb80015) with P2-Analytic, from the recorded contact and
    // launch velocity, and rebuilds the same velocity from the recorded request (aim, P, causal vectors) through the
    // product pipeline (pace -> causal -> legal bound -> intention -> solver) to prove the replay is exact.
    // Classifies each flight and maps, per stroke, the front-wall x that would have been clean at the same height
    // and speed. Inputs are the PO's ignored metrics (SC_FDD_METRICS); outputs go to ignored Logs.
    public sealed class FrontDeepDriveExperiment
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private static readonly string[] Inputs = { "metrics-20261120T102118232-", "metrics-20261120T103647667-" };
        private static string F(object v) => v is float f ? f.ToString("R", C) : v is double d ? d.ToString("R", C) : Convert.ToString(v, C);

        [Serializable] private sealed class Line { public string kind; public string payload; }

        internal readonly struct Stroke
        {
            internal readonly string File; internal readonly ShotRecord Shot; internal readonly CausalShotTrace Trace;
            internal Stroke(string file, ShotRecord shot, CausalShotTrace trace) { File = file; Shot = shot; Trace = trace; }
        }

        // Flight classes (spec 2026-11-20-front-deep-drive-design, section 2).
        internal sealed class Flight
        {
            internal string Class = "", Cause = "", FirstSurface = "";
            internal bool Success, SideBeforeBounce;
            internal double MaxZ, X1 = double.NaN, Z1 = double.NaN, X2 = double.NaN, Z2 = double.NaN, SideZ = double.NaN;
        }

        internal static Flight Classify(Vector3 from, Vector3 velocity, CourtDimensions court, PhysicsParams physics, IBallSimulationFactory factory, float footprint)
        {
            var flight = new Flight();
            double back = court.Length * 2d / 3d;
            var rules = new PointRules(court, footprint); rules.Begin(PlayerId.Human, PlayerId.Rival);
            bool first = true, front = false, reached = false, sideBeforeBack = false; int floors = 0;
            using (var simulation = factory.Create(physics, court))
            {
                simulation.Reset(new BallState { Position = from, Velocity = velocity });
                var contacts = new List<ContactEvent>(4);
                for (int step = 1; step <= physics.StepHz * 10 && !rules.Closed; step++)
                {
                    contacts.Clear(); simulation.Step(contacts);
                    foreach (var hit in contacts)
                    {
                        if (rules.Closed) break;
                        if (!hit.Persistent && first) { first = false; flight.FirstSurface = hit.Surface.ToString(); front = hit.Surface == SurfaceId.FrontWall; }
                        if (!hit.Persistent && front && (hit.Surface == SurfaceId.LeftWall || hit.Surface == SurfaceId.RightWall))
                        {
                            if (floors == 0 && !flight.SideBeforeBounce) { flight.SideBeforeBounce = true; flight.SideZ = hit.Point.z; }
                            if (!reached && hit.Point.z < back) sideBeforeBack = true;
                        }
                        if (!hit.Persistent && hit.Surface == SurfaceId.Floor)
                        {
                            floors++;
                            if (floors == 1) { flight.X1 = hit.Point.x; flight.Z1 = hit.Point.z; }
                            if (floors == 2) { flight.X2 = hit.Point.x; flight.Z2 = hit.Point.z; }
                        }
                        rules.OnBallContact(hit);
                    }
                    if (front && floors < 2 && !rules.Closed) { flight.MaxZ = Math.Max(flight.MaxZ, simulation.State.Position.z); if (simulation.State.Position.z >= back) reached = true; }
                    if (!rules.Closed && simulation.State.Position.y > court.WallHeight) rules.OnOverTop();
                }
            }
            flight.Cause = rules.Cause.ToString();
            if (!front) flight.Class = flight.FirstSurface == "LeftWall" || flight.FirstSurface == "RightWall" ? "side-first" : "out";
            else if (rules.Cause != PointEndCause.DoubleBounce) flight.Class = "out";
            else if (flight.SideBeforeBounce) flight.Class = "side-before-bounce";
            else if (!reached) flight.Class = "short";
            else if (sideBeforeBack) flight.Class = "side-before-back";
            else flight.Class = "clean";
            flight.Success = flight.Class == "clean" || flight.Class == "side-before-back";
            return flight;
        }

        // Intention from the request only (spec section 3): cross when the unfolded line through the aim reaches the far
        // half at the back wall (the ShotIntentSolver xStar rule); deep when P > 0.5 and the requested aim y >= 0.80 m.
        internal static bool IntendedCross(Vector3 from, Vector2 aim, CourtDimensions court)
        {
            double xStar = Math.Max(court.Width / 10d, Math.Min(9 * court.Width / 10d, from.x + ((double)aim.x - from.x) * (from.z + court.Length) / from.z));
            double half = court.Width / 2d;
            return (from.x - half) * (xStar - half) < 0;
        }
        internal static bool IntendedDeep(float power, Vector2 aim) => power > .5f && aim.y >= .80f;
        internal static bool Front(Vector3 from, CourtDimensions court) => from.z < court.Length - court.ShortLineNearBackEdgeFromBack;

        internal static List<Stroke> Load(string dir, List<string> hashes)
        {
            var strokes = new List<Stroke>();
            foreach (var prefix in Inputs)
            {
                var files = Directory.GetFiles(dir, prefix + "*.jsonl"); Assert.That(files.Length, Is.EqualTo(1), prefix);
                string path = files[0];
                using (var sha = SHA256.Create()) hashes.Add(Path.GetFileName(path) + "\t" + BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant());
                var traces = new Dictionary<string, CausalShotTrace>();
                foreach (var text in File.ReadAllLines(path))
                {
                    var line = JsonUtility.FromJson<Line>(text);
                    if (line.kind == "shot-causal") { var t = JsonUtility.FromJson<CausalShotTrace>(line.payload); traces[t.attemptId + "/" + t.contactStep] = t; }
                    if (line.kind != "shot") continue;
                    var shot = JsonUtility.FromJson<ShotRecord>(line.payload);
                    if (shot.Reason != "applied") continue;
                    // The causal trace is written while solving, before the shot line; the last one for the contact is the applied one.
                    Assert.That(traces.TryGetValue(shot.AttemptId + "/" + shot.ContactStep, out var trace), Is.True, shot.AttemptId);
                    strokes.Add(new Stroke(Path.GetFileName(path).Substring(8, 15), shot, trace));
                }
            }
            return strokes;
        }

        // The product pipeline of CausalQuality.Solve from the recorded vectors (the causal reference itself is not logged).
        internal static (CausalInput input, ShotPaceResult pace, ShotSolution solution, ShotIntentInfo info) Rebuild(RallyContext ctx, Stroke s)
        {
            var shot = s.Shot; var contact = new Vector3(shot.ContactX, shot.ContactY, shot.ContactZ); var aim = new Vector2(shot.AimX, shot.AimY);
            var settings = CausalSettings.From(ctx.Config.Source);
            var pace = ShotPace.Requested(shot.Power, contact, aim, ctx.Physics.BallRadius, ctx.Config.Source);
            var input = CausalQuality.BoundToLegalFront(CausalQuality.Apply(aim, pace.Speed, s.Trace.vectors, settings), aim, ctx.Court, ctx.Physics.BallRadius);
            var solution = ShotIntentSolver.Solve(new ShotRequest(contact, input.Aim, shot.Power), aim, input.Speed, ctx.VCap, ctx.Pace, ctx.Court, ctx.Factory, ctx.Physics,
                ShotIntentSolver.DefaultFootprintRadius, out var info);
            return (input, pace, solution, info);
        }

        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void WriteBaseline()
        {
            string run = Environment.GetEnvironmentVariable("SC_FDD_RUN");
            Assert.That(run == "baseline01" || run == "baseline02", Is.True, "Use a predeclared run ID.");
            string metrics = Environment.GetEnvironmentVariable("SC_FDD_METRICS");
            Assert.That(Directory.Exists(metrics), Is.True, "SC_FDD_METRICS must name the PO metrics folder.");
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            Assert.That(Git(root, "status --porcelain"), Is.Empty, "Commit the experiment before running.");
            var preset = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
            string dir = Path.Combine(root, "Logs/experiments/front-deep-drive/" + run);
            Assert.That(Directory.Exists(dir), Is.False, "Never overwrite evidence.");
            Directory.CreateDirectory(dir);
            var hashes = new List<string>();
            var strokes = Load(metrics, hashes);
            File.WriteAllText(Path.Combine(dir, "source.tsv"), "head\tdefaults\tkeys\tsimulationKey\trallyParams\tunity\n" + Git(root, "rev-parse HEAD").Trim() + "\t" + preset.Hash
                + "\t" + preset.Ordered.Count + "\t" + ctx.Config.SimulationKey + "\t" + ctx.Params.Hash + "\t" + Application.unityVersion + "\n" + string.Join("\n", hashes) + "\n", Utf8);
            var court = ctx.Court; var physics = ctx.Physics; float footprint = ShotIntentSolver.DefaultFootprintRadius;
            using (var w = new StreamWriter(Path.Combine(dir, "replay.tsv"), false, Utf8))
            {
                w.WriteLine("file\tlaunch\tstep\tside\tresult\tP\tcx\tcy\tcz\taimX\taimY\tliftedY\tclamped\tlift\tsumLateral\ttuX\ttuY\tspeed\tlowered\tintentReason\tevaluations\texact\t"
                    + "front\tcross\tdeep\tclass\tsuccess\tcause\tsideZ\tx1\tz1\tx2\tz2\tmaxZ\tprimClass\tprimSuccess\tprimTuX\tprimTuY\tplainClass\tplainSuccess\t"
                    + "windowLo\twindowHi\twindowCount\tcontactInWindow\toutsideBy\tslopeIn");
                foreach (var s in strokes)
                {
                    var shot = s.Shot; var contact = new Vector3(shot.ContactX, shot.ContactY, shot.ContactZ); var aim = new Vector2(shot.AimX, shot.AimY);
                    var (input, pace, solution, info) = Rebuild(ctx, s);
                    var recorded = new Vector3(shot.VelocityX, shot.VelocityY, shot.VelocityZ);
                    bool exact = solution.Solved && solution.Velocity.x == recorded.x && solution.Velocity.y == recorded.y && solution.Velocity.z == recorded.z && solution.TargetUsed.x == shot.TargetUsedX && solution.TargetUsed.y == shot.TargetUsedY;
                    var flight = Classify(contact, recorded, court, physics, ctx.Factory, footprint);
                    // The intention layer's contribution: the primitive on the same composed request (pre-A2 behaviour).
                    var primitive = ShotSolver.Solve(new ShotRequest(contact, input.Aim, shot.Power), input.Speed, input.Speed, ctx.VCap, physics.Gravity, physics.BallRadius, court, ctx.Factory, physics);
                    var prim = primitive.Solved ? Classify(contact, primitive.Velocity, court, physics, ctx.Factory, footprint) : new Flight { Class = "unsolved" };
                    // The causal layer's contribution: the same intention without causal vectors (requested aim and pace speed).
                    var plainSolution = ShotIntentSolver.Solve(new ShotRequest(contact, aim, shot.Power), aim, pace.Speed, ctx.VCap, ctx.Pace, court, ctx.Factory, physics, footprint, out _);
                    var plain = plainSolution.Solved ? Classify(contact, plainSolution.Velocity, court, physics, ctx.Factory, footprint) : new Flight { Class = "unsolved" };
                    // Clean front-wall x at the same effective height and frozen speed, every 5 cm.
                    var clean = new List<float>();
                    for (int i = 5; i <= 635; i += 5)
                    {
                        float x = i / 100f;
                        var probe = ShotSolver.Solve(new ShotRequest(contact, new Vector2(x, solution.TargetUsed.y), shot.Power), solution.Speed, solution.Speed, solution.Speed, physics.Gravity, physics.BallRadius, court, ctx.Factory, physics);
                        if (probe.Solved && !probe.TargetLowered && Classify(contact, probe.Velocity, court, physics, ctx.Factory, footprint).Success) clean.Add(x);
                    }
                    // The clean segment nearest to the effective aim.
                    float lo = float.NaN, hi = float.NaN; double best = double.PositiveInfinity;
                    for (int i = 0; i < clean.Count;)
                    {
                        int j = i; while (j + 1 < clean.Count && Math.Abs(clean[j + 1] - clean[j] - .05f) < 1e-4f) j++;
                        double d = solution.TargetUsed.x < clean[i] ? clean[i] - solution.TargetUsed.x : solution.TargetUsed.x > clean[j] ? solution.TargetUsed.x - clean[j] : 0;
                        if (d < best) { best = d; lo = clean[i]; hi = clean[j]; }
                        i = j + 1;
                    }
                    bool contactIn = clean.Count > 0 && clean.Any(x => Math.Abs(x - contact.x) <= .025f);
                    float lift = input.Aim.y - aim.y, lateral = s.Trace.vectors.Sum(v => v.DeltaLateral);
                    double slope = ((double)solution.TargetUsed.x - contact.x) / contact.z;
                    w.WriteLine(string.Join("\t", new object[] { s.File, shot.LaunchId, shot.ContactStep, shot.Side, shot.Result, shot.Power, contact.x, contact.y, contact.z, aim.x, aim.y,
                        input.Aim.y, input.Clamped, lift, lateral, solution.TargetUsed.x, solution.TargetUsed.y, solution.Speed, solution.TargetLowered, info.Reason, info.Evaluations, exact,
                        Front(contact, court), IntendedCross(contact, aim, court), IntendedDeep(shot.Power, aim), flight.Class, flight.Success, flight.Cause, flight.SideZ, flight.X1, flight.Z1, flight.X2, flight.Z2, flight.MaxZ,
                        prim.Class, prim.Success, primitive.TargetUsed.x, primitive.TargetUsed.y, plain.Class, plain.Success,
                        lo, hi, clean.Count, contactIn, double.IsInfinity(best) ? double.NaN : best, slope }.Select(F)));
                }
            }
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }

        private static string Git(string root, string args)
        {
            var info = new System.Diagnostics.ProcessStartInfo("git", args) { WorkingDirectory = root, RedirectStandardOutput = true, UseShellExecute = false };
            using (var p = System.Diagnostics.Process.Start(info)) { string o = p.StandardOutput.ReadToEnd(); p.WaitForExit(); return o; }
        }
    }
}
