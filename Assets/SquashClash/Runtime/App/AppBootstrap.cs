using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SquashClash.Core.Diagnostics;
using SquashClash.Core.Config;
using SquashClash.Runtime.Court;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Physics;
using SquashClash.Core.Simulation;
using SquashClash.Core.Court;
using SquashClash.Runtime.Camera;
using UnityEngine;
using UnityEngine.UIElements;

namespace SquashClash.Runtime.App
{
    [RequireComponent(typeof(UIDocument), typeof(BallSimulationHost), typeof(ResumeOverlay))]
    public sealed class AppBootstrap : MonoBehaviour
    {
        public static AppBootstrap Instance { get; private set; }
        public BuildInfo Build { get; private set; }
        public SquashClash.Runtime.Ball.BallSimulationHost Host => GetComponent<SquashClash.Runtime.Ball.BallSimulationHost>();
        public ParamSet Params { get; private set; }
        public EffectiveConfig Config { get; private set; }

        public TuningState Tuning { get; private set; }
        public PresetStore Presets { get; private set; }
        public MetricsLog Metrics { get; private set; }
        public FrameTimeStats FrameStats { get; private set; }
        public int MetricsSeed { get; private set; }
        public string ActivePresetName { get; private set; }
        public string ActivePresetFileHash { get; private set; }
        public BallSimulationHost Ball => Host;
        public SimClock Clock => Host.Clock;
        public void LaunchCurrent() => Host.Launch(EffectiveConfig.From(Tuning.ApplyPending()));
        public void ReplayCurrent()
        {
            if(Host.ActiveConfig==null)return;
            var source=Host.ActiveConfig.Source;
            foreach(var def in ParamCatalog.All)
                if(def.Apply==ApplyClass.Immediate)source=source.With(def.Key,Tuning.Applied.Get(def.Key));
            Host.Replay(EffectiveConfig.From(source));
        }
        public void SetPresetIdentity(string name,string fileHash)
        {
            FlushFrames();ActivePresetName=name;ActivePresetFileHash=fileHash;
        }
        private double repeatElapsed;
        private bool discardRepeatResume;
        public void TickRepeat(double seconds)
        {
            if(double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
            if(discardRepeatResume){discardRepeatResume=false;repeatElapsed=0;return;}
            var interval=Tuning.Applied.Get("launcher.repeatSeconds");
            if(interval<=0||Clock==null||Clock.Paused||diagnosticsPaused||!diagnosticsFocused){repeatElapsed=0;return;}
            repeatElapsed+=seconds;
            if(repeatElapsed>=interval){repeatElapsed=0;ReplayCurrent();}
        }
        private void OnTuningChanged(string key)
        {
            // Physical edits stay pending. Immediate consumers use the applied set without touching the flight.
            if(!ParamCatalog.TryGet(key,out var def)||def.Apply!=ApplyClass.Immediate||Clock==null)return;
            FlushFrames();Params=Tuning.Applied;
            if(key.StartsWith("camera.",StringComparison.Ordinal))cameraRig.Apply(Params,Config.Court);
            if(key=="placeholders.visible"||key.StartsWith("human.",StringComparison.Ordinal)||key.StartsWith("rival.",StringComparison.Ordinal))placeholderPlayers.Apply(Params);
            if(key=="debug.slowMotion")Clock.DebugSlowMotion=Params.Get(key);
            if(key=="launcher.repeatSeconds")repeatElapsed=0;
        }
        private readonly FrameTimeStats segmentFrames=new FrameTimeStats(),segmentCpu=new FrameTimeStats(),segmentGpu=new FrameTimeStats();
        private ThermalMonitor thermal;
        private FrameTimingSampler timing;
        private EffectiveConfig segmentConfig;
        private double segmentSeconds;
        private long droppedAtSegmentStart,segmentDroppedSinceRestart,lastTimingSerial,over250MsFrames;
        private int gcAtSegmentStart,segmentSeed;
        private string segmentPhase="free";
        private bool diagnosticsPaused,diagnosticsFocused=true,discardDiagnosticsResume;

        public void RecordDiagnosticsFrame(double seconds)
        {
            if(Metrics==null||segmentConfig==null||diagnosticsPaused||!diagnosticsFocused)return;
            if(discardDiagnosticsResume){discardDiagnosticsResume=false;return;}
            if(double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
            if(seconds==0)return;
            if(segmentPhase!=Metrics.Phase){FlushFrames();timing.RestartCapture();}
            double ms=seconds*1000;
            FrameStats.Add(ms);segmentFrames.Add(ms);segmentSeconds+=seconds;if(ms>250)over250MsFrames++;
            if(timing.SampleSerial!=lastTimingSerial)
            {
                lastTimingSerial=timing.SampleSerial;
                if(!double.IsNaN(timing.CpuMs))segmentCpu.Add(timing.CpuMs);
                if(!double.IsNaN(timing.GpuMs))segmentGpu.Add(timing.GpuMs);
            }
            segmentDroppedSinceRestart=Host.Clock.DroppedSinceRestart;
            if(segmentSeconds>=10)FlushFrames();
        }
        public void SetMetricsPhase(string phase,int seed=0)
        {
            if(Metrics==null)return;
            if(Metrics.Phase==phase&&MetricsSeed==seed)return;
            FlushFrames();Metrics.Phase=phase;MetricsSeed=seed;ResetSegment();timing.RestartCapture();
        }
        private void ResetSegment()
        {
            segmentFrames.ResetSession();segmentCpu.ResetSession();segmentGpu.ResetSession();segmentSeconds=0;over250MsFrames=0;
            droppedAtSegmentStart=Host.Clock?.DroppedSteps??0;segmentDroppedSinceRestart=Host.Clock?.DroppedSinceRestart??0;
            gcAtSegmentStart=GC.CollectionCount(0);lastTimingSerial=timing?.SampleSerial??0;
            segmentPhase=Metrics?.Phase??"free";segmentSeed=MetricsSeed;
        }
        private bool Comparable(long dropped)=>Build.profile!="debug"&&!Host.Clock.MetricsInvalid&&dropped==0;
        private void FlushFrames()
        {
            if(Metrics==null||segmentConfig==null||segmentFrames.SessionFrames==0)return;
            var cfg=segmentConfig;var phase=segmentPhase;var seed=segmentSeed;var currentPhase=Metrics.Phase;
            long dropped=Host.Clock.DroppedSteps-droppedAtSegmentStart,sinceRestart=segmentDroppedSinceRestart;
            bool valid=Comparable(dropped);
            // Fixed numeric schema; unknown/overflow percentiles are JSON null, never a fabricated zero.
            var payload=FormattableString.Invariant($@"{{
                ""frames"":{segmentFrames.SessionFrames},""seconds"":{Number(segmentSeconds)},
                ""p50Ms"":{Number(segmentFrames.SessionPercentile(50))},""p95Ms"":{Number(segmentFrames.SessionPercentile(95))},""p99Ms"":{Number(segmentFrames.SessionPercentile(99))},
                ""histogramResolutionMs"":0.1,""over250MsFrames"":{over250MsFrames},
                ""longFramesPerMinute33Ms"":{Number(segmentFrames.SessionLongFramesPerMinute(33))},
                ""cpuSamples"":{segmentCpu.SessionFrames},""cpuP95Ms"":{Number(segmentCpu.SessionPercentile(95))},""cpuP95Over250Ms"":{Bool(double.IsInfinity(segmentCpu.SessionPercentile(95)))},
                ""gpuSamples"":{segmentGpu.SessionFrames},""gpuP95Ms"":{Number(segmentGpu.SessionPercentile(95))},""gpuP95Over250Ms"":{Bool(double.IsInfinity(segmentGpu.SessionPercentile(95)))},
                ""gcGen0"":{GC.CollectionCount(0)-gcAtSegmentStart},""thermalCurrent"":""{thermal.Current}"",""thermalWorst"":""{thermal.WorstThisSession}"",
                ""dropped"":{Bool(dropped>0)},""droppedSteps"":{dropped},""sessionFrames"":{FrameStats.SessionFrames},""sessionMinutes"":{Number(FrameStats.SessionMinutes)}
            }}");
            ResetSegment(); // Includes any GC caused by writing this record in the next segment.
            Metrics.Phase=phase;
            Metrics.Write("frames",cfg,ActivePresetName,ActivePresetFileHash,Tuning.PendingKeys.Count,seed,valid,sinceRestart,payload);
            Metrics.Phase=currentPhase;
        }
        private static string Number(double value)=>double.IsNaN(value)||double.IsInfinity(value)?"null":value.ToString("R",CultureInfo.InvariantCulture);
        private static string Bool(bool value)=>value?"true":"false";
        private static string FileHash(string text)
        {
            using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-","").ToLowerInvariant();
        }
        private void LateUpdate()
        {
            if (Host.ActiveConfig != null && !Clock.Paused) cameraRig.Tick(Host.RenderPosition, Time.unscaledDeltaTime);
            RecordDiagnosticsFrame(Time.unscaledDeltaTime);
        }

        [SerializeField] private CourtView courtView;
        [SerializeField] private BallView ballView;
        private CourtDimensions shownCourt;
        [SerializeField] private PlaceholderPlayers placeholderPlayers;
        [SerializeField] private CourtCameraRig cameraRig;

        private Label fpsLabel;
        private int frames;
        private float elapsed;
        private double startedAt;
        private bool reportedDisplay;

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            startedAt = Time.realtimeSinceStartupAsDouble;
            var metadata = Resources.Load<TextAsset>("BuildInfo");
            if (metadata != null)
                Build = BuildInfo.FromJson(metadata.text);
#if UNITY_EDITOR
            else
                Build = new BuildInfo { version = Application.version, gitHash = "editor", profile = "editor", unityVersion = Application.unityVersion };
#endif
            if (Build == null)
                throw new InvalidOperationException("Missing build metadata. Use scripts/build-ios.sh.");
            var defaults = Resources.Load<TextAsset>("DefaultPreset");
            if (defaults == null) throw new InvalidOperationException("Missing DefaultPreset.json.");
            var defaultSet=PresetCodec.LoadDefault(defaults.text);
            Presets=new PresetStore(Path.Combine(Application.persistentDataPath,"presets"));
            var warnings=new List<string>();
            Params=Presets.LoadStartup(defaultSet,warnings,out var presetName,out var presetHash);
            ActivePresetName=presetName;ActivePresetFileHash=presetHash??FileHash(defaults.text);
            Tuning=new TuningState(defaultSet,Params);Config=EffectiveConfig.From(Params);
            foreach(var warning in warnings)Debug.LogWarning("[SquashClash] "+warning);
            Metrics=new MetricsLog(Path.Combine(Application.persistentDataPath,"metrics"),Build);FrameStats=new FrameTimeStats();
            thermal=gameObject.AddComponent<ThermalMonitor>();timing=gameObject.AddComponent<FrameTimingSampler>();
            var hasPanel = Type.GetType("SquashClash.DevPanel.DevPanelBootstrap, SquashClash.DevPanel") != null;
#if SQUASH_DEV
            const bool squashDev = true;
#else
            const bool squashDev = false;
#endif
            Debug.Log($"[SquashClash] boot {Build.Label} devpanel={(hasPanel ? "present" : "absent")} development={Debug.isDebugBuild} squashDev={squashDev} unity={Application.unityVersion}");
        }

