using System;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // Volley v2.1 (design 2026-11-20 section 6): the late touch between S and S' chooses the volley with a sprint.
    public sealed class VolleyV21Tests
    {
        private static RallyContext Sprint(double max)=>RallyFixtures.Context(new RallyParams{HumanVolleyMode=3,HumanVolleySprintMax=max});
        private static (Trajectory t,PointRules rules,PlayerBody body,int delay) NeutralReception(RallyContext ctx)
        {
            var t=ctx.Predict(ctx.NeutralLaunch(PlayerId.Human));var rules=ctx.NewRules();rules.Begin(null,PlayerId.Human);
            var body=RallyFixtures.Body(ctx,PlayerId.Human,ctx.WaitPosition(PlayerId.Human));int delay=ctx.Steps(ctx.Params.ReactionS);
            var plan=ReceptionPlanner.Plan(ctx,t,rules,body,delay,1);body.MoveTo(plan.Stance,delay,BodyPhase.Approaching,BodyPhase.Set);
            return (t,rules,body,delay);
        }
        private static PlayerBody Walk(PlayerBody start,long from,long to){var b=start.Clone();for(long c=from;c<to;c++)b.Step();return b;}

        [Test] public void Body_SprintOne_IsTheSameBody_AndAboveOneIsFaster()
        {
            var ctx=RallyFixtures.Context();var a=RallyFixtures.Body(ctx,PlayerId.Human,new Vector2(1,8));var b=a.Clone();b.Sprint=1;
            a.MoveTo(new Vector2(5,3),0,BodyPhase.Approaching,BodyPhase.Set);b.MoveTo(new Vector2(5,3),0,BodyPhase.Approaching,BodyPhase.Set);
            for(int i=0;i<200;i++){a.Step();b.Step();Assert.That(b.Position,Is.EqualTo(a.Position));}
            var slow=RallyFixtures.Body(ctx,PlayerId.Human,new Vector2(1,8));var fast=slow.Clone();fast.Sprint=1.5f;
            Assert.That(fast.Clone().Sprint,Is.EqualTo(1.5f),"a clone keeps the sprint");
            Assert.That(fast.StepsToReach(new Vector2(5,3),0,out _),Is.LessThan(slow.StepsToReach(new Vector2(5,3),0,out _)));
            Assert.Throws<ArgumentOutOfRangeException>(()=>slow.Sprint=.9f);
        }
        [Test] public void SprintDeadline_IsLaterThanS_TheAmberLastsUntilIt_AndOnlyTheSprintConvertsItsLastTouch()
        {
            var ctx=Sprint(1.5);var (t,rules,body,delay)=NeutralReception(ctx);long start=t.States[0].Step;
            var d=HumanVolley.OfferDetail(ctx,t,rules,body,delay,1);
            Assert.That(d.Offer.Exists,Is.True,d.Offer.Reason);
            Assert.That(d.SPrime,Is.GreaterThan(d.S),"the sprint gives the player more time to choose");
            Assert.That(d.Offer.SplitSteps,Is.EqualTo((double)d.SPrime),"the amber ring lasts until S'");
            var late=Walk(body,start,d.SPrime);int remaining=(int)Math.Max(0,start+delay-d.SPrime);
            var normal=late.Clone();int walkArrival=normal.StepsToReach(d.Intercept.Stance,remaining,out _,(int)(d.Intercept.Plan.TStarStep-d.SPrime)+1);
            Assert.That(walkArrival==int.MaxValue||d.SPrime+walkArrival>d.Intercept.Plan.TStarStep,Is.True,"at S' the normal body no longer reaches the offer stance");
            var commit=HumanVolley.Commit(ctx,t,rules,late,d.SPrime,delay,2,ctx.HumanVolleySprint);
            Assert.That(commit.Exists,Is.True,commit.Reason);
            Assert.That(HumanVolley.Lateness(d.S,d.SPrime,d.SPrime),Is.EqualTo(1));Assert.That(HumanVolley.Lateness(d.S,d.SPrime,d.S),Is.EqualTo(0));
        }
        // VOLLEY-V2-COMMIT-01: a late touch must still see the offered stance (candidates are sampled from launch + reaction, like the offer).
        [Test] public void LateTouch_StillReachingTheOfferedStance_CommitsToThatSameStance()
        {
            foreach(var ctx in new[]{RallyFixtures.Context(new RallyParams{HumanVolleyMode=2}),Sprint(1.5)})
            {
                var (t,rules,body,delay)=NeutralReception(ctx);long start=t.States[0].Step;
                var d=HumanVolley.OfferDetail(ctx,t,rules,body,delay,1);Assert.That(d.Offer.Exists,Is.True,d.Offer.Reason);
                Assert.That(d.S,Is.GreaterThan(start+delay),"the touch at S comes after launch + reaction");
                var atS=HumanVolley.Commit(ctx,t,rules,Walk(body,start,d.S),d.S,delay,2);
                Assert.That(atS.Exists,Is.True,atS.Reason);Assert.That(atS.Stance,Is.EqualTo(d.Intercept.Stance),"the touch at S keeps the offered stance");
                Assert.That(atS.Plan.TStarStep,Is.EqualTo(d.Intercept.Plan.TStarStep));
                if(d.SPrime>d.S)
                {
                    var atSPrime=HumanVolley.Commit(ctx,t,rules,Walk(body,start,d.SPrime),d.SPrime,delay,2,ctx.HumanVolleySprint);
                    Assert.That(atSPrime.Exists,Is.True,atSPrime.Reason);Assert.That(atSPrime.Stance,Is.EqualTo(d.Intercept.Stance),"the sprint at S' keeps the offered stance");
                }
            }
        }
        // The same invariant over real rally receptions: wherever an offer exists, a touch at S (normal body) and at S' (sprint) still commits
        // to the offered stance, because S and S' are by definition the last touches that reach it.
        private sealed class InvariantProbe : IRallyBrain
        {
            private readonly RallyContext ctx;private readonly IRallyBrain inner;public RallySim Sim;public int Offers,LostAtS,LostAtSPrime;
            public InvariantProbe(RallyContext ctx,IRallyBrain inner){this.ctx=ctx;this.inner=inner;}
            public int PerceptionSteps=>inner.PerceptionSteps;public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                var t=ctx.Predict(o.Ball);long start=t.States[0].Step;int delay=ctx.Steps(ctx.Params.ReactionS)+inner.PerceptionSteps;
                var d=HumanVolley.OfferDetail(ctx,t,Sim.Rules,Sim.Human,delay,1);
                if(d.Offer.Exists)
                {
                    Offers++;
                    var atS=HumanVolley.Commit(ctx,t,Sim.Rules,Walk(Sim.Human,start,d.S),d.S,delay,2);
                    if(!atS.Exists||atS.Stance!=d.Intercept.Stance)LostAtS++;
                    var atSPrime=HumanVolley.Commit(ctx,t,Sim.Rules,Walk(Sim.Human,start,d.SPrime),d.SPrime,delay,2,ctx.HumanVolleySprint);
                    if(!atSPrime.Exists||atSPrime.Stance!=d.Intercept.Stance)LostAtSPrime++;
                }
                return inner.Decide(o,own);
            }
        }
        [Test] public void RallyReceptions_ATouchAtSOrSPrime_AlwaysKeepsTheOfferedStance()
        {
            var ctx=RallyFixtures.Context(new RallyParams{HumanVolleyMode=3,HumanVolleySprintMax=1.5});
            var probe=new InvariantProbe(ctx,new BotBrain(ctx,30,20261122));
            using(var sim=new RallySim(ctx,probe,RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Fondista,20261123))){probe.Sim=sim;sim.PlayMatch(PlayerId.Rival);}
            Assert.That(probe.Offers,Is.GreaterThan(10));
            Assert.That(probe.LostAtS,Is.Zero,"offers whose own deadline touch lost the offered stance (of "+probe.Offers+")");
            Assert.That(probe.LostAtSPrime,Is.Zero,"offers whose sprint deadline touch lost the offered stance (of "+probe.Offers+")");
        }
        [Test] public void V2_HasNoSprint_SplitIsS()
        {
            var ctx=RallyFixtures.Context(new RallyParams{HumanVolleyMode=2,HumanVolleySprintMax=1.5});var (t,rules,body,delay)=NeutralReception(ctx);
            Assert.That(ctx.HumanVolleySprint,Is.EqualTo(1f));
            var d=HumanVolley.OfferDetail(ctx,t,rules,body,delay,1);Assert.That(d.Offer.Exists,Is.True,d.Offer.Reason);
            Assert.That(d.SPrime,Is.EqualTo(d.S));Assert.That(d.Offer.SplitSteps,Is.EqualTo((double)d.S));
        }
    }
}
