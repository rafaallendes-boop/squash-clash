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
    // [EXPERIMENTO] set22 (27 nov; local branch, never shipped): the rival reads only the long swings of the human, and the misread coin mixes the timing error.
    public class RivalLongSwingReadExperimentTests
    {
        private static RallyContext Context(double leadMs=0,double misread=0,double longOnly=0)=>RallyFixtures.Context(new RallyParams{RivalSwingReadLeadMs=leadMs,RivalSwingMisreadP=misread,RivalSwingReadLongOnly=longOnly});
        // Plays a drop or a straight drive, 50/50 by its own generator (the stroke chosen like the IntentBot of the deep-defense experiment), or only the drop.
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
        private static List<Played> PlayMatches(RallyContext c,int matches,bool dropOnly,bool filter,long seed=20261126)
        {
            var list=new List<Played>();
            for(int m=0;m<matches;m++)
            {
                var probe=new Probe(new RivalAi(c,AiDifficulty.For(c.Params,AiLevel.Medium),RivalStyleId.Completo,seed+1+2*m));
                using(var sim=new RallySim(c,new MixBrain(c,seed+2*m,dropOnly,filter),probe))
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

        [Test] public void Defaults_LongOnlyIsZero()=>Assert.That(new RallyParams().RivalSwingReadLongOnly,Is.EqualTo(0));
        [Test] public void LongSwing_DrivesMediumAndLobsAreLong_DropsAndBoastsAreShort()
        {
            var c=Context(200,0,1);int longs=0,drops=0;
            foreach(var o in ShotOptions.Build(c,new Vector3(1.5f,.8f,7f)))
            {
                var shot=new AiShot(true,o,o.Aim,o.P,0,false);
                if(o.Line==ShotLine.Straight||o.Line==ShotLine.Cross){longs++;Assert.That(RivalSwingRead.LongSwing(c,shot),Is.True,o.Line+" "+o.Height+" "+o.Power);}
                else if(o.Line==ShotLine.Drop){drops++;Assert.That(RivalSwingRead.LongSwing(c,shot),Is.False,"drop "+o.Height+" "+o.Power);}
            }
            Assert.That(longs,Is.GreaterThan(0));Assert.That(drops,Is.GreaterThan(0));
            var list=new List<ShotOption>();ShotOptions.AddBoasts(c,new Vector3(.6f,.8f,8.5f),list);
            Assert.That(list.Count,Is.GreaterThan(0));
            foreach(var o in list)Assert.That(RivalSwingRead.LongSwing(c,new AiShot(true,o,o.Aim,o.P,0,false,false,o.Side)),Is.False,"boast "+o.Height+" "+o.Power);
        }
        [Test] public void LongOnly_Lead200_NoMisread_EachLongHumanStrokeHasOneRead_AndNoShortOne()
        {
            var c=Context(200,0,1);int longStrokes=0,shortStrokes=0,reads=0;
            foreach(var p in PlayMatches(c,4,false,true))
            {
                var rec=p.Outcome.Receptions;
                for(int j=0;j+1<rec.Count;j++)
                {
                    if(rec[j].Who!=PlayerId.Human||rec[j].Result!=ReceptionResult.Contact||rec[j+1].Who!=PlayerId.Rival)continue;
                    bool isShort=rec[j].Line==ShotLine.Drop||rec[j].Line==ShotLine.Boast;
                    if(isShort)shortStrokes++;else longStrokes++;
                    Assert.That(p.Reads.Count(r=>r.RivalReception==j+1),Is.EqualTo(isShort?0:1),"stroke "+j+" "+rec[j].Line);
                }
                foreach(var r in p.Reads){reads++;Assert.That(r.Kind,Is.EqualTo(SwingReadKind.Right));}
            }
            Assert.That(longStrokes,Is.GreaterThan(5));Assert.That(shortStrokes,Is.GreaterThan(5));Assert.That(reads,Is.GreaterThan(5));
        }
        [Test] public void LongOnly_DropOnlyHuman_GivesTheSamePointsAsNoRead()
        {
            var read=PlayMatches(Context(200,.25,1),3,true,false);var none=PlayMatches(Context(),3,true,false);
            Assert.That(read.Count,Is.GreaterThan(5));
            Assert.That(read.SelectMany(p=>p.Reads),Is.Empty);
            Assert.That(DumpPoints(read),Is.EqualTo(DumpPoints(none)));
        }
        [Test] public void Kind_ManyTimingErrors_SameContact_MisreadRateIsNearP_BothKindsHalf()
        {
            var c=Context(100,.25);var rng=new RallyRng(7);int n=4000,mis=0,dir=0;
            for(int i=0;i<n;i++)
            {
                var k=RivalSwingRead.Kind(c,1234,new Vector3(3.2f,.8f,6f),rng.NextGaussian(30));
                if(k!=SwingReadKind.Right){mis++;if(k==SwingReadKind.Direction)dir++;}
            }
            Assert.That(mis/(double)n,Is.EqualTo(.25).Within(.03));Assert.That(dir/(double)mis,Is.EqualTo(.5).Within(.06));
        }
    }
}
