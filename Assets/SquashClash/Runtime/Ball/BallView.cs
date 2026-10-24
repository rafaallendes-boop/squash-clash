using UnityEngine;
namespace SquashClash.Runtime.Ball
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class BallView : MonoBehaviour
    {
        private BallSimulationHost host;
        private Mesh mesh;
        private Material material;
        private float radius;
        public void Initialize(BallSimulationHost host, float radius)
        {
            this.host = host;
            this.radius = radius;
            if (mesh == null)
            {
                const int rings = 8, segments = 16;
                var vertices = new Vector3[(rings + 1) * (segments + 1)];
                var normals = new Vector3[vertices.Length];
                var triangles = new int[rings * segments * 6]; int triangle = 0;
                for (int y = 0; y <= rings; y++) for (int x = 0; x <= segments; x++)
                {
                    float latitude = Mathf.PI * y / rings, longitude = 2 * Mathf.PI * x / segments;
                    var normal = new Vector3(Mathf.Sin(latitude)*Mathf.Cos(longitude), Mathf.Cos(latitude), Mathf.Sin(latitude)*Mathf.Sin(longitude));
                    int i = y*(segments+1)+x; vertices[i] = normal*0.5f; normals[i] = normal;
                    if (y == rings || x == segments) continue;
                    int next = i+segments+1;
                    triangles[triangle++]=i; triangles[triangle++]=i+1; triangles[triangle++]=next;
                    triangles[triangle++]=i+1; triangles[triangle++]=next+1; triangles[triangle++]=next;
                }
                mesh = new Mesh { name="Ball visual", vertices=vertices, normals=normals, triangles=triangles }; mesh.RecalculateBounds();
                GetComponent<MeshFilter>().sharedMesh = mesh;
                material = new Material(Resources.Load<Material>("CourtMaterials/CourtBlue")) { name="Ball visual black" };
                material.SetColor("_BaseColor", Color.black);
                GetComponent<MeshRenderer>().sharedMaterial = material;
            }
            transform.localScale = Vector3.one * (radius * 2);
            transform.position = host.RenderPosition;
        }
        public void ApplyVisualScale(float scale) => transform.localScale = Vector3.one * (radius * 2 * scale);
        private void LateUpdate() { if (host != null && host.ActiveConfig != null) transform.position = host.RenderPosition; }
        private void OnDestroy() { if (mesh != null) Destroy(mesh); if (material != null) Destroy(material); }
    }
}
