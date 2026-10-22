using NUnit.Framework;
using SquashClash.Core.Config;

namespace SquashClash.Core.Tests.Config
{
    public sealed class EffectiveConfigTests
    {
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
            Assert.That(a.Backend,Is.EqualTo("P1-PhysX")); Assert.That(a.Physics.Floor.Restitution,Is.EqualTo(0.45f));
            Assert.That(a.Physics.FrontWall.Restitution,Is.EqualTo(0.55f)); Assert.That(a.Physics.SideWalls.TangentialFriction,Is.EqualTo(0.2f));
            var changed=EffectiveConfig.From(a.Source.With("physics.floor.restitution",0.6));
            Assert.That(changed.Physics.Floor.Restitution,Is.EqualTo(0.6f)); changed.Physics.Floor.Restitution=99;
            Assert.That(changed.Physics.Floor.Restitution,Is.EqualTo(0.6f));
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
