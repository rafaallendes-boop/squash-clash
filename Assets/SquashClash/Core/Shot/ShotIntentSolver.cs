using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Core.Shot
{
    public readonly struct ShotIntentInfo
    {
        public readonly int Evaluations;
        public readonly string Reason;
        public ShotIntentInfo(int evaluations, string reason) { Evaluations = evaluations; Reason = reason; }
    }

    // Release-time intention search; the physical front-wall primitive remains unchanged.
    // This isolated core is not called by drag preview or the live ball.
    public static class ShotIntentSolver
    {
        private sealed class Candidate
        {
            internal ShotSolution Solution;
            internal bool FirstFront, Legal, BelowCeiling;
            internal double Cost = double.PositiveInfinity, Change;
        }

        public static ShotSolution Solve(in ShotRequest request, Vector2 requestedAim, float speed, float cap,
            ShotPaceSettings pace, CourtDimensions court, IBallSimulationFactory factory, PhysicsParams physics,
            float footprintRadius, out ShotIntentInfo info)
        {
            var original = ShotSolver.Solve(request, speed, speed, cap, physics.Gravity, physics.BallRadius, court, factory, physics);
            info = new ShotIntentInfo(0, "inactive");
            if (!original.Solved) return original;
            var input = request;
            var from = input.ContactCenter;
            double t = Math.Max(0, Math.Min(1, ((double)input.P - .5) / .5));
            float distance = ShotPace.Requested(input.P, from, requestedAim, physics.BallRadius, pace).DistanceM;
            double span = (double)pace.FullM - pace.NearM;
            double w = span > 0 ? Math.Max(0, Math.Min(1, ((double)pace.FullM - distance) / span)) : 0;
            if (t == 0 || w == 0 || from.y >= court.BackOutLower || from.z <= physics.BallRadius) return original;

            float alpha = (float)(t * w);
            double xStar = Math.Max(court.Width / 10d, Math.Min(9 * court.Width / 10d,
                from.x + ((double)requestedAim.x - from.x) * (from.z + court.Length) / from.z));
            double zStar = court.Length * (.8 + .1 * t);
            double ceiling = from.y + (court.BackOutLower - from.y) * (1 - .5 * t);
            // Inward rounding enforces the total cap even when a float would round out of the domain.
            float xLow = Ceiling(Math.Max(physics.BallRadius, requestedAim.x - alpha * court.Width / 20d));
            float xHigh = Floor(Math.Min(court.Width - physics.BallRadius, requestedAim.x + alpha * court.Width / 20d));
            float yHigh = Math.Min(input.AimOnFrontWall.y, requestedAim.y);
            float legalLow = Ceiling((double)court.TinTop + physics.BallRadius);
            float yLow = Ceiling(yHigh + (legalLow - (double)yHigh) * alpha);
            if (xLow > xHigh || yLow > yHigh) return original;
            Vector2 Project(Vector2 aim) => new Vector2(Mathf.Clamp(aim.x, xLow, xHigh), Mathf.Clamp(aim.y, yLow, yHigh));

            int evaluations = 1;
            var baseline = Evaluate(original, from, requestedAim, xStar, zStar, ceiling, court, factory, physics, footprintRadius);
            var best = baseline;
            Candidate Probe(Vector2 aim)
            {
                evaluations++;
                aim = Project(aim);
                // Freeze the already composed speed, including any original feasibility raise.
                var solution = ShotSolver.Solve(new ShotRequest(from, aim, input.P), original.Speed, original.Speed, original.Speed,
                    physics.Gravity, physics.BallRadius, court, factory, physics);
                // A primitive feasibility lowering cannot bypass the intention search domain.
                if (solution.Solved && (solution.TargetUsed.x < xLow || solution.TargetUsed.x > xHigh || solution.TargetUsed.y < yLow || solution.TargetUsed.y > yHigh))
                    solution = new ShotSolution(false, Vector3.zero, 0, aim, false, false, "intent-domain-infeasible");
                return Evaluate(solution, from, requestedAim, xStar, zStar, ceiling, court, factory, physics, footprintRadius);
            }
            void Consider(Candidate candidate)
            {
                if (baseline.Legal && !candidate.Legal) return;
                if (Compare(candidate, best) < 0) best = candidate;
            }

            var xs = new[] { input.AimOnFrontWall.x, xLow, xHigh };
            var ys = new[] { yHigh, yLow, (yHigh + yLow) * .5f };
            for (int ix = 0; ix < 3; ix++)
                for (int iy = 0; iy < 3; iy++)
                {
                    if (ix == 0 && iy == 0) continue; // The retained original is evaluation one.
                    Consider(Probe(new Vector2(xs[ix], ys[iy])));
                }

            double trustX = court.Width / 40d, trustY = (court.FrontOutLower - court.TinTop) / 40d;
            for (int i = 0; i < 5; i++)
            {
                var origin = best;
                Vector2 at = Project(origin.Solution.TargetUsed);
                float dx = at.x + physics.BallRadius <= xHigh ? physics.BallRadius : -physics.BallRadius;
                float dy = at.y + physics.BallRadius <= yHigh ? physics.BallRadius : -physics.BallRadius;
                var xProbe = Probe(new Vector2(at.x + dx, at.y));
                var yProbe = Probe(new Vector2(at.x, at.y + dy));
                double Derivative(Candidate sample, double difference) => origin.Legal && sample.Legal && difference != 0
                    ? (sample.Cost - origin.Cost) / difference : 0;
                double gx = Derivative(xProbe, xProbe.Solution.TargetUsed.x - (double)at.x) * trustX;
                double gy = Derivative(yProbe, yProbe.Solution.TargetUsed.y - (double)at.y) * trustY;
                double norm = Math.Max(Math.Abs(gx), Math.Abs(gy));
                var proposal = norm > 0 ? new Vector2((float)(at.x - trustX * gx / norm), (float)(at.y - trustY * gy / norm)) : at;
                var refined = Probe(proposal);
                Consider(xProbe); Consider(yProbe); Consider(refined);
            }

            bool changed = Compare(best, baseline) < 0;
            info = new ShotIntentInfo(evaluations, changed ? "intent-drive" : "fallback-original-no-improvement");
            if (!changed) return original;
            var chosen = best.Solution;
            return new ShotSolution(chosen.Solved, chosen.Velocity, chosen.Speed, chosen.TargetUsed, original.SpeedRaised,
                chosen.TargetUsed.y < input.AimOnFrontWall.y, "intent-drive");
        }

        private static Candidate Evaluate(ShotSolution solution, Vector3 from, Vector2 requested, double xStar, double zStar,
            double ceiling, CourtDimensions court, IBallSimulationFactory factory, PhysicsParams physics, float footprintRadius)
        {
            var result = new Candidate { Solution = solution };
            double dx = (double)solution.TargetUsed.x - requested.x, dy = (double)solution.TargetUsed.y - requested.y;
            result.Change = dx * dx + dy * dy;
            if (!solution.Solved) return result;
            var rules = new PointRules(court, footprintRadius);
            rules.Begin(PlayerId.Human, PlayerId.Rival);
            bool firstContact = true;
            float apex = from.y;
            ContactEvent? second = null;
            using (var simulation = factory.Create(physics, court))
            {
                simulation.Reset(new BallState { Position = from, Velocity = solution.Velocity });
                var contacts = new List<ContactEvent>(4);
                for (int step = 1; step <= physics.StepHz * 10 && !rules.Closed; step++)
                {
                    contacts.Clear(); simulation.Step(contacts);
                    foreach (var hit in contacts)
                    {
                        if (rules.Closed) break;
                        if (!hit.Persistent && firstContact) { result.FirstFront = hit.Surface == SurfaceId.FrontWall; firstContact = false; }
                        if (!hit.Persistent && hit.Surface == SurfaceId.Floor && rules.FloorBounces == 1) second = hit;
                        rules.OnBallContact(hit);
                    }
                    if (!rules.Closed) apex = Math.Max(apex, simulation.State.Position.y);
                    if (!rules.Closed && simulation.State.Position.y > court.WallHeight) rules.OnOverTop();
                }
            }
            result.Legal = rules.Cause == PointEndCause.DoubleBounce && second.HasValue;
            result.BelowCeiling = apex <= ceiling;
            if (result.Legal)
                result.Cost = Math.Abs(second.Value.Point.x - xStar) / court.Width + Math.Abs(second.Value.Point.z - zStar) / court.Length
                    + Math.Abs(apex - ceiling) / court.BackOutLower;
            return result;
        }

        private static int Compare(Candidate a, Candidate b)
        {
            int c = b.Solution.Solved.CompareTo(a.Solution.Solved); if (c != 0) return c;
            c = b.FirstFront.CompareTo(a.FirstFront); if (c != 0) return c;
            c = b.Legal.CompareTo(a.Legal); if (c != 0) return c;
            c = b.BelowCeiling.CompareTo(a.BelowCeiling); if (c != 0) return c;
            c = a.Cost.CompareTo(b.Cost); if (c != 0) return c;
            c = a.Change.CompareTo(b.Change); if (c != 0) return c;
            c = a.Solution.TargetUsed.x.CompareTo(b.Solution.TargetUsed.x); if (c != 0) return c;
            return a.Solution.TargetUsed.y.CompareTo(b.Solution.TargetUsed.y);
        }

        private static float Ceiling(double value)
        {
            float rounded = (float)value;
            return rounded < value ? BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(rounded) + 1) : rounded;
        }
        private static float Floor(double value)
        {
            float rounded = (float)value;
            return rounded > value ? BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(rounded) - 1) : rounded;
        }
    }
}
