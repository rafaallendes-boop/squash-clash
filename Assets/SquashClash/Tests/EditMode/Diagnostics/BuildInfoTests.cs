using SquashClash.Core.Diagnostics;
using NUnit.Framework;

namespace SquashClash.Core.Tests
{
    public sealed class BuildInfoTests
    {
        [TestCase(true, "0.1.0+a1b2c3d* dev")]
        [TestCase(false, "0.1.0+a1b2c3d dev")]
        public void FromJson_ReadsAllFields_AndLabelMarksDirty(bool dirty, string expected)
        {
            var json = "{\"version\":\"0.1.0\",\"gitHash\":\"a1b2c3d\",\"gitDirty\":"
                + (dirty ? "true" : "false")
                + ",\"profile\":\"dev\",\"builtAtUtc\":\"2026-10-21T00:00:00Z\",\"unityVersion\":\"6000.3.27f1\"}";
            var info = BuildInfo.FromJson(json);
            Assert.That(info.version, Is.EqualTo("0.1.0"));
            Assert.That(info.gitHash, Is.EqualTo("a1b2c3d"));
            Assert.That(info.profile, Is.EqualTo("dev"));
            Assert.That(info.builtAtUtc, Is.EqualTo("2026-10-21T00:00:00Z"));
            Assert.That(info.unityVersion, Is.EqualTo("6000.3.27f1"));
            Assert.That(info.gitDirty, Is.EqualTo(dirty));
            Assert.That(info.Label, Is.EqualTo(expected));
        }
    }
}
