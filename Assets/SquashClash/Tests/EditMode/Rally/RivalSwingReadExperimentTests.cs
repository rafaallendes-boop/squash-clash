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
    // [EXPERIMENTO] set21 (27 nov; local branch, never shipped): the rival reads the human's swing and runs before the contact.
    public class RivalSwingReadExperimentTests
    {
        private static RallyContext Context(double leadMs=0,double misread=0)=>RallyFixtures.Context(new RallyParams{RivalSwingReadLeadMs=leadMs,RivalSwingMisreadP=misread});
        // The real rival, plus what its body was doing when each of its receptions began (the read stance is still its target during the reaction).
        private sealed class Probe : IRallyBrain,IRallyPositioning,IRallyServer
        {
            private readonly RivalAi ai;public RallySim Sim;public readonly List<(Vector2 target,bool reacting)> Starts=new List<(Vector2,bool)>();
            public Probe(RivalAi ai){this.ai=ai;}
            public int PerceptionSteps=>ai.PerceptionSteps;
            public Vector2 StanceNoise()=>ai.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own){Starts.Add((Sim.Rival.Target,Sim.Rival.Reacting));return ai.Decide(o,own);}
            public Vector2 Wait=>ai.Wait;
            public Vector2 Recovery(in AiObservation a)=>ai.Recovery(a);
            public ServeType ChooseServe(ServiceBox box)=>ai.ChooseServe(box);
        }
        private sealed class Played{public RallyOutcome Outcome;public List<SwingRead> Reads;public List<(Vector2 target,bool reacting)> Starts;}
        private static List<Played> PlayMatches(RallyContext c,int matches,long seed=20261126)
        {
            var list=new List<Played>();
            for(int m=0;m<matches;m++)
            {
                var probe=new Probe(new RivalAi(c,AiDifficulty.For(c.Params,AiLevel.Medium),RivalStyleId.Completo,seed+1+2*m));
                using(var sim=new RallySim(c,new BotBrain(c,30,seed+2*m),probe))
                {
                    probe.Sim=sim;var score=new MatchScore(7,2,m%2==0?PlayerId.Human:PlayerId.Rival);int replays=0;
                    while(!score.Finished)
                    {
                        probe.Starts.Clear();var o=sim.PlayPoint(score.Server,score.Box);
                        list.Add(new Played{Outcome=o,Reads=sim.SwingReads.ToList(),Starts=probe.Starts.ToList()});
                        if(o.Winner.HasValue)score.PointWon(o.Winner.Value);else if(++replays>50)break;
                    }
                }
            }
            return list;
        }
        private static string Dump(List<Played> played)=>string.Join("|",played.SelectMany(p=>p.Reads).Select(r=>r.RivalReception+","+r.Kind+","+r.LeadSteps+","+r.ReadStep+","+r.ContactStep+","+r.Planned+","+r.Stance.x.ToString("R")+","+r.Stance.y.ToString("R")+","+r.BodyAtRead.x.ToString("R")+","+r.BodyAtRead.y.ToString("R")));

        [Test] public void Defaults_AreNeutral_AndNoPointRecordsAnyRead()
        {
            var d=new RallyParams();Assert.That(d.RivalSwingReadLeadMs,Is.EqualTo(0));Assert.That(d.RivalSwingMisreadP,Is.EqualTo(0));
            var played=PlayMatches(Context(),2);Assert.That(played.Count,Is.GreaterThan(5));
            foreach(var p in played)Assert.That(p.Reads,Is.Empty);
        }
        [Test] public void LeadSteps_FollowTheDeclaredFormula()
        {
            Assert.That(RivalSwingRead.LeadSteps(Context(0)),Is.EqualTo(0));
            foreach(var ms in new[]{100.0,200.0})
            {
                var c=Context(ms);Assert.That(RivalSwingRead.LeadSteps(c),Is.EqualTo((int)Math.Round(ms/1000*c.Tempo*c.StepHz)));
                Assert.That(RivalSwingRead.LeadSteps(c),Is.GreaterThan(0));
            }
        }
        [Test] public void Lead200_NoMisread_EveryReadHumanStrokeHasOneRightRead()
        {
            var c=Context(200,0);int lead=RivalSwingRead.LeadSteps(c);int strokes=0,reads=0,full=0,planned=0;
            foreach(var p in PlayMatches(c,3))
            {
                var rec=p.Outcome.Receptions;
                for(int j=0;j+1<rec.Count;j++)
                {
                    if(rec[j].Who!=PlayerId.Human||rec[j].Result!=ReceptionResult.Contact||rec[j+1].Who!=PlayerId.Rival)continue;
                    strokes++;Assert.That(p.Reads.Count(r=>r.RivalReception==j+1),Is.EqualTo(1),"stroke "+j);
                }
                foreach(var r in p.Reads)
                {
                    reads++;Assert.That(r.Kind,Is.EqualTo(SwingReadKind.Right));Assert.That(r.LeadSteps,Is.InRange(0,lead));if(r.LeadSteps==lead)full++;
                    if(r.Planned){planned++;Assert.That(r.Stance,Is.EqualTo(c.ClampBody(r.Stance)));}
                }
            }
            Assert.That(strokes,Is.GreaterThan(10));Assert.That(planned,Is.GreaterThan(0));Assert.That(full,Is.GreaterThan(reads/2));
        }
        [Test] public void Lead200_AlwaysMisread_OnlyDirectionAndLengthAppear_BothKinds()
        {
            var kinds=PlayMatches(Context(200,1),4).SelectMany(p=>p.Reads).Select(r=>r.Kind).ToList();
            Assert.That(kinds,Is.Not.Empty);Assert.That(kinds.All(k=>k==SwingReadKind.Direction||k==SwingReadKind.Length),Is.True);
            Assert.That(kinds.Contains(SwingReadKind.Direction),Is.True);Assert.That(kinds.Contains(SwingReadKind.Length),Is.True);
        }
        [Test] public void DirectionRead_IsTheMirrorOfTheRightRead()
        {
            var c=Context(200,0);var rival=RallyFixtures.Body(c,PlayerId.Rival,c.WaitPosition(PlayerId.Rival));int found=0;
            foreach(var at in new[]{new Vector3(1.5f,.8f,4f),new Vector3(3.2f,.8f,6f),new Vector3(4.5f,.7f,3f)})
            {
                var o=ShotOptions.Build(c,at).First(x=>x.Line==ShotLine.Straight&&x.Height==ShotHeight.Mid&&x.Power==ShotPower.Medium);
                var shot=new AiShot(true,o,o.Aim,o.P,0,false);
                var right=RivalSwingRead.Stance(c,at,shot,SwingReadKind.Right,rival,100,16);
                var dir=RivalSwingRead.Stance(c,at,shot,SwingReadKind.Direction,rival,100,16);
                if(!right.HasValue)continue;found++;
                Assert.That(dir.HasValue,Is.True);Assert.That(dir.Value,Is.EqualTo(c.ClampBody(new Vector2(c.Court.Width-right.Value.x,right.Value.y))));
            }
            Assert.That(found,Is.GreaterThan(0));
        }
        [Test] public void SameSeeds_GiveTheSameReads()
        {
            var a=Dump(PlayMatches(Context(200,.25),2));var b=Dump(PlayMatches(Context(200,.25),2));
            Assert.That(a,Is.Not.Empty);Assert.That(b,Is.EqualTo(a));
        }
        [Test] public void APlannedRead_TurnsTheRivalBodyToTheReadStance_UntilItsOwnReactionEnds()
        {
            int checkedReads=0;
            foreach(var p in PlayMatches(Context(200,.25),3))
                foreach(var r in p.Reads.Where(x=>x.Planned&&x.RivalReception<p.Outcome.Receptions.Count))
                {
                    int n=p.Outcome.Receptions.Take(r.RivalReception).Count(x=>x.Who==PlayerId.Rival);
                    if(n>=p.Starts.Count||!p.Starts[n].reacting)continue;
                    Assert.That(p.Starts[n].target,Is.EqualTo(r.Stance));checkedReads++;
                }
            Assert.That(checkedReads,Is.GreaterThan(0));
        }
    }
}
