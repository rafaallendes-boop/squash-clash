using UnityEngine;
namespace SquashClash.Core.Simulation
{
    public static class Launcher
    {
        public static BallState ToState(in LaunchSpec spec, double launchScale) => new BallState
        {
            Position = spec.Position,
            Velocity = spec.Direction.normalized * (float)(spec.SpeedRealMps * launchScale),
            Step = 0
        };
    }
}
