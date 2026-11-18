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
    // G1 A2 completion (18 nov). Explicit experiments, separate from regression.
    //  - WriteGrid (SC_A2_RUN=run01|run02): the phase-A population (every practice recipe with a real T*, five columns,
    //    14 heights, 21 powers, direct and cap22) solved twice per row, by the primitive (old SolveIntention) and by the
    //    intention layer (new SolveIntention), each followed in P2 to the second bounce or closure. Geometry is
    //    deterministic (grid.tsv); solve times go to timing.tsv. It is a blind measurement beyond the 24 C1 witnesses.
    //  - WriteRallyPoints (SC_A2_RALLY=before|after): the first 200 points of headless matches (bot vs Medium AI).
    public sealed class G1A2CompleteExperiment
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private static string F(object v) => v is float f ? f.ToString("R", C) : v is double d ? d.ToString("R", C) : Convert.ToString(v, C);

        private static (string root, ParamSet preset, RallyContext ctx) Setup()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            Assert.That(Git(root, "status --porcelain"), Is.Empty, "Commit the experiment before running.");
            var preset = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash, Is.EqualTo("2ec0c953786a")); Assert.That(preset.Ordered.Count, Is.EqualTo(132));
            var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
            Assert.That(ctx.Config.SimulationKey, Is.EqualTo("166a96e319c7")); Assert.That(ctx.Params.Hash, Is.EqualTo("927e304bbfec"));
            return (root, preset, ctx);
        }
        private static string Source(string root, ParamSet preset, RallyContext ctx) =>
            "head\tdefaults\tkeys\tsimulationKey\trallyParams\tunity\n" + Git(root, "rev-parse HEAD").Trim() + "\t" + preset.Hash + "\t" + preset.Ordered.Count
            + "\t" + ctx.Config.SimulationKey + "\t" + ctx.Params.Hash + "\t" + Application.unityVersion + "\n";

        [Test, Explicit, Category("Experiments"), Timeout(7200000)]
        public void WriteGrid()
        {
            string run = Environment.GetEnvironmentVariable("SC_A2_RUN");
            Assert.That(run == "run01" || run == "run02", Is.True, "Use a predeclared run ID.");
            var (root, preset, ctx) = Setup();
            string dir = Path.Combine(root, "Logs/experiments/g1a2-complete/grid-" + run);
            Assert.That(Directory.Exists(dir), Is.False, "Never overwrite evidence.");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "source.tsv"), Source(root, preset, ctx), Utf8);
            int rows = 0;
            using (var grid = new StreamWriter(Path.Combine(dir, "grid.tsv"), false, Utf8))
            using (var timing = new StreamWriter(Path.Combine(dir, "timing.tsv"), false, Utf8))
            {
                string Half(string p) => string.Join("\t", new[] { "solved", "reason", "evaluations", "targetX", "targetY", "speed", "cause", "overTop", "frontY", "apex", "x1", "z1", "x2", "z2", "witness" }.Select(c => p + c));
                grid.WriteLine("recipe\tmode\tcolumn\tmargin\tk\tP\tfromX\tfromY\tfromZ\taimX\taimY\tactive\t" + Half("old_") + "\t" + Half("new_"));
                timing.WriteLine("recipe\tmode\tcolumn\tmargin\tk\tactive\toldMs\tnewMs\tnewEvaluations");
                var recipes = PracticeLaunchDefinitions.Create(ctx.Config).OrderBy(r => r.Id, StringComparer.Ordinal);
                foreach (var recipe in recipes)
                {
                    var plan = ContactPlanner.Plan(ctx.Predict(recipe.Initial), recipe.Body, ctx.Court, ctx.Legal, 1);
                    if (!plan.Exists || plan.TStarStep < 0) { TestContext.WriteLine("no T* " + recipe.Id); continue; }
                    Vector3 from = plan.PStar;
                    float near = from.x < ctx.Court.Width / 2 ? .6f : ctx.Court.Width - .6f, far = ctx.Court.Width - near;
                    float Through(float corner) => from.x + (corner - from.x) * from.z / (from.z + ctx.Court.Length);
                    var columns = new[] { ("right", .6f), ("center", ctx.Court.Width / 2), ("left", ctx.Court.Width - .6f), ("straight", Through(near)), ("cross", Through(far)) };
                    foreach (var column in columns)
                        foreach (float margin in new[] { .05f, .10f, .15f, .25f, .40f, .60f, .80f, 1f, 1.2f, 1.4f, 1.6f, 2f, 2.4f, 3f })
                            for (int k = 0; k <= 20; k++)
                                foreach (string mode in new[] { "direct", "cap22" })
                                {
                                    Vector2 aim = new Vector2(column.Item2, ctx.Court.TinTop + margin);
                                    float cap = from.y + Mathf.Tan((float)ctx.Params.MaxAimElevationDeg * Mathf.Deg2Rad) * Mathf.Sqrt((aim.x - from.x) * (aim.x - from.x) + from.z * from.z);
                                    if (mode == "cap22") { aim.y = Mathf.Min(aim.y, cap); aim = ShotOptions.ClampAim(ctx, aim); }
                                    float p = k / 20f;
                                    var pace = ShotPace.Requested(p, from, aim, ctx.Physics.BallRadius, ctx.Pace);
                                    long t0 = Stopwatch.GetTimestamp();
                                    var old = ShotSolver.Solve(new ShotRequest(from, aim, p), pace.Speed, pace.Speed, ctx.VCap, ctx.Physics.Gravity, ctx.Physics.BallRadius, ctx.Court, ctx.Factory, ctx.Physics);
                                    long t1 = Stopwatch.GetTimestamp();
                                    var now = ShotIntentSolver.Solve(new ShotRequest(from, aim, p), aim, pace.Speed, ctx.VCap, ctx.Pace, ctx.Court, ctx.Factory, ctx.Physics,
                                        (float)ctx.Params.FootprintRadius, out var info);
                                    long t2 = Stopwatch.GetTimestamp();
                                    bool active = info.Evaluations > 0;
                                    string line = column.Item1 == "straight" || column.Item1 == "cross" ? column.Item1 : "";
                                    grid.WriteLine(string.Join("\t", recipe.Id, mode, column.Item1, F(margin), F(k), F(p), F(from.x), F(from.y), F(from.z), F(aim.x), F(aim.y), F(active),
                                        Follow(ctx, from, old, line, 0, ""), Follow(ctx, from, now, line, info.Evaluations, info.Reason)));
                                    double ms = 1000d / Stopwatch.Frequency;
                                    timing.WriteLine(string.Join("\t", recipe.Id, mode, column.Item1, F(margin), F(k), F(active), F((t1 - t0) * ms), F((t2 - t1) * ms), F(info.Evaluations)));
                                    rows++;
                                }
                }
            }
            Assert.That(rows, Is.GreaterThanOrEqualTo(5880));
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
            TestContext.WriteLine(dir + " rows=" + rows);
        }

        // The C1 witness predicate, unchanged: legal to the second bounce, front <= 1.20, apex <= 2.13, z2 >= 8 and the side rule.
        private static string Follow(RallyContext ctx, Vector3 from, ShotSolution s, string line, int evaluations, string reason)
        {
            string[] cells = { F(s.Solved), s.Reason ?? "", F(evaluations), F(s.TargetUsed.x), F(s.TargetUsed.y), F(s.Speed), "", "", "", "", "", "", "", "", "" };
            if (!s.Solved) return string.Join("\t", cells);
            var trajectory = ctx.Predict(new BallState { Position = from, Velocity = s.Velocity });
            var rules = new PointRules(ctx.Court, (float)ctx.Params.FootprintRadius); rules.Begin(PlayerId.Human, PlayerId.Rival);
            ContactEvent? front = null, first = null, second = null; float apex = from.y; int next = 0, floors = 0; bool overTop = false;
            foreach (var state in trajectory.States.Skip(1))
            {
                while (next < trajectory.Contacts.Count && trajectory.Contacts[next].Step <= state.Step && !rules.Closed)
                {
                    var e = trajectory.Contacts[next++];
                    if (!e.Persistent)
                    {
                        if (!front.HasValue && e.Surface == SurfaceId.FrontWall) front = e;
                        if (e.Surface == SurfaceId.Floor) { if (++floors == 1) first = e; else if (floors == 2) second = e; }
                    }
                    rules.OnBallContact(e);
                }
                if (!rules.Closed) apex = Mathf.Max(apex, state.Position.y);
                if (!rules.Closed && state.Position.y > ctx.Court.WallHeight) { rules.OnOverTop(); overTop = true; }
                if (rules.Closed) break;
            }
            float W = ctx.Court.Width; bool right = from.x < W / 2;
            bool side = first.HasValue && second.HasValue && (line == "straight"
                ? (right ? first.Value.Point.x : W - first.Value.Point.x) <= 1 && (right ? second.Value.Point.x : W - second.Value.Point.x) <= 1
                : line == "cross" && (right ? W - second.Value.Point.x : second.Value.Point.x) <= 1 && (from.x - W / 2) * (second.Value.Point.x - W / 2) < 0);
            bool witness = rules.Cause == PointEndCause.DoubleBounce && front.HasValue && second.HasValue
                && front.Value.Point.y <= 1.2f && apex <= 2.13f && second.Value.Point.z >= 8 && side;
            cells[6] = rules.Cause.ToString(); cells[7] = F(overTop); cells[8] = front.HasValue ? F(front.Value.Point.y) : ""; cells[9] = F(apex);
            cells[10] = first.HasValue ? F(first.Value.Point.x) : ""; cells[11] = first.HasValue ? F(first.Value.Point.z) : "";
            cells[12] = second.HasValue ? F(second.Value.Point.x) : ""; cells[13] = second.HasValue ? F(second.Value.Point.z) : "";
            cells[14] = line == "" ? "" : F(witness);
            return string.Join("\t", cells);
        }

        [Test, Explicit, Category("Experiments"), Timeout(7200000)]
        public void WriteRallyPoints()
        {
            string variant = Environment.GetEnvironmentVariable("SC_A2_RALLY");
            Assert.That(variant == "before" || variant == "after", Is.True, "Use a predeclared variant.");
            var (root, preset, ctx) = Setup();
            string dir = Path.Combine(root, "Logs/experiments/g1a2-complete/rally-" + variant);
            Assert.That(Directory.Exists(dir), Is.False, "Never overwrite evidence.");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "source.tsv"), Source(root, preset, ctx), Utf8);
            float backLine = ctx.Court.Length - ctx.Court.ShortLineNearBackEdgeFromBack;
            int points = 0;
            var watch = Stopwatch.StartNew();
            using (var writer = new StreamWriter(Path.Combine(dir, "points.tsv"), false, Utf8))
            {
                writer.WriteLine("match\tpoint\tserver\twinner\tstriker\tcause\tterminal\tshots\tsteps\tendX\tendZ\tbackWin\tlastReception\tstrongShots\tsignature");
                for (int i = 0; points < 200; i++)
                {
                    using (var sim = new RallySim(ctx, new BotBrain(ctx, 30, 20261117 + 2 * i), new FondoAi(ctx, AiDifficulty.For(ctx.Params, AiLevel.Medium), 20261118 + 2 * i)))
                    {
                        var score = new MatchScore(7, 2, i % 2 == 0 ? PlayerId.Human : PlayerId.Rival); int point = 0;
                        while (!score.Finished && points < 200)
                        {
                            var o = sim.PlayPoint(score.Server, score.Box);
                            var end = sim.Ball.Position;
                            bool back = o.Cause == PointEndCause.DoubleBounce && end.z >= backLine;
                            int strong = o.Receptions.Count(r => r.Result == ReceptionResult.Contact && r.Power == ShotPower.Strong);
                            writer.WriteLine(string.Join("\t", F(i), F(point), o.Server?.ToString() ?? "", o.Winner?.ToString() ?? "", o.Striker?.ToString() ?? "", o.Cause, o.TerminalSurface,
                                F(o.Shots), F(o.Steps), F(end.x), F(end.z), F(back), o.LastReception?.ToString() ?? "", F(strong), FatigueMatchExperiment.Signature(o, sim)));
                            Assert.That(o.End, Is.EqualTo(RallyEnd.Point), "nonterminal point: preserve and stop");
                            Assert.That(o.SystemFaults, Is.Zero);
                            if (o.Winner.HasValue) score.PointWon(o.Winner.Value);
                            point++; points++;
                        }
                    }
                }
            }
            File.WriteAllText(Path.Combine(dir, "elapsed.tsv"), "points\tseconds\n" + points + "\t" + F(watch.Elapsed.TotalSeconds) + "\n", Utf8);
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }

        private static string Git(string root, string arguments)
        {
            using (var process = Process.Start(new ProcessStartInfo("git", arguments) { WorkingDirectory = root, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true }))
            { string output = process.StandardOutput.ReadToEnd(), error = process.StandardError.ReadToEnd(); process.WaitForExit(); if (process.ExitCode != 0) throw new IOException(error); return output; }
        }
    }
}
