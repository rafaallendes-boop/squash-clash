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
    // Difficulty calibration for the rival styles (design 2026-11-19 section 5; targets fixed by Claude on 19 nov BEFORE measuring):
    // newcomer point share Easy 60 %, Medium 40 %, Hard 20 %; average of the three styles within +-4 pp, every style within +-8 pp;
    // at Hard, bot30 <= 45 % for every style. Only the level's five execution/perception noises move, by one factor k from
    // {1, 1.25, ..., 3}; temperature stays. Search: the smallest k whose average reaches target - 4 pp (share grows with k), then the
    // first k from there that meets every constraint; if the average passes target + 4 pp first, that level stops ("no-k").
    // Confirm: one run with new seeds at the chosen k, every cell with the newcomer and bot30, plus the secondary check with a newcomer
    // sigma that gives about 86 % contact (no tuning on it). 200 matches per cell. Never overwrites evidence.
    public sealed class RivalStylesCalibrationExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        private static readonly double[] Grid={1,1.25,1.5,1.75,2,2.25,2.5,2.75,3};
        private static readonly AiLevel[] Levels={AiLevel.Easy,AiLevel.Medium,AiLevel.Hard};
        private static readonly RivalStyleId[] Styles={RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo};
        private static double Target(AiLevel l)=>l==AiLevel.Easy?.60:l==AiLevel.Medium?.40:.20;
        private static string F(double v)=>v.ToString("R",C);
        private sealed class Cell
        {
            public int Points,Human,Receptions,Contacts,RivalContacts,RivalLong,RivalAttack,RivalVolleys,Faults,RivalBoasts,RivalBoastFallbacks;
            public double Share=>Points==0?0:Human/(double)Points;
            public string Row=>string.Join("\t",new[]{Points,Human,Receptions,Contacts,RivalContacts,RivalLong,RivalAttack,RivalVolleys,Faults}.Select(x=>x.ToString(C)))+"\t"+F(Share);
            public const string Header="points\thuman\thumanReceptions\thumanContacts\trivalContacts\trivalLong\trivalAttack\trivalVolleys\tsystemFaults\tshare";
        }
        public static void Scale(RallyParams p,AiLevel level,double k)
        {
            switch(level)
            {
                case AiLevel.Easy:p.EasyPerceptionS*=k;p.EasyStanceSigma*=k;p.EasyAimSigma*=k;p.EasyPowerSigma*=k;p.EasyTimingSigmaMs*=k;break;
                case AiLevel.Medium:p.MediumPerceptionS*=k;p.MediumStanceSigma*=k;p.MediumAimSigma*=k;p.MediumPowerSigma*=k;p.MediumTimingSigmaMs*=k;break;
                default:p.HardPerceptionS*=k;p.HardStanceSigma*=k;p.HardAimSigma*=k;p.HardPowerSigma*=k;p.HardTimingSigmaMs*=k;break;
            }
        }
        private static (string root,ParamSet preset,AnalyticBallSimulationFactory factory) Setup()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash,Is.EqualTo("207da8f2c636"));Assert.That(preset.Ordered.Count,Is.EqualTo(132));
            return (root,preset,new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
        }
        private static Cell Play(RallyContext ctx,AiLevel level,RivalStyleId style,string reference,double sigma,int matches,long seeds,AiDifficulty? difficulty=null,double earlyAllowanceMs=0)
        {
            var cell=new Cell();
            for(int i=0;i<matches;i++)
            {
                IRallyBrain human=reference=="bot30"?(IRallyBrain)new BotBrain(ctx,30,seeds+2*i):new RivalStylesBaselineExperiment.NewcomerBot(ctx,sigma,seeds+2*i);
                using(var sim=new RallySim(ctx,human,new RivalAi(ctx,difficulty??AiDifficulty.For(ctx.Params,level),style,seeds+1+2*i),earlyAllowanceMs))
                {
                    var score=new MatchScore(7,2,i%2==0?PlayerId.Human:PlayerId.Rival);int replays=0;
                    while(!score.Finished)
                    {
                        var o=sim.PlayPoint(score.Server,score.Box);cell.Faults+=o.SystemFaults;
                        foreach(var r in o.Receptions)
                        {
                            if(r.Who==PlayerId.Human){cell.Receptions++;if(r.Result==ReceptionResult.Contact)cell.Contacts++;continue;}
                            if(r.Result!=ReceptionResult.Contact)continue;
                            cell.RivalContacts++;if(r.Line!=ShotLine.Drop&&r.Height!=ShotHeight.Low)cell.RivalLong++;
                            if(r.Line==ShotLine.Drop||r.Height==ShotHeight.Low)cell.RivalAttack++;if(r.Volley)cell.RivalVolleys++;
                            if(r.Line==ShotLine.Boast)cell.RivalBoasts++;if(r.BoastFellBack)cell.RivalBoastFallbacks++;
                        }
                        if(o.Winner.HasValue){score.PointWon(o.Winner.Value);cell.Points++;if(o.Winner==PlayerId.Human)cell.Human++;}
                        else if(++replays>50)break;
                    }
                }
            }
            return cell;
        }
        private static RallyContext Context(ParamSet preset,AnalyticBallSimulationFactory factory,AiLevel level,double k)
        {var p=new RallyParams();Scale(p,level,k);return new RallyContext(EffectiveConfig.From(preset),p,factory);}
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void Calibrate()
        {
            var (root,preset,factory)=Setup();
            string dir=Path.Combine(root,"Logs/experiments/rival-styles-calibration/"+(Environment.GetEnvironmentVariable("SC_RS_RUN")??"search"));
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"source.tsv"),"defaults\trallyParams\tunity\n"+preset.Hash+"\t"+new RallyParams().Hash+"\t"+Application.unityVersion+"\n",Utf8);
            const long seeds=20261121;const int matches=200;
            using(var grid=new StreamWriter(Path.Combine(dir,"grid.tsv"),false,Utf8))
            using(var chosen=new StreamWriter(Path.Combine(dir,"chosen.tsv"),false,Utf8))
            {
                grid.WriteLine("level\tk\tstyle\treference\t"+Cell.Header+"\tseconds");chosen.WriteLine("level\tk\taverage\tverdict");
                foreach(var level in Levels)
                {
                    var cache=new Dictionary<int,Dictionary<string,Cell>>();
                    Dictionary<string,Cell> Eval(int index)
                    {
                        if(cache.TryGetValue(index,out var done))return done;
                        var ctx=Context(preset,factory,level,Grid[index]);var cells=new Dictionary<string,Cell>();
                        foreach(var style in Styles)foreach(var reference in level==AiLevel.Hard?new[]{"newcomer","bot30"}:new[]{"newcomer"})
                        {
                            var watch=Stopwatch.StartNew();var cell=Play(ctx,level,style,reference,90,matches,seeds);cells[style+"/"+reference]=cell;
                            grid.WriteLine(level+"\t"+F(Grid[index])+"\t"+style+"\t"+reference+"\t"+cell.Row+"\t"+F(watch.Elapsed.TotalSeconds));grid.Flush();
                        }
                        return cache[index]=cells;
                    }
                    double Average(int index)=>Styles.Average(s=>Eval(index)[s+"/newcomer"].Share);
                    double target=Target(level);
                    int lo=0,hi=Grid.Length-1;
                    if(Average(hi)<target-.04){chosen.WriteLine(level+"\t\t"+F(Average(hi))+"\tno-k (k=3 too weak)");chosen.Flush();continue;}
                    while(lo<hi){int mid=(lo+hi)/2;if(Average(mid)>=target-.04)hi=mid;else lo=mid+1;}
                    string verdict="no-k";int pick=-1;
                    for(int i=lo;i<Grid.Length;i++)
                    {
                        double avg=Average(i);if(avg>target+.04){verdict="no-k (average over target+4 pp at k="+F(Grid[i])+")";break;}
                        bool styles=Styles.All(s=>Math.Abs(Eval(i)[s+"/newcomer"].Share-target)<=.08);
                        bool hard=level!=AiLevel.Hard||Styles.All(s=>Eval(i)[s+"/bot30"].Share<=.45);
                        if(styles&&hard){pick=i;verdict="ok";break;}
                    }
                    chosen.WriteLine(level+"\t"+(pick>=0?F(Grid[pick]):"")+"\t"+(pick>=0?F(Average(pick)):"")+"\t"+verdict);chosen.Flush();
                    TestContext.WriteLine("level "+level+" k="+(pick>=0?F(Grid[pick]):"-")+" "+verdict);
                }
            }
        }
        // Claude, 19 nov (after search02): one k per (style, level), read from RallyParams. Fondista Hard first: k=2.5 on the search
        // seeds, then 2.25 and 2.75 only if 2.5 is outside 20 +- 4 pp; the one in band closest to 20 % is used. Then the confirmation
        // with NEW seeds for the nine cells (newcomer) and bot30 in every cell (Hard: <= 45 %; Easy and Medium reported only).
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void ConfirmPerStyle()=>ConfirmPerStyle("confirm-per-style",new RallyParams());
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void ConfirmBoundedServeMargin()
        {
            double margin=double.Parse(Environment.GetEnvironmentVariable("SC_MARGIN_MS")??"0",C);
            Assert.That(new[]{0.0,25,50},Does.Contain(margin));
            ConfirmPerStyle("bounded-"+margin,new RallyParams(),earlyAllowanceMs:margin);
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            string run=Environment.GetEnvironmentVariable("SC_RS_RUN")??"bounded-"+margin;
            File.AppendAllText(Path.Combine(root,"Logs/experiments/rival-styles-calibration",run,"source.tsv"),"earlyAllowanceMs\t"+F(margin)+"\n",Utf8);
        }
        // Serve kinds (design 2026-11-20-serve, revision 2): the same run, same seeds, with «Saque nuevo» on, to measure the calibration shift.
        // The reference bots have no serve choice, so they serve the lob; the rival chooses by its style. Nothing is recalibrated.
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void ConfirmPerStyleNewServe()=>ConfirmPerStyle("confirm-new-serve",new RallyParams{ServeKindOn=1});
        // Two-wall shot (design 2026-11-20-boast-predeclaration, M4): the same run, same seeds, with the rival's boast off (identity with
        // confirm01) and on. A newcomer share that moves more than 3 pp at any difficulty leaves «Dos paredes» off by default; nothing is
        // recalibrated. Named Boast* so the ConfirmPerStyle filter never picks them up.
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void BoastConfirmOff()=>ConfirmPerStyle("confirm-boast-off",new RallyParams{BoastRival=0});
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void BoastConfirmOn()=>ConfirmPerStyle("confirm-boast-on",new RallyParams{BoastRival=1});
        // Addendum 1 (round two): the same pair with NEW seeds (round one: 20261121 / 20261219). The boast counts are printed apart, so the
        // cells.tsv columns stay those of confirm01.
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void BoastRound2Off()=>ConfirmPerStyle("confirm-boast-r2-off",new RallyParams{BoastRival=0},20270121,20270219);
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void BoastRound2On()=>ConfirmPerStyle("confirm-boast-r2-on",new RallyParams{BoastRival=1},20270121,20270219);
        private static void ConfirmPerStyle(string defaultRun,RallyParams parameters,long hardSeeds=20261121,long confirmSeeds=20261219,double earlyAllowanceMs=0)
        {
            var (root,preset,factory)=Setup();
            string dir=Path.Combine(root,"Logs/experiments/rival-styles-calibration/"+(Environment.GetEnvironmentVariable("SC_RS_RUN")??defaultRun));
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");Directory.CreateDirectory(dir);
            var ctx=new RallyContext(EffectiveConfig.From(preset),parameters,factory);
            File.WriteAllText(Path.Combine(dir,"source.tsv"),"defaults\trallyParams\tunity\n"+preset.Hash+"\t"+ctx.Params.Hash+"\t"+Application.unityVersion+"\n",Utf8);
            using(var cells=new StreamWriter(Path.Combine(dir,"cells.tsv"),false,Utf8))
            using(var boasts=new StreamWriter(Path.Combine(dir,"boasts.tsv"),false,Utf8))
            {
                cells.WriteLine("phase\tlevel\tstyle\treference\tk\t"+Cell.Header+"\tseconds");boasts.WriteLine("phase\tlevel\tstyle\treference\trivalBoasts\trivalBoastFallbacks");
                AiDifficulty Scaled(AiLevel level,double k){var d=AiDifficulty.For(ctx.Params,level);return new AiDifficulty(d.PerceptionS*k,d.StanceSigma*k,d.AimSigma*k,d.PowerSigma*k,d.TimingSigmaMs*k,d.Temperature);}
                Cell Run(string phase,AiLevel level,RivalStyleId style,string reference,double k,long seeds)
                {
                    var watch=Stopwatch.StartNew();var cell=Play(ctx,level,style,reference,90,200,seeds,Scaled(level,k),earlyAllowanceMs);
                    cells.WriteLine(phase+"\t"+level+"\t"+style+"\t"+reference+"\t"+F(k)+"\t"+cell.Row+"\t"+F(watch.Elapsed.TotalSeconds));cells.Flush();
                    boasts.WriteLine(phase+"\t"+level+"\t"+style+"\t"+reference+"\t"+cell.RivalBoasts+"\t"+cell.RivalBoastFallbacks);boasts.Flush();UnityEngine.Debug.Log("N2 "+phase+" "+level+" "+style+" "+reference+" margin="+earlyAllowanceMs+" points="+cell.Points+" human="+cell.Human);return cell;
                }
                double fondoHard=double.NaN,best=double.MaxValue;
                foreach(var k in new[]{2.5,2.25,2.75})
                {
                    double share=Run("fondista-hard",AiLevel.Hard,RivalStyleId.Fondista,"newcomer",k,hardSeeds).Share;
                    if(Math.Abs(share-.20)<=.04&&Math.Abs(share-.20)<best){best=Math.Abs(share-.20);fondoHard=k;}
                    if(k==2.5&&!double.IsNaN(fondoHard))break;
                }
                File.WriteAllText(Path.Combine(dir,"fondista-hard.tsv"),"k\n"+(double.IsNaN(fondoHard)?"none":F(fondoHard))+"\n",Utf8);
                foreach(var level in Levels)foreach(var style in Styles)
                {
                    double k=style==RivalStyleId.Fondista&&level==AiLevel.Hard?(double.IsNaN(fondoHard)?RivalStyle.NoiseK(ctx.Params,style,level):fondoHard):RivalStyle.NoiseK(ctx.Params,style,level);
                    Run("confirm",level,style,"newcomer",k,confirmSeeds);Run("confirm",level,style,"bot30",k,confirmSeeds);
                }
            }
        }
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void Confirm()
        {
            var (root,preset,factory)=Setup();
            var k=(Environment.GetEnvironmentVariable("SC_RS_K")??"").Split(',').Select(x=>double.Parse(x,C)).ToArray();
            Assert.That(k.Length,Is.EqualTo(3),"SC_RS_K=easy,medium,hard");
            string dir=Path.Combine(root,"Logs/experiments/rival-styles-calibration/"+(Environment.GetEnvironmentVariable("SC_RS_RUN")??"confirm"));
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");Directory.CreateDirectory(dir);
            var p=new RallyParams();for(int i=0;i<3;i++)Scale(p,Levels[i],k[i]);
            var ctx=new RallyContext(EffectiveConfig.From(preset),p,factory);
            File.WriteAllText(Path.Combine(dir,"source.tsv"),"defaults\trallyParams\tk\tunity\n"+preset.Hash+"\t"+p.Hash+"\t"+string.Join(",",k.Select(F))+"\t"+Application.unityVersion+"\n",Utf8);
            const long seeds=20261219;const int matches=200;
            using(var cells=new StreamWriter(Path.Combine(dir,"cells.tsv"),false,Utf8))
            {
                cells.WriteLine("level\tstyle\treference\tsigma\t"+Cell.Header+"\tseconds");
                void Run(AiLevel level,RivalStyleId style,string reference,double sigma,int n)
                {
                    var watch=Stopwatch.StartNew();var cell=Play(ctx,level,style,reference,sigma,n,seeds);
                    cells.WriteLine(level+"\t"+style+"\t"+reference+"\t"+F(sigma)+"\t"+cell.Row+"\t"+F(watch.Elapsed.TotalSeconds));cells.Flush();
                }
                foreach(var level in Levels)foreach(var style in Styles){Run(level,style,"newcomer",90,matches);Run(level,style,"bot30",30,matches);}
                // Secondary check (no tuning): the newcomer sigma, from a fixed list, whose contact rate at Medium against the Fondista is
                // closest to 86 % (real PO, T3 builds), then every cell with it.
                double best=90,gap=double.MaxValue;
                foreach(var sigma in new[]{150.0,200,250,300})
                {
                    var probe=Play(ctx,AiLevel.Medium,RivalStyleId.Fondista,"newcomer",sigma,50,seeds+999);
                    double rate=probe.Contacts/(double)Math.Max(1,probe.Receptions);
                    cells.WriteLine("probe\tFondista\tnewcomer\t"+F(sigma)+"\t"+probe.Row+"\t0");cells.Flush();
                    if(Math.Abs(rate-.86)<gap){gap=Math.Abs(rate-.86);best=sigma;}
                }
                foreach(var level in Levels)foreach(var style in Styles)Run(level,style,"newcomer86",best,matches);
            }
        }
    }
}
