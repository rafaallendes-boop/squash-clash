using System;
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
    // Target 4 of the volley v2 design (section 6, data only, no threshold): real RallySim games with the preset's fatigue on, Medium rival
    // (three styles, 60 games each), a newcomer human (sigma 90 ms) that volleys on every touch that commits: v1 takes every offered v1
    // volley; v2 and v2.2 touch at the PO's sampled lead before the bounce T*. Reports the human energy at the start of points 5, 10, 15.
    public sealed class VolleyEnergyExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly double[] PoDeciles={477,578,684,758,850,940,1051,1130,1383};
        private static double PoLeadMs(double u)
        {
            if(u<=.1)return PoDeciles[0];if(u>=.9)return PoDeciles[8];
            double x=u*10-1;int k=(int)Math.Floor(x);return PoDeciles[k]+(PoDeciles[k+1]-PoDeciles[k])*(x-k);
        }
        private sealed class Bot : IRallyBrain,IRallyVolleyToucher
        {
            private readonly RallyContext ctx;private readonly RallyRng random;private readonly bool v1;
            public Bot(RallyContext ctx,long seed,bool v1){this.ctx=ctx;random=new RallyRng(seed);this.v1=v1;}
            public int PerceptionSteps=>0;
            public Vector2 StanceNoise()=>Vector2.zero;
            public double TouchMsAfterLaunch(in ReceptionPlan own,double bounceMs)=>v1||double.IsNaN(bounceMs)?double.NaN:bounceMs-PoLeadMs(random.NextDouble());
            public AiShot Decide(in AiObservation observation,in ReceptionPlan own)
            {
                bool volley=v1&&own.Volley.Exists;
                if(!volley&&(!own.Reachable||!own.Contact.Exists||own.Contact.TStarStep<0))return default;
                var at=volley?own.Volley.Plan.PStar:own.Contact.PStar;
                var line=random.NextDouble()<.5?ShotLine.Straight:ShotLine.Cross;var power=random.NextDouble()<.6?ShotPower.Strong:ShotPower.Medium;
                double delta=random.NextGaussian(90);var options=ShotOptions.Build(ctx,at);
                int Distance(ShotOption o)=>(o.Line==line?0:o.Line==ShotLine.Drop?8:4)+2*Math.Abs((int)o.Height-(int)ShotHeight.Mid)+(o.Power==power?0:1);
                options.Sort((a,b)=>Distance(a).CompareTo(Distance(b)));
                foreach(var option in options)if(RallyContact.Forecast(ctx,at,option.Aim,option.P).Good)return new AiShot(true,option,option.Aim,option.P,delta,false,volley);
                return new AiShot(true,options[0],options[0].Aim,options[0].P,delta,false,volley);
            }
        }
        [Test,Explicit,Category("Experiments"),Timeout(7200000)]
        public void MeasureEnergy()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));string dir=Path.Combine(root,"Logs/experiments/volley-energy");
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence");Directory.CreateDirectory(dir);
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash,Is.EqualTo("207da8f2c636"));
            var rows=new StringBuilder("variant\tstyle\tmatch\tpoints\thumanPoints\thumanVolleys\thumanReceptions\tvolleyCost\te5\te10\te15\n");
            foreach(var (name,mode) in new[]{("v1",1.0),("v2",2.0),("v2.2",3.0)})
            {
                var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams{HumanVolleyMode=mode},new AnalyticBallSimulationFactory(Physics.bounceThreshold));
                Assert.That(ctx.Fatigue.Enabled,Is.True);Assert.That(ctx.Config.SimulationKey,Is.EqualTo("166a96e319c7"));
                foreach(RivalStyleId style in Enum.GetValues(typeof(RivalStyleId)))
                    for(int i=0;i<60;i++)
                    {
                        long seed=20261130+1000*(int)style+2*i;
                        using(var sim=new RallySim(ctx,new Bot(ctx,seed,mode==1),RivalAi.For(ctx,AiLevel.Medium,style,seed+1)))
                        {
                            var m=sim.PlayMatch(i%2==0?PlayerId.Human:PlayerId.Rival);var p=m.Points;
                            string E(int k)=>p.Count>k?p[k].HumanFatigue.Start.ToString("0.###",C):"NaN";
                            int volleys=p.Sum(o=>o.Receptions.Count(r=>r.Who==PlayerId.Human&&r.Volley&&r.Result==ReceptionResult.Contact));
                            int receptions=p.Sum(o=>o.Receptions.Count(r=>r.Who==PlayerId.Human));
                            rows.Append(string.Join("\t",new[]{name,style.ToString(),i.ToString(C),p.Count.ToString(C),m.Human.ToString(C),volleys.ToString(C),receptions.ToString(C),
                                p.Sum(o=>o.HumanFatigue.VolleyCost).ToString("0.###",C),E(4),E(9),E(14)})).Append('\n');
                        }
                    }
            }
            File.WriteAllText(Path.Combine(dir,"matches.tsv"),"# rallyParams "+new RallyParams().Hash+" preset "+preset.Hash+"\n"+rows);
        }
    }
}
