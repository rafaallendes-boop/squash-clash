using System;
using System.Collections.Generic;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    public sealed partial class GesturePreview
    {
        public M24Model M24 {get;private set;}
        private string m24CancelReason,m24Id,m24Chain,m24Predecessor;
        private string priorCanceledId,priorChain,priorContext;
        private int priorTarget,m24Target,m24Trial,m24Session;
        private string m24Block,m24Requested,m24Hash;
        private bool m24Comparable,m24SameTarget,m24Finished;
        private Rect m24SafeArea;
        private readonly List<M24Transition> m24Transitions=new List<M24Transition>();
        private string M24Context=>gestureLaunch+":"+GestureConfig.Hash;
        private M24Model CreateM24(ParamSet source)
        {
            var camera=UnityEngine.Camera.main;double now=input.ReadRealTime();m24SafeArea=Screen.safeArea;
            var snapshot=new FrontisProjection(camera!=null?camera.cameraToWorldMatrix:Matrix4x4.zero,
                camera!=null?camera.projectionMatrix:Matrix4x4.zero,camera!=null?camera.pixelRect:new Rect(),
                SlingInputArea(),app.Host.ActiveConfig.Court,now,Time.frameCount);
            return new M24Model(SlingSettings.From(source),snapshot,scale);
        }
        private string M24CancelReason()=>m24CancelReason??(app.Clock.Paused?"pause":input.BlockedByUi?"dev":!app.DevHasInputFocus?"focus":"system-context");
        private bool M24ViewportChanged()=>M24?.Active==true&&(m24SafeArea!=Screen.safeArea||UnityEngine.Camera.main==null||UnityEngine.Camera.main.pixelRect!=M24.Projection.Viewport);
        private void ResetM24Chain(){priorCanceledId=priorChain=priorContext=null;}
        private void BeginM24Record()
        {
            if(M24==null)return;
            m24Id=Guid.NewGuid().ToString("N");m24Finished=false;m24Transitions.Clear();
            bool formal=SlingTrials.Running;
            m24Block=formal?SlingTrials.Block:"preview";m24Session=SlingTrials.Session;m24Trial=formal?SlingTrials.Completed+1:0;
            m24Target=formal?SlingTrials.TargetId:0;m24Requested=m24Block=="B1"?"none":formal?SlingTrials.RequestedPower:"unspecified";
            m24Hash=GestureConfig.Hash;m24Comparable=formal&&m24Hash==slingBlockHash&&pending==0;
            bool link=priorContext==M24Context&&priorCanceledId!=null;
            m24Predecessor=link?priorCanceledId:null;m24SameTarget=link&&priorTarget==m24Target;
            m24Chain=m24SameTarget?priorChain:m24Id;
        }
        private void OnM24Sample(in TouchSample sample)
        {
            string before=M24.Phase;M24.Push(sample);
            if(sample.Phase==TouchPhaseKind.Began||before!=M24.Phase)
                m24Transitions.Add(new M24Transition{kind=M24.Phase,timeReal=sample.TimeReal,observedReal=input.ReadRealTime()});
            Power=new PowerResult(M24.P,0,0,0,0,M24.Conservative,M24.Reason,"pull-distance");
            view.gameObject.SetActive(M24.HasAim);
            if(M24.HasAim)view.SetAim(M24.Aim,AimState.Frozen,M24.Projection.Court);
            if(!M24.Active){tracker.CancelBySystem();FinishGesture();}
            if(sample.Phase==TouchPhaseKind.Canceled&&Sampling)FinishSampling(input.ReadRealTime(),false,"touch-canceled");
        }
        [Serializable] private sealed class M24Transition {public string kind;public double timeReal,observedReal;}
        [Serializable] private sealed class M24Raw
        {
            public long sequence;public int id;public string phase;public double timeReal,receivedReal;
            public float xPx,yPx,displacementFromAnchorMm,dxMm,dyMm;public bool o2;
        }
        [Serializable] private sealed class M24Record
        {
            public int schema=1,version=1,launchSerial,session,trial,targetId,pending,projectionFrame,widthPx,heightPx,o2Count;
            public bool s1PreviewOnly=true,comparableToArmedBlock,hasAim,aimInvalid,conservative,tapCandidate,everOutsideDeadZone,
                sameRequestedTarget,plannedNoPull,correctionCandidate,physicallyVerified=false;
            public string control="M2-4",attemptId,chainId,predecessorCanceledAttemptId,block,requestedPower,taskKind,
                outcome,phase,reason,preset,gestureConfigHash,dpiSource,declaredHand,aid="none",lockOrigin="Began",matrixLayout="Unity column-major";
            public double beganTimeReal,lockObservedAt,releaseTimeReal,terminalTimeReal,durationMs,projectionObservedAt;
            public bool hasRelease;
            public float aimX,aimY,beganXPx,beganYPx,targetX,targetY,targetErrorMeters,power,distanceMm,maxDistanceMm,maxDisplacementMm,lateralMm,
                mmPerPx,reportedDpi,minimumAdmissionBottomMarginMm;
            public float[] projection,cameraToWorld,viewport,admissionArea,safeArea;
            public SquashClash.Core.Court.CourtDimensions court;
            public M24Raw[] rawSamples;public M24Transition[] transitions;
        }
        private static float[] MatrixElements(Matrix4x4 m){var a=new float[16];for(int i=0;i<16;i++)a[i]=m[i];return a;}
        private static float[] RectElements(Rect r)=>new[]{r.xMin,r.yMin,r.xMax,r.yMax};
        private void FinishM24Gesture()
        {
            if(m24Finished)return;m24Finished=true;
            var model=M24;var cfg=EffectiveConfig.From(GestureConfig);var snapshot=model.Projection;
            var requestedTarget=TargetPosition(m24Target,GestureConfig);
            var data=new M24Record{
                attemptId=m24Id,chainId=m24Chain,predecessorCanceledAttemptId=m24Predecessor,sameRequestedTarget=m24SameTarget,
                launchSerial=gestureLaunch,session=m24Session,trial=m24Trial,block=m24Block,requestedPower=m24Requested,
                taskKind=m24Block=="B1"?"aim-began-no-pull":m24Block=="B4"?"planned-cancel":"gesture",
                plannedNoPull=m24Block=="B1",correctionCandidate=m24Predecessor!=null&&m24SameTarget&&m24Block!="B1",
                targetId=m24Target,targetX=requestedTarget.x,targetY=requestedTarget.y,
                targetErrorMeters=m24Target>0&&model.HasAim?Vector2.Distance(model.Aim,requestedTarget):-1,
                hasAim=model.HasAim,aimInvalid=model.Invalid,aimX=model.Aim.x,aimY=model.Aim.y,beganXPx=model.Anchor.x,beganYPx=model.Anchor.y,
                outcome=model.Outcome.ToString(),phase=model.Phase,reason=model.Reason,conservative=model.Conservative,
                tapCandidate=model.TapCandidate,everOutsideDeadZone=model.EverOutsideDeadZone,
                power=model.P,distanceMm=model.DistanceMm,maxDistanceMm=model.MaxDistanceMm,maxDisplacementMm=model.MaxDisplacementMm,lateralMm=model.LateralMm,
                beganTimeReal=model.BeganAt,lockObservedAt=snapshot.ObservedAt,projectionObservedAt=snapshot.ObservedAt,projectionFrame=snapshot.Frame,
                hasRelease=!double.IsNaN(model.ReleaseAt),releaseTimeReal=double.IsNaN(model.ReleaseAt)?0:model.ReleaseAt,
                terminalTimeReal=model.TerminalAt,durationMs=(model.TerminalAt-model.BeganAt)*1000,
                preset=preset,gestureConfigHash=m24Hash,pending=pending,comparableToArmedBlock=m24Comparable,
                declaredHand=GestureConfig.GetBool("gesture.slingPreferLeft")?"left":"right",reportedDpi=reportedDpi,dpiSource=dpiSource.ToString(),mmPerPx=scale,
                widthPx=screenWidth,heightPx=screenHeight,projection=MatrixElements(snapshot.Projection),cameraToWorld=MatrixElements(snapshot.CameraToWorld),
                viewport=RectElements(snapshot.Viewport),admissionArea=RectElements(snapshot.AdmissionArea),safeArea=RectElements(m24SafeArea),court=snapshot.Court,
                rawSamples=new M24Raw[tracker.Samples.Count],transitions=m24Transitions.ToArray(),minimumAdmissionBottomMarginMm=float.MaxValue
            };
            for(int i=0;i<tracker.Samples.Count;i++)
            {
                var sample=tracker.Samples[i];bool late=i>0&&sample.TimeReal<snapshot.ObservedAt&&receipts[i]>snapshot.ObservedAt;
                if(late)data.o2Count++;
                data.rawSamples[i]=new M24Raw{sequence=sample.Sequence,id=sample.FingerId,phase=sample.Phase.ToString(),timeReal=sample.TimeReal,receivedReal=receipts[i],
                    xPx=sample.PositionPx.x,yPx=sample.PositionPx.y,o2=late,displacementFromAnchorMm=Vector2.Distance(model.Anchor,sample.PositionPx)*scale,
                    dxMm=(sample.PositionPx.x-model.Anchor.x)*scale,dyMm=(sample.PositionPx.y-model.Anchor.y)*scale};
                data.minimumAdmissionBottomMarginMm=Mathf.Min(data.minimumAdmissionBottomMarginMm,(sample.PositionPx.y-snapshot.AdmissionArea.yMin)*scale);
            }
            bool ca=model.Outcome==GestureOutcome.AbandonedNoPull||model.Outcome==GestureOutcome.CanceledReturnToDeadZone;
            if(ca){priorCanceledId=m24Id;priorChain=m24Chain;priorContext=M24Context;priorTarget=m24Target;}else ResetM24Chain();
            RecordM24ProtocolCompletion(data,cfg);
            var savedPreset=preset;var savedFile=fileHash;int savedPending=pending,savedSeed=seed;long savedDropped=dropped;
            deferred.Enqueue(()=>
            {
                string json=JsonUtility.ToJson(data);
                // Preserve the captured gesture parameters alongside the projection and genuine raw samples.
                var b=new System.Text.StringBuilder(json.Substring(0,json.Length-1));b.Append(",\"gestureParameters\":{");
                bool first=true;foreach(var p in cfg.Source.Ordered)if(IsGestureKey(p.Key)){if(!first)b.Append(',');first=false;b.Append(Q(p.Key)).Append(':').Append(N(p.Value));}
                LastRecordJson=b.Append("}}").ToString();Write(cfg,savedPreset,savedFile,savedPending,savedSeed,savedDropped,"m24-gesture",LastRecordJson);
            });
        }
        // Filled by the protocol/view stage, after its separate RED.
        private void RecordM24ProtocolCompletion(M24Record data,EffectiveConfig cfg) { }
    }
}
