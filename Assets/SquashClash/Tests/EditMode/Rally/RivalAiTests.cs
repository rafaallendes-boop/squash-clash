using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // Rival styles (design 2026-11-19): a style changes how the AI chooses, never what it can do; Fondista is FondoAi.
    public sealed class RivalAiTests
    {
        private static RallyContext Context(Action<RallyParams> edit=null){var p=new RallyParams();edit?.Invoke(p);return RallyFixtures.Context(p);}
        private static ReceptionPlan OwnPlan(RallyContext context,double slack,Vector3? contact=null)
        {
            var point=contact??new Vector3(1.6f,.8f,6.5f);var stance=new Vector2(point.x+.6f,point.z);
            var plan=new ContactPlan(true,new SimTime(100),new SimTime(140),120,point,StrokeSide.Drive,1,context.FrameAt(stance),context.Legal,context.Court);
            return new ReceptionPlan(true,stance,stance,plan,90,slack,Mathf.Clamp01((float)(slack/context.Params.SlackCapS)),"planned");
        }
        private static ReceptionPlan WithVolley(RallyContext context,ReceptionPlan own,Vector3 point)
        {
            var stance=new Vector2(point.x+.6f,point.z);
            var v=new ContactPlan(true,new SimTime(60),new SimTime(78),70,point,StrokeSide.Drive,1,context.FrameAt(stance),context.Legal,context.Court);
            return own.WithVolley(new VolleyOpportunity(v,90,"offered"));
        }
        private static AiObservation Observe(RallyContext context,Vector2 opponent,Vector2? self=null)
            =>new AiObservation(default,self??context.WaitPosition(PlayerId.Rival),Vector2.zero,opponent,Vector2.zero,3,0,false);
        private static List<AiShot> Sample(IRallyBrain ai,AiObservation o,ReceptionPlan own,int count=3000)
        {var list=new List<AiShot>(count);for(int i=0;i<count;i++)list.Add(ai.Decide(o,own));return list;}
        private static RivalAi Ai(RallyContext c,RivalStyleId style,AiLevel level=AiLevel.Medium,long seed=7)=>new RivalAi(c,AiDifficulty.For(c.Params,level),style,seed);
        private static double Share(List<AiShot> s,Func<AiShot,bool> f)=>s.Count(f)/(double)s.Count;
        private static bool Attack(AiShot s)=>s.Option.Line==ShotLine.Drop||s.Option.Height==ShotHeight.Low;
        internal static string Projection(Func<FieldInfo,bool> keep,RallyParams parameters=null)
        {
            var canonical=new StringBuilder();
            parameters??=new RallyParams();
            foreach(var field in typeof(RallyParams).GetFields(BindingFlags.Public|BindingFlags.Instance).Where(keep).OrderBy(f=>f.Name,StringComparer.Ordinal))
                canonical.Append(field.Name).Append('=').Append(((double)field.GetValue(parameters)).ToString("R",CultureInfo.InvariantCulture)).Append('\n');
            using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())),0,6).Replace("-","").ToLowerInvariant();
        }

        [Test] public void RallyParams_ProjectionWithoutStyleFields_IsTheCurrentHash()
        {
            Assert.That(Projection(f=>f.Name!="HumanServeEarlyMs"&&!f.Name.StartsWith("Style",StringComparison.Ordinal)&&!f.Name.StartsWith("HumanVolley",StringComparison.Ordinal)&&!f.Name.StartsWith("ServeKind",StringComparison.Ordinal)&&!f.Name.StartsWith("Boast",StringComparison.Ordinal)),Is.EqualTo("5cdbb01e4621"));
            // Volley v2 (20 nov) adds the HumanVolley* fields: without them the hash is still 0f58580f44ea (VolleyV2Tests).
            Assert.That(Projection(f=>f.Name!="HumanServeEarlyMs"&&!f.Name.StartsWith("HumanVolley",StringComparison.Ordinal)&&!f.Name.StartsWith("ServeKind",StringComparison.Ordinal)&&!f.Name.StartsWith("Boast",StringComparison.Ordinal)),Is.EqualTo("0f58580f44ea"),"measured in Unity with the style fields");
            // Fatigue feel (20 nov) adds HumanVolleySprintFatigue: without it the hash is still 0fe523164363 (FatigueFeelTests).
            Assert.That(Projection(f=>f.Name!="HumanServeEarlyMs"&&f.Name!="HumanVolleySprintFatigue"&&!f.Name.StartsWith("ServeKind",StringComparison.Ordinal)&&!f.Name.StartsWith("Boast",StringComparison.Ordinal)),Is.EqualTo("0fe523164363"),"with the volley v2.2 fields");
            // Serve (20 nov) adds the ServeKind* fields: without them the hash is still 13f88c8d1db2 (ServeTests).
            Assert.That(Projection(f=>f.Name!="HumanServeEarlyMs"&&!f.Name.StartsWith("ServeKind",StringComparison.Ordinal)&&!f.Name.StartsWith("Boast",StringComparison.Ordinal)),Is.EqualTo("13f88c8d1db2"),"with the fatigue feel field");
            // Two-wall shot (20 nov) adds the Boast* fields: without them the hash is still 992d0570c9de.
            Assert.That(Projection(f=>f.Name!="HumanServeEarlyMs"&&f.Name!="ServeKindHumanPauseSeconds"&&!f.Name.StartsWith("Boast",StringComparison.Ordinal),new RallyParams{ServeKindOn=0}),Is.EqualTo("ccea7e5eef72"),"historical serve kind fields, switch off");
            Assert.That(new RallyParams{ServeKindOn=0}.Hash,Is.EqualTo("263dfbb4bd5d"),"serve UX with the switch off");
            Assert.That(new RallyParams().Hash,Is.EqualTo("3833bb374542"),"new serve and rival boast on by default");
            TestContext.WriteLine("STYLE_RALLYPARAMS_HASH="+new RallyParams().Hash);
        }
        [TestCase(AiLevel.Easy)][TestCase(AiLevel.Medium)][TestCase(AiLevel.Hard)]
        public void Fondista_WithoutVolley_DecidesExactlyLikeFondoAi(AiLevel level)
        {
            var c=Context(p=>p.StyleRivalVolley=0);
            foreach(var (opp,slack,contact) in new[]{(new Vector2(2,8.5f),.35,new Vector3(1.6f,.8f,6.5f)),(new Vector2(3.2f,7.5f),.05,new Vector3(4.6f,.9f,3.5f)),(new Vector2(4,3),.5,new Vector3(3,.7f,8))})
            {
                var own=WithVolley(c,OwnPlan(c,slack,contact),new Vector3(contact.x,1.2f,contact.z-1));
                var a=new FondoAi(c,AiDifficulty.For(c.Params,level),11);var b=Ai(c,RivalStyleId.Fondista,level,11);
                for(int i=0;i<500;i++)
                {
                    Assert.That(b.StanceNoise(),Is.EqualTo(a.StanceNoise()));
                    var x=a.Decide(Observe(c,opp),own);var y=b.Decide(Observe(c,opp),own);
                    Assert.That((y.Swing,y.Option.Line,y.Option.Height,y.Option.Power,y.Aim,y.P,y.DeltaMs,y.Defensive,y.Volley),Is.EqualTo((x.Swing,x.Option.Line,x.Option.Height,x.Option.Power,x.Aim,x.P,x.DeltaMs,x.Defensive,false)),"decision "+i);
                }
            }
            Assert.That(Ai(c,RivalStyleId.Fondista).Wait,Is.EqualTo(c.WaitPosition(PlayerId.Rival)));
        }
        [Test] public void Fondista_WithVolleyAndCoverOff_ReplaysTheFrozenRallySimSignatures()
        {
            var ctx=new RallyContext(EffectiveConfig.From(ConfigFixtures.Default().With("fatigue.enabled",0)),new RallyParams{StyleRivalVolley=0,StyleFondoCoverDepth=0,ServeKindOn=0},new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            var rows=File.ReadAllLines(Path.Combine(Application.dataPath,"..","docs/validation/evidence/2026-11-26-posiciones-saque/fatigue-replay/off/points.tsv")).Skip(1).Select(r=>r.Split('\t')).Where(r=>int.Parse(r[0])<2).ToArray();
            for(int i=0;i<2;i++)
            using(var sim=new RallySim(ctx,new BotBrain(ctx,30,20261117+2*i),new RivalAi(ctx,AiDifficulty.For(ctx.Params,AiLevel.Medium),RivalStyleId.Fondista,20261118+2*i)))
            {
                var score=new MatchScore(7,2,i%2==0?PlayerId.Human:PlayerId.Rival);int point=0;
                while(!score.Finished)
                {var o=sim.PlayPoint(score.Server,score.Box);Assert.That(FatigueIntegrationTests.Signature(o,sim),Is.EqualTo(rows.Single(r=>int.Parse(r[0])==i&&int.Parse(r[1])==point)[9]),i+"/"+point);score.PointWon(o.Winner.Value);point++;}
            }
        }
        [Test] public void Styles_AreRecognizable_FromTheSameMidCourtBall()
        {
            var c=Context(p=>p.StyleRivalVolley=0);var own=OwnPlan(c,.4,new Vector3(1.6f,.8f,4.6f));var o=Observe(c,new Vector2(3.6f,7.5f));
            var fondo=Sample(Ai(c,RivalStyleId.Fondista),o,own);var shooter=Sample(Ai(c,RivalStyleId.Definidor),o,own);var all=Sample(Ai(c,RivalStyleId.Completo),o,own);
            // The match-level identity (Fondista >= 80 % long) is measured in the calibration experiment; here, one ball, the order.
            Assert.That(Share(fondo,s=>s.Option.Long),Is.GreaterThan(Share(all,s=>s.Option.Long)),"Fondista plays the longest");
            Assert.That(Share(all,s=>s.Option.Long),Is.GreaterThan(Share(shooter,s=>s.Option.Long)));
            Assert.That(Share(shooter,Attack),Is.GreaterThanOrEqualTo(.35),"Definidor tries to finish");
            Assert.That(Share(shooter,Attack),Is.GreaterThan(Share(all,Attack)));Assert.That(Share(all,Attack),Is.GreaterThan(Share(fondo,Attack)));
        }
        [Test] public void Completo_AttacksTheShortBall_AndDefendsTheDeepOne()
        {
            var c=Context(p=>p.StyleRivalVolley=0);var o=Observe(c,new Vector2(3.2f,6.5f));
            double front=Share(Sample(Ai(c,RivalStyleId.Completo),o,OwnPlan(c,.4,new Vector3(1.6f,.8f,3.4f))),Attack);
            double deep=Share(Sample(Ai(c,RivalStyleId.Completo),o,OwnPlan(c,.4,new Vector3(1.6f,.8f,8.2f))),Attack);
            Assert.That(front,Is.GreaterThan(deep+.15),"front "+front+" deep "+deep);
        }
        [Test] public void Volley_OnlyWithAnOpportunity_MoreForTheDefinidor_AndNeverWhenSwitchedOff()
        {
            var c=Context();var o=Observe(c,new Vector2(3.2f,7.5f));var plain=OwnPlan(c,.4,new Vector3(1.6f,.8f,5.4f));var own=WithVolley(c,plain,new Vector3(1.7f,1.2f,4.6f));
            foreach(RivalStyleId style in Enum.GetValues(typeof(RivalStyleId)))Assert.That(Sample(Ai(c,style),o,plain,500).Any(s=>s.Volley),Is.False,style+" without an opportunity");
            double shooter=Share(Sample(Ai(c,RivalStyleId.Definidor),o,own),s=>s.Volley),fondo=Share(Sample(Ai(c,RivalStyleId.Fondista),o,own),s=>s.Volley),all=Share(Sample(Ai(c,RivalStyleId.Completo),o,own),s=>s.Volley);
            Assert.That(shooter,Is.GreaterThan(1.5*all),"shooter "+shooter+" all "+all+" fondo "+fondo);Assert.That(all,Is.GreaterThan(fondo));Assert.That(shooter,Is.GreaterThan(.3),"shooter "+shooter);
            var off=Context(p=>p.StyleRivalVolley=0);
            Assert.That(Sample(Ai(off,RivalStyleId.Definidor),Observe(off,new Vector2(3.2f,7.5f)),WithVolley(off,OwnPlan(off,.4,new Vector3(1.6f,.8f,5.4f)),new Vector3(1.7f,1.2f,4.6f)),500).Any(s=>s.Volley),Is.False);
            // A volley release stays inside the volley window (60..78 steps) as the bounce release does in its own.
            foreach(var s in Sample(Ai(c,RivalStyleId.Definidor,AiLevel.Hard),o,own,500).Where(s=>s.Volley))
                Assert.That(70+s.DeltaMs/1000*c.StepHz*c.Tempo,Is.InRange(60-1e-6,78+1e-6));
        }
        [Test] public void Recovery_AfterAnOwnShortShot_FollowsTheStyle_AndAfterALongOne_IsTheStyleWait()
        {
            var c=Context();var shortBall=RallyFixtures.ShortBall(c);var longBall=c.NeutralLaunch(PlayerId.Human);
            float Bounce(BallState b)=>c.Predict(b).Contacts.First(x=>x.Surface==SurfaceId.Floor).Point.z;
            Assert.That(Bounce(shortBall),Is.LessThan(3.5f));Assert.That(Bounce(longBall),Is.GreaterThan(3.5f));
            AiObservation After(BallState b)=>new AiObservation(b,c.WaitPosition(PlayerId.Rival),Vector2.zero,new Vector2(3,7),Vector2.zero,3,0,true);
            var fondo=Ai(c,RivalStyleId.Fondista);var shooter=Ai(c,RivalStyleId.Definidor);var all=Ai(c,RivalStyleId.Completo);
            Assert.That(fondo.Recovery(After(shortBall)).y,Is.EqualTo(c.T.y),"Fondista recovers to the T after its own short ball");
            Assert.That(all.Recovery(After(shortBall)).y,Is.EqualTo(c.T.y));
            // Decision of 19 nov (search01): the Definidor keeps its attack and volley weights but no longer bets on position.
            Assert.That(shooter.Recovery(After(shortBall)).y,Is.EqualTo(c.T.y),"Definidor recovers to the T after its own short ball");
            foreach(var ai in new[]{fondo,shooter,all})Assert.That(ai.Recovery(After(longBall)),Is.EqualTo(ai.Wait));
            Assert.That(fondo.Wait.y,Is.EqualTo(c.T.y+.5f));Assert.That(shooter.Wait.y,Is.EqualTo(c.T.y).Within(1e-5),"Definidor waits on the T");Assert.That(all.Wait.y,Is.EqualTo(c.T.y+.2f).Within(1e-5));
        }
        // Claude, 19 nov (after search02): one noise factor per (style, level) on the five execution/perception noises; temperature stays.
        [Test] public void Difficulty_PerStyle_ScalesTheFiveNoises_AndKeepsTheTemperature()
        {
            var p=new RallyParams();
            var expected=new Dictionary<(RivalStyleId,AiLevel),double>{
                {(RivalStyleId.Fondista,AiLevel.Easy),1.5},{(RivalStyleId.Fondista,AiLevel.Medium),1.75},{(RivalStyleId.Fondista,AiLevel.Hard),2.5},
                {(RivalStyleId.Definidor,AiLevel.Easy),1.25},{(RivalStyleId.Definidor,AiLevel.Medium),1},{(RivalStyleId.Definidor,AiLevel.Hard),1},
                {(RivalStyleId.Completo,AiLevel.Easy),1.5},{(RivalStyleId.Completo,AiLevel.Medium),1.5},{(RivalStyleId.Completo,AiLevel.Hard),1.25}};
            foreach(var e in expected)
            {
                var (style,level)=e.Key;var shared=AiDifficulty.For(p,level);var d=RivalStyle.Difficulty(p,style,level);
                Assert.That(RivalStyle.NoiseK(p,style,level),Is.EqualTo(e.Value),style+" "+level);
                Assert.That((d.PerceptionS,d.StanceSigma,d.AimSigma,d.PowerSigma,d.TimingSigmaMs),Is.EqualTo((shared.PerceptionS*e.Value,shared.StanceSigma*e.Value,shared.AimSigma*e.Value,shared.PowerSigma*e.Value,shared.TimingSigmaMs*e.Value)));
                Assert.That(d.Temperature,Is.EqualTo(shared.Temperature),"temperature is not scaled");
                Assert.That(RivalStyle.NoiseK(p,style,level),Is.GreaterThanOrEqualTo(1),"never below the shared noise");
            }
            foreach(RivalStyleId style in Enum.GetValues(typeof(RivalStyleId)))
                Assert.That(RivalStyle.Difficulty(p,style,AiLevel.Easy).TimingSigmaMs,Is.GreaterThan(RivalStyle.Difficulty(p,style,AiLevel.Medium).TimingSigmaMs*.99),style+": Easy is not tighter than Medium");
        }
        [Test] public void StyleFromSeed_IsDeterministic_AndCoversTheThreeStyles()
        {
            var seen=Enumerable.Range(0,60).Select(i=>RivalStyle.FromSeed(1000+i)).ToArray();
            Assert.That(seen.Distinct().Count(),Is.EqualTo(3));Assert.That(RivalStyle.FromSeed(1234),Is.EqualTo(RivalStyle.FromSeed(1234)));
            Assert.That(new[]{RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo}.Select(RivalStyle.Name),Is.EqualTo(new[]{"Fondista","Definidor","Completo"}));
        }
        [Test] public void InformationContract_RivalAiHoldsNothingPrivate()
        {
            var forbidden=new[]{typeof(PlayerBody),typeof(PointRules),typeof(ClosedGesture),typeof(PresentedPlans),typeof(SwingIntent),typeof(TouchSample),typeof(IRallyBrain),typeof(Trajectory)};
            foreach(var field in typeof(RivalAi).GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static))
                Assert.That(forbidden,Has.No.Member(field.FieldType),field.Name);
            var styleFields=typeof(RivalStyle).GetFields(BindingFlags.Public|BindingFlags.Instance).Select(f=>f.Name).ToArray();
            foreach(var name in new[]{"PerceptionS","StanceSigma","AimSigma","PowerSigma","TimingSigmaMs","Temperature"})Assert.That(styleFields,Has.No.Member(name),"difficulty stays its own axis");
        }
        [Test] public void RallySim_WithStyles_IsDeterministic_RivalWaitsAtItsW_AndTheDefinidorVolleys()
        {
            var c=RallyFixtures.Context();var diag=new int[3];
            string Run(out int volleys,out double cost)
            {
                volleys=0;cost=0;var text=new StringBuilder();
                using(var sim=new RallySim(c,new BotBrain(c,90,5,ShotLine.Cross,ShotHeight.Mid,ShotPower.Strong),new RivalAi(c,AiDifficulty.For(c.Params,AiLevel.Medium),RivalStyleId.Definidor,6)))
                {
                    Assert.That(sim.Rival.Position,Is.EqualTo(sim.WaitOf(PlayerId.Rival)));
                    for(int m=0;m<3;m++){var match=sim.PlayMatch(m%2==0?PlayerId.Human:PlayerId.Rival);foreach(var p in match.Points){text.Append(p.Winner).Append(p.Shots).Append(p.Steps).Append('/');
                        foreach(var r in p.Receptions){if(r.Who==PlayerId.Rival&&r.Volley&&r.Result==ReceptionResult.Contact)volleys++;if(r.Who==PlayerId.Rival){diag[0]++;if(r.VolleyOffered)diag[1]++;if(r.Volley)diag[2]++;}}cost+=p.RivalFatigue.VolleyCost;}}
                }
                return text.ToString();
            }
            string a=Run(out int va,out double ca),b=Run(out int vb,out double cb);
            TestContext.WriteLine("RIVAL_VOLLEY_DIAG receptions/offered/aimed="+string.Join("/",diag));
            Assert.That(a,Is.EqualTo(b));Assert.That(va,Is.EqualTo(vb));
            Assert.That(va,Is.GreaterThan(0),"the Definidor volleys in three matches against strong cross drives");
            Assert.That(ca,Is.EqualTo(va*c.Params.VolleyEnergyCost).Within(1e-9),"each rival volley costs the volley energy");
        }
    }
}
