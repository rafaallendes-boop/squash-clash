using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    public sealed class SwingViewTests
    {
        private AppBootstrap app;
        private ShotController controller;
        private PracticeMode practice;
        private SwingView swing;
        private PerfectFlashView flash;
        private ShotNoteView note;
        private readonly List<ShotPresentation> seen=new List<ShotPresentation>();
        private readonly List<SwingIntent> intents=new List<SwingIntent>();
        private readonly List<SwingPhase> phaseAtIntent=new List<SwingPhase>();
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"S3 C fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);
            bool wired=app.GetComponent<ShotController>()!=null;
            controller=app.GetComponent<ShotController>()??app.gameObject.AddComponent<ShotController>();if(!wired)controller.Initialize(app);
            practice=app.GetComponent<PracticeMode>()??app.gameObject.AddComponent<PracticeMode>();if(!wired)practice.Initialize(app,controller);
            swing=controller.GetComponent<SwingView>();flash=controller.GetComponent<PerfectFlashView>();note=controller.GetComponent<ShotNoteView>();
            Assert.That(swing,Is.Not.Null);Assert.That(flash,Is.Not.Null);
            seen.Clear();intents.Clear();phaseAtIntent.Clear();controller.PresentationDelivered+=seen.Add;controller.SwingAccepted+=i=>{intents.Add(i);phaseAtIntent.Add(swing.Phase);};
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=practice.enabled=swing.enabled=flash.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return PlayModeTestWorld.ClearApp();}
        private void At(double target)
        {
            for(int i=0;i<1500&&app.Host.ObservedRealTime+StepReal<target-1e-10;i++)app.Host.Advance(StepReal);
            double left=target-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);
        }
        private double Star(){Assert.That(controller.Plan.Exists,Is.True);Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var t),Is.True);return t;}
        private ClosedGesture Order(double time,GestureOutcome outcome=GestureOutcome.Swing,string id=null,Vector2? aim=null,long? launch=null)
            =>new ClosedGesture(launch??app.Host.LaunchId,id??Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,time,true,outcome,aim??new Vector2(3.2f,2.2f),.5f,app.Host.ActiveConfig.Source,"fixture",time);
        private ShotRecord Submit(double time,Vector2? aim=null)
        {At(time);controller.Enqueue(Order(time,aim:aim));controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);return controller.LastRecord;}
        private void Exhaust(){for(int i=0;i<1200&&app.Host.IsRunning;i++)app.Host.Advance(StepReal);}
        private void Finish(ShotRecord r){while(app.Host.Current.Step<r.ContactStep+2)app.Host.Advance(StepReal);}
        private void LaunchWithSide(StrokeSide side)
        {
            foreach(double x in new[]{3.2,1.0,.6,.4,5.0,5.8,2.0,4.0,.8,1.5,2.6,3.8,4.6})
            {
                app.Tuning.Set("human.x",x);app.LaunchCurrent();seen.Clear();intents.Clear();phaseAtIntent.Clear();
                if(controller.Plan.Exists&&controller.Plan.Side==side)return;
            }
            Assert.Fail("no human.x produced a "+side+" plan");
        }
        private Vector3 Local(Vector3 world)=>app.GetComponentInChildren<PlaceholderPlayers>().Human.InverseTransformPoint(world);
        private static void Ticks(SwingView v,double total){for(double t=0;t<total-1e-9;t+=.01)v.Tick(.01);}
        private Vector3 IdealOffset(StrokeSide side){var b=controller.Snapshot.Body;var p=b.IdealContactPoint(side);return new Vector3(p.x-b.PositionXZ.x,p.y-.9f,p.z-b.PositionXZ.y);}
        [Test] public void Swing_DriveBackhandAndAir_ArePlayedProcedurally()
        {
            foreach(var side in new[]{StrokeSide.Drive,StrokeSide.Backhand})
            {
                LaunchWithSide(side);Assert.That(swing.Rig,Is.Not.Null);var idle=swing.RacketLocalPosition;
                var r=Submit(Star());Assert.That(r.Applied,Is.True,r.Reason);
                Assert.That(intents.Count,Is.EqualTo(1));Assert.That(intents[0].Side,Is.EqualTo(side));Assert.That(phaseAtIntent[0],Is.EqualTo(SwingPhase.Intention),"intention starts at acceptance");
                Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Impact));Assert.That(swing.Side,Is.EqualTo(side));
                Ticks(swing,SwingView.ReachSeconds);var ideal=IdealOffset(side);
                Assert.That(swing.RacketLocalPosition.x*Mathf.Sign(ideal.x),Is.GreaterThan(.3f),"racket on the "+side+" lateral side");
                Assert.That(Vector3.Distance(swing.RacketLocalPosition,ideal),Is.LessThan(.05f),"impact pose reaches the ideal contact point");
                Assert.That(Vector3.Distance(swing.RacketLocalPosition,idle),Is.GreaterThan(.3f));
            }
            LaunchWithSide(StrokeSide.Drive);float driveX=IdealOffset(StrokeSide.Drive).x;LaunchWithSide(StrokeSide.Backhand);Assert.That(IdealOffset(StrokeSide.Backhand).x*driveX,Is.LessThan(0),"opposite lateral sides");
            LaunchWithSide(StrokeSide.Drive);var f=Submit(Star()-.15);var o=IdealOffset(StrokeSide.Drive);
            Assert.That(f.Result,Is.EqualTo("Failed"));Assert.That(phaseAtIntent[0],Is.EqualTo(SwingPhase.Intention));Assert.That(swing.Phase,Is.EqualTo(SwingPhase.AirFollowThrough));
            for(int i=0;i<40;i++){swing.Tick(.01);Assert.That(swing.Phase,Is.Not.EqualTo(SwingPhase.Impact));Assert.That(Vector3.Distance(swing.RacketLocalPosition,o),Is.GreaterThan(.2f),"air never passes through the impact pose");}
        }
        [Test] public void Intention_StartsOncePerAcceptedOrder_AndWaitsForContactStep()
        {
            var r=Submit(Star()-.02);Assert.That(r.Scheduled,Is.True);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Intention));Assert.That(swing.StartCountFor(r.AttemptId),Is.EqualTo(1));
            controller.Enqueue(Order(r.ReleaseEventAtReal+.01));controller.ProcessPending();Assert.That(controller.LastDiscardReason,Is.EqualTo("attempt-consumed"));
            controller.Enqueue(Order(r.ReleaseEventAtReal+.01,id:r.AttemptId));controller.ProcessPending();
            Ticks(swing,.5);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Intention),"stays in intention until the contact is applied");
            while(app.Host.Current.Step<r.ContactStep-1){app.Host.Advance(StepReal);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Intention));}
            Finish(r);Assert.That(controller.LastRecord.Applied,Is.True);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Impact));
            Assert.That(swing.StartCountFor(r.AttemptId),Is.EqualTo(1));Assert.That(swing.TotalStarts,Is.EqualTo(1));Assert.That(intents.Count,Is.EqualTo(1));
        }
        [Test] public void NoSwing_ForCancelTapNoOrderOldLaunchAndDiscardedOrders()
        {
            controller.Enqueue(Order(app.Host.LaunchRealTime+.3,GestureOutcome.CanceledReturnToDeadZone));controller.ProcessPending();
            controller.Enqueue(Order(app.Host.LaunchRealTime+.3,GestureOutcome.Tap));controller.ProcessPending();
            controller.Enqueue(Order(app.Host.LaunchRealTime+.3,GestureOutcome.AbandonedNoPull));controller.ProcessPending();
            controller.Enqueue(Order(Star(),GestureOutcome.Tap,"dup"));controller.ProcessPending();controller.Enqueue(Order(Star(),id:"dup"));controller.ProcessPending();
            Assert.That(controller.LastDiscardReason,Is.EqualTo("duplicate-order"));
            var old=Order(Star());app.LaunchCurrent();controller.Enqueue(old);controller.ProcessPending();Assert.That(controller.LastDiscardReason,Is.EqualTo("wrong-launch"));
            Exhaust();practice.Tick(.001);Assert.That(controller.LastRecord.Result,Is.EqualTo("NoOrder"));
            Assert.That(intents.Count,Is.Zero);Assert.That(swing.TotalStarts,Is.Zero);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));
        }
        [Test] public void LateRejection_AfterAcceptance_ReturnsToIdle()
        {
            var r=Submit(Star(),new Vector2(8,2));Assert.That(r.Result,Is.EqualTo("SystemFault"));
            Assert.That(intents.Count,Is.EqualTo(1));Assert.That(phaseAtIntent[0],Is.EqualTo(SwingPhase.Intention));Assert.That(swing.StartCountFor(r.AttemptId),Is.EqualTo(1));
            Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));Ticks(swing,1);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));
            LaunchWithSide(StrokeSide.Drive);double end=Star();var order=Order(end);At(end+.3);controller.Enqueue(order);controller.ProcessPending();
            Assert.That(controller.LastRecord.Result,Is.EqualTo("NotEvaluable"));Assert.That(intents.Count,Is.EqualTo(1));Assert.That(swing.StartCountFor(order.AttemptId),Is.EqualTo(1));Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));
        }
        [Test] public void Relaunch_ClearsSwingAndFlashImmediately()
        {
            var r=Submit(Star());Assert.That(r.Result,Is.EqualTo("Perfect"));Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Impact));Assert.That(flash.IsVisible,Is.True);
            app.LaunchCurrent();Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));Assert.That(flash.IsVisible,Is.False);
            var s=Submit(Star()-.02);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Intention));app.LaunchCurrent();Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));
        }
        [Test] public void Flash_PerfectOnly_AtContactPosition_ExpiresAfterDuration()
        {
            Assert.That(SwingFlashDuration(),Is.LessThanOrEqualTo(.15f));
            var r=Submit(Star());Assert.That(r.Result,Is.EqualTo("Perfect"));Assert.That(flash.IsVisible,Is.True);Assert.That(flash.ShowCount,Is.EqualTo(1));
            Assert.That(Vector3.Distance(flash.Flash.position,seen[0].ContactPosition),Is.LessThan(1e-4f));
            flash.Tick(PerfectFlashView.DurationSeconds*.5);Assert.That(flash.IsVisible,Is.True);flash.Tick(PerfectFlashView.DurationSeconds);Assert.That(flash.IsVisible,Is.False);
        }
        private static float SwingFlashDuration()=>PerfectFlashView.DurationSeconds;
        private void AssertFrozenWhilePaused(SwingPhase phase)
        {
            app.Clock.Paused=true;
            try
            {
                var pos=swing.RacketLocalPosition;var rot=swing.RacketLocalRotation;
                for(int i=0;i<100;i++)swing.Tick(.01);
                Assert.That(swing.Phase,Is.EqualTo(phase));
                Assert.That(swing.RacketLocalPosition,Is.EqualTo(pos));Assert.That(swing.RacketLocalRotation,Is.EqualTo(rot));
            }
            finally{app.Clock.Paused=false;}
        }
        [Test] public void Swing_Intention_FreezesWhilePaused_AndResumes()
        {
            var r=Submit(Star()-.02);Assert.That(r.Scheduled,Is.True);Ticks(swing,.05);
            var pos=swing.RacketLocalPosition;AssertFrozenWhilePaused(SwingPhase.Intention);
            Ticks(swing,.2);Assert.That(swing.RacketLocalPosition,Is.Not.EqualTo(pos));Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Intention));
        }
        [Test] public void Swing_Impact_FreezesWhilePaused_AndFinishes()
        {
            var r=Submit(Star());Assert.That(r.Result,Is.EqualTo("Perfect"));Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Impact));Ticks(swing,.1);
            var pos=swing.RacketLocalPosition;AssertFrozenWhilePaused(SwingPhase.Impact);
            Ticks(swing,.05);Assert.That(swing.RacketLocalPosition,Is.Not.EqualTo(pos));
            Ticks(swing,2);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));
        }
        [Test] public void Swing_Air_FreezesWhilePaused_AndFinishes()
        {
            var r=Submit(Star()-.15);Assert.That(r.Result,Is.EqualTo("Failed"));Assert.That(swing.Phase,Is.EqualTo(SwingPhase.AirFollowThrough));Ticks(swing,.1);
            var pos=swing.RacketLocalPosition;AssertFrozenWhilePaused(SwingPhase.AirFollowThrough);
            Ticks(swing,.05);Assert.That(swing.RacketLocalPosition,Is.Not.EqualTo(pos));
            Ticks(swing,2);Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));
        }
        [Test] public void Flash_FreezesWhilePaused_AndExpiresAfterRemainingUnpausedTime()
        {
            var r=Submit(Star());Assert.That(r.Result,Is.EqualTo("Perfect"));Assert.That(flash.IsVisible,Is.True);
            flash.Tick(PerfectFlashView.DurationSeconds*.5);Assert.That(flash.IsVisible,Is.True);
            app.Clock.Paused=true;
            try{for(int i=0;i<100;i++)flash.Tick(.01);Assert.That(flash.IsVisible,Is.True);}finally{app.Clock.Paused=false;}
            flash.Tick(PerfectFlashView.DurationSeconds*.4);Assert.That(flash.IsVisible,Is.True,"remaining time not yet consumed");
            flash.Tick(PerfectFlashView.DurationSeconds*.2);Assert.That(flash.IsVisible,Is.False);
        }
        [Test] public void Relaunch_WhilePaused_ClearsSwingAndFlash()
        {
            var r=Submit(Star());Assert.That(r.Result,Is.EqualTo("Perfect"));Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Impact));Assert.That(flash.IsVisible,Is.True);
            app.Clock.Paused=true;
            try{app.LaunchCurrent();Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Idle));Assert.That(flash.IsVisible,Is.False);}finally{app.Clock.Paused=false;}
        }
        [Test] public void Flash_NeverOnValidFailedOrNoDelivery()
        {
            var v=Submit(Star()-.07);Assert.That(v.Result,Is.EqualTo("Valid"));Finish(v);Assert.That(controller.LastRecord.Applied,Is.True);Assert.That(flash.IsVisible,Is.False);Assert.That(flash.ShowCount,Is.Zero);
            app.LaunchCurrent();var f=Submit(Star()-.15);Assert.That(f.Result,Is.EqualTo("Failed"));Assert.That(flash.IsVisible,Is.False);Assert.That(flash.ShowCount,Is.Zero);
            app.LaunchCurrent();Submit(Star(),new Vector2(8,2));Assert.That(flash.IsVisible,Is.False);Assert.That(flash.ShowCount,Is.Zero);
        }
        [Test] public void Flash_IsNotGatedByShowNotes()
        {app.Tuning.Set("timing.showNotes",0);app.LaunchCurrent();var r=Submit(Star());Assert.That(r.Result,Is.EqualTo("Perfect"));Assert.That(flash.IsVisible,Is.True);}
        [Test] public void Notes_MatchResult_AndShowNotesOffHidesThem()
        {
            Assert.That(Submit(Star()).Result,Is.EqualTo("Perfect"));Assert.That(note.Text,Is.EqualTo("Perfecto"));
            app.LaunchCurrent();var e=Submit(Star()-.07);Assert.That(e.Result,Is.EqualTo("Valid"));Finish(e);Assert.That(note.Text,Is.EqualTo("Adelantado"));
            // The default launch has K_out within one assist window of T*, so a late Valid cannot be produced end to end; the late text is checked on the record mapping.
            note.Show(new ShotRecord{Result="Valid",DeltaMs=35});Assert.That(note.Text,Is.EqualTo("Atrasado"));note.Show(new ShotRecord{Result="Valid",DeltaMs=-35});Assert.That(note.Text,Is.EqualTo("Adelantado"));
            app.LaunchCurrent();var air=Submit(Star()-.15);Assert.That(air.Fail,Is.EqualTo("Air"));Assert.That(note.Text,Is.EqualTo("¡Al aire!"));
            app.LaunchCurrent();var late=Submit(Star()+.15);Assert.That(late.Fail,Is.EqualTo("Late"));Assert.That(note.Text,Is.EqualTo("¡Tarde!"));
            var label=app.GetComponent<UIDocument>().rootVisualElement.Q("shot-note");
            controller.enabled=true;app.Tuning.Set("timing.showNotes",1);controller.SendMessage("LateUpdate");Assert.That(label.style.display.value,Is.EqualTo(DisplayStyle.Flex));
            app.Tuning.Set("timing.showNotes",0);controller.SendMessage("LateUpdate");Assert.That(label.style.display.value,Is.EqualTo(DisplayStyle.None));
        }
        [Test] public void Views_HaveNoColliders_AndFollowPlaceholdersVisibility()
        {
            Submit(Star());Assert.That(swing.Rig,Is.Not.Null);Assert.That(flash.Flash,Is.Not.Null);
            Assert.That(swing.Rig.GetComponentsInChildren<Collider>(true).Length,Is.Zero);Assert.That(flash.Flash.GetComponentsInChildren<Collider>(true).Length,Is.Zero);
            Assert.That(swing.Rig.GetComponentsInChildren<Rigidbody>(true).Length,Is.Zero);Assert.That(swing.Rig.GetComponentInParent<Animator>(),Is.Null);
            Assert.That(swing.Rig.gameObject.activeInHierarchy,Is.True);
            app.Tuning.Set("placeholders.visible",0);Assert.That(swing.Rig.gameObject.activeInHierarchy,Is.False);
            app.Tuning.Set("placeholders.visible",1);Assert.That(swing.Rig.gameObject.activeInHierarchy,Is.True);
        }
        private static object[] Targets(object owner,string eventField)
        {
            var field=owner.GetType().GetField(eventField,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            Assert.That(field,Is.Not.Null,eventField+" backing field");
            return (field.GetValue(owner) as Delegate)?.GetInvocationList().Select(d=>d.Target).ToArray()??new object[0];
        }
        private void AssertViewSubscriptions(bool expected)
        {
            var delivered=Targets(controller,"PresentationDelivered");var launched=Targets(app.Host,"Launched");var accepted=Targets(controller,"SwingAccepted");var resolved=Targets(controller,"ShotResolved");
            foreach(var targets in new[]{delivered,launched,accepted,resolved})
                Assert.That(targets.Any(t=>t is SwingView||t is PerfectFlashView),Is.EqualTo(expected),expected?"views subscribed":"no view handler remains");
            Assert.That(delivered.Any(t=>t is SwingView),Is.EqualTo(expected));Assert.That(delivered.Any(t=>t is PerfectFlashView),Is.EqualTo(expected));
        }
        private void DestroyViews()
        {
            UnityEngine.Object.DestroyImmediate(swing);UnityEngine.Object.DestroyImmediate(flash);
            Assert.That(controller.GetComponent<SwingView>()==null,Is.True);Assert.That(controller.GetComponent<PerfectFlashView>()==null,Is.True);
            Assert.That(UnityEngine.Object.FindObjectsByType<SwingView>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,Is.Zero);
            Assert.That(UnityEngine.Object.FindObjectsByType<PerfectFlashView>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,Is.Zero);
        }
        private static string Bits(Vector3 v)=>BitConverter.SingleToInt32Bits(v.x)+","+BitConverter.SingleToInt32Bits(v.y)+","+BitConverter.SingleToInt32Bits(v.z);
        private static string FrameLine(SquashClash.Core.Simulation.BallState c,Vector3 render)=>c.Step+"|"+Bits(c.Position)+"|"+Bits(c.Velocity)+"|"+Bits(render);
        // Per-frame trace of the authoritative ball plus final canonical contacts and prediction states, all as raw float bits.
        private List<string> Record(bool tickViews,Action<ShotPresentation> perturb)
        {
            app.LaunchCurrent();var trace=new List<string>();
            if(perturb!=null)controller.PresentationDelivered+=perturb;
            try
            {
                var r=Submit(Star());Assert.That(r.Result,Is.EqualTo("Perfect"),r.Reason);Assert.That(r.Applied,Is.True,r.Reason);
                trace.Add("submit|"+FrameLine(app.Host.Current,app.Host.RenderPosition));
                for(int i=0;i<120;i++)
                {
                    app.Host.Advance(StepReal);if(tickViews){swing.Tick(.01);flash.Tick(.01);}
                    trace.Add(i+"|"+FrameLine(app.Host.Current,app.Host.RenderPosition));
                }
            }
            finally{if(perturb!=null)controller.PresentationDelivered-=perturb;}
            foreach(var e in app.Host.CanonicalContacts)trace.Add("contact|"+e.Step+"|"+e.Surface+"|"+Bits(e.Point)+"|"+Bits(e.Normal)+"|"+BitConverter.SingleToInt32Bits(e.TimeInStep));
            var p=app.Host.Prediction;trace.Add("prediction|"+p.States.Count+"|"+p.Contacts.Count+"|"+p.Exit+"|"+app.Host.PredictionMatchesBitwise);
            foreach(var s in p.States)trace.Add("pstate|"+FrameLine(s,Vector3.zero));
            foreach(var e in p.Contacts)trace.Add("pcontact|"+e.Step+"|"+e.Surface+"|"+Bits(e.Point));
            return trace;
        }
        private static List<string> Diff(List<string> a,List<string> b)
        {
            var d=new List<string>();if(a.Count!=b.Count)d.Add("count "+a.Count+" vs "+b.Count);
            for(int i=0;i<Math.Min(a.Count,b.Count)&&d.Count<5;i++)if(a[i]!=b[i])d.Add(i+": "+a[i]+" != "+b[i]);
            return d;
        }
        [Test] public void Ball_IsBitIdentical_WithViewsPresentAndAbsent()
        {
            AssertViewSubscriptions(true);var present=Record(true,null);
            DestroyViews();AssertViewSubscriptions(false);var absent=Record(false,null);
            Assert.That(present.Count(l=>l.StartsWith("contact|")),Is.GreaterThan(0),"trace includes canonical contacts");Assert.That(present.Count,Is.GreaterThan(240));
            Assert.That(Diff(present,absent),Is.Empty,"views present vs absent");
        }
        [Test] public void NegativeControl_ComparatorDetectsAViewThatPerturbsAuthoritativeState()
        {
            AssertViewSubscriptions(true);
            Action<ShotPresentation> perturbing=_=>app.Host.Advance(StepReal);
            var present=Record(true,perturbing);
            Assert.That(Targets(controller,"PresentationDelivered").Any(d=>d is SwingViewTests),Is.False,"perturbing subscriber removed after its run");
            DestroyViews();AssertViewSubscriptions(false);var absent=Record(false,null);
            Assert.That(Diff(present,absent),Is.Not.Empty,"the comparator must report the extra authoritative step");
        }
        [Test] public void Views_DoNotChangeCameraTempoOrSlowMotion()
        {
            var cam=UnityEngine.Camera.main;Assert.That(cam,Is.Not.Null);var pos=cam.transform.position;var rot=cam.transform.rotation;double tempo=app.Clock.Tempo,slow=app.Clock.DebugSlowMotion;
            var r=Submit(Star());Assert.That(r.Result,Is.EqualTo("Perfect"));for(int i=0;i<40;i++){swing.Tick(.01);flash.Tick(.01);}
            Assert.That(cam.transform.position,Is.EqualTo(pos));Assert.That(cam.transform.rotation,Is.EqualTo(rot));Assert.That(app.Clock.Tempo,Is.EqualTo(tempo));Assert.That(app.Clock.DebugSlowMotion,Is.EqualTo(slow));
        }
    }
}
