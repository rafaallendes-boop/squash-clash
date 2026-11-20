using System;
using System.Linq;
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
    // G1 A2 integration: one intention layer for the human, the rival, the headless rally and the AI forecast,
    // and the causal height lift kept on top of the intended aim (option B, 18 nov).
    public sealed class ShotIntentIntegrationTests
    {
        private RallyContext Context() => new RallyContext(EffectiveConfig.From(ConfigFixtures.Default()),
            new RallyParams(), new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));

        // The C1 witness fixture SL6 crossed, P=0.80, aim y=0.88: a request the search is known to change.
        private static (Vector3 from, Vector2 aim) Sl6Cross(RallyContext ctx, float height)
        {
            var recipe = PracticeLaunchDefinitions.Create(ctx.Config).Single(r => r.Id == "SL6");
            var plan = ContactPlanner.Plan(ctx.Predict(recipe.Initial), recipe.Body, ctx.Court, ctx.Legal, 1);
            Assert.That(plan.Exists && plan.TStarStep >= 0, Is.True, "Real T* fixture");
            var from = plan.PStar;
            float near = from.x < ctx.Court.Width / 2 ? .6f : ctx.Court.Width - .6f;
            float corner = ctx.Court.Width - near;
            return (from, new Vector2(from.x + (corner - from.x) * from.z / (from.z + ctx.Court.Length), height));
        }
        private static ShotSolution Direct(RallyContext ctx, Vector3 from, Vector2 input, Vector2 requested, float speed, float power)
            => ShotIntentSolver.Solve(new ShotRequest(from, input, power), requested, speed, ctx.VCap, ctx.Pace, ctx.Court, ctx.Factory,
                ctx.Physics, (float)ctx.Params.FootprintRadius, out _);
        // A reference whose ball path passes through the contact and a body at a comfortable reach: no stretch or jam.
        private static (CausalReference reference, BodyFrame body) Reference(Vector3 through)
        {
            var body = new BodyFrame(new Vector2(through.x - .6f, through.z), true, 1.3f, .6f, .4f, 1.1f, .3f);
            var flight = new Trajectory();
            for (int i = 0; i <= 230; i++)
                flight.States.Add(new BallState { Step = i, Position = new Vector3(through.x, through.y, through.z + (i - 150) * .01f), Velocity = new Vector3(0, 0, 1) });
            Func<SurfaceId, long, float, ContactEvent> ev = (surface, step, height) =>
            {
                var n = CourtDimensions.InwardNormal(surface);
                return new ContactEvent(surface, new Vector3(through.x, height, through.z) - n * .02f, Vector2.zero, n, step, 0, ContactTiming.Exact, Vector3.zero, false);
            };
            flight.Contacts.Add(ev(SurfaceId.FrontWall, 1, 1.2f)); flight.Contacts.Add(ev(SurfaceId.Floor, 101, .02f)); flight.Contacts.Add(ev(SurfaceId.Floor, 201, .02f));
            var plan = new ContactPlan(true, new SimTime(100), new SimTime(200), 150, flight.States[150].Position, StrokeSide.Drive, 1, body,
                new LegalSettings(.05f, 2, 100, 0), CourtDimensions.Standard());
            var clock = new ClockMap(); clock.Begin(1, 0, 0, 1, 1, 100);
            return (CausalQuality.Prepare(flight, plan, clock, new TimingSettings(40, true, 100, false, 0), 0, 1), body);
        }
        private static void Same(ShotSolution a, ShotSolution b)
        {
            Assert.That(a.Solved, Is.EqualTo(b.Solved)); Assert.That(a.Reason, Is.EqualTo(b.Reason));
            Assert.That(a.TargetLowered, Is.EqualTo(b.TargetLowered)); Assert.That(a.SpeedRaised, Is.EqualTo(b.SpeedRaised));
            foreach (var pair in new[] { (a.Speed, b.Speed), (a.Velocity.x, b.Velocity.x), (a.Velocity.y, b.Velocity.y), (a.Velocity.z, b.Velocity.z),
                (a.TargetUsed.x, b.TargetUsed.x), (a.TargetUsed.y, b.TargetUsed.y) })
                Assert.That(BitConverter.SingleToInt32Bits(pair.Item1), Is.EqualTo(BitConverter.SingleToInt32Bits(pair.Item2)));
        }

        [Test]
        public void CausalLift_IsKeptOnTopOfTheIntendedAim()
        {
            var ctx = Context(); var (from, aim) = Sl6Cross(ctx, .88f); const float power = .8f, lift = .1f;
            float speed = ShotPace.Requested(power, from, aim, ctx.Physics.BallRadius, ctx.Pace).Speed;
            var plain = Direct(ctx, from, aim, aim, speed, power);
            var lifted = Direct(ctx, from, new Vector2(aim.x, aim.y + lift), aim, speed, power);
            Assert.That(plain.Reason, Is.EqualTo("intent-drive"));
            Assert.That(lifted.Reason, Is.EqualTo("intent-drive"));
            Assert.That(lifted.TargetUsed.x, Is.EqualTo(plain.TargetUsed.x), "The lift does not move the intended direction");
            Assert.That(lifted.TargetUsed.y, Is.EqualTo(plain.TargetUsed.y + lift), "The causal rise survives the intention search");
            Assert.That(lifted.Speed, Is.EqualTo(plain.Speed));
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }

        [Test]
        public void CausalPath_HumanAndRival_UseTheIntentionLayer_AndKeepTheTimingLift()
        {
            var ctx = Context(); var (from, aim) = Sl6Cross(ctx, .88f); const float power = .8f;
            foreach (float offset in new[] { 0f, .1f })
            {
                var contact = new Vector3(from.x, from.y + offset, from.z);
                var (reference, body) = Reference(from);
                var human = CausalQuality.Solve(reference, body, contact, aim, power, ctx.Config, ctx.Factory);
                var rival = RallyContact.Solve(ctx, reference, body, contact, aim, power);
                Same(human.Solution, rival.Solution);
                Same(human.Solution, Direct(ctx, contact, human.Input.Aim, aim, human.Input.Speed, power));
                Assert.That(human.Solution.Reason, Is.EqualTo("intent-drive"), "offset " + offset);
                float lift = human.Input.Aim.y - aim.y;
                if (offset == 0) Assert.That(lift, Is.Zero, "On the reference path there is no timing lift");
                else
                {
                    Assert.That(lift, Is.GreaterThan(0f), "A contact off the reference path lifts the aim");
                    var plain = Direct(ctx, contact, aim, aim, human.Input.Speed, power);
                    Assert.That(human.Solution.TargetUsed.y, Is.EqualTo(plain.TargetUsed.y + lift), "Timing still costs height");
                }
            }
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }

        [Test]
        public void Forecast_UsesTheSameIntention_AndIsDeterministic()
        {
            var ctx = Context(); var (from, aim) = Sl6Cross(ctx, .88f); const float power = .8f;
            float speed = ShotPace.Requested(power, from, aim, ctx.Physics.BallRadius, ctx.Pace).Speed;
            var forecast = RallyContact.SolveIntention(ctx, from, aim, power);
            Assert.That(forecast.Reason, Is.EqualTo("intent-drive"));
            Same(forecast, Direct(ctx, from, aim, aim, speed, power));
            Same(forecast, RallyContact.SolveIntention(ctx, from, aim, power));
            Assert.That(RallyContact.Forecast(ctx, from, aim, power).Good, Is.True, "The witness drive bounces in court");
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }
    }
}
