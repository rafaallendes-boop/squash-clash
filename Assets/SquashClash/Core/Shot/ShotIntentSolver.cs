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

    // Release-time intention search shared by the human, the rival, the headless rally and the AI forecast
    // (CausalQuality.Solve and RallyContact.SolveIntention). The physical front-wall primitive remains unchanged and
    // there is no second ball authority: this only chooses the effective aim for a velocity the caller applies.
    // It never runs during the drag; the reticle shows the chosen TargetUsed after release.
    public static class ShotIntentSolver
    {
        private sealed class Candidate
        {
            internal ShotSolution Solution;
            internal bool FirstFront, Legal, BelowCeiling, CrossesMiddle;
            internal double Cost = double.PositiveInfinity, Change, Apex = double.NaN;
        }
        // Fixed budget of P2 trajectories per search, the original included. A causal height lift adds at most two
        // verification trajectories (the lifted shot and the unmodified causal shot), never more search.
        public const int Budget = 24;
        public const float DefaultFootprintRadius = .02f;

        public static ShotSolution Solve(in ShotRequest request, Vector2 requestedAim, float speed, float cap,
            ShotPaceSettings pace, CourtDimensions court, IBallSimulationFactory factory, PhysicsParams physics,
            float footprintRadius, out ShotIntentInfo info)
        {
            var original = ShotSolver.Solve(request, speed, speed, cap, physics.Gravity, physics.BallRadius, court, factory, physics);
            info = new ShotIntentInfo(0, "inactive");
            if (!original.Solved) return original;
            // Option B (deviation from the archived design, decided 18 nov): the causal layer's height lift (timing,
            // stretch, jam, fatigue) is kept. The search runs on the shot the player intended (requested height, the
            // composed speed) and the lift is then added to the chosen aim, so a late or tired stroke still rises.
            float lift = request.AimOnFrontWall.y - requestedAim.y;
            if (!(lift > 0)) return Search(request, original, requestedAim, pace, court, factory, physics, footprintRadius, out info);
            if (!Active(request.P, request.ContactCenter, requestedAim, pace, court, physics)) return original;
            var plainRequest = new ShotRequest(request.ContactCenter, new Vector2(request.AimOnFrontWall.x, requestedAim.y), request.P);
            var plainOriginal = ShotSolver.Solve(plainRequest, speed, speed, cap, physics.Gravity, physics.BallRadius, court, factory, physics);
            if (!plainOriginal.Solved) return original;
            var plain = Search(plainRequest, plainOriginal, requestedAim, pace, court, factory, physics, footprintRadius, out var plainInfo);
            info = plainInfo;
            if (plain.Reason != "intent-drive") return original;
            var target = new Vector2(plain.TargetUsed.x, plain.TargetUsed.y + lift);
            var lifted = ShotSolver.Solve(new ShotRequest(request.ContactCenter, target, request.P), plain.Speed, plain.Speed, plain.Speed,
                physics.Gravity, physics.BallRadius, court, factory, physics);
            info = new ShotIntentInfo(plainInfo.Evaluations + 2, "intent-drive");
            if (!lifted.Solved || lifted.TargetUsed.x != target.x || lifted.TargetUsed.y != target.y) { info = new ShotIntentInfo(plainInfo.Evaluations + 1, "fallback-causal-lift-unsolved"); return original; }
            // As in the search, a legal causal shot is never replaced by a fault.
            var from = request.ContactCenter;
            bool liftedLegal = Evaluate(lifted, from, requestedAim, 0, 0, 0, court, factory, physics, footprintRadius).Legal;
            if (!liftedLegal && Evaluate(original, from, requestedAim, 0, 0, 0, court, factory, physics, footprintRadius).Legal)
            { info = new ShotIntentInfo(plainInfo.Evaluations + 2, "fallback-causal-lift-fault"); return original; }
            return new ShotSolution(true, lifted.Velocity, lifted.Speed, lifted.TargetUsed, original.SpeedRaised,
                lifted.TargetUsed.y < request.AimOnFrontWall.y, "intent-drive");
        }

        // Policy zone: above medium power and near the front wall (existing pace keys), contact below the back out line.
        public static bool Active(float power, Vector3 from, Vector2 requestedAim, ShotPaceSettings pace, CourtDimensions court, PhysicsParams physics)
            => Active(power, from, requestedAim, pace, court, physics, out _, out _);
        private static bool Active(float power, Vector3 from, Vector2 requestedAim, ShotPaceSettings pace, CourtDimensions court, PhysicsParams physics,
            out double t, out double w)
        {
            t = Math.Max(0, Math.Min(1, ((double)power - .5) / .5));
            float distance = ShotPace.Requested(power, from, requestedAim, physics.BallRadius, pace).DistanceM;
            double span = (double)pace.FullM - pace.NearM;
            w = span > 0 ? Math.Max(0, Math.Min(1, ((double)pace.FullM - distance) / span)) : 0;
            return !(t == 0 || w == 0 || from.y >= court.BackOutLower || from.z <= physics.BallRadius);
        }

        private static ShotSolution Search(in ShotRequest request, ShotSolution original, Vector2 requestedAim,
            ShotPaceSettings pace, CourtDimensions court, IBallSimulationFactory factory, PhysicsParams physics,
            float footprintRadius, out ShotIntentInfo info)
        {
            info = new ShotIntentInfo(0, "inactive");
            var input = request;
            var from = input.ContactCenter;
            if (!Active(input.P, from, requestedAim, pace, court, physics, out double t, out double w)) return original;

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

            // Refinement (revision 02): probes are taken once per origin and reused while the origin stands, a stalled
            // step is halved instead of repeating identical trajectories, and a proposal the linear apex model places
            // above the ceiling is lowered along the apex level of its origin. Runs until the fixed budget is spent.
            double trustX = court.Width / 40d, trustY = (court.FrontOutLower - court.TinTop) / 40d;
            Candidate origin = null, xProbe = null, yProbe = null;
            Vector2 at = default;
            double scale = 1;
            while (true)
            {
                bool fresh = !ReferenceEquals(origin, best);
                if (evaluations + (fresh ? 3 : 1) > Budget) break;
                if (fresh)
                {
                    origin = best; scale = 1;
                    at = Project(origin.Solution.TargetUsed);
                    float dx = at.x + physics.BallRadius <= xHigh ? physics.BallRadius : -physics.BallRadius;
                    float dy = at.y + physics.BallRadius <= yHigh ? physics.BallRadius : -physics.BallRadius;
                    xProbe = Probe(new Vector2(at.x + dx, at.y));
                    yProbe = Probe(new Vector2(at.x, at.y + dy));
                }
                else scale *= .5;
                double ddx = xProbe.Solution.TargetUsed.x - (double)at.x, ddy = yProbe.Solution.TargetUsed.y - (double)at.y;
                double Derivative(Candidate sample, double difference) => origin.Legal && sample.Legal && difference != 0
                    ? (sample.Cost - origin.Cost) / difference : 0;
                double stepX = trustX * scale, stepY = trustY * scale;
                double gx = Derivative(xProbe, ddx) * stepX;
                double gy = Derivative(yProbe, ddy) * stepY;
                double norm = Math.Max(Math.Abs(gx), Math.Abs(gy));
                double px = at.x, py = at.y;
                if (norm > 0) { px -= stepX * gx / norm; py -= stepY * gy / norm; }
                if (origin.Solution.Solved && xProbe.Solution.Solved && yProbe.Solution.Solved && ddx != 0 && ddy != 0)
                {
                    double ax = (xProbe.Apex - origin.Apex) / ddx, ay = (yProbe.Apex - origin.Apex) / ddy;
                    double predicted = origin.Apex + ax * (px - at.x) + ay * (py - at.y);
                    if (ay > 0 && predicted > ceiling)
                    {
                        double level = origin.BelowCeiling ? origin.Apex : ceiling;
                        py = Math.Max(at.y - stepY, Math.Min(at.y + stepY, at.y + (level - origin.Apex - ax * (px - at.x)) / ay));
                    }
                }
                var proposal = Project(new Vector2((float)px, (float)py));
                if (proposal.x == at.x && proposal.y == at.y) break; // No movement left: further trajectories would repeat the origin.
                var refined = Probe(proposal);
                if (fresh) { Consider(xProbe); Consider(yProbe); }
                Consider(refined);
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
            ContactEvent? first = null, second = null;
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
                        if (!hit.Persistent && hit.Surface == SurfaceId.Floor)
                        {
                            if (rules.FloorBounces == 0) first = hit;
                            else if (rules.FloorBounces == 1) second = hit;
                        }
                        rules.OnBallContact(hit);
                    }
                    if (!rules.Closed) apex = Math.Max(apex, simulation.State.Position.y);
                    if (!rules.Closed && simulation.State.Position.y > court.WallHeight) rules.OnOverTop();
                }
            }
            result.Apex = apex;
            result.Legal = rules.Cause == PointEndCause.DoubleBounce && first.HasValue && second.HasValue;
            result.BelowCeiling = apex <= ceiling;
            if (result.Legal)
            {
                double half = court.Width / 2d;
                bool cross = (from.x - half) * (xStar - half) < 0;
                bool right = from.x < half;
                double x1 = first.Value.Point.x, x2 = second.Value.Point.x;
                double own1 = right ? x1 : court.Width - x1;
                double own2 = right ? x2 : court.Width - x2;
                double opposite2 = right ? court.Width - x2 : x2;
                // Same bounce geometry as the witness, rather than a point target for x2 alone.
                double lateral = cross ? opposite2 : own1 + own2;
                result.CrossesMiddle = !cross || (from.x - half) * (x2 - half) < 0;
                result.Cost = lateral / court.Width + Math.Abs(second.Value.Point.z - zStar) / court.Length
                    + Math.Abs(apex - ceiling) / court.BackOutLower;
            }
            return result;
        }

        private static int Compare(Candidate a, Candidate b)
        {
            int c = b.Solution.Solved.CompareTo(a.Solution.Solved); if (c != 0) return c;
            c = b.FirstFront.CompareTo(a.FirstFront); if (c != 0) return c;
            c = b.Legal.CompareTo(a.Legal); if (c != 0) return c;
            c = b.BelowCeiling.CompareTo(a.BelowCeiling); if (c != 0) return c;
            c = b.CrossesMiddle.CompareTo(a.CrossesMiddle); if (c != 0) return c;
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
