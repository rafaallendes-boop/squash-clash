using System.Collections;
using System.Linq;
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
            Assert.That(UnityEngine.Camera.main, Is.Not.Null);
            Assert.That(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator MainScene_HasOneActiveAudioListenerOnMainCamera()
        {
            yield return PlayModeTestWorld.ClearApp();
            yield return SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);

            var camera = UnityEngine.Camera.main;
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.gameObject.scene, Is.EqualTo(SceneManager.GetActiveScene()));

            var activeListeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None)
                .Where(listener => listener.isActiveAndEnabled)
                .ToArray();
            Assert.That(activeListeners, Has.Length.EqualTo(1), "Exactly one AudioListener must be active.");
            Assert.That(activeListeners[0].gameObject, Is.SameAs(camera.gameObject));
        }
    }
}
