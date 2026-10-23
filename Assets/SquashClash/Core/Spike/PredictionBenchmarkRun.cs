using System;
using System.Collections.Generic;
using System.Diagnostics;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
namespace SquashClash.Core.Spike
{
    public sealed class PredictionBenchmarkRun
    {
        private readonly CourtDimensions court;
        private readonly PhysicsScenario scenario;
        private readonly int warmup, runs;
        private readonly List<double> samples = new List<double>();
        private IBallSimulation simulation;
        private int count;
        private bool finished, cancelled;
        public int Samples => samples.Count;
        public PredictionBenchmarkRun(IBallSimulationFactory f, PhysicsParams p, CourtDimensions d, int warmup = 20, int runs = 200)
        {
            if (warmup < 0 || runs <= 0) throw new ArgumentOutOfRangeException();
            court = d; this.warmup = warmup; this.runs = runs;
            scenario = PhysicsScenarios.Fixed(d, p)[0]; simulation = f.Create(p, d);
        }
        public bool Step(double budgetMs)
        {
            if (finished) return true;
            if (budgetMs <= 0 || double.IsNaN(budgetMs) || double.IsInfinity(budgetMs)) throw new ArgumentOutOfRangeException(nameof(budgetMs));
            var budget = Stopwatch.StartNew();
            try
            {
                do
                {
                    var watch = Stopwatch.StartNew();
                    // TrajectoryRunner includes Reset. Do not time a second Reset or scene creation.
                    var trajectory = TrajectoryRunner.Run(simulation, scenario.Start, scenario.MaxSteps, StopCondition.SecondFloorBounce, court);
                    watch.Stop();
                    if (!PhysicsMeasures.AllFinite(trajectory, out _)) throw new InvalidOperationException("Nonfinite benchmark trajectory.");
                    int bounces = 0; foreach (var c in trajectory.Contacts) if (c.Surface == SurfaceId.Floor && !c.Persistent) bounces++;
                    if (bounces < 2) throw new InvalidOperationException("S1 benchmark did not reach second floor bounce.");
                    if (count++ >= warmup) samples.Add(watch.Elapsed.TotalMilliseconds);
                    if (samples.Count == runs) { finished = true; Release(); return true; }
                } while (budget.Elapsed.TotalMilliseconds < budgetMs);
                return false;
            }
            catch { Cancel(); throw; }
        }
        public (double median, double p95, double p99, double max) ResultMs
        {
            get
            {
                if (!finished || cancelled || samples.Count != runs) throw new InvalidOperationException("Benchmark incomplete.");
                var sorted = samples.ToArray(); Array.Sort(sorted); int n = sorted.Length;
                double median = n % 2 == 0 ? (sorted[n / 2 - 1] + sorted[n / 2]) / 2 : sorted[n / 2];
                return (median, sorted[(int)Math.Ceiling(.95 * n) - 1], sorted[(int)Math.Ceiling(.99 * n) - 1], sorted[n - 1]);
            }
        }
        public void Cancel() { if (finished) return; cancelled = finished = true; Release(); }
        private void Release() { simulation?.Dispose(); simulation = null; }
        public void ApplyTo(SpikeResult result)
        {
            var stats = ResultMs;
            result.PredictionSamplesMs.Clear();result.PredictionSamplesMs.AddRange(samples);
            var c = result.Criteria.Find(row => row.Id == "C17");
            c.Evaluated = Samples; c.NotEvaluable = 0; c.Expected = 200; c.CoverageComplete = warmup == 20 && runs == 200;
            c.Measured = FormattableString.Invariant($"n={Samples}; warmup={warmup}; median={stats.median:F6}; p95={stats.p95:F6}; p99={stats.p99:F6}; max={stats.max:F6} ms; percentiles nearest rank");
            c.Verdict = !c.CoverageComplete ? Verdict.NotEvaluated : c.Gating ? (stats.p95 <= SpikeCriteria.PredictionP95LimitMs ? Verdict.Pass : Verdict.Fail) : Verdict.Info;
        }
    }
}
