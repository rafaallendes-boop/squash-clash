using UnityEngine;
namespace SquashClash.Core.Court
{
    public readonly struct LineBand
    {
        public readonly LineId Id;
        public readonly SurfaceId Surface;
        public readonly Vector2 A, B, Growth;
        public readonly float Width;
        public LineBand(LineId id, SurfaceId surface, Vector2 a, Vector2 b, Vector2 growth, float width)
        { Id = id; Surface = surface; A = a; B = b; Growth = growth; Width = width; }
    }
}
