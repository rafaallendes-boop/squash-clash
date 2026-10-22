using System.Collections;
using System.IO;
using NUnit.Framework;
using SquashClash.DevPanel;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
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
        [UnityTest] public IEnumerator ScenarioButton_LaunchesS1ThenS2WithApprovedState() {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var section=Object.FindFirstObjectByType<SpikeSection>();var host=AppBootstrap.Instance.Host;host.enabled=false;
            section.LaunchNextScenario();Assert.That(section.SelectedScenario,Is.EqualTo("S1"));Assert.That(host.Current.Position,Is.EqualTo(new Vector3(1,.9f,7.5f)));
            section.LaunchNextScenario();Assert.That(section.SelectedScenario,Is.EqualTo("S2"));Assert.That(host.Current.Position,Is.EqualTo(new Vector3(4.5f,.6f,3)));
        }
    }
}
