using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Runtime.Camera;
using UnityEngine;
using UnityEngine.TestTools;

namespace SquashClash.Runtime.Tests
{
    public sealed class FollowCameraTests
    {
        private GameObject root;
        private CourtCameraRig rig;
        private UnityEngine.Camera camera;
        private ParamSet defaults;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();
            defaults = PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text);
            root = new GameObject("Follow camera test");
            camera = root.AddComponent<UnityEngine.Camera>(); camera.aspect = 1206f / 2622f;
            rig = root.AddComponent<CourtCameraRig>();
        }
        [UnityTearDown] public IEnumerator Cleanup() { Object.Destroy(root); yield return null; }

        [UnityTest] public IEnumerator Follow_MovesSmoothlyAndStaysWithinMaximumOnBothSides()
        {
            rig.Apply(defaults.With("camera.mode", 1), CourtDimensions.Standard());
            var rotation = root.transform.rotation;
            rig.Tick(new Vector3(0, 1, 5), 1f / 60);
            Assert.That(root.transform.position.x, Is.InRange(1.70001f, 3.19999f), "Follow must move, without snapping");
            foreach (float ballX in new[] { 0f, 6.4f, 0f })
            {
                for (int i = 0; i < 300; i++)
                {
                    rig.Tick(new Vector3(ballX, 1, 5), 1f / 60);
                    Assert.That(Mathf.Abs(root.transform.position.x - 3.2f), Is.LessThanOrEqualTo(1.500001f));
                }
                Assert.That(root.transform.position.x, Is.EqualTo(ballX == 0 ? 1.7f : 4.7f).Within(.001f));
            }
            Assert.That(Quaternion.Angle(rotation, root.transform.rotation), Is.LessThan(.001f), "Target and camera translate equally");
            yield return null;
        }
        [UnityTest] public IEnumerator Follow_DefaultFramingStillShowsFrontWallCorners()
        {
            rig.Apply(defaults.With("camera.mode", 1), CourtDimensions.Standard());
            foreach (float ballX in new[] { 0f, 6.4f })
            {
                for (int i = 0; i < 300; i++) rig.Tick(new Vector3(ballX, 1, 5), 1f / 60);
                foreach (var p in new[] { new Vector3(0,0,0), new Vector3(6.4f,0,0), new Vector3(0,4.57f,0), new Vector3(6.4f,4.57f,0) })
                {
                    var v = camera.WorldToViewportPoint(p);
                    Assert.That(v.z, Is.GreaterThan(0)); Assert.That(v.x, Is.InRange(0f, 1f)); Assert.That(v.y, Is.InRange(0f, 1f));
                }
            }
            yield return null;
        }
        [UnityTest] public IEnumerator Follow_ZeroDeltaZeroDampingAndImmediateModeChangesAreFinite()
        {
            var follow = defaults.With("camera.mode", 1).With("camera.followDamping", 0);
            rig.Apply(follow, CourtDimensions.Standard());
            rig.Tick(Vector3.zero, 0); Assert.That(root.transform.position.x, Is.EqualTo(3.2f));
            rig.Tick(Vector3.zero, .01f); Assert.That(root.transform.position.x, Is.EqualTo(1.7f).Within(.00001f));
            rig.Apply(follow.With("camera.followMaxOffset", .25), CourtDimensions.Standard());
            Assert.That(root.transform.position.x, Is.InRange(2.95f, 3.45f));
            rig.Apply(defaults, CourtDimensions.Standard()); var position = root.transform.position;
            rig.Tick(new Vector3(6.4f, 1, 5), 3);
            Assert.That(root.transform.position, Is.EqualTo(position)); Assert.That(position.x, Is.EqualTo(3.2f));
            yield return null;
        }
    }
}
