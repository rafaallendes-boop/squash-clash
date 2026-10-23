using System;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests
{
    public sealed class AnalyticBallSimulationTests
    {
        private static PhysicsParams Params(float gravity=9.81f, int hz=120, int cap=4, float restitution=.5f, float friction=0) => new PhysicsParams {
            Gravity=gravity,StepHz=hz,MaxImpactsPerStep=cap,
            Floor=new SurfaceMaterial {Restitution=restitution,TangentialFriction=friction},
            FrontWall=new SurfaceMaterial {Restitution=restitution,TangentialFriction=friction},
            SideWalls=new SurfaceMaterial {Restitution=restitution,TangentialFriction=friction},
            BackWall=new SurfaceMaterial {Restitution=restitution,TangentialFriction=friction} };
        private static IBallSimulation Create(PhysicsParams p=null, CourtDimensions d=null) => new AnalyticBallSimulationFactory(.1f).Create(p??Params(),d??new CourtDimensions());
        private static BallState State(Vector3 p,Vector3 v=default,long step=0)=>new BallState {Position=p,Velocity=v,Step=step};
        [Test] public void Factory_ReportsCapabilitiesAndReleasesOnce()
        {
            var f=new AnalyticBallSimulationFactory(.1f);var sim=f.Create(Params(),new CourtDimensions());
            Assert.That(f.Name,Is.EqualTo("P2-Analytic"));Assert.That(f.LiveInstances,Is.EqualTo(1));
            Assert.That(sim.Capabilities.Timing,Is.EqualTo(ContactTiming.Exact));Assert.That(sim.Capabilities.ImpactCapConfigurable,Is.True);Assert.That(sim.Capabilities.RunsInEditMode,Is.True);
            sim.Dispose();sim.Dispose();Assert.That(f.LiveInstances,Is.Zero);Assert.Throws<ObjectDisposedException>(()=>sim.Reset(default));
        }
        [Test] public void FreeFlight_IntegratesExactGravityAndAdvancesOneStep()
        {
            var p=Params();using var sim=Create(p);var s=State(new Vector3(3,2,5),new Vector3(1,2,-3),17);sim.Reset(s);
            var contacts=new List<ContactEvent>();sim.Step(contacts);
            Assert.That(sim.State.Position,Is.EqualTo(Ballistics.PositionAt(s,p.Gravity,p.StepSeconds)));Assert.That(sim.State.Velocity,Is.EqualTo(Ballistics.VelocityAt(s,p.Gravity,p.StepSeconds)));Assert.That(sim.State.Step,Is.EqualTo(18));Assert.That(contacts,Is.Empty);
        }
        [Test] public void WallImpact_UsesExactTimeAndConsumesRemainder()
        {
            using var sim=Create(Params(0,10));sim.Reset(State(new Vector3(.07f,2,5),new Vector3(-1,0,2)));
            var contacts=new List<ContactEvent>();sim.Step(contacts);Assert.That(contacts.Count,Is.EqualTo(1));var c=contacts[0];
            Assert.That(c.Surface,Is.EqualTo(SurfaceId.RightWall));Assert.That(c.TimeInStep,Is.EqualTo(.05f).Within(1e-7));Assert.That(c.Timing,Is.EqualTo(ContactTiming.Exact));Assert.That(c.Point.x,Is.Zero.Within(1e-7));Assert.That(c.Normal,Is.EqualTo(Vector3.right));Assert.That(c.Persistent,Is.False);
            Assert.That(sim.State.Position.x,Is.EqualTo(.045f).Within(1e-7));Assert.That(sim.State.Position.z,Is.EqualTo(5.2f).Within(1e-6));Assert.That(sim.State.Velocity.x,Is.EqualTo(.5f));
        }
        [Test] public void FloorImpact_SolvesQuadraticAndThenIntegratesGravity()
        {
            var p=Params(10,10);using var sim=Create(p);var s=State(new Vector3(3,.07f,5),new Vector3(0,-1,0));sim.Reset(s);var cs=new List<ContactEvent>();sim.Step(cs);
            double t=(-1+Math.Sqrt(1+20*((double)s.Position.y-p.BallRadius)))/10;double incoming=-1-10*t,remaining=.1-t;
            Assert.That(cs.Count,Is.EqualTo(1));Assert.That(cs[0].TimeInStep,Is.EqualTo((float)t).Within(1e-7));Assert.That(sim.State.Position.y,Is.EqualTo((float)(p.BallRadius-.5*incoming*remaining-5*remaining*remaining)).Within(1e-7));
            Assert.That(sim.State.Velocity.y,Is.EqualTo((float)(-.5*incoming-10*remaining)).Within(1e-7));
        }
        [TestCase(.0000005f,true)] [TestCase(.000002f,false)]
        public void Corner_GroupsOnlyImpactsWithinEpsilon(float separation,bool simultaneous)
        {
            using var sim=Create(Params(0,10));var initial=State(new Vector3(.07f+separation,.07f,5),new Vector3(-1,-1,3));sim.Reset(initial);
            var cs=new List<ContactEvent>();sim.Step(cs);Assert.That(cs.Count,Is.EqualTo(2));Assert.That(cs[0].Surface,Is.EqualTo(SurfaceId.Floor));Assert.That(cs[1].Surface,Is.EqualTo(SurfaceId.RightWall));
            if(simultaneous) {Assert.That(cs[1].TimeInStep,Is.EqualTo(cs[0].TimeInStep));Assert.That(cs[1].ApproachVelocity,Is.EqualTo(cs[0].ApproachVelocity));}
            else Assert.That(cs[1].TimeInStep-cs[0].TimeInStep,Is.GreaterThan(1e-6f));
            Assert.That(sim.State.Velocity,Is.EqualTo(new Vector3(.5f,.5f,3)));
        }
        [Test] public void ThreePlaneCorner_ReflectsAllNormalsTogetherInSurfaceOrder()
        {
            using var sim=Create(Params(0,10,restitution:1));sim.Reset(State(new Vector3(.07f,.07f,.07f),new Vector3(-1,-1,-1)));var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(cs.Count,Is.EqualTo(3));Assert.That(cs.ConvertAll(c=>c.Surface),Is.EqualTo(new[]{SurfaceId.Floor,SurfaceId.FrontWall,SurfaceId.RightWall}));
            foreach(var c in cs){Assert.That(c.ApproachVelocity,Is.EqualTo(new Vector3(-1,-1,-1)));Assert.That(c.TimeInStep,Is.EqualTo(cs[0].TimeInStep));}
            Assert.That(sim.State.Velocity,Is.EqualTo(Vector3.one));
        }
        [Test] public void SlowFloorImpact_SettlesAndEverySupportedStepReportsFloor()
        {
            var p=Params(10,120);using var sim=Create(p);sim.Reset(State(new Vector3(3,p.BallRadius,5),new Vector3(0,-.05f,0)));var cs=new List<ContactEvent>();
            sim.Step(cs);Assert.That(sim.State.Position.y,Is.EqualTo(p.BallRadius));Assert.That(sim.State.Velocity.y,Is.Zero);
            for(int i=0;i<60;i++){sim.Step(cs);Assert.That(sim.State.Position.y,Is.EqualTo(p.BallRadius));Assert.That(sim.State.Velocity.y,Is.Zero);Assert.That(cs.Exists(c=>c.Surface==SurfaceId.Floor&&c.Persistent),Is.True);}
        }
        [Test] public void SupportedSlide_DeceleratesByCoulombToRestWithoutReversing()
        {
            var p=Params(10,10,friction:.5f);using var sim=Create(p);sim.Reset(State(new Vector3(3,p.BallRadius,5),Vector3.right));var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(sim.State.Position.x,Is.EqualTo(3.075f).Within(1e-6));Assert.That(sim.State.Velocity.x,Is.EqualTo(.5f).Within(1e-6));Assert.That(sim.State.Position.y,Is.EqualTo(p.BallRadius));
            sim.Step(cs);Assert.That(sim.State.Position.x,Is.EqualTo(3.1f).Within(1e-6));Assert.That(sim.State.Velocity,Is.EqualTo(Vector3.zero));sim.Step(cs);Assert.That(sim.State.Position.x,Is.EqualTo(3.1f).Within(1e-6));Assert.That(cs[0].Persistent,Is.True);
        }
        [Test] public void SupportedSlide_StillDetectsAndReflectsWall()
        {
            var p=Params(10,10);using var sim=Create(p);sim.Reset(State(new Vector3(.07f,p.BallRadius,5),new Vector3(-1,0,0)));var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(sim.State.Position.x,Is.EqualTo(.045f).Within(1e-7));Assert.That(sim.State.Position.y,Is.EqualTo(p.BallRadius));Assert.That(sim.State.Velocity,Is.EqualTo(new Vector3(.5f,0,0)));Assert.That(cs.Exists(c=>c.Surface==SurfaceId.Floor&&c.Persistent),Is.True);Assert.That(cs.Exists(c=>c.Surface==SurfaceId.RightWall&&!c.Persistent),Is.True);
        }
        [TestCase(.05f,0f)] [TestCase(.1f,.05f)]
        public void WallThreshold_StopsOnlySmallNormalSpeedAndNeverSupportsOnWall(float incoming,float outgoing)
        {
            var p=Params(10,10);using var sim=Create(p);sim.Reset(State(new Vector3(p.BallRadius,2,5),new Vector3(-incoming,-1,0)));var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(sim.State.Velocity.x,Is.EqualTo(outgoing).Within(1e-7));Assert.That(sim.State.Position.y,Is.EqualTo(1.85f).Within(1e-6));Assert.That(sim.State.Velocity.y,Is.EqualTo(-2f));sim.Step(cs);Assert.That(sim.State.Velocity.y,Is.EqualTo(-3f));
        }
        [Test] public void ImpactCap_DiscardsRemainderWithReflectedVelocityAndCountsEveryReachedCap()
        {
            using var sim=Create(Params(0,10,cap:1));var initial=State(new Vector3(.07f,2,5),new Vector3(-1,0,0));sim.Reset(initial);var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(sim.State.Position.x,Is.EqualTo(.02f));Assert.That(sim.State.Velocity.x,Is.EqualTo(.5f));Assert.That(sim.State.Step,Is.EqualTo(1));Assert.That(((AnalyticBallSimulation)sim).ImpactCapReachedCount,Is.EqualTo(1));
            sim.Step(cs);Assert.That(sim.State.Position.x,Is.EqualTo(.07f).Within(1e-7));Assert.That(((AnalyticBallSimulation)sim).ImpactCapReachedCount,Is.EqualTo(1));
            sim.Reset(initial);Assert.That(((AnalyticBallSimulation)sim).ImpactCapReachedCount,Is.Zero);
        }
        [Test] public void ImpactCap_SimultaneousGroupIsAtomicAndNeverIntegratesThroughWall()
        {
            using var sim=Create(Params(0,10,cap:1,restitution:1));sim.Reset(State(new Vector3(.07f,.07f,.07f),new Vector3(-1,-1,-1)));var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(cs.Count,Is.EqualTo(3));Assert.That(sim.State.Position,Is.EqualTo(new Vector3(.02f,.02f,.02f)));Assert.That(sim.State.Velocity,Is.EqualTo(Vector3.one));Assert.That(((AnalyticBallSimulation)sim).ImpactCapReachedCount,Is.EqualTo(1));
        }
        [Test] public void ImpactCap_CountsRepeatedCappedStepsAndStopsAtLastImpact()
        {
            var d=new CourtDimensions {Width=.24f};using var sim=Create(Params(0,1,cap:2,restitution:1),d);sim.Reset(State(new Vector3(.1f,2,5),new Vector3(-4,0,0)));var cs=new List<ContactEvent>();
            for(int i=0;i<3;i++){sim.Step(cs);Assert.That(cs.Count,Is.EqualTo(2));Assert.That(((AnalyticBallSimulation)sim).ImpactCapReachedCount,Is.EqualTo(i+1));Assert.That(sim.State.Position.x,Is.InRange(.02f,.22f));}
        }
        [Test] public void Floor_HasNoHorizontalLimit()
        {
            using var sim=Create(Params(0,10));sim.Reset(State(new Vector3(-2,.07f,30),new Vector3(0,-1,0)));var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(cs.Count,Is.EqualTo(1));Assert.That(cs[0].Surface,Is.EqualTo(SurfaceId.Floor));Assert.That(cs[0].Point,Is.EqualTo(new Vector3(-2,0,30)));Assert.That(sim.State.Position.y,Is.EqualTo(.045f).Within(1e-7));
        }
        [TestCase(6f,true)] [TestCase(6.01f,false)] [TestCase(-.01f,false)]
        public void Wall_UsesCenterHeightAtImpactAndHasNoTopEdge(float height,bool hit)
        {
            using var sim=Create(Params(0,10));sim.Reset(State(new Vector3(.07f,height,5),new Vector3(-1,0,0)));var cs=new List<ContactEvent>();sim.Step(cs);Assert.That(cs.Count,Is.EqualTo(hit?1:0));
        }
        [TestCase(6.1f,-4f,true)] [TestCase(5.9f,4f,false)]
        public void Wall_EvaluatesHeightAtTimeOfImpactRatherThanAtStepStart(float y,float vy,bool hit)
        {
            using var sim=Create(Params(0,10));sim.Reset(State(new Vector3(.07f,y,5),new Vector3(-1,vy,0)));var cs=new List<ContactEvent>();sim.Step(cs);Assert.That(cs.Exists(c=>c.Surface==SurfaceId.RightWall),Is.EqualTo(hit));
        }
        [Test] public void UpperExit_UsesUnmodifiedRunnerDefinition()
        {
            using var sim=Create(Params(0,10));var trajectory=TrajectoryRunner.Run(sim,State(new Vector3(.07f,6.01f,5),new Vector3(-1,0,0)),5,StopCondition.MaxSteps,new CourtDimensions());
            Assert.That(trajectory.Exit,Is.EqualTo(TrajectoryExit.OverTop));Assert.That(trajectory.States.Count,Is.EqualTo(2));Assert.That(trajectory.Contacts,Is.Empty);
        }
        [TestCase(2f,1.4f)] [TestCase(.1f,0f)]
        public void WallImpact_AppliesMultiplicativePresetFrictionImpulseWithoutReversal(float tangent,float expected)
        {
            using var sim=Create(Params(0,10,friction:.2f));sim.Reset(State(new Vector3(.12f,2,5),new Vector3(-2,0,tangent)));var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(sim.State.Velocity.x,Is.EqualTo(1));Assert.That(sim.State.Velocity.z,Is.EqualTo(expected).Within(1e-6));Assert.That(cs[0].ApproachVelocity.z,Is.EqualTo(tangent));
        }
        [Test] public void FloorImpact_ReducesTangentialSpeedByCoulombImpulse()
        {
            using var sim=Create(Params(0,10,friction:.2f));sim.Reset(State(new Vector3(3,.12f,5),new Vector3(1,-2,0)));var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(sim.State.Velocity.x,Is.EqualTo(.4f).Within(1e-6));Assert.That(sim.State.Velocity.y,Is.EqualTo(1));
        }
        [Test] public void SimultaneousFriction_UsesJointTangentAndDoesNotAlterReflectedNormals()
        {
            using var sim=Create(Params(0,10,friction:.2f));sim.Reset(State(new Vector3(.07f,.07f,5),new Vector3(-1,-1,3)));var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(sim.State.Velocity.x,Is.EqualTo(.5f));Assert.That(sim.State.Velocity.y,Is.EqualTo(.5f));Assert.That(sim.State.Velocity.z,Is.EqualTo(2.4f).Within(1e-6));Assert.That(cs[0].ApproachVelocity,Is.EqualTo(cs[1].ApproachVelocity));
        }
        [Test] public void Configuration_IsCopiedAtCreateIncludingNestedMaterialsAndCourt()
        {
            var p=Params(0,10);var d=new CourtDimensions();float wall=d.Width;using var sim=Create(p,d);
            p.Gravity=10;p.StepHz=60;p.SideWalls.Restitution=1;p.SideWalls.TangentialFriction=1;d.Width=10;
            sim.Reset(State(new Vector3(wall-.07f,2,5),Vector3.right));var cs=new List<ContactEvent>();sim.Step(cs);
            Assert.That(cs.Count,Is.EqualTo(1));Assert.That(cs[0].Surface,Is.EqualTo(SurfaceId.LeftWall));Assert.That(sim.State.Velocity,Is.EqualTo(new Vector3(-.5f,0,0)));Assert.That(sim.State.Position.y,Is.EqualTo(2));
        }
        [Test] public void Factory_RejectsIncompleteOrNonfiniteConfigurationWithoutLeaking()
        {
            var f=new AnalyticBallSimulationFactory(.1f);Assert.Throws<ArgumentNullException>(()=>f.Create(null,new CourtDimensions()));Assert.Throws<ArgumentNullException>(()=>f.Create(Params(),null));
            var p=Params();p.MaxImpactsPerStep=0;Assert.Throws<ArgumentException>(()=>f.Create(p,new CourtDimensions()));
            p=Params(float.NaN);Assert.Throws<ArgumentException>(()=>f.Create(p,new CourtDimensions()));
            p=Params();p.Floor.TangentialFriction=float.NaN;Assert.Throws<ArgumentException>(()=>f.Create(p,new CourtDimensions()));
            Assert.Throws<ArgumentOutOfRangeException>(()=>new AnalyticBallSimulationFactory(-1));Assert.That(f.LiveInstances,Is.Zero);
        }
        [Test] public void SuppliedBounceThreshold_IsEffectiveAndDoesNotChangeGlobalPhysics()
        {
            var gravity=UnityEngine.Physics.gravity;var threshold=UnityEngine.Physics.bounceThreshold;
            var f=new AnalyticBallSimulationFactory(.3f);using var sim=f.Create(Params(0,10),new CourtDimensions());sim.Reset(State(new Vector3(.02f,2,5),new Vector3(-.2f,0,0)));sim.Step(new List<ContactEvent>());
            Assert.That(sim.State.Velocity.x,Is.Zero);Assert.That(UnityEngine.Physics.gravity,Is.EqualTo(gravity));Assert.That(UnityEngine.Physics.bounceThreshold,Is.EqualTo(threshold));
        }
    }
}
