using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class CausalReferenceCoverageTests
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static double D(string s)=>double.Parse(s,C);
        private static float F(string s)=>float.Parse(s,C);
        private static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,"../docs/validation/evidence/2026-11-13-causal-zone-proposal"));
        private static Dictionary<string,string>[] Rows(string file)
        {
            var lines=File.ReadAllLines(Path.Combine(Root,file));var headers=lines[0].Split('\t');
            return lines.Skip(1).Select(line=>
            {
                var values=line.Split('\t');Assert.That(values.Length,Is.EqualTo(headers.Length),file);
                return headers.Select((name,index)=>new KeyValuePair<string,string>(name,values[index])).ToDictionary(pair=>pair.Key,pair=>pair.Value,StringComparer.Ordinal);
            }).ToArray();
        }
        [Test,Timeout(180000)] public void ZoneCovers_AllReachablePerfect_AcrossTemposAndTrajectories_AndMeasuresEarlySnap()
        {
            // Frozen regression inputs from CQ-ZONE-v1, not a new rally/alternative experiment.
            var p=ConfigFixtures.Default();var cfg=EffectiveConfig.From(p);int contexts=0,contacts=0,snaps=0;
            var geometries=Rows("geometry.tsv").GroupBy(r=>r["id"]).ToDictionary(g=>g.Key,g=>g.ToArray());
            var events=Rows("events.tsv");var cells=Rows("cells.tsv");
            foreach(var zone in Rows("zones.tsv").Where(r=>r["form"]=="trajectory"))
            {
                string id=zone["id"];double tempo=D(zone["tempo"]),comp=D(zone["compMs"]);var geometry=geometries[id];var flight=new Trajectory();
                var frames=new BodyFrame[geometry.Length];long start=long.Parse(geometry[0]["step"],C),star=long.Parse(zone["tStar"],C);
                for(int i=0;i<geometry.Length;i++)
                {
                    var g=geometry[i];flight.States.Add(new BallState{Step=long.Parse(g["step"],C),Position=new Vector3(F(g["ballX"]),F(g["ballY"]),F(g["ballZ"])),Velocity=new Vector3(F(g["vx"]),F(g["vy"]),F(g["vz"]))});
                    frames[i]=BodyFrame.From(p,new Vector2(F(g["bodyX"]),F(g["bodyZ"])));
                }
                foreach(var e in events.Where(r=>r["id"]==id))
                {
                    var surface=(SurfaceId)Enum.Parse(typeof(SurfaceId),e["surface"]);var normal=CourtDimensions.InwardNormal(surface);
                    flight.Contacts.Add(new ContactEvent(surface,new Vector3(F(e["x"]),F(e["y"]),F(e["z"])),Vector2.zero,normal,long.Parse(e["step"],C),F(e["timeInStep"]),ContactTiming.Exact,Vector3.zero,bool.Parse(e["persistent"])));
                }
                var body=frames[(int)(star-start)];var legal=new LegalSettings((float)p.Get("legal.minHeight"),(float)p.Get("legal.maxHeight"),cfg.Physics.StepHz,cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court);
                var plan=new ContactPlan(true,SimTime.FromSteps(D(zone["kIn"])),SimTime.FromSteps(D(zone["kOut"])),star,flight.States[(int)(star-start)].Position,body.SideFor(flight.States[(int)(star-start)].Position),1,body,legal,cfg.Court);
                var clock=new ClockMap();clock.Begin(1,1,start,tempo,1,cfg.Physics.StepHz);var timing=new TimingSettings(40,true,100,false,comp);
                var reference=CausalQuality.Prepare(flight,plan,clock,timing,0,tempo,j=>frames[(int)(j-start)]);
                var expected=cells.Where(r=>r["id"]==id&&D(r["tempo"])==tempo&&D(r["compMs"])==comp&&r["result"]=="Perfect").Select(r=>long.Parse(r["contactStep"],C)).Distinct().OrderBy(j=>j).ToArray();
                Assert.That(reference.PerfectSteps,Is.EqualTo(expected),id+"/"+tempo+"/"+comp);
                Assert.That(reference.ArcRadius,Is.EqualTo(D(zone["LarcM"])).Within(1e-9),id);
                foreach(long j in reference.PerfectSteps)
                {
                    Assert.That(reference.Distance(flight.States[(int)(j-start)].Position,reference.BodyAt(j)),Is.Zero,id+"/"+j);contacts++;
                }
                {
                    Assert.That(clock.TrySimToReal(star,0,out double target),Is.True);var plans=new PresentedPlans();plans.Present(0,plan,target);
                    var before=ContactContract.Decide(target-.041+comp/1000,plans,clock,flight,timing,false);
                    var after=ContactContract.Decide(target-.040+comp/1000,plans,clock,flight,timing,false);
                    string Measure(ContactDecision decision)
                    {
                        if(decision.Result!=ShotResult.Perfect&&decision.Result!=ShotResult.Valid)return decision.Result+"/"+decision.Fail+"/no-vector";
                        var point=flight.States[(int)(decision.ContactStep-start)].Position;
                        var vector=CausalQuality.Evaluate(reference,reference.BodyAt(decision.ContactStep),point,CausalSettings.From(p)).First(v=>v.Cause=="timing");
                        return decision.Result+"/step="+decision.ContactStep+"/d="+reference.Distance(point,reference.BodyAt(decision.ContactStep)).ToString("R",C)+"/height="+vector.DeltaHeight.ToString("R",C)+"/speed="+vector.DeltaSpeedFraction.ToString("R",C);
                    }
                    if(before.ContactStep>=0&&after.ContactStep>=0&&before.ContactStep!=after.ContactStep)snaps++;
                    TestContext.WriteLine("B4_SNAP "+id+" tempo="+tempo.ToString("R",C)+" comp="+comp.ToString("R",C)+" minus41="+Measure(before)+" minus40="+Measure(after));
                }
                contexts++;
            }
            Assert.That(contexts,Is.EqualTo(270));Assert.That(contacts,Is.GreaterThan(270));Assert.That(snaps,Is.GreaterThan(0));
            TestContext.WriteLine("B4_COVERAGE contexts="+contexts+" perfectContacts="+contacts+" snapPairs="+snaps);
        }
        [Test] public void Hashes_AreMeasuredInUnity_FromDefaultsAndHistoricalP0Overlay()
        {
            var p=ConfigFixtures.Default();var p0=p.With("launcher.repeatSeconds",3);var sl5=p.With("practice.launch",4).With("human.z",(double)1.6f);
            string path=Environment.GetEnvironmentVariable("B4_P0_PATH");
            if(!string.IsNullOrEmpty(path))
            {
                byte[] before=File.ReadAllBytes(path);Assert.That(PresetCodec.TryParse(System.Text.Encoding.UTF8.GetString(before),out var file,out var error),Is.True,error);
                var warnings=new List<string>();var actual=PresetCodec.Overlay(p,file,warnings);Assert.That(warnings,Is.Empty);Assert.That(actual.Hash,Is.EqualTo(p0.Hash));Assert.That(File.ReadAllBytes(path),Is.EqualTo(before));
            }
            TestContext.WriteLine("B4_HASH defaults="+p.Hash+" P0="+p0.Hash+" SL5="+sl5.Hash+" keys="+p.Ordered.Count+" inherited="+p.HashOf(ConfigFixtures.FirstBallPrefixes)+" P0inherited="+p0.HashOf(ConfigFixtures.FirstBallPrefixes)+" simulation="+EffectiveConfig.From(p).SimulationKey);
            Assert.That(p.Ordered.Count,Is.EqualTo(121));Assert.That(p.HashOf(ConfigFixtures.FirstBallPrefixes),Is.EqualTo("0d39e6016bd8"));Assert.That(p0.HashOf(ConfigFixtures.FirstBallPrefixes),Is.EqualTo("8de3f2ecb217"));Assert.That(EffectiveConfig.From(p).SimulationKey,Is.EqualTo("166a96e319c7"));
        }
    }
}
