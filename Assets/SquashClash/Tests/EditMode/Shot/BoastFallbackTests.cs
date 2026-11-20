using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    // Two-wall shot, addendum 1 (design 2026-11-20-boast-predeclaration): a boast with no valid point at the real contact is not a fault;
    // the stroke goes to the front wall on the same side (straight line to the near back corner, normal height) through the existing
    // front-wall aim path, bit for bit.
    public sealed class BoastFallbackTests
    {
        private static readonly Vector3 Contact = new Vector3(.8f, .7f, 8.4f);
        private static CausalReference Reference(out BodyFrame body)
        {
            var frame = new BodyFrame(new Vector2(1.4f, 8.4f), true, 1.3f, .6f, .4f, 1.1f, .3f); body = frame;
            var flight = new Trajectory();
            for (int i = 0; i <= 230; i++) flight.States.Add(new BallState { Step = i, Position = new Vector3(Contact.x, Contact.y, Contact.z + (i - 150) * .01f), Velocity = new Vector3(0, 0, 1) });
            var n = CourtDimensions.InwardNormal(SurfaceId.FrontWall);
            flight.Contacts.Add(new ContactEvent(SurfaceId.FrontWall, new Vector3(Contact.x, 1.2f, 0), Vector2.zero, n, 1, 0, ContactTiming.Exact, Vector3.zero, false));
            var plan = new ContactPlan(true, new SimTime(100), new SimTime(200), 150, flight.States[150].Position, StrokeSide.Drive, 1, frame, new LegalSettings(.05f, 2, 100, 0), CourtDimensions.Standard());
            var clock = new ClockMap(); clock.Begin(1, 0, 0, 1, 1, 100);
            return CausalQuality.Prepare(flight, plan, clock, new TimingSettings(40, true, 100, false, 0), 0, 1);
        }
        [TestCase(SurfaceId.RightWall)] [TestCase(SurfaceId.LeftWall)]
        public void FallbackAim_IsTheStraightLineToTheNearCornerOfTheBoastSide_AtNormalHeight(SurfaceId wall)
        {
            var court = CourtDimensions.Standard(); var from = wall == SurfaceId.RightWall ? Contact : new Vector3(court.Width - Contact.x, Contact.y, Contact.z);
            var aim = BoastFallback.Aim(from, wall, court);
            float corner = wall == SurfaceId.RightWall ? BoastFallback.Inset : court.Width - BoastFallback.Inset;
            Assert.That(aim.x, Is.EqualTo(from.x + (corner - from.x) * from.z / (from.z + court.Length)).Within(1e-5));
            Assert.That(aim.y, Is.EqualTo(BoastFallback.Height));
            Assert.That((aim.x - court.Width * .5f) * (from.x - court.Width * .5f), Is.GreaterThan(0), "same side as the boast");
        }
        [Test] public void UnsolvableBoast_FallsBackToTheFrontWallPath_BitForBit_AndIsNotAFault()
        {
            var config = EffectiveConfig.From(ConfigFixtures.Default()); var factory = new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var reference = Reference(out var body); var side = new SideAim(SurfaceId.RightWall, Contact.z + 2, 1.2f);
            var r = CausalQuality.SolveBoast(reference, body, Contact, side, .5f, config, factory, FatigueArm.None, .02f, out var boast);
            Assert.That(boast.Solved, Is.False, "no valid boast in reach"); Assert.That(r.FellBack, Is.True);
            Assert.That(r.Solution.Solved, Is.True, r.Solution.Reason);
            var aim = BoastFallback.Aim(Contact, SurfaceId.RightWall, config.Court); Assert.That(r.FallbackAim, Is.EqualTo(aim));
            var front = CausalQuality.Solve(reference, body, Contact, aim, .5f, config, factory, FatigueArm.None, .02f);
            Assert.That((r.Solution.Velocity, r.Solution.Speed, r.Solution.TargetUsed, r.Solution.Reason), Is.EqualTo((front.Solution.Velocity, front.Solution.Speed, front.Solution.TargetUsed, front.Solution.Reason)));
        }
        [Test] public void SolvableBoast_DoesNotFallBack()
        {
            var config = EffectiveConfig.From(ConfigFixtures.Default()); var factory = new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var reference = Reference(out var body);
            var r = CausalQuality.SolveBoast(reference, body, Contact, new SideAim(SurfaceId.RightWall, 7.6f, 1.2f), .5f, config, factory, FatigueArm.None, .02f, out var boast);
            Assert.That(boast.Solved, Is.True); Assert.That(r.FellBack, Is.False); Assert.That(r.Solution.Reason, Does.StartWith("boast"));
        }
    }
}
