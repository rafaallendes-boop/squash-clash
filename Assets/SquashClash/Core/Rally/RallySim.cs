using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    // Stalled: a flight that never ends (a block). ShotCap / StepCap: a rally cut short by the configured limits.
    public enum RallyEnd { Point, LaunchFault, Stalled, StepCap, ShotCap }
    public enum ReceptionResult { Contact, Air, Late, Unreachable, NoSwing, SystemFault, NotEvaluable }
    public readonly struct ReceptionLog
    {
        public readonly PlayerId Who;public readonly ReceptionResult Result;public readonly bool Perfect,Reachable,Defensive;
        public readonly double DeltaMs,SlackS;public readonly ShotLine Line;public readonly ShotHeight Height;public readonly ShotPower Power;
        public readonly long ContactStep;public readonly string Reason;
        public readonly CausalVector[] Vectors;public readonly CausalInput Input;
        public readonly double FatigueReception,FatigueStriker;
        // The release went to the volley opportunity (contact before the bounce when it is a Contact).
        public readonly bool Volley;
        // A volley opportunity was offered in this reception (whether or not it was taken).
        public readonly bool VolleyOffered;
        // The speed factor of the chosen volley's run (v2.2 sprint; 1 = no sprint), set only for a touching human brain.
        public readonly float VolleySprintFactor;
        // Where the ball was struck (the contact step's centre), set only on a stroke that was applied; the boast's side-wall point.
        public readonly Vector3 ContactPoint;public readonly SideAim Side;
        // A boast that had no valid point at the contact and went to the front wall instead (addendum 1).
        public readonly bool BoastFellBack;
        public ReceptionLog(PlayerId who,ReceptionResult result,bool perfect,bool reachable,bool defensive,double deltaMs,double slack,ShotOption option,long contactStep,string reason,CausalShotResult causal=default,double fatigueReception=0,double fatigueStriker=0,bool volley=false,bool volleyOffered=false,float volleySprintFactor=1,Vector3 contactPoint=default)
        {BoastFellBack=option.Side.Exists&&causal.FellBack;ContactPoint=contactPoint;Side=option.Side;VolleySprintFactor=volleySprintFactor;Volley=volley;VolleyOffered=volleyOffered;FatigueReception=fatigueReception;FatigueStriker=fatigueStriker;Vectors=causal.Vectors;Input=causal.Input;Who=who;Result=result;Perfect=perfect;Reachable=reachable;Defensive=defensive;DeltaMs=deltaMs;SlackS=slack;Line=option.Line;Height=option.Height;Power=option.Power;ContactStep=contactStep;Reason=reason;}
    }
    public sealed class RallyOutcome
    {
        public RallyEnd End;public PointEndCause Cause;public PlayerId? Winner;public PlayerId FirstReceiver;
        public int Shots,SystemFaults,IgnoredAfterClose,RejectedContacts;public long Steps;public double SimSeconds,RealSeconds;
        // Why the last receiver did not return the ball, when the point ended on a double bounce.
        public ReceptionResult? LastReception;
        // Who struck the ball in flight when the point ended (null: neutral launch), and where it ended.
        public PlayerId? Striker;public SurfaceId TerminalSurface;public bool OutAfterBounce;
        // Set when the point started with a serve.
        public PlayerId? Server;public ServiceBox Box;
        // The serve kind played (Auto while the new serve is off).
        public ServeType ServeType;
        public FatiguePoint HumanFatigue,RivalFatigue;
        public readonly List<ReceptionLog> Receptions=new List<ReceptionLog>();
    }
    public sealed class MatchOutcome
    {
        public PlayerId? Winner;public int Human,Rival,Replays;public bool Aborted;public double RealSeconds;
        public readonly List<RallyOutcome> Points=new List<RallyOutcome>();
    }
    // Scripted stand-in for the human in headless runs: one fixed intention and a Gaussian release, same contract.
    public sealed class BotBrain : IRallyBrain
    {
        private readonly RallyContext context;private readonly RallyRng random;private readonly double timingSigmaMs;
        private readonly ShotLine line;private readonly ShotHeight height;private readonly ShotPower power;
        private readonly bool takeVolley;
        // takeVolley: release in every volley opportunity offered (tests and measurement only; the default never volleys).
        public BotBrain(RallyContext context,double timingSigmaMs,long seed,ShotLine line=ShotLine.Straight,ShotHeight height=ShotHeight.Mid,ShotPower power=ShotPower.Medium,bool takeVolley=false)
        {this.context=context;this.timingSigmaMs=timingSigmaMs;random=new RallyRng(seed);this.line=line;this.height=height;this.power=power;this.takeVolley=takeVolley;}
        public int PerceptionSteps=>0;
        public Vector2 StanceNoise()=>Vector2.zero;
        public AiShot Decide(in AiObservation observation,in ReceptionPlan own)
        {
            bool volley=takeVolley&&own.Volley.Exists;
            if(!volley&&(!own.Reachable||!own.Contact.Exists||own.Contact.TStarStep<0))return default;
            var at=volley?own.Volley.Plan.PStar:own.Contact.PStar;
            // The preferred intention first; if its own forecast is a fault, the nearest one that is not.
            var options=ShotOptions.Build(context,at);
            options.Sort((a,b)=>Distance(a).CompareTo(Distance(b)));
            double delta=random.NextGaussian(timingSigmaMs);
            foreach(var option in options)
                if(RallyContact.Forecast(context,at,option.Aim,option.P).Good)return new AiShot(true,option,option.Aim,option.P,delta,false,volley);
            return new AiShot(true,options[0],options[0].Aim,options[0].P,delta,false,volley);
        }
        private int Distance(ShotOption o)=>(o.Line==line?0:o.Line==ShotLine.Drop||line==ShotLine.Drop?8:4)+2*Math.Abs((int)o.Height-(int)height)+(o.Power==power?0:1);
    }
    // Headless rally: one analytic ball (the only authority), two bodies, the shared rules and the shared contact path.
    // No scene, no frames: every decision is taken in simulation steps, so a seed reproduces the rally exactly.
    public sealed class RallySim : IDisposable
    {
        private readonly RallyContext context;
        private readonly IRallyBrain[] brains=new IRallyBrain[2];
        private readonly IBallSimulation ball;
        private readonly List<ContactEvent> contacts=new List<ContactEvent>(8);
        private CausalReference pendingCausal;
        private bool hasPending;private long pendingStep;private AiShot pendingShot;private PlayerId pendingWho;private ReceptionLog pendingLog;
        private int planVersion;private long launchId;
        // Volley v2.x for a touching human brain (IRallyVolleyToucher): the body turns to the chosen stance at the touch step.
        private bool hasRedirect;private long redirectStep;private Vector2 redirectStance;private int redirectDelay;private float redirectSprint=1;private double pendingSprintCost;
        public FatigueState HumanEnergy {get;private set;}
        public FatigueState RivalEnergy {get;private set;}
        // Measurement and tests (like the DEV «Energía» buttons): both players' energy from now on; PlayMatch still starts at 100.
        public void SetEnergy(double human,double rival){HumanEnergy=new FatigueState(context.Fatigue,human);RivalEnergy=new FatigueState(context.Fatigue,rival);}
        private readonly double[] receptionFatigue=new double[2];
        public PointRules Rules {get;}
        public PlayerBody Human {get;}
        public PlayerBody Rival {get;}
        public BallState Ball=>ball.State;
        public ReceptionPlan LastPlan {get;private set;}
        public RallySim(RallyContext context,IRallyBrain human,IRallyBrain rival)
        {
            this.context=context??throw new ArgumentNullException(nameof(context));
            brains[0]=human??throw new ArgumentNullException(nameof(human));brains[1]=rival??throw new ArgumentNullException(nameof(rival));
            HumanEnergy=new FatigueState(context.Fatigue);RivalEnergy=new FatigueState(context.Fatigue);
            ball=context.Factory.Create(context.Physics,context.Court);
            Rules=context.NewRules();
            Human=new PlayerBody(PlayerId.Human,WaitOf(PlayerId.Human),context.Move);
            Rival=new PlayerBody(PlayerId.Rival,WaitOf(PlayerId.Rival),context.Move);
        }
        private PlayerBody Body(PlayerId who)=>who==PlayerId.Human?Human:Rival;
        // A brain with its own positioning (a rival style) waits and recovers there; any other brain uses the shared wait position.
        public Vector2 WaitOf(PlayerId who)=>brains[(int)who] is IRallyPositioning p?p.Wait:context.WaitPosition(who);
        private Vector2 RecoveryOf(PlayerId who)
        {
            if(!(brains[(int)who] is IRallyPositioning p))return context.WaitPosition(who);
            var self=Body(who);var other=Body(who.Other());
            return p.Recovery(new AiObservation(ball.State,self.Position,self.Velocity,other.Position,other.Velocity,Rules.Shots,Rules.FloorBounces,Rules.FrontWallReached));
        }
        private FatigueState Energy(PlayerId who)=>who==PlayerId.Human?HumanEnergy:RivalEnergy;
        private void BeginFatiguePoint()
        {HumanEnergy.BeginPoint();RivalEnergy.BeginPoint();Human.MovementFactor=Rival.MovementFactor=1;}
        public RallyOutcome Play(PlayerId firstReceiver)
        {
            BeginFatiguePoint();
            var outcome=new RallyOutcome{FirstReceiver=firstReceiver};
            Human.Teleport(WaitOf(PlayerId.Human));Rival.Teleport(WaitOf(PlayerId.Rival));
            ball.Reset(context.NeutralLaunch(firstReceiver));Rules.Begin(null,firstReceiver);
            return Run(outcome,firstReceiver);
        }
        // A point that starts with the automatic serve from the server's box (spec 8.5).
        public RallyOutcome PlayPoint(PlayerId server,ServiceBox box)
        {
            BeginFatiguePoint();Energy(server).Strike(0); // Automatic serve intention (spec v2.3).
            var outcome=new RallyOutcome{FirstReceiver=server.Other(),Server=server,Box=box};
            Human.Teleport(WaitOf(PlayerId.Human));Rival.Teleport(WaitOf(PlayerId.Rival));
            Body(server).Teleport(ServePlanner.BoxPosition(context,box));
            Body(server).MoveTo(WaitOf(server),0,BodyPhase.Recovering,BodyPhase.Waiting);
            // Serve kinds (design 2026-11-20-serve): Auto, today's serve, while the switch is off.
            outcome.ServeType=context.ServeTypeOf(brains[(int)server],box);
            ball.Reset(context.Serve(box,outcome.ServeType));Rules.BeginServe(server,box);
            return Run(outcome,server.Other());
        }
        // A whole game: point-a-rally to the target with the margin, winner serves, boxes alternate.
        public MatchOutcome PlayMatch(PlayerId firstServer)
        {
            HumanEnergy=new FatigueState(context.Fatigue);RivalEnergy=new FatigueState(context.Fatigue);
            var match=new MatchOutcome();var score=new MatchScore((int)context.Params.MatchTarget,(int)context.Params.MatchMargin,firstServer);
            while(!score.Finished)
            {
                var point=PlayPoint(score.Server,score.Box);match.Points.Add(point);match.RealSeconds+=point.RealSeconds+context.Params.MatchPauseSeconds;
                if(point.Winner.HasValue)score.PointWon(point.Winner.Value);
                // A point with no winner (stroke limit) is replayed from the same box.
                else if(++match.Replays>50){match.Aborted=true;break;}
            }
            match.Winner=score.Winner;match.Human=score.Human;match.Rival=score.Rival;return match;
        }
        private RallyOutcome Run(RallyOutcome outcome,PlayerId firstReceiver)
        {
            hasPending=false;hasRedirect=false;pendingSprintCost=0;Human.Sprint=1;
            long cap=context.Steps(context.Params.MaxRallySeconds),steps=0;long stall=context.Steps(context.Params.StallSeconds),flight=0;int shots=0;
            BeginReception(firstReceiver,outcome);
            while(!Rules.Closed)
            {
                if(steps>=cap){outcome.End=RallyEnd.StepCap;break;}
                if(Rules.Shots>=context.Params.MaxShots){outcome.End=RallyEnd.ShotCap;break;}
                if(hasPending&&ball.State.Step==pendingStep)Strike(outcome);
                if(hasRedirect&&ball.State.Step==redirectStep)
                {hasRedirect=false;Human.Sprint=redirectSprint;Human.MoveTo(redirectStance,redirectDelay,BodyPhase.Approaching,BodyPhase.Set);}
                if(Rules.Shots!=shots){shots=Rules.Shots;flight=0;}
                if(++flight>stall){outcome.End=RallyEnd.Stalled;break;}
                ball.Step(contacts);steps++;
                foreach(var contact in contacts)Rules.OnBallContact(contact);
                if(ball.State.Position.y>context.Court.WallHeight)Rules.OnOverTop();
                var humanBefore=Human.Position;var rivalBefore=Rival.Position;
                Human.Step();Rival.Step();
                if(context.Fatigue.Enabled)
                {
                    HumanEnergy.Step(Vector2.Distance(humanBefore,Human.Position),Human.Phase,Human.Speed,Human.Reacting);
                    RivalEnergy.Step(Vector2.Distance(rivalBefore,Rival.Position),Rival.Phase,Rival.Speed,Rival.Reacting);
                }
            }
            if(Rules.Closed)outcome.End=Rules.Cause==PointEndCause.LaunchFault?RallyEnd.LaunchFault:RallyEnd.Point;
            outcome.Striker=Rules.Striker;outcome.TerminalSurface=Rules.TerminalSurface;outcome.OutAfterBounce=Rules.OutAfterBounce;
            outcome.Cause=Rules.Cause;outcome.Winner=Rules.Winner;outcome.Shots=Rules.Shots;outcome.IgnoredAfterClose=Rules.IgnoredAfterClose;outcome.RejectedContacts=Rules.RejectedContacts;
            outcome.Steps=steps;outcome.SimSeconds=steps/(double)context.StepHz;outcome.RealSeconds=outcome.SimSeconds/context.Tempo;
            if(Rules.Cause==PointEndCause.DoubleBounce&&outcome.Receptions.Count>0)outcome.LastReception=outcome.Receptions[outcome.Receptions.Count-1].Result;
            HumanEnergy.ClosePoint();RivalEnergy.ClosePoint();
            outcome.HumanFatigue=HumanEnergy.Point.Copy();outcome.RivalFatigue=RivalEnergy.Point.Copy();
            return outcome;
        }
        private void BeginReception(PlayerId who,RallyOutcome outcome)
        {
            var brain=brains[(int)who];var body=Body(who);var other=Body(who.Other());var state=ball.State;
            // The frozen causal reference belongs to this reception only (RallyDirector clears its trace list per point).
            pendingCausal=null;
            var trajectory=context.Predict(state);
            double f=context.Fatigue.Factor(Energy(who).Energy);receptionFatigue[(int)who]=f;
            var effects=FatigueEffects.For(f,context.Fatigue);body.MovementFactor=effects.MovementFactor;
            int delay=context.Steps(context.Params.ReactionS)+brain.PerceptionSteps+effects.ExtraSteps;
            var plan=ReceptionPlanner.Plan(context,trajectory,Rules,body,delay,++planVersion,brain.StanceNoise());
            body.MoveTo(plan.Stance,delay,BodyPhase.Approaching,BodyPhase.Set);
            // M1: the volley, if any, lies on the path the body already runs towards the bounce plan.
            var path=context.VolleyOn?body.PositionsAfter(trajectory.States.Count):null;
            var volley=context.VolleyFor(trajectory,Rules,path,plan.Contact,delay,planVersion);
            if(volley.Exists)plan=plan.WithVolley(volley);
            LastPlan=plan;hasRedirect=false;
            // Volley v2.x (design 2026-11-20): only a touching human brain; every other brain takes the unchanged path below.
            VolleyIntercept chosen=default;Vector2[] chosenPath=null;bool v2Offered=false;float chosenSprint=1;
            if(who==PlayerId.Human&&context.HumanVolleyV2&&brain is IRallyVolleyToucher toucher)
                ChooseHumanVolley(toucher,trajectory,plan,body,delay,f,out chosen,out chosenPath,out v2Offered,out chosenSprint);
            var observation=new AiObservation(state,body.Position,body.Velocity,other.Position,other.Velocity,Rules.Shots,Rules.FloorBounces,Rules.FrontWallReached);
            var shot=chosen.Exists?brain.Decide(observation,new ReceptionPlan(true,chosen.Stance,chosen.Stance,chosen.Plan,plan.ArrivalStep,plan.SlackS,plan.Quality,"volley-v2")):brain.Decide(observation,plan);
            if(chosen.Exists&&!shot.Swing){hasRedirect=false;pendingSprintCost=0;chosen=default;chosenPath=null;chosenSprint=1;}
            bool offeredLog=chosen.Exists||v2Offered||plan.Volley.Exists;
            hasPending=false;
            if(!shot.Swing)
            {
                outcome.Receptions.Add(new ReceptionLog(who,plan.Reachable?ReceptionResult.NoSwing:ReceptionResult.Unreachable,false,plan.Reachable,false,0,plan.SlackS,default,-1,plan.Reason,fatigueReception:f,volleyOffered:offeredLog));
                return;
            }
            // Real perceived time for this flight: the same clock map type the touch path uses.
            double now=state.Step/(context.Tempo*context.StepHz);
            var clock=new ClockMap();clock.Begin(++launchId,now,state.Step,context.Tempo,1,context.StepHz);
            bool useVolley=!chosen.Exists&&shot.Volley&&plan.Volley.Exists;
            // A release aimed at the volley is judged like the human's: by the opportunity whose segment holds it (spec 7.1.3 case 14).
            // A chosen v2.x volley is the only opportunity left, judged against its own plan.
            var decision=chosen.Exists?RallyContact.Resolve(context,clock,trajectory,chosen.Plan,now,shot.DeltaMs):useVolley?RallyContact.ResolveAimedVolley(context,clock,trajectory,plan.Contact,plan.Volley,now,shot.DeltaMs,out useVolley)
                :RallyContact.Resolve(context,clock,trajectory,plan.Contact,now,shot.DeltaMs);
            if(chosen.Exists)useVolley=true;
            var contact=chosen.Exists?chosen.Plan:useVolley?plan.Volley.Plan:plan.Contact;
            if(contact.Exists&&contact.TStarStep>=0)
            {
                var positions=chosenPath??path??body.PositionsAfter(trajectory.States.Count);long first=trajectory.States[0].Step;
                pendingCausal=CausalQuality.Prepare(trajectory,contact,clock,context.Timing,now,context.Tempo,j=>context.FrameAt(positions[(int)(j-first)]));
            }
            ReceptionResult result;
            switch(decision.Result)
            {
                case ShotResult.Perfect:case ShotResult.Valid:result=ReceptionResult.Contact;break;
                case ShotResult.Failed:result=decision.Fail==FailKind.Air?ReceptionResult.Air:ReceptionResult.Late;break;
                case ShotResult.Unreachable:result=ReceptionResult.Unreachable;break;
                case ShotResult.SystemFault:result=ReceptionResult.SystemFault;outcome.SystemFaults++;break;
                default:result=ReceptionResult.NotEvaluable;break;
            }
            var log=new ReceptionLog(who,result,decision.Result==ShotResult.Perfect,plan.Reachable,shot.Defensive,decision.DeltaMs,plan.SlackS,shot.Option,decision.ContactStep,decision.Reason,fatigueReception:f,volley:useVolley,volleyOffered:offeredLog,volleySprintFactor:chosenSprint);
            if(result!=ReceptionResult.Contact){outcome.Receptions.Add(log);return;}
            hasPending=true;pendingStep=decision.ContactStep;pendingShot=shot;pendingWho=who;pendingLog=log;
        }
        private void Strike(RallyOutcome outcome)
        {
            hasPending=false;var who=pendingWho;var state=ball.State;
            // The arm is the striker's own energy at this contact step, read before this stroke's cost is charged below.
            var arm=context.Fatigue.Arm(Energy(who).Energy);
            var causal=Rules.CanHit(who)?RallyContact.Solve(context,pendingCausal,context.FrameAt(Body(who).Position),state.Position,pendingShot,arm):default;
            var solution=causal.Solution;
            if(!solution.Solved)
            {
                outcome.SystemFaults++;var l=pendingLog;
                outcome.Receptions.Add(new ReceptionLog(who,ReceptionResult.SystemFault,false,l.Reachable,l.Defensive,l.DeltaMs,l.SlackS,pendingShot.Option,l.ContactStep,solution.Reason??"contact-not-legal",causal,l.FatigueReception,arm.Factor,l.Volley,l.VolleyOffered,l.VolleySprintFactor));
                return;
            }
            var previous=pendingLog;
            outcome.Receptions.Add(new ReceptionLog(who,previous.Result,previous.Perfect,previous.Reachable,previous.Defensive,previous.DeltaMs,previous.SlackS,pendingShot.Option,previous.ContactStep,previous.Reason,causal,previous.FatigueReception,arm.Factor,previous.Volley,previous.VolleyOffered,previous.VolleySprintFactor,state.Position));
            // The racket only replaces the velocity of the authoritative state at the contact step.
            ball.Reset(new BallState{Position=state.Position,Velocity=solution.Velocity,Step=state.Step});
            // A contact before the bounce also costs the volley energy (19 nov); bots never volley by default, so nothing changes for them.
            double volleyCost=context.VolleyEnergyCost(Rules);
            // v2.x: a sprint volley of a touching human also pays its lateness cost (zero for everybody else).
            if(who==PlayerId.Human){if(volleyCost>0)volleyCost+=pendingSprintCost;pendingSprintCost=0;Human.Sprint=1;}
            Rules.RacketContact(who);Energy(who).Strike(pendingShot.P);Energy(who).Volley(volleyCost);
            Body(who).MoveTo(RecoveryOf(who),0,BodyPhase.Recovering,BodyPhase.Waiting);
            BeginReception(who.Other(),outcome);
        }
        // The v2.x choice of a touching human, exactly as RallyDirector makes it: the offer from the body at the reception start, the touch
        // inside [launch, S'], then the longest-window stance reachable without the sprint, otherwise with it. The body turns at the touch.
        // fatigue: the human's factor at this reception start; it limits the sprint (fatigue feel, section 1).
        private void ChooseHumanVolley(IRallyVolleyToucher toucher,Trajectory trajectory,in ReceptionPlan plan,PlayerBody body,int delay,double fatigue,
            out VolleyIntercept chosen,out Vector2[] chosenPath,out bool offered,out float sprintUsed)
        {
            chosen=default;chosenPath=null;sprintUsed=1;float maxSprint=context.HumanVolleySprintAt(fatigue);
            var detail=HumanVolley.OfferDetail(context,trajectory,Rules,body,delay,planVersion,fatigue);offered=detail.Offer.Exists;
            if(!offered)return;
            long first=trajectory.States[0].Step;double perMs=context.StepHz*context.Tempo/1000;
            double bounceMs=plan.Contact.Exists&&plan.Contact.TStarStep>=0?(plan.Contact.TStarStep-first)/perMs:double.NaN;
            double touch=toucher.TouchMsAfterLaunch(plan,bounceMs);
            if(double.IsNaN(touch)||touch<0||touch>(detail.SPrime-first)/perMs)return;
            long c=first+(long)Math.Floor(touch*perMs);var walk=body.Clone();for(long k=first;k<c;k++)walk.Step();
            var p=HumanVolley.Commit(context,trajectory,Rules,walk,c,delay,planVersion);float sprint=1;
            if(!p.Exists&&maxSprint>1&&c<=detail.SPrime)
            {var sp=HumanVolley.Commit(context,trajectory,Rules,walk,c,delay,planVersion,maxSprint);if(sp.Exists){p=sp;sprint=maxSprint;}}
            if(!p.Exists)return;
            chosen=p;sprintUsed=sprint;hasRedirect=true;redirectStep=c;redirectStance=p.Stance;redirectDelay=(int)Math.Max(0,first+delay-c);redirectSprint=sprint;
            pendingSprintCost=sprint>1?context.Params.HumanVolleySprintCost*HumanVolley.Lateness(detail.S,detail.SPrime,c):0;
            // The causal reference reads the body's real path: M1 until the touch, then towards the stance.
            int n=trajectory.States.Count,until=(int)(c-first);var before=body.PositionsAfter(Math.Min(n,until));
            var turned=walk.Clone();turned.Sprint=sprint;turned.MoveTo(p.Stance,redirectDelay,BodyPhase.Approaching,BodyPhase.Set);
            var after=turned.PositionsAfter(Math.Max(0,n-until));chosenPath=new Vector2[n];
            Array.Copy(before,chosenPath,before.Length);Array.Copy(after,0,chosenPath,before.Length,Math.Min(after.Length,n-before.Length));
        }
        public void Dispose()=>ball.Dispose();
    }
}
