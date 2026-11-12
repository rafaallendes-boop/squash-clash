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
        // Halo quad size in ball diameters: it fades from the ball's edge to nothing a little over one radius further out.
        public const float OutlineScale = 2.3f;
        private Mesh outlineMesh; private Material outlineMaterial; private Transform outline; private UnityEngine.Camera view;
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
                // Art pass: still the flat black ball (its strongest contrast against the light walls and floor), wrapped in a
                // soft dark halo that makes the dot read a little larger without changing its scale. Visual only: the radius is
                // unchanged. A white outline was tried and rejected: at game distance it averaged the dot to grey on the walls.
                material = new Material(SquashClash.Runtime.Visual.VisualKit.ToonShader) { name="Ball visual black" };
                material.SetColor("_BaseColor", new Color(.012f,.012f,.016f)); material.SetColor("_ShadeColor", Color.white);
                material.SetColor("_RimColor", new Color(0,0,0,0));
                var ballRenderer = GetComponent<MeshRenderer>(); ballRenderer.sharedMaterial = material;
                ballRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; ballRenderer.receiveShadows = false;
                outlineMesh = SquashClash.Runtime.Visual.VisualKit.BillboardQuad("Ball halo");
                outlineMaterial = SquashClash.Runtime.Visual.VisualKit.NewFx("Ball halo", new Color(.012f,.012f,.016f,.55f), false);
                outline = SquashClash.Runtime.Visual.VisualKit.Part("Halo", transform, outlineMesh, outlineMaterial).transform;
                outline.localScale = Vector3.one * OutlineScale;
            }
            transform.localScale = Vector3.one * (radius * 2);
            transform.position = host.RenderPosition;
        }
        public void ApplyVisualScale(float scale) => transform.localScale = Vector3.one * (radius * 2 * scale);
        private void LateUpdate()
        {
            if (host != null && host.ActiveConfig != null) transform.position = host.RenderPosition;
            if (outline == null) return;
            if (view == null) view = UnityEngine.Camera.main;
            if (view != null) outline.rotation = view.transform.rotation;
        }
        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh); if (material != null) Destroy(material);
            if (outlineMesh != null) Destroy(outlineMesh); if (outlineMaterial != null) Destroy(outlineMaterial);
        }
    }
}
