using SquashClash.Core.Court;
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

    // Kept separate from the physical front-wall primitive. Initial TDD seam delegates unchanged.
    public static class ShotIntentSolver
    {
        public static ShotSolution Solve(in ShotRequest request, Vector2 requestedAim, float speed, float cap,
            ShotPaceSettings pace, CourtDimensions court, IBallSimulationFactory factory, PhysicsParams physics,
            float footprintRadius, out ShotIntentInfo info)
        {
            info = new ShotIntentInfo(0, "unchanged");
            return ShotSolver.Solve(request, speed, speed, cap, physics.Gravity, physics.BallRadius, court, factory, physics);
        }
    }
}
