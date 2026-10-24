using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.DevPanel;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests
{
    public sealed class TuningDiagnosticsIntegrationTests
    {
        [Serializable] private sealed class Record{public string kind,phase,effectiveHash,presetName,presetFileHash,payload;public int pending,seed;public bool metricsValid;public long droppedSinceRestart;}
        [Serializable] private sealed class Frames{public long frames,cpuSamples,gpuSamples,droppedSteps;public double p50Ms,p95Ms,p99Ms;public string thermalCurrent,thermalWorst;public bool dropped;}
        [UnitySetUp] public IEnumerator Setup(){yield return PlayModeTestWorld.ClearApp();}
        [UnityTearDown] public IEnumerator Cleanup()
        {
            var section=Object.FindFirstObjectByType<SpikeSection>();if(section!=null&&section.LastReportPath!=null)File.Delete(section.LastReportPath);
            yield return PlayModeTestWorld.ClearApp();
        }
        private static Record[] Records(AppBootstrap app)=>File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Record>).ToArray();
        private static void StopAutomaticSampling(AppBootstrap app)
        {
            app.enabled=false;app.Host.enabled=false;app.SendMessage("OnApplicationPause",false);app.SendMessage("OnApplicationFocus",true);
            app.GetComponent<ThermalMonitor>().enabled=false;app.GetComponent<FrameTimingSampler>().enabled=false;
            app.GetComponent<FrameTimingSampler>().RestartCapture();app.RecordDiagnosticsFrame(.001); // discard resume delta
        }
        [UnityTest] public IEnumerator BootAndEveryLaunchCarryAppliedSnapshotAndPendingCount()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;Assert.That(app.Tuning,Is.Not.Null);Assert.That(app.Presets,Is.Not.Null);Assert.That(app.Tuning.PendingKeys,Is.Empty);
            Assert.That(app.Tuning.Applied.Hash,Is.EqualTo(app.Config.Hash));StopAutomaticSampling(app);
            var initial=Records(app).Last(r=>r.kind=="launch");Assert.That(initial.effectiveHash,Is.EqualTo(app.Config.Hash));Assert.That(initial.presetFileHash,Has.Length.EqualTo(64));
            app.Tuning.Set("launcher.speed",20);var cfg=EffectiveConfig.From(app.Params.With("physics.gravity",12));app.Host.Launch(cfg);
            var launch=Records(app).Last(r=>r.kind=="launch");Assert.That(launch.effectiveHash,Is.EqualTo(cfg.Hash));Assert.That(launch.pending,Is.EqualTo(1));Assert.That(launch.droppedSinceRestart,Is.Zero);
        }
        [UnityTest] public IEnumerator TenSecondSegmentsHaveComparableFieldsAndExplicitTimingAvailability()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;StopAutomaticSampling(app);app.Host.Launch(app.Config);app.SetMetricsPhase("session",42);
            app.GetComponent<ThermalMonitor>().Observe("serious",1);app.GetComponent<ThermalMonitor>().Observe("fair",1);
            long before=app.FrameStats.SessionFrames;for(int i=0;i<40;i++)app.RecordDiagnosticsFrame(.25);
            var row=Records(app).Last(r=>r.kind=="frames");var f=JsonUtility.FromJson<Frames>(row.payload);
            Assert.That(row.phase,Is.EqualTo("session"));Assert.That(row.seed,Is.EqualTo(42));Assert.That(f.frames,Is.EqualTo(40));
            Assert.That(f.p50Ms,Is.EqualTo(250));Assert.That(f.p95Ms,Is.EqualTo(250));Assert.That(f.p99Ms,Is.EqualTo(250));
            Assert.That(f.thermalCurrent,Is.EqualTo("fair"));Assert.That(f.thermalWorst,Is.EqualTo("serious"));
            Assert.That(f.cpuSamples,Is.Zero);Assert.That(f.gpuSamples,Is.Zero);Assert.That(f.dropped,Is.False);Assert.That(row.metricsValid,Is.True);
            Assert.That(app.FrameStats.SessionFrames-before,Is.EqualTo(40));
        }
        [UnityTest] public IEnumerator DroppedStepsAndDebugSlowMotionInvalidateTheirSegments()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;StopAutomaticSampling(app);app.Host.Launch(app.Config);app.SetMetricsPhase("session");
            app.Host.Clock.Advance(100);app.RecordDiagnosticsFrame(10);
            var row=Records(app).Last(r=>r.kind=="frames");var payload=JsonUtility.FromJson<Frames>(row.payload);
            Assert.That(row.metricsValid,Is.False);Assert.That(payload.dropped,Is.True);Assert.That(payload.droppedSteps,Is.GreaterThan(0));Assert.That(row.droppedSinceRestart,Is.GreaterThan(0));
            app.Host.Replay();app.Host.Clock.DebugSlowMotion=.5;app.RecordDiagnosticsFrame(10);
            row=Records(app).Last(r=>r.kind=="frames");payload=JsonUtility.FromJson<Frames>(row.payload);
            Assert.That(row.metricsValid,Is.False);Assert.That(payload.dropped,Is.False);
        }
        [UnityTest] public IEnumerator PhaseAndResumeBoundariesDoNotMixSegmentsOrCountBackgroundTime()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;StopAutomaticSampling(app);app.Host.Launch(app.Config);app.SetMetricsPhase("free");app.RecordDiagnosticsFrame(.5);
            app.SetMetricsPhase("benchmark",20261020);app.RecordDiagnosticsFrame(.5);app.SetMetricsPhase("free");
            var rows=Records(app).Where(r=>r.kind=="frames").ToArray();Assert.That(rows[rows.Length-2].phase,Is.EqualTo("free"));Assert.That(rows.Last().phase,Is.EqualTo("benchmark"));Assert.That(rows.Last().seed,Is.EqualTo(20261020));
            long before=app.FrameStats.SessionFrames;app.SendMessage("OnApplicationPause",true);app.RecordDiagnosticsFrame(60);
            app.SendMessage("OnApplicationPause",false);app.RecordDiagnosticsFrame(60);app.RecordDiagnosticsFrame(.016);
            Assert.That(app.FrameStats.SessionFrames-before,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator SpikeBeginAndCancellationRestoreMetricsPhaseAndSeed()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var app=AppBootstrap.Instance;app.SetMetricsPhase("session",5);var section=Object.FindFirstObjectByType<SpikeSection>();section.Begin();
            Assert.That(app.Metrics.Phase,Is.EqualTo("spike"));Assert.That(app.MetricsSeed,Is.EqualTo(20261020));
            section.CancelRun("metrics integration test");Assert.That(app.Metrics.Phase,Is.EqualTo("session"));Assert.That(app.MetricsSeed,Is.EqualTo(5));
        }
    }
}
