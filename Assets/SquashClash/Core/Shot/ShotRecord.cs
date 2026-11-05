using System;
using SquashClash.Core.Config;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    // The reception and gesture snapshots share immutable ParamSet values, never a live tuning object.
    public sealed class ShotConfig
    {
        public ParamSet Reception {get;}
        public ParamSet Gesture {get;}
        public BodyFrame Body {get;}
        public TimingSettings Timing {get;}
        public float VMin {get;}
        public float VMax {get;}
        public float VCap {get;}
        public ShotConfig(ParamSet reception,BodyFrame? body=null,ParamSet gesture=null)
        {
            if(!ShotConfigValidator.TryValidate(reception,out var error))throw new ArgumentException(error,nameof(reception));
            Reception=reception;Gesture=gesture??reception;Body=body??BodyFrame.From(reception);Timing=TimingSettings.From(reception);
            VMin=(float)reception.Get("shot.vMin");VMax=(float)reception.Get("shot.vMax");VCap=(float)reception.Get("shot.vCap");
        }
        public ShotConfig WithGesture(ParamSet gesture)=>new ShotConfig(Reception,Body,gesture);
    }
    public readonly struct ClosedGesture
    {
        public readonly long LaunchId;
        public readonly string AttemptId,Reason,PredecessorId,PresetName,FileHash,BuildLabel,LaunchHash;
        public readonly int Pending,Seed;
        public readonly int FingerId,O2Count;
        public readonly double BeganAt,ReleaseAt,ReceivedAt,ProjectionObservedAt,LockObservedAt;
        public readonly bool HasRelease;
        public readonly GestureOutcome Outcome;
        public readonly Vector2 Aim;
        public readonly float P;
        public readonly ParamSet Source;
        public ClosedGesture(long launchId,string attemptId,int finger,double began,double release,bool hasRelease,GestureOutcome outcome,
            Vector2 aim,float power,ParamSet source,string reason=null,double received=0,double projection=0,double locked=0,int o2=0,string predecessor=null,string preset=null,string fileHash=null,string build=null,string launchHash=null,int pending=0,int seed=0)
        { LaunchId=launchId;AttemptId=attemptId;FingerId=finger;BeganAt=began;ReleaseAt=release;HasRelease=hasRelease;Outcome=outcome;Aim=aim;P=power;Source=source;
          Reason=reason;ReceivedAt=received;ProjectionObservedAt=projection;LockObservedAt=locked;O2Count=o2;PredecessorId=predecessor;
          PresetName=preset;FileHash=fileHash;BuildLabel=build;LaunchHash=launchHash;Pending=pending;Seed=seed; }
    }
    [Serializable] public sealed class ShotRecord
    {
        public int Schema=1,Version,Seed,PlanVersion,Pending,O2Count,FingerId,StepsResimulated,ObsoleteEffects;
        public long LaunchId,ContactStep,TStarStep;
        public string AttemptId,PredecessorId,GestureOutcome,Reason,Result,Fail,LaunchHash,GestureHash,Preset,FileHash,Build,Side;
        public bool HasJumpPixels,HasRelease,Scheduled,EarlyGeometry,UsedNeighborLegalStep,QuantizationAboveThirdOfA,ValidLimitOn,EarlySoft,Applied,SpeedRaised,TargetLowered;
        public double BeganAtReal,ReleaseEventAtReal,ReleaseCorrectedAtReal,InputReceivedAtReal,ProcessingAtReal,PlanPresentedAtReal,TargetContactAtReal,
            DeltaMs,QuantizationErrorMsSigned,OrderAgeMs,ProjectionObservedAt,LockObservedAt,KInSteps,KOutSteps,AssistMs,ValidMs,LatencyCompMs,
            PlannerCostMs,SolverCostMs,ReconstructionCostMs,PredictionCostMs,ReconciliationCostMs;
        public float Power,AimX,AimY,ContactX,ContactY,ContactZ,TargetUsedX,TargetUsedY,Speed,VelocityX,VelocityY,VelocityZ,JumpMeters,JumpPixels,JumpBallDiameters;
        public string ToJson()=>JsonUtility.ToJson(this);
    }
}
