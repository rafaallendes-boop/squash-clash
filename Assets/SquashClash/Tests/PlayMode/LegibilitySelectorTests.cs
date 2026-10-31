using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    public sealed class LegibilitySelectorTests
    {
        private string selection,backup;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();
            selection=Path.Combine(Application.persistentDataPath,"presets","last-active.txt");
            backup=File.Exists(selection)?File.ReadAllText(selection):null;File.Delete(selection);
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            var app=AppBootstrap.Instance;app.Tuning.Set("practice.enabled",0);app.LaunchCurrent();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return PlayModeTestWorld.ClearApp();
            if(backup==null)File.Delete(selection);else File.WriteAllText(selection,backup);
        }
        [UnityTest] public IEnumerator Selector_UsesAllSixLaunchesAndPreservesConfig_P2() => Check(1);
        [UnityTest] public IEnumerator Selector_UsesAllSixLaunchesAndPreservesConfig_P1() => Check(0);
        private IEnumerator Check(int backend)
        {
            var app=AppBootstrap.Instance;app.Host.enabled=false;app.GetComponent<ResumeOverlay>().Continue();
            var root=app.GetComponent<UIDocument>().rootVisualElement;
            var selector=root.Q<DropdownField>("legibility-launch");Assert.That(selector,Is.Not.Null,"L1–L6 selector in the bar");
            Assert.That(selector.choices.Count,Is.EqualTo(7));Assert.That(selector.value,Is.EqualTo("Manual"));
            app.Tuning.Set("physics.backend",backend);app.Tuning.Set("physics.floor.restitution",.6);
            var expected=LaunchPresets.All(app.Config.Court,app.Config.Physics.Gravity);
            foreach(var e in expected)
            {
                selector.value=selector.choices.Single(c=>c.StartsWith(e.Id+" "));
                var initial=Launcher.ToState(e.Launch,app.Config.LaunchScale);
                Assert.That(Vector3.Distance(app.Host.Prediction.States[0].Position,initial.Position),Is.LessThan(.00001f));
                Assert.That(Vector3.Distance(app.Host.Prediction.States[0].Velocity,initial.Velocity),Is.LessThan(.0001f));
                Assert.That(app.Config.Source.Get("human.x"),Is.EqualTo((double)e.HumanXZ.x));
                Assert.That(app.Config.Source.Get("rival.z"),Is.EqualTo((double)e.RivalXZ.y));
                Assert.That(app.Host.ActiveConfig.Backend,Is.EqualTo(backend==1?"P2-Analytic":"P1-PhysX"));
                Assert.That(app.Config.Physics.Floor.Restitution,Is.EqualTo(.6f));Assert.That(app.Tuning.PendingKeys,Is.Empty);
                Assert.That(Object.FindFirstObjectByType<PlaceholderPlayers>().transform.Find("Human").localPosition,
                    Is.EqualTo(new Vector3(e.HumanXZ.x,.9f,e.HumanXZ.y)));
                app.Tuning.Set("camera.mode",1);yield return null;
                Assert.That(selector.value,Does.StartWith(e.Id+" "),"Camera changes preserve launch selection");
            }
            app.Tuning.Set("launcher.speed",19);yield return null;Assert.That(selector.value,Is.EqualTo("Manual"));
            Assert.That(File.Exists(selection),Is.False,"Selection must not write or replace saved presets");
        }
    }
}
