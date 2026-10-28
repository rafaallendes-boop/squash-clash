using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class M24ModelTests
    {
        // Orthographic fixture: front wall fills 640x600px, 100px/m. mmPerPx=1 only in this fixture.
        private static M24Model New()=>new M24Model(new SlingSettings(holdMs:120),new FrontisProjection(
            Matrix4x4.TRS(new Vector3(3.2f,3,10),Quaternion.identity,Vector3.one),
            Matrix4x4.Ortho(-3.2f,3.2f,-3,3,.1f,100),new Rect(0,0,640,600),new Rect(0,0,640,600),CourtDimensions.Standard(),.025,3),1);
        private static TouchSample S(double t,float x=320,float y=220,TouchPhaseKind phase=TouchPhaseKind.Moved,int id=1)=>new TouchSample(id,phase,new Vector2(x,y),t,0);
        private static M24Model Begin(float y=220){var m=New();m.Push(S(0,y:y,phase:TouchPhaseKind.Began));return m;}
        [Test] public void BeganFixesImmediately_AndMovedDoesNotRetarget()
        {var m=Begin();Assert.That(m.HasAim,Is.True);var aim=m.Aim;m.Push(S(.1,350,180));Assert.That(m.Aim,Is.EqualTo(aim));Assert.That(m.P,Is.EqualTo(1));}
        [TestCase(.15,GestureOutcome.Tap)] [TestCase(.25,GestureOutcome.AbandonedNoPull)] [TestCase(1,GestureOutcome.AbandonedNoPull)]
        public void N01_NoPull150ms_NoSwing(double time,GestureOutcome outcome)
        {var m=Begin();m.Push(S(time,phase:TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(outcome));Assert.That(m.P,Is.Zero);}
        [TestCase(2,.1,GestureOutcome.Tap)] [TestCase(3,.1,GestureOutcome.Swing)] [TestCase(2,.25,GestureOutcome.AbandonedNoPull)]
        public void N02_StrictTapAndDeadZoneBoundaries(float down,double time,GestureOutcome expected)
        {var m=Begin();m.Push(S(time,y:220-down,phase:TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(expected));}
        [Test] public void N03_JitterWithinDeadZoneNeverSwings()
        {var m=Begin();m.Push(S(.1,350,218));m.Push(S(.4,350,218,TouchPhaseKind.Ended));Assert.That(m.P,Is.Zero);Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.AbandonedNoPull));}
        [Test] public void N04_AccidentalSlipRiskIsNotFalselyFiltered()
        {var m=Begin();m.Push(S(.2,y:185));m.Push(S(.4,y:185,phase:TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Swing));Assert.That(m.P,Is.EqualTo(1));}
        [Test] public void N04b_LateralSlipWithSmallDownwardPartCanSwingWithTinyPositivePower()
        {var m=Begin();m.Push(S(.3,350,217.9f,TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Swing));Assert.That(m.P,Is.GreaterThan(0).And.LessThan(.01));}
        [Test] public void N05_HeldPullKeepsDistancePower_WithoutDecay()
        {var m=Begin();m.Push(S(.1,y:185));m.Push(S(.25,y:185,phase:TouchPhaseKind.Ended));Assert.That(m.P,Is.EqualTo(1));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Swing));}
        [Test] public void N06_ReturnCancelsWithoutPeak_AndCanPullAgainBeforeRelease()
        {var m=Begin();m.Push(S(.1,y:185));m.Push(S(.3,y:220,phase:TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.CanceledReturnToDeadZone));Assert.That(m.P,Is.Zero);
         m=Begin();m.Push(S(.1,y:185));m.Push(S(.2,y:225));m.Push(S(.3,y:201.5f,phase:TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Swing));Assert.That(m.P,Is.EqualTo(.5));}
        [TestCase(48)] [TestCase(457)]
        public void N08_InvalidPrecedesTapAndCancel(float y)
        {var m=Begin(y);Assert.That(m.Invalid,Is.True);m.Push(S(.1,y:y,phase:TouchPhaseKind.Ended));Assert.That(m.TapCandidate,Is.True);Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.InvalidAim));Assert.That(m.P,Is.Zero);}
        [Test] public void N12_OutsideFrontWallRejectsWithoutTarget()
        {var m=Begin(610);Assert.That(m.HasAim,Is.False);Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Rejected));Assert.That(m.Reason,Is.Not.Empty);}
        [Test] public void N07_IdenticalEndedClosesOnce_AndKeepsReliablePowerAndOriginalR()
        {var m=Begin();m.Push(S(.1,y:185));m.Push(S(.1,y:185,phase:TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.Swing));Assert.That(m.P,Is.EqualTo(1));Assert.That(m.ReleaseAt,Is.EqualTo(.1));m.Push(S(.2,phase:TouchPhaseKind.Ended));Assert.That(m.ReleaseAt,Is.EqualTo(.1));}
        [TestCase(.1)] [TestCase(.09)]
        public void N07_ContradictoryTimeCannotSwing(double end)
        {var m=Begin();m.Push(S(.1,y:185));m.Push(S(end,y:180,phase:TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));Assert.That(m.Conservative,Is.True);Assert.That(m.P,Is.Zero);Assert.That(double.IsNaN(m.ReleaseAt),Is.True);}
        [Test] public void N10_SystemCancelPrecedesInvalidAndCannotReleaseLater()
        {var m=Begin(48);m.Cancel(.1,"focus");m.Push(S(.2,y:20,phase:TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));Assert.That(m.Reason,Is.EqualTo("focus"));Assert.That(double.IsNaN(m.ReleaseAt),Is.True);Assert.That(m.Active,Is.False);}
        [Test] public void N11_SecondaryFingerAndDuplicateBeganNeverReanchorOrComplete()
        {var m=Begin();m.Push(S(.05,100,100,TouchPhaseKind.Began));Assert.That(m.Anchor,Is.EqualTo(new Vector2(320,220)));m.Push(S(.1,y:185,phase:TouchPhaseKind.Ended,id:2));Assert.That(m.Active,Is.True);m.Cancel(.2,"dev");m.Push(S(.3,y:185,phase:TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));}
        [Test] public void N12_NonFiniteDataConservativelyClosesWithoutSwing()
        {var m=Begin();m.Push(S(.1,float.NaN,180,TouchPhaseKind.Ended));Assert.That(m.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));Assert.That(m.P,Is.Zero);Assert.That(m.Conservative,Is.True);}
    }
}
