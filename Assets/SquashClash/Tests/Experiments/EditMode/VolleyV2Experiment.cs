using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    // Volley v2 phase 1 (design 2026-11-20 section 0, predeclared before data). Counterfactual and human-only: at every human reception
    // each variant's volley opportunity is recomputed on the same trajectory, body path and bounce plan, and one sampled release is
    // judged by the real contract (RallyContact -> ContactContract.Decide). The rally itself always continues with the bounce, so the
    // rival and the points are the same for every variant. Nothing in production changes; nothing is tuned.
    public sealed class VolleyV2Experiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private const double NewcomerSigmaMs=90;
        private sealed class Probe : IRallyBrain
        {
            private readonly RallyContext ctx;private readonly IRallyBrain inner;private readonly RallyRng random;
            public RallySim Sim;public string Style;public int Point;public int Mismatches;
            public readonly List<string> Rows=new List<string>();
            public Probe(RallyContext ctx,IRallyBrain inner,long seed){this.ctx=ctx;this.inner=inner;random=new RallyRng(seed);}
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own){Measure(o,own);return inner.Decide(o,own);}
            private string F(double v)=>double.IsNaN(v)?"NaN":double.IsInfinity(v)?(v>0?"inf":"-inf"):v.ToString("0.###",C);
            private double Ms(double steps)=>steps/(ctx.StepHz*ctx.Tempo)*1000;
            private void Measure(in AiObservation o,in ReceptionPlan own)
            {
                var t=ctx.Predict(o.Ball);long first=t.States[0].Step;bool serve=Sim.Rules.Serving;
                var path=Sim.Human.PositionsAfter(t.States.Count);
                int delay=ctx.Steps(ctx.Params.ReactionS)+inner.PerceptionSteps+FatigueEffects.For(ctx.Fatigue.Factor(Sim.HumanEnergy.Energy),ctx.Fatigue).ExtraSteps;
                Func<long,BodyFrame> bodyAt=j=>ctx.FrameAt(path[(int)Math.Max(0,Math.Min(path.Length-1,j-first))]);
                var bounce=own.Contact;int version=bounce.Version;
                // V0 must reproduce the simulation's own opportunity exactly.
                var v0=ctx.VolleyFor(t,Sim.Rules,path,bounce,delay,version);
                if(v0.Exists!=own.Volley.Exists||v0.Exists&&(v0.Plan.KIn.Steps!=own.Volley.Plan.KIn.Steps||v0.Plan.KOut.Steps!=own.Volley.Plan.KOut.Steps||v0.Plan.TStarStep!=own.Volley.Plan.TStarStep))Mismatches++;
                var p=ctx.Params;
                VolleyOpportunity Run(double sep,double window)=>VolleyPlanner.Plan(t,bodyAt,ctx.Court,ctx.Legal,
                    new VolleySettings(true,(float)p.VolleyMinHeight,(float)p.VolleyMaxHeight,sep,window),bounce,first+delay,ctx.Tempo,version);
                // Serve gate bypassed on purpose: A0s is measured on serve returns only.
                var a0=Run(double.NegativeInfinity,p.VolleyMinWindowS);var a1=Run(double.NegativeInfinity,.03);
                double now=first/(ctx.Tempo*ctx.StepHz);
                ClockMap Clock(){var c=new ClockMap();c.Begin(1,now,first,ctx.Tempo,1,ctx.StepHz);return c;}
                double dBounce=random.NextGaussian(NewcomerSigmaMs),dVolley=random.NextGaussian(NewcomerSigmaMs);
                bool bounceExists=bounce.Exists&&bounce.TStarStep>=0;
                string bouncePv="NA";
                if(own.Reachable&&bounceExists)
                {var d=RallyContact.Resolve(ctx,Clock(),t,bounce,now,dBounce);bouncePv=(d.Result==ShotResult.Perfect||d.Result==ShotResult.Valid)?"1":"0";}
                double leadB=bounceExists?Math.Min(500,Ms(bounce.TStarStep-first)):double.NaN;
                double kB=bounceExists?Ms(bounce.KOut.Steps-bounce.KIn.Steps):double.NaN;
                // No-pass: how far outside reach the ball passes the moving body before its bounce (legal height, after reaction).
                double excess=double.PositiveInfinity;
                var legal0=new LegalSettings((float)p.VolleyMinHeight,(float)p.VolleyMaxHeight,ctx.Legal.StepHz,ctx.Legal.Gravity,ctx.Legal.Radius,ctx.Court,0);
                var phase=ContactPlanner.PhaseLegal(t,bodyAt(first),legal0);
                for(int i=0;i<phase.Length;i++)
                {
                    if(!phase[i]||first+i<first+delay)continue;var b=bodyAt(first+i);var ball=t.States[i].Position;
                    double dx=ball.x-b.PositionXZ.x,dy=ball.y-b.ReferenceHeight,dz=ball.z-b.PositionXZ.y;excess=Math.Min(excess,Math.Sqrt(dx*dx+dy*dy+dz*dz)-b.Reach);
                }
                string Variant(in VolleyOpportunity v)
                {
                    if(!v.Exists)return "0\tNaN\tNaN\tNaN\tNA\tNA";
                    var r=RallyContact.ResolveAimedVolley(ctx,Clock(),t,bounce,v,now,dVolley,out bool used);
                    bool pv=used&&(r.Result==ShotResult.Perfect||r.Result==ShotResult.Valid);
                    double afterS=bounceExists&&!double.IsInfinity(v.SplitSteps)?Math.Min(500,Ms(bounce.TStarStep-v.SplitSteps)):double.NaN;
                    return "1\t"+F(Ms(v.Plan.KOut.Steps-v.Plan.KIn.Steps))+"\t"+F(Math.Min(500,Ms(v.Plan.TStarStep-first)))+"\t"+F(afterS)+"\t"+(pv?"1":"0")+"\t"+(used?"1":"0");
                }
                double gap=a0.Exists&&bounceExists?Ms(bounce.KIn.Steps-a0.Plan.KOut.Steps):double.NaN;
                // B: a chosen intercept stance (the longest fixed-frame volley K reachable before its T*); the bounce is forfeited.
                var bv=Intercept(t,first,delay,legal0,version);string bCols="0\tNaN\tNaN\tNA";
                if(bv.Exists)
                {
                    var r=RallyContact.Resolve(ctx,Clock(),t,bv,now,dVolley);
                    bCols="1\t"+F(Ms(bv.KOut.Steps-bv.KIn.Steps))+"\t"+F(Math.Min(500,Ms(bv.TStarStep-first)))+"\t"+((r.Result==ShotResult.Perfect||r.Result==ShotResult.Valid)?"1":"0");
                }
                Rows.Add(string.Join("\t",new[]{Style,Point.ToString(C),serve?"1":"0",own.Reachable?"1":"0",v0.Reason,F(gap),F(excess),F(kB),F(leadB),bouncePv,
                    Variant(v0),Variant(a0),Variant(a1),bCols}));
            }
            private ContactPlan Intercept(Trajectory t,long first,int delay,LegalSettings legal0,int version)
            {
                var phase=ContactPlanner.PhaseLegal(t,ctx.FrameAt(Sim.Human.Position),legal0);
                int interval=Math.Max(1,ctx.Steps(ctx.Params.CandidateIntervalS));ContactPlan best=default;double bestK=-1;
                var frame0=ctx.FrameAt(Sim.Human.Position);
                for(int i=0;i<phase.Length;i++)
                {
                    if(!phase[i]||i<delay||i%interval!=0)continue;var ball=t.States[i].Position;
                    foreach(float side in new[]{-1f,1f})
                    {
                        var stance=ctx.ClampBody(new Vector2(ball.x+side*frame0.IdealLateral,ball.z));
                        var plan=ContactPlanner.Plan(t,ctx.FrameAt(stance),ctx.Court,legal0,version);
                        if(!plan.Exists||plan.TStarStep<0||plan.KIn.Steps<first+delay)continue;
                        int arrival=Sim.Human.StepsToReach(stance,delay,out _,(int)(plan.TStarStep-first)+1);
                        if(arrival==int.MaxValue||first+arrival>plan.TStarStep)continue;
                        double k=plan.KOut.Steps-plan.KIn.Steps;if(k>bestK){bestK=k;best=plan;}
                    }
                }
                return best;
            }
        }
        [Test,Explicit,Category("Experiments"),Timeout(3600000)]
        public void MeasureVolleyV2()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));string dir=Path.Combine(root,"Logs/experiments/volley-v2");
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence");
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash,Is.EqualTo("207da8f2c636"));
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            Assert.That(ctx.Config.SimulationKey,Is.EqualTo("166a96e319c7"));
            var all=new StringBuilder("style\tpoint\tserve\treachable\tv0Reason\tgapMs\tnoPassExcessM\tkBounceMs\tleadBounceMs\tbouncePV");
            foreach(var v in new[]{"v0","a0","a1"})all.Append($"\t{v}Offered\t{v}KMs\t{v}LeadMs\t{v}BounceLeadAfterSMs\t{v}PV\t{v}UsedVolley");
            all.Append("\tbOffered\tbKMs\tbLeadMs\tbPV\n");int mismatches=0,faults=0;
            foreach(RivalStyleId style in Enum.GetValues(typeof(RivalStyleId)))
            {
                int points=0;
                for(int i=0;points<200;i++)
                {
                    long seed=20261120+1000*(int)style+2*i;
                    var probe=new Probe(ctx,new RivalStylesBaselineExperiment.NewcomerBot(ctx,NewcomerSigmaMs,seed),seed+500000){Style=style.ToString()};
                    using(var sim=new RallySim(ctx,probe,RivalAi.For(ctx,AiLevel.Medium,style,seed+1)))
                    {
                        probe.Sim=sim;var score=new MatchScore(7,2,i%2==0?PlayerId.Human:PlayerId.Rival);int replays=0;
                        while(!score.Finished&&points<200)
                        {
                            probe.Point=points;var o=sim.PlayPoint(score.Server,score.Box);points++;faults+=o.SystemFaults;
                            if(o.Winner.HasValue)score.PointWon(o.Winner.Value);else if(++replays>50)break;
                        }
                    }
                    foreach(var r in probe.Rows)all.Append(r).Append('\n');mismatches+=probe.Mismatches;
                }
            }
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"receptions.tsv"),"# rallyParams "+ctx.Params.Hash+" preset "+preset.Hash+"\n"+all);
            TestContext.WriteLine($"mismatches {mismatches} faults {faults}");
            Assert.That(mismatches,Is.Zero,"V0 must reproduce the simulation's volley opportunity");
        }
    }
}
