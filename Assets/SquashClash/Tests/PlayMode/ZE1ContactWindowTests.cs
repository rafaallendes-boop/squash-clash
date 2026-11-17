using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // E1: exercise real accepted orders and deliveries; compare the actual rendered racket transforms.
    public sealed class ZE1ContactWindowTests : ZFirstRunFixture
    {
        private const double ContactHalfWindow=.060;
        private const float WorldTolerance=.005f;
        private float worstDistance;private int contactSamples;
        private SwingView swing;private StrokeFx fx;private CharacterView human,rival;private bool wasLeft;
        [UnitySetUp] public IEnumerator Setup()
        {
            wasLeft=Handedness.Left;
            yield return Boot(false);
            swing=app.GetComponent<SwingView>();fx=app.GetComponent<StrokeFx>();
            human=app.Players.Human.Find("Figure").GetComponent<CharacterView>();
            rival=app.Players.Rival.Find("Figure").GetComponent<CharacterView>();
            yield return null;yield return null;
            swing.enabled=fx.enabled=human.enabled=rival.enabled=false;app.Clock.Paused=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return Shutdown();Handedness.Left=wasLeft;}
        private void LaunchWithPhysicalSide(StrokeSide side)
        {
            // Same practice fixture selection as SwingViewTests; cosmetic handedness never changes BodyFrame.
            foreach(double x in new[]{3.2,1.0,.6,.4,5.0,5.8,2.0,4.0,.8,1.5,2.6,3.8,4.6})
            {
                app.Tuning.Set("human.x",x);app.LaunchCurrent();
                if(controller.Plan.Exists&&controller.Plan.Side==side)return;
            }
            Assert.Fail("no practice position produced physical side "+side);
        }
        private void Deliver(StrokeSide physicalSide,float power)
        {
            LaunchWithPhysicalSide(physicalSide);
            Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out double release),Is.True);
            for(int i=0;i<1500&&app.Host.ObservedRealTime+StepReal<release-1e-10;i++)app.Host.Advance(StepReal);
            double remaining=release-app.Host.ObservedRealTime;if(remaining>0)app.Host.Advance(remaining);
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,"e1-contact-"+app.Host.LaunchId,1,app.Host.LaunchRealTime+.01,release,true,
                GestureOutcome.Swing,new Vector2(3.2f,2.2f),power,app.Host.ActiveConfig.Source,"E1 contact fixture",release));
            controller.ProcessPending();
            Assert.That(controller.LastRecord,Is.Not.Null);
            Assert.That(controller.LastRecord.Applied,Is.True,controller.LastRecord.Reason);
            Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Impact),"the test starts at a delivered contact, not at intention");
            Assert.That(swing.Side,Is.EqualTo(physicalSide));
        }
        private void AssertCoincident(double elapsed,bool left,bool backhand)
        {
            var drawn=human.transform.Find("Racket").position;
            var reference=swing.Rig.Find("Racket").position;
            float distance=Vector3.Distance(drawn,reference);worstDistance=Mathf.Max(worstDistance,distance);contactSamples++;
            Assert.That(distance,Is.LessThanOrEqualTo(WorldTolerance),
                $"{(left?"left":"right")}-handed {(backhand?"backhand":"forehand")}, {elapsed*1000:F3} ms after delivery: drawn head must track SwingView within 5 mm");
            Assert.That(human.RacketHand,Is.EqualTo(left?-1:1));
        }
        [TestCase(false,false)]
        [TestCase(false,true)]
        [TestCase(true,false)]
        [TestCase(true,true)]
        public void HumanRacket_CoincidesThroughoutContactWindow_ForEitherHandAndStroke(bool left,bool backhand)
        {
            Handedness.Left=left;worstDistance=0;contactSamples=0;
            // Core's physical Drive is on -x; the cosmetic left hand reverses the visual stroke names only.
            var physicalSide=left==backhand?StrokeSide.Drive:StrokeSide.Backhand;
            // SwingView reaches its contact key 60 ms after delivery. +/-60 ms therefore covers [0,120] ms,
            // including the complete reach and the first 60 ms of follow-through. Sample both boundaries and centre.
            double centre=SwingView.ReachSeconds,end=centre+ContactHalfWindow;
            Assert.That(centre-ContactHalfWindow,Is.EqualTo(0).Within(1e-8));
            foreach(float power in new[]{.1f,1f})
            foreach(double frame in new[]{1d/30,1d/60,1d/120,.050})
            {
                Deliver(physicalSide,power);
                human.Animate(0);AssertCoincident(0,left,backhand);
                double elapsed=0;
                while(elapsed<end-1e-9)
                {
                    double boundary=elapsed<centre-1e-9?centre:end;
                    double dt=Math.Min(frame,boundary-elapsed);
                    swing.Tick(dt);fx.Tick((float)dt);human.Animate((float)dt);elapsed+=dt;
                    AssertCoincident(elapsed,left,backhand);
                }
            }
            TestContext.WriteLine($"E1 contact samples={contactSamples}, worst distance={worstDistance:R} m, left={left}, backhand={backhand}");
        }
        private static object Read(object target,string field)=>target.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
        private static void Write(object target,string field,object value)=>target.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
        private static void Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        private Vector3 DrawnHead=>human.transform.Find("Racket").position;
        private Vector3 ReferenceHead=>swing.Rig.Find("Racket").position;
        private void BeginViewStroke(StrokeSide side,string attempt)
        {
            var intent=new SwingIntent(app.Host.LaunchId,attempt,side);
            // Deliver only the two views' wired listeners, in the real subscription order. No game state is edited.
            var listeners=(Action<SwingIntent>)Read(controller,"SwingAccepted");
            foreach(Action<SwingIntent> listener in listeners.GetInvocationList())
                if(ReferenceEquals(listener.Target,swing)||ReferenceEquals(listener.Target,human))listener(intent);
        }
        private ShotPresentation ViewContact(StrokeSide side,string attempt)=>new ShotPresentation(app.Host.LaunchId,attempt,PresentationCategory.Perfect,1,null,0,true,0,
            controller.Snapshot.Body.IdealContactPoint(side),side.ToString());
        private void DeliverView(ShotPresentation presentation)
        {
            var listeners=(Action<ShotPresentation>)Read(controller,"PresentationDelivered");
            foreach(Action<ShotPresentation> listener in listeners.GetInvocationList())
                if(ReferenceEquals(listener.Target,swing)||ReferenceEquals(listener.Target,human))listener(presentation);
        }
        private struct TransitionMetrics
        {
            public int EntrySamples,ExitSamples;
            public float EntryStep,EntryExtra,ExitStep,ExitExcess,ExitSpeed;
        }
        private void MeasureFrame(double dt,ref Vector3 previousDrawn,ref Vector3 previousReference,bool entry,ref TransitionMetrics metrics)
        {
            swing.Tick(dt);human.Animate((float)dt);
            var drawnDelta=DrawnHead-previousDrawn;var referenceDelta=ReferenceHead-previousReference;
            float step=drawnDelta.magnitude,extra=(drawnDelta-referenceDelta).magnitude;
            if(entry)
            {
                metrics.EntrySamples++;metrics.EntryStep=Mathf.Max(metrics.EntryStep,step);metrics.EntryExtra=Mathf.Max(metrics.EntryExtra,extra);
            }
            else
            {
                metrics.ExitSamples++;metrics.ExitStep=Mathf.Max(metrics.ExitStep,step);
                metrics.ExitExcess=Mathf.Max(metrics.ExitExcess,step-8*(float)dt);
                if(dt>0)metrics.ExitSpeed=Mathf.Max(metrics.ExitSpeed,step/(float)dt);
            }
            previousDrawn=DrawnHead;previousReference=ReferenceHead;
        }
        [TestCase(false,false)]
        [TestCase(false,true)]
        [TestCase(true,false)]
        [TestCase(true,true)]
        public void ContactTransitions_AreContinuousForEveryFrameStep(bool left,bool backhand)
        {
            Handedness.Left=left;var failures=new List<string>();
            var side=left==backhand?StrokeSide.Drive:StrokeSide.Backhand;
            foreach(double frame in new[]{1d/30,1d/60,1d/120,.050})
            foreach(double intention in new[]{.025,.30})
            foreach(float power in new[]{.1f,1f})
            {
                LaunchWithPhysicalSide(side);Write(human,"strokePower",power);
                // Include the real resting drawing before acceptance, rather than starting after its first snap.
                for(int i=0;i<40;i++)human.Animate(1f/60);
                var previousDrawn=DrawnHead;var previousReference=ReferenceHead;var metrics=new TransitionMetrics();
                string attempt="e1-continuity-"+app.Host.LaunchId;BeginViewStroke(side,attempt);
                Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Intention));
                // With dt=0, a phase change cannot borrow a large movement allowance from a long frame.
                MeasureFrame(0,ref previousDrawn,ref previousReference,true,ref metrics);
                double elapsed=0;
                while(elapsed<intention-1e-9)
                {
                    double dt=Math.Min(frame,intention-elapsed);
                    MeasureFrame(dt,ref previousDrawn,ref previousReference,true,ref metrics);elapsed+=dt;
                }
                DeliverView(ViewContact(side,attempt));Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Impact));
                MeasureFrame(0,ref previousDrawn,ref previousReference,true,ref metrics);
                elapsed=0;
                while(elapsed<.62-1e-9)
                {
                    double boundary=elapsed<.12-1e-9?.12:.62;
                    double dt=Math.Min(frame,boundary-elapsed);
                    MeasureFrame(dt,ref previousDrawn,ref previousReference,elapsed<.12-1e-9,ref metrics);elapsed+=dt;
                    if(Math.Abs(elapsed-.12)<1e-8)MeasureFrame(0,ref previousDrawn,ref previousReference,false,ref metrics);
                }
                string identity=$"left={left} backhand={backhand} frame={frame:R} intention={intention:R} power={power:R}";
                TestContext.WriteLine($"E1 continuity {identity} entrySamples={metrics.EntrySamples} exitSamples={metrics.ExitSamples} entryStep={metrics.EntryStep:R} entryExtra={metrics.EntryExtra:R} exitStep={metrics.ExitStep:R} exitExcess={metrics.ExitExcess:R} exitSpeed={metrics.ExitSpeed:R}");
                if(metrics.EntryExtra>WorldTolerance||metrics.ExitExcess>WorldTolerance)
                    failures.Add($"{identity}: entryExtra={metrics.EntryExtra:R} m; exitExcess={metrics.ExitExcess:R} m");
            }
            // Assert only after collecting every power, preparation duration and frame step for this hand/stroke.
            Assert.That(failures,Is.Empty,string.Join("\n",failures));
        }
        [Test] public void UnknownContactAge_DoesNotKeepTheWindowOpenForTheWholeImpact()
        {
            Deliver(StrokeSide.Backhand,.8f);human.Animate(0);swing.Tick(.20);
            Write(human,"contactAge",-1f);human.Animate(.20f);
            Assert.That(human.TrailVisible,Is.True,"a missing local age must fail closed instead of suppressing the trail for all of Impact");
        }
        [Test] public void HumanRacketScale_DoesNotPopAtWindowExit()
        {
            Deliver(StrokeSide.Backhand,1);human.Animate(0);
            var scale=human.transform.Find("Racket").localScale;
            for(int i=0;i<20;i++)
            {
                swing.Tick(.01);fx.Tick(.01f);human.Animate(.01f);
                Assert.That(Vector3.Distance(human.transform.Find("Racket").localScale,scale),Is.LessThan(1e-6f),"the human head keeps a constant scale through contact and exit");
            }
        }
        [Test] public void RejectedDeliveries_DoNotReactivateTheContactWindow()
        {
            LaunchWithPhysicalSide(StrokeSide.Backhand);const string attempt="e1-delivery-filter";
            BeginViewStroke(StrokeSide.Backhand,attempt);swing.Tick(.15);human.Animate(.15f);
            var valid=ViewContact(StrokeSide.Backhand,attempt);
            var wrong=ViewContact(StrokeSide.Backhand,"other-attempt");
            DeliverView(wrong);Assert.That((float)Read(human,"contactAge"),Is.LessThan(0),"wrong attempt during Intention");
            DeliverView(valid);human.Animate(0);swing.Tick(.20);human.Animate(.20f);
            float age=(float)Read(human,"contactAge");Assert.That(age,Is.GreaterThan(.12f));
            DeliverView(valid);Assert.That((float)Read(human,"contactAge"),Is.EqualTo(age),"duplicate after SwingView has already entered Impact");
            swing.Tick(1);human.Animate(1);age=(float)Read(human,"contactAge");
            DeliverView(valid);Assert.That((float)Read(human,"contactAge"),Is.EqualTo(age),"same identity after SwingView has returned to Idle");
        }
        [TestCase(StrokeSide.Drive)]
        [TestCase(StrokeSide.Backhand)]
        public void RivalPoseAndArc_UseTheDeliveredSideEvenNearTheBodyCentre(StrokeSide side)
        {
            // Place the visual body at the current ball x so the legacy dead zone retains the contradictory sign.
            var player=app.Players.Rival;var position=player.position;position.x=app.Host.RenderPosition.x;player.position=position;
            float expected=side==StrokeSide.Drive?-1:1;Write(rival,"racketSign",-expected);
            Call(rival,"OnRivalSwing",side,true);Call(fx,"OnRivalSwing",side,true);fx.Tick(.08f);rival.Animate(.01f);
            var arc=fx.transform.Find("Rival stroke arc").GetComponent<LineRenderer>();
            Assert.That(arc.enabled,Is.True);
            Assert.That(rival.RacketSign,Is.EqualTo(expected),"pose must use the event's side, never the ball-x dead zone");
            Assert.That(rival.RacketHand,Is.EqualTo(1));
            var delta=arc.GetPosition(arc.positionCount-1)-arc.GetPosition(0);
            Assert.That(delta.x*expected,side==StrokeSide.Backhand?Is.GreaterThan(.20f):Is.LessThan(-.20f),"arc must show the same stroke as the fixed-hand figure");
        }
        [TestCase(false,false)]
        [TestCase(false,true)]
        [TestCase(true,false)]
        [TestCase(true,true)]
        public void StrokeArc_SweepsInTheVisualStrokeDirection_AndFreezesAndExpires(bool left,bool backhand)
        {
            Handedness.Left=left;
            var physicalSide=left==backhand?StrokeSide.Drive:StrokeSide.Backhand;
            Deliver(physicalSide,.8f);fx.Tick(.08f);
            var child=fx.transform.Find("Human stroke arc");
            Assert.That(child,Is.Not.Null,"a pooled crescent makes the stroke side readable without labels");
            var arc=child.GetComponent<LineRenderer>();Assert.That(arc.enabled,Is.True);
            float sign=swing.ShoulderSign;
            var start=arc.GetPosition(0);var end=arc.GetPosition(arc.positionCount-1);
            Assert.That((end.x-start.x)*sign,backhand?Is.GreaterThan(.20f):Is.LessThan(-.20f),
                "backhand opens out toward the contact side; forehand crosses toward the centre");
            var contact=new Vector3(controller.LastRecord.ContactX,controller.LastRecord.ContactY,controller.LastRecord.ContactZ);
            for(int i=0;i<arc.positionCount;i++)
                Assert.That(arc.GetPosition(i).y,Is.LessThan(contact.y-.10f),"the crescent leaves a clear gap around the contact");
            app.Clock.Paused=true;fx.Tick(.5f);
            Assert.That(arc.GetPosition(0),Is.EqualTo(start));Assert.That(arc.GetPosition(arc.positionCount-1),Is.EqualTo(end));
            Assert.That(arc.enabled,Is.True);app.Clock.Paused=false;
            fx.Tick(.5f);Assert.That(arc.enabled,Is.False);
            Deliver(physicalSide,.8f);fx.Tick(.08f);
            Assert.That(fx.transform.Find("Human stroke arc"),Is.EqualTo(child),"reused for every stroke");
            Assert.That(child.GetComponentsInChildren<Collider>(true),Is.Empty);
        }
    }
}
