using System;
using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using SquashClash.DevPanel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Phase=UnityEngine.InputSystem.TouchPhase;
namespace SquashClash.Runtime.Tests
{
    public sealed class SlingPreviewTests
    {
        private AppBootstrap app;private TouchInputAdapter input;private GesturePreview preview;
        private Touchscreen screen;private double start,now;private float scale;private Func<Vector2,bool> productionBlocker;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Sling test",app.Build.Label),new System.Collections.Generic.List<string>());
            app.Tuning.Set("gesture.control",1);app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();app.Host.enabled=false;
            input=app.GetComponent<TouchInputAdapter>();preview=app.GetComponent<GesturePreview>();
            productionBlocker=input.UiBlocker;input.InputAreaOverride=new Rect(0,0,5000,5000);input.UiBlocker=_=>false;start=now=Math.Ceiling(Time.realtimeSinceStartupAsDouble)+1;input.ReadRealTime=()=>now;
            scale=ScreenMetrics.MmPerPx(Screen.dpi,(float)app.Tuning.Applied.Get("input.fallbackDpi"),out _);screen=InputSystem.AddDevice<Touchscreen>();
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(screen!=null)InputSystem.RemoveDevice(screen);yield return PlayModeTestWorld.ClearApp();}
        private void Send(Phase phase,double dt,float dx=0,float dy=0,int id=1)
        {now=start+dt;InputSystem.QueueStateEvent(screen,new TouchState{touchId=id,phase=phase,position=new Vector2(2000+dx/scale,2000+dy/scale)},now);InputSystem.Update();}
        private void Lock()
        {Send(Phase.Began,0);Send(Phase.Moved,.02,dy:5);foreach(double t in new[]{.06,.10,.14,.16})Send(Phase.Stationary,t,dy:5);}
        [UnityTest] public IEnumerator NewControl_UsesSameStreamAndKeepsBallAuthorityAndPrediction()
        {
            var before=app.Host.Current;var prediction=app.Host.Prediction;Lock();Assert.That(preview.Sling,Is.Not.Null);
            Assert.That(preview.Sling.Locked,Is.True);var aim=preview.Sling.Aim;Send(Phase.Moved,.18,dy:-30);Send(Phase.Ended,.18,dy:-30);preview.FlushRecords();
            Assert.That(preview.Power.P,Is.EqualTo(1));Assert.That(preview.Power.Conservative,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.Swing));Assert.That(preview.Sling.Aim,Is.EqualTo(aim));
            Assert.That(app.Host.Current.Position,Is.EqualTo(before.Position));Assert.That(app.Host.Current.Step,Is.EqualTo(before.Step));Assert.That(app.Host.Prediction,Is.SameAs(prediction));
            Assert.That(preview.LastRecordJson,Does.Contain("\"control\":\"S1-sling\"").And.Contain("\"transitions\":[").And.Contain("\"rawSamples\":[").And.Contain("\"minimumBottomMarginMm\":").And.Contain("\"lastOutsideToLockMs\":").And.Contain("\"gestureConfigHash\":"));yield return null;
        }
        [UnityTest] public IEnumerator SelectorAndParametersAreCapturedAtBegin_AndSecondFingerCannotComplete()
        {
            Send(Phase.Began,0);app.Tuning.Set("gesture.control",0);app.Tuning.Set("power.pullFullMm",40);
            Send(Phase.Moved,.02,dy:5);foreach(double t in new[]{.06,.10,.14,.16})Send(Phase.Stationary,t,dy:5);
            Assert.That(preview.Sling,Is.Not.Null);Send(Phase.Ended,.17,dy:-30,id:2);Assert.That(preview.Active,Is.True);
            Send(Phase.Ended,.18,dy:-30);Assert.That(preview.Power.P,Is.EqualTo(1));Assert.That(preview.GestureConfig.Get("power.pullFullMm"),Is.EqualTo(35));yield return null;
        }
        [UnityTest] public IEnumerator RelaunchAndDevInvalidateSling_WithoutSyntheticRelease()
        {
            Lock();app.LaunchCurrent();Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));Assert.That(double.IsNaN(preview.Sling.ReleaseAt),Is.True);
            Send(Phase.Ended,.20,dy:-30);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));
            start+=1;Lock();UnityEngine.Object.FindFirstObjectByType<DevPanelView>().Toggle();Assert.That(preview.Active,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));yield return null;
        }
        [UnityTest] public IEnumerator FocusAndPauseInvalidateLockedSling()
        {
            Lock();app.gameObject.SendMessage("OnApplicationFocus",false,SendMessageOptions.DontRequireReceiver);Assert.That(preview.Active,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));
            app.gameObject.SendMessage("OnApplicationFocus",true,SendMessageOptions.DontRequireReceiver);app.GetComponent<ResumeOverlay>().Continue();start+=1;Lock();app.Clock.Paused=true;preview.Tick(now);Assert.That(preview.Active,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));yield return null;
        }
        [UnityTest] public IEnumerator B1_ResponseIsLinkedToFinishedTrial_DoesNotCreateTouchGesture()
        {
            Assert.That(preview.ArmSlingBlock("B1"),Is.True);Lock();Send(Phase.Ended,.18,dy:5);preview.FlushRecords();
            Assert.That(preview.SlingTrials.AwaitingFeedback,Is.True);var record=preview.LastRecordJson;
            Assert.That(record,Does.Contain("\"block\":\"B1\"").And.Contain("\"trial\":1").And.Contain("\"targetId\":1").And.Contain("\"targetErrorMeters\":"));
            input.UiBlocker=productionBlocker;Send(Phase.Began,.20,id:7);Send(Phase.Ended,.22,id:7);preview.FlushRecords();
            Assert.That(preview.Active,Is.False);Assert.That(preview.LastRecordJson,Is.EqualTo(record));
            app.Tuning.Set("gesture.control",0);yield return new WaitForSecondsRealtime(.25f);
            Assert.That(app.GetComponent<UIDocument>().rootVisualElement.Q("sling-feedback").resolvedStyle.display,Is.EqualTo(DisplayStyle.Flex),"The pending answer must stay accessible even after changing selector in DEV");
            Assert.That(preview.RespondSling(true),Is.True);Assert.That(preview.LastFeedbackJson,Does.Contain("Se fijó antes de tiempo").And.Contain("\"trial\":1"));Assert.That(preview.SlingTrials.Completed,Is.EqualTo(1));Assert.That(preview.LastRecordJson,Is.EqualTo(record));yield return null;
        }
        [UnityTest] public IEnumerator ProtocolControlsAndFeedbackExistOnlyInDevAssembly()
        {
            var root=app.GetComponent<UIDocument>().rootVisualElement;
            foreach(string block in new[]{"B0","B1","B2-soft","B2-medium","B2-strong","B3","B4","B5"})Assert.That(root.Q<Button>("sling-"+block),Is.Not.Null,block);
            Assert.That(root.Q<Button>("sling-ok"),Is.Not.Null);Assert.That(root.Q<Button>("sling-premature"),Is.Not.Null);yield return null;
        }
        [Serializable] private class TimerRecord {public string lockOrigin;public double lockTheoreticalTimeReal,lockObservedTimeReal;public TimerSample[] rawSamples;}
        [Serializable] private class TimerSample {public double timeReal;}
        [UnityTest] public IEnumerator LiveQuietContactFixesInTick_WithoutAddingTouchSamples()
        {
            Send(Phase.Began,0);Send(Phase.Moved,.02,dy:5);Send(Phase.Stationary,.06,dy:5);Send(Phase.Stationary,.10,dy:5);
            now=start+.151;preview.Tick(now);Assert.That(preview.Sling.Locked,Is.True);Assert.That(preview.Sling.LockOrigin,Is.EqualTo("timer"));
            Send(Phase.Ended,.20,dy:-30);preview.FlushRecords();var data=JsonUtility.FromJson<TimerRecord>(preview.LastRecordJson);
            Assert.That(data.lockOrigin,Is.EqualTo("timer"));Assert.That(data.rawSamples.Length,Is.EqualTo(5));
            Assert.That(data.lockTheoreticalTimeReal,Is.EqualTo(start+.14).Within(1e-6));Assert.That(data.lockObservedTimeReal,Is.EqualTo(start+.151).Within(1e-6));yield return null;
        }
        [UnityTest] public IEnumerator B0CountdownAndInterruptionStayVisible_AndGuidedCancellationIsRecorded()
        {
            Assert.That(preview.ArmSlingBlock("B0"),Is.True);Assert.That(preview.ProtocolTimingText,Does.Contain("02:00"));
            now=start+30;preview.Tick(now);Assert.That(preview.ProtocolTimingText,Does.Contain("01:30"));Assert.That(preview.SlingGuidanceText,Does.Contain("fija"));
            app.Clock.Paused=true;preview.Tick(now);Assert.That(preview.ProtocolTimingText,Does.Contain("Interrumpido"));
            app.Clock.Paused=false;Assert.That(preview.ArmSlingBlock("B0"),Is.True);start+=31;Lock();Send(Phase.Ended,.18,dy:18);
            Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.ZoneCanceled));Assert.That(preview.PracticeCancellationDone,Is.True);
            now=start+120;preview.Tick(now);Assert.That(preview.ProtocolTimingText,Does.Contain("Completado"));yield return null;
        }
        [UnityTest] public IEnumerator PauseAndDevInvalidateBeforeAnOtherwiseEligibleTimerFrame()
        {
            Send(Phase.Began,0);Send(Phase.Moved,.02,dy:5);Send(Phase.Stationary,.06,dy:5);Send(Phase.Stationary,.10,dy:5);
            now=start+.151;app.Clock.Paused=true;preview.Tick(now);Assert.That(preview.Sling.Locked,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));
            app.Clock.Paused=false;start+=1;Send(Phase.Began,0,id:3);Send(Phase.Moved,.02,dy:5,id:3);Send(Phase.Stationary,.06,dy:5,id:3);Send(Phase.Stationary,.10,dy:5,id:3);
            UnityEngine.Object.FindFirstObjectByType<DevPanelView>().Toggle();now=start+.151;preview.Tick(now);Assert.That(preview.Sling.Locked,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));yield return null;
        }
        [UnityTest] public IEnumerator LockFlashIsTemporary_AndKeepsTheMinimumStroke()
        {
            var v=app.GetComponentInChildren<AimReticleView>(true);v.gameObject.SetActive(true);v.SetAim(new Vector2(3.2f,2.2f),AimState.Frozen,app.Config.Court);
            var line=v.GetComponent<LineRenderer>();float normal=line.widthMultiplier;v.FlashLock(0,.12);v.TickFlash(.05);
            Assert.That(line.widthMultiplier,Is.GreaterThan(normal));Assert.That(line.startColor,Is.EqualTo(Color.white));v.TickFlash(.121);Assert.That(line.widthMultiplier,Is.EqualTo(normal));Assert.That(line.startColor,Is.EqualTo(Color.yellow));yield return null;
        }
    }
}
