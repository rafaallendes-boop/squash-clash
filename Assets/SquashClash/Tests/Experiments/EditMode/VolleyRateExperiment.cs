using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    // Volley v1 checkpoint (design 2026-11-18, step 5), fixed before data: the first 200 points of games between the default bot and the
    // Medium Fondo rival (seeds as F2), real DefaultPreset. Observation, not a gate: stop and report if fewer than 5 % of the human
    // receptions offer a volley. A second pass with a bot that takes every offered volley measures its outcome; nothing is tuned.
    public sealed class VolleyRateExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private sealed class Tally {public int points,human,offered,rivalReceptions,rivalOffered,volleys,volleyContacts,faults,rejected;public double humanSlackAfterVolley,humanSlackAfterBounce;public int afterVolley,afterBounce;}
        private static Tally Run(RallyContext ctx,bool take)
        {
            var t=new Tally();
            for(int i=0;t.points<200;i++)
                using(var sim=new RallySim(ctx,new BotBrain(ctx,30,20261117+2*i,takeVolley:take),new FondoAi(ctx,AiDifficulty.For(ctx.Params,AiLevel.Medium),20261118+2*i)))
                {
                    var score=new MatchScore(7,2,i%2==0?PlayerId.Human:PlayerId.Rival);int replays=0;
                    while(!score.Finished&&t.points<200)
                    {
                        var o=sim.PlayPoint(score.Server,score.Box);t.points++;t.faults+=o.SystemFaults;t.rejected+=o.RejectedContacts;
                        for(int k=0;k<o.Receptions.Count;k++)
                        {
                            var r=o.Receptions[k];
                            if(r.Who==PlayerId.Human){t.human++;if(r.VolleyOffered)t.offered++;if(r.Volley){t.volleys++;if(r.Result==ReceptionResult.Contact)t.volleyContacts++;}}
                            else
                            {
                                t.rivalReceptions++;if(r.VolleyOffered)t.rivalOffered++;
                                // The rival's slack right after a human contact: volley versus bounce (R5, observation).
                                if(k>0&&o.Receptions[k-1].Who==PlayerId.Human&&o.Receptions[k-1].Result==ReceptionResult.Contact)
                                {if(o.Receptions[k-1].Volley){t.humanSlackAfterVolley+=r.SlackS;t.afterVolley++;}else{t.humanSlackAfterBounce+=r.SlackS;t.afterBounce++;}}
                            }
                        }
                        if(o.Winner.HasValue)score.PointWon(o.Winner.Value);else if(++replays>50)break;
                    }
                }
            return t;
        }
        private static string Line(string name,Tally t)=>string.Join("\t",new[]{name,t.points.ToString(C),t.human.ToString(C),t.offered.ToString(C),(t.human==0?0:t.offered/(double)t.human).ToString("R",C),
            t.rivalReceptions.ToString(C),t.rivalOffered.ToString(C),t.volleys.ToString(C),t.volleyContacts.ToString(C),t.faults.ToString(C),t.rejected.ToString(C),
            (t.afterVolley==0?double.NaN:t.humanSlackAfterVolley/t.afterVolley).ToString("R",C),t.afterVolley.ToString(C),(t.afterBounce==0?double.NaN:t.humanSlackAfterBounce/t.afterBounce).ToString("R",C),t.afterBounce.ToString(C)});
        [Test,Explicit,Category("Experiments"),Timeout(3600000)]
        public void MeasureOfferedRate()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));string dir=Path.Combine(root,"Logs/experiments/volley-rate");
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence");
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash,Is.EqualTo("2ec0c953786a"));
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),new AnalyticBallSimulationFactory(Physics.bounceThreshold));
            Assert.That(ctx.Config.SimulationKey,Is.EqualTo("166a96e319c7"));
            var seen=Run(ctx,false);var taken=Run(ctx,true);
            Directory.CreateDirectory(dir);
            string text="variant\tpoints\thumanReceptions\toffered\trate\trivalReceptions\trivalOffered\tvolleys\tvolleyContacts\tsystemFaults\trejected\trivalSlackAfterVolley\tn\trivalSlackAfterBounce\tn\n"+
                Line("bot-never",seen)+"\n"+Line("bot-takes",taken)+"\n";
            File.WriteAllText(Path.Combine(dir,"rate.tsv"),"rallyParams\t"+ctx.Params.Hash+"\n"+text);TestContext.WriteLine(text);
            Assert.That(seen.faults+taken.faults,Is.Zero);Assert.That(seen.rejected+taken.rejected,Is.Zero);
        }
    }
}
