using System;
using System.Collections.Generic;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Runtime.Physics
{
    // Presentation order of immutable samples, not an assertion about unknown impact times.
    public sealed class PhysXContactOrder : IComparer<ContactEvent>
    {
        public static readonly PhysXContactOrder Instance = new PhysXContactOrder();
        private PhysXContactOrder() { }
        public int Compare(ContactEvent a, ContactEvent b)
        {
            // NaN means StepOnly/unknown: place after numeric times, never substitute a time.
            int c = float.IsNaN(a.TimeInStep) ? (float.IsNaN(b.TimeInStep) ? 0 : 1) :
                float.IsNaN(b.TimeInStep) ? -1 : a.TimeInStep.CompareTo(b.TimeInStep);
            if (c != 0) return c;
            c = ((int)a.Surface).CompareTo((int)b.Surface); if (c != 0) return c;
            c = a.Persistent.CompareTo(b.Persistent); if (c != 0) return c; // impacts before stays
            // Complete deterministic tie-break, no instance IDs or callback/list order.
            // Lexicographic Step, Timing, then unsigned float bits: time, point, UV, normal, approach.
            c = a.Step.CompareTo(b.Step); if (c != 0) return c;
            c = ((int)a.Timing).CompareTo((int)b.Timing); if (c != 0) return c;
            c = Bits(a.TimeInStep, b.TimeInStep); if (c != 0) return c;
            c = VectorBits(a.Point, b.Point); if (c != 0) return c;
            c = Bits(a.UV.x, b.UV.x); if (c != 0) return c;
            c = Bits(a.UV.y, b.UV.y); if (c != 0) return c;
            c = VectorBits(a.Normal, b.Normal); if (c != 0) return c;
            return VectorBits(a.ApproachVelocity, b.ApproachVelocity);
            // Equal in every field/bit: indistinguishable duplicates; sort stability is irrelevant.
        }
        private static int Bits(float a, float b) => unchecked((uint)BitConverter.SingleToInt32Bits(a)).CompareTo(unchecked((uint)BitConverter.SingleToInt32Bits(b)));
        private static int VectorBits(Vector3 a, Vector3 b)
        {
            int c = Bits(a.x, b.x); if (c != 0) return c;
            c = Bits(a.y, b.y); return c != 0 ? c : Bits(a.z, b.z);
        }
    }
}
