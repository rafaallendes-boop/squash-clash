using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class FatigueIntegrationTests
    {
        private static RallyContext Context(bool on,RallyParams parameters=null)=>new RallyContext(EffectiveConfig.From(ConfigFixtures.Default().With("fatigue.enabled",on?1:0)),parameters??new RallyParams(),new AnalyticBallSimulationFactory(Physics.bounceThreshold));
        private static FatigueState Energy(RallySim sim,PlayerId who)
        {var p=typeof(RallySim).GetProperty(who==PlayerId.Human?"HumanEnergy":"RivalEnergy");Assert.That(p,Is.Not.Null,"RallySim must integrate fatigue");return (FatigueState)p.GetValue(sim);}
        [Test] public void Off_ReplaysTheFrozenMainReference_BitForBit()
        {
            var ctx=Context(false,new RallyParams{ServeKindOn=0});var rows=File.ReadAllLines(Path.Combine(Application.dataPath,"..","docs/validation/evidence/2026-11-26-posiciones-saque/fatigue-replay/off/points.tsv")).Skip(1).Select(r=>r.Split('\t')).Where(r=>int.Parse(r[0])<4).ToArray();
            for(int i=0;i<4;i++)
            using(var sim=new RallySim(ctx,new BotBrain(ctx,30,20261117+2*i),new FondoAi(ctx,AiDifficulty.For(ctx.Params,AiLevel.Medium),20261118+2*i)))
            {
                Energy(sim,PlayerId.Human);var score=new MatchScore(7,2,i%2==0?PlayerId.Human:PlayerId.Rival);int point=0;
                while(!score.Finished)
                {var o=sim.PlayPoint(score.Server,score.Box);Assert.That(Signature(o,sim),Is.EqualTo(rows.Single(r=>int.Parse(r[0])==i&&int.Parse(r[1])==point)[9]),i+"/"+point);Assert.That(Energy(sim,PlayerId.Human).Energy,Is.EqualTo(100));score.PointWon(o.Winner.Value);point++;}
            }
        }
        [Test] public void On_IsDeterministic_PersistsAcrossPoints_AndResetsAtMatchStart()
        {
            string Run()
            {
                var ctx=Context(true);
                using(var sim=new RallySim(ctx,new BotBrain(ctx,30,20261117),new FondoAi(ctx,AiDifficulty.For(ctx.Params,AiLevel.Medium),20261118)))
                {
                    var energy=Energy(sim,PlayerId.Human);energy.BeginPoint();energy.Strike(1);Assert.That(energy.Energy,Is.LessThan(100));
                    var match=sim.PlayMatch(PlayerId.Human);Assert.That(match.Aborted,Is.False);
                    var field=typeof(RallyOutcome).GetField("HumanFatigue");Assert.That(field,Is.Not.Null);
                    var first=(FatiguePoint)field.GetValue(match.Points[0]);Assert.That(first.Start,Is.EqualTo(100));
                    var summaries=match.Points.Select(p=>(FatiguePoint)field.GetValue(p)).ToArray();
                    for(int i=1;i<summaries.Length;i++)Assert.That(summaries[i].Start,Is.EqualTo(summaries[i-1].AfterRecovery));
                    Assert.That(summaries.Sum(p=>p.Meters),Is.GreaterThan(0));
                    return string.Join("/",match.Points.Select(SignatureOnly))+"/"+string.Join("/",summaries.Select(p=>p.End.ToString("R",CultureInfo.InvariantCulture)));
                }
            }
            Assert.That(Run(),Is.EqualTo(Run()));
        }
        private static string SignatureOnly(RallyOutcome o)=>o.Winner+":"+o.Shots+":"+o.Steps;
        [Test] public void DelayedContact_UsesItsStep_AndReplaysSubsequentEnergyAccounting()
        {
            var p=FatigueSettings.From(ConfigFixtures.Default());var expected=new FatigueState(p,1);var delayed=new FatigueState(p,1);
            expected.BeginPoint();delayed.BeginPoint();delayed.BeginFlight(0);
            for(int step=1;step<=120;step++)
            {expected.Step(0,BodyPhase.Set,0,false);if(step==10)expected.Strike(1);delayed.Advance(step,0,BodyPhase.Set,0,false);}
            Assert.That(delayed.EnergyAt(10),Is.EqualTo(1+10*.5/120).Within(1e-10));
            delayed.StrikeAt(10,1);Assert.That(delayed.Energy,Is.EqualTo(expected.Energy));Assert.That(delayed.Point.QuietRecovered,Is.EqualTo(expected.Point.QuietRecovered));
            delayed.StrikeAt(10,1);Assert.That(delayed.Energy,Is.EqualTo(expected.Energy),"duplicate callback must not charge twice");
        }
        [Test] public void DisabledHash_IsMeasuredInUnity()
        {var p=ConfigFixtures.Default().With("fatigue.enabled",0);Assert.That(EffectiveConfig.From(p).SimulationKey,Is.EqualTo("166a96e319c7"));TestContext.WriteLine("F1_OFF_HASH="+p.Hash);}
        public static string Signature(RallyOutcome o,RallySim sim)
        {
            using(var stream=new MemoryStream())
            using(var writer=new BinaryWriter(stream))
            {
                foreach(string name in new[]{"End","Cause","Winner","FirstReceiver","Shots","SystemFaults","IgnoredAfterClose","RejectedContacts","Steps","SimSeconds","RealSeconds","LastReception","Striker","TerminalSurface","OutAfterBounce","Server","Box"})
                    Write(writer,typeof(RallyOutcome).GetField(name).GetValue(o));
                writer.Write(o.Receptions.Count);
                foreach(var r in o.Receptions)
                    foreach(string name in new[]{"Who","Result","Perfect","Reachable","Defensive","DeltaMs","SlackS","Line","Height","Power","ContactStep","Reason","Vectors","Input"})
                        Write(writer,typeof(ReceptionLog).GetField(name).GetValue(r));
                Write(writer,sim.Human.Position);Write(writer,sim.Human.Velocity);Write(writer,sim.Rival.Position);Write(writer,sim.Rival.Velocity);
                writer.Flush();using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-","").ToLowerInvariant();
            }
        }
        private static void Write(BinaryWriter w,object value)
        {
            w.Write(value!=null);if(value==null)return;
            var t=value.GetType();
            if(t.IsEnum){w.Write(Convert.ToInt32(value));return;}
            if(value is bool b){w.Write(b);return;}if(value is int i){w.Write(i);return;}if(value is long l){w.Write(l);return;}
            if(value is float f){w.Write(f);return;}if(value is double d){w.Write(d);return;}if(value is string s){w.Write(s);return;}
            if(value is Array a){w.Write(a.Length);foreach(var item in a)Write(w,item);return;}
            foreach(var field in t.GetFields(BindingFlags.Public|BindingFlags.Instance).OrderBy(x=>x.Name,StringComparer.Ordinal))Write(w,field.GetValue(value));
        }
    }
}
