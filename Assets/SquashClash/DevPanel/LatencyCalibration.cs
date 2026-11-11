using System;
using System.Collections.Generic;
using System.Linq;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    [Serializable] public sealed class LatencyCalibrationRecord
    {
        public string kind,meaning="joint-perceptual-offset",limitation=LatencyCalibrationSession.Limitation,reason;
        public bool complete,valid,confirmed;
        public int series,dspBufferLength,dspBufferCount,sampleRate;
        public double meanMs,sdMs,ciLowMs,ciHighMs,seriesRangeMs,scheduledAt,emittedAt,eventAt,receivedAt;
        public double[] meansMs,sdBySeriesMs,starts,ends,gapsSeconds,stimuli,taps;
    }
    // DEV-only estimator. The event and emission stamps share real time; no simulation clock is owned here.
    public sealed class LatencyCalibrationSession
    {
        public const int SeriesCount=3,BeatsPerSeries=8;
        public const double PeriodSeconds=1;
        public const string Limitation="Offset conjunto de presentación + sonido + captura + respuesta humana; no mide la latencia del dispositivo ni separa sus componentes.";
        private readonly TuningState tuning;
        private readonly Action<LatencyCalibrationRecord> record;
        private readonly List<double> stimuli=new List<double>(),taps=new List<double>(),means=new List<double>(),sds=new List<double>(),starts=new List<double>(),ends=new List<double>();
        private bool active,invalid,confirmed;
        private double computedMean;
        public bool Running {get;private set;}
        public bool AwaitingSeries=>Running&&!active;
        public LatencyCalibrationRecord Result {get;private set;}=new LatencyCalibrationRecord();
        public LatencyCalibrationSession(TuningState tuning,Action<LatencyCalibrationRecord> record)
        {this.tuning=tuning??throw new ArgumentNullException(nameof(tuning));this.record=record??throw new ArgumentNullException(nameof(record));}
        public bool Begin(double at,double slowMotion=1,bool focused=true)
        {
            if(Running||!Finite(at)||at<0)return false;
            active=invalid=confirmed=false;computedMean=0;stimuli.Clear();taps.Clear();means.Clear();sds.Clear();starts.Clear();ends.Clear();
            Running=true;Result=new LatencyCalibrationRecord();ObserveConditions(slowMotion,focused);
            if(invalid)return false;
            record(new LatencyCalibrationRecord{kind="start",valid=true,eventAt=at});return true;
        }
        public bool BeginSeries(double at)
        {
            if(!AwaitingSeries||!Finite(at)||(ends.Count>0&&at<ends.Last()))return false;
            active=true;starts.Add(at);stimuli.Clear();taps.Clear();
            record(new LatencyCalibrationRecord{kind="series-start",series=starts.Count,eventAt=at,valid=true});return true;
        }
        public void Stimulus(double at)
        {
            if(!active||invalid)return;
            if(!Finite(at)||at<starts.Last()||(stimuli.Count>0&&at<=stimuli.Last())||stimuli.Count>=BeatsPerSeries){Invalidate("invalid-stimulus-timestamps");return;}
            stimuli.Add(at);record(new LatencyCalibrationRecord{kind="stimulus",series=starts.Count,emittedAt=at,valid=true});
        }
        public void Tap(double at)
        {
            if(!active||invalid)return;
            bool valid=Finite(at)&&at>=starts.Last()&&(taps.Count==0||at>taps.Last());
            record(new LatencyCalibrationRecord{kind="tap",series=starts.Count,eventAt=Finite(at)?at:0,valid=valid,reason=valid?null:"invalid-touch-timestamps"});
            if(Finite(at))taps.Add(at);
            if(!valid)Invalidate("invalid-touch-timestamps");
        }
        public bool EndSeries(double at)
        {
            if(!active||invalid)return false;
            if(!Finite(at)||stimuli.Count!=BeatsPerSeries||taps.Count!=BeatsPerSeries||at<stimuli.Last()+PeriodSeconds/2||at<taps.Last())
            {Invalidate("incomplete-series");return false;}
            var offsets=new double[BeatsPerSeries];var used=new bool[BeatsPerSeries];
            foreach(double tap in taps)
            {
                int closest=0;
                for(int i=1;i<stimuli.Count;i++)if(Math.Abs(tap-stimuli[i])<Math.Abs(tap-stimuli[closest]))closest=i;
                if(used[closest]||Math.Abs(tap-stimuli[closest])>PeriodSeconds/2){Invalidate("ambiguous-or-unpaired-taps");return false;}
                used[closest]=true;offsets[closest]=(tap-stimuli[closest])*1000;
            }
            means.Add(offsets.Average());sds.Add(Sd(offsets));ends.Add(at);active=false;
            record(new LatencyCalibrationRecord{kind="series",series=starts.Count,valid=true,complete=true,meanMs=means.Last(),sdMs=sds.Last(),stimuli=stimuli.ToArray(),taps=taps.ToArray()});
            if(means.Count==SeriesCount)
            {
                Running=false;computedMean=means.Average();
                // Two-sided 95% t interval for three independent series means, df=2 (not 24 independent taps).
                double half=4.302652729911275*Sd(means)/Math.Sqrt(SeriesCount);
                Result=new LatencyCalibrationRecord{kind="result",complete=true,valid=true,meanMs=computedMean,sdMs=Sd(means),
                    ciLowMs=computedMean-half,ciHighMs=computedMean+half,seriesRangeMs=means.Max()-means.Min(),meansMs=means.ToArray(),sdBySeriesMs=sds.ToArray(),
                    starts=starts.ToArray(),ends=ends.ToArray(),gapsSeconds=Enumerable.Range(1,SeriesCount-1).Select(i=>starts[i]-ends[i-1]).ToArray()};
                record(Result);
            }
            return true;
        }
        public void ObserveConditions(double slowMotion,bool focused)
        {if((Running||Result.complete)&&!confirmed&&(slowMotion!=1||!focused))Invalidate(!focused?"focus-lost":"slow-motion");}
        public void Cancel(){if(!confirmed&&(Running||Result.complete))Invalidate("canceled");}
        public bool Confirm()
        {
            if(invalid||confirmed||!Result.complete)return false;
            ParamCatalog.TryGet("timing.latencyCompMs",out var def);
            if(!Finite(computedMean)||computedMean<def.Min||computedMean>def.Max)
            {record(new LatencyCalibrationRecord{kind="confirmation-rejected",reason="outside-existing-catalog-range",meanMs=computedMean,complete=true,valid=false});return false;}
            tuning.Set("timing.latencyCompMs",computedMean);confirmed=true;
            record(new LatencyCalibrationRecord{kind="confirmed",confirmed=true,valid=true,complete=true,meanMs=computedMean});return true;
        }
        private void Invalidate(string reason)
        {
            if(invalid)return;invalid=true;Running=active=false;
            Result=new LatencyCalibrationRecord{kind="invalid",reason=reason,valid=false,series=starts.Count,stimuli=stimuli.ToArray(),taps=taps.ToArray()};record(Result);
        }
        private static double Sd(IEnumerable<double> values)
        {var a=values.ToArray();double m=a.Average();return Math.Sqrt(a.Sum(x=>(x-m)*(x-m))/(a.Length-1));}
        private static bool Finite(double n)=>!double.IsNaN(n)&&!double.IsInfinity(n);
    }
    public sealed class LatencyCalibration : MonoBehaviour
    {
        private const int WarmupBeats=4;
        private AppBootstrap app;
        private LatencyCalibrationSession session;
        private VisualElement overlay,pad;
        private Label status,progress;
        private Button next,confirm;
        private AudioSource audioSource;
        private AudioClip click;
        private bool seriesActive,blockedBefore;
        private double firstBeat,nextBeat,lastFlash;
        private int beat,series;
        public VisualElement Root {get;private set;}
        public void Initialize(AppBootstrap app)
        {
            this.app=app;session=new LatencyCalibrationSession(app.Tuning,Write);
            Root=new Foldout{name="latency-calibration",text="S4 · Offset perceptivo conjunto",value=false};Root.style.flexShrink=0;
            var explanation=new Label("Toca al ritmo del destello y clic: 3 series de 8, tras 4 pulsos de práctica. Mezcla presentación, sonido, captura y respuesta humana. No mide la latencia del dispositivo. No guarda presets.");
            explanation.style.whiteSpace=WhiteSpace.Normal;Root.Add(explanation);
            status=new Label();status.style.whiteSpace=WhiteSpace.Normal;Root.Add(status);
            Root.Add(new Button(StartCalibration){name="calibration-start",text="Iniciar calibración"});
            confirm=new Button(Confirm){name="calibration-confirm",text="Confirmar offset solo para esta sesión"};confirm.SetEnabled(false);Root.Add(confirm);
            click=Resources.Load<AudioClip>("Audio/contact-provisional");click?.LoadAudioData();
            audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;audioSource.spatialBlend=0;
        }
        private void StartCalibration()
        {
            if(overlay!=null)return;
            if(app.Tuning.PendingKeys.Count>0||app.Clock.MetricsInvalid||click==null||click.loadState!=AudioDataLoadState.Loaded||
                !session.Begin(Time.realtimeSinceStartupAsDouble,app.Tuning.Applied.Get("debug.slowMotion"),app.DevHasInputFocus))
            {status.text="No iniciada: aplica o descarta pendientes, sin cámara lenta ni pérdida de foco; audio precargado.";return;}
            confirm.SetEnabled(false);series=0;seriesActive=false;
            var input=app.GetComponent<TouchInputAdapter>();blockedBefore=input.BlockedByUi;input.BlockedByUi=true;
            var root=app.GetComponent<UIDocument>().rootVisualElement;
            overlay=new VisualElement{name="calibration-overlay"};overlay.style.position=Position.Absolute;
            overlay.style.left=overlay.style.right=overlay.style.top=overlay.style.bottom=0;overlay.style.backgroundColor=new Color(.02f,.04f,.06f);overlay.style.color=Color.white;
            // Timing-only modal; the runtime resume overlay still owns interruption input.
            int index=root.IndexOf(root.Q("resume-overlay"));root.Insert(index<0?root.childCount:index,overlay);
            var content=new VisualElement();content.style.position=Position.Absolute;content.style.left=Length.Percent(8);content.style.right=Length.Percent(8);content.style.top=Length.Percent(15);content.style.bottom=Length.Percent(15);overlay.Add(content);
            progress=new Label("Toca el área central al ritmo del destello y clic, no cuando aparezca texto.");progress.style.whiteSpace=WhiteSpace.Normal;content.Add(progress);
            pad=new VisualElement{name="calibration-tap-area"};pad.style.flexGrow=1;pad.style.backgroundColor=new Color(.12f,.2f,.23f);content.Add(pad);
            next=new Button(StartSeries){text="Comenzar serie 1"};next.style.minHeight=48;content.Add(next);
            var cancel=new Button(()=>{session.Cancel();FinishOverlay();}){text="Cancelar sin aplicar"};cancel.style.minHeight=48;content.Add(cancel);
            InputSystem.onEvent+=OnInput;
            AudioSettings.GetDSPBufferSize(out int size,out int count);
            Write(new LatencyCalibrationRecord{kind="audio-context",dspBufferLength=size,dspBufferCount=count,sampleRate=AudioSettings.outputSampleRate,reason="altavoz sin Bluetooth requerido; ruta no verificada automáticamente"});
        }
        private void StartSeries()
        {
            if(!session.BeginSeries(Time.realtimeSinceStartupAsDouble))return;
            series++;beat=0;firstBeat=nextBeat=Time.realtimeSinceStartupAsDouble+2;seriesActive=true;next.SetEnabled(false);
        }
        private void Update()
        {
            if(session==null||app==null)return;
            session.ObserveConditions(app.Tuning.Applied.Get("debug.slowMotion"),app.DevHasInputFocus);
            if(overlay==null){if(!session.Result.valid)confirm.SetEnabled(false);return;}
            if(!session.Running){FinishOverlay();return;}
            if(!seriesActive)return;
            double now=Time.realtimeSinceStartupAsDouble;
            if(beat<WarmupBeats+LatencyCalibrationSession.BeatsPerSeries&&now>=nextBeat)
            {
                // Never emit a burst of missed pulses and pretend it was a regular stimulus.
                if(now-nextBeat>=LatencyCalibrationSession.PeriodSeconds){session.Cancel();FinishOverlay();status.text="Serie inválida: pulso perdido; registro conservado.";return;}
                lastFlash=now;pad.style.backgroundColor=Color.white;
                if(!Application.isBatchMode)audioSource.PlayOneShot(click);
                Write(new LatencyCalibrationRecord{kind=beat<WarmupBeats?"warmup":"emission",series=series,scheduledAt=nextBeat,emittedAt=now});
                if(beat>=WarmupBeats)session.Stimulus(now);
                beat++;nextBeat=firstBeat+beat*LatencyCalibrationSession.PeriodSeconds;
            }
            else if(now-lastFlash>.1)pad.style.backgroundColor=new Color(.12f,.2f,.23f);
            progress.text="Serie "+series+"/3 · "+(beat<=WarmupBeats?"práctica "+beat+"/4":"pulsos "+(beat-WarmupBeats)+"/8");
            if(beat==WarmupBeats+LatencyCalibrationSession.BeatsPerSeries&&now>=lastFlash+LatencyCalibrationSession.PeriodSeconds/2)
            {
                seriesActive=false;session.EndSeries(now);
                if(!session.Running){FinishOverlay();return;}
                progress.text="Serie guardada. Descansa; continúa cuando estés listo.";next.text="Comenzar serie "+(series+1);next.SetEnabled(true);
            }
        }
        private void OnInput(InputEventPtr e,InputDevice device)
        {
            if(!seriesActive||!(device is Touchscreen)||!e.IsA<StateEvent>())return;
            TouchState raw;try{raw=StateEvent.GetState<TouchState>(e);}catch(InvalidOperationException){return;}
            if(raw.phase!=UnityEngine.InputSystem.TouchPhase.Began||raw.touchId==0||pad?.panel==null)return;
            var point=RuntimePanelUtils.ScreenToPanel(pad.panel,new Vector2(raw.position.x,Screen.height-raw.position.y));
            if(!pad.worldBound.Contains(point))return;
            Write(new LatencyCalibrationRecord{kind="input-event",series=series,eventAt=e.time,receivedAt=Time.realtimeSinceStartupAsDouble});
            if(e.time<firstBeat+WarmupBeats*LatencyCalibrationSession.PeriodSeconds-LatencyCalibrationSession.PeriodSeconds/2)
                Write(new LatencyCalibrationRecord{kind="warmup-tap",series=series,eventAt=e.time});
            else session.Tap(e.time);
        }
        private void Confirm()
        {
            session.ObserveConditions(app.Tuning.Applied.Get("debug.slowMotion"),app.DevHasInputFocus);
            bool applied=session.Confirm();status.text=applied?"Offset confirmado. Aplica al próximo lanzamiento; P0 y defaults no se guardaron.":"No aplicado: resultado inválido o fuera de −100…100 ms. No se recorta.";confirm.SetEnabled(false);
        }
        private void FinishOverlay()
        {
            InputSystem.onEvent-=OnInput;seriesActive=false;overlay?.RemoveFromHierarchy();overlay=null;
            if(app!=null)app.GetComponent<TouchInputAdapter>().BlockedByUi=blockedBefore;
            if(status==null)return;var r=session.Result;
            status.text=r.valid?$"Offset conjunto {r.meanMs:F1} ms · IC95% [{r.ciLowMs:F1}, {r.ciHighMs:F1}] · rango entre series {r.seriesRangeMs:F1} ms.\nMedias por serie: {string.Join(", ",r.meansMs.Select(n=>n.ToString("F1")))} ms; DE: {string.Join(", ",r.sdBySeriesMs.Select(n=>n.ToString("F1")))} ms.\nDescansos: {string.Join(", ",r.gapsSeconds.Select(n=>n.ToString("F1")))} s. No aplicado.":"Calibración inválida: "+r.reason+". No aplicado.";
            confirm.SetEnabled(r.complete&&r.valid);
        }
        private void Write(LatencyCalibrationRecord r)
        {
            if(app?.Metrics==null)return;
            app.Metrics.Write("latency-calibration",app.Config,app.ActivePresetName,app.ActivePresetFileHash,app.Tuning.PendingKeys.Count,0,r.valid,app.Clock.DroppedSinceRestart,JsonUtility.ToJson(r));
        }
        private void OnApplicationFocus(bool focused){if(!focused&&session!=null){session.ObserveConditions(1,false);if(overlay!=null)FinishOverlay();}}
        private void OnApplicationPause(bool paused){if(paused)OnApplicationFocus(false);}
        private void OnDestroy(){session?.Cancel();InputSystem.onEvent-=OnInput;overlay?.RemoveFromHierarchy();if(audioSource!=null)Destroy(audioSource);}
    }
}
