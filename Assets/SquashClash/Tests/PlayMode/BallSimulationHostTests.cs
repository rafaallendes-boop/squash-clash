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
    [TestFixture(0),TestFixture(1)]
    public sealed class BallSimulationHostTests {
        private readonly int backend;
        public BallSimulationHostTests(int backend){this.backend=backend;}
        private AnalyticBallSimulationFactory analytic;
        private int LiveInstances=>backend==0?factory.LiveInstances:analytic.LiveInstances;
        private GameObject root; private BallSimulationHost host; private PhysXBallSimulationFactory factory;private Vector3 gravity;
        private EffectiveConfig Default()=>EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text).With("physics.backend",backend));
        [UnitySetUp] public IEnumerator SetUp() {
            yield return PlayModeTestWorld.ClearApp();gravity=UnityEngine.Physics.gravity;
            root=new GameObject("Test host");host=root.AddComponent<BallSimulationHost>();host.enabled=false;
            factory=new PhysXBallSimulationFactory();analytic=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);host.Initialize(new IBallSimulationFactory[]{factory,analytic},new SimClock(120,0.65));
        }
        [UnityTearDown] public IEnumerator TearDown() {
            Object.Destroy(root);yield return null;
            for(int i=0;i<120&&factory.PendingUnloads!=0;i++)yield return null;
            Assert.That(LiveInstances,Is.Zero);Assert.That(factory.PendingUnloads,Is.Zero);Assert.That(factory.LiveInstances,Is.Zero);Assert.That(analytic.LiveInstances,Is.Zero);UnityEngine.Physics.gravity=gravity;
        }
        [UnityTest] public IEnumerator Launch_PredictionStartsFromSimulationState() {
            host.Launch(Default());AssertStateBits(host.Current,host.Prediction.States[0]);
            Assert.That(host.RenderPosition,Is.EqualTo(host.Current.Position));Assert.That(LiveInstances,Is.EqualTo(2));yield return null;
        }
        [UnityTest] public IEnumerator Launch_SameSimulationKey_KeepsInstances() {
            var cfg=Default();host.Launch(cfg);int generation=host.SimulationGeneration;
            host.Launch(EffectiveConfig.From(cfg.Source.With("camera.fovDeg",70)));
            Assert.That(host.SimulationGeneration,Is.EqualTo(generation));Assert.That(LiveInstances,Is.EqualTo(2));yield return null;
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
            Assert.That(host.SimulationGeneration,Is.EqualTo(generation+1));Assert.That(LiveInstances,Is.EqualTo(2));
            Assert.That(UnityEngine.Physics.gravity,Is.EqualTo(backend==0?Vector3.down*12:gravity));yield return null;
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
        [UnityTest] public IEnumerator ManualContinueAfterStep_ZeroAlpha_DoesNotRewind()=>ContinueAfterStep(0,false);
        [UnityTest] public IEnumerator ManualContinueAfterStep_FractionalAlpha_DoesNotRewind()=>ContinueAfterStep(0.5,false);
        [UnityTest] public IEnumerator SystemContinueAfterStep_ZeroAlpha_DoesNotRewind()=>ContinueAfterStep(0,true);
        [UnityTest] public IEnumerator SystemContinueAfterStep_FractionalAlpha_DoesNotRewind()=>ContinueAfterStep(0.5,true);
        private IEnumerator ContinueAfterStep(double fraction,bool system) {
            host.Launch(Default());host.Advance(fraction/120/0.65);host.Clock.Paused=true;
            host.Clock.RequestSingleStep();host.Advance(0);var shown=host.RenderPosition;var authority=host.Current;
            Assert.That(shown,Is.EqualTo(authority.Position));
            if(system) {host.ResumeAfterSystemPause();host.SendMessage("Update");}
            else {host.Clock.Paused=false;host.Advance(0);}
            Assert.That(host.RenderPosition,Is.EqualTo(shown),"Continuar must not rewind the displayed single step.");AssertStateBits(host.Current,authority);
            yield return null;
        }
        [UnityTest] public IEnumerator SystemPauseAndResume_PreserveInterpolatedRenderWithoutChangingAuthority() {
            host.Launch(Default());host.Advance(1.5/120/0.65);var shown=host.RenderPosition;var authority=host.Current;
            host.Clock.Paused=true;host.Advance(30);
            Assert.That(host.RenderPosition,Is.EqualTo(shown),"Pause must freeze the displayed interpolated point.");AssertStateBits(host.Current,authority);
            host.ResumeAfterSystemPause();host.SendMessage("Update");
            Assert.That(host.RenderPosition,Is.EqualTo(shown),"The suppressed resume frame must not rewind presentation.");AssertStateBits(host.Current,authority);yield return null;
        }
        [UnityTest] public IEnumerator ScenarioLaunchAndReplay_PreserveStateAndFullHorizon() {
            var cfg=Default();var initial=new BallState{Position=new Vector3(3.2f,.3f,5),Velocity=Vector3.down,Step=7};
            host.Launch(cfg,initial,720,StopCondition.MaxSteps);
            AssertStateBits(host.Current,initial);Assert.That(host.Prediction.States[host.Prediction.States.Count-1].Step,Is.EqualTo(727));
            host.Advance(1.0/60);host.Replay();AssertStateBits(host.Current,initial);yield return null;
        }
        [UnityTest] public IEnumerator ReleaseForExperiment_DisposesBothAndReplayCreatesNewPair() {
            host.Launch(Default());var initial=host.Current;host.ReleaseForExperiment();
            Assert.That(LiveInstances,Is.Zero);Assert.That(host.Clock.Paused,Is.True);Assert.That(host.IsRunning,Is.False);
            host.Advance(1);AssertStateBits(host.Current,initial);host.Replay();Assert.That(LiveInstances,Is.EqualTo(2));yield return null;
        }
        private static void AssertStateBits(BallState a,BallState b) {
            foreach(var pair in new[]{(a.Position.x,b.Position.x),(a.Position.y,b.Position.y),(a.Position.z,b.Position.z),
                (a.Velocity.x,b.Velocity.x),(a.Velocity.y,b.Velocity.y),(a.Velocity.z,b.Velocity.z)})
                Assert.That(BitConverter.SingleToInt32Bits(pair.Item1),Is.EqualTo(BitConverter.SingleToInt32Bits(pair.Item2)));
            Assert.That(a.Step,Is.EqualTo(b.Step));
        }
    }
}
