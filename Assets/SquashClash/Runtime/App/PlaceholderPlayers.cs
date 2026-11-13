using System;
using System.Collections.Generic;
using SquashClash.Core.Config;
using UnityEngine;

namespace SquashClash.Runtime.App
{
    public sealed class PlaceholderPlayers : MonoBehaviour
    {
        private GameObject human, rival;
        private Mesh capsule;
        public Transform Human => human != null ? human.transform : null;
        public Transform Rival => rival != null ? rival.transform : null;
        // Gameplay owns where the bodies are; this only draws them (floor x, z in metres).
        public void Place(Vector2 humanXZ, Vector2 rivalXZ)
        {
            if (human != null) human.transform.localPosition = new Vector3(humanXZ.x, 0.9f, humanXZ.y);
            if (rival != null) rival.transform.localPosition = new Vector3(rivalXZ.x, 0.9f, rivalXZ.y);
        }

        public void Apply(ParamSet p)
        {
            if (capsule == null) capsule = Capsule();
            if (human == null) human = Player("Human", "CourtBlue", true);
            if (rival == null) rival = Player("Rival", "CourtRed", false);
            human.transform.localPosition = new Vector3((float)p.Get("human.x"), 0.9f, (float)p.Get("human.z"));
            rival.transform.localPosition = new Vector3((float)p.Get("rival.x"), 0.9f, (float)p.Get("rival.z"));
            human.SetActive(p.GetBool("placeholders.visible"));
            rival.SetActive(p.GetBool("placeholders.visible"));
        }

        private GameObject Player(string name, string materialName, bool isHuman)
        {
            var player = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            player.transform.SetParent(transform, false);
            player.GetComponent<MeshFilter>().sharedMesh = capsule;
            player.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("CourtMaterials/" + materialName)
                ?? throw new InvalidOperationException("Missing reference-player material.");
            // Art pass: the 1.80 m capsule stays as the reference volume but is no longer drawn; the figure is.
            player.GetComponent<MeshRenderer>().forceRenderingOff = true;
            var figure = new GameObject("Figure").AddComponent<SquashClash.Runtime.Visual.CharacterView>();
            figure.transform.SetParent(player.transform, false);
            figure.Initialize(isHuman, isHuman ? SquashClash.Runtime.Visual.CharacterView.HumanPalette : SquashClash.Runtime.Visual.CharacterView.RivalPalette);
            return player;
        }

        private static Mesh Capsule()
        {
            const int segments = 16, hemisphereSteps = 6;
            const float radius = 0.28f, halfCylinder = 0.62f;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var hemisphere = 0; hemisphere < 2; hemisphere++)
                for (var ring = 0; ring <= hemisphereSteps; ring++)
                {
                    var latitude = (hemisphere == 0 ? -Mathf.PI / 2 : 0) + ring * Mathf.PI / (2 * hemisphereSteps);
                    var y = (hemisphere == 0 ? -halfCylinder : halfCylinder) + radius * Mathf.Sin(latitude);
                    var r = radius * Mathf.Cos(latitude);
                    for (var segment = 0; segment <= segments; segment++)
                    {
                        var angle = segment * 2 * Mathf.PI / segments;
                        vertices.Add(new Vector3(r * Mathf.Cos(angle), y, r * Mathf.Sin(angle)));
                    }
                }
            var rings = 2 * (hemisphereSteps + 1);
            for (var ring = 0; ring < rings - 1; ring++)
                for (var segment = 0; segment < segments; segment++)
                {
                    var a = ring * (segments + 1) + segment;
                    var b = a + segments + 1;
                    triangles.AddRange(new[] {a,b,a+1,a+1,b,b+1});
                }
            var mesh = new Mesh { name = "Reference capsule 1.80m" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
        private void OnDestroy()
        {
            if (human != null) Destroy(human);
            if (rival != null) Destroy(rival);
            if (capsule != null) Destroy(capsule);
        }
    }
}
