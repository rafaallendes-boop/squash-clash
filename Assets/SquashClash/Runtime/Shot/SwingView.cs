using System;
using System.Collections.Generic;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    public enum SwingPhase {Idle,Intention,Impact,AirFollowThrough}
    // Procedural placeholder arm + racket on the human capsule. Intention comes only from the accepted Swing order;
    // impact / air only from the single final delivery. No rig, Animator, assets or colliders; never touches the ball, clock or camera.
    public sealed class SwingView : MonoBehaviour
    {
        public const float IntentionSeconds=.15f,ReachSeconds=.06f,FollowSeconds=.18f,HoldSeconds=.3f,AirSeconds=.28f,RecoverSeconds=.2f;
        private const float ShoulderLateral=.25f,ShoulderHeight=.45f,GripBack=.32f;
        private readonly struct Key {public readonly Vector3 Pos;public readonly float Seconds;public Key(Vector3 pos,float seconds){Pos=pos;Seconds=seconds;}}
        private AppBootstrap app;
        private ShotController controller;
        private PlaceholderPlayers players;
        private Transform rig,arm,head;
        private Mesh cube,racket;
        private readonly Dictionary<string,int> starts=new Dictionary<string,int>();
        private readonly List<Key> sequence=new List<Key>();
        private int index;
        private float elapsed;
        private Vector3 from,pos;
        private float sign=1;
        private long launch;
        private string attempt;
        private bool hasIntent;
        public SwingPhase Phase {get;private set;}
        public StrokeSide Side {get;private set;}
        public Transform Rig {get{EnsureRig();return rig;}}
        public Vector3 RacketLocalPosition=>pos;
        public Quaternion RacketLocalRotation=>head!=null?head.localRotation:Quaternion.identity;
        public int TotalStarts {get;private set;}
        // Presentation hooks for the character figure: which shoulder holds the racket, where that shoulder is drawn
        // (human-local), and a re-pose after the figure moved. They never change the racket position, phases or timing.
        public float ShoulderSign=>sign;
        public Func<float,Vector3> ShoulderAnchor {get;set;}
        public void Repose()=>ApplyPose();
        public int StartCountFor(string attemptId)=>attemptId!=null&&starts.TryGetValue(attemptId,out var n)?n:0;
        public void Initialize(AppBootstrap app,ShotController controller)
        {
            if(this.app!=null)return;this.app=app;this.controller=controller;
            app.Host.Launched+=OnLaunched;controller.SwingAccepted+=OnAccepted;controller.PresentationDelivered+=OnDelivered;controller.ShotResolved+=OnResolved;
            EnsureRig();SnapIdle();
        }
        private void EnsureRig()
        {
            if(rig!=null)return;
            if(players==null)players=UnityEngine.Object.FindFirstObjectByType<PlaceholderPlayers>();
            var human=players?.Human;if(human==null)return;
            // Art pass: the same two parts, drawn as a sleeved arm and a racket instead of two boxes. Poses and timing are unchanged.
            var palette=SquashClash.Runtime.Visual.CharacterView.HumanPalette;var material=SquashClash.Runtime.Visual.VisualKit.Toon;
            cube=SquashClash.Runtime.Visual.VisualKit.Rebase(SquashClash.Runtime.Visual.VisualKit.Limb("Swing arm",.75f,.6f,palette.Accent,.3f,palette.Skin,.72f),
                Matrix4x4.TRS(new Vector3(0,0,-.5f),Quaternion.FromToRotation(Vector3.up,Vector3.forward),Vector3.one));
            racket=SquashClash.Runtime.Visual.VisualKit.Racket("Swing racket",new Color(.98f,.86f,.18f),SquashClash.Runtime.Visual.VisualKit.Ink,new Color(.93f,.96f,1f));
            rig=new GameObject("Swing").transform;rig.SetParent(human,false);
            arm=Part("Arm",material);head=Part("Racket",material);head.GetComponent<MeshFilter>().sharedMesh=racket;
            ApplyPose();
        }
        private Transform Part(string name,Material material)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(rig,false);
            go.GetComponent<MeshFilter>().sharedMesh=cube;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=material;
            r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;return go.transform;
        }
        // Racket-head centre in human-local space (x lateral, y relative to the capsule centre, z toward the court; the front wall is at -z).
        private Vector3 Idle=>new Vector3(sign*.3f,-.25f,-.1f);
        private Vector3 Back=>new Vector3(sign*.55f,.5f,.5f);
        private Vector3 Follow=>new Vector3(-sign*.3f,.5f,-.55f);
        private Vector3 Contact(StrokeSide side)
        {var b=controller.Snapshot.Body;var p=b.IdealContactPoint(side);return new Vector3(p.x-b.PositionXZ.x,p.y-b.ReferenceHeight,p.z-b.PositionXZ.y);}
        private void SetSide(StrokeSide side)
        {
            Side=side;var b=controller.Snapshot.Body;var x=b.IdealContactPoint(side).x-b.PositionXZ.x;sign=x<0?-1:1;
        }
        private void Begin(SwingPhase phase,params Key[] keys)
        {
            Phase=phase;sequence.Clear();sequence.AddRange(keys);index=0;elapsed=0;from=pos;
        }
        private void SnapIdle()
        {
            Phase=SwingPhase.Idle;sequence.Clear();index=0;elapsed=0;hasIntent=false;attempt=null;pos=from=Idle;ApplyPose();
        }
        private void OnLaunched(SquashClash.Core.Config.EffectiveConfig cfg)
        {starts.Clear();TotalStarts=0;SnapIdle();}
        private void OnAccepted(SwingIntent intent)
        {
            if(app==null||intent.LaunchId!=app.Host.LaunchId||string.IsNullOrEmpty(intent.AttemptId)||starts.ContainsKey(intent.AttemptId))return;
            EnsureRig();starts[intent.AttemptId]=1;TotalStarts++;launch=intent.LaunchId;attempt=intent.AttemptId;hasIntent=true;
            SetSide(intent.Side);Begin(SwingPhase.Intention,new Key(Back,IntentionSeconds));ApplyPose();
        }
        private bool Mine(long launchId,string attemptId)=>hasIntent&&launchId==launch&&attemptId==attempt&&launchId==app.Host.LaunchId;
        private void OnDelivered(ShotPresentation p)
        {
            if(!Mine(p.LaunchId,p.AttemptId)||Phase!=SwingPhase.Intention)return;
            if(p.Category==PresentationCategory.Failed){Begin(SwingPhase.AirFollowThrough,new Key(Follow,AirSeconds),new Key(Follow,HoldSeconds));return;}
            if(!p.HasContact)return;
            if(Enum.TryParse<StrokeSide>(p.Side,out var side))SetSide(side);
            Begin(SwingPhase.Impact,new Key(Contact(Side),ReachSeconds),new Key(Follow,FollowSeconds),new Key(Follow,HoldSeconds));
        }
        // Accepted but resolved without a delivery (system fault, history expired, schedule rejected): back to idle, no impact or air.
        private void OnResolved(ShotRecord r)
        {
            if(!Mine(r.LaunchId,r.AttemptId)||Phase!=SwingPhase.Intention)return;
            hasIntent=false;Phase=SwingPhase.Idle;Begin(SwingPhase.Idle,new Key(Idle,RecoverSeconds));
        }
        public void Tick(double dt)
        {
            if(sequence.Count==0||dt<=0||app.Clock.Paused)return; // frozen with the ball while the clock is paused
            float left=(float)dt;
            while(left>0&&index<sequence.Count)
            {
                var key=sequence[index];float step=Mathf.Min(left,key.Seconds-elapsed);elapsed+=step;left-=step;
                float t=key.Seconds>0?Mathf.Clamp01(elapsed/key.Seconds):1;pos=Vector3.Lerp(from,key.Pos,t*t*(3-2*t));
                if(elapsed<key.Seconds-1e-6f)break;
                from=pos=key.Pos;elapsed=0;index++;
                if(index>=sequence.Count)
                {
                    if(Phase==SwingPhase.Impact||Phase==SwingPhase.AirFollowThrough){hasIntent=false;Begin(SwingPhase.Idle,new Key(Idle,RecoverSeconds));}
                    else{sequence.Clear();break;} // Intention holds the backswing until the attempt resolves; Idle is done
                }
            }
            ApplyPose();
        }
        private void ApplyPose()
        {
            EnsureRig();if(arm==null)return;
            var shoulder=ShoulderAnchor!=null?ShoulderAnchor(sign):new Vector3(sign*ShoulderLateral,ShoulderHeight,0);var dir=pos-shoulder;float len=dir.magnitude;if(len<1e-4f){dir=Vector3.forward;len=1;}dir/=len;
            var grip=pos-dir*GripBack;var forearm=grip-shoulder;float flen=Mathf.Max(forearm.magnitude,1e-3f);
            arm.localPosition=(shoulder+grip)*.5f;arm.localRotation=Quaternion.LookRotation(forearm.sqrMagnitude>1e-8f?forearm:dir);arm.localScale=new Vector3(.07f,.07f,flen);
            head.localPosition=pos;head.localRotation=Quaternion.LookRotation(dir,Mathf.Abs(dir.z)>.95f?Vector3.up:Vector3.back);head.localScale=new Vector3(.22f,.03f,.3f);
        }
        private void Update(){Tick(Time.unscaledDeltaTime);}
        private static Mesh Cube()
        {
            var v=new List<Vector3>();var n=new List<Vector3>();var t=new List<int>();
            void Face(Vector3 normal,Vector3 u,Vector3 w)
            {
                int b=v.Count;v.Add((normal-u-w)*.5f);v.Add((normal+u-w)*.5f);v.Add((normal+u+w)*.5f);v.Add((normal-u+w)*.5f);for(int i=0;i<4;i++)n.Add(normal);
                t.AddRange(new[]{b,b+2,b+1,b,b+3,b+2});
            }
            Face(Vector3.right,Vector3.up,Vector3.forward);Face(Vector3.left,Vector3.forward,Vector3.up);Face(Vector3.up,Vector3.forward,Vector3.right);
            Face(Vector3.down,Vector3.right,Vector3.forward);Face(Vector3.forward,Vector3.right,Vector3.up);Face(Vector3.back,Vector3.up,Vector3.right);
            var mesh=new Mesh{name="Swing box"};mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetTriangles(t,0);mesh.RecalculateBounds();return mesh;
        }
        private void OnDestroy()
        {
            if(app!=null)app.Host.Launched-=OnLaunched;
            if(controller!=null){controller.SwingAccepted-=OnAccepted;controller.PresentationDelivered-=OnDelivered;controller.ShotResolved-=OnResolved;}
            if(rig!=null)Destroy(rig.gameObject);if(cube!=null)Destroy(cube);if(racket!=null)Destroy(racket);
        }
    }
}
