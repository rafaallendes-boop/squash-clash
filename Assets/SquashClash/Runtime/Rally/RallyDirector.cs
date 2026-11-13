using System;
using System.Collections.Generic;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Rally
{
    public enum RallyPhase { Off, HumanReception, RivalReception, PointOver, MatchOver }
    public readonly struct PointResult
    {
        public readonly PlayerId? Winner;public readonly PointEndCause Cause;public readonly int Shots;
        // Why the loser did not return the ball on a double bounce (Air, Late, Unreachable, NoSwing...).
        public readonly ReceptionResult? Reception;
        public PointResult(PlayerId? winner,PointEndCause cause,int shots,ReceptionResult? reception){Winner=winner;Cause=cause;Shots=shots;Reception=reception;}
    }
    // First Rally loop on top of the existing practice shot. The host stays the only ball authority: the director
    // never moves the ball, it only asks the host to launch (neutral feed and every rival stroke). The human's stroke
    // goes through ShotController untouched; the rival's goes through the same contract and solver (RallyContact).
    // State for views: Human / Rival (position, velocity, phase, target), HumanPlan / RivalPlan, Phase, Rules.
    public sealed class RallyDirector : MonoBehaviour
    {
        private AppBootstrap app;
        private ShotController controller;
        private PracticeMode practice;
        private PlaceholderPlayers players;
        private AnalyticBallSimulationFactory factory;
        private RallyContext context;
        private PointRules committed;
        private IRallyBrain rivalBrain,humanBrain;
        private ShotRecord resolved;
        private AiShot pendingShot,autoShot;
        private CausalReference rivalCausal;
        private readonly List<CausalShotTrace> pointCausal=new List<CausalShotTrace>();
        private bool launching,hasPending,needDecision,autoQueued;
        private long flightLaunchId,flightStartStep,bodyStep,pendingStep;
        private int planVersion,autoSerial;
        private double closedSeenAt=double.NaN,lastObserved,waited,bodyCarry;
        private ReceptionResult? rivalReception;
        private PlayerId? served;private ServiceBox servedBox;
        public RallyParams Params {get;private set;}=new RallyParams();
        public AiLevel Level {get;set;}=AiLevel.Medium;
        public long Seed {get;set;}=1;
        // Lets a scripted bot play the human side through the normal order queue (demo and tests).
        public bool Autopilot {get;set;}
        public double AutopilotTimingSigmaMs {get;set;}=25;
        public bool AutoRestart {get;set;}=true;
        public bool Active {get;private set;}
        public RallyPhase Phase {get;private set;}
        public PointRules Rules {get;private set;}
        public PlayerBody Human {get;private set;}
        public PlayerBody Rival {get;private set;}
        public ReceptionPlan HumanPlan {get;private set;}
        public ReceptionPlan RivalPlan {get;private set;}
        public int HumanPoints {get;private set;}
        public int RivalPoints {get;private set;}
        public int PointsPlayed {get;private set;}
        public int SystemFaults {get;private set;}
        public PointResult? LastPoint {get;private set;}
        // Null in the plain rally (neutral launch, open score). Set in match mode: serve, rotation, game to 7 by 2.
        public MatchScore Match {get;private set;}
        public double WaitBetweenPoints=>Match!=null?Params.MatchPauseSeconds:Params.RestartSeconds;
        public event Action RallyStarted;
        public event Action<PointResult> PointEnded;
        // Rival swing: stroke side and whether it makes contact. The human's swing already has ShotController.SwingAccepted.
        public event Action<StrokeSide,bool> RivalSwing;
        public event Action<bool> ModeChanged;
        public void Initialize(AppBootstrap app,ShotController controller,PracticeMode practice,PlaceholderPlayers players)
        {
            if(this.app!=null)return;
            this.app=app;this.controller=controller;this.practice=practice;this.players=players;
            factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            app.Host.Launched+=OnLaunched;controller.ShotResolved+=OnResolved;
        }
        public void SetParams(RallyParams parameters){Params=parameters??throw new ArgumentNullException(nameof(parameters));}
        public bool CanStart=>app!=null&&app.Host.ActiveConfig!=null&&app.Host.ActiveConfig.Backend=="P2-Analytic"&&app.Tuning.Applied.GetBool("practice.enabled");
        public bool StartMode()
        {
            if(Active)return true;
            if(!CanStart)return false;
            Active=true;practice.enabled=false;Human=Rival=null;Match=null;ResetScore();ModeChanged?.Invoke(true);
            BeginPoint();return true;
        }
        // First Match: every point starts with the automatic serve; the human serves first.
        public bool StartMatch()
        {
            if(Active)StopMode(false);
            if(!CanStart)return false;
            Active=true;practice.enabled=false;Human=Rival=null;ResetScore();Match=NewMatch();ModeChanged?.Invoke(true);
            BeginPoint();return true;
        }
        private MatchScore NewMatch()=>new MatchScore((int)Params.MatchTarget,(int)Params.MatchMargin,PlayerId.Human);
        // Skips the rest of the pause between points; the next tick starts the point.
        public void SkipWait(){if(Active&&Phase==RallyPhase.PointOver)waited=double.MaxValue/4;}
        public void StopMode()=>StopMode(true);
        private void StopMode(bool relaunchPractice)
        {
            if(!Active)return;
            Active=false;Phase=RallyPhase.Off;hasPending=needDecision=false;practice.enabled=true;ModeChanged?.Invoke(false);
            if(relaunchPractice)app.LaunchCurrent();
        }
        // Rematch: same rival, score back to zero, next point starts at once.
        public void ResetScore(){HumanPoints=RivalPoints=PointsPlayed=0;LastPoint=null;}
        public void Rematch(){if(!Active)return;ResetScore();if(Match!=null)Match=NewMatch();BeginPoint();}
        private void BeginPoint()
        {
            var cfg=EffectiveConfig.From(app.Tuning.ApplyPending());
            context=new RallyContext(cfg,Params,factory);
            Human=new PlayerBody(PlayerId.Human,Human?.Position??context.WaitPosition(PlayerId.Human),context.Move);
            Rival=new PlayerBody(PlayerId.Rival,Rival?.Position??context.WaitPosition(PlayerId.Rival),context.Move);
            Rival.MoveTo(context.WaitPosition(PlayerId.Rival),0,BodyPhase.Recovering,BodyPhase.Waiting);
            rivalBrain=new FondoAi(context,AiDifficulty.For(Params,Level),Seed+PointsPlayed*7919L);
            humanBrain=new BotBrain(context,AutopilotTimingSigmaMs,Seed*31+PointsPlayed*104729L);
            committed=new PointRules(context.Court,(float)Params.FootprintRadius);rivalReception=null;served=null;
            if(Match==null){committed.Begin(null,PlayerId.Human);LaunchToHuman(context.NeutralLaunch(PlayerId.Human));}
            else
            {
                // Automatic placement in the box and automatic serve for both players (spec 8.7.5 adaptation).
                var server=Match.Server;var box=Match.Box;var body=server==PlayerId.Human?Human:Rival;served=server;servedBox=box;
                committed.BeginServe(server,box);body.Teleport(ServePlanner.BoxPosition(context,box));
                body.MoveTo(context.WaitPosition(server),0,BodyPhase.Recovering,BodyPhase.Waiting);
                if(server==PlayerId.Rival)LaunchToHuman(context.Serve(box));else LaunchToRival(context.Serve(box));
            }
            RallyStarted?.Invoke();
        }
        private void LaunchToHuman(BallState ball)
        {
            var trajectory=context.Predict(ball);int delay=context.Steps(Params.ReactionS);
            HumanPlan=ReceptionPlanner.Plan(context,trajectory,committed,Human,delay,++planVersion);
            Human.MoveTo(HumanPlan.Stance,delay,BodyPhase.Approaching,BodyPhase.Set);
            var positions=Human.PositionsAfter(trajectory.States.Count);
            controller.SetNextReceptionBody(j=>context.FrameAt(positions[(int)(j-ball.Step)]));
            // The fixed-body contact planner reads the human from the launch configuration, as practice already does.
            var at=HumanPlan.BodyAtContact;
            var source=app.Tuning.ApplyPending().With("human.x",at.x).With("human.z",at.y);
            launching=true;
            try{app.Host.Launch(EffectiveConfig.From(source),ball,context.StepHz*10,StopCondition.SecondFloorBounce);}
            finally{launching=false;}
            flightLaunchId=app.Host.LaunchId;flightStartStep=ball.Step;bodyStep=app.Host.Current.Step;bodyCarry=0;lastObserved=app.Host.ObservedRealTime;
            Phase=RallyPhase.HumanReception;resolved=null;closedSeenAt=double.NaN;hasPending=needDecision=autoQueued=false;autoShot=default;
            Rules=committed.Clone();
            if(Autopilot)
            {
                var plan=controller.Plan;
                if(plan.Exists&&plan.TStarStep>=0)
                    autoShot=humanBrain.Decide(Observe(PlayerId.Human,ball),new ReceptionPlan(true,HumanPlan.Stance,at,plan,HumanPlan.ArrivalStep,HumanPlan.SlackS,HumanPlan.Quality,"autopilot"));
            }
            PlaceBodies();
        }
        // The human's own serve: the same host launch, with the practice shot switched off for this flight so the
        // human cannot strike the ball that is not theirs to hit (turn of stroke).
        private void LaunchToRival(BallState ball)
        {
            var source=app.Tuning.ApplyPending().With("practice.enabled",0);
            launching=true;
            try{app.Host.Launch(EffectiveConfig.From(source),ball,context.StepHz*10,StopCondition.SecondFloorBounce);}
            finally{launching=false;}
            flightLaunchId=app.Host.LaunchId;flightStartStep=ball.Step;bodyStep=app.Host.Current.Step;bodyCarry=0;lastObserved=app.Host.ObservedRealTime;
            Phase=RallyPhase.RivalReception;resolved=null;closedSeenAt=double.NaN;hasPending=needDecision=autoQueued=false;autoShot=default;
            Rules=committed.Clone();PlanRival();PlaceBodies();
        }
        private AiObservation Observe(PlayerId who,BallState ball)
        {
            var self=who==PlayerId.Human?Human:Rival;var other=who==PlayerId.Human?Rival:Human;
            return new AiObservation(ball,self.Position,self.Velocity,other.Position,other.Velocity,committed.Shots,committed.FloorBounces,committed.FrontWallReached,
                who==PlayerId.Human?HumanPoints:RivalPoints,who==PlayerId.Human?RivalPoints:HumanPoints);
        }
        private void OnLaunched(EffectiveConfig cfg)
        {
            if(!Active)return;
            // Any launch the director did not ask for (DEV buttons, presets, experiments) hands the court back to practice.
            if(!launching){StopMode(false);return;}
        }
        private void OnResolved(ShotRecord record)
        {
            if(Active&&Phase==RallyPhase.HumanReception&&record.LaunchId==flightLaunchId)
            {
                resolved=record;
                if(controller.LastCausal.Vectors!=null&&controller.CausalReference!=null)
                    pointCausal.Add(CausalShotTrace.Create("Human",record.LaunchId,record.AttemptId,record.ContactStep,controller.CausalReference,controller.CausalReference.BodyAt(record.ContactStep),controller.LastCausal));
            }
        }
        private void Update()=>Tick();
        // One evaluation at the host's observed present. Tests call it directly after Host.Advance.
        public void Tick()
        {
            if(!Active||app==null)return;
            var host=app.Host;
            if(host.LaunchId!=flightLaunchId){StopMode(false);return;}
            // Real time elapsed since the previous tick; zero while paused, so nothing drifts under a pause.
            double elapsed=host.Clock.Paused?0:Math.Max(0,host.ObservedRealTime-lastObserved);lastObserved=host.ObservedRealTime;
            StepBodies(elapsed);
            // A rally that reaches the configured stroke limit is cut with no winner (never seen with human timing).
            if(Phase!=RallyPhase.PointOver&&Phase!=RallyPhase.MatchOver&&committed.Shots>=Params.MaxShots){Rules=committed.Clone();if(Phase==RallyPhase.HumanReception)controller.CloseReception();EndPoint(null);}
            else if(Phase==RallyPhase.HumanReception)TickHuman();
            else if(Phase==RallyPhase.RivalReception)TickRival();
            else if(Phase==RallyPhase.PointOver)TickPointOver(elapsed);
            if(Active)PlaceBodies();
        }
        private void StepBodies(double elapsed)
        {
            var host=app.Host;long now=host.Current.Step;int steps;
            if(host.IsRunning||now!=bodyStep){steps=(int)Math.Max(0,Math.Min(now-bodyStep,context.StepHz));bodyCarry=0;}
            else
            {
                // The ball is dead: bodies keep walking back on the same tempo, in whole steps.
                bodyCarry=Math.Min(bodyCarry+elapsed*context.Tempo*context.StepHz,context.StepHz);
                steps=(int)bodyCarry;bodyCarry-=steps;
            }
            bodyStep=now;
            for(int i=0;i<steps;i++){Human.Step();Rival.Step();}
        }
        private PointRules Evaluate(long upToStep)
        {
            var host=app.Host;var rules=committed.Clone();
            foreach(var e in host.CanonicalContacts)
            {
                if(e.Step<=flightStartStep)continue;
                if(e.Step>upToStep)break;
                rules.OnBallContact(e);
            }
            if(upToStep==long.MaxValue&&host.Current.Position.y>context.Court.WallHeight)rules.OnOverTop();
            return rules;
        }
        private void TickHuman()
        {
            var host=app.Host;
            if(Autopilot&&autoShot.Swing&&!autoQueued&&!controller.AttemptConsumed&&host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out double target))
            {
                double release=target+autoShot.DeltaMs/1000+context.Timing.LatencyCompMs/1000;
                if(host.ObservedRealTime>=release)
                {
                    autoQueued=true;release=Math.Max(release,host.LaunchRealTime);
                    controller.Enqueue(new ClosedGesture(host.LaunchId,"auto-"+(++autoSerial),0,Math.Max(host.LaunchRealTime,release-.15),release,true,GestureOutcome.Swing,
                        autoShot.Aim,autoShot.P,controller.Snapshot.Reception,"autopilot",host.ObservedRealTime));
                }
            }
            if(resolved!=null&&resolved.Applied)
            {
                long contactStep=resolved.ContactStep;var at=Evaluate(contactStep);
                if(at.RacketContact(PlayerId.Human))
                {
                    committed=at;flightStartStep=contactStep;Phase=RallyPhase.RivalReception;resolved=null;
                    Human.MoveTo(context.WaitPosition(PlayerId.Human),0,BodyPhase.Recovering,BodyPhase.Waiting);
                    PlanRival();TickRival();return;
                }
                // The contract and the rules disagree: a system fault, never a silent contact.
                SystemFaults++;resolved=null;
            }
            Rules=Evaluate(long.MaxValue);
            if(!Rules.Closed){if(!host.IsRunning&&!controller.WaitingForContact)EndPoint(null);return;}
            if(Rules.Cause==PointEndCause.DoubleBounce&&Rules.Winner==PlayerId.Rival)
            {
                // A release stamped before the second bounce can still arrive within the rollback window.
                bool final=resolved!=null||(controller.AttemptConsumed&&!controller.WaitingForContact);
                if(!final)
                {
                    if(double.IsNaN(closedSeenAt))closedSeenAt=host.ObservedRealTime;
                    if(controller.WaitingForContact||host.ObservedRealTime-closedSeenAt<host.ActiveConfig.Source.Get("rollback.historyMs")/1000)return;
                }
            }
            controller.CloseReception();
            EndPoint(HumanReceptionResult());
        }
        private ReceptionResult? HumanReceptionResult()
        {
            var record=resolved??controller.LastRecord;
            if(record==null||record.LaunchId!=flightLaunchId)return ReceptionResult.NoSwing;
            if(record.Result==nameof(ShotResult.Failed))return record.Fail==nameof(FailKind.Air)?ReceptionResult.Air:ReceptionResult.Late;
            if(record.Result==nameof(ShotResult.Unreachable))return ReceptionResult.Unreachable;
            if(record.Result==nameof(ShotResult.NoOrder))return ReceptionResult.NoSwing;
            if(record.Result==nameof(ShotResult.SystemFault))return ReceptionResult.SystemFault;
            return ReceptionResult.NotEvaluable;
        }
        private void PlanRival()
        {
            var host=app.Host;var trajectory=host.Prediction;long start=trajectory.States[0].Step;
            int elapsed=(int)Math.Max(0,host.Current.Step-start);
            int delay=Math.Max(0,context.Steps(Params.ReactionS)+rivalBrain.PerceptionSteps-elapsed);
            RivalPlan=ReceptionPlanner.Plan(context,trajectory,committed,Rival,delay,++planVersion,rivalBrain.StanceNoise(),elapsed);
            Rival.MoveTo(RivalPlan.Stance,delay,BodyPhase.Approaching,BodyPhase.Set);
            rivalCausal=null;
            if(RivalPlan.Contact.Exists&&RivalPlan.Contact.TStarStep>=0&&host.ClockMap.TrySimToReal(RivalPlan.Contact.TStarStep,0,out _))
            {
                var future=Rival.PositionsAfter(trajectory.States.Count-elapsed);var past=new Vector2[trajectory.States.Count];
                // Before this presentation the receiver has no attempt; retain its present position for that prefix.
                for(int i=0;i<past.Length;i++)past[i]=i<elapsed?Rival.Position:future[i-elapsed];
                rivalCausal=CausalQuality.Prepare(trajectory,RivalPlan.Contact,host.ClockMap,context.Timing,host.ObservedRealTime,context.Tempo,j=>context.FrameAt(past[(int)(j-start)]));
            }
            pendingShot=rivalBrain.Decide(Observe(PlayerId.Rival,trajectory.States[0]),RivalPlan);
            hasPending=false;needDecision=pendingShot.Swing;
            rivalReception=pendingShot.Swing?(ReceptionResult?)null:RivalPlan.Reachable?ReceptionResult.NoSwing:ReceptionResult.Unreachable;
            if(needDecision)DecideRival();
        }
        private void DecideRival()
        {
            var host=app.Host;
            var decision=RallyContact.Resolve(context,host.ClockMap,host.Prediction,RivalPlan.Contact,host.ObservedRealTime,pendingShot.DeltaMs);
            // A paused clock cannot place a future instant yet: ask again on the next tick, same single attempt.
            if(decision.Result==ShotResult.NotEvaluable&&decision.Reason=="target-time-unmapped")return;
            needDecision=false;
            if(decision.Result==ShotResult.Perfect||decision.Result==ShotResult.Valid){hasPending=true;pendingStep=decision.ContactStep;return;}
            rivalReception=decision.Result==ShotResult.Failed?(decision.Fail==FailKind.Air?ReceptionResult.Air:ReceptionResult.Late):
                decision.Result==ShotResult.Unreachable?ReceptionResult.Unreachable:decision.Result==ShotResult.SystemFault?ReceptionResult.SystemFault:ReceptionResult.NotEvaluable;
            RivalSwing?.Invoke(RivalPlan.Contact.Side,false);
        }
        private void TickRival()
        {
            var host=app.Host;
            if(needDecision)DecideRival();
            if(hasPending&&host.Current.Step>=pendingStep)
            {
                hasPending=false;var at=Evaluate(pendingStep);var state=host.Current;
                bool have=state.Step==pendingStep||host.TryGetHistoricalState(pendingStep,out state);
                var causal=have&&at.CanHit(PlayerId.Rival)&&rivalCausal!=null?RallyContact.Solve(context,rivalCausal,rivalCausal.BodyAt(pendingStep),state.Position,pendingShot.Aim,pendingShot.P):default;
                var solution=causal.Solution;
                if(causal.Vectors!=null)
                {
                    var trace=CausalShotTrace.Create("Rival",host.LaunchId,"",pendingStep,rivalCausal,rivalCausal.BodyAt(pendingStep),causal);pointCausal.Add(trace);
                    app.Metrics?.Write("rally-causal",host.ActiveConfig,app.ActivePresetName,app.ActivePresetFileHash,app.Tuning.PendingKeys.Count,app.MetricsSeed,!app.Clock.MetricsInvalid,app.Clock.DroppedSinceRestart,JsonUtility.ToJson(trace));
                }
                if(solution.Solved&&at.RacketContact(PlayerId.Rival))
                {
                    committed=at;rivalReception=ReceptionResult.Contact;RivalSwing?.Invoke(RivalPlan.Contact.Side,true);
                    Rival.MoveTo(context.WaitPosition(PlayerId.Rival),0,BodyPhase.Recovering,BodyPhase.Waiting);
                    // The rival's stroke: a new launch from the exact state of its contact step.
                    LaunchToHuman(new BallState{Position=state.Position,Velocity=solution.Velocity,Step=0});
                    return;
                }
                SystemFaults++;rivalReception=ReceptionResult.SystemFault;
            }
            Rules=Evaluate(long.MaxValue);
            if(Rules.Closed||!host.IsRunning)EndPoint(rivalReception);
        }
        private void EndPoint(ReceptionResult? reception)
        {
            var rules=Rules;Phase=RallyPhase.PointOver;waited=0;hasPending=needDecision=false;
            if(rules.Winner==PlayerId.Human)HumanPoints++;else if(rules.Winner==PlayerId.Rival)RivalPoints++;
            PointsPlayed++;
            // A point with no winner is replayed from the same box; the game ends once and stays ended.
            if(Match!=null&&rules.Winner.HasValue)Match.PointWon(rules.Winner.Value);
            if(Match!=null&&Match.Finished)Phase=RallyPhase.MatchOver;
            var result=new PointResult(rules.Winner,rules.Cause,rules.Shots,rules.Cause==PointEndCause.DoubleBounce?reception:null);
            LastPoint=result;
            Human.MoveTo(context.WaitPosition(PlayerId.Human),0,BodyPhase.Recovering,BodyPhase.Waiting);
            Rival.MoveTo(context.WaitPosition(PlayerId.Rival),0,BodyPhase.Recovering,BodyPhase.Waiting);
            WriteMetrics(result);
            PointEnded?.Invoke(result);
        }
        private void TickPointOver(double elapsed)
        {
            waited+=elapsed;
            if(AutoRestart&&waited>=WaitBetweenPoints)BeginPoint();
        }
        private void PlaceBodies(){if(players!=null&&Human!=null)players.Place(Human.Position,Rival.Position);}
        [Serializable] private sealed class PointLine
        {public string winner,cause,reception,rallyParams,level,server,box;public int shots,humanPoints,rivalPoints,systemFaults;public bool autopilot,match,matchOver;public long seed;public CausalShotTrace[] causal;}
        private void WriteMetrics(PointResult r)
        {
            var line=new PointLine{winner=r.Winner?.ToString()??"None",cause=r.Cause.ToString(),reception=r.Reception?.ToString()??"",shots=r.Shots,humanPoints=HumanPoints,rivalPoints=RivalPoints,
                systemFaults=SystemFaults,rallyParams=Params.Hash,level=Level.ToString(),autopilot=Autopilot,seed=Seed,match=Match!=null,matchOver=Match?.Finished??false,
                server=served?.ToString()??"",box=served.HasValue?servedBox.ToString():"",causal=pointCausal.ToArray()};
            app.Metrics?.Write("rally-point",app.Host.ActiveConfig,app.ActivePresetName,app.ActivePresetFileHash,app.Tuning.PendingKeys.Count,app.MetricsSeed,
                !app.Clock.MetricsInvalid,app.Clock.DroppedSinceRestart,JsonUtility.ToJson(line));
        }
        private void OnDestroy()
        {
            if(app==null)return;
            if(app.Host!=null)app.Host.Launched-=OnLaunched;
            if(controller!=null)controller.ShotResolved-=OnResolved;
        }
    }
}
