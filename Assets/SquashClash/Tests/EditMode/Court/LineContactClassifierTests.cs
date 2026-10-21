using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using UnityEngine;

namespace SquashClash.Core.Tests.Court
{
    public sealed class LineContactClassifierTests
    {
        [TestCase(LineId.FrontOut, 3.2f, 4.559f, 0.010f, false)]
        [TestCase(LineId.FrontOut, 3.2f, 4.560f, 0.010f, true)]
        [TestCase(LineId.FrontOut, 3.2f, 4.600f, 0f, true)]
        [TestCase(LineId.FrontOut, 3.2f, 4.630f, 0.010f, true)]
        [TestCase(LineId.Tin, 3.2f, 0.490f, 0.010f, true)]
        [TestCase(LineId.Tin, 3.2f, 0.4911f, 0.010f, false)]
        [TestCase(LineId.Service, 3.2f, 1.770f, 0.010f, true)]
        [TestCase(LineId.Service, 3.2f, 1.8405f, 0.010f, false)]
        [TestCase(LineId.Service, 3.2f, 4.560f, 0.010f, false)]
        [TestCase(LineId.ShortLine, 1f, 5.430f, 0.010f, true)]
        [TestCase(LineId.ShortLine, 1f, 5.4289f, 0.010f, false)]
        [TestCase(LineId.ShortLine, 1f, 5.5011f, 0.010f, false)]
        [TestCase(LineId.BackOut, 3.2f, 2.120f, 0.010f, true)]
        [TestCase(LineId.BackOut, 3.2f, 2.1199f, 0.010f, false)]
        [TestCase(LineId.HalfCourt, 3.165f, 8f, 0.010f, true)]
        [TestCase(LineId.HalfCourt, 3.1649f, 8f, 0.010f, false)]
        [TestCase(LineId.RightBoxSide, 1.590f, 6f, 0.010f, true)]
        [TestCase(LineId.RightBoxSide, 1.5899f, 6f, 0.010f, false)]
        [TestCase(LineId.RightBoxBack, 1f, 7.150f, 0.010f, true)]
        [TestCase(LineId.RightBoxBack, 1f, 7.1501f, 0.010f, false)]
        [TestCase(LineId.LeftBoxSide, 4.740f, 6f, 0.010f, true)]
        [TestCase(LineId.LeftBoxSide, 4.7399f, 6f, 0.010f, false)]
        [TestCase(LineId.LeftBoxBack, 5f, 7.150f, 0.010f, true)]
        [TestCase(LineId.LeftBoxBack, 5f, 7.1501f, 0.010f, false)]
        public void Footprint_MatchesApprovedEdges(LineId id, float u, float v, float radius, bool expected)
        {
            var band = CourtLines.Build(CourtDimensions.Standard()).Single(b => b.Id == id);
            Assert.That(LineContactClassifier.Touches(in band, new Vector2(u, v), radius), Is.EqualTo(expected));
        }

        [TestCase(LineId.LeftSideOut, 4.875f, 0.0100f, true)]
        [TestCase(LineId.LeftSideOut, 4.875f, 0.0101f, false)]
        [TestCase(LineId.LeftSideOut, 0f, 0.0100f, true)]
        [TestCase(LineId.RightSideOut, 4.875f, 0.0100f, true)]
        [TestCase(LineId.RightSideOut, 4.875f, 0.0101f, false)]
        [TestCase(LineId.RightSideOut, 9.75f, 0.0100f, true)]
        public void DiagonalFootprint_UsesPerpendicularDistanceAndEndpoints(LineId id, float z, float offset, bool expected)
        {
            var band = CourtLines.Build(CourtDimensions.Standard()).Single(b => b.Id == id);
            var border = new Vector2(z, 4.57f - 2.44f * z / 9.75f);
            var independentNormal = new Vector2(2.44f, 9.75f).normalized;
            var uv = border - independentNormal * offset;
            Assert.That(LineContactClassifier.Touches(in band, uv, 0.010f), Is.EqualTo(expected));
        }

        [TestCase(-0.007f, 5.433f, true)]
        [TestCase(-0.008f, 5.432f, false)]
        [TestCase(6.407f, 5.433f, true)]
        [TestCase(6.408f, 5.432f, false)]
        public void CornerFootprint_UsesCircleRatherThanExpandedRectangle(float u, float v, bool expected)
        {
            var band = CourtLines.Build(CourtDimensions.Standard()).Single(b => b.Id == LineId.ShortLine);
            Assert.That(LineContactClassifier.Touches(in band, new Vector2(u, v), 0.010f), Is.EqualTo(expected));
        }

        [Test]
        public void Overlapping_FiltersSurfaceAndReplacesPreviousResults()
        {
            var bands = CourtLines.Build(CourtDimensions.Standard());
            var result = new List<LineId> { LineId.Tin };
            LineContactClassifier.Overlapping(bands, SurfaceId.FrontWall, new Vector2(3.2f, 4.56f), 0.010f, result);
            Assert.That(result, Is.EquivalentTo(new[] { LineId.FrontOut }));
            LineContactClassifier.Overlapping(bands, SurfaceId.Floor, new Vector2(3.2f, 5.49f), 0.010f, result);
            Assert.That(result, Is.EquivalentTo(new[] { LineId.ShortLine, LineId.HalfCourt }));
            LineContactClassifier.Overlapping(bands, SurfaceId.FrontWall, Vector2.zero, 0, result);
            Assert.That(result, Is.Empty);
        }
    }
}
