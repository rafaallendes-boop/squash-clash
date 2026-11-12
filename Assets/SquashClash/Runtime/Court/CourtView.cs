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
            // Art pass: the same unlit materials, now with small procedural textures that bake the light
            // (floor pool, contact occlusion at the wall joints). No realtime light or shadow is involved.
            var line = Material("CourtLine");
            var plaster = Keep(WallTexture());
            var wall = Textured("CourtWall", plaster, Color.white);
            var side = Textured("CourtWall", plaster, new Color(0.89f, 0.915f, 0.95f));
            // The camera looks through the back wall: a faint tint keeps the glass readable without washing out the court.
            var glass = Keep(new Material(Material("CourtGlass")));
            glass.SetColor("_BaseColor", new Color(0.75f, 0.88f, 0.95f, 0.07f));
            var floor = Textured("CourtFloor", Keep(FloorTexture(d)), Color.white);
            var tin = Textured("CourtTin", Keep(TinTexture()), Color.white);

            Surface(SurfaceId.Floor, d.Width, d.Length, floor, d);
            Surface(SurfaceId.FrontWall, d.Width, d.WallHeight, wall, d);
            Surface(SurfaceId.BackWall, d.Width, d.WallHeight, glass, d);
            Surface(SurfaceId.LeftWall, d.Length, d.WallHeight, side, d);
            Surface(SurfaceId.RightWall, d.Length, d.WallHeight, side, d);
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
                new Vector3(d.Width,d.TinTop,0.0005f), new Vector3(0,d.TinTop,0.0005f) }, Vector3.forward, tin);
            Decor(d);
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
                case SurfaceId.LeftWall: return new Vector3(d.Width, uv.y, uv.x);
                case SurfaceId.RightWall: return new Vector3(0, uv.y, uv.x);
                case SurfaceId.Floor: return new Vector3(uv.x, 0, uv.y);
                default: throw new ArgumentOutOfRangeException(nameof(surface));
            }
        }

        private static Material Material(string name) => Resources.Load<Material>("CourtMaterials/" + name)
            ?? throw new InvalidOperationException("Missing court material " + name + "; run ProjectSetup.Apply.");

        private T Keep<T>(T resource) where T : UnityEngine.Object { owned.Add(resource); return resource; }
        private Material Textured(string name, Texture texture, Color tint)
        {
            var material = Keep(new Material(Material(name)));
            material.SetTexture("_BaseMap", texture); material.SetTextureScale("_BaseMap", Vector2.one); material.SetColor("_BaseColor", tint);
            return material;
        }

        // Sober surroundings: a dark floor around the court and a backdrop behind the front wall, so the court
        // reads as a lit box in a hall instead of floating on the clear colour. Two opaque quads, no colliders, no tags.
        private void Decor(CourtDimensions d)
        {
            var decor = new GameObject("Decor");
            decor.transform.SetParent(geometry.transform, false);
            var hall = Textured("CourtWall", Keep(HallTexture()), Color.white);
            const float margin = 14, depth = 7;
            Quad("HallFloor", decor.transform, new[] { new Vector3(-margin, -0.02f, -depth), new Vector3(d.Width + margin, -0.02f, -depth),
                new Vector3(d.Width + margin, -0.02f, d.Length + 14), new Vector3(-margin, -0.02f, d.Length + 14) }, Vector3.up, hall);
            Quad("HallBackdrop", decor.transform, new[] { new Vector3(-margin, -0.02f, -depth), new Vector3(d.Width + margin, -0.02f, -depth),
                new Vector3(d.Width + margin, 18, -depth), new Vector3(-margin, 18, -depth) }, Vector3.forward, hall);
            // Wall thickness seen from above: light caps on the front and side walls.
            var cap = Textured("CourtWall", Texture2D.whiteTexture, new Color(0.80f, 0.83f, 0.87f));
            const float t = 0.18f; float h = d.WallHeight;
            Quad("FrontCap", decor.transform, new[] { new Vector3(-t, h, -t), new Vector3(d.Width + t, h, -t), new Vector3(d.Width + t, h, 0), new Vector3(-t, h, 0) }, Vector3.up, cap);
            Quad("RightCap", decor.transform, new[] { new Vector3(-t, h, 0), new Vector3(0, h, 0), new Vector3(0, h, d.Length), new Vector3(-t, h, d.Length) }, Vector3.up, cap);
            Quad("LeftCap", decor.transform, new[] { new Vector3(d.Width, h, 0), new Vector3(d.Width + t, h, 0), new Vector3(d.Width + t, h, d.Length), new Vector3(d.Width, h, d.Length) }, Vector3.up, cap);
        }

        private static Texture2D Finish(Texture2D texture, Color32[] pixels, bool mips)
        {
            texture.SetPixels32(pixels); texture.Apply(mips, true);
            return texture;
        }

        // One texture for the whole floor (u across, v from the front wall to the back): maple boards along the court,
        // a slightly brighter pool in the middle and darker contact bands where the floor meets the walls.
        private static Texture2D FloorTexture(CourtDimensions d)
        {
            const int w = 512, h = 1024, plankPx = 8, boardPx = 126;
            var texture = new Texture2D(w, h, TextureFormat.RGB24, true) { name = "Court floor", wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear, anisoLevel = 4 };
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                float fy = (y + 0.5f) / h, front = fy * d.Length, back = (1 - fy) * d.Length;
                for (var x = 0; x < w; x++)
                {
                    float fx = (x + 0.5f) / w; int plank = x / plankPx;
                    float tone = ((plank * 7919) % 13) / 13f * 0.07f - 0.035f;
                    float grain = 0.04f * Mathf.PerlinNoise(x * 0.37f, y * 0.022f) - 0.02f;
                    bool joint = x % plankPx == 0 || (y + (plank * 37) % boardPx) % boardPx == 0;
                    float sideDistance = Mathf.Min(fx, 1 - fx) * d.Width;
                    float occlusion = 1 - 0.24f * Mathf.Exp(-sideDistance / 0.30f) - 0.24f * Mathf.Exp(-front / 0.30f) - 0.10f * Mathf.Exp(-back / 0.30f);
                    float dx = (fx - 0.5f) * 2, dy = (fy - 0.45f) * 2;
                    float pool = 1.04f - 0.10f * Mathf.Clamp01(dx * dx * 0.6f + dy * dy * 0.6f);
                    float light = Mathf.Max(0.5f, occlusion) * pool * (joint ? 0.86f : 1) + tone + grain;
                    pixels[y * w + x] = new Color(0.87f * light, 0.71f * light, 0.48f * light);
                }
            }
            return Finish(texture, pixels, true);
        }

        // Shared by the front and side walls (u along the wall, v up): plaster with darker joints at the floor and corners.
        private static Texture2D WallTexture()
        {
            const int w = 128, h = 128;
            var texture = new Texture2D(w, h, TextureFormat.RGB24, true) { name = "Court wall", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear };
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w, v = (y + 0.5f) / h;
                    float light = 1 - 0.17f * Mathf.Exp(-v / 0.07f) - 0.10f * Mathf.Exp(-Mathf.Min(u, 1 - u) / 0.045f) - 0.05f * v * v;
                    light += 0.02f * Mathf.PerlinNoise(x * 0.21f, y * 0.21f) - 0.01f;
                    pixels[y * w + x] = new Color(0.955f * light, 0.955f * light, 0.94f * light);
                }
            return Finish(texture, pixels, true);
        }

        private static Texture2D TinTexture()
        {
            const int w = 4, h = 32;
            var texture = new Texture2D(w, h, TextureFormat.RGB24, false) { name = "Court tin", wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                float v = (y + 0.5f) / h;
                var color = v > 0.90f ? new Color(0.80f, 0.85f, 0.89f) : Color.Lerp(new Color(0.24f, 0.29f, 0.34f), new Color(0.46f, 0.52f, 0.58f), v);
                for (var x = 0; x < w; x++) pixels[y * w + x] = color;
            }
            return Finish(texture, pixels, false);
        }

        // Dark slate with a soft glow near the middle; used for the hall floor and the backdrop.
        private static Texture2D HallTexture()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGB24, false) { name = "Court hall", wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2 - 1, dy = (y + 0.5f) / size * 2 - 1;
                    float glow = Mathf.Clamp01(1 - Mathf.Sqrt(dx * dx + dy * dy)); glow *= glow;
                    pixels[y * size + x] = Color.Lerp(new Color(0.035f, 0.055f, 0.085f), new Color(0.10f, 0.16f, 0.22f), glow);
                }
            return Finish(texture, pixels, false);
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
