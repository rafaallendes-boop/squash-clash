using System.Collections;
using NUnit.Framework;
using SquashClash.DevPanel;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests
{
    // DEV build comfort (2026-11-21): the bottom DEV bar must not stay on screen while playing.
    public sealed class DevPanelUnobtrusiveTests
    {
        private VisualElement root;private DevPanelView view;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;yield return null;
            var app=AppBootstrap.Instance;root=app.GetComponent<UIDocument>().rootVisualElement;view=Object.FindFirstObjectByType<DevPanelView>();
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return PlayModeTestWorld.ClearApp();}
        [UnityTest] public IEnumerator ClosedByDefault_BarIsInvisibleAndToggleIsSmallAndFaint()
        {
            Assert.That(view.IsOpen,Is.False);
            var bar=root.Q("dev-bar");var toggle=root.Q<Button>("dev-toggle");
            Assert.That(bar,Is.Not.Null);Assert.That(toggle,Is.Not.Null);
            Assert.That(bar.resolvedStyle.visibility,Is.EqualTo(Visibility.Hidden),"bottom DEV bar while closed");
            Assert.That(toggle.resolvedStyle.width,Is.LessThanOrEqualTo(48f));Assert.That(toggle.resolvedStyle.height,Is.LessThanOrEqualTo(32f));
            Assert.That(toggle.resolvedStyle.opacity,Is.LessThanOrEqualTo(.6f));
            yield return null;
        }
        [UnityTest] public IEnumerator ClosedToggle_DoesNotOverlapHeaderHudScoreOrEnergyBars()
        {
            yield return null;
            var toggle=root.Q<Button>("dev-toggle").worldBound;
            foreach(var name in new[]{"boot-header","rally-hud","rally-score","rally-energy-human","rally-energy-rival"})
            {
                var element=root.Q(name);if(element==null||element.resolvedStyle.display==DisplayStyle.None)continue;
                var bounds=element.worldBound;if(float.IsNaN(bounds.width)||bounds.width<=0||bounds.height<=0)continue;
                Assert.That(bounds.Overlaps(toggle),Is.False,name+" "+bounds+" vs toggle "+toggle);
            }
        }
        [UnityTest] public IEnumerator OpeningThePanel_ShowsTheBar_AndClosingHidesItAgain()
        {
            var bar=root.Q("dev-bar");
            view.Toggle();yield return null;yield return null;
            Assert.That(view.IsOpen,Is.True);Assert.That(bar.resolvedStyle.visibility,Is.EqualTo(Visibility.Visible));
            view.Toggle();yield return null;yield return null;
            Assert.That(view.IsOpen,Is.False);Assert.That(bar.resolvedStyle.visibility,Is.EqualTo(Visibility.Hidden));
        }
    }
}
