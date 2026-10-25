using System;
using System.Collections;
using NUnit.Framework;
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
    public sealed class GesturePreviewTests
    {
        private AppBootstrap app;private TouchInputAdapter input;private GesturePreview preview;
        private Touchscreen screen;private double now,start;
        [Serializable] private sealed class SamplingData {public bool complete;public double deliveredHz;public long samples;}
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.Host.enabled=false;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.Set("launcher.repeatSeconds",0);input=app.GetComponent<TouchInputAdapter>();preview=app.GetComponent<GesturePreview>();
            // An integral origin makes (start + 120) - start exactly 120 in the fake clock.
            // A fractional realtime origin can round that duration just below the tested boundary.
            if(input!=null){input.InputAreaOverride=new Rect(0,0,10000,10000);input.UiBlocker=_=>false;now=Math.Ceiling(Time.realtimeSinceStartupAsDouble)+1;input.ReadRealTime=()=>now;}
            start=now;screen=InputSystem.AddDevice<Touchscreen>();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {if(screen!=null)InputSystem.RemoveDevice(screen);yield return PlayModeTestWorld.ClearApp();}
        private void RequirePreview(){Assert.That(input,Is.Not.Null,"Main creates the raw input adapter");Assert.That(preview,Is.Not.Null,"Main wires the S1 preview");}
        private void Send(Phase phase,double dt,float x=200,int id=1)
        {now=start+dt;InputSystem.QueueStateEvent(screen,new TouchState{touchId=id,phase=phase,position=new Vector2(x,300)},now);InputSystem.Update();}
        [UnityTest] public IEnumerator Main_WiresAimAndPowerWithoutChangingBallAuthority()
        {
            RequirePreview();var before=app.Host.Current;var prediction=app.Host.Prediction;Send(Phase.Began,0);Send(Phase.Moved,.02,240);
            Assert.That(preview.Active,Is.True);Assert.That(preview.Model.Aim.x,Is.LessThan(before.Position.x));
            Send(Phase.Moved,.04,280);Send(Phase.Moved,.06,320);Send(Phase.Ended,.08,360);
            Assert.That(preview.Active,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.Swing));Assert.That(preview.Power.DistinctSamples,Is.GreaterThanOrEqualTo(4));
            Assert.That(app.Host.Current.Position,Is.EqualTo(before.Position));Assert.That(app.Host.Current.Step,Is.EqualTo(before.Step));Assert.That(app.Host.Prediction,Is.SameAs(prediction));yield return null;
        }
        [UnityTest] public IEnumerator ConfigAndDpi_AreCapturedAtBegin_SecondFingerIsNotPromoted()
        {
            RequirePreview();Send(Phase.Began,0);var config=preview.GestureConfig;app.Tuning.Set("aim.gainMPerMm",.3);app.Tuning.Set("input.fallbackDpi",100);
            Send(Phase.Began,.01,900,2);Send(Phase.Moved,.02,220);Assert.That(preview.GestureConfig,Is.SameAs(config));Assert.That(config.Get("aim.gainMPerMm"),Is.EqualTo(.12));
            Send(Phase.Ended,.04,250);var outcome=preview.Outcome;Send(Phase.Ended,.05,1000,2);Assert.That(preview.Active,Is.False);Assert.That(preview.Outcome,Is.EqualTo(outcome));yield return null;
        }
        [UnityTest] public IEnumerator PanelOpen_CancelsWithoutEnd_AndHeldFingerCannotResume()
        {
            RequirePreview();Send(Phase.Began,0);UnityEngine.Object.FindFirstObjectByType<DevPanelView>().Toggle();
            Assert.That(input.BlockedByUi,Is.True);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));
            UnityEngine.Object.FindFirstObjectByType<DevPanelView>().Toggle();Send(Phase.Ended,.1,800);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));Assert.That(preview.Active,Is.False);yield return null;
        }
        [UnityTest] public IEnumerator RelaunchInvalidatesGesture_OldEndDoesNotReleaseNewLaunch()
        {
            RequirePreview();Send(Phase.Began,0);app.LaunchCurrent();Send(Phase.Ended,.08,600);
            Assert.That(preview.Active,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));
            Send(Phase.Began,.2,200,3);Assert.That(preview.Active,Is.True);yield return null;
        }
        [UnityTest] public IEnumerator FocusLoss_AndPauseCancelPreviewAndSampling()
        {
            RequirePreview();preview.BeginSamplingWindow();Send(Phase.Began,0);app.gameObject.SendMessage("OnApplicationFocus",false,SendMessageOptions.DontRequireReceiver);
            Assert.That(preview.Active,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));Assert.That(preview.Sampling,Is.False);Assert.That(preview.SamplingRecordJson,Does.Contain("\"complete\":false"));
            app.gameObject.SendMessage("OnApplicationFocus",true,SendMessageOptions.DontRequireReceiver);app.GetComponent<ResumeOverlay>().Continue();
            Send(Phase.Began,.2,200,2);app.Clock.Paused=true;preview.Tick(now);
            Assert.That(preview.Active,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));yield return null;
        }
        [UnityTest] public IEnumerator Sampling_UsesMeasuredIntervalsAnd120RealSeconds_NotFrameCount()
        {
            RequirePreview();preview.BeginSamplingWindow();Send(Phase.Began,0);Send(Phase.Moved,.01,210);Send(Phase.Moved,.02,220);Send(Phase.Ended,.03,230);
            preview.Tick(start+119.99);Assert.That(preview.Sampling,Is.True);preview.Tick(start+120);
            Assert.That(preview.Sampling,Is.False);var data=JsonUtility.FromJson<SamplingData>(preview.SamplingRecordJson);
            Assert.That(data.complete,Is.True);Assert.That(data.deliveredHz,Is.EqualTo(100).Within(1e-6));Assert.That(data.samples,Is.EqualTo(4));yield return null;
        }
        [UnityTest] public IEnumerator EmptySampling_ReportsUnknownRateAsNull()
        {
            RequirePreview();preview.BeginSamplingWindow();preview.Tick(start+120);
            Assert.That(preview.SamplingRecordJson,Does.Contain("\"deliveredHz\":null").And.Contain("\"intervalP95Ms\":null"));yield return null;
        }
        [UnityTest] public IEnumerator RulerAndFlickProtocol_KeepAllOutcomesAndRawTimestamps()
        {
            RequirePreview();preview.ArmRuler();Send(Phase.Began,0);Send(Phase.Moved,.2,1100);Send(Phase.Ended,.3,1100);
            preview.FlushRecords();
            Assert.That(preview.LastRecordJson,Does.Contain("\"protocol\":\"ruler-50mm\"").And.Contain("\"physicalDistanceDeclaredMm\":50").And.Contain("\"rawSamples\":[").And.Contain("\"dpiSource\":").And.Contain("\"physicallyVerified\":false"));
            preview.ArmFlicks(false);for(int i=0;i<10;i++){Send(Phase.Began,1+i,.1f,10+i);Send(Phase.Ended,1.01+i,.1f,10+i);}
            preview.FlushRecords();
            Assert.That(preview.TrialCount,Is.EqualTo(10));Assert.That(preview.LastRecordJson,Does.Contain("\"protocol\":\"soft\"").And.Contain("\"trial\":10").And.Contain("\"outcome\":\"Tap\""));yield return null;
        }
        [UnityTest] public IEnumerator DiagnosticControlsExistInDev_AndDoNotBlockAimLabels()
        {
            RequirePreview();var root=app.GetComponent<UIDocument>().rootVisualElement;
            foreach(string name in new[]{"s1-sampling","s1-ruler","s1-soft","s1-strong"})Assert.That(root.Q<Button>(name),Is.Not.Null,name);
            Assert.That(root.Q<Label>("shot-preview-status").pickingMode,Is.EqualTo(PickingMode.Ignore));yield return null;
        }
        [UnityTest] public IEnumerator LongGesture_KeepsOnlyPowerWindowAndBracketInHotPath()
        {
            RequirePreview();Send(Phase.Began,0);
            for(int i=1;i<=200;i++)Send(Phase.Moved,i*.01,200+i);
            Assert.That(preview.WindowSampleCount,Is.LessThanOrEqualTo(10),"80 ms window plus the real sample bracketing its start");
            Send(Phase.Ended,2.01,401);preview.FlushRecords();Assert.That(preview.LastRecordJson,Does.Contain("\"rawSamples\":["));yield return null;
        }
        [UnityTest] public IEnumerator NonMonotonicGesture_RemainsConservativeAfterBadSampleLeavesWindow()
        {
            RequirePreview();Send(Phase.Began,0);Send(Phase.Moved,.02,230);Send(Phase.Moved,.01,260);
            for(int i=3;i<=20;i++)Send(Phase.Moved,i*.01,260+i*30);
            Assert.That(preview.Power.Conservative,Is.True);Assert.That(preview.Power.Reason,Is.EqualTo("out-of-order"));yield return null;
        }
    }
}
