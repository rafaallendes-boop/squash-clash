using System;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class SlingModelTests
    {
        private static SlingModel New(SlingSettings? s=null,Rect? area=null)=>new SlingModel(s??new SlingSettings(holdMs:120),CourtDimensions.Standard(),3.2f,area??new Rect(0,0,200,300),1);
        private static TouchSample T(double t,float x,float y,TouchPhaseKind phase=TouchPhaseKind.Moved,int id=1)=>new TouchSample(id,phase,new Vector2(x,y),t,0);
        private static void Begin(SlingModel m)=>m.Push(T(0,100,100,TouchPhaseKind.Began));
        private static void Lock(SlingModel m,float y=103)
        {Begin(m);m.Push(T(.02,100,y));foreach(var t in new[]{.04,.08,.12,.14})m.Push(T(t,100,y,TouchPhaseKind.Stationary));}
        [Test] public void QuietRequiresRealDuration_AndFreezesOnce()
        {var m=New();Begin(m);m.Push(T(.02,100,103));m.Push(T(.06,100,103));m.Push(T(.10,100,103));Assert.That(m.Locked,Is.False);m.Push(T(.14,100,103));Assert.That(m.Locked,Is.True);var a=m.Aim;m.Push(T(.16,130,40));Assert.That(m.Aim,Is.EqualTo(a));Assert.That(m.Transitions.Count(x=>x.Kind=="Locked"),Is.EqualTo(1));}
        [Test] public void ReleaseAloneDoesNotCreateLock_WithoutPriorTick()
        {var m=New();Begin(m);m.Push(T(.02,100,103));m.Push(T(.30,100,103,TouchPhaseKind.Ended));Assert.That(m.Locked,Is.False);Assert.That(m.Outcome.ToString(),Is.EqualTo("AbandonedBeforeFix"));}
        [Test] public void TapAndLongHoldWithoutDrag_DoNotLock()
        {var m=New();Begin(m);m.Push(T(.13,100,100,TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Tap));m=New();Begin(m);for(int i=1;i<=10;i++)m.Push(T(i*.04,100,100));Assert.That(m.Locked,Is.False);}
        [Test] public void ToleranceAllowsSmallJitter_ButResetsOnFastMotion()
        {var m=New();Begin(m);m.Push(T(.02,100,103));m.Push(T(.06,100.2f,103));m.Push(T(.10,100.4f,103));m.Push(T(.14,100.2f,103));Assert.That(m.Locked,Is.True);m=New();Begin(m);m.Push(T(.02,100,103));m.Push(T(.06,100,103));m.Push(T(.10,100,105));m.Push(T(.14,100,105));Assert.That(m.Locked,Is.False);}
        [Test] public void PowerIsZeroBeforeLock_IncludingDownwardAim()
        {var m=New();Begin(m);m.Push(T(.02,100,60));Assert.That(m.P,Is.Zero);Assert.That(m.Locked,Is.False);Assert.That(m.Aim.y,Is.LessThan(2.2));}
        [TestCase(0,0)] [TestCase(2,0)] [TestCase(18.5f,.5f)] [TestCase(35,1)] [TestCase(60,1)]
        public void PullUsesCurrentDownwardDistance_DeadZoneAndSaturation(float distance,float power)
        {var m=New();Lock(m);m.Push(T(.16,100,103-distance));m.Push(T(.18,100,103-distance,TouchPhaseKind.Ended));Assert.That(m.P,Is.EqualTo(power).Within(1e-6));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Swing));}
        [Test] public void LateralDoesNotCharge_AndReturningUpReduces()
        {var m=New();Lock(m);m.Push(T(.16,110,103));Assert.That(m.P,Is.Zero);m.Push(T(.18,110,68));Assert.That(m.P,Is.EqualTo(1));m.Push(T(.20,110,102));Assert.That(m.P,Is.Zero);}
        [Test] public void ReturnToBeganAfterHighAimIsValidPull_NotLegacyN1()
        {var m=New();Lock(m,115);m.Push(T(.16,100,100,TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Swing));Assert.That(m.P,Is.GreaterThan(.25).And.LessThan(.6));}
        [Test] public void CancelOnlyOnReleaseInside_PassingThroughDoesNotLatch()
        {var m=New();Lock(m);m.Push(T(.16,100,115));Assert.That(m.Phase,Is.EqualTo(SlingPhase.CancelZone));Assert.That(m.Active,Is.True);m.Push(T(.18,100,80));m.Push(T(.20,100,80,TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Swing));m=New();Lock(m);m.Push(T(.16,100,115,TouchPhaseKind.Ended));Assert.That(m.Outcome.ToString(),Is.EqualTo("ZoneCanceled"));}
        [Test] public void NeutralAboveLock_IsZeroWithoutCancel()
        {var m=New();Lock(m);m.Push(T(.16,100,110,TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Swing));Assert.That(m.P,Is.Zero);}
        [Test] public void TopSpaceUsesLateralFallback_AndNoSpacePreventsLock()
        {var m=New(area:new Rect(0,0,200,120));Lock(m);Assert.That(m.Region,Is.EqualTo(SlingCancelRegion.Left));m.Push(T(.16,80,80,TouchPhaseKind.Ended));Assert.That(m.Outcome.ToString(),Is.EqualTo("ZoneCanceled"));m=New(area:new Rect(90,90,20,20));Lock(m);Assert.That(m.Locked,Is.False);Assert.That(m.LockBlocked,Is.True);}
        [Test] public void IdenticalEndedClosesOnce_WithoutAnnullingStrongPower()
        {var m=New();Lock(m);m.Push(T(.16,100,68));m.Push(T(.16,100,68,TouchPhaseKind.Ended));Assert.That(m.P,Is.EqualTo(1));Assert.That(m.Conservative,Is.False);Assert.That(m.ReleaseAt,Is.EqualTo(.16));m.Push(T(.18,100,68,TouchPhaseKind.Ended));Assert.That(m.Transitions.Count(x=>x.Kind=="Ended"),Is.EqualTo(1));}
        [Test] public void ContradictoryOrNonMonotonicTime_RemainsConservative()
        {var m=New();Lock(m);m.Push(T(.16,100,68));m.Push(T(.15,100,60));m.Push(T(.18,100,60,TouchPhaseKind.Ended));Assert.That(m.Conservative,Is.True);Assert.That(m.P,Is.Zero);}
        [Test] public void PrimaryFingerIsNotReplaced_AndSystemCancelDoesNotRelease()
        {var m=New();Lock(m);m.Push(T(.16,100,68,TouchPhaseKind.Ended,2));Assert.That(m.Active,Is.True);m.Cancel(.17);Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));Assert.That(double.IsNaN(m.ReleaseAt),Is.True);m.Push(T(.18,100,68,TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));}
        [Test] public void RB4_HeldTensionDoesNotDecay_WhileRB6RemainsObservable()
        {var m=New();Lock(m);m.Push(T(.16,100,68));m.Push(T(.31,100,68,TouchPhaseKind.Ended));Assert.That(m.P,Is.EqualTo(1));m=New();Lock(m,90);m.Push(T(.16,100,60));Assert.That(m.P,Is.GreaterThan(.6),"RB6 is recorded, not hidden by an artificial zero");}
        [Test] public void LockOutsideUsefulAreaIsBlocked_EvenIfSpaceExistsAbove()
        {var m=New(area:new Rect(0,120,200,200));Lock(m);Assert.That(m.Locked,Is.False);Assert.That(m.LockBlocked,Is.True);}
        [Test] public void TimerFixesSupportedStationaryContact_WithoutAnotherSample()
        {
            var m=New();Begin(m);m.Push(T(.02,100,103));m.Push(T(.04,100,103));m.Push(T(.06,100,103));
            m.Tick(.139);Assert.That(m.Locked,Is.False);m.Tick(.151);
            Assert.That(m.Locked,Is.True);Assert.That(m.LockOrigin,Is.EqualTo("timer"));
            Assert.That(m.LockTheoreticalAt,Is.EqualTo(.14).Within(1e-9));Assert.That(m.LockObservedAt,Is.EqualTo(.151));
            Assert.That(m.LastSampleAt,Is.EqualTo(.06));Assert.That(m.LockPositionPx,Is.EqualTo(new Vector2(100,103)));
            var aim=m.Aim;m.Tick(2);Assert.That(m.Transitions.Count(t=>t.Kind=="Locked"),Is.EqualTo(1));Assert.That(m.Aim,Is.EqualTo(aim));
        }
        [Test] public void TimerDoesNotInventMinimumSamples_AndLongGapDoesNotResetSupport()
        {
            var m=New();Begin(m);m.Push(T(.02,100,103));m.Push(T(.04,100,103));m.Tick(.5);
            Assert.That(m.Locked,Is.False,"Only two actual samples in the candidate");
            m.Push(T(.6,100,103));Assert.That(m.Locked,Is.True);Assert.That(m.LockOrigin,Is.EqualTo("sample"));
            Assert.That(m.LockTheoreticalAt,Is.EqualTo(.6));
        }
        [TestCase(TouchPhaseKind.Ended)] [TestCase(TouchPhaseKind.Canceled)]
        public void TerminalBeforeFramePreventsTimerFixation(TouchPhaseKind phase)
        {
            var m=New();Begin(m);m.Push(T(.02,100,103));m.Push(T(.04,100,103));m.Push(T(.06,100,103));m.Push(T(.13,100,103,phase));m.Tick(1);
            Assert.That(m.Locked,Is.False);Assert.That(m.Active,Is.False);
        }
        [Test] public void MovementBeforeDeadlineResetsTimer_AndCannotFixOutsideUsefulArea()
        {
            var m=New();Begin(m);m.Push(T(.02,100,103));m.Push(T(.04,100,103));m.Push(T(.06,100,103));
            m.Push(T(.13,100,105));m.Push(T(.15,100,105));m.Push(T(.17,100,105));m.Tick(.249);Assert.That(m.Locked,Is.False);m.Tick(.251);Assert.That(m.Locked,Is.True);
            m=New(area:new Rect(0,120,200,200));Begin(m);m.Push(T(.02,100,103));m.Push(T(.04,100,103));m.Push(T(.06,100,103));m.Tick(1);Assert.That(m.Locked,Is.False);Assert.That(m.LockBlocked,Is.True);
        }
        [Test] public void SystemCancellationAndNoPriorDragCannotBeCompletedByClock()
        {
            var m=New();Begin(m);m.Push(T(.02,100,103));m.Push(T(.04,100,103));m.Push(T(.06,100,103));m.Cancel(.1);m.Tick(1);Assert.That(m.Locked,Is.False);
            m=New();Begin(m);m.Push(T(.04,100,100));m.Push(T(.08,100,100));m.Tick(1);Assert.That(m.Locked,Is.False);
        }
        [Test] public void BothFixationTimesUseRealEventTimes()
        {var m=New();Lock(m);Assert.That(m.LockAt,Is.EqualTo(.14).Within(1e-9));Assert.That(m.LastOutsideAt,Is.EqualTo(.02).Within(1e-9));Assert.That(m.BeganAt,Is.Zero);}
    }
}
