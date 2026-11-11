using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.DevPanel;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    public sealed class LatencyCalibrationTests
    {
        private TuningState tuning;
        private List<LatencyCalibrationRecord> records;
        private LatencyCalibrationSession session;
        [SetUp] public void Setup()
        {
            var p=PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text);
            tuning=new TuningState(p,p);records=new List<LatencyCalibrationRecord>();
            session=new LatencyCalibrationSession(tuning,records.Add);
        }
        private void Series(double start,double offsetMs)
        {
            Assert.That(session.BeginSeries(start),Is.True);
            // Deliberately not frame time: these are exact synthetic emission/event timestamps.
            for(int i=0;i<8;i++)session.Stimulus(start+2+i);
            for(int i=0;i<8;i++)session.Tap(start+2+i+offsetMs/1000+(i%2==0?-.002:.002));
            Assert.That(session.EndSeries(start+10),Is.True);
        }
        private void Complete(double offset=30)
        {Assert.That(session.Begin(0),Is.True);Series(0,offset-10);Series(12,offset);Series(24,offset+10);}
        [Test] public void Calibration_ComputesJointOffsetWithUncertaintyAndRepeatability()
        {
            Complete();var r=session.Result;
            Assert.That(r.valid&&r.complete,Is.True);Assert.That(r.meanMs,Is.EqualTo(30).Within(1e-9));
            Assert.That(r.meansMs,Is.EqualTo(new[]{20d,30d,40d}).Within(1e-9));
            Assert.That(r.sdBySeriesMs.All(sd=>Math.Abs(sd-Math.Sqrt(32d/7))<1e-8),Is.True);
            Assert.That(r.seriesRangeMs,Is.EqualTo(20).Within(1e-8));
            Assert.That(r.ciLowMs,Is.EqualTo(30-4.302652729911275*10/Math.Sqrt(3)).Within(1e-8));
            Assert.That(r.ciHighMs,Is.EqualTo(30+4.302652729911275*10/Math.Sqrt(3)).Within(1e-8));
            Assert.That(r.gapsSeconds,Is.EqualTo(new[]{2d,2d}));
        }
        [Test] public void Calibration_ResultIsWrittenToLatencyCompOnlyOnConfirm()
        {
            var before=tuning.Applied;Complete();Assert.That(tuning.Applied.Hash,Is.EqualTo(before.Hash));
            Assert.That(records.Any(r=>r.confirmed),Is.False);Assert.That(session.Confirm(),Is.True);
            Assert.That(tuning.Applied.Get("timing.latencyCompMs"),Is.EqualTo(30).Within(1e-8));
            foreach(var e in before.Ordered.Where(e=>e.Key!="timing.latencyCompMs"))Assert.That(tuning.Applied.Get(e.Key),Is.EqualTo(e.Value),e.Key);
            Assert.That(session.Confirm(),Is.False);Assert.That(records.Count(r=>r.confirmed),Is.EqualTo(1));
        }
        [TestCase(.5,true)] [TestCase(1,false)]
        public void Calibration_InvalidWithSlowMotionOrFocusLoss(double slow,bool focus)
        {
            Assert.That(session.Begin(0),Is.True);Series(0,30);session.ObserveConditions(slow,focus);
            Assert.That(session.Result.valid,Is.False);Assert.That(session.Confirm(),Is.False);
            Assert.That(tuning.Applied.Get("timing.latencyCompMs"),Is.Zero);
            Assert.That(session.Begin(20,slow,focus),Is.False);
        }
        [Test] public void Calibration_RecordStatesJointOffsetNotDeviceLatency()
        {
            Complete();var r=records.Last();Assert.That(r.meaning,Is.EqualTo("joint-perceptual-offset"));
            Assert.That(r.limitation,Does.Contain("presentación").And.Contain("sonido").And.Contain("captura").And.Contain("respuesta humana"));
            Assert.That(r.limitation,Does.Contain("no mide la latencia del dispositivo"));
            Assert.That(records.Count(r=>r.kind=="series"),Is.EqualTo(3));
            Assert.That(records.Count(r=>r.kind=="tap"),Is.EqualTo(24));
        }
        [TestCase(150)] [TestCase(-150)]
        public void OutOfCatalogRangeIsReportedWithoutClampingOrWriting(double offset)
        {Complete(offset);Assert.That(session.Result.meanMs,Is.EqualTo(offset).Within(1e-8));Assert.That(session.Confirm(),Is.False);Assert.That(tuning.Applied.Get("timing.latencyCompMs"),Is.Zero);}
        [Test] public void MissingAndDuplicateTapsInvalidateInsteadOfDiscardingOutliers()
        {
            session.Begin(0);session.BeginSeries(0);for(int i=0;i<8;i++)session.Stimulus(2+i);
            for(int i=0;i<7;i++)session.Tap(2+i);session.Tap(2.01);
            Assert.That(session.EndSeries(10),Is.False);Assert.That(session.Result.valid,Is.False);Assert.That(session.Confirm(),Is.False);
            Assert.That(records.Count(r=>r.kind=="tap"),Is.EqualTo(8));
        }
        [Test] public void CancelOrFocusLossAfterCompletionCannotConfirm()
        {Complete();session.ObserveConditions(1,false);Assert.That(session.Confirm(),Is.False);Complete();session.Cancel();Assert.That(session.Confirm(),Is.False);}
        [Test] public void PairingUsesActualEmissionNotNominalBeatAndAcceptsNegativeOffset()
        {Complete(-30);Assert.That(session.Result.meanMs,Is.EqualTo(-30).Within(1e-8));Assert.That(session.Confirm(),Is.True);}
        [UnityTest] public IEnumerator CalibrationIsReachableOnlyThroughDevTimingSection()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            var root=AppBootstrap.Instance.GetComponent<UIDocument>().rootVisualElement;
            Assert.That(root.Q("first-shot-section").Q("latency-calibration"),Is.Not.Null);
            Assert.That(root.Q<Button>("calibration-start"),Is.Not.Null);Assert.That(root.Q<Button>("calibration-confirm"),Is.Not.Null);
            yield return PlayModeTestWorld.ClearApp();
        }
    }
}
