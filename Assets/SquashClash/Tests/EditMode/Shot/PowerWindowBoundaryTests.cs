using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Shot;
using SquashClash.Core.Tests.Config;
using UnityEngine;

namespace SquashClash.Core.Tests.Shot
{
    public sealed class PowerWindowBoundaryTests
    {
        private static readonly PowerSettings Settings=new PowerSettings(80,30,4,34,100,1500,1);
        private static TouchSample S(double ms,float x)=>new TouchSample(1,TouchPhaseKind.Moved,new Vector2(x,0),1+ms/1000,(long)(ms*100));
        [Test] public void GapEndingExactlyAtRecognizedDragStart_IsOutsidePowerWindow()
        {
            var samples=new[]{S(0,0),S(100,10),S(116,26),S(132,42),S(148,58),S(154,64)};
            var r=PowerEstimator.Estimate(samples,1.154,1,Settings,1.1);
            Assert.That(r.Conservative,Is.False);
            Assert.That(r.MaxGapMs,Is.EqualTo(16).Within(1e-6));
            Assert.That(r.NetSpeedMmPerS,Is.EqualTo(1000).Within(.01));
        }
        [Test] public void GapStraddlingWindowStart_StillGuardsInterpolation()
        {
            var samples=new[]{S(0,0),S(60,60),S(70,70),S(80,80),S(90,90)};
            var r=PowerEstimator.Estimate(samples,1.09,1,Settings);
            Assert.That(r.Conservative,Is.True);
            Assert.That(r.Reason,Is.EqualTo("sample-gap"));
            Assert.That(r.MaxGapMs,Is.EqualTo(60).Within(1e-6));
        }
        [Test] public void GapStartingAtRelease_OutsideWindowCannotInvalidatePastPower()
        {
            var samples=new[]{S(0,0),S(20,20),S(40,40),S(60,60),S(80,80),S(180,180)};
            var r=PowerEstimator.Estimate(samples,1.08,1,Settings);
            Assert.That(r.Conservative,Is.False);
            Assert.That(r.MaxGapMs,Is.EqualTo(20).Within(1e-6));
        }

        [Serializable] private sealed class Evidence {public Record[] records;}
        [Serializable] private sealed class Record {public string ts;public Payload payload;}
        [Serializable] private sealed class Payload
        {
            public string protocol,reason,outcome;public int trial;public float mmPerPx,power;
            public bool conservative;public Sample[] rawSamples;
        }
        [Serializable] private sealed class Sample
        {
            public long sequence;public int id;public string phase;public double timeReal;public float xPx,yPx;
        }
        [TestCase("2026-10-25T14:10:46.4342030Z",5)]
        [TestCase("2026-10-25T14:10:49.0012170Z",7)]
        [TestCase("2026-10-25T14:10:50.9853840Z",9)]
        public void NativeStrongTrace_WithGapBeforeRecognizedDrag_RemainsMeasurable(string ts,int trial)
        {
            var path=Path.Combine(Application.dataPath,"../docs/validation/evidence/2026-10-25-s1-touch-sampling.json");
            var evidence=JsonUtility.FromJson<Evidence>(File.ReadAllText(path));
            var p=evidence.records.Single(r=>r.ts==ts).payload;
            Assert.That(p.protocol,Is.EqualTo("strong"));Assert.That(p.trial,Is.EqualTo(trial));
            Assert.That(p.conservative,Is.True,"Preserve the original native observation, including its bug.");
            Assert.That(p.reason,Is.EqualTo("sample-gap"));Assert.That(p.power,Is.Zero);
            var samples=p.rawSamples.Select(s=>new TouchSample(s.id,(TouchPhaseKind)Enum.Parse(typeof(TouchPhaseKind),s.phase),new Vector2(s.xPx,s.yPx),s.timeReal,s.sequence)).ToArray();
            var config=ConfigFixtures.Default();
            var tracker=new GestureTracker((float)config.Get("gesture.tapMaxMm"),(float)config.Get("gesture.tapMaxMs"),(float)config.Get("gesture.cancelRadiusMm"),p.mmPerPx);
            foreach(var sample in samples)tracker.Push(sample);
            Assert.That(tracker.Outcome,Is.EqualTo(GestureOutcome.Swing));
            var result=PowerEstimator.Estimate(samples,samples.Last().TimeReal,p.mmPerPx,PowerSettings.From(config),tracker.DragStartedAtReal);
            Assert.That(result.Conservative,Is.False);
            Assert.That(result.MaxGapMs,Is.LessThanOrEqualTo(config.Get("power.maxGapMs")));
            Assert.That(result.P,Is.GreaterThan(0));
            Debug.Log(FormattableString.Invariant($"[S1 boundary replay] trial={trial} P={result.P:R} speed={result.NetSpeedMmPerS:R} coveredMs={result.CoveredMs:R} samples={result.DistinctSamples} maxGapMs={result.MaxGapMs:R}"));
        }
    }
}
