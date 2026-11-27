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
    // High-and-long rival defence (spec 2026-11-26-defensa-alta-larga): the success metrics, predeclared. A scripted human with a fixed
    // intention (the 26 nov "fondo gana" harness) against the real RivalAi. Per cell: human point share, and after every rival contact
    // with Defensive set, the first bounce of that ball and the human's slack on it (the human's next reception). Never overwrites evidence.
    public sealed class DeepDefenseExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        private static string F(double v)=>v.ToString("R",C);
        private sealed class IntentBot : IRallyBrain
        {
            private readonly RallyContext ctx;private readonly RallyRng random;private readonly double sigma;
            private readonly ShotLine line;private readonly ShotHeight height;private readonly ShotPower power;private readonly float? pOverride;
            public IntentBot(RallyContext ctx,double sigma,long seed,ShotLine line,ShotHeight height,ShotPower power,float? p)
            {this.ctx=ctx;this.sigma=sigma;random=new RallyRng(seed);this.line=line;this.height=height;this.power=power;pOverride=p;}
            public int PerceptionSteps=>0;
            public Vector2 StanceNoise()=>Vector2.zero;
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                if(!own.Reachable||!own.Contact.Exists||own.Contact.TStarStep<0)return default;
                var at=own.Contact.PStar;var options=ShotOptions.Build(ctx,at);
                options.Sort((a,b)=>Distance(a).CompareTo(Distance(b)));
                double delta=random.NextGaussian(sigma);
                foreach(var option in options)
                {
                    float p=option.Line==line&&pOverride.HasValue?pOverride.Value:option.P;
                    if(RallyContact.Forecast(ctx,at,option.Aim,p).Good)return new AiShot(true,option,option.Aim,p,delta,false);
                }
                return new AiShot(true,options[0],options[0].Aim,options[0].P,delta,false);
            }
            private int Distance(ShotOption o)=>(o.Line==line?0:o.Line==ShotLine.Drop||line==ShotLine.Drop?8:4)+2*Math.Abs((int)o.Height-(int)height)+(o.Power==power?0:1);
        }
        // What a receiver sees when it decides: the incoming ball's own first bounce (z), glass on the fly before it, and its plan slack.
        private class Tap : IRallyBrain
        {
            public readonly List<(double bz,bool glass,double slack)> Rows=new List<(double,bool,double)>();public int Lunges;
            private readonly IRallyBrain inner;private readonly RallyContext ctx;
            public Tap(IRallyBrain inner,RallyContext ctx){this.inner=inner;this.ctx=ctx;}
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public virtual AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                var (bz,glass)=Bounce(ctx,o.Ball);Rows.Add((bz,glass,own.SlackS));if(own.Reason=="lunge")Lunges++;
                return inner.Decide(o,own);
            }
        }
        // set15 rival time: what the rival had at the human's contact (t0), copied without touching the sim (its body, rules and energy).
        private sealed class Snap
        {
            public Trajectory Trajectory;public PointRules Rules;public PlayerBody Body;public int React,Perception,Extra;public ReceptionPlan Plan;
        }
        private sealed class RivalTap : Tap,IRallyPositioning,IRallyServer
        {
            private readonly RivalAi rival;private readonly RallyContext context;
            public RallySim Sim;public bool Snapping;public readonly List<Snap> Snaps=new List<Snap>();
            public RivalTap(RivalAi rival,RallyContext ctx):base(rival,ctx){this.rival=rival;context=ctx;}
            public override AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                if(Snapping)Snaps.Add(new Snap{Trajectory=context.Predict(o.Ball),Rules=Sim.Rules.Clone(),Body=Sim.Rival.Clone(),React=context.Steps(context.Params.ReactionS),
                    Perception=rival.PerceptionSteps,Extra=FatigueEffects.For(context.Fatigue.Factor(Sim.RivalEnergy.Energy),context.Fatigue).ExtraSteps,Plan=own});
                else Snaps.Add(null);
                return base.Decide(o,own);
            }
            public Vector2 Wait=>rival.Wait;
            public Vector2 Recovery(in AiObservation a)=>rival.Recovery(a);
            public ServeType ChooseServe(ServiceBox box)=>rival.ChooseServe(box);
        }
        private static (double z,bool glass) Bounce(RallyContext ctx,BallState ball)
        {
            bool front=false,glass=false;
            foreach(var c in ctx.Predict(ball).Contacts)
            {
                if(c.Persistent)continue;
                if(c.Surface==SurfaceId.FrontWall){front=true;continue;}
                if(!front)continue;
                if(c.Surface==SurfaceId.Floor)return (c.Point.z,glass);
                if(c.Surface==SurfaceId.BackWall)glass=true;
            }
            return (double.NaN,glass);
        }
        // set15: the planner's own candidates (legal window, interval, both sides, no stance noise); late = arrival - steps until T* of that
        // stance, minimised over the candidates. int.MaxValue: no candidate arrives within the limit (censored). Same PlayerBody code.
        private struct Late{public int Steps;public int Until;public float Distance;public Vector2 Stance;}
        private static Late Deficit(RallyContext ctx,Trajectory trajectory,PointRules rulesAtStart,PlayerBody body,int delay)
        {
            var best=new Late{Steps=int.MaxValue,Until=-1,Distance=float.NaN};
            if(trajectory.States.Count<2)return best;
            var frame=ctx.FrameAt(body.Position);var legal=ctx.Legal;var court=ctx.Court;
            long start=trajectory.States[0].Step;var who=rulesAtStart.ToHit;var rules=rulesAtStart.Clone();int next=0;
            var window=new List<(long step,Vector3 at,float feature)>(256);
            for(int i=1;i<trajectory.States.Count&&!rules.Closed;i++)
            {
                var state=trajectory.States[i];
                while(next<trajectory.Contacts.Count&&trajectory.Contacts[next].Step<=state.Step)rules.OnBallContact(trajectory.Contacts[next++]);
                if(rules.Closed||!rules.CanHitAfterBounce(who)||state.Position.y<legal.MinHeight||state.Position.y>legal.MaxHeight)continue;
                window.Add((state.Step,state.Position,Feature(frame,court,state.Position)));
            }
            int interval=Math.Max(1,ctx.Steps(ctx.Params.CandidateIntervalS));float reach2=frame.Reach*frame.Reach;
            for(int index=0;index<window.Count;index++)
            {
                if((window[index].step-start)%interval!=0)continue;
                foreach(float side in new[]{-1f,1f})
                {
                    var stance=ctx.ClampBody(new Vector2(window[index].at.x+side*frame.IdealLateral,window[index].at.z));
                    bool In(int k){var d=window[k].at-new Vector3(stance.x,frame.ReferenceHeight,stance.y);return d.sqrMagnitude<=reach2;}
                    if(!In(index))continue;
                    int first=index,last=index;
                    while(first>0&&window[first-1].step==window[first].step-1&&In(first-1))first--;
                    while(last+1<window.Count&&window[last+1].step==window[last].step+1&&In(last+1))last++;
                    int star=first;for(int k=first+1;k<=last;k++)if(window[k].feature>window[star].feature)star=k;
                    int until=(int)(window[star].step-start),available=(int)(window[index].step-start);
                    int arrival=body.StepsToReach(stance,delay,out _,Math.Max(until,available)+2*ctx.StepHz);
                    if(arrival==int.MaxValue)continue;
                    int late=arrival-until;
                    if(late<best.Steps)best=new Late{Steps=late,Until=until,Distance=Vector2.Distance(body.Position,stance),Stance=stance};
                }
            }
            return best;
        }
        private static float Height(BodyFrame frame,Vector3 ball)
        {
            float gap=ball.y<frame.PreferredMinHeight?frame.PreferredMinHeight-ball.y:ball.y>frame.PreferredMaxHeight?ball.y-frame.PreferredMaxHeight:0;
            return Mathf.Max(0,1-gap/(frame.PreferredMaxHeight-frame.PreferredMinHeight));
        }
        private static float Wall(BodyFrame frame,CourtDimensions court,Vector3 ball)
        {
            float clearance=Mathf.Min(Mathf.Min(ball.x,court.Width-ball.x),Mathf.Min(ball.z,court.Length-ball.z));
            return frame.WallClearance<=0?1:Mathf.Clamp01(clearance/frame.WallClearance);
        }
        private static float Feature(BodyFrame frame,CourtDimensions court,Vector3 ball)=>Height(frame,ball)+Wall(frame,court,ball);
        private static PlayerBody AtRest(RallyContext ctx,PlayerBody from,bool instant)
        {
            var m=ctx.Move;if(instant)m=new MoveSettings(1e5f,m.MaxSpeed,m.Brake,m.ArriveEpsilon,m.StepHz);
            return new PlayerBody(PlayerId.Rival,from.Position,m){MovementFactor=from.MovementFactor};
        }
        private static string S(RallyContext ctx,Late l)=>l.Steps==int.MaxValue?"inf":F(l.Steps/(double)ctx.StepHz);
        private static string TimeRow(RallyContext ctx,Snap x)
        {
            int full=x.React+x.Perception+x.Extra;var b=x.Body;
            var lBase=Deficit(ctx,x.Trajectory,x.Rules,b,full);
            var lNoReact=Deficit(ctx,x.Trajectory,x.Rules,b,x.Perception+x.Extra);
            var lNoPerc=Deficit(ctx,x.Trajectory,x.Rules,b,x.React+x.Extra);
            var lNoDelay=Deficit(ctx,x.Trajectory,x.Rules,b,0);
            var lRest=Deficit(ctx,x.Trajectory,x.Rules,AtRest(ctx,b,false),full);
            var lRestInst=Deficit(ctx,x.Trajectory,x.Rules,AtRest(ctx,b,true),full);
            var lCore=Deficit(ctx,x.Trajectory,x.Rules,AtRest(ctx,b,true),0);
            var noiseless=ReceptionPlanner.Plan(ctx,x.Trajectory,x.Rules,b,full,0);
            Vector2 toward=lBase.Steps==int.MaxValue?Vector2.zero:(lBase.Stance-b.Position);float v0toward=toward.sqrMagnitude>0?Vector2.Dot(b.Velocity,toward.normalized):float.NaN;
            float vmax=ctx.Move.MaxSpeed*b.MovementFactor;
            return F(b.Speed)+"\t"+F(v0toward)+"\t"+F(lBase.Distance)+"\t"+(lBase.Until<0?"nan":F(lBase.Until/(double)ctx.StepHz))+"\t"+x.React+"\t"+x.Perception+"\t"+x.Extra
                +"\t"+S(ctx,lBase)+"\t"+S(ctx,lNoReact)+"\t"+S(ctx,lNoPerc)+"\t"+S(ctx,lNoDelay)+"\t"+S(ctx,lRest)+"\t"+S(ctx,lRestInst)+"\t"+S(ctx,lCore)
                +"\t"+F(lCore.Distance/vmax)+"\t"+noiseless.Reason+"\t"+F(Vector2.Distance(x.Plan.Stance,noiseless.Stance))+"\t"+x.Plan.Reason;
        }
        private static readonly (string name,ShotLine line,ShotHeight height,ShotPower power,float? p)[] Intents=
        {
            ("fondo-max",ShotLine.Straight,ShotHeight.Mid,ShotPower.Strong,1f),
            ("fondo-085",ShotLine.Straight,ShotHeight.Mid,ShotPower.Strong,null),
            ("medio-suave",ShotLine.Straight,ShotHeight.Mid,ShotPower.Medium,null),
            ("corto-drop",ShotLine.Drop,ShotHeight.Low,ShotPower.Medium,null),
        };
        private static double Median(List<double> v){if(v.Count==0)return double.NaN;var s=v.OrderBy(x=>x).ToList();int n=s.Count;return n%2==1?s[n/2]:(s[n/2-1]+s[n/2])*.5;}
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void WriteDeepDefense()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(preset.Hash,Is.EqualTo("207da8f2c636"));
            var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            string dir=Path.Combine(root,"Logs/experiments/deep-defense/"+(Environment.GetEnvironmentVariable("SC_DD_RUN")??"run01"));
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");Directory.CreateDirectory(dir);
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),factory);
            // RallyParams hash recorded in source.tsv (the rival-reach variants change it).
            var levels=(Environment.GetEnvironmentVariable("SC_DD_LEVELS")??"Medium").Split(',').Select(s=>(AiLevel)Enum.Parse(typeof(AiLevel),s)).ToArray();
            int matches=int.Parse(Environment.GetEnvironmentVariable("SC_DD_MATCHES")??"100",C);const long seeds=20261126;
            File.WriteAllText(Path.Combine(dir,"source.tsv"),"defaults\trallyParams\tunity\tmatches\tseeds\n"+preset.Hash+"\t"+ctx.Params.Hash+"\t"+Application.unityVersion+"\t"+matches+"\t"+seeds+"\n",Utf8);
            using(var times=new StreamWriter(Path.Combine(dir,"rival-time.tsv"),false,Utf8))
            using(var cells=new StreamWriter(Path.Combine(dir,"cells.tsv"),false,Utf8))
            {
                times.WriteLine("level\tstyle\tintent\tmatch\tresult\tdrive\tserve\tv0\tv0Toward\tdistance\tuntilS\treactSteps\tperceptionSteps\textraSteps\tlateBaseS\tlateNoReactS\tlateNoPerceptionS\tlateNoDelayS\tlateRestS\tlateRestInstantS\tlateCoreS\tcruiseS\tnoiselessReason\tstanceError\tplanReason");
                cells.WriteLine("level\tstyle\tintent\tpoints\thuman\tshare\trivalContacts\tdefensiveContacts\tdefBounceN\tdefBounceMedianZ\tdefGlassFly\thumanSlackMedianS\trivalVolleys\trivalDefensiveVolleys\tmismatch\tseconds\trivalReceptions\trivalNoReach\thumanWinsNoReach\trivalLunges");
                foreach(var level in levels)foreach(var style in new[]{RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo})foreach(var intent in Intents)
                {
                    var watch=Stopwatch.StartNew();int points=0,human=0,mismatch=0,contacts=0,defensive=0,glass=0,volleys=0,defVolleys=0,rivalReceptions=0,noReach=0,winsNoReach=0,lunges=0;
                    var bounce=new List<double>();var slack=new List<double>();
                    for(int m=0;m<matches;m++)
                    {
                        var h=new Tap(new IntentBot(ctx,30,seeds+2*m,intent.line,intent.height,intent.power,intent.p),ctx);
                        var r=new RivalTap(RivalAi.For(ctx,level,style,seeds+1+2*m),ctx){Snapping=intent.name.StartsWith("fondo",StringComparison.Ordinal)};
                        using(var sim=new RallySim(ctx,h,r))
                        {
                            r.Sim=sim;
                            var score=new MatchScore(7,2,m%2==0?PlayerId.Human:PlayerId.Rival);int replays=0;
                            while(!score.Finished)
                            {
                                h.Rows.Clear();r.Rows.Clear();r.Snaps.Clear();
                                var o=sim.PlayPoint(score.Server,score.Box);
                                // The receivers alternate from the serve's receiver; a human row k+1 describes the rival's ball k.
                                var seen=new List<(double bz,bool glass,double slack)?>();int hi=0,ri=0;var who=o.FirstReceiver;
                                for(int k=0;k<o.Receptions.Count;k++){if(who==PlayerId.Human)seen.Add(hi<h.Rows.Count?h.Rows[hi]:((double,bool,double)?)null);else seen.Add(null);if(who==PlayerId.Human)hi++;else ri++;who=who.Other();}
                                bool ok=hi==h.Rows.Count&&ri==r.Rows.Count&&o.Receptions.Select((x,k)=>x.Who==(k%2==0?o.FirstReceiver:o.FirstReceiver.Other())).All(b=>b);
                                if(!ok)mismatch++;
                                else for(int k=0;k<o.Receptions.Count;k++)
                                {
                                    var x=o.Receptions[k];
                                    if(x.Who!=PlayerId.Rival||x.Result!=ReceptionResult.Contact)continue;
                                    contacts++;if(x.Volley){volleys++;if(x.Defensive)defVolleys++;}
                                    if(!x.Defensive)continue;
                                    defensive++;
                                    if(k+1<o.Receptions.Count&&seen[k+1].HasValue&&!double.IsNaN(seen[k+1].Value.bz))
                                    {var row=seen[k+1].Value;bounce.Add(row.bz);slack.Add(row.slack);if(row.glass)glass++;}
                                }
                                // Case (a): the rival did not get to the ball (Unreachable or NoSwing), counted over every reception of the rival.
                                foreach(var x in o.Receptions)if(x.Who==PlayerId.Rival){rivalReceptions++;if(x.Result==ReceptionResult.Unreachable||x.Result==ReceptionResult.NoSwing)noReach++;}
                                // set15: the time budget of every rival reception that did not get to the ball, aligned like the rows above.
                                if(ok&&r.Snapping)
                                {
                                    int rk=0;
                                    for(int k=0;k<o.Receptions.Count;k++)
                                    {
                                        var x=o.Receptions[k];if(x.Who!=PlayerId.Rival)continue;var snap=r.Snaps[rk++];
                                        if(x.Result!=ReceptionResult.Unreachable&&x.Result!=ReceptionResult.NoSwing)continue;
                                        bool serve=k==0&&o.Server==PlayerId.Human;
                                        bool drive=k>0&&o.Receptions[k-1].Who==PlayerId.Human&&o.Receptions[k-1].Result==ReceptionResult.Contact&&o.Receptions[k-1].Line==ShotLine.Straight&&o.Receptions[k-1].Height==ShotHeight.Mid&&o.Receptions[k-1].Power==ShotPower.Strong;
                                        times.WriteLine(level+"\t"+style+"\t"+intent.name+"\t"+m+"\t"+x.Result+"\t"+(drive?1:0)+"\t"+(serve?1:0)+"\t"+TimeRow(ctx,snap));
                                    }
                                }
                                if(o.Winner==PlayerId.Human&&o.Receptions.Count>0){var last=o.Receptions[o.Receptions.Count-1];if(last.Who==PlayerId.Rival&&(last.Result==ReceptionResult.Unreachable||last.Result==ReceptionResult.NoSwing))winsNoReach++;}
                                if(o.Winner.HasValue){score.PointWon(o.Winner.Value);points++;if(o.Winner==PlayerId.Human)human++;}
                                else if(++replays>50)break;
                            }
                            lunges+=r.Lunges;
                        }
                    }
                    cells.WriteLine(level+"\t"+style+"\t"+intent.name+"\t"+points+"\t"+human+"\t"+F(points==0?0:human/(double)points)+"\t"+contacts+"\t"+defensive+"\t"+bounce.Count+"\t"+F(Median(bounce))+"\t"+glass+"\t"+F(Median(slack))+"\t"+volleys+"\t"+defVolleys+"\t"+mismatch+"\t"+F(watch.Elapsed.TotalSeconds)+"\t"+rivalReceptions+"\t"+noReach+"\t"+winsNoReach+"\t"+lunges);
                    cells.Flush();times.Flush();
                }
            }
        }
    }
}
