using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    public readonly struct BoastInfo
    {
        public readonly bool Solved;
        public readonly SideAim Requested, Used;
        // Where the verified flight meets the front wall (x, y), after the side wall.
        public readonly Vector2 Front;
        public readonly float RequestedSpeed, Speed;
        public readonly int Evaluations;
        public readonly string Reason;
        public BoastInfo(bool solved, SideAim requested, SideAim used, Vector2 front, float requestedSpeed, float speed, int evaluations, string reason)
        { Solved = solved; Requested = requested; Used = used; Front = front; RequestedSpeed = requestedSpeed; Speed = speed; Evaluations = evaluations; Reason = reason; }
        public bool Exact => Solved && Used.Equals(Requested) && Speed == RequestedSpeed;
    }

    // Addendum 1 (design 2026-11-20-boast-predeclaration): a boast with no valid point at the real contact goes to the front wall on the
    // same side, through the existing front-wall aim path. The aim is the straight drive of the rival's options on that side: the
    // front-wall point on the line to the mirrored near back corner (inset 0.6 m, RallyParams.CornerInset) at the mid height 1.7 m
    // (RallyParams.AimMid), so a failed boast becomes a plain length down the same wall.
    public static class BoastFallback
    {
        public const float Inset = .6f, Height = 1.7f;
        public static Vector2 Aim(Vector3 contact, SurfaceId wall, CourtDimensions court)
        {
            float corner = wall == SurfaceId.RightWall ? Inset : court.Width - Inset;
            return new Vector2(contact.x + (corner - contact.x) * contact.z / (contact.z + court.Length), Height);
        }
    }
    // The two-wall shot (boast), design 2026-11-20-boast-predeclaration, section 2. Aims the ball centre at a side-wall point with the
    // stroke's speed and VERIFIES with the shared physics (P2) the sequence racket -> that side wall -> front wall, with the front-wall
    // contact clear of the tin and of the out line by a margin, no floor before the front wall, and legal up to the first bounce.
    // If the exact point fails, the nearest valid point (side height and depth, speed within the power band) is used; if none, the
    // shot is unsolved, the same path an unsolvable front-wall aim takes. Physics, the contact contract and timing are untouched:
    // this only chooses a velocity the caller applies.
    public static class BoastSolver
    {
        public const string ReasonExact = "boast", ReasonNearest = "boast-nearest", ReasonNone = "boast-no-valid";
        // Front-wall clearance of the verified contact (centre height) above the tin and below the out line, in metres.
        public const float TinMargin = .10f, OutMargin = .10f;
        // The first contact must be the aimed side wall within this distance of the aimed point (surface coordinates).
        public const float PointTolerance = .05f;
        // The power band of the search: the requested speed plus or minus this fraction, inside [vMin, vCap].
        public const float SpeedBand = .15f;
        // Fixed budget of P2 trajectories per solve, the exact point included.
        public const int Budget = 25;

        public static ShotSolution Solve(Vector3 from, in SideAim aim, float speed, float vMin, float vCap, CourtDimensions court,
            IBallSimulationFactory factory, PhysicsParams physics, float footprintRadius, out BoastInfo info)
        {
            var requested = aim;
            info = new BoastInfo(false, requested, requested, default, speed, 0, 0, ReasonNone);
            if (!aim.Exists || court == null || physics == null || factory == null || !Finite(from.x) || !Finite(from.y) || !Finite(from.z) ||
                !Finite(aim.Z) || !Finite(aim.Y) || !Finite(speed) || speed <= 0 || !Finite(vMin) || !Finite(vCap) || vMin <= 0 || vMin > vCap || physics.Gravity <= 0)
                return Fail("invalid-request-or-physics", 0, requested, speed, out info);
            float r = physics.BallRadius;
            if (from.x < r || from.x > court.Width - r || from.y < r || from.z < r || from.z > court.Length - r)
                return Fail("contact-outside-court", 0, requested, speed, out info);
            float low = Math.Max(vMin, speed * (1 - SpeedBand)), high = Math.Min(vCap, speed * (1 + SpeedBand));
            // Nearest valid point (design section 2): the height first, in the direction the miss asks for (a ball that dies before the
            // front wall or under its margin goes higher, one over the out line goes lower), then the depth and the speed inside the band.
            // Each (depth, speed) pair is one probe at the requested height plus a short height walk. Fixed budget, deterministic order.
            int evaluations = 0;
            var pairs = new List<(float dz, float s)>(8) { (0, speed) };
            if (high != speed) pairs.Add((0, high));
            pairs.Add((-DepthStep, speed)); pairs.Add((DepthStep, speed));
            if (low != speed) pairs.Add((0, low));
            pairs.Add((-2 * DepthStep, speed)); pairs.Add((2 * DepthStep, speed));
            foreach (var (dz, s) in pairs)
            {
                if (s < vMin || s > vCap) continue;
                float z = aim.Z + dz;
                if (!DepthInDomain(from, z, r)) continue;
                float yLow = r + .05f, yHigh = SideAim.OutEdge(court, z) - r - .05f;
                // A requested height off the wall's legal band walks back into it.
                Miss first = aim.Y < yLow ? Miss.Low : aim.Y > yHigh ? Miss.High : Miss.Other;
                for (int k = 0; k <= HeightWalk; k++)
                {
                    float y = aim.Y + (k == 0 ? 0 : (first == Miss.High ? -k : k) * HeightStep);
                    if (k > 0 && first == Miss.Other) break;
                    if (y < yLow || y > yHigh) continue;
                    if (evaluations >= Budget) return Fail(ReasonNone, evaluations, requested, speed, out info);
                    evaluations++;
                    var side = aim.With(z, y);
                    // Out of reach at this speed: the point is too high or too far, so the walk goes lower.
                    var miss = Miss.High;
                    if (Launch(from, side.Center(court, r), s, physics.Gravity, out var velocity) &&
                        Classify(from, velocity, side, court, factory, physics, footprintRadius, out var front, out miss))
                    {
                        bool exact = dz == 0 && k == 0 && s == speed;
                        string reason = exact ? ReasonExact : ReasonNearest;
                        info = new BoastInfo(true, requested, side, front, speed, s, evaluations, reason);
                        return new ShotSolution(true, velocity, s, front, s > speed, false, reason);
                    }
                    if (first == Miss.Other) first = miss;
                }
            }
            return Fail(ReasonNone, evaluations, requested, speed, out info);
        }
        private static ShotSolution Fail(string reason, int evaluations, SideAim requested, float speed, out BoastInfo info)
        {
            info = new BoastInfo(false, requested, requested, default, speed, 0, evaluations, reason);
            return new ShotSolution(false, Vector3.zero, 0, default, false, false, reason);
        }

        // Search steps (metres) on the side wall: the height walk and the depth offsets.
        public const float HeightStep = .3f, DepthStep = .4f;
        public const int HeightWalk = 5;
        private enum Miss { Low, High, Other }

        // A side depth the ball can meet on its way forward: ahead of the contact and inside the length.
        private static bool DepthInDomain(Vector3 from, float z, float r) => z > r && z < from.z - .1f;

        // The low-arc launch to a point at a given speed (the primitive solver's rationalized root), or false when out of reach.
        private static bool Launch(Vector3 from, Vector3 to, float speed, float gravity, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            double dx = (double)to.x - from.x, dz = (double)to.z - from.z, d2 = dx * dx + dz * dz, height = (double)to.y - from.y;
            if (d2 == 0) return false;
            double v2 = (double)speed * speed, discriminant = v2 * v2 - gravity * (gravity * d2 + 2 * height * v2);
            if (discriminant < 0) return false;
            double timeSquared = 2 * (d2 + height * height) / (v2 - gravity * height + Math.Sqrt(discriminant));
            if (!(timeSquared > 0) || double.IsInfinity(timeSquared)) return false;
            double time = Math.Sqrt(timeSquared);
            velocity = new Vector3((float)(dx / time), (float)(height / time + .5 * gravity * time), (float)(dz / time));
            return Finite(velocity.x) && Finite(velocity.y) && Finite(velocity.z);
        }

        public static bool Verify(Vector3 from, Vector3 velocity, in SideAim target, CourtDimensions court, IBallSimulationFactory factory,
            PhysicsParams physics, float footprintRadius, out Vector2 front)
            => Classify(from, velocity, target, court, factory, physics, footprintRadius, out front, out _);
        // The verification itself, with why a flight is not a boast: Low (floor first, under the tin margin, or too slow to arrive), High
        // (over the out lines or the top) or Other (another surface first, or off the aimed point).
        private static bool Classify(Vector3 from, Vector3 velocity, in SideAim target, CourtDimensions court, IBallSimulationFactory factory,
            PhysicsParams physics, float footprintRadius, out Vector2 front, out Miss miss)
        {
            front = default; miss = Miss.Other;
            if (!target.Exists || court == null || physics == null || factory == null) return false;
            var rules = new PointRules(court, footprintRadius); rules.Begin(PlayerId.Human, PlayerId.Rival);
            int stage = 0;
            using (var simulation = factory.Create(physics, court))
            {
                simulation.Reset(new BallState { Position = from, Velocity = velocity });
                var contacts = new List<ContactEvent>(4);
                for (int step = 1; step <= physics.StepHz * 6; step++)
                {
                    contacts.Clear(); simulation.Step(contacts);
                    foreach (var hit in contacts)
                    {
                        if (hit.Persistent || hit.Surface == SurfaceId.Floor)
                        {
                            if (stage < 2) { miss = stage == 0 ? Miss.Other : Miss.Low; return false; }
                            rules.OnBallContact(hit);
                            if (rules.Closed) { miss = Miss.Other; return false; }
                            if (!hit.Persistent) return true;
                            continue;
                        }
                        if (stage == 0)
                        {
                            if (hit.Surface != target.Wall || Vector2.Distance(hit.UV, target.UV) > PointTolerance) return false;
                            rules.OnBallContact(hit); if (rules.Closed) { miss = Miss.High; return false; }
                            stage = 1; continue;
                        }
                        if (stage == 1)
                        {
                            if (hit.Surface != SurfaceId.FrontWall) return false;
                            float y = hit.Point.y;
                            if (y < court.TinTop + TinMargin) { miss = Miss.Low; return false; }
                            if (y > court.FrontOutLower - OutMargin) { miss = Miss.High; return false; }
                            rules.OnBallContact(hit); if (rules.Closed) return false;
                            front = new Vector2(hit.Point.x, hit.Point.y); stage = 2; continue;
                        }
                        rules.OnBallContact(hit);
                        if (rules.Closed) { miss = Miss.High; return false; }
                    }
                    if (simulation.State.Position.y > court.WallHeight) { miss = Miss.High; return false; }
                }
            }
            miss = stage == 2 ? Miss.Other : Miss.Low;
            return false;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
