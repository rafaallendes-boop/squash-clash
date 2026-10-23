using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests
{
    public sealed class RuntimeDiagnosticsTests
    {
        private GameObject root;
        [UnitySetUp] public IEnumerator Setup(){yield return PlayModeTestWorld.ClearApp();root=new GameObject("Diagnostics tests");}
        [UnityTearDown] public IEnumerator Cleanup(){Object.Destroy(root);yield return null;}
        [Test] public void Thermal_TracksSecondsAndWorstThenResets()
        {
            var t=root.AddComponent<ThermalMonitor>();t.enabled=false;
            t.Observe("nominal",2);t.Observe("serious",3);t.Observe("fair",1);
            Assert.That(t.Current,Is.EqualTo("fair"));Assert.That(t.WorstThisSession,Is.EqualTo("serious"));
            Assert.That(t.SecondsByState["nominal"],Is.EqualTo(2));Assert.That(t.SecondsByState["serious"],Is.EqualTo(3));
            Assert.Throws<System.NotSupportedException>(()=>((IDictionary<string,double>)t.SecondsByState).Add("invalid",1));
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>t.Observe("fair",-1));
            t.ResetSession();Assert.That(t.WorstThisSession,Is.EqualTo("unknown"));Assert.That(t.SecondsByState,Is.Empty);
        }
        [Test] public void FrameTiming_MissingDataIsExplicitAndInvalidDurationsAreNotMeasured()
        {
            var s=root.AddComponent<FrameTimingSampler>();s.enabled=false;
            Assert.That(double.IsNaN(s.CpuMs),Is.True);Assert.That(double.IsNaN(s.GpuMs),Is.True);
            Assert.That(s.AcceptTiming(new FrameTiming{frameStartTimestamp=1,cpuFrameTime=double.NaN,gpuFrameTime=0}),Is.False);
            Assert.That(s.SampleSerial,Is.Zero);Assert.That(double.IsNaN(s.GpuMs),Is.True);
        }
        [Test] public void FrameTiming_AcceptsEachTimestampOnceAndKeepsCpuGpuDistinct()
        {
            var s=root.AddComponent<FrameTimingSampler>();s.enabled=false;var timing=new FrameTiming{frameStartTimestamp=10,cpuFrameTime=12,gpuFrameTime=4};
            Assert.That(s.AcceptTiming(timing),Is.True);Assert.That(s.CpuMs,Is.EqualTo(12));Assert.That(s.GpuMs,Is.EqualTo(4));Assert.That(s.SampleSerial,Is.EqualTo(1));
            Assert.That(s.AcceptTiming(timing),Is.False);Assert.That(s.SampleSerial,Is.EqualTo(1));
            timing.frameStartTimestamp=11;timing.gpuFrameTime=0;Assert.That(s.AcceptTiming(timing),Is.True);Assert.That(double.IsNaN(s.GpuMs),Is.True);Assert.That(s.SampleSerial,Is.EqualTo(2));
        }
    }
}
