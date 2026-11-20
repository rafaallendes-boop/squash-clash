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
    // Volley v2.1 grid (design 2026-11-20 section 6, predeclared): HumanVolleySprintMax in {1.25, 1.5, 1.75, 2.0}, first that meets targets
    // 1-3 wins. Counterfactual exactly as VolleyV2CommitExperiment (same seeds, games, newcomer, PO touch draws): the product's
    // HumanVolley.OfferDetail and Commit (normal up to S, sprint from S to S'). Also records launch->S and launch->S' for the report.
    public sealed class VolleyV21GridExperiment
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
            public RallySim Sim;public string Style;public readonly List<string> Rows=new List<string>();
            public Probe(RallyContext ctx,IRallyBrain inner,long seed){this.ctx=ctx;this.inner=inner;random=new RallyRng(seed);}
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own){Measure(o,own);return inner.Decide(o,own);}
            private double Ms(double steps)=>steps/(ctx.StepHz*ctx.Tempo)*1000;
            private static bool Pv(in ContactDecision d)=>d.Result==ShotResult.Perfect||d.Result==ShotResult.Valid;
            private string F(double v)=>double.IsNaN(v)?"NaN":v.ToString("0.#",C);
            private void Measure(in AiObservation o,in ReceptionPlan own)
            {
                var t=ctx.Predict(o.Ball);long first=t.States[0].Step;bool serve=Sim.Rules.Serving;
                int delay=ctx.Steps(ctx.Params.ReactionS)+inner.PerceptionSteps+FatigueEffects.For(ctx.Fatigue.Factor(Sim.HumanEnergy.Energy),ctx.Fatigue).ExtraSteps;
                var bounce=own.Contact;int version=bounce.Version;bool bounceExists=bounce.Exists&&bounce.TStarStep>=0;
                double now=first/(ctx.Tempo*ctx.StepHz);
                ClockMap Clock(){var c=new ClockMap();c.Begin(1,now,first,ctx.Tempo,1,ctx.StepHz);return c;}
                // The same three draws, in the same order, as VolleyV2CommitExperiment.
                double dBounce=random.NextGaussian(NewcomerSigmaMs),dVolley=random.NextGaussian(NewcomerSigmaMs),u=random.NextDouble();
                string bouncePv="NA";
                if(own.Reachable&&bounceExists)bouncePv=Pv(RallyContact.Resolve(ctx,Clock(),t,bounce,now,dBounce))?"1":"0";
                var d=HumanVolley.OfferDetail(ctx,t,Sim.Rules,Sim.Human,delay,version);bool offered=d.Offer.Exists;
                double sMs=offered?Ms(d.S-first):double.NaN,spMs=offered?Ms(d.SPrime-first):double.NaN;
                double whiteLead=offered&&bounceExists?Math.Min(500,Ms(bounce.TStarStep-d.SPrime)):double.NaN;
                double touch=bounceExists?Ms(bounce.TStarStep-first)-PoLeadMs(u):double.NaN;
                string committed="0",pv="NA",sprinted="0",lateness="NaN";
                if(offered&&!double.IsNaN(touch)&&touch>=0&&touch<=spMs)
                {
                    long c=first+(long)Math.Floor(touch/1000*ctx.StepHz*ctx.Tempo);var walk=Sim.Human.Clone();for(long k=first;k<c;k++)walk.Step();
                    var commit=c<=d.S?HumanVolley.Commit(ctx,t,Sim.Rules,walk,c,delay,version):default;
                    if(!commit.Exists){commit=HumanVolley.Commit(ctx,t,Sim.Rules,walk,c,delay,version,ctx.HumanVolleySprint);if(commit.Exists){sprinted="1";lateness=HumanVolley.Lateness(d.S,d.SPrime,c).ToString("0.###",C);}}
                    if(commit.Exists){committed="1";pv=Pv(RallyContact.Resolve(ctx,Clock(),t,commit.Plan,now,dVolley))?"1":"0";}
                }
                Rows.Add(string.Join("\t",new[]{Style,serve?"1":"0",offered?"1":"0",d.Offer.Reason??"",bouncePv,F(sMs),F(spMs),F(whiteLead),committed,pv,sprinted,lateness}));
            }
        }
        [Test,Explicit,Category("Experiments"),Timeout(7200000)]
        public void MeasureGrid()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));string dir=Path.Combine(root,"Logs/experiments/volley-v21-grid");
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence");Directory.CreateDirectory(dir);
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash,Is.EqualTo("207da8f2c636"));
            foreach(var max in new[]{1.25,1.5,1.75,2.0})
            {
                var p=new RallyParams{HumanVolleyMode=3,HumanVolleySprintMax=max};
                var ctx=new RallyContext(EffectiveConfig.From(preset),p,new AnalyticBallSimulationFactory(Physics.bounceThreshold));
                Assert.That(ctx.Config.SimulationKey,Is.EqualTo("166a96e319c7"));Assert.That(ctx.HumanVolleySprint,Is.EqualTo((float)max));
                var all=new StringBuilder("style\tserve\toffered\treason\tbouncePV\tlaunchToSMs\tlaunchToSPrimeMs\twhiteLeadAfterSPrimeMs\tcommitted\tvolleyPV\tsprint\tlateness\n");
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
                                var o=sim.PlayPoint(score.Server,score.Box);points++;
                                if(o.Winner.HasValue)score.PointWon(o.Winner.Value);else if(++replays>50)break;
                            }
                        }
                        foreach(var r in probe.Rows)all.Append(r).Append('\n');
                    }
                }
                File.WriteAllText(Path.Combine(dir,"sprint-"+max.ToString("0.00",C)+".tsv"),"# rallyParams "+p.Hash+" preset "+preset.Hash+"\n"+all);
            }
        }
    }
}
