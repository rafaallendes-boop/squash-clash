using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    public sealed class DevPanelIntegrationTests
    {
        [Serializable] private sealed class Row { public string kind,effectiveHash; public bool metricsValid; public int pending; }
        [UnitySetUp] public IEnumerator Setup(){yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;}
        [UnityTearDown] public IEnumerator Cleanup(){yield return PlayModeTestWorld.ClearApp();}
        [UnityTest] public IEnumerator LaunchCurrent_AppliesPendingAndKeepsBothBackendsAvailable()
        {
            var app=AppBootstrap.Instance;
            foreach(var backend in new[]{0,1})
            {
                app.Tuning.Set("physics.backend",backend);app.Tuning.Set("physics.floor.restitution",.6);app.LaunchCurrent();
                Assert.That(app.Tuning.PendingKeys,Is.Empty);Assert.That(app.Ball.ActiveConfig.Physics.Floor.Restitution,Is.EqualTo(.6f));
                Assert.That(app.Config.Hash,Is.EqualTo(app.Tuning.Applied.Hash));Assert.That(app.Config.Source.GetInt("physics.backend"),Is.EqualTo(backend));
                var row=File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Row>).Last(r=>r.kind=="launch");
                Assert.That(row.effectiveHash,Is.EqualTo(app.Config.Hash));Assert.That(row.pending,Is.Zero);
            }
            yield return null;
        }
        [UnityTest] public IEnumerator ImmediateCameraAndSlowMotionApplyWithoutRelaunchAndInvalidateNextLog()
        {
            var app=AppBootstrap.Instance;app.enabled=false;app.Host.enabled=false;var generation=app.Host.SimulationGeneration;
            app.Tuning.Set("camera.fovDeg",70);Assert.That(UnityEngine.Camera.main.fieldOfView,Is.EqualTo(70));
            app.Tuning.Set("debug.slowMotion",.5);Assert.That(app.Clock.DebugSlowMotion,Is.EqualTo(.5));
            app.RecordDiagnosticsFrame(10);
            var row=File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Row>).Last();Assert.That(row.metricsValid,Is.False);
            app.Tuning.Set("debug.slowMotion",1);app.LaunchCurrent();Assert.That(app.Clock.MetricsInvalid,Is.True);Assert.That(app.Host.SimulationGeneration,Is.EqualTo(generation));
            yield return null;
        }
        [UnityTest] public IEnumerator RepeatUsesRealTimeSkipsPauseAndDoesNotApplyPending()
        {
            var app=AppBootstrap.Instance;app.enabled=false;app.Host.enabled=false;
            Assert.That(app.Tuning.Defaults.Get("launcher.repeatSeconds"),Is.Zero);Assert.That(app.Tuning.Defaults.Get("debug.slowMotion"),Is.EqualTo(1));
            int launches=0;app.Host.Launched+=_=>launches++;app.Tuning.Set("launcher.repeatSeconds",2);app.Tuning.Set("launcher.speed",22);
            app.TickRepeat(1);Assert.That(launches,Is.Zero);app.Clock.Paused=true;app.TickRepeat(20);Assert.That(launches,Is.Zero);
            app.Clock.Paused=false;app.TickRepeat(1);Assert.That(launches,Is.Zero);app.TickRepeat(1);Assert.That(launches,Is.EqualTo(1));
            Assert.That(app.Config.Source.Get("launcher.speed"),Is.Not.EqualTo(22));Assert.That(app.Tuning.PendingKeys,Does.Contain("launcher.speed"));
            app.TickRepeat(20);Assert.That(launches,Is.EqualTo(2),"No catch-up burst");app.Tuning.Set("launcher.repeatSeconds",0);app.TickRepeat(20);Assert.That(launches,Is.EqualTo(2));
            yield return null;
        }
    }
}
