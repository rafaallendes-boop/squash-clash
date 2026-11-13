using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace SquashClash.Runtime.Visual
{
    // Shared art-pass resources: shaders, a few shared materials and procedural mesh builders.
    // Everything is created in code so the pass adds no imported assets besides two small shaders.
    public static class VisualKit
    {
        public static readonly Color Skin=new Color(.96f,.74f,.58f),SkinDark=new Color(.62f,.42f,.30f),Ink=new Color(.10f,.12f,.16f);
        private static Material toon,shadow;
        private static Texture2D soft;
        public static Shader ToonShader=>Resources.Load<Shader>("Visual/Toon");
        public static Shader FxShader=>Resources.Load<Shader>("Visual/Fx");
        // One opaque material for every vertex-coloured part: the SRP batcher draws them together.
        public static Material Toon
        {
            get
            {
                if(toon==null)toon=new Material(ToonShader){name="Art toon",hideFlags=HideFlags.HideAndDontSave};
                return toon;
            }
        }
        public static Texture2D SoftDisc
        {
            get
            {
                if(soft!=null)return soft;
                const int size=64;soft=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Art soft disc",wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};
                var pixels=new Color32[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    float dx=(x+.5f)/size*2-1,dy=(y+.5f)/size*2-1,d=Mathf.Sqrt(dx*dx+dy*dy);
                    float a=Mathf.Clamp01(1-d);a=a*a*(3-2*a);
                    pixels[y*size+x]=new Color32(255,255,255,(byte)(a*255));
                }
                soft.SetPixels32(pixels);soft.Apply(false,true);return soft;
            }
        }
        // Soft ring for outlines and impact flashes: transparent inside `inner`, opaque up to `outer` (fractions of the half size).
        public static Texture2D RingTexture(string name,float inner,float outer,float feather=.08f)
        {
            const int size=64;var texture=new Texture2D(size,size,TextureFormat.RGBA32,true){name=name,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear};
            var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=(x+.5f)/size*2-1,dy=(y+.5f)/size*2-1,d=Mathf.Sqrt(dx*dx+dy*dy);
                float a=Mathf.Clamp01((d-inner)/feather)*Mathf.Clamp01((outer-d)/feather);
                pixels[y*size+x]=new Color32(255,255,255,(byte)(a*255));
            }
            texture.SetPixels32(pixels);texture.Apply(true,true);return texture;
        }
        // Unit quad on the XY plane facing -Z (toward a camera it is aligned with), centred at the origin.
        public static Mesh BillboardQuad(string name)
        {
            var mesh=new Mesh{name=name};
            mesh.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};
            mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};
            mesh.colors=new[]{Color.white,Color.white,Color.white,Color.white};
            mesh.normals=new[]{Vector3.back,Vector3.back,Vector3.back,Vector3.back};
            mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateBounds();return mesh;
        }
        public static Material NewFx(string name,Color color,bool additive,Texture texture=null)
        {
            var material=new Material(FxShader){name=name};
            material.SetColor("_BaseColor",color);material.SetTexture("_BaseMap",texture!=null?texture:SoftDisc);
            material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",additive?(float)BlendMode.One:(float)BlendMode.OneMinusSrcAlpha);
            return material;
        }
        public static Material ContactShadow
        {
            get
            {
                if(shadow==null){shadow=NewFx("Art contact shadow",new Color(0,0,0,.42f),false);shadow.hideFlags=HideFlags.HideAndDontSave;}
                return shadow;
            }
        }
        public readonly struct Ring
        {
            public readonly float Y,R;public readonly Color Color;
            public Ring(float y,float r,Color color){Y=y;R=r;Color=color;}
        }
        // Surface of revolution around +Y. Repeat a (y, r) pair with another colour for a hard colour edge.
        public static Mesh Lathe(string name,IReadOnlyList<Ring> profile,int segments=10,float scaleX=1,float scaleZ=1)
        {
            int rings=profile.Count,stride=segments+1;
            var vertices=new Vector3[rings*stride];var normals=new Vector3[vertices.Length];var colors=new Color[vertices.Length];
            var triangles=new List<int>(rings*segments*6);
            for(int i=0;i<rings;i++)
            {
                int a=i,b=i;
                while(a>0&&Same(profile[a],profile[i]))a--;
                while(b<rings-1&&Same(profile[b],profile[i]))b++;
                float dy=profile[b].Y-profile[a].Y,dr=profile[b].R-profile[a].R;
                var slope=new Vector2(dy,-dr);if(slope.sqrMagnitude<1e-10f)slope=Vector2.right;slope.Normalize();
                for(int s=0;s<=segments;s++)
                {
                    float angle=s*2*Mathf.PI/segments,c=Mathf.Cos(angle),n=Mathf.Sin(angle);int v=i*stride+s;
                    vertices[v]=new Vector3(c*profile[i].R*scaleX,profile[i].Y,n*profile[i].R*scaleZ);
                    normals[v]=new Vector3(c*slope.x/scaleX,slope.y,n*slope.x/scaleZ).normalized;colors[v]=profile[i].Color;
                }
                if(i==rings-1||Same(profile[i],profile[i+1]))continue;
                for(int s=0;s<segments;s++)
                {
                    int v=i*stride+s,up=v+stride;
                    triangles.Add(v);triangles.Add(up);triangles.Add(v+1);triangles.Add(v+1);triangles.Add(up);triangles.Add(up+1);
                }
            }
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.normals=normals;mesh.colors=colors;mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
            return mesh;
        }
        private static bool Same(Ring a,Ring b)=>Mathf.Approximately(a.Y,b.Y)&&Mathf.Approximately(a.R,b.R);
        public static Mesh Sphere(string name,float radius,Color color,int segments=12,int rings=8)
        {
            var profile=new List<Ring>(rings+1);
            for(int i=0;i<=rings;i++){float a=-Mathf.PI/2+Mathf.PI*i/rings;profile.Add(new Ring(radius*Mathf.Sin(a),Mathf.Max(radius*Mathf.Cos(a),1e-4f),color));}
            return Lathe(name,profile,segments);
        }
        // Limb of unit length along +Y (scale Y by the bone length); colours switch at the given fractions.
        public static Mesh Limb(string name,float r0,float r1,Color first,float split,Color second,float endBulb=0)
        {
            var p=new List<Ring>();
            p.Add(new Ring(0,r0*.55f,first));p.Add(new Ring(.04f,r0,first));
            float rs=Mathf.Lerp(r0,r1,split);
            p.Add(new Ring(split,rs,first));p.Add(new Ring(split,rs,second));
            if(endBulb>0){p.Add(new Ring(.86f,r1,second));p.Add(new Ring(.93f,endBulb,second));p.Add(new Ring(1.0f,endBulb*.8f,second));p.Add(new Ring(1.04f,1e-4f,second));}
            else{p.Add(new Ring(.96f,r1,second));p.Add(new Ring(1f,r1*.55f,second));}
            return Lathe(name,p,8);
        }
        // Re-expresses a mesh through a matrix (used to fit a part to the box another view scales).
        public static Mesh Rebase(Mesh mesh,Matrix4x4 matrix)
        {
            var vertices=mesh.vertices;var normals=mesh.normals;var normalMatrix=matrix.inverse.transpose;
            for(int i=0;i<vertices.Length;i++){vertices[i]=matrix.MultiplyPoint3x4(vertices[i]);normals[i]=normalMatrix.MultiplyVector(normals[i]).normalized;}
            mesh.vertices=vertices;mesh.normals=normals;mesh.RecalculateBounds();return mesh;
        }
        // Unit quad on the XZ plane facing up, centred at the origin.
        public static Mesh FloorQuad(string name)
        {
            var mesh=new Mesh{name=name};
            mesh.vertices=new[]{new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(.5f,0,.5f),new Vector3(-.5f,0,.5f)};
            mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};
            mesh.colors=new[]{Color.white,Color.white,Color.white,Color.white};
            mesh.normals=new[]{Vector3.up,Vector3.up,Vector3.up,Vector3.up};
            mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateBounds();return mesh;
        }
        // Racket inside the unit box the swing code scales to (0.22, 0.03, 0.30): head ring centred at the origin on the XZ plane,
        // long axis +Z, handle running back along -Z to the grip.
        public static Mesh Racket(string name,Color frame,Color grip,Color strings)
        {
            const int sides=20;const float outer=.5f,inner=.36f,half=.5f;
            var v=new List<Vector3>();var n=new List<Vector3>();var c=new List<Color>();var t=new List<int>();
            void Quad(Vector3 a,Vector3 b,Vector3 d,Vector3 e,Vector3 normal,Color color)
            {
                int i=v.Count;v.Add(a);v.Add(b);v.Add(d);v.Add(e);for(int k=0;k<4;k++){n.Add(normal);c.Add(color);}
                t.Add(i);t.Add(i+1);t.Add(i+2);t.Add(i);t.Add(i+2);t.Add(i+3);t.Add(i);t.Add(i+2);t.Add(i+1);t.Add(i);t.Add(i+3);t.Add(i+2);
            }
            for(int s=0;s<sides;s++)
            {
                float a0=s*2*Mathf.PI/sides,a1=(s+1)*2*Mathf.PI/sides;
                var o0=new Vector3(Mathf.Cos(a0)*outer,0,Mathf.Sin(a0)*outer);var o1=new Vector3(Mathf.Cos(a1)*outer,0,Mathf.Sin(a1)*outer);
                var i0=o0*(inner/outer);var i1=o1*(inner/outer);var up=Vector3.up*half;
                Quad(i0+up,o0+up,o1+up,i1+up,Vector3.up,frame);Quad(i0-up,o0-up,o1-up,i1-up,Vector3.down,frame);
                Quad(o0-up,o0+up,o1+up,o1-up,(o0+o1).normalized,frame);
                // Strings: a pale fan, thin enough to read as a mesh of strings at game distance.
                Quad(Vector3.zero,i0*.999f,i1*.999f,Vector3.zero,Vector3.up,strings);
            }
            // Handle from the head to the grip (0.32 m behind the head centre = -1.07 units of the 0.30 m box).
            const float hx=.085f,z0=-.48f,z1=-.78f,z2=-1.12f;
            void Bar(float za,float zb,float halfX,Color color)
            {
                var up=Vector3.up*half;
                Vector3 a=new Vector3(-halfX,0,za),b=new Vector3(halfX,0,za),d=new Vector3(halfX,0,zb),e=new Vector3(-halfX,0,zb);
                Quad(a+up,b+up,d+up,e+up,Vector3.up,color);Quad(a-up,b-up,d-up,e-up,Vector3.down,color);
                Quad(a-up,a+up,e+up,e-up,Vector3.left,color);Quad(b-up,b+up,d+up,d-up,Vector3.right,color);
            }
            Bar(z0,z1,hx*.8f,frame);Bar(z1,z2,hx,grip);
            var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetColors(c);mesh.SetTriangles(t,0);mesh.RecalculateBounds();return mesh;
        }
        // Two-bone IK: the joint between root and target for bone lengths l1, l2, bending toward hint.
        public static Vector3 Joint(Vector3 root,Vector3 target,float l1,float l2,Vector3 hint,out Vector3 end)
        {
            var to=target-root;float d=to.magnitude;
            if(d<1e-4f){to=Vector3.down;d=1e-4f;}
            var dir=to/d;float reach=Mathf.Clamp(d,Mathf.Abs(l1-l2)+1e-3f,l1+l2-1e-3f);
            float a=(l1*l1-l2*l2+reach*reach)/(2*reach),h=Mathf.Sqrt(Mathf.Max(0,l1*l1-a*a));
            var perp=hint-Vector3.Dot(hint,dir)*dir;
            if(perp.sqrMagnitude<1e-8f)perp=Vector3.Cross(dir,Vector3.right);
            perp.Normalize();end=root+dir*reach;
            return root+dir*a+perp*h;
        }
        // Places a unit-length +Y part from a to b.
        public static void Bone(Transform part,Vector3 a,Vector3 b)
        {
            var d=b-a;float length=d.magnitude;
            part.localPosition=a;
            if(length>1e-5f)part.localRotation=Quaternion.FromToRotation(Vector3.up,d/length);
            part.localScale=new Vector3(1,length,1);
        }
        public static MeshRenderer Part(string name,Transform parent,Mesh mesh,Material material)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);
            go.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
            return renderer;
        }
    }
}
