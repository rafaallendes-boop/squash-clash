using System;
using NUnit.Framework;
using SquashClash.Core.Shot;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class ClockMapTests
    {
        [Test] public void ContinuousSegment_MapsWithTempo()
        {
            var c=new ClockMap();c.Begin(7,10,0,.65,1,120);
            Assert.That(c.TryRealToSim(10.04,out var s,out var f),Is.True);Assert.That(s,Is.EqualTo(3));Assert.That(f,Is.EqualTo(.12).Within(1e-10));
            Assert.That(c.InvalidSince(10),Is.False);
        }
        [Test] public void PausedSegment_DoesNotAdvanceSimTime()
        {
            var c=new ClockMap();c.Begin(1,0,0,1,1,120);c.Mark(.1,12,0,1,1,true);
            Assert.That(c.TryRealToSim(.3,out var s,out var f),Is.True);Assert.That(s,Is.EqualTo(12));Assert.That(f,Is.Zero);
            c.Mark(.4,12,0,1,1,false);Assert.That(c.TryRealToSim(.5,out s,out f),Is.True);Assert.That(s,Is.EqualTo(24));Assert.That(f,Is.EqualTo(0).Within(1e-10));
            Assert.That(c.InvalidSince(.05),Is.True);
        }
        [Test] public void SlowMotionChange_IsSegmented_AndMarksInvalid()
        {
            var c=new ClockMap();c.Begin(1,0,0,.65,1,120);c.Mark(.1,7,.8,.65,.5,false);
            Assert.That(c.TryRealToSim(.2,out var s,out var f),Is.True);Assert.That(s,Is.EqualTo(11));Assert.That(f,Is.EqualTo(.7).Within(1e-10));Assert.That(c.InvalidSince(0),Is.True);
            Assert.That(c.TryRealToSim(.05,out s,out f),Is.True);Assert.That(s,Is.EqualTo(3));Assert.That(f,Is.EqualTo(.9).Within(1e-10));
        }
        [Test] public void DroppedSteps_MarkInvalid_DoNotFabricateTime()
        {
            var c=new ClockMap();c.Begin(1,0,0,1,1,120);c.Mark(.2,8,0,1,1,false,droppedSteps:true);
            Assert.That(c.TryRealToSim(.1,out _,out _),Is.False,"Unknown position of discarded real time cannot be invented");
            Assert.That(c.TryRealToSim(.2,out var s,out _),Is.True);Assert.That(s,Is.EqualTo(8));Assert.That(c.InvalidSince(0),Is.True);
            Assert.That(c.TrySimToReal(12,0,out var time),Is.True);Assert.That(time,Is.EqualTo(.2+4d/120).Within(1e-12));
        }
        [Test] public void OtherLaunchId_IsRejected()
        {
            var c=new ClockMap();c.Begin(22,3,0,1,1,120);
            Assert.That(c.TryRealToSim(21,3.1,out _,out _),Is.False);Assert.That(c.TryRealToSim(22,3.1,out _,out _),Is.True);
            c.Begin(23,4,0,1,1,120);Assert.That(c.LaunchId,Is.EqualTo(23));Assert.That(c.TryRealToSim(3.5,out _,out _),Is.False);
        }
        [Test] public void RoundTrip_RealSimReal_IsStableWithinOneMicrosecond()
        {
            var c=new ClockMap();c.Begin(1,10,500,.65,1,120);
            for(int i=0;i<100;i++){double r=10+i*.0197;Assert.That(c.TryRealToSim(r,out var s,out var f),Is.True);Assert.That(c.TrySimToReal(s,f,out var back),Is.True);Assert.That(back,Is.EqualTo(r).Within(.000001));}
        }
        [Test] public void M24LongGesture_OriginalEndedMapsAcrossFrames()
        {
            var c=new ClockMap();c.Begin(1,10,0,.65,1,120);c.Mark(11,78,0,.65,1,false);
            Assert.That(c.TryRealToSim(11.5,out var s,out _),Is.True);Assert.That(s,Is.EqualTo(117));Assert.That(c.InvalidSince(10),Is.False);
            Assert.That(c.TryRealToSim(11.49,out s,out var f),Is.True);Assert.That(s,Is.EqualTo(116));Assert.That(f,Is.EqualTo(.22).Within(1e-10));
        }
        [Test] public void LateDelivery_UsesEventTime_NotCurrentFrame()
        {
            var c=new ClockMap();c.Begin(1,0,0,1,1,120);c.Mark(1,120,0,1,1,false);
            Assert.That(c.TryRealToSim(.5,out var s,out _),Is.True);Assert.That(s,Is.EqualTo(60));Assert.That(c.TryRealToSim(-.1,out _,out _),Is.False);
        }
        [Test] public void ObservedFrameFraction_IsNotMistakenForDroppedSteps()
        {
            var c=new ClockMap();c.Begin(1,0,0,1,1,120);c.Mark(.1,12,.001,1,1,false);
            Assert.That(c.TryRealToSim(.05,out var step,out var fraction),Is.True);Assert.That(step,Is.EqualTo(6));Assert.That(fraction,Is.EqualTo(.0005).Within(1e-10));Assert.That(c.InvalidSince(0),Is.False);
        }
        [Test] public void TempoChange_IsSegmented_AndInvalidatesGesture()
        {
            var c=new ClockMap();c.Begin(1,0,0,1,1,120);c.Mark(.1,12,0,.65,1,false);
            Assert.That(c.TryRealToSim(.2,out var step,out var fraction),Is.True);Assert.That(step,Is.EqualTo(19));Assert.That(fraction,Is.EqualTo(.8).Within(1e-10));Assert.That(c.InvalidSince(0),Is.True);
        }
        [Test] public void PausedSingleStep_DoesNotInventContinuousAdvance()
        {
            var c=new ClockMap();c.Begin(1,0,0,1,1,120);c.Mark(.1,12,0,1,1,true);c.Mark(.2,13,0,1,1,true);
            Assert.That(c.TryRealToSim(.15,out var step,out var fraction),Is.True);Assert.That(step,Is.EqualTo(12));Assert.That(fraction,Is.Zero);
            Assert.That(c.TryRealToSim(.2,out step,out fraction),Is.True);Assert.That(step,Is.EqualTo(13));
        }
        [Test] public void StableObservations_PreserveOriginalEndedAndExactStateTransitions()
        {
            var c=new ClockMap();c.Begin(1,0,0,1,1,120);
            c.Mark(.5,60,0,1,1,false);c.Mark(1,120,0,1,1,true);
            c.Mark(30,120,0,1,1,true);c.Mark(60,120,0,1,1,true);
            Assert.That(c.TryRealToSim(.25,out var step,out var fraction),Is.True);
            Assert.That(step,Is.EqualTo(30));Assert.That(fraction,Is.Zero);
            c.Mark(61,121,0,1,1,true); // manual step remains discrete
            Assert.That(c.TryRealToSim(60.5,out step,out fraction),Is.True);Assert.That(step,Is.EqualTo(120));Assert.That(fraction,Is.Zero);
            c.Mark(62,121,0,1,1,false);c.Mark(63,241,0,.5,1,false);
            c.Mark(64,301,0,.5,.5,false);c.Mark(65,331,0,.5,.5,false);
            Assert.That(c.TryRealToSim(62.5,out step,out fraction),Is.True);Assert.That(step,Is.EqualTo(181));Assert.That(fraction,Is.Zero);
            Assert.That(c.TryRealToSim(63.5,out step,out fraction),Is.True);Assert.That(step,Is.EqualTo(271));Assert.That(fraction,Is.Zero);
            Assert.That(c.TrySimToReal(316,0,out var real),Is.True);Assert.That(real,Is.EqualTo(64.5));
            c.Mark(66,335,0,.5,.5,false,true);
            Assert.That(c.TryRealToSim(65.5,out _,out _),Is.False);Assert.That(c.InvalidSince(65),Is.True);
            c.Mark(67,365,0,.5,.5,false);
            Assert.That(c.TryRealToSim(66.5,out step,out fraction),Is.True);Assert.That(step,Is.EqualTo(350));Assert.That(fraction,Is.Zero);
        }
        private static int SegmentCount(ClockMap c)=>( (System.Collections.ICollection)typeof(ClockMap).GetField("segments",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(c)).Count;
        [TestCase(false)][TestCase(true)]
        public void StableObservations_CoalesceOnlyExactContinuation(bool paused)
        {
            var c=new ClockMap();c.Begin(1,0,0,1,1,120);
            if(paused)c.Mark(0,0,0,1,1,true);
            for(int i=1;i<=120;i++)c.Mark(i,paused?0:120*i,0,1,1,paused);
            Assert.That(SegmentCount(c),Is.EqualTo(1));
            Assert.That(c.TryRealToSim(119.5,out var step,out var fraction),Is.True);
            Assert.That(step,Is.EqualTo(paused?0:14340));Assert.That(fraction,Is.Zero);
            Assert.Throws<ArgumentOutOfRangeException>(()=>c.Mark(119,paused?0:14280,0,1,1,paused));
        }
        [Test] public void StablePrefix_BeforeDiscardOrInexactObservation_RemainsMappable()
        {
            var c=new ClockMap();c.Begin(1,0,0,1,1,120);c.Mark(1,120,0,1,1,false);c.Mark(2,240,0,1,1,false);
            c.Mark(3,241,0,1,1,false,true);
            Assert.That(c.TryRealToSim(1.5,out var step,out var fraction),Is.True);Assert.That(step,Is.EqualTo(180));Assert.That(fraction,Is.Zero);
            Assert.That(c.TryRealToSim(2.5,out _,out _),Is.False);
            Assert.That(c.TrySimToReal(180,0,out var real),Is.True);Assert.That(real,Is.EqualTo(1.5));
            c.Begin(2,0,0,1,1,120);c.Mark(1,120,0,1,1,false);c.Mark(2,240,0,1,1,false);
            c.Mark(3,360,.0000000001,1,1,false);
            Assert.That(c.TryRealToSim(1.5,out step,out fraction),Is.True);Assert.That(step,Is.EqualTo(180));Assert.That(fraction,Is.Zero);
            Assert.That(c.TryRealToSim(3,out step,out fraction),Is.True);Assert.That(step,Is.EqualTo(360));Assert.That(fraction,Is.GreaterThan(0));
        }
        [Test] public void InvalidInputs_DoNotCreateClockSegments()
        {
            var c=new ClockMap();Assert.That(c.TryRealToSim(0,out _,out _),Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(()=>c.Begin(1,0,0,0,1,120));
            c.Begin(1,0,0,1,1,120);Assert.That(c.TryRealToSim(double.NaN,out _,out _),Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(()=>c.Mark(-1,0,0,1,1,false));Assert.Throws<ArgumentOutOfRangeException>(()=>c.Mark(1,1,1,1,1,false));
        }
    }
}
