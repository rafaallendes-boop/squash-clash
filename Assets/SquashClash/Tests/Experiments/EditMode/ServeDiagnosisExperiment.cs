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
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    // Serve, phase 1 (design docs/superpowers/specs/2026-11-20-serve-design.md, section 2). Explicit, outside regression and AutoQA.
    // Nothing in production changes. Three parts, all on the default preset and RallyParams:
    //  1. today's automatic serve, one trajectory per box (the same for both servers);
    //  2. RallySim at Medium against the three rival styles (newcomer sigma 90 ms as the human): the serve return of both receivers,
    //     the human's v2.2 return-of-serve volley offer (counterfactual: the newcomer never touches), serve faults and how fast the server wins;
    //  3. a counterfactual grid of candidate serves (front-wall aim x, height, horizontal speed) judged by the serve rules, each with one
    //     isolated reception from the receivers' wait positions: reach, window, sampled P+V (newcomer sigma 90; rival styles at Medium with
    //     their own release rule) and the human's volley offer. The grid only describes the design space; the shapes are chosen later by
    //     the rule predeclared in the design (section 4). Never overwrites evidence.
    public sealed class ServeDiagnosisExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        private const double NewcomerSigmaMs=90;
        private static string F(double v)=>double.IsNaN(v)?"NaN":double.IsInfinity(v)?(v>0?"inf":"-inf"):v.ToString("0.####",C);
        private static readonly RivalStyleId[] Styles={RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo};
        internal static (string root,ParamSet preset,AnalyticBallSimulationFactory factory) Setup()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash,Is.EqualTo("207da8f2c636"));Assert.That(preset.Ordered.Count,Is.EqualTo(132));
            return (root,preset,new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
        }
        // Shape of one serve flight up to its second floor bounce.
        internal struct Shape
        {
            public float FrontX,FrontY,SideZ,SideY,BounceX,BounceZ,BackY,Bounce2Z,PassY,Speed,HSpeed,BounceHSpeed;public double FrontMs,BounceMs;public bool SideBefore,BackBefore2;public string Exit;
            public const string Header="frontX\tfrontY\tfrontMs\tsideBefore\tsideZ\tsideY\tbounceX\tbounceZ\tbounceMs\tbounceHSpeed\tbackY\tbounce2Z\tpassYAtWaitZ\tspeed\thSpeed\texit";
            public string Row=>string.Join("\t",new[]{F(FrontX),F(FrontY),F(FrontMs),SideBefore?"1":"0",F(SideZ),F(SideY),F(BounceX),F(BounceZ),F(BounceMs),F(BounceHSpeed),F(BackY),F(Bounce2Z),F(PassY),F(Speed),F(HSpeed),Exit});
        }
        internal static Shape Describe(RallyContext ctx,in BallState serve,Trajectory t,float waitZ)
        {
            var s=new Shape{FrontX=float.NaN,FrontY=float.NaN,SideZ=float.NaN,SideY=float.NaN,BounceX=float.NaN,BounceZ=float.NaN,BackY=float.NaN,Bounce2Z=float.NaN,PassY=float.NaN,FrontMs=double.NaN,BounceMs=double.NaN,BounceHSpeed=float.NaN};
            s.Speed=serve.Velocity.magnitude;s.HSpeed=new Vector2(serve.Velocity.x,serve.Velocity.z).magnitude;s.Exit=t.Exit.ToString();
            long first=t.States[0].Step;double perMs=ctx.StepHz*ctx.Tempo/1000;bool front=false;int floors=0;
            foreach(var c in t.Contacts)
            {
                if(c.Persistent)continue;
                if(!front&&c.Surface==SurfaceId.FrontWall){front=true;s.FrontX=c.Point.x;s.FrontY=c.Point.y;s.FrontMs=(c.Step-first)/perMs;continue;}
                if(c.Surface==SurfaceId.Floor)
                {
                    floors++;
                    if(floors==1){s.BounceX=c.Point.x;s.BounceZ=c.Point.z;s.BounceMs=(c.Step-first)/perMs;int i=(int)(c.Step-first);if(i>=0&&i<t.States.Count){var v=t.States[i].Velocity;s.BounceHSpeed=new Vector2(v.x,v.z).magnitude;}}
                    else if(floors==2){s.Bounce2Z=c.Point.z;}
                    continue;
                }
                if(front&&floors==0&&(c.Surface==SurfaceId.LeftWall||c.Surface==SurfaceId.RightWall)&&!s.SideBefore){s.SideBefore=true;s.SideZ=c.Point.z;s.SideY=c.Point.y;}
                if(c.Surface==SurfaceId.BackWall&&floors<2&&float.IsNaN(s.BackY)){s.BackY=c.Point.y;s.BackBefore2=true;}
            }
            // Height of the ball when it first passes the receivers' waiting depth after the front wall.
            bool afterFront=false;
            for(int i=1;i<t.States.Count;i++)
            {
                var a=t.States[i-1].Position;var b=t.States[i].Position;
                if(!afterFront){if(t.States[i].Velocity.z>0&&a.z<1f)afterFront=true;else continue;}
                if(a.z<waitZ&&b.z>=waitZ){s.PassY=b.y;break;}
            }
            return s;
        }
        // One isolated reception of a serve: the receiver at its wait position, its own plan, sampled releases through the real contract.
        internal struct Reception
        {
            public bool Reachable,Bounce,Offer;public double KMs,SlackS,Move,LeadMs,PV,Perfect;public string Reason;
            public const string Header="reachable\treason\tkMs\tleadMs\tslackS\tmoveM\tpv\tperfect\tvolleyOffer";
            public string Row=>string.Join("\t",new[]{Reachable?"1":"0",Reason??"",F(KMs),F(LeadMs),F(SlackS),F(Move),F(PV),F(Perfect),Offer?"1":"0"});
        }
        internal static Reception Receive(RallyContext ctx,in BallState serve,Trajectory t,PlayerId server,ServiceBox box,PlayerId receiver,RivalStyleId style,int samples,RallyRng random)
        {
            var rules=ctx.NewRules();rules.BeginServe(server,box);
            var rival=receiver==PlayerId.Rival?RivalAi.For(ctx,AiLevel.Medium,style,7):null;
            var wait=rival!=null?rival.Wait:ctx.WaitPosition(receiver);
            var body=new PlayerBody(receiver,wait,ctx.Move);
            int delay=ctx.Steps(ctx.Params.ReactionS)+(rival!=null?rival.PerceptionSteps:0);
            var plan=ReceptionPlanner.Plan(ctx,t,rules,body.Clone(),delay,1,default);
            var r=new Reception{Reachable=plan.Reachable,Reason=plan.Reason,SlackS=plan.SlackS,Move=Vector2.Distance(plan.Stance,wait),KMs=double.NaN,LeadMs=double.NaN,PV=double.NaN,Perfect=double.NaN};
            long first=t.States[0].Step;double perMs=ctx.StepHz*ctx.Tempo/1000;
            r.Bounce=plan.Reachable&&plan.Contact.Exists&&plan.Contact.TStarStep>=0;
            // As RallySim does: the body already runs to its bounce stance when the offer is computed.
            if(receiver==PlayerId.Human){var moving=body.Clone();moving.MoveTo(plan.Stance,delay,BodyPhase.Approaching,BodyPhase.Set);r.Offer=HumanVolley.OfferDetail(ctx,t,rules,moving,delay,1).Offer.Exists;}
            if(!r.Bounce){r.PV=0;r.Perfect=0;return r;}
            var c=plan.Contact;r.KMs=(c.KOut.Steps-c.KIn.Steps)/perMs;r.LeadMs=(c.TStarStep-first)/perMs;
            double now=first/(ctx.Tempo*ctx.StepHz);int pv=0,perfect=0;
            var difficulty=rival!=null?RivalStyle.Difficulty(ctx.Params,style,AiLevel.Medium):default;
            double rate=ctx.StepHz*ctx.Tempo,lower=(c.KIn.Steps-c.TStarStep)*1000/rate,upper=(c.KOut.Steps-c.TStarStep)*1000/rate;
            if(ctx.Timing.ValidLimitOn){lower=Math.Max(lower,-ctx.Timing.ValidMs);upper=Math.Min(upper,ctx.Timing.ValidMs);}
            for(int i=0;i<samples;i++)
            {
                double delta=rival!=null?RivalReleaseTiming.Adjust(random.NextGaussian(difficulty.TimingSigmaMs),lower,upper,difficulty.TimingSigmaMs,ctx.Params.FondoReleaseMarginSigma):random.NextGaussian(NewcomerSigmaMs);
                var clock=new ClockMap();clock.Begin(1,now,first,ctx.Tempo,1,ctx.StepHz);
                var d=RallyContact.Resolve(ctx,clock,t,c,now,delta);
                if(d.Result==ShotResult.Perfect||d.Result==ShotResult.Valid)pv++;if(d.Result==ShotResult.Perfect)perfect++;
            }
            r.PV=pv/(double)samples;r.Perfect=perfect/(double)samples;return r;
        }
        // Records, at the start of every serve reception, what the receiver had and (for the human) the v2.2 offer.
        private sealed class Probe : IRallyBrain,IRallyPositioning
        {
            private readonly IRallyBrain inner;private readonly RallyContext ctx;
            public RallySim Sim;public PlayerId Who;
            public bool Seen;public ReceptionPlan Plan;public Vector2 Start;public bool Offer;
            public Probe(RallyContext ctx,IRallyBrain inner,PlayerId who){this.ctx=ctx;this.inner=inner;Who=who;}
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public Vector2 Wait=>inner is IRallyPositioning p?p.Wait:ctx.WaitPosition(Who);
            public Vector2 Recovery(in AiObservation o)=>inner is IRallyPositioning p?p.Recovery(o):ctx.WaitPosition(Who);
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                if(Sim!=null&&Sim.Rules.Serving&&o.Shots==0)
                {
                    Seen=true;Plan=own;Start=o.SelfPosition;Offer=false;
                    if(Who==PlayerId.Human)
                    {
                        var t=ctx.Predict(o.Ball);int delay=ctx.Steps(ctx.Params.ReactionS)+inner.PerceptionSteps+FatigueEffects.For(ctx.Fatigue.Factor(Sim.HumanEnergy.Energy),ctx.Fatigue).ExtraSteps;
                        Offer=HumanVolley.OfferDetail(ctx,t,Sim.Rules,Sim.Human,delay,own.Contact.Version,ctx.Fatigue.Factor(Sim.HumanEnergy.Energy)).Offer.Exists;
                    }
                }
                return inner.Decide(o,own);
            }
        }
        internal const string PointHeader="style\tmatch\tpoint\tserver\tbox\twinner\tcause\tshots\treceiverResult\treceiverPerfect\treachable\tslackS\tmoveM\tvolleyOffer\tserveType";
        // One RallySim cell at Medium: per point, who served, the serve return and the end. serveType is read from the outcome when the
        // product records it, otherwise "auto".
        internal static void PlayCell(RallyContext ctx,RivalStyleId style,int matches,long seeds,StreamWriter points,Func<RallyOutcome,string> serveType=null)
        {
            for(int m=0;m<matches;m++)
            {
                var human=new Probe(ctx,new RivalStylesBaselineExperiment.NewcomerBot(ctx,NewcomerSigmaMs,seeds+2*m),PlayerId.Human);
                var rival=new Probe(ctx,RivalAi.For(ctx,AiLevel.Medium,style,seeds+1+2*m),PlayerId.Rival);
                using(var sim=new RallySim(ctx,human,rival))
                {
                    human.Sim=rival.Sim=sim;var score=new MatchScore(7,2,m%2==0?PlayerId.Human:PlayerId.Rival);int replays=0,n=0;
                    while(!score.Finished)
                    {
                        human.Seen=rival.Seen=false;
                        var o=sim.PlayPoint(score.Server,score.Box);
                        var receiver=o.Server.Value.Other();var probe=receiver==PlayerId.Human?human:rival;
                        var first=o.Receptions.Count>0&&o.Receptions[0].Who==receiver?o.Receptions[0]:default;
                        bool has=o.Receptions.Count>0&&o.Receptions[0].Who==receiver;
                        points.WriteLine(string.Join("\t",new[]{style.ToString(),m.ToString(C),(n++).ToString(C),o.Server.ToString(),o.Box.ToString(),o.Winner?.ToString()??"",o.Cause.ToString(),o.Shots.ToString(C),
                            has?first.Result.ToString():"None",has&&first.Perfect?"1":"0",probe.Seen&&probe.Plan.Reachable?"1":"0",probe.Seen?F(probe.Plan.SlackS):"NaN",
                            probe.Seen?F(Vector2.Distance(probe.Plan.Stance,probe.Start)):"NaN",probe.Seen&&probe.Offer?"1":"0",serveType?.Invoke(o)??"auto"}));
                        if(o.Winner.HasValue)score.PointWon(o.Winner.Value);else if(++replays>50)break;
                    }
                }
            }
            points.Flush();
        }
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void Diagnose()
        {
            var (root,preset,factory)=Setup();
            string dir=Path.Combine(root,"Logs/experiments/serve/"+(Environment.GetEnvironmentVariable("SC_SERVE_RUN")??"diag01"));
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");Directory.CreateDirectory(dir);
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),factory);
            File.WriteAllText(Path.Combine(dir,"source.tsv"),"defaults\trallyParams\tsimulationKey\tstepHz\ttempo\tunity\n"+preset.Hash+"\t"+ctx.Params.Hash+"\t"+ctx.Config.SimulationKey+"\t"+ctx.StepHz+"\t"+F(ctx.Tempo)+"\t"+Application.unityVersion+"\n",Utf8);
            float waitZ=ctx.WaitPosition(PlayerId.Human).y;
            // 1. Today's serve, per box.
            using(var w=new StreamWriter(Path.Combine(dir,"today-shape.tsv"),false,Utf8))
            {
                w.WriteLine("box\tfromX\tfromZ\t"+Shape.Header+"\treceiver\tstyle\t"+Reception.Header);
                foreach(ServiceBox box in Enum.GetValues(typeof(ServiceBox)))
                {
                    var serve=ctx.Serve(box);var t=ctx.Predict(serve);var shape=Describe(ctx,serve,t,waitZ);
                    var random=new RallyRng(20261120+(int)box);
                    w.WriteLine(box+"\t"+F(serve.Position.x)+"\t"+F(serve.Position.z)+"\t"+shape.Row+"\tHuman\t-\t"+Receive(ctx,serve,t,PlayerId.Rival,box,PlayerId.Human,RivalStyleId.Fondista,2000,random).Row);
                    foreach(var style in Styles)
                        w.WriteLine(box+"\t"+F(serve.Position.x)+"\t"+F(serve.Position.z)+"\t"+shape.Row+"\tRival\t"+style+"\t"+Receive(ctx,serve,t,PlayerId.Human,box,PlayerId.Rival,style,2000,random).Row);
                }
            }
            // 2. RallySim, Medium, three styles, 200 matches each.
            using(var points=new StreamWriter(Path.Combine(dir,"points.tsv"),false,Utf8))
            {
                points.WriteLine(PointHeader);
                foreach(var style in Styles)PlayCell(ctx,style,200,20261120+1000*(int)style,points);
            }
            // 3. Candidate grid (describes the design space only).
            var watch=Stopwatch.StartNew();int legal=0,total=0;
            using(var g=new StreamWriter(Path.Combine(dir,"grid.tsv"),false,Utf8))
            {
                g.Write("box\taimX\taimY\thSpeedIn\t"+Shape.Header+"\th."+Reception.Header.Replace("\t","\th."));
                foreach(var style in Styles)g.Write("\t"+style+"."+Reception.Header.Replace("\t","\t"+style+"."));
                g.WriteLine();
                foreach(ServiceBox box in Enum.GetValues(typeof(ServiceBox)))
                {
                    var at=ServePlanner.BoxPosition(ctx,box);var from=new Vector3(at.x,(float)ctx.Params.ServeHandHeight,at.y);
                    for(int ix=0;ix<=16;ix++)for(int iy=0;iy<=17;iy++)for(int iv=0;iv<=14;iv++)
                    {
                        float aimX=box==ServiceBox.Right?1.6f+.2f*ix:ctx.Court.Width-1.6f-.2f*ix,aimY=1.95f+.15f*iy,speed=8+iv;total++;
                        var center=Ballistics.CenterForContact(SurfaceId.FrontWall,new Vector3(aimX,aimY,0),ctx.Physics.BallRadius);
                        var serve=new BallState{Position=from,Velocity=Ballistics.SolveCenterTo(from,center,speed,ctx.Physics.Gravity)};
                        if(!ServePlanner.Forecast(ctx,serve,box,out _))continue;
                        legal++;var t=ctx.Predict(serve);var shape=Describe(ctx,serve,t,waitZ);
                        var random=new RallyRng(20261120L*31+total);
                        // Human serves -> the rival receives (styles); rival serves -> the human receives. Same box, same ball.
                        var line=new StringBuilder();
                        line.Append(box).Append('\t').Append(F(aimX)).Append('\t').Append(F(aimY)).Append('\t').Append(F(speed)).Append('\t').Append(shape.Row);
                        line.Append('\t').Append(Receive(ctx,serve,t,PlayerId.Rival,box,PlayerId.Human,RivalStyleId.Fondista,300,random).Row);
                        foreach(var style in Styles)line.Append('\t').Append(Receive(ctx,serve,t,PlayerId.Human,box,PlayerId.Rival,style,150,random).Row);
                        g.WriteLine(line.ToString());
                    }
                }
            }
            File.AppendAllText(Path.Combine(dir,"source.tsv"),"grid\t"+total+"\tlegal\t"+legal+"\tseconds\t"+F(watch.Elapsed.TotalSeconds)+"\n",Utf8);
            TestContext.WriteLine("grid "+total+" legal "+legal+" in "+watch.Elapsed.TotalSeconds+" s");
        }
    }
}
