using System;
using System.Collections.Generic;
using UnityEngine;

namespace SquashClash.Core.Court
{
    // Geometry only: rule consequences depend on phase and are outside First Ball I2.
    public static class LineContactClassifier
    {
        public const float EdgeEpsilon = 1e-6f;

        public static bool Touches(in LineBand band, Vector2 uv, float footprintRadius)
        {
            if (footprintRadius < 0 || float.IsNaN(footprintRadius) || float.IsInfinity(footprintRadius))
                throw new ArgumentOutOfRangeException(nameof(footprintRadius));
            var edge = band.B - band.A;
            var length = edge.magnitude;
            var tangent = edge / length;
            var relative = uv - band.A;
            var along = Mathf.Clamp(Vector2.Dot(relative, tangent), 0, length);
            var across = Mathf.Clamp(Vector2.Dot(relative, band.Growth), 0, band.Width);
            var nearest = band.A + tangent * along + band.Growth * across;
            var radius = footprintRadius + EdgeEpsilon;
            return (uv - nearest).sqrMagnitude <= radius * radius;
        }

        public static void Overlapping(IReadOnlyList<LineBand> bands, SurfaceId surface, Vector2 uv, float r, List<LineId> result)
        {
            result.Clear();
            for (var i = 0; i < bands.Count; i++)
            {
                var band = bands[i];
                if (band.Surface == surface && Touches(in band, uv, r)) result.Add(band.Id);
            }
        }
    }
}
