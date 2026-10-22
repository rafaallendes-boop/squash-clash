using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;

namespace SquashClash.Core.Tests.Config
{
    public sealed class PresetCodecTests
    {
        [Test]
        public void LoadDefault_MissingKey_Throws_ListingKey()
        {
            var file = ConfigFixtures.File();
            file.values.RemoveAll(e => e.key == "court.tinTop");
            var error = Assert.Throws<InvalidDataException>(() => PresetCodec.LoadDefault(PresetCodec.ToJson(file)));
            Assert.That(error.Message, Does.Contain("court.tinTop"));
        }

        [Test]
        public void LoadDefault_UnknownKeyOrOutOfRange_ThrowsWithAllProblems()
        {
            var file = ConfigFixtures.File();
            file.values.Add(new PresetEntry { key = "bogus.key", value = 1 });
            file.values.Single(e => e.key == "camera.height").value = 99;
            file.values.RemoveAll(e => e.key == "court.tinTop");
            var error = Assert.Throws<InvalidDataException>(() => PresetCodec.LoadDefault(PresetCodec.ToJson(file)));
            Assert.That(error.Message, Does.Contain("bogus.key").And.Contain("camera.height").And.Contain("court.tinTop"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{not json")]
        [TestCase("{}")]
        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{\"schemaVersion\":1,\"values\":null}")]
        [TestCase("{\"schemaVersion\":1,\"values\":[]},garbage")]
        [TestCase("{\"schemaVersion\":1,\"values\":[{\"key\":\"camera.height\"}]}")]
        [TestCase("{\"schemaVersion\":1,\"values\":[{\"key\":\"camera.height\",\"value\":\"4.5\"}]}")]
        [TestCase("{\"schemaVersion\":1,\"values\":[{\"key\":\"camera.height\",\"value\":4.5,}]}")]
        [TestCase("{\"schemaVersion\":1.5,\"values\":[]}")]
        public void TryParse_Malformed_ReturnsFalseWithoutThrowing(string json)
        {
            Assert.That(PresetCodec.TryParse(json, out var file, out var error), Is.False);
            Assert.That(file, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        [TestCase(0)]
        [TestCase(2)]
        public void TryParse_OtherSchema_IsRejected(int schema)
        {
            var json = "{\"schemaVersion\":" + schema + ",\"values\":[]}";
            Assert.That(PresetCodec.TryParse(json, out _, out var error), Is.False);
            Assert.That(error, Does.Contain("schema"));
        }

        [Test]
        public void TryParse_DuplicateKeys_IsRejected()
        {
            const string json = "{\"schemaVersion\":1,\"values\":[{\"key\":\"camera.fovDeg\",\"value\":62},{\"key\":\"camera.fovDeg\",\"value\":70}]}";
            Assert.That(PresetCodec.TryParse(json, out _, out var error), Is.False);
            Assert.That(error, Does.Contain("camera.fovDeg"));
        }

        [Test]
        public void Overlay_FollowsTable()
        {
            var original = ConfigFixtures.Default();
            var fovBefore = original.Get("camera.fovDeg");
            var heightBefore = original.Get("camera.height");
            var file = new PresetFile { values = new List<PresetEntry> {
                new PresetEntry { key = "bogus.key", value = 1 },
                new PresetEntry { key = "camera.fovDeg", value = double.PositiveInfinity },
                new PresetEntry { key = "camera.height", value = 99 },
                new PresetEntry { key = "court.tinTop", value = 0.5 } } };
            var warnings = new List<string>();
            var result = PresetCodec.Overlay(original, file, warnings);
            Assert.That(warnings.Count, Is.EqualTo(4));
            Assert.That(result.Get("camera.fovDeg"), Is.EqualTo(fovBefore));
            Assert.That(result.Get("camera.height"), Is.EqualTo(8));
            Assert.That(result.Get("court.tinTop"), Is.EqualTo(0.48));
            Assert.That(result.Get("camera.backOffset"), Is.EqualTo(original.Get("camera.backOffset")));
            Assert.That(original.Get("camera.height"), Is.EqualTo(heightBefore));
        }

        [TestCase("court.tinTop", 0.5)]
        [TestCase("placeholders.visible", 0.5)]
        public void Overlay_InvalidDiscreteValueKeepsBase(string key, double value)
        {
            var original = ConfigFixtures.Default().With("court.tinTop", 0.43).With("placeholders.visible", 0);
            var file = new PresetFile { values = new List<PresetEntry> { new PresetEntry { key = key, value = value } } };
            var warnings = new List<string>();
            var result = PresetCodec.Overlay(original, file, warnings);
            Assert.That(result.Get(key), Is.EqualTo(original.Get(key)));
            Assert.That(warnings.Count, Is.EqualTo(1));
        }

        [Test]
        public void Hash_IndependentOfEntryOrder_And12Hex()
        {
            var original = ConfigFixtures.Default();
            var file = PresetCodec.FromSet(original, "quoted \"name\"", "test");
            file.values.Reverse();
            var reparsed = PresetCodec.LoadDefault(PresetCodec.ToJson(file));
            Assert.That(reparsed.Hash, Is.EqualTo(original.Hash));
            Assert.That(original.Hash, Does.Match("^[0-9a-f]{12}$"));
            Assert.That(original.HashOf("court."), Is.EqualTo("c389d15acccd"));
            var before = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-CL");
                Assert.That(PresetCodec.LoadDefault(PresetCodec.ToJson(file)).Hash, Is.EqualTo(original.Hash));
            }
            finally { CultureInfo.CurrentCulture = before; }
        }
    }
}
