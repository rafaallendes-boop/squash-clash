using System.Collections.Generic;
namespace SquashClash.Core.Simulation {
    public enum StopCondition { SecondFloorBounce, MaxSteps }
    public enum TrajectoryExit { None, OverTop }
    public sealed class Trajectory {
        public readonly List<BallState> States=new List<BallState>();
        public readonly List<ContactEvent> Contacts=new List<ContactEvent>();
        public TrajectoryExit Exit;
    }
}
