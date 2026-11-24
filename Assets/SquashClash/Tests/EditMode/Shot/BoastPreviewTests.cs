using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    // Two-wall preview (design 2026-11-24-two-wall-preview-hint): a forecast path beside the stroke. The estimate is the boast pace and the
    // unchanged boast solver from P*, without the causal layer; the schedule bounds how often the view recomputes it.
    public sealed class BoastPreviewTests
    {
        // Match 2 of the diagnosis (docs/reviews/2026-11-24-dos-paredes-alto.md): a backhand from the bounce at the centre, left wall.
        private static readonly Vector3 Match2Contact = new Vector3(3.21f, .30f, 4.36f);
        private const float Match2Depth = 2.73f, Match2Power = .73f;
        private static EffectiveConfig Config() => EffectiveConfig.From(ConfigFixtures.Default());
        private static AnalyticBallSimulationFactory Factory() => new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);

        [Test] public void Estimate_IsTheBoastPaceAndTheUnchangedBoastSolver_BitForBit()
        {
            var config = Config(); var side = new SideAim(SurfaceId.LeftWall, Match2Depth, 1.8f);
            var preview = BoastPreview.Estimate(Match2Contact, side, Match2Power, config, Factory(), ShotIntentSolver.DefaultFootprintRadius, out var info);
            var pace = ShotPaceSettings.From(config.Source);
            float speed = ShotPace.Boast(Match2Power, Match2Contact, side, config.Court, config.Physics.BallRadius, pace).Speed;
            var direct = BoastSolver.Solve(Match2Contact, side, speed, pace.VMin, (float)config.Source.Get("shot.vCap"), config.Court, Factory(), config.Physics,
                ShotIntentSolver.DefaultFootprintRadius, out var directInfo);
            Assert.That(preview.Solved, Is.True, preview.Reason);
            Assert.That((preview.Velocity, preview.Speed, preview.TargetUsed, preview.Reason), Is.EqualTo((direct.Velocity, direct.Speed, direct.TargetUsed, direct.Reason)));
            Assert.That((info.Front, info.Used, info.Evaluations), Is.EqualTo((directInfo.Front, directInfo.Used, directInfo.Evaluations)));
            Assert.That(info.Evaluations, Is.LessThanOrEqualTo(BoastSolver.Budget));
        }
        [Test] public void HigherSideTouch_ShowsAHigherFrontWallZone_FromALowContact()
        {
            var config = Config();
            BoastPreview.Estimate(Match2Contact, new SideAim(SurfaceId.LeftWall, Match2Depth, 1.8f), Match2Power, config, Factory(), ShotIntentSolver.DefaultFootprintRadius, out var low);
            BoastPreview.Estimate(Match2Contact, new SideAim(SurfaceId.LeftWall, Match2Depth, 2.4f), Match2Power, config, Factory(), ShotIntentSolver.DefaultFootprintRadius, out var high);
            Assert.That(low.Solved && high.Solved, Is.True, low.Reason + " / " + high.Reason);
            Assert.That(high.Front.y, Is.GreaterThan(low.Front.y + .6f), "touching 0.6 m higher on the side wall lands higher still on the front wall");
        }
        [Test] public void Estimate_WithoutAValidBoast_IsUnsolved()
        {
            var side = new SideAim(SurfaceId.LeftWall, Match2Contact.z + 1, 1.2f);
            var preview = BoastPreview.Estimate(Match2Contact, side, Match2Power, Config(), Factory(), ShotIntentSolver.DefaultFootprintRadius, out var info);
            Assert.That(preview.Solved, Is.False); Assert.That(info.Solved, Is.False);
        }

        [Test] public void Schedule_SolvesAtTheStartOfATouch_ThenAtMostOncePerInterval_AndOnlyWhenThePowerMoved()
        {
            var s = new BoastPreviewSchedule(); var side = new SideAim(SurfaceId.LeftWall, 2, 1.5f); var p = Match2Contact;
            Assert.That(s.ShouldSolve(10, 1, p, side, .3f), Is.True, "first touch");
            Assert.That(s.ShouldSolve(10, 1, p, side, .3f), Is.False, "same frame, same input");
            Assert.That(s.ShouldSolve(10.05, 1, p, side, .4f), Is.False, "the power moved, but inside the interval");
            Assert.That(s.ShouldSolve(10 + BoastPreviewSchedule.MinIntervalSeconds, 1, p, side, .4f), Is.True, "the power moved and the interval passed");
            Assert.That(s.ShouldSolve(12, 1, p, side, .4f), Is.False, "finger still: no recompute");
            Assert.That(s.SolveCount, Is.EqualTo(2));
        }
        [Test] public void Schedule_ANewTouchOrANewContact_SolvesAtOnce()
        {
            var s = new BoastPreviewSchedule(); var side = new SideAim(SurfaceId.LeftWall, 2, 1.5f); var p = Match2Contact;
            Assert.That(s.ShouldSolve(10, 1, p, side, .3f), Is.True);
            Assert.That(s.ShouldSolve(10.01, 1, p, side.With(2, 1.9f), .3f), Is.True, "another side point");
            Assert.That(s.ShouldSolve(10.02, 1, p + Vector3.forward, side.With(2, 1.9f), .3f), Is.True, "another contact (the volley was chosen)");
            Assert.That(s.ShouldSolve(10.03, 2, p + Vector3.forward, side.With(2, 1.9f), .3f), Is.True, "another launch");
            s.Reset();
            Assert.That(s.ShouldSolve(10.04, 2, p + Vector3.forward, side.With(2, 1.9f), .3f), Is.True, "after the touch ended");
        }
    }
}
