using System;
using System.Linq;
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
            // v2 semantics (offer-stance deadline); v2.2 picks nearer stances late on purpose (V22_ test), so it is not checked here.
            foreach(var ctx in new[]{RallyFixtures.Context(new RallyParams{HumanVolleyMode=2})})
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
        // The same invariant over real rally receptions: wherever an offer exists, a touch at S (normal body) still commits to the offered stance
        // and a touch at S' (sprint) to it or to a stance with a window at least as long, because S and S' are the last touches that reach it.
        private sealed class InvariantProbe : IRallyBrain
        {
            private readonly RallyContext ctx;private readonly IRallyBrain inner;public RallySim Sim;public int Offers,LostAtS,LostAtSPrime,Better;
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
                    if(!atSPrime.Exists)LostAtSPrime++;
                    // The sprint may also reach a stance with an even longer window than the offer's: never a shorter one.
                    else if(atSPrime.Stance!=d.Intercept.Stance){Better++;if(atSPrime.Plan.KOut.Steps-atSPrime.Plan.KIn.Steps<d.Intercept.Plan.KOut.Steps-d.Intercept.Plan.KIn.Steps)LostAtSPrime++;}
                }
                return inner.Decide(o,own);
            }
        }
        [Test] public void RallyReceptions_ATouchAtSOrSPrime_AlwaysKeepsTheOfferedStance()
        {
            // v2 (offer-stance deadlines); v2.2 is covered by V22_SPrimeIsTheLastTouchAnyStanceIsReachable_AndLateTouchesTakeANearerStance.
            var ctx=RallyFixtures.Context(new RallyParams{HumanVolleyMode=2});
            var probe=new InvariantProbe(ctx,new BotBrain(ctx,30,20261122));
            using(var sim=new RallySim(ctx,probe,RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Fondista,20261123))){probe.Sim=sim;sim.PlayMatch(PlayerId.Rival);}
            Assert.That(probe.Offers,Is.GreaterThan(10));
            Assert.That(probe.LostAtS,Is.Zero,"offers whose own deadline touch lost the offered stance (of "+probe.Offers+")");
            Assert.That(probe.LostAtSPrime,Is.Zero,"offers whose sprint deadline touch found no stance at least as good as the offered one (of "+probe.Offers+")");
            TestContext.WriteLine("V21_INVARIANT offers="+probe.Offers+" betterAtSPrime="+probe.Better);
        }
        // v2.2 (design section 7): S' is the last touch at which ANY stance is reachable with the sprint. Over a real match: a touch at S' commits,
        // a touch a little later never does, and some late touches commit to a nearer stance than the offer's (longest-window) one.
        private sealed class V22Probe : IRallyBrain
        {
            private readonly RallyContext ctx;private readonly IRallyBrain inner;public RallySim Sim;public int Offers,NoCommitAtSPrime,CommitAfterSPrime,NearerLate;
            public V22Probe(RallyContext ctx,IRallyBrain inner){this.ctx=ctx;this.inner=inner;}
            public int PerceptionSteps=>inner.PerceptionSteps;public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                var t=ctx.Predict(o.Ball);long start=t.States[0].Step;int delay=ctx.Steps(ctx.Params.ReactionS)+inner.PerceptionSteps;
                var d=HumanVolley.OfferDetail(ctx,t,Sim.Rules,Sim.Human,delay,1);
                if(d.Offer.Exists)
                {
                    Offers++;float sprint=ctx.HumanVolleySprint;
                    VolleyIntercept At(long c){var b=Walk(Sim.Human,start,c);var n=HumanVolley.Commit(ctx,t,Sim.Rules,b,c,delay,2);return n.Exists?n:HumanVolley.Commit(ctx,t,Sim.Rules,b,c,delay,2,sprint);}
                    if(!At(d.SPrime).Exists)NoCommitAtSPrime++;
                    foreach(var k in new[]{3,10,25})if(At(d.SPrime+k).Exists){CommitAfterSPrime++;break;}
                    var late=At(d.SPrime);if(late.Exists&&late.Stance!=d.Intercept.Stance&&late.Plan.KOut.Steps-late.Plan.KIn.Steps<d.Intercept.Plan.KOut.Steps-d.Intercept.Plan.KIn.Steps)NearerLate++;
                }
                return inner.Decide(o,own);
            }
        }
        [Test] public void V22_SPrimeIsTheLastTouchAnyStanceIsReachable_AndLateTouchesTakeANearerStance()
        {
            var ctx=Sprint(1.5);var probe=new V22Probe(ctx,new BotBrain(ctx,30,20261124));
            using(var sim=new RallySim(ctx,probe,RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Fondista,20261125))){probe.Sim=sim;sim.PlayMatch(PlayerId.Rival);}
            Assert.That(probe.Offers,Is.GreaterThan(10));
            Assert.That(probe.NoCommitAtSPrime,Is.Zero,"a touch at S' always commits (of "+probe.Offers+")");
            Assert.That(probe.CommitAfterSPrime,Is.Zero,"no touch after S' commits: S' is the last one (of "+probe.Offers+")");
            Assert.That(probe.NearerLate,Is.GreaterThan(0),"late touches commit to a nearer, shorter-window stance");
        }
        // v2.3 (design section 8): with the lunge, the window reaches later than v2.2's (a moving body may make contact), S' stays the last
        // touch that commits, and a lunge plan freezes the body where it is at the planned contact (not at a stance it has reached).
        private sealed class V23Probe : IRallyBrain
        {
            private readonly RallyContext ctx,v22;private readonly IRallyBrain inner;public RallySim Sim;public int Offers,Later,NoCommitAtSPrime,CommitAfterSPrime;
            public V23Probe(RallyContext ctx,RallyContext v22,IRallyBrain inner){this.ctx=ctx;this.v22=v22;this.inner=inner;}
            public int PerceptionSteps=>inner.PerceptionSteps;public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                var t=ctx.Predict(o.Ball);long start=t.States[0].Step;int delay=ctx.Steps(ctx.Params.ReactionS)+inner.PerceptionSteps;
                var d=HumanVolley.OfferDetail(ctx,t,Sim.Rules,Sim.Human,delay,1);var old=HumanVolley.OfferDetail(v22,t,Sim.Rules,Sim.Human,delay,1);
                if(d.Offer.Exists&&old.Offer.Exists)
                {
                    Offers++;if(d.SPrime>old.SPrime)Later++;float sprint=ctx.HumanVolleySprint;
                    VolleyIntercept At(long c){var b=Walk(Sim.Human,start,c);var n=HumanVolley.Commit(ctx,t,Sim.Rules,b,c,delay,2);return n.Exists?n:HumanVolley.Commit(ctx,t,Sim.Rules,b,c,delay,2,sprint);}
                    if(!At(d.SPrime).Exists)NoCommitAtSPrime++;
                    foreach(var k in new[]{3,10,25})if(At(d.SPrime+k).Exists){CommitAfterSPrime++;break;}
                }
                return inner.Decide(o,own);
            }
        }
        [Test] public void V23_LungeExtendsTheWindow_AndSPrimeIsStillTheLastTouch()
        {
            var ctx=RallyFixtures.Context(new RallyParams{HumanVolleyMode=4,HumanVolleySprintMax=1.5});var v22=Sprint(1.5);
            Assert.That(ctx.HumanVolleyLunge,Is.True);Assert.That(v22.HumanVolleyLunge,Is.False);
            var probe=new V23Probe(ctx,v22,new BotBrain(ctx,30,20261126));
            using(var sim=new RallySim(ctx,probe,RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Fondista,20261127))){probe.Sim=sim;sim.PlayMatch(PlayerId.Rival);}
            Assert.That(probe.Offers,Is.GreaterThan(10));
            Assert.That(probe.Later,Is.GreaterThan(probe.Offers/4),"the lunge lets later touches commit (of "+probe.Offers+")");
            Assert.That(probe.NoCommitAtSPrime,Is.Zero);Assert.That(probe.CommitAfterSPrime,Is.Zero);
        }
        // Headless v2.2: a brain that touches chooses the volley through RallySim exactly as RallyDirector does, and a sprint volley pays more.
        private sealed class Toucher : IRallyBrain,IRallyVolleyToucher
        {
            private readonly BotBrain inner;private readonly double leadMs;
            public Toucher(RallyContext ctx,long seed,double leadMs){inner=new BotBrain(ctx,30,seed);this.leadMs=leadMs;}
            public int PerceptionSteps=>inner.PerceptionSteps;public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)=>inner.Decide(o,own);
            public double TouchMsAfterLaunch(in ReceptionPlan own,double bounceTStarMsAfterLaunch)=>bounceTStarMsAfterLaunch-leadMs;
        }
        [Test] public void RallySim_ATouchingHuman_VolleysThroughTheSharedRules_AndASprintVolleyCostsMore()
        {
            // The v2.2 sprint as shipped (fatigue feel DEV off): with the default energy limit these three matches have no sprint volley at
            // all, since their energy falls below the onset (FatigueFeelTests covers that rule).
            var ctx=RallyFixtures.Context(new RallyParams{HumanVolleySprintFatigue=0});Assert.That(ctx.HumanVolleySprint,Is.EqualTo(2f));Assert.That(ctx.Fatigue.Enabled,Is.True);
            int volleys=0,faults=0,rejected=0;double volleyCost=0;
            for(int i=0;i<3;i++)
                using(var sim=new RallySim(ctx,new Toucher(ctx,20261128+2*i,850),RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Fondista,20261129+2*i)))
                    foreach(var o in sim.PlayMatch(PlayerId.Rival).Points)
                    {
                        faults+=o.SystemFaults;rejected+=o.RejectedContacts;volleyCost+=o.HumanFatigue.VolleyCost;
                        volleys+=o.Receptions.Count(r=>r.Who==PlayerId.Human&&r.Volley&&r.Result==ReceptionResult.Contact&&r.Vectors!=null);
                    }
            Assert.That(volleys,Is.GreaterThan(5),"a touching human volleys in three games");
            Assert.That(faults,Is.Zero);Assert.That(rejected,Is.Zero);
            Assert.That(volleyCost,Is.GreaterThan(2.0*volleys+1e-9),"some volleys were sprints and paid the extra cost");
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
