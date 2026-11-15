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
    // Explicit diagnosis of the Unreachable increase with the distance-aware pace curve.
    // Method (predeclared): docs/validation/2026-11-15-unreachable-diagnosis-method.md. Read-only: no product code is touched.
    // Not part of regression or AutoQA.
    public sealed class UnreachableDiagnosisExperiment
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private static readonly double[] Gammas = { 1.0, 2.0 };
        private const int Rallies = 1000;

        private sealed class Rec
        {
            public int Rally, K; public PlayerId Who;
            public BallState Ball; public Vector2 Self, SelfV, Opp, OppV, Noise;
            public bool Reachable; public string Reason; public long ArrivalStep; public double Slack; public bool HasContact; public long TStar; public Vector3 PStar; public Vector2 Stance;
            public bool Swing, Defensive; public ShotOption Option; public Vector2 Aim; public float P; public double DeltaMs;
            public int Delay; public int FloorBounces, Shots; public bool FrontWallReached;
            // mirror of the planner
            public bool Mirrored; public int Candidates, ReachableCandidates; public long BestLateAvail = long.MaxValue, BestLateStar = long.MaxValue, BestAvailSteps = -1, BestArrivalSteps = -1;
            // independent probe
            public bool ProbeReach, ProbeContact; public long ProbeAvail = -1, ProbeArrival = -1;
            // receiver at the second bounce
            public Vector2 SelfAtSecond; public bool HasSelfAtSecond;
        }

        private sealed class Recorder : IRallyBrain
        {
            private readonly IRallyBrain inner; private readonly PlayerId who; private readonly List<Rec> sink; private readonly RallyContext ctx; private readonly int rally;
            public RallySim Sim; private Vector2 noise;
            public Recorder(IRallyBrain inner, PlayerId who, List<Rec> sink, RallyContext ctx, int rally) { this.inner = inner; this.who = who; this.sink = sink; this.ctx = ctx; this.rally = rally; }
            public int PerceptionSteps => inner.PerceptionSteps;
            public Vector2 StanceNoise() { noise = inner.StanceNoise(); return noise; }
            public AiShot Decide(in AiObservation observation, in ReceptionPlan own)
            {
                var shot = inner.Decide(observation, own);
                var body = who == PlayerId.Human ? Sim.Human : Sim.Rival;
                var r = new Rec
                {
                    Rally = rally, K = sink.Count, Who = who, Ball = observation.Ball, Self = observation.SelfPosition, SelfV = observation.SelfVelocity, Opp = observation.OpponentPosition, OppV = observation.OpponentVelocity,
                    Noise = noise, Reachable = own.Reachable, Reason = own.Reason, ArrivalStep = own.ArrivalStep, Slack = own.SlackS, HasContact = own.Contact.Exists && own.Contact.TStarStep >= 0, TStar = own.Contact.TStarStep, PStar = own.Contact.PStar, Stance = own.Stance,
                    Swing = shot.Swing, Defensive = shot.Defensive, Option = shot.Option, Aim = shot.Aim, P = shot.P, DeltaMs = shot.DeltaMs,
                    FloorBounces = observation.FloorBounces, Shots = observation.Shots, FrontWallReached = observation.FrontWallReached
                };
                r.Delay = ctx.Steps(ctx.Params.ReactionS) + inner.PerceptionSteps;
                if(!own.Reachable) MirrorAndProbe(r, body);
                sink.Add(r);
                return shot;
            }

            private static float Height(BodyFrame frame, Vector3 ball)
            {
                float gap = ball.y < frame.PreferredMinHeight ? frame.PreferredMinHeight - ball.y : ball.y > frame.PreferredMaxHeight ? ball.y - frame.PreferredMaxHeight : 0;
                return Mathf.Max(0, 1 - gap / (frame.PreferredMaxHeight - frame.PreferredMinHeight));
            }
            private static float Wall(BodyFrame frame, CourtDimensions court, Vector3 ball)
            {
                float clearance = Mathf.Min(Mathf.Min(ball.x, court.Width - ball.x), Mathf.Min(ball.z, court.Length - ball.z));
                return frame.WallClearance <= 0 ? 1 : Mathf.Clamp01(clearance / frame.WallClearance);
            }

            // Read-only mirror of ReceptionPlanner's candidate loop (same body, same noise) plus an independent probe.
            private void MirrorAndProbe(Rec r, PlayerBody body)
            {
                var p = ctx.Params; var legal = ctx.Legal; var court = ctx.Court; var frame = ctx.FrameAt(body.Position);
                var trajectory = ctx.Predict(r.Ball);
                if(trajectory.States.Count < 2) return;
                long start = trajectory.States[0].Step; var rules = Sim.Rules.Clone(); var who2 = rules.ToHit; int next = 0;
                var window = new List<(long step, Vector3 pos, float feature)>(256);
                for(int i = 1; i < trajectory.States.Count && !rules.Closed; i++)
                {
                    var state = trajectory.States[i];
                    while(next < trajectory.Contacts.Count && trajectory.Contacts[next].Step <= state.Step) rules.OnBallContact(trajectory.Contacts[next++]);
                    if(rules.Closed || !rules.CanHit(who2) || state.Position.y < legal.MinHeight || state.Position.y > legal.MaxHeight) continue;
                    window.Add((state.Step, state.Position, Height(frame, state.Position) + Wall(frame, court, state.Position)));
                }
                r.Mirrored = true;
                if(window.Count == 0) return;
                int interval = Math.Max(1, ctx.Steps(p.CandidateIntervalS)); float reach2 = frame.Reach * frame.Reach;
                foreach(float side in new float[] { -1, 1 })
                    for(int index = 0; index < window.Count; index++)
                    {
                        var sample = window[index];
                        if((sample.step - start) % interval != 0) continue;
                        int available = (int)(sample.step - start);
                        if(available < 0) continue;
                        var stance = ctx.ClampBody(new Vector2(sample.pos.x + side * frame.IdealLateral, sample.pos.z) + r.Noise);
                        bool In(int k) { var d = window[k].pos - new Vector3(stance.x, frame.ReferenceHeight, stance.y); return d.sqrMagnitude <= reach2; }
                        if(!In(index)) continue;
                        int first = index, last = index;
                        while(first > 0 && window[first - 1].step == window[first].step - 1 && In(first - 1)) first--;
                        while(last + 1 < window.Count && window[last + 1].step == window[last].step + 1 && In(last + 1)) last++;
                        int star = first; for(int k = first + 1; k <= last; k++) if(window[k].feature > window[star].feature) star = k;
                        int untilStar = (int)(window[star].step - start);
                        int arrival = body.StepsToReach(stance, r.Delay, out _, Math.Max(untilStar, available) + ctx.StepHz);
                        r.Candidates++;
                        if(arrival <= untilStar) { r.ReachableCandidates++; continue; }
                        long lateAvail = arrival == int.MaxValue ? int.MaxValue - 1 : arrival - available;
                        long lateStar = arrival == int.MaxValue ? int.MaxValue - 1 : arrival - untilStar;
                        if(lateAvail < r.BestLateAvail) { r.BestLateAvail = lateAvail; r.BestAvailSteps = available; r.BestArrivalSteps = arrival; }
                        if(lateStar < r.BestLateStar) r.BestLateStar = lateStar;
                    }
                // Independent probe: for every grid step of the legal window, the nearest standing point inside the reach disk.
                for(int index = 0; index < window.Count; index++)
                {
                    var sample = window[index];
                    if((sample.step - start) % interval != 0) continue;
                    int available = (int)(sample.step - start);
                    float dy = sample.pos.y - frame.ReferenceHeight;
                    if(frame.Reach <= Mathf.Abs(dy)) continue;
                    float rh = Mathf.Sqrt(frame.Reach * frame.Reach - dy * dy);
                    var ball = new Vector2(sample.pos.x, sample.pos.z); var dir = body.Position - ball;
                    var stance = dir.magnitude <= rh ? body.Position : ball + dir.normalized * rh;
                    stance = ctx.ClampBody(stance);
                    var dd = sample.pos - new Vector3(stance.x, frame.ReferenceHeight, stance.y);
                    if(dd.sqrMagnitude > reach2 * 1.0001f) continue; // clamped out of reach
                    int arrival = body.StepsToReach(stance, r.Delay, out _, available + 1);
                    if(arrival > available) continue;
                    if(!r.ProbeReach) { r.ProbeReach = true; r.ProbeAvail = available; r.ProbeArrival = arrival; }
                    var contact = ContactPlanner.Plan(trajectory, ctx.FrameAt(stance), court, legal, 0);
                    if(contact.Exists && contact.TStarStep >= 0 && start + arrival <= contact.TStarStep) { r.ProbeContact = true; r.ProbeAvail = available; r.ProbeArrival = arrival; break; }
                }
            }
        }

        private sealed class RallyRun { public int Index; public PlayerId First; public RallyOutcome Outcome; public List<Rec> Recs; }

        private sealed class ShotRow
        {
            public int Rally, K; public PlayerId Striker, Receiver; public Vector3 Contact; public Vector2 Aim; public float P; public ShotOption Option;
            public double D, Gamma, Linear, Curve, Ratio, Speed; public bool VecZero, Clamped; public double InputSpeed;
            public double X1, Z1, T1, X2, Z2, T2; public bool Has1, Has2;
            public string NextResult; public bool Unreachable; public string Cls;
            public double ForecastX, ForecastZ; public bool ForecastGood; public double ForecastErr = double.NaN, IdenticalErr = double.NaN, PaceDiff = double.NaN, IntentionDiff = double.NaN;
        }

        [Test, Explicit, Category("Experiments"), Timeout(21600000)]
        public void WriteUnreachableDiagnosis()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string run = Environment.GetEnvironmentVariable("SC_UNREACH_RUN");
            if(run != "run01" && run != "run02") throw new InvalidDataException("Set SC_UNREACH_RUN=run01 or run02; outputs are never overwritten.");
            string presetFile = Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json");
            var defaults = PresetCodec.LoadDefault(File.ReadAllText(presetFile));
            if(defaults.Hash != "06fed4bd4ac9" || defaults.Ordered.Count != 121 || defaults.Get("shot.paceGammaMax") != 2 || defaults.Get("shot.paceNearM") != 1.5 || defaults.Get("shot.paceFullM") != 6)
                throw new InvalidDataException("Protected defaults changed.");
            string outDir = Path.Combine(root, "Logs/experiments/unreachable", run);
            if(Directory.Exists(outDir)) throw new IOException("Experiment output already exists: " + outDir);
            if(Git(root, "status --porcelain").Length != 0) throw new InvalidDataException("Run from a clean committed source.");
            Directory.CreateDirectory(outDir);
            File.WriteAllText(Path.Combine(outDir, "source.txt"), Git(root, "rev-parse HEAD") + "\n", Utf8);
            var tables = new StringBuilder("table\tkey\tcount\n");
            var rates = new StringBuilder("table\tkey\tshots\tunreachable\n");
            var checks = new StringBuilder("check\tstream\tvariant\tvalue\n");
            var streamB = new Dictionary<int, List<RallyRun>>();
            foreach(string stream in new[] { "A", "B" })
                for(int v = 0; v < Gammas.Length; v++)
                {
                    var preset = defaults.With("shot.paceGammaMax", Gammas[v]);
                    var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), new AnalyticBallSimulationFactory(Physics.bounceThreshold));
                    string variant = "gamma" + Gammas[v].ToString("0.0", C);
                    var runs = stream == "A" ? RunA(ctx) : RunB(ctx);
                    if(stream == "B") streamB[v] = runs;
                    Analyse(outDir, stream, variant, ctx, runs, tables, rates, checks);
                }
            // Pairing (flow B): same rally index with gamma 1.0 and gamma 2.0.
            WritePairs(outDir, streamB[0], streamB[1]);
            File.WriteAllText(Path.Combine(outDir, "tables.tsv"), tables.ToString(), Utf8);
            File.WriteAllText(Path.Combine(outDir, "rates.tsv"), rates.ToString(), Utf8);
            File.WriteAllText(Path.Combine(outDir, "checks.tsv"), checks.ToString(), Utf8);
            TestContext.WriteLine("UNREACH_DATA=" + outDir);
        }

        // Same shot, other curve (method addendum, section 7): the shot that produced each Unreachable ending, replayed with the
        // linear speed (gamma 1) and the curve (gamma 2) from the same contact, aim and power, against the same receiver.
        [Test, Explicit, Category("Experiments"), Timeout(21600000)]
        public void WriteSameShotCounterfactual()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string run = Environment.GetEnvironmentVariable("SC_UNREACH_RUN");
            if(run != "cf01" && run != "cf02") throw new InvalidDataException("Set SC_UNREACH_RUN=cf01 or cf02; outputs are never overwritten.");
            string presetFile = Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json");
            var defaults = PresetCodec.LoadDefault(File.ReadAllText(presetFile));
            if(defaults.Hash != "06fed4bd4ac9" || defaults.Ordered.Count != 121 || defaults.Get("shot.paceGammaMax") != 2)
                throw new InvalidDataException("Protected defaults changed.");
            string outDir = Path.Combine(root, "Logs/experiments/unreachable", run);
            if(Directory.Exists(outDir)) throw new IOException("Experiment output already exists: " + outDir);
            if(Git(root, "status --porcelain").Length != 0) throw new InvalidDataException("Run from a clean committed source.");
            Directory.CreateDirectory(outDir);
            File.WriteAllText(Path.Combine(outDir, "source.txt"), Git(root, "rev-parse HEAD") + "\n", Utf8);
            var ctx2 = new RallyContext(EffectiveConfig.From(defaults.With("shot.paceGammaMax", 2.0)), new RallyParams(), new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            var ctx1 = new RallyContext(EffectiveConfig.From(defaults.With("shot.paceGammaMax", 1.0)), new RallyParams(), new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            var runs = RunA(ctx2);
            var summary = new SortedDictionary<string, int>(StringComparer.Ordinal);
            void Add(string key) { summary.TryGetValue(key, out int n); summary[key] = n + 1; }
            using(var w = new StreamWriter(Path.Combine(outDir, "counterfactual.tsv"), false, Utf8))
            {
                w.WriteLine("rally\tk\treceiver\tstriker\tP\tD\tgamma\tvecZero\tg2Reachable\tg2Reason\tg2Slack\tg2Speed\tg2z1\tg2t1\tg2z2\tg2t2\tg1Reachable\tg1Reason\tg1Slack\tg1Speed\tg1z1\tg1t1\tg1z2\tg1t2\trestSpeed");
                foreach(var r in runs)
                {
                    if(r.Outcome.LastReception != ReceptionResult.Unreachable || r.Recs.Count < 2) continue;
                    var last = r.Recs[r.Recs.Count - 1]; var prev = r.Recs[r.Recs.Count - 2];
                    var contact = last.Ball.Position;
                    var log = r.Outcome.Receptions[r.Outcome.Receptions.Count - 2];
                    bool vecZero = log.Vectors == null || log.Vectors.All(x => x.DeltaHeight == 0 && x.DeltaSpeedFraction == 0);
                    var pace = ShotPace.Requested(prev.P, contact, prev.Aim, ctx2.Physics.BallRadius, ctx2.Pace);
                    string[] res = new string[2]; bool[] reach = new bool[2];
                    var cells = new List<string>();
                    for(int g = 0; g < 2; g++)
                    {
                        var c = g == 0 ? ctx2 : ctx1;
                        var sol = RallyContact.SolveIntention(c, contact, prev.Aim, prev.P);
                        if(!sol.Solved) { cells.AddRange(new[] { "NotSolved", sol.Reason ?? "", "", "", "", "", "", "" }); reach[g] = false; res[g] = "NotSolved"; continue; }
                        var ball = new BallState { Position = contact, Velocity = sol.Velocity, Step = last.Ball.Step };
                        var tr = c.Predict(ball);
                        var rules = new PointRules(c.Court, (float)c.Params.FootprintRadius); rules.Begin(prev.Who, last.Who);
                        var body = new PlayerBody(last.Who, last.Self, c.Move);
                        var plan = ReceptionPlanner.Plan(c, tr, rules, body, last.Delay, 1, last.Noise);
                        double z1 = double.NaN, t1 = double.NaN, z2 = double.NaN, t2 = double.NaN; bool front = false; int floors = 0;
                        foreach(var e in tr.Contacts)
                        {
                            if(e.Surface == SurfaceId.FrontWall) front = true;
                            if(e.Surface == SurfaceId.Floor && front)
                            {
                                floors++; double t = (e.Step - tr.States[0].Step) / (double)c.StepHz;
                                if(floors == 1) { z1 = e.Point.z; t1 = t; } else { z2 = e.Point.z; t2 = t; break; }
                            }
                        }
                        reach[g] = plan.Reachable; res[g] = plan.Reachable ? "Reachable" : "Unreachable";
                        cells.AddRange(new[] { res[g], plan.Reason ?? "", F(plan.SlackS), F(sol.Velocity.magnitude), F(z1), F(t1), F(z2), F(t2) });
                    }
                    w.WriteLine(r.Index + "\t" + (r.Recs.Count - 1) + "\t" + last.Who + "\t" + prev.Who + "\t" + F(prev.P) + "\t" + F(pace.DistanceM) + "\t" + F(pace.Gamma) + "\t" + vecZero + "\t" + string.Join("\t", cells) + "\t" + F(last.Ball.Velocity.magnitude));
                    string bucket = last.Who + (vecZero ? "|vecZero" : "|vecNonZero");
                    Add(bucket + "|g2=" + res[0] + "|g1=" + res[1]);
                }
            }
            var sb = new StringBuilder("key\tcount\n");
            foreach(var kv in summary) sb.Append(kv.Key + "\t" + kv.Value + "\n");
            File.WriteAllText(Path.Combine(outDir, "summary.tsv"), sb.ToString(), Utf8);
            TestContext.WriteLine("UNREACH_CF=" + outDir);
        }

        private static List<RallyRun> RunA(RallyContext ctx)
        {
            var runs = new List<RallyRun>(Rallies); var sink = new List<Rec>();
            int rally = 0;
            var bot = new Recorder(new BotBrain(ctx, 30, 2026), PlayerId.Human, sink, ctx, 0);
            var fondo = new Recorder(new FondoAi(ctx, AiDifficulty.For(ctx.Params, AiLevel.Medium), 1111), PlayerId.Rival, sink, ctx, 0);
            // The recorders keep the rally index through the shared sink; the index is stamped afterwards.
            using(var sim = new RallySim(ctx, bot, fondo))
            {
                bot.Sim = sim; fondo.Sim = sim;
                for(rally = 0; rally < Rallies; rally++)
                {
                    sink.Clear();
                    var first = rally % 2 == 0 ? PlayerId.Human : PlayerId.Rival;
                    var outcome = sim.Play(first);
                    var recs = new List<Rec>(sink); foreach(var r in recs) r.Rally = rally;
                    FinishRecs(ctx, outcome, recs, sim);
                    runs.Add(new RallyRun { Index = rally, First = first, Outcome = outcome, Recs = recs });
                }
            }
            return runs;
        }

        private static List<RallyRun> RunB(RallyContext ctx)
        {
            var runs = new List<RallyRun>(Rallies);
            for(int rally = 0; rally < Rallies; rally++)
            {
                var sink = new List<Rec>();
                var bot = new Recorder(new BotBrain(ctx, 30, 2026 + rally), PlayerId.Human, sink, ctx, rally);
                var fondo = new Recorder(new FondoAi(ctx, AiDifficulty.For(ctx.Params, AiLevel.Medium), 1111 + rally), PlayerId.Rival, sink, ctx, rally);
                using(var sim = new RallySim(ctx, bot, fondo))
                {
                    bot.Sim = sim; fondo.Sim = sim;
                    var first = rally % 2 == 0 ? PlayerId.Human : PlayerId.Rival;
                    var outcome = sim.Play(first);
                    FinishRecs(ctx, outcome, sink, sim);
                    runs.Add(new RallyRun { Index = rally, First = first, Outcome = outcome, Recs = sink });
                }
            }
            return runs;
        }

        // The loop stops at the closing step, so the receiver's body now stands where it was when the ball made its second bounce.
        private static void FinishRecs(RallyContext ctx, RallyOutcome outcome, List<Rec> recs, RallySim sim)
        {
            if(recs.Count == 0 || outcome.LastReception != ReceptionResult.Unreachable) return;
            var last = recs[recs.Count - 1];
            var body = last.Who == PlayerId.Human ? sim.Human : sim.Rival;
            last.SelfAtSecond = body.Position; last.HasSelfAtSecond = true;
        }

        private static string Band(double v, double a, double b, string lo, string mid, string hi) => v < a ? lo : v <= b ? mid : hi;
        private static string PBand(float p) => p <= .25f ? "soft" : p <= .65f ? "medium" : "strong";
        private static string DBand(double d) => d < 3 ? "near<3" : d < 6 ? "mid3-6" : "far>=6";
        private static string ZBand(double z) => z < 3 ? "front<3" : z < 6 ? "mid3-6" : "back>=6";
        private static string AimBand(double y) => y < 1.2 ? "low<1.2" : y <= 2.4 ? "mid1.2-2.4" : "high>2.4";
        private static string Cls(double d, float p, double ratio, bool has2, double z2)
            => d < 6 && p <= .25f && has2 && z2 < 3 ? "C1-short-soft-front" : d < 6 && ratio < .85 ? "C2-slowed-not-soft" : "C3-curve-unaffected";
        private static string F(double v) => v.ToString("R", C);
        private static string F(float v) => v.ToString("R", C);

        private void Analyse(string outDir, string stream, string variant, RallyContext ctx, List<RallyRun> runs, StringBuilder tables, StringBuilder rates, StringBuilder checks)
        {
            string dir = Path.Combine(outDir, stream + "-" + variant); Directory.CreateDirectory(dir);
            float radius = ctx.Physics.BallRadius; var shots = new List<ShotRow>();
            int misaligned = 0, nonFinite = 0, outOfRange = 0;
            foreach(var run in runs)
            {
                var recs = run.Recs; var logs = run.Outcome.Receptions;
                bool aligned = recs.Count == logs.Count || recs.Count == logs.Count + 1;
                for(int i = 0; i < Math.Min(recs.Count, logs.Count); i++) if(logs[i].Who != recs[i].Who) aligned = false;
                if(!aligned) misaligned++;
                for(int k = 1; k < recs.Count; k++)
                {
                    var prev = recs[k - 1]; var cur = recs[k];
                    var s = new ShotRow { Rally = run.Index, K = k, Striker = prev.Who, Receiver = cur.Who, Contact = cur.Ball.Position, Aim = prev.Aim, P = prev.P, Option = prev.Option };
                    var pace = ShotPace.Requested(prev.P, s.Contact, prev.Aim, radius, ctx.Pace);
                    s.D = pace.DistanceM; s.Gamma = pace.Gamma; s.Linear = pace.LinearSpeed; s.Curve = pace.Speed; s.Ratio = pace.LinearSpeed > 0 ? pace.Speed / pace.LinearSpeed : 1;
                    s.Speed = cur.Ball.Velocity.magnitude;
                    if(float.IsNaN(pace.Speed) || float.IsInfinity(pace.Speed) || float.IsNaN(pace.DistanceM) || float.IsInfinity(pace.DistanceM) || double.IsNaN(s.Speed) || double.IsInfinity(s.Speed)) nonFinite++;
                    if(pace.Speed < ctx.VMin - 1e-4 || pace.Speed > ctx.VMax + 1e-4 || s.Speed > ctx.VCap + 1e-3) outOfRange++;
                    var tr = ctx.Predict(cur.Ball); bool front = false; int floors = 0;
                    foreach(var e in tr.Contacts)
                    {
                        if(e.Surface == SurfaceId.FrontWall) front = true;
                        if(e.Surface == SurfaceId.Floor && front)
                        {
                            floors++;
                            double t = (e.Step - tr.States[0].Step) / (double)ctx.StepHz;
                            if(floors == 1) { s.Has1 = true; s.X1 = e.Point.x; s.Z1 = e.Point.z; s.T1 = t; }
                            else { s.Has2 = true; s.X2 = e.Point.x; s.Z2 = e.Point.z; s.T2 = t; break; }
                        }
                    }
                    s.Cls = Cls(s.D, s.P, s.Ratio, s.Has2, s.Z2);
                    // Consistency (check a): same inputs through the solver path the AI forecasts with and through CausalQuality.
                    if(k - 1 < logs.Count)
                    {
                        var log = logs[k - 1]; var vec = log.Vectors;
                        s.VecZero = vec == null || vec.All(x => x.DeltaHeight == 0 && x.DeltaSpeedFraction == 0);
                        s.Clamped = log.Input.Clamped || Vector2.Distance(log.Input.Aim, prev.Aim) > 1e-6f;
                        s.InputSpeed = log.Input.Speed;
                        s.PaceDiff = Math.Abs(log.Input.Speed - pace.Speed);
                        var sol = RallyContact.SolveIntention(ctx, s.Contact, prev.Aim, prev.P);
                        if(sol.Solved) s.IntentionDiff = Math.Abs(sol.Velocity.magnitude - s.Speed);
                        if(prev.Who == PlayerId.Rival)
                        {
                            var fc = RallyContact.Forecast(ctx, s.Contact, prev.Aim, prev.P);
                            if(fc.Solved && fc.Good && s.Has1) s.IdenticalErr = Math.Sqrt(Math.Pow(fc.FirstBounce.x - s.X1, 2) + Math.Pow(fc.FirstBounce.z - s.Z1, 2));
                            if(prev.HasContact)
                            {
                                var fo = RallyContact.Forecast(ctx, prev.PStar, prev.Option.Aim, prev.Option.P);
                                s.ForecastGood = fo.Good;
                                if(fo.Solved && fo.Good && s.Has1) s.ForecastErr = Math.Sqrt(Math.Pow(fo.FirstBounce.x - s.X1, 2) + Math.Pow(fo.FirstBounce.z - s.Z1, 2));
                            }
                        }
                    }
                    s.NextResult = k < logs.Count ? logs[k].Result.ToString() : (k == recs.Count - 1 ? "NoLog" : "");
                    s.Unreachable = k == recs.Count - 1 && run.Outcome.LastReception == ReceptionResult.Unreachable;
                    shots.Add(s);
                }
            }
            // ---- files
            using(var w = new StreamWriter(Path.Combine(dir, "endings.tsv"), false, Utf8))
            {
                w.WriteLine("rally\tfirstReceiver\tend\tcause\twinner\tshots\tlastReception\tlastReceiver\tstriker\tterminalSurface\toutAfterBounce\trecords\tlogs");
                foreach(var r in runs)
                {
                    var o = r.Outcome; var lastRec = r.Recs.Count > 0 ? r.Recs[r.Recs.Count - 1].Who.ToString() : "";
                    w.WriteLine(r.Index + "\t" + r.First + "\t" + o.End + "\t" + o.Cause + "\t" + (o.Winner?.ToString() ?? "") + "\t" + o.Shots + "\t" + (o.LastReception?.ToString() ?? "") + "\t" + lastRec + "\t" + (o.Striker?.ToString() ?? "") + "\t" + o.TerminalSurface + "\t" + o.OutAfterBounce + "\t" + r.Recs.Count + "\t" + o.Receptions.Count);
                }
            }
            using(var w = new StreamWriter(Path.Combine(dir, "shots.tsv"), false, Utf8))
            {
                w.WriteLine("rally\tk\tstriker\treceiver\tcx\tcy\tcz\taimX\taimY\tP\tline\theight\tpower\tD\tgamma\tlinear\tcurve\tratio\tspeed\tinputSpeed\tvecZero\tclamped\tpaceDiff\tintentionDiff\tidenticalForecastErr\tforecastErr\tx1\tz1\tt1\tx2\tz2\tt2\tnext\tunreachable\tclass");
                foreach(var s in shots)
                    w.WriteLine(s.Rally + "\t" + s.K + "\t" + s.Striker + "\t" + s.Receiver + "\t" + F(s.Contact.x) + "\t" + F(s.Contact.y) + "\t" + F(s.Contact.z) + "\t" + F(s.Aim.x) + "\t" + F(s.Aim.y) + "\t" + F(s.P) + "\t" + s.Option.Line + "\t" + s.Option.Height + "\t" + s.Option.Power + "\t" + F(s.D) + "\t" + F(s.Gamma) + "\t" + F(s.Linear) + "\t" + F(s.Curve) + "\t" + F(s.Ratio) + "\t" + F(s.Speed) + "\t" + F(s.InputSpeed) + "\t" + s.VecZero + "\t" + s.Clamped + "\t" + F(s.PaceDiff) + "\t" + F(s.IntentionDiff) + "\t" + F(s.IdenticalErr) + "\t" + F(s.ForecastErr) + "\t" + (s.Has1 ? F(s.X1) : "") + "\t" + (s.Has1 ? F(s.Z1) : "") + "\t" + (s.Has1 ? F(s.T1) : "") + "\t" + (s.Has2 ? F(s.X2) : "") + "\t" + (s.Has2 ? F(s.Z2) : "") + "\t" + (s.Has2 ? F(s.T2) : "") + "\t" + s.NextResult + "\t" + s.Unreachable + "\t" + s.Cls);
            }
            string tag = stream + "/" + variant + "/";
            void T(string table, string key, int count) => tables.Append(tag + table + "\t" + key + "\t" + count + "\n");
            // all point endings
            foreach(var g in runs.Where(r => r.Outcome.End == RallyEnd.Point).GroupBy(r => (r.Outcome.Cause, last: r.Outcome.LastReception?.ToString() ?? "-", receiver: r.Recs.Count > 0 ? r.Recs[r.Recs.Count - 1].Who.ToString() : "-", striker: r.Outcome.Striker?.ToString() ?? "-")).OrderBy(g => g.Key.ToString()))
                T("endings:cause|lastReception|receiver|striker", g.Key.Cause + "|" + g.Key.last + "|" + g.Key.receiver + "|" + g.Key.striker, g.Count());
            foreach(var g in runs.GroupBy(r => r.Outcome.Winner?.ToString() ?? "none")) T("winner", g.Key, g.Count());
            T("mean-shots-x1000", ((int)Math.Round(runs.Average(r => (double)r.Outcome.Shots) * 1000)).ToString(C), runs.Count);
            T("misaligned-rallies", "count", misaligned);
            // receptions by result and receiver (all receptions)
            foreach(var g in runs.SelectMany(r => r.Outcome.Receptions).GroupBy(l => l.Who + "|" + l.Result)) T("receptions:receiver|result", g.Key, g.Count());
            var unr = shots.Where(s => s.Unreachable).ToList();
            // unreachable endings: breakdowns
            void Dist(string table, Func<ShotRow, string> key) { foreach(var g in unr.GroupBy(key).OrderBy(g => g.Key)) T(table, g.Key, g.Count()); }
            Dist("unreach:receiver", s => s.Receiver.ToString());
            Dist("unreach:receiver|class", s => s.Receiver + "|" + s.Cls);
            Dist("unreach:striker|class", s => s.Striker + "|" + s.Cls);
            Dist("unreach:receiver|P", s => s.Receiver + "|" + PBand(s.P));
            Dist("unreach:receiver|D", s => s.Receiver + "|" + DBand(s.D));
            Dist("unreach:receiver|contactZ", s => s.Receiver + "|" + ZBand(s.Contact.z));
            Dist("unreach:receiver|aimY", s => s.Receiver + "|" + AimBand(s.Aim.y));
            Dist("unreach:receiver|line", s => s.Receiver + "|" + s.Option.Line);
            Dist("unreach:receiver|deathZ2", s => s.Receiver + "|" + (s.Has2 ? ZBand(s.Z2) : "na"));
            Dist("unreach:receiver|ratio", s => s.Receiver + "|" + (s.Ratio < .85 ? "slowed<0.85" : "unaffected>=0.85"));
            // by receiver geometry
            var recOf = new Dictionary<(int, int), Rec>();
            foreach(var run in runs) foreach(var r in run.Recs) recOf[(run.Index, r.K)] = r;
            string RecvDist(ShotRow s)
            {
                var r = recOf[(s.Rally, s.K)];
                if(!s.Has2) return "na";
                double d = Vector2.Distance(r.HasSelfAtSecond ? r.SelfAtSecond : r.Self, new Vector2((float)s.X2, (float)s.Z2));
                return d < 2 ? "<2m" : d < 4 ? "2-4m" : ">=4m";
            }
            string Quad(ShotRow s)
            {
                var r = recOf[(s.Rally, s.K)]; if(!s.Has2) return "na";
                var at = r.HasSelfAtSecond ? r.SelfAtSecond : r.Self;
                return (at.y < s.Z2 ? "ahead" : "behind") + "/" + (at.x < s.X2 ? "left" : "right");
            }
            string Late(ShotRow s)
            {
                var r = recOf[(s.Rally, s.K)]; if(!r.Mirrored || r.BestLateAvail == long.MaxValue) return "nocandidate";
                double late = r.BestLateAvail / (double)ctx.StepHz; return late < .1 ? "<0.1s" : late < .3 ? "0.1-0.3s" : ">=0.3s";
            }
            Dist("unreach:receiver|receiverToSecondBounce", s => s.Receiver + "|" + RecvDist(s));
            Dist("unreach:receiver|receiverQuadrantAtSecondBounce", s => s.Receiver + "|" + Quad(s));
            Dist("unreach:receiver|lateness", s => s.Receiver + "|" + Late(s));
            Dist("unreach:planner-path", s => { var r = recOf[(s.Rally, s.K)]; return !r.Mirrored ? "no-mirror" : r.ReachableCandidates > 0 ? "ii-reachable-candidates-but-plan-failed" : r.Candidates == 0 ? "no-candidates" : "i-all-candidates-late"; });
            // unreachable per class: rate over all shots
            foreach(var who in new[] { PlayerId.Human, PlayerId.Rival })
            {
                var mine = shots.Where(s => s.Striker == who).ToList();
                foreach(var g in mine.GroupBy(s => s.Cls)) rates.Append(tag + "striker=" + who + "|class\t" + g.Key + "\t" + g.Count() + "\t" + g.Count(s => s.Unreachable) + "\n");
                foreach(var g in mine.GroupBy(s => PBand(s.P) + "|" + DBand(s.D))) rates.Append(tag + "striker=" + who + "|P|D\t" + g.Key + "\t" + g.Count() + "\t" + g.Count(s => s.Unreachable) + "\n");
                foreach(var g in mine.GroupBy(s => s.Option.Line + "|" + s.Option.Height + "|" + s.Option.Power)) rates.Append(tag + "striker=" + who + "|option\t" + g.Key + "\t" + g.Count() + "\t" + g.Count(s => s.Unreachable) + "\n");
                foreach(var g in mine.GroupBy(s => ZBand(s.Contact.z))) rates.Append(tag + "striker=" + who + "|contactZ\t" + g.Key + "\t" + g.Count() + "\t" + g.Count(s => s.Unreachable) + "\n");
            }
            // unreachable detail file
            using(var w = new StreamWriter(Path.Combine(dir, "unreachable.tsv"), false, Utf8))
            {
                w.WriteLine("rally\tk\treceiver\tstriker\tcls\tP\tD\tgamma\tlinear\tcurve\tratio\tcx\tcy\tcz\taimX\taimY\tline\theight\tpower\tx1\tz1\tt1\tx2\tz2\tt2\tselfX\tselfY\tselfVx\tselfVy\tstanceX\tstanceY\tstanceDist\tselfAtSecondX\tselfAtSecondY\tdistToSecond\treason\tplanArrivalStep\tcandidates\treachableCandidates\tbestLateAvailSteps\tbestLateStarSteps\tbestAvailS\tbestArrivalS\tprobeReach\tprobeContact\tprobeAvailS\tprobeArrivalS\tfirstRecvResultOnSameRallyGamma1");
                foreach(var s in unr)
                {
                    var r = recOf[(s.Rally, s.K)]; var at = r.HasSelfAtSecond ? r.SelfAtSecond : r.Self;
                    w.WriteLine(s.Rally + "\t" + s.K + "\t" + s.Receiver + "\t" + s.Striker + "\t" + s.Cls + "\t" + F(s.P) + "\t" + F(s.D) + "\t" + F(s.Gamma) + "\t" + F(s.Linear) + "\t" + F(s.Curve) + "\t" + F(s.Ratio) + "\t" + F(s.Contact.x) + "\t" + F(s.Contact.y) + "\t" + F(s.Contact.z) + "\t" + F(s.Aim.x) + "\t" + F(s.Aim.y) + "\t" + s.Option.Line + "\t" + s.Option.Height + "\t" + s.Option.Power + "\t" + (s.Has1 ? F(s.X1) : "") + "\t" + (s.Has1 ? F(s.Z1) : "") + "\t" + (s.Has1 ? F(s.T1) : "") + "\t" + (s.Has2 ? F(s.X2) : "") + "\t" + (s.Has2 ? F(s.Z2) : "") + "\t" + (s.Has2 ? F(s.T2) : "")
                        + "\t" + F(r.Self.x) + "\t" + F(r.Self.y) + "\t" + F(r.SelfV.x) + "\t" + F(r.SelfV.y) + "\t" + F(r.Stance.x) + "\t" + F(r.Stance.y) + "\t" + F(Vector2.Distance(r.Self, r.Stance)) + "\t" + F(at.x) + "\t" + F(at.y) + "\t" + (s.Has2 ? F(Vector2.Distance(at, new Vector2((float)s.X2, (float)s.Z2))) : "")
                        + "\t" + r.Reason + "\t" + r.ArrivalStep + "\t" + r.Candidates + "\t" + r.ReachableCandidates + "\t" + (r.BestLateAvail == long.MaxValue ? "" : r.BestLateAvail.ToString(C)) + "\t" + (r.BestLateStar == long.MaxValue ? "" : r.BestLateStar.ToString(C)) + "\t" + (r.BestAvailSteps < 0 ? "" : F(r.BestAvailSteps / (double)ctx.StepHz)) + "\t" + (r.BestArrivalSteps < 0 ? "" : F(r.BestArrivalSteps / (double)ctx.StepHz))
                        + "\t" + r.ProbeReach + "\t" + r.ProbeContact + "\t" + (r.ProbeAvail < 0 ? "" : F(r.ProbeAvail / (double)ctx.StepHz)) + "\t" + (r.ProbeArrival < 0 ? "" : F(r.ProbeArrival / (double)ctx.StepHz)) + "\t");
                }
            }
            // ---- checks
            void Ch(string name, string value) => checks.Append(name + "\t" + stream + "\t" + variant + "\t" + value + "\n");
            Ch("shots", shots.Count.ToString(C));
            Ch("non-finite", nonFinite.ToString(C));
            Ch("out-of-range(curve in [vMin,vMax], speed<=vCap)", outOfRange.ToString(C));
            Ch("misaligned-rallies", misaligned.ToString(C));
            var zeroVec = shots.Where(s => s.VecZero && !s.Clamped && !double.IsNaN(s.PaceDiff)).ToList();
            Ch("a1: zero-vector shots checked", zeroVec.Count.ToString(C));
            Ch("a1: max |Input.Speed - curve speed|", zeroVec.Count == 0 ? "na" : F(zeroVec.Max(s => s.PaceDiff)));
            Ch("a1: count >1e-3", zeroVec.Count(s => s.PaceDiff > 1e-3).ToString(C));
            var intent = zeroVec.Where(s => !double.IsNaN(s.IntentionDiff)).ToList();
            Ch("a2: |v_intention - v_actual| zero-vector shots", intent.Count.ToString(C));
            Ch("a2: max", intent.Count == 0 ? "na" : F(intent.Max(s => s.IntentionDiff)));
            Ch("a2: count >1e-3", intent.Count(s => s.IntentionDiff > 1e-3).ToString(C));
            var ident = shots.Where(s => !double.IsNaN(s.IdenticalErr)).ToList();
            Ch("a3: forecast(first bounce) vs actual, identical inputs, rival shots", ident.Count.ToString(C));
            Ch("a3: identical-input max err (m)", ident.Count == 0 ? "na" : F(ident.Max(s => s.IdenticalErr)));
            Ch("a3: identical-input count >1e-3 m, zero-vector only", ident.Count(s => s.VecZero && !s.Clamped && s.IdenticalErr > 1e-3).ToString(C));
            var fe = shots.Where(s => !double.IsNaN(s.ForecastErr)).Select(s => s.ForecastErr).OrderBy(x => x).ToList();
            Ch("a4: AI forecast (clean option at P*) vs actual first bounce: n", fe.Count.ToString(C));
            Ch("a4: p50 / p95 / max (m)", fe.Count == 0 ? "na" : F(fe[fe.Count / 2]) + " / " + F(fe[(int)Math.Min(fe.Count - 1, Math.Floor(.95 * fe.Count))]) + " / " + F(fe[fe.Count - 1]));
            Ch("b: unreachable endings", unr.Count.ToString(C));
            Ch("b: mirrored", unr.Count(s => recOf[(s.Rally, s.K)].Mirrored).ToString(C));
            Ch("b: plan failed although mirror found reachable candidates (path ii)", unr.Count(s => recOf[(s.Rally, s.K)].ReachableCandidates > 0).ToString(C));
            Ch("b: probe found a stance reaching in time", unr.Count(s => recOf[(s.Rally, s.K)].ProbeReach).ToString(C));
            Ch("b: probe stance reaching in time and with a legal contact (contradiction candidate)", unr.Count(s => recOf[(s.Rally, s.K)].ProbeContact).ToString(C));
            Ch("b: min best lateness (steps) over unreachable endings", unr.Count == 0 ? "na" : unr.Min(s => recOf[(s.Rally, s.K)].BestLateAvail).ToString(C));
            Ch("c: unreachable endings with cause != DoubleBounce", runs.Count(r => r.Outcome.LastReception == ReceptionResult.Unreachable && r.Outcome.Cause != PointEndCause.DoubleBounce).ToString(C));
            Ch("c: unreachable endings with terminal surface != Floor", runs.Count(r => r.Outcome.LastReception == ReceptionResult.Unreachable && r.Outcome.TerminalSurface != SurfaceId.Floor).ToString(C));
            Ch("c: unreachable endings whose striker is not the opponent of the last receiver", unr.Count(s => runs[s.Rally].Outcome.Striker != s.Striker).ToString(C));
            var rivalShots = shots.Where(s => s.Striker == PlayerId.Rival).ToList();
            foreach(var g in rivalShots.GroupBy(s => s.Option.Line + "|" + s.Option.Height + "|" + s.Option.Power).OrderBy(g => g.Key))
                Ch("e: rival option share", g.Key + " = " + g.Count() + "/" + rivalShots.Count + " (" + F(100.0 * g.Count() / Math.Max(1, rivalShots.Count)) + " %), unreachable " + g.Count(s => s.Unreachable));
        }

        private static void WritePairs(string outDir, List<RallyRun> a, List<RallyRun> b)
        {
            using(var w = new StreamWriter(Path.Combine(outDir, "pairs.tsv"), false, Utf8))
            {
                w.WriteLine("rally\tfirstReceiver\tg1Cause\tg1Last\tg1Shots\tg2Cause\tg2Last\tg2Shots\tdivergeAtRecord\tg2UnreachableAtK\tg2UnreachableBeforeDivergence");
                for(int i = 0; i < a.Count; i++)
                {
                    var x = a[i]; var y = b[i]; int div = -1; int n = Math.Min(x.Recs.Count, y.Recs.Count);
                    for(int k = 0; k < n; k++)
                    {
                        var p = x.Recs[k]; var q = y.Recs[k];
                        if(p.Who != q.Who || p.Ball.Position != q.Ball.Position || p.Ball.Velocity != q.Ball.Velocity || p.Ball.Step != q.Ball.Step || p.Self != q.Self || p.Opp != q.Opp) { div = k; break; }
                    }
                    if(div < 0 && x.Recs.Count != y.Recs.Count) div = n;
                    bool unr = y.Outcome.LastReception == ReceptionResult.Unreachable; int kun = unr ? y.Recs.Count - 1 : -1;
                    w.WriteLine(i + "\t" + x.First + "\t" + x.Outcome.Cause + "\t" + (x.Outcome.LastReception?.ToString() ?? "") + "\t" + x.Outcome.Shots + "\t" + y.Outcome.Cause + "\t" + (y.Outcome.LastReception?.ToString() ?? "") + "\t" + y.Outcome.Shots + "\t" + div + "\t" + kun + "\t" + (unr ? (div < 0 || kun < div).ToString() : ""));
                }
            }
        }

        private static string Git(string root, string args)
        {
            var info = new System.Diagnostics.ProcessStartInfo("git", args) { WorkingDirectory = root, RedirectStandardOutput = true, UseShellExecute = false };
            using(var p = System.Diagnostics.Process.Start(info)) { string o = p.StandardOutput.ReadToEnd().Trim(); p.WaitForExit(); return o; }
        }
    }
}
