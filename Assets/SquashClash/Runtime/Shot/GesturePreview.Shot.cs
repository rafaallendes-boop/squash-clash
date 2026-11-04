using System;
using SquashClash.Core.Shot;
namespace SquashClash.Runtime.Shot
{
    public sealed partial class GesturePreview
    {
        private ShotController shotController;
        private long shotHostLaunch;
        private string shotGestureId,shotLaunchHash;
        private bool shotDelivered,shotPreviewOnly=true;
        public void AttachShotController(ShotController controller)
        {shotController=controller;if(controller.PracticeEnabled)Status="S2: apoya, carga y suelta; el host resuelve el golpe.";}
        private void BeginShotHandoff()
        {shotHostLaunch=app.Host.LaunchId;shotLaunchHash=app.Host.ActiveConfig.Hash;shotGestureId=Guid.NewGuid().ToString("N");shotDelivered=false;shotPreviewOnly=shotController?.PracticeEnabled!=true;}
        private void EmitShotClosure(int o2=0)
        {
            if(shotDelivered||shotController==null||tracker==null||tracker.Samples.Count==0)return;shotDelivered=true;
            double began=tracker.Samples[0].TimeReal;
            var last=tracker.Samples[tracker.Samples.Count-1];
            bool hasRelease=last.Phase==TouchPhaseKind.Ended&&Outcome!=GestureOutcome.SystemCanceled&&Outcome!=GestureOutcome.Rejected;
            double release=M24!=null?M24.ReleaseAt:Sling!=null?Sling.ReleaseAt:tracker.ReleaseTimeReal;
            var aim=M24!=null?M24.Aim:Sling!=null?Sling.Aim:Model.Aim;
            shotController.Enqueue(new ClosedGesture(shotHostLaunch,shotGestureId,primary,began,hasRelease?release:0,hasRelease,Outcome,aim,Power.P,GestureConfig,
                M24?.Reason??Sling?.Reason??Outcome.ToString(),input.ReadRealTime(),M24?.Projection.ObservedAt??0,M24!=null?m24LockObservedAt:0,o2,m24Predecessor,preset,fileHash,app.Build.Label,shotLaunchHash,pending,seed));
        }
        private void UpdateReturnPreview(double now)
        {
            if(!Active||shotController==null||!shotController.Snapshot.ReturnRule.Enabled)return;
            var aim=M24!=null?M24.Aim:Sling!=null?Sling.Aim:Model.Aim;
            if(view.IsInvalid||!shotController.TryPreviewReturn(aim,Power.P,now,out var result))
            {view.ShowFeasible(default,false);return;}
            view.ShowFeasible(result.Solution.TargetUsed,result.Solution.Solved&&(result.State==ReturnRuleState.Changed||result.Solution.TargetLowered));
        }
    }
}
