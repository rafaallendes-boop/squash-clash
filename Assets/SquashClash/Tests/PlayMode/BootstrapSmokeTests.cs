using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SquashClash.Runtime.Tests
{
    public sealed class BootstrapSmokeTests
    {
        [UnityTest]
        public IEnumerator MainScene_LoadsWithCameraAndRenderPipeline()
        {
            yield return SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
            Assert.That(Camera.main, Is.Not.Null);
            Assert.That(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline, Is.Not.Null);
        }
    }
}
