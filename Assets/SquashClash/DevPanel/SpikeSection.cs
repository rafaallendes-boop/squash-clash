using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SquashClash.Core.Config;
using SquashClash.Core.Spike;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    public sealed class SpikeSection : MonoBehaviour
    {
        private AppBootstrap app;
        private VisualElement layer;
        private Label status, reportLabel;
        private ScrollView scroll;
        private Button cancel, close;
        private SpikeRun run;
        private PredictionBenchmarkRun benchmark;
        private PhysXBallSimulationFactory factory;
        private EffectiveConfig config;
        private SpikeResult result;
        private Vector3 savedGravity;
        private int previousSleepTimeout, scenarioIndex, progressMark;
        private bool waiting, restoreHost;
        private string started, thermalStart;
        public bool IsRunning { get; private set; }
        public string LastReport { get; private set; }
        public string LastReportPath { get; private set; }
        public string SelectedScenario { get; private set; }
        public void Initialize(AppBootstrap app, VisualElement root)
        {
            this.app = app;
            layer = new VisualElement { name = "spike-section" };
            layer.style.position = Position.Absolute; layer.style.left = Length.Percent(3); layer.style.right = Length.Percent(3);
            layer.style.top = Length.Percent(17); layer.style.bottom = Length.Percent(21);
            layer.style.backgroundColor = new Color(.015f,.03f,.04f,.98f); layer.style.paddingLeft = layer.style.paddingRight = 12;
            layer.style.paddingTop = layer.style.paddingBottom = 12;
            status = new Label { name = "spike-status" }; status.style.fontSize = 15; status.style.whiteSpace = WhiteSpace.Normal;
            status.style.color = Color.white; layer.Add(status);
            scroll = new ScrollView(); scroll.style.flexGrow = 1; layer.Add(scroll);
            reportLabel = new Label { name = "spike-report" }; reportLabel.style.fontSize = 12; reportLabel.style.whiteSpace = WhiteSpace.Normal; reportLabel.style.color = Color.white; scroll.Add(reportLabel);
            cancel = new Button(() => CancelRun("Cancelado por usuario")) { text = "Cancelar", name = "spike-cancel" }; cancel.style.height = 44; layer.Add(cancel);
            close = new Button(() => layer.style.display = DisplayStyle.None) { text = "Cerrar informe", name = "spike-close" }; close.style.height = 44; layer.Add(close);
            root.Insert(Math.Max(0, root.childCount - 1), layer); layer.style.display = DisplayStyle.None;
        }
        public void LaunchNextScenario()
        {
            if (IsRunning) return;
            var cfg = EffectiveConfig.From(app.Params);
            var scenarios = PhysicsScenarios.Fixed(cfg.Court, cfg.Physics).ToList();
            scenarios.Insert(7, PhysicsScenarios.Random(cfg.Court, cfg.Physics, 1, 20261020).Single());
            var scenario = scenarios[scenarioIndex++ % scenarios.Count];
            SelectedScenario = scenario.Id;
            app.Host.Launch(cfg, scenario.Start, scenario.MaxSteps, scenario.Stop);
            app.Host.Clock.Paused = false;
        }
        public void Begin()
        {
            if (IsRunning) return;
            layer.style.display = DisplayStyle.Flex; scroll.style.display = DisplayStyle.None;
            close.style.display = DisplayStyle.None; cancel.style.display = DisplayStyle.Flex;
            string thermal = ThermalState();
            if (Application.platform == RuntimePlatform.IPhonePlayer && thermal != "nominal")
            {
                status.text = "Espera a que el iPhone se enfríe: térmico=" + thermal + ". No se inició la prueba.";
                cancel.style.display = DisplayStyle.None; close.style.display = DisplayStyle.Flex; return;
            }
            started = DateTime.UtcNow.ToString("O"); thermalStart = thermal;
            config = EffectiveConfig.From(app.Params); savedGravity = UnityEngine.Physics.gravity;
            previousSleepTimeout = Screen.sleepTimeout; Screen.sleepTimeout = SleepTimeout.NeverSleep;
            app.Host.ReleaseForExperiment(); IsRunning = true; waiting = true; progressMark = -1;
            run = null; benchmark = null; result = null; LastReport = null; LastReportPath = null;
            status.text = "Preparando escenas…";
        }
        private void Update()
        {
            if (restoreHost && !IsRunning && !HasPhysicsScenes() && app != null && app.Host != null)
            { restoreHost = false; app.Host.Replay(); app.Host.Clock.Paused = true; }
            if (!IsRunning) return;
            try
            {
                if (waiting)
                {
                    if (HasPhysicsScenes()) return;
                    waiting = false; factory = new PhysXBallSimulationFactory();
                    run = new SpikeRun(factory, config.Physics, config.Court, new SpikeOptions {
                        Device = SystemInfo.deviceModel, Build = app.Build.Label, ConfigHash = config.Hash,
                        Variant = "baseline/" + PhysXVariant.Baseline.Name,
                        ProjectPhysics = "EnhancedDeterminism=true; bounceThreshold=" + UnityEngine.Physics.bounceThreshold + "; Unity=" + Application.unityVersion
                    }); result = run.Result;
                }
                if (benchmark == null)
                {
                    bool done = run.Step(8); status.text = $"{run.Progress:P0} — {run.Phase}";
                    int mark = (int)(run.Progress * 20);
                    if (mark != progressMark) { progressMark = mark; Debug.Log("[SquashClash] spike " + status.text); }
                    if (!done) return;
                    benchmark = new PredictionBenchmarkRun(factory, config.Physics, config.Court);
                    result.Details.Add("Térmico antes del benchmark=" + ThermalState());
                    return; // Separate frames; do not spend two budgets in one frame.
                }
                status.text = "Benchmark — 20 calentamientos y 200 mediciones; n=" + benchmark.Samples;
                if (!benchmark.Step(8)) return;
                benchmark.ApplyTo(result); Finish("Prueba completa");
            }
            catch (Exception e) { Debug.LogException(e); CancelRun("Excepción: " + e.GetType().Name + ": " + e.Message); }
        }
        public void CancelRun(string reason)
        {
            if (!IsRunning) return;
            run?.Cancel(); benchmark?.Cancel();
            if (result == null) result = new SpikeResult { Backend = "P1-PhysX", Variant = "baseline/" + PhysXVariant.Baseline.Name,
                Build = app.Build.Label, ConfigHash = config.Hash, Device = SystemInfo.deviceModel,
                Criteria = SpikeCriteria.CreateResults(Application.platform == RuntimePlatform.IPhonePlayer) };
            result.Complete = false; result.Details.Add(reason); Finish("INCOMPLETO — " + reason);
        }
        private void Finish(string message)
        {
            IsRunning = waiting = false; run?.Cancel(); benchmark?.Cancel();
            UnityEngine.Physics.gravity = savedGravity; Screen.sleepTimeout = previousSleepTimeout;
            result.Profile = app.Build.profile; result.OS = SystemInfo.operatingSystem;
            result.StartedAtUtc = started; result.EndedAtUtc = DateTime.UtcNow.ToString("O");
            result.ThermalStart = thermalStart; result.ThermalEnd = ThermalState();
            LastReport = SpikeReportWriter.ToMarkdown(result); GUIUtility.systemCopyBuffer = LastReport;
            var directory = Path.Combine(Application.persistentDataPath, "spike"); Directory.CreateDirectory(directory);
            LastReportPath = Path.Combine(directory, "P1-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".md");
            File.WriteAllText(LastReportPath, LastReport);
            status.text = message + "\nInforme guardado y copiado."; reportLabel.text = LastReport;
            scroll.style.display = DisplayStyle.Flex; cancel.style.display = DisplayStyle.None; close.style.display = DisplayStyle.Flex;
            restoreHost = true;
            Debug.Log("[SquashClash] spike saved complete=" + result.Complete + " thermal=" + result.ThermalStart + "→" + result.ThermalEnd + " path=" + LastReportPath);
        }
        private static bool HasPhysicsScenes() => Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).Any(s => s.name.StartsWith("P1-PhysX-", StringComparison.Ordinal));
        private void OnApplicationPause(bool paused) { if (paused) CancelRun("Segundo plano"); }
        private void OnApplicationFocus(bool focused) { if (!focused) CancelRun("Pérdida de foco"); }
        private void OnDestroy() { CancelRun("Sección destruida"); layer?.RemoveFromHierarchy(); }
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int SquashClashThermalState();
#endif
        private static string ThermalState()
        {
#if UNITY_IOS && !UNITY_EDITOR
            switch (SquashClashThermalState()) { case 0: return "nominal"; case 1: return "fair"; case 2: return "serious"; case 3: return "critical"; default: return "unknown"; }
#else
            return "editor / no disponible";
#endif
        }
    }
}
