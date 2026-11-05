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
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    public sealed class ShotPresentationTests
    {
        private AppBootstrap app;
        private ShotController controller;
        private ShotFeedback feedback;
        private PracticeMode practice;
        private readonly List<ShotPresentation> seen=new List<ShotPresentation>();
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"S3 fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);
            bool wired=app.GetComponent<ShotController>()!=null;
            controller=app.GetComponent<ShotController>()??app.gameObject.AddComponent<ShotController>();
            if(!wired)controller.Initialize(app);
            practice=app.GetComponent<PracticeMode>()??app.gameObject.AddComponent<PracticeMode>();
            if(!wired)practice.Initialize(app,controller);
            feedback=controller.GetComponent<ShotFeedback>();seen.Clear();controller.PresentationDelivered+=seen.Add;
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=practice.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return PlayModeTestWorld.ClearApp();}
        private void At(double target)
        {
            for(int i=0;i<1500&&app.Host.ObservedRealTime+StepReal<target-1e-10;i++)app.Host.Advance(StepReal);
            double left=target-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);
        }
        private double Star()
        {Assert.That(controller.Plan.Exists,Is.True);Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var t),Is.True);return t;}
        private ClosedGesture Order(double time,GestureOutcome outcome=GestureOutcome.Swing,string id=null,Vector2? aim=null,long? launch=null,float p=.5f)
            =>new ClosedGesture(launch??app.Host.LaunchId,id??Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,time,true,outcome,aim??new Vector2(3.2f,2.2f),p,app.Host.ActiveConfig.Source,"fixture",time);
        private ShotRecord Submit(double time,Vector2? aim=null)
        {At(time);controller.Enqueue(Order(time,aim:aim));controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);return controller.LastRecord;}
        private void Exhaust(){for(int i=0;i<1200&&app.Host.IsRunning;i++)app.Host.Advance(StepReal);}
        [Test] public void PunctualAppliedContact_DeliversExactlyOnce_AndDrivesContactAudio()
        {
            int contact=feedback.ContactCount,swish=feedback.SwishCount;var r=Submit(Star());Assert.That(r.Applied,Is.True,r.Reason);
            Assert.That(seen.Count,Is.EqualTo(1));Assert.That(controller.PresentationCount,Is.EqualTo(1));Assert.That(controller.LastPresentation.Value.Category.ToString(),Is.EqualTo(r.Result));
            Assert.That(seen[0].HasContact,Is.True);Assert.That(seen[0].ContactStep,Is.EqualTo(r.ContactStep));Assert.That(seen[0].AttemptId,Is.EqualTo(r.AttemptId));Assert.That(seen[0].LaunchId,Is.EqualTo(app.Host.LaunchId));
            Assert.That(feedback.ContactCount,Is.EqualTo(contact+1));Assert.That(feedback.SwishCount,Is.EqualTo(swish));
            for(int i=0;i<60;i++)app.Host.Advance(StepReal);Assert.That(seen.Count,Is.EqualTo(1),"resimulation / later frames must not deliver");
        }
        [Test] public void ScheduledFuturePerfect_DeliversNothingBeforeContactStep_ThenOnce()
        {
            var r=Submit(Star()-.02);Assert.That(r.Scheduled,Is.True);Assert.That(controller.WaitingForContact,Is.True);
            Assert.That(seen.Count,Is.Zero,"order version / scheduling never delivers");
            while(app.Host.Current.Step<r.ContactStep-1){app.Host.Advance(StepReal);Assert.That(seen.Count,Is.Zero);}
            while(app.Host.Current.Step<r.ContactStep+2)app.Host.Advance(StepReal);
            Assert.That(controller.LastRecord.Applied,Is.True);Assert.That(seen.Count,Is.EqualTo(1));Assert.That(seen[0].ContactStep,Is.EqualTo(r.ContactStep));
        }
        [Test] public void RetroactiveApplication_DeliversOnce_NoneFromResimulation()
        {
            app.Tuning.Set("practice.launch",4);app.Tuning.Set("timing.validLimit",0);app.LaunchCurrent();seen.Clear();
            app.Host.ClockMap.TrySimToReal(controller.Plan.KOut.Step,controller.Plan.KOut.Fraction,out var kout);var order=Order(kout-.03);
            Exhaust();app.Host.Advance(StepReal*2);practice.Tick(.001);Assert.That(controller.LastRecord.Result,Is.EqualTo("NoOrder"));Assert.That(seen.Count,Is.Zero);
            controller.Enqueue(order);controller.ProcessPending();Assert.That(controller.LastRecord.Applied,Is.True,controller.LastRecord.Reason);
            Assert.That(seen.Count,Is.EqualTo(1));Assert.That(seen[0].HasContact,Is.True);for(int i=0;i<40;i++)app.Host.Advance(StepReal);Assert.That(seen.Count,Is.EqualTo(1));
        }
        [Test] public void FailedRelease_DeliversOneFailedPresentation_AndOnlySwish()
        {
            int contact=feedback.ContactCount,swish=feedback.SwishCount;var r=Submit(Star()-.15);Assert.That(r.Result,Is.EqualTo("Failed"));
            Assert.That(seen.Count,Is.EqualTo(1));Assert.That(seen[0].Category,Is.EqualTo(PresentationCategory.Failed));Assert.That(seen[0].HasContact,Is.False);Assert.That(seen[0].Fail,Is.EqualTo(r.Fail));
            Assert.That(feedback.SwishCount,Is.EqualTo(swish+1));Assert.That(feedback.ContactCount,Is.EqualTo(contact));
        }
        [Test] public void NoOrderAtReceptionEnd_DeliversNothing()
        {Exhaust();practice.Tick(.001);Assert.That(controller.LastRecord.Result,Is.EqualTo("NoOrder"));Assert.That(seen.Count,Is.Zero);Assert.That(controller.PresentationCount,Is.Zero);}
        [Test] public void CancelClosure_DeliversNothing()
        {controller.Enqueue(Order(app.Host.LaunchRealTime+.3,GestureOutcome.CanceledReturnToDeadZone));controller.ProcessPending();Assert.That(controller.LastClosureRecord,Is.Not.Null);Assert.That(seen.Count,Is.Zero);}
        [Test] public void DuplicateOrderId_DeliversNothing()
        {
            controller.Enqueue(Order(app.Host.LaunchRealTime+.3,GestureOutcome.Tap,"dup"));controller.ProcessPending();controller.Enqueue(Order(Star(),id:"dup"));controller.ProcessPending();
            Assert.That(controller.LastDiscardReason,Is.EqualTo("duplicate-order"));Assert.That(seen.Count,Is.Zero);
        }
        [Test] public void OldLaunchOrder_DeliversNothing()
        {double t=Star();var old=Order(t);app.LaunchCurrent();controller.Enqueue(old);controller.ProcessPending();Assert.That(controller.LastDiscardReason,Is.EqualTo("wrong-launch"));Assert.That(seen.Count,Is.Zero);}
        [Test] public void SecondReleaseAfterConsumption_DeliversNothing_AndKeepsSingleDelivery()
        {
            var r=Submit(Star());Assert.That(seen.Count,Is.EqualTo(1));controller.Enqueue(Order(r.ReleaseEventAtReal+.01));controller.ProcessPending();
            Assert.That(controller.LastDiscardReason,Is.EqualTo("attempt-consumed"));Assert.That(seen.Count,Is.EqualTo(1));
        }
        [Test] public void HistoryExpired_DeliversNothing()
        {double end=Star();var order=Order(end);At(end+.3);controller.Enqueue(order);controller.ProcessPending();Assert.That(controller.LastRecord.Result,Is.EqualTo("NotEvaluable"));Assert.That(seen.Count,Is.Zero);}
        [Test] public void SolverUnsolved_SystemFault_DeliversNothing()
        {var r=Submit(Star(),new Vector2(8,2));Assert.That(r.Result,Is.EqualTo("SystemFault"));Assert.That(seen.Count,Is.Zero);Assert.That(feedback.ContactCount,Is.Zero);Assert.That(feedback.SwishCount,Is.Zero);}
        [Test] public void ReentrantSubscriber_StillOneDelivery()
        {
            int calls=0;controller.PresentationDelivered+=_=>{calls++;controller.Enqueue(Order(app.Host.ObservedRealTime));controller.ProcessPending();};
            var r=Submit(Star());Assert.That(r.Applied,Is.True);Assert.That(seen.Count,Is.EqualTo(1));Assert.That(calls,Is.EqualTo(1));Assert.That(controller.PresentationCount,Is.EqualTo(1));
        }
        [Test] public void NewLaunch_ResetsPresentationState_AndDeliversForNewLaunch()
        {
            Submit(Star());Assert.That(controller.PresentationCount,Is.EqualTo(1));long old=app.Host.LaunchId;app.LaunchCurrent();
            Assert.That(controller.PresentationCount,Is.Zero);Assert.That(controller.LastPresentation,Is.Null);
            var r=Submit(Star());Assert.That(r.Applied,Is.True);Assert.That(seen.Count,Is.EqualTo(2));Assert.That(seen[1].LaunchId,Is.EqualTo(old+1));Assert.That(controller.PresentationCount,Is.EqualTo(1));
        }
        [TestCase("lateral-slip")][TestCase("accidental-slip")]
        public void SlipGestures_DeliveryFollowsFinalResultOnly(string kind)
        {
            double end=Star(),b=end-1;
            var projection=new FrontisProjection(Matrix4x4.TRS(new Vector3(3.2f,3,10),Quaternion.identity,Vector3.one),Matrix4x4.Ortho(-3.2f,3.2f,-3,3,.1f,100),new Rect(0,0,640,600),new Rect(0,0,640,600),app.Config.Court,b,0);
            var model=new M24Model(SlingSettings.From(app.Tuning.Applied),projection,1);
            model.Push(new TouchSample(1,TouchPhaseKind.Began,new Vector2(320,220),b,1));model.Push(new TouchSample(1,TouchPhaseKind.Moved,new Vector2(320,185),b+.1,2));
            var point=kind=="lateral-slip"?new Vector2(350,217.9f):new Vector2(320,220);
            model.Push(new TouchSample(1,TouchPhaseKind.Ended,point,end,3));
            At(end);controller.Enqueue(new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,b,end,true,model.Outcome,model.Aim,model.P,app.Tuning.Applied));controller.ProcessPending();
            var r=controller.LastRecord;
            if(model.Outcome!=GestureOutcome.Swing||r==null){Assert.That(seen.Count,Is.Zero,"a closure that is not a final shot result never delivers");return;}
            Assert.That(seen.Count,Is.EqualTo(r.Applied||r.Result=="Failed"?1:0));
            if(seen.Count==1)Assert.That(seen[0].Category.ToString(),Is.EqualTo(r.Result));
        }
    }
}
