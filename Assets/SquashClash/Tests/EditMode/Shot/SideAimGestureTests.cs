using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    // Two-wall shot gesture (design 2026-11-20-boast-predeclaration, section 1; target M2): the same M2-4 touch; a ray that misses the
    // front wall and meets a visible side wall (front half, between the tin height and the side out line) is a side-wall aim. Front-wall
    // touches are bit for bit today's. Captured perspective projection of the default camera (as in M24ProjectionTests).
    public sealed class SideAimGestureTests
    {
        private GameObject go; private Camera camera; private FrontisProjection projection;
        [SetUp] public void Setup()
        {
            go = new GameObject("Side aim projection fixture"); camera = go.AddComponent<Camera>();
            camera.transform.position = new Vector3(3.2f, 8, 14.75f); camera.transform.LookAt(new Vector3(3.2f, .4f, 0));
            camera.fieldOfView = 90; camera.aspect = 1206f / 2622; camera.pixelRect = new Rect(0, 0, 1206, 2622);
            projection = new FrontisProjection(camera.cameraToWorldMatrix, camera.projectionMatrix, camera.pixelRect, new Rect(0, 102, 1206, 2334), CourtDimensions.Standard(), 10.025, 7);
        }
        [TearDown] public void Cleanup() => Object.DestroyImmediate(go);
        private Vector2 Screen(Vector3 world) => camera.WorldToScreenPoint(world);
        private M24Model Model(bool side) => new M24Model(new SlingSettings(holdMs: 120), projection, .1f) { AllowSide = side };
        private static TouchSample S(double t, Vector2 at, TouchPhaseKind phase) => new TouchSample(1, phase, at, t, 0);

        [Test] public void M2_FrontWallTouches_AreBitIdenticalWithTheSideAimOn()
        {
            int compared = 0;
            for (float x = .05f; x <= 6.36f; x += .35f)
                for (float y = .05f; y <= 5.96f; y += .35f)
                {
                    var at = Screen(new Vector3(x, y, 0)); var off = Model(false); var on = Model(true);
                    off.Push(S(0, at, TouchPhaseKind.Began)); on.Push(S(0, at, TouchPhaseKind.Began));
                    Assert.That((on.HasAim, on.Aim, on.Invalid, on.Reason, on.Outcome, on.Active), Is.EqualTo((off.HasAim, off.Aim, off.Invalid, off.Reason, off.Outcome, off.Active)), x + "," + y);
                    Assert.That(on.Side.Exists, Is.False); compared++;
                    var end = at + new Vector2(0, -250);
                    off.Push(S(.3, end, TouchPhaseKind.Ended)); on.Push(S(.3, end, TouchPhaseKind.Ended));
                    Assert.That((on.P, on.Outcome), Is.EqualTo((off.P, off.Outcome)));
                }
            Assert.That(compared, Is.GreaterThan(300));
        }
        [TestCase(SurfaceId.RightWall, 3f, 2f)] [TestCase(SurfaceId.RightWall, 1f, 1f)] [TestCase(SurfaceId.LeftWall, 4f, 1.5f)] [TestCase(SurfaceId.LeftWall, 2f, 3.2f)]
        public void M2_SideWallTouch_BecomesASideAim_AndSwingsWithPull(SurfaceId wall, float z, float y)
        {
            var world = new Vector3(wall == SurfaceId.RightWall ? 0 : 6.4f, y, z); var m = Model(true); var at = Screen(world);
            m.Push(S(0, at, TouchPhaseKind.Began));
            Assert.That(m.HasAim, Is.True, m.Reason); Assert.That(m.Side.Exists, Is.True); Assert.That(m.Side.Wall, Is.EqualTo(wall));
            Assert.That(m.Side.Z, Is.EqualTo(z).Within(.01)); Assert.That(m.Side.Y, Is.EqualTo(y).Within(.01));
            Assert.That(m.Aim, Is.EqualTo(m.Side.UV)); Assert.That(m.Invalid, Is.False);
            m.Push(S(.3, at + new Vector2(0, -250), TouchPhaseKind.Ended));
            Assert.That(m.Outcome, Is.EqualTo(GestureOutcome.Swing)); Assert.That(m.P, Is.GreaterThan(0));
        }
        [Test] public void SideWallTouch_WithTheSideAimOff_IsRejectedAsToday()
        {
            var m = Model(false); m.Push(S(0, Screen(new Vector3(0, 2, 3)), TouchPhaseKind.Began));
            Assert.That(m.HasAim, Is.False); Assert.That(m.Outcome, Is.EqualTo(GestureOutcome.Rejected)); Assert.That(m.Reason, Is.EqualTo("outside-front-wall"));
        }
        [TestCase(.3f, 3f)] [TestCase(2f, 6.5f)] [TestCase(4.2f, 3f)]
        public void SideWallTouch_OutsideTheVisibleBand_IsRejected(float y, float z)
        {
            var m = Model(true); m.Push(S(0, Screen(new Vector3(0, y, z)), TouchPhaseKind.Began));
            Assert.That(m.HasAim, Is.False); Assert.That(m.Outcome, Is.EqualTo(GestureOutcome.Rejected)); Assert.That(m.Side.Exists, Is.False);
        }
        [Test] public void TryMapSide_UsesTheCapturedProjection_AndLeavesTryMapUnchanged()
        {
            var at = Screen(new Vector3(0, 2, 3));
            Assert.That(projection.TryMap(at, out _, out var reason), Is.False); Assert.That(reason, Is.EqualTo("outside-front-wall"));
            Assert.That(projection.TryMapSide(at, out var side), Is.True); Assert.That(side.Wall, Is.EqualTo(SurfaceId.RightWall));
            camera.transform.position += Vector3.right * 2;
            Assert.That(projection.TryMapSide(at, out var still), Is.True); Assert.That(still, Is.EqualTo(side));
            Assert.That(projection.TryMapSide(Screen(new Vector3(3.2f, 2, 0)), out _), Is.False, "a front-wall point is not a side aim");
        }
    }
}
