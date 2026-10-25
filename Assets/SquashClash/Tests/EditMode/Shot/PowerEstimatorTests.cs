using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class PowerEstimatorTests
    {
        private static PowerSettings Settings(float coverage=30,int count=4,float exponent=1)=>new PowerSettings(80,coverage,count,34,100,1500,exponent);
        private static TouchSample S(double ms,float x,TouchPhaseKind phase=TouchPhaseKind.Moved)=>new TouchSample(1,phase,new Vector2(x,0),1+ms/1000,(long)(ms*100));
        private static TouchSample[] Line(float speed,double duration=80,int n=9)=>Enumerable.Range(0,n).Select(i=>S(duration*i/(n-1),(float)(speed*duration*i/(n-1)/1000))).ToArray();
        private static PowerResult Estimate(TouchSample[] s,PowerSettings? settings=null,double? release=null)=>PowerEstimator.Estimate(s,release??s.Last().TimeReal,1,settings??Settings());
        [Test] public void NetDisplacement_OverWindow_GivesSpeed()
        {var r=Estimate(Line(1000));Assert.That(r.Conservative,Is.False);Assert.That(r.NetSpeedMmPerS,Is.EqualTo(1000).Within(.01));Assert.That(r.P,Is.EqualTo(900f/1400).Within(1e-5));}
        [Test] public void ReverseInWindow_GivesLowPower()
        {var s=new[]{S(0,0),S(20,10),S(40,20),S(60,10),S(80,0)};var r=Estimate(s);Assert.That(r.Conservative,Is.False);Assert.That(r.P,Is.Zero);}
        [Test] public void BelowDeadZone_IsZeroPower_AboveFull_IsOne()
        {Assert.That(Estimate(Line(50)).P,Is.Zero);Assert.That(Estimate(Line(2000)).P,Is.EqualTo(1));}
        [Test] public void CurveExponent_IsApplied()=>Assert.That(Estimate(Line(800),Settings(exponent:2)).P,Is.EqualTo(.25f).Within(1e-5));
        [Test] public void ShortGesture_WithEnoughSamples_UsesAvailableDuration()
        {var r=Estimate(Line(1500,40,5));Assert.That(r.Conservative,Is.False);Assert.That(r.CoveredMs,Is.EqualTo(40).Within(1e-6));Assert.That(r.P,Is.EqualTo(1).Within(1e-5));}
        [Test] public void ShortGesture_TooFewSamples_IsConservativeMinimum()
        {var r=Estimate(Line(2000,40,3));Assert.That(r.Conservative,Is.True);Assert.That(r.P,Is.Zero);Assert.That(r.Reason,Is.Not.Empty);}
        [Test] public void RepeatedOrNonMonotonicTimestamps_AreConservative()
        {foreach(var times in new[]{new[]{0d,20,20,60,80},new[]{0d,40,20,60,80}}){var r=Estimate(times.Select((t,i)=>S(t,i*10)).ToArray());Assert.That(r.Conservative,Is.True);Assert.That(r.P,Is.Zero);}}
        [Test] public void WindowEdges_InterpolateOnlyBetweenRealSamples()
        {var r=Estimate(Line(1000,100,11),release:1.095);Assert.That(r.CoveredMs,Is.EqualTo(80).Within(1e-6));Assert.That(r.NetSpeedMmPerS,Is.EqualTo(1000).Within(.01));Assert.That(Estimate(Line(1000),release:1.09).Conservative,Is.True,"No extrapolated release endpoint.");}
        [Test] public void DuplicateStationarySamples_DoNotCountAsDistinct()
        {var s=new[]{S(0,0),S(20,0),S(20,0,TouchPhaseKind.Stationary),S(20,0,TouchPhaseKind.Stationary),S(40,0)};var r=Estimate(s);Assert.That(r.DistinctSamples,Is.EqualTo(3));Assert.That(r.Conservative,Is.True);}
        [Test] public void EnoughSamplesButGapAboveMax_IsConservative()
        {var r=Estimate(new[]{S(0,0),S(10,10),S(20,20),S(80,80)});Assert.That(r.Conservative,Is.True);Assert.That(r.MaxGapMs,Is.EqualTo(60).Within(1e-5));}
        [Test] public void CoverageExactlyMin_IsValid_JustBelow_IsConservative()
        {Assert.That(Estimate(Line(1000,30,4)).Conservative,Is.False);Assert.That(Estimate(Line(1000,29.99,4)).Conservative,Is.True);}
        [Test] public void SamplingRates_ProduceSamePowerFromSamePhysicalGesture()
        {foreach(int n in new[]{6,11,21})Assert.That(Estimate(Line(800,80,n)).P,Is.EqualTo(.5f).Within(1e-5));}
        [Test] public void AvailableWindow_StartsWhenDragWasRecognized()
        {var r=PowerEstimator.Estimate(Line(1500,80,9),1.08,1,Settings(),1.06);Assert.That(r.Conservative,Is.True);Assert.That(r.CoveredMs,Is.EqualTo(20).Within(1e-5));}
    }
}
