using System;
using System.Collections.Generic;
using System.Diagnostics;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Runtime.Ball
{
    public readonly struct ShotApplyResult
    {
        public readonly bool Applied;
        public readonly string Reason;
        public readonly long ContactStep;
        public readonly int StepsResimulated, ObsoleteEffects;
        public readonly float JumpMeters, JumpPixels, JumpBallDiameters;
        public readonly double OrderAgeMs, ReconstructionCostMs, PredictionCostMs, ReconciliationCostMs;
        public ShotApplyResult(bool applied,string reason,long contactStep,int steps=0,float jump=0,float pixels=0,float diameters=0,double age=0,int obsolete=0,double reconstruction=0,double prediction=0,double reconciliation=0)
        { Applied=applied;Reason=reason;ContactStep=contactStep;StepsResimulated=steps;JumpMeters=jump;JumpPixels=pixels;JumpBallDiameters=diameters;OrderAgeMs=age;ObsoleteEffects=obsolete;ReconstructionCostMs=reconstruction;PredictionCostMs=prediction;ReconciliationCostMs=reconciliation; }
    }
    public sealed partial class BallSimulationHost
    {
        private struct HistoricalFrame { public BallState State; public int Bounces,Version; public double Real; public bool Valid; }
        private HistoricalFrame[] history=Array.Empty<HistoricalFrame>();
        private double historyMilliseconds;
        private int shotVersion;
        private bool hasShot,shotReserved,applyingShot;
        private long scheduledStep;
        private double? scheduledRelease;
        private Func<BallState,Vector3> scheduledVelocity;
        private readonly List<ContactEvent> canonicalContacts=new List<ContactEvent>();
        public long LaunchId {get;private set;}
        public ClockMap ClockMap {get;}=new ClockMap();
        public double ObservedRealTime {get;private set;}
        public double LaunchRealTime {get;private set;}
        public int ReceptionBounces {get;private set;}
        public int OutgoingBounces=>hasShot?floorBounces:0;
        public IReadOnlyList<ContactEvent> CanonicalContacts=>canonicalContacts;
        public Func<float> ReadVisualBallDiameter {get;set;}
        public event Action<Trajectory> PredictionReplaced;
        public event Action<ShotApplyResult> ShotApplied;
        public void ConfigureShotHistory(double milliseconds)
        {
            if(double.IsNaN(milliseconds)||double.IsInfinity(milliseconds)||milliseconds<100||milliseconds>500)throw new ArgumentOutOfRangeException(nameof(milliseconds));
            historyMilliseconds=milliseconds;
        }
        private void BeginShotLaunch()
        {
            ConfigureShotHistory(ActiveConfig.Source.Get("rollback.historyMs"));
            Clock.DebugSlowMotion=ActiveConfig.Source.Get("debug.slowMotion");
            LaunchId++;shotVersion=0;hasShot=shotReserved=applyingShot=false;scheduledVelocity=null;ReceptionBounces=0;lastObservedDrops=0;canonicalContacts.Clear();
            LaunchRealTime=ObservedRealTime=Time.realtimeSinceStartupAsDouble;
            ClockMap.Begin(LaunchId,LaunchRealTime,current.Step,Clock.Tempo,Clock.DebugSlowMotion,Clock.StepHz);
            if(Clock.Paused)ClockMap.Mark(ObservedRealTime,current.Step,0,Clock.Tempo,Clock.DebugSlowMotion,true);
            int capacity=checked((int)Math.Ceiling(historyMilliseconds/1000*Clock.StepHz*Clock.Tempo)+3);
            if(history.Length!=capacity)history=new HistoricalFrame[capacity];else Array.Clear(history,0,history.Length);
            RecordHistorical(current);
        }
        private void ObserveClock(double delta,int steps,double? observedAt)
        {
            ObservedRealTime=observedAt.HasValue?Math.Max(ObservedRealTime,observedAt.Value):ObservedRealTime+delta;
            if(IsRunning)
                ClockMap.Mark(ObservedRealTime,current.Step+steps,Clock.Alpha,Clock.Tempo,Clock.DebugSlowMotion,Clock.Paused,Clock.DroppedSinceRestart>lastObservedDrops);
            else if(ClockMap.TryRealToSim(ObservedRealTime,out var step,out var fraction))
                ClockMap.Mark(ObservedRealTime,step,fraction,Clock.Tempo,Clock.DebugSlowMotion,Clock.Paused);
            lastObservedDrops=Clock.DroppedSinceRestart;
        }
        private long lastObservedDrops;
        private void RecordHistorical(BallState state)
        {
            ClockMap.TrySimToReal(state.Step,0,out double real);
            history[(int)(state.Step%history.Length)]=new HistoricalFrame{State=state,Bounces=ReceptionBounces,Version=shotVersion,Real=real,Valid=true};
        }
        private bool TryHistorical(long step,out HistoricalFrame frame)
        {
            frame=default;if(step<0||history.Length==0)return false;
            var found=history[(int)(step%history.Length)];
            if(!found.Valid||found.State.Step!=step||ObservedRealTime-found.Real>historyMilliseconds/1000)return false;
            frame=found;return true;
        }
        public bool TryGetHistoricalState(long step,out BallState s)
        {bool ok=TryHistorical(step,out var frame);s=frame.State;return ok;}
        public ShotApplyResult ApplyShot(long launchId,long contactStep,Vector3 outgoingVelocity,double? releaseEventAtReal=null)
            =>ApplyShotCore(launchId,contactStep,outgoingVelocity,releaseEventAtReal,false);
        private ShotApplyResult ApplyShotCore(long launchId,long contactStep,Vector3 outgoingVelocity,double? releaseEventAtReal,bool scheduled)
        {
            ShotApplyResult Reject(string reason)=>new ShotApplyResult(false,reason,contactStep);
            if(launchId!=LaunchId)return Reject("wrong-launch");
            if(game==null||ActiveConfig.Backend!="P2-Analytic")return Reject("unsupported-backend");
            if(applyingShot||hasShot||shotReserved&&!scheduled)return Reject("attempt-consumed");
            if(!Finite(outgoingVelocity))return Reject("invalid-velocity");
            if(!TryHistorical(contactStep,out var frame))return Reject("history-expired");
            if(!ClockMap.TryRealToSim(ObservedRealTime,out long presentStep,out double presentFraction)||contactStep>presentStep)
                return Reject("present-not-mappable");
            double start=Timestamp();
            var initial=frame.State;initial.Velocity=outgoingVelocity;
            var oldPosition=RenderPosition;
            if(scheduled)
            {
                long end=Prediction.States[Prediction.States.Count-1].Step;
                if(presentStep>=end)oldPosition=Prediction.States[Prediction.States.Count-1].Position;
                else
                {
                    int index=(int)(presentStep-Prediction.States[0].Step);
                    oldPosition=Clock.Paused?Prediction.States[index].Position:Vector3.Lerp(Prediction.States[Math.Max(0,index-1)].Position,Prediction.States[index].Position,(float)presentFraction);
                }
            }
            var camera=UnityEngine.Camera.main;
            Vector3 oldPixel=camera!=null?camera.WorldToScreenPoint(oldPosition):Vector3.zero;
            double reconstructed=Elapsed(start);start=Timestamp();
            // Predict and play from the identical full-step state, through the same P2 path.
            var replacement=TrajectoryRunner.Run(predictor,initial,checked(maxSteps-(int)contactStep),stopCondition,ActiveConfig.Court);
            double predicted=Elapsed(start);start=Timestamp();
            applyingShot=shotReserved=hasShot=true;shotVersion++;
            try
            {
                game.Reset(initial);previous=current=initial;floorBounces=0;ReceptionBounces=frame.Bounces;
                var replacementContacts=new List<ContactEvent>();
                foreach(var e in canonicalContacts)if(e.Step<=contactStep)replacementContacts.Add(e);
                RecordHistorical(current);IsRunning=true;
                int resimulated=0;
                while(current.Step<presentStep&&IsRunning)
                {
                    previous=current;game.Step(contacts);current=game.State;resimulated++;
                    foreach(var e in contacts){replacementContacts.Add(e);if(e.Surface==SurfaceId.Floor&&!e.Persistent)floorBounces++;}
                    RecordHistorical(current);
                    if((stopCondition==StopCondition.SecondFloorBounce&&floorBounces>=2)||current.Position.y>wallHeight||current.Step>=maxSteps)IsRunning=false;
                }
                int obsolete=CountObsolete(canonicalContacts,replacementContacts);
                canonicalContacts.Clear();canonicalContacts.AddRange(replacementContacts);
                Prediction=replacement;PredictionMatchesBitwise=true;
                long index=current.Step-Prediction.States[0].Step;
                if(index<0||index>=Prediction.States.Count||!SameBits(current,Prediction.States[(int)index]))PredictionMatchesBitwise=false;
                // A stopped incoming flight retained its fractional clock. Restore the real-present fraction
                // in that same accumulator, without Advance or a second simulation clock.
                Clock.RestoreFraction(presentFraction);
                RenderPosition=!IsRunning||Clock.Paused?current.Position:Vector3.Lerp(previous.Position,current.Position,Clock.Alpha);
                float jump=Vector3.Distance(oldPosition,RenderPosition);
                float pixels=camera!=null?Vector2.Distance(oldPixel,camera.WorldToScreenPoint(RenderPosition)):float.NaN;
                float diameter=ReadVisualBallDiameter?.Invoke()??ActiveConfig.Physics.BallRadius*2*(float)ActiveConfig.Source.Get("aids.ballVisualScale");
                var result=new ShotApplyResult(true,"applied",contactStep,resimulated,jump,pixels,jump/diameter,
                    (ObservedRealTime-(releaseEventAtReal??frame.Real))*1000,obsolete,reconstructed,predicted,Elapsed(start));
                PredictionReplaced?.Invoke(Prediction);ShotApplied?.Invoke(result);
                return result;
            }
            finally {applyingShot=false;}
        }
        public bool ScheduleShot(long launchId,long contactStep,Func<BallState,Vector3> velocityAtContact,double? releaseEventAtReal=null)
        {
            if(launchId!=LaunchId||ActiveConfig==null||ActiveConfig.Backend!="P2-Analytic"||!IsRunning||shotReserved||hasShot||applyingShot||
                contactStep<=current.Step||contactStep>=maxSteps||velocityAtContact==null)return false;
            shotReserved=true;scheduledStep=contactStep;scheduledVelocity=velocityAtContact;scheduledRelease=releaseEventAtReal;return true;
        }
        private readonly struct EventIdentity : IEquatable<EventIdentity>
        {
            private readonly long step;private readonly SurfaceId surface;private readonly int ordinal;
            public EventIdentity(ContactEvent e,int ordinal){step=e.Step;surface=e.Surface;this.ordinal=ordinal;}
            public bool Equals(EventIdentity other)=>step==other.step&&surface==other.surface&&ordinal==other.ordinal;
            public override bool Equals(object obj)=>obj is EventIdentity other&&Equals(other);
            public override int GetHashCode()=>unchecked((step.GetHashCode()*397^(int)surface)*397^ordinal);
        }
        private static IEnumerable<(EventIdentity id,ContactEvent hit)> Identities(List<ContactEvent> events)
        {
            long step=-1;var ordinals=new int[5];
            foreach(var e in events){if(e.Step!=step){step=e.Step;Array.Clear(ordinals,0,ordinals.Length);}yield return(new EventIdentity(e,ordinals[(int)e.Surface]++),e);}
        }
        private static int CountObsolete(List<ContactEvent> old,List<ContactEvent> replacement)
        {
            var ids=new HashSet<EventIdentity>();foreach(var e in Identities(replacement))ids.Add(e.id);
            int count=0;foreach(var e in Identities(old))if(!e.hit.Persistent&&!ids.Contains(e.id))count++;return count;
        }
        private static double Timestamp()=>Stopwatch.GetTimestamp();
        private static double Elapsed(double start)=>(Stopwatch.GetTimestamp()-start)*1000/Stopwatch.Frequency;
    }
}
