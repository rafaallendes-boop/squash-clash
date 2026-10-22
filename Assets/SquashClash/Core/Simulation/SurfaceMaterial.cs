using System;
namespace SquashClash.Core.Simulation
{
    public sealed class SurfaceMaterial : IEquatable<SurfaceMaterial>
    {
        public float Restitution, TangentialFriction;
        public bool Equals(SurfaceMaterial other) => other != null && Restitution == other.Restitution && TangentialFriction == other.TangentialFriction;
        public override bool Equals(object obj) => obj is SurfaceMaterial other && Equals(other);
        public override int GetHashCode() => unchecked(Restitution.GetHashCode() * 31 + TangentialFriction.GetHashCode());
    }
}
