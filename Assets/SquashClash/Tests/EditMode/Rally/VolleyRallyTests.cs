using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // Volley v1 in the shared rally loop (design 2026-11-18): rules, M1 movement, opportunity and contact, identity when nobody volleys.
    public sealed class VolleyRallyTests
    {
        private static ContactEvent Front()=>new ContactEvent(SurfaceId.FrontWall,new Vector3(3.2f,2.5f,0),Vector2.zero,CourtDimensions.InwardNormal(SurfaceId.FrontWall),1,0,ContactTiming.Exact,Vector3.zero,false);
        private static string Signature(RallyOutcome o)=>o.End+"/"+o.Cause+"/"+o.Winner+"/"+o.Shots+"/"+o.Steps+"/"+string.Join(",",o.Receptions.Select(r=>r.Who+":"+r.Result+":"+r.ContactStep+":"+r.DeltaMs.ToString("R",CultureInfo.InvariantCulture)));
        private static RallyContext Context(double volley)=>RallyFixtures.Context(new RallyParams{VolleyEnabled=volley});
        private static System.Collections.Generic.List<RallyOutcome> Points(RallyContext context,int matches,bool takeVolley,long seed=20261118)
        {
            var points=new System.Collections.Generic.List<RallyOutcome>();
            for(int i=0;i<matches;i++)
                using(var sim=new RallySim(context,new BotBrain(context,30,seed+2*i,takeVolley:takeVolley),new FondoAi(context,AiDifficulty.For(context.Params,AiLevel.Medium),seed+1+2*i)))
                    points.AddRange(sim.PlayMatch(i%2==0?PlayerId.Human:PlayerId.Rival).Points);
            return points;
        }

        [Test] public void Rules_NeverAllowAVolleyOfTheServe()
        {
            var rules=new PointRules(CourtDimensions.Standard(),.02f,true);rules.BeginServe(PlayerId.Rival,ServiceBox.Right);
            var front=new ContactEvent(SurfaceId.FrontWall,new Vector3(3.2f,2.5f,0),Vector2.zero,CourtDimensions.InwardNormal(SurfaceId.FrontWall),1,0,ContactTiming.Exact,Vector3.zero,false);
            rules.OnBallContact(front);Assert.That(rules.FrontWallReached,Is.True);Assert.That(rules.Serving,Is.True);
            Assert.That(rules.CanHit(PlayerId.Human),Is.False,"v1: the serve is never volleyed");
            Assert.That(rules.RacketContact(PlayerId.Human),Is.False);
        }
        [Test] public void Rules_AllowTheVolleyInTheRally_AndContextFollowsTheSwitch()
        {
            var rules=new PointRules(CourtDimensions.Standard(),.02f,true);rules.Begin(PlayerId.Rival,PlayerId.Human);rules.OnBallContact(Front());
            Assert.That(rules.CanHit(PlayerId.Human),Is.True);Assert.That(rules.CanHitAfterBounce(PlayerId.Human),Is.False);
            Assert.That(Context(1).NewRules().VolleyAllowed,Is.True);Assert.That(Context(0).NewRules().VolleyAllowed,Is.False);
        }
        [Test] public void MovementPlanner_PlansTheBounceOnly_WhateverTheVolleySwitch()
        {
            var context=Context(1);var ball=context.NeutralLaunch(PlayerId.Human);var trajectory=context.Predict(ball);
            var with=new PointRules(context.Court,.02f,true);with.Begin(null,PlayerId.Human);var without=new PointRules(context.Court,.02f,false);without.Begin(null,PlayerId.Human);
            var a=ReceptionPlanner.Plan(context,trajectory,with,RallyFixtures.Body(context,PlayerId.Human,context.WaitPosition(PlayerId.Human)),20,1);
            var b=ReceptionPlanner.Plan(context,trajectory,without,RallyFixtures.Body(context,PlayerId.Human,context.WaitPosition(PlayerId.Human)),20,1);
            Assert.That((a.Reachable,a.Stance,a.Contact.TStarStep,a.Contact.KIn.Steps,a.Contact.KOut.Steps,a.ArrivalStep,a.SlackS),Is.EqualTo((b.Reachable,b.Stance,b.Contact.TStarStep,b.Contact.KIn.Steps,b.Contact.KOut.Steps,b.ArrivalStep,b.SlackS)));
        }
        [Test] public void RallyParams_ProjectionWithoutVolleyFields_IsTheCurrentHash()
        {
            var canonical=new StringBuilder();
            foreach(var field in typeof(RallyParams).GetFields(BindingFlags.Public|BindingFlags.Instance).Where(f=>!f.Name.StartsWith("Volley",StringComparison.Ordinal)&&!f.Name.StartsWith("Style",StringComparison.Ordinal)&&!f.Name.StartsWith("HumanVolley",StringComparison.Ordinal)&&!f.Name.StartsWith("ServeKind",StringComparison.Ordinal)&&!f.Name.StartsWith("Boast",StringComparison.Ordinal)).OrderBy(f=>f.Name,StringComparer.Ordinal))
                canonical.Append(field.Name).Append('=').Append(((double)field.GetValue(new RallyParams())).ToString("R",CultureInfo.InvariantCulture)).Append('\n');
            string projection;using(var sha=SHA256.Create())projection=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())),0,6).Replace("-","").ToLowerInvariant();
            Assert.That(projection,Is.EqualTo("927e304bbfec"));
            // Six since 19 nov: the five v1 fields plus VolleyEnergyCost (VolleyEnergyTests checks its own projection).
            Assert.That(typeof(RallyParams).GetFields().Count(f=>f.Name.StartsWith("Volley",StringComparison.Ordinal)),Is.EqualTo(6));
            TestContext.WriteLine("VOLLEY_RALLYPARAMS_HASH="+new RallyParams().Hash+" projection="+projection);
        }
        [Test] public void NobodyVolleys_TheRallyIsIdenticalWithTheVolleyOnOrOff()
        {
            var on=Points(Context(1),3,false);var off=Points(Context(0),3,false);
            Assert.That(on.Select(Signature).ToArray(),Is.EqualTo(off.Select(Signature).ToArray()));
            Assert.That(on.SelectMany(p=>p.Receptions).Any(r=>r.VolleyOffered),Is.True,"the opportunity is computed even when not taken");
            Assert.That(off.SelectMany(p=>p.Receptions).Any(r=>r.VolleyOffered),Is.False);
            Assert.That(on.SelectMany(p=>p.Receptions).Any(r=>r.Volley),Is.False,"17.A.7: the planner never volleys without a release in the volley window");
        }
        [Test] public void AVolleyingPlayer_HitsBeforeTheBounce_ThroughTheSharedRules()
        {
            var points=Points(Context(1),3,true);var receptions=points.SelectMany(p=>p.Receptions).ToList();
            var volleys=receptions.Where(r=>r.Who==PlayerId.Human&&r.Volley).ToList();
            Assert.That(volleys,Is.Not.Empty,"a bot that takes every offered volley must volley at least once in three games");
            Assert.That(volleys.All(r=>r.VolleyOffered),Is.True);
            Assert.That(volleys.Any(r=>r.Result==ReceptionResult.Contact&&r.Vectors!=null),Is.True,"a volley contact is solved and applied");
            Assert.That(points.Sum(p=>p.SystemFaults),Is.Zero);Assert.That(points.Sum(p=>p.RejectedContacts),Is.Zero);
            Assert.That(receptions.Where(r=>r.Who==PlayerId.Rival).Any(r=>r.Volley),Is.False,"the Fondo rival does not volley in v1");
        }
    }
}
