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
        [UnityTest] public IEnumerator Completion_SaveFailureStillShowsReportAndRecoversHost()=>SaveFailure(false);
        [UnityTest] public IEnumerator Cancellation_SaveFailureStillShowsReportAndRecoversHost()=>SaveFailure(true);
        [UnityTest] public IEnumerator FocusLoss_RecordsLifecycleContextAndStillCancels()=>LifecycleCancellation(false);
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
