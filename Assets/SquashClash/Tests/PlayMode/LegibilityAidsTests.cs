using System.Collections;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.Ball;
using UnityEngine;
using UnityEngine.TestTools;

namespace SquashClash.Runtime.Tests
{
    public sealed class LegibilityAidsTests
    {
        private GameObject root;
        private BallSimulationHost host;
        private BallView view;
        private LegibilityAids aids;
        private TuningState tuning;
        private EffectiveConfig config;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();
            var p = PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text);
            tuning = new TuningState(p, p); config = EffectiveConfig.From(p);
            root = new GameObject("Aids test"); host = root.AddComponent<BallSimulationHost>(); host.enabled = false;
            host.Initialize(new IBallSimulationFactory[] { new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold) }, new SimClock(120, .65));
            host.Launch(config);
            var ball = new GameObject("Ball"); ball.transform.SetParent(root.transform); view = ball.AddComponent<BallView>(); view.Initialize(host, config.Physics.BallRadius);
            var go = new GameObject("Legibility aids"); go.transform.SetParent(root.transform); aids = go.AddComponent<LegibilityAids>(); aids.enabled = false;
            aids.Initialize(host, tuning, view);
        }
        [UnityTearDown] public IEnumerator Cleanup() { Object.Destroy(root); yield return null; }

        [UnityTest] public IEnumerator Shadow_FollowsBallXZAndRemainsVisibleAsHeightIncreases()
        {
            aids.Tick(new Vector3(2, 3, 5), .01f);
            var shadow = aids.transform.Find("Ball shadow"); Assert.That(shadow, Is.Not.Null);
            Assert.That(shadow.position, Is.EqualTo(new Vector3(2, .001f, 5)));
            float size = shadow.localScale.x; var block = new MaterialPropertyBlock();
            shadow.GetComponent<Renderer>().GetPropertyBlock(block); float alpha = block.GetColor("_BaseColor").a;
            aids.Tick(new Vector3(4, 6, 7), .01f);
            Assert.That(shadow.position, Is.EqualTo(new Vector3(4, .001f, 7)));
            Assert.That(shadow.localScale.x, Is.GreaterThan(0).And.LessThan(size));
            shadow.GetComponent<Renderer>().GetPropertyBlock(block);
            Assert.That(block.GetColor("_BaseColor").a, Is.GreaterThan(0).And.LessThan(alpha));
            yield return null;
        }
        [UnityTest] public IEnumerator ShadowToggle_AppliesImmediatelyWithoutRelaunch()
        {
            var shadow = aids.transform.Find("Ball shadow"); Assert.That(shadow, Is.Not.Null);
            tuning.Set("aids.shadow", 0); Assert.That(shadow.gameObject.activeSelf, Is.False);
            tuning.Set("aids.shadow", 1); Assert.That(shadow.gameObject.activeSelf, Is.True);
            Assert.That(tuning.PendingKeys, Is.Empty); yield return null;
        }
        [UnityTest] public IEnumerator BallVisualScale_ChangesOnlyDrawingAndKeepsPhysicsAndPrediction()
        {
            var state = host.Current; var prediction = host.Prediction; int generation = host.SimulationGeneration;
            tuning.Set("aids.ballVisualScale", 2);
            Assert.That(view.transform.localScale, Is.EqualTo(Vector3.one * .08f));
            Assert.That(host.ActiveConfig.Physics.BallRadius, Is.EqualTo(.020f));
            Assert.That(host.Current.Position, Is.EqualTo(state.Position)); Assert.That(host.Current.Velocity, Is.EqualTo(state.Velocity));
            Assert.That(host.Current.Step, Is.EqualTo(state.Step)); Assert.That(host.SimulationGeneration, Is.EqualTo(generation));
            Assert.That(host.Prediction, Is.SameAs(prediction)); Assert.That(root.GetComponentsInChildren<Collider>(), Is.Empty);
            yield return null;
        }
        [UnityTest] public IEnumerator Trail_UsesRealDurationAndImmediateToggleAndLength()
        {
            var trail = aids.GetComponentInChildren<TrailRenderer>(); Assert.That(trail, Is.Not.Null);
            Assert.That(trail.time, Is.EqualTo(.15f)); Assert.That(trail.autodestruct, Is.False);
            host.Clock.DebugSlowMotion = .1;
            tuning.Set("aids.trailSeconds", .3); Assert.That(trail.time, Is.EqualTo(.3f));
            trail.AddPosition(Vector3.zero); trail.AddPosition(Vector3.one);
            tuning.Set("aids.trail", 0); Assert.That(trail.enabled, Is.False); Assert.That(trail.positionCount, Is.Zero);
            tuning.Set("aids.trail", 1); Assert.That(trail.enabled, Is.True); Assert.That(trail.emitting, Is.True);
            tuning.Set("aids.trailSeconds", 0); Assert.That(trail.enabled, Is.False); yield return null;
        }
        private MeshRenderer[] Marks() => aids.GetComponentsInChildren<MeshRenderer>().Where(r => r.name == "Wall impact").ToArray();
        private static ContactEvent Contact(SurfaceId surface, bool persistent = false) => new ContactEvent(surface,
            new Vector3(2, 1, 0), Vector2.one, Vector3.forward, 1, .001f, ContactTiming.Exact, Vector3.back, persistent);
        [UnityTest] public IEnumerator ImpactMarks_OnlyWallImpactsAndAtMostSixAndImmediateToggle()
        {
            for (int i = 0; i < 8; i++) aids.RecordContact(Contact(SurfaceId.FrontWall));
            aids.RecordContact(Contact(SurfaceId.Floor));
            for (int i = 0; i < 3; i++) aids.RecordContact(Contact(SurfaceId.BackWall, true));
            Assert.That(Marks().Length, Is.EqualTo(6));
            foreach (var mark in Marks())
            {
                Assert.That(Vector3.Dot(mark.transform.forward, Vector3.forward), Is.EqualTo(1).Within(.00001));
                Assert.That(mark.transform.position.z, Is.GreaterThan(0).And.LessThan(.01f));
            }
            tuning.Set("aids.impactMarks", 0); Assert.That(Marks(), Is.Empty);
            aids.RecordContact(Contact(SurfaceId.LeftWall)); Assert.That(Marks(), Is.Empty);
            tuning.Set("aids.impactMarks", 1); aids.RecordContact(Contact(SurfaceId.RightWall)); Assert.That(Marks().Length, Is.EqualTo(1));
            yield return null;
        }
        [UnityTest] public IEnumerator ImpactMarks_FadeAndExpireAfterOneAndAHalfRealSeconds()
        {
            aids.RecordContact(Contact(SurfaceId.FrontWall)); Assert.That(Marks().Length, Is.EqualTo(1));
            var mark = Marks()[0]; var block = new MaterialPropertyBlock(); mark.GetPropertyBlock(block); float alpha = block.GetColor("_BaseColor").a;
            host.Clock.Paused = true; host.Clock.DebugSlowMotion = .1;
            aids.Tick(host.RenderPosition, .75f); mark.GetPropertyBlock(block);
            Assert.That(block.GetColor("_BaseColor").a, Is.EqualTo(alpha / 2).Within(.00001));
            aids.Tick(host.RenderPosition, .76f); Assert.That(Marks(), Is.Empty); yield return null;
        }
        [UnityTest] public IEnumerator Prediction_ToggleDrawsTheIsolatedHostTrajectoryWithoutChangingIt()
        {
            var line = aids.GetComponentInChildren<LineRenderer>(); Assert.That(line, Is.Not.Null);
            Assert.That(line.enabled, Is.False); var prediction = host.Prediction;
            tuning.Set("aids.prediction", 1); Assert.That(line.enabled, Is.True);
            Assert.That(line.useWorldSpace, Is.True); Assert.That(line.positionCount, Is.EqualTo(prediction.States.Count));
            for (int i = 0; i < line.positionCount; i++) Assert.That(line.GetPosition(i), Is.EqualTo(prediction.States[i].Position));
            Assert.That(host.Prediction, Is.SameAs(prediction));
            tuning.Set("aids.prediction", 0); Assert.That(line.enabled, Is.False); Assert.That(line.positionCount, Is.Zero); yield return null;
        }
        [UnityTest] public IEnumerator HostContactEvents_CreateWallMarks()
        {
            host.Launch(config, new BallState { Position = new Vector3(2, 1, .03f), Velocity = Vector3.back * 2 }, 30, StopCondition.MaxSteps);
            host.Advance(2.0 / 120 / .65);
            Assert.That(Marks().Length, Is.EqualTo(1)); yield return null;
        }
        [UnityTest] public IEnumerator Relaunch_ClearsOldTrailAndMarksAndRefreshesPrediction()
        {
            tuning.Set("aids.prediction", 1);
            aids.RecordContact(Contact(SurfaceId.FrontWall));
            var trail = aids.GetComponentInChildren<TrailRenderer>(); trail.AddPosition(Vector3.zero); trail.AddPosition(Vector3.one);
            host.Launch(config, new BallState { Position = new Vector3(1, 2, 4), Velocity = Vector3.back }, 30, StopCondition.MaxSteps);
            Assert.That(new[] { Marks().Length, trail.positionCount,
                aids.GetComponentInChildren<LineRenderer>().positionCount - host.Prediction.States.Count }, Is.EqualTo(new[] { 0, 0, 0 }));
            yield return null;
        }
        [UnityTest] public IEnumerator DestroyingAids_ReleasesOwnedDrawingAndEventSubscriptions()
        {
            Object.Destroy(aids); yield return null;
            Assert.DoesNotThrow(() => { tuning.Set("aids.shadow", 0); host.Replay(); host.Advance(.02); });
            Assert.That(root.GetComponentsInChildren<TrailRenderer>(), Is.Empty);
            Assert.That(root.GetComponentsInChildren<LineRenderer>(), Is.Empty); yield return null;
        }
    }
}
