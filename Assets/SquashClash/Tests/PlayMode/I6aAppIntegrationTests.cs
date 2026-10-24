using System.Collections;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SquashClash.Runtime.Tests
{
    public sealed class I6aAppIntegrationTests
    {
        private string selection, backup;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();
            selection = Path.Combine(Application.persistentDataPath, "presets", "last-active.txt");
            backup = File.Exists(selection) ? File.ReadAllText(selection) : null; File.Delete(selection);
            yield return SceneManager.LoadSceneAsync("Main"); yield return null; yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return PlayModeTestWorld.ClearApp();
            if (backup == null) File.Delete(selection); else File.WriteAllText(selection, backup);
        }
        [UnityTest] public IEnumerator Main_ImmediateAidsUseTheSameHostAndDoNotMutatePhysics()
        {
            var app = AppBootstrap.Instance; app.Host.enabled = false;
            var aids = app.GetComponent<LegibilityAids>(); Assert.That(aids, Is.Not.Null);
            var view = Object.FindFirstObjectByType<BallView>(); var state = app.Host.Current; var prediction = app.Host.Prediction;
            app.Tuning.Set("aids.ballVisualScale", 2);
            Assert.That(view.transform.localScale, Is.EqualTo(Vector3.one * .08f));
            Assert.That(app.Host.Current.Position, Is.EqualTo(state.Position)); Assert.That(app.Host.Prediction, Is.SameAs(prediction));
            Assert.That(app.Config.Physics.BallRadius, Is.EqualTo(.020f)); Assert.That(app.Tuning.PendingKeys, Is.Empty);
            app.Tuning.Set("aids.ballVisualScale", 2.5); app.Tuning.Set("aids.prediction", 1);
            app.Host.Launch(EffectiveConfig.From(app.Tuning.ApplyPending()), new BallState { Position = new Vector3(2, 3, 5) }, 30, StopCondition.MaxSteps);
            app.Clock.Paused = true;
            aids.RecordContact(new ContactEvent(SquashClash.Core.Court.SurfaceId.FrontWall, new Vector3(2, 1.5f, 0), Vector2.one,
                Vector3.forward, 1, .001f, ContactTiming.Exact, Vector3.back, false));
            yield return Capture();
        }
        [UnityTest] public IEnumerator Main_FollowTicksRenderPositionAndFreezesDuringManualPause()
        {
            var app = AppBootstrap.Instance; app.Host.enabled = false;
            app.Host.Launch(app.Config, new BallState { Position = new Vector3(.4f, 2, 5) }, 30, StopCondition.MaxSteps);
            app.Tuning.Set("camera.mode", 1);
            for (int i = 0; i < 6; i++) yield return null;
            Assert.That(UnityEngine.Camera.main.transform.position.x, Is.LessThan(3.2f));
            app.Clock.Paused = true; var position = UnityEngine.Camera.main.transform.position;
            for (int i = 0; i < 6; i++) yield return null;
            Assert.That(UnityEngine.Camera.main.transform.position, Is.EqualTo(position));
            app.Tuning.Set("camera.mode", 0); Assert.That(UnityEngine.Camera.main.transform.position.x, Is.EqualTo(3.2f));
        }
        private static IEnumerator Capture()
        {
            var camera = UnityEngine.Camera.main; var texture = new RenderTexture(720, 1566, 24); var previous = camera.targetTexture;
            camera.targetTexture = texture; camera.aspect = 720f / 1566;
            yield return null; yield return null;
            var active = RenderTexture.active; var image = new Texture2D(720, 1566, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = texture; image.ReadPixels(new Rect(0, 0, 720, 1566), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(Application.dataPath, "..", "Logs", "i6a-legibility-editor.png"), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previous; RenderTexture.active = active; texture.Release(); Object.Destroy(texture); Object.Destroy(image);
            }
        }
    }
}
