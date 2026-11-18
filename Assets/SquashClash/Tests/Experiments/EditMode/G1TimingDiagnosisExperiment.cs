using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
using static SquashClash.Experiments.EditMode.G1BackCourtDiagnosisExperiment;

namespace SquashClash.Experiments.EditMode
{
    // Read-only A1-bis. Neither the phase-A experiment nor product code is changed by this run.
    public sealed class G1TimingDiagnosisExperiment
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private readonly List<Dictionary<string, string>> plans = new List<Dictionary<string, string>>();
        private readonly List<Dictionary<string, string>> intervals = new List<Dictionary<string, string>>();

        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void WriteG1TimingDiagnosis()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string run = Environment.GetEnvironmentVariable("SC_G1_TIMING_RUN");
            Assert.That(run == "run01" || run == "run02", Is.True);
            Assert.That(Git(root, "status --porcelain"), Is.Empty, "Use clean, committed source.");
            string directory = Path.Combine(root, "Logs/experiments/g1-timing", run);
            Assert.That(Directory.Exists(directory), Is.False, "Do not overwrite evidence.");
            var preset = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash, Is.EqualTo("430da2faa818"));
            Assert.That(preset.Ordered.Count, Is.EqualTo(121));
            var ctx = new RallyContext(EffectiveConfig.From(preset), new RallyParams(), new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            Assert.That(ctx.Config.SimulationKey, Is.EqualTo("166a96e319c7"));
            Assert.That(ctx.Params.Hash, Is.EqualTo("7d50ddaf5d4f"));
            string frozen = Path.Combine(root, "docs/validation/evidence/2026-11-17-g1-diagnosis");
            var shots = Read(Path.Combine(frozen, "a1-shots.tsv"), false);
            var original = Read(Path.Combine(frozen, "a1-receptions.tsv"), true);
            Assert.That(shots.Count, Is.EqualTo(108));
            Assert.That(original.Count, Is.EqualTo(648));
            Directory.CreateDirectory(directory);
            var source = new Dictionary<string, string>();
            Put(source, "head", Git(root, "rev-parse HEAD").Trim(), "defaults", preset.Hash, "simulationKey", ctx.Config.SimulationKey,
                "rallyParams", ctx.Params.Hash, "unity", Application.unityVersion, "tempo", ctx.Tempo, "stepHz", ctx.StepHz,
                "assistMs", ctx.Timing.AssistMs, "validMs", ctx.Timing.ValidMs, "validLimit", ctx.Timing.ValidLimitOn,
                "earlySoft", ctx.Timing.EarlySoft, "latencyCompMs", ctx.Timing.LatencyCompMs, "bounceThreshold", Physics.bounceThreshold);
            WriteRows(Path.Combine(directory, "source.tsv"), new List<Dictionary<string, string>> { source });
            using (var samples = new StreamWriter(Path.Combine(directory, "samples.tsv"), false, Utf8))
            {
                samples.WriteLine("id\treceiver\tdifficulty\tsample\tdeltaMs\tresult\tfail\tcategory\tcontactStep\treason");
                foreach (var shot in shots)
                {
                    string id = shot["id"];
                    int index = int.Parse(id.Substring(3), C);
                    Vector3 from = new Vector3(F(shot, "fromX"), F(shot, "fromY"), F(shot, "fromZ"));
                    var solution = RallyContact.SolveIntention(ctx, from, new Vector2(F(shot, "aimX"), F(shot, "aimY")), F(shot, "power"));
                    Assert.That(solution.Solved, Is.True, id);
                    Assert.That(solution.Velocity.Equals(new Vector3(F(shot, "vx"), F(shot, "vy"), F(shot, "vz"))), Is.True, id + ": launch replay");
                    Assert.That(solution.TargetUsed.Equals(new Vector2(F(shot, "targetX"), F(shot, "targetY"))), Is.True, id + ": target replay");
                    var trajectory = ctx.Predict(new BallState { Position = from, Velocity = solution.Velocity });
                    foreach (var old in original.Where(r => r["id"] == id))
                    {
                        var who = (PlayerId)Enum.Parse(typeof(PlayerId), old["receiver"]);
                        var level = (AiLevel)Enum.Parse(typeof(AiLevel), old["difficulty"]);
                        var difficulty = AiDifficulty.For(ctx.Params, level);
                        var brain = new FondoAi(ctx, difficulty, 1111 + index);
                        Vector2 noise = who == PlayerId.Rival ? brain.StanceNoise() : Vector2.zero;
                        int delay = ctx.Steps(ctx.Params.ReactionS) + (who == PlayerId.Rival ? brain.PerceptionSteps : 0);
                        var body = new PlayerBody(who, ctx.WaitPosition(who), ctx.Move);
                        var rules = new PointRules(ctx.Court, (float)ctx.Params.FootprintRadius); rules.Begin(who.Other(), who);
                        var plan = ReceptionPlanner.Plan(ctx, trajectory, rules, body, delay, 1, noise);
                        Assert.That(plan.Reason, Is.EqualTo(old["plan"]), id + ": plan replay");
                        Assert.That(noise.Equals(new Vector2(F(old, "noiseX"), F(old, "noiseZ"))), Is.True, id + ": noise replay");
                        Assert.That(plan.Stance.Equals(new Vector2(F(old, "stanceX"), F(old, "stanceZ"))), Is.True, id + ": stance replay");
                        Assert.That(plan.BodyAtContact.Equals(new Vector2(F(old, "bodyContactX"), F(old, "bodyContactZ"))), Is.True, id + ": body replay");
                        bool usable = plan.Reachable && plan.Contact.Exists && plan.Contact.TStarStep >= 0;
                        Assert.That(usable, Is.EqualTo(old["contactExists"] == "true"), id + ": contact replay");
                        double sigma = who == PlayerId.Rival ? difficulty.TimingSigmaMs : 0;
                        var row = new Dictionary<string, string>(); plans.Add(row);
                        Put(row, "id", id, "receiver", who, "difficulty", level, "fixture", shot["fixture"], "power", F(shot, "power"),
                            "launchSpeed", solution.Speed, "launchNorm", solution.Velocity.magnitude, "flightCause", shot["cause"], "backInPlay", shot["backInPlay"],
                            "plan", plan.Reason, "usable", usable, "sigmaMs", sigma, "waitX", body.Position.x, "waitZ", body.Position.y,
                            "stanceX", plan.Stance.x, "stanceZ", plan.Stance.y, "bodyContactX", plan.BodyAtContact.x, "bodyContactZ", plan.BodyAtContact.y,
                            "noiseX", noise.x, "noiseZ", noise.y, "delaySteps", delay, "slackS", plan.SlackS, "postBackContact", old["postBackContact"],
                            "star", "", "contactSpeed", "", "kInMs", "", "kOutMs", "", "widthMs", "", "zeroResult", "", "zeroFail", "");
                        if (!usable) continue;
                        Assert.That(plan.Contact.TStarStep, Is.EqualTo(long.Parse(old["contactSteps"], C)), id + ": star replay");
                        Assert.That(plan.Contact.PStar.Equals(new Vector3(F(old, "contactX"), F(old, "contactY"), F(old, "contactZ"))), Is.True, id + ": contact position replay");
                        var clock = new ClockMap(); clock.Begin(1, 0, 0, ctx.Tempo, 1, ctx.StepHz);
                        Assert.That(clock.TrySimToReal(plan.Contact.TStarStep, 0, out double target), Is.True);
                        Assert.That(clock.TrySimToReal(plan.Contact.KIn.Step, plan.Contact.KIn.Fraction, out double kin), Is.True);
                        Assert.That(clock.TrySimToReal(plan.Contact.KOut.Step, plan.Contact.KOut.Fraction, out double kout), Is.True);
                        var zero = RallyContact.Resolve(ctx, clock, trajectory, plan.Contact, 0, 0);
                        Assert.That(Category(zero), Is.EqualTo("Contact"), id + ": zero release");
                        Put(row, "star", plan.Contact.TStarStep, "contactSpeed", trajectory.States[checked((int)plan.Contact.TStarStep)].Velocity.magnitude,
                            "kInMs", (kin - target) * 1000, "kOutMs", (kout - target) * 1000, "widthMs", (kout - kin) * 1000,
                            "zeroResult", zero.Result, "zeroFail", zero.Fail);
                        DescribeCells(ctx, trajectory, plan.Contact, clock, target, id, who, level);
                        if (who != PlayerId.Rival) continue;
                        var random = new RallyRng(20261117 + index);
                        for (int sample = 0; sample < 200; sample++)
                        {
                            double delta = random.NextGaussian(sigma);
                            var decision = RallyContact.Resolve(ctx, clock, trajectory, plan.Contact, 0, delta);
                            string category = Category(decision);
                            Assert.That(category == "Contact" || category == "Air" || category == "Late", Is.True, id + ": sampled release");
                            samples.WriteLine(string.Join("\t", new[] { id, who.ToString(), level.ToString(), sample.ToString(C), N(delta), decision.Result.ToString(), decision.Fail.ToString(), category, decision.ContactStep.ToString(C), decision.Reason ?? "" }));
                        }
                    }
                }
            }
            Assert.That(plans.Count, Is.EqualTo(648));
            Assert.That(plans.Count(r => r["receiver"] == "Rival" && r["usable"] == "true"), Is.EqualTo(291));
            Assert.That(plans.Count(r => r["receiver"] == "Human" && r["usable"] == "true"), Is.EqualTo(297));
            WriteRows(Path.Combine(directory, "plans.tsv"), plans);
            WriteRows(Path.Combine(directory, "intervals.tsv"), intervals);
            TestContext.WriteLine(directory);
        }

        private void DescribeCells(RallyContext ctx, Trajectory trajectory, ContactPlan plan, ClockMap clock, double target, string id, PlayerId who, AiLevel level)
        {
            var cuts = new SortedSet<double> { 0, -ctx.Timing.AssistMs, ctx.Timing.AssistMs, -ctx.Timing.ValidMs, ctx.Timing.ValidMs,
                -target * 1000, -target * 1000 - ctx.Timing.LatencyCompMs };
            void Cut(double position)
            {
                var when = SimTime.FromSteps(position);
                if (when.Step >= 0 && clock.TrySimToReal(when.Step, when.Fraction, out double real)) cuts.Add((real - target) * 1000);
            }
            Cut(plan.KIn.Steps); Cut(plan.KOut.Steps); Cut(plan.TStarStep);
            foreach (var state in trajectory.States)
                if (state.Step >= Math.Floor(plan.KIn.Steps) - 1 && state.Step <= Math.Ceiling(plan.KOut.Steps) + 1)
                { Cut(state.Step); Cut(state.Step + .5); }
            foreach (var hit in trajectory.Contacts)
            {
                double at = hit.Step - 1 + (double)hit.TimeInStep * ctx.StepHz;
                if (at >= plan.KIn.Steps - 1 && at <= plan.KOut.Steps + 1) Cut(at);
            }
            var bounds = new List<double> { double.NegativeInfinity }; bounds.AddRange(cuts); bounds.Add(double.PositiveInfinity);
            for (int i = 0; i + 1 < bounds.Count; i++)
            {
                double low = bounds[i], high = bounds[i + 1];
                double Probe(double fraction) => double.IsNegativeInfinity(low) ? high - 10000 * fraction : double.IsPositiveInfinity(high) ? low + 10000 * fraction : low + (high - low) * fraction;
                string At(double fraction) => Category(RallyContact.Resolve(ctx, clock, trajectory, plan, 0, Probe(fraction)));
                string category = At(.5);
                Assert.That(At(.25), Is.EqualTo(category), id + ": nonconstant decision cell");
                Assert.That(At(.75), Is.EqualTo(category), id + ": nonconstant decision cell");
                Assert.That(category == "Contact" || category == "Air" || category == "Late", Is.True, id + ": unexpected decision cell");
                var row = new Dictionary<string, string>(); intervals.Add(row);
                Put(row, "id", id, "receiver", who, "difficulty", level, "lowMs", low, "highMs", high, "category", category);
            }
        }

        private static string Category(ContactDecision decision)
            => decision.Result == ShotResult.Perfect || decision.Result == ShotResult.Valid ? "Contact" : decision.Result == ShotResult.Failed ? decision.Fail.ToString() : decision.Result.ToString();
        private static float F(Dictionary<string, string> row, string key) => float.Parse(row[key], C);
        private static string N(double value) => value.ToString("R", C);
        private static List<Dictionary<string, string>> Read(string path, bool frozenEmptyHeader)
        {
            var lines = File.ReadAllLines(path); var keys = lines[0].Split('\t');
            Assert.That(keys.Distinct().Count(), Is.EqualTo(keys.Length));
            Assert.That(keys.Count(string.IsNullOrWhiteSpace), Is.EqualTo(frozenEmptyHeader ? 1 : 0));
            var rows = new List<Dictionary<string, string>>();
            foreach (string line in lines.Skip(1))
            {
                var values = line.Split('\t'); Assert.That(values.Length, Is.EqualTo(keys.Length));
                var row = keys.Select((k, i) => (k, values[i])).ToDictionary(p => p.k, p => p.Item2);
                if (frozenEmptyHeader) Assert.That(row[""], Is.EqualTo("bodyContactZ"));
                rows.Add(row);
            }
            return rows;
        }
        private static string Git(string root, string arguments)
        {
            using (var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("git", arguments) { WorkingDirectory = root, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true }))
            { string output = process.StandardOutput.ReadToEnd(), error = process.StandardError.ReadToEnd(); process.WaitForExit(); if (process.ExitCode != 0) throw new IOException(error); return output; }
        }
    }
}
