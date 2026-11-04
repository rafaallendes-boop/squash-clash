using System;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class ShotSolverTests
    {
        private EffectiveConfig config;
        private IBallSimulationFactory factory;
        [SetUp] public void Setup() {config=EffectiveConfig.From(ConfigFixtures.Default());factory=new AnalyticBallSimulationFactory(0);}
        private ShotSolution Solve(Vector3 p,Vector2 aim,float power=0,float min=6,float max=28,float cap=32)=>
            ShotSolver.Solve(new ShotRequest(p,aim,power),min,max,cap,config.Physics.Gravity,.02f,config.Court,factory,config.Physics);
        private ContactEvent Verify(Vector3 from,ShotSolution solution)
        {
            Assert.That(solution.Solved,Is.True,solution.Reason);
            using(var sim=factory.Create(config.Physics,config.Court))
            {
                var t=TrajectoryRunner.Run(sim,new BallState{Position=from,Velocity=solution.Velocity},config.Physics.StepHz*4,StopCondition.SecondFloorBounce,config.Court);
                Assert.That(t.Contacts.Count,Is.GreaterThan(0));Assert.That(t.Contacts[0].Surface,Is.EqualTo(SurfaceId.FrontWall));return t.Contacts[0];
            }
        }
        [Test] public void Speed_IsTotalSpeed_vMinPlusPTimesRange()
        {
            foreach(float p in new[]{0f,.25f,.5f,1f})
            {var from=new Vector3(3.2f,1,1.2f);var s=Solve(from,new Vector2(3.2f,1),p);Assert.That(s.Solved,Is.True,s.Reason);Assert.That(s.Velocity.magnitude,Is.EqualTo(6+p*22).Within(.00003));Assert.That(s.SpeedRaised,Is.False);}
        }
        [Test] public void LowArc_IsChosen()
        {var s=Solve(new Vector3(3.2f,1,3),new Vector2(3.2f,1),.5f);Assert.That(s.Solved,Is.True);Assert.That(Math.Abs(s.Velocity.y/s.Velocity.z),Is.LessThan(.2));Assert.That(s.Velocity.z,Is.LessThan(0));}
        [Test] public void ReachesAimCenter_WithPhysicalRadius20mm_NotVisualScale()
        {var from=new Vector3(2,1,5);var s=Solve(from,new Vector2(4,2),.5f);var hit=Verify(from,s);Assert.That(Vector2.Distance(new Vector2(hit.Point.x,hit.Point.y),s.TargetUsed),Is.LessThan(.05f));Assert.That(hit.Point.z,Is.Zero.Within(.0001));}
        [Test] public void Infeasible_RaisesMinimumSpeedUpToCap()
        {var from=new Vector3(3.2f,.5f,5.5f);var s=Solve(from,new Vector2(3.2f,4),0,6,6,32);Assert.That(s.Solved,Is.True,s.Reason);Assert.That(s.SpeedRaised,Is.True);Assert.That(s.Speed,Is.GreaterThan(6).And.LessThanOrEqualTo(32));Assert.That(s.TargetLowered,Is.False);Verify(from,s);}
        [Test] public void InfeasibleAtCap_LowersTargetKeepingX()
        {var from=new Vector3(3.2f,1,3);var s=Solve(from,new Vector2(3.2f,4.4f),0,6,6,6);Assert.That(s.Solved,Is.True,s.Reason);Assert.That(s.TargetLowered,Is.True);Assert.That(s.TargetUsed.x,Is.EqualTo(3.2f));Assert.That(s.TargetUsed.y,Is.LessThan(4.4f).And.GreaterThan(.5f));Assert.That(s.Speed,Is.LessThanOrEqualTo(6));Verify(from,s);}
        [Test] public void NearDoubleRoot_IsStable()
        {var from=new Vector3(3.2f,1,3);float critical=Mathf.Sqrt(9.81f*2.98f);var s=Solve(from,new Vector2(3.2f,1),0,critical,critical,32);Assert.That(s.Solved,Is.True,s.Reason);Assert.That(float.IsNaN(s.Velocity.y),Is.False);Verify(from,s);}
        [Test] public void VerticalOrZeroHorizontalDistance_IsHandled()
        {var s=Solve(new Vector3(3.2f,1,.02f),new Vector2(3.2f,1));Assert.That(s.Solved,Is.False);Assert.That(s.Reason,Is.Not.Null.And.Not.Empty);Assert.That(float.IsNaN(s.Velocity.x),Is.False);}
        [Test] public void VerifiedWithP2_NoFloorOrSideBeforeFrontWall()
        {foreach(float x in new[]{.1f,3.2f,6.3f}){var from=new Vector3(3.2f,.4f,8);var s=Solve(from,new Vector2(x,.6f),.3f);Verify(from,s);}var bad=Solve(new Vector3(3.2f,1,3),new Vector2(8,2),1);Assert.That(bad.Solved,Is.False);Assert.That(bad.Reason,Is.Not.Empty);}
        [Test] public void SolveCenterTo_AndL1toL6_AreUnchanged()
        {var v=Ballistics.SolveCenterTo(new Vector3(3.2f,1,7),new Vector3(3.2f,2,.02f),15,9.81f);Assert.That(v.z,Is.EqualTo(-15));Assert.That(v.x,Is.Zero);Assert.That(v.y,Is.EqualTo((2-1)/(6.98f/15)+4.905f*(6.98f/15)).Within(.00001));var launches=LaunchPresets.All(config.Court,config.Physics.Gravity);Assert.That(launches.Select(l=>l.Id),Is.EqualTo(new[]{"L1","L2","L3","L4","L5","L6"}));Assert.That(launches[1].Launch.Position,Is.EqualTo(new Vector3(.3f,.35f,4)));}
        [Test] public void NoSolutionEvenLoweringY_ReturnsUnsolvedWithReason()
        {var s=Solve(new Vector3(3.2f,.02f,9),new Vector2(3.2f,4),0,2,2,2);Assert.That(s.Solved,Is.False);Assert.That(s.Reason,Is.Not.Null.And.Not.Empty);}
        [Test] public void SameRequest_SameSolutionBits()
        {var from=new Vector3(2,.5f,7);var a=Solve(from,new Vector2(5,3),.2f);var b=Solve(from,new Vector2(5,3),.2f);Assert.That(a.Solved,Is.True);Assert.That(BitConverter.SingleToInt32Bits(a.Velocity.x),Is.EqualTo(BitConverter.SingleToInt32Bits(b.Velocity.x)));Assert.That(BitConverter.SingleToInt32Bits(a.Velocity.y),Is.EqualTo(BitConverter.SingleToInt32Bits(b.Velocity.y)));Assert.That(BitConverter.SingleToInt32Bits(a.Velocity.z),Is.EqualTo(BitConverter.SingleToInt32Bits(b.Velocity.z)));}
        [Test] public void ApprovedSpeedKeys_ArePresentAndUnchanged()
        {var p=ConfigFixtures.Default();Assert.That(p.Ordered.Count,Is.EqualTo(109));Assert.That(p.Get("shot.vMin"),Is.EqualTo(6));Assert.That(p.Get("shot.vMax"),Is.EqualTo(28));Assert.That(p.Get("shot.vCap"),Is.EqualTo(32));}
    }
}
