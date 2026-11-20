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

    // Human-only side-wall guard for a deep drive struck in front of the short line (design 2026-11-20-front-deep-drive).
    // Called by the human ShotController only, after CausalQuality.Solve; the rival, RallySim and the AI forecast never call it.
    public static class FrontDeepDrive
    {
        public const string ReasonActed = "front-deep";
        public const float MinAimHeight = .80f;
        public const int BisectionSteps = 8;

        public static bool Active(float power, Vector3 from, Vector2 requestedAim, CourtDimensions court)
            => false;

        public static bool Deep(Vector3 from, Vector3 velocity, CourtDimensions court, IBallSimulationFactory factory, PhysicsParams physics, float footprintRadius)
            => false;

        public static ShotSolution Apply(Vector3 from, Vector2 requestedAim, float power, in ShotSolution solution, CourtDimensions court,
            IBallSimulationFactory factory, PhysicsParams physics, float footprintRadius, out FrontDeepInfo info)
        {
            info = new FrontDeepInfo("inactive", 0, 0);
            return solution;
        }
    }
}
