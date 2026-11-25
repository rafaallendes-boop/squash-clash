using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    // Diagnosis 25 nov (read-only, local branch, no PR): «from the front the stroke goes very long and hits the back glass».
    // Explicit experiment, separate from regression. Part A replays the PO's applied human strokes of 23-24 nov (builds
    // 90ddc7e/f306d70, the same solve path as c9ed8af) with P2-Analytic from the recorded contact and launch velocity, rebuilds
    // them through the product path (pace -> causal -> legal bound -> intention -> solver -> front deep drive guard) and
    // measures counterfactuals. Part B is a deterministic grid of front-zone contacts with perfect timing (no causal vectors).
    // Inputs are the PO's ignored metrics (SC_FLD_METRICS); outputs go to ignored Logs.
    public sealed class FrontLongDiagnosisExperiment
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private static readonly string[] Inputs = { "metrics-20261123T174154689-", "metrics-20261123T192900716-", "metrics-20261124T175940193-",
            "metrics-20261124T210240729-", "metrics-20261124T230522015-" };
        private static string F(object v) => v is float f ? f.ToString("R", C) : v is double d ? d.ToString("R", C) : Convert.ToString(v, C);

        internal sealed class Length
        {
            internal string First = "", Cause = ""; internal bool Legal, BackFly, BackBefore2, SideBeforeBounce;
            internal double Z1 = double.NaN, Z2 = double.NaN, T1 = double.NaN, T2 = double.NaN, FrontY = double.NaN, BackY = double.NaN;
        }

        // Where the flight bounces and whether it meets the back wall before the first bounce (on the fly) or before the second.
        internal static Length Measure(Vector3 from, Vector3 velocity, CourtDimensions court, PhysicsParams physics, IBallSimulationFactory factory, float footprint)
        {
            var m = new Length(); var rules = new PointRules(court, footprint); rules.Begin(PlayerId.Human, PlayerId.Rival);
            bool first = true; int floors = 0;
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
                        if (!hit.Persistent)
                        {
                            if (first) { first = false; m.First = hit.Surface.ToString(); if (hit.Surface == SurfaceId.FrontWall) m.FrontY = hit.Point.y; }
                            if (hit.Surface == SurfaceId.BackWall)
                            {
                                if (floors == 0 && !m.BackFly) { m.BackFly = true; m.BackY = hit.Point.y; }
                                if (floors < 2) m.BackBefore2 = true;
                            }
                            if (floors == 0 && (hit.Surface == SurfaceId.LeftWall || hit.Surface == SurfaceId.RightWall) && m.First == "FrontWall") m.SideBeforeBounce = true;
                            if (hit.Surface == SurfaceId.Floor)
                            {
                                floors++;
                                double t = (step - 1 + (double)hit.TimeInStep) / physics.StepHz;
                                if (floors == 1) { m.Z1 = hit.Point.z; m.T1 = t; }
                                if (floors == 2) { m.Z2 = hit.Point.z; m.T2 = t; }
                            }
                        }
                        rules.OnBallContact(hit);
                    }
                    if (!rules.Closed && simulation.State.Position.y > court.WallHeight) rules.OnOverTop();
                }
            }
            m.Cause = rules.Cause.ToString(); m.Legal = m.First == "FrontWall" && rules.Cause == PointEndCause.DoubleBounce;
            return m;
        }

        private static readonly string LengthHeader = "first\tlegal\tcause\tz1\tt1\tz2\tt2\tbackFly\tbackY\tbackBefore2\tsideBeforeBounce\tfrontY";
        private static IEnumerable<object> Cells(Length m) => new object[] { m.First, m.Legal, m.Cause, m.Z1, m.T1, m.Z2, m.T2, m.BackFly, m.BackY, m.BackBefore2, m.SideBeforeBounce, m.FrontY };

        // The product path of ShotController.Solve for a front-wall aim: CausalQuality pipeline, then the guard inside its zone.
        private static (ShotSolution solution, ShotPaceResult pace, CausalInput input, string guard) Product(RallyContext ctx, Vector3 contact, Vector2 aim, float power, CausalVector[] vectors)
        {
            var settings = CausalSettings.From(ctx.Config.Source);
            var pace = ShotPace.Requested(power, contact, aim, ctx.Physics.BallRadius, ctx.Config.Source);
            var input = CausalQuality.BoundToLegalFront(CausalQuality.Apply(aim, pace.Speed, vectors ?? new CausalVector[0], settings), aim, ctx.Court, ctx.Physics.BallRadius);
            var solution = ShotIntentSolver.Solve(new ShotRequest(contact, input.Aim, power), aim, input.Speed, ctx.VCap, ctx.Pace, ctx.Court, ctx.Factory, ctx.Physics,
                ShotIntentSolver.DefaultFootprintRadius, out _);
            string guard = "inactive";
            if (FrontDeepDrive.Active(power, contact, aim, ctx.Court))
            {
                solution = FrontDeepDrive.Apply(contact, aim, power, solution, ctx.Court, ctx.Factory, ctx.Physics, ShotIntentSolver.DefaultFootprintRadius, out var info);
                guard = info.Reason;
            }
            return (solution, pace, input, guard);
        }

        // Fix-option estimate only (never product code): a copy of FrontDeepDrive.Apply where (a) the guard acts only when the
        // stroke it receives meets a side wall before its first bounce (the case its design was written for), and/or (b) its
        // success also forbids the back wall before the first bounce. Same scan, same budget, same height fallback.
        private static bool DeepVariant(Vector3 from, Vector3 velocity, CourtDimensions court, IBallSimulationFactory factory, PhysicsParams physics, float footprint, bool noFly)
        {
            if (!FrontDeepDrive.Deep(from, velocity, court, factory, physics, footprint)) return false;
            return !noFly || !Measure(from, velocity, court, physics, factory, footprint).BackFly;
        }
        private static ShotSolution GuardVariant(Vector3 from, Vector2 aim, float power, ShotSolution solution, CourtDimensions court, IBallSimulationFactory factory, PhysicsParams physics,
            float footprint, bool sideOnly, bool noFly, out string reason)
        {
            reason = "inactive";
            if (!solution.Solved || !FrontDeepDrive.Active(power, from, aim, court)) return solution;
            if (sideOnly && !Measure(from, solution.Velocity, court, physics, factory, footprint).SideBeforeBounce) { reason = "no-side"; return solution; }
            if (DeepVariant(from, solution.Velocity, court, factory, physics, footprint, noFly)) { reason = "already-deep"; return solution; }
            float y0 = solution.TargetUsed.y, speed = solution.Speed, x0 = from.x, x1 = solution.TargetUsed.x;
            float legalLow = (float)((double)court.TinTop + physics.BallRadius);
            bool Try(float t, float y, out ShotSolution candidate)
            {
                float x = t >= 1 ? x1 : x0 + (x1 - x0) * t;
                candidate = ShotSolver.Solve(new ShotRequest(from, new Vector2(x, y), power), speed, speed, speed, physics.Gravity, physics.BallRadius, court, factory, physics);
                return candidate.Solved && !candidate.TargetLowered && DeepVariant(from, candidate.Velocity, court, factory, physics, footprint, noFly);
            }
            for (int level = 0; level <= FrontDeepDrive.FallbackLevels; level++)
            {
                float y = level == 0 ? y0 : (float)(y0 - ((double)y0 - legalLow) * level / FrontDeepDrive.FallbackLevels);
                if (!(y > court.TinTop)) break;
                for (int k = level == 0 ? FrontDeepDrive.CoarseSteps - 1 : FrontDeepDrive.CoarseSteps; k >= 0; k--)
                {
                    float t = (float)k / FrontDeepDrive.CoarseSteps;
                    if (!Try(t, y, out var best)) continue;
                    float low = t, high = k < FrontDeepDrive.CoarseSteps ? (float)(k + 1) / FrontDeepDrive.CoarseSteps : t;
                    for (int i = 0; i < FrontDeepDrive.RefineSteps && high > low; i++)
                    {
                        float mid = (low + high) * .5f;
                        if (Try(mid, y, out var candidate)) { low = mid; best = candidate; } else high = mid;
                    }
                    reason = "acted"; return best;
                }
            }
            reason = "no-deep-x"; return solution;
        }

        // Fix-option estimate only: the guard's success additionally requires the first bounce at z <= zMax with no back wall
        // before it (illustrative ceilings, not criteria). At the frozen speed first (x scan, then the height fallback); with
        // speed factors, the same scan is repeated at lower speeds, highest first (option B, human-only).
        private static ShotSolution CeilingVariant(Vector3 from, Vector2 aim, float power, ShotSolution solution, CourtDimensions court, IBallSimulationFactory factory, PhysicsParams physics,
            float footprint, float zMax, float[] speedFactors, out string reason)
        {
            reason = "inactive";
            if (!solution.Solved || !FrontDeepDrive.Active(power, from, aim, court)) return solution;
            bool Good(Vector3 v)
            {
                if (!FrontDeepDrive.Deep(from, v, court, factory, physics, footprint)) return false;
                var m = Measure(from, v, court, physics, factory, footprint); return !m.BackFly && m.Z1 <= zMax;
            }
            if (Good(solution.Velocity)) { reason = "already-good"; return solution; }
            float y0 = solution.TargetUsed.y, x0 = from.x, x1 = solution.TargetUsed.x;
            float legalLow = (float)((double)court.TinTop + physics.BallRadius);
            foreach (float factor in speedFactors)
            {
                float speed = solution.Speed * factor;
                for (int level = 0; level <= FrontDeepDrive.FallbackLevels; level++)
                {
                    float y = level == 0 ? y0 : (float)(y0 - ((double)y0 - legalLow) * level / FrontDeepDrive.FallbackLevels);
                    if (!(y > court.TinTop)) break;
                    for (int k = FrontDeepDrive.CoarseSteps; k >= 0; k--)
                    {
                        float t = (float)k / FrontDeepDrive.CoarseSteps, x = t >= 1 ? x1 : x0 + (x1 - x0) * t;
                        var c = ShotSolver.Solve(new ShotRequest(from, new Vector2(x, y), power), speed, speed, speed, physics.Gravity, physics.BallRadius, court, factory, physics);
                        if (c.Solved && !c.TargetLowered && Good(c.Velocity)) { reason = "acted@" + factor.ToString("0.00", C); return c; }
                    }
                }
            }
            reason = "none"; return solution;
        }

        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void WriteCeilingVariants()
        {
            string run = Environment.GetEnvironmentVariable("SC_FLD_RUN");
            Assert.That(run == "ceil01" || run == "ceil02", Is.True, "Use a predeclared run ID.");
            string metrics = Environment.GetEnvironmentVariable("SC_FLD_METRICS");
            Assert.That(Directory.Exists(metrics), Is.True, "SC_FLD_METRICS must name the PO metrics folder.");
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var preset = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
            string dir = Path.Combine(root, "Logs/experiments/front-long/" + run);
            Assert.That(Directory.Exists(dir), Is.False, "Never overwrite evidence.");
            Directory.CreateDirectory(dir);
            var court = ctx.Court; var physics = ctx.Physics; float footprint = ShotIntentSolver.DefaultFootprintRadius;
            var frozen = new[] { 1f }; var lower = new[] { 1f, .9f, .8f, .7f, .6f, .5f };
            var variants = new[] { ("z85", 8.5f, frozen), ("z80", 8.0f, frozen), ("z85speed", 8.5f, lower), ("z80speed", 8.0f, lower) };
            string Pre(string p) => string.Join("\t", LengthHeader.Split('\t').Select(h => p + h));
            string header = string.Join("\t", variants.Select(v => v.Item1 + "_reason\t" + v.Item1 + "_speed\t" + Pre(v.Item1 + "_")));
            IEnumerable<object> Run(Vector3 contact, Vector2 aim, float p, ShotSolution intent)
            {
                var cells = new List<object>();
                foreach (var (name, zMax, factors) in variants)
                {
                    var s = CeilingVariant(contact, aim, p, intent, court, ctx.Factory, physics, footprint, zMax, factors, out var reason);
                    cells.Add(reason); cells.Add(s.Speed); cells.AddRange(Cells(s.Solved ? Measure(contact, s.Velocity, court, physics, ctx.Factory, footprint) : new Length { First = "unsolved" }));
                }
                return cells;
            }
            var hashes = new List<string>();
            using (var w = new StreamWriter(Path.Combine(dir, "replay-ceiling.tsv"), false, Utf8))
            {
                w.WriteLine("file\tlaunch\tstep\tP\tcx\tcy\tcz\taimX\taimY\tintentSpeed\t" + header);
                foreach (var s in FrontDeepDriveExperiment.Load(metrics, hashes, Inputs))
                {
                    var shot = s.Shot; var contact = new Vector3(shot.ContactX, shot.ContactY, shot.ContactZ); var aim = new Vector2(shot.AimX, shot.AimY);
                    var intent = FrontDeepDriveExperiment.Rebuild(ctx, s).solution;
                    w.WriteLine(string.Join("\t", new object[] { s.File, shot.LaunchId, shot.ContactStep, shot.Power, contact.x, contact.y, contact.z, aim.x, aim.y, intent.Speed }.Concat(Run(contact, aim, shot.Power, intent)).Select(F)));
                }
            }
            using (var w = new StreamWriter(Path.Combine(dir, "grid-ceiling.tsv"), false, Utf8))
            {
                w.WriteLine("cx\tcy\tcz\taimX\taimY\tP\tintentSpeed\t" + header);
                foreach (float cz in new[] { 1.0f, 1.5f, 2.5f, 3.5f, 4.5f, 5.4f })
                    foreach (float cx in new[] { 1.2f, 3.2f, 5.2f })
                        foreach (float cy in new[] { .3f, .8f, 1.3f })
                            foreach (float ax in new[] { 1.0f, 3.2f, 5.4f })
                                foreach (float ay in new[] { .7f, 1.0f, 1.4f, 2.0f })
                                    foreach (int k in new[] { 14, 17, 20 })
                                    {
                                        float p = k / 20f; var contact = new Vector3(cx, cy, cz); var aim = new Vector2(ax, ay);
                                        var pace = ShotPace.Requested(p, contact, aim, physics.BallRadius, ctx.Config.Source);
                                        var intent = ShotIntentSolver.Solve(new ShotRequest(contact, aim, p), aim, pace.Speed, ctx.VCap, ctx.Pace, court, ctx.Factory, physics, footprint, out _);
                                        w.WriteLine(string.Join("\t", new object[] { cx, cy, cz, ax, ay, p, intent.Speed }.Concat(Run(contact, aim, p, intent)).Select(F)));
                                    }
            }
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }

        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void WriteGuardVariants()
        {
            string run = Environment.GetEnvironmentVariable("SC_FLD_RUN");
            Assert.That(run == "var01" || run == "var02", Is.True, "Use a predeclared run ID.");
            string metrics = Environment.GetEnvironmentVariable("SC_FLD_METRICS");
            Assert.That(Directory.Exists(metrics), Is.True, "SC_FLD_METRICS must name the PO metrics folder.");
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var preset = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
            string dir = Path.Combine(root, "Logs/experiments/front-long/" + run);
            Assert.That(Directory.Exists(dir), Is.False, "Never overwrite evidence.");
            Directory.CreateDirectory(dir);
            var court = ctx.Court; var physics = ctx.Physics; float footprint = ShotIntentSolver.DefaultFootprintRadius;
            var variants = new[] { ("sideOnly", true, false), ("noFly", false, true), ("sideOnlyNoFly", true, true) };
            string Pre(string p) => string.Join("\t", LengthHeader.Split('\t').Select(h => p + h));
            string header = string.Join("\t", variants.Select(v => v.Item1 + "_reason\t" + Pre(v.Item1 + "_")));
            IEnumerable<object> Run(Vector3 contact, Vector2 aim, float p, ShotSolution intent)
            {
                var cells = new List<object>();
                foreach (var (name, sideOnly, noFly) in variants)
                {
                    var s = GuardVariant(contact, aim, p, intent, court, ctx.Factory, physics, footprint, sideOnly, noFly, out var reason);
                    cells.Add(reason); cells.AddRange(Cells(s.Solved ? Measure(contact, s.Velocity, court, physics, ctx.Factory, footprint) : new Length { First = "unsolved" }));
                }
                return cells;
            }
            var hashes = new List<string>();
            using (var w = new StreamWriter(Path.Combine(dir, "replay-variants.tsv"), false, Utf8))
            {
                w.WriteLine("file\tlaunch\tstep\tP\tcx\tcy\tcz\taimX\taimY\t" + header);
                foreach (var s in FrontDeepDriveExperiment.Load(metrics, hashes, Inputs))
                {
                    var shot = s.Shot; var contact = new Vector3(shot.ContactX, shot.ContactY, shot.ContactZ); var aim = new Vector2(shot.AimX, shot.AimY);
                    var intent = FrontDeepDriveExperiment.Rebuild(ctx, s).solution;
                    w.WriteLine(string.Join("\t", new object[] { s.File, shot.LaunchId, shot.ContactStep, shot.Power, contact.x, contact.y, contact.z, aim.x, aim.y }.Concat(Run(contact, aim, shot.Power, intent)).Select(F)));
                }
            }
            using (var w = new StreamWriter(Path.Combine(dir, "grid-variants.tsv"), false, Utf8))
            {
                w.WriteLine("cx\tcy\tcz\taimX\taimY\tP\t" + header);
                foreach (float cz in new[] { 1.0f, 1.5f, 2.5f, 3.5f, 4.5f, 5.4f })
                    foreach (float cx in new[] { 1.2f, 3.2f, 5.2f })
                        foreach (float cy in new[] { .3f, .8f, 1.3f })
                            foreach (float ax in new[] { 1.0f, 3.2f, 5.4f })
                                foreach (float ay in new[] { .7f, 1.0f, 1.4f, 2.0f })
                                    for (int k = 11; k <= 20; k++)
                                    {
                                        float p = k / 20f; var contact = new Vector3(cx, cy, cz); var aim = new Vector2(ax, ay);
                                        var pace = ShotPace.Requested(p, contact, aim, physics.BallRadius, ctx.Config.Source);
                                        var intent = ShotIntentSolver.Solve(new ShotRequest(contact, aim, p), aim, pace.Speed, ctx.VCap, ctx.Pace, court, ctx.Factory, physics, footprint, out _);
                                        w.WriteLine(string.Join("\t", new object[] { cx, cy, cz, ax, ay, p }.Concat(Run(contact, aim, p, intent)).Select(F)));
                                    }
            }
            using (var w = new StreamWriter(Path.Combine(dir, "s1-variants.tsv"), false, Utf8))
            {
                w.WriteLine("contact\tkind\taimX\taimY\tpi\t" + header);
                var legal = new LegalSettings((float)preset.Get("legal.minHeight"), (float)preset.Get("legal.maxHeight"), physics.StepHz, physics.Gravity, physics.BallRadius, court);
                var heights = new List<(float y, string kind)>();
                foreach (float o in new[] { .05f, .10f, .15f }) heights.Add((court.TinTop + o, "drop"));
                foreach (float h in new[] { .8f, 1.2f, 1.6f }) heights.Add((h, "low"));
                foreach (float h in new[] { 1.8f, 2.2f, 2.55f }) heights.Add((h, "mid"));
                foreach (var recipe in PracticeLaunchDefinitions.Create(ctx.Config).Where(r => r.Id == "SL5" || r.Id == "SL6"))
                {
                    Trajectory trajectory;
                    using (var sim = ctx.Factory.Create(physics, court))
                        trajectory = TrajectoryRunner.Run(sim, recipe.Initial, physics.StepHz * 10, StopCondition.SecondFloorBounce, court);
                    var plan = ContactPlanner.Plan(trajectory, recipe.Body, court, legal, 1);
                    foreach (int off in new[] { -12, 0, 12 })
                    {
                        string label = recipe.Id + (off == 0 ? "-Tstar" : off < 0 ? "-Tstar" + off : "-Tstar+" + off);
                        long step = plan.TStarStep + off, first0 = trajectory.States[0].Step;
                        Assert.That(plan.Exists && ContactPlanner.IsLegalStep(trajectory, step, recipe.Body, legal), Is.True, label);
                        var center = trajectory.States[(int)(step - first0)].Position;
                        foreach (var (y, kind) in heights)
                            foreach (float x in new[] { 1.2f, 2.6f, 4.0f })
                                for (int k = 8; k <= 12; k++)
                                {
                                    var aim = new Vector2(x, y); float p = k / 20f;
                                    var pace = ShotPace.Requested(p, center, aim, physics.BallRadius, ctx.Config.Source);
                                    var intent = ShotIntentSolver.Solve(new ShotRequest(center, aim, p), aim, pace.Speed, ctx.VCap, ctx.Pace, court, ctx.Factory, physics, footprint, out _);
                                    w.WriteLine(string.Join("\t", new object[] { label, kind, x, y, k }.Concat(Run(center, aim, p, intent)).Select(F)));
                                }
                    }
                }
            }
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }

        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void WriteFrontLong()
        {
            string run = Environment.GetEnvironmentVariable("SC_FLD_RUN");
            Assert.That(run == "diag01" || run == "diag02" || run == "diag03" || run == "diag04", Is.True, "Use a predeclared run ID.");
            string metrics = Environment.GetEnvironmentVariable("SC_FLD_METRICS");
            Assert.That(Directory.Exists(metrics), Is.True, "SC_FLD_METRICS must name the PO metrics folder.");
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var preset = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
            string dir = Path.Combine(root, "Logs/experiments/front-long/" + run);
            Assert.That(Directory.Exists(dir), Is.False, "Never overwrite evidence.");
            Directory.CreateDirectory(dir);
            var hashes = new List<string>();
            var strokes = FrontDeepDriveExperiment.Load(metrics, hashes, Inputs);
            File.WriteAllText(Path.Combine(dir, "source.tsv"), "defaults\tkeys\tsimulationKey\trallyParams\tunity\n" + preset.Hash + "\t" + preset.Ordered.Count + "\t"
                + ctx.Config.SimulationKey + "\t" + ctx.Params.Hash + "\t" + Application.unityVersion + "\n" + string.Join("\n", hashes) + "\n", Utf8);
            var court = ctx.Court; var physics = ctx.Physics; float footprint = ShotIntentSolver.DefaultFootprintRadius;
            float shortLine = court.Length - court.ShortLineNearBackEdgeFromBack;

            // Part A: replay and counterfactuals.
            using (var w = new StreamWriter(Path.Combine(dir, "replay.tsv"), false, Utf8))
            {
                string Prefixed(string p) => string.Join("\t", LengthHeader.Split('\t').Select(h => p + h));
                w.WriteLine("file\tlaunch\tstep\tresult\tP\tcx\tcy\tcz\taimX\taimY\tfront\tlift\tspeedFraction\tgamma\tvLinear\tvPace\tvFinal\ttuX\ttuY\tguard\texact\tdeviation\t"
                    + Prefixed("rec_") + "\t" + Prefixed("noGuard_") + "\t" + Prefixed("plain_") + "\t" + Prefixed("p075_") + "\t" + Prefixed("p050_"));
                foreach (var s in strokes)
                {
                    var shot = s.Shot; var contact = new Vector3(shot.ContactX, shot.ContactY, shot.ContactZ); var aim = new Vector2(shot.AimX, shot.AimY);
                    var recorded = new Vector3(shot.VelocityX, shot.VelocityY, shot.VelocityZ);
                    var rec = Measure(contact, recorded, court, physics, ctx.Factory, footprint);
                    var (final, pace, input, guard) = Product(ctx, contact, aim, shot.Power, s.Trace.vectors);
                    bool exact = final.Solved && final.Velocity.x == recorded.x && final.Velocity.y == recorded.y && final.Velocity.z == recorded.z;
                    double deviation = final.Solved ? (final.Velocity - recorded).magnitude : double.NaN;
                    var noGuardSol = FrontDeepDriveExperiment.Rebuild(ctx, s).solution;
                    Length M(ShotSolution x) => x.Solved ? Measure(contact, x.Velocity, court, physics, ctx.Factory, footprint) : new Length { First = "unsolved" };
                    var noGuard = M(noGuardSol);
                    var plain = M(Product(ctx, contact, aim, shot.Power, null).solution);
                    var p075 = M(Product(ctx, contact, aim, Math.Min(shot.Power, .75f), s.Trace.vectors).solution);
                    var p050 = M(Product(ctx, contact, aim, Math.Min(shot.Power, .5f), s.Trace.vectors).solution);
                    float lift = input.Aim.y - aim.y, fraction = s.Trace.vectors.Sum(v => v.DeltaSpeedFraction);
                    w.WriteLine(string.Join("\t", new object[] { s.File, shot.LaunchId, shot.ContactStep, shot.Result, shot.Power, contact.x, contact.y, contact.z, aim.x, aim.y,
                        contact.z < shortLine, lift, fraction, pace.Gamma, pace.LinearSpeed, pace.Speed, final.Speed, final.TargetUsed.x, final.TargetUsed.y, guard, exact, deviation }
                        .Concat(Cells(rec)).Concat(Cells(noGuard)).Concat(Cells(plain)).Concat(Cells(p075)).Concat(Cells(p050)).Select(F)));
                }
            }

            // Part B: deterministic front-zone grid, perfect timing (no causal vectors), the product path including the guard.
            using (var w = new StreamWriter(Path.Combine(dir, "grid.tsv"), false, Utf8))
            {
                string Pre(string p) => string.Join("\t", LengthHeader.Split('\t').Select(h => p + h));
                w.WriteLine("cx\tcy\tcz\taimX\taimY\tP\tgamma\tvPace\tvFinal\ttuX\ttuY\tlowered\tguard\tsolved\t" + LengthHeader + "\t" + Pre("noGuard_") + "\t" + Pre("prim_"));
                foreach (float cz in new[] { 1.0f, 1.5f, 2.5f, 3.5f, 4.5f, 5.4f })
                    foreach (float cx in new[] { 1.2f, 3.2f, 5.2f })
                        foreach (float cy in new[] { .3f, .8f, 1.3f })
                            foreach (float ax in new[] { 1.0f, 3.2f, 5.4f })
                                foreach (float ay in new[] { .7f, 1.0f, 1.4f, 2.0f })
                                    for (int k = 0; k <= 20; k++)
                                    {
                                        float p = k / 20f; var contact = new Vector3(cx, cy, cz); var aim = new Vector2(ax, ay);
                                        var (sol, pace, _, guard) = Product(ctx, contact, aim, p, null);
                                        Length M(ShotSolution x) => x.Solved ? Measure(contact, x.Velocity, court, physics, ctx.Factory, footprint) : new Length { First = "unsolved" };
                                        var m = M(sol);
                                        // Layer isolation: the intention without the guard, and the bare primitive at the pace speed (pre G1 A2).
                                        var noGuard = M(ShotIntentSolver.Solve(new ShotRequest(contact, aim, p), aim, pace.Speed, ctx.VCap, ctx.Pace, court, ctx.Factory, physics, footprint, out _));
                                        var prim = M(ShotSolver.Solve(new ShotRequest(contact, aim, p), pace.Speed, pace.Speed, ctx.VCap, physics.Gravity, physics.BallRadius, court, ctx.Factory, physics));
                                        w.WriteLine(string.Join("\t", new object[] { cx, cy, cz, ax, ay, p, pace.Gamma, pace.Speed, sol.Speed, sol.TargetUsed.x, sol.TargetUsed.y, sol.TargetLowered, guard, sol.Solved }
                                            .Concat(Cells(m)).Concat(Cells(noGuard)).Concat(Cells(prim)).Select(F)));
                                    }
            }
            // Part C: the exact S1 sample of the 14 nov pace design (SL5/SL6 at T*-12, T*, T*+12; the same aims and P = 8..12/20).
            // "legacy" is the 14 nov path (pace -> bare solver, SportScenarioExperiment.Run, its own judge); "product" is today's
            // path (pace -> intention -> guard). Both are also measured with Measure so the two judges can be compared.
            using (var w = new StreamWriter(Path.Combine(dir, "s1.tsv"), false, Utf8))
            {
                string Pre(string p) => string.Join("\t", LengthHeader.Split('\t').Select(h => p + h));
                w.WriteLine("contact\tkind\taimX\taimY\tpi\tlegacySolved\tlegacyLegal\tlegacyBackBeforeFirst\tlegacySecondObserved\tlegacyZ2\tguard\tproductSolved\t" + Pre("legacy_") + "\t" + Pre("product_"));
                var legal = new LegalSettings((float)preset.Get("legal.minHeight"), (float)preset.Get("legal.maxHeight"), physics.StepHz, physics.Gravity, physics.BallRadius, court);
                var heights = new List<(float y, string kind)>();
                foreach (float o in new[] { .05f, .10f, .15f }) heights.Add((court.TinTop + o, "drop"));
                foreach (float h in new[] { .8f, 1.2f, 1.6f }) heights.Add((h, "low"));
                foreach (float h in new[] { 1.8f, 2.2f, 2.55f }) heights.Add((h, "mid"));
                var factory = (AnalyticBallSimulationFactory)ctx.Factory; int id = 0;
                foreach (var recipe in PracticeLaunchDefinitions.Create(ctx.Config).Where(r => r.Id == "SL5" || r.Id == "SL6"))
                {
                    Trajectory trajectory;
                    using (var sim = factory.Create(physics, court))
                        trajectory = TrajectoryRunner.Run(sim, recipe.Initial, physics.StepHz * 10, StopCondition.SecondFloorBounce, court);
                    var plan = ContactPlanner.Plan(trajectory, recipe.Body, court, legal, 1);
                    foreach (int off in new[] { -12, 0, 12 })
                    {
                        string label = recipe.Id + (off == 0 ? "-Tstar" : off < 0 ? "-Tstar" + off : "-Tstar+" + off);
                        if (!plan.Exists || plan.TStarStep < 0) { w.WriteLine(label + "\tnot-evaluated:no-plan"); continue; }
                        long step = plan.TStarStep + off, first0 = trajectory.States[0].Step;
                        if (step < first0 || step >= first0 + trajectory.States.Count || !ContactPlanner.IsLegalStep(trajectory, step, recipe.Body, legal)) { w.WriteLine(label + "\tnot-evaluated:step"); continue; }
                        var contact = new SportScenarioExperiment.Contact { id = label, center = trajectory.States[(int)(step - first0)].Position, body = recipe.Body.PositionXZ, tStarStep = (int)step };
                        foreach (var (y, kind) in heights)
                            foreach (float x in new[] { 1.2f, 2.6f, 4.0f })
                                for (int k = 8; k <= 12; k++)
                                {
                                    var aim = new Vector2(x, y); float p = k / 20f;
                                    var t = SportScenarioExperiment.Run(id++, 0, 0, 0, k, contact, aim, preset, ctx.Config, factory, footprint);
                                    var legacy = t.solved ? Measure(contact.center, t.velocity, court, physics, factory, footprint) : new Length { First = "unsolved" };
                                    var (sol, _, _, guard) = Product(ctx, contact.center, aim, p, null);
                                    var product = sol.Solved ? Measure(contact.center, sol.Velocity, court, physics, factory, footprint) : new Length { First = "unsolved" };
                                    w.WriteLine(string.Join("\t", new object[] { label, kind, x, y, k, t.solved, t.legal, t.backBeforeFirst, t.secondObserved, t.second.z, guard, sol.Solved }
                                        .Concat(Cells(legacy)).Concat(Cells(product)).Select(F)));
                                }
                    }
                }
            }
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }
    }
}
