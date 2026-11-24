using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    // Two-wall preview (design 2026-11-24-two-wall-preview-hint): the cost of one BoastPreview.Estimate on this machine over a grid of
    // contacts, side points and powers, failures included (the solver's full budget). Explicit, outside regression and AutoQA; it only
    // measures and prints, nothing is asserted beyond the solver's own budget.
    public sealed class BoastPreviewCostExperiment
    {
        [Test, Explicit, Category("Experiments"), Timeout(3600000)]
        public void MeasureBoastPreviewCost()
        {
            var preset = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            var config = EffectiveConfig.From(preset); var court = config.Court;
            var factory = new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            // Warm-up outside the measurement.
            for (int i = 0; i < 20; i++) BoastPreview.Estimate(new Vector3(3.2f, .3f, 6.5f), new SideAim(SurfaceId.LeftWall, 3, 1.5f), .7f, config, factory, ShotIntentSolver.DefaultFootprintRadius, out _);
            var ms = new List<double>(); var evals = new List<int>(); int solved = 0;
            foreach (float x in new[] { .8f, 3.2f, 5.6f })
            foreach (float y in new[] { .3f, .6f, 1.0f })
            foreach (float z in new[] { 4.5f, 6.5f, 8.5f })
            {
                var contact = new Vector3(x, y, z); var wall = SideAim.NearWall(x, court);
                for (float sz = .8f; sz < z - .2f; sz += .8f)
                foreach (float sy in new[] { .6f, 1.2f, 1.8f, 2.4f })
                foreach (float p in new[] { .3f, .6f, 1f })
                {
                    var watch = Stopwatch.StartNew();
                    var s = BoastPreview.Estimate(contact, new SideAim(wall, sz, sy), p, config, factory, ShotIntentSolver.DefaultFootprintRadius, out var info);
                    watch.Stop(); ms.Add(watch.Elapsed.TotalMilliseconds); evals.Add(info.Evaluations); if (s.Solved) solved++;
                }
            }
            Assert.That(evals.Max(), Is.LessThanOrEqualTo(BoastSolver.Budget));
            int top = evals.Max(); double topMs = evals.Zip(ms, (e, m) => (e, m)).Where(t => t.e == top).Max(t => t.m);
            var sorted = ms.OrderBy(m => m).ToList(); int full = sorted.Count;
            double P(double q) => sorted[(int)System.Math.Min(full - 1, System.Math.Floor(q * full))];
            string line = $"boast-preview-cost n={full} solved={solved} evalsMax={top} evalsMean={evals.Average():F2} " +
                $"msP50={P(.5):F3} msP95={P(.95):F3} msMax={sorted[full - 1]:F3} msMaxAtEvalsMax={topMs:F3} cpu={SystemInfo.processorType}";
            TestContext.WriteLine(line); UnityEngine.Debug.Log(line);
        }
    }
}
