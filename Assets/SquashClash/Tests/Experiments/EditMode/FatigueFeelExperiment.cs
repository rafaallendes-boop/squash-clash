using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    // Fatigue feel (design 2026-11-20-fatigue-feel, section 4), predeclared, one run. RallySim with the real DefaultPreset.json (fatigue on),
    // Medium rival in the three styles, the v2.2 energy-experiment human (newcomer sigma 90 ms, touches at the PO's sampled lead). Four arms:
    // A-off (sprint always full), A-on (energy-limited sprint), B32 and B43 (strong fatigue x3/x2 and x4/x3, both players). Set 1: the human
    // starts each point at 100, 55, 25 or 5 and the rival at 100, 50 points per (start, style). Set 2: natural matches, 30 per style.
    public sealed class FatigueFeelExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static string F(double v)=>v.ToString("0.######",C);
        private static readonly (string id,double sprintFatigue,double legs,double reaction)[] Arms={("A-off",0,1,1),("A-on",1,1,1),("B32",1,3,2),("B43",1,4,3)};
        private static readonly double[] Starts={100,55,25,5};
        private static string Band(double f,double onset)
        {
            if(!(f>0))return ">=70";double e=onset*(1-f);
            return e>=40?"40-70":e>=10?"10-40":"<10";
        }
        [Test,Explicit,Category("Experiments"),Timeout(7200000)]
        public void MeasureFatigueFeel()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));string dir=Path.Combine(root,"Logs/experiments/fatigue-feel");
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence");Directory.CreateDirectory(dir);
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash,Is.EqualTo("207da8f2c636"));Assert.That(new RallyParams().Hash,Is.EqualTo("13f88c8d1db2"));
            var factory=new AnalyticBallSimulationFactory(Physics.bounceThreshold);
            var receptions=new StringBuilder("arm\tset\tstyle\tstart\tgame\tpoint\tindex\twho\tserveReturn\tf\tband\tresult\treachable\tslackS\tvolley\tvolleyOffered\tsprint\tperfect\n");
            var points=new StringBuilder("arm\tset\tstyle\tstart\tgame\tpoint\tserver\twinner\tshots\thumanStart\thumanEnd\thumanMeters\trivalStart\trivalEnd\trivalMeters\tsimSeconds\n");
            var matches=new StringBuilder("arm\tstyle\tgame\tpoints\thuman\trival\twinner\th5\th10\th15\tr5\tr10\tr15\n");
            var source=new StringBuilder("arm\tpreset\trallyParams\tsimulationKey\tlegsMax\treactionMaxMs\tonset\n");
            foreach(var arm in Arms)
            {
                var set=arm.legs==1&&arm.reaction==1?preset:FatigueSettings.Strong(preset,arm.legs,arm.reaction);
                var ctx=new RallyContext(EffectiveConfig.From(set),new RallyParams{HumanVolleySprintFatigue=arm.sprintFatigue},factory);
                Assert.That(ctx.Fatigue.Enabled,Is.True);Assert.That(ctx.Config.SimulationKey,Is.EqualTo("166a96e319c7"));
                Assert.That(ctx.Params.HumanVolleyMode,Is.EqualTo(3));Assert.That(ctx.Params.HumanVolleySprintMax,Is.EqualTo(2));
                source.Append(string.Join("\t",new[]{arm.id,set.Hash,ctx.Params.Hash,ctx.Config.SimulationKey,F(ctx.Fatigue.LegsMax),F(ctx.Fatigue.ReactionMaxMs),F(ctx.Fatigue.Onset)})).Append('\n');
                void Log(string setName,RivalStyleId style,string start,int game,int index,RallyOutcome o)
                {
                    for(int k=0;k<o.Receptions.Count;k++)
                    {
                        var r=o.Receptions[k];
                        receptions.Append(string.Join("\t",new[]{arm.id,setName,style.ToString(),start,game.ToString(C),index.ToString(C),k.ToString(C),r.Who.ToString(),
                            (k==0&&o.Server.HasValue?1:0).ToString(C),F(r.FatigueReception),Band(r.FatigueReception,ctx.Fatigue.Onset),r.Result.ToString(),(r.Reachable?1:0).ToString(C),
                            F(r.SlackS),(r.Volley?1:0).ToString(C),(r.VolleyOffered?1:0).ToString(C),F(r.VolleySprintFactor),(r.Perfect?1:0).ToString(C)})).Append('\n');
                    }
                    points.Append(string.Join("\t",new[]{arm.id,setName,style.ToString(),start,game.ToString(C),index.ToString(C),o.Server?.ToString()??"",o.Winner?.ToString()??"",o.Shots.ToString(C),
                        F(o.HumanFatigue.Start),F(o.HumanFatigue.End),F(o.HumanFatigue.Meters),F(o.RivalFatigue.Start),F(o.RivalFatigue.End),F(o.RivalFatigue.Meters),F(o.SimSeconds)})).Append('\n');
                }
                foreach(RivalStyleId style in Enum.GetValues(typeof(RivalStyleId)))
                {
                    for(int s=0;s<Starts.Length;s++)
                    {
                        long seed=20261301+1000*(int)style+100*s;
                        using(var sim=new RallySim(ctx,new VolleyEnergyExperiment.Bot(ctx,seed,false),RivalAi.For(ctx,AiLevel.Medium,style,seed+1)))
                            for(int i=0;i<50;i++)
                            {
                                sim.SetEnergy(Starts[s],100);
                                var o=sim.PlayPoint(i%2==0?PlayerId.Human:PlayerId.Rival,(i/2)%2==0?ServiceBox.Right:ServiceBox.Left);
                                Log("fixed",style,F(Starts[s]),s,i,o);
                            }
                    }
                    for(int i=0;i<30;i++)
                    {
                        long seed=20261401+1000*(int)style+2*i;
                        using(var sim=new RallySim(ctx,new VolleyEnergyExperiment.Bot(ctx,seed,false),RivalAi.For(ctx,AiLevel.Medium,style,seed+1)))
                        {
                            var m=sim.PlayMatch(i%2==0?PlayerId.Human:PlayerId.Rival);var p=m.Points;
                            for(int k=0;k<p.Count;k++)Log("match",style,"",i,k,p[k]);
                            string E(int k,bool human)=>p.Count>k?F(human?p[k].HumanFatigue.Start:p[k].RivalFatigue.Start):"NaN";
                            matches.Append(string.Join("\t",new[]{arm.id,style.ToString(),i.ToString(C),p.Count.ToString(C),m.Human.ToString(C),m.Rival.ToString(C),m.Winner?.ToString()??"",
                                E(4,true),E(9,true),E(14,true),E(4,false),E(9,false),E(14,false)})).Append('\n');
                        }
                    }
                }
            }
            File.WriteAllText(Path.Combine(dir,"source.tsv"),source.ToString());
            File.WriteAllText(Path.Combine(dir,"receptions.tsv"),receptions.ToString());
            File.WriteAllText(Path.Combine(dir,"points.tsv"),points.ToString());
            File.WriteAllText(Path.Combine(dir,"matches.tsv"),matches.ToString());
        }
    }
}
