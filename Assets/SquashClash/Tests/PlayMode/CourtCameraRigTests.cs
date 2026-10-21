using System.Collections;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Camera;
using SquashClash.Runtime.Court;
using SquashClash.Runtime.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SquashClash.Runtime.Tests
{
    public sealed class CourtCameraRigTests
    {
        private GameObject root;
        [SetUp] public void SetUp() { root = new GameObject("Test camera"); }
        [UnityTearDown] public IEnumerator TearDown() { Object.Destroy(root); yield return null; }
        private static ParamSet Default() => PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text);

        [UnityTest]
        public IEnumerator Fixed_DefaultFraming_ShowsFrontWallAndBackFloorCorners()
        {
            var camera = root.AddComponent<UnityEngine.Camera>();
            camera.aspect = 1206f / 2622f;
            root.AddComponent<CourtCameraRig>().Apply(Default(), CourtDimensions.Standard());
            yield return null;
            AssertFraming(camera);
        }

        [UnityTest]
        public IEnumerator Fixed_ApplyUsesConfig_AndTickLeavesCameraStationary()
        {
            var camera = root.AddComponent<UnityEngine.Camera>();
            var parameters = Default().With("camera.height", 7).With("camera.backOffset", 4).With("camera.fovDeg", 80);
            var rig = root.AddComponent<CourtCameraRig>();
            rig.Apply(parameters, CourtDimensions.Standard());
            yield return null;
            Assert.That(camera.transform.position, Is.EqualTo(new Vector3(3.2f, 7f, 13.75f)));
            Assert.That(camera.fieldOfView, Is.EqualTo(80));
            var rotation = camera.transform.rotation;
            rig.Tick(new Vector3(1, 2, 3), 1f);
            Assert.That(camera.transform.position, Is.EqualTo(new Vector3(3.2f, 7f, 13.75f)));
            Assert.That(camera.transform.rotation, Is.EqualTo(rotation));
        }

        [UnityTest]
        public IEnumerator MainScene_WiresEffectiveConfigCourtAndPlayers()
        {
            yield return SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
            yield return null;
            var app = AppBootstrap.Instance;
            Assert.That(app.Params, Is.Not.Null);
            Assert.That(app.Config.Hash, Is.EqualTo(app.Params.Hash));
            Assert.That(Object.FindFirstObjectByType<CourtView>(), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<SurfaceTag>(FindObjectsSortMode.None).Length, Is.EqualTo(5));
            Assert.That(Object.FindObjectsByType<Collider>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(Object.FindFirstObjectByType<PlaceholderPlayers>(), Is.Not.Null);
            var camera = UnityEngine.Camera.main;
            var texture = new RenderTexture(1206, 2622, 24);
            var previous = camera.targetTexture;
            camera.targetTexture = texture;
            camera.aspect = 1206f / 2622f;
            yield return null;
            yield return null;
            AssertFraming(camera);
            var active = RenderTexture.active;
            var image = new Texture2D(1206, 2622, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, 1206, 2622), 0, 0);
                image.Apply();
                var destination = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "i2-court-editor.png"));
                File.WriteAllBytes(destination, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previous;
                RenderTexture.active = active;
                texture.Release();
                Object.Destroy(texture);
                Object.Destroy(image);
            }
        }

        private static void AssertFraming(UnityEngine.Camera camera)
        {
            foreach (var point in new[] { new Vector3(0,0,0), new Vector3(6.4f,0,0), new Vector3(0,4.57f,0),
                new Vector3(6.4f,4.57f,0), new Vector3(0,0,9.75f), new Vector3(6.4f,0,9.75f),
                new Vector3(0,2.18f,9.75f), new Vector3(6.4f,2.18f,9.75f) })
            {
                var viewport = camera.WorldToViewportPoint(point);
                Assert.That(viewport.z, Is.GreaterThan(0), point.ToString());
                Assert.That(viewport.x, Is.InRange(0f, 1f), point.ToString());
                Assert.That(viewport.y, Is.InRange(0f, 1f), point.ToString());
            }
        }
    }
}
