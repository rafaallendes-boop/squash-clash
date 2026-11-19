using System.IO;
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
    // F2: A7 (the arm is the striker's), A6 and A1 in RallySim.
    public sealed class FatigueArmRallyTests
    {
        private static RallyContext Context(System.Func<ParamSet,ParamSet> edit=null)
            =>new RallyContext(EffectiveConfig.From(edit==null?ConfigFixtures.Default():edit(ConfigFixtures.Default())),new RallyParams(),new AnalyticBallSimulationFactory(Physics.bounceThreshold));
        private static RallySim Sim(RallyContext ctx,int i=0)=>new RallySim(ctx,new BotBrain(ctx,30,20261117+2*i),new FondoAi(ctx,AiDifficulty.For(ctx.Params,AiLevel.Medium),20261118+2*i));
        private static void Tire(FatigueState energy,int strikes){energy.BeginPoint();for(int i=0;i<strikes;i++)energy.Strike(1);}
        private static ReceptionLog FirstStrike(RallyOutcome o,PlayerId who)=>o.Receptions.First(r=>r.Who==who&&r.Result==ReceptionResult.Contact&&r.Vectors!=null);
        [Test] public void A7_ChangingOnlyTheReceiversEnergy_LeavesTheStrikeUntouched()
        {
            var ctx=Context();ReceptionLog baseline,rested;
            using(var sim=Sim(ctx)){baseline=FirstStrike(sim.Play(PlayerId.Human),PlayerId.Human);}
            using(var sim=Sim(ctx)){Tire(sim.RivalEnergy,65);rested=FirstStrike(sim.Play(PlayerId.Human),PlayerId.Human);}
            Assert.That(rested.Vectors,Is.EqualTo(baseline.Vectors));Assert.That(rested.Input.Aim,Is.EqualTo(baseline.Input.Aim));Assert.That(rested.Input.Speed,Is.EqualTo(baseline.Input.Speed));Assert.That(rested.FatigueStriker,Is.EqualTo(baseline.FatigueStriker));
            Assert.That(baseline.Vectors.Any(v=>v.Cause=="fatigue"),Is.False,"a rested striker has no fatigue vector");
        }
        [Test] public void A7_ChangingOnlyTheStrikersEnergy_AddsTheFatigueVector_AtThatPlayersFactor()
        {
            var ctx=Context();ReceptionLog tired;
            using(var sim=Sim(ctx)){Tire(sim.HumanEnergy,65);tired=FirstStrike(sim.Play(PlayerId.Human),PlayerId.Human);}
            var vector=tired.Vectors.Where(v=>v.Cause=="fatigue").ToArray();
            Assert.That(vector,Has.Length.EqualTo(1),"fatigue vector present for the tired striker");
            Assert.That(tired.FatigueStriker,Is.GreaterThan(0));
            Assert.That(vector[0].DeltaSpeedFraction,Is.EqualTo(-(float)(ctx.Fatigue.ArmSpeedMax*tired.FatigueStriker)).Within(1e-7));
            Assert.That(ctx.Fatigue.Arm(0).Active,Is.True);
        }
        [Test] public void A7_TheRivalStrikesWithItsOwnFactor()
        {
            var ctx=Context();ReceptionLog tired;
            using(var sim=Sim(ctx)){Tire(sim.RivalEnergy,65);var o=sim.Play(PlayerId.Rival);tired=FirstStrike(o,PlayerId.Rival);}
            Assert.That(tired.Vectors.Count(v=>v.Cause=="fatigue"),Is.EqualTo(1));Assert.That(tired.FatigueStriker,Is.GreaterThan(0));
        }
        [Test] public void A6_AllActive_TwoRunsAreIdentical_AndTheArmIsExercised()
        {
            string Run(out int fatigueVectors)
            {
                var ctx=Context();int n=0;
                using(var sim=Sim(ctx))
                {
                    var match=sim.PlayMatch(PlayerId.Human);Assert.That(match.Aborted,Is.False);
                    var text=string.Join("/",match.Points.Select(p=>FatigueIntegrationTests.Signature(p,sim)));
                    n=match.Points.SelectMany(p=>p.Receptions).Count(r=>r.Vectors!=null&&r.Vectors.Any(v=>v.Cause=="fatigue"));fatigueVectors=n;return text;
                }
            }
            string a=Run(out int na),b=Run(out int nb);
            Assert.That(a,Is.EqualTo(b));Assert.That(na,Is.EqualTo(nb));Assert.That(na,Is.GreaterThan(0),"at least one fatigue vector in a whole match");
        }
        [Test] public void A1_EnabledWithZeroArm_ReplaysTheFrozenF1AdjustedSignatures()
        {
            var ctx=Context(p=>p.With("fatigue.armSpeedMax",0).With("fatigue.positionGainMax",0).With("fatigue.costPerMeter",.4).With("fatigue.recoverBetween",.25)); // the frozen reference (F1 values, re-anchored with A2 on 18 nov) was measured with the F1 values
            var rows=File.ReadAllLines(Path.Combine(Application.dataPath,"..","docs/validation/evidence/2026-11-18-g1-a2-complete/fatigue-replay/arm0/points.tsv")).Skip(1).Select(r=>r.Split('\t')).Where(r=>int.Parse(r[0])<4).ToArray();
            for(int i=0;i<4;i++)
            using(var sim=Sim(ctx,i))
            {
                var score=new MatchScore(7,2,i%2==0?PlayerId.Human:PlayerId.Rival);int point=0;
                while(!score.Finished)
                {var o=sim.PlayPoint(score.Server,score.Box);Assert.That(FatigueIntegrationTests.Signature(o,sim),Is.EqualTo(rows.Single(r=>int.Parse(r[0])==i&&int.Parse(r[1])==point)[9]),i+"/"+point);score.PointWon(o.Winner.Value);point++;}
            }
        }
    }
}
