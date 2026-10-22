using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Physics;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests {
    public sealed class BallSimulationHostTests {
        private GameObject root; private BallSimulationHost host; private PhysXBallSimulationFactory factory;private Vector3 gravity;
        private static EffectiveConfig Default()=>EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
        [UnitySetUp] public IEnumerator SetUp() {
            yield return PlayModeTestWorld.ClearApp();gravity=UnityEngine.Physics.gravity;
            root=new GameObject("Test host");host=root.AddComponent<BallSimulationHost>();host.enabled=false;
            factory=new PhysXBallSimulationFactory();host.Initialize(new IBallSimulationFactory[]{factory},new SimClock(120,0.65));
        }
        [UnityTearDown] public IEnumerator TearDown() {
            Object.Destroy(root);yield return null;
            for(int i=0;i<120&&factory.PendingUnloads!=0;i++)yield return null;
            Assert.That(factory.LiveInstances,Is.Zero);Assert.That(factory.PendingUnloads,Is.Zero);UnityEngine.Physics.gravity=gravity;
        }
        [UnityTest] public IEnumerator Launch_PredictionStartsFromSimulationState() {
            host.Launch(Default());AssertStateBits(host.Current,host.Prediction.States[0]);
            Assert.That(host.RenderPosition,Is.EqualTo(host.Current.Position));Assert.That(factory.LiveInstances,Is.EqualTo(2));yield return null;
        }
        [UnityTest] public IEnumerator Launch_SameSimulationKey_KeepsInstances() {
            var cfg=Default();host.Launch(cfg);int generation=host.SimulationGeneration;
            host.Launch(EffectiveConfig.From(cfg.Source.With("camera.fovDeg",70)));
            Assert.That(host.SimulationGeneration,Is.EqualTo(generation));Assert.That(factory.LiveInstances,Is.EqualTo(2));yield return null;
        }
        [UnityTest] public IEnumerator Relaunch_ClearsInterpolationAndFraction_AndReplayUsesActiveSnapshot() {
            var cfg=Default();host.Launch(cfg);host.Advance(1.5/120/0.65);
            Assert.That(host.Clock.Alpha,Is.GreaterThan(0));host.Replay();
            Assert.That(host.Clock.Alpha,Is.Zero);Assert.That(host.RenderPosition,Is.EqualTo(cfg.Launch.Position));
            Assert.That(host.Current.Step,Is.Zero);Assert.That(host.ActiveConfig.Hash,Is.EqualTo(cfg.Hash));yield return null;
        }
        [UnityTest] public IEnumerator Launch_GravityChangeDisposesBothBeforeCreatingReplacements() {
            host.Launch(Default());int generation=host.SimulationGeneration;
            host.Launch(EffectiveConfig.From(Default().Source.With("physics.gravity",12)));
            Assert.That(host.SimulationGeneration,Is.EqualTo(generation+1));Assert.That(factory.LiveInstances,Is.EqualTo(2));
            Assert.That(UnityEngine.Physics.gravity.y,Is.EqualTo(-12));yield return null;
        }
        [UnityTest] public IEnumerator Paused_SingleStepAdvancesAndRendersCurrentState_WithoutCatchup() {
            host.Launch(Default());host.Clock.Paused=true;var initial=host.Current;host.Advance(30);AssertStateBits(host.Current,initial);
            host.Clock.RequestSingleStep();host.Advance(30);
            Assert.That(host.Current.Step,Is.EqualTo(1));Assert.That(host.RenderPosition,Is.EqualTo(host.Current.Position));
            host.Advance(30);Assert.That(host.Current.Step,Is.EqualTo(1));Assert.That(host.Clock.DroppedSteps,Is.Zero);yield return null;
        }
        [UnityTest] public IEnumerator RenderInterpolation_DoesNotChangeAuthoritativeState() {
            host.Launch(Default());var start=host.Current;host.Advance(1.5/120/0.65);
            Assert.That(host.Current.Step,Is.EqualTo(1));
            var midpoint=(start.Position+host.Current.Position)*0.5f;
            Assert.That(Vector3.Distance(host.RenderPosition,midpoint),Is.LessThan(1e-6));AssertStateBits(host.Prediction.States[0],start);yield return null;
        }
        [UnityTest] public IEnumerator Flight_StopsOnSecondNonpersistentFloorBounce() {
            host.Launch(Default());int bounces=0;host.ContactRaised+=c=>{if(c.Surface==SquashClash.Core.Court.SurfaceId.Floor&&!c.Persistent)bounces++;};
            for(int i=0;i<600&&host.IsRunning;i++)host.Advance(1.0/60);
            Assert.That(bounces,Is.EqualTo(2));Assert.That(host.IsRunning,Is.False);
            var last=host.Current;host.Advance(30);AssertStateBits(host.Current,last);yield return null;
        }
        private static void AssertStateBits(BallState a,BallState b) {
            foreach(var pair in new[]{(a.Position.x,b.Position.x),(a.Position.y,b.Position.y),(a.Position.z,b.Position.z),
                (a.Velocity.x,b.Velocity.x),(a.Velocity.y,b.Velocity.y),(a.Velocity.z,b.Velocity.z)})
                Assert.That(BitConverter.SingleToInt32Bits(pair.Item1),Is.EqualTo(BitConverter.SingleToInt32Bits(pair.Item2)));
            Assert.That(a.Step,Is.EqualTo(b.Step));
        }
    }
}
