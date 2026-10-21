using System;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using UnityEngine;

namespace SquashClash.Core.Tests.Court
{
    public sealed class CourtGeometryTests
    {
        [Test]
        public void Standard_UsesVerifiedWsfValues()
        {
            var d = CourtDimensions.Standard();
            Assert.That(d.Length, Is.EqualTo(9.75f));
            Assert.That(d.Width, Is.EqualTo(6.40f));
            Assert.That(d.LineWidth, Is.EqualTo(0.05f));
            Assert.That(d.FrontOutLower, Is.EqualTo(4.57f));
            Assert.That(d.ServiceLower, Is.EqualTo(1.78f));
            Assert.That(d.BackOutLower, Is.EqualTo(2.13f));
            Assert.That(d.TinTop, Is.EqualTo(0.48f));
            Assert.That(CourtDimensions.WithTinTop(0.43f).TinTop, Is.EqualTo(0.43f));
        }

        [Test]
        public void Build_PlacesAllBandsOnApprovedReferenceEdges()
        {
            var bands = CourtLines.Build(CourtDimensions.Standard()).ToDictionary(b => b.Id);
            Check(bands[LineId.FrontOut], SurfaceId.FrontWall, 0, 4.57f, 6.4f, 4.57f, 0, 1);
            Check(bands[LineId.Service], SurfaceId.FrontWall, 0, 1.78f, 6.4f, 1.78f, 0, 1);
            Check(bands[LineId.Tin], SurfaceId.FrontWall, 0, 0.43f, 6.4f, 0.43f, 0, 1);
            Check(bands[LineId.BackOut], SurfaceId.BackWall, 0, 2.13f, 6.4f, 2.13f, 0, 1);
            Check(bands[LineId.ShortLine], SurfaceId.Floor, 0, 5.44f, 6.4f, 5.44f, 0, 1);
            Check(bands[LineId.HalfCourt], SurfaceId.Floor, 3.175f, 5.49f, 3.175f, 9.75f, 1, 0);
            Check(bands[LineId.RightBoxSide], SurfaceId.Floor, 1.6f, 5.49f, 1.6f, 7.14f, 1, 0);
            Check(bands[LineId.RightBoxBack], SurfaceId.Floor, 0, 7.09f, 1.65f, 7.09f, 0, 1);
            Check(bands[LineId.LeftBoxSide], SurfaceId.Floor, 4.75f, 5.49f, 4.75f, 7.14f, 1, 0);
            Check(bands[LineId.LeftBoxBack], SurfaceId.Floor, 4.75f, 7.09f, 6.4f, 7.09f, 0, 1);
            foreach (var id in new[] { LineId.LeftSideOut, LineId.RightSideOut })
            {
                Assert.That(bands[id].A, Is.EqualTo(new Vector2(0, 4.57f)));
                Assert.That(bands[id].B, Is.EqualTo(new Vector2(9.75f, 2.13f)));
                Assert.That(bands[id].Growth.y, Is.GreaterThan(0));
            }
            var psa = CourtLines.Build(CourtDimensions.WithTinTop(0.43f)).Single(b => b.Id == LineId.Tin);
            Assert.That(psa.A.y + psa.Width, Is.EqualTo(0.43f).Within(1e-5));
        }

        [Test]
        public void ShortLine_SpansFrom5_44To5_49FromFrontWall()
        {
            var band = CourtLines.Build(CourtDimensions.Standard()).Single(b => b.Id == LineId.ShortLine);
            Assert.That(band.A.y, Is.EqualTo(5.44f).Within(1e-5));
            Assert.That(band.A.y + band.Width, Is.EqualTo(5.49f).Within(1e-5));
        }

        [Test]
        public void Build_Returns12Bands_WithUnitPerpendicularGrowth()
        {
            var bands = CourtLines.Build(CourtDimensions.Standard());
            Assert.That(bands.Count, Is.EqualTo(12));
            Assert.That(bands.Select(b => b.Id).Distinct().Count(), Is.EqualTo(12));
            foreach (var b in bands)
            {
                Assert.That(b.Width, Is.EqualTo(0.05f));
                Assert.That(b.Growth.magnitude, Is.EqualTo(1).Within(1e-5));
                Assert.That(Vector2.Dot(b.Growth, b.B - b.A), Is.EqualTo(0).Within(1e-5));
            }
        }

        [Test]
        public void SideOut_PerpendicularWidthIs50mm()
        {
            var band = CourtLines.Build(CourtDimensions.Standard()).Single(b => b.Id == LineId.LeftSideOut);
            var mid = new Vector2(4.875f, 3.35f);
            var outer = mid + band.Growth * band.Width;
            var edge = band.B - band.A;
            var distance = Mathf.Abs(edge.x * (outer.y - band.A.y) - edge.y * (outer.x - band.A.x)) / edge.magnitude;
            Assert.That(distance, Is.EqualTo(0.050f).Within(1e-5));
        }

        [TestCase(SurfaceId.FrontWall, 1, 2, 0, 0, 1)]
        [TestCase(SurfaceId.BackWall, 1, 2, 0, 0, -1)]
        [TestCase(SurfaceId.LeftWall, 3, 2, -1, 0, 0)]
        [TestCase(SurfaceId.RightWall, 3, 2, 1, 0, 0)]
        [TestCase(SurfaceId.Floor, 1, 3, 0, 1, 0)]
        public void ToSurfaceUV_FollowsPlanConvention(SurfaceId surface, float u, float v, float nx, float ny, float nz)
        {
            Assert.That(CourtDimensions.Standard().ToSurfaceUV(surface, new Vector3(1, 2, 3)), Is.EqualTo(new Vector2(u, v)));
            Assert.That(CourtDimensions.InwardNormal(surface), Is.EqualTo(new Vector3(nx, ny, nz)));
        }

        [Test]
        public void Equals_RecognizesEquivalentGeometryAndTinChanges()
        {
            var standard = CourtDimensions.Standard();
            Assert.That(standard.Equals(CourtDimensions.Standard()), Is.True);
            Assert.That(standard.Equals(CourtDimensions.WithTinTop(0.43f)), Is.False);
        }

        private static void Check(LineBand b, SurfaceId s, float ax, float ay, float bx, float by, float gx, float gy)
        {
            Assert.That(b.Surface, Is.EqualTo(s));
            Assert.That(Vector2.Distance(b.A, new Vector2(ax, ay)), Is.LessThan(1e-5));
            Assert.That(Vector2.Distance(b.B, new Vector2(bx, by)), Is.LessThan(1e-5));
            Assert.That(b.Growth, Is.EqualTo(new Vector2(gx, gy)));
        }
    }
}
