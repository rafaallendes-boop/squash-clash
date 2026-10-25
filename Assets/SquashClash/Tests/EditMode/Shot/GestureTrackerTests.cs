using NUnit.Framework;
using SquashClash.Core.Shot;
using UnityEngine;

namespace SquashClash.Core.Tests.Shot
{
    public sealed class GestureTrackerTests
    {
        private static TouchSample S(TouchPhaseKind p, float x, double t, int finger = 1) =>
            new TouchSample(finger, p, new Vector2(x, 0), t, (long)(t * 10000));
        private static GestureTracker Gesture(float max, float end, double duration)
        {
            var g = new GestureTracker(3, 250, 6, 1);
            g.Push(S(TouchPhaseKind.Began, 0, 1));
            g.Push(S(TouchPhaseKind.Moved, max, 1 + duration / 2));
            g.Push(S(TouchPhaseKind.Ended, end, 1 + duration));
            return g;
        }
        [Test] public void Tap_MaxDisplacementBelow3mmUnder250ms_IsTap() => Assert.That(Gesture(2.9f, 2, .2).Outcome, Is.EqualTo(GestureOutcome.Tap));
        [Test] public void Tap_DisplacementExactly3mm_IsNotTap() => Assert.That(Gesture(3, 3, .2).Outcome, Is.EqualTo(GestureOutcome.Swing));
        [Test] public void Tap_DurationExactly250ms_IsNotTap() => Assert.That(Gesture(1, 1, .25).Outcome, Is.EqualTo(GestureOutcome.Swing));
        [Test] public void Tap_JustBelowBothLimits_IsTap() => Assert.That(Gesture(2.999f, 2.999f, .2499).Outcome, Is.EqualTo(GestureOutcome.Tap));
        [Test] public void ShortDrag_EndsInsideCancelCircleWithoutLeaving_IsSwing() => Assert.That(Gesture(4, 4, .1).Outcome, Is.EqualTo(GestureOutcome.Swing));
        [Test] public void DragOutAndBack_EndsInsideCircle_IsCanceled()
        {
            var g = Gesture(9, 5, .1);
            Assert.That(g.Outcome, Is.EqualTo(GestureOutcome.Canceled));
            Assert.That(g.CancelArmed, Is.True); Assert.That(g.Active, Is.False);
        }
        [Test] public void DragOutAndBack_ExactlyOnRadius_IsCanceled() => Assert.That(Gesture(9, 6, .1).Outcome, Is.EqualTo(GestureOutcome.Canceled));
        [Test] public void DragOut_EndsOutsideCircle_IsSwingWithEventTime()
        {
            var g = Gesture(9, 10, .123456789);
            Assert.That(g.Outcome, Is.EqualTo(GestureOutcome.Swing));
            Assert.That(g.ReleaseTimeReal, Is.EqualTo(1.123456789));
            Assert.That(g.MaxDisplacementMm, Is.EqualTo(10));
        }
        [Test] public void TapMeasuredByMaxNotFinalDistance() => Assert.That(Gesture(5, 0, .15).Outcome, Is.EqualTo(GestureOutcome.Swing));
        [Test] public void SecondFinger_IsIgnored_AndNotPromotedWhenFirstLifts()
        {
            var g = new GestureTracker(3, 250, 6, 1);
            g.Push(S(TouchPhaseKind.Began, 0, 1));
            g.Push(S(TouchPhaseKind.Began, 10, 1.01, 2));
            g.Push(S(TouchPhaseKind.Moved, 50, 1.02, 2));
            g.Push(S(TouchPhaseKind.Ended, 4, 1.05));
            g.Push(S(TouchPhaseKind.Ended, 60, 1.06, 2));
            Assert.That(g.Outcome, Is.EqualTo(GestureOutcome.Swing));
            Assert.That(g.MaxDisplacementMm, Is.EqualTo(4));
            Assert.That(g.ReleaseTimeReal, Is.EqualTo(1.05));
            Assert.That(g.Samples.Count, Is.EqualTo(2));
        }
        [Test] public void SystemCancel_MidDrag_DoesNotConsumeAttempt()
        {
            var g = new GestureTracker(3, 250, 6, 1);
            g.Push(S(TouchPhaseKind.Began, 0, 1)); g.Push(S(TouchPhaseKind.Moved, 9, 1.02));
            g.CancelBySystem(); g.Push(S(TouchPhaseKind.Ended, 12, 1.03));
            Assert.That(g.Outcome, Is.EqualTo(GestureOutcome.SystemCanceled)); Assert.That(g.Active, Is.False);
            g.Push(S(TouchPhaseKind.Began, 0, 2)); g.Push(S(TouchPhaseKind.Ended, 4, 2.05));
            Assert.That(g.Outcome, Is.EqualTo(GestureOutcome.Swing), "A fresh Begin may try again.");
        }
        [Test] public void CanceledPhaseFromOs_IsSystemCanceled()
        {
            var g = new GestureTracker(3, 250, 6, 1);
            g.Push(S(TouchPhaseKind.Began, 0, 1)); g.Push(S(TouchPhaseKind.Canceled, 0, 1.01));
            Assert.That(g.Outcome, Is.EqualTo(GestureOutcome.SystemCanceled));
        }
        [Test] public void SamplesAreCopied_NotReferenced()
        {
            var input = new[] { S(TouchPhaseKind.Began, 0, 1), S(TouchPhaseKind.Ended, 4, 1.1) };
            var g = new GestureTracker(3, 250, 6, 1);
            foreach (var s in input) g.Push(s);
            input[0] = S(TouchPhaseKind.Began, 999, 99);
            Assert.That(g.Samples.Count, Is.EqualTo(2)); Assert.That(g.Samples[0].PositionPx.x, Is.Zero);
        }
    }
}
