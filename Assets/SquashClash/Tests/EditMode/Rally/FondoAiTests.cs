using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class ShotOptionsTests
    {
        private static readonly Vector3 Contact=new Vector3(1.6f,.8f,6.5f);
        [Test] public void Build_GivesEighteenIntentions_InsideTheLegalFrontWall()
        {
            var context=RallyFixtures.Context();var options=ShotOptions.Build(context,Contact);
            Assert.That(options.Count,Is.EqualTo(18));
            Assert.That(options.Select(o=>(o.Line,o.Height,o.Power)).Distinct().Count(),Is.EqualTo(18));
            float r=context.Physics.BallRadius;
            foreach(var o in options)
            {
                Assert.That(o.Aim.y,Is.GreaterThan(context.Court.TinTop+r));Assert.That(o.Aim.y,Is.LessThan(context.Court.FrontOutLower-r));
                Assert.That(o.Aim.x,Is.InRange(r,context.Court.Width-r));Assert.That(o.P,Is.InRange(0f,1f));
            }
        }
        [Test] public void Lines_GoWhereTheirNameSays()
        {
            var context=RallyFixtures.Context();var options=ShotOptions.Build(context,Contact);float mid=context.Court.Width*.5f;
            foreach(var o in options)
            {
                if(o.Line==ShotLine.Straight){Assert.That(o.Zone.x,Is.LessThan(mid));Assert.That(o.Aim.x,Is.LessThan(Contact.x));}
                if(o.Line==ShotLine.Cross){Assert.That(o.Zone.x,Is.GreaterThan(mid));Assert.That(o.Aim.x,Is.GreaterThan(Contact.x));}
                if(o.Line==ShotLine.Drop){Assert.That(o.Zone.y,Is.LessThan(context.T.y));Assert.That(o.P,Is.LessThan(.4f));Assert.That(o.Aim.y,Is.LessThan(1.4f));}
                else Assert.That(o.Zone.y,Is.GreaterThan(context.T.y));
            }
            var straight=options.Where(o=>o.Line==ShotLine.Straight&&o.Power==ShotPower.Medium).OrderBy(o=>o.Height).Select(o=>o.Aim.y).ToArray();
            Assert.That(straight,Is.Ordered.Ascending);
            Assert.That(options.Where(o=>o.Power==ShotPower.Strong&&o.Line!=ShotLine.Drop).Min(o=>o.P),Is.GreaterThan(options.Where(o=>o.Power==ShotPower.Medium&&o.Line!=ShotLine.Drop).Max(o=>o.P)));
        }
        [Test] public void EveryOption_IsSolvable_FromMidCourt()
        {
            var context=RallyFixtures.Context();
            foreach(var o in ShotOptions.Build(context,Contact))
                Assert.That(RallyContact.Solve(context,Contact,o.Aim,o.P).Solved,Is.True,o.Line+"/"+o.Height+"/"+o.Power);
        }
        [Test] public void ClampAim_KeepsNoisyIntentionsInsideTheSolverDomain()
        {
            var context=RallyFixtures.Context();float r=context.Physics.BallRadius;
            var low=ShotOptions.ClampAim(context,new Vector2(-1,.1f));var high=ShotOptions.ClampAim(context,new Vector2(9,7));
            Assert.That(low.y,Is.GreaterThan(context.Court.TinTop+r));Assert.That(low.x,Is.GreaterThanOrEqualTo(r));
            Assert.That(high.y,Is.LessThan(context.Court.FrontOutLower-r));Assert.That(high.x,Is.LessThanOrEqualTo(context.Court.Width-r));
            Assert.That(RallyContact.Solve(context,Contact,low,.5f).Reason,Is.Not.EqualTo("target-or-contact-outside-court"));
        }
    }
    public sealed class FondoAiTests
    {
        private static ReceptionPlan OwnPlan(RallyContext context,double slack,Vector3? contact=null)
        {
            var point=contact??new Vector3(1.6f,.8f,6.5f);var stance=new Vector2(point.x+.6f,point.z);
            var plan=new ContactPlan(true,new SimTime(100),new SimTime(140),120,point,StrokeSide.Drive,1,context.FrameAt(stance),context.Legal,context.Court);
            return new ReceptionPlan(true,stance,stance,plan,90,slack,Mathf.Clamp01((float)(slack/context.Params.SlackCapS)),"planned");
        }
        private static AiObservation Observe(RallyContext context,Vector2 opponent,Vector2? self=null)
            =>new AiObservation(default,self??context.WaitPosition(PlayerId.Rival),Vector2.zero,opponent,Vector2.zero,3,0,false);
        private static FondoAi Ai(RallyContext context,AiLevel level=AiLevel.Medium,long seed=7)=>new FondoAi(context,AiDifficulty.For(context.Params,level),seed);
        private static List<AiShot> Sample(FondoAi ai,AiObservation observation,ReceptionPlan own,int count=3000)
        {var list=new List<AiShot>(count);for(int i=0;i<count;i++)list.Add(ai.Decide(observation,own));return list;}

        [Test] public void Fondo_PrefersLongShots()
        {
            var context=RallyFixtures.Context();
            var shots=Sample(Ai(context),Observe(context,context.WaitPosition(PlayerId.Human)),OwnPlan(context,.35));
            Assert.That(shots.All(s=>s.Swing),Is.True);
            Assert.That(shots.Count(s=>s.Option.Long)/(double)shots.Count,Is.GreaterThan(.6));
            Assert.That(shots.Count(s=>s.Option.Line==ShotLine.Drop)/(double)shots.Count,Is.LessThan(.05),"human at the T: no drop");
        }
        [Test] public void Fondo_DropsOnlyWithTheHumanBack_AndHighSlack()
        {
            var context=RallyFixtures.Context();var back=new Vector2(4.8f,8.6f);var front=new Vector2(3,2.5f);
            double Rate(Vector2 human,double slack)=>Sample(Ai(context),Observe(context,human),OwnPlan(context,slack,new Vector3(1.6f,.8f,3.5f))).Count(s=>s.Option.Line==ShotLine.Drop)/3000d;
            double allowed=Rate(back,.4),pressed=Rate(back,.15),humanFront=Rate(front,.4);
            Assert.That(allowed,Is.GreaterThan(.03));Assert.That(allowed,Is.LessThan(.5),"still a low weight");
            Assert.That(pressed,Is.LessThan(allowed/4));Assert.That(humanFront,Is.LessThan(allowed/4));
        }
        [Test] public void Fondo_HitsAwayFromTheHuman()
        {
            var context=RallyFixtures.Context();var own=OwnPlan(context,.35);
            var humanOnStraightSide=new Vector2(1.2f,7.5f);var humanOnCrossSide=new Vector2(5.4f,7.5f);
            double Cross(Vector2 human)=>Sample(Ai(context),Observe(context,human),own).Count(s=>s.Option.Line==ShotLine.Cross)/3000d;
            Assert.That(Cross(humanOnStraightSide),Is.GreaterThan(Cross(humanOnCrossSide)+.1));
        }
        [Test] public void MandatoryDefense_UnderPressure_IsHighAndLong()
        {
            var context=RallyFixtures.Context();
            var shots=Sample(Ai(context),Observe(context,context.WaitPosition(PlayerId.Human)),OwnPlan(context,.05),500);
            Assert.That(shots.All(s=>s.Defensive&&s.Option.Height==ShotHeight.High&&s.Option.Power==ShotPower.Medium&&s.Option.Line!=ShotLine.Drop),Is.True);
        }
        [Test] public void Temperature_HardConcentratesMoreThanEasy()
        {
            var context=RallyFixtures.Context();var own=OwnPlan(context,.35);var observation=Observe(context,context.WaitPosition(PlayerId.Human));
            double Top(AiLevel level)=>Sample(Ai(context,level),observation,own).GroupBy(s=>(s.Option.Line,s.Option.Height,s.Option.Power)).Max(g=>g.Count())/3000d;
            Assert.That(Top(AiLevel.Hard),Is.GreaterThan(Top(AiLevel.Medium)));Assert.That(Top(AiLevel.Medium),Is.GreaterThan(Top(AiLevel.Easy)));
            int distinct=Sample(Ai(context,AiLevel.Medium),observation,own).Select(s=>(s.Option.Line,s.Option.Height,s.Option.Power)).Distinct().Count();
            Assert.That(distinct,Is.GreaterThan(2),"variation among reasonable options, no fixed alternation");
        }
        [TestCase(AiLevel.Easy)]
        [TestCase(AiLevel.Medium)]
        [TestCase(AiLevel.Hard)]
        public void Noise_FollowsTheDifficulty(AiLevel level)
        {
            var context=RallyFixtures.Context();var difficulty=AiDifficulty.For(context.Params,level);var ai=Ai(context,level);
            var shots=Sample(ai,Observe(context,context.WaitPosition(PlayerId.Human)),OwnPlan(context,.35),6000);
            double mean=shots.Average(s=>s.DeltaMs),sigma=Math.Sqrt(shots.Average(s=>(s.DeltaMs-mean)*(s.DeltaMs-mean)));
            Assert.That(mean,Is.EqualTo(0).Within(difficulty.TimingSigmaMs*.08));Assert.That(sigma,Is.EqualTo(difficulty.TimingSigmaMs).Within(difficulty.TimingSigmaMs*.08));
            var mid=shots.Where(s=>s.Option.Height==ShotHeight.Mid&&s.Option.Line==ShotLine.Straight).ToArray();
            Assert.That(mid.Length,Is.GreaterThan(200));
            double aimMean=mid.Average(s=>s.Aim.y-s.Option.Aim.y),aimSigma=Math.Sqrt(mid.Average(s=>Math.Pow(s.Aim.y-s.Option.Aim.y-aimMean,2)));
            Assert.That(aimSigma,Is.EqualTo(difficulty.AimSigma).Within(difficulty.AimSigma*.2));
            var noise=Enumerable.Range(0,4000).Select(_=>ai.StanceNoise()).ToArray();
            Assert.That(Math.Sqrt(noise.Average(n=>(double)n.x*n.x)),Is.EqualTo(difficulty.StanceSigma).Within(difficulty.StanceSigma*.1+1e-9));
            Assert.That(ai.PerceptionSteps,Is.EqualTo(context.Steps(difficulty.PerceptionS)));
        }
        [Test] public void Difficulty_IsOrdered_AndMediumIsTheMiddle()
        {
            var p=new RallyParams();var easy=AiDifficulty.For(p,AiLevel.Easy);var medium=AiDifficulty.For(p,AiLevel.Medium);var hard=AiDifficulty.For(p,AiLevel.Hard);
            Assert.That(easy.TimingSigmaMs,Is.GreaterThan(medium.TimingSigmaMs));Assert.That(medium.TimingSigmaMs,Is.GreaterThan(hard.TimingSigmaMs));
            Assert.That(easy.AimSigma,Is.GreaterThan(medium.AimSigma));Assert.That(medium.AimSigma,Is.GreaterThan(hard.AimSigma));
            Assert.That(easy.PerceptionS,Is.GreaterThan(medium.PerceptionS));Assert.That(medium.PerceptionS,Is.GreaterThanOrEqualTo(hard.PerceptionS));
            Assert.That(easy.PerceptionS,Is.LessThanOrEqualTo(.08),"spec 11.2: d_perc within 0-80 ms");
            Assert.That(easy.Temperature,Is.GreaterThan(medium.Temperature));Assert.That(medium.Temperature,Is.GreaterThan(hard.Temperature));
        }
        [Test] public void SameSeed_SameDecisions_DifferentSeed_Different()
        {
            var context=RallyFixtures.Context();var own=OwnPlan(context,.35);var observation=Observe(context,context.WaitPosition(PlayerId.Human));
            var a=Sample(Ai(context,AiLevel.Medium,11),observation,own,50);var b=Sample(Ai(context,AiLevel.Medium,11),observation,own,50);var c=Sample(Ai(context,AiLevel.Medium,12),observation,own,50);
            Assert.That(a.Select(s=>(s.Aim,s.P,s.DeltaMs)),Is.EqualTo(b.Select(s=>(s.Aim,s.P,s.DeltaMs))));
            Assert.That(a.Select(s=>s.DeltaMs),Is.Not.EqualTo(c.Select(s=>s.DeltaMs)));
        }
        [Test] public void NoisyIntention_StaysInsideTheSolverDomain()
        {
            var context=RallyFixtures.Context();float r=context.Physics.BallRadius;
            foreach(var s in Sample(Ai(context,AiLevel.Easy),Observe(context,new Vector2(4.8f,8.6f)),OwnPlan(context,.4,new Vector3(1.6f,.8f,3.5f)),2000))
            {
                Assert.That(s.Aim.y,Is.GreaterThan(context.Court.TinTop+r));Assert.That(s.Aim.y,Is.LessThan(context.Court.FrontOutLower-r));
                Assert.That(s.Aim.x,Is.InRange(r,context.Court.Width-r));Assert.That(s.P,Is.InRange(0f,1f));
            }
        }
        [Test] public void WithoutAPlan_TheAiDoesNotSwing()
        {
            var context=RallyFixtures.Context();
            var none=new ReceptionPlan(false,Vector2.zero,Vector2.zero,default,0,0,0,"unreachable");
            Assert.That(Ai(context).Decide(Observe(context,context.WaitPosition(PlayerId.Human)),none).Swing,Is.False);
        }
        // Spec 11.2 / 17.A.9: the AI can only be handed observable data.
        [Test] public void InformationContract_ObservationHasOnlyObservableFields()
        {
            var allowed=new[]{"Ball","SelfPosition","SelfVelocity","OpponentPosition","OpponentVelocity","Shots","FloorBounces","FrontWallReached","ScoreSelf","ScoreOpponent"};
            var fields=typeof(AiObservation).GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).Select(f=>f.Name).ToArray();
            Assert.That(fields,Is.EquivalentTo(allowed));
            var types=typeof(AiObservation).GetFields(BindingFlags.Public|BindingFlags.Instance).Select(f=>f.FieldType).Distinct().ToArray();
            Assert.That(types,Is.SubsetOf(new[]{typeof(BallState),typeof(Vector2),typeof(int),typeof(bool)}));
        }
        [Test] public void InformationContract_DecideSeesOnlyTheObservationAndItsOwnPlan()
        {
            var decide=typeof(IRallyBrain).GetMethod(nameof(IRallyBrain.Decide));
            Assert.That(decide.GetParameters().Select(p=>p.ParameterType.GetElementType()??p.ParameterType),Is.EqualTo(new[]{typeof(AiObservation),typeof(ReceptionPlan)}));
            var forbidden=new[]{typeof(PlayerBody),typeof(PointRules),typeof(ClosedGesture),typeof(PresentedPlans),typeof(SwingIntent),typeof(TouchSample),typeof(IRallyBrain),typeof(Trajectory)};
            foreach(var field in typeof(FondoAi).GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static))
                Assert.That(forbidden,Has.No.Member(field.FieldType),field.Name+" could leak the opponent's private state or the future");
            Assert.That(typeof(AiDifficulty).GetFields(BindingFlags.Public|BindingFlags.Instance).Select(f=>f.Name),
                Is.EquivalentTo(new[]{"PerceptionS","StanceSigma","AimSigma","PowerSigma","TimingSigmaMs","Temperature"}));
        }
        [Test] public void InformationContract_SameObservation_SameDecision_WhateverTheOpponentPlans()
        {
            var context=RallyFixtures.Context();var own=OwnPlan(context,.35);var observation=Observe(context,new Vector2(2,7));
            // Two worlds that differ only in what the human privately intends cannot be told apart through AiObservation.
            var first=Ai(context,AiLevel.Medium,5).Decide(observation,own);var second=Ai(context,AiLevel.Medium,5).Decide(observation,own);
            Assert.That((second.Aim,second.P,second.DeltaMs,second.Option.Line),Is.EqualTo((first.Aim,first.P,first.DeltaMs,first.Option.Line)));
        }
    }
    // The AI hits through the human's contract (spec 11.3 step 3, 17.A.9).
    public sealed class RallyContactTests
    {
        private static (RallyContext context,Trajectory trajectory,ContactPlan plan,ClockMap clock) Setup()
        {
            var context=RallyFixtures.Context();var trajectory=context.Predict(context.NeutralLaunch(PlayerId.Rival));
            var body=RallyFixtures.Body(context,PlayerId.Rival,context.WaitPosition(PlayerId.Rival));
            var plan=ReceptionPlanner.Plan(context,trajectory,RallyFixtures.Rules(context,PlayerId.Human,PlayerId.Rival),body,18,1);
            Assert.That(plan.Reachable,Is.True,plan.Reason);
            var clock=new ClockMap();clock.Begin(1,0,0,context.Tempo,1,context.StepHz);
            return (context,trajectory,plan.Contact,clock);
        }
        [Test] public void OnTime_IsPerfect_AtTStar()
        {
            var s=Setup();var d=RallyContact.Resolve(s.context,s.clock,s.trajectory,s.plan,0,0);
            Assert.That(d.Result,Is.EqualTo(ShotResult.Perfect));Assert.That(d.ContactStep,Is.EqualTo(s.plan.TStarStep));
        }
        [Test] public void EarlyAndLate_InsideV_AreValid_AtTheReleaseInstant()
        {
            var s=Setup();
            var early=RallyContact.Resolve(s.context,s.clock,s.trajectory,s.plan,0,-70);var late=RallyContact.Resolve(s.context,s.clock,s.trajectory,s.plan,0,70);
            Assert.That(early.Result,Is.EqualTo(ShotResult.Valid));Assert.That(early.ContactStep,Is.LessThan(s.plan.TStarStep));
            Assert.That(late.Result,Is.EqualTo(ShotResult.Valid));Assert.That(late.ContactStep,Is.GreaterThan(s.plan.TStarStep));
        }
        [Test] public void OutsideV_Fails_LikeTheHuman()
        {
            var s=Setup();
            var air=RallyContact.Resolve(s.context,s.clock,s.trajectory,s.plan,0,-160);var late=RallyContact.Resolve(s.context,s.clock,s.trajectory,s.plan,0,160);
            Assert.That(air.Result,Is.EqualTo(ShotResult.Failed));Assert.That(air.Fail,Is.EqualTo(FailKind.Air));
            Assert.That(late.Result,Is.EqualTo(ShotResult.Failed));Assert.That(late.Fail,Is.EqualTo(FailKind.Late));
        }
        [Test] public void Resolve_IsExactlyTheSharedContract()
        {
            var s=Setup();Assert.That(s.clock.TrySimToReal(s.plan.TStarStep,0,out double target),Is.True);
            foreach(double delta in new[]{-120d,-70,-30,0,25,70,130})
            {
                var plans=new PresentedPlans();plans.Present(0,s.plan,target);
                var direct=ContactContract.Decide(target+delta/1000+s.context.Timing.LatencyCompMs/1000,plans,s.clock,s.trajectory,s.context.Timing,false);
                var via=RallyContact.Resolve(s.context,s.clock,s.trajectory,s.plan,0,delta);
                Assert.That((via.Result,via.Fail,via.ContactStep,via.Scheduled,via.DeltaMs),Is.EqualTo((direct.Result,direct.Fail,direct.ContactStep,direct.Scheduled,direct.DeltaMs)),"delta "+delta);
            }
        }
        [Test] public void NoPlan_IsUnreachable()
        {
            var s=Setup();var none=new ContactPlan(false,default,default,0,default,default,1,s.plan.Body,s.plan.Legal,s.plan.Court,"unreachable");
            Assert.That(RallyContact.Resolve(s.context,s.clock,s.trajectory,none,0,0).Result,Is.EqualTo(ShotResult.Unreachable));
        }
        [Test] public void Solve_UsesTheSharedSolverAndSpeedLimits()
        {
            var s=Setup();var contact=s.plan.PStar;var aim=new Vector2(3.2f,1.8f);
            var via=RallyContact.Solve(s.context,contact,aim,.5f);
            var direct=ShotSolver.Solve(new ShotRequest(contact,aim,.5f),s.context.VMin,s.context.VMax,s.context.VCap,s.context.Physics.Gravity,s.context.Physics.BallRadius,s.context.Court,s.context.Factory,s.context.Physics);
            Assert.That(via.Solved,Is.True);Assert.That(via.Velocity,Is.EqualTo(direct.Velocity));Assert.That(via.Speed,Is.EqualTo(direct.Speed));
        }
    }
}
