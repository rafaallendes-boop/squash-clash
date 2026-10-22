using System;
using NUnit.Framework;
using SquashClash.Core.Simulation;
namespace SquashClash.Core.Tests.Simulation {
    public sealed class SimClockTests {
        [Test] public void Advance_Tempo1_SixtiethSecond_TwoSteps() => Assert.That(new SimClock(120,1).Advance(1.0/60),Is.EqualTo(2));
        [TestCase(60,78)] [TestCase(36000,46800)]
        public void Advance_Tempo065_AccumulatesWithoutDrift(int frames,int expected) {
            var clock=new SimClock(120,0.65); int total=0;
            for(int i=0;i<frames;i++) { int n=clock.Advance(1.0/60); Assert.That(n,Is.InRange(0,8)); total+=n; }
            Assert.That(total,Is.EqualTo(expected)); Assert.That(clock.DroppedSteps,Is.Zero);
            Assert.That(clock.Alpha,Is.InRange(0f,0.99999999f));
        }
        [Test] public void Advance_AccumulatesFractions() {
            var c=new SimClock(120,1);
            Assert.That(c.Advance(0.004),Is.Zero); Assert.That(c.Advance(0.004),Is.Zero);
            Assert.That(c.Advance(0.004),Is.EqualTo(1)); Assert.That(c.Alpha,Is.EqualTo(0.44f).Within(1e-6));
        }
        [Test] public void Advance_HugeDelta_CapsAndCountsDropped() {
            var c=new SimClock(120,1);
            Assert.That(c.Advance(5),Is.EqualTo(8)); Assert.That(c.DroppedSteps,Is.EqualTo(592));
            Assert.That(c.DroppedSinceRestart,Is.EqualTo(592)); Assert.That(c.Advance(0),Is.Zero);
        }
        [Test] public void Advance_WhilePaused_DiscardsTime() {
            var c=new SimClock(120,1) { Paused=true };
            Assert.That(c.Advance(30),Is.Zero); Assert.That(c.DroppedSteps,Is.Zero);
            c.Paused=false; Assert.That(c.Advance(1.0/60),Is.EqualTo(2));
        }
        [Test] public void Paused_SingleStep_RunsExactlyOne() {
            var c=new SimClock(120,1) { Paused=true }; c.RequestSingleStep();
            Assert.That(c.Advance(30),Is.EqualTo(1)); Assert.That(c.Advance(30),Is.Zero);
            Assert.That(c.DroppedSteps,Is.Zero); Assert.That(c.Paused,Is.True);
        }
        [Test] public void Configure_ChangesHz_AndRestartClearsFraction() {
            var c=new SimClock(120,1); c.Advance(5); c.Advance(0.004); c.Configure(240,0.8);
            Assert.That(c.StepHz,Is.EqualTo(240)); Assert.That(c.Tempo,Is.EqualTo(0.8)); Assert.That(c.Alpha,Is.Zero);
            Assert.That(c.Advance(1.0/60),Is.EqualTo(3)); c.Restart();
            Assert.That(c.Alpha,Is.Zero); Assert.That(c.DroppedSinceRestart,Is.Zero); Assert.That(c.DroppedSteps,Is.EqualTo(592));
        }
        [Test] public void SlowMotion_MarksMetricsInvalid_Sticky() {
            var c=new SimClock(120,1); c.DebugSlowMotion=0.5;
            Assert.That(c.Advance(1.0/60),Is.EqualTo(1)); Assert.That(c.MetricsInvalid,Is.True);
            c.DebugSlowMotion=1; c.Restart(); c.Configure(120,1); Assert.That(c.MetricsInvalid,Is.True);
        }
        [TestCase(-1)] [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)]
        public void Advance_InvalidDeltaIsRejected(double delta) => Assert.Throws<ArgumentOutOfRangeException>(()=>new SimClock(120,1).Advance(delta));
    }
}
