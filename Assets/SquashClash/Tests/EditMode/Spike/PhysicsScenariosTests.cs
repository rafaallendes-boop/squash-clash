using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Core.Spike;
using UnityEngine;
namespace SquashClash.Core.Tests.Spike
{
    public sealed class PhysicsScenariosTests
    {
        private CourtDimensions d;
        private PhysicsParams p;
        [SetUp] public void Setup() { d=CourtDimensions.Standard(); p=EffectiveConfig.From(Config.ConfigFixtures.Default()).Physics; }
        private PhysicsScenario Scenario(string id) => PhysicsScenarios.Fixed(d,p).Single(s=>s.Id==id);
        private (SurfaceId face,double t) Hit(PhysicsScenario s) {
            Assert.That(Ballistics.FirstPlaneHit(s.Start,p.Gravity,p.BallRadius,d,6,out var face,out var t),Is.True,s.Id);
            return (face,t);
        }
        [TestCase("S1",0.6f,2.2f,28f)]
        [TestCase("S2",5.5f,0.53f,6f)]
        [TestCase("S5",0.025f,1.5f,15f)]
        public void FrontTargets_AreFirstImpactAtIdealPoint(string id,float x,float y,float horizontalSpeed) {
            var s=Scenario(id);var hit=Hit(s);
            Assert.That(hit.face,Is.EqualTo(SurfaceId.FrontWall));
            var point=Ballistics.PositionAt(s.Start,p.Gravity,hit.t)-Vector3.forward*p.BallRadius;
            Assert.That(Vector3.Distance(point,new Vector3(x,y,0)),Is.LessThanOrEqualTo(1e-5f));
            Assert.That(new Vector2(s.Start.Velocity.x,s.Start.Velocity.z).magnitude,Is.EqualTo(horizontalSpeed).Within(1e-5));
            if(id=="S1")Assert.That(s.MinContactsBeforeStop,Is.EqualTo(3));
            if(id=="S5")Assert.That(point.x-p.BallRadius,Is.EqualTo(0.005f).Within(1e-6));
        }
        [Test] public void S3_FirstHitsRightWallAtRealIncidenceBelowTenDegrees() {
            var s=Scenario("S3");var hit=Hit(s);
            Assert.That(s.Start.Position,Is.EqualTo(new Vector3(0.8f,1,8)));
            Assert.That(hit.face,Is.EqualTo(SurfaceId.RightWall));
            var v=Ballistics.VelocityAt(s.Start,p.Gravity,hit.t);
            Assert.That(Math.Asin(Math.Abs(v.x)/v.magnitude)*180/Math.PI,Is.InRange(0.01,9.999));
            Assert.That(s.Start.Velocity.magnitude,Is.EqualTo(20).Within(1e-5));
        }
        [Test] public void S4_SideAndFloorArriveInSameStep() {
            var s=Scenario("S4");
            double side=((double)p.BallRadius-s.Start.Position.x)/s.Start.Velocity.x;
            double vy=s.Start.Velocity.y;
            double floor=(vy+Math.Sqrt(vy*vy+2*p.Gravity*(s.Start.Position.y-p.BallRadius)))/p.Gravity;
            Assert.That(Math.Ceiling(side/p.StepSeconds),Is.EqualTo(Math.Ceiling(floor/p.StepSeconds)));
            Assert.That(Vector3.Distance(Ballistics.PositionAt(s.Start,p.Gravity,side),new Vector3(.02f,.02f,2.5f)),Is.LessThan(1e-5));
        }
        [Test] public void S6_BothBackCornerPlanesAreInsideSameStepAwayFromEdges() {
            var s=Scenario("S6");
            double side=((double)d.Width-p.BallRadius-s.Start.Position.x)/s.Start.Velocity.x;
            double back=((double)d.Length-p.BallRadius-s.Start.Position.z)/s.Start.Velocity.z;
            Assert.That(Math.Ceiling(side/p.StepSeconds),Is.EqualTo(Math.Ceiling(back/p.StepSeconds)));
            foreach(var t in new[]{side,back}) {
                double phase=t-Math.Floor(t/p.StepSeconds)*p.StepSeconds;
                Assert.That(phase,Is.InRange(.001,p.StepSeconds-.001));
            }
            Assert.That(Vector3.Distance(Ballistics.PositionAt(s.Start,p.Gravity,back),new Vector3(6.38f,1,9.73f)),Is.LessThan(1e-5));
        }
        [Test] public void S7_FirstHitsBackWall() { Assert.That(Hit(Scenario("S7")).face,Is.EqualTo(SurfaceId.BackWall)); }
        [Test] public void S10_RunsSixSecondsWithoutStoppingAtSecondBounce() {
            var s=Scenario("S10");Assert.That(s.Stop,Is.EqualTo(StopCondition.MaxSteps));
            Assert.That(s.MaxSteps,Is.EqualTo(720));Assert.That(s.Start.Position,Is.EqualTo(new Vector3(3.2f,.3f,5)));
            Assert.That(s.Start.Velocity,Is.EqualTo(Vector3.down));
        }
        [Test] public void S9_ReferenceClassificationMatchesDesign() {
            float[] heights={4.557f,4.560f,4.563f,.493f,.490f,.487f,1.767f,1.770f,1.773f};
            var lines=new[]{LineId.FrontOut,LineId.Tin,LineId.Service};
            var all=PhysicsScenarios.Fixed(d,p);
            for(int i=0;i<9;i++) {
                var s=all.Single(v=>v.Id=="S9"+(char)('a'+i));var hit=Hit(s);
                Assert.That(s.Line,Is.EqualTo(lines[i/3]));Assert.That(hit.face,Is.EqualTo(SurfaceId.FrontWall));
                var ideal=new Vector3(3.2f,heights[i],0);
                Assert.That(Vector3.Distance(s.ContactPoint.Value,ideal),Is.LessThan(1e-6));
                Assert.That(Vector3.Distance(Ballistics.PositionAt(s.Start,p.Gravity,hit.t)-Vector3.forward*.02f,ideal),Is.LessThan(1e-5));
                if(i%3==1)Assert.That(s.ExpectTouch,Is.Null,"exact border is precision-only");
                else {
                    bool expected=i%3==2;Assert.That(s.ExpectTouch,Is.EqualTo(expected));
                    var band=CourtLines.Build(d).Single(b=>b.Id==lines[i/3]);
                    Assert.That(LineContactClassifier.Touches(band,new Vector2(3.2f,heights[i]),.01f),Is.EqualTo(expected));
                }
            }
        }
        [Test] public void S9_TinTargetsFollowSelectedTinWithoutChangingMargins() {
            d.TinTop=.43f;var s=Scenario("S9d");Assert.That(s.ContactPoint.Value.y,Is.EqualTo(.443f).Within(1e-6));
            Assert.That(Scenario("S2").ContactPoint.Value.y,Is.EqualTo(.48f).Within(1e-6));
        }
        [Test] public void Random_RobustnessRunsAll360StepsEvenAfterSecondBounce() {
            var s=PhysicsScenarios.Random(d,p,1,7).Single();
            using(var simulation=new EveryStepFloorSimulation()) {
                var trajectory=TrajectoryRunner.Run(simulation,s.Start,s.MaxSteps,s.Stop,d);
                Assert.That(trajectory.States.Last().Step,Is.EqualTo(360));
            }
        }
        private sealed class EveryStepFloorSimulation:IBallSimulation {
            private BallState state;
            public BackendCapabilities Capabilities=>new BackendCapabilities{RunsInEditMode=true};
            public BallState State=>state;
            public void Reset(in BallState initial){state=initial;state.Position=new Vector3(3,.02f,5);}
            public void Step(List<ContactEvent> contacts){state.Step++;contacts.Clear();contacts.Add(new ContactEvent(SurfaceId.Floor,state.Position,Vector2.zero,Vector3.up,state.Step,0,ContactTiming.Exact,Vector3.down,false));}
            public void Dispose(){}
        }
        [Test] public void Random_IsReproducibleForSeed() {
            var a=PhysicsScenarios.Random(d,p,32,7).ToArray();var b=PhysicsScenarios.Random(d,p,32,7).ToArray();
            Assert.That(a.Length,Is.EqualTo(32));
            for(int i=0;i<a.Length;i++) { Assert.That(a[i].Start.Position,Is.EqualTo(b[i].Start.Position));Assert.That(a[i].Start.Velocity,Is.EqualTo(b[i].Start.Velocity)); }
            Assert.That(a[0].Start.Velocity,Is.Not.EqualTo(PhysicsScenarios.Random(d,p,1,8).First().Start.Velocity));
        }
        [Test] public void Random_CoversSphereAndApprovedRangesWithoutPolarBias() {
            var a=PhysicsScenarios.Random(d,p,10000,20261020).ToArray();var mean=Vector3.zero;var second=Vector3.zero;
            foreach(var s in a) {
                var pos=s.Start.Position;Assert.That(pos.x,Is.InRange(.3f,6.1f));Assert.That(pos.y,Is.InRange(.3f,2.5f));Assert.That(pos.z,Is.InRange(.3f,9.45f));
                Assert.That(s.Start.Velocity.magnitude,Is.InRange(1.99999f,35.00001f));Assert.That(s.MaxSteps,Is.EqualTo(360));
                var v=s.Start.Velocity.normalized;mean+=v;second+=Vector3.Scale(v,v);
            }
            mean/=a.Length;second/=a.Length;
            foreach(var value in new[]{mean.x,mean.y,mean.z})Assert.That(Math.Abs(value),Is.LessThan(.05));
            foreach(var value in new[]{second.x,second.y,second.z})Assert.That(value,Is.InRange(.308f,.358f));
        }
        [Test] public void EdgeDirected_Has36LaunchesInsideCourtAtSpecifiedTotalSpeeds() {
            var a=PhysicsScenarios.EdgeDirected(d,p);Assert.That(a.Count,Is.EqualTo(36));
            foreach(var s in a) {
                Assert.That(s.Start.Position,Is.EqualTo(new Vector3(3.2f,1,4.875f)));
                float speed=s.Start.Velocity.magnitude;
                Assert.That(new[]{10f,25f,40f}.Min(v=>Math.Abs(v-speed)),Is.LessThan(1e-4));
                var center=Ballistics.CenterForContact(s.ContactSurface.Value,s.ContactPoint.Value,p.BallRadius);
                var hit=Hit(s);Assert.That(Vector3.Distance(Ballistics.PositionAt(s.Start,p.Gravity,hit.t),center),Is.LessThan(1e-5),s.Id);
                Assert.That(center.x,Is.InRange(.01999f,6.38001f));Assert.That(center.y,Is.InRange(.01999f,1.00001f));Assert.That(center.z,Is.InRange(.01999f,9.73001f));
            }
            Assert.That(a.Select(s=>s.ContactPoint).Distinct().Count(),Is.EqualTo(12));
        }
    }
}
