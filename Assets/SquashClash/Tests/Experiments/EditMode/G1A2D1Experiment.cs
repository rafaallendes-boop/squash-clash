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
    // G1 A2 diagnostic D1 (read-only on product). Uses an instrumented local copy of the ShotIntentSolver loop
    // (eeb4a83) after proving it reproduces ShotIntentSolver.Solve bit for bit on the 24 C1 requests.
    public sealed class G1A2D1Experiment
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        private sealed class Cand
        {
            internal bool Solved; internal Vector3 Velocity; internal float Speed; internal Vector2 TargetUsed;
            internal bool FirstFront, Legal, BelowCeiling, CrossesMiddle, BecameBest;
            internal double Cost = double.PositiveInfinity, Change;
            internal double X2 = double.NaN, Z2 = double.NaN, Apex = double.NaN;
            internal int Index; internal string Phase = "";
        }

        private sealed class Domain
        {
            internal float Alpha, XLow, XHigh, YHigh, YLow, LegalLow; internal double XStar, ZStar, Ceiling;
            internal bool Active; // false = the solver returns the original untouched
        }

        private sealed class RunResult
        {
            internal bool Active; internal bool Solved; internal Vector3 Velocity; internal float Speed; internal Vector2 TargetUsed;
            internal bool SpeedRaised, TargetLowered; internal string Reason, SolReason; internal int Evaluations, BestIndex;
            internal List<Cand> Trace = new List<Cand>();
        }

        private sealed class Case
        {
            internal string Recipe, Line; internal float Power, Height; internal Vector3 From; internal Vector2 Aim; internal float Speed;
        }

        private RallyContext ctx;

        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void WriteG1A2D1()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string run = Environment.GetEnvironmentVariable("SC_A2_D1_RUN");
            Assert.That(run == "run01" || run == "run02", Is.True, "Set SC_A2_D1_RUN to run01 or run02.");
            Assert.That(Git(root, "status --porcelain"), Is.Empty, "Use clean, committed source.");
            string directory = Path.Combine(root, "Logs/experiments/g1-a2-d1", run);
            Assert.That(Directory.Exists(directory), Is.False, "Do not overwrite evidence.");
            var preset = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash, Is.EqualTo("430da2faa818"));
            ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            Assert.That(ctx.Config.SimulationKey, Is.EqualTo("166a96e319c7"));

            var cases = BuildCases();
            Assert.That(cases.Count, Is.EqualTo(24));

            // Equivalence gate: the instrumented copy must match the product solver bit for bit.
            var normal = new List<RunResult>();
            var equivalence = new List<string[]>();
            foreach (var k in cases)
            {
                var real = ShotIntentSolver.Solve(new ShotRequest(k.From, k.Aim, k.Power), k.Aim, k.Speed, ctx.VCap, ctx.Pace,
                    ctx.Court, ctx.Factory, ctx.Physics, (float)ctx.Params.FootprintRadius, out var info);
                var mine = Run(k, false);
                normal.Add(mine);
                bool same = real.Solved == mine.Solved && Bits(real.Velocity.x) == Bits(mine.Velocity.x) && Bits(real.Velocity.y) == Bits(mine.Velocity.y)
                    && Bits(real.Velocity.z) == Bits(mine.Velocity.z) && Bits(real.Speed) == Bits(mine.Speed)
                    && Bits(real.TargetUsed.x) == Bits(mine.TargetUsed.x) && Bits(real.TargetUsed.y) == Bits(mine.TargetUsed.y)
                    && real.Reason == mine.SolReason && real.SpeedRaised == mine.SpeedRaised && real.TargetLowered == mine.TargetLowered
                    && info.Evaluations == mine.Evaluations && info.Reason == mine.Reason;
                equivalence.Add(new[] { Id(k), F(real.TargetUsed.x), F(real.TargetUsed.y), F(mine.TargetUsed.x), F(mine.TargetUsed.y),
                    info.Evaluations.ToString(C), mine.Evaluations.ToString(C), same ? "True" : "False" });
                Assert.That(same, Is.True, "Instrumented copy differs from ShotIntentSolver.Solve on " + Id(k) + "; stop.");
            }
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);

            Directory.CreateDirectory(directory);
            Tsv(Path.Combine(directory, "equivalence.tsv"), "request\trealX\trealY\tcopyX\tcopyY\trealEvals\tcopyEvals\tbitIdentical", equivalence);

            var source = new List<string[]> { new[] { "defaults", preset.Hash }, new[] { "simulationKey", ctx.Config.SimulationKey },
                new[] { "head", Git(root, "rev-parse HEAD").Trim() }, new[] { "unity", Application.unityVersion }, new[] { "bounceThreshold", F(Physics.bounceThreshold) } };
            Tsv(Path.Combine(directory, "source.tsv"), "key\tvalue", source);

            // (a) index of the evaluation that produced the final best candidate + normal witness.
            var a = new List<string[]>();
            for (int i = 0; i < cases.Count; i++)
            {
                var w = Witness(cases[i], normal[i].Solved, normal[i].Velocity);
                a.Add(new[] { Id(cases[i]), normal[i].Evaluations.ToString(C), normal[i].Reason, normal[i].BestIndex.ToString(C),
                    normal[i].BestIndex > 0 ? normal[i].Trace[normal[i].BestIndex - 1].Phase : "", F(normal[i].TargetUsed.x), F(normal[i].TargetUsed.y), w.Pass.ToString(), F(w.SecondX) });
            }
            Tsv(Path.Combine(directory, "a-best-index.tsv"), "request\tevaluations\treason\tbestIndex\tbestPhase\ttargetX\ttargetY\twitness\tsecondX", a);

            // (b) full trace of the failing request.
            int fail = cases.FindIndex(k => k.Recipe == "SL6" && k.Line == "cross" && k.Power == .80f && k.Height == .88f);
            Assert.That(fail, Is.GreaterThanOrEqualTo(0));
            Tsv(Path.Combine(directory, "b-sl6-cross-p080-y088-trace.tsv"), TraceHeader, TraceRows(normal[fail]));

            // (c) existence exploration on the failing request and its passing neighbour.
            int neighbour = cases.FindIndex(k => k.Recipe == "SL6" && k.Line == "cross" && k.Power == .80f && k.Height == 1.08f);
            var cRows = new List<string[]>(); var cSummary = new List<string[]>();
            foreach (int index in new[] { fail, neighbour }) Explore(cases[index], cRows, cSummary);
            Tsv(Path.Combine(directory, "c-existence-grid.tsv"),
                "request\txRequested\tx\ty\tyIndex\tsolved\ttargetX\ttargetY\tinSearchDomain\tcause\tfrontY\tapex\tfirstX\tfirstZ\tsecondX\tsecondZ\toppositeWall2\twitness", cRows);
            Tsv(Path.Combine(directory, "c-existence-summary.tsv"),
                "request\tdomainXLow\tdomainXHigh\tdomainYLow\tdomainYHigh\tlegalLow\tpoints\twitnessAnywhere\twitnessInSearchDomain\tminOppositeWall2Legal\tminOppositeWall2InDomain", cSummary);

            // (d) lateral cap 0.
            var d = new List<string[]>(); int passes = 0, passesNormal = 0;
            for (int i = 0; i < cases.Count; i++)
            {
                var zero = Run(cases[i], true);
                var w = Witness(cases[i], zero.Solved, zero.Velocity);
                if (w.Pass) passes++;
                if (a[i][7] == "True") passesNormal++;
                d.Add(new[] { Id(cases[i]), zero.Evaluations.ToString(C), zero.Reason, zero.BestIndex.ToString(C), F(zero.TargetUsed.x), F(zero.TargetUsed.y),
                    F(cases[i].Aim.x), w.Pass.ToString(), w.Cause, F(w.FrontY), F(w.Apex), F(w.FirstX), F(w.SecondX), F(w.SecondZ), F(w.Opposite2) });
            }
            Tsv(Path.Combine(directory, "d-lateral-zero.tsv"), "request\tevaluations\treason\tbestIndex\ttargetX\ttargetY\trequestedX\twitness\tcause\tfrontY\tapex\tfirstX\tsecondX\tsecondZ\toppositeWall2", d);
            Tsv(Path.Combine(directory, "d-summary.tsv"), "metric\tvalue", new List<string[]> {
                new[] { "witnessesNormal", passesNormal.ToString(C) }, new[] { "witnessesLateralZero", passes.ToString(C) }, new[] { "requests", "24" } });
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }

        // ---- fixtures (copied from ShotIntentSolverTests.DriveBlock/C1) ----
        private List<Case> BuildCases()
        {
            var list = new List<Case>();
            foreach (string recipeId in new[] { "SL5", "SL6" })
                foreach (string line in new[] { "straight", "cross" })
                    foreach (float power in new[] { .70f, .75f, .80f })
                        foreach (float height in new[] { .88f, 1.08f })
                        {
                            var recipe = PracticeLaunchDefinitions.Create(ctx.Config).Single(r => r.Id == recipeId);
                            var incoming = ctx.Predict(recipe.Initial);
                            var plan = ContactPlanner.Plan(incoming, recipe.Body, ctx.Court, ctx.Legal, 1);
                            Assert.That(plan.Exists && plan.TStarStep >= 0, Is.True, "Real T* fixture");
                            var from = plan.PStar;
                            float near = from.x < ctx.Court.Width / 2 ? .6f : ctx.Court.Width - .6f;
                            float corner = line == "straight" ? near : ctx.Court.Width - near;
                            float through = from.x + (corner - from.x) * from.z / (from.z + ctx.Court.Length);
                            var aim = new Vector2(through, height);
                            var pace = ShotPace.Requested(power, from, aim, ctx.Physics.BallRadius, ctx.Pace);
                            list.Add(new Case { Recipe = recipeId, Line = line, Power = power, Height = height, From = from, Aim = aim, Speed = pace.Speed });
                        }
            return list;
        }

        private static string Id(Case k) => k.Recipe + "|" + k.Line + "|P" + k.Power.ToString("0.00", C) + "|y" + k.Height.ToString("0.00", C);

        // ---- witness predicate (verbatim from the C1 test) ----
        private struct WitnessResult
        {
            public bool Pass; public string Cause; public float FrontY, Apex, FirstX, FirstZ, SecondX, SecondZ, Opposite2;
        }

        private WitnessResult Witness(Case k, bool solved, Vector3 velocity)
        {
            var w = new WitnessResult { Cause = solved ? "" : "Unsolved", FrontY = float.NaN, Apex = float.NaN, FirstX = float.NaN, FirstZ = float.NaN, SecondX = float.NaN, SecondZ = float.NaN, Opposite2 = float.NaN };
            if (!solved) return w;
            var from = k.From;
            var trajectory = ctx.Predict(new BallState { Position = from, Velocity = velocity });
            var rules = new PointRules(ctx.Court, (float)ctx.Params.FootprintRadius);
            rules.Begin(PlayerId.Human, PlayerId.Rival);
            ContactEvent? front = null, first = null, second = null;
            float apex = from.y;
            int next = 0, floors = 0;
            foreach (var state in trajectory.States.Skip(1))
            {
                while (next < trajectory.Contacts.Count && trajectory.Contacts[next].Step <= state.Step && !rules.Closed)
                {
                    var e = trajectory.Contacts[next++];
                    if (!e.Persistent)
                    {
                        if (!front.HasValue && e.Surface == SurfaceId.FrontWall) front = e;
                        if (e.Surface == SurfaceId.Floor)
                        { if (++floors == 1) first = e; else if (floors == 2) second = e; }
                    }
                    rules.OnBallContact(e);
                }
                if (!rules.Closed) apex = Mathf.Max(apex, state.Position.y);
                if (!rules.Closed && state.Position.y > ctx.Court.WallHeight) rules.OnOverTop();
                if (rules.Closed) break;
            }
            bool right = from.x < ctx.Court.Width / 2;
            bool side = first.HasValue && second.HasValue && (k.Line == "straight"
                ? (right ? first.Value.Point.x : ctx.Court.Width - first.Value.Point.x) <= 1 && (right ? second.Value.Point.x : ctx.Court.Width - second.Value.Point.x) <= 1
                : (right ? ctx.Court.Width - second.Value.Point.x : second.Value.Point.x) <= 1 && (from.x - ctx.Court.Width / 2) * (second.Value.Point.x - ctx.Court.Width / 2) < 0);
            w.Pass = rules.Cause == PointEndCause.DoubleBounce && front.HasValue && second.HasValue
                && front.Value.Point.y <= 1.2f && apex <= 2.13f && second.Value.Point.z >= 8 && side;
            w.Cause = rules.Cause.ToString();
            w.FrontY = front?.Point.y ?? float.NaN; w.Apex = apex;
            w.FirstX = first?.Point.x ?? float.NaN; w.FirstZ = first?.Point.z ?? float.NaN;
            w.SecondX = second?.Point.x ?? float.NaN; w.SecondZ = second?.Point.z ?? float.NaN;
            if (second.HasValue) w.Opposite2 = right ? ctx.Court.Width - second.Value.Point.x : second.Value.Point.x;
            return w;
        }

        // ---- instrumented copy of ShotIntentSolver.Solve (eeb4a83) ----
        private Domain MakeDomain(Case k, ShotSolution original, bool zeroLateral)
        {
            var court = ctx.Court; var physics = ctx.Physics; var pace = ctx.Pace; var from = k.From; var requestedAim = k.Aim;
            var d = new Domain();
            double t = Math.Max(0, Math.Min(1, ((double)k.Power - .5) / .5));
            float distance = ShotPace.Requested(k.Power, from, requestedAim, physics.BallRadius, pace).DistanceM;
            double span = (double)pace.FullM - pace.NearM;
            double w = span > 0 ? Math.Max(0, Math.Min(1, ((double)pace.FullM - distance) / span)) : 0;
            if (t == 0 || w == 0 || from.y >= court.BackOutLower || from.z <= physics.BallRadius) return d;
            d.Alpha = (float)(t * w);
            d.XStar = Math.Max(court.Width / 10d, Math.Min(9 * court.Width / 10d,
                from.x + ((double)requestedAim.x - from.x) * (from.z + court.Length) / from.z));
            d.ZStar = court.Length * (.8 + .1 * t);
            d.Ceiling = from.y + (court.BackOutLower - from.y) * (1 - .5 * t);
            double lateral = zeroLateral ? 0d : d.Alpha * court.Width / 20d;
            d.XLow = Ceil(Math.Max(physics.BallRadius, requestedAim.x - lateral));
            d.XHigh = Flo(Math.Min(court.Width - physics.BallRadius, requestedAim.x + lateral));
            d.YHigh = Math.Min(k.Aim.y, requestedAim.y);
            d.LegalLow = Ceil((double)court.TinTop + physics.BallRadius);
            d.YLow = Ceil(d.YHigh + (d.LegalLow - (double)d.YHigh) * d.Alpha);
            d.Active = !(d.XLow > d.XHigh || d.YLow > d.YHigh);
            return d;
        }

        private RunResult Run(Case k, bool zeroLateral)
        {
            var court = ctx.Court; var physics = ctx.Physics; var from = k.From; var requestedAim = k.Aim;
            var original = ShotSolver.Solve(new ShotRequest(from, k.Aim, k.Power), k.Speed, k.Speed, ctx.VCap, physics.Gravity, physics.BallRadius, court, ctx.Factory, physics);
            var r = new RunResult { Evaluations = 0, Reason = "inactive" };
            void TakeOriginal()
            {
                r.Solved = original.Solved; r.Velocity = original.Velocity; r.Speed = original.Speed; r.TargetUsed = original.TargetUsed;
                r.SpeedRaised = original.SpeedRaised; r.TargetLowered = original.TargetLowered; r.SolReason = original.Reason;
            }
            if (!original.Solved) { TakeOriginal(); return r; }
            var dom = MakeDomain(k, original, zeroLateral);
            if (!dom.Active) { TakeOriginal(); return r; }
            float xLow = dom.XLow, xHigh = dom.XHigh, yHigh = dom.YHigh, yLow = dom.YLow;
            Vector2 Project(Vector2 aim) => new Vector2(Mathf.Clamp(aim.x, xLow, xHigh), Mathf.Clamp(aim.y, yLow, yHigh));

            int evaluations = 1;
            var baseline = Evaluate(original.Solved, original.Velocity, original.Speed, original.TargetUsed, from, requestedAim, dom);
            baseline.Index = 1; baseline.Phase = "baseline"; r.Trace.Add(baseline);
            var best = baseline;
            Cand Probe(Vector2 aim, string phase)
            {
                evaluations++;
                aim = Project(aim);
                var solution = ShotSolver.Solve(new ShotRequest(from, aim, k.Power), original.Speed, original.Speed, original.Speed,
                    physics.Gravity, physics.BallRadius, court, ctx.Factory, physics);
                bool solved = solution.Solved; var target = solution.TargetUsed; var velocity = solution.Velocity; float speed = solution.Speed;
                if (solved && (target.x < xLow || target.x > xHigh || target.y < yLow || target.y > yHigh))
                { solved = false; velocity = Vector3.zero; speed = 0; target = aim; }
                var c = Evaluate(solved, velocity, speed, target, from, requestedAim, dom);
                c.Index = evaluations; c.Phase = phase; r.Trace.Add(c);
                return c;
            }
            void Consider(Cand candidate)
            {
                if (baseline.Legal && !candidate.Legal) return;
                if (Compare(candidate, best) < 0) { best = candidate; candidate.BecameBest = true; }
            }

            var xs = new[] { k.Aim.x, xLow, xHigh };
            var ys = new[] { yHigh, yLow, (yHigh + yLow) * .5f };
            for (int ix = 0; ix < 3; ix++)
                for (int iy = 0; iy < 3; iy++)
                {
                    if (ix == 0 && iy == 0) continue;
                    Consider(Probe(new Vector2(xs[ix], ys[iy]), "seed" + ix + iy));
                }

            double trustX = court.Width / 40d, trustY = (court.FrontOutLower - court.TinTop) / 40d;
            for (int i = 0; i < 5; i++)
            {
                var origin = best;
                Vector2 at = Project(origin.TargetUsed);
                float dx = at.x + physics.BallRadius <= xHigh ? physics.BallRadius : -physics.BallRadius;
                float dy = at.y + physics.BallRadius <= yHigh ? physics.BallRadius : -physics.BallRadius;
                var xProbe = Probe(new Vector2(at.x + dx, at.y), "xProbe" + (i + 1));
                var yProbe = Probe(new Vector2(at.x, at.y + dy), "yProbe" + (i + 1));
                double Derivative(Cand sample, double difference) => origin.Legal && sample.Legal && difference != 0
                    ? (sample.Cost - origin.Cost) / difference : 0;
                double gx = Derivative(xProbe, xProbe.TargetUsed.x - (double)at.x) * trustX;
                double gy = Derivative(yProbe, yProbe.TargetUsed.y - (double)at.y) * trustY;
                double norm = Math.Max(Math.Abs(gx), Math.Abs(gy));
                var proposal = norm > 0 ? new Vector2((float)(at.x - trustX * gx / norm), (float)(at.y - trustY * gy / norm)) : at;
                var refined = Probe(proposal, "refine" + (i + 1));
                Consider(xProbe); Consider(yProbe); Consider(refined);
            }

            bool changed = Compare(best, baseline) < 0;
            r.Active = true; r.Evaluations = evaluations; r.Reason = changed ? "intent-drive" : "fallback-original-no-improvement";
            r.BestIndex = best.Index;
            if (!changed) { TakeOriginal(); return r; }
            r.Solved = best.Solved; r.Velocity = best.Velocity; r.Speed = best.Speed; r.TargetUsed = best.TargetUsed;
            r.SpeedRaised = original.SpeedRaised; r.TargetLowered = best.TargetUsed.y < k.Aim.y;
            r.Reason = "intent-drive"; r.SolReason = "intent-drive";
            return r;
        }

        private Cand Evaluate(bool solved, Vector3 velocity, float speed, Vector2 target, Vector3 from, Vector2 requested, Domain dom)
        {
            var court = ctx.Court; var physics = ctx.Physics;
            var result = new Cand { Solved = solved, Velocity = velocity, Speed = speed, TargetUsed = target };
            double dx = (double)target.x - requested.x, dy = (double)target.y - requested.y;
            result.Change = dx * dx + dy * dy;
            if (!solved) return result;
            var rules = new PointRules(court, (float)ctx.Params.FootprintRadius);
            rules.Begin(PlayerId.Human, PlayerId.Rival);
            bool firstContact = true;
            float apex = from.y;
            ContactEvent? first = null, second = null;
            using (var simulation = ctx.Factory.Create(physics, court))
            {
                simulation.Reset(new BallState { Position = from, Velocity = velocity });
                var contacts = new List<ContactEvent>(4);
                for (int step = 1; step <= physics.StepHz * 10 && !rules.Closed; step++)
                {
                    contacts.Clear(); simulation.Step(contacts);
                    foreach (var hit in contacts)
                    {
                        if (rules.Closed) break;
                        if (!hit.Persistent && firstContact) { result.FirstFront = hit.Surface == SurfaceId.FrontWall; firstContact = false; }
                        if (!hit.Persistent && hit.Surface == SurfaceId.Floor)
                        {
                            if (rules.FloorBounces == 0) first = hit;
                            else if (rules.FloorBounces == 1) second = hit;
                        }
                        rules.OnBallContact(hit);
                    }
                    if (!rules.Closed) apex = Math.Max(apex, simulation.State.Position.y);
                    if (!rules.Closed && simulation.State.Position.y > court.WallHeight) rules.OnOverTop();
                }
            }
            result.Legal = rules.Cause == PointEndCause.DoubleBounce && first.HasValue && second.HasValue;
            result.BelowCeiling = apex <= dom.Ceiling;
            result.Apex = apex;
            if (second.HasValue) { result.X2 = second.Value.Point.x; result.Z2 = second.Value.Point.z; }
            if (result.Legal)
            {
                double half = court.Width / 2d;
                bool cross = (from.x - half) * (dom.XStar - half) < 0;
                bool right = from.x < half;
                double x1 = first.Value.Point.x, x2 = second.Value.Point.x;
                double own1 = right ? x1 : court.Width - x1;
                double own2 = right ? x2 : court.Width - x2;
                double opposite2 = right ? court.Width - x2 : x2;
                double lateral = cross ? opposite2 : own1 + own2;
                result.CrossesMiddle = !cross || (from.x - half) * (x2 - half) < 0;
                result.Cost = lateral / court.Width + Math.Abs(second.Value.Point.z - dom.ZStar) / court.Length
                    + Math.Abs(apex - dom.Ceiling) / court.BackOutLower;
            }
            return result;
        }

        private static int Compare(Cand a, Cand b)
        {
            int c = b.Solved.CompareTo(a.Solved); if (c != 0) return c;
            c = b.FirstFront.CompareTo(a.FirstFront); if (c != 0) return c;
            c = b.Legal.CompareTo(a.Legal); if (c != 0) return c;
            c = b.BelowCeiling.CompareTo(a.BelowCeiling); if (c != 0) return c;
            c = b.CrossesMiddle.CompareTo(a.CrossesMiddle); if (c != 0) return c;
            c = a.Cost.CompareTo(b.Cost); if (c != 0) return c;
            c = a.Change.CompareTo(b.Change); if (c != 0) return c;
            c = a.TargetUsed.x.CompareTo(b.TargetUsed.x); if (c != 0) return c;
            return a.TargetUsed.y.CompareTo(b.TargetUsed.y);
        }

        private static float Ceil(double value)
        {
            float rounded = (float)value;
            return rounded < value ? BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(rounded) + 1) : rounded;
        }
        private static float Flo(double value)
        {
            float rounded = (float)value;
            return rounded > value ? BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(rounded) - 1) : rounded;
        }

        // ---- (b) ----
        private const string TraceHeader = "index\tphase\tTargetUsed.x\tTargetUsed.y\tSolved\tFirstFront\tLegal\tBelowCeiling\tCrossesMiddle\tCost\tx2\tz2\tapex\tbecameBest\tisFinalBest";
        private List<string[]> TraceRows(RunResult r)
        {
            var rows = new List<string[]>();
            foreach (var c in r.Trace)
                rows.Add(new[] { c.Index.ToString(C), c.Phase, F(c.TargetUsed.x), F(c.TargetUsed.y), c.Solved.ToString(), c.FirstFront.ToString(), c.Legal.ToString(),
                    c.BelowCeiling.ToString(), c.CrossesMiddle.ToString(), F(c.Cost), F(c.X2), F(c.Z2), F(c.Apex), c.BecameBest.ToString(), (c.Index == r.BestIndex).ToString() });
            return rows;
        }

        // ---- (c) ----
        private void Explore(Case k, List<string[]> rows, List<string[]> summary)
        {
            var original = ShotSolver.Solve(new ShotRequest(k.From, k.Aim, k.Power), k.Speed, k.Speed, ctx.VCap, ctx.Physics.Gravity, ctx.Physics.BallRadius, ctx.Court, ctx.Factory, ctx.Physics);
            Assert.That(original.Solved, Is.True);
            var dom = MakeDomain(k, original, false);
            Assert.That(dom.Active, Is.True);
            var xs = new List<float>();
            for (int i = 0; ; i++)
            {
                float x = (float)(dom.XLow + .02 * i);
                if (x > dom.XHigh) break;
                xs.Add(x);
            }
            if (xs[xs.Count - 1] < dom.XHigh - 1e-6f) xs.Add(dom.XHigh);
            int points = 0, anywhere = 0, inDomain = 0; double minAll = double.PositiveInfinity, minDom = double.PositiveInfinity;
            foreach (float x in xs)
                for (int iy = 0; iy < 5; iy++)
                {
                    float y = (float)(dom.LegalLow + ((double)dom.YHigh - dom.LegalLow) * iy / 4d);
                    var aim = new Vector2(x, y);
                    var solution = ShotSolver.Solve(new ShotRequest(k.From, aim, k.Power), original.Speed, original.Speed, original.Speed,
                        ctx.Physics.Gravity, ctx.Physics.BallRadius, ctx.Court, ctx.Factory, ctx.Physics);
                    bool inside = solution.Solved && solution.TargetUsed.x >= dom.XLow && solution.TargetUsed.x <= dom.XHigh
                        && solution.TargetUsed.y >= dom.YLow && solution.TargetUsed.y <= dom.YHigh;
                    var w = Witness(k, solution.Solved, solution.Velocity);
                    points++;
                    if (w.Pass) anywhere++;
                    if (w.Pass && inside) inDomain++;
                    if (!float.IsNaN(w.Opposite2) && w.Cause == "DoubleBounce")
                    {
                        minAll = Math.Min(minAll, w.Opposite2);
                        if (inside) minDom = Math.Min(minDom, w.Opposite2);
                    }
                    rows.Add(new[] { Id(k), F(k.Aim.x), F(x), F(y), iy.ToString(C), solution.Solved.ToString(), F(solution.TargetUsed.x), F(solution.TargetUsed.y), inside.ToString(),
                        w.Cause, F(w.FrontY), F(w.Apex), F(w.FirstX), F(w.FirstZ), F(w.SecondX), F(w.SecondZ), F(w.Opposite2), w.Pass.ToString() });
                }
            summary.Add(new[] { Id(k), F(dom.XLow), F(dom.XHigh), F(dom.YLow), F(dom.YHigh), F(dom.LegalLow), points.ToString(C), anywhere.ToString(C), inDomain.ToString(C),
                double.IsInfinity(minAll) ? "NaN" : minAll.ToString("R", C), double.IsInfinity(minDom) ? "NaN" : minDom.ToString("R", C) });
        }

        // ---- IO ----
        private static int Bits(float v) => BitConverter.SingleToInt32Bits(v);
        private static string F(float v) => v.ToString("R", C);
        private static string F(double v) => v.ToString("R", C);

        private static void Tsv(string path, string header, List<string[]> rows)
        {
            using (var writer = new StreamWriter(path, false, Utf8))
            {
                writer.NewLine = "\n";
                writer.WriteLine(header);
                foreach (var row in rows) writer.WriteLine(string.Join("\t", row));
            }
        }

        private static string Git(string root, string arguments)
        {
            using (var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("git", arguments) { WorkingDirectory = root, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true }))
            { string output = process.StandardOutput.ReadToEnd(), error = process.StandardError.ReadToEnd(); process.WaitForExit(); if (process.ExitCode != 0) throw new IOException(error); return output; }
        }
    }
}
