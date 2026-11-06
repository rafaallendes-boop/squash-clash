using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Feedback;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests
{
    // Late rejections: the attempt was accepted (intention pose shown) and the contact is rejected later.
    // Host-level tests (S3LateRejectionHostTests) prove the host really emits the rejection results;
    // controller-level tests (S3LateRejectionControllerTests) prove controller + views react to them.
    public sealed class S3LateRejectionHostTests
    {
        private readonly List<GameObject> roots=new List<GameObject>();
        private EffectiveConfig config;
        private double StepReal=>1d/config.Physics.StepHz/config.Tempo;
        [UnitySetUp] public IEnumerator Setup(){yield return PlayModeTestWorld.ClearApp();config=EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));}
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var root in roots)Object.Destroy(root);roots.Clear();yield return null;}
        private BallSimulationHost Host()
        {
            var root=new GameObject("Late rejection host");roots.Add(root);var host=root.AddComponent<BallSimulationHost>();host.enabled=false;
            host.Initialize(new IBallSimulationFactory[]{new AnalyticBallSimulationFactory(0)},new SimClock(config.Physics.StepHz,config.Tempo));host.ConfigureShotHistory(250);
            host.Launch(config,new BallState{Position=new Vector3(3.2f,1.2f,3),Velocity=new Vector3(0,0,-6)},1200,StopCondition.SecondFloorBounce);
            return host;
        }
        private void To(BallSimulationHost host,long step){for(int i=0;i<1500&&host.Current.Step<step&&host.IsRunning;i++)host.Advance(StepReal);Assert.That(host.Current.Step,Is.EqualTo(step));}
        [Test] public void ScheduledShot_NonFiniteVelocity_EmitsInvalidVelocityAtContactStep_AndKeepsTrajectory()
        {
            var h=Host();var control=Host();To(h,20);To(control,20);
            var results=new List<ShotApplyResult>();h.ShotApplied+=results.Add;
            Assert.That(h.ScheduleShot(h.LaunchId,25,s=>new Vector3(float.NaN,0,0)),Is.True);
            To(h,24);Assert.That(results,Is.Empty);
            To(h,25);
            Assert.That(results.Count,Is.EqualTo(1));Assert.That(results[0].Applied,Is.False);Assert.That(results[0].Reason,Is.EqualTo("invalid-velocity"));Assert.That(results[0].ContactStep,Is.EqualTo(25));
            To(h,40);To(control,40);
            Assert.That(results.Count,Is.EqualTo(1),"emitted once");
            Assert.That(h.OutgoingBounces,Is.Zero);Assert.That(h.CanonicalContacts.Count,Is.EqualTo(control.CanonicalContacts.Count));
            Assert.That(h.Current.Position,Is.EqualTo(control.Current.Position));Assert.That(h.Current.Velocity,Is.EqualTo(control.Current.Velocity));
        }
        // This test only checks that a regular scheduled shot is not rejected as present-not-mappable; it does not prove
        // that the reason is unreachable. That claim rests on reading the code: in Advance the scheduled callback runs at
        // current.Step==contactStep after ObserveClock already marked the end of the frame, so ObservedRealTime always maps
        // to a present step >= contactStep. Without a product seam only the reflection-based controller test below covers it.
        [Test] public void RegularScheduledShot_IsNotRejectedAsPresentNotMappable()
        {
            var h=Host();To(h,20);var results=new List<ShotApplyResult>();h.ShotApplied+=results.Add;
            Assert.That(h.ScheduleShot(h.LaunchId,25,s=>new Vector3(0,6,-15)),Is.True);
            for(int i=0;i<4;i++){h.Advance(StepReal*3.5);}
            Assert.That(results.Count,Is.EqualTo(1));Assert.That(results[0].Reason,Is.Not.EqualTo("present-not-mappable"));
        }
    }
    public sealed class S3LateRejectionControllerTests
    {
        private AppBootstrap app;private ShotController controller;private PracticeMode practice;private SwingView swing;private PerfectFlashView flash;private ShotFeedback feedback;private RecordingHaptics haptics;
        private readonly List<ShotPresentation> seen=new List<ShotPresentation>();
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"S3 late rejection fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);
            bool wired=app.GetComponent<ShotController>()!=null;
            controller=app.GetComponent<ShotController>()??app.gameObject.AddComponent<ShotController>();if(!wired)controller.Initialize(app);
            practice=app.GetComponent<PracticeMode>()??app.gameObject.AddComponent<PracticeMode>();if(!wired)practice.Initialize(app,controller);
            swing=controller.GetComponent<SwingView>();flash=controller.GetComponent<PerfectFlashView>();feedback=controller.GetComponent<ShotFeedback>();
            haptics=new RecordingHaptics();Haptics.Backend=haptics;
            seen.Clear();controller.PresentationDelivered+=seen.Add;
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=practice.enabled=swing.enabled=flash.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){Haptics.ResetBackend();yield return PlayModeTestWorld.ClearApp();}
        private void At(double target)
        {
            for(int i=0;i<1500&&app.Host.ObservedRealTime+StepReal<target-1e-10;i++)app.Host.Advance(StepReal);
            double left=target-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);
        }
        private double Star(){Assert.That(controller.Plan.Exists,Is.True);Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var t),Is.True);return t;}
        private ClosedGesture Order(double time)
            =>new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,time,true,GestureOutcome.Swing,new Vector2(3.2f,2.2f),.5f,app.Host.ActiveConfig.Source,"fixture",time);
        private ShotRecord Submit(double time){At(time);controller.Enqueue(Order(time));controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);return controller.LastRecord;}
        private static void Ticks(SwingView v,double total){for(double t=0;t<total-1e-9;t+=.01)v.Tick(.01);}
        private void AssertSilentAndIdle(ShotRecord r,string reason,int swishBefore,int contactBefore)
        {
            Assert.That(r.Result,Is.EqualTo("SystemFault"));Assert.That(r.Reason,Is.EqualTo(reason));Assert.That(r.Applied,Is.False);
            Assert.That(seen.Count,Is.Zero);Assert.That(controller.PresentationCount,Is.Zero);
            Assert.That(feedback.ContactCount,Is.EqualTo(contactBefore));Assert.That(feedback.SwishCount,Is.EqualTo(swishBefore));
            Assert.That(haptics.Firm,Is.Zero);Assert.That(haptics.Discreet,Is.Zero);
            Assert.That(flash.ShowCount,Is.Zero);Assert.That(flash.IsVisible,Is.False);
            Assert.That(swing.StartCountFor(r.AttemptId),Is.EqualTo(1));Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));
            Ticks(swing,.5);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));
        }
        [Test] public void ScheduleRejected_EndToEnd_FaultsWithoutPresentationAndReturnsToIdle()
        {
            long reserved=controller.Plan.TStarStep+100;
            Assert.That(app.Host.ScheduleShot(app.Host.LaunchId,reserved,s=>Vector3.zero),Is.True,"test reserves the host so the controller's schedule is refused");
            int swish=feedback.SwishCount,contact=feedback.ContactCount;
            var r=Submit(Star()-.02);
            AssertSilentAndIdle(r,"schedule-rejected",swish,contact);
            Assert.That(controller.WaitingForContact,Is.False);
        }
        private void RaiseRejection(string reason,long contactStep)
        {
            var field=typeof(BallSimulationHost).GetField("ShotApplied",BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(field,Is.Not.Null,"event backing field");
            var handler=(Action<ShotApplyResult>)field.GetValue(app.Host);Assert.That(handler,Is.Not.Null);
            handler(new ShotApplyResult(false,reason,contactStep));
        }
        private void ScheduledThenRejected(string reason)
        {
            int swish=feedback.SwishCount,contact=feedback.ContactCount;
            var r=Submit(Star()-.035);Assert.That(r.Scheduled,Is.True);Assert.That(controller.WaitingForContact,Is.True);
            int intentionFrames=0;
            for(int i=0;i<3&&app.Host.Current.Step<r.ContactStep-1;i++){app.Host.Advance(StepReal);Ticks(swing,.01);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Intention));Assert.That(seen.Count,Is.Zero);intentionFrames++;}
            Assert.That(intentionFrames,Is.GreaterThanOrEqualTo(1),"intention must be checked on at least one host frame before contact");
            Assert.That(app.Host.Current.Step,Is.LessThan(r.ContactStep));
            // Host-level tests prove the host emits these results; here the event is raised in test code only.
            RaiseRejection(reason,r.ContactStep);
            AssertSilentAndIdle(controller.LastRecord,reason,swish,contact);Assert.That(controller.LastRecord,Is.SameAs(r));
            Assert.That(controller.WaitingForContact,Is.False);
            // Later host frames: the controller no longer waits, so nothing is delivered for this attempt.
            while(app.Host.Current.Step<r.ContactStep+3&&app.Host.IsRunning)app.Host.Advance(StepReal);
            Assert.That(controller.LastRecord.Result,Is.EqualTo("SystemFault"));Assert.That(controller.LastRecord.Applied,Is.False);
            Assert.That(seen.Count,Is.Zero);Assert.That(controller.PresentationCount,Is.Zero);Assert.That(feedback.ContactCount,Is.EqualTo(contact));
            Assert.That(haptics.Firm,Is.Zero);Assert.That(haptics.Discreet,Is.Zero);Assert.That(flash.ShowCount,Is.Zero);
            Ticks(swing,.5);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));Assert.That(swing.StartCountFor(r.AttemptId),Is.EqualTo(1));
        }
        [Test] public void ScheduledOrder_InvalidVelocityRejection_FaultsSilentlyAndReturnsToIdle(){ScheduledThenRejected("invalid-velocity");}
        [Test] public void ScheduledOrder_PresentNotMappableRejection_FaultsSilentlyAndReturnsToIdle(){ScheduledThenRejected("present-not-mappable");}
    }
}
