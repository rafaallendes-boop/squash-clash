using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.DevPanel;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    public sealed class PerformanceSessionTests
    {
        [Serializable] private sealed class Row { public string kind,phase,payload; public bool metricsValid; }
        [Serializable] private sealed class Summary
        {
            public bool complete; public string reason,simulationKey,thermalStart,thermalWorst;
            public double seconds,p95Ms,p99Ms,cpuP95Ms,gpuP95Ms,nominalSeconds,seriousSeconds;
            public long frames,cpuSamples,gpuSamples,droppedSteps; public int casesVisited;
        }
        private AppBootstrap app;
        private string selection,backup;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();
            selection=Path.Combine(Application.persistentDataPath,"presets","last-active.txt");
            backup=File.Exists(selection)?File.ReadAllText(selection):null;File.Delete(selection);
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.enabled=false;app.Host.enabled=false;app.GetComponent<ResumeOverlay>().Continue();
            app.GetComponent<ThermalMonitor>().enabled=false;app.GetComponent<ThermalMonitor>().Observe("nominal",0);
            app.GetComponent<FrameTimingSampler>().enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return PlayModeTestWorld.ClearApp();
            if(backup==null)File.Delete(selection);else File.WriteAllText(selection,backup);
        }
        private Component Session()
        {
            var type=typeof(DevBar).Assembly.GetType("SquashClash.DevPanel.PerformanceSession");
            Assert.That(type,Is.Not.Null,"H17 needs a bounded native session, independent of spike/benchmark");
            var component=UnityEngine.Object.FindFirstObjectByType(type) as Component;
            Assert.That(component,Is.Not.Null);((Behaviour)component).enabled=false;return component;
        }
        private static object Call(Component session,string method,params object[] args)=>session.GetType().GetMethod(method).Invoke(session,args);
        private static bool Running(Component session)=>(bool)session.GetType().GetProperty("IsRunning").GetValue(session);
        private Row[] Rows()=>File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Row>).ToArray();
        [UnityTest] public IEnumerator StartRequiresRepetitionAndCleanNormalConfiguration_ThenClosesPanelAndSelectsL1()
        {
            var s=Session();Assert.That(Call(s,"Begin"),Is.EqualTo(false),"Default repeat=0 must not silently choose a new default");
            app.Tuning.Set("launcher.repeatSeconds",3);app.Tuning.Set("physics.gravity",12);
            Assert.That(Call(s,"Begin"),Is.EqualTo(false),"Pending edits must be applied or discarded first");
            app.Tuning.Set("physics.gravity",app.Tuning.Applied.Get("physics.gravity"));app.Tuning.Set("debug.slowMotion",.5);
            Assert.That(Call(s,"Begin"),Is.EqualTo(false));app.Tuning.Set("debug.slowMotion",1);
            // MetricsInvalid is sticky: restarting the flight must not clear evidence of slow motion.
            Assert.That(Call(s,"Begin"),Is.EqualTo(false));
            yield return null;
        }
        [UnityTest] public IEnumerator FullSessionKeepsAllSamplesAcrossSixLaunchesAndRestoresPhase()
        {
            var s=Session();app.Tuning.Set("launcher.repeatSeconds",3);app.SetMetricsPhase("free",17);
            var panel=UnityEngine.Object.FindFirstObjectByType<DevPanelView>();panel.Toggle();
            string key=app.Config.SimulationKey;Assert.That(Call(s,"Begin"),Is.EqualTo(true));
            Assert.That(app.Metrics.Phase,Is.EqualTo("session"));Assert.That(panel.IsOpen,Is.False);
            panel.Toggle();Assert.That(panel.IsOpen,Is.False,"Panel stays closed for H17");
            var selector=app.GetComponent<UIDocument>().rootVisualElement.Q<DropdownField>("legibility-launch");
            Assert.That(selector.value,Does.StartWith("L1 "));
            var timing=app.GetComponent<FrameTimingSampler>();
            for(int i=1;i<=90001&&Running(s);i++)
            {
                timing.AcceptTiming(new FrameTiming{frameStartTimestamp=(ulong)i,cpuFrameTime=4,gpuFrameTime=2});
                Call(s,"Tick",.01);
            }
            Assert.That(Running(s),Is.False);Assert.That(app.Metrics.Phase,Is.EqualTo("free"));Assert.That(app.MetricsSeed,Is.EqualTo(17));
            var row=Rows().Single(r=>r.kind=="session-summary");var f=JsonUtility.FromJson<Summary>(row.payload);
            Assert.That(row.phase,Is.EqualTo("session"));Assert.That(row.metricsValid,Is.True);Assert.That(f.complete,Is.True);
            Assert.That(f.seconds,Is.InRange(900,900.011));Assert.That(f.frames,Is.InRange(90000L,90001L));
            Assert.That(f.p95Ms,Is.EqualTo(10));Assert.That(f.p99Ms,Is.EqualTo(10));
            Assert.That(f.cpuSamples,Is.EqualTo(f.frames));Assert.That(f.cpuP95Ms,Is.EqualTo(4));Assert.That(f.gpuP95Ms,Is.EqualTo(2));
            Assert.That(f.casesVisited,Is.EqualTo(6));Assert.That(selector.value,Does.StartWith("L6 "));
            Assert.That(f.simulationKey,Is.EqualTo(key));Assert.That(f.droppedSteps,Is.Zero);
            Assert.That(f.thermalStart,Is.EqualTo("nominal"));Assert.That(f.thermalWorst,Is.EqualTo("nominal"));Assert.That(f.nominalSeconds,Is.EqualTo(f.seconds));
            Assert.That(File.Exists(selection),Is.False,"H17 never saves or promotes a preset");yield return null;
        }
        [UnityTest] public IEnumerator InterruptedSessionIsPersistedIncomplete_AndMissingTimingIsExplicit()
        {
            var s=Session();app.Tuning.Set("launcher.repeatSeconds",3);Assert.That(Call(s,"Begin"),Is.EqualTo(true));
            Call(s,"Tick",.016);app.GetComponent<ThermalMonitor>().Observe("serious",0);Call(s,"Tick",.016);
            s.SendMessage("OnApplicationFocus",false);Assert.That(Running(s),Is.False);
            var row=Rows().Single(r=>r.kind=="session-summary");var f=JsonUtility.FromJson<Summary>(row.payload);
            Assert.That(row.metricsValid,Is.False);Assert.That(f.complete,Is.False);Assert.That(f.reason,Does.Contain("foco"));
            Assert.That(f.thermalWorst,Is.EqualTo("serious"));Assert.That(f.seriousSeconds,Is.EqualTo(.016));
            Assert.That(f.cpuSamples,Is.Zero);Assert.That(f.gpuSamples,Is.Zero);Assert.That(row.payload,Does.Contain("\"cpuP95Ms\":null"));
            Assert.That(app.Metrics.Phase,Is.EqualTo("free"));yield return null;
        }
        [UnityTest] public IEnumerator RestartWhileL1SelectedRelaunchesAndShowsSessionProgressOutsideClosedPanel()
        {
            var s=Session();app.Tuning.Set("launcher.repeatSeconds",3);Assert.That(Call(s,"Begin"),Is.EqualTo(true));
            Call(s,"Tick",.016);Call(s,"Cancel");
            int launches=0;app.Host.Launched+=_=>launches++;
            Assert.That(Call(s,"Begin"),Is.EqualTo(true));
            Assert.That(launches,Is.EqualTo(1),"Starting with L1 already selected must still restart L1");
            var status=app.GetComponent<UIDocument>().rootVisualElement.Q<Label>("performance-session-status");
            Assert.That(status,Is.Not.Null);Assert.That(status.text,Does.Contain("L1"));
            Call(s,"Tick",150d);Assert.That(status.text,Does.Contain("L2"));
            app.TickRepeat(3);Assert.That(app.Metrics.Phase,Is.EqualTo("session"));
            Call(s,"Cancel");Assert.That(status.text,Does.Contain("incompleta"));yield return null;
        }
    }
}
