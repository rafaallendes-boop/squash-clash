using System;
using System.Collections.Generic;
using System.Globalization;
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
    public sealed class ShotIntentSolverTests
    {
        private RallyContext Context() => new RallyContext(EffectiveConfig.From(ConfigFixtures.Default()),
            new RallyParams(), new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));

        public static IEnumerable<TestCaseData> DriveBlock()
        {
            foreach (string recipe in new[] { "SL5", "SL6" })
                foreach (string line in new[] { "straight", "cross" })
                    foreach (float p in new[] { .70f, .75f, .80f })
                        foreach (float y in new[] { .88f, 1.08f })
                            yield return new TestCaseData(recipe, line, p, y);
        }

        [TestCaseSource(nameof(DriveBlock))]
        public void C1_AllPredeclaredDirectDriveWitnesses(string recipeId, string line, float power, float height)
        {
            var ctx = Context();
            var recipe = PracticeLaunchDefinitions.Create(ctx.Config).Single(r => r.Id == recipeId);
            var incoming = ctx.Predict(recipe.Initial);
            var plan = ContactPlanner.Plan(incoming, recipe.Body, ctx.Court, ctx.Legal, 1);
            Assert.That(plan.Exists && plan.TStarStep >= 0, Is.True, "Real T* fixture");
            var from = plan.PStar;
            float near = from.x < ctx.Court.Width / 2 ? .6f : ctx.Court.Width - .6f;
            float corner = line == "straight" ? near : ctx.Court.Width - near;
            float through = from.x + (corner - from.x) * from.z / (from.z + ctx.Court.Length);
            var aim = new Vector2(through, height);
            var pace = ShotPace.Requested(power, from, aim, ctx.Physics.BallRadius, ctx.Pace);
            var solution = ShotIntentSolver.Solve(new ShotRequest(from, aim, power), aim, pace.Speed, ctx.VCap,
                ctx.Pace, ctx.Court, ctx.Factory, ctx.Physics, (float)ctx.Params.FootprintRadius, out var info);
            Assert.That(solution.Solved, Is.True, solution.Reason);
            Assert.That(Math.Abs((double)solution.TargetUsed.x - aim.x), Is.LessThanOrEqualTo(ctx.Court.Width / 20d), "Total lateral cap");
            Assert.That(solution.TargetUsed.y, Is.LessThanOrEqualTo(aim.y), "Height only lowers");
            Assert.That(info.Evaluations, Is.LessThanOrEqualTo(24));
            Assert.That(solution.Speed, Is.EqualTo(pace.Speed), "No speed change to reach the witness");
            var trajectory = ctx.Predict(new BallState { Position = from, Velocity = solution.Velocity });
            var rules = new PointRules(ctx.Court, (float)ctx.Params.FootprintRadius);
            rules.Begin(PlayerId.Human, PlayerId.Rival);
            ContactEvent? front = null, first = null, second = null;
            float apex = from.y;
            int next = 0, floors = 0;
            foreach (var state in trajectory.States.Skip(1))
            {
                while (next < trajectory.Contacts.Count && trajectory.Contacts[next].Step <= state.Step && !rules.Closed)
                {
                    var e = trajectory.Contacts[next++];
                    if (!e.Persistent)
                    {
                        if (!front.HasValue && e.Surface == SurfaceId.FrontWall) front = e;
                        if (e.Surface == SurfaceId.Floor)
                        { if (++floors == 1) first = e; else if (floors == 2) second = e; }
                    }
                    rules.OnBallContact(e);
                }
                // Same discrete P2 apex convention as the immutable phase-A diagnostic.
                if (!rules.Closed) apex = Mathf.Max(apex, state.Position.y);
                if (!rules.Closed && state.Position.y > ctx.Court.WallHeight) rules.OnOverTop();
                if (rules.Closed) break;
            }
            bool right = from.x < ctx.Court.Width / 2;
            bool side = first.HasValue && second.HasValue && (line == "straight"
                ? (right ? first.Value.Point.x : ctx.Court.Width - first.Value.Point.x) <= 1 && (right ? second.Value.Point.x : ctx.Court.Width - second.Value.Point.x) <= 1
                : (right ? ctx.Court.Width - second.Value.Point.x : second.Value.Point.x) <= 1 && (from.x - ctx.Court.Width / 2) * (second.Value.Point.x - ctx.Court.Width / 2) < 0);
            bool witness = rules.Cause == PointEndCause.DoubleBounce && front.HasValue && second.HasValue
                && front.Value.Point.y <= 1.2f && apex <= 2.13f && second.Value.Point.z >= 8 && side;
            string F(float v) => v.ToString("R", CultureInfo.InvariantCulture);
            string row = string.Join("\t", recipeId, line, F(power), F(height), rules.Cause, F(front?.Point.y ?? float.NaN),
                F(apex), F(first?.Point.x ?? float.NaN), F(first?.Point.z ?? float.NaN), F(second?.Point.x ?? float.NaN),
                F(second?.Point.z ?? float.NaN), F(solution.TargetUsed.x), F(solution.TargetUsed.y), info.Evaluations, info.Reason, witness);
            TestContext.WriteLine(row);
            Assert.That(witness, Is.True, "A2-C1 witness: " + row);
            Assert.That(ctx.Factory.LiveInstances, Is.Zero);
        }

        [TestCase(.5f, 2f)] [TestCase(1f, 8f)]
        public void InactivePolicy_ReturnsPrimitiveBitForBit(float power, float depth)
        {
            var ctx = Context(); var from = new Vector3(2.7f, .6f, depth); var aim = new Vector2(2.6f, 1.2f);
            var pace = ShotPace.Requested(power, from, aim, ctx.Physics.BallRadius, ctx.Pace);
            var old = ShotSolver.Solve(new ShotRequest(from, aim, power), pace.Speed, pace.Speed, ctx.VCap,
                ctx.Physics.Gravity, ctx.Physics.BallRadius, ctx.Court, ctx.Factory, ctx.Physics);
            var now = ShotIntentSolver.Solve(new ShotRequest(from, aim, power), aim, pace.Speed, ctx.VCap,
                ctx.Pace, ctx.Court, ctx.Factory, ctx.Physics, (float)ctx.Params.FootprintRadius, out var info);
            Assert.That(now.Solved, Is.EqualTo(old.Solved)); Assert.That(now.Reason, Is.EqualTo(old.Reason));
            Assert.That(now.SpeedRaised, Is.EqualTo(old.SpeedRaised)); Assert.That(now.TargetLowered, Is.EqualTo(old.TargetLowered));
            foreach (var pair in new[] { (now.Speed, old.Speed), (now.Velocity.x, old.Velocity.x), (now.Velocity.y, old.Velocity.y),
                (now.Velocity.z, old.Velocity.z), (now.TargetUsed.x, old.TargetUsed.x), (now.TargetUsed.y, old.TargetUsed.y) })
                Assert.That(BitConverter.SingleToInt32Bits(pair.Item1), Is.EqualTo(BitConverter.SingleToInt32Bits(pair.Item2)));
            Assert.That(info.Evaluations, Is.Zero);
        }
    }
}
