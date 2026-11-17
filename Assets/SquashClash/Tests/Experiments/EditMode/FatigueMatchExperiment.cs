using System;
using System.Collections;
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
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    // Method committed before data: docs/validation/2026-11-17-f1-fatigue-method.md (F1) and docs/validation/2026-11-17-f2-fatigue-method.md (F2: SC_EXPERIMENT=f2, variants f2off and f2all).
    public sealed class FatigueMatchExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        [Test,Explicit,Category("Experiments"),Timeout(7200000)]
        public void WriteMatches()
        {
            string mode=Environment.GetEnvironmentVariable("SC_F1_VARIANT");
            bool f2=Environment.GetEnvironmentVariable("SC_EXPERIMENT")=="f2";
            Assert.That(f2?new[]{"f2off","f2all"}:new[]{"baseline","off","soft","adjusted"},Does.Contain(mode));
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            string dir=Path.Combine(root,f2?"Logs/experiments/f2-fatigue":"Logs/experiments/f1-fatigue",mode);
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence");
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            if(mode=="baseline")Assert.That(preset.Hash,Is.EqualTo("430da2faa818"));
            else preset=preset.With("fatigue.enabled",mode=="off"||mode=="f2off"?0:1);
            if(mode=="f2off")Assert.That(preset.Hash,Is.EqualTo("5cf3885e7c02"));
            if(mode=="f2all")Assert.That(preset.Hash,Is.EqualTo("2ec0c953786a"));
            if(f2)Assert.That(preset.Ordered.Count,Is.EqualTo(132));
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            Assert.That(ctx.Config.SimulationKey,Is.EqualTo("166a96e319c7"));if(f2)Assert.That(ctx.Params.Hash,Is.EqualTo("927e304bbfec"));
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"source.tsv"),"variant\tdefaults\trallyParams\tsimulationKey\tunity\tkeys\n"+mode+"\t"+preset.Hash+"\t"+ctx.Params.Hash+"\t"+ctx.Config.SimulationKey+"\t"+Application.unityVersion+"\t"+preset.Ordered.Count+"\n");
            using(var points=new StreamWriter(Path.Combine(dir,"points.tsv")))
            using(var receptions=new StreamWriter(Path.Combine(dir,"receptions.tsv")))
            using(var matches=new StreamWriter(Path.Combine(dir,"matches.tsv")))
            {
                points.WriteLine("match\tpoint\thumanBefore\trivalBefore\tserver\twinner\tshots\tsteps\trealSeconds\tsignature\tfatigueHuman\tfatigueRival");
                receptions.WriteLine("match\tpoint\treception\twho\tresult\tstep\tdeltaMs\tf_r"+(f2?"\tf_s\tspeed":""));
                matches.WriteLine("match\thuman\trival\twinner\tpoints\treplays");
                for(int i=0;i<200;i++)
                {
                    using(var sim=new RallySim(ctx,new BotBrain(ctx,30,20261117+2*i),new FondoAi(ctx,AiDifficulty.For(ctx.Params,AiLevel.Medium),20261118+2*i)))
                    {
                        var score=new MatchScore(7,2,i%2==0?PlayerId.Human:PlayerId.Rival);int point=0,replays=0;
                        while(!score.Finished)
                        {
                            var outcome=sim.PlayPoint(score.Server,score.Box);
                            points.WriteLine(string.Join("\t",new[]{i.ToString(C),point.ToString(C),score.Human.ToString(C),score.Rival.ToString(C),score.Server.ToString(),outcome.Winner?.ToString()??"",outcome.Shots.ToString(C),outcome.Steps.ToString(C),outcome.RealSeconds.ToString("R",C),Signature(outcome,sim),FatigueJson(outcome,"HumanFatigue"),FatigueJson(outcome,"RivalFatigue")}));
                            int k=0;
                            foreach(var r in outcome.Receptions)
                            {
                                object fr=typeof(ReceptionLog).GetField("FatigueReception")?.GetValue(r)??0d;
                                receptions.WriteLine(string.Join("\t",new[]{i.ToString(C),point.ToString(C),(k++).ToString(C),r.Who.ToString(),r.Result.ToString(),r.ContactStep.ToString(C),r.DeltaMs.ToString("R",C),Convert.ToString(fr,C)}.Concat(f2?new[]{r.FatigueStriker.ToString("R",C),r.Input.Speed.ToString("R",C)}:new string[0])));
                            }
                            points.Flush();receptions.Flush();
                            Assert.That(outcome.End,Is.EqualTo(RallyEnd.Point),"nonterminal point: preserve and stop");
                            Assert.That(outcome.SystemFaults,Is.Zero);Assert.That(outcome.RejectedContacts,Is.Zero);
                            Assert.That(outcome.Winner,Is.Not.Null);
                            if(outcome.Winner.HasValue)score.PointWon(outcome.Winner.Value);else replays++;
                            point++;
                        }
                        matches.WriteLine(i+"\t"+score.Human+"\t"+score.Rival+"\t"+score.Winner+"\t"+point+"\t"+replays);matches.Flush();
                    }
                    TestContext.WriteLine((f2?"F2 ":"F1 ")+mode+" match="+i);
                }
            }
        }
        private static string FatigueJson(RallyOutcome o,string field)
        {object value=typeof(RallyOutcome).GetField(field)?.GetValue(o);return value==null?"":JsonUtility.ToJson(value);}
        // Fixed list: additions to RallyOutcome/ReceptionLog cannot weaken the historical comparison.
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
