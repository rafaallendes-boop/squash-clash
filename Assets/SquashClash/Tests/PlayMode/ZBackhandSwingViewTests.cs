using System.Collections;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Backhand pose on the human's real SwingView path. A backhand is the stroke whose racket arm reaches across the body:
    // the contact on the left for the right-hander, on the right for the left-hander (cosmetic hand). Rig heights are
    // relative to the capsule centre (+0.9 m above the floor).
    public sealed class ZBackhandSwingViewTests : ZFirstRunFixture
    {
        private SwingView swing;private StrokeFx fx;private CharacterView human;private bool wasLeft;
        [UnitySetUp] public IEnumerator Setup()
        {
            wasLeft=Handedness.Left;yield return Boot(false);
            swing=app.GetComponent<SwingView>();fx=app.GetComponent<StrokeFx>();human=app.Players.Human.Find("Figure").GetComponent<CharacterView>();
            yield return null;yield return null;swing.enabled=fx.enabled=human.enabled=false;app.Clock.Paused=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return Shutdown();Handedness.Left=wasLeft;}
        private void LaunchWithPhysicalSide(StrokeSide side)
        {
            foreach(double x in new[]{3.2,1.0,.6,.4,5.0,5.8,2.0,4.0,.8,1.5,2.6,3.8,4.6})
            {app.Tuning.Set("human.x",x);app.LaunchCurrent();if(controller.Plan.Exists&&controller.Plan.Side==side)return;}
            Assert.Fail("no practice position produced physical side "+side);
        }
        private static StrokeSide Drawn(bool left)=>left?StrokeSide.Drive:StrokeSide.Backhand;
        [TestCase(false)][TestCase(true)]
        public void Intention_ArmsTheRacketHighBehindTheContactSideShoulder_OutsideTheTorso(bool left)
        {
            Handedness.Left=left;LaunchWithPhysicalSide(Drawn(left));
            typeof(SwingView).GetMethod("OnAccepted",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(swing,new object[]{new SwingIntent(app.Host.LaunchId,"bh-prep",Drawn(left))});
            for(int i=0;i<30;i++)swing.Tick(.01);
            Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Intention));
            var head=swing.RacketLocalPosition;
            Assert.That(head.y+.9f,Is.GreaterThanOrEqualTo(1.60f),"head above the shoulder");
            Assert.That(head.x*swing.ShoulderSign,Is.GreaterThanOrEqualTo(.35f),"contact side, outside the torso silhouette");
        }
        [TestCase(false)][TestCase(true)]
        public void Impact_FinishesHighOnTheHittingArmSide(bool left)
        {
            Handedness.Left=left;LaunchWithPhysicalSide(Drawn(left));
            Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out double release),Is.True);
            double step=1d/app.Config.Physics.StepHz/app.Config.Tempo;
            for(int i=0;i<1500&&app.Host.ObservedRealTime+step<release-1e-10;i++)app.Host.Advance(step);
            double remaining=release-app.Host.ObservedRealTime;if(remaining>0)app.Host.Advance(remaining);
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,"bh-finish",1,app.Host.LaunchRealTime+.01,release,true,GestureOutcome.Swing,new Vector2(3.2f,2.2f),.8f,app.Host.ActiveConfig.Source,"backhand fixture",release));
            controller.ProcessPending();
            Assert.That(swing.Phase,Is.EqualTo(SwingPhase.Impact));
            for(int i=0;i<30;i++)swing.Tick(.01);
            var head=swing.RacketLocalPosition;
            Assert.That(head.x*swing.ShoulderSign,Is.LessThanOrEqualTo(-.25f),"crossed the body to the hitting-arm side");
            Assert.That(head.y+.9f,Is.GreaterThanOrEqualTo(1.50f),"and finishing high (above the shoulder)");
            // The drawn figure reaches the same high, open finish before it returns to rest.
            float best=0,x=0;for(int i=0;i<60;i++){swing.Tick(.005);human.Animate(.005f);var p=human.RacketLocalPosition;if(p.y>best){best=p.y;x=p.x;}}
            Assert.That(best,Is.GreaterThanOrEqualTo(1.60f));Assert.That(x*swing.ShoulderSign,Is.LessThanOrEqualTo(-.10f));
        }
        [Test] public void WaitingForABackhand_TheRacketIsAlreadyRaisedAndCocked()
        {
            Handedness.Left=false;LaunchWithPhysicalSide(StrokeSide.Backhand);
            for(int i=0;i<100;i++)swing.Tick(.01);
            Assert.That(swing.RacketLocalPosition.y+.9f,Is.GreaterThanOrEqualTo(1.60f),"armed while the planned stroke is a backhand");
            LaunchWithPhysicalSide(StrokeSide.Drive);
            for(int i=0;i<100;i++)swing.Tick(.01);
            Assert.That(swing.RacketLocalPosition.y+.9f,Is.LessThan(1.0f),"the right-hander's forehand wait is unchanged");
        }
    }
}
