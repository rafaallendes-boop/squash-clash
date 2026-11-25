using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    // Length ceiling of the front deep drive guard (PO, 25 nov: «Sí, tope de largo a 8,5 m»). Explicit experiment, separate
    // from regression. It runs the product path (pace -> causal -> legal bound -> intention -> solver -> FrontDeepDrive) as the
    // checked-out code has it, so the same method measures "before" (main) and "after" (the ceiling) in separate processes.
    // Population A: the PO's applied front-wall human strokes of 23-24 nov (the diagnosis population: contact in front of the
    // short line, P > 0.5, no two-wall attempt). Population B: the diagnosis grid at P = 0.70/0.85/1 with perfect timing.
    // Population C: the S1 sample of the 14 nov pace design (informative; S1 itself is not touched).
    // The long judge is independent of the product predicate: first bounce beyond 8.5 m or the back wall before it.
    // Inputs are the PO's ignored metrics (SC_FLC_METRICS); outputs go to ignored Logs. Timing goes to its own file.
    public sealed class FrontLengthCeilingExperiment
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private static readonly string[] Inputs = { "metrics-20261123T174154689-", "metrics-20261123T192900716-", "metrics-20261124T175940193-",
            "metrics-20261124T210240729-", "metrics-20261124T230522015-" };
        private static readonly string[] Runs = { "before01", "before02", "after01", "after02" };
        private const float LongZ = 8.5f; // the measuring judge (PO decision of 25 nov), deliberately not the product constant
        private const int TimingRepeats = 3;
        private static string F(object v) => v is float f ? f.ToString("R", C) : v is double d ? d.ToString("R", C) : Convert.ToString(v, C);

        [Serializable] private sealed class Line { public string kind; public string payload; }
        [Serializable] private sealed class Boast { public string attemptId; public long contactStep; }

        private static readonly string Header = "first\tlegal\tcause\tz1\tz2\tbackFly\tbackBefore2\tsideBeforeBounce\tlong";
        private static IEnumerable<object> Cells(FrontLongDiagnosisExperiment.Length m)
            => new object[] { m.First, m.Legal, m.Cause, m.Z1, m.Z2, m.BackFly, m.BackBefore2, m.SideBeforeBounce, Long(m) };
        private static bool Long(FrontLongDiagnosisExperiment.Length m) => m.First == "FrontWall" && (m.BackFly || !(m.Z1 <= LongZ));

        private static HashSet<string> BoastAttempts(string dir)
        {
            var keys = new HashSet<string>();
            foreach (var prefix in Inputs)
                foreach (var text in File.ReadAllLines(Directory.GetFiles(dir, prefix + "*.jsonl").Single()))
                {
                    var line = JsonUtility.FromJson<Line>(text);
                    if (line.kind != "shot-boast") continue;
                    var b = JsonUtility.FromJson<Boast>(line.payload); keys.Add(b.attemptId + "/" + b.contactStep);
                }
            return keys;
        }

        // The guard on the intention solution, timed (best of TimingRepeats; deterministic result checked across repeats).
        private static (ShotSolution solution, FrontDeepInfo info, double ms) Guard(RallyContext ctx, Vector3 contact, Vector2 aim, float power, in ShotSolution intent)
        {
            ShotSolution result = intent; FrontDeepInfo info = new FrontDeepInfo("inactive", 0, 0); double best = double.MaxValue;
            if (!FrontDeepDrive.Active(power, contact, aim, ctx.Court)) return (result, info, 0);
            for (int r = 0; r < TimingRepeats; r++)
            {
                long start = Stopwatch.GetTimestamp();
                var s = FrontDeepDrive.Apply(contact, aim, power, intent, ctx.Court, ctx.Factory, ctx.Physics, ShotIntentSolver.DefaultFootprintRadius, out var i);
                double ms = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
                if (r > 0) Assert.That(s.Velocity == result.Velocity && i.Reason == info.Reason && i.Evaluations == info.Evaluations, Is.True, "deterministic guard");
                result = s; info = i; best = Math.Min(best, ms);
            }
            return (result, info, best);
        }

        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void WriteLengthCeiling()
        {
            string run = Environment.GetEnvironmentVariable("SC_FLC_RUN");
            Assert.That(Runs.Contains(run), Is.True, "Use a predeclared run ID.");
            string metrics = Environment.GetEnvironmentVariable("SC_FLC_METRICS");
            Assert.That(Directory.Exists(metrics), Is.True, "SC_FLC_METRICS must name the PO metrics folder.");
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            Assert.That(Git(root, "status --porcelain"), Is.Empty, "Commit the experiment before running.");
            var preset = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
            string dir = Path.Combine(root, "Logs/experiments/front-length-ceiling/" + run);
            Assert.That(Directory.Exists(dir), Is.False, "Never overwrite evidence.");
            Directory.CreateDirectory(dir);
            var hashes = new List<string>();
            var strokes = FrontDeepDriveExperiment.Load(metrics, hashes, Inputs);
            var boasts = BoastAttempts(metrics);
            File.WriteAllText(Path.Combine(dir, "source.tsv"), "head\tdefaults\tkeys\tsimulationKey\trallyParams\tunity\n" + Git(root, "rev-parse HEAD").Trim() + "\t"
                + preset.Hash + "\t" + preset.Ordered.Count + "\t" + ctx.Config.SimulationKey + "\t" + ctx.Params.Hash + "\t" + Application.unityVersion + "\n"
                + string.Join("\n", hashes) + "\n", Utf8);
            var court = ctx.Court; var physics = ctx.Physics; float footprint = ShotIntentSolver.DefaultFootprintRadius;
            float shortLine = court.Length - court.ShortLineNearBackEdgeFromBack;
            FrontLongDiagnosisExperiment.Length M(Vector3 from, in ShotSolution s)
                => s.Solved ? FrontLongDiagnosisExperiment.Measure(from, s.Velocity, court, physics, ctx.Factory, footprint) : new FrontLongDiagnosisExperiment.Length { First = "unsolved" };
            string guardHeader = "zone\tintentTuX\tintentTuY\tintentSpeed\tintentLowered\tguard\tt\tevaluations\ttuX\ttuY\tspeed\tlowered\t";
            IEnumerable<object> GuardCells(bool zone, in ShotSolution intent, in ShotSolution final, in FrontDeepInfo info)
                => new object[] { zone, intent.TargetUsed.x, intent.TargetUsed.y, intent.Speed, intent.TargetLowered, info.Reason, info.T, info.Evaluations,
                    final.TargetUsed.x, final.TargetUsed.y, final.Speed, final.TargetLowered };

            using (var w = new StreamWriter(Path.Combine(dir, "replay.tsv"), false, Utf8))
            using (var cost = new StreamWriter(Path.Combine(dir, "cost.tsv"), false, Utf8))
            {
                w.WriteLine("file\tlaunch\tstep\tP\tcx\tcy\tcz\taimX\taimY\t" + guardHeader + Header);
                cost.WriteLine("population\tid\tguard\tevaluations\tms");
                foreach (var s in strokes)
                {
                    var shot = s.Shot; var contact = new Vector3(shot.ContactX, shot.ContactY, shot.ContactZ); var aim = new Vector2(shot.AimX, shot.AimY);
                    if (!(contact.z < shortLine) || !(shot.Power > .5f) || boasts.Contains(shot.AttemptId + "/" + shot.ContactStep)) continue;
                    var intent = FrontDeepDriveExperiment.Rebuild(ctx, s).solution;
                    var (final, info, ms) = Guard(ctx, contact, aim, shot.Power, intent);
                    bool zone = FrontDeepDrive.Active(shot.Power, contact, aim, court);
                    w.WriteLine(string.Join("\t", new object[] { s.File, shot.LaunchId, shot.ContactStep, shot.Power, contact.x, contact.y, contact.z, aim.x, aim.y }
                        .Concat(GuardCells(zone, intent, final, info)).Concat(Cells(M(contact, final))).Select(F)));
                    cost.WriteLine(string.Join("\t", new object[] { "replay", s.File + "/" + shot.LaunchId, info.Reason, info.Evaluations, ms }.Select(F)));
                }

                using (var g = new StreamWriter(Path.Combine(dir, "grid.tsv"), false, Utf8))
                {
                    g.WriteLine("cx\tcy\tcz\taimX\taimY\tP\t" + guardHeader + Header);
                    int id = 0;
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
                                            var (final, info, ms) = Guard(ctx, contact, aim, p, intent);
                                            bool zone = FrontDeepDrive.Active(p, contact, aim, court);
                                            g.WriteLine(string.Join("\t", new object[] { cx, cy, cz, ax, ay, p }.Concat(GuardCells(zone, intent, final, info)).Concat(Cells(M(contact, final))).Select(F)));
                                            if (zone) cost.WriteLine(string.Join("\t", new object[] { "grid", id, info.Reason, info.Evaluations, ms }.Select(F)));
                                            id++;
                                        }
                }

                using (var s1 = new StreamWriter(Path.Combine(dir, "s1.tsv"), false, Utf8))
                {
                    s1.WriteLine("contact\tkind\taimX\taimY\tpi\t" + guardHeader + Header);
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
                                        var (final, info, ms) = Guard(ctx, center, aim, p, intent);
                                        bool zone = FrontDeepDrive.Active(p, center, aim, court);
                                        s1.WriteLine(string.Join("\t", new object[] { label, kind, x, y, k }.Concat(GuardCells(zone, intent, final, info)).Concat(Cells(M(center, final))).Select(F)));
                                        if (zone) cost.WriteLine(string.Join("\t", new object[] { "s1", label + "/" + kind + "/" + F(x) + "/" + F(y) + "/" + k, info.Reason, info.Evaluations, ms }.Select(F)));
                                    }
                        }
                    }
                }
            }
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }

        private static string Git(string root, string args)
        {
            var info = new ProcessStartInfo("git", args) { WorkingDirectory = root, RedirectStandardOutput = true, UseShellExecute = false };
            using (var p = Process.Start(info)) { string o = p.StandardOutput.ReadToEnd(); p.WaitForExit(); return o; }
        }
    }
}
