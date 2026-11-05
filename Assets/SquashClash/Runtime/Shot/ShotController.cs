using System;
using System.Collections.Generic;
using System.Diagnostics;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Feedback;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    // Receives immutable closures from GesturePreview; never subscribes to another input source.
    public sealed class ShotController : MonoBehaviour
    {
        private AppBootstrap app;
        private readonly Queue<ClosedGesture> orders=new Queue<ClosedGesture>();
        private readonly HashSet<string> seen=new HashSet<string>();
        private ShotRecord pendingContact;
        private int recordVersion;
        private double? closedAt;
        private double plannerCost,lastTempo,lastSlow;
        private bool lastPaused;
        private long lastDrops;
        private TimingRingView ring;
        private ShotNoteView notes;
        private ShotFeedback feedback;
        public ShotConfig Snapshot {get;private set;}
        public ContactPlan Plan {get;private set;}
        public PresentedPlans Presented {get;private set;}=new PresentedPlans();
        public ShotRecord LastRecord {get;private set;}
        public ShotRecord LastClosureRecord {get;private set;}
        public ShotSolution LastSolution {get;private set;}
        public bool AttemptConsumed {get;private set;}
        public bool WaitingForContact {get;private set;}
        public bool PracticeEnabled=>Snapshot?.Reception.GetBool("practice.enabled")??false;
        public bool Available=>PracticeEnabled&&app.Host.ActiveConfig.Backend=="P2-Analytic";
        public string UnavailableReason=>PracticeEnabled&&!Available?"First Shot no disponible con P1":"";
        public string LastDiscardReason {get;private set;}
        public event Action<ShotRecord> ShotResolved;
        public event Action<ShotPresentation> PresentationDelivered;
        public event Action<SwingIntent> SwingAccepted;
        public ShotPresentation? LastPresentation {get;private set;}
        public int PresentationCount {get;private set;}
        public int PresentationDiscrepancyCount=>gate.DiscrepancyCount;
        public string LastPresentationDiscrepancy=>gate.LastDiscrepancy;
        private readonly ShotPresentationGate gate=new ShotPresentationGate();
        public void Initialize(AppBootstrap app)
        {
            if(this.app!=null)return;this.app=app;
            app.Host.Launched+=OnLaunched;app.Host.ShotApplied+=OnApplied;
            ring=gameObject.AddComponent<TimingRingView>();notes=gameObject.AddComponent<ShotNoteView>();notes.Initialize(app);
            feedback=gameObject.AddComponent<ShotFeedback>();feedback.Initialize();PresentationDelivered+=feedback.Present;
            gameObject.AddComponent<SwingView>().Initialize(app,this);gameObject.AddComponent<PerfectFlashView>().Initialize(app,this);
            OnLaunched(app.Host.ActiveConfig);
        }
        private void OnLaunched(EffectiveConfig cfg)
        {
            Snapshot=new ShotConfig(cfg.Source);Haptics.Enabled=Snapshot.Reception.GetBool("feedback.haptics");seen.Clear();Presented=new PresentedPlans();AttemptConsumed=WaitingForContact=false;
            LastRecord=LastClosureRecord=pendingContact=null;gate.Reset(app.Host.LaunchId);LastPresentation=null;PresentationCount=0;LastDiscardReason=null;LastSolution=default;closedAt=null;recordVersion=0;
            double start=Stopwatch.GetTimestamp();
            var legal=new LegalSettings((float)cfg.Source.Get("legal.minHeight"),(float)cfg.Source.Get("legal.maxHeight"),cfg.Physics.StepHz,cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court);
            Plan=Available?ContactPlanner.Plan(app.Host.Prediction,Snapshot.Body,cfg.Court,legal,1):default;
            plannerCost=(Stopwatch.GetTimestamp()-start)*1000/Stopwatch.Frequency;
            CaptureClockContext();PresentCurrentPlan();
            app.GetComponentInChildren<AimReticleView>(true)?.ShowFeasible(default,false);
            if(PracticeEnabled&&!Available)notes.ShowText(UnavailableReason);
            RecordLaunchTiming();
        }
        private void CaptureClockContext(){lastTempo=app.Clock.Tempo;lastSlow=app.Clock.DebugSlowMotion;lastPaused=app.Clock.Paused;lastDrops=app.Clock.DroppedSinceRestart;}
        private void PresentCurrentPlan()
        {
            if(!Available){ring.Show(default,0,false);return;}
            double target=0;
            bool mapped=Plan.Exists&&Plan.TStarStep>=0&&app.Host.ClockMap.TrySimToReal(Plan.TStarStep,0,out target);
            Presented.Present(Time.realtimeSinceStartupAsDouble,Plan,target);
            ring.Show(Plan,target,mapped&&!AttemptConsumed);
        }
        public bool CanBegin(double eventTime,out string reason)
        {
            reason=null;if(!PracticeEnabled)return true;
            if(!Available){reason="unsupported-backend";return false;}
            if(AttemptConsumed){reason="attempt-consumed";return false;}
            if(double.IsNaN(eventTime)||double.IsInfinity(eventTime)||eventTime<app.Host.LaunchRealTime||closedAt.HasValue&&eventTime>closedAt.Value)
            {reason="outside-reception";return false;}
            return true;
        }
        public void Enqueue(ClosedGesture gesture)=>orders.Enqueue(gesture);
        public void ProcessPending()
        {
            if(app==null)return;
            while(orders.Count>0)Process(orders.Dequeue());
        }
        private void Process(ClosedGesture order)
        {
            if(order.LaunchId!=app.Host.LaunchId){Discard(order,"wrong-launch");return;}
            if(!PracticeEnabled)return;
            if(!Available){Discard(order,"unsupported-backend");return;}
            if(string.IsNullOrEmpty(order.AttemptId)||!seen.Add(order.AttemptId)){Discard(order,"duplicate-order");return;}
            LastClosureRecord=BaseRecord(order);LastClosureRecord.Result="Closure";Write("shot-gesture",LastClosureRecord);
            if(order.Outcome!=GestureOutcome.Swing)return;
            if(AttemptConsumed){Discard(order,"attempt-consumed");return;}
            if(!order.HasRelease||!Finite(order.ReleaseAt)||!Finite(order.BeganAt)||order.ReleaseAt<order.BeganAt||!Finite(order.P)||order.P<0||order.P>1||!Finite(order.Aim.x)||!Finite(order.Aim.y)||order.Source==null)
            {Discard(order,"invalid-order");return;}
            if(!CanBegin(order.BeganAt,out var reason)){Discard(order,reason);return;}
            // Reserve before any callback or solver. Failed timing / no plan / system failure never reopens it.
            AttemptConsumed=true;var shot=Snapshot.WithGesture(order.Source);
            SwingAccepted?.Invoke(new SwingIntent(order.LaunchId,order.AttemptId,Plan.Side));
            var authoritative=AuthoritativeTrajectory();
            var decision=ContactContract.Decide(order.ReleaseAt,Presented,app.Host.ClockMap,authoritative,shot.Timing,false);
            var record=BaseRecord(order);FillDecision(record,decision);record.Version=++recordVersion;LastRecord=record;Write("shot-order",record);
            ring.Show(Plan,record.TargetContactAtReal,false);
            if(decision.Result!=ShotResult.Perfect&&decision.Result!=ShotResult.Valid){Finish(record);return;}
            if(decision.ContactStep<=app.Host.Current.Step&&!app.Host.TryGetHistoricalState(decision.ContactStep,out _))
            {Fault(record,ShotResult.NotEvaluable,"history-expired");return;}
            long offset=decision.ContactStep-authoritative.States[0].Step;
            if(offset<0||offset>=authoritative.States.Count){Fault(record,ShotResult.NotEvaluable,"contact-state-unavailable");return;}
            var contact=authoritative.States[(int)offset];
            LastSolution=Solve(shot,order,contact,record);FillSolution(record,LastSolution,contact);
            var reticle=app.GetComponentInChildren<AimReticleView>(true);reticle.gameObject.SetActive(true);
            reticle.SetAim(order.Aim,AimState.Frozen,app.Host.ActiveConfig.Court);reticle.ShowFeasible(LastSolution.TargetUsed,LastSolution.Solved&&LastSolution.TargetLowered);
            if(!LastSolution.Solved){Fault(record,ShotResult.SystemFault,LastSolution.Reason);return;}
            pendingContact=record;
            if(decision.ContactStep>app.Host.Current.Step)
            {
                WaitingForContact=true;
                bool accepted=app.Host.ScheduleShot(order.LaunchId,decision.ContactStep,state=>
                {
                    LastSolution=Solve(shot,order,state,record);FillSolution(record,LastSolution,state);
                    return LastSolution.Solved?LastSolution.Velocity:new Vector3(float.NaN,0,0);
                },order.ReleaseAt);
                if(!accepted)Fault(record,ShotResult.SystemFault,"schedule-rejected");
            }
            else
            {
                var result=app.Host.ApplyShot(order.LaunchId,decision.ContactStep,LastSolution.Velocity,order.ReleaseAt);
                if(!result.Applied)Fault(record,result.Reason=="history-expired"?ShotResult.NotEvaluable:ShotResult.SystemFault,result.Reason);
            }
        }
        private Trajectory AuthoritativeTrajectory()
        {
            var host=app.Host;var t=new Trajectory();
            foreach(var predicted in host.Prediction.States)t.States.Add(host.TryGetHistoricalState(predicted.Step,out var actual)?actual:predicted);
            t.Contacts.AddRange(host.CanonicalContacts);
            foreach(var e in host.Prediction.Contacts)if(e.Step>host.Current.Step)t.Contacts.Add(e);
            t.Exit=host.Prediction.Exit;return t;
        }
        private ShotSolution Solve(ShotConfig shot,ClosedGesture order,BallState contact,ShotRecord record)
        {
            double start=Stopwatch.GetTimestamp();
            var cfg=app.Host.ActiveConfig;
            var solution=ShotSolver.Solve(new ShotRequest(contact.Position,order.Aim,order.P),shot.VMin,shot.VMax,shot.VCap,cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court,
                new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold),cfg.Physics);
            record.SolverCostMs+=(Stopwatch.GetTimestamp()-start)*1000/Stopwatch.Frequency;return solution;
        }
        private static void FillSolution(ShotRecord r,ShotSolution s,BallState contact)
        {
            r.ContactX=contact.Position.x;r.ContactY=contact.Position.y;r.ContactZ=contact.Position.z;r.SpeedRaised=s.SpeedRaised;r.TargetLowered=s.TargetLowered;
            r.TargetUsedX=s.TargetUsed.x;r.TargetUsedY=s.TargetUsed.y;r.Speed=s.Speed;r.VelocityX=s.Velocity.x;r.VelocityY=s.Velocity.y;r.VelocityZ=s.Velocity.z;
        }
        private void OnApplied(ShotApplyResult result)
        {
            if(pendingContact==null||pendingContact.LaunchId!=app.Host.LaunchId||pendingContact.ContactStep!=result.ContactStep)return;
            var record=pendingContact;pendingContact=null;WaitingForContact=false;
            if(!result.Applied){Fault(record,ShotResult.SystemFault,result.Reason);return;}
            record.Applied=true;record.Reason=result.Reason;record.StepsResimulated=result.StepsResimulated;record.JumpMeters=result.JumpMeters;
            record.JumpPixels=Finite(result.JumpPixels)?result.JumpPixels:0;record.HasJumpPixels=Finite(result.JumpPixels);record.JumpBallDiameters=result.JumpBallDiameters;
            record.ObsoleteEffects=result.ObsoleteEffects;record.OrderAgeMs=result.OrderAgeMs;record.ReconstructionCostMs=result.ReconstructionCostMs;
            record.PredictionCostMs=result.PredictionCostMs;record.ReconciliationCostMs=result.ReconciliationCostMs;
            Finish(record);
        }
        private void Fault(ShotRecord record,ShotResult result,string reason)
        {pendingContact=null;WaitingForContact=false;record.Result=result.ToString();record.Reason=reason;Finish(record);}
        private void Finish(ShotRecord record)
        {
            record.Version=++recordVersion;LastRecord=record;Write("shot",record);notes.Show(record);
            Present(record);
            ShotResolved?.Invoke(record);
        }
        private void Present(ShotRecord record)
        {
            int before=gate.DiscrepancyCount;
            if(!gate.TryDeliver(record,app.Host.LaunchId,out var p,out _))
            {if(gate.DiscrepancyCount>before)WriteDiscrepancy(record);return;}
            LastPresentation=p;PresentationCount++;WritePresentation(p);PresentationDelivered?.Invoke(p);
        }
        public void CloseReception()
        {
            if(!PracticeEnabled||!Available||closedAt.HasValue)return;
            closedAt=app.Host.ObservedRealTime;
            if(AttemptConsumed)return;
            var record=new ShotRecord{LaunchId=app.Host.LaunchId,Version=++recordVersion,Result=(Plan.Exists?ShotResult.NoOrder:ShotResult.Unreachable).ToString(),Reason="reception-ended-without-order",
                LaunchHash=Snapshot.Reception.Hash,Preset=app.ActivePresetName,FileHash=app.ActivePresetFileHash,Build=app.Build.Label,Seed=app.MetricsSeed,ProcessingAtReal=app.Host.ObservedRealTime,
                PlanVersion=Plan.Version,KInSteps=Plan.KIn.Steps,KOutSteps=Plan.KOut.Steps,TStarStep=Plan.TStarStep,PlannerCostMs=plannerCost};
            LastRecord=record;Write("shot",record);notes.Show(record);ShotResolved?.Invoke(record);
        }
        private ShotRecord BaseRecord(ClosedGesture o)
        {
            var timing=Snapshot.Timing;
            return new ShotRecord{LaunchId=o.LaunchId,AttemptId=o.AttemptId,PredecessorId=o.PredecessorId,FingerId=o.FingerId,GestureOutcome=o.Outcome.ToString(),Reason=o.Reason,
                HasRelease=o.HasRelease,BeganAtReal=o.BeganAt,ReleaseEventAtReal=o.HasRelease?o.ReleaseAt:0,InputReceivedAtReal=o.ReceivedAt,ProcessingAtReal=app.Host.ObservedRealTime,
                Power=o.P,AimX=o.Aim.x,AimY=o.Aim.y,LaunchHash=o.LaunchHash??Snapshot.Reception.Hash,GestureHash=o.Source?.Hash,Preset=o.PresetName??app.ActivePresetName,FileHash=o.FileHash??app.ActivePresetFileHash,
                Build=o.BuildLabel??app.Build.Label,Seed=o.BuildLabel!=null?o.Seed:app.MetricsSeed,Pending=o.BuildLabel!=null?o.Pending:app.Tuning.PendingKeys.Count,O2Count=o.O2Count,ProjectionObservedAt=o.ProjectionObservedAt,LockObservedAt=o.LockObservedAt,
                ValidLimitOn=timing.ValidLimitOn,EarlySoft=timing.EarlySoft,AssistMs=timing.AssistMs,ValidMs=timing.ValidMs,LatencyCompMs=timing.LatencyCompMs,
                KInSteps=Plan.KIn.Steps,KOutSteps=Plan.KOut.Steps,TStarStep=Plan.TStarStep,Side=Plan.Side.ToString(),PlannerCostMs=plannerCost};
        }
        private void FillDecision(ShotRecord r,ContactDecision d)
        {
            r.Result=d.Result.ToString();r.Fail=d.Fail.ToString();r.Reason=d.Reason;r.DeltaMs=d.DeltaMs;r.ContactStep=d.ContactStep;r.Scheduled=d.Scheduled;r.EarlyGeometry=d.EarlyGeometry;
            r.PlanVersion=d.PlanVersionUsed;r.PlanPresentedAtReal=d.PlanPresentedAtReal;r.TargetContactAtReal=d.TargetContactAtReal;r.ReleaseCorrectedAtReal=d.ReleaseCorrectedAtReal;
            r.QuantizationErrorMsSigned=d.QuantizationErrorMsSigned;r.UsedNeighborLegalStep=d.UsedNeighborLegalStep;r.QuantizationAboveThirdOfA=d.QuantizationAboveThirdOfA;
        }
        private void Discard(ClosedGesture o,string reason)
        {LastDiscardReason=reason;if(Snapshot==null)return;var r=BaseRecord(o);r.Result="Discarded";r.Reason=reason;Write("shot-discarded",r);}
        private void Write(string kind,ShotRecord record)
        {app.Metrics?.Write(kind,app.Host.ActiveConfig,record.Preset,record.FileHash,record.Pending,record.Seed,!app.Clock.MetricsInvalid,app.Clock.DroppedSinceRestart,record.ToJson());}
        private void LateUpdate()
        {
            ProcessPending();if(app==null)return;
            if(lastTempo!=app.Clock.Tempo||lastSlow!=app.Clock.DebugSlowMotion||lastPaused!=app.Clock.Paused||lastDrops!=app.Clock.DroppedSinceRestart)
            {
                CaptureClockContext();var p=Plan;
                Plan=new ContactPlan(p.Exists,p.KIn,p.KOut,p.TStarStep,p.PStar,p.Side,p.Version+1,p.Body,p.Legal,p.Court,p.Reason);PresentCurrentPlan();
            }
            ring.Tick(Time.realtimeSinceStartupAsDouble);notes.SetVisible(app.Tuning.Applied.GetBool("timing.showNotes"));
        }
        [Serializable] private sealed class LaunchTiming
        {public long launchId;public int planVersion;public string hash;public bool hasPlan,hasFront,hasBounce;public double launchReal,frontReal,bounceReal,kInReal,kOutReal,targetReal,plannerCostMs;}
        private void RecordLaunchTiming()
        {
            if(!PracticeEnabled||!Available)return;
            var host=app.Host;var t=new LaunchTiming{launchId=host.LaunchId,planVersion=Plan.Version,hash=Snapshot.Reception.Hash,hasPlan=Plan.Exists,launchReal=host.LaunchRealTime,plannerCostMs=plannerCost};
            foreach(var e in host.Prediction.Contacts)
            {
                if(e.Surface==SurfaceId.FrontWall&&!t.hasFront)t.hasFront=host.ClockMap.TrySimToReal(e.Step-1,e.TimeInStep*app.Clock.StepHz,out t.frontReal);
                if(e.Surface==SurfaceId.Floor&&!e.Persistent&&!t.hasBounce)t.hasBounce=host.ClockMap.TrySimToReal(e.Step-1,e.TimeInStep*app.Clock.StepHz,out t.bounceReal);
            }
            if(Plan.Exists){host.ClockMap.TrySimToReal(Plan.KIn.Step,Plan.KIn.Fraction,out t.kInReal);host.ClockMap.TrySimToReal(Plan.KOut.Step,Plan.KOut.Fraction,out t.kOutReal);host.ClockMap.TrySimToReal(Plan.TStarStep,0,out t.targetReal);}
            app.Metrics?.Write("shot-plan",host.ActiveConfig,app.ActivePresetName,app.ActivePresetFileHash,app.Tuning.PendingKeys.Count,app.MetricsSeed,!app.Clock.MetricsInvalid,app.Clock.DroppedSinceRestart,JsonUtility.ToJson(t));
        }
        [Serializable] private sealed class PresentationLine
        {public long launchId,contactStep;public string attemptId,category,fail,description;public int version;public bool hasContact;public double observedRealTime;}
        private void WritePresentation(ShotPresentation p)
        {WriteLine("shot-presentation",new PresentationLine{launchId=p.LaunchId,attemptId=p.AttemptId,category=p.Category.ToString(),version=p.Version,hasContact=p.HasContact,contactStep=p.ContactStep,fail=p.Fail,observedRealTime=app.Host.ObservedRealTime});}
        private void WriteDiscrepancy(ShotRecord record)
        {WriteLine("shot-presentation-discrepancy",new PresentationLine{launchId=record.LaunchId,attemptId=record.AttemptId,category=record.Result,version=record.Version,description=gate.LastDiscrepancy,observedRealTime=app.Host.ObservedRealTime});}
        private void WriteLine(string kind,PresentationLine line)
        {app.Metrics?.Write(kind,app.Host.ActiveConfig,app.ActivePresetName,app.ActivePresetFileHash,app.Tuning.PendingKeys.Count,app.MetricsSeed,!app.Clock.MetricsInvalid,app.Clock.DroppedSinceRestart,JsonUtility.ToJson(line));}
        private static bool Finite(double x)=>!double.IsNaN(x)&&!double.IsInfinity(x);
        private void OnDestroy(){if(app!=null){app.Host.Launched-=OnLaunched;app.Host.ShotApplied-=OnApplied;}if(feedback!=null)PresentationDelivered-=feedback.Present;}
    }
}
