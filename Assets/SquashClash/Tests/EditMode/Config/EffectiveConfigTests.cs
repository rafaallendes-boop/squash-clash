using NUnit.Framework;
using SquashClash.Core.Config;

namespace SquashClash.Core.Tests.Config
{
    public sealed class EffectiveConfigTests
    {
        [Test] public void FirstBallAcceptance_DefaultRetainsApprovedPhysicalGateAndNonPhysicalDefaults()
        {
            var current=EffectiveConfig.From(ConfigFixtures.Default());
            Assert.That(current.Hash,Is.EqualTo("0d39e6016bd8"));
            Assert.That(current.SimulationKey,Is.EqualTo("166a96e319c7"));
            // I4d injected the P2 factory while its source preset still said backend=0.
            // This raw key is historical; the actual P2 gate uses the explicit factory selection.
            Assert.That(EffectiveConfig.From(current.Source.With("physics.backend",0)).SimulationKey,Is.EqualTo("04372f672e9e"));
            Assert.That(EffectiveConfig.From(current.Source.With("launcher.repeatSeconds",3).With("camera.mode",1).With("aids.ballVisualScale",2)).SimulationKey,
                Is.EqualTo(current.SimulationKey));
            UnityEngine.Debug.Log("[SquashClash] acceptance Default="+current.Hash+" SimulationKey="+current.SimulationKey+" gateRaw=04372f672e9e gateEffectiveP2=166a96e319c7");
        }
        [Test]
        public void EffectiveConfig_SimulationKeyIgnoresCameraButNotCourt()
        {
            var p = ConfigFixtures.Default();
            var a = EffectiveConfig.From(p);
            Assert.That(a.Hash, Is.EqualTo(p.Hash));
            Assert.That(a.SimulationKey, Is.EqualTo(EffectiveConfig.From(p.With("camera.fovDeg", 70)).SimulationKey));
            Assert.That(a.SimulationKey, Is.Not.EqualTo(EffectiveConfig.From(p.With("court.tinTop", 0.43)).SimulationKey));
            Assert.That(a.Court.TinTop, Is.EqualTo(0.48f));
        }

        [Test]
        public void EffectiveConfig_PhysicsAndPaceUseSourceValues() {
            var a=EffectiveConfig.From(ConfigFixtures.Default());
            Assert.That(a.Tempo,Is.EqualTo(0.65)); Assert.That(a.LaunchScale,Is.EqualTo(1));
            Assert.That(a.Physics.Gravity,Is.EqualTo(9.81f)); Assert.That(a.Physics.StepHz,Is.EqualTo(120));
            Assert.That(a.Physics.MaxImpactsPerStep,Is.EqualTo(4));
            var changed=EffectiveConfig.From(a.Source.With("physics.gravity",12).With("pace.tempo",0.8));
            Assert.That(changed.Physics.Gravity,Is.EqualTo(12)); Assert.That(changed.Tempo,Is.EqualTo(0.8));
            Assert.That(changed.SimulationKey,Is.Not.EqualTo(a.SimulationKey));
            changed.Physics.Gravity=99; Assert.That(changed.Physics.Gravity,Is.EqualTo(12));
        }

        [Test]
        public void EffectiveConfig_MaterialsAndBackendComeFromPreset() {
            var a=EffectiveConfig.From(ConfigFixtures.Default());
            Assert.That(a.Backend,Is.EqualTo("P2-Analytic")); Assert.That(a.Physics.Floor.Restitution,Is.EqualTo(0.45f));
            Assert.That(a.Physics.FrontWall.Restitution,Is.EqualTo(0.55f)); Assert.That(a.Physics.SideWalls.TangentialFriction,Is.EqualTo(0.2f));
            var changed=EffectiveConfig.From(a.Source.With("physics.floor.restitution",0.6));
            Assert.That(changed.Physics.Floor.Restitution,Is.EqualTo(0.6f)); changed.Physics.Floor.Restitution=99;
            Assert.That(changed.Physics.Floor.Restitution,Is.EqualTo(0.6f));
        }

        [Test]
        public void BackendSelection_P1RemainsConfigurableAndRoundTripsAlongsideP2()
        {
            var defaults = ConfigFixtures.Default();
            var p1 = defaults.With("physics.backend", 0);
            var p2 = defaults.With("physics.backend", 1);
            Assert.That(EffectiveConfig.From(p1).Backend, Is.EqualTo("P1-PhysX"));
            Assert.That(EffectiveConfig.From(p2).Backend, Is.EqualTo("P2-Analytic"));
            Assert.That(EffectiveConfig.From(p1).SimulationKey, Is.Not.EqualTo(EffectiveConfig.From(p2).SimulationKey));
            foreach (var set in new[] { p1, p2 })
            {
                var json = PresetCodec.ToJson(PresetCodec.FromSet(set, "backend selection", "test"));
                Assert.That(PresetCodec.LoadDefault(json).Hash, Is.EqualTo(set.Hash));
            }
            Assert.That(defaults.GetInt("physics.backend"), Is.EqualTo(1), "Defaults come only from DefaultPreset.json.");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => defaults.With("physics.backend", 2));
        }

        [Test]
        public void EffectiveConfig_LaunchUsesPresetAndKeepsPaceSeparate() {
            var a=EffectiveConfig.From(ConfigFixtures.Default());
            Assert.That(a.Launch.Position,Is.EqualTo(new UnityEngine.Vector3(3.2f,1,7)));
            Assert.That(a.Launch.SpeedRealMps,Is.EqualTo(15));Assert.That(a.Launch.Direction.y,Is.EqualTo(0.17364818f).Within(1e-6));
            Assert.That(a.Launch.Direction.z,Is.EqualTo(-0.98480775f).Within(1e-6));
            var b=EffectiveConfig.From(a.Source.With("pace.tempo",0.8).With("launcher.speed",20));
            Assert.That(b.SimulationKey,Is.EqualTo(a.SimulationKey));Assert.That(b.Launch.SpeedRealMps,Is.EqualTo(20));Assert.That(b.Tempo,Is.EqualTo(0.8));
        }

        [Test]
        public void EffectiveConfig_ExternalCourtMutationCannotChangeSnapshot()
        {
            var a = EffectiveConfig.From(ConfigFixtures.Default());
            a.Court.TinTop = 99;
            Assert.That(a.Court.TinTop, Is.EqualTo(0.48f));
            Assert.That(a.Source.Get("court.tinTop"), Is.EqualTo(0.48));
        }
    }
}
