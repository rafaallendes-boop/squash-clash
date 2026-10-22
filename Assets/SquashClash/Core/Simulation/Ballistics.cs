using System;
using SquashClash.Core.Court;
using UnityEngine;
namespace SquashClash.Core.Simulation
{
    public static class Ballistics
    {
        public static Vector3 PositionAt(in BallState s, float gravity, double t) => new Vector3(
            (float)(s.Position.x + (double)s.Velocity.x * t),
            (float)(s.Position.y + (double)s.Velocity.y * t - 0.5 * gravity * t * t),
            (float)(s.Position.z + (double)s.Velocity.z * t));
        public static Vector3 VelocityAt(in BallState s, float gravity, double t) =>
            new Vector3(s.Velocity.x, (float)(s.Velocity.y - gravity * t), s.Velocity.z);
        public static Vector3 SolveCenterTo(Vector3 from, Vector3 center, float horizontalSpeed, float gravity)
        {
            double dx = (double)center.x - from.x, dz = (double)center.z - from.z;
            double distance = Math.Sqrt(dx * dx + dz * dz);
            if (distance <= 0 || horizontalSpeed <= 0) throw new ArgumentOutOfRangeException(nameof(horizontalSpeed));
            double t = distance / horizontalSpeed;
            return new Vector3((float)(dx / t), (float)(((double)center.y - from.y + 0.5 * gravity * t * t) / t), (float)(dz / t));
        }
        public static Vector3 CenterForContact(SurfaceId s, Vector3 contactPoint, float radius) => contactPoint + CourtDimensions.InwardNormal(s) * radius;
        public static bool FirstPlaneHit(in BallState s, float gravity, float radius, CourtDimensions d, double maxT,
            out SurfaceId surface, out double t)
        {
            var start = s;
            double best = double.PositiveInfinity;
            var face = SurfaceId.Floor;
            void Consider(double hit, SurfaceId candidate)
            {
                if (double.IsNaN(hit) || hit < 0 || hit > maxT || hit >= best) return;
                var p = PositionAt(start, gravity, hit);
                if (Vector3.Dot(VelocityAt(start, gravity, hit), CourtDimensions.InwardNormal(candidate)) >= 0) return;
                bool finite = candidate == SurfaceId.Floor ? p.x >= 0 && p.x <= d.Width && p.z >= 0 && p.z <= d.Length :
                    (p.y >= 0 && p.y <= d.WallHeight &&
                     (candidate == SurfaceId.FrontWall || candidate == SurfaceId.BackWall ? p.x >= 0 && p.x <= d.Width : p.z >= 0 && p.z <= d.Length));
                if (finite) { best = hit; face = candidate; }
            }
            if (s.Velocity.x < 0) Consider(((double)radius - s.Position.x) / s.Velocity.x, SurfaceId.RightWall);
            if (s.Velocity.x > 0) Consider(((double)d.Width - radius - s.Position.x) / s.Velocity.x, SurfaceId.LeftWall);
            if (s.Velocity.z < 0) Consider(((double)radius - s.Position.z) / s.Velocity.z, SurfaceId.FrontWall);
            if (s.Velocity.z > 0) Consider(((double)d.Length - radius - s.Position.z) / s.Velocity.z, SurfaceId.BackWall);
            double height = (double)s.Position.y - radius;
            if (gravity > 0)
            {
                double discriminant = (double)s.Velocity.y * s.Velocity.y + 2.0 * gravity * height;
                if (discriminant >= 0) Consider((s.Velocity.y + Math.Sqrt(discriminant)) / gravity, SurfaceId.Floor);
            }
            else if (s.Velocity.y < 0) Consider(-height / s.Velocity.y, SurfaceId.Floor);
            surface = face; t = best;
            return !double.IsPositiveInfinity(best);
        }
    }
}
