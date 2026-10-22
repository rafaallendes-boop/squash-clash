using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Core.Spike
{
    public static class PhysicsMeasures
    {
        private static readonly SurfaceId[] Faces = { SurfaceId.Floor, SurfaceId.FrontWall, SurfaceId.BackWall, SurfaceId.RightWall, SurfaceId.LeftWall };

        public static int Crossings(Trajectory t, CourtDimensions d, out string detail)
        {
            if (!AllFinite(t, out _)) throw new ArgumentException("Crossings requires finite ball states.", nameof(t));
            int count = 0;
            var findings = new List<string>();
            for (int i = 1; i < t.States.Count; i++)
            {
                var a = t.States[i - 1]; var b = t.States[i];
                foreach (var face in Faces)
                {
                    double da = Distance(a.Position, face, d), db = Distance(b.Position, face, d);
                    bool outside = db < 0 && AtWallHeight(b.Position, face, d);
                    bool unreported = false;
                    if ((da >= 0 && db < 0) || (da < 0 && db >= 0))
                    {
                        var crossing = Vector3.LerpUnclamped(a.Position, b.Position, (float)(da / (da - db)));
                        unreported = OnFiniteFace(crossing, face, d) && !HasContact(t, b.Step, face);
                    }
                    // One finding per surface/step, even when both checks detect it.
                    if (outside || unreported)
                    {
                        count++; findings.Add($"{face} step={b.Step}: {(outside ? "center beyond plane" : "segment without contact")}");
                    }
                }
            }
            detail = string.Join("; ", findings);
            return count;
        }

        public static bool ExitedOverTop(Trajectory t, CourtDimensions d)
        {
            foreach (var s in t.States) if (Finite(s.Position) && s.Position.y > d.WallHeight) return true;
            return false;
        }

        public static float MaxPenetration(Trajectory t, CourtDimensions d, float radius)
        {
            if (!AllFinite(t, out _) || !Finite(radius) || radius < 0) return float.NaN;
            double maximum = 0;
            foreach (var s in t.States) foreach (var face in Faces)
                if (AtWallHeight(s.Position, face, d)) maximum = Math.Max(maximum, radius - Distance(s.Position, face, d));
            return (float)maximum;
        }

        public static bool AllFinite(Trajectory t, out int firstBadIndex)
        {
            firstBadIndex = -1;
            if (t == null || t.States.Count == 0) return false;
            for (int i = 0; i < t.States.Count; i++)
                if (!Finite(t.States[i].Position) || !Finite(t.States[i].Velocity)) { firstBadIndex = i; return false; }
            return true;
        }

        public static float MaxRelativeEnergyGain(Trajectory t, PhysicsParams p, bool cornerStepsOnly, out long step)
        {
            step = -1;
            if (!AllFinite(t, out _) || !Finite(p.Gravity) || !Finite(p.BallMass) || p.BallMass <= 0) return float.NaN;
            var considered = new HashSet<long>();
            double maximum = 0;
            foreach (var c in t.Contacts)
            {
                if (!considered.Add(c.Step) || (cornerStepsOnly && !IsCornerStep(t, c.Step))) continue;
                int index = StateIndex(t, c.Step);
                if (index < 1 || t.States[index - 1].Step != c.Step - 1) return float.NaN;
                double before = Energy(t.States[index - 1], p), after = Energy(t.States[index], p);
                if (before < 0) return float.NaN;
                double gain = before == 0 ? (after > 0 ? double.PositiveInfinity : 0) : (after - before) / before;
                if (step < 0 || gain > maximum) { maximum = Math.Max(0, gain); step = c.Step; }
            }
            return step < 0 ? float.NaN : (float)maximum;
        }

        public static bool IsCornerStep(Trajectory t, long step)
        {
            foreach (var a in t.Contacts)
            {
                if (a.Step != step) continue;
                foreach (var b in t.Contacts)
                    if (a.Surface != b.Surface && (b.Step == step || b.Step == step - 1 || b.Step == step + 1)) return true;
            }
            return false;
        }

        public static int RepeatedImpacts(Trajectory t, float minNormalSpeed)
        {
            int count = 0;
            var steps = new HashSet<(SurfaceId surface, long step)>();
            foreach (var c in t.Contacts)
            {
                if (c.Persistent) continue;
                // Unknown timing is valid here; unknown normal speed is not.
                if (!Finite(c.ApproachVelocity) || !Finite(c.Normal) || c.Normal.normalized == Vector3.zero)
                    throw new ArgumentException("C4 requires finite approach velocity and a usable contact normal.", nameof(t));
                if (Mathf.Abs(Vector3.Dot(c.ApproachVelocity, c.Normal.normalized)) > minNormalSpeed) steps.Add((c.Surface, c.Step));
            }
            foreach (var entry in steps) if (steps.Contains((entry.surface, entry.step - 1))) count++;
            return count;
        }

        public static int PersistentSamples(Trajectory t)
        {
            int count = 0; foreach (var c in t.Contacts) if (c.Persistent) count++; return count;
        }

        public static bool TrajectoriesBitwiseEqual(Trajectory a, Trajectory b, out int firstDiffIndex)
        {
            firstDiffIndex = 0;
            if (a.States.Count == 0 || b.States.Count == 0) return false;
            int common = Math.Min(a.States.Count, b.States.Count);
            for (int i = 0; i < common; i++)
            {
                var x = a.States[i]; var y = b.States[i];
                if (x.Step != y.Step || !SameBits(x.Position, y.Position) || !SameBits(x.Velocity, y.Velocity)) { firstDiffIndex = i; return false; }
            }
            firstDiffIndex = common;
            if (a.States.Count != b.States.Count) return false;
            int contacts = Math.Min(a.Contacts.Count, b.Contacts.Count);
            for (int i = 0; i < contacts; i++)
            {
                var x = a.Contacts[i]; var y = b.Contacts[i];
                if (x.Step != y.Step || x.Surface != y.Surface)
                {
                    firstDiffIndex = DivergentContactStateIndex(a, b, Math.Min(x.Step, y.Step)); return false;
                }
            }
            if (a.Contacts.Count != b.Contacts.Count)
            {
                var extra = a.Contacts.Count > contacts ? a.Contacts[contacts] : b.Contacts[contacts];
                firstDiffIndex = DivergentContactStateIndex(a, b, extra.Step); return false;
            }
            firstDiffIndex = -1; return true;
        }

        public static int MaxImpactsInOneStep(Trajectory t)
        {
            int maximum = 0; var counts = new Dictionary<long, int>();
            foreach (var c in t.Contacts)
            {
                if (c.Persistent) continue;
                counts.TryGetValue(c.Step, out int count); counts[c.Step] = ++count; maximum = Math.Max(maximum, count);
            }
            return maximum;
        }

        public static float ContactPrecisionMm(Trajectory t, PhysicsScenario s)
        {
            if (!s.ContactPoint.HasValue || !Finite(s.ContactPoint.Value)) return float.NaN;
            foreach (var c in t.Contacts)
                if (!c.Persistent) return Finite(c.Point) ? Vector3.Distance(c.Point, s.ContactPoint.Value) * 1000 : float.NaN;
            return float.NaN;
        }

        public static float RemainingTimeConsumed(Trajectory t, PhysicsParams p, int contactIndex)
        {
            if (contactIndex < 0 || contactIndex >= t.Contacts.Count) return float.NaN;
            var c = t.Contacts[contactIndex];
            if (c.Persistent || (c.Timing != ContactTiming.Exact && c.Timing != ContactTiming.Estimated) || !Finite(c.TimeInStep) || c.TimeInStep < 0) return float.NaN;
            int impacts = 0; foreach (var other in t.Contacts) if (other.Step == c.Step && !other.Persistent) impacts++;
            int index = StateIndex(t, c.Step);
            double remaining = p.StepSeconds - c.TimeInStep;
            if (impacts != 1 || index < 0 || p.StepHz <= 0 || remaining <= 0 || !Finite(c.Point) || !Finite(c.Normal)) return float.NaN;
            var normal = c.Normal.normalized; var end = t.States[index];
            if (!Finite(end.Position) || !Finite(end.Velocity)) return float.NaN;
            double outgoing = Vector3.Dot(end.Velocity, normal);
            if (outgoing <= 0) return float.NaN;
            // Diagnostic defined by T4.2: no clamping or invented exact timing.
            return (float)((Vector3.Dot(end.Position - c.Point, normal) - p.BallRadius) / (outgoing * remaining));
        }

        private static double Energy(BallState s, PhysicsParams p)
        {
            double x = s.Velocity.x, y = s.Velocity.y, z = s.Velocity.z;
            return .5 * p.BallMass * (x * x + y * y + z * z) + (double)p.BallMass * p.Gravity * s.Position.y;
        }
        private static int DivergentContactStateIndex(Trajectory a, Trajectory b, long step)
        {
            int x = StateIndex(a, step), y = StateIndex(b, step);
            return x >= 0 ? x : y >= 0 ? y : Math.Min(a.States.Count, b.States.Count);
        }
        private static int StateIndex(Trajectory t, long step) { for (int i = 0; i < t.States.Count; i++) if (t.States[i].Step == step) return i; return -1; }
        private static bool SameBits(Vector3 a, Vector3 b) => Bits(a.x) == Bits(b.x) && Bits(a.y) == Bits(b.y) && Bits(a.z) == Bits(b.z);
        private static int Bits(float value) => BitConverter.SingleToInt32Bits(value);
        private static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        private static bool Finite(Vector3 v) => Finite(v.x) && Finite(v.y) && Finite(v.z);
        private static bool HasContact(Trajectory t, long step, SurfaceId face) { foreach (var c in t.Contacts) if (c.Step == step && c.Surface == face) return true; return false; }
        private static bool AtWallHeight(Vector3 v, SurfaceId face, CourtDimensions d) => face == SurfaceId.Floor || (v.y >= 0 && v.y <= d.WallHeight);
        private static bool OnFiniteFace(Vector3 v, SurfaceId face, CourtDimensions d) => AtWallHeight(v, face, d) &&
            (face == SurfaceId.Floor ? v.x >= 0 && v.x <= d.Width && v.z >= 0 && v.z <= d.Length :
             face == SurfaceId.FrontWall || face == SurfaceId.BackWall ? v.x >= 0 && v.x <= d.Width : v.z >= 0 && v.z <= d.Length);
        private static double Distance(Vector3 v, SurfaceId face, CourtDimensions d)
        {
            switch (face)
            {
                case SurfaceId.Floor: return v.y;
                case SurfaceId.FrontWall: return v.z;
                case SurfaceId.BackWall: return (double)d.Length - v.z;
                case SurfaceId.RightWall: return v.x;
                case SurfaceId.LeftWall: return (double)d.Width - v.x;
                default: throw new ArgumentOutOfRangeException(nameof(face));
            }
        }
    }
}
