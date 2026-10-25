using UnityEngine;

namespace SquashClash.Core.Shot
{
    public enum TouchPhaseKind { Began, Moved, Stationary, Ended, Canceled }
    public readonly struct TouchSample
    {
        public readonly int FingerId;
        public readonly TouchPhaseKind Phase;
        public readonly Vector2 PositionPx;
        public readonly double TimeReal;
        public readonly long Sequence;
        public TouchSample(int fingerId, TouchPhaseKind phase, Vector2 positionPx, double timeReal, long sequence)
        { FingerId = fingerId; Phase = phase; PositionPx = positionPx; TimeReal = timeReal; Sequence = sequence; }
    }
}
