using System;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;

namespace SquashClash.Core.Tests.Shot
{
    // Front deep drive (design 2026-11-20): human-only side-wall guard. Fixtures are the PO's strokes of 20 nov (build fb80015),
    // rebuilt from the recorded contact, effective aim and speed with the primitive solver.
    public sealed class FrontDeepDriveTests
    {
        private RallyContext Context() => new RallyContext(EffectiveConfig.From(ConfigFixtures.Default()),
            new RallyParams(), new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));

        private readonly struct Stroke
        {
            internal readonly Vector3 From; internal readonly Vector2 Aim, Target; internal readonly float Power, Speed;
            internal Stroke(Vector3 from, Vector2 aim, Vector2 target, float power, float speed) { From = from; Aim = aim; Target = target; Power = power; Speed = speed; }
        }
        // Launch 4: straight from (4.60; 5.21), into the left wall at z ~ 1.8 m before the first bounce.
        private static readonly Stroke Straight = new Stroke(new Vector3(4.59859943f, .154499114f, 5.20911f), new Vector2(6.034647f, 1.95351791f), new Vector2(6.011661f, 1.96049631f), 1f, 26.7004814f);
        // Launch 5: cross from (5.75; 4.74), into the right wall before the first bounce.
        private static readonly Stroke Cross = new Stroke(new Vector3(5.751618f, .270855576f, 4.74478245f), new Vector2(2.396859f, 1.79629183f), new Vector2(2.3972f, 1.92371213f), .8027929f, 22.3851166f);
        // Launch 24: straight from (4.60; 5.09) that already reaches the back cleanly.
        private static readonly Stroke Clean = new Stroke(new Vector3(4.59859943f, .161849082f, 5.085526f), new Vector2(5.84474754f, .9192538f), new Vector2(5.89656925f, .956363142f), 1f, 27.5670586f);
        // Launch 14: behind the short line (z 5.78), into the side wall.
        private static readonly Stroke Behind = new Stroke(new Vector3(3.729567f, .351921678f, 5.77995062f), new Vector2(5.690976f, 1.50431156f), new Vector2(5.690976f, 1.58444893f), .8630299f, 24.15234f);

        private static ShotSolution Primitive(RallyContext ctx, in Stroke s)
        {
            var solution = ShotSolver.Solve(new ShotRequest(s.From, s.Target, s.Power), s.Speed, s.Speed, s.Speed, ctx.Physics.Gravity, ctx.Physics.BallRadius, ctx.Court, ctx.Factory, ctx.Physics);
            Assert.That(solution.Solved && !solution.TargetLowered, Is.True, "fixture solves at its own target");
            return solution;
        }
        private static ShotSolution Apply(RallyContext ctx, in Stroke s, in ShotSolution solution, out FrontDeepInfo info, float? power = null, Vector2? aim = null)
            => FrontDeepDrive.Apply(s.From, aim ?? s.Aim, power ?? s.Power, solution, ctx.Court, ctx.Factory, ctx.Physics, ShotIntentSolver.DefaultFootprintRadius, out info);

