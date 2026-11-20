using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // Volley v2, human only (design 2026-11-20): the chosen intercept, its commit deadline, the human's return-of-serve volley, and the
    // rival bit for bit identical with v2 on or off.
    public sealed class VolleyV2Tests
    {
        private static ContactEvent Front()=>new ContactEvent(SurfaceId.FrontWall,new Vector3(3.2f,2.5f,0),Vector2.zero,CourtDimensions.InwardNormal(SurfaceId.FrontWall),1,0,ContactTiming.Exact,Vector3.zero,false);
        private static string Projection(Func<FieldInfo,bool> keep)
        {
            var canonical=new StringBuilder();
            foreach(var field in typeof(RallyParams).GetFields(BindingFlags.Public|BindingFlags.Instance).Where(keep).OrderBy(f=>f.Name,StringComparer.Ordinal))
                canonical.Append(field.Name).Append('=').Append(((double)field.GetValue(new RallyParams())).ToString("R",CultureInfo.InvariantCulture)).Append('\n');
            using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())),0,6).Replace("-","").ToLowerInvariant();
        }
        private static RallyContext V1()=>RallyFixtures.Context(new RallyParams{HumanVolleyMode=1,HumanVolleyServe=0});
        // The human's reception of the neutral feed, set up exactly as RallyDirector.LaunchToHuman does (M1 move already given).
        private static (RallyContext ctx,Trajectory t,PointRules rules,PlayerBody body,int delay) NeutralReception(RallyContext ctx=null)
        {
            ctx=ctx??RallyFixtures.Context();var t=ctx.Predict(ctx.NeutralLaunch(PlayerId.Human));
            var rules=ctx.NewRules();rules.Begin(null,PlayerId.Human);
            var body=RallyFixtures.Body(ctx,PlayerId.Human,ctx.WaitPosition(PlayerId.Human));int delay=ctx.Steps(ctx.Params.ReactionS);
            var plan=ReceptionPlanner.Plan(ctx,t,rules,body,delay,1);body.MoveTo(plan.Stance,delay,BodyPhase.Approaching,BodyPhase.Set);
            return (ctx,t,rules,body,delay);
        }
        private static long FirstBounce(Trajectory t)=>t.Contacts.First(c=>c.Surface==SurfaceId.Floor&&!c.Persistent).Step;

        [Test] public void Defaults_AreV2WithTheServeVolley_AndOnlyTheRallyParamsHashMoves()
        {
            var p=new RallyParams();
            // v2.2 ships (design section 8): mode 3, sprint 2.0, 4 points x lateness.
            Assert.That(p.HumanVolleyMode,Is.EqualTo(3));Assert.That(p.HumanVolleyMinWindowS,Is.EqualTo(.05));Assert.That(p.HumanVolleyServe,Is.EqualTo(1));
            Assert.That(p.HumanVolleySprintMax,Is.EqualTo(2));Assert.That(p.HumanVolleySprintCost,Is.EqualTo(4));
            // Six since the fatigue feel (20 nov): the five v2.x fields plus HumanVolleySprintFatigue (FatigueFeelTests checks its own projection).
            Assert.That(typeof(RallyParams).GetFields().Count(f=>f.Name.StartsWith("HumanVolley",StringComparison.Ordinal)),Is.EqualTo(6));
            Assert.That(Projection(f=>!f.Name.StartsWith("HumanVolley",StringComparison.Ordinal)&&!f.Name.StartsWith("ServeKind",StringComparison.Ordinal)),Is.EqualTo("0f58580f44ea"),"without the v2 fields the hash is the current one");
            Assert.That(RallyFixtures.Context().HumanVolleyV2,Is.True);Assert.That(RallyFixtures.Context().HumanServeVolley,Is.True);
            TestContext.WriteLine("VOLLEY_V2_RALLYPARAMS_HASH="+p.Hash);
        }
        [Test] public void ServeVolley_IsTheHumansOnly_AndFollowsBothSwitches()
        {
            var rules=RallyFixtures.Context().NewRules();rules.BeginServe(PlayerId.Rival,ServiceBox.Right);rules.OnBallContact(Front());
            Assert.That(rules.Serving,Is.True);Assert.That(rules.CanHit(PlayerId.Human),Is.True,"the human may volley the return of serve");
            Assert.That(rules.Clone().CanHit(PlayerId.Human),Is.True,"the clone keeps the rule");
            Assert.That(rules.RacketContact(PlayerId.Human),Is.True);Assert.That(rules.Serving,Is.False);
            var rival=RallyFixtures.Context().NewRules();rival.BeginServe(PlayerId.Human,ServiceBox.Left);rival.OnBallContact(Front());
            Assert.That(rival.CanHit(PlayerId.Rival),Is.False,"the rival never volleys the serve");
            var off=RallyFixtures.Context(new RallyParams{HumanVolleyServe=0}).NewRules();off.BeginServe(PlayerId.Rival,ServiceBox.Right);off.OnBallContact(Front());
            Assert.That(off.CanHit(PlayerId.Human),Is.False);
            var v1=V1().NewRules();v1.BeginServe(PlayerId.Rival,ServiceBox.Right);v1.OnBallContact(Front());Assert.That(v1.CanHit(PlayerId.Human),Is.False);
        }
        [Test] public void Offer_IsAnInterceptBeforeTheBounce_WithACommitWindow_AndMovesNothing()
        {
            var (ctx,t,rules,body,delay)=NeutralReception();var position=body.Position;var target=body.Target;long start=t.States[0].Step;
            var offer=HumanVolley.Offer(ctx,t,rules,body,delay,1);
            Assert.That(offer.Exists,Is.True,offer.Reason);
            Assert.That(offer.Plan.TStarStep,Is.LessThan(FirstBounce(t)));Assert.That(offer.Plan.Legal.Bounces,Is.Zero);
            Assert.That(offer.Plan.KOut.Steps-offer.Plan.KIn.Steps,Is.GreaterThanOrEqualTo(ctx.Params.HumanVolleyMinWindowS*ctx.StepHz));
            Assert.That(offer.Plan.KIn.Steps,Is.GreaterThanOrEqualTo(start+delay),"never before the reaction delay");
            Assert.That(offer.SplitSteps,Is.GreaterThanOrEqualTo(HumanVolley.AmberFromSteps(ctx,start,offer.Plan.TStarStep)));
            Assert.That(offer.SplitSteps,Is.LessThanOrEqualTo(offer.Plan.TStarStep));
            var again=HumanVolley.Offer(ctx,t,rules,body,delay,1);
            Assert.That((again.Plan.KIn.Steps,again.Plan.KOut.Steps,again.Plan.TStarStep,again.SplitSteps),Is.EqualTo((offer.Plan.KIn.Steps,offer.Plan.KOut.Steps,offer.Plan.TStarStep,offer.SplitSteps)));
            Assert.That(body.Position,Is.EqualTo(position));Assert.That(body.Target,Is.EqualTo(target),"pure: the body keeps its M1 move");
        }
        [Test] public void Commit_AtTheDeadline_StillReachesTheBall_AfterTStarItCannot()
        {
            // v2 semantics (S = the normal-body deadline); v2.2's S' needs the sprint and is covered by VolleyV21Tests.
            var (ctx,t,rules,body,delay)=NeutralReception(RallyFixtures.Context(new RallyParams{HumanVolleyMode=2}));long start=t.States[0].Step;
            var offer=HumanVolley.Offer(ctx,t,rules,body,delay,1);Assert.That(offer.Exists,Is.True,offer.Reason);
            var walk=body.Clone();long s=(long)offer.SplitSteps;for(long c=start;c<s;c++)walk.Step();
            var commit=HumanVolley.Commit(ctx,t,rules,walk,s,delay,2);
            Assert.That(commit.Exists,Is.True,commit.Reason);
            int remaining=(int)Math.Max(0,start+delay-s);int arrival=walk.StepsToReach(commit.Stance,remaining,out _);
            Assert.That(s+arrival,Is.LessThanOrEqualTo(commit.Plan.TStarStep),"the body is set before its contact");
            var late=body.Clone();long after=offer.Plan.TStarStep+1;for(long c=start;c<after;c++)late.Step();
            Assert.That(HumanVolley.Commit(ctx,t,rules,late,after,delay,2).Exists,Is.False);
            Assert.That(HumanVolley.Offer(V1(),t,rules,body,delay,1).Reason,Is.EqualTo("disabled"));
        }
        [Test] public void Offer_OnTheReturnOfServe_FollowsTheServeSwitch()
        {
            RallyContext Ctx(double serve)=>RallyFixtures.Context(new RallyParams{HumanVolleyServe=serve});
            VolleyOpportunity Offer(RallyContext ctx)
            {
                var t=ctx.Predict(ctx.Serve(ServiceBox.Right));var rules=ctx.NewRules();rules.BeginServe(PlayerId.Rival,ServiceBox.Right);
                var body=RallyFixtures.Body(ctx,PlayerId.Human,ctx.WaitPosition(PlayerId.Human));int delay=ctx.Steps(ctx.Params.ReactionS);
                var plan=ReceptionPlanner.Plan(ctx,t,rules,body,delay,1);body.MoveTo(plan.Stance,delay,BodyPhase.Approaching,BodyPhase.Set);
                return HumanVolley.Offer(ctx,t,rules,body,delay,1);
            }
            var on=Offer(Ctx(1));Assert.That(on.Exists,Is.True,on.Reason);
            Assert.That(Offer(Ctx(0)).Reason,Is.EqualTo("serve"));
        }
        // The rival (and RallySim, whose human bot never volleys) cannot see v2: every point and every reception is the same with it on or off.
        [Test] public void Rival_IsBitForBitIdentical_WithV2OnOrOff()
        {
            string Signature(RallyOutcome o)=>o.End+"/"+o.Cause+"/"+o.Winner+"/"+o.Shots+"/"+o.Steps+"/"+string.Join(",",o.Receptions.Select(r=>
                r.Who+":"+r.Result+":"+r.ContactStep+":"+r.DeltaMs.ToString("R",CultureInfo.InvariantCulture)+":"+r.Line+":"+r.Height+":"+r.Power+":"+r.Volley+":"+r.VolleyOffered+":"+r.SlackS.ToString("R",CultureInfo.InvariantCulture)));
            string[] Play(RallyContext ctx)
            {
                var all=new System.Collections.Generic.List<string>();
                foreach(RivalStyleId style in Enum.GetValues(typeof(RivalStyleId)))
                    using(var sim=new RallySim(ctx,new BotBrain(ctx,30,20261120+(int)style),RivalAi.For(ctx,AiLevel.Medium,style,20261121+(int)style)))
                        all.AddRange(sim.PlayMatch(PlayerId.Rival).Points.Select(Signature));
                return all.ToArray();
            }
            var on=Play(RallyFixtures.Context());var off=Play(V1());
            Assert.That(on,Is.EqualTo(off));Assert.That(on.Length,Is.GreaterThan(20));
        }
    }
}
