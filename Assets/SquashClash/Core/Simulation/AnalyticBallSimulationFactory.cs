using System;
using SquashClash.Core.Court;
namespace SquashClash.Core.Simulation
{
    public sealed class AnalyticBallSimulationFactory : IBallSimulationFactory
    {
        private readonly float bounceThreshold;
        public string Name => "P2-Analytic";
        public int LiveInstances { get; private set; }
        // The caller passes the same effective project setting used by P1.
        public AnalyticBallSimulationFactory(float bounceThreshold) { this.bounceThreshold = bounceThreshold; }
        public IBallSimulation Create(PhysicsParams p, CourtDimensions d)
        {
            var simulation = new AnalyticBallSimulation(p, d, bounceThreshold, () => LiveInstances--);
            LiveInstances++; return simulation;
        }
    }
}
