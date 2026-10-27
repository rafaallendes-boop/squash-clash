using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Shot;
using SquashClash.Core.Court;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class SlingTimerReplayTests
    {
        [Serializable] private class Root {public Record[] records;}
        [Serializable] private class Record {public string kind;public Payload payload;}
        [Serializable] private class Payload {public float mmPerPx;public float[] areaPx;public double lockTimeReal;public Sample[] rawSamples;}
        [Serializable] private class Sample {public double timeReal;public float xPx,yPx;public int id;public long sequence;public string phase;}
        [Test] public void B0Counterfactual_ProductionModelMatchesPredeclaredSyntheticGrid_DiagnosticOnly()
        {
            string path=Path.Combine(Application.dataPath,"../docs/validation/evidence/2026-10-27-s1-sling-B0.json");
            var records=JsonUtility.FromJson<Root>(File.ReadAllText(path)).records.Where(r=>r.kind=="s1-sling-gesture").ToArray();
            Assert.That(records.Length,Is.EqualTo(33));int fixedCount=0;var newlyFixed=new List<int>();
            for(int i=0;i<records.Length;i++)
            {
                var p=records[i].payload;var a=p.areaPx;var m=new SlingModel(SlingSettings.From(ConfigFixtures.Default()),CourtDimensions.Standard(),3.2f,Rect.MinMaxRect(a[0],a[1],a[2],a[3]),p.mmPerPx);
                double began=p.rawSamples[0].timeReal;int frame=0;
                foreach(var s in p.rawSamples)
                {
                    while(began+frame/60d<s.timeReal-1e-9){m.Tick(began+frame/60d);frame++;}
                    m.Push(new TouchSample(s.id,(TouchPhaseKind)Enum.Parse(typeof(TouchPhaseKind),s.phase),new Vector2(s.xPx,s.yPx),s.timeReal,s.sequence));
                }
                if(m.Locked){fixedCount++;if(p.lockTimeReal==0)newlyFixed.Add(i+1);}
            }
            Assert.That(fixedCount,Is.EqualTo(23));Assert.That(newlyFixed,Is.EqualTo(new[]{8,11,16,22,25,26}));
            TestContext.WriteLine("OFFLINE SYNTHETIC 60Hz: 23 fixations, six new. Not a new phone measurement or intention judgment.");
        }
    }
}
