using System;
using System.Text;
using System.Linq;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Shot
{
    public sealed partial class GesturePreview
    {
        public SlingModel Sling {get;private set;}
        public SlingProtocol SlingTrials {get;}=new SlingProtocol();
        private SlingCancelIndicator cancelIndicator;
        private float cancelRefreshAt;
        private double blockStartedAt,blockEndedAt,protocolNow;
        private string timedBlock,blockState,blockEndReason;
        public bool HasSlingProtocolStatus=>timedBlock!=null;
        public bool PracticeCancellationDone {get;private set;}
        public string ProtocolTimingText
        {
            get
            {
                if(timedBlock==null)return string.Empty;
                double elapsed=Math.Max(0,(blockState=="active"?protocolNow:blockEndedAt)-blockStartedAt);
                string time=System.TimeSpan.FromSeconds(elapsed).ToString(@"mm\:ss");
                if(blockState=="interrupted")return timedBlock+" · Interrumpido en "+time+" · "+blockEndReason;
                if(blockState=="complete")return timedBlock+" · Completado · "+time+(timedBlock=="B0"&&!PracticeCancellationDone?" · Falta cancelación guiada":"");
                if(timedBlock=="B0")return "B0 · Restan "+System.TimeSpan.FromSeconds(Math.Ceiling(Math.Max(0,120-elapsed))).ToString(@"mm\:ss");
                return timedBlock+" · "+time+" · "+SlingTrials.Completed+" intentos";
            }
        }
        public string SlingGuidanceText
        {
            get
            {
                if(timedBlock!="B0")return string.Empty;
                if(blockState=="interrupted")return "Práctica interrumpida. No cuenta como 120 s completos.";
                if(PracticeCancellationDone)return "Cancelación guiada registrada. "+(blockState=="complete"?"Espera las instrucciones del PO.":"Sigue practicando hasta terminar el tiempo.");
                if(blockState=="complete")return "Falta identificar y completar la cancelación guiada. No avances a B1.";
                if(SlingTrials.AbsoluteSupport)return "El apoyo ya apunta; bajar solo da potencia. Para cancelar, vuelve a la altura del apoyo o más arriba y suelta.";
                if(Sling?.Active!=true||!Sling.Locked)return "Guía: arrastra y pausa hasta fijar; después cancela siguiendo el límite ✕.";
                if(Sling.Phase==SlingPhase.CancelZone)return "Guía: DENTRO de cancelación. Suelta ahora para cancelar.";
                string direction=Sling.Region==SlingCancelRegion.Above?"sube":Sling.Region==SlingCancelRegion.Left?"ve a la izquierda":"ve a la derecha";
                return "Guía: "+direction+" desde donde fijaste hasta cruzar el límite ✕; después suelta.";
            }
        }
        private void StartSlingBlockTimer(string block,double now)
        {timedBlock=block;blockStartedAt=protocolNow=now;blockState="active";blockEndReason=null;if(block=="B0")PracticeCancellationDone=false;}
        private void EndSlingBlockTimer(double now,bool complete,string reason)
        {
            if(timedBlock==null||blockState!="active")return;
            blockEndedAt=protocolNow=now;blockState=complete?"complete":"interrupted";blockEndReason=reason;
            string json="{\"block\":"+Q(timedBlock)+",\"session\":"+SlingTrials.Session+",\"complete\":"+B(complete)+",\"reason\":"+Q(reason)+",\"elapsedRealSeconds\":"+N(Math.Max(0,now-blockStartedAt))+",\"guidedCancellationRecorded\":"+B(PracticeCancellationDone)+"}";
            var cfg=app.Host.ActiveConfig;var name=app.ActivePresetName;var file=app.ActivePresetFileHash;var pendingNow=app.Tuning.PendingKeys.Count;var seedNow=app.MetricsSeed;var droppedNow=app.Clock.DroppedSinceRestart;
            deferred.Enqueue(()=>Write(cfg,name,file,pendingNow,seedNow,droppedNow,"s1-sling-block-end",json));
        }
        private void TickSling(double now)
        {
            protocolNow=now;
            if(Sling?.Active==true)
            {
                bool locked=Sling.Locked;Sling.Tick(now);
                if(!locked&&Sling.Locked)UpdateSlingView(true,now);
            }
        }
        private void UpdateSlingView(bool newlyLocked,double now)
        {
            Power=new PowerResult(Sling.P,0,0,0,0,Sling.Conservative,Sling.Reason,"pull-distance");
            view.gameObject.SetActive(Active||Outcome==GestureOutcome.Swing);
            view.SetAim(Sling.Aim,Sling.Locked?AimState.Frozen:AimState.Aiming,app.Host.ActiveConfig.Court);
            if(newlyLocked){bottomMargin=Mathf.Min(bottomMargin,(Sling.LastPositionPx.y-slingArea.yMin)*scale);view.FlashLock(now,GestureConfig.Get("aim.lockFlashMs")/1000);}
            view.TickFlash(now);RefreshSlingStatus();
        }
        public string LastFeedbackJson {get;private set;}
        public Vector2 ProtocolTarget {get;private set;}
        public bool ShowProtocolTarget=>SlingTrials.Running||SlingTrials.AwaitingFeedback;
        private Label slingStatus;
        private Rect slingArea;
        private string slingId,slingBlock,slingRequestedPower,slingBlockHash;
        private int slingSession,slingTrial,slingTargetId;
        private Vector2 slingTarget;
        private float bottomMargin;
        private bool slingComparable;
        private Action<bool> pendingFeedback;

        private ParamSet CurrentGestureSource()
        {
            var source=app.Host.ActiveConfig.Source;
            foreach(var def in ParamCatalog.All)if(IsGestureKey(def.Key))source=source.With(def.Key,app.Tuning.Applied.Get(def.Key));
            return source;
        }
        private Rect SlingInputArea()
        {
            if(input.InputAreaOverride.HasValue)return input.InputAreaOverride.Value;
            var area=Screen.safeArea;
            var bar=root.Q("dev-bar");
            // Screen coordinates grow upwards; UI coordinates grow downwards.
            if(bar!=null&&root.layout.height>0&&!float.IsNaN(bar.worldBound.yMin))
            {
                float bottom=Screen.height*(1-(bar.worldBound.yMin-root.worldBound.yMin)/root.layout.height)+30*Screen.height/root.layout.height;
                area.yMin=Mathf.Min(area.yMax,Mathf.Max(area.yMin,bottom));
            }
            else area.yMin=Mathf.Min(area.yMax,area.yMin+30);
            return area;
        }
        private void BeginSlingRecord()
        {
            if(Sling==null)return;
            slingArea=SlingInputArea();bottomMargin=float.PositiveInfinity;slingId=Guid.NewGuid().ToString("N");
            bool formal=SlingTrials.Running;
            slingBlock=formal?SlingTrials.Block:"preview";slingSession=SlingTrials.Session;
            slingTrial=formal?SlingTrials.Completed+1:0;slingTargetId=formal?SlingTrials.TargetId:0;
            slingRequestedPower=formal?SlingTrials.RequestedPower:"unspecified";
            slingTarget=TargetPosition(slingTargetId,GestureConfig);
            slingComparable=formal&&GestureConfig.Hash==slingBlockHash&&app.Tuning.PendingKeys.Count==0;
        }
        public Vector2 TargetPosition(int id,ParamSet config)
        {
            if(id<1||id>5)return Vector2.zero;
            var d=app.Host.ActiveConfig.Court;
            float margin=d.LineWidth+.01f;
            // AX-1: screen left points towards positive court X.
            float x=Mathf.Lerp(d.Width-margin,margin,(float)config.Get("gesture.slingTarget"+id+"X"));
            float y=Mathf.Lerp(d.TinTop+margin,d.FrontOutLower-margin,(float)config.Get("gesture.slingTarget"+id+"Y"));
            return new Vector2(x,y);
        }
        private void OnSlingSample(in TouchSample sample)
        {
            bool wasLocked=Sling.Locked;
            Sling.Push(sample,input.ReadRealTime());
            if(!tracker.Active&&Sling.Active)Sling.Cancel(input.ReadRealTime());
            if(Sling.Locked)bottomMargin=Mathf.Min(bottomMargin,(sample.PositionPx.y-slingArea.yMin)*scale);
            UpdateSlingView(!wasLocked&&Sling.Locked,input.ReadRealTime());
            if(!Sling.Active)FinishGesture();
            if(sample.Phase==TouchPhaseKind.Canceled&&Sampling)FinishSampling(input.ReadRealTime(),false,"touch-canceled");
        }
        public bool ProtectSlingProtocol=>app!=null&&(app.Tuning.Applied.GetInt("gesture.control")>=1||
            app.Tuning.Visible.GetInt("gesture.control")>=1||SlingTrials.Running||SlingTrials.AwaitingFeedback||blockState=="active");
        public string SlingArmRejection {get;private set;}=string.Empty;
        public string SlingReadinessText=>app.ActivePresetName+" · "+EffectiveConfig.From(CurrentGestureSource()).Hash+" · "+
            (!app.DevHasInputFocus?"Sin foco":app.Clock.Paused?"Pausa":app.Tuning.PendingKeys.Count>0?"Pendientes":app.Tuning.Applied.Get("launcher.repeatSeconds")!=0?"Repetición activa":"Listo");
        private bool RejectSlingArm(string block,string reason,string remedy)
        {
            SlingArmRejection=block+": "+remedy;Status="No se inició "+SlingArmRejection;
            app.RecordDevControl("sling-"+block,false,reason);return false;
        }
        public bool ArmSlingBlock(string block)
        {
            app.RecordDevControl("sling-"+block,true,"invoked","activation");
            if(!new[]{"B0","B1","B2-soft","B2-medium","B2-strong","B3","B4","B5"}.Contains(block))
                return RejectSlingArm(block,"unknown-block","Bloque desconocido. Usa un botón B0–B5 del protocolo.");
            if(!initialized)return RejectSlingArm(block,"not-ready","Espera a que termine el arranque y vuelve a pulsar el bloque.");
            if(!app.DevHasInputFocus)return RejectSlingArm(block,"focus","Vuelve a la app, continúa si lo pide y vuelve a pulsar el bloque.");
            if(app.Clock.Paused)return RejectSlingArm(block,"paused","Juego en pausa. Pulsa Continuar en la barra y vuelve a pulsar el bloque.");
            if(resume.IsVisible)return RejectSlingArm(block,"resume-overlay","Toca para continuar y vuelve a pulsar el bloque.");
            if(input.BlockedByUi)return RejectSlingArm(block,"dev-open","Cierra DEV antes de iniciar el bloque.");
            if(Active)return RejectSlingArm(block,"active-gesture","Levanta el dedo y vuelve a pulsar el bloque.");
            if(app.Tuning.Applied.GetInt("gesture.control")<1)return RejectSlingArm(block,"wrong-control","Carga el preset experimental autorizado, aplica y relanza antes del bloque.");
            if(app.Tuning.PendingKeys.Count!=0)return RejectSlingArm(block,"pending-changes","Aplica y relanza los cambios pendientes; verifica preset y hash.");
            if(app.Tuning.Applied.Get("launcher.repeatSeconds")!=0)return RejectSlingArm(block,"repeat-not-zero","Pon repetición en 0, aplica y relanza sin guardar.");
            if(!SlingTrials.Arm(block))return RejectSlingArm(block,"pending-feedback","Responde el intento anterior con una de las opciones visibles.");
            SlingTrials.AbsoluteSupport=app.Tuning.Applied.GetInt("gesture.control")==2;
            SlingArmRejection=string.Empty;
            var config=EffectiveConfig.From(CurrentGestureSource());slingBlockHash=config.Hash;
            ProtocolTarget=TargetPosition(SlingTrials.TargetId,config.Source);
            string json="{\"schema\":1,\"block\":"+Q(block)+",\"session\":"+SlingTrials.Session+
                ",\"control\":"+Q(SlingTrials.AbsoluteSupport?"M2-4":"S1-sling")+",\"expectedGestureHash\":"+Q(config.Hash)+",\"pending\":0,\"repeatSeconds\":0}";
            Write(config,app.ActivePresetName,app.ActivePresetFileHash,0,app.MetricsSeed,app.Clock.DroppedSinceRestart,"s1-sling-block",json);
            Status=block+" preparado · hash "+config.Hash+" · cero pendientes · repetición 0";
            StartSlingBlockTimer(block,input.ReadRealTime());
            if(block=="B0")BeginSamplingWindow();
            app.RecordDevControl("sling-"+block,true,"armed");return true;
        }
        public bool RespondSling(bool premature)
        {
            if(!SlingTrials.Respond(premature))return false;
            var save=pendingFeedback;pendingFeedback=null;save?.Invoke(premature);
            if(SlingTrials.Running)ProtocolTarget=TargetPosition(SlingTrials.TargetId,CurrentGestureSource());
            if(!SlingTrials.Running)EndSlingBlockTimer(input.ReadRealTime(),true,"attempt-count");
            RefreshSlingStatus();return true;
        }
        private void FinishSlingGesture()
        {
            EmitShotClosure();var previewOnlyAtBegin=shotPreviewOnly;
            var model=Sling;var finished=tracker;var savedReceipts=receipts.ToArray();
            var config=EffectiveConfig.From(GestureConfig);var id=slingId;var block=slingBlock;
            int trial=slingTrial,session=slingSession,targetId=slingTargetId,serial=gestureLaunch;
            var target=slingTarget;var requested=slingRequestedPower;var comparable=slingComparable;
            var density=reportedDpi;var source=dpiSource;var mm=scale;var area=slingArea;var margin=bottomMargin;
            var savedPreset=preset;var savedFile=fileHash;int savedPending=pending,savedSeed=seed;long savedDropped=dropped;
            bool invalid=view.IsInvalid;int width=screenWidth,height=screenHeight;
            // A response is a separate record linked to the already completed gesture.
            if(block!="preview")
            {
                if(block=="B0"&&model.Outcome==GestureOutcome.ZoneCanceled)PracticeCancellationDone=true;
                SlingTrials.CompleteAttempt();
                if(!SlingTrials.Running&&!SlingTrials.AwaitingFeedback)EndSlingBlockTimer(input.ReadRealTime(),true,"attempt-count");
                if(SlingTrials.AwaitingFeedback)
                {
                    pendingFeedback=premature=>
                    {
                        string json="{\"schema\":1,\"attemptId\":"+Q(id)+",\"block\":"+Q(block)+",\"session\":"+session+
                            ",\"trial\":"+trial+",\"response\":"+Q(premature?"Se fijó antes de tiempo":"OK")+",\"premature\":"+B(premature)+"}";
                        LastFeedbackJson=json;deferred.Enqueue(()=>Write(config,savedPreset,savedFile,savedPending,savedSeed,savedDropped,"s1-sling-feedback",json));
                    };
                }
                else if(SlingTrials.Running)ProtocolTarget=TargetPosition(SlingTrials.TargetId,config.Source);
            }
            deferred.Enqueue(()=>
            {
                var b=new StringBuilder(4096);
                b.Append("{\"schema\":2,\"quietPolicy\":\"active-contact-timer-D2\",\"s1PreviewOnly\":").Append(B(previewOnlyAtBegin)).Append(",\"control\":\"S1-sling\",\"attemptId\":").Append(Q(id))
                    .Append(",\"block\":").Append(Q(block)).Append(",\"session\":").Append(session).Append(",\"trial\":").Append(trial)
                    .Append(",\"launchSerial\":").Append(serial).Append(",\"comparableToArmedBlock\":").Append(B(comparable))
                    .Append(",\"requestedPower\":").Append(Q(requested)).Append(",\"targetId\":").Append(targetId)
                    .Append(",\"targetX\":").Append(targetId>0?N(target.x):"null").Append(",\"targetY\":").Append(targetId>0?N(target.y):"null")
                    .Append(",\"targetErrorMeters\":").Append(targetId>0&&model.Locked?N(Vector2.Distance(model.Aim,target)):"null")
                    .Append(",\"outcome\":").Append(Q(model.Outcome.ToString())).Append(",\"phase\":").Append(Q(model.Phase.ToString()))
                    .Append(",\"reason\":").Append(Q(model.Reason)).Append(",\"conservative\":").Append(B(model.Conservative))
                    .Append(",\"aimX\":").Append(N(model.Aim.x)).Append(",\"aimY\":").Append(N(model.Aim.y)).Append(",\"aimInvalid\":").Append(B(invalid))
                    .Append(",\"beganTimeReal\":").Append(N(model.BeganAt)).Append(",\"lockTimeReal\":").Append(N(model.LockAt))
                    .Append(",\"lockOrigin\":").Append(Q(model.LockOrigin)).Append(",\"lockTheoreticalTimeReal\":").Append(N(model.LockTheoreticalAt))
                    .Append(",\"lockObservedTimeReal\":").Append(N(model.LockObservedAt)).Append(",\"lockSampleTimeReal\":").Append(N(model.Locked?model.LockSampleAt:double.NaN))
                    .Append(",\"lastOutsideTimeReal\":").Append(N(model.LastOutsideAt)).Append(",\"releaseTimeReal\":").Append(N(model.ReleaseAt))
                    .Append(",\"beganToLockMs\":").Append(N((model.LockAt-model.BeganAt)*1000))
                    .Append(",\"lastOutsideToLockMs\":").Append(N((model.LockAt-model.LastOutsideAt)*1000))
                    .Append(",\"durationMs\":").Append(N((model.ReleaseAt-model.BeganAt)*1000))
                    .Append(",\"lockXPx\":").Append(model.Locked?N(model.LockPositionPx.x):"null").Append(",\"lockYPx\":").Append(model.Locked?N(model.LockPositionPx.y):"null")
                    .Append(",\"cancelRegion\":").Append(Q(model.Region.ToString())).Append(",\"lockBlocked\":").Append(B(model.LockBlocked))
                    .Append(",\"distanceMm\":").Append(N(model.DistanceMm)).Append(",\"maxDistanceMm\":").Append(N(model.MaxDistanceMm))
                    .Append(",\"lateralMm\":").Append(N(model.LateralMm)).Append(",\"minimumBottomMarginMm\":").Append(N(margin))
                    .Append(",\"power\":").Append(N(model.P)).Append(",\"band\":").Append(Q(model.P<.25?"soft":model.P<.6?"medium":"strong"))
                    .Append(",\"reportedDpi\":").Append(N(density)).Append(",\"dpiSource\":").Append(Q(source.ToString()))
                    .Append(",\"mmPerPx\":").Append(N(mm)).Append(",\"physicallyVerified\":false,\"widthPx\":").Append(width).Append(",\"heightPx\":").Append(height)
                    .Append(",\"areaPx\":[").Append(N(area.xMin)).Append(',').Append(N(area.yMin)).Append(',').Append(N(area.xMax)).Append(',').Append(N(area.yMax)).Append(']')
                    .Append(",\"gestureConfigHash\":").Append(Q(config.Hash)).Append(",\"gestureParameters\":{");
                bool first=true;foreach(var e in config.Source.Ordered)if(IsGestureKey(e.Key)){if(!first)b.Append(',');first=false;b.Append(Q(e.Key)).Append(':').Append(N(e.Value));}
                b.Append("},\"transitions\":[");
                for(int i=0;i<model.Transitions.Count;i++)
                {if(i>0)b.Append(',');var t=model.Transitions[i];b.Append("{\"kind\":").Append(Q(t.Kind)).Append(",\"timeReal\":").Append(N(t.TimeReal));if(t.Kind=="Locked")b.Append(",\"origin\":").Append(Q(model.LockOrigin)).Append(",\"theoreticalTimeReal\":").Append(N(model.LockTheoreticalAt)).Append(",\"observedTimeReal\":").Append(N(model.LockObservedAt));b.Append('}');}
                b.Append("],\"rawSamples\":[");
                for(int i=0;i<finished.Samples.Count;i++)
                {
                    if(i>0)b.Append(',');var s=finished.Samples[i];
                    b.Append("{\"sequence\":").Append(s.Sequence).Append(",\"id\":").Append(s.FingerId).Append(",\"phase\":").Append(Q(s.Phase.ToString()))
                        .Append(",\"timeReal\":").Append(N(s.TimeReal)).Append(",\"receivedReal\":").Append(N(savedReceipts[i]))
                        .Append(",\"xPx\":").Append(N(s.PositionPx.x)).Append(",\"yPx\":").Append(N(s.PositionPx.y)).Append('}');
                }
                LastRecordJson=b.Append("]}").ToString();Write(config,savedPreset,savedFile,savedPending,savedSeed,savedDropped,"s1-sling-gesture",LastRecordJson);
            });
        }
        private void InitializeSlingStatus()
        {
            slingStatus=new Label {name="sling-status",pickingMode=PickingMode.Ignore};
            slingStatus.style.position=Position.Absolute;slingStatus.style.height=24;slingStatus.style.fontSize=14;
            slingStatus.style.color=Color.white;slingStatus.style.backgroundColor=new Color(.02f,.06f,.08f,.86f);
            slingStatus.style.paddingLeft=8;slingStatus.style.display=DisplayStyle.None;root.Insert(0,slingStatus);
        }
        private void RefreshSlingStatus()
        {
            if(slingStatus==null)return;
            bool enabled=Active?Sling!=null||M24!=null:app.Tuning.Applied.GetInt("gesture.control")>=1;
            slingStatus.style.display=enabled?DisplayStyle.Flex:DisplayStyle.None;
            if(!enabled){cancelIndicator?.Hide();return;}
            if(M24!=null)cancelIndicator?.Hide();
            if(M24==null&&Sling?.Active==true&&Sling.Locked)
            {
                if(Time.unscaledTime>=cancelRefreshAt)
                {cancelRefreshAt=Time.unscaledTime+.05f;if(cancelIndicator==null)cancelIndicator=new SlingCancelIndicator(root);cancelIndicator.Show(Sling,GestureConfig,app.Host.ActiveConfig.Court,slingArea,scale);}
            }
            else cancelIndicator?.Hide();
            string phase=Sling==null?"Apuntando":Sling.Phase==SlingPhase.Aiming?"Apuntando":Sling.Phase==SlingPhase.Locked?"Fijada":Sling.Phase==SlingPhase.Pulling?"Tensando":"Zona de cancelación";
            bool invalid=Sling!=null&&(Sling.LockBlocked||Sling.Conservative||view.IsInvalid);
            string cancel=Sling?.Locked==true?Sling.Region==SlingCancelRegion.Above?$"✕ ↑{GestureConfig.Get("gesture.slingCancelAboveMm"):F0} mm":Sling.Region==SlingCancelRegion.Left?$"✕ ←{GestureConfig.Get("gesture.slingCancelLateralMm"):F0} mm":$"✕ →{GestureConfig.Get("gesture.slingCancelLateralMm"):F0} mm":"";
            slingStatus.text=M24!=null?$"M2-4 · {M24.Phase} · P {Power.P*100:F0}% · {(M24.Active?M24.DistanceMm<=GestureConfig.Get("power.pullDeadZoneMm")?"Suelta sin golpe":"Vuelve al apoyo para cancelar":M24.Reason)}":$"{(invalid?"Inválida · ":"")}{phase} · P {Power.P*100:F0}% {cancel}";
            float w=root.layout.width,h=root.layout.height;if(w<=0||h<=0)return;
            var safe=Screen.safeArea;var bar=root.Q("dev-bar");
            float bottom=bar!=null&&!float.IsNaN(bar.worldBound.yMin)?bar.worldBound.yMin-root.worldBound.yMin:(Screen.height-safe.yMin)/Screen.height*h;
            slingStatus.style.left=safe.xMin/Screen.width*w+8;slingStatus.style.right=(Screen.width-safe.xMax)/Screen.width*w+8;
            slingStatus.style.top=Mathf.Max((Screen.height-safe.yMax)/Screen.height*h,bottom-30);
        }
    }
}
