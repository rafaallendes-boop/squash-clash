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
    // opens it further on the way back: the ball meets a side wall before its first bounce. When that happens, and the
    // straight return from the contact would reach the back, the guard keeps the height and the speed and closes the
    // front-wall x towards the contact to the widest one that still reaches the back third before any side wall.
    // It changes only the front-wall x of a velocity the caller applies; physics and the ball authority are untouched.
    public static class FrontDeepDrive
    {
        public const string ReasonActed = "front-deep";
        public const float MinAimHeight = .80f;
        public const int BisectionSteps = 8;
        // Revision 1 bound (design section 5): one check, 8 + 3 x 9 coarse probes and 4 bisection steps.
        public const int MaxEvaluations = 40;

        // Zone: above medium power, a requested aim high enough for length, contact in front of the short line.
        public static bool Active(float power, Vector3 from, Vector2 requestedAim, CourtDimensions court)
            => power > .5f && requestedAim.y >= MinAimHeight && from.z < court.Length - court.ShortLineNearBackEdgeFromBack;

        // Success (design section 2): legal to the second bounce, no side wall between the front wall and the first bounce,
        // and the ball reaches the back third (z >= 2/3 of the length) before its second bounce.
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
                            else if (floors == 0 && (hit.Surface == SurfaceId.LeftWall || hit.Surface == SurfaceId.RightWall)) return false;
                            if (hit.Surface == SurfaceId.Floor) floors++;
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
            float y = solution.TargetUsed.y, speed = solution.Speed, x0 = from.x, x1 = solution.TargetUsed.x;
            // Same height, frozen speed; a request the primitive can only meet by lowering the target is not a candidate.
            bool Try(float t, out ShotSolution candidate)
            {
                evaluations++;
                float x = t >= 1 ? x1 : x0 + (x1 - x0) * t;
                candidate = ShotSolver.Solve(new ShotRequest(from, new Vector2(x, y), power), speed, speed, speed, physics.Gravity, physics.BallRadius, court, factory, physics);
                return candidate.Solved && !candidate.TargetLowered && Deep(from, candidate.Velocity, court, factory, physics, footprintRadius);
            }
            if (!Try(0, out var best)) { info = new FrontDeepInfo("pivot-not-deep", 0, evaluations); return solution; }
            float low = 0, high = 1;
            for (int i = 0; i < BisectionSteps; i++)
            {
                float mid = (low + high) * .5f;
                if (Try(mid, out var candidate)) { low = mid; best = candidate; } else high = mid;
            }
            info = new FrontDeepInfo(ReasonActed, low, evaluations);
            return new ShotSolution(true, best.Velocity, best.Speed, best.TargetUsed, solution.SpeedRaised, solution.TargetLowered, ReasonActed);
        }
    }
}