        // Independent of the product predicate: side wall before the first bounce, back third (z >= 6.5) before the second, legal end.
        private static (bool sideBeforeBounce, bool reachedBack, bool legal) Flight(RallyContext ctx, Vector3 from, Vector3 velocity)
        {
            var rules = new PointRules(ctx.Court, ShotIntentSolver.DefaultFootprintRadius); rules.Begin(PlayerId.Human, PlayerId.Rival);
            bool side = false, reached = false, front = false; int floors = 0;
            using (var simulation = ctx.Factory.Create(ctx.Physics, ctx.Court))
            {
                simulation.Reset(new BallState { Position = from, Velocity = velocity });
                var contacts = new List<ContactEvent>(4);
                for (int step = 1; step <= ctx.StepHz * 10 && !rules.Closed; step++)
                {
                    contacts.Clear(); simulation.Step(contacts);
                    foreach (var hit in contacts)
                    {
                        if (rules.Closed) break;
                        if (!hit.Persistent && hit.Surface == SurfaceId.FrontWall) front = true;
                        if (!hit.Persistent && front && floors == 0 && (hit.Surface == SurfaceId.LeftWall || hit.Surface == SurfaceId.RightWall)) side = true;
                        if (!hit.Persistent && hit.Surface == SurfaceId.Floor) floors++;
                        rules.OnBallContact(hit);
                    }
                    if (front && floors < 2 && simulation.State.Position.z >= ctx.Court.Length * 2f / 3f) reached = true;
                    if (!rules.Closed && simulation.State.Position.y > ctx.Court.WallHeight) rules.OnOverTop();
                }
            }
            return (side, reached, rules.Cause == PointEndCause.DoubleBounce);
        }
        private static void Same(in ShotSolution a, in ShotSolution b)
        {
            Assert.That(a.Solved, Is.EqualTo(b.Solved)); Assert.That(a.Reason, Is.EqualTo(b.Reason));
            Assert.That(a.TargetLowered, Is.EqualTo(b.TargetLowered)); Assert.That(a.SpeedRaised, Is.EqualTo(b.SpeedRaised));
            foreach (var pair in new[] { (a.Speed, b.Speed), (a.Velocity.x, b.Velocity.x), (a.Velocity.y, b.Velocity.y), (a.Velocity.z, b.Velocity.z),
                (a.TargetUsed.x, b.TargetUsed.x), (a.TargetUsed.y, b.TargetUsed.y) })
                Assert.That(BitConverter.SingleToInt32Bits(pair.Item1), Is.EqualTo(BitConverter.SingleToInt32Bits(pair.Item2)));
        }
        private void IntoTheSideWall_BecomesDeep(in Stroke s)
        {
            var ctx = Context(); var original = Primitive(ctx, s);
            Assert.That(Flight(ctx, s.From, original.Velocity).sideBeforeBounce, Is.True, "fixture: today it meets a side wall before the bounce");
            var guarded = Apply(ctx, s, original, out var info);
            Assert.That(info.Reason, Is.EqualTo(FrontDeepDrive.ReasonActed)); Assert.That(info.Acted, Is.True);
            Assert.That(guarded.Reason, Is.EqualTo(FrontDeepDrive.ReasonActed)); Assert.That(guarded.Solved, Is.True);
            Assert.That(guarded.TargetUsed.y, Is.EqualTo(original.TargetUsed.y), "the height is kept");
            Assert.That(guarded.Speed, Is.EqualTo(original.Speed), "the speed is kept");
            float lo = Math.Min(s.From.x, original.TargetUsed.x), hi = Math.Max(s.From.x, original.TargetUsed.x);
            Assert.That(guarded.TargetUsed.x, Is.InRange(lo, hi), "same direction, closed towards the contact");
            Assert.That(guarded.TargetUsed.x, Is.Not.EqualTo(original.TargetUsed.x));
            Assert.That(info.T, Is.InRange(0f, 1f));
            var flight = Flight(ctx, s.From, guarded.Velocity);
            Assert.That(flight.sideBeforeBounce, Is.False, "no side wall before the first bounce");
            Assert.That(flight.reachedBack, Is.True, "reaches the back third");
            Assert.That(flight.legal, Is.True);
            Assert.That(info.Evaluations, Is.LessThanOrEqualTo(2 + FrontDeepDrive.BisectionSteps));
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }

        [Test] public void Straight_FromTheFront_IntoTheSideWall_BecomesADeepDrive() => IntoTheSideWall_BecomesDeep(Straight);
        [Test] public void Cross_FromTheFront_IntoTheSideWall_BecomesADeepCross() => IntoTheSideWall_BecomesDeep(Cross);

        [Test]
        public void AlreadyDeep_IsReturnedBitForBit()
        {
            var ctx = Context(); var original = Primitive(ctx, Clean);
            var guarded = Apply(ctx, Clean, original, out var info);
            Assert.That(info.Reason, Is.EqualTo("already-deep"));
            Same(guarded, original);
        }

        [TestCase("power")][TestCase("low-aim")][TestCase("behind-short-line")]
        public void OutsideTheZone_IsReturnedBitForBit(string which)
        {
            var ctx = Context(); var s = which == "behind-short-line" ? Behind : Straight; var original = Primitive(ctx, s);
            var guarded = which == "power" ? Apply(ctx, s, original, out var info, power: .5f)
                : which == "low-aim" ? Apply(ctx, s, original, out info, aim: new Vector2(s.Aim.x, .79f)) : Apply(ctx, s, original, out info);
            Assert.That(info.Reason, Is.EqualTo("inactive")); Assert.That(info.Evaluations, Is.Zero);
            Same(guarded, original);
        }

        [Test]
        public void Active_IsTheDeclaredZone()
        {
            var court = CourtDimensions.Standard();
            Assert.That(FrontDeepDrive.Active(1, Straight.From, Straight.Aim, court), Is.True);
            Assert.That(FrontDeepDrive.Active(.5f, Straight.From, Straight.Aim, court), Is.False, "P > 0.5");
            Assert.That(FrontDeepDrive.Active(1, Straight.From, new Vector2(Straight.Aim.x, .79f), court), Is.False, "aim y >= 0.80");
            Assert.That(FrontDeepDrive.Active(1, Straight.From, new Vector2(Straight.Aim.x, .80f), court), Is.True);
            Assert.That(FrontDeepDrive.Active(1, Behind.From, Behind.Aim, court), Is.False, "in front of the short line");
        }

        [Test]
        public void Deep_IsTheDeclaredSuccess()
        {
            var ctx = Context();
            Assert.That(FrontDeepDrive.Deep(Clean.From, Primitive(ctx, Clean).Velocity, ctx.Court, ctx.Factory, ctx.Physics, ShotIntentSolver.DefaultFootprintRadius), Is.True);
            Assert.That(FrontDeepDrive.Deep(Straight.From, Primitive(ctx, Straight).Velocity, ctx.Court, ctx.Factory, ctx.Physics, ShotIntentSolver.DefaultFootprintRadius), Is.False);
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }
    }
}
