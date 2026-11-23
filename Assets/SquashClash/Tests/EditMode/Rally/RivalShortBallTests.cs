using System;
using System.Collections.Generic;
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
    public sealed class RivalShortBallTests
    {
        [Test] public void Hashes_Unchanged()
        {
            var c=RallyFixtures.Context();
            Assert.That(c.Config.Hash,Is.EqualTo("207da8f2c636"));
            Assert.That(c.Params.Hash,Is.EqualTo("1063d0f6cf66"),"approved main 230b758, including the new serve defaults");
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
            var solution=option.Side.Exists?RallyContact.SolveBoastIntention(c,at,option.Side,option.P,out _):RallyContact.SolveIntention(c,at,option.Aim,option.P);
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
            // Spec section 5.2, step 8: a legal front contact and the opponent at T, with one return of each class.
            // The first fixture incorrectly assumed height .8 m would contain such a case.
            var points=from h in Enumerable.Range(0,11)
                from d in Enumerable.Range(8,(int)(c.T.y*10)-7)
                from x in new[]{.6f,1.6f,3.2f,4.8f,5.8f}
                select new Vector3(x,Mathf.Lerp(c.Legal.MinHeight,c.Legal.MaxHeight,h/10f),d/10f);
            foreach(var at in points)
            {
                var options=Defensive(c,at);
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
                    Assert.That(shot.Option.Line,Is.Not.EqualTo(ShotLine.Boast),"E1 never boasts");
                    Assert.That(late[shot.Option.Line],Is.EqualTo(best),"no reachable option: choose the least late");
                }
                var flag=typeof(RivalAi).GetProperty("LastStretchedFallback");
                Assert.That(flag,Is.Not.Null,"the unavoidable R3 exception must be observable for measurement");
                Assert.That(flag.GetValue(ai),Is.True);break;
            }
            Assert.That(found,Is.True,"a deterministic no-reachable-option case is required");
        }
        [TestCase(RivalStyleId.Fondista)][TestCase(RivalStyleId.Definidor)][TestCase(RivalStyleId.Completo)]
        public void RivalAi_StretchedFront_NeverBoasts(RivalStyleId style)
        {
            var c=RallyFixtures.Context();Assert.That(c.BoastOn,Is.True);
            var own=Own(c,new Vector3(.9f,.8f,1.5f));var ai=Ai(c,style);var o=Observe(c,new Vector2(1.5f,6.5f));int violations=0;
            for(int i=0;i<500;i++)if(ai.Decide(o,own).Option.Line==ShotLine.Boast)violations++;
            Assert.That(violations,Is.Zero,"R2: stretched boast count, "+style);
        }
        private static List<ShotOption> BackOptions(RallyContext c,Vector3 at)
        {
            var options=Defensive(c,at);var boasts=new List<ShotOption>();ShotOptions.AddBoasts(c,at,boasts);
            options.AddRange(boasts);return options;
        }
        private static bool BackReachable(RallyContext c,Vector3 at,ShotOption option,Vector2 opponent)
        {
            var forecast=option.Side.Exists?RallyContact.ForecastBoast(c,at,option.Side,option.P):RallyContact.Forecast(c,at,option.Aim,option.P);
            return forecast.Good&&ReturnPlan(c,at,option,opponent).Reachable;
        }
        [TestCase(RivalStyleId.Definidor)][TestCase(RivalStyleId.Completo)]
        public void RivalAi_StretchedBack_MayBoast_WhenTheHumanReachesIt(RivalStyleId style)
        {
            var c=RallyFixtures.Context();var at=new Vector3(.9f,.8f,8.6f);var options=BackOptions(c,at);
            Assert.That(options.Any(s=>s.Line==ShotLine.Boast&&BackReachable(c,at,s,c.T)),Is.True,"B2 predeclared back-corner/T fixture must offer a Good, Reachable boast");
            var ai=Ai(c,style);var own=Own(c,at);var o=Observe(c,c.T);int boasts=0;
            for(int i=0;i<500;i++)if(ai.Decide(o,own).Option.Line==ShotLine.Boast)boasts++;
            Assert.That(boasts,Is.GreaterThan(0),"B2: reachable back-corner boasts stay available, "+style);
        }
        [TestCase(RivalStyleId.Fondista)][TestCase(RivalStyleId.Definidor)][TestCase(RivalStyleId.Completo)]
        public void RivalAi_StretchedBack_NeverPicksAnUnreachableOption(RivalStyleId style)
        {
            var c=RallyFixtures.Context();int negativeBoasts=0,emptyCells=0,decisions=0;
            foreach(float x in new[]{.5f,.9f,1.5f})
            foreach(float y in new[]{.3f,.8f})
            foreach(float z in new[]{7.6f,8.6f,9.3f})
            foreach(var observed in new[]{c.T,new Vector2(c.Court.Width,c.Court.Length),new Vector2(c.Court.Width,0),Vector2.zero})
            {
                var at=new Vector3(x,y,z);var opponent=c.ClampBody(observed);
                var options=Allowed(c,at);
                var reach=options.ToDictionary(s=>(s.Line,s.Height,s.Power),s=>BackReachable(c,at,s,opponent));
                bool any=reach.Values.Any(v=>v);
                negativeBoasts+=options.Count(s=>s.Line==ShotLine.Boast&&RallyContact.ForecastBoast(c,at,s.Side,s.P).Good&&!reach[(s.Line,s.Height,s.Power)]);
                if(!any)emptyCells++;
                var ai=Ai(c,style);var own=Own(c,at);var o=Observe(c,opponent);
                for(int i=0;i<100;i++)
                {
                    var shot=ai.Decide(o,own);decisions++;
                    Assert.That(shot.Swing,Is.True,"B3b never stopped: "+at+" / "+opponent);
                    if(any)Assert.That(reach.TryGetValue((shot.Option.Line,shot.Option.Height,shot.Option.Power),out bool ok)&&ok,
                        Is.True,"B3b reachable alternative exists: "+at+" / "+opponent+" / "+shot.Option.Line);
                    else
                    {
                        Assert.That(shot.Option.Line,Is.Not.EqualTo(ShotLine.Boast),"E1 never boasts");
                        Assert.That(ai.LastStretchedFallback,Is.True,"E1 is observable");
                    }
                }
            }
            TestContext.WriteLine($"B3b style={style} decisions={decisions} negativeBoasts={negativeBoasts} emptyCells={emptyCells}");
            if(negativeBoasts==0)TestContext.WriteLine("la grilla no ejercita el rechazo de dos paredes");
        }
        private static List<ShotOption> Allowed(RallyContext c,Vector3 at)
        {
            var options=ShotOptions.Build(c,at);if(c.BoastOn)ShotOptions.AddBoasts(c,at,options);
            return options.Where(s=>s.Line==ShotLine.Boast||
                (s.Height==ShotHeight.High&&s.Power==ShotPower.Medium&&(s.Line==ShotLine.Straight||s.Line==ShotLine.Cross))).ToList();
        }
        [TestCase(false)][TestCase(true)]
        public void RivalAi_EvaluateStretched_MatchesThePlannerPerOption(bool oppositeCorner)
        {
            var c=RallyFixtures.Context();var at=new Vector3(.9f,.8f,8.6f);
            var opponent=oppositeCorner?c.ClampBody(new Vector2(c.Court.Width,c.Court.Length)):c.T;
            var options=Allowed(c,at);var actual=Ai(c,RivalStyleId.Definidor).EvaluateStretched(Observe(c,opponent),Own(c,at));
            Assert.That(actual.Length,Is.EqualTo(options.Count));
            for(int i=0;i<options.Count;i++)
            {
                var option=options[i];var label=$"B3d {i} {option.Line}/{option.Height}/{option.Power}, corner={oppositeCorner}";
                bool good=(option.Side.Exists?RallyContact.ForecastBoast(c,at,option.Side,option.P):RallyContact.Forecast(c,at,option.Aim,option.P)).Good;
                bool reachable=false;long delay=long.MaxValue;
                if(good)
                {
                    var solution=option.Side.Exists?RallyContact.SolveBoastIntention(c,at,option.Side,option.P,out _):RallyContact.SolveIntention(c,at,option.Aim,option.P);
                    Assert.That(solution.Solved,Is.True,label);
                    var trajectory=c.Predict(new BallState{Position=at,Velocity=solution.Velocity});
                    var rules=c.NewRules();rules.Begin(PlayerId.Rival,PlayerId.Human);
                    var plan=ReceptionPlanner.Plan(c,trajectory,rules,new PlayerBody(PlayerId.Human,opponent,c.Move),c.Steps(c.Params.ReactionS),1);
                    long until=plan.Contact.Exists&&plan.Contact.TStarStep>=0?plan.Contact.TStarStep:trajectory.States.Last().Step;
                    reachable=plan.Reachable;delay=plan.ArrivalStep-until;
                }
                Assert.That(actual[i].Good,Is.EqualTo(good),label+" Good");
                Assert.That(actual[i].Reachable,Is.EqualTo(reachable),label+" Reachable");
                Assert.That(actual[i].Delay,Is.EqualTo(delay),label+" Delay");
                Assert.That(actual[i].IsBoast,Is.EqualTo(option.Line==ShotLine.Boast),label+" IsBoast");
            }
        }
        [Test] public void RivalAi_StretchedBack_FilterPredictsTheBoastPath()
        {
            var c=RallyFixtures.Context();var at=new Vector3(.9f,.8f,8.6f);
            var boast=BackOptions(c,at).First(s=>s.Line==ShotLine.Boast&&RallyContact.ForecastBoast(c,at,s.Side,s.P).Good);
            var ai=Ai(c,RivalStyleId.Definidor);
            // The intention method used by FilterStretched, not a separate implementation of the filter's trajectory.
            var method=typeof(RivalAi).GetMethod("StretchedIntention",BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(method,Is.Not.Null);
            var solution=(ShotSolution)method.Invoke(ai,new object[]{at,boast});
            Assert.That(solution.Solved,Is.True,"B3: a Good boast must use the side-wall intention");
            var trajectory=c.Predict(new BallState{Position=at,Velocity=solution.Velocity});
            var walls=trajectory.Contacts.Where(e=>e.Surface==SurfaceId.LeftWall||e.Surface==SurfaceId.RightWall||e.Surface==SurfaceId.FrontWall).Take(2).ToArray();
            Assert.That(walls.Length,Is.EqualTo(2));
            Assert.That(walls[0].Surface,Is.EqualTo(boast.Side.Wall));
            Assert.That(walls[1].Surface,Is.EqualTo(SurfaceId.FrontWall));
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
            Assert.That(hash,Is.EqualTo("53870c765e9e1cc410b87b41d810afc7fbe53ac0ffa42345b9b97192786de4de"),"518f9c3: 1,080 planned decisions, three styles and three levels");
        }
    }
}
