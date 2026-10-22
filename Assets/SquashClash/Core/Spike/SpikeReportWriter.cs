using System;
using System.Collections.Generic;
using System.Text;
namespace SquashClash.Core.Spike
{
    public static class SpikeReportWriter
    {
        public static string ToMarkdown(SpikeResult r)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            var text = new StringBuilder("# Prueba de física\n\n");
            text.AppendLine(r.Complete ? "Estado: **COMPLETO**" : "Estado: **INCOMPLETO**");
            text.AppendLine(r.AllGatingPass ? "Criterios de gate evaluados: **Pass**" : "Criterios de gate: **NO APROBADOS / NO EVALUADOS**");
            text.AppendLine("Este cálculo no elige backend ni autoriza el gate I4c.\n");
            text.AppendLine($"Backend: {Escape(r.Backend)}; variante: {Escape(r.Variant)}; build: {Escape(r.Build)}");
            text.AppendLine($"Dispositivo: {Escape(r.Device)}; configuración: {Escape(r.ConfigHash)}");
            text.AppendLine($"ProjectPhysics: {Escape(r.ProjectPhysics)}\n");
            text.AppendLine("| ID | Requisito | Escenarios | Medición | Umbral | Veredicto | Gate | evaluados/no evaluables/esperados | Cobertura |");
            text.AppendLine("|---|---|---|---|---|---|---|---|---|");
            var expected = SpikeCriteria.CreateResults(!string.Equals(r.Device, "editor", StringComparison.OrdinalIgnoreCase));
            var known = new HashSet<string>();
            foreach (var definition in expected)
            {
                known.Add(definition.Id);
                CriterionResult row = null; int count = 0;
                if (r.Criteria != null) foreach (var candidate in r.Criteria)
                    if (candidate != null && candidate.Id == definition.Id) { row = candidate; count++; }
                if (count != 1) { row = definition; row.Measured = count == 0 ? "Falta criterio" : "Criterio duplicado"; }
                WriteRow(text, row);
            }
            if (r.Criteria != null) foreach (var row in r.Criteria)
                if (row != null && !known.Contains(row.Id ?? "")) { text.AppendLine("\nCriterio desconocido (no permite aprobación):"); WriteRow(text, row); }
            text.AppendLine("\n## Evidencia por caso (hasta 12 ejemplos por criterio; contadores completos arriba)\n");
            foreach (var detail in r.Details) text.AppendLine("- " + Escape(detail));
            return text.ToString();
        }
        private static void WriteRow(StringBuilder text, CriterionResult c) => text.AppendLine(
            $"| {Escape(c.Id)} | {Escape(c.Requirement)} | {Escape(c.Scenarios)} | {Escape(c.Measured)} | {Escape(c.Threshold)} | {c.Verdict} | {(c.Gating ? "sí" : "no")} | {c.Evaluated}/{c.NotEvaluable}/{c.Expected} | {(c.CoverageComplete ? "cerrada" : "abierta")} |");
        private static string Escape(string value) => (value ?? "—").Replace("|", "\\|").Replace("\r", "").Replace("\n", "<br>");
    }
}
