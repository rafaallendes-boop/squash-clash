using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
namespace SquashClash.Core.Simulation {
    public sealed class BackendCapabilities { public ContactTiming Timing; public bool ImpactCapConfigurable, RunsInEditMode; }
    public interface IBallSimulation : IDisposable {
        BackendCapabilities Capabilities { get; }
        void Reset(in BallState initial);
        BallState State { get; }
        void Step(List<ContactEvent> contactsThisStep);
    }
    public interface IBallSimulationFactory {
        string Name { get; }
        IBallSimulation Create(PhysicsParams p, CourtDimensions d);
        int LiveInstances { get; }
    }
}
