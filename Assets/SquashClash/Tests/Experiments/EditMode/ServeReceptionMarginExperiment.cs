using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    // Explicit measurement outside regression and AutoQA; never merged. Original brains, seeds and match loop.
    // Disposable measurement only. M1 uses the original std newcomer and serve seeds, with a lob human serve.
    // The touch cohort uses the original options wrapper and independent 50/50 human serve stream. Never overwrites evidence.
    public sealed class ServeReceptionMarginExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        private static readonly RivalStyleId[] Styles={RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo};
        private static string Env(string k,string d){var v=Environment.GetEnvironmentVariable(k);return string.IsNullOrWhiteSpace(v)?d:v;}
        private static string[] List(string k,string d)=>Env(k,d).Split(',').Select(s=>s.Trim()).Where(s=>s.Length>0).ToArray();
        private const string Header="variant\thuman\tlevel\tstyle\tmatch\tpoint\tserver\tbox\tserveType\twinner\tcause\tshots\treceiverResult\treceiverPerfect\treceiverVolley\treceiverVolleyOffered\thumanContacts\thumanVolleyContacts\tmatchHuman\tmatchRival";
        // Forwards exactly as ServeDiagnosisExperiment.Probe (without recording); Mix makes a brain without its own choice serve lob/drive 50/50.
        private class Fwd : IRallyBrain,IRallyPositioning,IRallyServer
        {
            public RallyRng Mix;protected readonly IRallyBrain inner;private readonly RallyContext ctx;private readonly PlayerId who;
            public Fwd(RallyContext ctx,IRallyBrain inner,PlayerId who){this.ctx=ctx;this.inner=inner;this.who=who;}
            public ServeType ChooseServe(ServiceBox box)=>inner is IRallyServer s?s.ChooseServe(box):Mix!=null?(Mix.NextDouble()<.5?ServeType.Drive:ServeType.Lob):ServeType.Lob;
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public Vector2 Wait=>inner is IRallyPositioning p?p.Wait:ctx.WaitPosition(who);
            public Vector2 Recovery(in AiObservation o)=>inner is IRallyPositioning p?p.Recovery(o):ctx.WaitPosition(who);
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)=>inner.Decide(o,own);
        }
        // Seen as a toucher by RallySim only when this wrapper is used, i.e. when the inner brain is one.
        private sealed class FwdTouch : Fwd,IRallyVolleyToucher
        {
            public FwdTouch(RallyContext ctx,IRallyBrain inner,PlayerId who):base(ctx,inner,who){}
            public double TouchMsAfterLaunch(in ReceptionPlan own,double bounceMs)=>((IRallyVolleyToucher)inner).TouchMsAfterLaunch(own,bounceMs);
        }
        private static Fwd Wrap(RallyContext ctx,IRallyBrain b,PlayerId who)=>b is IRallyVolleyToucher?new FwdTouch(ctx,b,who):new Fwd(ctx,b,who);
        private static void PlayCell(RallyContext ctx,string variant,string humanKind,AiLevel level,RivalStyleId style,int matches,StreamWriter points,double allowance)
        {
            long seeds=20261120+1000*(int)style;
            for(int m=0;m<matches;m++)
            {
                long humanSeed=seeds+2*m;
                IRallyBrain hb=humanKind=="touch"?(IRallyBrain)new VolleyEnergyExperiment.Bot(ctx,humanSeed,false):new RivalStylesBaselineExperiment.NewcomerBot(ctx,90,humanSeed);
                var human=Wrap(ctx,hb,PlayerId.Human);if(humanKind=="touch")human.Mix=new RallyRng(seeds*31+m);
                var rival=new Fwd(ctx,RivalAi.For(ctx,level,style,seeds+1+2*m),PlayerId.Rival);
                using(var sim=new RallySim(ctx,human,rival,allowance))
                {
                    var score=new MatchScore(7,2,m%2==0?PlayerId.Human:PlayerId.Rival);int replays=0,n=0;
                    while(!score.Finished)
                    {
                        var o=sim.PlayPoint(score.Server,score.Box);
                        var receiver=o.Server.Value.Other();
                        bool has=o.Receptions.Count>0&&o.Receptions[0].Who==receiver;var first=has?o.Receptions[0]:default;
                        int hc=o.Receptions.Count(r=>r.Who==PlayerId.Human&&r.Result==ReceptionResult.Contact),hv=o.Receptions.Count(r=>r.Who==PlayerId.Human&&r.Result==ReceptionResult.Contact&&r.Volley);
                        if(o.Winner.HasValue)score.PointWon(o.Winner.Value);
                        points.WriteLine(string.Join("\t",new[]{variant,humanKind,level.ToString(),style.ToString(),m.ToString(C),(n++).ToString(C),o.Server.ToString(),o.Box.ToString(),o.ServeType.ToString(),o.Winner?.ToString()??"",o.Cause.ToString(),o.Shots.ToString(C),
                            has?first.Result.ToString():"None",has&&first.Perfect?"1":"0",has&&first.Volley?"1":"0",has&&first.VolleyOffered?"1":"0",hc.ToString(C),hv.ToString(C),score.Human.ToString(C),score.Rival.ToString(C)}));
                        if(!o.Winner.HasValue&&++replays>50)break;
                    }
                    Assert.That(score.Finished,Is.True,"A partial match must never count as a measurement.");
                }
            }
            points.Flush();
        }
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void MeasureBoundedMargin()
        {
            var (root,preset,factory)=ServeDiagnosisExperiment.Setup();
            string dir=Path.Combine(root,"Logs/experiments/serve-margin/"+Env("SC_MARGIN_RUN","options01"));
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");Directory.CreateDirectory(dir);
            const int stdMatches=200,touchMatches=60;
            var levels=List("SC_LEVELS","Easy,Medium,Hard").Select(s=>(AiLevel)Enum.Parse(typeof(AiLevel),s)).ToArray();
            var variants=List("SC_VARIANTS","base,early25,early50");var humans=List("SC_HUMANS","std,touch");
            var watch=Stopwatch.StartNew();
            var src=new StringBuilder("variant\tdefaults\trallyParams\tsimulationKey\tallowanceMs\tunity\tstdMatches\ttouchMatches\tlevels\thumans\n");
            using(var points=new StreamWriter(Path.Combine(dir,"points.tsv"),false,Utf8))
            {
                points.WriteLine(Header);
                foreach(var variant in variants)
                {
                    var p=preset;var rp=new RallyParams();
                    double allowance=variant=="base"?0:variant=="early25"?25:variant=="early50"?50:throw new ArgumentException(variant);
                    var ctx=new RallyContext(EffectiveConfig.From(p),rp,factory);
                    Assert.That(ctx.Params.Hash,Is.EqualTo("1063d0f6cf66"));Assert.That(ctx.Config.SimulationKey,Is.EqualTo("166a96e319c7"));
                    src.Append(variant+"\t"+p.Hash+"\t"+ctx.Params.Hash+"\t"+ctx.Config.SimulationKey+"\t"+allowance.ToString(C)+"\t"+Application.unityVersion+"\t"+stdMatches+"\t"+touchMatches+"\t"+string.Join(",",levels)+"\t"+string.Join(",",humans)+"\n");
                    foreach(var human in humans)foreach(var level in levels)foreach(var style in Styles)
                    {
                        double t0=watch.Elapsed.TotalSeconds;PlayCell(ctx,variant,human,level,style,human=="std"?stdMatches:touchMatches,points,allowance);
                        UnityEngine.Debug.Log(variant+" "+human+" "+level+" "+style+" cell "+(watch.Elapsed.TotalSeconds-t0).ToString("0.#",C)+" s, total "+watch.Elapsed.TotalSeconds.ToString("0.#",C)+" s");
                    }
                }
            }
            src.Append("elapsed\t"+watch.Elapsed.TotalSeconds.ToString("0.###",C)+"\n");
            File.WriteAllText(Path.Combine(dir,"source.tsv"),src.ToString(),Utf8);
        }
    }
}
