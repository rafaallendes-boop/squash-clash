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
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    // Rival styles, phase 1 baseline (design docs/superpowers/specs/2026-11-19-rival-styles-design.md, section 5 and 4).
    // Explicit, outside regression and AutoQA. Product code is not touched: the current Fondo AI per level against two
    // reference opponents, plus two wait-position variants of the current rival for the short-ball positioning question.
    // SC_RS_MATCHES (default 200) matches per cell; SC_RS_RUN names the evidence folder. Never overwrites evidence.
    public sealed class RivalStylesBaselineExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        private static string F(double v)=>v.ToString("R",C);
        // Newcomer reference candidate: no tactics (never looks at the opponent), wide release spread. The menu is the plain
        // drive a first-time player reaches for: straight or cross, mid height, strong 60 % (real-play median power 0,83).
        public sealed class NewcomerBot : IRallyBrain
        {
            private readonly RallyContext context;private readonly RallyRng random;private readonly double timingSigmaMs;
            public NewcomerBot(RallyContext context,double timingSigmaMs,long seed){this.context=context;this.timingSigmaMs=timingSigmaMs;random=new RallyRng(seed);}
            public int PerceptionSteps=>0;
            public Vector2 StanceNoise()=>Vector2.zero;
            public AiShot Decide(in AiObservation observation,in ReceptionPlan own)
            {
                if(!own.Reachable||!own.Contact.Exists||own.Contact.TStarStep<0)return default;
                var line=random.NextDouble()<.5?ShotLine.Straight:ShotLine.Cross;
                var power=random.NextDouble()<.6?ShotPower.Strong:ShotPower.Medium;
                double delta=random.NextGaussian(timingSigmaMs);
                var options=ShotOptions.Build(context,own.Contact.PStar);
                int Distance(ShotOption o)=>(o.Line==line?0:o.Line==ShotLine.Drop?8:4)+2*Math.Abs((int)o.Height-(int)ShotHeight.Mid)+(o.Power==power?0:1);
                options.Sort((a,b)=>Distance(a).CompareTo(Distance(b)));
                foreach(var option in options)
                    if(RallyContact.Forecast(context,own.Contact.PStar,option.Aim,option.P).Good)return new AiShot(true,option,option.Aim,option.P,delta,false);
                return new AiShot(true,options[0],options[0].Aim,options[0].P,delta,false);
            }
        }
        // Records what each receiver saw at the start of its reception (observable state and its own plan only).
        private sealed class Recorder : IRallyBrain
        {
            public readonly List<string> Rows=new List<string>();
            private readonly IRallyBrain inner;private readonly RallyContext ctx;
            public Recorder(IRallyBrain inner,RallyContext ctx){this.inner=inner;this.ctx=ctx;}
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                var t=ctx.Predict(o.Ball);float bx=float.NaN,bz=float.NaN;
                foreach(var c in t.Contacts)if(c.Surface==SurfaceId.Floor){bx=c.Point.x;bz=c.Point.z;break;}
                Rows.Add(string.Join("\t",new[]{F(o.Ball.Position.x),F(o.Ball.Position.z),F(o.Ball.Position.y),F(bx),F(bz),F(o.SelfPosition.x),F(o.SelfPosition.y),F(o.OpponentPosition.x),F(o.OpponentPosition.y),
                    own.Reachable?"1":"0",F(own.SlackS),own.Reason,own.Contact.Exists?F(own.Contact.PStar.z):"",o.Shots.ToString(C)}));
                return inner.Decide(o,own);
            }
        }
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void WriteBaseline()
        {
            string run=Environment.GetEnvironmentVariable("SC_RS_RUN")??"probe";
            int matches=int.Parse(Environment.GetEnvironmentVariable("SC_RS_MATCHES")??"200",C);
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            string dir=Path.Combine(root,"Logs/experiments/rival-styles-baseline/"+run);
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash,Is.EqualTo("207da8f2c636"));Assert.That(preset.Ordered.Count,Is.EqualTo(132));
            var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var baseCtx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),factory);
            Assert.That(baseCtx.Config.SimulationKey,Is.EqualTo("166a96e319c7"));Assert.That(baseCtx.Params.Hash,Is.EqualTo("5cdbb01e4621"));
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"source.tsv"),"defaults\tkeys\tsimulationKey\trallyParams\tunity\tmatches\n"+preset.Hash+"\t"+preset.Ordered.Count+"\t"+baseCtx.Config.SimulationKey+"\t"+baseCtx.Params.Hash+"\t"+Application.unityVersion+"\t"+matches+"\n",Utf8);
            // Cells fixed before data: reference x level with the current rival, then the wait-position variants at Medium.
            // Feasibility (scale): the six Easy noise parameters multiplied, to see whether the targets are reachable with them alone.
            var cells=new List<(string reference,AiLevel level,double waitBehind,double scale)>();
            foreach(var reference in new[]{"bot30","newcomer"})foreach(var level in new[]{AiLevel.Easy,AiLevel.Medium,AiLevel.Hard})cells.Add((reference,level,.5,1));
            cells.Add(("newcomer",AiLevel.Medium,0,1));cells.Add(("newcomer",AiLevel.Medium,-.5,1));
            cells.Add(("newcomer",AiLevel.Easy,.5,1.5));cells.Add(("newcomer",AiLevel.Easy,.5,2));
            using(var pointsOut=new StreamWriter(Path.Combine(dir,"points.tsv"),false,Utf8))
            using(var recOut=new StreamWriter(Path.Combine(dir,"receptions.tsv"),false,Utf8))
            using(var cellsOut=new StreamWriter(Path.Combine(dir,"cells.tsv"),false,Utf8))
            using(var faultsOut=new StreamWriter(Path.Combine(dir,"faults.tsv"),false,Utf8))
            {
                pointsOut.WriteLine("reference\tlevel\twait\tmatch\tpoint\tserver\twinner\tcause\tshots\trealSeconds\tlastReception");
                recOut.WriteLine("reference\tlevel\twait\tmatch\tpoint\tk\twho\tresult\tline\theight\tpower\tdefensive\tvolleyOffered\tdeltaMs\tstrikerX\tstrikerZ\tstrikerY\tbounceX\tbounceZ\tselfX\tselfZ\toppX\toppZ\treachable\tslack\treason\tpStarZ\tshots");
                faultsOut.WriteLine("reference\tlevel\twait\tmatch\tpoint\twho\treason\tline\theight\tpower\tsystemFaults\trejected\twinner");
                cellsOut.WriteLine("reference\tlevel\twait\tmatches\tpoints\thumanWins\trivalWins\tmatchesHuman\tseconds");
                foreach(var cell in cells)
                {
                    var p=new RallyParams();p.RivalWaitBehindT=cell.waitBehind;
                    if(cell.scale!=1){p.EasyPerceptionS*=cell.scale;p.EasyStanceSigma*=cell.scale;p.EasyAimSigma*=cell.scale;p.EasyPowerSigma*=cell.scale;p.EasyTimingSigmaMs*=cell.scale;p.EasyTemperature*=cell.scale;}
                    var ctx=cell.waitBehind==.5&&cell.scale==1?baseCtx:new RallyContext(EffectiveConfig.From(preset),p,factory);
                    var watch=Stopwatch.StartNew();int points=0,hw=0,rw=0,mh=0;
                    string tag=cell.reference+"\t"+cell.level+(cell.scale==1?"":"x"+F(cell.scale))+"\t"+F(cell.waitBehind);
                    for(int i=0;i<matches;i++)
                    {
                        IRallyBrain human=cell.reference=="bot30"?(IRallyBrain)new BotBrain(ctx,30,20261119+2*i):new NewcomerBot(ctx,90,20261119+2*i);
                        var hr=new Recorder(human,ctx);var rr=new Recorder(new FondoAi(ctx,AiDifficulty.For(ctx.Params,cell.level),20261120+2*i),ctx);
                        using(var sim=new RallySim(ctx,hr,rr))
                        {
                            var score=new MatchScore(7,2,i%2==0?PlayerId.Human:PlayerId.Rival);int point=0,replays=0;
                            while(!score.Finished)
                            {
                                hr.Rows.Clear();rr.Rows.Clear();
                                var o=sim.PlayPoint(score.Server,score.Box);
                                // A system fault is recorded, never hidden (run01 stopped on one: bot30 Easy, match 43, point 10).
                                if(o.SystemFaults>0||o.RejectedContacts>0)foreach(var f in o.Receptions.Where(x=>x.Result==ReceptionResult.SystemFault))
                                    faultsOut.WriteLine(string.Join("\t",new[]{tag,i.ToString(C),point.ToString(C),f.Who.ToString(),f.Reason??"",f.Line.ToString(),f.Height.ToString(),f.Power.ToString(),o.SystemFaults.ToString(C),o.RejectedContacts.ToString(C),o.Winner?.ToString()??""}));
                                pointsOut.WriteLine(string.Join("\t",new[]{tag,i.ToString(C),point.ToString(C),score.Server.ToString(),o.Winner?.ToString()??"",o.Cause.ToString(),o.Shots.ToString(C),F(o.RealSeconds),o.LastReception?.ToString()??""}));
                                int h=0,r=0;
                                for(int k=0;k<o.Receptions.Count;k++)
                                {
                                    var x=o.Receptions[k];var rows=x.Who==PlayerId.Human?hr.Rows:rr.Rows;int idx=x.Who==PlayerId.Human?h++:r++;
                                    string seen=idx<rows.Count?rows[idx]:string.Join("\t",Enumerable.Repeat("",14));
                                    recOut.WriteLine(string.Join("\t",new[]{tag,i.ToString(C),point.ToString(C),k.ToString(C),x.Who.ToString(),x.Result.ToString(),x.Line.ToString(),x.Height.ToString(),x.Power.ToString(),x.Defensive?"1":"0",x.VolleyOffered?"1":"0",F(x.DeltaMs),seen}));
                                }
                                Assert.That(h,Is.EqualTo(hr.Rows.Count),"human receptions align");Assert.That(r,Is.EqualTo(rr.Rows.Count),"rival receptions align");
                                points++;
                                if(o.Winner.HasValue){score.PointWon(o.Winner.Value);if(o.Winner==PlayerId.Human)hw++;else rw++;}
                                else if(++replays>50)break;
                                point++;
                            }
                            if(score.Winner==PlayerId.Human)mh++;
                        }
                    }
                    cellsOut.WriteLine(string.Join("\t",new[]{tag,matches.ToString(C),points.ToString(C),hw.ToString(C),rw.ToString(C),mh.ToString(C),F(watch.Elapsed.TotalSeconds)}));
                    cellsOut.Flush();faultsOut.Flush();pointsOut.Flush();recOut.Flush();
                    TestContext.WriteLine("cell "+tag.Replace('\t',' ')+" points="+points+" human="+hw+" rival="+rw+" s="+watch.Elapsed.TotalSeconds.ToString("F1",C));
                }
            }
        }
    }
}
