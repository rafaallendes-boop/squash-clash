using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Tests
{
    public sealed class PracticeTaskTests
    {
        private static PracticeTaskRequest Request()=>new PracticeTaskRequest(0,1,0,2,2);
        private static ShotRecord Shot()=>new ShotRecord{LaunchId=8,AttemptId="attempt",GestureOutcome="Swing",HasRelease=true,AimX=1.5f,AimY=2.1f,Power=1,Solved=true,Applied=true,ContactStep=120,TargetUsedX=1.4f,TargetUsedY=2,Result="Perfect"};
        [Test] public void Task_RecordsRequestedVsFinalAimInCm()
        {
            var r=PracticeTask.Observe(Request(),Shot());
            Assert.That(r.RequestedAim,Is.EqualTo(new Vector2(1.2f,2.1f)));
            Assert.That(r.HasFrozenAim,Is.True);Assert.That(r.AimErrorCm,Is.EqualTo(30).Within(.001));
            Assert.That(r.FrozenAim,Is.EqualTo(new Vector2(1.5f,2.1f)));Assert.That(r.AimZoneHit,Is.True);
            Assert.That(r.HasFeasibleAim,Is.True);Assert.That(r.FeasibleAim,Is.EqualTo(new Vector2(1.4f,2)));
            Assert.That(r.FeasibleErrorCm,Is.EqualTo(Mathf.Sqrt(.05f)*100).Within(.001));
            Assert.That(r.HasFrontImpact,Is.False);Assert.That(r.FrontErrorCm,Is.EqualTo(-1));
        }
        [Test] public void Task_RecordsInvoluntaryPower()
        {
            var r=PracticeTask.Observe(Request(),Shot());
            Assert.That(r.Requested.PowerThird,Is.Zero);Assert.That(r.RequestedPower,Is.EqualTo(1f/6).Within(.00001));
            Assert.That(r.HasFrozenPower,Is.True);Assert.That(r.FrozenPower,Is.EqualTo(1));Assert.That(r.PowerBandHit,Is.False);
            Assert.That(r.ActualPowerThird,Is.EqualTo(2));Assert.That(r.PowerSaturated,Is.True);
        }
        private static ContactEvent Hit(SurfaceId s,Vector3 center,long step)
            =>new ContactEvent(s,center-CourtDimensions.InwardNormal(s)*.02f,Vector2.zero,CourtDimensions.InwardNormal(s),step,0,ContactTiming.Exact,Vector3.zero,false);
        [Test] public void Task_RecordsFirstBounceZoneHit()
        {
            var r=PracticeTask.Observe(Request(),Shot());
            PracticeTask.ObserveContacts(r,new[]{Hit(SurfaceId.Floor,new Vector3(2,.02f,1),110),Hit(SurfaceId.LeftWall,new Vector3(6.38f,1,2),121),Hit(SurfaceId.FrontWall,new Vector3(1.4f,2,.02f),130),Hit(SurfaceId.Floor,new Vector3(4.2f,.02f,6.4f),150),Hit(SurfaceId.Floor,new Vector3(1,.02f,2),190)},CourtDimensions.Standard(),.02f);
            Assert.That(r.HasFrontImpact,Is.True);Assert.That(r.FrontImpact.x,Is.EqualTo(1.4f));Assert.That(r.FrontErrorCm,Is.GreaterThan(0));
            Assert.That(r.HasFirstBounce,Is.True);Assert.That(r.FirstBounce,Is.EqualTo(new Vector3(4.2f,.02f,6.4f)));
            Assert.That(r.ActualBounceColumn,Is.EqualTo(2));Assert.That(r.ActualBounceDepth,Is.EqualTo(2));Assert.That(r.FirstBounceZoneHit,Is.True);
            Assert.That(r.PriorSurfaces,Does.Contain("LeftWall"));
        }
        [Test] public void Task_FailedApplicationRetainsSolvedTarget_ButDoesNotReadIncomingBounces()
        {
            var shot=Shot();shot.Applied=false;shot.Reason="schedule-rejected";
            var r=PracticeTask.Observe(Request(),shot);
            Assert.That(r.HasFeasibleAim,Is.True);Assert.That(r.FeasibleAim,Is.EqualTo(new Vector2(1.4f,2)));
            PracticeTask.ObserveContacts(r,new[]{Hit(SurfaceId.Floor,new Vector3(4.2f,.02f,6.4f),150)},CourtDimensions.Standard(),.02f);
            Assert.That(r.HasFirstBounce,Is.False);Assert.That(r.BounceFailure,Is.EqualTo("no-applied-contact"));
        }
        [Test] public void FailedCapture_IsMissingWithReason_NeverZeroError()
        {
            var shot=Shot();shot.GestureOutcome="InvalidAim";shot.Applied=false;shot.Result="Closure";shot.Reason="ray-missed";
            var r=PracticeTask.Observe(Request(),shot);
            Assert.That(r.HasFrozenAim,Is.False);Assert.That(r.HasFrozenPower,Is.False);Assert.That(r.HasFeasibleAim,Is.False);
            Assert.That(r.AimErrorCm,Is.EqualTo(-1));Assert.That(r.CaptureFailure,Does.Contain("ray-missed"));
            Assert.That(r.HasFirstBounce,Is.False);Assert.That(r.BounceFailure,Is.Not.Null.And.Not.Empty);
        }
        [TestCase(.2f,0)][TestCase(2.1999f,0)][TestCase(2.2f,1)][TestCase(4.2f,2)][TestCase(6.2f,2)][TestCase(6.2001f,-1)][TestCase(float.NaN,-1)]
        public void ZoneX_UsesDocumentedHalfOpenEdges(float x,int band)=>Assert.That(PracticeTask.FrontColumn(x),Is.EqualTo(band));
        [TestCase(.6f,0)][TestCase(1.6f,1)][TestCase(2.6f,2)][TestCase(3.6f,2)][TestCase(.5999f,-1)]
        public void ZoneY_UsesDocumentedHalfOpenEdges(float y,int band)=>Assert.That(PracticeTask.FrontRow(y),Is.EqualTo(band));
        [TestCase(.2f,0)][TestCase(3.2f,1)][TestCase(6.4f,2)][TestCase(9.55f,2)][TestCase(9.5501f,-1)]
        public void Depth_UsesDocumentedHalfOpenEdges(float z,int band)=>Assert.That(PracticeTask.BounceDepth(z),Is.EqualTo(band));
        [Test] public void Power_ExactThirdsAndEndpoints()
        {
            Assert.That(PracticeTask.PowerThird(0),Is.Zero);Assert.That(PracticeTask.PowerThird(1f/3),Is.EqualTo(1));
            Assert.That(PracticeTask.PowerThird(2f/3),Is.EqualTo(2));Assert.That(PracticeTask.PowerThird(1),Is.EqualTo(2));
            Assert.That(PracticeTask.PowerThird(float.NaN),Is.EqualTo(-1));
        }
    }
}
