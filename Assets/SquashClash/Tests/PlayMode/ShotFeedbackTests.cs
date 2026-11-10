using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Feedback;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Phase=UnityEngine.InputSystem.TouchPhase;
namespace SquashClash.Runtime.Tests
{
    public sealed class RecordingHaptics:IHapticsBackend
    {
        public int Prepares,Firm,Discreet;
        public void Prepare(){Prepares++;}
        public void Impact(HapticStyle style){if(style==HapticStyle.Firm)Firm++;else Discreet++;}
    }
    public sealed class ShotFeedbackTests
    {
        private AppBootstrap app;private ShotController controller;private ShotFeedback feedback;private PracticeMode practice;private RecordingHaptics haptics;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"S3 B fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);
            bool wired=app.GetComponent<ShotController>()!=null;
            controller=app.GetComponent<ShotController>()??app.gameObject.AddComponent<ShotController>();if(!wired)controller.Initialize(app);
            practice=app.GetComponent<PracticeMode>()??app.gameObject.AddComponent<PracticeMode>();if(!wired)practice.Initialize(app,controller);
            feedback=controller.GetComponent<ShotFeedback>();haptics=new RecordingHaptics();Haptics.Backend=haptics;
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=practice.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){Haptics.ResetBackend();yield return PlayModeTestWorld.ClearApp();}
        private void At(double target)
        {
            for(int i=0;i<1500&&app.Host.ObservedRealTime+StepReal<target-1e-10;i++)app.Host.Advance(StepReal);
            double left=target-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);
        }
        private double Star(){Assert.That(controller.Plan.Exists,Is.True);Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var t),Is.True);return t;}
        private ClosedGesture Order(double time,GestureOutcome outcome=GestureOutcome.Swing,Vector2? aim=null)
            =>new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,time,true,outcome,aim??new Vector2(3.2f,2.2f),.5f,app.Host.ActiveConfig.Source,"fixture",time);
        private ShotRecord Submit(double time,Vector2? aim=null){At(time);controller.Enqueue(Order(time,aim:aim));controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);return controller.LastRecord;}
        private void AssertCounts(int perfect,int valid,int swish,int firm,int discreet)
        {Assert.That(feedback.PerfectCount,Is.EqualTo(perfect),"perfect");Assert.That(feedback.ValidCount,Is.EqualTo(valid),"valid");Assert.That(feedback.SwishCount,Is.EqualTo(swish),"swish");Assert.That(haptics.Firm,Is.EqualTo(firm),"firm");Assert.That(haptics.Discreet,Is.EqualTo(discreet),"discreet");}
        [Test] public void Perfect_StrongSoundFirmHaptic()
        {var r=Submit(Star());Assert.That(r.Result,Is.EqualTo("Perfect"),r.Reason);AssertCounts(1,0,0,1,0);Assert.That(feedback.LastClip,Is.EqualTo("impact-perfect"));}
        [Test] public void Valid_SofterSoundDiscreteHaptic()
        {
            var r=Submit(Star()-.07);Assert.That(r.Result,Is.EqualTo("Valid"),r.Reason);
            while(app.Host.Current.Step<r.ContactStep+2)app.Host.Advance(StepReal);
            Assert.That(controller.LastRecord.Applied,Is.True,controller.LastRecord.Reason);AssertCounts(0,1,0,0,1);Assert.That(feedback.LastClip,Is.EqualTo("impact-valid"));
        }
        [Test] public void Failed_SwishNoHaptic()
        {var r=Submit(Star()-.15);Assert.That(r.Result,Is.EqualTo("Failed"));AssertCounts(0,0,1,0,0);Assert.That(feedback.LastClip,Is.EqualTo("swish-provisional"));}
        [Test] public void HapticsOff_FailedAttempt_NoImpactAudioNoHaptic()
        {
            app.Tuning.Set("feedback.haptics",0);app.LaunchCurrent();
            var r=Submit(Star()-.15);Assert.That(r.Result,Is.EqualTo("Failed"));Assert.That(feedback.ContactCount,Is.Zero,"impact audio");
            Assert.That(haptics.Firm,Is.Zero);Assert.That(haptics.Discreet,Is.Zero);
        }
        [Test] public void NoDelivery_NoAudioNoHaptic()
        {
            controller.Enqueue(Order(app.Host.LaunchRealTime+.3,GestureOutcome.CanceledReturnToDeadZone));controller.ProcessPending();
            for(int i=0;i<1200&&app.Host.IsRunning;i++)app.Host.Advance(StepReal);practice.Tick(.001);Assert.That(controller.LastRecord.Result,Is.EqualTo("NoOrder"));
            AssertCounts(0,0,0,0,0);Assert.That(feedback.LastClip,Is.Null);
        }
        [Test] public void SystemFault_NoAudioNoHaptic()
        {var r=Submit(Star(),new Vector2(8,2));Assert.That(r.Result,Is.EqualTo("SystemFault"));AssertCounts(0,0,0,0,0);}
        [Test] public void NotEvaluable_NoAudioNoHaptic()
        {double end=Star();var order=Order(end);At(end+.3);controller.Enqueue(order);controller.ProcessPending();Assert.That(controller.LastRecord.Result,Is.EqualTo("NotEvaluable"));AssertCounts(0,0,0,0,0);}
        private void AssertInformativeOnly(string result,string text)
        {
            Assert.That(controller.LastRecord.Result,Is.EqualTo(result));Assert.That(controller.GetComponent<ShotNoteView>().Text,Is.EqualTo(text));
            Assert.That(controller.PresentationCount,Is.Zero,"presentations");Assert.That(controller.LastPresentation,Is.Null);
            AssertCounts(0,0,0,0,0);Assert.That(feedback.ContactCount,Is.Zero,"impact audio");Assert.That(feedback.LastClip,Is.Null);
            Assert.That(controller.GetComponent<PerfectFlashView>().ShowCount,Is.Zero,"flash");Assert.That(controller.GetComponent<SwingView>().Phase,Is.Not.EqualTo(SwingPhase.Impact));
        }
        [Test] public void NoOrder_InformativeNoteOnly()
        {
            for(int i=0;i<1200&&app.Host.IsRunning;i++)app.Host.Advance(StepReal);practice.Tick(.001);
            AssertInformativeOnly("NoOrder","Sin golpe");
        }
        [Test] public void SystemFault_InformativeNoteOnly()
        {Submit(Star(),new Vector2(8,2));AssertInformativeOnly("SystemFault","Fallo del sistema");}
        [Test] public void NotEvaluable_InformativeNoteOnly()
        {double end=Star();var order=Order(end);At(end+.3);controller.Enqueue(order);controller.ProcessPending();AssertInformativeOnly("NotEvaluable","No evaluable");}
        [Test] public void HapticsToggle_RuntimeOff_KeepsVisualAlternative()
        {
            app.Tuning.Set("feedback.haptics",0);app.LaunchCurrent();
            var r=Submit(Star());Assert.That(r.Result,Is.EqualTo("Perfect"),r.Reason);AssertCounts(1,0,0,0,0);
            var note=controller.GetComponent<ShotNoteView>();Assert.That(note,Is.Not.Null);Assert.That(note.Text,Is.EqualTo("Perfecto"));
        }
        [Test] public void ClipsArePreloaded_NoLoadOnContact()
        {Assert.That(feedback.ClipsLoaded,Is.True);Submit(Star());Assert.That(feedback.ClipsLoaded,Is.True);}
        [Test] public void NoCameraShakeOrSlowMotionOnFeedback()
        {
            var cam=UnityEngine.Camera.main.transform;var pos=cam.position;var rot=cam.rotation;double tempo=app.Clock.Tempo;var slow=app.Clock.DebugSlowMotion;
            Submit(Star());Assert.That(haptics.Firm,Is.EqualTo(1));
            Assert.That(cam.position,Is.EqualTo(pos));Assert.That(cam.rotation,Is.EqualTo(rot));Assert.That(app.Clock.Tempo,Is.EqualTo(tempo));Assert.That(app.Clock.DebugSlowMotion,Is.EqualTo(slow));
        }
        [Test] public void ScheduledPerfect_OneAudioOneHaptic()
        {
            var r=Submit(Star()-.02);Assert.That(r.Scheduled,Is.True);AssertCounts(0,0,0,0,0);
            while(app.Host.Current.Step<r.ContactStep+2)app.Host.Advance(StepReal);
            for(int i=0;i<40;i++)app.Host.Advance(StepReal);
            Assert.That(feedback.PerfectCount+feedback.ValidCount,Is.EqualTo(1));Assert.That(haptics.Firm+haptics.Discreet,Is.EqualTo(1));
        }
        [Test] public void RetroactivePerfect_OneAudioOneHaptic()
        {
            app.Tuning.Set("practice.launch",4);app.Tuning.Set("timing.validLimit",0);app.LaunchCurrent();
            app.Host.ClockMap.TrySimToReal(controller.Plan.KOut.Step,controller.Plan.KOut.Fraction,out var kout);var order=Order(kout-.03);
            for(int i=0;i<1200&&app.Host.IsRunning;i++)app.Host.Advance(StepReal);app.Host.Advance(StepReal*2);practice.Tick(.001);
            controller.Enqueue(order);controller.ProcessPending();Assert.That(controller.LastRecord.Applied,Is.True,controller.LastRecord.Reason);
            for(int i=0;i<40;i++)app.Host.Advance(StepReal);
            Assert.That(feedback.PerfectCount+feedback.ValidCount,Is.EqualTo(1));Assert.That(haptics.Firm+haptics.Discreet,Is.EqualTo(1));
        }
    }
    public sealed class HapticsPrepareTests
    {
        private AppBootstrap app;private GesturePreview preview;private TouchInputAdapter input;private Touchscreen screen;private RecordingHaptics haptics;private Vector2 anchor;private double start,now;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"S3 prepare fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("gesture.control",2);app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();app.Host.enabled=false;
            preview=app.GetComponent<GesturePreview>();input=app.GetComponent<TouchInputAdapter>();input.InputAreaOverride=new Rect(0,0,5000,5000);input.UiBlocker=_=>false;
            anchor=UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(3.2f,2.2f,0));start=now=Math.Ceiling(Time.realtimeSinceStartupAsDouble)+1;input.ReadRealTime=()=>now;
            screen=InputSystem.AddDevice<Touchscreen>();haptics=new RecordingHaptics();Haptics.Backend=haptics;
        }
        [UnityTearDown] public IEnumerator Cleanup(){Haptics.ResetBackend();if(screen!=null)InputSystem.RemoveDevice(screen);yield return PlayModeTestWorld.ClearApp();}
        private void Send(Phase phase,double dt,int id=1){now=start+dt;InputSystem.QueueStateEvent(screen,new TouchState{touchId=id,phase=phase,position=anchor},start+dt);InputSystem.Update();}
        [UnityTest] public IEnumerator AcceptedBegan_WithHapticsSwitchOff_DoesNotPrepare()
        {
            app.Tuning.Set("feedback.haptics",0);app.LaunchCurrent();Assert.That(Haptics.Enabled,Is.False,"launch snapshot applied the switch");
            Send(Phase.Began,0);Assert.That(preview.M24,Is.Not.Null,"accepted");Assert.That(haptics.Prepares,Is.Zero);
            yield return null;
        }
        [UnityTest] public IEnumerator AcceptedBegan_PreparesOnce_RejectedDoesNot()
        {
            Send(Phase.Began,0);Assert.That(preview.M24,Is.Not.Null,"accepted");Assert.That(haptics.Prepares,Is.EqualTo(1));
            Send(Phase.Began,.01,2);Assert.That(haptics.Prepares,Is.EqualTo(1),"secondary began is ignored");
            Send(Phase.Ended,.2);
            var shot=app.GetComponent<ShotController>();shot.Enqueue(new ClosedGesture(app.Host.LaunchId,"consume",1,start,start+.2,true,GestureOutcome.Swing,new Vector2(3.2f,2.2f),.5f,app.Host.ActiveConfig.Source,"fixture",start+.2));shot.ProcessPending();
            Assert.That(shot.AttemptConsumed,Is.True);Send(Phase.Began,.5);Assert.That(haptics.Prepares,Is.EqualTo(1),"rejected began (attempt-consumed)");
            yield return null;
        }
    }
}
