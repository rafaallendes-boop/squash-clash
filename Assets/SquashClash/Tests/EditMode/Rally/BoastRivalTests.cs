using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // Two-wall shot for the rival (design 2026-11-20-boast-predeclaration, section 5): a Boast line from the back quarter near a side
    // wall, behind the DEV «Dos paredes» switch (RallyParams.BoastRival). Off: the options and the draws are today's.
    public sealed class BoastRivalTests
    {
        private static RallyContext Context(bool on, Action<RallyParams> edit = null)
        { var p = new RallyParams { BoastRival = on ? 1 : 0 }; edit?.Invoke(p); return RallyFixtures.Context(p); }
        private static ReceptionPlan OwnPlan(RallyContext context, Vector3 point, double slack = .35)
        {
            var stance = new Vector2(point.x + (point.x < context.Court.Width * .5f ? .6f : -.6f), point.z);
            var plan = new ContactPlan(true, new SimTime(100), new SimTime(140), 120, point, StrokeSide.Drive, 1, context.FrameAt(stance), context.Legal, context.Court);
            return new ReceptionPlan(true, stance, stance, plan, 90, slack, Mathf.Clamp01((float)(slack / context.Params.SlackCapS)), "planned");
        }
        private static AiObservation Observe(RallyContext context, Vector2 opponent)
            => new AiObservation(default, context.WaitPosition(PlayerId.Rival), Vector2.zero, opponent, Vector2.zero, 3, 0, false);
        private static List<ShotOption> Options(RallyContext c, Vector3 at)
        { var o = ShotOptions.Build(c, at); if (c.BoastOn) ShotOptions.AddBoasts(c, at, o); return o; }

        [Test] public void Off_ByDefault_TheOptionsAreTodays18()
        {
            Assert.That(new RallyParams().BoastRival, Is.EqualTo(0), "the rival's boast is off by default (M4)");
            var c = Context(false); var at = new Vector3(.9f, .8f, 8.6f);
            Assert.That(Options(c, at).Count, Is.EqualTo(18)); Assert.That(Options(c, at).Any(o => o.Line == ShotLine.Boast), Is.False);
        }
        [TestCase(.9f, SurfaceId.RightWall)] [TestCase(5.5f, SurfaceId.LeftWall)]
        public void On_FromTheBackQuarterNearAWall_AddsFourBoastsAtTheNearWall(float x, SurfaceId wall)
        {
            var c = Context(true); var at = new Vector3(x, .8f, 8.6f); var options = Options(c, at);
            Assert.That(options.Count, Is.EqualTo(22)); var boasts = options.Skip(18).ToList();
            Assert.That(boasts.All(o => o.Line == ShotLine.Boast && o.Side.Exists && o.Side.Wall == wall && o.Side.Z < at.z && o.Aim == o.Side.UV && !o.Long), Is.True);
            Assert.That(boasts.Select(o => (o.Height, o.Power)).Distinct().Count(), Is.EqualTo(4));
            Assert.That(boasts.All(o => (o.Zone.x - c.Court.Width * .5f) * (at.x - c.Court.Width * .5f) < 0), Is.True, "aimed at the front opposite corner");
        }
        [TestCase(.9f, 6.0f)] [TestCase(3.2f, 8.6f)]
        public void On_OutsideTheBackQuarterOrFarFromTheWalls_NoBoast(float x, float z)
            => Assert.That(Options(Context(true), new Vector3(x, .8f, z)).Count, Is.EqualTo(18));
        [Test] public void On_TheDefinidorBoastsFromTheBackCorner_WithANoisySideAimOnTheSameWall()
        {
            var c = Context(true); var at = new Vector3(.9f, .8f, 8.6f); var own = OwnPlan(c, at); var o = Observe(c, new Vector2(1.5f, 6.5f));
            var ai = new RivalAi(c, AiDifficulty.For(c.Params, AiLevel.Medium), RivalStyleId.Definidor, 7);
            var shots = Enumerable.Range(0, 600).Select(_ => ai.Decide(o, own)).ToList();
            var boasts = shots.Where(s => s.Option.Line == ShotLine.Boast).ToList();
            Assert.That(boasts.Count, Is.GreaterThan(0));
            Assert.That(boasts.All(s => s.Side.Exists && s.Side.Wall == SurfaceId.RightWall && s.Aim == s.Side.UV), Is.True);
            Assert.That(shots.Where(s => s.Option.Line != ShotLine.Boast).All(s => !s.Side.Exists), Is.True);
            foreach (var b in boasts.Take(20)) Assert.That(RallyContact.ForecastBoast(c, at, b.Option.Side, b.Option.P).Solved, Is.True, "chosen boasts forecast solved");
        }
        // M3's ordering, measured in play as M3 defines it (share of the rival's strokes from the back quarter), not in one fixed situation:
        // the style utilities differ, so a single observation can order the styles either way.
        [Test] public void On_InPlay_TheFondistaBoastsLessFromTheBackQuarterThanTheDefinidor()
        {
            var c = Context(true); float back = (float)(c.Params.BoastBackFraction * c.Court.Length);
            double Share(RivalStyleId style)
            {
                int strokes = 0, boasts = 0;
                for (int m = 0; m < 6; m++)
                    using (var sim = new RallySim(c, new BotBrain(c, 30, 300 + 2 * m, ShotLine.Straight, ShotHeight.High, ShotPower.Strong), RivalAi.For(c, AiLevel.Medium, style, 301 + 2 * m)))
                    {
                        var score = new MatchScore(7, 2, PlayerId.Human); int guard = 0;
                        while (!score.Finished && guard++ < 60)
                        {
                            var o = sim.PlayPoint(score.Server, score.Box);
                            foreach (var r in o.Receptions.Where(r => r.Who == PlayerId.Rival && r.Result == ReceptionResult.Contact && r.ContactPoint.z >= back))
                            { strokes++; if (r.Line == ShotLine.Boast) boasts++; }
                            if (o.Winner.HasValue) score.PointWon(o.Winner.Value);
                        }
                    }
                return boasts / (double)Math.Max(1, strokes);
            }
            Assert.That(Share(RivalStyleId.Fondista), Is.LessThan(Share(RivalStyleId.Definidor)));
        }
        // Addendum 1: a boast with no valid point at the real contact goes to the front wall instead; never a system fault.
        [Test] public void On_RallySim_AnUnsolvableRivalBoastFallsBackToTheFrontWall_NeverASystemFault()
        {
            var c = Context(true, p => p.BoastWeightShooter = 3); int fallbacks = 0, boastFaults = 0;
            for (int m = 0; m < 6; m++)
                using (var sim = new RallySim(c, new BotBrain(c, 30, 500 + 2 * m, ShotLine.Straight, ShotHeight.High, ShotPower.Strong), new RivalAi(c, AiDifficulty.For(c.Params, AiLevel.Easy), RivalStyleId.Definidor, 501 + 2 * m)))
                {
                    var score = new MatchScore(7, 2, PlayerId.Human); int guard = 0;
                    while (!score.Finished && guard++ < 60)
                    {
                        var o = sim.PlayPoint(score.Server, score.Box);
                        foreach (var r in o.Receptions.Where(r => r.Who == PlayerId.Rival && r.Line == ShotLine.Boast))
                        {
                            if (r.Result == ReceptionResult.SystemFault) boastFaults++;
                            if (r.BoastFellBack) { fallbacks++; Assert.That(r.Result, Is.EqualTo(ReceptionResult.Contact)); }
                        }
                        if (o.Winner.HasValue) score.PointWon(o.Winner.Value);
                    }
                }
            Assert.That(boastFaults, Is.Zero); Assert.That(fallbacks, Is.GreaterThan(0), "Easy mistimes enough boasts to need the fallback");
        }
        [Test] public void On_RallySim_AppliesRivalBoastsFromTheBackQuarterOnly()
        {
            var c = Context(true, p => p.BoastWeightShooter = 3); int boasts = 0;
            // The opponent plays high, strong straight lengths, so the rival is often in a back corner.
            for (int m = 0; m < 6; m++)
                using (var sim = new RallySim(c, new BotBrain(c, 30, 100 + 2 * m, ShotLine.Straight, ShotHeight.High, ShotPower.Strong), new RivalAi(c, AiDifficulty.For(c.Params, AiLevel.Medium), RivalStyleId.Definidor, 101 + 2 * m)))
                {
                    var score = new MatchScore(7, 2, PlayerId.Human); int guard = 0;
                    while (!score.Finished && guard++ < 60)
                    {
                        var o = sim.PlayPoint(score.Server, score.Box);
                        foreach (var r in o.Receptions.Where(r => r.Who == PlayerId.Rival && r.Result == ReceptionResult.Contact && r.Line == ShotLine.Boast))
                        {
                            boasts++; Assert.That(r.ContactPoint.z, Is.GreaterThanOrEqualTo((float)(c.Params.BoastBackFraction * c.Court.Length) - .5f));
                            Assert.That(r.Side.Exists, Is.True);
                        }
                        if (o.Winner.HasValue) score.PointWon(o.Winner.Value);
                    }
                }
            Assert.That(boasts, Is.GreaterThan(0));
        }
    }
}
