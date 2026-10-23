using System;
using System.Collections.Generic;
namespace SquashClash.Core.Spike
{
    public sealed class SpikeResult
    {
        public string Backend, Variant, ProjectPhysics, Build, Device, ConfigHash;
        public string Profile, OS, StartedAtUtc, EndedAtUtc, ThermalStart, ThermalEnd, ThermalBenchmarkStart;
        public int FocusInterruptions;
        public bool Complete;
        public readonly List<string> Details = new List<string>();
        public List<CriterionResult> Criteria = new List<CriterionResult>();
        public bool AllGatingPass
        {
            get
            {
                if (!Complete || Criteria == null || Criteria.Count != 18) return false;
                // Only explicitly labelled editor runs have an informational C17.
                var expected = SpikeCriteria.CreateResults(!string.Equals(Device, "editor", StringComparison.OrdinalIgnoreCase));
                foreach (var definition in expected)
                {
                    CriterionResult found = null;
                    foreach (var c in Criteria)
                    {
                        if (c == null || c.Id != definition.Id) continue;
                        if (found != null) return false;
                        found = c;
                    }
                    if (found == null || found.Gating != definition.Gating) return false;
                    if (found.Gating)
                    {
                        if (found.Verdict != Verdict.Pass || !found.CoverageComplete || found.Expected <= 0 ||
                            found.Evaluated != found.Expected || found.NotEvaluable != 0 || string.IsNullOrWhiteSpace(found.Measured)) return false;
                        if (definition.Expected > 0 && found.Expected != definition.Expected) return false;
                        if (found.Id == "C2" && found.Expected < SpikeCriteria.RequiredRobustnessLaunches) return false;
                    }
                }
                return true;
            }
        }
    }
}
