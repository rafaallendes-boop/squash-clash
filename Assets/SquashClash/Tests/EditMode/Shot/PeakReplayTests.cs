using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class PeakReplayTests
    {
        [Serializable] private sealed class Root {public Measurement measurements;public Selected selectedB;}
        [Serializable] private sealed class Measurement {public Gesture[] gestures;}
        [Serializable] private sealed class Gesture {public string ts,protocol;public Record originalRecord;}
        [Serializable] private sealed class Record {public Payload payload;}
        [Serializable] private sealed class Payload {public float mmPerPx;public Sample[] rawSamples;}
        [Serializable] private sealed class Sample {public string phase;public double timeReal;public float xPx,yPx;public long sequence;public int id;}
        [Serializable] private sealed class Selected {public Row[] rows;}
        [Serializable] private sealed class Row {public string timestamp,reason;public float power;}
        [Test] public void SelectedB_ReplaysAll40RecordedGesturesWithinFloatTolerance()
        {
            string root=Path.GetDirectoryName(Application.dataPath);
            var source=JsonUtility.FromJson<Root>(File.ReadAllText(Path.Combine(root,"docs/validation/evidence/2026-10-25-s1-revalidation-2.json")));
            var expected=JsonUtility.FromJson<Root>(File.ReadAllText(Path.Combine(root,"docs/validation/evidence/2026-10-25-s1-gesture-calibration-offline.json"))).selectedB.rows;
            int checkedRows=0;
            foreach(var gesture in source.measurements.gestures.Where(g=>g.protocol=="soft"||g.protocol=="strong"))
            {
                var estimator=new PeakPowerEstimator(new PowerSettings(80,30,3,51,0,400,1),20,120);PowerResult actual=default;
                foreach(var x in gesture.originalRecord.payload.rawSamples)
                    actual=estimator.Push(new TouchSample(x.id,(TouchPhaseKind)Enum.Parse(typeof(TouchPhaseKind),x.phase),new Vector2(x.xPx,x.yPx),x.timeReal,x.sequence),gesture.originalRecord.payload.mmPerPx);
                var row=expected.Single(r=>r.timestamp==gesture.ts);
                Assert.That(actual.P,Is.EqualTo(row.power).Within(1e-5),gesture.ts);Assert.That(actual.Reason,Is.EqualTo(row.reason),gesture.ts);checkedRows++;
            }
            Assert.That(checkedRows,Is.EqualTo(40));
        }
    }
}
