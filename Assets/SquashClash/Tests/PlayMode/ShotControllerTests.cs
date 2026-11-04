using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
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
    public sealed partial class ShotControllerTests
    {
        private AppBootstrap app;
        private ShotController controller;
        private PracticeMode practice;
        private TouchInputAdapter input;
        private GesturePreview preview;
        private Touchscreen screen;
        private bool wiredAtStartup;
        private double now;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"S2 fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);
            wiredAtStartup=app.GetComponent<ShotController>()!=null;
            controller=app.GetComponent<ShotController>()??app.gameObject.AddComponent<ShotController>();
            if(!wiredAtStartup)controller.Initialize(app);
            practice=app.GetComponent<PracticeMode>()??app.gameObject.AddComponent<PracticeMode>();
            if(!wiredAtStartup)practice.Initialize(app,controller);
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=practice.enabled=false;
            input=app.GetComponent<TouchInputAdapter>();preview=app.GetComponent<GesturePreview>();input.UiBlocker=_=>false;input.InputAreaOverride=new Rect(0,0,5000,5000);
            now=app.Host.LaunchRealTime;input.ReadRealTime=()=>now;screen=InputSystem.AddDevice<Touchscreen>();
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(screen!=null)InputSystem.RemoveDevice(screen);yield return PlayModeTestWorld.ClearApp();}
        private void At(double target)
        {
            for(int i=0;i<1500&&app.Host.ObservedRealTime+StepReal<target-1e-10;i++)app.Host.Advance(StepReal);
            double left=target-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);now=target;
        }
        private double Star()
        {Assert.That(controller.Plan.Exists,Is.True,"practice must present a reachable plan");Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var t),Is.True);return t;}
        private ClosedGesture Order(double time,GestureOutcome outcome=GestureOutcome.Swing,float p=.5f,string id=null,Vector2? aim=null,long? launch=null,string predecessor=null,ParamSet source=null)
            =>new ClosedGesture(launch??app.Host.LaunchId,id??Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,time,true,outcome,aim??new Vector2(3.2f,2.2f),p,source??app.Host.ActiveConfig.Source,"fixture",time,predecessor:predecessor);
        private ShotRecord Submit(double time,GestureOutcome outcome=GestureOutcome.Swing,float p=.5f,Vector2? aim=null)
        {At(time);controller.Enqueue(Order(time,outcome,p,aim:aim));controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null,"accepted order must be recorded");return controller.LastRecord;}
        private void Send(Phase phase,double time,float down=0,int id=101,double receivedAge=0)
        {
            var anchor=(Vector2)UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(3.2f,2.2f,0));
            float mm=ScreenMetrics.MmPerPx(Screen.dpi,(float)app.Tuning.Applied.Get("input.fallbackDpi"),out _);
            now=time+receivedAge;InputSystem.QueueStateEvent(screen,new TouchState{touchId=id,phase=phase,position=anchor+Vector2.down*(down/mm)},time);InputSystem.Update();
        }
        [Test] public void FirstPlayableShot_BallReachesAimWithinSolverTolerance()
        {
            double end=Star();var r=Submit(end);Assert.That(r.Applied,Is.True,r.Reason);var front=app.Host.Prediction.Contacts.First();Assert.That(front.Surface,Is.EqualTo(SquashClash.Core.Court.SurfaceId.FrontWall));
            Assert.That(Vector2.Distance(new Vector2(front.Point.x,front.Point.y),new Vector2(r.TargetUsedX,r.TargetUsedY)),Is.LessThanOrEqualTo(.05));Assert.That(app.Host.PredictionMatchesBitwise,Is.True);
        }
        [Test] public void FeasibleAdjustment_ShowsIntentionAndFeasibleMarks()
        {
            Assert.That(ParamCatalog.TryGet("practice.launch",out _),Is.True);app.Tuning.Set("practice.launch",2);app.Tuning.Set("shot.vMax",10);app.Tuning.Set("shot.vCap",10);app.LaunchCurrent();
            var r=Submit(Star(),p:0,aim:new Vector2(3.2f,4.4f));Assert.That(r.TargetLowered,Is.True);var reticle=app.GetComponentInChildren<AimReticleView>(true);
            Assert.That(reticle.Aim,Is.EqualTo(new Vector2(3.2f,4.4f)));Assert.That(reticle.FeasibleVisible,Is.True);Assert.That(reticle.FeasibleAim.y,Is.LessThan(reticle.Aim.y));
        }
        [Test] public void PanelChange_MidReception_DoesNotAffectCurrentShot()
        {
            double end=Star()-.15;var body=controller.Snapshot.Body;app.Tuning.Set("timing.validLimit",0);app.Tuning.Set("shot.vMax",10);app.Tuning.Set("human.x",4);
            var r=Submit(end);Assert.That(r.ValidLimitOn,Is.True);Assert.That(r.Result,Is.EqualTo("Failed"));Assert.That(controller.Snapshot.Body.PositionXZ,Is.EqualTo(body.PositionXZ));
            app.LaunchCurrent();Assert.That(controller.Snapshot.Timing.ValidLimitOn,Is.False);Assert.That(controller.Snapshot.Body.PositionXZ.x,Is.EqualTo(4));
        }
        [Test] public void FocusLoss_DuringGesture_NoSwingOnResume()
        {Send(Phase.Began,app.Host.LaunchRealTime+.1);app.gameObject.SendMessage("OnApplicationFocus",false);app.gameObject.SendMessage("OnApplicationFocus",true);app.GetComponent<ResumeOverlay>().Continue();Send(Phase.Ended,app.Host.LaunchRealTime+.4,35);controller.ProcessPending();Assert.That(controller.AttemptConsumed,Is.False);Assert.That(controller.LastClosureRecord,Is.Not.Null);Assert.That(controller.LastClosureRecord.GestureOutcome,Is.EqualTo("SystemCanceled"));}
        [TestCase("pause")][TestCase("tempo")][TestCase("slow")][TestCase("dev")]
        public void PauseTempoOrSlowMotionChange_DuringGesture_CancelsGesture(string cause)
        {
            double b=app.Host.LaunchRealTime+.1;Send(Phase.Began,b);
            if(cause=="pause"){app.Clock.Paused=true;preview.Tick(now);}if(cause=="tempo"){app.Clock.Configure(app.Clock.StepHz,.7);preview.Tick(now);}if(cause=="slow")app.Tuning.Set("debug.slowMotion",.5);if(cause=="dev")input.BlockedByUi=true;
            Send(Phase.Ended,b+.3,35);controller.ProcessPending();Assert.That(controller.AttemptConsumed,Is.False);Assert.That(controller.LastClosureRecord,Is.Not.Null);Assert.That(controller.LastClosureRecord.HasRelease,Is.False);
        }
        [Test] public void OneAttemptPerReception_SecondReleaseIgnored()
        {var r=Submit(Star());Assert.That(controller.AttemptConsumed,Is.True);controller.Enqueue(Order(r.ReleaseEventAtReal+.01));controller.ProcessPending();Assert.That(controller.LastRecord,Is.SameAs(r));Assert.That(controller.LastDiscardReason,Is.EqualTo("attempt-consumed"));}
        [Test] public void VModeAndPureT3_AreSwitchableAndRecordedPerShot()
        {var v=Submit(Star()-.15);Assert.That(v.Result,Is.EqualTo("Failed"));Assert.That(v.ValidLimitOn,Is.True);app.Tuning.Set("timing.validLimit",0);app.LaunchCurrent();var pure=Submit(Star()-.15);Assert.That(pure.Result,Is.EqualTo("Valid"));Assert.That(pure.ValidLimitOn,Is.False);Assert.That(pure.EarlySoft,Is.False);}
        [Test] public void T3AndT3Soft_AreSeparateSwitch()
        {app.Tuning.Set("timing.validLimit",0);app.LaunchCurrent();app.Host.ClockMap.TrySimToReal(controller.Plan.KIn.Step,controller.Plan.KIn.Fraction,out var kin);var hard=Submit(kin-.01);Assert.That(hard.Fail,Is.EqualTo("Air"));app.Tuning.Set("timing.earlyPolicy",1);app.LaunchCurrent();app.Host.ClockMap.TrySimToReal(controller.Plan.KIn.Step,controller.Plan.KIn.Fraction,out kin);var soft=Submit(kin-.01);Assert.That(soft.Result,Is.EqualTo("Valid"));Assert.That(soft.EarlyGeometry,Is.True);Assert.That(soft.EarlySoft,Is.True);}
        [Test] public void P1Backend_PracticeShowsNotAvailable_AndNeverShoots()
        {app.Tuning.Set("physics.backend",0);app.LaunchCurrent();Assert.That(controller.Available,Is.False);Assert.That(controller.UnavailableReason,Does.Contain("P1"));controller.Enqueue(Order(app.Host.LaunchRealTime+.2));controller.ProcessPending();Assert.That(controller.AttemptConsumed,Is.False);Assert.That(controller.LastDiscardReason,Is.EqualTo("unsupported-backend"));}
        [Test] public void PracticeCycle_DoesNotRelaunchBeforeOutgoingObservationEnds()
        {Submit(Star());long launch=app.Host.LaunchId;practice.Tick(.01);Assert.That(practice.Phase,Is.EqualTo(PracticePhase.Observing));practice.Tick(3);Assert.That(app.Host.LaunchId,Is.EqualTo(launch));for(int i=0;i<1200&&app.Host.IsRunning;i++)app.Host.Advance(StepReal);practice.Tick(.01);Assert.That(practice.Phase,Is.EqualTo(PracticePhase.Waiting));practice.Tick(1.5);Assert.That(app.Host.LaunchId,Is.EqualTo(launch+1));}
        [Test] public void PracticeCycle_NoOrderOrUnreachable_RelaunchesAfterReceptionEnds()
        {long id=app.Host.LaunchId;for(int i=0;i<1200&&app.Host.IsRunning;i++)app.Host.Advance(StepReal);practice.Tick(.01);Assert.That(controller.LastRecord,Is.Not.Null);Assert.That(controller.LastRecord.Result,Is.EqualTo("NoOrder"));Assert.That(controller.AttemptConsumed,Is.False);Assert.That(practice.Phase,Is.EqualTo(PracticePhase.Waiting));practice.Tick(1.5);Assert.That(app.Host.LaunchId,Is.EqualTo(id+1));}
        [Test] public void PracticeCycle_PausedOrUnfocused_DoesNotAdvanceWaiting()
        {for(int i=0;i<1200&&app.Host.IsRunning;i++)app.Host.Advance(StepReal);practice.Tick(.01);long id=app.Host.LaunchId;app.Clock.Paused=true;practice.Tick(100);Assert.That(app.Host.LaunchId,Is.EqualTo(id));app.Clock.Paused=false;app.gameObject.SendMessage("OnApplicationFocus",false);practice.Tick(100);Assert.That(app.Host.LaunchId,Is.EqualTo(id));app.gameObject.SendMessage("OnApplicationFocus",true);app.GetComponent<ResumeOverlay>().Continue();practice.Tick(1.5);Assert.That(app.Host.LaunchId,Is.EqualTo(id+1));}
        [Test] public void PracticeEnabled_SuspendsLauncherRepeat_WithoutChangingIt()
        {Assert.That(practice.Enabled,Is.True);app.Tuning.Set("launcher.repeatSeconds",3);long id=app.Host.LaunchId;var dir=Path.Combine(Application.persistentDataPath,"presets");var files=Directory.Exists(dir)?Directory.GetFiles(dir).ToDictionary(f=>f,File.ReadAllBytes):new Dictionary<string,byte[]>();app.TickRepeat(0);app.TickRepeat(30);Assert.That(app.Host.LaunchId,Is.EqualTo(id));Assert.That(app.Tuning.Applied.Get("launcher.repeatSeconds"),Is.EqualTo(3));foreach(var f in files)Assert.That(File.ReadAllBytes(f.Key),Is.EqualTo(f.Value));}
        [Test] public void PracticeDisabled_RestoresFirstBallRepeat()
        {Assert.That(ParamCatalog.TryGet("practice.enabled",out _),Is.True);app.Tuning.Set("practice.enabled",0);app.Tuning.Set("launcher.repeatSeconds",3);app.LaunchCurrent();long id=app.Host.LaunchId;app.TickRepeat(0);app.TickRepeat(3.1);Assert.That(practice.Enabled,Is.False);Assert.That(app.Host.LaunchId,Is.EqualTo(id+1));}
        [Test] public void SolverUnsolved_IsSystemFaultSwingWithoutContact()
        {var before=app.Host.Prediction;var r=Submit(Star(),aim:new Vector2(8,2));Assert.That(r.Result,Is.EqualTo("SystemFault"));Assert.That(r.Applied,Is.False);Assert.That(controller.AttemptConsumed,Is.True);Assert.That(app.Host.Prediction,Is.SameAs(before));}
        [Test] public void ProdConfig_HasPracticeAndFeedback_WithoutDevPanelTypes()
        {Assert.That(wiredAtStartup,Is.True);Assert.That(Resources.Load<AudioClip>("Audio/contact-provisional"),Is.Not.Null);Assert.That(Resources.Load<AudioClip>("Audio/swish-provisional"),Is.Not.Null);foreach(var f in Directory.GetFiles(Path.Combine(Application.dataPath,"SquashClash/Runtime/Shot"),"*.cs"))Assert.That(File.ReadAllText(f),Does.Not.Contain("SquashClash.DevPanel"),f);Assert.That(controller.PracticeEnabled,Is.True);}
        [TestCase(GestureOutcome.InvalidAim)][TestCase(GestureOutcome.Tap)][TestCase(GestureOutcome.AbandonedNoPull)][TestCase(GestureOutcome.CanceledReturnToDeadZone)][TestCase(GestureOutcome.Rejected)][TestCase(GestureOutcome.SystemCanceled)]
        public void M24ClosureTable_PreservesAttemptAndReason(GestureOutcome outcome)
        {controller.Enqueue(Order(app.Host.LaunchRealTime+.3,outcome));controller.ProcessPending();Assert.That(controller.AttemptConsumed,Is.False);Assert.That(controller.LastClosureRecord,Is.Not.Null);Assert.That(controller.LastClosureRecord.GestureOutcome,Is.EqualTo(outcome.ToString()));Assert.That(controller.LastClosureRecord.Reason,Is.EqualTo("fixture"));}
        [Test] public void NoOrderWithOnlyCanceledContacts_DoesNotShoot()
        {controller.Enqueue(Order(app.Host.LaunchRealTime+.2,GestureOutcome.CanceledReturnToDeadZone));controller.ProcessPending();controller.CloseReception();Assert.That(controller.LastRecord,Is.Not.Null);Assert.That(controller.LastRecord.Result,Is.EqualTo("NoOrder"));Assert.That(controller.AttemptConsumed,Is.False);}
        [Test] public void BeganOutsideReception_IsNotPromoted()
        {for(int i=0;i<1200&&app.Host.IsRunning;i++)app.Host.Advance(StepReal);practice.Tick(.01);Assert.That(controller.CanBegin(app.Host.ObservedRealTime+.01,out var reason),Is.False);Assert.That(reason,Does.Contain("reception"));}
        [Test] public void CancelThenNewBegan_CorrelatesWithoutReplacement()
        {double end=Star();var first=Order(end-.1,GestureOutcome.CanceledReturnToDeadZone,id:"cancel-1");controller.Enqueue(first);controller.ProcessPending();Assert.That(controller.LastClosureRecord,Is.Not.Null);var saved=controller.LastClosureRecord;At(end);controller.Enqueue(Order(end,id:"new-2",predecessor:"cancel-1"));controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);Assert.That(controller.LastRecord.PredecessorId,Is.EqualTo(saved.AttemptId));Assert.That(saved.GestureOutcome,Is.EqualTo("CanceledReturnToDeadZone"));}
        [Test] public void SameTimestampIdenticalEnded_OneShotWithReliableP()
        {double end=Star(),b=app.Host.LaunchRealTime+.1;Send(Phase.Began,b);Send(Phase.Moved,end,18.5f);At(end+.02);double delivered=double.NaN;Action<TouchSample> capture=sample=>{if(sample.Phase==TouchPhaseKind.Ended)delivered=sample.TimeReal;};input.SampleReceived+=capture;Send(Phase.Ended,end,18.5f,receivedAge:.02);input.SampleReceived-=capture;controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);Assert.That(controller.LastRecord.Power,Is.EqualTo(.5f).Within(.002));Assert.That(controller.LastRecord.ReleaseEventAtReal,Is.EqualTo(delivered));Assert.That(controller.LastRecord.Applied,Is.True);var record=controller.LastRecord;Send(Phase.Ended,end+.01,18.5f);controller.ProcessPending();Assert.That(controller.LastRecord,Is.SameAs(record));}
        [Test] public void ContradictoryEnded_NoShot()
        {double b=app.Host.LaunchRealTime+.1;Send(Phase.Began,b);Send(Phase.Moved,b+.1,20);Send(Phase.Ended,b+.1,21);controller.ProcessPending();Assert.That(controller.AttemptConsumed,Is.False);Assert.That(controller.LastClosureRecord,Is.Not.Null);Assert.That(controller.LastClosureRecord.GestureOutcome,Is.EqualTo("SystemCanceled"));}
        [Test] public void SecondaryFinger_NoSecondOrder()
        {double b=app.Host.LaunchRealTime+.1;Send(Phase.Began,b);Send(Phase.Began,b+.01,35,id:202);Send(Phase.Ended,b+.02,35,id:202);controller.ProcessPending();Assert.That(controller.AttemptConsumed,Is.False);Assert.That(preview.Active,Is.True);Send(Phase.Ended,b+.3,0);controller.ProcessPending();Assert.That(controller.LastClosureRecord,Is.Not.Null);Assert.That(controller.LastClosureRecord.FingerId,Is.EqualTo(101));}
        [Test] public void FollowCamera_MovesScreenProjectionNotWorldAimOrPower()
        {app.Tuning.Set("camera.mode",1);app.LaunchCurrent();double b=app.Host.LaunchRealTime+.1;Send(Phase.Began,b);Assert.That(preview.M24,Is.Not.Null);var aim=preview.M24.Aim;var camera=UnityEngine.Camera.main;var old=camera.WorldToScreenPoint(new Vector3(aim.x,aim.y,0));var rig=UnityEngine.Object.FindFirstObjectByType<SquashClash.Runtime.Camera.CourtCameraRig>();rig.Tick(new Vector3(0,1,5),.3f);Assert.That(preview.M24.Aim,Is.EqualTo(aim));Assert.That(preview.M24.P,Is.Zero);Assert.That(Vector3.Distance(old,camera.WorldToScreenPoint(new Vector3(aim.x,aim.y,0))),Is.GreaterThan(0));}
        [Test] public void PendingOrWrongHash_BlockArmRejectedWithRemedy()
        {app.Tuning.Set("launcher.speed",16);Assert.That(preview.ArmSlingBlock("B1"),Is.False);Assert.That(preview.SlingArmRejection,Is.Not.Empty);}
        [Test] public void ControlProtection_NoAccidentalP2H17Scenario()
        {var bar=UnityEngine.Object.FindFirstObjectByType<DevBar>();Assert.That(bar,Is.Not.Null);foreach(var control in new[]{"scenario","physics-spike","performance-session"})Assert.That(bar.ActivateExperiment(control),Is.False);Assert.That(app.Host.ActiveConfig.Backend,Is.EqualTo("P2-Analytic"));}
        [Test] public void DelayedEndedAfterSecondBounce_PreservesOriginalR()
        {Assert.That(ParamCatalog.TryGet("practice.launch",out _),Is.True);app.Tuning.Set("practice.launch",4);app.Tuning.Set("timing.validLimit",0);app.LaunchCurrent();app.Host.ClockMap.TrySimToReal(controller.Plan.KOut.Step,controller.Plan.KOut.Fraction,out var kout);double original=kout-.03;var order=Order(original);for(int i=0;i<1200&&app.Host.IsRunning;i++)app.Host.Advance(StepReal);app.Host.Advance(StepReal*2);practice.Tick(.001);Assert.That(controller.LastRecord,Is.Not.Null);Assert.That(controller.LastRecord.Result,Is.EqualTo("NoOrder"));int version=controller.LastRecord.Version;controller.Enqueue(order);controller.ProcessPending();Assert.That(controller.LastRecord.ReleaseEventAtReal,Is.EqualTo(original));Assert.That(controller.LastRecord.Applied,Is.True,controller.LastRecord.Reason);Assert.That(controller.LastRecord.Version,Is.GreaterThan(version));}
        [Test] public void OldLaunchEnded_DoesNotTouchNewBall()
        {double t=Star();var old=Order(t);app.LaunchCurrent();var before=app.Host.Current;controller.Enqueue(old);controller.ProcessPending();Assert.That(controller.LastDiscardReason,Is.EqualTo("wrong-launch"));Assert.That(controller.AttemptConsumed,Is.False);Assert.That(app.Host.Current,Is.EqualTo(before));}
        [Test] public void FuturePerfect_ReservesOnce_NoSecondOrder()
        {var r=Submit(Star()-.02);Assert.That(controller.WaitingForContact,Is.True);Assert.That(r.Scheduled,Is.True);controller.Enqueue(Order(r.ReleaseEventAtReal+.001));controller.ProcessPending();Assert.That(controller.LastDiscardReason,Is.EqualTo("attempt-consumed"));while(app.Host.Current.Step<r.ContactStep)app.Host.Advance(StepReal);Assert.That(controller.WaitingForContact,Is.False);Assert.That(controller.LastRecord.Applied,Is.True);}
        [Test] public void DroppedSteps_DuringGesture_CancelsWithoutSyntheticRelease()
        {double b=app.Host.LaunchRealTime+.1;Send(Phase.Began,b);app.Clock.Advance(.5);Send(Phase.Ended,b+.4,35);controller.ProcessPending();Assert.That(controller.LastClosureRecord,Is.Not.Null);Assert.That(controller.LastClosureRecord.GestureOutcome,Is.EqualTo("SystemCanceled"));Assert.That(controller.AttemptConsumed,Is.False);}
        [Test] public void PauseAfterAcceptedEnded_DoesNotCancelOrDuplicateOrder()
        {double end=Star(),b=app.Host.LaunchRealTime+.1;Send(Phase.Began,b);Send(Phase.Moved,end-.01,18.5f);At(end+.02);Send(Phase.Ended,end,18.5f,receivedAge:.02);app.Clock.Paused=true;controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);Assert.That(controller.LastRecord.Applied,Is.True);Assert.That(controller.AttemptConsumed,Is.True);}
        [TestCase("held")][TestCase("accidental-slip")][TestCase("lateral-slip")][TestCase("return-reduces")]
        public void M24DistanceAndOpenRisks_AreNotReinterpretedByController(string kind)
        {
            double end=Star(),b=end-1;
            var projection=new FrontisProjection(Matrix4x4.TRS(new Vector3(3.2f,3,10),Quaternion.identity,Vector3.one),Matrix4x4.Ortho(-3.2f,3.2f,-3,3,.1f,100),new Rect(0,0,640,600),new Rect(0,0,640,600),app.Config.Court,b,0);
            var model=new M24Model(SlingSettings.From(app.Tuning.Applied),projection,1);
            model.Push(new TouchSample(1,TouchPhaseKind.Began,new Vector2(320,220),b,1));
            model.Push(new TouchSample(1,TouchPhaseKind.Moved,new Vector2(320,185),b+.1,2));
            var point=kind=="lateral-slip"?new Vector2(350,217.9f):kind=="return-reduces"?new Vector2(320,201.5f):new Vector2(320,185);
            model.Push(new TouchSample(1,TouchPhaseKind.Ended,point,end,3));Assert.That(model.Outcome,Is.EqualTo(GestureOutcome.Swing));
            At(end);controller.Enqueue(new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,b,end,true,model.Outcome,model.Aim,model.P,app.Tuning.Applied));controller.ProcessPending();
            Assert.That(controller.LastRecord,Is.Not.Null);Assert.That(controller.LastRecord.Power,Is.EqualTo(model.P));Assert.That(controller.AttemptConsumed,Is.True);
            if(kind=="lateral-slip")Assert.That(controller.LastRecord.Power,Is.GreaterThan(0).And.LessThan(.01));
            if(kind=="return-reduces")Assert.That(controller.LastRecord.Power,Is.EqualTo(.5f));
        }
        [Test] public void RequestUsesBeganAimAndEndedP_ContactCenterFromDecidedStep()
        {
            app.SetPresetIdentity("at-began","file-at-began");double end=Star(),b=app.Host.LaunchRealTime+.1;var incoming=app.Host.Prediction;
            Send(Phase.Began,b);Assert.That(preview.M24,Is.Not.Null);var intended=preview.M24.Aim;Send(Phase.Moved,end-.01,18.5f);At(end+.02);Send(Phase.Ended,end,18.5f,receivedAge:.02);
            app.SetPresetIdentity("after-ended","later-file");controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);var record=controller.LastRecord;
            Assert.That(record.Preset,Is.EqualTo("at-began"));Assert.That(record.FileHash,Is.EqualTo("file-at-began"));Assert.That(new Vector2(record.AimX,record.AimY),Is.EqualTo(intended));
            var state=incoming.States.First(s=>s.Step==record.ContactStep);Assert.That(new Vector3(record.ContactX,record.ContactY,record.ContactZ),Is.EqualTo(state.Position));Assert.That(record.Power,Is.EqualTo(.5f).Within(.002));
        }
        [Test] public void DevPanel_ShowsCapturedShotSettingsSeparatelyFromNextReception()
        {var root=app.GetComponent<UnityEngine.UIElements.UIDocument>().rootVisualElement;Assert.That(root.Q("first-shot-section"),Is.Not.Null);app.Tuning.Set("timing.validLimit",0);UnityEngine.Object.FindFirstObjectByType<DevPanelView>().SendMessage("Refresh");Assert.That(root.Q<UnityEngine.UIElements.Label>("shot-captured").text,Does.Contain("Actual: V").And.Contain("Próximo: T3 puro"));}
        [TestCase(0f)][TestCase(1.5f)]
        public void M2S2_N01_NoPullOrJitter150ms_NoSwingOrConsumption(float down)
        {
            double b=app.Host.LaunchRealTime+.1;Send(Phase.Began,b);Send(Phase.Moved,b+.1,down);
            controller.ProcessPending();Assert.That(controller.LastClosureRecord,Is.Null);Assert.That(controller.AttemptConsumed,Is.False);
            Send(Phase.Ended,b+.15,down);controller.ProcessPending();
            Assert.That(controller.LastClosureRecord.GestureOutcome,Is.EqualTo("Tap"));Assert.That(controller.LastClosureRecord.Power,Is.Zero);
            Assert.That(controller.AttemptConsumed,Is.False);Assert.That(controller.LastRecord,Is.Null);
        }
        [Test] public void M2S2_N01_HeldFingerAtReceptionEnd_DoesNotInventEnded()
        {
            Send(Phase.Began,app.Host.LaunchRealTime+.1);Send(Phase.Moved,app.Host.LaunchRealTime+.2,18.5f);
            while(app.Host.IsRunning)app.Host.Advance(StepReal);
            practice.Tick(.001);controller.ProcessPending();Assert.That(controller.LastRecord.Result,Is.EqualTo("NoOrder"));
            Assert.That(controller.LastRecord.HasRelease,Is.False);Assert.That(controller.AttemptConsumed,Is.False);Assert.That(controller.LastClosureRecord,Is.Null);
        }
        [Test] public void M2S2_N02_HeldPullAbovePointSix_KeepsDistanceAtRealEnded()
        {
            double end=Star(),b=end-1;Send(Phase.Began,b);Send(Phase.Moved,b+.1,25.1f);At(end+.02);Send(Phase.Ended,end,25.1f,receivedAge:.02);controller.ProcessPending();
            Assert.That(controller.LastRecord.Power,Is.EqualTo(.7f).Within(.002));Assert.That(controller.LastRecord.Applied,Is.True);
        }
        [Test] public void M2S2_N04_N08_CancelAndReapply_PreserveBothClosuresAndOpportunity()
        {
            double end=Star(),b=end-.6;long launch=app.Host.LaunchId;var plan=controller.Plan;
            Send(Phase.Began,b);Send(Phase.Moved,b+.1,18.5f);Send(Phase.Ended,b+.2);controller.ProcessPending();
            var first=controller.LastClosureRecord;Assert.That(first.GestureOutcome,Is.EqualTo("CanceledReturnToDeadZone"));Assert.That(controller.AttemptConsumed,Is.False);
            Send(Phase.Began,b+.3,id:202);Send(Phase.Moved,b+.4,18.5f,id:202);Send(Phase.Moved,b+.45,0,id:202);
            controller.ProcessPending();Assert.That(controller.AttemptConsumed,Is.False);Assert.That(preview.Active,Is.True);
            At(end+.02);Send(Phase.Ended,end,18.5f,id:202,receivedAge:.02);controller.ProcessPending();
            Assert.That(controller.LastRecord.Applied,Is.True);Assert.That(controller.LastRecord.PredecessorId,Is.EqualTo(first.AttemptId));
            Assert.That(controller.LastRecord.AttemptId,Is.Not.EqualTo(first.AttemptId));Assert.That(first.GestureOutcome,Is.EqualTo("CanceledReturnToDeadZone"));
            Assert.That(app.Host.LaunchId,Is.EqualTo(launch));Assert.That(controller.Plan.KOut,Is.EqualTo(plan.KOut));
        }
        [TestCase("config")][TestCase("viewport")]
        public void M2S2_N07_HeldAcrossContextChange_RequiresNewBegan(string cause)
        {
            double b=app.Host.LaunchRealTime+.1;Send(Phase.Began,b);Send(Phase.Moved,b+.1,18.5f);
            if(cause=="config")app.Tuning.Set("power.pullFullMm",40);else UnityEngine.Camera.main.rect=new Rect(.05f,0,.95f,1);
            Send(Phase.Ended,b+.2,35);controller.ProcessPending();
            Assert.That(controller.LastClosureRecord.GestureOutcome,Is.EqualTo("SystemCanceled"));Assert.That(controller.LastClosureRecord.HasRelease,Is.False);
            Assert.That(controller.AttemptConsumed,Is.False);Assert.That(controller.LastRecord,Is.Null);
            Send(Phase.Ended,b+.3,35);controller.ProcessPending();Assert.That(controller.AttemptConsumed,Is.False);
            Send(Phase.Began,b+.4,id:202);Assert.That(preview.Active,Is.True);
        }
        [TestCase("late-began")][TestCase("early")][TestCase("late")][TestCase("outside-v")][TestCase("no-plan")]
        public void M2S2_N09_EndedDecidesCategoryAndConsumption_WithoutRescue(string kind)
        {
            double end=Star();string result="Perfect",fail="None";
            if(kind=="early"){app.Host.ClockMap.TrySimToReal(controller.Plan.KIn.Step,controller.Plan.KIn.Fraction,out end);end-=.01;result="Failed";fail="Air";}
            if(kind=="late"){app.Host.ClockMap.TrySimToReal(controller.Plan.KOut.Step,controller.Plan.KOut.Fraction,out end);end+=.01;result="Failed";fail="Late";}
            if(kind=="outside-v"){end-=.15;result="Failed";fail="Air";}
            if(kind=="no-plan"){app.Tuning.Set("human.x",0);app.Tuning.Set("human.z",0);app.LaunchCurrent();Assert.That(controller.Plan.Exists,Is.False);end=app.Host.LaunchRealTime+.4;result="Unreachable";}
            At(end+.02);var order=new ClosedGesture(app.Host.LaunchId,"n09",1,end-.001,end,true,GestureOutcome.Swing,new Vector2(3.2f,2.2f),.5f,app.Tuning.Applied);
            controller.Enqueue(order);controller.ProcessPending();Assert.That(controller.LastRecord.Result,Is.EqualTo(result));Assert.That(controller.LastRecord.Fail,Is.EqualTo(fail));
            Assert.That(controller.LastRecord.ReleaseEventAtReal,Is.EqualTo(end));Assert.That(controller.AttemptConsumed,Is.True);
            Assert.That(controller.LastRecord.Applied,Is.EqualTo(kind=="late-began"));
        }
        [Test] public void WrongPredeclaredHash_BlockArmRejectedWithVisibleRemedy()
        {
            var before=app.Host.Current;long launch=app.Host.LaunchId;
            Assert.That(preview.ArmSlingBlock("B1","incorrect-predeclared-hash"),Is.False);
            Assert.That(preview.SlingTrials.Running,Is.False);Assert.That(preview.SlingArmRejection,Does.Contain("hash").And.Contain("relanza"));
            Assert.That(app.Host.Current,Is.EqualTo(before));Assert.That(app.Host.LaunchId,Is.EqualTo(launch));Assert.That(controller.AttemptConsumed,Is.False);
            Assert.That(preview.ArmSlingBlock("B1",app.Host.ActiveConfig.Hash),Is.True);
        }
        [Test] public void M2S2_N08_MultipleClosuresRemainAppendOnly_WithoutExtendingReception()
        {
            var saved=new List<ShotRecord>();long launch=app.Host.LaunchId;var plan=controller.Plan;
            foreach(var outcome in new[]{GestureOutcome.InvalidAim,GestureOutcome.Rejected,GestureOutcome.CanceledReturnToDeadZone})
            {
                controller.Enqueue(Order(app.Host.LaunchRealTime+.2,outcome,id:"n08-"+outcome,predecessor:saved.LastOrDefault()?.AttemptId));controller.ProcessPending();saved.Add(controller.LastClosureRecord);
            }
            controller.CloseReception();Assert.That(controller.LastRecord.Result,Is.EqualTo("NoOrder"));Assert.That(controller.AttemptConsumed,Is.False);
            Assert.That(app.Host.LaunchId,Is.EqualTo(launch));Assert.That(controller.Plan.KOut,Is.EqualTo(plan.KOut));
            var log=File.ReadAllText(app.Metrics.CurrentFilePath);
            foreach(var record in saved){Assert.That(log,Does.Contain(record.AttemptId));Assert.That(record.Result,Is.EqualTo("Closure"));Assert.That(record.HasRelease,Is.True);}
            Assert.That(saved.Select(r=>r.AttemptId).Distinct().Count(),Is.EqualTo(3));Assert.That(saved[1].PredecessorId,Is.EqualTo(saved[0].AttemptId));
        }
        [TestCase(0)][TestCase(1)]
        public void M2S2_N12_ExplicitAlternativeSnapshot_IsNotReinterpretedAsM24(int control)
        {
            app.Tuning.Set("gesture.control",control);app.LaunchCurrent();double end=Star();
            var record=Submit(end,p:.7f);Assert.That(record.Applied,Is.True);Assert.That(record.Power,Is.EqualTo(.7f));
            Assert.That(app.Host.ActiveConfig.Source.GetInt("gesture.control"),Is.EqualTo(control));Assert.That(record.GestureHash,Is.EqualTo(app.Tuning.Applied.Hash));
            Send(Phase.Began,end+.1,id:202);controller.ProcessPending();Assert.That(preview.Active,Is.False);Assert.That(controller.LastRecord,Is.SameAs(record));
        }
        [Test] public void SameM24Ended_PunctualAndDelayed_SameBits()
        {
            BallState Capture(bool delayed)
            {
                double end=Star(),b=end-.4;long present=controller.Plan.TStarStep+6;
                Send(Phase.Began,b);Send(Phase.Moved,end-.01,18.5f);
                if(delayed){while(app.Host.Current.Step<present)app.Host.Advance(StepReal);Send(Phase.Ended,end,18.5f,receivedAge:6*StepReal);}
                else{At(end+.02);Send(Phase.Ended,end,18.5f,receivedAge:.02);}
                controller.ProcessPending();Assert.That(controller.LastRecord.Applied,Is.True,controller.LastRecord.Reason);
                while(app.Host.Current.Step<present)app.Host.Advance(StepReal);
                Assert.That(app.Host.Current.Step,Is.EqualTo(present));return app.Host.Current;
            }
            var punctual=Capture(false);app.LaunchCurrent();var delayed=Capture(true);
            Assert.That(delayed.Step,Is.EqualTo(punctual.Step));
            foreach(var pair in new[]{(punctual.Position.x,delayed.Position.x),(punctual.Position.y,delayed.Position.y),(punctual.Position.z,delayed.Position.z),(punctual.Velocity.x,delayed.Velocity.x),(punctual.Velocity.y,delayed.Velocity.y),(punctual.Velocity.z,delayed.Velocity.z)})
                Assert.That(BitConverter.SingleToInt32Bits(pair.Item1),Is.EqualTo(BitConverter.SingleToInt32Bits(pair.Item2)));
        }
        [Test] public void M2S2_N10_ExpiredHistory_IsSystemNotEvaluable_WithoutContactOrSecondAttempt()
        {
            double end=Star();var order=Order(end);At(end+.3);var before=app.Host.Current;
            controller.Enqueue(order);controller.ProcessPending();Assert.That(controller.LastRecord.Result,Is.EqualTo("NotEvaluable"));Assert.That(controller.LastRecord.Reason,Is.EqualTo("history-expired"));
            Assert.That(controller.LastRecord.ReleaseEventAtReal,Is.EqualTo(end));Assert.That(controller.LastRecord.Applied,Is.False);Assert.That(controller.AttemptConsumed,Is.True);Assert.That(app.Host.Current,Is.EqualTo(before));
        }
        [Test] public void M2S2_N11_PlanAfterBegan_UsesVisibleAtOriginalEnded_NotProcessingPlan()
        {
            double end=Star();var p=controller.Plan;
            var changed=new ContactPlan(p.Exists,p.KIn,p.KOut,p.TStarStep,p.PStar,p.Side,p.Version+1,p.Body,p.Legal,p.Court,p.Reason);
            controller.Presented.Present(end-.02,changed,end+.2);
            controller.Presented.Present(end+.01,new ContactPlan(p.Exists,p.KIn,p.KOut,p.TStarStep,p.PStar,p.Side,p.Version+2,p.Body,p.Legal,p.Court,p.Reason),end+.5);
            At(end+.02);var record=Submit(end);Assert.That(record.PlanVersion,Is.EqualTo(changed.Version));Assert.That(record.TargetContactAtReal,Is.EqualTo(end+.2));
            Assert.That(record.DeltaMs,Is.EqualTo(-200).Within(.00001));Assert.That(new Vector2(record.AimX,record.AimY),Is.EqualTo(new Vector2(3.2f,2.2f)));
        }
        [Test] public void InputInTransit_RecordsO2AndBeganProjectionSeparately()
        {double end=Star(),b=app.Host.LaunchRealTime+.1;Send(Phase.Began,b,receivedAge:.02);Send(Phase.Moved,b+.01,5,receivedAge:.04);Send(Phase.Moved,end-.01,18.5f);At(end+.02);double delivered=double.NaN;Action<TouchSample> capture=sample=>{if(sample.Phase==TouchPhaseKind.Ended)delivered=sample.TimeReal;};input.SampleReceived+=capture;Send(Phase.Ended,end,18.5f,receivedAge:.02);input.SampleReceived-=capture;controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);Assert.That(controller.LastRecord.O2Count,Is.EqualTo(1));Assert.That(controller.LastRecord.ProjectionObservedAt,Is.EqualTo(b+.02));Assert.That(controller.LastRecord.ReleaseEventAtReal,Is.EqualTo(delivered));}
    }
}
