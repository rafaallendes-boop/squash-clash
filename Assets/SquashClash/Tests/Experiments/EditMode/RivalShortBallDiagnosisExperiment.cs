using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Experiments.EditMode
{
    // Explicit diagnosis of the rival's answer to short balls (design docs/superpowers/specs/2026-11-22-rival-short-ball-design.md, section 2).
    // Read-only: defaults and RallyParams untouched. Not part of regression or AutoQA.
    public sealed class RivalShortBallDiagnosisExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        private const int Matches=150;private const long Seeds=20261122;

        // Human stand-in: 40 % drop, 15 % low straight, 45 % long like NewcomerBot. sigma 90 ms.
        public sealed class ShortMixBot : IRallyBrain
        {
            private readonly RallyContext ctx;private readonly RallyRng random;private readonly double sigma;
            public ShortMixBot(RallyContext ctx,double sigma,long seed){this.ctx=ctx;this.sigma=sigma;random=new RallyRng(seed);}
            public int PerceptionSteps=>0;
            public Vector2 StanceNoise()=>Vector2.zero;
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                if(!own.Reachable||!own.Contact.Exists||own.Contact.TStarStep<0)return default;
                double u=random.NextDouble();ShotLine line;ShotHeight height;ShotPower power;
                if(u<.40){line=ShotLine.Drop;height=(ShotHeight)Math.Min(2,(int)(random.NextDouble()*3));power=random.NextDouble()<.7?ShotPower.Medium:ShotPower.Strong;}
                else if(u<.55){line=ShotLine.Straight;height=ShotHeight.Low;power=random.NextDouble()<.5?ShotPower.Medium:ShotPower.Strong;}
                else{line=random.NextDouble()<.5?ShotLine.Straight:ShotLine.Cross;height=ShotHeight.Mid;power=random.NextDouble()<.6?ShotPower.Strong:ShotPower.Medium;}
                double delta=random.NextGaussian(sigma);
                var options=ShotOptions.Build(ctx,own.Contact.PStar);
                int D(ShotOption x)=>(x.Line==line?0:x.Line==ShotLine.Drop||line==ShotLine.Drop?8:4)+2*Math.Abs((int)x.Height-(int)height)+(x.Power==power?0:1);
                options.Sort((a,b)=>D(a).CompareTo(D(b)));
                foreach(var x in options)if(RallyContact.Forecast(ctx,own.Contact.PStar,x.Aim,x.P).Good)return new AiShot(true,x,x.Aim,x.P,delta,false);
                return new AiShot(true,options[0],options[0].Aim,options[0].P,delta,false);
            }
        }

        private sealed class Seen
        {
            public PlayerId Who;public BallState Ball;public ReceptionPlan Plan;public AiShot Shot;
            public double DelayMs,FromWait,PathM,ShortM;public Vector2 OpponentAtStart;
            public string Fb="NA";
        }

        // Wraps a brain and keeps the rival's positioning and serve (RallySim reads them through the interfaces).
        private sealed class Recorder : IRallyBrain,IRallyPositioning,IRallyServer
        {
            private readonly IRallyBrain inner;private readonly PlayerId who;private readonly RallyContext ctx;public readonly List<Seen> Sink;
            public RallySim Sim;
            public Recorder(IRallyBrain inner,PlayerId who,RallyContext ctx,List<Seen> sink){this.inner=inner;this.who=who;this.ctx=ctx;Sink=sink;}
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public Vector2 Wait=>inner is IRallyPositioning p?p.Wait:ctx.WaitPosition(who);
            public Vector2 Recovery(in AiObservation a)=>inner is IRallyPositioning p?p.Recovery(a):ctx.WaitPosition(who);
            public ServeType ChooseServe(ServiceBox box)=>inner is IRallyServer s?s.ChooseServe(box):ServeType.Lob;
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                var shot=inner.Decide(o,own);
                var s=new Seen{Who=who,Ball=o.Ball,Plan=own,Shot=shot,OpponentAtStart=o.OpponentPosition};
                // Reflection keeps this observer compatible with the incorporated main base.
                var fb=inner.GetType().GetProperty("LastStretchedFallback");if(fb!=null)s.Fb=(bool)fb.GetValue(inner)?"1":"0";
                if(who==PlayerId.Rival)
                {
                    var body=Sim.Rival;var energy=Sim.RivalEnergy.Energy;
                    int extra=FatigueEffects.For(ctx.Fatigue.Factor(energy),ctx.Fatigue).ExtraSteps;
                    s.DelayMs=(ctx.Steps(ctx.Params.ReactionS)+inner.PerceptionSteps+extra)*1000.0/ctx.StepHz;
                    s.FromWait=Vector2.Distance(body.Position,Wait);
                    // MoveTo already ran in BeginReception: this is the body's real path until its own stroke.
                    var t=ctx.Predict(o.Ball);var path=body.PositionsAfter(t.States.Count);long first=t.States[0].Step;
                    long b1=-1,b2=-1;foreach(var c in t.Contacts)if(c.Surface==SurfaceId.Floor){if(b1<0)b1=c.Step;else{b2=c.Step;break;}}
                    long end=shot.Swing&&own.Contact.Exists&&own.Contact.TStarStep>=0?own.Contact.TStarStep:(b2>=0?b2:first+t.States.Count-1);
                    double len=0;for(long j=first+1;j<=end&&j-first<path.Length;j++)len+=Vector2.Distance(path[j-first],path[j-first-1]);
                    s.PathM=len;
                    var frame=ctx.FrameAt(body.Position);double best=double.PositiveInfinity;
                    if(b1>=0)for(long j=b1;j<=(b2>=0?b2:end)&&j-first<path.Length;j++)
                    {var q=t.States[(int)(j-first)].Position-new Vector3(path[j-first].x,frame.ReferenceHeight,path[j-first].y);best=Math.Min(best,q.magnitude-frame.Reach);}
                    s.ShortM=best;
                }
                Sink.Add(s);return shot;
            }
        }

        private static string F(double v)=>double.IsNaN(v)||double.IsInfinity(v)?"NA":v.ToString("0.###",C);

        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void WriteShortBallDiagnosis()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),factory);
            string dir=Path.Combine(root,"Logs/experiments/rival-short-ball/"+(Environment.GetEnvironmentVariable("SC_RSB_RUN")??"run01"));
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"source.tsv"),"defaults\trallyParams\tunity\n"+preset.Hash+"\t"+ctx.Params.Hash+"\t"+Application.unityVersion+"\n",Utf8);
            float tz=ctx.T.y,tin=ctx.Court.TinTop;
            int extraMissingLogs=0,roleMismatches=0;
            using(var w=new StreamWriter(Path.Combine(dir,"rows.tsv"),false,Utf8))
            using(var pw=new StreamWriter(Path.Combine(dir,"points.tsv"),false,Utf8))
            {
                pw.WriteLine("style\tlevel\tmatch\tpoint\twinner");
                w.WriteLine("style\tlevel\tmatch\tpoint\tj\tkind\tzone\th_line\th_height\th_power\th_z\tfw_tin\tb1z\tb2z\tavail_ms\tapex2\treason\tslack\tdelay_ms\tfrom_wait\tpath_m\tshort_m\tswing\tresult\tdef\tline\theight\tpower\tvolley\tret_fw\tret_b2z\tnext\twon\tr_z\tfb");
                foreach(RivalStyleId style in new[]{RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo})
                foreach(AiLevel level in new[]{AiLevel.Easy,AiLevel.Medium,AiLevel.Hard})
                for(int m=0;m<Matches;m++)
                {
                    var seen=new List<Seen>();
                    var human=new Recorder(new ShortMixBot(ctx,90,Seeds+2*m),PlayerId.Human,ctx,seen);
                    var rival=new Recorder(RivalAi.For(ctx,level,style,Seeds+1+2*m),PlayerId.Rival,ctx,seen);
                    using(var sim=new RallySim(ctx,human,rival))
                    {
                        human.Sim=rival.Sim=sim;
                        // PlayMatch's own loop (RallySim.cs:145-157), point by point, so the recorder's sink is cleared per point:
                        // within a point, Decide k and log k belong to the same reception (RallySim.cs:213-248); a pending contact
                        // cut by the close leaves one Decide without a log, which only the last index can be.
                        var score=new MatchScore((int)ctx.Params.MatchTarget,(int)ctx.Params.MatchMargin,PlayerId.Human);int replays=0;
                        for(int pt=0;!score.Finished;pt++)
                        {
                            seen.Clear();var point=sim.PlayPoint(score.Server,score.Box);var logs=point.Receptions;var pointSeen=seen;
                            int used=pointSeen.Count;
                            if(used>logs.Count+1)extraMissingLogs+=used-logs.Count-1;
                            for(int k=0;k<Math.Min(used,logs.Count);k++)if(pointSeen[k].Who!=logs[k].Who)roleMismatches++;
                            for(int j=1;j<used;j++)
                            {
                                var s=pointSeen[j];if(s.Who!=PlayerId.Rival||j-1>=logs.Count||logs[j-1].Who!=PlayerId.Human)continue;
                                var hl=logs[j-1];var t=ctx.Predict(s.Ball);
                                float fw=float.NaN,b1z=float.NaN,b2z=float.NaN;long b1=-1,b2=-1;
                                foreach(var c in t.Contacts)
                                {
                                    if(c.Surface==SurfaceId.FrontWall&&float.IsNaN(fw))fw=c.Point.y;
                                    else if(c.Surface==SurfaceId.Floor){if(b1<0){b1=c.Step;b1z=c.Point.z;}else{b2=c.Step;b2z=c.Point.z;break;}}
                                }
                                double apex=double.NaN;if(b1>=0){apex=0;foreach(var st in t.States)if(st.Step>b1&&(b2<0||st.Step<b2))apex=Math.Max(apex,st.Position.y);}
                                long first=t.States[0].Step;double avail=b2>=0?(b2-first)*1000.0/ctx.StepHz:double.NaN;
                                string kind=hl.Line==ShotLine.Drop?"dejada":!float.IsNaN(b2z)&&b2z<tz?"corto":"largo";
                                string zone=hl.ContactPoint.z<tz?"adelante":"fondo";
                                string result=j<logs.Count?logs[j].Result.ToString():"none";
                                double retFw=double.NaN,retB2=double.NaN;string next="none";bool won=false;
                                if(j+1<pointSeen.Count&&result=="Contact")
                                {
                                    var rt=ctx.Predict(pointSeen[j+1].Ball);int floors=0;
                                    foreach(var c in rt.Contacts){if(c.Surface==SurfaceId.FrontWall&&double.IsNaN(retFw))retFw=c.Point.y;else if(c.Surface==SurfaceId.Floor&&++floors==2){retB2=c.Point.z;break;}}
                                    next=j+1<logs.Count?logs[j+1].Result.ToString():"none";
                                    won=point.Winner==PlayerId.Rival&&j+1==logs.Count-1;
                                }
                                var sh=s.Shot;
                                w.WriteLine(string.Join("\t",new[]{RivalStyle.Name(style),level.ToString(),m.ToString(C),pt.ToString(C),j.ToString(C),kind,zone,
                                    hl.Line.ToString(),hl.Height.ToString(),hl.Power.ToString(),F(hl.ContactPoint.z),F(fw-tin),F(b1z),F(b2z),F(avail),F(apex),
                                    s.Plan.Reason,F(s.Plan.SlackS),F(s.DelayMs),F(s.FromWait),F(s.PathM),F(s.ShortM),sh.Swing?"1":"0",result,
                                    sh.Defensive?"1":"0",sh.Swing?sh.Option.Line.ToString():"-",sh.Swing?sh.Option.Height.ToString():"-",sh.Swing?sh.Option.Power.ToString():"-",sh.Volley?"1":"0",
                                    F(retFw),F(retB2),next,won?"1":"0",
                                    result=="Contact"?F(logs[j].ContactPoint.z):"NA",s.Fb}));  // r_z, fb (camino B, F2)
                            }
                            pw.WriteLine(string.Join("\t",RivalStyle.Name(style),level.ToString(),m.ToString(C),pt.ToString(C),point.Winner.HasValue?point.Winner.Value.ToString():"none"));
                            if(point.Winner.HasValue)score.PointWon(point.Winner.Value);
                            else if(++replays>50)break;
                        }
                    }
                }
            }
            File.WriteAllText(Path.Combine(dir,"alignment.tsv"),"extraMissingLogs\troleMismatches\n"+extraMissingLogs+"\t"+roleMismatches+"\n",Utf8);
            Assert.Pass("rows written: "+dir);
        }
    }
}
