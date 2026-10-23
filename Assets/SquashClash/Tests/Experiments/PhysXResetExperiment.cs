using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
using SquashClash.Core.Spike;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SquashClash.Experiments
{
    // Physical equality is data here, as in the full spike. This does not modify Reset.
    public sealed class PhysXResetExperiment
    {
        [UnityTest, Timeout(180000)]
        public IEnumerator ResetAfterContactHistory_ComparedWithFreshAndSameHistoryControls()
        {
            foreach (var app in Object.FindObjectsByType<AppBootstrap>(FindObjectsSortMode.None)) Object.Destroy(app.gameObject);
            yield return null; yield return null;
            var savedGravity = UnityEngine.Physics.gravity; int scenes = SceneManager.sceneCount;
            var cfg = EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
            var fixedCases = PhysicsScenarios.Fixed(cfg.Court, cfg.Physics);
            var targets = fixedCases.Where(s => s.Id == "S1" || s.Id == "S5" || s.Id == "S6").Concat(
                PhysicsScenarios.Random(cfg.Court, cfg.Physics, 4001, 20261020).Where(s => s.Id == "S8-62" || s.Id == "S8-4000")).ToArray();
            var histories = fixedCases.Where(s => s.Id == "S10" || s.Id == "S6").ToArray();
            var factory = new PhysXBallSimulationFactory();
            var report = new StringBuilder("# Diagnóstico de Reset e historial de contactos\n\n");
            report.AppendLine("Editor; baseline corregida CCD; configuración=" + cfg.Hash + "; fuente=" + Environment.GetEnvironmentVariable("SQUASH_SPIKE_COMMIT"));
            report.AppendLine("Igualdad según medida aprobada (estados bit a bit y contactos superficie/paso). Diferencias son datos; no se altera Reset ni se atribuye automáticamente la causa a caché interna.\n");
            report.AppendLine("| Control/historial | Objetivo | Contactos previos A/B | Igualdad | Primera diferencia |\n|---|---|---|---|---|");
            try
            {
                foreach (var target in targets)
                {
                    using (var a = factory.Create(cfg.Physics, cfg.Court))
                    using (var b = factory.Create(cfg.Physics, cfg.Court))
                        Compare("nueva/nueva", target, Run(a, target), Run(b, target), 0, 0);
                    foreach (var history in histories)
                    {
                        using (var a = factory.Create(cfg.Physics, cfg.Court))
                        {
                            int contacts = Run(a, history).Contacts.Count;
                            using (var b = factory.Create(cfg.Physics, cfg.Court))
                                Compare(history.Id + "/nueva", target, Run(a, target), Run(b, target), contacts, 0);
                        }
                        using (var a = factory.Create(cfg.Physics, cfg.Court))
                        using (var b = factory.Create(cfg.Physics, cfg.Court))
                        {
                            int ac = Run(a, history).Contacts.Count, bc = Run(b, history).Contacts.Count;
                            Compare(history.Id + "/" + history.Id, target, Run(a, target), Run(b, target), ac, bc);
                        }
                    }
                    // Bound pending scene unloads too, without adding simulation steps.
                    for (int i = 0; i < 120 && factory.PendingUnloads != 0; i++) yield return null;
                }
                Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/physics-reset-diagnostic.md", report.ToString());
            }
            finally { UnityEngine.Physics.gravity = savedGravity; }
            for (int i = 0; i < 120 && factory.PendingUnloads != 0; i++) yield return null;
            Assert.That(factory.LiveInstances, Is.Zero); Assert.That(factory.PendingUnloads, Is.Zero);
            Assert.That(SceneManager.sceneCount, Is.EqualTo(scenes)); Assert.That(UnityEngine.Physics.gravity, Is.EqualTo(savedGravity));

            Trajectory Run(IBallSimulation sim, PhysicsScenario s) => TrajectoryRunner.Run(sim, s.Start, s.MaxSteps, s.Stop, cfg.Court);
            void Compare(string history, PhysicsScenario target, Trajectory a, Trajectory b, int ac, int bc)
            {
                bool equal = PhysicsMeasures.TrajectoriesBitwiseEqual(a, b, out int first);
                report.AppendLine($"| {history} | {target.Id} | {ac}/{bc} | {equal} | {first} |");
                if (equal) return;
                report.AppendLine($"\n{history} → {target.Id}, índice {first} (contexto de primer paso divergente):\n");
                for (int i = Math.Max(0, first - 1); i <= first + 1; i++)
                {
                    Dump("A", a, i); Dump("B", b, i);
                }
                report.AppendLine();
            }
            void Dump(string side, Trajectory t, int index)
            {
                if (index >= t.States.Count) { report.AppendLine($"- {side}[{index}]: ausente"); return; }
                var s = t.States[index];
                string bits = string.Join(",", new[] { s.Position.x,s.Position.y,s.Position.z,s.Velocity.x,s.Velocity.y,s.Velocity.z }.Select(v => BitConverter.SingleToInt32Bits(v).ToString("X8", CultureInfo.InvariantCulture)));
                report.AppendLine($"- {side}[{index}] step={s.Step}; p={s.Position.ToString("R", CultureInfo.InvariantCulture)}; v={s.Velocity.ToString("R", CultureInfo.InvariantCulture)}; bits={bits}");
                foreach (var c in t.Contacts.Where(c => c.Step == s.Step))
                    report.AppendLine($"  - {c.Surface}; persistent={c.Persistent}; {c.Timing}; p={c.Point.ToString("R", CultureInfo.InvariantCulture)}; n={c.Normal.ToString("R", CultureInfo.InvariantCulture)}");
            }
        }
    }
}
