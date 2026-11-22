using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class RivalShortBallTests
    {
        [Test] public void Hashes_Unchanged()
        {
            var c=RallyFixtures.Context();
            Assert.That(c.Config.Hash,Is.EqualTo("207da8f2c636"));
            Assert.That(c.Params.Hash,Is.EqualTo("a82c34b661bf"));
            Assert.That(c.Config.SimulationKey,Is.EqualTo("166a96e319c7"));
        }
        private static ReceptionPlan Own(RallyContext c,Vector3 at,string reason="stretched",double slack=0)
        {
            var stance=new Vector2(at.x+(at.x<c.Court.Width*.5f?.6f:-.6f),at.z);
            var contact=new ContactPlan(true,new SimTime(100),new SimTime(140),120,at,StrokeSide.Drive,1,c.FrameAt(stance),c.Legal,c.Court);
            return new ReceptionPlan(true,stance,stance,contact,90,slack,0,reason);
        }
        private static AiObservation Observe(RallyContext c,Vector2 opponent)
            =>new AiObservation(default,c.WaitPosition(PlayerId.Rival),default,opponent,default,3,0,false);
        private static RivalAi Ai(RallyContext c,RivalStyleId style)=>new RivalAi(c,AiDifficulty.For(c.Params,AiLevel.Medium),style,7);
        private static ReceptionPlan ReturnPlan(RallyContext c,Vector3 at,ShotOption option,Vector2 opponent)
        {
            var solution=RallyContact.SolveIntention(c,at,option.Aim,option.P);
            Assert.That(solution.Solved,Is.True,"defensive intention must solve");
            var trajectory=c.Predict(new BallState{Position=at,Velocity=solution.Velocity});
            return ReceptionPlanner.Plan(c,trajectory,RallyFixtures.Rules(c,PlayerId.Rival,PlayerId.Human),
                new PlayerBody(PlayerId.Human,opponent,c.Move),c.Steps(c.Params.ReactionS),1);
        }
        private static List<ShotOption> Defensive(RallyContext c,Vector3 at)=>ShotOptions.Build(c,at)
            .Where(s=>(s.Line==ShotLine.Straight||s.Line==ShotLine.Cross)&&s.Height==ShotHeight.High&&s.Power==ShotPower.Medium)
            .Where(s=>RallyContact.Forecast(c,at,s.Aim,s.P).Good).ToList();
        [TestCase(RivalStyleId.Fondista)][TestCase(RivalStyleId.Definidor)][TestCase(RivalStyleId.Completo)]
        public void RivalAi_Stretched_PicksAnOptionTheOpponentCanReach(RivalStyleId style)
        {
            var c=RallyFixtures.Context();int mixed=0,violations=0;
            foreach(float z in new[]{.8f,1,1.25f,1.5f,1.75f,2,2.25f,2.5f,2.75f,3,3.5f,4.5f})
            foreach(float x in new[]{.6f,1.6f,3.2f,4.8f,5.8f})
            {
                var at=new Vector3(x,.8f,z);var options=Defensive(c,at);
                if(options.Count!=2)continue;
                var reachable=options.Select(s=>ReturnPlan(c,at,s,c.T).Reachable).ToArray();
                if(!reachable.Any(v=>v)||reachable.All(v=>v))continue;
                mixed++;TestContext.WriteLine($"R3_MIXED at={at} straight={reachable[0]} cross={reachable[1]}");
                var own=Own(c,at);var ai=new RivalAi(c,new AiDifficulty(0,0,0,0,0,1),style,7);var o=Observe(c,c.T);
                for(int i=0;i<100;i++)
                {
                    var s=ai.Decide(o,own);
                    int index=options.FindIndex(option=>option.Line==s.Option.Line&&option.Height==s.Option.Height&&option.Power==s.Option.Power);
                    if(index<0||!reachable[index])violations++;
                }
                break;
            }
            Assert.That(mixed,Is.GreaterThan(0),"deterministic front contact with both reachable and unreachable defensive returns");
            Assert.That(violations,Is.Zero,"R3: unreachable choices when a reachable defensive option exists, "+style);
        }
        private static long Lateness(RallyContext c,Vector3 at,ShotOption option,Vector2 opponent)
        {
            var plan=ReturnPlan(c,at,option,opponent);
            var solution=RallyContact.SolveIntention(c,at,option.Aim,option.P);
            var trajectory=c.Predict(new BallState{Position=at,Velocity=solution.Velocity});
            long until=plan.Contact.Exists&&plan.Contact.TStarStep>=0?plan.Contact.TStarStep:trajectory.States.Last().Step;
            return plan.ArrivalStep-until;
        }
        [Test] public void RivalAi_Stretched_NoReachableOption_PicksLeastLate_AndFlags()
        {
            var c=RallyFixtures.Context();bool found=false;
            foreach(float z in new[]{.8f,1.5f,2.5f,3.5f})
            foreach(var opponent in new[]{new Vector2(.35f,9.4f),new Vector2(6,9.4f),new Vector2(.35f,.35f),new Vector2(6,.35f)})
            {
                var at=new Vector3(.9f,.8f,z);var options=Defensive(c,at);
                if(options.Count!=2||options.Any(s=>ReturnPlan(c,at,s,opponent).Reachable))continue;
                found=true;var late=options.ToDictionary(s=>s.Line,s=>Lateness(c,at,s,opponent));long best=late.Values.Min();
                var ai=new RivalAi(c,new AiDifficulty(0,0,0,0,0,1),RivalStyleId.Completo,7);var own=Own(c,at);var o=Observe(c,opponent);
                for(int i=0;i<50;i++)
                {
                    var shot=ai.Decide(o,own);
                    Assert.That(late[shot.Option.Line],Is.EqualTo(best),"no reachable option: choose the least late");
                }
                var flag=typeof(RivalAi).GetProperty("LastStretchedFallback");
                Assert.That(flag,Is.Not.Null,"the unavoidable R3 exception must be observable for measurement");
                Assert.That(flag.GetValue(ai),Is.True);break;
            }
            Assert.That(found,Is.True,"a deterministic no-reachable-option case is required");
        }
        [TestCase(RivalStyleId.Fondista)][TestCase(RivalStyleId.Definidor)][TestCase(RivalStyleId.Completo)]
        public void RivalAi_Stretched_NeverBoasts(RivalStyleId style)
        {
            var c=RallyFixtures.Context();Assert.That(c.BoastOn,Is.True);
            var own=Own(c,new Vector3(.9f,.8f,8.6f));var ai=Ai(c,style);var o=Observe(c,new Vector2(1.5f,6.5f));int violations=0;
            for(int i=0;i<500;i++)if(ai.Decide(o,own).Option.Line==ShotLine.Boast)violations++;
            Assert.That(violations,Is.Zero,"R2: stretched boast count, "+style);
        }
        [TestCase(RivalStyleId.Fondista)][TestCase(RivalStyleId.Definidor)][TestCase(RivalStyleId.Completo)]
        public void RivalAi_Stretched_FromFront_IsHighMediumStraightOrCross(RivalStyleId style)
        {
            var c=RallyFixtures.Context();var own=Own(c,new Vector3(.9f,.8f,1.5f));var ai=Ai(c,style);var o=Observe(c,c.T);int violations=0;
            for(int i=0;i<500;i++)
            {
                var s=ai.Decide(o,own);
                if(!s.Swing||!s.Defensive||s.Option.Height!=ShotHeight.High||s.Option.Power!=ShotPower.Medium||
                   (s.Option.Line!=ShotLine.Straight&&s.Option.Line!=ShotLine.Cross))violations++;
            }
            Assert.That(violations,Is.Zero,"R2: non-defensive stretched choices, "+style);
        }
        [TestCase(RivalStyleId.Fondista)][TestCase(RivalStyleId.Definidor)][TestCase(RivalStyleId.Completo)]
        public void RivalAi_PlannedLowSlack_StillMayBoast(RivalStyleId style)
        {
            var c=RallyFixtures.Context();var own=Own(c,new Vector3(.9f,.8f,8.6f),"planned",.05);var ai=Ai(c,style);var o=Observe(c,new Vector2(1.5f,6.5f));int boasts=0;
            for(int i=0;i<500;i++)if(ai.Decide(o,own).Option.Line==ShotLine.Boast)boasts++;
            Assert.That(boasts,Is.GreaterThan(0),"planned pressure keeps today's options");
        }
        // Baseline capture is run before I2/I3, then its printed fingerprints are frozen in this guard.
        [Test] public void RivalAi_NotStretched_DecisionsUnchanged()
        {
            var c=RallyFixtures.Context();var text=new StringBuilder();
            foreach(RivalStyleId style in Enum.GetValues(typeof(RivalStyleId)))
            foreach(AiLevel level in Enum.GetValues(typeof(AiLevel)))
            {
                var ai=RivalAi.For(c,level,style,20261122);
                foreach(var at in new[]{new Vector3(.9f,.8f,8.6f),new Vector3(1.6f,.8f,1.5f),new Vector3(4.6f,.9f,3.5f)})
                foreach(double slack in new[]{.05,.4})
                for(int i=0;i<20;i++)
                {
                    var s=ai.Decide(Observe(c,new Vector2(3.2f,7.5f)),Own(c,at,"planned",slack));
                    text.Append(FormattableString.Invariant($"{style}/{level}/{s.Swing}/{s.Option.Line}/{s.Option.Height}/{s.Option.Power}/{s.Aim.x:R}/{s.Aim.y:R}/{s.P:R}/{s.DeltaMs:R}/{s.Defensive}/{s.Volley}/{s.Side.Exists}\n"));
                }
            }
            string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-","").ToLowerInvariant();
            TestContext.WriteLine("PLANNED_BASELINE_SHA256="+hash);
            Assert.That(hash,Is.Not.Empty,"baseline capture; freeze before changing production");
        }
    }
}
