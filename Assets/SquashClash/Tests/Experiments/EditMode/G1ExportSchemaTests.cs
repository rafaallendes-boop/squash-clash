using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Experiments.EditMode
{
    [Category("Experiments")]
    public sealed class G1ExportSchemaTests
    {
        private static readonly Type Exporter = typeof(G1BackCourtDiagnosisExperiment);
        private static object Call(string name, params object[] args)
        {
            try { return Exporter.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, args); }
            catch (TargetInvocationException error)
            { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); throw; }
        }

        [Test]
        public void IncompletePairsAreRejectedBeforeMutatingRow()
        {
            var row = new Dictionary<string, string>();
            Assert.Throws<ArgumentException>(() => Call("Put", row, new object[] { "first", 1, "unpaired" }));
            Assert.That(row, Is.Empty);
        }

        [Test]
        public void EmptyKeyIsRejectedBeforeMutatingRow()
        {
            var row = new Dictionary<string, string>();
            Assert.Throws<ArgumentException>(() => Call("Put", row, new object[] { "first", 1, "", "wrong" }));
            Assert.That(row, Is.Empty);
        }

        [Test]
        public void EmptyHeaderIsRejectedBeforeCreatingFile()
        {
            string path = Path.Combine(Path.GetTempPath(), "g1-schema-" + Guid.NewGuid().ToString("N") + ".tsv");
            try
            {
                var rows = new List<Dictionary<string, string>> { new Dictionary<string, string> { { "", "wrong" } } };
                Assert.Throws<ArgumentException>(() => Call("WriteRows", path, rows));
                Assert.That(File.Exists(path), Is.False);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Test]
        public void ReceptionInitializerPreservesEveryNamedPair()
        {
            var source = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json")));
            var context = new RallyContext(EffectiveConfig.From(source), new RallyParams(), new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            var exporter = new G1BackCourtDiagnosisExperiment();
            Exporter.GetField("ctx", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(exporter, context);
            var flight = Activator.CreateInstance(Exporter.GetNestedType("Flight", BindingFlags.NonPublic), true);
            Exporter.GetMethod("Receive", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(exporter, new[] { flight, (object)0, PlayerId.Rival, AiLevel.Medium });
            var rows = (IList)Exporter.GetField("receptions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(exporter);
            var row = (Dictionary<string, string>)rows[0];
            Assert.That(row.ContainsKey(""), Is.False, "The frozen phase-A defect must not recur in new exports.");
            foreach (string key in new[] { "stanceX", "stanceZ", "bodyContactX", "bodyContactZ", "arrivalSteps" })
            { Assert.That(row.ContainsKey(key), Is.True, key); Assert.That(row[key], Is.Empty, key); }
        }
    }
}
