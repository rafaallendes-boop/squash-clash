using System;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Simulation {
    public sealed class BallisticsTests {
        [Test] public void PhysicsParams_EqualityIncludesSurfaceCoefficientsAndStep() {
            var a=new PhysicsParams {Gravity=10,StepHz=120,Floor=new SurfaceMaterial {Restitution=0.5f,TangentialFriction=0.2f}};
            var b=new PhysicsParams {Gravity=10,StepHz=120,Floor=new SurfaceMaterial {Restitution=0.5f,TangentialFriction=0.2f}};
            Assert.That(a.Equals(b),Is.True); Assert.That(a.GetHashCode(),Is.EqualTo(b.GetHashCode()));
            b.Floor.Restitution=0.6f; Assert.That(a.Equals(b),Is.False);
            b.Floor.Restitution=0.5f; b.StepHz=240; Assert.That(a.Equals(b),Is.False);
            Assert.That(a.Floor.Equals(new SurfaceMaterial {Restitution=0.5f,TangentialFriction=0.3f}),Is.False);
        }
        [Test] public void ToState_AppliesLaunchScale() {
            var spec=new LaunchSpec(new Vector3(3,1,7),new Vector3(0,0,-2),10);
            var s=Launcher.ToState(spec,1.2);
            Assert.That(s.Position,Is.EqualTo(spec.Position)); Assert.That(s.Velocity,Is.EqualTo(new Vector3(0,0,-12))); Assert.That(s.Step,Is.Zero);
        }
        [Test] public void SolveCenterTo_ReachesCenterAnalytically() {
            var from=new Vector3(3.2f,1,5); var target=new Vector3(2,2.2f,0.02f);
            var v=Ballistics.SolveCenterTo(from,target,15,9.81f);
            double t=Math.Sqrt(1.2*1.2+4.98*4.98)/15;
            var s=new BallState { Position=from,Velocity=v };
            Assert.That(Vector3.Distance(Ballistics.PositionAt(s,9.81f,t),target),Is.LessThan(1e-4));
            Assert.That(Ballistics.VelocityAt(s,9.81f,t).y,Is.EqualTo(v.y-9.81*t).Within(1e-5));
        }
        [TestCase(SurfaceId.FrontWall,3.2f,4.56f,0,3.2f,4.56f,0.02f)]
        [TestCase(SurfaceId.LeftWall,6.4f,1,3,6.38f,1,3)]
        [TestCase(SurfaceId.RightWall,0,1,3,0.02f,1,3)]
        public void CenterForContact_OffsetsAlongApprovedInwardNormal(SurfaceId surface,float x,float y,float z,float ex,float ey,float ez) {
            Assert.That(Vector3.Distance(Ballistics.CenterForContact(surface,new Vector3(x,y,z),0.02f),new Vector3(ex,ey,ez)),Is.LessThan(1e-6));
        }
        [Test] public void FirstPlaneHit_FrontWallContactMatchesTarget() {
            var p=new Vector3(3.2f,4.56f,0); var center=new Vector3(3.2f,4.56f,0.02f);
            var s=new BallState {Position=new Vector3(3.2f,1,5)};
            s.Velocity=Ballistics.SolveCenterTo(s.Position,center,15,9.81f);
            Assert.That(Ballistics.FirstPlaneHit(s,9.81f,0.02f,CourtDimensions.Standard(),1,out var surface,out var t),Is.True);
            Assert.That(surface,Is.EqualTo(SurfaceId.FrontWall));
            Assert.That(Vector3.Distance(Ballistics.PositionAt(s,9.81f,t)-Vector3.forward*0.02f,p),Is.LessThan(1e-5));
        }
        [TestCase(-10,SurfaceId.RightWall)] [TestCase(10,SurfaceId.LeftWall)]
        public void FirstPlaneHit_UsesAx1SideNames(float vx,SurfaceId expected) {
            var s=new BallState { Position=new Vector3(3.2f,2,5),Velocity=new Vector3(vx,0,0) };
            Assert.That(Ballistics.FirstPlaneHit(s,9.81f,0.02f,CourtDimensions.Standard(),0.4,out var side,out var t),Is.True);
            Assert.That(side,Is.EqualTo(expected)); Assert.That(t,Is.EqualTo(0.318).Within(1e-6));
        }
        [Test] public void FirstPlaneHit_ReportsNoHitInsideShortHorizonOrAboveWalls() {
            var s=new BallState {Position=new Vector3(3,7,5),Velocity=new Vector3(0,0,-10)};
            Assert.That(Ballistics.FirstPlaneHit(s,0,0.02f,CourtDimensions.Standard(),1,out _,out _),Is.False);
            s.Position.y=1;
            Assert.That(Ballistics.FirstPlaneHit(s,9.81f,0.02f,CourtDimensions.Standard(),0.001,out _,out _),Is.False);
        }
        [Test] public void FirstPlaneHit_FromFloorMovingUp_SkipsSeparatingContact() {
            var s=new BallState {Position=new Vector3(3,0.02f,5),Velocity=new Vector3(0,2,0)};
            Assert.That(Ballistics.FirstPlaneHit(s,10,0.02f,CourtDimensions.Standard(),1,out var side,out var t),Is.True);
            Assert.That(side,Is.EqualTo(SurfaceId.Floor)); Assert.That(t,Is.EqualTo(0.4).Within(1e-6));
        }
    }
}
