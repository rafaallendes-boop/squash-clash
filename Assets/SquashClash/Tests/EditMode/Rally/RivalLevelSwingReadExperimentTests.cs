using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // [EXPERIMENTO] set25 (28 nov; local branch, never shipped): the rival reads long swings only at Medium and Hard.
    public class RivalLevelSwingReadExperimentTests
    {
        private static RallyContext Context(double minLevel,double leadMs=100,double misread=.25,double longOnly=1)=>RallyFixtures.Context(new RallyParams{RivalSwingReadLeadMs=leadMs,RivalSwingMisreadP=misread,RivalSwingReadLongOnly=longOnly,RivalSwingReadMinLevel=minLevel});
        // The human of the set22 tests: a drop or a straight drive, 50/50 by its own generator.
        private sealed class MixBrain : IRallyBrain
        {
            private readonly RallyContext ctx;private readonly RallyRng random,timing;private readonly bool dropOnly,filter;
            public MixBrain(RallyContext ctx,long seed,bool dropOnly,bool filter){this.ctx=ctx;random=new RallyRng(seed);timing=new RallyRng(seed+7919);this.dropOnly=dropOnly;this.filter=filter;}
            public int PerceptionSteps=>0;
            public Vector2 StanceNoise()=>Vector2.zero;
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                if(!own.Reachable||!own.Contact.Exists||own.Contact.TStarStep<0)return default;
                var at=own.Contact.PStar;var options=ShotOptions.Build(ctx,at);
                bool drop=dropOnly||random.NextDouble()<.5;
                var line=drop?ShotLine.Drop:ShotLine.Straight;var height=drop?ShotHeight.Low:ShotHeight.Mid;var power=drop?ShotPower.Medium:ShotPower.Strong;
                options.Sort((a,b)=>Distance(a,line,height,power).CompareTo(Distance(b,line,height,power)));
                double delta=timing.NextGaussian(30);
                if(filter)foreach(var option in options)if(RallyContact.Forecast(ctx,at,option.Aim,option.P).Good)return new AiShot(true,option,option.Aim,option.P,delta,false);
                return new AiShot(true,options[0],options[0].Aim,options[0].P,delta,false);
            }
            private static int Distance(ShotOption o,ShotLine line,ShotHeight height,ShotPower power)=>(o.Line==line?0:o.Line==ShotLine.Drop||line==ShotLine.Drop?8:4)+2*Math.Abs((int)o.Height-(int)height)+(o.Power==power?0:1);
        }
        // A rival brain that does not implement IRallyLevel.
        private sealed class Probe : IRallyBrain,IRallyPositioning,IRallyServer
        {
            private readonly RivalAi ai;
            public Probe(RivalAi ai){this.ai=ai;}
            public int PerceptionSteps=>ai.PerceptionSteps;
            public Vector2 StanceNoise()=>ai.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)=>ai.Decide(o,own);
            public Vector2 Wait=>ai.Wait;
            public Vector2 Recovery(in AiObservation a)=>ai.Recovery(a);
            public ServeType ChooseServe(ServiceBox box)=>ai.ChooseServe(box);
        }
        private sealed class Played{public RallyOutcome Outcome;public List<SwingRead> Reads;public PlayerId? Winner;}
        private static List<Played> PlayMatches(RallyContext c,AiLevel level,bool wrapped,int matches,long seed=20261128)
        {
            var list=new List<Played>();
            for(int m=0;m<matches;m++)
            {
                var ai=RivalAi.For(c,level,RivalStyleId.Completo,seed+1+2*m);IRallyBrain rival=wrapped?(IRallyBrain)new Probe(ai):ai;
                using(var sim=new RallySim(c,new MixBrain(c,seed+2*m,false,true),rival))
                {
                    var score=new MatchScore(7,2,m%2==0?PlayerId.Human:PlayerId.Rival);int replays=0;
                    while(!score.Finished)
                    {
                        var o=sim.PlayPoint(score.Server,score.Box);
                        list.Add(new Played{Outcome=o,Reads=sim.SwingReads.ToList(),Winner=o.Winner});
                        if(o.Winner.HasValue)score.PointWon(o.Winner.Value);else if(++replays>50)break;
                    }
                }
            }
            return list;
        }
        private static string DumpPoints(List<Played> played)=>string.Join("|",played.Select(p=>p.Winner+":"+p.Outcome.Receptions.Count+":"+string.Join(";",p.Outcome.Receptions.Select(r=>r.Who+","+r.Result+","+r.ContactStep))));
        private static string DumpReads(List<Played> played)=>string.Join("|",played.Select(p=>string.Join(";",p.Reads.Select(r=>r.RivalReception+","+r.Kind+","+r.LeadSteps+","+r.ReadStep+","+r.Planned+","+r.Stance.x.ToString("R")+","+r.Stance.y.ToString("R")))));
        private static int Reads(List<Played> played)=>played.Sum(p=>p.Reads.Count);

        [Test] public void Defaults_MinLevelIsZero()=>Assert.That(new RallyParams().RivalSwingReadMinLevel,Is.EqualTo(0));
        [Test] public void Level_FromTheConfiguredDifficulty()
        {
            var c=Context(1);
            foreach(AiLevel level in Enum.GetValues(typeof(AiLevel)))
                foreach(RivalStyleId style in Enum.GetValues(typeof(RivalStyleId)))
                    Assert.That(RivalAi.For(c,level,style,5).Level,Is.EqualTo(level),level+" "+style);
            foreach(AiLevel level in Enum.GetValues(typeof(AiLevel)))
            {
                var d=AiDifficulty.For(c.Params,level);const double k=2.5;
                var scaled=new AiDifficulty(d.PerceptionS*k,d.StanceSigma*k,d.AimSigma*k,d.PowerSigma*k,d.TimingSigmaMs*k,d.Temperature);
                Assert.That(new RivalAi(c,scaled,RivalStyleId.Definidor,5).Level,Is.EqualTo(level),"scaled "+level);
            }
            var d0=AiDifficulty.For(c.Params,AiLevel.Medium);
            var none=new AiDifficulty(d0.PerceptionS,d0.StanceSigma,d0.AimSigma,d0.PowerSigma,d0.TimingSigmaMs,.77);
            Assert.That(new RivalAi(c,none,RivalStyleId.Completo,5).Level,Is.Null);
        }
        [Test] public void MinLevel1_Easy_NoReads_AndThePointsAreThoseWithTheReadOff()
        {
            var played=PlayMatches(Context(1),AiLevel.Easy,false,3);var none=PlayMatches(Context(0,0,0,0),AiLevel.Easy,false,3);
            Assert.That(played.Count,Is.GreaterThan(5));
            foreach(var p in played)Assert.That(p.Reads,Is.Empty);
            int longStrokes=0;
            foreach(var p in played){var rec=p.Outcome.Receptions;for(int j=0;j+1<rec.Count;j++)if(rec[j].Who==PlayerId.Human&&rec[j].Result==ReceptionResult.Contact&&rec[j+1].Who==PlayerId.Rival&&rec[j].Line!=ShotLine.Drop&&rec[j].Line!=ShotLine.Boast)longStrokes++;}
            Assert.That(longStrokes,Is.GreaterThan(5));
            Assert.That(DumpPoints(played),Is.EqualTo(DumpPoints(none)));
        }
        [Test] public void MinLevel1_MediumAndHard_ReadLikeMinLevel0()
        {
            foreach(var level in new[]{AiLevel.Medium,AiLevel.Hard})
            {
                var gated=PlayMatches(Context(1),level,false,3);var open=PlayMatches(Context(0),level,false,3);
                Assert.That(Reads(open),Is.GreaterThan(5),level.ToString());
                Assert.That(Reads(gated),Is.EqualTo(Reads(open)),level.ToString());
                Assert.That(DumpReads(gated),Is.EqualTo(DumpReads(open)),level.ToString());
                Assert.That(DumpPoints(gated),Is.EqualTo(DumpPoints(open)),level.ToString());
            }
        }
        [Test] public void MinLevel0_Easy_Reads()=>Assert.That(Reads(PlayMatches(Context(0),AiLevel.Easy,false,3)),Is.GreaterThan(5));
        [Test] public void MinLevel1_RivalWithoutLevel_NoReads()
        {
            Assert.That(Reads(PlayMatches(Context(1),AiLevel.Medium,true,3)),Is.EqualTo(0));
            Assert.That(Reads(PlayMatches(Context(0),AiLevel.Medium,true,3)),Is.GreaterThan(5));
        }
    }
}
