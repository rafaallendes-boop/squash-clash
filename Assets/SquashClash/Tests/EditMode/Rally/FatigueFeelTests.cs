using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // Fatigue feel (design 2026-11-20-fatigue-feel): A, the human's energy limits the late-volley sprint; B, «Cansancio fuerte» (DEV only).
    public sealed class FatigueFeelTests
    {
        private static (Trajectory t,PointRules rules,PlayerBody body,int delay) NeutralReception(RallyContext ctx)
        {
            var t=ctx.Predict(ctx.NeutralLaunch(PlayerId.Human));var rules=ctx.NewRules();rules.Begin(null,PlayerId.Human);
            var body=RallyFixtures.Body(ctx,PlayerId.Human,ctx.WaitPosition(PlayerId.Human));int delay=ctx.Steps(ctx.Params.ReactionS);
            var plan=ReceptionPlanner.Plan(ctx,t,rules,body,delay,1);body.MoveTo(plan.Stance,delay,BodyPhase.Approaching,BodyPhase.Set);
            return (t,rules,body,delay);
        }
        private sealed class Toucher : IRallyBrain,IRallyVolleyToucher
        {
            private readonly BotBrain inner;private readonly double leadMs;
            public Toucher(RallyContext ctx,long seed,double leadMs){inner=new BotBrain(ctx,30,seed);this.leadMs=leadMs;}
            public int PerceptionSteps=>inner.PerceptionSteps;public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)=>inner.Decide(o,own);
            public double TouchMsAfterLaunch(in ReceptionPlan own,double bounceTStarMsAfterLaunch)=>bounceTStarMsAfterLaunch-leadMs;
        }

        [Test] public void SprintAt_IsFullAtOrAboveTheOnset_NoneAtEnergyZero_AndLinearBetween()
        {
            var ctx=RallyFixtures.Context();Assert.That(ctx.HumanVolleySprint,Is.EqualTo(2f));Assert.That(ctx.Params.HumanVolleySprintFatigue,Is.EqualTo(1));
            Assert.That(ctx.HumanVolleySprintAt(0),Is.EqualTo(2f),"f = 0 is exactly today's sprint");
            Assert.That(ctx.HumanVolleySprintAt(.5),Is.EqualTo(1.5f).Within(1e-6));
            Assert.That(ctx.HumanVolleySprintAt(.25),Is.EqualTo(1.75f).Within(1e-6));
            Assert.That(ctx.HumanVolleySprintAt(1),Is.EqualTo(1f),"no sprint at energy 0");
            Assert.That(ctx.HumanVolleySprintAt(60/70.0),Is.LessThan(1.15f),"below energy 10 the sprint is under 1.15x");
            var off=RallyFixtures.Context(new RallyParams{HumanVolleySprintFatigue=0});
            Assert.That(off.HumanVolleySprintAt(1),Is.EqualTo(2f),"DEV off: v2.2 as shipped");
            var v2=RallyFixtures.Context(new RallyParams{HumanVolleyMode=2});Assert.That(v2.HumanVolleySprintAt(0),Is.EqualTo(1f),"v2 has no sprint at all");
        }
        [Test] public void OfferDetail_Exhausted_ClosesTheSprintStretch_AndFreshIsBitForBitToday()
        {
            var ctx=RallyFixtures.Context();var (t,rules,body,delay)=NeutralReception(ctx);
            var today=HumanVolley.OfferDetail(ctx,t,rules,body,delay,1);
            Assert.That(today.Offer.Exists,Is.True,today.Offer.Reason);Assert.That(today.SPrime,Is.GreaterThan(today.S),"control: the neutral feed has a sprint stretch");
            var fresh=HumanVolley.OfferDetail(ctx,t,rules,body,delay,1,0);
            Assert.That((fresh.S,fresh.SPrime,fresh.Offer.SplitSteps,fresh.Intercept.Stance),Is.EqualTo((today.S,today.SPrime,today.Offer.SplitSteps,today.Intercept.Stance)));
            var empty=HumanVolley.OfferDetail(ctx,t,rules,body,delay,1,1);
            Assert.That(empty.Offer.Exists,Is.True,"the volley itself is still offered");Assert.That(empty.S,Is.EqualTo(today.S));
            Assert.That(empty.SPrime,Is.EqualTo(empty.S),"at energy 0 there is no sprint stretch: the amber ring ends at S");
            Assert.That(empty.Offer.SplitSteps,Is.EqualTo((double)empty.S));
            var half=HumanVolley.OfferDetail(ctx,t,rules,body,delay,1,.5);
            Assert.That(half.SPrime,Is.InRange(today.S,today.SPrime));Assert.That(half.SPrime,Is.LessThan(today.SPrime),"half the sprint, a shorter stretch");
            var off=RallyFixtures.Context(new RallyParams{HumanVolleySprintFatigue=0});
            Assert.That(HumanVolley.OfferDetail(off,t,rules,body,delay,1,1).SPrime,Is.EqualTo(today.SPrime),"DEV off ignores the energy");
        }
        [Test] public void RallySim_TheSprintOfEveryChosenVolley_FollowsTheReceptionEnergy()
        {
            var ctx=RallyFixtures.Context();Assert.That(ctx.Fatigue.Enabled,Is.True);
            var used=new Dictionary<double,List<(float factor,double f)>>{{100,new List<(float,double)>()},{0,new List<(float,double)>()}};
            foreach(var energy in used.Keys.ToArray())
                for(int i=0;i<3;i++)
                    using(var sim=new RallySim(ctx,new Toucher(ctx,20261140+2*i,850),RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Fondista,20261141+2*i)))
                        for(int k=0;k<12;k++)
                        {
                            sim.SetEnergy(energy,100);var o=sim.PlayPoint(k%2==0?PlayerId.Rival:PlayerId.Human,k%4<2?ServiceBox.Right:ServiceBox.Left);
                            foreach(var r in o.Receptions.Where(r=>r.Who==PlayerId.Human&&r.Volley))used[energy].Add((r.VolleySprintFactor,r.FatigueReception));
                        }
            Assert.That(used[100].Count(u=>u.factor==2f),Is.GreaterThan(0),"fresh, some late volleys sprint at 2x ("+used[100].Count+" volleys)");
            Assert.That(used[0].Count,Is.GreaterThan(0),"exhausted, the human still volleys");
            Assert.That(used[0].Max(u=>u.factor),Is.LessThan(1.05f),"exhausted, no real sprint");
            foreach(var u in used[100].Concat(used[0]).Where(u=>u.factor>1))
                Assert.That(u.factor,Is.EqualTo(ctx.HumanVolleySprintAt(u.f)),"a sprint is always the reception's own energy-limited factor");
        }
        // Control (passes before and after): a human brain that does not touch never reaches the sprint, so the rally is bit for bit the same.
        [Test] public void Control_NonTouchingHuman_RallyIsBitForBit_WithTheRuleOnOrOff()
        {
            var on=RallyFixtures.Context();var off=RallyFixtures.Context(new RallyParams{HumanVolleySprintFatigue=0});
            foreach(RivalStyleId style in Enum.GetValues(typeof(RivalStyleId)))
            {
                MatchOutcome Play(RallyContext c){using(var sim=new RallySim(c,new BotBrain(c,90,20261150),RivalAi.For(c,AiLevel.Medium,style,20261151)))return sim.PlayMatch(PlayerId.Human);}
                var a=Play(on);var b=Play(off);
                Assert.That((a.Human,a.Rival,a.Points.Count),Is.EqualTo((b.Human,b.Rival,b.Points.Count)));
                for(int i=0;i<a.Points.Count;i++)
                {
                    var x=a.Points[i];var y=b.Points[i];
                    Assert.That((x.Winner,x.Shots,x.Steps,x.HumanFatigue.End,x.RivalFatigue.End),Is.EqualTo((y.Winner,y.Shots,y.Steps,y.HumanFatigue.End,y.RivalFatigue.End)),style+" point "+i);
                    Assert.That(x.Receptions.Select(r=>(r.Who,r.Result,r.ContactStep,r.DeltaMs)),Is.EqualTo(y.Receptions.Select(r=>(r.Who,r.Result,r.ContactStep,r.DeltaMs))));
                }
            }
        }
        [Test] public void Strong_MultipliesLegsAndReaction_ForTheRallyOnly_AndClampsToTheCatalog()
        {
            var source=ConfigFixtures.Default();var strong=FatigueSettings.Strong(source);
            Assert.That(strong.Get("fatigue.legsMax"),Is.EqualTo(.3).Within(1e-12));Assert.That(strong.Get("fatigue.reactionMaxMs"),Is.EqualTo(80).Within(1e-12));
            foreach(var key in source.Ordered.Select(kv=>kv.Key).Where(k=>k!="fatigue.legsMax"&&k!="fatigue.reactionMaxMs"))Assert.That(strong.Get(key),Is.EqualTo(source.Get(key)),key);
            var b43=FatigueSettings.Strong(source,4,3);
            Assert.That(b43.Get("fatigue.legsMax"),Is.EqualTo(.4).Within(1e-12));Assert.That(b43.Get("fatigue.reactionMaxMs"),Is.EqualTo(120).Within(1e-12));
            Assert.That(FatigueSettings.Strong(source,5,3).Get("fatigue.legsMax"),Is.EqualTo(.4),"clamped to the catalog maximum");
            Assert.That(EffectiveConfig.From(strong).SimulationKey,Is.EqualTo(EffectiveConfig.From(source).SimulationKey),"no physics key moves");
            var ctx=new RallyContext(EffectiveConfig.From(strong),new RallyParams(),new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
            var e=FatigueEffects.For(1,ctx.Fatigue);
            Assert.That(e.MovementFactor,Is.EqualTo(.7f).Within(1e-6),"30 % slower at energy 0");Assert.That(e.ExtraSteps,Is.EqualTo((int)Math.Round(.08*ctx.StepHz)));
            Assert.That(FatigueEffects.For(0,ctx.Fatigue).MovementFactor,Is.EqualTo(1f),"no effect at or above the onset");
        }
        // Control: the projection of RallyParams without the new field is the current hash (passes before and after).
        [Test] public void RallyParams_ProjectionWithoutTheSprintFatigueField_IsTheCurrentHash()
        {
            Assert.That(RivalAiTests.Projection(f=>f.Name!="HumanVolleySprintFatigue"),Is.EqualTo("0fe523164363"));
            TestContext.WriteLine("FATIGUE_FEEL_RALLYPARAMS_HASH="+new RallyParams().Hash);
        }
    }
}
