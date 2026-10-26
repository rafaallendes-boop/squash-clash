using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class AimReleaseTests
    {
        private static TouchSample S(double t,float x,float y,TouchPhaseKind phase=TouchPhaseKind.Moved)=>new TouchSample(1,phase,new Vector2(x,y),t,(long)(t*10000));
        private static AimModel Model(bool release=true)
        {var m=new AimModel(new AimSettings(.12f,2.2f,250,40,50,2,release),CourtDimensions.Standard(),3.2f);m.Update(S(0,0,0,TouchPhaseKind.Began),1);return m;}
        [Test] public void ReleaseWithoutFlick_FreezesAtCurrentValidAimWithoutLookbackJump()
        {
            var m=Model();m.Update(S(.5,5,0),1);m.Update(S(.6,6,0,TouchPhaseKind.Ended),1);
            Assert.That(m.State,Is.EqualTo(AimState.Frozen));Assert.That(m.Aim.x,Is.EqualTo(2.48f).Within(1e-5));
            Assert.That(m.FreezeCause,Is.EqualTo(AimFreezeCause.Release));Assert.That(m.FreezeTimeReal,Is.EqualTo(.6));Assert.That(m.FreezeJumpMeters,Is.Zero);
        }
        [Test] public void ReleaseAtInvalidTin_FreezesInvalidPosition()
        {
            var m=Model();m.Update(S(.5,0,-20),1);m.Update(S(.6,0,-21,TouchPhaseKind.Ended),1);
            Assert.That(m.State,Is.EqualTo(AimState.Frozen));Assert.That(m.FreezeCause,Is.EqualTo(AimFreezeCause.Release));Assert.That(m.Aim.y,Is.LessThan(CourtDimensions.Standard().TinTop));
        }
        [Test] public void PriorFlick_IsNotFrozenAgainAtRelease()
        {
            var m=Model();m.Update(S(.02,16,0),1);m.Update(S(.04,32,0),1);var aim=m.Aim;var time=m.FreezeTimeReal;var jump=m.FreezeJumpMeters;
            m.Update(S(.15,36,10,TouchPhaseKind.Ended),1);
            Assert.That(m.FreezeCause,Is.EqualTo(AimFreezeCause.Flick));Assert.That(m.Aim,Is.EqualTo(aim));Assert.That(m.FreezeTimeReal,Is.EqualTo(time));Assert.That(m.FreezeJumpMeters,Is.EqualTo(jump));
        }
        [Test] public void DefaultReleaseDisabled_KeepsLegacyAimingState()
        {var m=Model(false);m.Update(S(.5,5,0,TouchPhaseKind.Ended),1);Assert.That(m.State,Is.EqualTo(AimState.Aiming));Assert.That(m.FreezeCause,Is.EqualTo(AimFreezeCause.None));}
        [Test] public void CancelNeverFreezesAtRelease_AndNextBeganClearsCause()
        {
            var m=Model();m.Update(S(.5,5,0,TouchPhaseKind.Canceled),1);Assert.That(m.State,Is.EqualTo(AimState.Aiming));
            m.Update(S(1,0,0,TouchPhaseKind.Began),1);m.Update(S(1.5,5,0,TouchPhaseKind.Ended),1);
            m.Update(S(2,0,0,TouchPhaseKind.Began),1);Assert.That(m.FreezeCause,Is.EqualTo(AimFreezeCause.None));
        }
    }
}
