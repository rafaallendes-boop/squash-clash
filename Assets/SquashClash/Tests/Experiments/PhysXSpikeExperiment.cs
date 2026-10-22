using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Spike;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
namespace SquashClash.Experiments
{
    public sealed class PhysXSpikeExperiment
    {
        [UnityTest, Timeout(1800000)]
        public IEnumerator FullRun_ReportsPhysicalVerdictsAsData_AndReleasesResources()
        {
            foreach (var app in Object.FindObjectsByType<AppBootstrap>(FindObjectsSortMode.None)) Object.Destroy(app.gameObject);
            yield return null; yield return null;
            var savedGravity = UnityEngine.Physics.gravity;
            int scenes = SceneManager.sceneCount;
            string name = Environment.GetEnvironmentVariable("SQUASH_SPIKE_VARIANT") ?? "baseline";
            var variant = PhysXVariant.Baseline;
            switch (name)
            {
                case "baseline": case "no-ed": break;
                case "speculative": variant.Ccd = CollisionDetectionMode.ContinuousSpeculative; break;
                case "offset05": variant.ContactOffset = .0005f; break;
                case "solver12": variant.SolverIterations = 12; variant.SolverVelocityIterations = 4; break;
                default: throw new ArgumentException("Unknown spike variant: " + name);
            }
            var config = EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
            var factory = new PhysXBallSimulationFactory(variant);
            var run = new SpikeRun(factory, config.Physics, config.Court, new SpikeOptions {
                Variant = name + "/" + variant.Name, Build = "editor/" + (Environment.GetEnvironmentVariable("SQUASH_SPIKE_COMMIT") ?? "unknown"), ConfigHash = config.Hash,
                ProjectPhysics = "EnhancedDeterminism=" + (name != "no-ed") + "; bounceThreshold=" + UnityEngine.Physics.bounceThreshold + "; Unity=" + Application.unityVersion
            });
            PredictionBenchmarkRun benchmark = null;
            try
            {
                int mark = -1;
                while (!run.Step(8)) {
                    int next = (int)(run.Progress * 20);
                    if (next != mark) { mark = next; Debug.Log($"[SquashClash] spike {name} {run.Progress:P0} {run.Phase}"); }
                    yield return null;
                }
                benchmark = new PredictionBenchmarkRun(factory, config.Physics, config.Court);
                while (!benchmark.Step(8)) yield return null;
                benchmark.ApplyTo(run.Result);
                Assert.That(run.Result.Complete, Is.True);
                run.Result.Profile = "editor"; run.Result.OS = SystemInfo.operatingSystem;
                run.Result.ThermalStart = run.Result.ThermalEnd = "not measured / editor";
                string report = SpikeReportWriter.ToMarkdown(run.Result);
                Directory.CreateDirectory("Logs");
                File.WriteAllText("Logs/physics-spike-P1-" + name + ".md", report);
                Debug.Log("[SquashClash] spike complete variant=" + name + " AllGatingPass=" + run.Result.AllGatingPass);
            }
            finally { run.Cancel(); benchmark?.Cancel(); UnityEngine.Physics.gravity = savedGravity; }
            for (int i = 0; i < 600 && factory.PendingUnloads != 0; i++) yield return null;
            Assert.That(factory.LiveInstances, Is.Zero); Assert.That(factory.PendingUnloads, Is.Zero);
            Assert.That(SceneManager.sceneCount, Is.EqualTo(scenes));
            Assert.That(UnityEngine.Physics.gravity, Is.EqualTo(savedGravity));
        }
    }
}
