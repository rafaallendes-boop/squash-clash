using UnityEngine;
namespace SquashClash.Core.Simulation {
    public readonly struct LaunchSpec {
        public readonly Vector3 Position, Direction;
        public readonly float SpeedRealMps;
        public LaunchSpec(Vector3 position, Vector3 direction, float speedRealMps) {
            Position=position; Direction=direction; SpeedRealMps=speedRealMps;
        }
    }
}
