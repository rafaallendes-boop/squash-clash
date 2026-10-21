using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace SquashClash.Runtime.Tests
{
    public sealed class BootScreenTests
    {
        [UnityTest]
        public IEnumerator MainScene_ShowsBuildAndLiveFps()
        {
            yield return SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
            yield return null;
            var document = Object.FindFirstObjectByType<UIDocument>();
            Assert.That(document, Is.Not.Null, "The boot screen must exist in the built scene.");
            var root = document.rootVisualElement;
            Assert.That(root.Q<Label>("boot-title").text, Is.EqualTo("Squash Clash — First Ball"));
            Assert.That(root.Q<Label>("build-label").text, Does.StartWith("0.1.0+"));
            Assert.That(Application.targetFrameRate, Is.EqualTo(60));
            yield return new WaitForSecondsRealtime(0.7f);
            Assert.That(root.Q<Label>("fps-label").text, Does.Match(@"^[1-9][0-9]* FPS$"));
        }
    }
}
