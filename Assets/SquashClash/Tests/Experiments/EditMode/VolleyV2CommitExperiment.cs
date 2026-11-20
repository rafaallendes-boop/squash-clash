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
    // Volley v2 phase 2 checks (design 2026-11-20 section 5, predeclared before data, window corrected to [launch, S] before data).
    // Same seeds, games and newcomer as VolleyV2Experiment. At each human reception: the product's own HumanVolley.Offer and, for a touch
    // 150 ms or 300 ms after the launch or at the PO's sampled touch-down lead before the bounce T*, the product's HumanVolley.Commit from the
    // body walked along its M1 path to that touch; then one release N(0, 90 ms) around the committed T*v judged by the real contract.
    // Counterfactual: the rally itself always plays the bounce. Stop rule: PO-replay P+V gap to the bounce above 15 pp.
    public sealed class VolleyV2CommitExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private const double NewcomerSigmaMs=90;
        private static readonly double[] PoDeciles={477,578,684,758,850,940,1051,1130,1383};
        private static double PoLeadMs(double u)
        {
            if(u<=.1)return PoDeciles[0];if(u>=.9)return PoDeciles[8];
            double x=u*10-1;int k=(int)Math.Floor(x);return PoDeciles[k]+(PoDeciles[k+1]-PoDeciles[k])*(x-k);
        }
        private sealed class Probe : IRallyBrain
        {
            private readonly RallyContext ctx;private readonly IRallyBrain inner;private readonly RallyRng random;
            public RallySim Sim;public string Style;
            public readonly List<string> Rows=new List<string>();
            public Probe(RallyContext ctx,IRallyBrain inner,long seed){this.ctx=ctx;this.inner=inner;random=new RallyRng(seed);}
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own){Measure(o,own);return inner.Decide(o,own);}
            private double Ms(double steps)=>steps/(ctx.StepHz*ctx.Tempo)*1000;
            private static bool Pv(in ContactDecision d)=>d.Result==ShotResult.Perfect||d.Result==ShotResult.Valid;
            private void Measure(in AiObservation o,in ReceptionPlan own)
            {
                var t=ctx.Predict(o.Ball);long first=t.States[0].Step;bool serve=Sim.Rules.Serving;
                int delay=ctx.Steps(ctx.Params.ReactionS)+inner.PerceptionSteps+FatigueEffects.For(ctx.Fatigue.Factor(Sim.HumanEnergy.Energy),ctx.Fatigue).ExtraSteps;
                var bounce=own.Contact;int version=bounce.Version;bool bounceExists=bounce.Exists&&bounce.TStarStep>=0;
                double now=first/(ctx.Tempo*ctx.StepHz);
                ClockMap Clock(){var c=new ClockMap();c.Begin(1,now,first,ctx.Tempo,1,ctx.StepHz);return c;}
                double dBounce=random.NextGaussian(NewcomerSigmaMs),dVolley=random.NextGaussian(NewcomerSigmaMs),u=random.NextDouble();
                string bouncePv="NA";
                if(own.Reachable&&bounceExists)bouncePv=Pv(RallyContact.Resolve(ctx,Clock(),t,bounce,now,dBounce))?"1":"0";
                var offer=HumanVolley.Offer(ctx,t,Sim.Rules,Sim.Human,delay,version);
                double leadAfterS=offer.Exists&&bounceExists?Math.Min(500,Ms(bounce.TStarStep-offer.SplitSteps)):double.NaN;
                var walk=Sim.Human.Clone();long walked=first;
                string Case(double touchMsAfterLaunch)
                {
                    if(!offer.Exists||double.IsNaN(touchMsAfterLaunch))return "0\tNA";
                    double sMs=Ms(offer.SplitSteps-first);
                    if(touchMsAfterLaunch<0||touchMsAfterLaunch>sMs)return "0\tNA";
                    long c=first+(long)Math.Floor(touchMsAfterLaunch/1000*ctx.StepHz*ctx.Tempo);
                    if(c<walked){walk=Sim.Human.Clone();walked=first;}
                    while(walked<c){walk.Step();walked++;}
                    var commit=HumanVolley.Commit(ctx,t,Sim.Rules,walk,c,delay,version);
                    if(!commit.Exists)return "0\tNA";
                    return "1\t"+(Pv(RallyContact.Resolve(ctx,Clock(),t,commit.Plan,now,dVolley))?"1":"0");
                }
                double po=bounceExists?Ms(bounce.TStarStep-first)-PoLeadMs(u):double.NaN;
                Rows.Add(string.Join("\t",new[]{Style,serve?"1":"0",own.Reachable?"1":"0",offer.Exists?"1":"0",offer.Reason??"",bouncePv,
                    double.IsNaN(leadAfterS)?"NaN":leadAfterS.ToString("0.#",C),Case(150),Case(300),Case(po)}));
            }
        }
        [Test,Explicit,Category("Experiments"),Timeout(3600000)]
        public void MeasureCommitChecks()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));string dir=Path.Combine(root,"Logs/experiments/volley-v2-commit");
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence");
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash,Is.EqualTo("207da8f2c636"));
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            Assert.That(ctx.Config.SimulationKey,Is.EqualTo("166a96e319c7"));Assert.That(ctx.HumanVolleyV2,Is.True);
            var all=new StringBuilder("style\tserve\treachable\toffered\treason\tbouncePV\tbounceLeadAfterSMs\tc150\tpv150\tc300\tpv300\tcPo\tpvPo\n");int faults=0;
            foreach(RivalStyleId style in Enum.GetValues(typeof(RivalStyleId)))
            {
                int points=0;
                for(int i=0;points<200;i++)
                {
                    long seed=20261120+1000*(int)style+2*i;
                    var probe=new Probe(ctx,new RivalStylesBaselineExperiment.NewcomerBot(ctx,NewcomerSigmaMs,seed),seed+700000){Style=style.ToString()};
                    using(var sim=new RallySim(ctx,probe,RivalAi.For(ctx,AiLevel.Medium,style,seed+1)))
                    {
                        probe.Sim=sim;var score=new MatchScore(7,2,i%2==0?PlayerId.Human:PlayerId.Rival);int replays=0;
                        while(!score.Finished&&points<200)
                        {
                            var o=sim.PlayPoint(score.Server,score.Box);points++;faults+=o.SystemFaults;
                            if(o.Winner.HasValue)score.PointWon(o.Winner.Value);else if(++replays>50)break;
                        }
                    }
                    foreach(var r in probe.Rows)all.Append(r).Append('\n');
                }
            }
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"receptions.tsv"),"# rallyParams "+ctx.Params.Hash+" preset "+preset.Hash+" faults "+faults+"\n"+all);
            TestContext.WriteLine("faults "+faults);
        }
    }
}
