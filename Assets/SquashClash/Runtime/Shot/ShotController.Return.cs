using System.Diagnostics;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    public sealed partial class ShotController
    {
        private ReturnShotResolver returnResolver;
        public ReturnResolution? LastPreviewReturn {get;private set;}
        public double LastPreviewReturnCostMs {get;private set;}
        public long LastPreviewReturnContactStep {get;private set;}
        private void InitializeReturnResolver()
        {
            var cfg=app.Host.ActiveConfig;
            returnResolver=new ReturnShotResolver(Snapshot.VMin,Snapshot.VMax,Snapshot.VCap,cfg.Court,cfg.Physics,
                new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold),Snapshot.ReturnRule);
            LastPreviewReturn=null;LastPreviewReturnCostMs=0;LastPreviewReturnContactStep=-1;
        }
        public bool TryPreviewReturn(Vector2 aim,float power,double releaseAt,out ReturnResolution resolution)
        {
            resolution=default;LastPreviewReturn=null;
            if(!Available||AttemptConsumed||!Snapshot.ReturnRule.Enabled||!Plan.Exists)return false;
            long start=Stopwatch.GetTimestamp();var host=app.Host;
            // Forecast only. Ended resolves again against authoritative contact history.
            var forecast=host.Prediction;
            var decision=ContactContract.Decide(releaseAt,Presented,host.ClockMap,forecast,Snapshot.Timing,false);
            long step=decision.Result==ShotResult.Perfect||decision.Result==ShotResult.Valid?decision.ContactStep:Plan.TStarStep;
            long offset=step-forecast.States[0].Step;if(offset<0||offset>=forecast.States.Count)return false;
            resolution=returnResolver.Resolve(new ShotRequest(forecast.States[(int)offset].Position,aim,power));
            LastPreviewReturnContactStep=step;LastPreviewReturn=resolution;
            LastPreviewReturnCostMs=(Stopwatch.GetTimestamp()-start)*1000d/Stopwatch.Frequency;return true;
        }
        private ReturnResolution ResolveReturn(ShotConfig shot,ClosedGesture order,BallState contact,ShotRecord record)
        {
            var result=returnResolver.Resolve(new ShotRequest(contact.Position,order.Aim,order.P));
            record.ReturnRuleEnabled=shot.ReturnRule.Enabled;record.ReturnRuleState=result.State.ToString();
            record.ReturnRequestedX=result.AdjustedRequest.AimOnFrontWall.x;record.ReturnRequestedY=result.AdjustedRequest.AimOnFrontWall.y;
            record.ReturnSolverCalls+=result.SolverCalls;record.ReturnPreviewCostMs=LastPreviewReturnCostMs;
            record.ReturnPreviewSolverCalls=LastPreviewReturn?.SolverCalls??0;return result;
        }
    }
}
