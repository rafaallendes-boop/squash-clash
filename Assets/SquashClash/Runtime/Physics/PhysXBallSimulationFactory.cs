using System;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Runtime.Physics
{
    public sealed class PhysXBallSimulationFactory : IBallSimulationFactory
    {
        // Unity gravity is global, including across separate factories and local scenes.
        private static int globalLive;
        private static float activeGravity;
        private readonly PhysXVariant variant;
        public string Name => "P1-PhysX";
        public int LiveInstances { get; private set; }
        public int PendingUnloads { get; private set; }
        public PhysXBallSimulationFactory(PhysXVariant variant = null) => this.variant = (variant ?? PhysXVariant.Baseline).Copy();
        public IBallSimulation Create(PhysicsParams p, CourtDimensions d)
        {
            if (!Application.isPlaying) throw new InvalidOperationException("P1 requires PlayMode collision callbacks.");
            if (p == null || d == null) throw new ArgumentNullException(p == null ? nameof(p) : nameof(d));
            if (p.StepHz <= 0 || p.Gravity <= 0 || float.IsNaN(p.Gravity) || float.IsInfinity(p.Gravity) ||
                p.BallRadius <= 0 || p.BallMass <= 0 || p.Floor == null || p.FrontWall == null || p.SideWalls == null || p.BackWall == null)
                throw new ArgumentException("Incomplete physics parameters.");
            if (globalLive > 0 && (activeGravity != p.Gravity || UnityEngine.Physics.gravity != Vector3.down * activeGravity))
                throw new InvalidOperationException("Dispose all live P1 instances before changing global gravity.");
            activeGravity = p.Gravity;
            UnityEngine.Physics.gravity = Vector3.down * p.Gravity;
            var sim = new PhysXBallSimulation(p, d, variant,
                () => { LiveInstances--; globalLive--; }, () => PendingUnloads++, () => PendingUnloads--);
            LiveInstances++; globalLive++;
            return sim;
        }
    }
}
