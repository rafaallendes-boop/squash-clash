using System;
using System.Collections.Generic;
namespace SquashClash.Core.Spike
{
    public sealed class SpikeResult
    {
        public string Backend, Variant, ProjectPhysics, Build, Device, ConfigHash;
        public bool Complete;
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
                    if (found.Gating && (found.Verdict != Verdict.Pass || found.Evaluated <= 0 || found.NotEvaluable != 0 || string.IsNullOrWhiteSpace(found.Measured))) return false;
                }
                return true;
            }
        }
    }
}
