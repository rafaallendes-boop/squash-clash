using System;
using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.Ball;
using UnityEngine;
using UnityEngine.TestTools;

namespace SquashClash.Runtime.Tests
{
    public sealed class BallShadowNearBounceTests
    {
        [Test] public void Alpha_AtFloorReachesTheCeilingAndNegativeHeightsClampToIt()
        {
            Assert.That(BallShadowLook.Alpha(0), Is.EqualTo(.72f).Within(1e-5f));
            Assert.That(BallShadowLook.Alpha(-1), Is.EqualTo(BallShadowLook.Alpha(0)));
            for (float h = 0; h <= 10; h += .05f) Assert.That(BallShadowLook.Alpha(h), Is.LessThanOrEqualTo(.72f + 1e-5f), "h=" + h);
        }
        [Test] public void Alpha_KeepsAVisibleFloorAtAnyHeight()
        {
            for (float h = 0; h <= 10; h += .05f) Assert.That(BallShadowLook.Alpha(h), Is.GreaterThan(.22f), "h=" + h);
            Assert.That(BallShadowLook.Alpha(float.PositiveInfinity), Is.EqualTo(.22f).Within(1e-5f));
        }
        [Test] public void Alpha_FallsStrictlyAndSmoothlyAsHeightGrows()
        {
            float previous = BallShadowLook.Alpha(0);
            for (int i = 1; i <= 120; i++)
            {
                float alpha = BallShadowLook.Alpha(i * .05f);
                Assert.That(alpha, Is.LessThan(previous), "h=" + i * .05f);
                Assert.That(previous - alpha, Is.LessThanOrEqualTo(.03f), "h=" + i * .05f);
                previous = alpha;
            }
        }
        [Test] public void Alpha_SeparatesNearBounceFromHighBallClearly()
        {
            Assert.That(BallShadowLook.Alpha(.25f) - BallShadowLook.Alpha(1.5f), Is.GreaterThanOrEqualTo(.30f));
        }
        [Test] public void Alpha_AllocatesNothing()
        {
            float sink = 0; BallShadowLook.Alpha(.3f);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 10000; i++) sink += BallShadowLook.Alpha(i * .001f);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero); Assert.That(sink, Is.GreaterThan(0));
        }

        private GameObject root; private BallSimulationHost host; private LegibilityAids aids; private float radius;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();
            var p = PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text);
            var tuning = new TuningState(p, p); var config = EffectiveConfig.From(p); radius = (float)config.Physics.BallRadius;
            root = new GameObject("Shadow test"); host = root.AddComponent<BallSimulationHost>(); host.enabled = false;
            host.Initialize(new IBallSimulationFactory[] { new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold) }, new SimClock(120, .65));
            host.Launch(config);
            var ball = new GameObject("Ball"); ball.transform.SetParent(root.transform);
            var view = ball.AddComponent<BallView>(); view.Initialize(host, config.Physics.BallRadius);
            var go = new GameObject("Legibility aids"); go.transform.SetParent(root.transform);
            aids = go.AddComponent<LegibilityAids>(); aids.enabled = false; aids.Initialize(host, tuning, view);
        }
        [UnityTearDown] public IEnumerator Cleanup() { UnityEngine.Object.Destroy(root); yield return null; }

        private float PaintedAlpha(Transform shadow)
        {
            var block = new MaterialPropertyBlock(); shadow.GetComponent<Renderer>().GetPropertyBlock(block); return block.GetColor("_BaseColor").a;
        }
        [UnityTest] public IEnumerator Tick_PaintsTheCurveAndLeavesSizePositionAndSimulationUntouched()
        {
            var shadow = aids.transform.Find("Ball shadow"); Assert.That(shadow, Is.Not.Null);
            var state = host.Current; var position = host.RenderPosition; int generation = host.SimulationGeneration;
            foreach (float height in new[] { 0f, .1f, .5f, 1.5f, 4f })
            {
                aids.Tick(new Vector3(2, radius + height, 5), .01f);
                Assert.That(PaintedAlpha(shadow), Is.EqualTo(BallShadowLook.Alpha(height)).Within(1e-4f), "h=" + height);
                float proximity = 1 / (1 + height * .4f);
                Assert.That(shadow.localScale.x, Is.EqualTo(.085f + .085f * proximity).Within(1e-5f), "h=" + height);
                Assert.That(shadow.position, Is.EqualTo(new Vector3(2, .001f, 5)));
            }
            aids.Tick(new Vector3(2, radius - .01f, 5), .01f);
            Assert.That(PaintedAlpha(shadow), Is.EqualTo(BallShadowLook.Alpha(0)).Within(1e-4f));
            Assert.That(host.Current.Position, Is.EqualTo(state.Position)); Assert.That(host.Current.Step, Is.EqualTo(state.Step));
            Assert.That(host.RenderPosition, Is.EqualTo(position)); Assert.That(host.SimulationGeneration, Is.EqualTo(generation));
            yield return null;
        }
    }
}
