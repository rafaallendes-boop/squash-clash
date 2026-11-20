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
    // Two-wall shot (design docs/superpowers/specs/2026-11-20-boast-predeclaration.md). Explicit, outside regression and AutoQA.
    // Targets fixed BEFORE these runs: M1 >= 90 % verified boasts on the grid below and >= 80 % of them on the opposite front half;
    // M3 from the back quarter: Definidor 5-15 %, Fondista <= 5 %, Completo between. Evidence under Logs/experiments/boast/<run>,
    // never overwritten (SC_BOAST_RUN names the folder).
    public sealed class BoastExperiment
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private static readonly RivalStyleId[] Styles = { RivalStyleId.Fondista, RivalStyleId.Definidor, RivalStyleId.Completo };
        private static string F(double v) => v.ToString("R", C);
        private static (ParamSet preset, AnalyticBallSimulationFactory factory) Setup()
        {
            var preset = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash, Is.EqualTo("207da8f2c636")); Assert.That(preset.Ordered.Count, Is.EqualTo(132));
            return (preset, new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
        }
        private static string Dir(string run)
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string dir = Path.Combine(root, "Logs/experiments/boast/" + (Environment.GetEnvironmentVariable("SC_BOAST_RUN") ?? run));
            Assert.That(Directory.Exists(dir), Is.False, "Never overwrite evidence."); Directory.CreateDirectory(dir); return dir;
        }

        // M1 grid (fixed before data, 20 nov): contacts in the back quarter near each side wall, x from the wall {0.5, 0.9, 1.3} m,
        // z {7.5, 8.1, 8.7, 9.2} m, height {0.5, 0.9} m; aims at the NEAR side wall at depth z_c - {0.6, 1.0, 1.5} m and at the two
        // depths the gesture admits L/2 and L/2 - 1.5 m, heights {0.9, 1.5, 2.1} m; normal power P = 0.5 through the boast pace.
        // Reported overall (the M1 verdict) and split by whether the gesture can aim that point (z <= L/2).
        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void M1Grid()
        {
            var (preset, factory) = Setup(); var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), factory); var court = ctx.Court;
            string dir = Dir("m1");
            int total = 0, valid = 0, opposite = 0, exact = 0, admittedTotal = 0, admittedValid = 0, admittedOpposite = 0, maxEval = 0; long evals = 0;
            using (var rows = new StreamWriter(Path.Combine(dir, "grid.tsv"), false, Utf8))
            {
                rows.WriteLine("wall\tcx\tcy\tcz\taimZ\taimY\tadmitted\tsolved\treason\tusedZ\tusedY\tspeed\tfrontX\tfrontY\topposite\tevaluations");
                foreach (var wall in new[] { SurfaceId.RightWall, SurfaceId.LeftWall })
                    foreach (float off in new[] { .5f, .9f, 1.3f })
                        foreach (float cz in new[] { 7.5f, 8.1f, 8.7f, 9.2f })
                            foreach (float cy in new[] { .5f, .9f })
                            {
                                var from = new Vector3(wall == SurfaceId.RightWall ? off : court.Width - off, cy, cz);
                                foreach (float az in new[] { cz - .6f, cz - 1f, cz - 1.5f, court.Length * .5f, court.Length * .5f - 1.5f })
                                    foreach (float ay in new[] { .9f, 1.5f, 2.1f })
                                    {
                                        var aim = new SideAim(wall, az, ay); bool admitted = az <= court.Length * .5f;
                                        var pace = ShotPace.Boast(.5f, from, aim, court, ctx.Physics.BallRadius, ctx.Pace);
                                        var s = BoastSolver.Solve(from, aim, pace.Speed, ctx.VMin, ctx.VCap, court, factory, ctx.Physics, (float)ctx.Params.FootprintRadius, out var info);
                                        bool opp = s.Solved && (info.Front.x - court.Width * .5f) * (from.x - court.Width * .5f) < 0;
                                        total++; if (admitted) admittedTotal++;
                                        if (s.Solved) { valid++; if (opp) opposite++; if (info.Exact) exact++; if (admitted) { admittedValid++; if (opp) admittedOpposite++; } }
                                        evals += info.Evaluations; maxEval = Math.Max(maxEval, info.Evaluations);
                                        rows.WriteLine(string.Join("\t", wall, F(from.x), F(cy), F(cz), F(az), F(ay), admitted, s.Solved, s.Reason, F(info.Used.Z), F(info.Used.Y), F(info.Speed), F(info.Front.x), F(info.Front.y), opp, info.Evaluations));
                                    }
                            }
            }
            string summary = "total\tvalid\tvalidShare\topposite\toppositeShareOfValid\texact\tadmittedTotal\tadmittedValid\tadmittedOpposite\tmeanEvaluations\tmaxEvaluations\n" +
                string.Join("\t", total, valid, F(valid / (double)total), opposite, F(valid == 0 ? 0 : opposite / (double)valid), exact, admittedTotal, admittedValid, admittedOpposite, F(evals / (double)total), maxEval) + "\n";
            File.WriteAllText(Path.Combine(dir, "summary.tsv"), summary, Utf8); TestContext.WriteLine(summary);
        }

        // Decorates the rival to count, at each decision, what the boast offered: options, forecasts and the choice (no draw is consumed).
        private sealed class Probe : IRallyBrain, IRallyPositioning, IRallyServer
        {
            private readonly RivalAi inner; private readonly RallyContext ctx;
            public int Decisions, BackQuarter, WithBoasts, Defensive, Forecasts, Solved, Good, ChosenBoasts; public readonly Dictionary<string, int> Faults = new Dictionary<string, int>();
            public Probe(RivalAi inner, RallyContext ctx) { this.inner = inner; this.ctx = ctx; }
            public int PerceptionSteps => inner.PerceptionSteps;
            public Vector2 StanceNoise() => inner.StanceNoise();
            public Vector2 Wait => inner.Wait;
            public Vector2 Recovery(in AiObservation o) => inner.Recovery(o);
            public ServeType ChooseServe(ServiceBox box) => inner.ChooseServe(box);
            public AiShot Decide(in AiObservation o, in ReceptionPlan own)
            {
                var shot = inner.Decide(o, own);
                if (!own.Reachable || !own.Contact.Exists || own.Contact.TStarStep < 0) return shot;
                Decisions++; var at = own.Contact.PStar;
                if (at.z >= ctx.Params.BoastBackFraction * ctx.Court.Length) BackQuarter++;
                var options = new List<ShotOption>(); ShotOptions.AddBoasts(ctx, at, options);
                if (options.Count > 0) WithBoasts++;
                if (options.Count > 0 && own.SlackS < inner.Style.DefenseSlackS) Defensive++;
                foreach (var b in options)
                {
                    Forecasts++; var f = RallyContact.ForecastBoast(ctx, at, b.Side, b.P);
                    if (f.Solved) Solved++; if (f.Good) Good++;
                    if (f.Solved && !f.Good) { string k = f.Fault.ToString(); Faults[k] = Faults.TryGetValue(k, out var n) ? n + 1 : 1; }
                }
                if (shot.Option.Line == ShotLine.Boast) ChosenBoasts++;
                return shot;
            }
        }
        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void RivalDiagnosis()
        {
            var (preset, factory) = Setup(); string dir = Dir("diag");
            var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams { BoastRival = 1, BoastWeightShooter = 3 }, factory);
            var probe = new Probe(RivalAi.For(ctx, AiLevel.Medium, RivalStyleId.Definidor, 20261201), ctx);
            for (int i = 0; i < 10; i++)
                using (var sim = new RallySim(ctx, new RivalStylesBaselineExperiment.NewcomerBot(ctx, 90, 20261200 + 2 * i), probe))
                {
                    var score = new MatchScore(7, 2, PlayerId.Human); int replays = 0;
                    while (!score.Finished) { var o = sim.PlayPoint(score.Server, score.Box); if (o.Winner.HasValue) score.PointWon(o.Winner.Value); else if (++replays > 50) break; }
                }
            string text = "decisions\tbackQuarter\twithBoasts\tdefensive\tforecasts\tsolved\tgood\tchosenBoasts\tfaults\n" +
                string.Join("\t", probe.Decisions, probe.BackQuarter, probe.WithBoasts, probe.Defensive, probe.Forecasts, probe.Solved, probe.Good, probe.ChosenBoasts,
                string.Join(",", probe.Faults.Select(kv => kv.Key + "=" + kv.Value))) + "\n";
            File.WriteAllText(Path.Combine(dir, "diag.tsv"), text, Utf8); TestContext.WriteLine(text);
        }

        private sealed class Share { public int Strokes, Back, BackBoasts, Boasts, Points, Human, Faults; public double Of => Back == 0 ? 0 : BackBoasts / (double)Back; }
        private static Share Play(RallyContext ctx, AiLevel level, RivalStyleId style, int matches, long seeds)
        {
            var share = new Share(); float back = (float)(ctx.Params.BoastBackFraction * ctx.Court.Length);
            for (int i = 0; i < matches; i++)
                using (var sim = new RallySim(ctx, new RivalStylesBaselineExperiment.NewcomerBot(ctx, 90, seeds + 2 * i), RivalAi.For(ctx, level, style, seeds + 1 + 2 * i)))
                {
                    var score = new MatchScore(7, 2, i % 2 == 0 ? PlayerId.Human : PlayerId.Rival); int replays = 0;
                    while (!score.Finished)
                    {
                        var o = sim.PlayPoint(score.Server, score.Box); share.Faults += o.SystemFaults;
                        foreach (var r in o.Receptions)
                        {
                            if (r.Who != PlayerId.Rival || r.Result != ReceptionResult.Contact || r.ContactPoint == default) continue;
                            share.Strokes++; bool boast = r.Line == ShotLine.Boast; if (boast) share.Boasts++;
                            if (r.ContactPoint.z >= back) { share.Back++; if (boast) share.BackBoasts++; }
                        }
                        if (o.Winner.HasValue) { score.PointWon(o.Winner.Value); share.Points++; if (o.Winner == PlayerId.Human) share.Human++; }
                        else if (++replays > 50) break;
                    }
                }
            return share;
        }
        private const string ShareHeader = "phase\tlevel\tstyle\tweight\tmatches\tstrokes\tbackStrokes\tbackBoasts\tbackShare\tboasts\tpoints\thuman\tsystemFaults\tseconds";
        private static string Row(string phase, AiLevel level, RivalStyleId style, double weight, int matches, Share s, double seconds)
            => string.Join("\t", phase, level, style, F(weight), matches, s.Strokes, s.Back, s.BackBoasts, F(s.Of), s.Boasts, s.Points, s.Human, s.Faults, F(seconds));
        private static RallyParams WithWeight(RivalStyleId style, double w)
        {
            var p = new RallyParams { BoastRival = 1 };
            if (style == RivalStyleId.Definidor) p.BoastWeightShooter = w; else if (style == RivalStyleId.Completo) p.BoastWeightAllRound = w; else p.BoastWeightFondo = w;
            return p;
        }
        // M3 search (rule fixed before data, 20 nov): Medium, newcomer (sigma 90), 40 matches per weight from the grid below on the search
        // seeds; per style the weight whose back-quarter boast share is closest to its centre (Fondista 2.5 %, Completo 6 %, Definidor 10 %),
        // ties to the lower weight. The chosen weights go to RallyParams and M3Confirm judges them with new seeds.
        private static readonly double[] WeightGrid = { -3, -2, -1, -.5, 0, .5, 1, 1.5, 2, 3 };
        [Test, Explicit, Category("Experiments"), Timeout(14400000)]
        public void M3Search()
        {
            var (preset, factory) = Setup(); string dir = Dir("m3-search");
            using (var w = new StreamWriter(Path.Combine(dir, "search.tsv"), false, Utf8))
            using (var chosen = new StreamWriter(Path.Combine(dir, "chosen.tsv"), false, Utf8))
            {
                w.WriteLine(ShareHeader); chosen.WriteLine("style\tweight\tshare\tcentre");
                foreach (var style in Styles)
                {
                    double centre = style == RivalStyleId.Fondista ? .025 : style == RivalStyleId.Completo ? .06 : .10, bestGap = double.MaxValue, best = double.NaN, bestShare = 0;
                    foreach (var weight in WeightGrid)
                    {
                        var watch = Stopwatch.StartNew(); var ctx = new RallyContext(EffectiveConfig.From(preset), WithWeight(style, weight), factory);
                        var s = Play(ctx, AiLevel.Medium, style, 40, 20261130); w.WriteLine(Row("search", AiLevel.Medium, style, weight, 40, s, watch.Elapsed.TotalSeconds)); w.Flush();
                        double gap = Math.Abs(s.Of - centre); if (gap < bestGap) { bestGap = gap; best = weight; bestShare = s.Of; }
                    }
                    chosen.WriteLine(style + "\t" + F(best) + "\t" + F(bestShare) + "\t" + F(centre)); chosen.Flush();
                }
            }
        }
        // M3 confirm: the RallyParams defaults (with the rival's boast switched on), new seeds, 100 matches per style and level; the M3
        // verdict is read at Medium, Easy and Hard are reported.
        [Test, Explicit, Category("Experiments"), Timeout(14400000)]
        public void M3Confirm()
        {
            var (preset, factory) = Setup(); string dir = Dir("m3-confirm");
            var p = new RallyParams { BoastRival = 1 }; var ctx = new RallyContext(EffectiveConfig.From(preset), p, factory);
            File.WriteAllText(Path.Combine(dir, "source.tsv"), "defaults\trallyParams\tdefaultRallyParams\tunity\n" + preset.Hash + "\t" + p.Hash + "\t" + new RallyParams().Hash + "\t" + Application.unityVersion + "\n", Utf8);
            using (var w = new StreamWriter(Path.Combine(dir, "confirm.tsv"), false, Utf8))
            {
                w.WriteLine(ShareHeader);
                foreach (var level in new[] { AiLevel.Medium, AiLevel.Easy, AiLevel.Hard })
                    foreach (var style in Styles)
                    {
                        double weight = style == RivalStyleId.Definidor ? p.BoastWeightShooter : style == RivalStyleId.Completo ? p.BoastWeightAllRound : p.BoastWeightFondo;
                        var watch = Stopwatch.StartNew(); var s = Play(ctx, level, style, 100, 20261231);
                        w.WriteLine(Row("confirm", level, style, weight, 100, s, watch.Elapsed.TotalSeconds)); w.Flush();
                    }
            }
        }

        // Diagnostic only (no target): what a slow, low boast from the back corner meets, height by height and speed by speed, and what
        // the rival sees against the plain bot of the regression test.
        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void SolverDiagnosis()
        {
            var (preset, factory) = Setup(); var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), factory); var court = ctx.Court;
            var from = new Vector3(.8f, .7f, 8.4f); var sb = new StringBuilder("speed\tz\ty\tlaunch\tcontacts\n");
            foreach (float speed in new[] { 10.4f, 12f, 14f, 17f })
                foreach (float z in new[] { 7.6f, 7.2f })
                    for (float y = .25f; y <= 3.01f; y += .25f)
                    {
                        var side = new SideAim(SurfaceId.RightWall, z, y); var c = side.Center(court, ctx.Physics.BallRadius);
                        var s = BoastSolver.Solve(from, side, speed, speed, speed, court, factory, ctx.Physics, .02f, out var info);
                        var list = new List<string>();
                        if (s.Solved) list.Add("SOLVED " + info.Reason + " used " + info.Used);
                        else
                        {
                            // Replay the plain low-arc launch to this exact point.
                            double dx = c.x - from.x, dz = c.z - from.z, d2 = dx * dx + dz * dz, h = c.y - from.y, v2 = speed * speed, g = ctx.Physics.Gravity;
                            double disc = v2 * v2 - g * (g * d2 + 2 * h * v2);
                            if (disc < 0) list.Add("unreachable");
                            else
                            {
                                double t = Math.Sqrt(2 * (d2 + h * h) / (v2 - g * h + Math.Sqrt(disc)));
                                var v = new Vector3((float)(dx / t), (float)(h / t + .5 * g * t), (float)(dz / t));
                                using (var sim = factory.Create(ctx.Physics, court))
                                {
                                    sim.Reset(new BallState { Position = from, Velocity = v }); var ev = new List<ContactEvent>();
                                    for (int i = 0; i < ctx.StepHz * 4 && list.Count < 4; i++) { ev.Clear(); sim.Step(ev); foreach (var e in ev) list.Add(e.Surface + (e.Persistent ? "*" : "") + "(" + F(Math.Round(e.Point.x, 2)) + "," + F(Math.Round(e.Point.y, 2)) + "," + F(Math.Round(e.Point.z, 2)) + ")"); }
                                }
                            }
                        }
                        sb.Append(F(speed) + "\t" + F(z) + "\t" + F(Math.Round(y, 2)) + "\t" + string.Join(" ", list) + "\n");
                    }
            var bctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams { BoastRival = 1, BoastWeightShooter = 3 }, factory);
            var probe = new Probe(RivalAi.For(bctx, AiLevel.Medium, RivalStyleId.Definidor, 101), bctx);
            for (int m = 0; m < 6; m++)
                using (var sim = new RallySim(bctx, new BotBrain(bctx, 30, 100 + 2 * m), probe))
                {
                    var score = new MatchScore(7, 2, PlayerId.Human); int guard = 0;
                    while (!score.Finished && guard++ < 60) { var o = sim.PlayPoint(score.Server, score.Box); if (o.Winner.HasValue) score.PointWon(o.Winner.Value); }
                }
            sb.Append("bot30: decisions " + probe.Decisions + " back " + probe.BackQuarter + " withBoasts " + probe.WithBoasts + " defensive " + probe.Defensive + " forecasts " + probe.Forecasts + " solved " + probe.Solved + " good " + probe.Good + " chosen " + probe.ChosenBoasts + "\n");
            File.WriteAllText(Path.Combine(Dir("solver-diag"), "diag.tsv"), sb.ToString(), Utf8); TestContext.WriteLine(sb.ToString());
        }
    }
}
