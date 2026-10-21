using NUnit.Framework;
using UnityEngine;

namespace SquashClash.Core.Tests
{
    public sealed class BootstrapSmokeTests
    {
        [Test]
        public void RegressionRunner_UsesPinnedEditor()
        {
            Assert.That(Application.unityVersion, Is.EqualTo("6000.3.27f1"));
        }
    }
}
