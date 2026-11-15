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
            public string recipe,mode,result,fail,reason,firstSurface,causalReason,causalFirstSurface;
            public int seed,attempt,planVersion;
            public double offsetMs,releaseReal,targetReal,presentedReal,deltaMs,quantizationMs,kinSteps,koutSteps,geometryDistanceToPStar;
            public long contactStep;
            public bool contact,zoneObserved,zoneHit,speedRaised,targetLowered;
            public Vector2 aim,targetUsed;
            public float power;
            public Vector3 contactPosition,velocity,firstPoint;
            // Productive path: CausalQuality.Prepare once per recipe and mode, then CausalQuality.Solve (the function RallyContact.Solve calls).
            public bool causalSolved,causalZoneObserved,causalZoneHit,causalAimClamped,causalSpeedRaised,causalTargetLowered;
            public double deadZoneDistance;
            public float heightTotal,speedTotal,heightTiming,speedTiming,heightStretch,speedStretch,heightJam,speedJam;
            public Vector2 causalAim;public float causalSpeed;public Vector3 causalVelocity,causalFirstPoint;
            // Cue visibility at the instant of release, derived arithmetically from the view rules; not a bot behaviour.
            public bool cueVisibleComplete,cueVisiblePartial,cueVisibleNone;
        }
        [Serializable] private sealed class Data
        {
            public string source,defaultsHash,simulationKey,p0Hash,aidNote="Bots read the visible plan and do not perceive the view: contract, solver and causal outcomes are identical across aid levels by construction. Only cue visibility at release is derived.";
            public bool gitDirty;
            public int pairedContactsCompared,pairedGeometryDifferences;
            public List<Trial> trials=new List<Trial>();
        }
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        [Test,Explicit,Category("Experiments"),Timeout(600000)]
        public void WriteH3bReport_CausalAndAidLevels()
        {
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            var p=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            var cfg=EffectiveConfig.From(p);
            if(p.Hash!="06fed4bd4ac9"||p.Ordered.Count!=121||cfg.SimulationKey!="166a96e319c7"||p.With("launcher.repeatSeconds",3).Hash!="0a3d3262a79d"||
                p.Get("timing.latencyCompMs")!=0||p.Get("power.pullFullMm")!=35||cfg.Backend!="P2-Analytic")throw new InvalidDataException("Protected configuration changed.");
            var data=new Data{source=Git(root,"rev-parse HEAD"),gitDirty=Git(root,"status --porcelain").Length>0,defaultsHash=p.Hash,simulationKey=cfg.SimulationKey,p0Hash=p.With("launcher.repeatSeconds",3).Hash};
            var directory=Path.Combine(root,"Logs/experiments/h3b-causal",DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'",C)+"-"+data.source.Substring(0,7));Directory.CreateDirectory(directory);
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
                    var references=new Dictionary<bool,CausalReference>();
                    foreach(bool mode in new[]{false,true})references[mode]=CausalQuality.Prepare(incoming,plan,clock,new TimingSettings(40,mode,100,false,0),0,cfg.Tempo);
                    for(int seed=0;seed<8;seed++)foreach(double offset in new[]{0d,-60,-100,-150})foreach(bool v in new[]{false,true})
                    {
                        var timing=new TimingSettings(40,v,100,false,0);var bot=new AnticipationBot(seed,offset);
                        if(!bot.TrySchedule(plans,Math.Max(0,target-ImpactCue.ApproachMs/1000),timing,recipeIndex,out var release))throw new InvalidDataException("Bot could not schedule visible plan");
                        var intent=bot.Intent(recipeIndex,new Vector2(3.2f,2.2f),.2f);
                        var d=ContactContract.Decide(release.EventAtReal,plans,clock,incoming,timing,false);
                        var row=new Trial{recipe=recipe.Id,mode=v?"V":"T3",seed=seed,attempt=recipeIndex,offsetMs=offset,planVersion=d.PlanVersionUsed,
                            releaseReal=release.EventAtReal,targetReal=d.TargetContactAtReal,presentedReal=d.PlanPresentedAtReal,deltaMs=d.DeltaMs,quantizationMs=d.QuantizationErrorMsSigned,
                            kinSteps=plan.KIn.Steps,koutSteps=plan.KOut.Steps,result=d.Result.ToString(),fail=d.Fail.ToString(),reason=d.Reason,contactStep=d.ContactStep,aim=intent.aim,power=intent.power};
                        row.cueVisibleComplete=true;row.cueVisiblePartial=release.EventAtReal<release.TargetAtReal-.15;row.cueVisibleNone=false;
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
                            var legacy=FirstImpact(factory,cfg,state.Position,solution.Velocity);
                            row.zoneObserved=legacy.observed;row.firstSurface=legacy.surface;row.firstPoint=legacy.point;row.zoneHit=legacy.hit;
                            var reference=references[v];var frame=reference.BodyAt(d.ContactStep);
                            row.deadZoneDistance=reference.Distance(state.Position,frame);
                            var causal=CausalQuality.Solve(reference,frame,state.Position,intent.aim,intent.power,cfg,factory);
                            row.causalSolved=causal.Solution.Solved;row.causalReason=causal.Solution.Reason;
                            row.causalAimClamped=causal.Input.Clamped;row.causalAim=causal.Input.Aim;row.causalSpeed=causal.Input.Speed;
                            foreach(var vector in causal.Vectors)
                            {
                                row.heightTotal+=vector.DeltaHeight;row.speedTotal+=vector.DeltaSpeedFraction;
                                if(vector.Cause=="timing"){row.heightTiming+=vector.DeltaHeight;row.speedTiming+=vector.DeltaSpeedFraction;}
                                else if(vector.Cause=="stretch"){row.heightStretch+=vector.DeltaHeight;row.speedStretch+=vector.DeltaSpeedFraction;}
                                else if(vector.Cause=="jam"){row.heightJam+=vector.DeltaHeight;row.speedJam+=vector.DeltaSpeedFraction;}
                            }
                            if(causal.Solution.Solved)
                            {
                                row.causalVelocity=causal.Solution.Velocity;row.causalSpeedRaised=causal.Solution.SpeedRaised;row.causalTargetLowered=causal.Solution.TargetLowered;
                                var productive=FirstImpact(factory,cfg,state.Position,causal.Solution.Velocity);
                                row.causalZoneObserved=productive.observed;row.causalFirstSurface=productive.surface;row.causalFirstPoint=productive.point;row.causalZoneHit=productive.hit;
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
            // Execution completed does not mean H3b passed: the hypothesis is judged by the PO from these descriptive data.
        }
        private static (bool observed,string surface,Vector3 point,bool hit) FirstImpact(AnalyticBallSimulationFactory factory,EffectiveConfig cfg,Vector3 position,Vector3 velocity)
        {
            using(var sim=factory.Create(cfg.Physics,cfg.Court))
            {
                sim.Reset(new BallState{Position=position,Velocity=velocity});var events=new List<ContactEvent>();
                for(int step=0;step<cfg.Physics.StepHz*10;step++)
                {
                    events.Clear();sim.Step(events);
                    foreach(var e in events)
                    {
                        if(e.Persistent)continue;
                        bool hit=e.Surface==SurfaceId.FrontWall&&e.Point.x>=2.7f&&e.Point.x<=3.7f&&e.Point.y>=1.7f&&e.Point.y<=2.7f;
                        return (true,e.Surface.ToString(),e.Point,hit);
                    }
                    if(sim.State.Position.y>cfg.Court.WallHeight)break;
                }
            }
            return (false,null,default,false);
        }
        private sealed class Aggregate
        {
            public int Total,Contacts,Air,Late,Other,ZoneObserved,ZoneHits,CausalSolved,CausalUnsolved,CausalZoneObserved,CausalZoneHits,Clamped,SpeedRaised,TargetLowered,DeadZero,CueComplete,CuePartial,CueNone;
            public double H,S,Ht,St,Hs,Ss,Hj,Sj,D;
            public Aggregate(IEnumerable<Trial> rows)
            {
                foreach(var r in rows)
                {
                    Total++;if(r.cueVisibleComplete)CueComplete++;if(r.cueVisiblePartial)CuePartial++;if(r.cueVisibleNone)CueNone++;
                    if(r.contact)
                    {
                        Contacts++;if(r.zoneObserved){ZoneObserved++;if(r.zoneHit)ZoneHits++;}
                        if(!r.causalSolved){CausalUnsolved++;continue;}
                        CausalSolved++;H+=r.heightTotal;S+=r.speedTotal;Ht+=r.heightTiming;St+=r.speedTiming;Hs+=r.heightStretch;Ss+=r.speedStretch;Hj+=r.heightJam;Sj+=r.speedJam;D+=r.deadZoneDistance;
                        if(r.deadZoneDistance==0)DeadZero++;if(r.causalAimClamped)Clamped++;if(r.causalSpeedRaised)SpeedRaised++;if(r.causalTargetLowered)TargetLowered++;
                        if(r.causalZoneObserved){CausalZoneObserved++;if(r.causalZoneHit)CausalZoneHits++;}
                    }
                    else if(r.result=="Failed"&&r.fail=="Air")Air++;
                    else if(r.result=="Failed"&&r.fail=="Late")Late++;
                    else Other++;
                }
            }
            public string Mean(double sum)=>CausalSolved==0?"NotEvaluated":(sum/CausalSolved).ToString("R",C);
        }
        private static void WriteSummary(string directory,Data data)
        {
            var tsv=new StringBuilder("mode\toffsetMs\trecipe\ttotal\tcontacts\tair\tlate\tother\tlegacyZoneObserved\tlegacyZoneHits\tcausalSolved\tcausalUnsolved\tcausalZoneObserved\tcausalZoneHits\tmeanHeightTotal\tmeanSpeedFractionTotal\tmeanHeightTiming\tmeanSpeedTiming\tmeanHeightStretch\tmeanSpeedStretch\tmeanHeightJam\tmeanSpeedJam\tmeanDeadZoneDistance\tdeadZoneZero\taimClamped\tspeedRaised\ttargetLowered\n");
            var report=new StringBuilder("# H3b descriptivo — vector causal productivo y niveles de ayuda\n\nDatos offline por la ruta compartida (CausalQuality.Prepare una vez por receta y modo, CausalQuality.Solve) y P2. Sin protocolo humano ni calibración física. Sin veredicto Pass/Fail de H3b ni desigualdad exigida. Las medias causales son solo sobre contactos resueltos; denominador cero es NotEvaluated, no 0.\n\n");
            report.Append("Fuente: `"+data.source+"`; gitDirty="+data.gitDirty+"; defaults="+data.defaultsHash+"; SimulationKey="+data.simulationKey+"; P0="+data.p0Hash+".\n\n");
            report.Append("Zona del arnés H3b (no H2): x=[2,7;3,7], y=[1,7;2,7] m del frontis, primer impacto. 48 intentos por modo y desfase (6 recetas × 8 semillas). «Zona (sin causal)» es la ruta histórica del 11 nov; «Zona (causal)» es la ruta productiva.\n\n");
            report.Append("| Modo | Δ ms | Intentos | Contactos | Al aire | Tarde | Otros | Zona sin causal | Zona causal | Resueltos causal | ΔAltura media (m) | ΔVelocidad media (frac.) | d zona muerta media (m) | d=0 | Objetivo acotado |\n|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|\n");
            foreach(var group in data.trials.GroupBy(r=>(r.mode,r.offsetMs)).OrderBy(g=>g.Key.mode).ThenByDescending(g=>g.Key.offsetMs))
            {
                var a=new Aggregate(group);
                report.Append($"| {group.Key.mode} | {group.Key.offsetMs} | {a.Total} | {a.Contacts} | {a.Air} | {a.Late} | {a.Other} | {a.ZoneHits}/{a.ZoneObserved} | {a.CausalZoneHits}/{a.CausalZoneObserved} | {a.CausalSolved}/{a.Contacts} | {a.Mean(a.H)} | {a.Mean(a.S)} | {a.Mean(a.D)} | {a.DeadZero} | {a.Clamped} |\n");
                AddTsv(tsv,group.Key.mode,group.Key.offsetMs,"ALL",a);
                foreach(var recipe in group.GroupBy(r=>r.recipe))AddTsv(tsv,group.Key.mode,group.Key.offsetMs,recipe.Key,new Aggregate(recipe));
            }
            report.Append("\n## Niveles de ayuda (0 Completa, 1 Parcial, 2 Ninguna)\n\nLos bots no perciben la vista: contrato, solver y capa causal no leen el nivel de ayuda, así que los resultados de cada fila son **los mismos intentos** en los tres niveles (no son muestras independientes). Lo único que depende del nivel es la visibilidad del anillo en el instante de soltar, derivada de las reglas de la vista (parcial: oculto desde T*−0,15 s inclusive; ninguna: nunca). No es un efecto sobre el bot.\n\n| Ayuda | Modo | Δ ms | Intentos | Contactos | Al aire | Tarde | Anillo visible al soltar |\n|---|---|---:|---:|---:|---:|---:|---:|\n");
            foreach(int aid in new[]{0,1,2})
                foreach(var group in data.trials.GroupBy(r=>(r.mode,r.offsetMs)).OrderBy(g=>g.Key.mode).ThenByDescending(g=>g.Key.offsetMs))
                {
                    var a=new Aggregate(group);int visible=aid==0?a.CueComplete:aid==1?a.CuePartial:a.CueNone;
                    report.Append($"| {aid} | {group.Key.mode} | {group.Key.offsetMs} | {a.Total} | {a.Contacts} | {a.Air} | {a.Late} | {visible}/{a.Total} |\n");
                }
            report.Append("\nPares con contacto en ambos modos: "+data.pairedContactsCompared+"; diferencias de paso/geometría/salida legacy: "+data.pairedGeometryDifferences+". Vector y distancia son de contactos aplicados; ningún fallo aporta una muestra cero. Detalle por intento en trials.json.\n");
            File.WriteAllText(Path.Combine(directory,"summary.tsv"),tsv.ToString());File.WriteAllText(Path.Combine(directory,"report.md"),report.ToString());
        }
        private static void AddTsv(StringBuilder s,string mode,double offset,string recipe,Aggregate a)
        {s.Append(string.Join("\t",new[]{mode,offset.ToString("R",C),recipe,a.Total.ToString(C),a.Contacts.ToString(C),a.Air.ToString(C),a.Late.ToString(C),a.Other.ToString(C),a.ZoneObserved.ToString(C),a.ZoneHits.ToString(C),a.CausalSolved.ToString(C),a.CausalUnsolved.ToString(C),a.CausalZoneObserved.ToString(C),a.CausalZoneHits.ToString(C),a.Mean(a.H),a.Mean(a.S),a.Mean(a.Ht),a.Mean(a.St),a.Mean(a.Hs),a.Mean(a.Ss),a.Mean(a.Hj),a.Mean(a.Sj),a.Mean(a.D),a.DeadZero.ToString(C),a.Clamped.ToString(C),a.SpeedRaised.ToString(C),a.TargetLowered.ToString(C)})+"\n");}
        private static string Git(string root,string arguments)
        {using(var p=Process.Start(new ProcessStartInfo("git",arguments){WorkingDirectory=root,RedirectStandardOutput=true,UseShellExecute=false,CreateNoWindow=true})){string output=p.StandardOutput.ReadToEnd().Trim();p.WaitForExit();if(p.ExitCode!=0)throw new IOException("Git metadata failed");return output;}}
    }
}
