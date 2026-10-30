using System;
using System.Collections.Generic;
using UnityEngine;

namespace SquashClash.Core.Shot
{
    public enum GestureOutcome { None, Tap, Canceled, SystemCanceled, Swing, ZoneCanceled, AbandonedBeforeFix, InvalidAim, AbandonedNoPull, CanceledReturnToDeadZone, Rejected }
    public sealed class GestureTracker
    {
        private readonly float tapMaxMm, tapMaxMs, cancelRadiusMm, mmPerPx;
        private readonly List<TouchSample> samples = new List<TouchSample>(256);
        private int finger;
        private Vector2 origin;
        private double beganAt;
        public GestureTracker(float tapMaxMm, float tapMaxMs, float cancelRadiusMm, float mmPerPx)
        {
            if (!Positive(tapMaxMm) || !Positive(tapMaxMs) || !Positive(cancelRadiusMm) || !Positive(mmPerPx))
                throw new ArgumentOutOfRangeException(nameof(tapMaxMm));
            this.tapMaxMm = tapMaxMm; this.tapMaxMs = tapMaxMs;
            this.cancelRadiusMm = cancelRadiusMm; this.mmPerPx = mmPerPx;
            Samples = samples.AsReadOnly();
        }
        public bool Active { get; private set; }
        public GestureOutcome Outcome { get; private set; }
        public double ReleaseTimeReal { get; private set; }
        public IReadOnlyList<TouchSample> Samples { get; }
        public float MaxDisplacementMm { get; private set; }
        public bool CancelArmed { get; private set; }
        public double DragStartedAtReal { get; private set; } = double.NaN;
        public void Push(in TouchSample s)
        {
            if (s.Phase == TouchPhaseKind.Began)
            {
                if (Active) return;
                Reset(); Active = true; finger = s.FingerId; origin = s.PositionPx; beganAt = s.TimeReal;
            }
            else if (!Active || s.FingerId != finger) return;
            // Bound recordings without inventing replacement samples or consuming an attempt.
            if (samples.Count >= 32768) { CancelBySystem(); return; }
            samples.Add(s);
            float distance = Vector2.Distance(origin, s.PositionPx) * mmPerPx;
            MaxDisplacementMm = Mathf.Max(MaxDisplacementMm, distance);
            if (double.IsNaN(DragStartedAtReal) && MaxDisplacementMm >= tapMaxMm) DragStartedAtReal = s.TimeReal;
            if (distance > cancelRadiusMm) CancelArmed = true;
            if (s.Phase == TouchPhaseKind.Canceled) { CancelBySystem(); return; }
            if (s.Phase != TouchPhaseKind.Ended) return;
            ReleaseTimeReal = s.TimeReal; Active = false;
            double elapsedMs = (s.TimeReal - beganAt) * 1000;
            Outcome = MaxDisplacementMm < tapMaxMm && elapsedMs >= 0 && elapsedMs < tapMaxMs
                ? GestureOutcome.Tap : CancelArmed && distance <= cancelRadiusMm
                    ? GestureOutcome.Canceled : GestureOutcome.Swing;
        }
        public void CancelBySystem()
        {
            if (!Active) return;
            Active = false; Outcome = GestureOutcome.SystemCanceled;
        }
        public void Reset()
        {
            Active = false; Outcome = GestureOutcome.None; samples.Clear();
            ReleaseTimeReal = 0; MaxDisplacementMm = 0; CancelArmed = false; DragStartedAtReal = double.NaN;
        }
        private static bool Positive(float value) => value > 0 && !float.IsInfinity(value) && !float.IsNaN(value);
    }
}
