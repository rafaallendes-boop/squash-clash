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
        private struct Call{public BallState Ball;public ReceptionPlan Plan;public AiShot Shot;}
        private struct Rec{public BallState Ball;public Vector2 Target,Wait;}
        private class Tap : IRallyBrain
        {
            public readonly List<(double bz,bool glass,double slack)> Rows=new List<(double,bool,double)>();public int Lunges;
            // set19: what each Decide saw and returned (recorded only, nothing is changed).
            public readonly List<Call> Calls=new List<Call>();
            private readonly IRallyBrain inner;private readonly RallyContext ctx;
            public Tap(IRallyBrain inner,RallyContext ctx){this.inner=inner;this.ctx=ctx;}
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public virtual AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                var (bz,glass)=Bounce(ctx,o.Ball);Rows.Add((bz,glass,own.SlackS));if(own.Reason=="lunge")Lunges++;
                var shot=inner.Decide(o,own);Calls.Add(new Call{Ball=o.Ball,Plan=own,Shot=shot});
                return shot;
            }
        }
        // set15 rival time: what the rival had at the human's contact (t0), copied without touching the sim (its body, rules and energy).
        private sealed class Snap
        {
            public Trajectory Trajectory;public PointRules Rules;public PlayerBody Body;public int React,Perception,Extra;public ReceptionPlan Plan;
        }
        private sealed class RivalTap : Tap,IRallyPositioning,IRallyServer,IRallyLevel
        {
            private readonly RivalAi rival;private readonly RallyContext context;
            public RallySim Sim;public bool Snapping;public readonly List<Snap> Snaps=new List<Snap>();public readonly List<Rec> Recoveries=new List<Rec>();
            public RivalTap(RivalAi rival,RallyContext ctx):base(rival,ctx){this.rival=rival;context=ctx;}
            public override AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                if(Snapping)Snaps.Add(new Snap{Trajectory=context.Predict(o.Ball),Rules=Sim.Rules.Clone(),Body=Sim.Rival.Clone(),React=context.Steps(context.Params.ReactionS),
                    Perception=rival.PerceptionSteps,Extra=FatigueEffects.For(context.Fatigue.Factor(Sim.RivalEnergy.Energy),context.Fatigue).ExtraSteps,Plan=own});
                else Snaps.Add(null);
                return base.Decide(o,own);
            }
            public AiLevel? Level=>rival.Level;
            public Vector2 Wait=>rival.Wait;
            public Vector2 Recovery(in AiObservation a){var target=rival.Recovery(a);Recoveries.Add(new Rec{Ball=a.Ball,Target=target,Wait=rival.Wait});return target;}
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
        // set19: the first bounce of a ball after the front wall (Persistent contacts skipped) and whether it touched the back wall on the fly or after it.
        private struct Land{public bool Found;public double X,Z;public bool BackFly,BackAfter;}
        private static Land Landing(Trajectory t)
        {
            var l=new Land{X=double.NaN,Z=double.NaN};bool front=false;int floors=0;
            foreach(var c in t.Contacts)
            {
                if(c.Persistent)continue;
                if(c.Surface==SurfaceId.FrontWall){front=true;continue;}
                if(!front)continue;
                if(c.Surface==SurfaceId.BackWall){if(floors==0)l.BackFly=true;else if(floors==1)l.BackAfter=true;}
                else if(c.Surface==SurfaceId.Floor){if(++floors==1){l.Found=true;l.X=c.Point.x;l.Z=c.Point.z;}else break;}
            }
            return l;
        }
        private static string N(double v)=>double.IsNaN(v)?"nan":F(v);
        private static string ShotClass(ShotLine line,ShotHeight height)=>line==ShotLine.Boast?"boast":line==ShotLine.Drop?"drop":height==ShotHeight.High?"globo":line==ShotLine.Straight?"drive-recto":line==ShotLine.Cross?"cruzado":"otro";
        // One rival reception that reached a drive: the rows after "level style intent match k" of defense.tsv.
        private static string DefenseRow(RallyContext ctx,RallyOutcome o,int k,Call rv,Call? next)
        {
            var x=o.Receptions[k];var plan=rv.Plan;var shot=rv.Shot;
            bool vol=shot.Volley&&plan.Volley.Exists;var cp=vol?plan.Volley.Plan:plan.Contact;
            double reach=double.NaN,window=double.NaN,release=double.NaN;
            if(cp.Exists&&cp.TStarStep>=0)
            {
                double rate=ctx.StepHz*ctx.Tempo;
                reach=cp.Body.Reach-(cp.PStar-new Vector3(plan.BodyAtContact.x,cp.Body.ReferenceHeight,plan.BodyAtContact.y)).magnitude;
                window=(cp.KOut.Steps-cp.KIn.Steps)*1000/rate;
                double lower=(cp.KIn.Steps-cp.TStarStep)*1000/rate,upper=(cp.KOut.Steps-cp.TStarStep)*1000/rate;
                release=Math.Min(shot.DeltaMs-lower,upper-shot.DeltaMs);
            }
            string bx="nan",bz="nan",sd="nan",bf="nan",ba="nan",len="none",tight="none",hs="nan",hr="none",hside="nan";
            if(x.Result==ReceptionResult.Contact&&next.HasValue)
            {
                var l=Landing(ctx.Predict(next.Value.Ball));var hl=o.Receptions[k+1];
                bf=l.BackFly?"1":"0";ba=l.BackAfter?"1":"0";hs=N(next.Value.Plan.SlackS);hr=hl.Result.ToString();
                if(l.Found)
                {
                    double side=Math.Min(l.X,ctx.Court.Width-l.X);bx=N(l.X);bz=N(l.Z);sd=N(side);
                    len=l.Z<ctx.T.y?"corto":l.BackFly?"fondo-vuelo":l.BackAfter?"largo":"medio";
                    tight=side<=0.5?"pegado":side<=1.0?"intermedio":"suelto";
                }
                if(hl.Who==PlayerId.Human&&hl.Result==ReceptionResult.Contact)hside=N(Math.Min(hl.ContactPoint.x,ctx.Court.Width-hl.ContactPoint.x));
            }
            string winner=o.Winner.HasValue?o.Winner.Value.ToString():"none";
            int lostNow=o.Winner==PlayerId.Human&&o.Receptions.Count-1<=k+2?1:0;
            return string.Join("\t",new[]{x.Result.ToString(),x.Line.ToString(),x.Height.ToString(),x.Power.ToString(),x.Volley?"1":"0",x.Defensive?"1":"0",ShotClass(x.Line,x.Height),
                N(plan.SlackS),plan.Reason,N(reach),N(window),N(release),bx,bz,sd,bf,ba,len,tight,hs,hr,hside,winner,lostNow.ToString(C),(o.Receptions.Count-1-k).ToString(C)});
        }
        // One rival reception that did not reach a drive (set15 population): the rival's previous ball, where Recovery sent it and what that left.
        private static string PrevRow(RallyContext ctx,RallyOutcome o,int k,Snap snap,List<Rec> recs,int[] recIdx,Vector2 wait)
        {
            string origin=k==1&&o.Server==PlayerId.Rival?"serve":k>=2&&recIdx[k-2]>=0?"stroke":"other";
            double pbx=double.NaN,pbz=double.NaN;Vector2? target=null;
            if(origin=="stroke")
            {
                var rec=recs[recIdx[k-2]];target=rec.Target;
                foreach(var c in ctx.Predict(rec.Ball).Contacts)if(c.Surface==SurfaceId.Floor){pbx=c.Point.x;pbz=c.Point.z;break;}
            }
            else if(origin=="serve")target=wait;
            var land=Landing(snap.Trajectory);
            bool shifted=origin=="stroke"&&!double.IsNaN(pbz)&&pbz>=ctx.Params.StyleCoverBounceZ&&ctx.Params.RivalRecoverySideShift>0;
            int sameSide=-1;
            if(shifted&&land.Found)sameSide=Math.Sign(target.Value.x-ctx.T.x)==Math.Sign(land.X-ctx.T.x)?1:0;
            double dist=target.HasValue?Vector2.Distance(snap.Body.Position,target.Value):double.NaN;
            int full=snap.React+snap.Perception+snap.Extra;
            string LateAt(Vector2 at)=>S(ctx,Deficit(ctx,snap.Trajectory,snap.Rules,new PlayerBody(PlayerId.Rival,at,ctx.Move){MovementFactor=snap.Body.MovementFactor},full));
            string lateBase=S(ctx,Deficit(ctx,snap.Trajectory,snap.Rules,snap.Body,full));
            string lateTarget=target.HasValue?LateAt(target.Value):"nan";
            string lateSide=land.Found?LateAt(ctx.ClampBody(new Vector2(ctx.T.x+(land.X<ctx.T.x?-1f:1f)*(float)ctx.Params.RivalRecoverySideShift,wait.y))):"nan";
            string diag=origin=="serve"?"serve":origin=="other"?"other":!shifted?"short":sameSide==0?"other-side":dist>0.25?"not-arrived":"arrived-short";
            return string.Join("\t",new[]{origin,N(pbx),N(pbz),target.HasValue?N(target.Value.x):"nan",target.HasValue?N(target.Value.y):"nan",N(wait.x),N(wait.y),shifted?"1":"0",N(land.X),N(land.Z),
                sameSide<0?"nan":sameSide.ToString(C),N(snap.Body.Position.x),N(snap.Body.Position.y),N(dist),N(snap.Body.Speed),lateBase,lateTarget,lateSide,diag});
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
            using(var defW=new StreamWriter(Path.Combine(dir,"defense.tsv"),false,Utf8))
            using(var prevW=new StreamWriter(Path.Combine(dir,"prev-ball.tsv"),false,Utf8))
            using(var dcW=new StreamWriter(Path.Combine(dir,"defense-cells.tsv"),false,Utf8))
            using(var srW=new StreamWriter(Path.Combine(dir,"swing-read.tsv"),false,Utf8))
            {
                defW.WriteLine("level\tstyle\tintent\tmatch\tk\tresult\tline\theight\tpower\tvolley\tdefensive\tshotClass\trivalSlackS\tplanReason\treachMargin\twindowMs\treleaseMarginMs\tbounceX\tbounceZ\tsideDist\tbackFly\tbackAfter\tlengthClass\ttightClass\thumanSlackS\thumanResult\thumanContactSideDist\tpointWinner\tlostNow\treceptionsAfter");
                prevW.WriteLine("level\tstyle\tintent\tmatch\tk\tresult\torigin\tprevBounceX\tprevBounceZ\ttargetX\ttargetZ\twaitX\twaitZ\tshifted\tdriveBounceX\tdriveBounceZ\tsameSide\tbodyX\tbodyZ\tdistToTarget\tbodySpeed\tlateBaseS\tlateAtTargetS\tlateDriveSideS\tdiagClass");
                dcW.WriteLine("level\tstyle\tintent\tdefenseRows\tprevBallRows\tnewMismatch");
                srW.WriteLine("level\tstyle\tintent\tmatch\tk\thumanLine\thumanHeight\thumanPower\thumanVolley\tkind\tplanned\tleadSteps\tleadMs\tstanceX\tstanceZ\tbodyX\tbodyZ\trivalResult\trivalSlackS\trivalPlanReason\tpointWinner\tlostNow\treceptionsAfter");
                times.WriteLine("level\tstyle\tintent\tmatch\tresult\tdrive\tserve\tv0\tv0Toward\tdistance\tuntilS\treactSteps\tperceptionSteps\textraSteps\tlateBaseS\tlateNoReactS\tlateNoPerceptionS\tlateNoDelayS\tlateRestS\tlateRestInstantS\tlateCoreS\tcruiseS\tnoiselessReason\tstanceError\tplanReason");
                cells.WriteLine("level\tstyle\tintent\tpoints\thuman\tshare\trivalContacts\tdefensiveContacts\tdefBounceN\tdefBounceMedianZ\tdefGlassFly\thumanSlackMedianS\trivalVolleys\trivalDefensiveVolleys\tmismatch\tseconds\trivalReceptions\trivalNoReach\thumanWinsNoReach\trivalLunges");
                foreach(var level in levels)foreach(var style in new[]{RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo})foreach(var intent in Intents)
                {
                    var watch=Stopwatch.StartNew();int points=0,human=0,mismatch=0,contacts=0,defensive=0,glass=0,volleys=0,defVolleys=0,rivalReceptions=0,noReach=0,winsNoReach=0,lunges=0,defRows=0,prevRows=0,newMismatch=0;
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
                                h.Rows.Clear();r.Rows.Clear();r.Snaps.Clear();h.Calls.Clear();r.Calls.Clear();r.Recoveries.Clear();
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
                                // set19: Decide calls align with each brain's receptions, Recovery calls with the rival's Contact receptions.
                                bool aligned=false;int[] hIdx=null,recIdx=null;
                                if(ok)
                                {
                                    int nHum=0,nRec=0;hIdx=new int[o.Receptions.Count];recIdx=new int[o.Receptions.Count];
                                    for(int k=0;k<o.Receptions.Count;k++)
                                    {
                                        hIdx[k]=-1;recIdx[k]=-1;
                                        if(o.Receptions[k].Who==PlayerId.Human)hIdx[k]=nHum++;
                                        else if(o.Receptions[k].Result==ReceptionResult.Contact)recIdx[k]=nRec++;
                                    }
                                    aligned=nRec==r.Recoveries.Count&&nHum==h.Calls.Count&&r.Calls.Count==r.Rows.Count;
                                    if(!aligned)newMismatch++;
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
                                        if(aligned&&drive){prevW.WriteLine(level+"\t"+style+"\t"+intent.name+"\t"+m+"\t"+k+"\t"+x.Result+"\t"+PrevRow(ctx,o,k,snap,r.Recoveries,recIdx,r.Wait));prevRows++;}
                                    }
                                }
                                // set19: the rival's receptions that reached a drive to the back: how it defended.
                                if(aligned&&r.Snapping)
                                {
                                    int rj=0;
                                    for(int k=0;k<o.Receptions.Count;k++)
                                    {
                                        var x=o.Receptions[k];if(x.Who!=PlayerId.Rival)continue;var rv=r.Calls[rj++];
                                        if(k==0||x.Result==ReceptionResult.Unreachable||x.Result==ReceptionResult.NoSwing)continue;
                                        var pv=o.Receptions[k-1];
                                        if(pv.Who!=PlayerId.Human||pv.Result!=ReceptionResult.Contact||pv.Line!=ShotLine.Straight||pv.Height!=ShotHeight.Mid||pv.Power!=ShotPower.Strong)continue;
                                        Call? next=k+1<o.Receptions.Count?h.Calls[hIdx[k+1]]:(Call?)null;
                                        defW.WriteLine(level+"\t"+style+"\t"+intent.name+"\t"+m+"\t"+k+"\t"+DefenseRow(ctx,o,k,rv,next));defRows++;
                                    }
                                }
                                // set21: one row per read of the rival (the human's swing), aligned like the rows above.
                                if(ok)
                                {
                                    int rivals=o.Receptions.Count(x=>x.Who==PlayerId.Rival);
                                    foreach(var read in sim.SwingReads)
                                    {
                                        int k=read.RivalReception;string hl="none\tnone\tnone\tnone";
                                        if(k-1>=0&&k-1<o.Receptions.Count){var hx=o.Receptions[k-1];hl=hx.Line+"\t"+hx.Height+"\t"+hx.Power+"\t"+(hx.Volley?"1":"0");}
                                        string rr="none",rs="nan",rp="none";
                                        if(k<o.Receptions.Count&&o.Receptions[k].Who==PlayerId.Rival)
                                        {
                                            rr=o.Receptions[k].Result.ToString();
                                            int before=o.Receptions.Take(k).Count(x=>x.Who==PlayerId.Rival);
                                            if(r.Calls.Count==rivals&&before<r.Calls.Count){rs=N(r.Calls[before].Plan.SlackS);rp=r.Calls[before].Plan.Reason;}
                                        }
                                        string pw=o.Winner.HasValue?o.Winner.Value.ToString():"none";int lost=o.Winner==PlayerId.Human&&o.Receptions.Count-1<=k+2?1:0;
                                        srW.WriteLine(level+"\t"+style+"\t"+intent.name+"\t"+m+"\t"+k+"\t"+hl+"\t"+read.Kind+"\t"+(read.Planned?"1":"0")+"\t"+read.LeadSteps+"\t"+F(read.LeadSteps*1000/(ctx.StepHz*ctx.Tempo))
                                            +"\t"+(read.Planned?N(read.Stance.x):"nan")+"\t"+(read.Planned?N(read.Stance.y):"nan")+"\t"+N(read.BodyAtRead.x)+"\t"+N(read.BodyAtRead.y)+"\t"+rr+"\t"+rs+"\t"+rp+"\t"+pw+"\t"+lost+"\t"+(o.Receptions.Count-1-k));
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
                    dcW.WriteLine(level+"\t"+style+"\t"+intent.name+"\t"+defRows+"\t"+prevRows+"\t"+newMismatch);
                    cells.Flush();times.Flush();defW.Flush();prevW.Flush();dcW.Flush();srW.Flush();
                }
            }
        }
    }
}
