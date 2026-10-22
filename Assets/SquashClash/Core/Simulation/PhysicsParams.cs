using System;
namespace SquashClash.Core.Simulation
{
    public sealed class PhysicsParams : IEquatable<PhysicsParams>
    {
        public float Gravity, BallRadius = 0.020f, BallMass = 0.024f;
        public int StepHz, MaxImpactsPerStep;
        public SurfaceMaterial Floor, FrontWall, SideWalls, BackWall;
        public double StepSeconds => 1.0 / StepHz;
        public bool Equals(PhysicsParams other) => other != null && Gravity == other.Gravity && BallRadius == other.BallRadius &&
            BallMass == other.BallMass && StepHz == other.StepHz && MaxImpactsPerStep == other.MaxImpactsPerStep &&
            Equals(Floor, other.Floor) && Equals(FrontWall, other.FrontWall) && Equals(SideWalls, other.SideWalls) && Equals(BackWall, other.BackWall);
        public override bool Equals(object obj) => obj is PhysicsParams other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Gravity.GetHashCode();
                hash = hash * 31 + BallRadius.GetHashCode(); hash = hash * 31 + BallMass.GetHashCode();
                hash = hash * 31 + StepHz; hash = hash * 31 + MaxImpactsPerStep;
                foreach (var material in new[] { Floor, FrontWall, SideWalls, BackWall }) hash = hash * 31 + (material?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
