using SquashClash.Core.Court;
using UnityEngine;
namespace SquashClash.Core.Simulation {
    public enum ContactTiming { StepOnly, Estimated, Exact }
    public readonly struct ContactEvent {
        public readonly SurfaceId Surface;
        public readonly Vector3 Point, Normal, ApproachVelocity;
        public readonly Vector2 UV;
        public readonly long Step;
        public readonly float TimeInStep;
        public readonly ContactTiming Timing;
        public readonly bool Persistent;
        public ContactEvent(SurfaceId surface, Vector3 point, Vector2 uv, Vector3 normal, long step,
            float timeInStep, ContactTiming timing, Vector3 approachVelocity, bool persistent) {
            Surface=surface; Point=point; UV=uv; Normal=normal; Step=step; TimeInStep=timeInStep;
            Timing=timing; ApproachVelocity=approachVelocity; Persistent=persistent;
        }
    }
}
