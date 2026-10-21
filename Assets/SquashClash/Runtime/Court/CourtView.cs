using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Runtime.Physics;
using UnityEngine;

namespace SquashClash.Runtime.Court
{
    public sealed class CourtView : MonoBehaviour
    {
        private GameObject geometry;
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();

        public void Build(CourtDimensions d)
        {
            Clear();
            geometry = new GameObject("Geometry");
            geometry.transform.SetParent(transform, false);
            var wall = Material("CourtWall");
            var line = Material("CourtLine");
            var glass = Material("CourtGlass");
            var floor = new Material(Material("CourtFloor"));
            var wood = WoodTexture();
            owned.Add(floor); owned.Add(wood);
            floor.SetTexture("_BaseMap", wood);
            floor.SetTextureScale("_BaseMap", new Vector2(d.Width / 1.44f, d.Length / 2.4f));

            Surface(SurfaceId.Floor, d.Width, d.Length, floor, d);
            Surface(SurfaceId.FrontWall, d.Width, d.WallHeight, wall, d);
            Surface(SurfaceId.BackWall, d.Width, d.WallHeight, glass, d);
            Surface(SurfaceId.LeftWall, d.Length, d.WallHeight, wall, d);
            Surface(SurfaceId.RightWall, d.Length, d.WallHeight, wall, d);
            var lines = new GameObject("Lines");
            lines.transform.SetParent(geometry.transform, false);
            foreach (var band in CourtLines.Build(d))
            {
                var offset = CourtDimensions.InwardNormal(band.Surface) * 0.001f;
                Quad(band.Id.ToString(), lines.transform, new[] {
                    World(band.Surface, band.A, d) + offset,
                    World(band.Surface, band.B, d) + offset,
                    World(band.Surface, band.B + band.Growth * band.Width, d) + offset,
                    World(band.Surface, band.A + band.Growth * band.Width, d) + offset },
                    CourtDimensions.InwardNormal(band.Surface), line);
            }
            // Flat tin is the approved visual simplification; it has no collision or rule logic.
            Quad("TinPanel", geometry.transform, new[] { new Vector3(0,0,0.0005f), new Vector3(d.Width,0,0.0005f),
                new Vector3(d.Width,d.TinTop,0.0005f), new Vector3(0,d.TinTop,0.0005f) }, Vector3.forward, Material("CourtTin"));
        }

        private void Surface(SurfaceId surface, float width, float height, Material material, CourtDimensions d)
        {
            var vertices = new[] { World(surface, Vector2.zero, d), World(surface, new Vector2(width,0), d),
                World(surface, new Vector2(width,height), d), World(surface, new Vector2(0,height), d) };
            Quad(surface.ToString(), geometry.transform, vertices, CourtDimensions.InwardNormal(surface), material)
                .AddComponent<SurfaceTag>().Surface = surface;
        }

        private GameObject Quad(string name, Transform parent, Vector3[] vertices, Vector3 normal, Material material)
        {
            var mesh = new Mesh { name = name };
            var forward = Vector3.Dot(Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]), normal) > 0;
            mesh.vertices = vertices;
            mesh.triangles = forward ? new[] {0,1,2,0,2,3} : new[] {0,2,1,0,3,2};
            mesh.uv = new[] {Vector2.zero, Vector2.right, Vector2.one, Vector2.up};
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            owned.Add(mesh);
            var item = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            item.transform.SetParent(parent, false);
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            item.GetComponent<MeshRenderer>().sharedMaterial = material;
            return item;
        }

        private static Vector3 World(SurfaceId surface, Vector2 uv, CourtDimensions d)
        {
            switch (surface)
            {
                case SurfaceId.FrontWall: return new Vector3(uv.x, uv.y, 0);
                case SurfaceId.BackWall: return new Vector3(uv.x, uv.y, d.Length);
                case SurfaceId.LeftWall: return new Vector3(0, uv.y, uv.x);
                case SurfaceId.RightWall: return new Vector3(d.Width, uv.y, uv.x);
                case SurfaceId.Floor: return new Vector3(uv.x, 0, uv.y);
                default: throw new ArgumentOutOfRangeException(nameof(surface));
            }
        }

        private static Material Material(string name) => Resources.Load<Material>("CourtMaterials/" + name)
            ?? throw new InvalidOperationException("Missing court material " + name + "; run ProjectSetup.Apply.");

        private static Texture2D WoodTexture()
        {
            var texture = new Texture2D(128, 256, TextureFormat.RGB24, false) { name = "Court wood", wrapMode = TextureWrapMode.Repeat };
            var pixels = new Color[128 * 256];
            for (var y = 0; y < 256; y++)
                for (var x = 0; x < 128; x++)
                {
                    var plank = x / 16;
                    var grain = 0.025f * Mathf.Sin(y * 0.21f + x * 1.3f) + 0.035f * Mathf.PerlinNoise(x * 0.12f, y * 0.035f);
                    var joint = x % 16 == 0 || (y + (plank % 2) * 32) % 64 == 0;
                    pixels[y * 128 + x] = joint ? new Color(0.60f,0.48f,0.32f) : new Color(0.82f + grain,0.70f + grain,0.51f + grain);
                }
            texture.SetPixels(pixels); texture.Apply(false, true);
            return texture;
        }

        private void Clear()
        {
            if (geometry != null)
            {
                geometry.SetActive(false);
                geometry.transform.SetParent(null);
                Destroy(geometry);
                geometry = null;
            }
            foreach (var resource in owned) if (resource != null) Destroy(resource);
            owned.Clear();
        }
        private void OnDestroy() => Clear();
    }
}
