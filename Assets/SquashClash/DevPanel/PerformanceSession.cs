using System;
using System.Globalization;
using SquashClash.Core.Diagnostics;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // H17 instrumentation belongs only to dev. It runs ordinary launches, never a spike or benchmark.
    [DefaultExecutionOrder(200)]
    public sealed class PerformanceSession : MonoBehaviour
    {
        public const double DurationSeconds=900,CaseSeconds=150;
        private readonly FrameTimeStats frames=new FrameTimeStats(),cpu=new FrameTimeStats(),gpu=new FrameTimeStats();
        private readonly double[] thermalSeconds=new double[5],caseSeconds=new double[6];
        private AppBootstrap app;
        private VisualElement root;
        private Label progress;
        private Action<int> selectLaunch;
        private string previousPhase,startedAt,simulationKey,thermalStart,worst;
        private int previousSeed,previousSleep,caseIndex,shownSecond;
        private long droppedStart,lastTiming;
        private double seconds,repeatSeconds,brightnessStart;
        private bool skipStartFrame;
        public bool IsRunning {get;private set;}
        public string Status {get;private set;}="Sesión H17 · 15 min";
        public void Initialize(AppBootstrap app,VisualElement root,Action<int> selectLaunch)
        {
            this.app=app;this.root=root;this.selectLaunch=selectLaunch;
            progress=new Label {name="performance-session-status"};progress.style.fontSize=13;progress.style.whiteSpace=WhiteSpace.Normal;
            progress.style.maxWidth=Length.Percent(100);progress.style.display=DisplayStyle.None;root.Q("boot-header").Add(progress);
        }
        private void ShowStatus(){progress.text=Status;progress.style.display=DisplayStyle.Flex;}
        public bool Begin()
        {
            if(IsRunning)return false;
            string state=app.GetComponent<ThermalMonitor>().Current;
            if((GetComponent<SpikeSection>()?.IsRunning??false)||app.Tuning.PendingKeys.Count>0||
                app.Tuning.Applied.Get("launcher.repeatSeconds")<=0||app.Tuning.Applied.Get("debug.slowMotion")!=1||
                app.Clock.MetricsInvalid||app.Build.profile=="debug"||app.Config.Backend!="P2-Analytic"||
                (Application.platform==RuntimePlatform.IPhonePlayer&&state!="nominal"))
            {
                Status="H17 no iniciada: aplica cambios, P2, repetición > 0, sin cámara lenta y térmico nominal.";ShowStatus();return false;
            }
            frames.ResetSession();cpu.ResetSession();gpu.ResetSession();Array.Clear(thermalSeconds,0,5);Array.Clear(caseSeconds,0,6);
            seconds=0;caseIndex=0;shownSecond=0;thermalStart=worst=state;startedAt=DateTime.UtcNow.ToString("O");
            repeatSeconds=app.Tuning.Applied.Get("launcher.repeatSeconds");brightnessStart=Screen.brightness;
            previousPhase=app.Metrics.Phase;previousSeed=app.MetricsSeed;app.SetMetricsPhase("session");
            simulationKey=app.Config.SimulationKey;droppedStart=app.Clock.DroppedSteps;
            previousSleep=Screen.sleepTimeout;Screen.sleepTimeout=SleepTimeout.NeverSleep;
            IsRunning=true;skipStartFrame=true;
            root.Q("dev-panel").style.display=DisplayStyle.None;
            selectLaunch(1);app.Clock.Paused=false;lastTiming=app.GetComponent<FrameTimingSampler>().SampleSerial;
            app.Metrics.Write("session-start",app.Config,app.ActivePresetName,app.ActivePresetFileHash,0,0,true,0,
                "{\"durationSeconds\":900,\"caseSeconds\":150,\"repeatSeconds\":"+Number(repeatSeconds)+
                ",\"simulationKey\":\""+simulationKey+"\",\"thermalStart\":\""+state+"\",\"brightnessStart\":"+Number(brightnessStart)+"}");
            Status="H17 L1 · 00:00 / 15:00";ShowStatus();return true;
        }
        private void LateUpdate()
        {
            if(!IsRunning)return;
            if(skipStartFrame){skipStartFrame=false;return;} // Exclude UI/start delta before the boundary.
            Tick(Time.unscaledDeltaTime);
        }
        public void Tick(double deltaSeconds)
        {
            if(!IsRunning)return;
            if(double.IsNaN(deltaSeconds)||double.IsInfinity(deltaSeconds)||deltaSeconds<0)throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if(app.Metrics.Phase!="session"||app.Clock.Paused||app.Clock.MetricsInvalid||app.Tuning.Applied.Get("debug.slowMotion")!=1||
                app.Config.SimulationKey!=simulationKey||app.Tuning.Applied.Get("launcher.repeatSeconds")!=repeatSeconds||
                root.Q("dev-panel").style.display.value!=DisplayStyle.None)
            {Finish(false,"Condiciones H17 modificadas");return;}
            if(deltaSeconds==0)return;
            frames.Add(deltaSeconds*1000);seconds+=deltaSeconds;caseSeconds[caseIndex]+=deltaSeconds;
            var timing=app.GetComponent<FrameTimingSampler>();
            if(timing.SampleSerial!=lastTiming)
            {
                lastTiming=timing.SampleSerial;
                if(!double.IsNaN(timing.CpuMs))cpu.Add(timing.CpuMs);
                if(!double.IsNaN(timing.GpuMs))gpu.Add(timing.GpuMs);
            }
            string state=app.GetComponent<ThermalMonitor>().Current;int rank=Rank(state);thermalSeconds[rank]+=deltaSeconds;
            if(rank>Rank(worst))worst=state;
            if(seconds>=DurationSeconds){Finish(true,"15 minutos completos");return;}
            int next=Math.Min(5,(int)(seconds/CaseSeconds));
            if(next!=caseIndex){caseIndex=next;selectLaunch(caseIndex+1);}
            int whole=(int)seconds;
            if(whole!=shownSecond){shownSecond=whole;Status=$"H17 L{caseIndex+1} · {whole/60:00}:{whole%60:00} / 15:00";ShowStatus();}
        }
        public void Cancel()=>Finish(false,"Cancelada por usuario");
        private void Finish(bool complete,string reason)
        {
            if(!IsRunning)return;IsRunning=false;Screen.sleepTimeout=previousSleep;
            if(app==null)return;
            long dropped=app.Clock.DroppedSteps-droppedStart;
            // Flush the final ordinary frame segment before restoring the previous phase/seed.
            app.SetMetricsPhase(previousPhase,previousSeed);
            string payload="{\"complete\":"+(complete?"true":"false")+",\"reason\":\""+reason+"\",\"startedAtUtc\":\""+startedAt+
                "\",\"endedAtUtc\":\""+DateTime.UtcNow.ToString("O")+"\",\"seconds\":"+Number(seconds)+
                ",\"simulationKey\":\""+simulationKey+"\",\"frames\":"+frames.SessionFrames+
                ",\"p50Ms\":"+Number(frames.SessionPercentile(50))+",\"p95Ms\":"+Number(frames.SessionPercentile(95))+",\"p99Ms\":"+Number(frames.SessionPercentile(99))+
                ",\"histogramResolutionMs\":0.1,\"longFramesPerMinute33Ms\":"+Number(frames.SessionLongFramesPerMinute(33))+
                ",\"cpuSamples\":"+cpu.SessionFrames+",\"cpuP95Ms\":"+Number(cpu.SessionPercentile(95))+
                ",\"gpuSamples\":"+gpu.SessionFrames+",\"gpuP95Ms\":"+Number(gpu.SessionPercentile(95))+
                ",\"thermalStart\":\""+thermalStart+"\",\"thermalWorst\":\""+worst+"\",\"unknownSeconds\":"+Number(thermalSeconds[0])+
                ",\"nominalSeconds\":"+Number(thermalSeconds[1])+",\"fairSeconds\":"+Number(thermalSeconds[2])+",\"seriousSeconds\":"+Number(thermalSeconds[3])+",\"criticalSeconds\":"+Number(thermalSeconds[4])+
                ",\"droppedSteps\":"+dropped+",\"repeatSeconds\":"+Number(repeatSeconds)+",\"brightnessStart\":"+Number(brightnessStart)+
                ",\"casesVisited\":"+(caseIndex+1)+",\"caseSeconds\":["+string.Join(",",Array.ConvertAll(caseSeconds,Number))+"]}";
            app.Metrics.Phase="session";
            app.Metrics.Write("session-summary",app.Config,app.ActivePresetName,app.ActivePresetFileHash,app.Tuning.PendingKeys.Count,0,
                complete&&!app.Clock.MetricsInvalid&&dropped==0,app.Clock.DroppedSinceRestart,payload);
            app.Metrics.Phase=previousPhase;
            Status=complete?"H17 completa · informe guardado":"H17 incompleta · "+reason;
            ShowStatus();
        }
        private static string Number(double n)=>double.IsNaN(n)||double.IsInfinity(n)?"null":n.ToString("R",CultureInfo.InvariantCulture);
        private static int Rank(string s)=>s switch {"nominal"=>1,"fair"=>2,"serious"=>3,"critical"=>4,_=>0};
        private void OnApplicationFocus(bool value){if(!value)Finish(false,"Pérdida de foco");}
        private void OnApplicationPause(bool value){if(value)Finish(false,"Segundo plano");}
        private void OnDestroy()=>Finish(false,"Sesión destruida");
    }
}
