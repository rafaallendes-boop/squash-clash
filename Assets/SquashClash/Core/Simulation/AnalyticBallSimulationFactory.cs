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
        public AnalyticBallSimulationFactory(float bounceThreshold)
        {
            if(!Finite(bounceThreshold)||bounceThreshold<0)throw new ArgumentOutOfRangeException(nameof(bounceThreshold));
            this.bounceThreshold = bounceThreshold;
        }
        public IBallSimulation Create(PhysicsParams p, CourtDimensions d)
        {
            if(p==null||d==null)throw new ArgumentNullException(p==null?nameof(p):nameof(d));
            if(p.StepHz<=0||p.MaxImpactsPerStep<=0||!Finite(p.Gravity)||p.Gravity<0||!Finite(p.BallRadius)||p.BallRadius<=0||!Finite(p.BallMass)||p.BallMass<=0||
                !Valid(p.Floor)||!Valid(p.FrontWall)||!Valid(p.SideWalls)||!Valid(p.BackWall)||
                !Finite(d.Width)||!Finite(d.Length)||!Finite(d.WallHeight)||d.Width<=2*p.BallRadius||d.Length<=2*p.BallRadius||d.WallHeight<=0)
                throw new ArgumentException("Incomplete or invalid analytic physics configuration.");
            var simulation = new AnalyticBallSimulation(p, d, bounceThreshold, () => LiveInstances--);
            LiveInstances++; return simulation;
        }
        private static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
        private static bool Valid(SurfaceMaterial m)=>m!=null&&Finite(m.Restitution)&&m.Restitution>=0&&m.Restitution<=1&&Finite(m.TangentialFriction)&&m.TangentialFriction>=0;
    }
}
