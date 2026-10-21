using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SquashClash.Experiments
{
    public sealed class BootstrapSmokeTests
    {
        [UnityTest]
        public IEnumerator ExperimentRunner_EntersPlayMode()
        {
            yield return null;
            Assert.That(Application.isPlaying, Is.True);
        }
    }
}