        private void Start()
        {
            if (courtView == null || placeholderPlayers == null || cameraRig == null || ballView == null)
                throw new InvalidOperationException("The main scene must reference court, camera and players.");

            var root = GetComponent<UIDocument>().rootVisualElement;
            root.Clear();
            root.pickingMode = PickingMode.Ignore;
            root.style.flexGrow = 1;
            root.style.alignItems = Align.Center;
            root.style.paddingTop = 60;
            root.style.backgroundColor = Color.clear;
            var header = new VisualElement { name = "boot-header", pickingMode = PickingMode.Ignore };
            header.style.alignItems = Align.Center;
            header.style.maxWidth = Length.Percent(94);
            header.style.paddingLeft = 12;
            header.style.paddingRight = 12;
            header.style.paddingTop = 8;
            header.style.paddingBottom = 8;
            header.style.backgroundColor = new Color(0.025f, 0.06f, 0.08f, 0.85f);
            root.Add(header);
            var title = new Label("Squash Clash — First Ball") { name = "boot-title", pickingMode = PickingMode.Ignore };
            title.style.fontSize = 18;
            title.style.color = Color.white;
            header.Add(title);
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 4;
            header.Add(row);
            var buildLabel = new Label(Build.Label) { name = "build-label", pickingMode = PickingMode.Ignore };
            buildLabel.style.fontSize = 13;
            buildLabel.style.color = new Color(0.77f, 0.85f, 0.88f);
            row.Add(buildLabel);
            fpsLabel = new Label("… FPS") { name = "fps-label", pickingMode = PickingMode.Ignore };
            fpsLabel.style.fontSize = 13;
            fpsLabel.style.marginLeft = 12;
            fpsLabel.style.color = new Color(0.72f, 0.93f, 0.83f);
            row.Add(fpsLabel);
            var physX = new PhysXBallSimulationFactory();
            Host.Initialize(new IBallSimulationFactory[] { physX, new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold) },
                new SimClock(Config.Physics.StepHz, Config.Tempo));
            Host.Launched += OnLaunched;
            Tuning.Changed += OnTuningChanged;
            GetComponent<ResumeOverlay>().Initialize(Host.Clock);
            ballView.Initialize(Host, Config.Physics.BallRadius);
            Host.Launch(Config);
            gameObject.AddComponent<LegibilityAids>().Initialize(Host, Tuning, ballView);
            var input=gameObject.AddComponent<SquashClash.Runtime.Input.TouchInputAdapter>();
            var reticle=new GameObject("First Shot aim").AddComponent<SquashClash.Runtime.Shot.AimReticleView>();
            reticle.transform.SetParent(transform,false);
            gameObject.AddComponent<SquashClash.Runtime.Shot.GesturePreview>().Initialize(this,input,reticle);
            Debug.Log($"[SquashClash] ball ready backend={Config.Backend} visualColliders=0 localPhysicsScenes={physX.LiveInstances} config={Config.Hash} predictedSteps={Host.Prediction.States.Count-1}");
        }

