using System;
using System.Collections;
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
            Assert.That(Vector3.Distance(drawn,reference),Is.LessThanOrEqualTo(WorldTolerance),
                $"{(left?"left":"right")}-handed {(backhand?"backhand":"forehand")}, {elapsed*1000:F3} ms after delivery: drawn head must track SwingView within 5 mm");
            Assert.That(human.RacketHand,Is.EqualTo(left?-1:1));
        }
        [TestCase(false,false)]
        [TestCase(false,true)]
        [TestCase(true,false)]
        [TestCase(true,true)]
        public void HumanRacket_CoincidesThroughoutContactWindow_ForEitherHandAndStroke(bool left,bool backhand)
        {
            Handedness.Left=left;
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
        }
    }
}
