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
    // Explicit experiment for the distance-aware pace curve (method: docs/validation/2026-11-14-shot-pace-distance-method.md).
    // It describes data and applies the predeclared adoption rule; it is not part of regression or AutoQA.
    public sealed class ShotPaceDistanceExperiment
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private static readonly string[] Variants = { "gamma1.0", "gamma2.0", "gamma2.5" };
        private static readonly double[] Gammas = { 1.0, 2.0, 2.5 };
        private static readonly float[] SlAimX = { 1.2f, 2.6f, 4.0f };
        private static readonly float[] DropOffsets = { .05f, .10f, .15f };
        private static readonly float[] LowHeights = { .8f, 1.2f, 1.6f };
        private static readonly float[] MidHeights = { 1.8f, 2.2f, 2.55f };
        private static readonly int[] StepOffsets = { -12, 0, 12 };

        [Serializable] private sealed class RelMirror { public string id; public int evaluated, increasing, equal, counterexamples, notEvaluated; public double fractionIncreasing; }
        [Serializable] private sealed class SumMirror { public string source; public bool gitDirty; public List<RelMirror> relations = new List<RelMirror>(); }
        [Serializable] private sealed class Crit
        {
            public string id, rule;
            public int pass, den, passSolved, denSolved, passAll, denAll;
            public double fraction, fractionSolved, fractionAll, threshold;
            public bool met;
        }
        [Serializable] private sealed class SlNotEvaluated { public string contact, reason; }
        [Serializable] private sealed class SlSummary
        {
            public string variant;
            public float gammaMax;
            public int trials, solved, legal;
            public List<Crit> criteria = new List<Crit>();
            public List<SlNotEvaluated> notEvaluated = new List<SlNotEvaluated>();
            public List<string> contacts = new List<string>();
        }
        [Serializable] private sealed class VariantReport
        {
            public string variant;
            public double gammaMax;
            public SlSummary sl56;
            public List<RelMirror> relations;
            public bool s5Identical, s5SpeedLinear;
            public int s5Rows;
            public bool s1, s2, s3a, s3b, r3a, r3b, meetsAll;
            public double r3aFraction, r3AllPairsFraction;
            public int r3aEvaluated, r3P0Evaluated, r3bEvaluated, r3bIncreasing;
        }
        [Serializable] private sealed class Report
        {
            public string source, rule = "adopt gamma2.0 if S1,S2,S3a,S3b,R3a,R3b are met; else gamma2.5 if met; else stop (no other values)";
            public List<VariantReport> variants = new List<VariantReport>();
            public string adopted = "none-stop";
        }

        [Test, Explicit, Category("Experiments"), Timeout(21600000)]
        public void WriteShotPaceDistanceReport()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string run = Environment.GetEnvironmentVariable("SC_PACE_RUN");
            if(run != "run01" && run != "run02") throw new InvalidDataException("Set SC_PACE_RUN=run01 or run02; outputs are never overwritten.");
            string presetFile = Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json");
            byte[] bytes = File.ReadAllBytes(presetFile);
            var defaults = PresetCodec.LoadDefault(File.ReadAllText(presetFile));
            if(defaults.Hash != "06fed4bd4ac9" || defaults.Ordered.Count != 121 || defaults.Get("shot.paceGammaMax") != 2 || defaults.Get("shot.paceNearM") != 1.5 || defaults.Get("shot.paceFullM") != 6)
                throw new InvalidDataException("Protected defaults changed.");
            string outDir = Path.Combine(root, "Logs/experiments/pace", run);
            if(Directory.Exists(outDir)) throw new IOException("Experiment output already exists: " + outDir);
            Directory.CreateDirectory(outDir);
            var report = new Report { source = Git(root, "rev-parse HEAD") };
            if(Git(root, "status --porcelain").Length != 0) throw new InvalidDataException("Run from a clean committed source.");
            var perVariant = new List<(VariantReport r, string dir)>();
            for(int v = 0; v < Variants.Length; v++)
            {
                var preset = defaults.With("shot.paceGammaMax", Gammas[v]);
                string dir = Path.Combine(outDir, Variants[v]);
                SportScenarioExperiment.Harness(dir, preset, bytes, Variants[v]);
                var vr = new VariantReport { variant = Variants[v], gammaMax = Gammas[v], sl56 = Sl56(dir, preset, Variants[v]) };
                perVariant.Add((vr, dir));
            }
            // S5, R3 and the adoption rule from the files each variant wrote.
            var baselineB = BLines(perVariant[0].dir);
            foreach(var (vr, dir) in perVariant)
            {
                var sum = JsonUtility.FromJson<SumMirror>(File.ReadAllText(Path.Combine(dir, "summary.json")));
                vr.relations = sum.relations;
                var lines = BLines(dir);
                vr.s5Rows = lines.Count; vr.s5Identical = lines.SequenceEqual(baselineB);
                vr.s5SpeedLinear = BSpeedLinear(dir);
                RelMirror R(string id) => sum.relations.First(x => x.id == id);
                var a = R("R3a"); var b = R("R3b"); var p0 = R("R3-P0"); var all = R("R3");
                vr.r3aEvaluated = a.evaluated; vr.r3aFraction = a.fractionIncreasing; vr.r3P0Evaluated = p0.evaluated;
                vr.r3AllPairsFraction = all.fractionIncreasing; vr.r3bEvaluated = b.evaluated; vr.r3bIncreasing = b.increasing;
                vr.r3a = a.evaluated > 0 && a.fractionIncreasing >= .75;
                vr.r3b = b.evaluated > 0 && b.increasing == b.evaluated;
                Crit C(string id) => vr.sl56.criteria.First(x => x.id == id);
                vr.s1 = C("S1").met; vr.s2 = C("S2").met; vr.s3a = C("S3a").met; vr.s3b = C("S3b").met;
                vr.meetsAll = vr.s1 && vr.s2 && vr.s3a && vr.s3b && vr.r3a && vr.r3b && vr.s5Identical && vr.s5SpeedLinear;
                report.variants.Add(vr);
            }
            foreach(var vr in report.variants.Where(x => x.gammaMax > 1).OrderBy(x => x.gammaMax))
                if(vr.meetsAll) { report.adopted = vr.variant; break; }
            File.WriteAllText(Path.Combine(outDir, "report.json"), JsonUtility.ToJson(report, true) + "\n", Utf8);
            TestContext.WriteLine("PACE_DATA=" + outDir + " adopted=" + report.adopted);
            // A completed invocation means complete acquisition, not acceptance of the sensation (S6, R3d in the playtest).
        }

        private static List<string> BLines(string dir)
            => File.ReadLines(Path.Combine(dir, "trials.tsv")).Skip(1).Where(l => l.Split('\t')[1].StartsWith("B", StringComparison.Ordinal)).ToList();
        private static bool BSpeedLinear(string dir)
        {
            foreach(var l in File.ReadLines(Path.Combine(dir, "trials.tsv")).Skip(1))
            {
                var f = l.Split('\t'); if(!f[1].StartsWith("B", StringComparison.Ordinal)) continue;
                if(f[f.Length - 4] != f[f.Length - 3]) return false; // speedLinear == requested (same float, invariant text)
            }
            return true;
        }

        private static SlSummary Sl56(string dir, ParamSet preset, string variant)
        {
            var cfg = EffectiveConfig.From(preset);
            var factory = new AnalyticBallSimulationFactory(Physics.bounceThreshold);
            var court = cfg.Court; var physics = cfg.Physics;
            float footprint = (float)new RallyParams().FootprintRadius;
            var legal = new LegalSettings((float)preset.Get("legal.minHeight"), (float)preset.Get("legal.maxHeight"), physics.StepHz, physics.Gravity, physics.BallRadius, court);
            var heights = new List<(float y, string kind)>();
            foreach(float o in DropOffsets) heights.Add((court.TinTop + o, "drop"));
            foreach(float h in LowHeights) heights.Add((h, "low"));
            foreach(float h in MidHeights) heights.Add((h, "mid"));
            var summary = new SlSummary { variant = variant, gammaMax = (float)preset.Get("shot.paceGammaMax") };
            var rows = new List<(SportScenarioExperiment.Trial t, string kind, string group)>();
            int id = 0;
            using(var w = new StreamWriter(Path.Combine(dir, "sl56-trials.tsv"), false, Utf8))
            {
                w.NewLine = "\n";
                w.WriteLine("id\tcontact\tcx\tcy\tcz\taimX\taimY\tpower\tsolved\tspeedRaised\ttargetLowered\ttargetX\ttargetY\tspeed\tvx\tvy\tvz\tgoodFront\tlegal\tterminal\tfirstObserved\tx1\ty1\tz1\tsecondObserved\tx2\ty2\tz2\tfrontMargin\tsideBeforeFirst\tbackBeforeFirst\toppositeAfterFirst\tsideHeight\tsideDepth\tsideMin\tsideMax\tsettled\treason\tpaceDistance\tpaceGamma\tspeedLinear\trequested\tt1\tt2");
                foreach(var recipe in PracticeLaunchDefinitions.Create(cfg).Where(r => r.Id == "SL5" || r.Id == "SL6"))
                {
                    Trajectory trajectory;
                    using(var sim = factory.Create(physics, court))
                        trajectory = TrajectoryRunner.Run(sim, recipe.Initial, physics.StepHz * 10, StopCondition.SecondFloorBounce, court);
                    var plan = ContactPlanner.Plan(trajectory, recipe.Body, court, legal, 1);
                    foreach(int off in StepOffsets)
                    {
                        string label = recipe.Id + (off == 0 ? "-Tstar" : off < 0 ? "-Tstar" + off : "-Tstar+" + off);
                        if(!plan.Exists || plan.TStarStep < 0) { summary.notEvaluated.Add(new SlNotEvaluated { contact = label, reason = "no-plan:" + plan.Reason }); continue; }
                        long step = plan.TStarStep + off;
                        long first = trajectory.States[0].Step;
                        if(step < first || step >= first + trajectory.States.Count || !ContactPlanner.IsLegalStep(trajectory, step, recipe.Body, legal))
                        { summary.notEvaluated.Add(new SlNotEvaluated { contact = label, reason = "step-not-legal-or-outside-trajectory" }); continue; }
                        var contact = new SportScenarioExperiment.Contact { id = label, center = trajectory.States[(int)(step - first)].Position, body = recipe.Body.PositionXZ, tStarStep = (int)step };
                        summary.contacts.Add(label);
                        foreach(var (y, kind) in heights)
                            foreach(float x in SlAimX)
                                for(int k = 0; k <= 20; k++)
                                {
                                    var t = SportScenarioExperiment.Run(id++, 0, 0, 0, k, contact, new Vector2(x, y), preset, cfg, factory, footprint);
                                    rows.Add((t, kind, label));
                                    SportScenarioExperiment.WriteTrial(w, t);
                                }
                    }
                }
            }
            if(factory.LiveInstances != 0 || factory.ImpactCapReachedCount != 0) throw new InvalidDataException("P2 resource leak or impact cap: experiment invalid.");
            summary.trials = rows.Count; summary.solved = rows.Count(r => r.t.solved); summary.legal = rows.Count(r => r.t.legal);
            float vMax = (float)preset.Get("shot.vMax");
            Crit Make(string id2, string rule, double threshold, IEnumerable<(SportScenarioExperiment.Trial t, string kind, string group)> sample,
                Func<SportScenarioExperiment.Trial, bool> pass, bool primaryOnLegal)
            {
                var s = sample.ToArray();
                var denLegal = s.Where(r => r.t.solved && r.t.legal).ToArray(); var denSolved = s.Where(r => r.t.solved).ToArray();
                var primary = primaryOnLegal ? denLegal : denSolved;
                var c = new Crit { id = id2, rule = rule, threshold = threshold, den = primary.Length, pass = primary.Count(r => pass(r.t)),
                    denSolved = denSolved.Length, passSolved = denSolved.Count(r => pass(r.t)), denAll = s.Length, passAll = s.Count(r => pass(r.t) && r.t.solved) };
                c.fraction = c.den == 0 ? 0 : c.pass / (double)c.den; c.fractionSolved = c.denSolved == 0 ? 0 : c.passSolved / (double)c.denSolved;
                c.fractionAll = c.denAll == 0 ? 0 : c.passAll / (double)c.denAll; c.met = c.den > 0 && c.fraction >= threshold;
                return c;
            }
            var midP = new HashSet<int> { 8, 9, 10, 11, 12 };
            summary.criteria.Add(Make("S1", "P in [.40,.60], all aims: z2<=6.5 and no back wall before the first bounce", .9,
                rows.Where(r => midP.Contains(r.t.pi)), t => !t.backBeforeFirst && t.secondObserved && t.second.z <= 6.5f, true));
            summary.criteria.Add(Make("S2", "P<=.25, drop aims: z2<=2.0", .9, rows.Where(r => r.kind == "drop" && r.t.pi <= 5), t => t.secondObserved && t.second.z <= 2f, true));
            var s3a = rows.Where(r => r.t.pi == 20).ToArray();
            summary.criteria.Add(new Crit { id = "S3a", rule = "P=1: requested speed bit-identical to vMax", threshold = 1, den = s3a.Length,
                pass = s3a.Count(r => BitConverter.SingleToInt32Bits(r.t.requested) == BitConverter.SingleToInt32Bits(vMax)), denSolved = s3a.Count(r => r.t.solved), denAll = s3a.Length,
                fraction = s3a.Length == 0 ? 0 : s3a.Count(r => BitConverter.SingleToInt32Bits(r.t.requested) == BitConverter.SingleToInt32Bits(vMax)) / (double)s3a.Length });
            var c3a = summary.criteria.Last(); c3a.fractionSolved = c3a.fraction; c3a.fractionAll = c3a.fraction; c3a.met = c3a.den > 0 && c3a.pass == c3a.den;
            summary.criteria.Add(Make("S3b", "P=1, drop aims: back wall before first bounce or second bounce z>=8 (denominator solved; 90% chosen by the operator)", .9,
                rows.Where(r => r.kind == "drop" && r.t.pi == 20), t => t.backBeforeFirst || (t.secondObserved && t.second.z >= 8f), false));
            // S4: adjacent powers, same contact/aim, comparable pairs only.
            int pairs = 0, increasing = 0;
            var byKey = rows.GroupBy(r => (r.group, r.t.aim.x, r.t.aim.y)).ToArray();
            foreach(var g in byKey)
            {
                var ordered = g.OrderBy(r => r.t.pi).ToArray();
                for(int i = 0; i + 1 < ordered.Length; i++)
                {
                    var a = ordered[i].t; var b = ordered[i + 1].t;
                    if(!a.solved || !b.solved || a.speedRaised || b.speedRaised || a.targetLowered || b.targetLowered) continue;
                    if(a.sideBeforeFirst || b.sideBeforeFirst || a.backBeforeFirst || b.backBeforeFirst || !a.firstObserved || !b.firstObserved) continue;
                    pairs++; if(b.first.z > a.first.z) increasing++;
                }
            }
            summary.criteria.Add(new Crit { id = "S4", rule = "adjacent powers: first bounce increasing (trend, not in the adoption rule)", threshold = 1, den = pairs, pass = increasing,
                denSolved = pairs, passSolved = increasing, denAll = pairs, passAll = increasing, fraction = pairs == 0 ? 0 : increasing / (double)pairs,
                fractionSolved = pairs == 0 ? 0 : increasing / (double)pairs, fractionAll = pairs == 0 ? 0 : increasing / (double)pairs, met = pairs > 0 && increasing == pairs });
            File.WriteAllText(Path.Combine(dir, "sl56-summary.json"), JsonUtility.ToJson(summary, true) + "\n", Utf8);
            return summary;
        }

        private static string Git(string root, string arguments)
        {
            using(var p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("git", arguments) { WorkingDirectory = root, RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true }))
            { string output = p.StandardOutput.ReadToEnd().Trim(); p.WaitForExit(); if(p.ExitCode != 0) throw new IOException("Git metadata failed"); return output; }
        }
    }
}
