using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Camera;
using UnityEngine;
namespace SquashClash.Runtime.Visual
{
    // Art pass: short, cheap effects on ball impacts. A dust puff where the ball bounces on the floor, a ripple where it
    // meets a wall, and a subtle camera shake only for strong strokes. Pooled quads, no allocation after start, no
    // gameplay state. Racket contacts get no flash here on purpose: that cue belongs to PerfectFlashView (Perfect only).
    public sealed class ImpactFx : MonoBehaviour
    {
        public const int PoolSize=10;
        public const float PuffSeconds=.30f,RippleSeconds=.26f,StrongFraction=.8f,ShakeMeters=.028f,ShakeSeconds=.18f;
        private struct Slot { public Transform Transform;public MeshRenderer Renderer;public float Age,Life,From,To,Alpha;public bool Active; }
        private readonly Slot[] slots=new Slot[PoolSize];
        private AppBootstrap app;private BallSimulationHost host;private CourtCameraRig rig;
        private Mesh quad;private Material puff,ripple;private Texture2D ring;private MaterialPropertyBlock block;
        private int next;private bool strokePending;private SquashClash.Runtime.Rally.RallyDirector rally;
        public int ActiveCount {get{int n=0;for(int i=0;i<PoolSize;i++)if(slots[i].Active)n++;return n;}}
        public int Spawned {get;private set;}
        public int Shakes {get;private set;}
        public void Initialize(AppBootstrap app,CourtCameraRig rig)
        {
            if(this.app!=null)return;this.app=app;host=app.Host;this.rig=rig;
            block=new MaterialPropertyBlock();quad=VisualKit.BillboardQuad("Impact quad");
            ring=VisualKit.RingTexture("Impact ring",.62f,.96f,.14f);
            puff=VisualKit.NewFx("Impact puff",Color.white,false);ripple=VisualKit.NewFx("Impact ripple",Color.white,false,ring);
            for(int i=0;i<PoolSize;i++)
            {
                var renderer=VisualKit.Part("Impact",transform,quad,puff);renderer.gameObject.SetActive(false);
                slots[i]=new Slot{Transform=renderer.transform,Renderer=renderer};
            }
            host.ContactRaised+=OnContact;host.ShotApplied+=OnShot;host.Launched+=OnLaunched;
            rally=app.Rally;if(rally!=null)rally.RivalSwing+=OnRivalSwing;
        }
        private void OnLaunched(SquashClash.Core.Config.EffectiveConfig cfg){}
        private void OnShot(ShotApplyResult result){if(result.Applied)strokePending=true;}
        private void OnRivalSwing(StrokeSide side,bool contact){if(contact)strokePending=true;}
        private void OnContact(ContactEvent contact)
        {
            if(contact.Persistent)return;
            float speed=Mathf.Abs(Vector3.Dot(contact.ApproachVelocity,contact.Normal));
            if(speed<1.2f)return; // rolling and dying bounces stay quiet
            float strength=Mathf.Clamp01(speed/14f);
            bool floor=contact.Surface==SurfaceId.Floor;
            Spawn(contact.Point+contact.Normal*.006f,contact.Normal,floor?puff:ripple,floor?PuffSeconds:RippleSeconds,
                floor?.07f:.08f,floor?.20f+.16f*strength:.26f+.22f*strength,floor?.30f+.25f*strength:.45f+.35f*strength);
        }
        private void Spawn(Vector3 position,Vector3 normal,Material material,float life,float from,float to,float alpha)
        {
            ref var slot=ref slots[next];next=(next+1)%PoolSize;
            slot.Age=0;slot.Life=life;slot.From=from;slot.To=to;slot.Alpha=alpha;slot.Active=true;
            slot.Transform.position=position;slot.Transform.rotation=Quaternion.LookRotation(-normal);
            slot.Renderer.sharedMaterial=material;slot.Transform.gameObject.SetActive(true);Spawned++;
            Paint(ref slot);
        }
        private void Paint(ref Slot slot)
        {
            float t=Mathf.Clamp01(slot.Age/slot.Life),ease=1-(1-t)*(1-t);
            slot.Transform.localScale=Vector3.one*Mathf.Lerp(slot.From,slot.To,ease);
            block.SetColor("_BaseColor",new Color(1,1,1,slot.Alpha*(1-t)));slot.Renderer.SetPropertyBlock(block);
        }
        public void Tick(float dt)
        {
            if(app==null)return;
            // A stroke is judged once its velocity is in the ball: strong strokes (near the top of the speed range) nudge the camera.
            if(strokePending)
            {
                strokePending=false;
                if(host.ActiveConfig!=null&&rig!=null&&host.Current.Velocity.magnitude>=StrongFraction*(float)host.ActiveConfig.Source.Get("shot.vMax"))
                {rig.Shake(ShakeMeters,ShakeSeconds);Shakes++;}
            }
            if(dt<=0||app.Clock==null||app.Clock.Paused)return;
            for(int i=0;i<PoolSize;i++)
            {
                if(!slots[i].Active)continue;
                slots[i].Age+=dt;
                if(slots[i].Age>=slots[i].Life){slots[i].Active=false;slots[i].Transform.gameObject.SetActive(false);continue;}
                Paint(ref slots[i]);
            }
        }
        private void LateUpdate()=>Tick(Time.unscaledDeltaTime);
        private void OnDestroy()
        {
            if(host!=null){host.ContactRaised-=OnContact;host.ShotApplied-=OnShot;host.Launched-=OnLaunched;}
            if(rally!=null)rally.RivalSwing-=OnRivalSwing;
            if(quad!=null)Destroy(quad);if(puff!=null)Destroy(puff);if(ripple!=null)Destroy(ripple);if(ring!=null)Destroy(ring);
        }
    }
}
