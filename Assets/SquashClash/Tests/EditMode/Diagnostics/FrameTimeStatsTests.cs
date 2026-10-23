using System;
using NUnit.Framework;
using SquashClash.Core.Diagnostics;
namespace SquashClash.Core.Tests
{
    public sealed class FrameTimeStatsTests
    {
        [Test] public void SessionPercentiles_NearestRankOverHistogram()
        {
            var s=new FrameTimeStats();for(int i=1;i<=100;i++)s.Add(i);
            Assert.That(s.SessionFrames,Is.EqualTo(100));
            Assert.That(s.SessionPercentile(50),Is.EqualTo(50).Within(.1));Assert.That(s.SessionPercentile(95),Is.EqualTo(95).Within(.1));Assert.That(s.SessionPercentile(99),Is.EqualTo(99).Within(.1));
        }
        [Test] public void Session_CoversMoreThanWindowUsingActualElapsedTime()
        {
            var s=new FrameTimeStats(3600);double elapsed=0;int longFrames=0;
            for(int i=0;i<54000;i++){double ms=i%10800==0?40:1000d/60;s.Add(ms);elapsed+=ms;if(ms>33)longFrames++;}
            Assert.That(s.SessionFrames,Is.EqualTo(54000));Assert.That(s.SessionMinutes,Is.EqualTo(elapsed/60000).Within(1e-9));
            Assert.That(s.SessionLongFramesPerMinute(33),Is.EqualTo(longFrames/(elapsed/60000)).Within(1e-9));
            Assert.That(s.WindowPercentile(100),Is.EqualTo(1000d/60));
        }
        [Test] public void WindowWrapsAndUsesExactNearestRank()
        {
            var s=new FrameTimeStats(3);foreach(var x in new[]{100d,1.123,2.234,3.345})s.Add(x);
            Assert.That(s.WindowPercentile(50),Is.EqualTo(2.234));Assert.That(s.WindowPercentile(95),Is.EqualTo(3.345));
            Assert.That(s.SessionPercentile(100),Is.EqualTo(100));
        }
        [Test] public void OverflowIsExplicitAndLongFrameThresholdIsStrict()
        {
            var s=new FrameTimeStats();s.Add(33);s.Add(33.0001);s.Add(250);s.Add(1000);
            Assert.That(s.SessionPercentile(100),Is.EqualTo(double.PositiveInfinity));
            Assert.That(s.SessionLongFramesPerMinute(33),Is.EqualTo(3/s.SessionMinutes));
        }
        [Test] public void EmptyAndResetDoNotInventMeasurements()
        {
            var s=new FrameTimeStats();Assert.That(double.IsNaN(s.SessionPercentile(95)),Is.True);Assert.That(double.IsNaN(s.WindowPercentile(95)),Is.True);
            s.Add(40);s.ResetSession();Assert.That(s.SessionFrames,Is.Zero);Assert.That(s.SessionMinutes,Is.Zero);Assert.That(double.IsNaN(s.WindowPercentile(95)),Is.True);
        }
        [Test] public void InvalidInputsDoNotMutateStatistics()
        {
            var s=new FrameTimeStats();foreach(var v in new[]{-1d,double.NaN,double.PositiveInfinity})Assert.Throws<ArgumentOutOfRangeException>(()=>s.Add(v));
            Assert.Throws<ArgumentOutOfRangeException>(()=>s.SessionPercentile(101));Assert.Throws<ArgumentOutOfRangeException>(()=>s.WindowPercentile(-1));
            Assert.Throws<ArgumentOutOfRangeException>(()=>new FrameTimeStats(0));Assert.That(s.SessionFrames,Is.Zero);
        }
    }
}
