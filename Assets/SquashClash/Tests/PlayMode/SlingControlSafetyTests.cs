using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.DevPanel;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    public sealed class SlingControlSafetyTests
    {
        private AppBootstrap app;private GesturePreview preview;private DevBar bar;private VisualElement root;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Control test",app.Build.Label),new System.Collections.Generic.List<string>());
            app.Tuning.Set("practice.enabled",0);app.Tuning.Set("gesture.control",1);app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();app.Host.enabled=false;
            preview=app.GetComponent<GesturePreview>();bar=UnityEngine.Object.FindFirstObjectByType<DevBar>();root=app.GetComponent<UIDocument>().rootVisualElement;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return PlayModeTestWorld.ClearApp();}
        [Serializable] private class Envelope {public string kind,presetName,effectiveHash,payload;public int pending;}
        [Serializable] private class ActionRecord {public string control,stage,reason;public bool accepted,paused;public double timeReal;}
        private Envelope[] Audits()=>File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Envelope>).Where(x=>x.kind=="dev-control").ToArray();
        private ActionRecord Last(string control)=>Audits().Select(e=>JsonUtility.FromJson<ActionRecord>(e.payload)).LastOrDefault(a=>a.control==control&&a.stage=="result");
        private void Submit(string name){var b=root.Q<Button>(name);b.Focus();using(var e=NavigationSubmitEvent.GetPooled())b.SendEvent(e);}
        [UnityTest] public IEnumerator PreparedSlingDisablesAllExperimentButtonsBeforeB0()
        {
            yield return null;
            foreach(var name in new[]{"scenario","physics-spike","performance-session"})Assert.That(root.Q<Button>(name).enabledInHierarchy,Is.False,name);
            Assert.That(preview.SlingTrials.Running,Is.False);Assert.That(bar.GetComponent<SpikeSection>().IsRunning,Is.False);
        }
        [UnityTest] public IEnumerator ExperimentCallbacksRejectPreparedSlingAndRecordWhy_WithoutChangingBallOrPhase()
        {
            var cfg=app.Host.ActiveConfig;var before=app.Host.Current;var phase=app.Metrics.Phase;
            foreach(var name in new[]{"scenario","physics-spike","performance-session"})
            {
                Assert.That(bar.ActivateExperiment(name),Is.False);var record=Last(name);Assert.That(record,Is.Not.Null,name);
                Assert.That(record.accepted,Is.False);Assert.That(record.reason,Is.EqualTo("sling-protocol-protected"));
            }
            Assert.That(app.Host.ActiveConfig,Is.SameAs(cfg));Assert.That(app.Host.Current,Is.EqualTo(before));Assert.That(app.Metrics.Phase,Is.EqualTo(phase));yield return null;
        }
        [UnityTest] public IEnumerator ProtectionSurvivesChangingSelectorDuringAnActiveBlock()
        {
            Assert.That(preview.ArmSlingBlock("B1"),Is.True);app.Tuning.Set("gesture.control",0);yield return null;
            Assert.That(root.Q<Button>("physics-spike").enabledInHierarchy,Is.False);Assert.That(bar.ActivateExperiment("scenario"),Is.False);
        }
        [UnityTest] public IEnumerator OutsideProtocolScenarioStillRuns_AndActivationAndResultAreAudited()
        {
            app.Tuning.Set("gesture.control",0);yield return null;
            foreach(var name in new[]{"scenario","physics-spike","performance-session"})Assert.That(root.Q<Button>(name).enabledInHierarchy,Is.True,name);
            Assert.That(bar.ActivateExperiment("scenario"),Is.True);Assert.That(bar.GetComponent<SpikeSection>().SelectedScenario,Is.Not.Empty);
            Assert.That(Last("scenario")?.accepted,Is.True);Assert.That(Audits().Any(e=>JsonUtility.FromJson<ActionRecord>(e.payload).stage=="activation"),Is.True);yield return null;
        }
        [UnityTest] public IEnumerator PausedBlockAttemptShowsPersistentVisibleRemedyAndLogsRejection()
        {
            app.Clock.Paused=true;Assert.That(preview.ArmSlingBlock("B0"),Is.False);yield return new WaitForSecondsRealtime(.15f);
            Assert.That(root.Q<Label>("sling-protocol-guide").text,Does.Contain("Continuar"));
            Assert.That(root.Q("shot-overlay").resolvedStyle.display,Is.EqualTo(DisplayStyle.Flex));
            Assert.That(preview.SlingTrials.Running,Is.False);Assert.That(preview.ProtocolTimingText,Is.Empty);
            var record=Last("sling-B0");Assert.That(record,Is.Not.Null);Assert.That(record.accepted,Is.False);Assert.That(record.paused,Is.True);Assert.That(record.reason,Is.EqualTo("paused"));
        }
        [UnityTest] public IEnumerator DevAndFocusRejectionsHaveDistinctReasons_WithoutStartingTimer()
        {
            var panel=UnityEngine.Object.FindFirstObjectByType<DevPanelView>();panel.Toggle();Assert.That(preview.ArmSlingBlock("B2-medium"),Is.False);
            Assert.That(Last("sling-B2-medium")?.reason,Is.EqualTo("dev-open"));panel.Toggle();
            app.gameObject.SendMessage("OnApplicationFocus",false,SendMessageOptions.DontRequireReceiver);Assert.That(preview.ArmSlingBlock("B3"),Is.False);
            Assert.That(Last("sling-B3")?.reason,Is.EqualTo("focus"));Assert.That(preview.ProtocolTimingText,Is.Empty);yield return null;
        }
        [UnityTest] public IEnumerator ConfigPreconditionsNameTheirRemedy_AndDoNotArmUnknownBlocks()
        {
            app.Tuning.Set("launcher.repeatSeconds",3);app.LaunchCurrent();Assert.That(preview.ArmSlingBlock("B4"),Is.False);Assert.That(Last("sling-B4")?.reason,Is.EqualTo("repeat-not-zero"));
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();app.Tuning.Set("physics.floor.restitution",.6);Assert.That(preview.ArmSlingBlock("B5"),Is.False);Assert.That(Last("sling-B5")?.reason,Is.EqualTo("pending-changes"));
            app.LaunchCurrent();Assert.That(preview.ArmSlingBlock("unknown"),Is.False);Assert.That(Last("sling-unknown")?.reason,Is.EqualTo("unknown-block"));yield return null;
        }
        [UnityTest] public IEnumerator AcceptedB0ShowsCountdownAndAuditsExactPresetHashTimeAndPause()
        {
            string hash=app.Tuning.Applied.Hash;Assert.That(preview.ArmSlingBlock("B0"),Is.True);yield return new WaitForSecondsRealtime(.15f);
            var e=Audits().LastOrDefault();Assert.That(e,Is.Not.Null);Assert.That(e.presetName,Is.EqualTo(app.ActivePresetName));Assert.That(e.effectiveHash,Is.EqualTo(hash));Assert.That(e.pending,Is.Zero);
            var record=Last("sling-B0");Assert.That(record.accepted,Is.True);Assert.That(record.paused,Is.False);Assert.That(record.timeReal,Is.GreaterThan(0));
            Assert.That(root.Q<Label>("sling-protocol-timer").text,Does.Contain("B0").And.Contain("01:").Or.Contain("02:00"));
        }
        [UnityTest] public IEnumerator ReadinessAndOrdinaryDevControlsExposeHashAndWriteAuditWithoutLoggingPastedData()
        {
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(root.Q<Label>("shot-compact").text,Does.Contain(app.Tuning.Applied.Hash).And.Contain("Listo"));
            Submit("dev-toggle");Assert.That(Last("dev-toggle")?.accepted,Is.True);
            var section=new ParamSection(app,()=>{});Assert.That(section.TrySet("camera.height",double.NaN,out _),Is.False);
            Assert.That(Last("camera.height")?.accepted,Is.False);Assert.That(Last("camera.height")?.reason,Is.EqualTo("invalid-value"));
            var presets=new PresetsSection(app,()=>{});Assert.That(presets.Import("secret-invalid-text"),Is.False);Assert.That(Last("preset-import")?.accepted,Is.False);
            Assert.That(string.Join("\n",Audits().Select(e=>e.payload)),Does.Not.Contain("secret-invalid-text"));yield return null;
        }
    }
}
