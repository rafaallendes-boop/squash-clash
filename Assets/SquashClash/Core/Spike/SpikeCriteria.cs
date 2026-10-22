using System.Collections.Generic;
namespace SquashClash.Core.Spike
{
    public enum Verdict { Pass, Fail, Info, NotEvaluated }
    public sealed class CriterionResult
    {
        public string Id, Requirement, Scenarios, Measured, Threshold;
        public Verdict Verdict = Verdict.NotEvaluated;
        public bool Gating;
        public int Evaluated, NotEvaluable;
    }
    public static class SpikeCriteria
    {
        public const float CornerEnergyGainLimit = 1e-5f, ContactPrecisionLimitMm = 1f, RepeatedImpactMinNormalSpeed = .5f;
        public const double PredictionP95LimitMs = 1;

        public static List<CriterionResult> CreateResults(bool onIphone)
        {
            var rows = new List<CriterionResult>(18);
            void Add(string id, string requirement, string scenarios, string threshold, bool gating) => rows.Add(new CriterionResult {
                Id = id, Requirement = requirement, Scenarios = scenarios, Threshold = threshold, Gating = gating, Measured = "No evaluado"
            });
            Add("C1", "Robustez (§8.2.2, §17.A.1): Crossings", "S1–S10; S8 (10 000); S8d", "0 atravesamientos", true);
            Add("C2", "Estados finitos: AllFinite", "Todos", "Todos finitos", true);
            Add("C3", "Esquinas sin ganancia (§8.2.2): MaxRelativeEnergyGain", "S4, S5, S6, S8, S8d; pasos de esquina", "≤ 1e-5 (ruido float)", true);
            Add("C4", "Sin vibración (§8.2.2): RepeatedImpacts", "S4, S5, S6, S10", "0 (|vn| > 0,5 m/s; no persistentes)", true);
            Add("C5", "Consistencia (§17.A.2): igualdad bit a bit", "S1–S7, S9, S10 nuevas; 1/100 S8 reutilizada/nueva", "Igualdad de estados y contactos", true);
            Add("C6", "Predicción desde lanzamiento (§8.2.1, §17.A.3)", "S1–S10 y S8 completo; Reset(s)", "Igualdad bit a bit", true);
            Add("C7", "Relanzamiento en vuelo (§17.A.3)", "S1–S7, S9, S8 completo; k=30 y siguiente al primer contacto; Reset de ambos", "Igualdad bit a bit", true);
            Add("C8", "Reutilización (§8.2.1 cond.1)", "S10→S10b (S1 reutilizada), S6→S1 frente a nueva", "Igualdad bit a bit", true);
            Add("C9", "Continuación sin reiniciar el juego (D-3)", "S1, S6; k=30", "Diagnóstico; no decide el gate", false);
            Add("C10", "Precisión de contacto S9: ContactPrecisionMm", "S9a–i", "≤ 1 mm [EXPERIMENTO, D-1]", true);
            Add("C11", "Clasificación de líneas: punto observado", "S9 fuera/dentro; borde exacto solo precisión", "Igual a ExpectTouch", true);
            Add("C12", "Primer impacto (§8.3.1)", "S5, S6, S9", "Informe al gate; sin umbral inventado", false);
            Add("C13", "Tiempo restante (§8.3.2): mediana y mínimo", "Impactos únicos S1, S2, S7, S9", "Informe al gate; ratio sin umbral", false);
            Add("C14", "Impactos por paso (§8.3.2)", "Todos", "Informe al gate; tope configurable sí/no (P1: no)", false);
            Add("C15", "Simultáneos e instante (§8.3.3, §8.3.7)", "S4, S6", "Mismo paso y ContactTiming; informe al gate", false);
            Add("C16", "Penetración y muestras persistentes", "Todos", "Informativo", false);
            Add("C17", "Rendimiento (§8.2.2, §17.C, D-2)", "S1 hasta segundo bote; Reset incluido; 20 warmup y 200 mediciones", "p95 ≤ 1 ms en iPhone; editor informativo", onIphone);
            Add("C18", "Facilidad de prueba (§8.2.2)", "Capabilities.RunsInEditMode", "Preferencia, no gate", false);
            return rows;
        }
    }
}
