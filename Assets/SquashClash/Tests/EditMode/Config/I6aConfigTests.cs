using NUnit.Framework;
using SquashClash.Core.Config;

namespace SquashClash.Core.Tests.Config
{
    public sealed class I6aConfigTests
    {
        [TestCase("camera.mode", 0)]
        [TestCase("camera.followDamping", 0.3)]
        [TestCase("camera.followMaxOffset", 1.5)]
        [TestCase("aids.shadow", 1)]
        [TestCase("aids.trail", 1)]
        [TestCase("aids.impactMarks", 1)]
        [TestCase("aids.prediction", 0)]
        [TestCase("aids.trailSeconds", 0.15)]
        [TestCase("aids.ballVisualScale", 1)]
        public void LegibilityDefaults_AreCataloguedAndApplyImmediately(string key, double expected)
        {
            Assert.That(ParamCatalog.TryGet(key, out var def), Is.True, key);
            Assert.That(def.Apply, Is.EqualTo(ApplyClass.Immediate));
            var defaults = ConfigFixtures.Default();
            Assert.That(defaults.Get(key), Is.EqualTo(expected));
            Assert.That(defaults.Get("physics.backend"), Is.EqualTo(1));
        }
    }
}
