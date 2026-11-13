using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Experiments.EditMode
{
    // CQ-ZONE-v1: proposal only. No production causal function and no regression/AutoQA registration.
    public sealed class CausalZoneProposalExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        private sealed class Sample
        {
            public string Id,Origin;public Trajectory Flight;public ContactPlan Plan;
            public Vector3[] Q;public Vector2[] Bodies;public double[] Arc;public bool[] Legal;
        }
        private sealed class Cell
        {
            public string Kind;public double Left,Right,Release;public ContactDecision Decision;
            public double DurationMs=>(Right-Left)*1000;
            public bool Contact=>Decision.Result==ShotResult.Perfect||Decision.Result==ShotResult.Valid;
        }
        private readonly struct Segment
        {
            public readonly Vector3 A,B;
            public Segment(Vector3 a,Vector3 b){A=a;B=b;}
        }
        private sealed class Condition
        {
            public Sample Sample;public double Tempo,Comp,Target,Rate,L,Radius;
            public List<Cell> Cells;public readonly SortedSet<long> Perfect=new SortedSet<long>();
            public readonly List<Segment> Zone=new List<Segment>();
        }
        private sealed class Capture : IRallyBrain
        {
            private readonly IRallyBrain inner;public Action<AiObservation,ReceptionPlan> Observe;
            public Capture(IRallyBrain brain){inner=brain;}
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation observation,in ReceptionPlan own)
            {Observe?.Invoke(observation,own);return inner.Decide(observation,own);}
        }
        private static string F(double value)=>value.ToString("R",C);
        private static void Row(StringBuilder b,params object[] columns)
            =>b.Append(string.Join("\t",columns.Select(v=>v is double d?F(d):v is float f?f.ToString("R",C):Convert.ToString(v,C)))).Append('\n');
        private static double Distance(Vector3 a,Vector3 b)
        {double x=(double)a.x-b.x,y=(double)a.y-b.y,z=(double)a.z-b.z;return Math.Sqrt(x*x+y*y+z*z);}
        private static double Distance(Vector3 p,Segment line)
        {
            double x=(double)line.B.x-line.A.x,y=(double)line.B.y-line.A.y,z=(double)line.B.z-line.A.z;
            double dx=(double)p.x-line.A.x,dy=(double)p.y-line.A.y,dz=(double)p.z-line.A.z;
            double norm=x*x+y*y+z*z,t=norm==0?0:Math.Max(0,Math.Min(1,(dx*x+dy*y+dz*z)/norm));
            dx-=t*x;dy-=t*y;dz-=t*z;return Math.Sqrt(dx*dx+dy*dy+dz*dz);
        }
        private static bool Contact(ContactDecision d)=>d.Result==ShotResult.Perfect||d.Result==ShotResult.Valid;
        private static int Index(Sample s,long step)=>checked((int)(step-s.Flight.States[0].Step));
        private static Sample Prepare(string id,string origin,Trajectory t,ContactPlan plan,Func<int,Vector2> body)
        {
            var s=new Sample{Id=id,Origin=origin,Flight=t,Plan=plan,Q=new Vector3[t.States.Count],Bodies=new Vector2[t.States.Count],Arc=new double[t.States.Count],Legal=new bool[t.States.Count]};
            for(int i=0;i<t.States.Count;i++)
            {
                var state=t.States[i];s.Bodies[i]=body(i);
                s.Q[i]=state.Position-new Vector3(s.Bodies[i].x,plan.Body.ReferenceHeight,s.Bodies[i].y);
                if(i>0)s.Arc[i]=s.Arc[i-1]+Distance(s.Q[i],s.Q[i-1]);
                s.Legal[i]=state.Step>=plan.KIn.Steps&&state.Step<=plan.KOut.Steps&&ContactPlanner.IsLegalStep(t,state.Step,plan.Body,plan.Legal);
            }
            double at=s.Arc[Index(s,plan.TStarStep)];for(int i=0;i<s.Arc.Length;i++)s.Arc[i]-=at;
            return s;
        }
        private static Condition Measure(Sample s,double tempo,double comp,int hz)
        {
            long start=s.Flight.States[0].Step;double rate=tempo*hz;
            var clock=new ClockMap();clock.Begin(1,1,start,tempo,1,hz);
            if(!clock.TrySimToReal(s.Plan.TStarStep,0,out double target))throw new InvalidDataException("Unmapped ideal");
            var plans=new PresentedPlans();plans.Present(0,s.Plan,target);
            var timing=new TimingSettings(40,true,100,false,comp);
            var cuts=new SortedSet<double>{target-.1,target+.1,target-.04,target+.04,target,1};
            double Real(double step)=>1+(step-start)/rate;
            cuts.Add(Real(s.Plan.KIn.Steps));cuts.Add(Real(s.Plan.KOut.Steps));
            for(long j=(long)Math.Floor(s.Plan.KIn.Steps)-2;j<=Math.Ceiling(s.Plan.KOut.Steps)+2;j++)
            {cuts.Add(Real(j));cuts.Add(Real(j+.5));}
            foreach(var e in s.Flight.Contacts)cuts.Add(Real(e.Step-1+e.TimeInStep*hz));
            double low=target-.1,high=target+.1;
            var times=cuts.Where(t=>t>=low&&t<=high).ToArray();
            var condition=new Condition{Sample=s,Tempo=tempo,Comp=comp,Target=target,Rate=rate,Cells=new List<Cell>()};
            void Add(string kind,double a,double b,double when)
            {
                var d=ContactContract.Decide(when+comp/1000,plans,clock,s.Flight,timing,false);
                var cell=new Cell{Kind=kind,Left=a-target,Right=b-target,Release=when-target,Decision=d};condition.Cells.Add(cell);
                if(d.Result==ShotResult.Perfect)condition.Perfect.Add(d.ContactStep);
            }
            for(int i=0;i<times.Length;i++)
            {Add("boundary",times[i],times[i],times[i]);if(i+1<times.Length)Add("open",times[i],times[i+1],times[i]+(times[i+1]-times[i])*.5);}
            foreach(double ms in new double[]{-100,-60,-41,-40,-39,0,39,40,41,60,100})Add("probe",target+ms/1000,target+ms/1000,target+ms/1000);
            if(condition.Perfect.Count==0)throw new InvalidDataException("No reachable Perfect: "+s.Id);
            int star=Index(s,s.Plan.TStarStep);
            foreach(long j in condition.Perfect)
            {int i=Index(s,j);condition.L=Math.Max(condition.L,Math.Abs(s.Arc[i]));condition.Radius=Math.Max(condition.Radius,Distance(s.Q[i],s.Q[star]));}
            // Closed geometric image of the symmetric arc interval; zero-length pieces retain isolated nodes.
            for(int i=0;i<s.Q.Length;i++)
            {
                if(s.Legal[i]&&Math.Abs(s.Arc[i])<=condition.L)condition.Zone.Add(new Segment(s.Q[i],s.Q[i]));
                if(i+1>=s.Q.Length||!s.Legal[i]||!s.Legal[i+1])continue;
                double a=Math.Max(-condition.L,s.Arc[i]),b=Math.Min(condition.L,s.Arc[i+1]);if(b<a)continue;
                double span=s.Arc[i+1]-s.Arc[i];
                Vector3 At(double v)=>span==0?s.Q[i]:Vector3.LerpUnclamped(s.Q[i],s.Q[i+1],(float)((v-s.Arc[i])/span));
                condition.Zone.Add(new Segment(At(a),At(b)));
            }
            foreach(long j in condition.Perfect)
                if(EffectDistance(condition,Index(s,j),"trajectory",0)!=0||EffectDistance(condition,Index(s,j),"adaptive-radius",0)!=0)
                    throw new InvalidDataException("Perfect image not covered");
            return condition;
        }
        private static double EffectDistance(Condition c,int index,string form,double fixedRadius)
        {
            var s=c.Sample;
            if(form=="trajectory")return c.Zone.Min(line=>Distance(s.Q[index],line));
            double radius=form=="adaptive-radius"?c.Radius:fixedRadius;
            return Math.Max(0,Distance(s.Q[index],s.Q[Index(s,s.Plan.TStarStep)])-radius);
        }
        [Test,Explicit,Category("Experiments"),Timeout(600000)]
        public void WriteProposalOnly_DeadZoneWidths()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            string run=Environment.GetEnvironmentVariable("CQ_PROPOSAL_RUN");
            if(run!="run1"&&run!="run2")throw new InvalidDataException("CQ_PROPOSAL_RUN must be run1 or run2");
            var p=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            var cfg=EffectiveConfig.From(p);string source=Git(root,"rev-parse HEAD");
            if(Git(root,"status --porcelain").Length!=0||p.Hash!="c0e9f00176d5"||p.Ordered.Count!=108||cfg.SimulationKey!="166a96e319c7"||cfg.Backend!="P2-Analytic")throw new InvalidDataException("Unclean/protected configuration");
            var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var context=new RallyContext(cfg,new RallyParams(),factory);var samples=new List<Sample>();
            var legal=context.Legal;
            foreach(var recipe in PracticeLaunchDefinitions.Create(cfg))
            {
                Trajectory t;using(var sim=factory.Create(cfg.Physics,cfg.Court))t=TrajectoryRunner.Run(sim,recipe.Initial,cfg.Physics.StepHz*10,StopCondition.SecondFloorBounce,cfg.Court);
                var plan=ContactPlanner.Plan(t,recipe.Body,cfg.Court,legal,1);
                if(!plan.Exists||plan.TStarStep<0)throw new InvalidDataException("No SL plan: "+recipe.Id);
                samples.Add(Prepare(recipe.Id,"practice",t,plan,i=>recipe.Body.PositionXZ));
            }
            int seen=0,captured=0,unusable=0,beyond=0;var captureRows=new StringBuilder("reception\trally\tcapturedId\treachable\treason\ttStarStep\tbodyX\tbodyZ\tbodyVx\tbodyVz\n");
            var outcomeRows=new StringBuilder("rally\tfirstReceiver\tend\tcause\twinner\tshots\tsteps\n");
            var brain=new Capture(new FondoAi(context,AiDifficulty.For(context.Params,AiLevel.Medium),1111));
            using(var sim=new RallySim(context,new BotBrain(context,30,2026),brain))
            {
                int rallyIndex=0;
                brain.Observe=(observation,plan)=>
                {
                    seen++;string id="";bool usable=plan.Contact.Exists&&plan.Contact.TStarStep>=0;
                    if(!usable)unusable++;
                    else if(captured>=24)beyond++;
                    else
                    {
                        captured++;id="FR"+captured.ToString("D2",C);
                        var t=context.Predict(observation.Ball);var live=sim.Rival;
                        int delay=context.Steps(context.Params.ReactionS)+brain.PerceptionSteps;
                        samples.Add(Prepare(id,"rival",t,plan.Contact,i=>live.PositionAfter(plan.Stance,delay,i)));
                    }
                    Row(captureRows,seen,rallyIndex,id,plan.Reachable,plan.Reason,plan.Contact.TStarStep,observation.SelfPosition.x,observation.SelfPosition.y,observation.SelfVelocity.x,observation.SelfVelocity.y);
                };
                for(rallyIndex=0;rallyIndex<8;rallyIndex++)
                {
                    var o=sim.Play(rallyIndex%2==0?PlayerId.Human:PlayerId.Rival);
                    if(o.End!=RallyEnd.Point||o.SystemFaults!=0)throw new InvalidDataException("Rally sample did not terminate cleanly");
                    Row(outcomeRows,rallyIndex,o.FirstReceiver,o.End,o.Cause,o.Winner,o.Shots,o.Steps);
                }
            }
            var conditions=new List<Condition>();
            foreach(var s in samples)foreach(double tempo in new[]{.3,.65,1})foreach(double comp in new[]{-75d,0,75})conditions.Add(Measure(s,tempo,comp,cfg.Physics.StepHz));
            double fixedRadius=conditions.Max(c=>c.Radius);
            var zones=new StringBuilder("id\torigin\ttempo\tcompMs\tform\tkIn\tkOut\ttStar\tperfectDistinctSteps\tperfectFirst\tperfectLast\tperfectSpanSteps\tperfectSpanRealMs\tLarcM\tadaptiveRadiusM\tfixedRadiusM\tkNodes\tzeroNodes\tnonzeroNodes\tkNonzeroFraction\tzeroSpanSteps\tzeroSpanRealMs\tzeroSpanArcM\tzeroNodeDiameterM\tcontactMs\tfailedMs\tperfectMs\tvalidNonPerfectMs\tnonzeroContactMs\tnonzeroOverContactFraction\tnonzeroOver200msFraction\tearlyContactMs\tearlyNonzeroMs\tlateContactMs\tlateNonzeroMs\tmaxHeightM\tmaxSpeedFractionLoss\n");
            var cells=new StringBuilder("id\ttempo\tcompMs\tkind\tleftDeltaMs\trightDeltaMs\tprobeDeltaMs\tresult\tfail\treason\tcontactStep\tscheduled\tneighbor\tdTrajectoryM\tdAdaptiveM\tdFixedM\n");
            var probes=new StringBuilder("id\ttempo\tcompMs\tdeltaMs\tresult\tfail\tcontactStep\tform\tdM\theightM\tspeedFraction\n");
            foreach(var c in conditions)
            {
                var s=c.Sample;
                foreach(var cell in c.Cells)
                {
                    var d=cell.Decision;double[] distances=cell.Contact?new[]{"trajectory","adaptive-radius","fixed-radius"}.Select(f=>EffectDistance(c,Index(s,d.ContactStep),f,fixedRadius)).ToArray():null;
                    Row(cells,s.Id,c.Tempo,c.Comp,cell.Kind,cell.Left*1000,cell.Right*1000,cell.Release*1000,d.Result,d.Fail,d.Reason,d.ContactStep,d.Scheduled,d.UsedNeighborLegalStep,
                        distances==null?"":F(distances[0]),distances==null?"":F(distances[1]),distances==null?"":F(distances[2]));
                    if(cell.Kind=="probe")foreach(string form in new[]{"trajectory","adaptive-radius","fixed-radius"})
                    {
                        double distance=cell.Contact?EffectDistance(c,Index(s,d.ContactStep),form,fixedRadius):0;
                        Row(probes,s.Id,c.Tempo,c.Comp,cell.Release*1000,d.Result,d.Fail,d.ContactStep,form,cell.Contact?F(distance):"",cell.Contact?F(Math.Min(1,.6*distance)):"",cell.Contact?F(Math.Max(-.4,-.25*distance)):"");
                    }
                }
                foreach(string form in new[]{"trajectory","adaptive-radius","fixed-radius"})
                {
                    var legalNodes=Enumerable.Range(0,s.Q.Length).Where(i=>s.Legal[i]).ToArray();
                    var zeros=legalNodes.Where(i=>EffectDistance(c,i,form,fixedRadius)==0).ToArray();
                    double diameter=0;foreach(int i in zeros)foreach(int j in zeros)diameter=Math.Max(diameter,Distance(s.Q[i],s.Q[j]));
                    var open=c.Cells.Where(x=>x.Kind=="open").ToArray();
                    double contact=0,failed=0,perfect=0,valid=0,nonzero=0,earlyContact=0,earlyNonzero=0,lateContact=0,lateNonzero=0,maxD=0;
                    foreach(var cell in open)
                    {
                        if(!cell.Contact){failed+=cell.DurationMs;continue;}
                        double distance=EffectDistance(c,Index(s,cell.Decision.ContactStep),form,fixedRadius);contact+=cell.DurationMs;maxD=Math.Max(maxD,distance);
                        if(cell.Decision.Result==ShotResult.Perfect)perfect+=cell.DurationMs;else valid+=cell.DurationMs;
                        if(distance>0)nonzero+=cell.DurationMs;
                        if(cell.Release<-.04){earlyContact+=cell.DurationMs;if(distance>0)earlyNonzero+=cell.DurationMs;}
                        if(cell.Release>.04){lateContact+=cell.DurationMs;if(distance>0)lateNonzero+=cell.DurationMs;}
                    }
                    int first=zeros.First(),last=zeros.Last();double span=last-first;
                    Row(zones,s.Id,s.Origin,c.Tempo,c.Comp,form,s.Plan.KIn.Steps,s.Plan.KOut.Steps,s.Plan.TStarStep,c.Perfect.Count,c.Perfect.Min,c.Perfect.Max,c.Perfect.Max-c.Perfect.Min,(c.Perfect.Max-c.Perfect.Min)/c.Rate*1000,
                        c.L,c.Radius,fixedRadius,legalNodes.Length,zeros.Length,legalNodes.Length-zeros.Length,(legalNodes.Length-zeros.Length)/(double)legalNodes.Length,span,span/c.Rate*1000,s.Arc[last]-s.Arc[first],diameter,
                        contact,failed,perfect,valid,nonzero,contact==0?"":F(nonzero/contact),nonzero/200,earlyContact,earlyNonzero,lateContact,lateNonzero,Math.Min(1,.6*maxD),Math.Min(.4,.25*maxD));
                }
            }
            var geometry=new StringBuilder("id\tstep\tballX\tballY\tballZ\tvx\tvy\tvz\tbodyX\tbodyZ\tqX\tqY\tqZ\tarcM\tlegal\n");
            var events=new StringBuilder("id\tsurface\tstep\ttimeInStep\tx\ty\tz\tpersistent\n");
            foreach(var s in samples)
            {
                for(int i=0;i<s.Flight.States.Count;i++)
                {var b=s.Flight.States[i];Row(geometry,s.Id,b.Step,b.Position.x,b.Position.y,b.Position.z,b.Velocity.x,b.Velocity.y,b.Velocity.z,s.Bodies[i].x,s.Bodies[i].y,s.Q[i].x,s.Q[i].y,s.Q[i].z,s.Arc[i],s.Legal[i]);}
                foreach(var e in s.Flight.Contacts)Row(events,s.Id,e.Surface,e.Step,e.TimeInStep,e.Point.x,e.Point.y,e.Point.z,e.Persistent);
            }
            if(samples.Count!=6+captured||factory.ImpactCapReachedCount!=0||factory.LiveInstances!=0)throw new InvalidDataException("Incomplete sample/cap/resource issue");
            var metadata=new StringBuilder();
            Row(metadata,"method","CQ-ZONE-v1");Row(metadata,"source",source);Row(metadata,"defaults",p.Hash);Row(metadata,"keys",p.Ordered.Count);Row(metadata,"simulationKey",cfg.SimulationKey);Row(metadata,"rallyParams",context.Params.Hash);Row(metadata,"p0Effective",p.With("launcher.repeatSeconds",3).Hash);Row(metadata,"bounceThreshold",UnityEngine.Physics.bounceThreshold);Row(metadata,"stepHz",cfg.Physics.StepHz);Row(metadata,"rallies",8);Row(metadata,"rivalReceptionsSeen",seen);Row(metadata,"rivalCaptured",captured);Row(metadata,"rivalUnusable",unusable);Row(metadata,"rivalBeyondCaptureLimit",beyond);Row(metadata,"samples",samples.Count);Row(metadata,"conditions",conditions.Count);Row(metadata,"fixedRadiusM",fixedRadius);Row(metadata,"impactCaps",factory.ImpactCapReachedCount);Row(metadata,"liveInstances",factory.LiveInstances);Row(metadata,"status","proposal only; causal product absent; not H3b or acceptance");
            string directory=Path.Combine(root,"Logs/experiments/causal-zone",run);
            if(Directory.Exists(directory))throw new IOException("Refusing to overwrite experiment data");Directory.CreateDirectory(directory);
            foreach(var file in new[]{("metadata.tsv",metadata),("zones.tsv",zones),("cells.tsv",cells),("probes.tsv",probes),("geometry.tsv",geometry),("events.tsv",events),("capture.tsv",captureRows),("rally-sample.tsv",outcomeRows)})File.WriteAllText(Path.Combine(directory,file.Item1),file.Item2.ToString(),Utf8);
            TestContext.WriteLine("CQ_ZONE_DATA="+directory);TestContext.WriteLine("CQ_ZONE_COUNTS samples="+samples.Count+" conditions="+conditions.Count+" fixedRadiusM="+F(fixedRadius));
        }
        private static string Git(string root,string arguments)
        {using(var process=Process.Start(new ProcessStartInfo("git",arguments){WorkingDirectory=root,RedirectStandardOutput=true,UseShellExecute=false,CreateNoWindow=true})){string s=process.StandardOutput.ReadToEnd().Trim();process.WaitForExit();if(process.ExitCode!=0)throw new IOException("Git metadata failed");return s;}}
    }
}
