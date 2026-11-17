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
    // Read-only experiment. Method committed before execution: 2026-11-17-g1-diagnosis-method.md.
    public sealed class G1BackCourtDiagnosisExperiment
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private readonly List<Dictionary<string, string>> shots = new List<Dictionary<string, string>>();
        private readonly List<Dictionary<string, string>> receptions = new List<Dictionary<string, string>>();
        private StreamWriter states, contacts;
        private RallyContext ctx;

        private sealed class Flight
        {
            public string Id;
            public Vector3 From;
            public ShotSolution Solution;
            public Trajectory Trajectory;
            public PointRules Rules;
            public double Close = double.PositiveInfinity;
            public ContactEvent? Back, First, Second, Front;
            public bool BackInPlay;
            public int FloorsBeforeBack;
            public float Apex;
            public Dictionary<string, string> Row;
        }
        private sealed class Sample
        {
            public long Step;
            public Vector3 Position;
            public float Feature;
        }
        private sealed class Mirror
        {
            public int RuleSamples, Below, Above, Window, AfterBack, Candidates, OnTime, ContactOk, ContactRejected;
            public long BestLate = long.MaxValue, BestLateAvailable = long.MaxValue;
            public int Available, Required;
            public Vector2 Stance;
        }

        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void WriteG1BackCourtDiagnosis()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string run = Environment.GetEnvironmentVariable("SC_G1_RUN");
            Assert.That(run == "run01" || run == "run02", Is.True, "Use a predeclared run ID.");
            Assert.That(Git(root, "status --porcelain"), Is.Empty, "Commit the experiment before running.");
            var defaults = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(defaults.Hash, Is.EqualTo("430da2faa818"));
            Assert.That(defaults.Ordered.Count, Is.EqualTo(121));
            ctx = new RallyContext(EffectiveConfig.From(defaults), new RallyParams(), new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            Assert.That(ctx.Config.SimulationKey, Is.EqualTo("166a96e319c7"));
            Assert.That(ctx.Court.BackOutLower, Is.EqualTo(2.13f));
            string dir = Path.Combine(root, "Logs/experiments/g1", run);
            Assert.That(Directory.Exists(dir), Is.False, "Never overwrite evidence.");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "source.tsv"), "head\tdefaults\tsimulationKey\trallyParams\tunity\tbounceThreshold\n" +
                Git(root, "rev-parse HEAD").Trim() + "\t" + defaults.Hash + "\t" + ctx.Config.SimulationKey + "\t" + ctx.Params.Hash + "\t" + Application.unityVersion + "\t" + F(Physics.bounceThreshold) + "\n", Utf8);
            using (states = new StreamWriter(Path.Combine(dir, "states.tsv"), false, Utf8))
            using (contacts = new StreamWriter(Path.Combine(dir, "contacts.tsv"), false, Utf8))
            {
                states.WriteLine("id\tstep\tx\ty\tz\tvx\tvy\tvz\tinPlay");
                contacts.WriteLine("id\tstep\ttimeInStep\tsurface\tx\ty\tz\tvx\tvy\tvz\tpersistent\tinPlay");
                RunA1();
                RunA2();
            }
            WriteRows(Path.Combine(dir, "shots.tsv"), shots);
            WriteRows(Path.Combine(dir, "receptions.tsv"), receptions);
            Assert.That(shots.Count(r => r["section"] == "A1"), Is.EqualTo(108));
            Assert.That(shots.Count(r => r["section"] == "A2"), Is.EqualTo(5880));
            Assert.That(receptions.Count, Is.EqualTo(648));
            TestContext.WriteLine(dir);
        }

        private void RunA1()
        {
            var fixtures = new[] {
                ("T", new Vector3(ctx.T.x - .6f, 1.1f, ctx.T.y)),
                ("backRight", new Vector3(.6f, 1.1f, 8.3f)),
                ("backLeft", new Vector3(ctx.Court.Width - .6f, 1.1f, 8.3f)),
                ("middle", new Vector3(ctx.T.x - .6f, 1.1f, 3.5f)) };
            int index = 0;
            foreach (var fixture in fixtures)
                foreach (float power in new[] { .7f, .85f, 1f })
                    foreach (float y in new[] { .75f, 1.7f, 3f })
                        foreach (float x in new[] { .6f, ctx.Court.Width / 2, ctx.Court.Width - .6f })
                        {
                            var f = Measure("A1-" + index.ToString("D3", C), "A1", fixture.Item1, "direct", "grid", fixture.Item2, new Vector2(x, y), new Vector2(x, y), power);
                            foreach (AiLevel level in new[] { AiLevel.Easy, AiLevel.Medium, AiLevel.Hard })
                                foreach (PlayerId receiver in new[] { PlayerId.Rival, PlayerId.Human })
                                    Receive(f, index, receiver, level);
                            index++;
                        }
        }

        private void RunA2()
        {
            int index = 0;
            foreach (var recipe in PracticeLaunchDefinitions.Create(ctx.Config).Where(r => r.Id == "SL5" || r.Id == "SL6").OrderBy(r => r.Id))
            {
                var incoming = ctx.Predict(recipe.Initial);
                var contact = ContactPlanner.Plan(incoming, recipe.Body, ctx.Court, ctx.Legal, 1);
                Assert.That(contact.Exists && contact.TStarStep >= 0, Is.True, "Missing fixture contact: " + recipe.Id);
                Vector3 from = contact.PStar;
                float near = from.x < ctx.Court.Width / 2 ? .6f : ctx.Court.Width - .6f;
                float far = ctx.Court.Width - near;
                float Through(float corner) => from.x + (corner - from.x) * from.z / (from.z + ctx.Court.Length);
                var columns = new[] { ("right", .6f), ("center", ctx.Court.Width / 2), ("left", ctx.Court.Width - .6f), ("straight", Through(near)), ("cross", Through(far)) };
                foreach (var column in columns)
                    foreach (float margin in new[] { .05f, .10f, .15f, .25f, .40f, .60f, .80f, 1f, 1.2f, 1.4f, 1.6f, 2f, 2.4f, 3f })
                        for (int k = 0; k <= 20; k++)
                            foreach (string mode in new[] { "direct", "cap22" })
                            {
                                Vector2 requested = new Vector2(column.Item2, ctx.Court.TinTop + margin), aim = requested;
                                float cap = from.y + Mathf.Tan((float)ctx.Params.MaxAimElevationDeg * Mathf.Deg2Rad) * Mathf.Sqrt((aim.x - from.x) * (aim.x - from.x) + from.z * from.z);
                                if (mode == "cap22") { aim.y = Mathf.Min(aim.y, cap); aim = ShotOptions.ClampAim(ctx, aim); }
                                var f = Measure("A2-" + index.ToString("D4", C), "A2", recipe.Id, mode, column.Item1, from, requested, aim, k / 20f);
                                Put(f.Row, "incomingTStar", contact.TStarStep, "capHeight", cap, "capChanged", aim.y != requested.y, "heightMargin", margin);
                                index++;
                            }
            }
        }

        private Flight Measure(string id, string section, string fixture, string mode, string column, Vector3 from, Vector2 requested, Vector2 aim, float power)
        {
            var solution = RallyContact.SolveIntention(ctx, from, aim, power);
            var f = new Flight { Id = id, From = from, Solution = solution, Apex = from.y, Row = new Dictionary<string, string>() };
            var r = f.Row; shots.Add(r);
            Put(r, "id", id, "section", section, "fixture", fixture, "mode", mode, "column", column, "power", power,
                "fromX", from.x, "fromY", from.y, "fromZ", from.z, "requestedX", requested.x, "requestedY", requested.y,
                "aimX", aim.x, "aimY", aim.y, "solved", solution.Solved, "solveReason", solution.Reason,
                "targetX", solution.TargetUsed.x, "targetY", solution.TargetUsed.y, "speedRaised", solution.SpeedRaised, "targetLowered", solution.TargetLowered,
                "speed", solution.Speed, "vx", solution.Velocity.x, "vy", solution.Velocity.y, "vz", solution.Velocity.z,
                "geometricElevation", Mathf.Atan2(aim.y - from.y, Mathf.Sqrt((aim.x - from.x) * (aim.x - from.x) + (ctx.Physics.BallRadius - from.z) * (ctx.Physics.BallRadius - from.z))) * Mathf.Rad2Deg,
                "launchElevation", Mathf.Atan2(solution.Velocity.y, new Vector2(solution.Velocity.x, solution.Velocity.z).magnitude) * Mathf.Rad2Deg,
                "cause", "Unsolved", "terminalSurface", "", "closeSteps", "", "frontY", "", "apex", "", "backY", "", "backSteps", "", "backInPlay", false,
                "floorsBeforeBack", "", "firstX", "", "firstZ", "", "firstSteps", "", "secondX", "", "secondZ", "", "secondSteps", "", "secondBackGap", "",
                "drive", false, "driveLine", "other", "witnessStatus", "NotEvaluated:unsolved");
            if (!solution.Solved) return f;
            Assert.That(Finite(solution.Velocity) && solution.Speed <= ctx.VCap + .0001f, Is.True, id + ": nonfinite or overspeed");
            f.Trajectory = ctx.Predict(new BallState { Position = from, Velocity = solution.Velocity });
            f.Rules = Rules(PlayerId.Rival);
            int next = 0;
            foreach (var state in f.Trajectory.States.Skip(1))
            {
                while (next < f.Trajectory.Contacts.Count && f.Trajectory.Contacts[next].Step <= state.Step)
                {
                    var hit = f.Trajectory.Contacts[next++];
                    if (!f.Rules.Closed)
                    {
                        f.Rules.OnBallContact(hit);
                        if (f.Rules.Closed) f.Close = Time(hit);
                    }
                }
                if (!f.Rules.Closed && state.Position.y > ctx.Court.WallHeight) { f.Rules.OnOverTop(); f.Close = state.Step; }
            }
            int floors = 0;
            foreach (var hit in f.Trajectory.Contacts)
            {
                contacts.WriteLine(string.Join("\t", new[] { id, F(hit.Step), F(hit.TimeInStep), hit.Surface.ToString(), F(hit.Point.x), F(hit.Point.y), F(hit.Point.z), F(hit.ApproachVelocity.x), F(hit.ApproachVelocity.y), F(hit.ApproachVelocity.z), F(hit.Persistent), F(Time(hit) <= f.Close) }));
                if (hit.Persistent) continue;
                if (hit.Surface == SurfaceId.FrontWall && !f.Front.HasValue) f.Front = hit;
                if (hit.Surface == SurfaceId.BackWall && !f.Back.HasValue) { f.Back = hit; f.FloorsBeforeBack = floors; f.BackInPlay = Time(hit) < f.Close; }
                if (hit.Surface == SurfaceId.Floor)
                {
                    floors++;
                    if (floors == 1) f.First = hit;
                    if (floors == 2) f.Second = hit;
                }
            }
            foreach (var state in f.Trajectory.States)
            {
                Assert.That(Finite(state.Position) && Finite(state.Velocity), Is.True, id + ": nonfinite trajectory");
                if (state.Step <= f.Close) f.Apex = Mathf.Max(f.Apex, state.Position.y);
                states.WriteLine(string.Join("\t", new[] { id, F(state.Step), F(state.Position.x), F(state.Position.y), F(state.Position.z), F(state.Velocity.x), F(state.Velocity.y), F(state.Velocity.z), F(state.Step <= f.Close) }));
            }
            Put(r, "cause", f.Rules.Closed ? f.Rules.Cause.ToString() : "Horizon", "terminalSurface", f.Rules.Closed ? f.Rules.TerminalSurface.ToString() : "",
                "closeSteps", double.IsPositiveInfinity(f.Close) ? "" : F(f.Close), "apex", f.Apex, "backInPlay", f.BackInPlay);
            if (f.Front.HasValue) Put(r, "frontY", f.Front.Value.Point.y);
            if (f.Back.HasValue) Put(r, "backY", f.Back.Value.Point.y, "backSteps", Time(f.Back.Value), "floorsBeforeBack", f.FloorsBeforeBack);
            if (f.First.HasValue) Put(r, "firstX", f.First.Value.Point.x, "firstZ", f.First.Value.Point.z, "firstSteps", Time(f.First.Value));
            if (f.Second.HasValue) Put(r, "secondX", f.Second.Value.Point.x, "secondZ", f.Second.Value.Point.z, "secondSteps", Time(f.Second.Value), "secondBackGap", ctx.Court.Length - f.Second.Value.Point.z);
            bool legal = f.Rules.Cause == PointEndCause.DoubleBounce;
            bool drive = legal && f.Second.HasValue && f.Front.HasValue && f.Front.Value.Point.y <= 1.2f && f.Apex <= 2.13f && f.Second.Value.Point.z >= 8;
            string line = "other";
            if (f.Second.HasValue && f.First.HasValue)
            {
                bool right = from.x < ctx.Court.Width / 2;
                float x1 = f.First.Value.Point.x, x2 = f.Second.Value.Point.x;
                if ((right ? x1 : ctx.Court.Width - x1) <= 1 && (right ? x2 : ctx.Court.Width - x2) <= 1) line = "straight";
                else if ((right ? ctx.Court.Width - x2 : x2) <= 1 && (from.x - ctx.Court.Width / 2) * (x2 - ctx.Court.Width / 2) < 0) line = "cross";
            }
            Put(r, "drive", drive, "driveLine", line, "witnessStatus", legal && f.Second.HasValue ? "Evaluated" : "NotEvaluated:" + r["cause"]);
            return f;
        }

        private void Receive(Flight f, int index, PlayerId receiver, AiLevel level)
        {
            var r = new Dictionary<string, string>(); receptions.Add(r);
            var wait = ctx.WaitPosition(receiver);
            var brain = new FondoAi(ctx, AiDifficulty.For(ctx.Params, level), 1111 + index);
            Vector2 noise = receiver == PlayerId.Rival ? brain.StanceNoise() : Vector2.zero;
            int delay = ctx.Steps(ctx.Params.ReactionS) + (receiver == PlayerId.Rival ? brain.PerceptionSteps : 0);
            Put(r, "id", f.Id, "receiver", receiver, "difficulty", level, "waitX", wait.x, "waitZ", wait.y, "noiseX", noise.x, "noiseZ", noise.y, "delaySteps", delay,
                "plan", "Unsolved", "bucket", "e", "route", "unsolved", "postBackContact", false, "postBackWindow", 0,
                "ruleSamples", 0, "belowMin", 0, "aboveMax", 0, "window", 0, "candidates", 0, "onTimeCandidates", 0, "candidateContactOk", 0, "candidateContactRejected", 0,
                "lateStarSteps", "", "lateAvailableSteps", "", "bestAvailableSteps", "", "bestRequiredSteps", "", "bestStanceX", "", "bestStanceZ", "", "bestDistance", "",
                "stanceX", "", "stanceZ", "bodyContactX", "", "bodyContactZ", "arrivalSteps", "", "slackS", "", "contactExists", false,
                "contactReason", "", "contactSteps", "", "contactX", "", "contactY", "", "contactZ", "", "contactClearance", "");
            if (!f.Solution.Solved) return;
            var body = new PlayerBody(receiver, wait, ctx.Move);
            var rules = Rules(receiver);
            var plan = ReceptionPlanner.Plan(ctx, f.Trajectory, rules, body, delay, 1, noise);
            Mirror m = Inspect(f, body, rules, delay, noise);
            string route = m.Window == 0 ? "no-legal-window" : m.Candidates == 0 ? "no-geometric-candidates" : m.OnTime == 0 ? "i:no-on-time-candidate" : "ii:contact-selection";
            bool fault = f.Rules.Cause != PointEndCause.DoubleBounce;
            string bucket = f.Rules.Cause == PointEndCause.Out && f.Rules.TerminalSurface == SurfaceId.BackWall ? "d" : fault ? "e" : plan.Reachable ? "receivable" : m.Window == 0 ? "a" : m.OnTime > 0 ? "c" : m.Candidates > 0 ? "b" : "e";
            bool contact = plan.Contact.Exists && plan.Contact.TStarStep >= 0;
            bool afterBack = contact && f.BackInPlay && plan.Contact.TStarStep > Time(f.Back.Value) && f.First.HasValue && plan.Contact.TStarStep > Time(f.First.Value);
            Put(r, "plan", plan.Reason, "bucket", bucket, "route", plan.Reachable ? plan.Reason : route, "postBackContact", afterBack, "postBackWindow", m.AfterBack,
                "ruleSamples", m.RuleSamples, "belowMin", m.Below, "aboveMax", m.Above, "window", m.Window, "candidates", m.Candidates, "onTimeCandidates", m.OnTime,
                "candidateContactOk", m.ContactOk, "candidateContactRejected", m.ContactRejected,
                "stanceX", plan.Stance.x, "stanceZ", plan.Stance.y, "bodyContactX", plan.BodyAtContact.x, "bodyContactZ", plan.BodyAtContact.y,
                "arrivalSteps", plan.ArrivalStep, "slackS", plan.SlackS, "contactExists", contact, "contactReason", plan.Contact.Reason ?? "");
            if (m.BestLate != long.MaxValue)
                Put(r, "lateStarSteps", m.BestLate, "lateAvailableSteps", m.BestLateAvailable, "bestAvailableSteps", m.Available, "bestRequiredSteps", m.Required,
                    "bestStanceX", m.Stance.x, "bestStanceZ", m.Stance.y, "bestDistance", Vector2.Distance(wait, m.Stance));
            if (contact) Put(r, "contactSteps", plan.Contact.TStarStep, "contactX", plan.Contact.PStar.x, "contactY", plan.Contact.PStar.y, "contactZ", plan.Contact.PStar.z, "contactClearance", Clearance(plan.Contact.PStar));
        }

        private Mirror Inspect(Flight f, PlayerBody body, PointRules initial, int delay, Vector2 noise)
        {
            var m = new Mirror(); var window = new List<Sample>(); var rules = initial.Clone();
            var frame = ctx.FrameAt(body.Position); int next = 0;
            foreach (var state in f.Trajectory.States.Skip(1))
            {
                while (next < f.Trajectory.Contacts.Count && f.Trajectory.Contacts[next].Step <= state.Step) rules.OnBallContact(f.Trajectory.Contacts[next++]);
                if (rules.Closed) break;
                if (!rules.CanHit(initial.ToHit)) continue;
                m.RuleSamples++;
                if (state.Position.y < ctx.Legal.MinHeight) { m.Below++; continue; }
                if (state.Position.y > ctx.Legal.MaxHeight) { m.Above++; continue; }
                float gap = state.Position.y < frame.PreferredMinHeight ? frame.PreferredMinHeight - state.Position.y : state.Position.y > frame.PreferredMaxHeight ? state.Position.y - frame.PreferredMaxHeight : 0;
                float feature = Mathf.Max(0, 1 - gap / (frame.PreferredMaxHeight - frame.PreferredMinHeight)) + (frame.WallClearance <= 0 ? 1 : Mathf.Clamp01(Clearance(state.Position) / frame.WallClearance));
                window.Add(new Sample { Step = state.Step, Position = state.Position, Feature = feature });
                if (f.BackInPlay && state.Step > Time(f.Back.Value)) m.AfterBack++;
            }
            m.Window = window.Count;
            int interval = Math.Max(1, ctx.Steps(ctx.Params.CandidateIntervalS));
            for (int index = 0; index < window.Count; index++)
            {
                var sample = window[index]; if (sample.Step % interval != 0) continue;
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector2 stance = ctx.ClampBody(new Vector2(sample.Position.x + side * frame.IdealLateral, sample.Position.z) + noise);
                    bool In(int k) => (window[k].Position - new Vector3(stance.x, frame.ReferenceHeight, stance.y)).sqrMagnitude <= frame.Reach * frame.Reach;
                    if (!In(index)) continue;
                    m.Candidates++;
                    int first = index, last = index;
                    while (first > 0 && window[first - 1].Step == window[first].Step - 1 && In(first - 1)) first--;
                    while (last + 1 < window.Count && window[last + 1].Step == window[last].Step + 1 && In(last + 1)) last++;
                    int star = first;
                    for (int k = first + 1; k <= last; k++) if (window[k].Feature > window[star].Feature) star = k;
                    int untilStar = checked((int)window[star].Step), available = checked((int)sample.Step);
                    int arrival = body.StepsToReach(stance, delay, out _, Math.Max(untilStar, available) + ctx.StepHz);
                    if (arrival <= untilStar)
                    {
                        m.OnTime++;
                        var contact = ContactPlanner.Plan(f.Trajectory, ctx.FrameAt(stance), ctx.Court, ctx.Legal, 1);
                        if (contact.Exists && contact.TStarStep >= arrival) m.ContactOk++; else m.ContactRejected++;
                    }
                    else
                    {
                        long late = (long)arrival - untilStar;
                        if (late < m.BestLate) { m.BestLate = late; m.Available = untilStar; m.Required = arrival; m.Stance = stance; }
                        m.BestLateAvailable = Math.Min(m.BestLateAvailable, (long)arrival - available);
                    }
                }
            }
            return m;
        }

        private PointRules Rules(PlayerId receiver)
        { var rules = new PointRules(ctx.Court, (float)ctx.Params.FootprintRadius); rules.Begin(receiver.Other(), receiver); return rules; }
        private double Time(ContactEvent hit) => hit.Step - 1 + (double)hit.TimeInStep * ctx.StepHz;
        private float Clearance(Vector3 p) => Mathf.Min(Mathf.Min(p.x, ctx.Court.Width - p.x), Mathf.Min(p.z, ctx.Court.Length - p.z));
        private static bool Finite(Vector3 p) => !(float.IsNaN(p.x) || float.IsInfinity(p.x) || float.IsNaN(p.y) || float.IsInfinity(p.y) || float.IsNaN(p.z) || float.IsInfinity(p.z));
        private static string F(object value) => value == null ? "" : value is float f ? f.ToString("R", C) : value is double d ? d.ToString("R", C) : value is bool b ? (b ? "true" : "false") : Convert.ToString(value, C).Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        private static void Put(Dictionary<string, string> row, params object[] values)
        { for (int i = 0; i < values.Length; i += 2) row[(string)values[i]] = F(values[i + 1]); }
        private static void WriteRows(string path, List<Dictionary<string, string>> rows)
        {
            var keys = rows.SelectMany(r => r.Keys).Distinct().OrderBy(k => k, StringComparer.Ordinal).ToArray();
            using (var writer = new StreamWriter(path, false, Utf8))
            { writer.WriteLine(string.Join("\t", keys)); foreach (var row in rows) writer.WriteLine(string.Join("\t", keys.Select(k => row.TryGetValue(k, out var value) ? value : ""))); }
        }
        private static string Git(string root, string arguments)
        {
            using (var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("git", arguments) { WorkingDirectory = root, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true }))
            { string output = process.StandardOutput.ReadToEnd(), error = process.StandardError.ReadToEnd(); process.WaitForExit(); if (process.ExitCode != 0) throw new IOException(error); return output; }
        }
    }
}
