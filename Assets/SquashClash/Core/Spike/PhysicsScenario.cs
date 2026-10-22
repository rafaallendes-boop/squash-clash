using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Spike
{
    public sealed class PhysicsScenario
    {
        public string Id, Description;
        public BallState Start;
        public int MaxSteps;
        public StopCondition Stop;
        public Vector3? ContactPoint;
        public SurfaceId? ContactSurface;
        public LineId? Line;
        public bool? ExpectTouch;
        public int MinContactsBeforeStop;
    }
}
