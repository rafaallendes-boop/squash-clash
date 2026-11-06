using SquashClash.Runtime.Feedback;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using UnityEngine;
using UnityEngine.UIElements;

namespace SquashClash.Runtime.Shot
{
    // Gesture preview and immutable closure handoff; the host remains the sole ball authority.
    public sealed partial class GesturePreview : MonoBehaviour
    {
        private AppBootstrap app;
        private TouchInputAdapter input;
        private AimReticleView view;
        private ResumeOverlay resume;
        private VisualElement root;
        private GestureTracker tracker;
        private PowerSettings powerSettings;
        private PeakPowerEstimator peakEstimator;
        private readonly List<double> receipts=new List<double>(256);
        private readonly List<TouchSample> powerWindow=new List<TouchSample>(128);
        private TouchSample previousSample;
        private string invalidPowerTime;
        private readonly Queue<Action> deferred=new Queue<Action>();
        private int primary,launchSerial,gestureLaunch,screenWidth,screenHeight;
        private double beganTempo,beganSlowMotion,samplingStart;
        private float scale,reportedDpi;
        private DpiSource dpiSource;
        private string protocol="preview",gestureProtocol="preview",preset,fileHash;
        private int gestureTrial,seed,pending;
        private long dropped;
        private bool initialized,captureGesture;

        public bool Active=>M24?.Active??Sling?.Active??tracker?.Active??false;
        public ParamSet GestureConfig {get;private set;}
        public AimModel Model {get;private set;}
        public PowerResult Power {get;private set;}
        public GestureOutcome Outcome=>M24?.Outcome??Sling?.Outcome??tracker?.Outcome??GestureOutcome.None;
        public string LastRecordJson {get;private set;}
        public string SamplingRecordJson {get;private set;}
        public string Status {get;private set;}="S1: apunta; la pelota sigue sin golpe.";
        public bool Sampling {get;private set;}
        public int TrialCount {get;private set;}
        public int WindowSampleCount=>powerWindow.Count;
        public float MmPerPx=>scale;
        public DpiSource DensitySource=>dpiSource;

