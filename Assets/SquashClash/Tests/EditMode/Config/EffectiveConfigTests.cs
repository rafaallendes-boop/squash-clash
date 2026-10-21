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
        public void EffectiveConfig_ExternalCourtMutationCannotChangeSnapshot()
        {
            var a = EffectiveConfig.From(ConfigFixtures.Default());
            a.Court.TinTop = 99;
            Assert.That(a.Court.TinTop, Is.EqualTo(0.48f));
            Assert.That(a.Source.Get("court.tinTop"), Is.EqualTo(0.48));
        }
    }
}
