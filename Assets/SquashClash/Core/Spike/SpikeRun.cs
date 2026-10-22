using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Spike
{
    public sealed class SpikeOptions
    {
        public int RandomCount = 10000, Seed = 20261020;
        public string Device = "editor", Build, ConfigHash, Variant, ProjectPhysics;
    }
    // Main-thread, launch-granularity budget: one indivisible job may overrun it.
    public sealed class SpikeRun
    {
        private readonly IBallSimulationFactory factory;
        private readonly PhysicsParams physics;
        private readonly CourtDimensions court;
        private readonly SpikeOptions options;
        private readonly IEnumerator<Action> jobs;
        private readonly Dictionary<string, int> failures = new Dictionary<string, int>();
        private readonly Dictionary<string, int> notes = new Dictionary<string, int>();
        private readonly Dictionary<EnergyStepPopulation, (EnergyGainSample sample, string scenario)> energy = new Dictionary<EnergyStepPopulation, (EnergyGainSample, string)>();
        private readonly Dictionary<(EnergyStepPopulation, MeasurementStatus), int> energyCounts = new Dictionary<(EnergyStepPopulation, MeasurementStatus), int>();
        private readonly List<float> remaining = new List<float>();
        private IBallSimulation game, predictor;
        private bool finished;
        private int completedJobs, expectedJobs, noCorner, overTop, crossings, repeated, maxImpacts, persistent;
        private float maxPenetration, maxPrecision;
        private BackendCapabilities capabilities;
        public SpikeResult Result { get; }
        public string Phase { get; private set; } = "Preparando";
        public float Progress => finished ? (Result.Complete ? 1 : Mathf.Min(.999f, (float)completedJobs / expectedJobs)) : (float)completedJobs / expectedJobs;
        public SpikeRun(IBallSimulationFactory f, PhysicsParams p, CourtDimensions d, SpikeOptions o)
        {
            factory = f ?? throw new ArgumentNullException(nameof(f)); physics = p; court = d; options = o ?? throw new ArgumentNullException(nameof(o));
            if (o.RandomCount < 0) throw new ArgumentOutOfRangeException(nameof(o.RandomCount));
            Result = new SpikeResult { Backend = f.Name, Build = o.Build, ConfigHash = o.ConfigHash, Variant = o.Variant, Device = o.Device, ProjectPhysics = o.ProjectPhysics,
                Criteria = SpikeCriteria.CreateResults(!string.Equals(o.Device, "editor", StringComparison.OrdinalIgnoreCase)) };
            expectedJobs = 17 * 5 - 2 + o.RandomCount * 4 + (o.RandomCount + 99) / 100 + 36 + 2 + 2;
            jobs = Jobs();
        }
        public bool Step(double budgetMs)
        {
            if (finished) return true;
            if (budgetMs <= 0 || double.IsNaN(budgetMs) || double.IsInfinity(budgetMs)) throw new ArgumentOutOfRangeException(nameof(budgetMs));
            var watch = Stopwatch.StartNew();
            try
            {
                if (game == null) { game = factory.Create(physics, court); predictor = factory.Create(physics, court); capabilities = game.Capabilities; }
                do
                {
                    if (!jobs.MoveNext()) { Finish(); return true; }
                    jobs.Current(); completedJobs++;
                } while (watch.Elapsed.TotalMilliseconds < budgetMs);
                return false;
            }
            catch { Cancel(); throw; }
        }
        public void Cancel() { if (finished) return; Phase = "Cancelado — INCOMPLETO"; finished = true; Result.Complete = false; Release(); }
        private void Release() { game?.Dispose(); predictor?.Dispose(); game = predictor = null; jobs.Dispose(); }
        private IEnumerator<Action> Jobs()
        {
            var fixedCases = PhysicsScenarios.Fixed(court, physics);
            foreach (var scenario in fixedCases.Concat(PhysicsScenarios.Random(court, physics, options.RandomCount, options.Seed)))
            {
                var s = scenario; Trajectory robust = null, fromLaunch = null;
                yield return () => { Phase = s.Id + " robustez"; robust = Run(game, s.Start, s.MaxSteps, s.Stop); Robust(s, robust); };
                yield return () => { Phase = s.Id + " C6"; fromLaunch = Run(game, s.Start, s.MaxSteps, StopCondition.SecondFloorBounce); Compare("C6", s.Id, fromLaunch, Run(predictor, s.Start, s.MaxSteps, StopCondition.SecondFloorBounce)); };
                bool random = s.Id.StartsWith("S8-", StringComparison.Ordinal);
                if (!random || int.Parse(s.Id.Substring(3), CultureInfo.InvariantCulture) % 100 == 0)
                    yield return () => {
                        Phase = s.Id + " C5";
                        var a = random ? fromLaunch : Fresh(s.Start, s.MaxSteps, StopCondition.SecondFloorBounce);
                        Compare("C5", s.Id, a, Fresh(s.Start, s.MaxSteps, StopCondition.SecondFloorBounce));
                    };
                if (s.Id != "S10")
                {
                    yield return () => Relaunch(s, robust, 30, "k30");
                    yield return () => {
                        var contact = FirstImpact(robust);
                        Relaunch(s, robust, contact.HasValue ? contact.Value.Step + 1 : -1, "postcontacto");
                    };
                }
                if (s.Id == "S1" || s.Id == "S6") yield return () => ContinueWithoutReset(s);
            }
            foreach (var s in PhysicsScenarios.EdgeDirected(court, physics))
                yield return () => { Phase = s.Id + " robustez"; Robust(s, Run(game, s.Start, s.MaxSteps, s.Stop)); };
            foreach (var id in new[] { "S10", "S6" }) yield return () => {
                var prior = fixedCases.Single(s => s.Id == id); var s1 = fixedCases[0]; Phase = id + "→S1 C8";
                Run(game, prior.Start, prior.MaxSteps, prior.Stop);
                Compare("C8", id + "→S1", Run(game, s1.Start, s1.MaxSteps, StopCondition.SecondFloorBounce), Fresh(s1.Start, s1.MaxSteps, StopCondition.SecondFloorBounce));
            };
        }
        private Trajectory Run(IBallSimulation sim, BallState from, int maxSteps, StopCondition stop)
        {
            var t = TrajectoryRunner.Run(sim, from, maxSteps, stop, court); Track(t); return t;
        }
        private Trajectory Fresh(BallState from, int maxSteps, StopCondition stop)
        { using (var sim = factory.Create(physics, court)) return Run(sim, from, maxSteps, stop); }
        private void Track(Trajectory t)
        {
            bool finite = PhysicsMeasures.AllFinite(t, out var bad);
            Record("C2", finite, $"{Phase}; estado inválido índice={bad}");
            maxImpacts = Math.Max(maxImpacts, PhysicsMeasures.MaxImpactsInOneStep(t)); persistent += PhysicsMeasures.PersistentSamples(t);
            var penetration = PhysicsMeasures.MaxPenetration(t, court, physics.BallRadius);
            if (!float.IsNaN(penetration)) maxPenetration = Math.Max(maxPenetration, penetration);
        }
        private void Robust(PhysicsScenario s, Trajectory t)
        {
            bool finite = PhysicsMeasures.AllFinite(t, out _);
            if (PhysicsMeasures.ExitedOverTop(t, court)) overTop++;
            if (finite) { int n = PhysicsMeasures.Crossings(t, court, out var detail); crossings += n; Record("C1", n == 0, s.Id + ": " + detail); }
            else Record("C1", null, s.Id + ": estado inválido");
            bool cornerPopulation = s.Id == "S4" || s.Id == "S5" || s.Id == "S6" || s.Id.StartsWith("S8", StringComparison.Ordinal);
            if (cornerPopulation)
            {
                var all = PhysicsMeasures.MaxRelativeEnergyGain(t, physics, EnergyStepPopulation.Corner);
                if (all.Status == MeasurementStatus.NotApplicable) noCorner++;
                else Record("C3", all.Status == MeasurementStatus.NotEvaluable ? (bool?)null : all.RelativeGain <= SpikeCriteria.CornerEnergyGainLimit, s.Id + " " + EnergyText(all));
                foreach (var population in new[] { EnergyStepPopulation.Corner, EnergyStepPopulation.CornerWithImpact, EnergyStepPopulation.CornerPersistentOnly })
                {
                    var sample = population == EnergyStepPopulation.Corner ? all : PhysicsMeasures.MaxRelativeEnergyGain(t, physics, population);
                    var key = (population, sample.Status); energyCounts.TryGetValue(key, out int count); energyCounts[key] = count + 1;
                    if (sample.Status == MeasurementStatus.Measured && (!energy.TryGetValue(population, out var old) || sample.RelativeGain > old.sample.RelativeGain)) energy[population] = (sample, s.Id);
                }
            }
            if (s.Id == "S4" || s.Id == "S5" || s.Id == "S6" || s.Id == "S10")
            {
                try { int n = PhysicsMeasures.RepeatedImpacts(t, SpikeCriteria.RepeatedImpactMinNormalSpeed); repeated += n; Record("C4", n == 0, s.Id + ": repetidos=" + n); }
                catch (ArgumentException e) { Record("C4", null, s.Id + ": " + e.Message); }
            }
            if (s.Id.StartsWith("S9", StringComparison.Ordinal))
            {
                float mm = PhysicsMeasures.ContactPrecisionMm(t, s); bool valid = !float.IsNaN(mm) && !float.IsInfinity(mm);
                if (valid) maxPrecision = Math.Max(maxPrecision, mm);
                Record("C10", valid ? mm <= SpikeCriteria.ContactPrecisionLimitMm : (bool?)null, s.Id + ": error mm=" + F(mm));
                if (s.ExpectTouch.HasValue)
                {
                    var observed = FirstImpact(t);
                    if (!observed.HasValue || !Finite(observed.Value.UV)) Record("C11", null, s.Id + ": sin punto válido");
                    else {
                        var c = observed.Value; var line = CourtLines.Build(court).Single(b => b.Id == s.Line.Value);
                        bool touch = c.Surface == line.Surface && LineContactClassifier.Touches(line, c.UV, .01f);
                        Record("C11", touch == s.ExpectTouch.Value, $"{s.Id}: superficie={c.Surface}; UV={c.UV.ToString("F6")}; observado={touch}, esperado={s.ExpectTouch}");
                    }
                }
            }
            if (s.Id == "S5" || s.Id == "S6" || s.Id.StartsWith("S9", StringComparison.Ordinal))
            {
                var observed = FirstImpact(t);
                bool ideal = Ballistics.FirstPlaneHit(s.Start, physics.Gravity, physics.BallRadius, court, s.MaxSteps * physics.StepSeconds, out var face, out var time);
                string detail = observed.HasValue ? $"{s.Id}: observado={observed.Value.Surface}, punto={observed.Value.Point.ToString("F6")}, paso={observed.Value.Step}; ideal={face}, t={time:R}, punto={(Ballistics.PositionAt(s.Start, physics.Gravity, time)-CourtDimensions.InwardNormal(face)*physics.BallRadius).ToString("F6")}" : s.Id + ": sin contacto";
                Record("C12", observed.HasValue && ideal ? true : (bool?)null, detail, true);
            }
            if (s.Id == "S1" || s.Id == "S2" || s.Id == "S7" || s.Id.StartsWith("S9", StringComparison.Ordinal))
                for (int i = 0; i < t.Contacts.Count; i++) if (!t.Contacts[i].Persistent)
                {
                    float ratio = PhysicsMeasures.RemainingTimeConsumed(t, physics, i); bool valid = !float.IsNaN(ratio) && !float.IsInfinity(ratio);
                    if (valid) remaining.Add(ratio); Record("C13", valid ? true : (bool?)null, s.Id + " paso=" + t.Contacts[i].Step + " timing=" + t.Contacts[i].Timing);
                }
            if (s.Id == "S4" || s.Id == "S6")
            {
                var first = FirstImpact(t);
                var distinct = t.Contacts.Where(c => !c.Persistent).GroupBy(c => c.Surface).Select(g => g.First()).Take(2).ToArray();
                bool together = distinct.Length == 2 && distinct[0].Step == distinct[1].Step;
                Record("C15", first.HasValue ? true : (bool?)null, s.Id + ": primer paso=" + (first?.Step.ToString() ?? "ausente") + "; mismo paso=" + together + "; " + string.Join("; ", distinct.Select(c => $"{c.Surface}, paso={c.Step}, {c.Timing}, t={F(c.TimeInStep)}")), true);
            }
        }
        private void Relaunch(PhysicsScenario s, Trajectory baseline, long k, string label)
        {
            Phase = s.Id + " C7 " + label;
            int index = baseline.States.FindIndex(state => state.Step == k);
            if (index < 0 || k < 0 || index >= s.MaxSteps) { Record("C7", null, Phase + ": checkpoint no observable antes del fin/salida superior"); return; }
            var from = baseline.States[index]; int length = s.MaxSteps - index;
            Compare("C7", Phase, Run(game, from, length, StopCondition.SecondFloorBounce), Run(predictor, from, length, StopCondition.SecondFloorBounce));
        }
        private void ContinueWithoutReset(PhysicsScenario s)
        {
            Phase = s.Id + " C9";
            game.Reset(s.Start); var events = new List<ContactEvent>();
            for (int i = 0; i < 30; i++) { events.Clear(); game.Step(events); }
            var from = game.State; var prediction = Run(predictor, from, s.MaxSteps - 30, StopCondition.SecondFloorBounce);
            var actual = new Trajectory(); actual.States.Add(from); int bounces = 0;
            for (int i = 30; i < s.MaxSteps; i++)
            {
                events.Clear(); game.Step(events); actual.States.Add(game.State); actual.Contacts.AddRange(events);
                foreach (var c in events) if (c.Surface == SurfaceId.Floor && !c.Persistent) bounces++;
                if (game.State.Position.y > court.WallHeight) { actual.Exit = TrajectoryExit.OverTop; break; }
                if (bounces >= 2) break;
            }
            Track(actual); Compare("C9", s.Id, actual, prediction);
        }
        private void Compare(string id, string scenario, Trajectory a, Trajectory b)
        {
            bool equal = PhysicsMeasures.TrajectoriesBitwiseEqual(a, b, out int first);
            Record(id, equal, scenario + ": primera diferencia índice=" + first);
        }
        private CriterionResult Row(string id) => Result.Criteria.Single(c => c.Id == id);
        private void Record(string id, bool? pass, string detail, bool alwaysDetail = false)
        {
            var c = Row(id);
            if (id == "C2" || id == "C3" || id == "C7" || id == "C9" || id == "C12" || id == "C13" || id == "C15") c.Expected++;
            if (pass.HasValue) { c.Evaluated++; if (!pass.Value) { failures.TryGetValue(id, out int count); failures[id] = count + 1; } }
            else c.NotEvaluable++;
            if (alwaysDetail || pass != true)
            {
                notes.TryGetValue(id, out int n); if (n < 12) Result.Details.Add(id + " " + detail); notes[id] = n + 1;
            }
        }
        private void Finish()
        {
            foreach (var c in Result.Criteria)
            {
                if (c.Id == "C17") continue; // Filled only by the separate benchmark.
                c.CoverageComplete = c.Expected > 0 && c.Evaluated + c.NotEvaluable == c.Expected;
                c.Measured = $"fallos={FailureCount(c.Id)}; evaluados={c.Evaluated}; no evaluables={c.NotEvaluable}";
                c.Verdict = FailureCount(c.Id) > 0 ? Verdict.Fail : !c.CoverageComplete || c.NotEvaluable > 0 ? Verdict.NotEvaluated : c.Gating ? Verdict.Pass : Verdict.Info;
            }
            Row("C1").Measured += $"; atravesamientos={crossings}; salidas superiores={overTop}; seed={options.Seed}; S8={options.RandomCount}; S8d=36";
            Row("C3").Measured += "; no aplica (sin pasos de esquina, fuera de población)=" + noCorner;
            foreach (var population in new[] { EnergyStepPopulation.Corner, EnergyStepPopulation.CornerWithImpact, EnergyStepPopulation.CornerPersistentOnly })
            {
                string counts = string.Join(", ", new[] { MeasurementStatus.Measured, MeasurementStatus.NotApplicable, MeasurementStatus.NotEvaluable }.Select(status => status + "=" + (energyCounts.TryGetValue((population, status), out int n) ? n : 0)));
                Row("C3").Measured += "; " + population + " [" + counts + "]: " + (energy.TryGetValue(population, out var max) ? "máximo entre casos medidos: " + max.scenario + " " + EnergyText(max.sample) : "sin máximo medido; consultar estados explícitos");
            }
            Row("C4").Measured += "; impactos repetidos=" + repeated;
            Row("C10").Measured += "; error máximo mm=" + F(maxPrecision);
            remaining.Sort(); Row("C13").Measured += remaining.Count == 0 ? "; sin ratios disponibles" : "; mínimo=" + F(remaining[0]) + "; mediana=" + F(remaining[remaining.Count / 2]);
            Inform("C14", $"máximo impactos no persistentes/paso={maxImpacts}; tope configurable={capabilities.ImpactCapConfigurable}; parámetro maxImpactsPerStep no operativo con P1");
            Inform("C16", $"penetración máxima={F(maxPenetration * 1000)} mm; muestras persistentes={persistent} (todas las trayectorias ejecutadas)");
            Inform("C18", "RunsInEditMode=" + capabilities.RunsInEditMode + "; timing=" + capabilities.Timing);
            Result.Complete = true; finished = true; Phase = "Física completa; benchmark pendiente"; Release();
        }
        private int FailureCount(string id) => failures.TryGetValue(id, out int n) ? n : 0;
        private void Inform(string id, string measured) { var c = Row(id); c.Measured = measured; c.Expected = c.Evaluated = 1; c.CoverageComplete = true; c.Verdict = Verdict.Info; }
        private static ContactEvent? FirstImpact(Trajectory t) { foreach (var c in t.Contacts) if (!c.Persistent) return c; return null; }
        private static bool Finite(Vector2 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) && !float.IsNaN(value.y) && !float.IsInfinity(value.y);
        private static string F(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        private static string EnergyText(EnergyGainSample s) => $"{s.Status}, ganancia={F(s.RelativeGain)}, paso={s.Step}, rapidez antes={F(s.SpeedBefore)} m/s, después={F(s.SpeedAfter)} m/s";
    }
}
