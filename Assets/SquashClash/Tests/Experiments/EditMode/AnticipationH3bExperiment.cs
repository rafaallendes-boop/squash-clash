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
    // Explicit descriptive data run, intentionally outside the regression assembly.
    public sealed class AnticipationH3bExperiment
    {
        [Serializable] private sealed class Trial
        {
            public string recipe,mode,result,fail,reason,firstSurface,causalStatus="NotEvaluated";
            public int seed,attempt,planVersion;
            public double offsetMs,releaseReal,targetReal,presentedReal,deltaMs,quantizationMs,kinSteps,koutSteps,geometryDistanceToPStar;
            public long contactStep;
            public bool contact,zoneObserved,zoneHit,speedRaised,targetLowered;
            public Vector2 aim,targetUsed;
            public float power;
            public Vector3 contactPosition,velocity,firstPoint;
        }
        [Serializable] private sealed class Data
        {
            public string source,defaultsHash,simulationKey,p0Hash,aid="current-44",causal="NotEvaluated",partialAid="NotEvaluated",noAid="NotEvaluated";
            public bool gitDirty;
            public int pairedContactsCompared,pairedGeometryDifferences;
            public List<Trial> trials=new List<Trial>();
        }
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        [Test,Explicit,Category("Experiments"),Timeout(600000)]
        public void WriteH3bReport_CurrentAid_T3AndV()
        {
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            var p=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            var cfg=EffectiveConfig.From(p);
            if(p.Hash!="c0e9f00176d5"||p.Ordered.Count!=108||cfg.SimulationKey!="166a96e319c7"||p.With("launcher.repeatSeconds",3).Hash!="42d9efbf50fc"||
                p.Get("timing.latencyCompMs")!=0||p.Get("power.pullFullMm")!=35||cfg.Backend!="P2-Analytic")throw new InvalidDataException("Protected configuration changed.");
            var data=new Data{source=Git(root,"rev-parse HEAD"),gitDirty=Git(root,"status --porcelain").Length>0,defaultsHash=p.Hash,simulationKey=cfg.SimulationKey,p0Hash=p.With("launcher.repeatSeconds",3).Hash};
            var directory=Path.Combine(root,"Logs/experiments/h3b",DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'",C)+"-"+data.source.Substring(0,7));Directory.CreateDirectory(directory);
            var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            int recipeIndex=0;
            try
            {
                foreach(var recipe in PracticeLaunchDefinitions.Create(cfg))
                {
                    Trajectory incoming;
                    using(var sim=factory.Create(cfg.Physics,cfg.Court))incoming=TrajectoryRunner.Run(sim,recipe.Initial,cfg.Physics.StepHz*10,StopCondition.SecondFloorBounce,cfg.Court);
                    var legal=new LegalSettings((float)p.Get("legal.minHeight"),(float)p.Get("legal.maxHeight"),cfg.Physics.StepHz,cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court);
                    var plan=ContactPlanner.Plan(incoming,recipe.Body,cfg.Court,legal,1);
                    if(!plan.Exists||plan.TStarStep<0)throw new InvalidDataException("No usable plan for "+recipe.Id);
                    var clock=new ClockMap();clock.Begin(1,0,0,cfg.Tempo,1,cfg.Physics.StepHz);
                    if(!clock.TrySimToReal(plan.TStarStep,0,out double target))throw new InvalidDataException("Unmapped T*");
                    var plans=new PresentedPlans();plans.Present(0,plan,target);
                    for(int seed=0;seed<8;seed++)foreach(double offset in new[]{0d,-60,-100,-150})foreach(bool v in new[]{false,true})
                    {
                        var timing=new TimingSettings(40,v,100,false,0);var bot=new AnticipationBot(seed,offset);
                        if(!bot.TrySchedule(plans,Math.Max(0,target-ImpactCue.ApproachMs/1000),timing,recipeIndex,out var release))throw new InvalidDataException("Bot could not schedule visible plan");
                        var intent=bot.Intent(recipeIndex,new Vector2(3.2f,2.2f),.2f);
                        var d=ContactContract.Decide(release.EventAtReal,plans,clock,incoming,timing,false);
                        var row=new Trial{recipe=recipe.Id,mode=v?"V":"T3",seed=seed,attempt=recipeIndex,offsetMs=offset,planVersion=d.PlanVersionUsed,
                            releaseReal=release.EventAtReal,targetReal=d.TargetContactAtReal,presentedReal=d.PlanPresentedAtReal,deltaMs=d.DeltaMs,quantizationMs=d.QuantizationErrorMsSigned,
                            kinSteps=plan.KIn.Steps,koutSteps=plan.KOut.Steps,result=d.Result.ToString(),fail=d.Fail.ToString(),reason=d.Reason,contactStep=d.ContactStep,aim=intent.aim,power=intent.power};
                        data.trials.Add(row);
                        if(d.Result==ShotResult.Perfect||d.Result==ShotResult.Valid)
                        {
                            var state=incoming.States[checked((int)(d.ContactStep-incoming.States[0].Step))];row.contactPosition=state.Position;
                            row.geometryDistanceToPStar=Vector3.Distance(state.Position,plan.PStar);
                            var solution=ShotSolver.Solve(new ShotRequest(state.Position,intent.aim,intent.power),(float)p.Get("shot.vMin"),(float)p.Get("shot.vMax"),(float)p.Get("shot.vCap"),
                                cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court,factory,cfg.Physics);
                            row.velocity=solution.Velocity;row.targetUsed=solution.TargetUsed;row.speedRaised=solution.SpeedRaised;row.targetLowered=solution.TargetLowered;
                            if(!solution.Solved){row.result="SystemFault";row.reason=solution.Reason;throw new InvalidDataException("Unsolved original solver: "+recipe.Id+" "+solution.Reason);}
                            row.contact=true;
                            using(var sim=factory.Create(cfg.Physics,cfg.Court))
                            {
                                sim.Reset(new BallState{Position=state.Position,Velocity=solution.Velocity});var events=new List<ContactEvent>();
                                for(int step=0;step<cfg.Physics.StepHz*10&&!row.zoneObserved;step++)
                                {
                                    events.Clear();sim.Step(events);
                                    foreach(var e in events)
                                    {
                                        if(e.Persistent)continue;
                                        row.zoneObserved=true;row.firstSurface=e.Surface.ToString();row.firstPoint=e.Point;
                                        row.zoneHit=e.Surface==SurfaceId.FrontWall&&e.Point.x>=2.7f&&e.Point.x<=3.7f&&e.Point.y>=1.7f&&e.Point.y<=2.7f;break;
                                    }
                                    if(sim.State.Position.y>cfg.Court.WallHeight)break;
                                }
                            }
                        }
                        else if(d.Result!=ShotResult.Failed)throw new InvalidDataException("Unexpected contact category: "+d.Result+" "+d.Reason);
                    }
                    recipeIndex++;
                }
                if(data.trials.Count!=384||factory.ImpactCapReachedCount!=0||factory.LiveInstances!=0)throw new InvalidDataException("Incomplete experiment or analytic resource/cap issue");
                foreach(var pair in data.trials.GroupBy(r=>(r.recipe,r.seed,r.offsetMs)))
                {
                    var a=pair.Single(r=>r.mode=="T3");var b=pair.Single(r=>r.mode=="V");
                    if(!a.contact||!b.contact)continue;
                    data.pairedContactsCompared++;
                    if(a.contactStep!=b.contactStep||a.contactPosition!=b.contactPosition||a.velocity!=b.velocity||a.targetUsed!=b.targetUsed)data.pairedGeometryDifferences++;
                }
                if(data.pairedGeometryDifferences!=0)throw new InvalidDataException("V changed geometry in shared accepted pairs");
            }
            finally
            {
                File.WriteAllText(Path.Combine(directory,"trials.json"),JsonUtility.ToJson(data,true));
                WriteSummary(directory,data);
                TestContext.WriteLine("H3B_DATA="+directory);
            }
            // Execution completed does not mean H3b passed: causal quality and other aid levels are NotEvaluated.
        }
        private static void WriteSummary(string directory,Data data)
        {
            var tsv=new StringBuilder("mode\toffsetMs\trecipe\ttotal\tcontacts\tair\tlate\tother\tzoneObserved\tzoneHits\tzoneRateObserved\tzoneRateAllAttempts\tcausalStatus\tcausalMean\n");
            var report=new StringBuilder("# H3b descriptivo — ayuda actual #44\n\nDatos offline del contrato, solver y P2. Sin protocolo humano ni calibración física. La geometría del contacto no se etiqueta como vector causal. Capa causal ausente: NotEvaluated; niveles parcial/ninguno: NotEvaluated. No veredicto Pass/Fail de H3b.\n\n");
            report.Append("Fuente: `"+data.source+"`; gitDirty="+data.gitDirty+"; defaults="+data.defaultsHash+"; SimulationKey="+data.simulationKey+"; P0="+data.p0Hash+".\n\n");
            report.Append("Zona de fixture H3b (no H2 aprobado): x=[2,7;3,7], y=[1,7;2,7] m del frontis; primer impacto. Cada modo/desfase: 48 intentos (6 recetas × 8 semillas).\n\n");
            report.Append("| Modo | Δ fijo ms | Intentos | Contactos | Al aire | Tarde | Otros | Zona/aciertos observados | Aciertos/intentos | Causal |\n|---|---:|---:|---:|---:|---:|---:|---:|---:|---|\n");
            foreach(var group in data.trials.GroupBy(r=>(r.mode,r.offsetMs)).OrderBy(g=>g.Key.mode).ThenByDescending(g=>g.Key.offsetMs))
            {
                var aggregate=Reduce(group);
                report.Append($"| {group.Key.mode} | {group.Key.offsetMs} | {aggregate.Total} | {aggregate.Contacts} | {aggregate.Air} | {aggregate.Late} | {aggregate.Other} | {aggregate.ZoneHits}/{aggregate.ZoneObserved} | {aggregate.ZoneHits}/{aggregate.Total} | NotEvaluated |\n");
                AddTsv(tsv,group.Key.mode,group.Key.offsetMs,"ALL",aggregate);
                foreach(var recipe in group.GroupBy(r=>r.recipe))AddTsv(tsv,group.Key.mode,group.Key.offsetMs,recipe.Key,Reduce(recipe));
            }
            report.Append("\nPares con contacto en ambos modos: "+data.pairedContactsCompared+"; diferencias de paso/geometría/salida: "+data.pairedGeometryDifferences+". V solo filtra por la ventana temporal. Las categorías al aire/tarde y el acierto de zona se informan sin combinarse en una puntuación. Ajustes de factibilidad y geometría por intento están en trials.json; causalMean vacío en TSV significa NotEvaluated, no cero.\n");
            File.WriteAllText(Path.Combine(directory,"summary.tsv"),tsv.ToString());File.WriteAllText(Path.Combine(directory,"report.md"),report.ToString());
        }
        private static AnticipationReport Reduce(IEnumerable<Trial> rows)
        {
            var all=rows.ToArray();var r=new AnticipationReport(all[0].mode=="V");
            foreach(var row in all)r.Add(row.mode=="V",(ShotResult)Enum.Parse(typeof(ShotResult),row.result),(FailKind)Enum.Parse(typeof(FailKind),row.fail),row.contact,
                row.contact&&row.zoneObserved?(bool?)row.zoneHit:null,null);return r;
        }
        private static void AddTsv(StringBuilder s,string mode,double offset,string recipe,AnticipationReport r)
        {s.Append(string.Join("\t",new[]{mode,offset.ToString("R",C),recipe,r.Total.ToString(C),r.Contacts.ToString(C),r.Air.ToString(C),r.Late.ToString(C),r.Other.ToString(C),r.ZoneObserved.ToString(C),r.ZoneHits.ToString(C),r.ZoneRateAmongObserved?.ToString("R",C)??"",r.Total>0?(r.ZoneHits/(double)r.Total).ToString("R",C):"","NotEvaluated",""})+"\n");}
        private static string Git(string root,string arguments)
        {using(var p=Process.Start(new ProcessStartInfo("git",arguments){WorkingDirectory=root,RedirectStandardOutput=true,UseShellExecute=false,CreateNoWindow=true})){string output=p.StandardOutput.ReadToEnd().Trim();p.WaitForExit();if(p.ExitCode!=0)throw new IOException("Git metadata failed");return output;}}
    }
}
