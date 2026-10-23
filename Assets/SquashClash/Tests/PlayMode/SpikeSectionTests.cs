using System;
using System.Collections;
using System.Reflection;
using SquashClash.Core.Spike;
using System.IO;
using NUnit.Framework;
using SquashClash.DevPanel;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests
{
    public sealed class SpikeSectionTests
    {
        private int sleep;
        [UnitySetUp] public IEnumerator Setup() { sleep=Screen.sleepTimeout;yield return PlayModeTestWorld.ClearApp(); }
        [UnityTearDown] public IEnumerator Teardown() {
            var section=Object.FindFirstObjectByType<SpikeSection>();
            if(section!=null){section.CancelRun("Test cleanup");if(section.LastReportPath!=null)File.Delete(section.LastReportPath);}
            yield return PlayModeTestWorld.ClearApp();Screen.sleepTimeout=sleep;
        }
        [UnityTest] public IEnumerator Background_CancelsSavesIncompleteAndRestoresSleepTimeout() {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();Assert.That(section,Is.Not.Null);
            section.Begin();Assert.That(section.IsRunning,Is.True);Assert.That(AppBootstrap.Instance.Host.Clock.Paused,Is.True);
            Assert.That(Screen.sleepTimeout,Is.EqualTo(SleepTimeout.NeverSleep));
            section.SendMessage("OnApplicationPause",true);
            Assert.That(section.IsRunning,Is.False);Assert.That(Screen.sleepTimeout,Is.EqualTo(sleep));
            Assert.That(section.LastReport,Does.Contain("INCOMPLETO"));Assert.That(section.LastReport,Does.Contain("Segundo plano"));Assert.That(section.LastReport,Does.Contain("Dispositivo: editor"));
            Assert.That(File.ReadAllText(section.LastReportPath),Is.EqualTo(section.LastReport));Assert.That(GUIUtility.systemCopyBuffer,Is.EqualTo(section.LastReport));
        }
        [UnityTest] public IEnumerator Campaign_CancelBeforeRunnerStillIdentifiesBackendAndSettings()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();section.Begin();section.CancelRun("early cancellation");
            Assert.That(section.LastReport,Does.Contain("Backend: P2-Analytic; variante: analytic/r2-20261023"));Assert.That(section.LastReport,Does.Contain("fastMotionThreshold=0.01"));Assert.That(section.LastReport,Does.Contain("INCOMPLETO"));
        }
        [UnityTest] public IEnumerator Campaign_UsesP2WithoutSelectingRuntimeBackend()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var app=AppBootstrap.Instance;Assert.That(app.Config.Backend,Is.EqualTo("P1-PhysX"));
            var section=Object.FindFirstObjectByType<SpikeSection>();section.Begin();
            for(int i=0;i<120&&Field(section,"run")==null;i++)yield return null;
            Assert.That(Field(section,"run"),Is.Not.Null);section.CancelRun("test output");
            Assert.That(section.LastReport,Does.Contain("Backend: P2-Analytic; variante: analytic/r2-20261023"));
            Assert.That(app.Config.Backend,Is.EqualTo("P1-PhysX"));
            Assert.That(app.GetComponent<UIDocument>().rootVisualElement.Q<Button>("physics-spike").text,Does.Contain("P2"));
        }
        [UnityTest] public IEnumerator CampaignHeader_RecordsEffectiveFastMotionThresholdAndItsP1Scope()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();section.Begin();
            for(int i=0;i<120&&Field(section,"run")==null;i++)yield return null;
            Assert.That(Field(section,"run"),Is.Not.Null);section.CancelRun("test output");
            Assert.That(section.LastReport,Does.Contain("fastMotionThreshold=0.01"));Assert.That(section.LastReport,Does.Contain("fastMotionThreshold relevante solo para P1"));
        }
        [UnityTest] public IEnumerator Completion_SaveFailureStillShowsReportAndRecoversHost()=>SaveFailure(false);
        [UnityTest] public IEnumerator Cancellation_SaveFailureStillShowsReportAndRecoversHost()=>SaveFailure(true);
        [UnityTest] public IEnumerator FocusLoss_RobustnessWaitsAndResumes_RecordsTwoInterruptions()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();var app=AppBootstrap.Instance;
            section.Begin();
            for(int i=0;i<120&&Field(section,"run")==null;i++)yield return null;
            var run=(SpikeRun)Field(section,"run");Assert.That(run,Is.Not.Null);float before=run.Progress;
            section.SendMessage("OnApplicationFocus",false);app.SendMessage("OnApplicationFocus",false);
            Assert.That(section.IsRunning,Is.True);Assert.That(section.LastReport,Is.Null);
            for(int i=0;i<3;i++)yield return null;
            Assert.That(run.Progress,Is.EqualTo(before),"no jobs while focus is absent");
            section.SendMessage("OnApplicationFocus",false); // duplicate loss is one interval
            section.SendMessage("OnApplicationFocus",true);
            Assert.That(app.GetComponent<SquashClash.Runtime.App.ResumeOverlay>().IsVisible,Is.False,"experiment resumes without an extra tap");
            yield return null;Assert.That(run.Progress,Is.GreaterThan(before));
            section.SendMessage("OnApplicationFocus",false);yield return null;section.SendMessage("OnApplicationFocus",true);
            section.CancelRun("test output");
            Assert.That(section.LastReport,Does.Contain("Interrupciones de foco: 2"));Assert.That(section.LastReport,Does.Contain("focus(false)"));Assert.That(section.LastReport,Does.Contain("focus(true)"));
            foreach(var field in new[]{"utc=","durationSeconds=","phase=","progress=","uikitState=","outcome=resumed"})Assert.That(section.LastReport,Does.Contain(field));
            Assert.That(section.LastReport,Does.Contain("INCOMPLETO"),"logged interruptions do not approve a cancelled run");
        }
        [UnityTest] public IEnumerator FocusLoss_BenchmarkStillCancelsAndCountsInterruption()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();section.Begin();section.enabled=false;
            var cfg=AppBootstrap.Instance.Config;
            var benchmark=new PredictionBenchmarkRun(new SquashClash.Runtime.Physics.PhysXBallSimulationFactory(),cfg.Physics,cfg.Court);
            SetField(section,"benchmark",benchmark);SetField(section,"waiting",false);
            section.SendMessage("OnApplicationFocus",false);
            Assert.That(section.IsRunning,Is.False);Assert.That(section.LastReport,Does.Contain("Pérdida de foco"));
            Assert.That(section.LastReport,Does.Contain("phase=Benchmark"));Assert.That(section.LastReport,Does.Contain("Interrupciones de foco: 1"));
            Assert.That(section.LastReport,Does.Contain("INCOMPLETO"));
        }
        [UnityTest] public IEnumerator FocusLoss_TimeoutAt60Seconds_CancelsEvenWhenFocusReturns()=>FocusTimeout(true);
        [UnityTest] public IEnumerator FocusLoss_TimeoutAt60Seconds_CancelsWithoutRecovery()=>FocusTimeout(false);
        private IEnumerator FocusTimeout(bool recover)
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();section.Begin();section.enabled=false;
            section.SendMessage("OnApplicationFocus",false);Assert.That(section.IsRunning,Is.True);
            // Move the monotonic loss timestamp to avoid a 60-second test sleep.
            SetField(section,"focusLostAt",Time.realtimeSinceStartupAsDouble-60.01);
            if(recover)section.SendMessage("OnApplicationFocus",true);else section.SendMessage("Update");
            Assert.That(section.IsRunning,Is.False);Assert.That(section.LastReport,Does.Contain("60 s"));
            Assert.That(section.LastReport,Does.Contain("INCOMPLETO"));Assert.That(section.LastReport,Does.Contain("durationSeconds="));
        }
        [UnityTest] public IEnumerator FocusLoss_RecoveryBefore60Seconds_ThenBackgroundStillCancels()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();section.Begin();section.enabled=false;
            section.SendMessage("OnApplicationFocus",false);Assert.That(section.IsRunning,Is.True);
            SetField(section,"focusLostAt",Time.realtimeSinceStartupAsDouble-59);
            section.SendMessage("OnApplicationFocus",true);Assert.That(section.IsRunning,Is.True);
            section.SendMessage("OnApplicationPause",true);Assert.That(section.IsRunning,Is.False);
            Assert.That(section.LastReport,Does.Contain("Segundo plano"));Assert.That(section.LastReport,Does.Contain("Interrupciones de foco: 1"));
        }
        [UnityTest] public IEnumerator CompletedReport_WithFocusInterruptionKeepsCompleteStatus()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();section.Begin();section.enabled=false;
            section.SendMessage("OnApplicationFocus",false);Assert.That(section.IsRunning,Is.True);section.SendMessage("OnApplicationFocus",true);
            SetField(section,"result",new SpikeResult{Complete=true,Device="editor",Criteria=SpikeCriteria.CreateResults(false)});
            typeof(SpikeSection).GetMethod("Finish",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(section,new object[]{"Prueba completa"});
            Assert.That(section.LastReport,Does.Contain("**COMPLETO**"));Assert.That(section.LastReport,Does.Contain("Interrupciones de foco: 1"));
            Assert.That(section.LastReport,Does.Contain("NO APROBADOS / NO EVALUADOS"),"completion alone does not approve physics");
        }
        [UnityTest] public IEnumerator BenchmarkStart_RecordsThermalStateBeforeAnyMeasurements()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();section.Begin();section.enabled=false;
            var cfg=AppBootstrap.Instance.Config;var factory=new SquashClash.Runtime.Physics.PhysXBallSimulationFactory();
            var run=new SpikeRun(factory,cfg.Physics,cfg.Court,new SpikeOptions{RandomCount=0,Device="editor"});
            // Inject only the completed-robustness boundary; no 10k physical campaign in a UI regression.
            typeof(SpikeRun).GetField("finished",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(run,true);run.Result.Complete=true;
            SetField(section,"run",run);SetField(section,"result",run.Result);SetField(section,"factory",factory);SetField(section,"waiting",false);
            section.SendMessage("Update");Assert.That(((PredictionBenchmarkRun)Field(section,"benchmark")).Samples,Is.Zero);
            section.CancelRun("thermal metadata check");
            Assert.That(section.LastReport,Does.Contain("Térmico al iniciar benchmark C17 (informativo): editor / no disponible"));
            Assert.That(section.LastReport,Does.Contain("INCOMPLETO"));
        }
        private static object Field(SpikeSection section,string name)=>typeof(SpikeSection).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(section);
        private static void SetField(SpikeSection section,string name,object value)=>typeof(SpikeSection).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(section,value);
        [UnityTest] public IEnumerator Background_RecordsLifecycleContextAndStillCancels()=>LifecycleCancellation(true);
        private IEnumerator LifecycleCancellation(bool paused) {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();section.Begin();
            section.SendMessage(paused?"OnApplicationPause":"OnApplicationFocus",paused);
            Assert.That(section.IsRunning,Is.False);Assert.That(section.LastReport,Does.Contain("**INCOMPLETO**"));
            Assert.That(section.LastReport,Does.Contain(paused?"Segundo plano":"Pérdida de foco"));
            Assert.That(section.LastReport,Does.Contain(paused?"Lifecycle event=pause(true)":"Lifecycle event=focus(false)"));
            foreach(var field in new[]{"utc=","elapsedSeconds=","frame=","unityFocused=","uikitState=editor-unavailable","uikitIdleTimerDisabled=editor-unavailable","sleepTimeout=-1","thermal=","phase=Preparando escenas","progress=0.000000","benchmarkSamples=0"})
                Assert.That(section.LastReport,Does.Contain(field));
            Assert.That(File.ReadAllText(section.LastReportPath),Is.EqualTo(section.LastReport));
            Assert.That(Screen.sleepTimeout,Is.EqualTo(sleep));
        }
        private IEnumerator SaveFailure(bool cancelled) {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var app=AppBootstrap.Instance;var host=app.Host;host.enabled=false;int generation=host.SimulationGeneration;
            var section=Object.FindFirstObjectByType<SpikeSection>();
            var root=app.GetComponent<UnityEngine.UIElements.UIDocument>().rootVisualElement;
            string blocked=Path.Combine(Path.GetTempPath(),"squash-report-"+Guid.NewGuid().ToString("N"));File.WriteAllText(blocked,"occupied directory path");
            var gravity=UnityEngine.Physics.gravity;
            try {
                section.Initialize(app,root,blocked);section.Begin();
                if(cancelled)Assert.DoesNotThrow(()=>section.CancelRun("test cancellation"));
                else {
                    // Inject a completed result at the output boundary, avoiding a 10k physical experiment in regression.
                    typeof(SpikeSection).GetField("result",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(section,new SpikeResult{Complete=true,Device="editor",Build=app.Build.Label,ConfigHash=app.Config.Hash,Criteria=SpikeCriteria.CreateResults(false)});
                    Assert.DoesNotThrow(()=>typeof(SpikeSection).GetMethod("Finish",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(section,new object[]{"Prueba completa"}));
                }
                Assert.That(section.IsRunning,Is.False);Assert.That(section.LastReport,Does.Contain(cancelled?"**INCOMPLETO**":"**COMPLETO**"));
                Assert.That(section.LastReportPath,Is.Null);Assert.That(Screen.sleepTimeout,Is.EqualTo(sleep));Assert.That(UnityEngine.Physics.gravity,Is.EqualTo(gravity));
                var label=root.Q<UnityEngine.UIElements.Label>("spike-report");Assert.That(label.text,Is.EqualTo(section.LastReport));
                Assert.That(root.Q<UnityEngine.UIElements.Label>("spike-status").text,Does.Contain("No se pudo guardar"));
                Assert.That(root.Q<UnityEngine.UIElements.Button>("spike-close").style.display.value,Is.EqualTo(UnityEngine.UIElements.DisplayStyle.Flex));
                for(int i=0;i<120&&host.SimulationGeneration==generation;i++)yield return null;
                Assert.That(host.SimulationGeneration,Is.EqualTo(generation+1));Assert.That(host.Clock.Paused,Is.True);
            } finally {File.Delete(blocked);}
        }
        [UnityTest] public IEnumerator ScenarioButton_LaunchesS1ThenS2WithApprovedState() {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();var host=AppBootstrap.Instance.Host;host.enabled=false;
            section.LaunchNextScenario();Assert.That(section.SelectedScenario,Is.EqualTo("S1"));Assert.That(host.Current.Position,Is.EqualTo(new Vector3(1,.9f,7.5f)));
            section.LaunchNextScenario();Assert.That(section.SelectedScenario,Is.EqualTo("S2"));Assert.That(host.Current.Position,Is.EqualTo(new Vector3(4.5f,.6f,3)));
        }
    }
}
