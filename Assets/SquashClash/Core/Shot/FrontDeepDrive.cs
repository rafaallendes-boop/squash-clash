using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Core.Shot
{
    public readonly struct FrontDeepInfo
    {
        public readonly string Reason;
        public readonly float T;
        public readonly int Evaluations;
        public FrontDeepInfo(string reason, float t, int evaluations) { Reason = reason; T = t; Evaluations = evaluations; }
        public bool Acted => Reason == FrontDeepDrive.ReasonActed;
    }

    // One "front-deep" metrics line per human solve inside the zone, so the PO's play shows when the guard acted.
    [Serializable] public sealed class FrontDeepTrace
    {
        public long launchId, contactStep; public string attemptId, reason; public bool on, acted; public float t, originalX, chosenX, y; public int evaluations;
    }

    // Human-only side-wall guard for a deep drive struck in front of the short line (design 2026-11-20-front-deep-drive).
    // Called by the human ShotController only, after CausalQuality.Solve; the rival, RallySim and the AI forecast never call it.
    // From the front, the same front-wall point means a much wider angle than from the back, and the front wall's restitution
    // opens it further on the way back: the ball meets a side wall before its first bounce. When that happens, the
    // guard keeps the speed and closes the front-wall x towards the contact to the widest one that still reaches the back third
    // before any side wall; only when no x works at that height (right at the front wall) it tries lower heights.
    // It changes only the front-wall target of a velocity the caller applies; physics and the ball authority are untouched.
    public static class FrontDeepDrive
    {
        public const string ReasonActed = "front-deep";
        public const float MinAimHeight = .80f;
        // Length ceiling (PO, 25 nov: «Sí, tope de largo a 8,5 m»): the first bounce of a successful stroke lands at z <= this.
        public const float MaxFirstBounceZ = 8.5f;
        public const int CoarseSteps = 8, RefineSteps = 4, FallbackLevels = 3;
        // Revision 1 bound (design section 5): one check, 8 + 3 x 9 coarse probes and 4 bisection steps.
        public const int MaxEvaluations = 1 + CoarseSteps + FallbackLevels * (CoarseSteps + 1) + RefineSteps;

        // Zone: above medium power, a requested aim high enough for length, contact in front of the short line.
        public static bool Active(float power, Vector3 from, Vector2 requestedAim, CourtDimensions court)
            => power > .5f && requestedAim.y >= MinAimHeight && from.z < court.Length - court.ShortLineNearBackEdgeFromBack;

        // Success (design section 2, revision 2 of 25 nov): legal to the second bounce, no side wall and no back wall between the
        // front wall and the first bounce, the first bounce at z <= MaxFirstBounceZ, and the ball reaches the back third
        // (z >= 2/3 of the length) before its second bounce.
        public static bool Deep(Vector3 from, Vector3 velocity, CourtDimensions court, IBallSimulationFactory factory, PhysicsParams physics, float footprintRadius)
        {
            var rules = new PointRules(court, footprintRadius); rules.Begin(PlayerId.Human, PlayerId.Rival);
            float back = court.Length * 2f / 3f;
            bool first = true, reached = false; int floors = 0;
            using (var simulation = factory.Create(physics, court))
            {
                simulation.Reset(new BallState { Position = from, Velocity = velocity });
                var contacts = new List<ContactEvent>(4);
                for (int step = 1; step <= physics.StepHz * 10 && !rules.Closed; step++)
                {
                    contacts.Clear(); simulation.Step(contacts);
                    foreach (var hit in contacts)
                    {
                        if (rules.Closed) break;
                        if (!hit.Persistent)
                        {
                            if (first) { first = false; if (hit.Surface != SurfaceId.FrontWall) return false; }
                            else if (floors == 0 && (hit.Surface == SurfaceId.LeftWall || hit.Surface == SurfaceId.RightWall || hit.Surface == SurfaceId.BackWall)) return false;
                            if (hit.Surface == SurfaceId.Floor && ++floors == 1 && !(hit.Point.z <= MaxFirstBounceZ)) return false;
                        }
                        rules.OnBallContact(hit);
                    }
                    if (!first && floors < 2 && !rules.Closed && simulation.State.Position.z >= back) reached = true;
                    if (!rules.Closed && simulation.State.Position.y > court.WallHeight) rules.OnOverTop();
                }
            }
            return reached && rules.Cause == PointEndCause.DoubleBounce;
        }

        public static ShotSolution Apply(Vector3 from, Vector2 requestedAim, float power, in ShotSolution solution, CourtDimensions court,
            IBallSimulationFactory factory, PhysicsParams physics, float footprintRadius, out FrontDeepInfo info)
        {
            info = new FrontDeepInfo("inactive", 0, 0);
            if (!solution.Solved || !Active(power, from, requestedAim, court)) return solution;
            int evaluations = 1;
            if (Deep(from, solution.Velocity, court, factory, physics, footprintRadius)) { info = new FrontDeepInfo("already-deep", 1, evaluations); return solution; }
            float y0 = solution.TargetUsed.y, speed = solution.Speed, x0 = from.x, x1 = solution.TargetUsed.x;
            float legalLow = Ceiling((double)court.TinTop + physics.BallRadius);
            // Frozen speed; a request the primitive can only meet by lowering the target is not a candidate.
            bool Try(float t, float y, out ShotSolution candidate)
            {
                evaluations++;
                float x = t >= 1 ? x1 : x0 + (x1 - x0) * t;
                candidate = ShotSolver.Solve(new ShotRequest(from, new Vector2(x, y), power), speed, speed, speed, physics.Gravity, physics.BallRadius, court, factory, physics);
                return candidate.Solved && !candidate.TargetLowered && Deep(from, candidate.Velocity, court, factory, physics, footprintRadius);
            }
            // Coarse scan from the widest fraction down, at the effective height first and then at lower heights only if no x works.
            for (int level = 0; level <= FallbackLevels; level++)
            {
                float y = level == 0 ? y0 : (float)(y0 - ((double)y0 - legalLow) * level / FallbackLevels);
                if (!(y > court.TinTop)) break;
                for (int k = level == 0 ? CoarseSteps - 1 : CoarseSteps; k >= 0; k--)
                {
                    float t = (float)k / CoarseSteps;
                    if (!Try(t, y, out var best)) continue;
                    float low = t, high = k < CoarseSteps ? (float)(k + 1) / CoarseSteps : t;
                    for (int i = 0; i < RefineSteps && high > low; i++)
                    {
                        float mid = (low + high) * .5f;
                        if (Try(mid, y, out var candidate)) { low = mid; best = candidate; } else high = mid;
                    }
                    info = new FrontDeepInfo(ReasonActed, low, evaluations);
                    return new ShotSolution(true, best.Velocity, best.Speed, best.TargetUsed, solution.SpeedRaised,
                        solution.TargetLowered || best.TargetUsed.y < y0, ReasonActed);
                }
            }
            info = new FrontDeepInfo("no-deep-x", 0, evaluations);
            return solution;
        }

        private static float Ceiling(double value)
        {
            float rounded = (float)value;
            return rounded < value ? BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(rounded) + 1) : rounded;
        }
    }
}
