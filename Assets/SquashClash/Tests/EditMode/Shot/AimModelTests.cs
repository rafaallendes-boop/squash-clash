using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class AimModelTests
    {
        private static AimSettings Settings(float flick=250)=>new AimSettings(.12f,2.2f,flick,40,50,2);
        private static TouchSample S(double t,float x,float y=0,TouchPhaseKind phase=TouchPhaseKind.Moved)=>new TouchSample(1,phase,new Vector2(x,y),t,(long)(t*10000));
        private static AimModel Model(float flick=250)
        {var m=new AimModel(Settings(flick),CourtDimensions.Standard(),3.2f);m.Update(S(1,0,0,TouchPhaseKind.Began),1);return m;}
        [Test] public void StartPosition_IsNeutral2_2mOnBallSideDriveLine()
        {var m=new AimModel(Settings(),CourtDimensions.Standard(),true);Assert.That(m.Aim.y,Is.EqualTo(2.2f));Assert.That(m.Aim.x,Is.GreaterThan(3.2f));}
        [Test] public void Drag10mm_MovesAimByGainTimes10()
        {var m=Model(1000);m.Update(S(1.1,10),1);Assert.That(m.Aim.x,Is.EqualTo(2).Within(1e-6),"AX-1: screen right maps to -X.");}
        [Test] public void Aim_ClampsAtFrontWallEdges()
        {var m=Model();m.Update(S(1.1,1000,1000),1);Assert.That(m.Aim,Is.EqualTo(new Vector2(0,5)));}
        private static AimModel Frozen()
        {
            var m=Model();m.Update(S(1.04,2),1);m.Update(S(1.06,3),1);
            m.Update(S(1.08,13),1);m.Update(S(1.10,23),1);return m;
        }
        [Test] public void Flick_FreezesAtPositionFromLookbackBeforeCrossing()
        {var m=Frozen();Assert.That(m.State,Is.EqualTo(AimState.Frozen));Assert.That(m.FreezeTimeReal,Is.EqualTo(1.08));Assert.That(m.Aim.x,Is.EqualTo(3.02f).Within(1e-5));}
        [Test] public void Flick_WithoutHistory_UsesFirstValidPointAndFlagsIt()
        {var m=Model();m.Update(S(1.01,10),1);m.Update(S(1.02,20),1);Assert.That(m.State,Is.EqualTo(AimState.Frozen));Assert.That(m.FreezeUsedFirstValidPoint,Is.True);Assert.That(m.Aim,Is.EqualTo(new Vector2(3.2f,2.2f)));}
        [Test] public void Freeze_HappensOncePerGesture_WithHysteresis()
        {var m=Frozen();double at=m.FreezeTimeReal;m.Update(S(1.2,0),1);m.Update(S(1.21,100),1);Assert.That(m.State,Is.EqualTo(AimState.Frozen));Assert.That(m.FreezeTimeReal,Is.EqualTo(at));}
        [Test] public void AfterFreeze_FurtherMovementDoesNotMoveAim()
        {var m=Frozen();var at=m.Aim;m.Update(S(1.3,99,99),1);Assert.That(m.Aim,Is.EqualTo(at));}
        [Test] public void NoiseBetweenTwoCloseSamples_DoesNotTriggerFreeze()
        {var m=Model();m.Update(S(1.0001,.1f),1);m.Update(S(1.02,.11f),1);Assert.That(m.State,Is.EqualTo(AimState.Aiming));}
        [Test] public void CameraAndLaterScaleChanges_DoNotMoveOrRescaleStoredAim()
        {var m=Model(1000);m.Update(S(1.1,5),1);m.Update(S(1.2,10),9);Assert.That(m.Aim.x,Is.EqualTo(2).Within(1e-6));}
    }
}