        public void Initialize(AppBootstrap app,TouchInputAdapter input,AimReticleView view)
        {
            this.app=app;this.input=input;this.view=view;resume=app.GetComponent<ResumeOverlay>();
            root=app.GetComponent<UIDocument>().rootVisualElement;
            input.UiBlocker=OverUi;input.SampleReceived+=OnSample;input.SystemCanceled+=OnSystemCancel;input.SampleFiltered+=OnM24Filtered;
            app.Host.Launched+=OnLaunched;app.Tuning.Changed+=OnTuningChanged;
            view.gameObject.SetActive(false);InitializeSlingStatus();initialized=true;
        }
        private bool OverUi(Vector2 point)
        {
            if(resume.IsVisible||app.Clock.Paused||SlingTrials.AwaitingFeedback)return true;
            if(root.panel==null)return false;
            var panelPoint=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(point.x,Screen.height-point.y));
            for(var picked=root.panel.Pick(panelPoint);picked!=null;picked=picked.parent)
                if(picked.name=="dev-bar"||picked.name=="dev-toggle"||picked.name=="dev-panel"||picked.name=="resume-overlay"||picked is Button)return true;
            return false;
        }
        private bool ContextChanged()=>app.Clock.Paused||resume.IsVisible||
            (Active&&(app.Clock.DroppedSinceRestart!=dropped||gestureLaunch!=launchSerial||beganTempo!=app.Clock.Tempo||beganSlowMotion!=app.Clock.DebugSlowMotion||screenWidth!=Screen.width||screenHeight!=Screen.height));
        private void OnSample(TouchSample sample)
        {
            double receivedAt=input.ReadRealTime();
            if(ContextChanged()||M24ViewportChanged()){input.CancelSystem();return;}
            if(sample.Phase==TouchPhaseKind.Began)
            {
                if(Active){AuditM24Input(sample,"duplicate-or-secondary-began","m24-input-ignored");return;}
                if(shotController!=null&&!shotController.CanBegin(sample.TimeReal,out var rejection))
                {AuditM24Input(sample,rejection,"m24-input-filtered");return;}
                Haptics.Prepare();
                BeginShotHandoff();
                var source=app.Host.ActiveConfig.Source;
                foreach(var def in ParamCatalog.All)
                    if(IsGestureKey(def.Key))source=source.With(def.Key,app.Tuning.Applied.Get(def.Key));
                GestureConfig=source;primary=sample.FingerId;gestureLaunch=launchSerial;
                beganTempo=app.Clock.Tempo;beganSlowMotion=app.Clock.DebugSlowMotion;
                reportedDpi=Screen.dpi;screenWidth=Screen.width;screenHeight=Screen.height;
                scale=ScreenMetrics.MmPerPx(reportedDpi,(float)source.Get("input.fallbackDpi"),out dpiSource);
                tracker=new GestureTracker((float)source.Get("gesture.tapMaxMm"),(float)source.Get("gesture.tapMaxMs"),(float)source.Get("gesture.cancelRadiusMm"),scale);
                M24=source.GetInt("gesture.control")==2?CreateM24(source):null;
                Sling=source.GetInt("gesture.control")==1?new SlingModel(SlingSettings.From(source),app.Host.ActiveConfig.Court,app.Host.Current.Position.x,SlingInputArea(),scale):null;
                BeginSlingRecord();
                Model=new AimModel(AimSettings.From(source),app.Host.ActiveConfig.Court,app.Host.Current.Position.x);
                powerSettings=PowerSettings.From(source);
                peakEstimator=source.GetInt("power.method")==1?new PeakPowerEstimator(powerSettings,source.Get("power.peakSmoothingMs"),source.Get("power.peakDecayMs")):null;
                receipts.Clear();powerWindow.Clear();invalidPowerTime=null;captureGesture=Sampling||protocol!="preview";
                gestureProtocol=protocol;gestureTrial=IsSeries(protocol)?TrialCount+1:1;
                preset=app.ActivePresetName;fileHash=app.ActivePresetFileHash;seed=app.MetricsSeed;pending=app.Tuning.PendingKeys.Count;dropped=app.Clock.DroppedSinceRestart;
                BeginM24Record();
            }
            if((!Active&&sample.Phase!=TouchPhaseKind.Began)||sample.FingerId!=primary){AuditM24Input(sample,"secondary-or-terminal","m24-input-ignored");return;}
            tracker.Push(sample);receipts.Add(receivedAt);
            if(M24!=null){OnM24Sample(sample);return;}
            if(Sling!=null){OnSlingSample(sample);return;}
            Model.Update(sample,scale);
            bool stationaryDuplicate=false;
            if(powerWindow.Count>0)
            {
                double dt=sample.TimeReal-previousSample.TimeReal;
                stationaryDuplicate=dt==0&&sample.Phase==TouchPhaseKind.Stationary&&sample.PositionPx==previousSample.PositionPx;
                if(invalidPowerTime==null&&(dt<0||dt==0&&!stationaryDuplicate))invalidPowerTime=dt<0?"out-of-order":"repeated-time";
            }
            if(!stationaryDuplicate){powerWindow.Add(sample);previousSample=sample;}
            double windowStart=sample.TimeReal-powerSettings.WindowMs/1000d;
            int remove=0;while(remove+1<powerWindow.Count&&powerWindow[remove+1].TimeReal<windowStart)remove++;
            if(remove>0)powerWindow.RemoveRange(0,remove);
            Power=invalidPowerTime==null
                ?(peakEstimator!=null?peakEstimator.Push(sample,scale):PowerEstimator.Estimate(powerWindow,sample.TimeReal,scale,powerSettings,tracker.DragStartedAtReal))
                :new PowerResult(0,0,0,0,0,true,invalidPowerTime,peakEstimator!=null?"decayed-peak":"net-window");
            view.gameObject.SetActive(Active||Outcome==GestureOutcome.Swing);view.SetAim(Model.Aim,Model.State,app.Host.ActiveConfig.Court);
            if(!tracker.Active)FinishGesture();
            if(sample.Phase==TouchPhaseKind.Canceled&&Sampling)FinishSampling(input.ReadRealTime(),false,"touch-canceled");
        }
        private static bool IsGestureKey(string key)=>key.StartsWith("aim.",StringComparison.Ordinal)||key.StartsWith("power.",StringComparison.Ordinal)||key.StartsWith("gesture.",StringComparison.Ordinal)||key.StartsWith("input.",StringComparison.Ordinal);
        private void OnSystemCancel()
        {
            ResetM24Chain();
            if(Active){if(M24!=null)m24Comparable=false;M24?.Cancel(input.ReadRealTime(),M24CancelReason());Sling?.Cancel(input.ReadRealTime());tracker.CancelBySystem();FinishGesture();}
            cancelIndicator?.Hide();
            if(!Sampling)EndSlingBlockTimer(input.ReadRealTime(),false,"system-cancel");
            if(view!=null)view.gameObject.SetActive(false);
            if(Sampling)FinishSampling(input.ReadRealTime(),false,"system-cancel");
        }
        private void OnLaunched(EffectiveConfig config)
        {
            launchSerial++;ResetM24Chain();
            // A repeat invalidates this gesture, not the two-minute sampling window.
            if(Active){if(M24!=null)m24Comparable=false;M24?.Cancel(input.ReadRealTime(),M24CancelReason());Sling?.Cancel(input.ReadRealTime());tracker.CancelBySystem();FinishGesture();}
            view.gameObject.SetActive(false);
        }
        private void OnTuningChanged(string key)
        {
            if(M24?.Active==true){m24CancelReason="configuration-changed";input.CancelSystem();m24CancelReason=null;}
            else if(key=="debug.slowMotion"&&Active)input.CancelSystem();
        }
        public void Tick(double now)
        {
            if(!initialized)return;
            if(ContextChanged()||M24ViewportChanged()){if(Active||Sampling)input.CancelSystem();return;}
            TickSling(now);
            if(Sampling&&now-samplingStart>=120)FinishSampling(now,true,"elapsed");
        }
        private void Update(){Tick(input?.ReadRealTime()??0);RefreshSlingStatus();}

