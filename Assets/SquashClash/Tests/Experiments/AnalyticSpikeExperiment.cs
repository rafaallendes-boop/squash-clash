using System;
using System.Collections;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
using SquashClash.Core.Spike;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace SquashClash.Experiments
{
    public sealed class AnalyticSpikeExperiment
    {
        [UnityTest,Timeout(1800000)]
        public IEnumerator FullRun_UsesSameScenariosAndCriteria_ReportsVerdictsAsData()
        {
            if(Application.isPlaying)
            {
                foreach(var app in Object.FindObjectsByType<AppBootstrap>(FindObjectsSortMode.None))Object.Destroy(app.gameObject);
                yield return null;yield return null;
            }
            int scenes=SceneManager.sceneCount;var gravity=UnityEngine.Physics.gravity;float threshold=UnityEngine.Physics.bounceThreshold;
            var config=EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
            var factory=new AnalyticBallSimulationFactory(threshold);string mode=Application.isPlaying?"playmode":"editmode";
            string started=DateTime.UtcNow.ToString("O"),fastMotionThreshold="no disponible";
#if UNITY_EDITOR
            fastMotionThreshold=new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset")[0]).FindProperty("m_FastMotionThreshold").floatValue.ToString("R",CultureInfo.InvariantCulture);
#endif
            var run=new SpikeRun(factory,config.Physics,config.Court,new SpikeOptions {
                Device="editor",Build="editor/"+(Environment.GetEnvironmentVariable("SQUASH_SPIKE_COMMIT")??"unknown"),ConfigHash=config.Hash,
                Variant="analytic/r2-20261023",ProjectPhysics="fastMotionThreshold="+fastMotionThreshold+" (relevante solo para P1); bounceThreshold="+threshold.ToString("R",CultureInfo.InvariantCulture)+"; Unity="+Application.unityVersion });
            PredictionBenchmarkRun benchmark=null;
            try
            {
                int previous=-1;
                while(!run.Step(8)) {int mark=(int)(run.Progress*20);if(mark!=previous){previous=mark;Debug.Log("[SquashClash] P2 "+mode+" "+run.Progress.ToString("P0")+" "+run.Phase);}yield return null;}
                run.Result.ThermalBenchmarkStart="no medido / editor";
                benchmark=new PredictionBenchmarkRun(factory,config.Physics,config.Court);
                while(!benchmark.Step(8))yield return null;
                benchmark.ApplyTo(run.Result);run.Result.Profile="editor-"+mode;run.Result.OS=SystemInfo.operatingSystem;
                run.Result.StartedAtUtc=started;run.Result.EndedAtUtc=DateTime.UtcNow.ToString("O");run.Result.ThermalStart=run.Result.ThermalEnd="no medido / editor";
                Assert.That(run.Result.Complete,Is.True);Assert.That(run.Result.PredictionSamplesMs.Count,Is.EqualTo(200));
                Directory.CreateDirectory("Logs");File.WriteAllText("Logs/physics-spike-P2-"+mode+".md",SpikeReportWriter.ToMarkdown(run.Result));
                Debug.Log("[SquashClash] P2 complete mode="+mode+" AllGatingPass="+run.Result.AllGatingPass);
            }
            finally {run.Cancel();benchmark?.Cancel();}
            Assert.That(factory.LiveInstances,Is.Zero);Assert.That(SceneManager.sceneCount,Is.EqualTo(scenes));
            Assert.That(UnityEngine.Physics.gravity,Is.EqualTo(gravity));Assert.That(UnityEngine.Physics.bounceThreshold,Is.EqualTo(threshold));
        }
    }
}
