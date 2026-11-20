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
        private ContactDecision rivalDecision;
        private CausalReference rivalCausal,rivalCausalVolley;private bool rivalUsedVolley;
        private Func<long,BodyFrame> rivalCausalBody;
        private readonly List<CausalShotTrace> pointCausal=new List<CausalShotTrace>();
        private bool launching,hasPending,needDecision,autoQueued;
        private long flightLaunchId,flightStartStep,bodyStep,pendingStep;
        private int planVersion,autoSerial;
        private double closedSeenAt=double.NaN,lastObserved,waited,bodyCarry;
        private ReceptionResult? rivalReception;
        private PlayerId? served;private ServiceBox servedBox;
        private bool resetEnergy=true;
        private double? testEnergy;
        private bool? pendingVolley;
        public FatigueState HumanEnergy {get;private set;}
        public FatigueState RivalEnergy {get;private set;}
        public FatigueSettings FatigueSettings=>context?.Fatigue;
        public double HumanReceptionFatigue {get;private set;}
        public double RivalReceptionFatigue {get;private set;}
        public RallyParams Params {get;private set;}=new RallyParams();
        public AiLevel Level {get;set;}=AiLevel.Medium;
        // Rival styles (design 2026-11-19). A match picks its style once, at StartMatch: the DEV override if set, otherwise a random
        // style when RandomStyles is on (the start menu), otherwise Fondista (tests and the plain rally stay deterministic).
        public RivalStyleId Style {get;private set;}=RivalStyleId.Fondista;
        public RivalStyleId? StyleOverride {get;set;}
        public bool RandomStyles {get;set;}
        // Seeds the match's style and the rival's random stream; equal to Seed unless RandomStyles draws a new one per match.
        public long MatchSeed {get;private set;}=1;
        public int RivalVolleys {get;private set;}
        // Random styles draw from this counter, so a sequence of matches is reproducible (the start menu keeps it in PlayerPrefs).
        public int StyleCounter {get;set;}
        public long Seed {get;set;}=1;
        // Lets a scripted bot play the human side through the normal order queue (demo and tests).
        public bool Autopilot {get;set;}
        public double AutopilotTimingSigmaMs {get;set;}=25;
        // The autopilot's fixed intention (demo and tests); the default is the plain drive it always played.
        public ShotLine AutopilotLine {get;set;}=ShotLine.Straight;
        public ShotPower AutopilotPower {get;set;}=ShotPower.Medium;
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
        // Visual read-outs for the rival's figure (volley pose, 2026-11-20). They are written only right before RivalSwing is raised and
        // nothing in the rally reads them: the rival's decisions, timing and positions do not depend on them.
        public bool RivalSwingVolley {get;private set;}
        public float RivalSwingHeight {get;private set;}
        // The rival will volley this reception (its aimed volley exists): the figure arms a short backswing instead of the full one.
        public bool RivalVolleyIntended=>Active&&Phase==RallyPhase.RivalReception&&pendingShot.Swing&&pendingShot.Volley&&RivalPlan.Volley.Exists;
        private void RaiseRivalSwing(bool contact)
        {
            RivalSwingVolley=rivalUsedVolley;RivalSwingHeight=rivalUsedVolley?RivalPlan.Volley.Plan.PStar.y:0;
            RivalSwing?.Invoke(rivalUsedVolley?RivalPlan.Volley.Plan.Side:RivalPlan.Contact.Side,contact);
        }
        public event Action<bool> ModeChanged;
        public void Initialize(AppBootstrap app,ShotController controller,PracticeMode practice,PlaceholderPlayers players)
        {
            if(this.app!=null)return;
            this.app=app;this.controller=controller;this.practice=practice;this.players=players;
            factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            app.Host.Launched+=OnLaunched;controller.ShotResolved+=OnResolved;controller.SideAimGate=()=>Active;
            // View-only hold of the rendered pose for 100 ms inside the pause (PO, 19 nov); touches no simulation state.
            PointEnded+=_=>SquashClash.Runtime.Visual.PointEndFreeze.Begin(WaitBetweenPoints,()=>Active&&(Phase==RallyPhase.PointOver||Phase==RallyPhase.MatchOver));
        }
        public void SetParams(RallyParams parameters){Params=parameters??throw new ArgumentNullException(nameof(parameters));}
        public bool CanStart=>app!=null&&app.Host.ActiveConfig!=null&&app.Host.ActiveConfig.Backend=="P2-Analytic"&&app.Tuning.Applied.GetBool("practice.enabled");
        public bool StartMode()
        {
            if(Active)return true;
            if(!CanStart)return false;
            Active=true;practice.enabled=false;Human=Rival=null;Match=null;ResetScore();MatchSeed=Seed;Style=StyleOverride??RivalStyleId.Fondista;ModeChanged?.Invoke(true);
            BeginPoint();return true;
        }
        // First Match: every point starts with the automatic serve; the human serves first.
        public bool StartMatch()
        {
            if(Active)StopMode(false);
            if(!CanStart)return false;
            Active=true;practice.enabled=false;Human=Rival=null;ResetScore();Match=NewMatch();PickStyle();ModeChanged?.Invoke(true);
            BeginPoint();return true;
        }
        private void PickStyle()
        {
            MatchSeed=RandomStyles?(long)(new RallyRng(Seed*1000003L+StyleCounter++).NextBits()>>2):Seed;
            Style=StyleOverride??(RandomStyles?RivalStyle.FromSeed(MatchSeed):RivalStyleId.Fondista);RivalVolleys=0;
        }
        private Vector2 RivalWait()=>rivalBrain is IRallyPositioning p?p.Wait:context.WaitPosition(PlayerId.Rival);
        private MatchScore NewMatch()=>new MatchScore((int)Params.MatchTarget,(int)Params.MatchMargin,PlayerId.Human);
        // Skips the rest of the pause between points; the next tick starts the point.
        public void SkipWait(){if(Active&&Phase==RallyPhase.PointOver)waited=double.MaxValue/4;}
        public void StopMode()=>StopMode(true);
        private void StopMode(bool relaunchPractice)
        {
            if(!Active)return;
            Active=false;Phase=RallyPhase.Off;hasPending=needDecision=false;practice.enabled=true;pendingHumanServe=null;RefreshServeAim();ModeChanged?.Invoke(false);
            if(relaunchPractice)app.LaunchCurrent();
        }
        // Rematch: same rival, score back to zero, next point starts at once.
        // DEV test tool (PO, 18 nov): the next point starts with this energy for both players, once. Stays pending until then, also outside a rally. Unused: no effect.
        public void SetTestEnergy(double energy){testEnergy=Math.Max(0,Math.Min(100,energy));}
        // DEV only (design 2026-11-18): switch the volley on or off from the NEXT point on.
        public void SetVolley(bool on){pendingVolley=on;}
        public bool VolleyNextPoint=>pendingVolley??Params.VolleyEnabled!=0;
        // Volley v2.2 (design section 8): the human's offer of this reception (S, S') and the sprint energy its chosen volley will pay.
        public VolleyOfferDetail LastHumanOffer {get;private set;}
        public double HumanSprintCost {get;private set;}
        // DEV only (design 2026-11-20): volley v2 (chosen by touch) or v1, and the human's return-of-serve volley, from the NEXT point on.
        private bool? pendingVolleyV2,pendingServeVolley;
        private int? pendingVolleyMode;
        // 1 = v1, 2 = v2, 3 = v2.2 (default), 4 = v2.3 lunge. SetVolleyV2(true) brings back the default v2.x mode.
        public void SetVolleyMode(int mode){pendingVolleyMode=Math.Max(1,Math.Min(4,mode));pendingVolleyV2=null;}
        public void SetVolleyV2(bool on){pendingVolleyV2=on;pendingVolleyMode=null;}
        public int VolleyModeNextPoint=>pendingVolleyMode??(pendingVolleyV2.HasValue?(pendingVolleyV2.Value?(int)new RallyParams().HumanVolleyMode:1):(int)Params.HumanVolleyMode);
        public bool VolleyV2NextPoint=>VolleyModeNextPoint>=2;
        public void SetServeVolley(bool on){pendingServeVolley=on;}
        // Serve kinds (design 2026-11-20-serve), DEV «Saque nuevo»: lob and drive from the NEXT point on; off by default (today's serve).
        public void SetNewServe(bool on){pendingNewServe=on;RefreshServeAim();}
        public bool NewServeNextPoint=>pendingNewServe??Params.ServeKindOn!=0;
        // Two-wall shot (design 2026-11-20-boast-predeclaration), DEV «Dos paredes»: the rival's boast from the NEXT point on. The human's
        // side-wall aim is always on in Rally/Match play.
        public void SetRivalBoast(bool on){pendingRivalBoast=on;}
        public bool RivalBoastNextPoint=>pendingRivalBoast??Params.BoastRival!=0;
        private bool? pendingRivalBoast;
        private bool? pendingNewServe;private ServeType? pendingHumanServe;
        // The kind of the serve that started the current point (Auto while the new serve is off).
        public ServeType LastServeType {get;private set;}
        // The human's own choice for its next serve, made by a touch on the front wall in the pause before it (null: the lob).
        public ServeType? PendingHumanServe=>pendingHumanServe;
        // The front-wall aim height chooses (lob at or above ServeKindLobMinAimY, drive below) and the serve goes at once.
        public bool ChooseHumanServe(ClosedGesture gesture)
        {
            if(!Active||Phase!=RallyPhase.PointOver||Match==null||Match.Finished||Match.Server!=PlayerId.Human||!NewServeNextPoint)return false;
            pendingHumanServe=gesture.Aim.y>=Params.ServeKindLobMinAimY?ServeType.Lob:ServeType.Drive;SkipWait();return true;
        }
        // Open only in the pause before the human's own serve with «Saque nuevo» on; otherwise every touch keeps its stroke path.
        private void RefreshServeAim()
        {
            if(controller==null)return;
            bool open=Active&&Phase==RallyPhase.PointOver&&Match!=null&&!Match.Finished&&Match.Server==PlayerId.Human&&NewServeNextPoint;
            controller.ServeAim=open?ChooseHumanServe:(Func<ClosedGesture,bool>)null;
        }
        public bool ServeVolleyNextPoint=>pendingServeVolley??Params.HumanVolleyServe!=0;
        // DEV only (design 2026-11-20-fatigue-feel): the energy-limited sprint (section 1) and «Cansancio fuerte» (section 2), from the NEXT point on.
        private bool? pendingSprintFatigue,pendingStrongFatigue;
        public void SetVolleySprintFatigue(bool on){pendingSprintFatigue=on;}
        public bool VolleySprintFatigueNextPoint=>pendingSprintFatigue??Params.HumanVolleySprintFatigue!=0;
        // «Cansancio fuerte»: the rally context of BOTH players reads legsMax and reactionMaxMs multiplied (FatigueSettings.Strong); the
        // tuning, DefaultPreset.json and the practice launch never change.
        public bool StrongFatigue {get;private set;}
        public void SetStrongFatigue(bool on){pendingStrongFatigue=on;}
        public bool StrongFatigueNextPoint=>pendingStrongFatigue??StrongFatigue;
        public void ResetScore(){HumanPoints=RivalPoints=PointsPlayed=0;LastPoint=null;resetEnergy=true;}
        public void Rematch(){if(!Active)return;ResetScore();if(Match!=null)Match=NewMatch();BeginPoint();}
        private void BeginPoint()
        {
            pointCausal.Clear();
            if(pendingSprintFatigue.HasValue){var p=Params.Clone();p.HumanVolleySprintFatigue=pendingSprintFatigue.Value?1:0;Params=p;pendingSprintFatigue=null;}
            if(pendingStrongFatigue.HasValue){StrongFatigue=pendingStrongFatigue.Value;pendingStrongFatigue=null;}
            var source=app.Tuning.ApplyPending();var cfg=EffectiveConfig.From(StrongFatigue?FatigueSettings.Strong(source):source);
            if(pendingVolley.HasValue){var p=Params.Clone();p.VolleyEnabled=pendingVolley.Value?1:0;Params=p;pendingVolley=null;}
            if(pendingVolleyV2.HasValue||pendingVolleyMode.HasValue){var p=Params.Clone();p.HumanVolleyMode=VolleyModeNextPoint;Params=p;pendingVolleyV2=null;pendingVolleyMode=null;}
            if(pendingServeVolley.HasValue){var p=Params.Clone();p.HumanVolleyServe=pendingServeVolley.Value?1:0;Params=p;pendingServeVolley=null;}
            if(pendingNewServe.HasValue){var p=Params.Clone();p.ServeKindOn=pendingNewServe.Value?1:0;Params=p;pendingNewServe=null;}
            if(pendingRivalBoast.HasValue){var p=Params.Clone();p.BoastRival=pendingRivalBoast.Value?1:0;Params=p;pendingRivalBoast=null;}
            context=new RallyContext(cfg,Params,factory);
            double humanStart=testEnergy??(resetEnergy?100:HumanEnergy?.Energy??100),rivalStart=testEnergy??(resetEnergy?100:RivalEnergy?.Energy??100);
            HumanEnergy=new FatigueState(context.Fatigue,humanStart);
            RivalEnergy=new FatigueState(context.Fatigue,rivalStart);
            resetEnergy=false;testEnergy=null;HumanEnergy.BeginPoint();RivalEnergy.BeginPoint();
            rivalBrain=RivalAi.For(context,Level,Style,MatchSeed+PointsPlayed*7919L);
            Human=new PlayerBody(PlayerId.Human,Human?.Position??context.WaitPosition(PlayerId.Human),context.Move);
            Rival=new PlayerBody(PlayerId.Rival,Rival?.Position??RivalWait(),context.Move);
            Rival.MoveTo(RivalWait(),0,BodyPhase.Recovering,BodyPhase.Waiting);
            humanBrain=new BotBrain(context,AutopilotTimingSigmaMs,Seed*31+PointsPlayed*104729L,AutopilotLine,ShotHeight.Mid,AutopilotPower);
            committed=context.NewRules();rivalReception=null;served=null;var humanServe=pendingHumanServe;pendingHumanServe=null;LastServeType=ServeType.Auto;
            if(controller!=null)controller.ServeAim=null;
            if(Match==null){committed.Begin(null,PlayerId.Human);LaunchToHuman(context.NeutralLaunch(PlayerId.Human));}
            else
            {
                // Automatic placement in the box and automatic serve for both players (spec 8.7.5 adaptation).
                var server=Match.Server;var box=Match.Box;var body=server==PlayerId.Human?Human:Rival;served=server;servedBox=box;
                committed.BeginServe(server,box);(server==PlayerId.Human?HumanEnergy:RivalEnergy).Strike(0);body.Teleport(ServePlanner.BoxPosition(context,box));
                body.MoveTo(server==PlayerId.Rival?RivalWait():context.WaitPosition(server),0,BodyPhase.Recovering,BodyPhase.Waiting);
                // Serve kinds (design 2026-11-20-serve): the rival by its style, the human by its touch in the pause (the lob without one);
                // Auto, today's serve, while «Saque nuevo» is off.
                LastServeType=server==PlayerId.Rival?context.ServeTypeOf(rivalBrain,box):context.ServeKindOn?humanServe??ServeType.Lob:ServeType.Auto;
                if(server==PlayerId.Rival)LaunchToHuman(context.Serve(box,LastServeType));else LaunchToRival(context.Serve(box,LastServeType));
            }
            RallyStarted?.Invoke();
        }
        // Volley v2: the touch chose the volley. The intercept is recomputed from the body as it is now; the body only turns when the
        // controller accepts (Apply), so a refused commit leaves the M1 path untouched.
        private VolleyCommit CommitHumanVolley(RallyContext ctx,Trajectory t,PointRules rules,int delay,int version,VolleyOfferDetail offer,double fatigue)
        {
            if(!Active||Phase!=RallyPhase.HumanReception||t==null||t.States.Count<2)return new VolleyCommit(false,default,null,null,"not-reception");
            // v2.2 order: the longest-window stance the normal body reaches; otherwise, up to S', the one it reaches sprinting.
            // The sprint is limited by the human's energy at the reception start (fatigue feel, section 1).
            long now=bodyStep;var p=HumanVolley.Commit(ctx,t,rules,Human,now,delay,version);float sprint=1;float maxSprint=ctx.HumanVolleySprintAt(fatigue);
            if(!p.Exists&&maxSprint>1&&now<=offer.SPrime)
            {var sp=HumanVolley.Commit(ctx,t,rules,Human,now,delay,version,maxSprint);if(sp.Exists){p=sp;sprint=maxSprint;}}
            if(!p.Exists)return new VolleyCommit(false,default,null,null,p.Reason=="no-intercept"?"commit-unreachable":p.Reason);
            double cost=sprint>1?Params.HumanVolleySprintCost*HumanVolley.Lateness(offer.S,offer.SPrime,now):0;
            int remaining=(int)Math.Max(0,t.States[0].Step+delay-now);
            var probe=Human.Clone();probe.Sprint=sprint;probe.MoveTo(p.Stance,remaining,BodyPhase.Approaching,BodyPhase.Set);
            var positions=probe.PositionsAfter((int)Math.Max(1,t.States[t.States.Count-1].Step-now+1));
            Func<long,BodyFrame> body=j=>ctx.FrameAt(positions[(int)Math.Max(0,Math.Min(positions.Length-1,j-now))]);
            var stance=p.Stance;
            return new VolleyCommit(true,p.Plan,body,()=>{Human.Sprint=sprint;Human.MoveTo(stance,remaining,BodyPhase.Approaching,BodyPhase.Set);HumanSprintCost=cost;},"committed");
        }
        private void LaunchToHuman(BallState ball,double? frozenFatigue=null)
        {
            Human.Sprint=1;HumanSprintCost=0;LastHumanOffer=VolleyOfferDetail.None("not-planned");
            HumanReceptionFatigue=frozenFatigue??context.Fatigue.Factor(HumanEnergy.Energy);
            var effects=FatigueEffects.For(HumanReceptionFatigue,context.Fatigue);Human.MovementFactor=effects.MovementFactor;
            var trajectory=context.Predict(ball);int delay=context.Steps(Params.ReactionS)+effects.ExtraSteps;
            HumanPlan=ReceptionPlanner.Plan(context,trajectory,committed,Human,delay,++planVersion);
            Human.MoveTo(HumanPlan.Stance,delay,BodyPhase.Approaching,BodyPhase.Set);
            var positions=Human.PositionsAfter(trajectory.States.Count);
            controller.SetNextReceptionBody(j=>context.FrameAt(positions[(int)(j-ball.Step)]));
            // Volley v1 (M1): the opportunity on this very body path, offered to the human's next launch only.
            if(context.VolleyOn)
            {
                var rules=committed;var ctx=context;int version=planVersion;
                if(ctx.HumanVolleyV2)
                {
                    // Volley v2.x (design 2026-11-20): the offer from the body as it starts this reception (M1 already set), chosen by a touch.
                    var start=Human.Clone();VolleyOfferDetail offer=VolleyOfferDetail.None("not-planned");double fatigue=HumanReceptionFatigue;
                    controller.SetNextVolley((t,bounce)=>{offer=HumanVolley.OfferDetail(ctx,t,rules,start,delay,version,fatigue);LastHumanOffer=offer;return offer.Offer;});
                    controller.SetNextVolleyCommit((t,touch)=>CommitHumanVolley(ctx,t,rules,delay,version,offer,fatigue));
                }
                else controller.SetNextVolley((t,bounce)=>ctx.VolleyFor(t,rules,positions,bounce,delay,version));
            }
            controller.SetNextReceptionFatigue(HumanReceptionFatigue);
            // The human's arm is read at its exact contact step, inside the authoritative solve, so bodies must be advanced to it.
            if(context.Fatigue.Enabled)controller.SetNextStrikerFatigue(HumanStrikerArm);
            // The fixed-body contact planner reads the human from the launch configuration, as practice already does.
            var at=HumanPlan.BodyAtContact;
            var source=app.Tuning.ApplyPending().With("human.x",at.x).With("human.z",at.y);
            launching=true;
            try{app.Host.Launch(EffectiveConfig.From(source),ball,context.StepHz*10,StopCondition.SecondFloorBounce);}
            finally{launching=false;}
            flightLaunchId=app.Host.LaunchId;flightStartStep=ball.Step;bodyStep=app.Host.Current.Step;bodyCarry=0;lastObserved=app.Host.ObservedRealTime;
            HumanEnergy.BeginFlight(bodyStep);RivalEnergy.BeginFlight(bodyStep);
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
            HumanEnergy.BeginFlight(bodyStep);RivalEnergy.BeginFlight(bodyStep);
            Phase=RallyPhase.RivalReception;resolved=null;closedSeenAt=double.NaN;hasPending=needDecision=autoQueued=false;autoShot=default;
            Rules=committed.Clone();RivalReceptionFatigue=context.Fatigue.Factor(RivalEnergy.Energy);PlanRival();PlaceBodies();
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
                var reference=controller.UsedCausalReference??controller.CausalReference;
                if(controller.LastCausal.Vectors!=null&&reference!=null)
                    pointCausal.Add(CausalShotTrace.Create("Human",record.LaunchId,record.AttemptId,record.ContactStep,reference,reference.BodyAt(record.ContactStep),controller.LastCausal));
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
        // f_s of the human at its contact step. The preview (contact still ahead) uses the energy as it is now; the authoritative
        // solve first brings bodies and energy up to that very step, exactly what the next StepBodies would do, only earlier.
        private FatigueArm HumanStrikerArm(long step,bool authoritative)
        {
            if(!authoritative)return context.Fatigue.Arm(HumanEnergy.Energy);
            AdvanceBodiesTo(step);return context.Fatigue.Arm(HumanEnergy.EnergyAt(step));
        }
        // The same whole-step advance StepBodies performs for a moving ball, up to an exact step. A step already taken is a no-op,
        // and the clamp to one second of steps is the one StepBodies has always had.
        private void AdvanceBodiesTo(long toStep)
        {
            long previousStep=bodyStep;int steps=(int)Math.Max(0,Math.Min(toStep-bodyStep,context.StepHz));
            if(toStep<=bodyStep)return;
            bodyCarry=0;bodyStep=toStep;RunBodySteps(steps,previousStep,true);
        }
        private void StepBodies(double elapsed)
        {
            var host=app.Host;long now=host.Current.Step;long previousStep=bodyStep;bool ballAdvanced=host.IsRunning||now!=bodyStep;int steps;
            if(host.IsRunning||now!=bodyStep){steps=(int)Math.Max(0,Math.Min(now-bodyStep,context.StepHz));bodyCarry=0;}
            else
            {
                // The ball is dead: bodies keep walking back on the same tempo, in whole steps.
                bodyCarry=Math.Min(bodyCarry+elapsed*context.Tempo*context.StepHz,context.StepHz);
                steps=(int)bodyCarry;bodyCarry-=steps;
            }
            bodyStep=now;RunBodySteps(steps,previousStep,ballAdvanced);
        }
        private void RunBodySteps(int steps,long previousStep,bool ballAdvanced)
        {
            for(int i=0;i<steps;i++)
            {
                var humanBefore=Human.Position;var rivalBefore=Rival.Position;
                Human.Step();Rival.Step();
                if(context.Fatigue.Enabled&&ballAdvanced&&(Phase==RallyPhase.HumanReception||Phase==RallyPhase.RivalReception))
                {
                    long step=previousStep+i+1;
                    HumanEnergy.Advance(step,Vector2.Distance(humanBefore,Human.Position),Human.Phase,Human.Speed,Human.Reacting);
                    RivalEnergy.Advance(step,Vector2.Distance(rivalBefore,Rival.Position),Rival.Phase,Rival.Speed,Rival.Reacting);
                }
            }
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
                // Judged before the racket resets the bounce count: a contact before the bounce is a volley (extra energy, 19 nov).
                double volleyCost=context.VolleyEnergyCost(at);
                // v2.2: a sprint volley also pays its lateness cost, at the same contact (design section 6).
                if(volleyCost>0)volleyCost+=HumanSprintCost;HumanSprintCost=0;
                if(at.RacketContact(PlayerId.Human))
                {
                    RivalReceptionFatigue=context.Fatigue.Factor(RivalEnergy.EnergyAt(contactStep));
                    HumanEnergy.StrikeAt(contactStep,resolved.Power,volleyCost);
                    committed=at;flightStartStep=contactStep;Phase=RallyPhase.RivalReception;resolved=null;
                    Human.Sprint=1;Human.MoveTo(context.WaitPosition(PlayerId.Human),0,BodyPhase.Recovering,BodyPhase.Waiting);
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
            var effects=FatigueEffects.For(RivalReceptionFatigue,context.Fatigue);Rival.MovementFactor=effects.MovementFactor;
            int delay=Math.Max(0,context.Steps(Params.ReactionS)+rivalBrain.PerceptionSteps+effects.ExtraSteps-elapsed);
            RivalPlan=ReceptionPlanner.Plan(context,trajectory,committed,Rival,delay,++planVersion,rivalBrain.StanceNoise(),elapsed);
            Rival.MoveTo(RivalPlan.Stance,delay,BodyPhase.Approaching,BodyPhase.Set);
            rivalCausal=rivalCausalVolley=null;rivalCausalBody=null;rivalUsedVolley=false;
            var future=Rival.PositionsAfter(trajectory.States.Count-elapsed);var past=new Vector2[trajectory.States.Count];
            // Before this presentation the receiver has no attempt; retain its present position for that prefix.
            for(int i=0;i<past.Length;i++)past[i]=i<elapsed?Rival.Position:future[i-elapsed];
            // The rival's volley (design 2026-11-19): the same opportunity as the human's, on its own frozen body path (M1).
            if(context.VolleyOn){var volley=context.VolleyFor(trajectory,committed,past,RivalPlan.Contact,delay+elapsed,planVersion);if(volley.Exists)RivalPlan=RivalPlan.WithVolley(volley);}
            if((RivalPlan.Contact.Exists&&RivalPlan.Contact.TStarStep>=0)||RivalPlan.Volley.Exists)rivalCausalBody=j=>context.FrameAt(past[(int)(j-start)]);
            pendingShot=rivalBrain.Decide(Observe(PlayerId.Rival,trajectory.States[0]),RivalPlan);
            hasPending=false;needDecision=pendingShot.Swing;
            rivalReception=pendingShot.Swing?(ReceptionResult?)null:RivalPlan.Reachable?ReceptionResult.NoSwing:ReceptionResult.Unreachable;
            if(needDecision)DecideRival();
        }
        private void DecideRival()
        {
            var host=app.Host;
            // The body path is already frozen at reception planning. A paused map only defers
            // preparation until its target is observable; no attempt or category builds the zone.
            if(rivalCausalBody!=null&&host.ClockMap.TrySimToReal(RivalPlan.Volley.Exists?RivalPlan.Volley.Plan.TStarStep:RivalPlan.Contact.TStarStep,0,out _))
            {
                // One causal reference per opportunity, both prepared before the attempt (D36 clarification of volley v1).
                if(RivalPlan.Contact.Exists&&RivalPlan.Contact.TStarStep>=0)rivalCausal=CausalQuality.Prepare(host.Prediction,RivalPlan.Contact,host.ClockMap,context.Timing,host.ObservedRealTime,context.Tempo,rivalCausalBody);
                if(RivalPlan.Volley.Exists)rivalCausalVolley=CausalQuality.Prepare(host.Prediction,RivalPlan.Volley.Plan,host.ClockMap,context.Timing,host.ObservedRealTime,context.Tempo,rivalCausalBody);
                rivalCausalBody=null;
            }
            bool aimedVolley=pendingShot.Volley&&RivalPlan.Volley.Exists;
            var decision=aimedVolley?RallyContact.ResolveAimedVolley(context,host.ClockMap,host.Prediction,RivalPlan.Contact,RivalPlan.Volley,host.ObservedRealTime,pendingShot.DeltaMs,out rivalUsedVolley)
                :RallyContact.Resolve(context,host.ClockMap,host.Prediction,RivalPlan.Contact,host.ObservedRealTime,pendingShot.DeltaMs);
            if(!aimedVolley)rivalUsedVolley=false;
            if(rivalUsedVolley)rivalCausal=rivalCausalVolley;
            // A paused clock cannot place a future instant yet: ask again on the next tick, same single attempt.
            if(decision.Result==ShotResult.NotEvaluable&&decision.Reason=="target-time-unmapped")return;
            needDecision=false;rivalDecision=decision;
            if(decision.Result==ShotResult.Perfect||decision.Result==ShotResult.Valid){hasPending=true;pendingStep=decision.ContactStep;return;}
            rivalReception=decision.Result==ShotResult.Failed?(decision.Fail==FailKind.Air?ReceptionResult.Air:ReceptionResult.Late):
                decision.Result==ShotResult.Unreachable?ReceptionResult.Unreachable:decision.Result==ShotResult.SystemFault?ReceptionResult.SystemFault:ReceptionResult.NotEvaluable;
            RaiseRivalSwing(false);
        }
        private void TickRival()
        {
            var host=app.Host;
            if(needDecision)DecideRival();
            if(hasPending&&host.Current.Step>=pendingStep)
            {
                hasPending=false;var at=Evaluate(pendingStep);var state=host.Current;
                bool have=state.Step==pendingStep||host.TryGetHistoricalState(pendingStep,out state);
                // The arm is the rival's own energy at its contact step, read before this stroke is charged below.
                var causal=have&&at.CanHit(PlayerId.Rival)&&rivalCausal!=null?RallyContact.Solve(context,rivalCausal,rivalCausal.BodyAt(pendingStep),state.Position,pendingShot,context.Fatigue.Arm(RivalEnergy.EnergyAt(pendingStep))):default;
                var solution=causal.Solution;
                if(causal.Vectors!=null)
                {
                    var trace=CausalShotTrace.Create("Rival",host.LaunchId,"",pendingStep,rivalCausal,rivalCausal.BodyAt(pendingStep),causal);pointCausal.Add(trace);
                    app.Metrics?.Write("rally-causal",host.ActiveConfig,app.ActivePresetName,app.ActivePresetFileHash,app.Tuning.PendingKeys.Count,app.MetricsSeed,!app.Clock.MetricsInvalid,app.Clock.DroppedSinceRestart,JsonUtility.ToJson(trace));
                }
                // Same rule as the human's (the Fondo rival never volleys in v1, so this is zero today).
                double rivalVolleyCost=context.VolleyEnergyCost(at);
                if(solution.Solved&&at.RacketContact(PlayerId.Rival))
                {
                    double humanFatigue=context.Fatigue.Factor(HumanEnergy.EnergyAt(pendingStep));
                    RivalEnergy.StrikeAt(pendingStep,pendingShot.P,rivalVolleyCost);
                    WriteRivalShot(pendingStep,causal);
                    committed=at;rivalReception=ReceptionResult.Contact;RaiseRivalSwing(true);
                    if(rivalUsedVolley)RivalVolleys++;
                    var struck=new BallState{Position=state.Position,Velocity=solution.Velocity,Step=0};
                    // Style recovery: from the ball just struck and the shared predictor only (cover after an own short ball).
                    Rival.MoveTo(rivalBrain is IRallyPositioning p?p.Recovery(new AiObservation(struck,Rival.Position,Rival.Velocity,Human.Position,Human.Velocity,committed.Shots,committed.FloorBounces,committed.FrontWallReached)):context.WaitPosition(PlayerId.Rival),0,BodyPhase.Recovering,BodyPhase.Waiting);
                    // The rival's stroke: a new launch from the exact state of its contact step.
                    LaunchToHuman(struck,humanFatigue);
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
            LastPoint=result;RefreshServeAim();
            Human.MoveTo(context.WaitPosition(PlayerId.Human),0,BodyPhase.Recovering,BodyPhase.Waiting);
            Rival.MoveTo(RivalWait(),0,BodyPhase.Recovering,BodyPhase.Waiting);
            HumanEnergy.ClosePoint();RivalEnergy.ClosePoint();WriteFatigueMetrics();
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
        {public string winner,cause,reception,rallyParams,level,server,box,style,serveType;public int shots,humanPoints,rivalPoints,systemFaults;public bool autopilot,match,matchOver;public long seed;public CausalShotTrace[] causal;}
        private void WriteMetrics(PointResult r)
        {
            var line=new PointLine{winner=r.Winner?.ToString()??"None",cause=r.Cause.ToString(),reception=r.Reception?.ToString()??"",shots=r.Shots,humanPoints=HumanPoints,rivalPoints=RivalPoints,
                systemFaults=SystemFaults,rallyParams=Params.Hash,level=Level.ToString(),style=RivalStyle.Name(Style),autopilot=Autopilot,seed=Seed,match=Match!=null,matchOver=Match?.Finished??false,
                server=served?.ToString()??"",box=served.HasValue?servedBox.ToString():"",serveType=served.HasValue?LastServeType.ToString():"",causal=pointCausal.ToArray()};
            app.Metrics?.Write("rally-point",app.Host.ActiveConfig,app.ActivePresetName,app.ActivePresetFileHash,app.Tuning.PendingKeys.Count,app.MetricsSeed,
                !app.Clock.MetricsInvalid,app.Clock.DroppedSinceRestart,JsonUtility.ToJson(line));
        }
        [Serializable] private sealed class FatigueLine
        {public int point;public FatiguePoint human,rival;}
        private void WriteFatigueMetrics()
        {
            var line=new FatigueLine{point=PointsPlayed,human=HumanEnergy.Point.Copy(),rival=RivalEnergy.Point.Copy()};
            app.Metrics?.Write("fatigue",app.Host.ActiveConfig,app.ActivePresetName,app.ActivePresetFileHash,app.Tuning.PendingKeys.Count,app.MetricsSeed,
                !app.Clock.MetricsInvalid,app.Clock.DroppedSinceRestart,JsonUtility.ToJson(line));
        }
        private void WriteRivalShot(long step,CausalShotResult causal)
        {
            if(!context.Fatigue.Enabled)return;
            var record=new ShotRecord{LaunchId=app.Host.LaunchId,ContactStep=step,Result=rivalDecision.Result.ToString(),Fail=rivalDecision.Fail.ToString(),DeltaMs=rivalDecision.DeltaMs,Applied=true,Solved=true,
                Power=pendingShot.P,AimX=pendingShot.Aim.x,AimY=pendingShot.Aim.y,f_r=RivalReceptionFatigue,Reason="rival-contact",
                LaunchHash=app.Host.ActiveConfig.Hash,Seed=app.MetricsSeed,Speed=causal.Solution.Speed};
            app.Metrics?.Write("shot",app.Host.ActiveConfig,app.ActivePresetName,app.ActivePresetFileHash,app.Tuning.PendingKeys.Count,app.MetricsSeed,
                !app.Clock.MetricsInvalid,app.Clock.DroppedSinceRestart,record.ToJson());
        }
        private void OnDestroy()
        {
            if(app==null)return;
            if(app.Host!=null)app.Host.Launched-=OnLaunched;
            if(controller!=null)controller.ShotResolved-=OnResolved;
        }
    }
}
