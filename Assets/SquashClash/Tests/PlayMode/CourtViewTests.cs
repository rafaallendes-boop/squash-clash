using System.Collections;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Court;
using SquashClash.Runtime.Physics;
using UnityEngine;
using UnityEngine.TestTools;

namespace SquashClash.Runtime.Tests
{
    public sealed class CourtViewTests
    {
        private GameObject root;
        [SetUp] public void SetUp() { root = new GameObject("Test court"); }
        [UnityTearDown] public IEnumerator TearDown() { Object.Destroy(root); yield return null; }

        [UnityTest]
        public IEnumerator Build_CreatesFiveSurfacesAndTwelveLines_NoColliders()
        {
            var view = root.AddComponent<CourtView>();
            view.Build(CourtDimensions.Standard());
            yield return null;
            var surfaces = root.GetComponentsInChildren<SurfaceTag>();
            Assert.That(surfaces.Length, Is.EqualTo(5));
            Assert.That(surfaces.Select(s => s.Surface).Distinct().Count(), Is.EqualTo(5));
            Assert.That(root.transform.Find("Geometry/Lines").childCount, Is.EqualTo(12));
            Assert.That(root.GetComponentsInChildren<Collider>(), Is.Empty);
        }

        [UnityTest]
        public IEnumerator Build_Twice_ReplacesPreviousAndReleasesOwnedMesh()
        {
            var view = root.AddComponent<CourtView>();
            view.Build(CourtDimensions.Standard());
            var oldMesh = root.GetComponentsInChildren<SurfaceTag>().First().GetComponent<MeshFilter>().sharedMesh;
            view.Build(CourtDimensions.WithTinTop(0.43f));
            yield return null;
            Assert.That(root.GetComponentsInChildren<SurfaceTag>().Length, Is.EqualTo(5));
            Assert.That(root.transform.Find("Geometry/Lines").childCount, Is.EqualTo(12));
            Assert.That(oldMesh == null, Is.True, "A rebuild must release its generated mesh.");
        }

        [UnityTest]
        public IEnumerator FrontOutMesh_UsesApprovedReferenceAndOneMillimetreInset()
        {
            root.AddComponent<CourtView>().Build(CourtDimensions.Standard());
            yield return null;
            var mesh = root.transform.Find("Geometry/Lines/FrontOut").GetComponent<MeshFilter>().sharedMesh;
            var expected = new[] { new Vector3(0, 4.57f, 0.001f), new Vector3(6.4f, 4.57f, 0.001f),
                new Vector3(6.4f, 4.62f, 0.001f), new Vector3(0, 4.62f, 0.001f) };
            for (var i = 0; i < expected.Length; i++)
                Assert.That(Vector3.Distance(mesh.vertices[i], expected[i]), Is.LessThan(1e-5));
        }

        [UnityTest]
        public IEnumerator Placeholders_ApplyPositionsVisibilityAndHeightWithoutColliders()
        {
            var p = PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text);
            var players = root.AddComponent<PlaceholderPlayers>();
            players.Apply(p);
            yield return null;
            Assert.That(root.transform.childCount, Is.EqualTo(2));
            Assert.That(root.GetComponentsInChildren<Collider>(), Is.Empty);
            var human = root.transform.Find("Human");
            var rival = root.transform.Find("Rival");
            Assert.That(human.position, Is.EqualTo(new Vector3(3.2f, 0.9f, 5.5f)));
            Assert.That(rival.position, Is.EqualTo(new Vector3(4.5f, 0.9f, 6.5f)));
            Assert.That(human.GetComponent<MeshRenderer>().bounds.size.y, Is.EqualTo(1.8f).Within(1e-4));
            players.Apply(p.With("placeholders.visible", 0).With("human.x", 2));
            Assert.That(human.gameObject.activeSelf, Is.False);
            Assert.That(rival.gameObject.activeSelf, Is.False);
            Assert.That(human.position.x, Is.EqualTo(2));
            Assert.That(root.transform.childCount, Is.EqualTo(2));
        }
    }
}
