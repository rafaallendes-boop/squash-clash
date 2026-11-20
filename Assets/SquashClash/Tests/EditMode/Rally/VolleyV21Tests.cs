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
        [Test] public void V2_HasNoSprint_SplitIsS()
        {
            var ctx=RallyFixtures.Context(new RallyParams{HumanVolleyMode=2,HumanVolleySprintMax=1.5});var (t,rules,body,delay)=NeutralReception(ctx);
            Assert.That(ctx.HumanVolleySprint,Is.EqualTo(1f));
            var d=HumanVolley.OfferDetail(ctx,t,rules,body,delay,1);Assert.That(d.Offer.Exists,Is.True,d.Offer.Reason);
            Assert.That(d.SPrime,Is.EqualTo(d.S));Assert.That(d.Offer.SplitSteps,Is.EqualTo((double)d.S));
        }
    }
}
