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
        public ReceptionLog(PlayerId who,ReceptionResult result,bool perfect,bool reachable,bool defensive,double deltaMs,double slack,ShotOption option,long contactStep,string reason,CausalShotResult causal=default,double fatigueReception=0,double fatigueStriker=0,bool volley=false,bool volleyOffered=false)
        {Volley=volley;VolleyOffered=volleyOffered;FatigueReception=fatigueReception;FatigueStriker=fatigueStriker;Vectors=causal.Vectors;Input=causal.Input;Who=who;Result=result;Perfect=perfect;Reachable=reachable;Defensive=defensive;DeltaMs=deltaMs;SlackS=slack;Line=option.Line;Height=option.Height;Power=option.Power;ContactStep=contactStep;Reason=reason;}
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
        public FatigueState HumanEnergy {get;private set;}
        public FatigueState RivalEnergy {get;private set;}
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
            Human=new PlayerBody(PlayerId.Human,context.WaitPosition(PlayerId.Human),context.Move);
            Rival=new PlayerBody(PlayerId.Rival,context.WaitPosition(PlayerId.Rival),context.Move);
        }
        private PlayerBody Body(PlayerId who)=>who==PlayerId.Human?Human:Rival;
        private FatigueState Energy(PlayerId who)=>who==PlayerId.Human?HumanEnergy:RivalEnergy;
        private void BeginFatiguePoint()
        {HumanEnergy.BeginPoint();RivalEnergy.BeginPoint();Human.MovementFactor=Rival.MovementFactor=1;}
        public RallyOutcome Play(PlayerId firstReceiver)
        {
            BeginFatiguePoint();
            var outcome=new RallyOutcome{FirstReceiver=firstReceiver};
            Human.Teleport(context.WaitPosition(PlayerId.Human));Rival.Teleport(context.WaitPosition(PlayerId.Rival));
            ball.Reset(context.NeutralLaunch(firstReceiver));Rules.Begin(null,firstReceiver);
            return Run(outcome,firstReceiver);
        }
        // A point that starts with the automatic serve from the server's box (spec 8.5).
        public RallyOutcome PlayPoint(PlayerId server,ServiceBox box)
        {
            BeginFatiguePoint();Energy(server).Strike(0); // Automatic serve intention (spec v2.3).
            var outcome=new RallyOutcome{FirstReceiver=server.Other(),Server=server,Box=box};
            Human.Teleport(context.WaitPosition(PlayerId.Human));Rival.Teleport(context.WaitPosition(PlayerId.Rival));
            Body(server).Teleport(ServePlanner.BoxPosition(context,box));
            Body(server).MoveTo(context.WaitPosition(server),0,BodyPhase.Recovering,BodyPhase.Waiting);
            ball.Reset(context.Serve(box));Rules.BeginServe(server,box);
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
            hasPending=false;
            long cap=context.Steps(context.Params.MaxRallySeconds),steps=0;long stall=context.Steps(context.Params.StallSeconds),flight=0;int shots=0;
            BeginReception(firstReceiver,outcome);
            while(!Rules.Closed)
            {
                if(steps>=cap){outcome.End=RallyEnd.StepCap;break;}
                if(Rules.Shots>=context.Params.MaxShots){outcome.End=RallyEnd.ShotCap;break;}
                if(hasPending&&ball.State.Step==pendingStep)Strike(outcome);
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
            LastPlan=plan;
            var shot=brain.Decide(new AiObservation(state,body.Position,body.Velocity,other.Position,other.Velocity,Rules.Shots,Rules.FloorBounces,Rules.FrontWallReached),plan);
            hasPending=false;
            if(!shot.Swing)
            {
                outcome.Receptions.Add(new ReceptionLog(who,plan.Reachable?ReceptionResult.NoSwing:ReceptionResult.Unreachable,false,plan.Reachable,false,0,plan.SlackS,default,-1,plan.Reason,fatigueReception:f,volleyOffered:plan.Volley.Exists));
                return;
            }
            // Real perceived time for this flight: the same clock map type the touch path uses.
            double now=state.Step/(context.Tempo*context.StepHz);
            var clock=new ClockMap();clock.Begin(++launchId,now,state.Step,context.Tempo,1,context.StepHz);
            bool useVolley=shot.Volley&&plan.Volley.Exists;var contact=useVolley?plan.Volley.Plan:plan.Contact;
            if(contact.Exists&&contact.TStarStep>=0)
            {
                var positions=path??body.PositionsAfter(trajectory.States.Count);long first=trajectory.States[0].Step;
                pendingCausal=CausalQuality.Prepare(trajectory,contact,clock,context.Timing,now,context.Tempo,j=>context.FrameAt(positions[(int)(j-first)]));
            }
            var decision=RallyContact.Resolve(context,clock,trajectory,contact,now,shot.DeltaMs);
            ReceptionResult result;
            switch(decision.Result)
            {
                case ShotResult.Perfect:case ShotResult.Valid:result=ReceptionResult.Contact;break;
                case ShotResult.Failed:result=decision.Fail==FailKind.Air?ReceptionResult.Air:ReceptionResult.Late;break;
                case ShotResult.Unreachable:result=ReceptionResult.Unreachable;break;
                case ShotResult.SystemFault:result=ReceptionResult.SystemFault;outcome.SystemFaults++;break;
                default:result=ReceptionResult.NotEvaluable;break;
            }
            var log=new ReceptionLog(who,result,decision.Result==ShotResult.Perfect,plan.Reachable,shot.Defensive,decision.DeltaMs,plan.SlackS,shot.Option,decision.ContactStep,decision.Reason,fatigueReception:f,volley:useVolley,volleyOffered:plan.Volley.Exists);
            if(result!=ReceptionResult.Contact){outcome.Receptions.Add(log);return;}
            hasPending=true;pendingStep=decision.ContactStep;pendingShot=shot;pendingWho=who;pendingLog=log;
        }
        private void Strike(RallyOutcome outcome)
        {
            hasPending=false;var who=pendingWho;var state=ball.State;
            // The arm is the striker's own energy at this contact step, read before this stroke's cost is charged below.
            var arm=context.Fatigue.Arm(Energy(who).Energy);
            var causal=Rules.CanHit(who)?RallyContact.Solve(context,pendingCausal,context.FrameAt(Body(who).Position),state.Position,pendingShot.Aim,pendingShot.P,arm):default;
            var solution=causal.Solution;
            if(!solution.Solved)
            {
                outcome.SystemFaults++;var l=pendingLog;
                outcome.Receptions.Add(new ReceptionLog(who,ReceptionResult.SystemFault,false,l.Reachable,l.Defensive,l.DeltaMs,l.SlackS,pendingShot.Option,l.ContactStep,solution.Reason??"contact-not-legal",causal,l.FatigueReception,arm.Factor,l.Volley,l.VolleyOffered));
                return;
            }
            var previous=pendingLog;
            outcome.Receptions.Add(new ReceptionLog(who,previous.Result,previous.Perfect,previous.Reachable,previous.Defensive,previous.DeltaMs,previous.SlackS,pendingShot.Option,previous.ContactStep,previous.Reason,causal,previous.FatigueReception,arm.Factor,previous.Volley,previous.VolleyOffered));
            // The racket only replaces the velocity of the authoritative state at the contact step.
            ball.Reset(new BallState{Position=state.Position,Velocity=solution.Velocity,Step=state.Step});
            Rules.RacketContact(who);Energy(who).Strike(pendingShot.P);
            Body(who).MoveTo(context.WaitPosition(who),0,BodyPhase.Recovering,BodyPhase.Waiting);
            BeginReception(who.Other(),outcome);
        }
        public void Dispose()=>ball.Dispose();
    }
}
