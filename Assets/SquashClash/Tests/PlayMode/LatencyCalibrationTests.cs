using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            foreach(double t in new[]{2d,2.01,3,4,5,6,7,8})session.Tap(t);
            Assert.That(session.EndSeries(10),Is.False);Assert.That(session.Result.valid,Is.False);Assert.That(session.Confirm(),Is.False);
            Assert.That(records.Count(r=>r.kind=="tap"),Is.EqualTo(8));
        }
        [Test] public void CancelOrFocusLossAfterCompletionCannotConfirm()
        {Complete();session.ObserveConditions(1,false);Assert.That(session.Confirm(),Is.False);Complete();session.Cancel();Assert.That(session.Confirm(),Is.False);}
        [Test] public void PairingUsesActualEmissionNotNominalBeatAndAcceptsNegativeOffset()
        {
            session.Begin(0);
            for(int series=0;series<3;series++)
            {
                double start=series*12;session.BeginSeries(start);
                for(int i=0;i<8;i++)session.Stimulus(start+2+i+.01*(i%3));
                for(int i=0;i<8;i++)session.Tap(start+2+i+.01*(i%3)-.03);
                Assert.That(session.EndSeries(start+10),Is.True);
            }
            Assert.That(session.Result.meanMs,Is.EqualTo(-30).Within(1e-8));Assert.That(session.Confirm(),Is.True);
        }
        [Test] public void ConfirmedResultIsNotCanceledByTeardown()
        {Complete();Assert.That(session.Confirm(),Is.True);session.Cancel();Assert.That(session.Result.complete&&session.Result.valid,Is.True);Assert.That(records.Last().confirmed,Is.True);}
        [UnityTest] public IEnumerator CalibrationModalBlocksGameplayAndCancelRestoresInputWithoutApplying()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            var app=AppBootstrap.Instance;var root=app.GetComponent<UIDocument>().rootVisualElement;
            app.GetComponent<ResumeOverlay>().Continue();var input=app.GetComponent<SquashClash.Runtime.Input.TouchInputAdapter>();
            UnityEngine.Object.FindFirstObjectByType<DevPanelView>().Toggle();
            root.Q<Foldout>("first-shot-section").value=true;root.Q<Foldout>("latency-calibration").value=true;yield return null;yield return null;
            bool blocked=input.BlockedByUi;string before=app.Tuning.Applied.Hash;
            root.Q<Button>("calibration-start").Focus();
            using(var e=NavigationSubmitEvent.GetPooled())root.Q<Button>("calibration-start").SendEvent(e);
            Assert.That(root.Q("calibration-overlay"),Is.Not.Null);Assert.That(input.BlockedByUi,Is.True);yield return null;yield return null;
            var cancel=root.Q("calibration-overlay").Query<Button>().ToList().Single(b=>b.text=="Cancelar sin aplicar");
            cancel.Focus();using(var e=NavigationSubmitEvent.GetPooled())cancel.SendEvent(e);
            Assert.That(root.Q("calibration-overlay"),Is.Null);Assert.That(input.BlockedByUi,Is.EqualTo(blocked));Assert.That(app.Tuning.Applied.Hash,Is.EqualTo(before));
            yield return PlayModeTestWorld.ClearApp();
        }
        [UnityTest] public IEnumerator CalibrationIsReachableOnlyThroughDevTimingSection()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            var root=AppBootstrap.Instance.GetComponent<UIDocument>().rootVisualElement;
            Assert.That(root.Q("first-shot-section").Q("latency-calibration"),Is.Not.Null);
            Assert.That(root.Q<Button>("calibration-start"),Is.Not.Null);Assert.That(root.Q<Button>("calibration-confirm"),Is.Not.Null);
            yield return PlayModeTestWorld.ClearApp();
        }
        [Serializable] private sealed class MetricsRow
        {public string kind,payload;public bool metricsValid;}
        [Serializable] private sealed class DevControlRow
        {public string control,stage,reason;public bool accepted;}
        private static IEnumerator OpenCalibrationModal()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            var app=AppBootstrap.Instance;var root=app.GetComponent<UIDocument>().rootVisualElement;
            app.GetComponent<ResumeOverlay>().Continue();var view=UnityEngine.Object.FindFirstObjectByType<DevPanelView>();
            if(!view.IsOpen)view.Toggle();
            root.Q<Foldout>("first-shot-section").value=true;root.Q<Foldout>("latency-calibration").value=true;
            yield return null;yield return null;
            var start=root.Q<Button>("calibration-start");start.Focus();
            using(var e=NavigationSubmitEvent.GetPooled())start.SendEvent(e);
            yield return null;yield return null;
            Assert.That(root.Q("calibration-overlay"),Is.Not.Null,"The test must start a real DEV calibration modal");
        }
        [UnityTest] public IEnumerator CalibrationModalRejectsDevToggleAndPreservesBlockedInput()
        {
            yield return OpenCalibrationModal();
            var app=AppBootstrap.Instance;var view=UnityEngine.Object.FindFirstObjectByType<DevPanelView>();
            var hold=UnityEngine.Object.FindFirstObjectByType<ThreeFingerHold>();
            bool before=view.IsOpen;view.Toggle();
            Assert.That(view.IsOpen,Is.EqualTo(before),"DEV must not change while calibration owns input");
            Assert.That(app.GetComponent<SquashClash.Runtime.Input.TouchInputAdapter>().BlockedByUi,Is.True);
            Assert.That(hold.CanToggle(),Is.False,"The three-finger path must use the same guard");
            var row=File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<MetricsRow>)
                .Where(r=>r.kind=="dev-control").Select(r=>JsonUtility.FromJson<DevControlRow>(r.payload)).Last();
            Assert.That(row.control,Is.EqualTo("dev-toggle"));Assert.That(row.stage,Is.EqualTo("result"));
            Assert.That(row.accepted,Is.False);Assert.That(row.reason,Is.EqualTo("dev-unavailable"));
            yield return PlayModeTestWorld.ClearApp();
        }
        [UnityTest] public IEnumerator CalibrationCancelMatchesOpenDev()=>CancelAgainstActualDev(true);
        [UnityTest] public IEnumerator CalibrationCancelMatchesClosedDev()=>CancelAgainstActualDev(false);
        private static IEnumerator CancelAgainstActualDev(bool open)
        {
            yield return OpenCalibrationModal();
            var app=AppBootstrap.Instance;var root=app.GetComponent<UIDocument>().rootVisualElement;
            var view=UnityEngine.Object.FindFirstObjectByType<DevPanelView>();
            // Exercise teardown against the actual UI state independently of the state captured at start.
            root.Q("dev-panel").style.display=open?DisplayStyle.Flex:DisplayStyle.None;
            var cancel=root.Q("calibration-overlay").Query<Button>().ToList().Single(b=>b.text=="Cancelar sin aplicar");
            cancel.Focus();using(var e=NavigationSubmitEvent.GetPooled())cancel.SendEvent(e);
            Assert.That(root.Q("calibration-overlay"),Is.Null);
            Assert.That(view.IsOpen,Is.EqualTo(open));
            Assert.That(app.GetComponent<SquashClash.Runtime.Input.TouchInputAdapter>().BlockedByUi,Is.EqualTo(view.IsOpen));
            yield return PlayModeTestWorld.ClearApp();
        }
        [UnityTest] public IEnumerator CalibrationMetricsEnvelopeTracksClockIndependentlyOfPayload()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            var app=AppBootstrap.Instance;var calibration=app.GetComponent<LatencyCalibration>();
            var write=typeof(LatencyCalibration).GetMethod("Write",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Assert.That(write,Is.Not.Null);Assert.That(app.Clock.MetricsInvalid,Is.False);
            foreach(bool invalidClock in new[]{false,true})
            {
                if(invalidClock)app.Clock.DebugSlowMotion=.5;
                foreach(bool payloadValid in new[]{false,true})
                {
                    var payload=new LatencyCalibrationRecord{kind=payloadValid?"stimulus":"audio-context",valid=payloadValid,reason="synthetic-envelope-check"};
                    write.Invoke(calibration,new object[]{payload});
                    var row=File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<MetricsRow>).Last(r=>r.kind=="latency-calibration");
                    Assert.That(row.metricsValid,Is.EqualTo(!app.Clock.MetricsInvalid),"Envelope validity is the clock's state, not calibration validity");
                    var saved=JsonUtility.FromJson<LatencyCalibrationRecord>(row.payload);
                    Assert.That(saved.valid,Is.EqualTo(payloadValid));Assert.That(saved.kind,Is.EqualTo(payload.kind));Assert.That(saved.reason,Is.EqualTo(payload.reason));
                }
            }
            yield return PlayModeTestWorld.ClearApp();
        }
    }
}
