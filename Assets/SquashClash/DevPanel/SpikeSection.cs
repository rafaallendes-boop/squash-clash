using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SquashClash.Core.Config;
using SquashClash.Core.Spike;
using SquashClash.Core.Simulation;
using System.Globalization;
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
        private IBallSimulationFactory factory;
        private const string ExperimentBackend="P2-Analytic", ExperimentVariant="analytic/r2-20261023";
        private EffectiveConfig config;
        private SpikeResult result;
        private Vector3 savedGravity;
        private int previousSleepTimeout, scenarioIndex, progressMark;
        private bool waiting, restoreHost;
        private double startedRealtime, focusLostAt;
        private bool waitingForFocus;
        private int focusInterruptionCount;
        private readonly List<string> focusEvents = new List<string>();
        private string focusLossContext, thermalBenchmarkStart;
        private string started, thermalStart, reportDirectory;
        public bool IsRunning { get; private set; }
        public string LastReport { get; private set; }
        public string LastReportPath { get; private set; }
        public string SelectedScenario { get; private set; }
        public void Initialize(AppBootstrap app, VisualElement root, string reportDirectory = null)
        {
            this.app = app;
            this.reportDirectory = reportDirectory ?? Path.Combine(Application.persistentDataPath, "spike");
            layer?.RemoveFromHierarchy();
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
            startedRealtime = Time.realtimeSinceStartupAsDouble;
            waitingForFocus = false; focusInterruptionCount = 0; focusEvents.Clear();
            focusLossContext = thermalBenchmarkStart = null;
            config = EffectiveConfig.From(app.Params); savedGravity = UnityEngine.Physics.gravity;
            previousSleepTimeout = Screen.sleepTimeout; Screen.sleepTimeout = SleepTimeout.NeverSleep;
            app.Host.ReleaseForExperiment(); IsRunning = true; waiting = true; progressMark = -1;
            run = null; benchmark = null; result = null; LastReport = null; LastReportPath = null;
            status.text = "Preparando prueba P2…";
        }
        private void Update()
        {
            if (restoreHost && !IsRunning && !HasPhysicsScenes() && app != null && app.Host != null)
            { restoreHost = false; app.Host.Replay(); app.Host.Clock.Paused = true; }
            if (!IsRunning) return;
            if (waitingForFocus)
            {
                if (Time.realtimeSinceStartupAsDouble - focusLostAt >= 60)
                    CancelRun("Foco no recuperado en 60 s", LifecycleContext("focus(timeout)"));
                return; // No robustness jobs or benchmark samples while focus is absent.
            }
            try
            {
                if (waiting)
                {
                    if (HasPhysicsScenes()) return;
                    waiting = false; factory = new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
                    run = new SpikeRun(factory, config.Physics, config.Court, new SpikeOptions {
                        Device = Application.isEditor ? "editor" : SystemInfo.deviceModel, Build = app.Build.Label, ConfigHash = config.Hash,
                        Variant = ExperimentVariant,
                        ProjectPhysics = ProjectPhysicsDescription()
                    }); result = run.Result;
                }
                if (benchmark == null)
                {
                    bool done = run.Step(8); status.text = $"{run.Progress:P0} — {run.Phase}";
                    int mark = (int)(run.Progress * 20);
                    if (mark != progressMark) { progressMark = mark; Debug.Log("[SquashClash] spike " + status.text); }
                    if (!done) return;
                    thermalBenchmarkStart = ThermalState(); // Informational snapshot before scene creation/warmup.
                    benchmark = new PredictionBenchmarkRun(factory, config.Physics, config.Court);
                    return; // Separate frames; do not spend two budgets in one frame.
                }
                status.text = "Benchmark — 20 calentamientos y 200 mediciones; n=" + benchmark.Samples;
                if (!benchmark.Step(8)) return;
                benchmark.ApplyTo(result); Finish("Prueba completa");
            }
            catch (Exception e) { Debug.LogException(e); CancelRun("Excepción: " + e.GetType().Name + ": " + e.Message); }
        }
        private string ProjectPhysicsDescription()
        {
            string description=app.Build.projectPhysics;
#if UNITY_EDITOR
            var settings=new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset")[0]);
            description="EnhancedDeterminism="+settings.FindProperty("m_EnableEnhancedDeterminism").boolValue+
                "; fastMotionThreshold="+settings.FindProperty("m_FastMotionThreshold").floatValue.ToString("R",CultureInfo.InvariantCulture)+
                "; bounceThreshold="+UnityEngine.Physics.bounceThreshold.ToString("R",CultureInfo.InvariantCulture)+"; Unity="+Application.unityVersion;
#endif
            return (description??"ProjectPhysics no disponible")+"; fastMotionThreshold relevante solo para P1; bounceThreshold efectivo P2="+
                UnityEngine.Physics.bounceThreshold.ToString("R",CultureInfo.InvariantCulture);
        }
        public void CancelRun(string reason) => CancelRun(reason, null);
        private void CancelRun(string reason, string lifecycleContext)
        {
            if (!IsRunning) return;
            EndFocusInterruption("cancelled", "focus(end)");
            run?.Cancel(); benchmark?.Cancel();
            if (result == null) result = new SpikeResult { Backend = ExperimentBackend, Variant = ExperimentVariant, ProjectPhysics=ProjectPhysicsDescription(),
                Build = app.Build.Label, ConfigHash = config.Hash, Device = Application.isEditor ? "editor" : SystemInfo.deviceModel,
                Criteria = SpikeCriteria.CreateResults(Application.platform == RuntimePlatform.IPhonePlayer) };
            if (lifecycleContext != null) result.Details.Add(lifecycleContext);
            result.Complete = false; result.Details.Add(reason); Finish("INCOMPLETO — " + reason);
        }
        private void Finish(string message)
        {
            IsRunning = waiting = false;
            try { run?.Cancel(); benchmark?.Cancel(); }
            finally
            {
                UnityEngine.Physics.gravity = savedGravity; Screen.sleepTimeout = previousSleepTimeout;
                restoreHost = true;
            }
            result.Profile = app.Build.profile; result.OS = SystemInfo.operatingSystem;
            result.StartedAtUtc = started; result.EndedAtUtc = DateTime.UtcNow.ToString("O");
            result.ThermalStart = thermalStart; result.ThermalEnd = ThermalState();
            result.ThermalBenchmarkStart = thermalBenchmarkStart;
            result.FocusInterruptions = focusInterruptionCount; result.Details.AddRange(focusEvents);
            LastReport = SpikeReportWriter.ToMarkdown(result); LastReportPath = null;
            // Recover controls and present the in-memory result before attempting independent outputs.
            reportLabel.text = LastReport; scroll.style.display = DisplayStyle.Flex;
            cancel.style.display = DisplayStyle.None; close.style.display = DisplayStyle.Flex;
            var outputs = new List<string>();
            try { GUIUtility.systemCopyBuffer = LastReport; outputs.Add("Informe copiado."); }
            catch (Exception e) { outputs.Add("No se pudo copiar: " + e.GetType().Name); Debug.LogWarning("[SquashClash] " + outputs[outputs.Count - 1]); }
            try
            {
                Directory.CreateDirectory(reportDirectory);
                var path = Path.Combine(reportDirectory, "P1-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".md");
                File.WriteAllText(path, LastReport); LastReportPath = path; outputs.Add("Informe guardado.");
            }
            catch (Exception e) { outputs.Add("No se pudo guardar: " + e.GetType().Name); Debug.LogWarning("[SquashClash] " + outputs[outputs.Count - 1]); }
            status.text = message + "\n" + string.Join("\n", outputs);
            Debug.Log("[SquashClash] spike output complete=" + result.Complete + " thermal=" + result.ThermalStart + "→" + result.ThermalEnd + " saved=" + (LastReportPath != null));
        }
        private static bool HasPhysicsScenes() => Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).Any(s => s.name.StartsWith("P1-PhysX-", StringComparison.Ordinal));
        private string LifecycleContext(string lifecycleEvent)
        {
            string phase = waiting ? "Preparando escenas" : benchmark != null ? "Benchmark" : run?.Phase ?? "sin runner";
            float progress = run?.Progress ?? 0;
            string uikitState = "editor-unavailable", idleTimerDisabled = "editor-unavailable";
#if UNITY_IOS && !UNITY_EDITOR
            switch (SquashClashApplicationState())
            { case 0: uikitState = "active"; break; case 1: uikitState = "inactive"; break; case 2: uikitState = "background"; break; default: uikitState = "unknown"; break; }
            idleTimerDisabled = SquashClashIdleTimerDisabled() == 1 ? "true" : "false";
#endif
            return FormattableString.Invariant($"Lifecycle event={lifecycleEvent}; utc={DateTime.UtcNow:O}; elapsedSeconds={Time.realtimeSinceStartupAsDouble - startedRealtime:F3}; frame={Time.frameCount}; unityFocused={Application.isFocused}; uikitState={uikitState}; uikitIdleTimerDisabled={idleTimerDisabled}; sleepTimeout={Screen.sleepTimeout}; thermal={ThermalState()}; phase={phase}; progress={progress:F6}; benchmarkSamples={benchmark?.Samples ?? 0}");
        }
        private void OnApplicationPause(bool paused) { if (paused && IsRunning) CancelRun("Segundo plano", LifecycleContext("pause(true)")); }
        private void OnApplicationFocus(bool focused)
        {
            if (!IsRunning) return;
            if (!focused)
            {
                if (!waitingForFocus)
                {
                    waitingForFocus = true; focusLostAt = Time.realtimeSinceStartupAsDouble;
                    focusInterruptionCount++; focusLossContext = LifecycleContext("focus(false)");
                }
                if (benchmark != null) CancelRun("Pérdida de foco durante C17");
                return;
            }
            if (!waitingForFocus) return;
            if (Time.realtimeSinceStartupAsDouble - focusLostAt >= 60)
            { CancelRun("Foco no recuperado en 60 s", LifecycleContext("focus(true)")); return; }
            EndFocusInterruption("resumed", "focus(true)");
            // The Runtime overlay also receives focus(false). Resume this experiment
            // automatically without changing normal gameplay's tap-to-resume policy.
            app.GetComponent<ResumeOverlay>().Continue(); app.Host.Clock.Paused = true;
        }
        private void EndFocusInterruption(string outcome, string lifecycleEvent)
        {
            if (!waitingForFocus) return;
            string endContext = LifecycleContext(lifecycleEvent);
            focusEvents.Add(focusLossContext);
            focusEvents.Add(FormattableString.Invariant($"Focus interruption={focusInterruptionCount}; durationSeconds={Time.realtimeSinceStartupAsDouble - focusLostAt:F3}; outcome={outcome}; {endContext}"));
            waitingForFocus = false;
        }
        private void OnDestroy() { CancelRun("Sección destruida"); layer?.RemoveFromHierarchy(); }
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int SquashClashThermalState();
        [DllImport("__Internal")] private static extern int SquashClashApplicationState();
        [DllImport("__Internal")] private static extern int SquashClashIdleTimerDisabled();
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
