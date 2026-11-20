using System;
using SquashClash.Core.Court;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    // A point on a side wall (two-wall shot, design 2026-11-20-boast-predeclaration): the wall (AX-1: RightWall x = 0, LeftWall
    // x = Width) and the point's surface coordinates (z, y). default is "no side aim": every front-wall path ignores it.
    public readonly struct SideAim : IEquatable<SideAim>
    {
        public readonly bool Exists;
        public readonly SurfaceId Wall;
        public readonly float Z, Y;
        public SideAim(SurfaceId wall, float z, float y)
        {
            if (wall != SurfaceId.LeftWall && wall != SurfaceId.RightWall) throw new ArgumentOutOfRangeException(nameof(wall));
            Exists = true; Wall = wall; Z = z; Y = y;
        }
        // The wall surface coordinates, as ContactEvent.UV reports them for a side wall.
        public Vector2 UV => new Vector2(Z, Y);
        // The ball centre touching the wall at this point.
        public Vector3 Center(CourtDimensions court, float radius) => new Vector3(Wall == SurfaceId.RightWall ? radius : court.Width - radius, Y, Z);
        public SideAim With(float z, float y) => new SideAim(Wall, z, y);
        // The side wall nearer to a contact x (ties go to the right wall, x = 0).
        public static SurfaceId NearWall(float x, CourtDimensions court) => x <= court.Width * .5f ? SurfaceId.RightWall : SurfaceId.LeftWall;
        // Lower edge of the side out line at depth z (the line runs from the front out line to the back out line).
        public static float OutEdge(CourtDimensions court, float z) => Mathf.Lerp(court.FrontOutLower, court.BackOutLower, z / court.Length);
        public bool Equals(SideAim other) => Exists == other.Exists && Wall == other.Wall && Z.Equals(other.Z) && Y.Equals(other.Y);
        public override bool Equals(object obj) => obj is SideAim other && Equals(other);
        public override int GetHashCode() => Exists ? ((int)Wall * 397 ^ Z.GetHashCode()) * 397 ^ Y.GetHashCode() : 0;
        public override string ToString() => Exists ? Wall + "(" + Z + "," + Y + ")" : "none";
    }
}
