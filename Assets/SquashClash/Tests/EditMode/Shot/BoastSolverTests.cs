using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Rally;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    // Two-wall shot solver (design 2026-11-20-boast-predeclaration, section 2): aim at a side-wall point, verify racket -> side -> front
    // with P2, nearest valid point otherwise, unsolved when none.
    public sealed class BoastSolverTests
    {
        private static readonly Vector3 BackRight = new Vector3(.8f, .7f, 8.4f);
        private static float Speed(RallyContext c, float p) => c.VMin + p * (c.VMax - c.VMin);
        private static ShotSolution Solve(RallyContext c, Vector3 from, SideAim aim, float speed, out BoastInfo info)
            => BoastSolver.Solve(from, aim, speed, c.VMin, c.VCap, c.Court, c.Factory, c.Physics, (float)c.Params.FootprintRadius, out info);
        // Independent replay of a flight up to its first bounce: the non-persistent contacts and the rules after each.
        private static List<(ContactEvent hit, bool closed)> Replay(RallyContext c, Vector3 from, Vector3 velocity)
        {
            var rules = RallyFixtures.Rules(c, PlayerId.Human, PlayerId.Rival); var list = new List<(ContactEvent, bool)>();
            using (var sim = c.Factory.Create(c.Physics, c.Court))
            {
                sim.Reset(new BallState { Position = from, Velocity = velocity }); var events = new List<ContactEvent>(4);
                for (int i = 0; i < c.StepHz * 6; i++)
                {
                    events.Clear(); sim.Step(events);
                    foreach (var e in events)
                    {
                        rules.OnBallContact(e); if (e.Persistent) continue;
                        list.Add((e, rules.Closed)); if (rules.Closed || e.Surface == SurfaceId.Floor) return list;
                    }
                }
            }
            return list;
        }

        [Test] public void BackRightCorner_ToTheNearSideWall_IsAVerifiedBoast()
        {
            var c = RallyFixtures.Context(); var aim = new SideAim(SurfaceId.RightWall, 7.6f, 1.2f);
            var s = Solve(c, BackRight, aim, Speed(c, .5f), out var info);
            Assert.That(s.Solved, Is.True, s.Reason); Assert.That(info.Solved, Is.True); Assert.That(s.Reason, Does.StartWith("boast"));
            Assert.That(info.Evaluations, Is.InRange(1, BoastSolver.Budget));
            var flight = Replay(c, BackRight, s.Velocity);
            Assert.That(flight.Count, Is.GreaterThanOrEqualTo(3), "side, front and the first bounce");
            Assert.That(flight[0].hit.Surface, Is.EqualTo(SurfaceId.RightWall)); Assert.That(flight[0].closed, Is.False);
            Assert.That(Vector2.Distance(flight[0].hit.UV, info.Used.UV), Is.LessThanOrEqualTo(BoastSolver.PointTolerance));
            Assert.That(flight[1].hit.Surface, Is.EqualTo(SurfaceId.FrontWall)); Assert.That(flight[1].closed, Is.False);
            Assert.That(flight[1].hit.Point.y, Is.InRange(c.Court.TinTop + BoastSolver.TinMargin, c.Court.FrontOutLower - BoastSolver.OutMargin));
            Assert.That(flight[flight.Count - 1].hit.Surface, Is.EqualTo(SurfaceId.Floor)); Assert.That(flight[flight.Count - 1].closed, Is.False, "legal to the first bounce");
            Assert.That(info.Front.x, Is.EqualTo(flight[1].hit.Point.x).Within(1e-4)); Assert.That(info.Front.y, Is.EqualTo(flight[1].hit.Point.y).Within(1e-4));
            Assert.That(s.TargetUsed, Is.EqualTo(info.Front), "the front-wall point is the effective target");
        }
        [Test] public void ExactPointThatDiesBeforeTheFrontWall_UsesTheNearestValidPoint()
        {
            var c = RallyFixtures.Context(); var aim = new SideAim(SurfaceId.RightWall, 7.6f, .25f);
            var s = Solve(c, BackRight, aim, Speed(c, .5f), out var info);
            Assert.That(s.Solved, Is.True, s.Reason); Assert.That(s.Reason, Is.EqualTo(BoastSolver.ReasonNearest)); Assert.That(info.Exact, Is.False);
            Assert.That(info.Used.Wall, Is.EqualTo(SurfaceId.RightWall)); Assert.That(info.Used.Y, Is.GreaterThan(aim.Y), "raised off the floor");
            Assert.That(info.Speed, Is.InRange(info.RequestedSpeed * (1 - BoastSolver.SpeedBand) - 1e-3f, info.RequestedSpeed * (1 + BoastSolver.SpeedBand) + 1e-3f));
            var flight = Replay(c, BackRight, s.Velocity);
            Assert.That(flight[0].hit.Surface, Is.EqualTo(SurfaceId.RightWall)); Assert.That(flight[1].hit.Surface, Is.EqualTo(SurfaceId.FrontWall));
        }
        [Test] public void NoValidPointInReach_IsUnsolved_LikeAnUnsolvableFrontAim()
        {
            var c = RallyFixtures.Context();
            var s = Solve(c, BackRight, new SideAim(SurfaceId.RightWall, BackRight.z + 2, 1.2f), Speed(c, .5f), out var info);
            Assert.That(s.Solved, Is.False); Assert.That(info.Solved, Is.False); Assert.That(s.Reason, Is.EqualTo(BoastSolver.ReasonNone));
            Assert.That(s.Velocity, Is.EqualTo(Vector3.zero));
            s = Solve(c, BackRight, default, Speed(c, .5f), out info);
            Assert.That(s.Solved, Is.False, "no side aim");
        }
        [Test] public void Verify_RejectsAFlightThatMeetsTheFrontWallFirst()
        {
            var c = RallyFixtures.Context();
            var front = ShotSolver.Solve(new ShotRequest(BackRight, new Vector2(1f, 1.5f), .5f), 17, 17, 17, c.Physics.Gravity, c.Physics.BallRadius, c.Court, c.Factory, c.Physics);
            Assert.That(front.Solved, Is.True);
            Assert.That(BoastSolver.Verify(BackRight, front.Velocity, new SideAim(SurfaceId.RightWall, 7.6f, 1.2f), c.Court, c.Factory, c.Physics, .02f, out _), Is.False);
        }
        [Test] public void LeftWallMirror_IsAlsoAVerifiedBoast()
        {
            var c = RallyFixtures.Context(); var from = new Vector3(c.Court.Width - BackRight.x, BackRight.y, BackRight.z);
            var s = Solve(c, from, new SideAim(SurfaceId.LeftWall, 7.6f, 1.2f), Speed(c, .5f), out var info);
            Assert.That(s.Solved, Is.True, s.Reason); Assert.That(info.Used.Wall, Is.EqualTo(SurfaceId.LeftWall));
            Assert.That(Replay(c, from, s.Velocity)[0].hit.Surface, Is.EqualTo(SurfaceId.LeftWall));
        }
    }
}
