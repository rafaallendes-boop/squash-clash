using System;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;

namespace SquashClash.Core.Tests.Config
{
    public sealed class ParamCatalogTests
    {
        [Test]
        public void DefaultPreset_IsCompleteAndValid()
        {
            var p = ConfigFixtures.Default();
            Assert.That(p.Ordered.Count, Is.EqualTo(24));
            Assert.That(p.Ordered.Select(e => e.Key), Is.EquivalentTo(ParamCatalog.All.Select(d => d.Key)));
            Assert.That(p.Get("court.tinTop"), Is.EqualTo(0.48));
            Assert.That(p.GetBool("placeholders.visible"), Is.True);
        }

        [Test]
        public void Catalog_ChoiceAndBoolHaveExplicitValues()
        {
            foreach (var def in ParamCatalog.All)
            {
                Assert.That(ParamCatalog.TryGet(def.Key, out var found), Is.True);
                Assert.That(found.Key, Is.EqualTo(def.Key));
                Assert.That(def.OperativeFor("P1"), Is.EqualTo(def.Key != "physics.maxImpactsPerStep"));
                if (def.Kind == ParamKind.Choice)
                {
                    Assert.That(def.Choices.Length, Is.GreaterThanOrEqualTo(1));
                    Assert.That(def.Choices.Select(c => c.Value).Distinct().Count(), Is.EqualTo(def.Choices.Length));
                }
                if (def.Kind == ParamKind.Bool) Assert.That(def.Choices, Is.Null.Or.Empty);
            }
            Assert.That(ParamCatalog.TryGet("physics.not-yet-authorized", out _), Is.False);
        }

        [TestCase("placeholders.visible", 0.5)]
        [TestCase("camera.height", 99)]
        [TestCase("camera.height", double.NaN)]
        [TestCase("court.tinTop", 0.45)]
        public void With_RejectsInvalidValuesWithoutMutatingSource(string key, double value)
        {
            var p = ConfigFixtures.Default();
            var before = p.Hash;
            Assert.Throws<ArgumentOutOfRangeException>(() => p.With(key, value));
            Assert.That(p.Hash, Is.EqualTo(before));
        }

        [Test]
        public void With_ValidValueLeavesSourceUnchanged()
        {
            var original = ConfigFixtures.Default();
            var heightBefore = original.Get("camera.height");
            var changed = original.With("camera.height", 7);
            Assert.That(changed.Get("camera.height"), Is.EqualTo(7));
            Assert.That(original.Get("camera.height"), Is.EqualTo(heightBefore));
        }
    }
}
