using System;
using UnityEngine;

namespace SquashClash.Core.Court
{
    public sealed class CourtDimensions : IEquatable<CourtDimensions>
    {
        public float Length = 9.75f, Width = 6.40f, WallHeight = 6f, LineWidth = 0.05f;
        public float FrontOutLower = 4.57f, ServiceLower = 1.78f, BackOutLower = 2.13f, TinTop = 0.48f;
        public float ShortLineNearBackEdgeFromBack = 4.26f, ServiceBoxInner = 1.60f;

        public static CourtDimensions Standard() => new CourtDimensions();
        public static CourtDimensions WithTinTop(float tinTop) => new CourtDimensions { TinTop = tinTop };

        public Vector2 ToSurfaceUV(SurfaceId surface, Vector3 point)
        {
            switch (surface)
            {
                case SurfaceId.FrontWall:
                case SurfaceId.BackWall: return new Vector2(point.x, point.y);
                case SurfaceId.LeftWall:
                case SurfaceId.RightWall: return new Vector2(point.z, point.y);
                case SurfaceId.Floor: return new Vector2(point.x, point.z);
                default: throw new ArgumentOutOfRangeException(nameof(surface));
            }
        }

        public static Vector3 InwardNormal(SurfaceId surface)
        {
            switch (surface)
            {
                case SurfaceId.FrontWall: return Vector3.forward;
                case SurfaceId.BackWall: return Vector3.back;
                case SurfaceId.LeftWall: return Vector3.right;
                case SurfaceId.RightWall: return Vector3.left;
                case SurfaceId.Floor: return Vector3.up;
                default: throw new ArgumentOutOfRangeException(nameof(surface));
            }
        }

        public bool Equals(CourtDimensions other) => other != null &&
            Length == other.Length && Width == other.Width && WallHeight == other.WallHeight &&
            LineWidth == other.LineWidth && FrontOutLower == other.FrontOutLower &&
            ServiceLower == other.ServiceLower && BackOutLower == other.BackOutLower && TinTop == other.TinTop &&
            ShortLineNearBackEdgeFromBack == other.ShortLineNearBackEdgeFromBack && ServiceBoxInner == other.ServiceBoxInner;

        public override bool Equals(object obj) => obj is CourtDimensions other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Length.GetHashCode();
                hash = hash * 31 + Width.GetHashCode();
                hash = hash * 31 + WallHeight.GetHashCode();
                hash = hash * 31 + LineWidth.GetHashCode();
                hash = hash * 31 + FrontOutLower.GetHashCode();
                hash = hash * 31 + ServiceLower.GetHashCode();
                hash = hash * 31 + BackOutLower.GetHashCode();
                hash = hash * 31 + TinTop.GetHashCode();
                hash = hash * 31 + ShortLineNearBackEdgeFromBack.GetHashCode();
                return hash * 31 + ServiceBoxInner.GetHashCode();
            }
        }
    }
}