        private void Update()
        {
            TickRepeat(Time.unscaledDeltaTime);
            if (fpsLabel == null) return;
            elapsed += Time.unscaledDeltaTime;
            frames++;
            if (elapsed < 0.5f) return;
            var fps = frames / elapsed;
            fpsLabel.text = $"{fps:0} FPS";
            frames = 0;
            elapsed = 0;
            if (!reportedDisplay && Time.realtimeSinceStartupAsDouble - startedAt >= 3)
            {
                reportedDisplay = true;
                Debug.Log($"[SquashClash] display profile={Build.profile} fps={fps:0.0} orientation={Screen.orientation} resolution={Screen.width}x{Screen.height}");
            }
        }

        private void OnLaunched(EffectiveConfig cfg)
        {
            FlushFrames();repeatElapsed=0;Clock.DebugSlowMotion=cfg.Source.Get("debug.slowMotion");Config=cfg; Params=cfg.Source;segmentConfig=cfg;ResetSegment();timing.RestartCapture();
            Metrics.Write("launch",cfg,ActivePresetName,ActivePresetFileHash,Tuning.PendingKeys.Count,MetricsSeed,Comparable(0),Host.Clock.DroppedSinceRestart,"{}");
            var court=cfg.Court;
            if (shownCourt==null || !shownCourt.Equals(court)) { courtView.Build(court); shownCourt=court; }
            placeholderPlayers.Apply(cfg.Source); cameraRig.Apply(cfg.Source,court);
        }
        private void OnApplicationPause(bool paused)
        {
            repeatElapsed=0;discardRepeatResume=true;
            if(paused)FlushFrames();diagnosticsPaused=paused;discardDiagnosticsResume=true;
            if(paused)GetComponent<ResumeOverlay>().Suspend();
        }
        private void OnApplicationFocus(bool focused)
        {
            repeatElapsed=0;discardRepeatResume=true;
            if(!focused)FlushFrames();diagnosticsFocused=focused;discardDiagnosticsResume=true;
            if(!focused)GetComponent<ResumeOverlay>().Suspend();
        }
        private void OnDestroy()
        {
            FlushFrames();
            if (Host != null) Host.Launched -= OnLaunched;
            if(Tuning!=null)Tuning.Changed-=OnTuningChanged;
            if (Instance == this) Instance = null;
        }
    }
}
