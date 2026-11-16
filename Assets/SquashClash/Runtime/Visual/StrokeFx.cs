using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Camera;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.Rendering;
namespace SquashClash.Runtime.Visual
{
    public enum StrokeKind {None,Perfect,Valid,Failed,Rival}
    // Feel pass: what a racket stroke looks like. It only listens: the single final delivery of the human's attempt
    // (ShotController.PresentationDelivered, with the record already written) and the rival's swing event. From those it
    // draws a contact burst sized by power and coloured by result, and a short streak behind the ball for the best
    // strokes. A failed swing draws nothing here on purpose: the absence of the burst is the cue, and the figure shows the miss.
    // One dynamic mesh (one draw call) and one line, both off when idle; no allocation after start; frozen with the clock.
    // It never touches the ball, the clock, the camera pose, audio or haptics.
    public sealed class StrokeFx : MonoBehaviour
    {
        public const int Rings=4,MaxQuads=24,StreakPoints=10;
        public const float RingSeconds=.28f,SparkSeconds=.36f,StrongFraction=.8f;
        public static readonly Color PerfectColor=new Color(1f,.80f,.16f),PerfectGlow=new Color(1f,.96f,.74f),ValidColor=new Color(1f,1f,1f),RivalColor=new Color(.55f,1f,.94f);
        private struct Quad { public bool Active;public Vector3 Position,Velocity;public float Age,Life,From,To,Alpha;public Color32 Color; }
        private readonly Quad[] quads=new Quad[MaxQuads];
        private readonly Vector3[] vertices=new Vector3[MaxQuads*4];private readonly Color32[] colors=new Color32[MaxQuads*4];
        private readonly Vector3[] streakPoints=new Vector3[StreakPoints];
        private AppBootstrap app;private BallSimulationHost host;private ShotController controller;private RallyDirector rally;private Transform eye;
        private Mesh mesh;private MeshRenderer body;private Material material,streakMaterial;private Texture2D atlas;private LineRenderer streak;
        private int nextRing,nextSpark=Rings;private uint random=2463534242u;
        private bool rivalPending,dirty;private float streakLeft,streakLife;private Color streakColor;
        // Read-only state for the figures and the tests.
        public StrokeKind LastKind {get;private set;}
        public StrokeKind HumanKind {get;private set;}
        public float HumanPower {get;private set;}
        public int HumanStrokes {get;private set;}
        public int RivalStrokes {get;private set;}
        public float LastPower {get;private set;}
        public bool StreakVisible=>streakLeft>0;
        public int ActiveQuads {get{int n=0;for(int i=0;i<MaxQuads;i++)if(quads[i].Active)n++;return n;}}
        public Vector3 VertexAt(int index)=>vertices[index];
        public void Initialize(AppBootstrap app,CourtCameraRig rig)
        {
            if(this.app!=null)return;this.app=app;host=app.Host;controller=app.GetComponent<ShotController>();rally=app.Rally;eye=rig!=null?rig.transform:null;
            atlas=Atlas();material=VisualKit.NewFx("Stroke fx",Color.white,false,atlas);
            mesh=new Mesh{name="Stroke fx"};mesh.MarkDynamic();
            var uv=new Vector2[MaxQuads*4];var triangles=new int[MaxQuads*6];
            for(int q=0;q<MaxQuads;q++)
            {
                float u0=q<Rings?.5f:0,u1=q<Rings?1:.5f;int v=q*4,t=q*6;
                uv[v]=new Vector2(u0,0);uv[v+1]=new Vector2(u1,0);uv[v+2]=new Vector2(u1,1);uv[v+3]=new Vector2(u0,1);
                triangles[t]=v;triangles[t+1]=v+2;triangles[t+2]=v+1;triangles[t+3]=v;triangles[t+4]=v+3;triangles[t+5]=v+2;
            }
            mesh.vertices=vertices;mesh.uv=uv;mesh.colors32=colors;mesh.triangles=triangles;
            mesh.bounds=new Bounds(new Vector3(3.2f,3,5),new Vector3(40,40,40));
            body=VisualKit.Part("Stroke fx",transform,mesh,material);body.enabled=false;
            var line=new GameObject("Stroke streak");line.transform.SetParent(transform,false);
            streak=line.AddComponent<LineRenderer>();streakMaterial=VisualKit.NewFx("Stroke streak",Color.white,false,Texture2D.whiteTexture);
            streak.sharedMaterial=streakMaterial;streak.useWorldSpace=true;streak.positionCount=StreakPoints;streak.numCapVertices=2;
            streak.shadowCastingMode=ShadowCastingMode.Off;streak.receiveShadows=false;streak.enabled=false;
            if(controller!=null)controller.PresentationDelivered+=OnDelivered;
            if(rally!=null)rally.RivalSwing+=OnRivalSwing;
            host.Launched+=OnLaunched;
        }
        // Left half: soft disc (sparks). Right half: ring (bursts).
        private static Texture2D Atlas()
        {
            const int size=64;var texture=new Texture2D(size*2,size,TextureFormat.RGBA32,true){name="Stroke atlas",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear};
            var pixels=new Color32[size*2*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=(x+.5f)/size*2-1,dy=(y+.5f)/size*2-1,d=Mathf.Sqrt(dx*dx+dy*dy);
                float disc=Mathf.Clamp01((1-d)*1.6f);disc=disc*disc*(3-2*disc);
                float ring=Mathf.Clamp01((d-.66f)/.10f)*Mathf.Clamp01((.94f-d)/.07f);
                pixels[y*size*2+x]=new Color32(255,255,255,(byte)(disc*255));pixels[y*size*2+size+x]=new Color32(255,255,255,(byte)(ring*255));
            }
            texture.SetPixels32(pixels);texture.Apply(true,true);return texture;
        }
        private void OnLaunched(SquashClash.Core.Config.EffectiveConfig cfg){streakLeft=0;if(streak!=null)streak.enabled=false;}
        private void OnRivalSwing(StrokeSide side,bool contact){if(contact)rivalPending=true;}
        private void OnDelivered(ShotPresentation p)
        {
            if(p.LaunchId!=host.LaunchId)return;
            var record=controller.LastRecord;bool mine=record!=null&&record.LaunchId==p.LaunchId&&record.AttemptId==p.AttemptId;
            float power=mine?Mathf.Clamp01(record.Power):.5f;
            HumanStrokes++;HumanPower=LastPower=power;
            if(p.Category==PresentationCategory.Failed||!p.HasContact){HumanKind=LastKind=StrokeKind.Failed;return;}
            var kind=p.Category==PresentationCategory.Perfect?StrokeKind.Perfect:StrokeKind.Valid;HumanKind=LastKind=kind;
            var direction=mine?new Vector3(record.VelocityX,record.VelocityY,record.VelocityZ):Vector3.back;
            Burst(kind,p.ContactPosition,direction,power);
        }
        private float Next(){random^=random<<13;random^=random>>17;random^=random<<5;return (random&0xFFFFFF)/(float)0x1000000;}
        private void Ring(Vector3 position,float from,float to,float alpha,float delay,Color color)
        {
            ref var q=ref quads[nextRing];nextRing=(nextRing+1)%Rings;
            q.Active=true;q.Position=position;q.Velocity=Vector3.zero;q.Age=-delay;q.Life=RingSeconds;q.From=from;q.To=to;q.Alpha=alpha;q.Color=color;
        }
        private void Spark(Vector3 position,Vector3 velocity,float size,float alpha,Color color)
        {
            ref var q=ref quads[nextSpark];nextSpark=nextSpark+1>=MaxQuads?Rings:nextSpark+1;
            q.Active=true;q.Position=position;q.Velocity=velocity;q.Age=0;q.Life=SparkSeconds*(.7f+.5f*Next());q.From=size;q.To=size*.35f;q.Alpha=alpha;q.Color=color;
        }
        // The burst: rings that open from the contact and sparks thrown along the outgoing ball, both larger with power.
        private void Burst(StrokeKind kind,Vector3 position,Vector3 direction,float power)
        {
            direction=direction.sqrMagnitude>1e-6f?direction.normalized:Vector3.back;
            bool perfect=kind==StrokeKind.Perfect;
            var colour=perfect?PerfectColor:kind==StrokeKind.Rival?RivalColor:ValidColor;
            Ring(position,.16f,(perfect?.62f:.44f)+(perfect?.78f:.46f)*power,perfect?.95f:.90f,0,colour);
            if(perfect)Ring(position,.16f,.80f+.80f*power,.60f,.07f,PerfectGlow);
            int sparks=perfect?6+Mathf.RoundToInt(8*power):3+Mathf.RoundToInt(3*power);
            var side=Vector3.Cross(direction,Vector3.up);if(side.sqrMagnitude<1e-4f)side=Vector3.right;side.Normalize();var up=Vector3.Cross(side,direction);
            for(int i=0;i<sparks;i++)
            {
                float angle=Next()*2*Mathf.PI,spread=(perfect?.75f:.55f)*Mathf.Sqrt(Next()),speed=(perfect?3.5f+6.5f*power:2.2f+3f*power)*(.55f+.6f*Next());
                var velocity=(direction+(side*Mathf.Cos(angle)+up*Mathf.Sin(angle))*spread).normalized*speed;
                Spark(position,velocity,perfect?.13f+.05f*power:.11f,perfect?.95f:.90f,perfect&&(i&1)==0?PerfectGlow:colour);
            }
            if(perfect||power>=StrongFraction)
            {
                streakLife=streakLeft=perfect?.24f+.30f*power:.18f;streakColor=perfect?PerfectColor:colour;
                streak.widthMultiplier=perfect?.10f+.08f*power:.07f;
                for(int i=0;i<StreakPoints;i++)streakPoints[i]=position;
                streak.SetPositions(streakPoints);streak.enabled=true;PaintStreak();
            }
            dirty=true;
        }
        private void PaintStreak()
        {
            float a=Mathf.Clamp01(streakLeft/streakLife);
            streak.startColor=new Color(streakColor.r,streakColor.g,streakColor.b,0);streak.endColor=new Color(streakColor.r,streakColor.g,streakColor.b,.85f*a);
        }
        public void Tick(float dt)
        {
            if(app==null)return;
            // The rival's stroke is judged once its velocity is in the ball (the director relaunches from the contact state).
            if(rivalPending)
            {
                rivalPending=false;
                if(host.ActiveConfig!=null)
                {
                    var velocity=host.Current.Velocity;float strength=Mathf.Clamp01(velocity.magnitude/(float)host.ActiveConfig.Source.Get("shot.vMax"));
                    RivalStrokes++;LastKind=StrokeKind.Rival;LastPower=strength;Burst(StrokeKind.Rival,host.Current.Position,velocity,strength);
                }
            }
            if(app.Clock==null||app.Clock.Paused)return;
            if(dt>0)
            {
                for(int i=0;i<MaxQuads;i++)
                {
                    if(!quads[i].Active)continue;
                    quads[i].Age+=dt;dirty=true;
                    if(quads[i].Age>=quads[i].Life){quads[i].Active=false;continue;}
                    if(i>=Rings&&quads[i].Age>0)
                    {
                        quads[i].Position+=quads[i].Velocity*dt;quads[i].Velocity*=Mathf.Exp(-5f*dt);quads[i].Velocity.y-=4f*dt;
                    }
                }
                if(streakLeft>0)
                {
                    streakLeft-=dt;
                    if(streakLeft<=0){streakLeft=0;streak.enabled=false;}
                    else
                    {
                        for(int i=0;i<StreakPoints-1;i++)streakPoints[i]=streakPoints[i+1];
                        streakPoints[StreakPoints-1]=host.RenderPosition;streak.SetPositions(streakPoints);PaintStreak();
                    }
                }
            }
            if(dirty)Rebuild();
        }
        private void Rebuild()
        {
            dirty=false;bool any=false;
            Vector3 right=eye!=null?eye.right:Vector3.right,up=eye!=null?eye.up:Vector3.up,forward=eye!=null?eye.forward:Vector3.forward;
            for(int i=0;i<MaxQuads;i++)
            {
                int v=i*4;ref var q=ref quads[i];
                if(!q.Active||q.Age<0){vertices[v]=vertices[v+1]=vertices[v+2]=vertices[v+3]=Vector3.zero;any|=q.Active;continue;}
                any=true;
                float t=Mathf.Clamp01(q.Age/q.Life),ease=1-(1-t)*(1-t),size=Mathf.Lerp(q.From,q.To,ease)*.5f;
                Vector3 a=right*size,b=up*size;
                if(i>=Rings)
                {
                    // A spark is a short dash along its own motion, as the camera sees it.
                    var flat=q.Velocity-forward*Vector3.Dot(q.Velocity,forward);float speed=flat.magnitude;
                    if(speed>.05f){a=flat/speed*(size*(1+speed*.22f));b=Vector3.Cross(forward,a).normalized*size;}
                }
                vertices[v]=q.Position-a-b;vertices[v+1]=q.Position+a-b;vertices[v+2]=q.Position+a+b;vertices[v+3]=q.Position-a+b;
                var c=q.Color;c.a=(byte)(255*q.Alpha*(1-t*t));colors[v]=colors[v+1]=colors[v+2]=colors[v+3]=c;
            }
            mesh.SetVertices(vertices,0,vertices.Length,UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds);
            mesh.SetColors(colors,0,colors.Length,UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds);
            if(body.enabled!=any)body.enabled=any;
        }
        private void LateUpdate()=>Tick(Mathf.Min(Time.unscaledDeltaTime,.05f));
        private void OnDestroy()
        {
            if(controller!=null)controller.PresentationDelivered-=OnDelivered;
            if(rally!=null)rally.RivalSwing-=OnRivalSwing;
            if(host!=null)host.Launched-=OnLaunched;
            if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);if(streakMaterial!=null)Destroy(streakMaterial);if(atlas!=null)Destroy(atlas);
        }
    }
}
