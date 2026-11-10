using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    public sealed class Sl56PostFrontisTrajectoryReport
    {
        private const float BounceThreshold=.1f;
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private sealed class Flight
        {
            public readonly Trajectory Trajectory=new Trajectory();
            public string Stop="horizon-10s";
            public int? EscapeStep;
            public Vector3 EscapePosition,EscapeVelocity;
        }
        private sealed class Case
        {
            public string Id,Status="Unsolved",Reason="",After="notObserved",Surface="",Stop="";
            public Vector3 Incoming,PStar,Velocity;
            public long TStar;
            public float? FrontHeight,TargetX,TargetHeight,HeightError,TargetError,BounceHeight,BounceX,BounceZ,LateralDistance;
            public bool PointOut,FootprintOut,TouchesOutBand,Escape,FloorOutside;
            public int? EscapeStep;
            public Vector3 EscapePosition,EscapeVelocity;
            public readonly List<string> Contacts=new List<string>();
            public readonly List<string> OutContacts=new List<string>();
            public bool GeometricOut=>FootprintOut||Escape;
        }
        [Test, Explicit, Category("Experiments")]
        public void WriteSl56PostFrontisTrajectoryReport()
        {
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            if(preset.Hash!="c0d8ec7050e0")throw new InvalidDataException("Unexpected DefaultPreset hash: "+preset.Hash);
            var cfg=EffectiveConfig.From(preset);var physics=cfg.Physics;var court=cfg.Court;
            var factory=new AnalyticBallSimulationFactory(BounceThreshold);var cases=new List<Case>();
            foreach(var recipe in PracticeLaunchDefinitions.Create(cfg).Where(r=>r.Id=="SL5"||r.Id=="SL6").OrderBy(r=>r.Id))
            {
                var row=new Case{Id=recipe.Id,Incoming=recipe.Initial.Position};cases.Add(row);
                Trajectory incoming;
                using(var sim=factory.Create(physics,court))incoming=TrajectoryRunner.Run(sim,recipe.Initial,physics.StepHz*10,StopCondition.SecondFloorBounce,court);
                var legal=new LegalSettings(.05f,2,physics.StepHz,physics.Gravity,physics.BallRadius);
                var plan=ContactPlanner.Plan(incoming,recipe.Body,court,legal,1);
                row.TStar=plan.TStarStep;row.PStar=plan.PStar;
                if(!plan.Exists||plan.TStarStep<0||!Finite(plan.PStar)){row.Reason=plan.Reason??"no-usable-contact";continue;}
                var solution=ShotSolver.Solve(new ShotRequest(plan.PStar,new Vector2(3.2f,2.2f),.5f),
                    (float)preset.Get("shot.vMin"),(float)preset.Get("shot.vMax"),(float)preset.Get("shot.vCap"),
                    physics.Gravity,physics.BallRadius,court,factory,physics);
                row.Reason=solution.Reason;row.Velocity=solution.Velocity;
                if(!solution.Solved)continue;
                row.Status="Solved";row.TargetX=solution.TargetUsed.x;row.TargetHeight=solution.TargetUsed.y;
                Flight flight;
                using(var sim=factory.Create(physics,court))flight=RunUntilSecondFloor(sim,new BallState{Position=plan.PStar,Velocity=solution.Velocity},physics.StepHz*10,court);
                row.Stop=flight.Stop;row.EscapeStep=flight.EscapeStep;row.EscapePosition=flight.EscapePosition;row.EscapeVelocity=flight.EscapeVelocity;
                Describe(row,flight.Trajectory,court,solution.TargetUsed);
            }
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));var git=GitShort(root);
            var utc=DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'",C);
            var directory=Path.Combine(root,"Logs/experiments/sl56-post-frontis",utc+"-"+git);Directory.CreateDirectory(directory);
            var tsv=new StringBuilder("id\tstatus\treason\tstopReason\tincomingX\tincomingY\tincomingZ\tcontactStep\tcontactX\tcontactY\tcontactZ\tvelocityX\tvelocityY\tvelocityZ\tfirstAfterFrontisCategory\tfirstAfterFrontisSurface\tfrontHeight\ttargetUsedX\ttargetUsedHeight\tfrontHeightError\tfrontTargetDistance\tfirstBounceHeight\tfirstBounceX\tfirstBounceZ\tnearestLateralDistance\tfirstBounceOutsideCourt\tpointOut\tfootprintOut\ttouchesOutBand\tgeometryEnvelopeEscape\tescapeStep\tescapeX\tescapeY\tescapeZ\tgeometricOut\n");
            foreach(var r in cases)tsv.Append(string.Join("\t",new[]{r.Id,r.Status,Clean(r.Reason),r.Stop,R(r.Incoming.x),R(r.Incoming.y),R(r.Incoming.z),r.TStar.ToString(C),R(r.PStar.x),R(r.PStar.y),R(r.PStar.z),R(r.Velocity.x),R(r.Velocity.y),R(r.Velocity.z),r.After,r.Surface,N(r.FrontHeight),N(r.TargetX),N(r.TargetHeight),N(r.HeightError),N(r.TargetError),N(r.BounceHeight),N(r.BounceX),N(r.BounceZ),N(r.LateralDistance),B(r.FloorOutside),B(r.PointOut),B(r.FootprintOut),B(r.TouchesOutBand),B(r.Escape),r.EscapeStep.HasValue?r.EscapeStep.Value.ToString(C):"",r.EscapeStep.HasValue?R(r.EscapePosition.x):"",r.EscapeStep.HasValue?R(r.EscapePosition.y):"",r.EscapeStep.HasValue?R(r.EscapePosition.z):"",B(r.GeometricOut)})).Append('\n');
            File.WriteAllText(Path.Combine(directory,"cases.tsv"),tsv.ToString(),new UTF8Encoding(false));
            var groups=new[]{"SL5","SL6","total"}.Select(id=>Group(id,cases.Where(r=>id=="total"||r.Id==id).ToList()));
            var json="{\n\"utc\":"+Q(utc)+",\"gitShort\":"+Q(git)+",\"presetHash\":"+Q(preset.Hash)+",\"SimulationKey\":"+Q(cfg.SimulationKey)+",\"unityVersion\":"+Q(Application.unityVersion)+
                ",\"bounceThreshold\":"+R(BounceThreshold)+",\"horizonSeconds\":10,\"outgoingStop\":\"second non-persistent floor bounce or 1200 steps; envelope escape is recorded but does not stop the flight\",\"power\":0.5,\"aim\":[3.2,2.2],\"footprintRadius\":0.01,\"nPerRecipe\":1,"+
                "\"units\":\"metres; percentages 0..100; height error = observed minus TargetUsed.y\",\"denominator\":\"solved cases, including notObserved; unsolved excluded\",\n\"groups\":["+string.Join(",",groups)+"],\n\"cases\":["+
                string.Join(",",cases.Select(CaseJson))+ "]\n}\n";
            File.WriteAllText(Path.Combine(directory,"summary.json"),json,new UTF8Encoding(false));
            Assert.That(Directory.Exists(directory)&&File.Exists(Path.Combine(directory,"summary.json"))&&File.Exists(Path.Combine(directory,"cases.tsv")),Is.True);
            var rows=File.ReadAllLines(Path.Combine(directory,"cases.tsv"));
            Assert.That(rows.Any(r=>r.StartsWith("SL5\t",StringComparison.Ordinal)),Is.True);
            Assert.That(rows.Any(r=>r.StartsWith("SL6\t",StringComparison.Ordinal)),Is.True);
            TestContext.WriteLine(directory);
        }
        // Same horizon as docs/diagnostics/2026-11-04-sl56-p2-reproduction.md: do not abort at y>WallHeight; the floor is unbounded.
        private static Flight RunUntilSecondFloor(IBallSimulation sim,in BallState from,int maxSteps,CourtDimensions court)
        {
            var flight=new Flight();var hits=new List<ContactEvent>(4);int floors=0;
            sim.Reset(from);flight.Trajectory.States.Add(sim.State);
            for(int i=0;i<maxSteps;i++)
            {
                hits.Clear();sim.Step(hits);var state=sim.State;
                flight.Trajectory.States.Add(state);flight.Trajectory.Contacts.AddRange(hits);
                foreach(var hit in hits)if(hit.Surface==SurfaceId.Floor&&!hit.Persistent)floors++;
                if(!flight.EscapeStep.HasValue&&(state.Position.y>court.WallHeight||state.Position.x<0||state.Position.x>court.Width||state.Position.z<0||state.Position.z>court.Length))
                {flight.EscapeStep=checked((int)state.Step);flight.EscapePosition=state.Position;flight.EscapeVelocity=state.Velocity;}
                if(floors>=2){flight.Stop="second-floor-step-complete";break;}
            }
            return flight;
        }
        private static void Describe(Case r,Trajectory t,CourtDimensions court,Vector2 target)
        {
            foreach(var h in t.Contacts)
                r.Contacts.Add("{\"step\":"+h.Step.ToString(C)+",\"timeInStep\":"+R(h.TimeInStep)+",\"surface\":"+Q(h.Surface.ToString())+",\"point\":["+R(h.Point.x)+","+R(h.Point.y)+","+R(h.Point.z)+"],\"persistent\":"+B(h.Persistent)+",\"approach\":["+R(h.ApproachVelocity.x)+","+R(h.ApproachVelocity.y)+","+R(h.ApproachVelocity.z)+"]}");
            int front=t.Contacts.FindIndex(h=>h.Surface==SurfaceId.FrontWall&&!h.Persistent);
            if(front>=0)
            {
                var h=t.Contacts[front];r.FrontHeight=h.Point.y;r.HeightError=h.Point.y-target.y;
                r.TargetError=Vector2.Distance(new Vector2(h.Point.x,h.Point.y),target);
                for(int i=front+1;i<t.Contacts.Count;i++)
                {
                    h=t.Contacts[i];if(h.Persistent)continue;
                    r.Surface=h.Surface.ToString();r.After=h.Surface==SurfaceId.LeftWall||h.Surface==SurfaceId.RightWall?"lateral":h.Surface==SurfaceId.Floor?"floor":h.Surface==SurfaceId.BackWall?"backWall":"other";break;
                }
            }
            int floor=t.Contacts.FindIndex(h=>h.Surface==SurfaceId.Floor&&!h.Persistent);
            if(floor>=0)
            {
                var p=t.Contacts[floor].Point;r.BounceHeight=p.y;r.BounceX=p.x;r.BounceZ=p.z;r.LateralDistance=Mathf.Min(p.x,court.Width-p.x);
                r.FloorOutside=p.x<0||p.x>court.Width||p.z<0||p.z>court.Length;
            }
            var bands=CourtLines.Build(court);
            foreach(var h in t.Contacts)foreach(var band in bands)if(band.Surface==h.Surface&&IsOut(band.Id))
            {
                float distance=Vector2.Dot(h.UV-band.A,band.Growth);var edge=band.B-band.A;
                float along=Vector2.Dot(h.UV-band.A,edge/edge.magnitude);
                bool point=distance>=0&&along>=0&&along<=edge.magnitude;
                bool touches=LineContactClassifier.Touches(in band,h.UV,.01f),footprint=point||touches;
                r.PointOut|=point;r.FootprintOut|=footprint;r.TouchesOutBand|=touches;
                r.OutContacts.Add("{\"surface\":"+Q(h.Surface.ToString())+",\"step\":"+h.Step.ToString(C)+",\"timeInStep\":"+R(h.TimeInStep)+",\"persistent\":"+B(h.Persistent)+",\"outDistance\":"+R(distance)+",\"pointOut\":"+B(point)+",\"footprintOut\":"+B(footprint)+",\"touchesOutBand\":"+B(touches)+"}");
            }
            r.Escape=r.EscapeStep.HasValue||t.States.Any(s=>s.Position.y>court.WallHeight||s.Position.x<0||s.Position.x>court.Width||s.Position.z<0||s.Position.z>court.Length);
        }
        private static string Group(string id,List<Case> rows)
        {
            var solved=rows.Where(r=>r.Status=="Solved").ToList();
            string Percent(int count)=>solved.Count==0?"null":(100.0*count/solved.Count).ToString("R",C);
            string Mean(Func<Case,float?> value){var v=solved.Select(value).Where(x=>x.HasValue).Select(x=>(double)x.Value).ToArray();return v.Length==0?"null":v.Average().ToString("R",C);}
            return "{\"id\":"+Q(id)+",\"n\":"+rows.Count.ToString(C)+",\"solved\":"+solved.Count.ToString(C)+",\"unsolved\":"+(rows.Count-solved.Count).ToString(C)+
                ",\"firstAfterFrontisPercent\":{"+string.Join(",",new[]{"lateral","floor","backWall","notObserved","other"}.Select(k=>Q(k)+":"+Percent(solved.Count(r=>r.After==k))))+"},"+
                "\"meanFrontHeight\":"+Mean(r=>r.FrontHeight)+",\"meanTargetUsedHeight\":"+Mean(r=>r.TargetHeight)+",\"meanFrontHeightError\":"+Mean(r=>r.HeightError)+",\"meanFrontTargetDistance\":"+Mean(r=>r.TargetError)+
                ",\"meanFirstBounceHeight\":"+Mean(r=>r.BounceHeight)+",\"meanFirstBounceX\":"+Mean(r=>r.BounceX)+",\"meanFirstBounceZ\":"+Mean(r=>r.BounceZ)+",\"meanNearestLateralDistance\":"+Mean(r=>r.LateralDistance)+
                ",\"firstBounceOutsideCourtCount\":"+solved.Count(r=>r.FloorOutside).ToString(C)+",\"geometricOutCount\":"+solved.Count(r=>r.GeometricOut).ToString(C)+",\"geometricOutPercent\":"+Percent(solved.Count(r=>r.GeometricOut))+
                ",\"envelopeEscapeCount\":"+solved.Count(r=>r.Escape).ToString(C)+"}";
        }
        private static string CaseJson(Case r)=>"{\"id\":"+Q(r.Id)+",\"status\":"+Q(r.Status)+",\"reason\":"+Q(r.Reason)+",\"stopReason\":"+Q(r.Stop)+
            ",\"incoming\":["+R(r.Incoming.x)+","+R(r.Incoming.y)+","+R(r.Incoming.z)+"],\"contactStep\":"+r.TStar.ToString(C)+
            ",\"contact\":["+R(r.PStar.x)+","+R(r.PStar.y)+","+R(r.PStar.z)+"],\"velocity\":["+R(r.Velocity.x)+","+R(r.Velocity.y)+","+R(r.Velocity.z)+"]"+
            ",\"escapeStep\":"+(r.EscapeStep.HasValue?r.EscapeStep.Value.ToString(C):"null")+
            ",\"escapePosition\":"+(r.EscapeStep.HasValue?"["+R(r.EscapePosition.x)+","+R(r.EscapePosition.y)+","+R(r.EscapePosition.z)+"]":"null")+
            ",\"escapeVelocity\":"+(r.EscapeStep.HasValue?"["+R(r.EscapeVelocity.x)+","+R(r.EscapeVelocity.y)+","+R(r.EscapeVelocity.z)+"]":"null")+
            ",\"contacts\":["+string.Join(",",r.Contacts)+"],\"outBandContacts\":["+string.Join(",",r.OutContacts)+"]}";
        private static bool IsOut(LineId id)=>id==LineId.FrontOut||id==LineId.BackOut||id==LineId.LeftSideOut||id==LineId.RightSideOut;
        private static bool Finite(Vector3 p)=>!float.IsNaN(p.x)&&!float.IsInfinity(p.x)&&!float.IsNaN(p.y)&&!float.IsInfinity(p.y)&&!float.IsNaN(p.z)&&!float.IsInfinity(p.z);
        private static string R(float x)=>x.ToString("R",C);
        private static string N(float? x)=>x.HasValue?R(x.Value):"";
        private static string B(bool x)=>x?"true":"false";
        private static string Clean(string x)=>(x??"").Replace('\t',' ').Replace('\r',' ').Replace('\n',' ');
        private static string Q(string x)=>"\""+(x??"").Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r","\\r").Replace("\n","\\n").Replace("\t","\\t")+"\"";
        private static string GitShort(string root)
        {
            using(var process=Process.Start(new ProcessStartInfo("git","rev-parse --short HEAD"){WorkingDirectory=root,UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true}))
            {
                string value=process.StandardOutput.ReadToEnd().Trim();string error=process.StandardError.ReadToEnd();process.WaitForExit();
                if(process.ExitCode!=0||value.Length==0||value.Any(c=>!Uri.IsHexDigit(c)))throw new IOException("Cannot obtain git short HEAD: "+error);
                return value;
            }
        }
    }
}