        public void BeginSamplingWindow()
        {
            if(!CanMeasure())return;
            if(Sampling)FinishSampling(input.ReadRealTime(),false,"restarted");
            input.Stats.Clear();samplingStart=input.ReadRealTime();Sampling=true;SamplingRecordJson=null;
            Status="Apunta durante 120 s con un dedo. UI, pausa o foco interrumpen la medición.";
        }
        public void ArmRuler()
        {
            if(!CanMeasure())return;
            protocol="ruler-50mm";TrialCount=0;Status="Siguiente gesto: recorre 50 mm con regla, sin volver al origen.";
        }
        public void ArmSeries(string name)
        {
            if(!IsSeries(name))throw new ArgumentOutOfRangeException(nameof(name));
            if(!CanMeasure())return;
            protocol=name;TrialCount=0;Status="Siguientes 10 gestos: "+SeriesLabel(name)+". Se registran también toques y cancelaciones.";
        }
        private static bool IsSeries(string name)=>name=="soft"||name=="medium"||name=="strong"||name=="aim-soft";
        private static string SeriesLabel(string name)=>name=="soft"?"suaves":name=="medium"?"medios":name=="strong"?"fuertes":"apuntar rápido y soltar suave";
        public void ArmFlicks(bool strong)
        {ArmSeries(strong?"strong":"soft");}
        private bool CanMeasure()
        {
            if(!initialized||app.Clock.Paused||resume.IsVisible||input.BlockedByUi){Status="Cierra DEV y reanuda antes de medir.";return false;}
            return true;
        }
        private void FinishGesture()
        {
            if(M24!=null){FinishM24Gesture();return;}
            if(Sling!=null){FinishSlingGesture();return;}
            // Serialization and file I/O run after input dispatch. Completed trackers are never reused.
            EmitShotClosure();
            var previewOnlyAtBegin=shotPreviewOnly;
            var finished=tracker;var savedReceipts=receipts.ToArray();var config=EffectiveConfig.From(GestureConfig);
            var aim=Model.Aim;var state=Model.State;var power=Power;var jump=Model.FreezeJumpMeters;
            var usedFirst=Model.FreezeUsedFirstValidPoint;var freezeTime=Model.FreezeTimeReal;
            var freezeCause=Model.FreezeCause;var aimInvalid=view.IsInvalid;var method=GestureConfig.GetInt("power.method");
            var tag=gestureProtocol;var trial=gestureTrial;var serial=gestureLaunch;var capture=captureGesture;
            var density=reportedDpi;var densitySource=dpiSource;var mm=scale;var width=screenWidth;var height=screenHeight;
            var presetAtBegin=preset;var fileAtBegin=fileHash;var pendingAtBegin=pending;var seedAtBegin=seed;var droppedAtBegin=dropped;
            deferred.Enqueue(()=>
            {
                var b=new StringBuilder(2048);
                b.Append("{\"schema\":2,\"s1PreviewOnly\":").Append(B(previewOnlyAtBegin)).Append(",\"protocol\":").Append(Q(tag)).Append(",\"trial\":").Append(trial)
                    .Append(",\"launchSerial\":").Append(serial).Append(",\"outcome\":").Append(Q(finished.Outcome.ToString()))
                    .Append(",\"dpiSource\":").Append(Q(densitySource.ToString())).Append(",\"reportedDpi\":").Append(N(density))
                    .Append(",\"mmPerPx\":").Append(N(mm)).Append(",\"physicallyVerified\":false,\"widthPx\":").Append(width).Append(",\"heightPx\":").Append(height)
                    .Append(",\"physicalDistanceDeclaredMm\":").Append(tag=="ruler-50mm"?"50":"null").Append(",\"maxDisplacementMm\":").Append(N(finished.MaxDisplacementMm))
                    .Append(",\"aimState\":").Append(Q(state.ToString())).Append(",\"aimX\":").Append(N(aim.x)).Append(",\"aimY\":").Append(N(aim.y))
                    .Append(",\"freezeJumpMeters\":").Append(N(jump)).Append(",\"freezeUsedFirstValidPoint\":").Append(B(usedFirst)).Append(",\"freezeTimeReal\":").Append(N(freezeTime))
                    .Append(",\"freezeCause\":").Append(Q(freezeCause.ToString())).Append(",\"aimInvalid\":").Append(B(aimInvalid))
                    .Append(",\"powerMethod\":").Append(method).Append(",\"power\":").Append(N(power.P))
                    .Append(",\"powerSpeedMmPerS\":").Append(N(power.PowerSpeedMmPerS)).Append(",\"powerSpeedBasis\":").Append(Q(power.SpeedBasis)).Append(",\"powerSupportAgeMs\":").Append(N(power.SupportAgeMs))
                    .Append(",\"netSpeedMmPerS\":").Append(power.SpeedBasis=="net-window"?N(power.NetSpeedMmPerS):"null").Append(",\"coveredMs\":").Append(N(power.CoveredMs))
                    .Append(",\"distinctSamples\":").Append(power.DistinctSamples).Append(",\"maxGapMs\":").Append(N(power.MaxGapMs)).Append(",\"conservative\":").Append(B(power.Conservative)).Append(",\"reason\":").Append(Q(power.Reason))
                    .Append(",\"gestureConfigHash\":").Append(Q(config.Hash)).Append(",\"gestureParameters\":{");
                bool first=true;foreach(var p in config.Source.Ordered)if(IsGestureKey(p.Key)){if(!first)b.Append(',');first=false;b.Append(Q(p.Key)).Append(':').Append(N(p.Value));}
                b.Append("},\"rawSamples\":[");
                for(int i=0;i<finished.Samples.Count;i++)
                {
                    var s=finished.Samples[i];if(i>0)b.Append(',');
                    b.Append("{\"sequence\":").Append(s.Sequence).Append(",\"id\":").Append(s.FingerId).Append(",\"phase\":").Append(Q(s.Phase.ToString())).Append(",\"timeReal\":").Append(N(s.TimeReal))
                        .Append(",\"receivedReal\":").Append(N(savedReceipts[i])).Append(",\"xPx\":").Append(N(s.PositionPx.x)).Append(",\"yPx\":").Append(N(s.PositionPx.y)).Append('}');
                }
                LastRecordJson=b.Append("]}").ToString();
                if(capture)Write(config,presetAtBegin,fileAtBegin,pendingAtBegin,seedAtBegin,droppedAtBegin,"s1-gesture",LastRecordJson);
            });
            if(tag==protocol&&tag=="ruler-50mm")
            {
                protocol="preview";Status=$"Regla: {finished.MaxDisplacementMm:F2} mm calculados / 50 mm declarados · {densitySource}. Pendiente de confirmar el recorrido físico del PO.";
            }
            if(tag==protocol&&IsSeries(tag))
            {
                TrialCount++;
                if(TrialCount>=10){protocol="preview";Status="Serie de 10 "+SeriesLabel(tag)+" registrada. Revisa los resultados medidos.";}
            }
        }
        private void FinishSampling(double now,bool complete,string reason)
        {
            Sampling=false;EndSlingBlockTimer(now,complete,reason);SlingTrials.EndPractice();var s=input.Stats;
            SamplingRecordJson="{\"schema\":1,\"complete\":"+B(complete)+",\"reason\":"+Q(reason)+",\"elapsedRealSeconds\":"+N(now-samplingStart)+
                ",\"samples\":"+s.Samples+",\"distinctTimes\":"+s.DistinctTimes+",\"duplicates\":"+s.DuplicateTimes+",\"outOfOrder\":"+s.OutOfOrder+
                ",\"deliveredHz\":"+N(s.DeliveredHz)+",\"intervalP50Ms\":"+N(s.IntervalP50Ms)+",\"intervalP95Ms\":"+N(s.IntervalP95Ms)+",\"maxGapMs\":"+N(s.MaxGapMs)+
                ",\"maxSamplesPerFrame\":"+s.MaxSamplesPerFrame+",\"ageP50Ms\":"+N(s.AgeP50Ms)+",\"ageP95Ms\":"+N(s.AgeP95Ms)+",\"maxAgeMs\":"+N(s.MaxAgeMs)+
                ",\"negativeAges\":"+s.NegativeAges+",\"intervalOverflow\":"+s.IntervalOverflow+",\"ageOverflow\":"+s.AgeOverflow+",\"histogramResolutionMs\":0.1,\"hardwareRateAssumed\":false}";
            var json=SamplingRecordJson;var config=app.Host.ActiveConfig;var name=app.ActivePresetName;var hash=app.ActivePresetFileHash;var count=app.Tuning.PendingKeys.Count;var seedValue=app.MetricsSeed;var steps=app.Clock.DroppedSinceRestart;
            deferred.Enqueue(()=>Write(config,name,hash,count,seedValue,steps,"s1-sampling",json));
            Status=complete?"120 s terminados; frecuencia de eventos entregados medida, sin asumir frecuencia del sensor.":"Medición interrumpida: "+reason+". No cuenta como completa.";
        }
        private void Write(EffectiveConfig config,string presetName,string hash,int pendingCount,int seedValue,long steps,string kind,string payload)
            =>app.Metrics.Write(kind,config,presetName,hash,pendingCount,seedValue,false,steps,payload);
        public void FlushRecords(){while(deferred.Count>0)deferred.Dequeue()();}
        private void LateUpdate()=>FlushRecords();
        private void OnApplicationPause(bool paused){if(paused&&initialized){OnSystemCancel();FlushRecords();}}
        private void OnDestroy()
        {
            if(!initialized)return;
            input.SampleReceived-=OnSample;input.SystemCanceled-=OnSystemCancel;input.SampleFiltered-=OnM24Filtered;
            app.Host.Launched-=OnLaunched;app.Tuning.Changed-=OnTuningChanged;
            OnSystemCancel();FlushRecords();cancelIndicator?.Dispose();slingStatus?.RemoveFromHierarchy();
        }
        private static string N(double value)=>double.IsNaN(value)||double.IsInfinity(value)?"null":value.ToString("R",CultureInfo.InvariantCulture);
        private static string B(bool value)=>value?"true":"false";
        private static string Q(string value)=>value==null?"null":"\""+value.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r","\\r").Replace("\n","\\n")+"\"";
    }
}
