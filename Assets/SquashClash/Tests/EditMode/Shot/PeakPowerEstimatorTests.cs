using System;
using NUnit.Framework;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class PeakPowerEstimatorTests
    {
        private static PeakPowerEstimator Estimator()=>new PeakPowerEstimator(new PowerSettings(80,30,3,51,0,400,1),20,120);
        private static TouchSample S(double t,float x,TouchPhaseKind phase=TouchPhaseKind.Moved)=>new TouchSample(1,phase,new Vector2(x,0),t,(long)(t*100000));
        [Test] public void FlickStoppedFor50ms_PreservesHighPower()
        {
            var e=Estimator();e.Push(S(0,0,TouchPhaseKind.Began),1);e.Push(S(.02,16),1);e.Push(S(.04,32),1);
            var r=e.Push(S(.09,32,TouchPhaseKind.Ended),1);
            Assert.That(r.Conservative,Is.False);Assert.That(r.P,Is.GreaterThanOrEqualTo(.6f));
        }
        [Test] public void SustainedSlowDrag_NeverBecomesStrong()
        {
            var e=Estimator();PowerResult r=default;for(int i=0;i<=50;i++)r=e.Push(S(i*.02,i*.4f),1);
            Assert.That(r.Conservative,Is.False);Assert.That(r.P,Is.GreaterThan(0).And.LessThan(.25f));
        }
        [Test] public void FastAim400mmPerS_Then150msSoftRelease_IsNotStrong()
        {
            var e=Estimator();for(int i=0;i<=10;i++)e.Push(S(i*.02,i*8),1);
            var r=e.Push(S(.35,80,TouchPhaseKind.Ended),1);
            Assert.That(r.Conservative,Is.False);Assert.That(r.P,Is.LessThan(.6f));
        }
        [Test] public void GapCannotInventSupport_ThreeReliableSamplesAreRequired()
        {
            var e=Estimator();e.Push(S(0,0),1);e.Push(S(.08,100),1);var r=e.Push(S(.16,200),1);
            Assert.That(r.Conservative,Is.True);Assert.That(r.P,Is.Zero);Assert.That(r.Reason,Is.EqualTo("no-supported-run"));
        }
        [Test] public void DecayUsesRealTime_IndependentOfStationaryEventDensity()
        {
            var sparse=Estimator();var dense=Estimator();foreach(var e in new[]{sparse,dense}){e.Push(S(0,0),1);e.Push(S(.02,16),1);e.Push(S(.04,32),1);}
            var a=sparse.Push(S(.19,32,TouchPhaseKind.Ended),1);PowerResult b=default;
            for(int i=1;i<=15;i++)b=dense.Push(S(.04+i*.01,32),1);
            Assert.That(a.Conservative,Is.False);Assert.That(a.P,Is.EqualTo(b.P).Within(1e-6));Assert.That(a.P,Is.LessThan(.6f));
        }
        [Test] public void TimestampError_RemainsConservative()
        {
            var e=Estimator();e.Push(S(0,0),1);e.Push(S(.02,16),1);e.Push(S(.01,20),1);
            var r=e.Push(S(.04,32),1);Assert.That(r.Conservative,Is.True);Assert.That(r.Reason,Is.EqualTo("out-of-order"));
        }
        [Test] public void NewBegan_DoesNotInheritPeakOrSupport()
        {
            var e=Estimator();e.Push(S(0,0,TouchPhaseKind.Began),1);e.Push(S(.02,16),1);e.Push(S(.04,32),1);
            var r=e.Push(S(1,0,TouchPhaseKind.Began),1);Assert.That(r.P,Is.Zero);Assert.That(r.Conservative,Is.True);Assert.That(r.Reason,Is.EqualTo("no-supported-run"));
        }
        [Test] public void NonFiniteInput_IsConservative_StationaryDuplicateDoesNotAddSupport()
        {
            var e=Estimator();e.Push(S(0,0),1);e.Push(S(.02,16),1);
            var r=e.Push(S(.02,16,TouchPhaseKind.Stationary),1);Assert.That(r.Conservative,Is.True);
            r=e.Push(S(double.NaN,32),1);Assert.That(r.Conservative,Is.True);Assert.That(r.P,Is.Zero);Assert.That(r.Reason,Is.EqualTo("invalid-input"));
        }
    }
}
