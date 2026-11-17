using System.Collections.Generic;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Visual
{
    // Art pass: the drawn player. A figure of simple vertex-coloured parts posed in code every frame (two-bone IK for
    // legs and arms). It only reads state: the transform gameplay places, the rally bodies (velocity, phase), the swing
    // view's racket path, the gesture preview (how far the pull is), the stroke effects (result and power) and the
    // rival's swing event. It owns no gameplay state, colliders, rigs or Animator.
    // Feel pass: the body turns toward where it runs, backpedals with short steps, leans into accelerations, takes a
    // split step when the opponent hits, winds up and follows through in proportion to power, and both players have an
    // elbow and a racket that shows its face to the camera. E1 adds hand-specific preparation and follow-through;
    // the human's drawn head matches SwingView throughout the approved contact window. SwingView is read-only.
    public sealed class CharacterView : MonoBehaviour
    {
        public struct Palette { public Color Shirt,Accent,Shorts,Hair,Skin,Shoes,Socks; }
        public static Palette HumanPalette=>new Palette{Shirt=new Color(.10f,.50f,1f),Accent=new Color(.97f,.98f,1f),Shorts=new Color(.07f,.13f,.36f),
            Hair=new Color(.24f,.15f,.10f),Skin=VisualKit.Skin,Shoes=new Color(.97f,.97f,.97f),Socks=new Color(.97f,.97f,.97f)};
        public static Palette RivalPalette=>new Palette{Shirt=new Color(1f,.30f,.20f),Accent=new Color(.17f,.18f,.24f),Shorts=new Color(.96f,.96f,.98f),
            Hair=new Color(.09f,.08f,.08f),Skin=new Color(.82f,.60f,.45f),Shoes=new Color(1f,.82f,.18f),Socks=new Color(.17f,.18f,.24f)};
        public const int TrailPoints=8;
        public const float ContactHalfWindow=.060f;
        private const float ContactWindowEnd=SwingView.ReachSeconds+ContactHalfWindow;
        // A longer ease lowers the geometric speed peak; this is a duration, not a velocity clamp.
        private const float HumanFollowSeconds=.28f;
        public const float SplitStepSeconds=.24f,TrailFadeSeconds=.14f,MaxFootSwing=.26f,MaxStride=1.5f;
        private const float Thigh=.40f,Shin=.40f,UpperArm=.27f,Forearm=.26f,HipHeight=.80f,GripBack=.32f,CentreHeight=.9f;
        private static readonly Vector3 RacketScale=new Vector3(.22f,.03f,.30f),ToCamera=new Vector3(0,.55f,.83f);
        private static readonly Color HumanTrail=new Color(1f,.90f,.35f),RivalTrail=new Color(.45f,.98f,.90f),PerfectTrail=new Color(1f,.80f,.16f),MissTrail=new Color(.62f,.66f,.72f);
        private readonly List<Mesh> meshes=new List<Mesh>();
        private readonly Transform[] thigh=new Transform[2],shin=new Transform[2],shoe=new Transform[2],upper=new Transform[2],lower=new Transform[2];
        private readonly Vector3[] trailPoints=new Vector3[TrailPoints];
        private Transform torso,head,racket;private LineRenderer trail;private Material trailMaterial;
        private bool human,built,hasLast,swingHidden,trailLive;
        private Vector2 lastPosition,direction=new Vector2(0,-1),lastVelocity,accelLean;
        private float runPhase,move,yaw,crouch=.04f,idle,headYaw,swingClock=-1,racketSign=1,backpedal,wind,strokeClock=-1,strokePower=.5f,hopClock=-1,trailAlpha,footSwing;
        private StrokeKind strokeKind;private int seenHuman,seenRival;
        private Vector3 racketPosition,shoulderRacket,contactExit;
        private float contactAge=-1;private bool firstContactFrame,awaitingDelivery;
        private long acceptedLaunch;private string acceptedAttempt;
        private RallyDirector rally;private SwingView swing;private AppBootstrap app;private StrokeFx fx;private GesturePreview preview;private ShotController controller;private BallSimulationHost host;
        private int lookup;private bool subscribed;
        public bool IsHuman=>human;
        public float RunPhase=>runPhase;
        public float Move=>move;
        public float RacketSign=>racketSign;
        public bool Swinging=>human&&swing!=null?swing.Phase!=SwingPhase.Idle:swingClock>=0;
        public Vector3 RacketLocalPosition=>racketPosition;
        // Feel-pass read-outs for the tests.
        public float Yaw=>yaw;
        public float Backpedal=>backpedal;
        public float FootSwing=>footSwing;
        public float WindUp=>wind;
        public bool SplitStepping=>hopClock>=0;
        public bool TrailVisible=>trail!=null&&trail.enabled;
        public float StrokePower=>strokePower;
        // Device robustness read-outs: which of the player's own hands holds the racket (-1 left, +1 right) and how far
        // the wrist is from the grip. The player faces the front wall and +x is the left wall (AX-1), so the left hand
        // is the arm on +x (the parts whose names end in "R" are named after the sign of x, not after the player's side).
        public int RacketHand {get;private set;}=-1;
        public float GripGap {get;private set;}
        public void Initialize(bool isHuman,Palette palette)
        {
            if(built)return;built=true;human=isHuman;
            transform.localPosition=new Vector3(0,-CentreHeight,0);
            var toon=VisualKit.Toon;
            var body=Own(VisualKit.Lathe("Figure torso",new[]{
                new VisualKit.Ring(-.035f,.02f,palette.Shorts),new VisualKit.Ring(0,.125f,palette.Shorts),new VisualKit.Ring(.11f,.14f,palette.Shorts),
                new VisualKit.Ring(.11f,.14f,palette.Shirt),new VisualKit.Ring(.30f,.15f,palette.Shirt),new VisualKit.Ring(.41f,.185f,palette.Shirt),
                new VisualKit.Ring(.41f,.185f,palette.Accent),new VisualKit.Ring(.50f,.18f,palette.Accent),new VisualKit.Ring(.56f,.10f,palette.Accent),
                new VisualKit.Ring(.585f,.055f,palette.Skin),new VisualKit.Ring(.64f,.05f,palette.Skin)},12,1.12f,.74f));
            torso=VisualKit.Part("Torso",transform,body,toon).transform;
            // Feel pass: head and hair are one mesh (one draw call less per figure); the hair never moved on its own.
            head=VisualKit.Part("Head",transform,Own(Merge("Figure head",VisualKit.Sphere("Figure skull",.165f,palette.Skin),VisualKit.Sphere("Figure hair",.172f,palette.Hair),
                Matrix4x4.TRS(new Vector3(0,.035f,.045f),Quaternion.identity,new Vector3(1,.93f,1)))),toon).transform;
            var thighMesh=Own(VisualKit.Limb("Figure thigh",.088f,.064f,palette.Shorts,.48f,palette.Skin));
            var shinMesh=Own(VisualKit.Limb("Figure shin",.062f,.046f,palette.Skin,.6f,palette.Socks));
            var upperMesh=Own(VisualKit.Limb("Figure upper arm",.06f,.048f,palette.Accent,.62f,palette.Skin));
            var lowerMesh=Own(VisualKit.Limb("Figure forearm",.046f,.04f,palette.Skin,.5f,palette.Skin,.054f));
            var shoeMesh=Own(VisualKit.Sphere("Figure shoe",1,palette.Shoes,10,6));
            for(int i=0;i<2;i++)
            {
                string side=i==0?"L":"R";
                thigh[i]=VisualKit.Part("Thigh"+side,transform,thighMesh,toon).transform;shin[i]=VisualKit.Part("Shin"+side,transform,shinMesh,toon).transform;
                shoe[i]=VisualKit.Part("Shoe"+side,transform,shoeMesh,toon).transform;shoe[i].localScale=new Vector3(.068f,.052f,.135f);
                upper[i]=VisualKit.Part("UpperArm"+side,transform,upperMesh,toon).transform;lower[i]=VisualKit.Part("Forearm"+side,transform,lowerMesh,toon).transform;
            }
            racket=VisualKit.Part("Racket",transform,Own(VisualKit.Racket("Figure racket",isHuman?new Color(.98f,.86f,.18f):new Color(.30f,.95f,.85f),VisualKit.Ink,new Color(.93f,.96f,1f),openFace:true)),toon).transform;
            racket.localScale=RacketScale;
            var shadow=VisualKit.Part("Contact shadow",transform,Own(VisualKit.FloorQuad("Figure shadow")),VisualKit.ContactShadow).transform;
            shadow.localPosition=new Vector3(0,.004f,.03f);shadow.localScale=new Vector3(.95f,1,.68f);
            // The racket head leaves a short arc while it swings: it is what makes the instant of the stroke readable from the camera.
            var line=new GameObject("Racket trail");line.transform.SetParent(transform,false);
            trail=line.AddComponent<LineRenderer>();trailMaterial=VisualKit.NewFx("Racket trail",Color.white,false,Texture2D.whiteTexture);
            trail.sharedMaterial=trailMaterial;trail.useWorldSpace=true;trail.positionCount=TrailPoints;trail.numCapVertices=3;trail.widthMultiplier=.2f;
            trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;trail.receiveShadows=false;trail.enabled=false;
            racketPosition=RelaxedPose(1);
            Animate(0);
        }
        private Mesh Own(Mesh mesh){meshes.Add(mesh);return mesh;}
        private static Mesh Merge(string name,Mesh first,Mesh second,Matrix4x4 secondPlace)
        {
            var mesh=new Mesh{name=name};
            mesh.CombineMeshes(new[]{new CombineInstance{mesh=first,transform=Matrix4x4.identity},new CombineInstance{mesh=second,transform=secondPlace}},true,true);
            Destroy(first);Destroy(second);return mesh;
        }
        // Racket-head centres in figure space (x lateral, y from the floor, -z toward the front wall).
        private static Vector3 RelaxedPose(float s)=>new Vector3(s*.40f,.62f,-.22f);
        private static Vector3 ReadyPose(float s)=>new Vector3(s*.34f,1.22f,-.58f);
        private static Vector3 BackPose(float s)=>new Vector3(s*.52f,1.38f,.42f);
        private static Vector3 HitPose(float s)=>new Vector3(s*.62f,.95f,-.30f);
        private static Vector3 FollowPose(float s)=>new Vector3(-s*.28f,1.40f,-.52f);
        private void Resolve()
        {
            if(subscribed&&(!human||swing!=null))return;
            if(lookup>0){lookup--;return;}
            lookup=30;
            if(app==null)app=AppBootstrap.Instance;if(app==null)return;
            if(!subscribed)
            {
                rally=app.Rally;host=app.Host;if(rally==null||host==null)return;
                fx=app.GetComponent<StrokeFx>();preview=app.GetComponent<GesturePreview>();controller=app.GetComponent<ShotController>();
                rally.RivalSwing+=OnRivalSwing;host.ShotApplied+=OnShotApplied;host.Launched+=OnLaunched;
                if(human&&controller!=null){controller.SwingAccepted+=OnAccepted;controller.PresentationDelivered+=OnDelivered;}subscribed=true;
                if(fx!=null){seenHuman=fx.HumanStrokes;seenRival=fx.RivalStrokes;}
            }
            if(human&&swing==null)swing=app.GetComponent<SwingView>();
        }
        private void OnLaunched(SquashClash.Core.Config.EffectiveConfig config)
        {contactAge=-1;firstContactFrame=false;awaitingDelivery=false;acceptedAttempt=null;}
        private void OnAccepted(SwingIntent intent)
        {
            // SwingView's listener runs first. Record only the intention that it actually accepted.
            if(swing==null||intent.LaunchId!=host.LaunchId||string.IsNullOrEmpty(intent.AttemptId)||
                swing.Phase!=SwingPhase.Intention||swing.StartCountFor(intent.AttemptId)!=1)return;
            acceptedLaunch=intent.LaunchId;acceptedAttempt=intent.AttemptId;awaitingDelivery=true;contactAge=-1;
        }
        private void OnDelivered(ShotPresentation presentation)
        {
            if(!awaitingDelivery||presentation.LaunchId!=host.LaunchId||presentation.LaunchId!=acceptedLaunch||
                presentation.AttemptId!=acceptedAttempt||swing==null)return;
            // At this subscriber, SwingView has already left Intention if it accepted this final delivery.
            // A duplicate, unrelated identity or delivery after Idle must not restart the local window.
            if(presentation.Category==PresentationCategory.Failed)
            {if(swing.Phase==SwingPhase.AirFollowThrough)awaitingDelivery=false;return;}
            if(!presentation.HasContact||swing.Phase!=SwingPhase.Impact)return;
            awaitingDelivery=false;
            // This is a local presentation age, never a game clock. The first draw does not consume time: if the
            // delivery occurred after SwingView.Update, the lock may end a frame later, never a frame early.
            contactAge=0;firstContactFrame=true;
        }
        private void OnRivalSwing(StrokeSide side,bool contact)
        {
            // The opponent just hit: the human takes a split step. The rival swings.
            if(human){if(contact&&rally!=null&&rally.Active)hopClock=0;return;}
            racketSign=side==StrokeSide.Drive?-1:1;
            if(host.ActiveConfig!=null)
            {
                var ball=transform.InverseTransformPoint(host.RenderPosition);
                // The racket meets the ball where the contact happened, when that is within reach of the drawn arm.
                if(contact)racketPosition=new Vector2(ball.x,ball.z).magnitude<1.1f&&ball.y<1.9f?ball:HitPose(racketSign);
            }
            swingClock=contact?.07f:0;strokeClock=0;strokeKind=contact?StrokeKind.Rival:StrokeKind.Failed;strokePower=.6f;
        }
        private void OnShotApplied(ShotApplyResult result)
        {
            if(!human&&result.Applied&&rally!=null&&rally.Active)hopClock=0;
        }
        private void LateUpdate()
        {
            Resolve();
            bool paused=app!=null&&app.Clock!=null&&app.Clock.Paused;
            Animate(paused?0:Mathf.Min(Time.unscaledDeltaTime,.05f));
        }
        private static float Smooth(float t){t=Mathf.Clamp01(t);return t*t*(3-2*t);}
        // dt drives only smoothing, the idle breath and the stroke clocks; the stride follows the distance actually
        // travelled, so feet do not slide and a paused clock (dt 0, no travel) freezes the figure.
        public void Animate(float dt)
        {
            if(!built)return;
            var parent=transform.parent;var world=parent!=null?parent.position:transform.position;
            var position=new Vector2(world.x,world.z);
            var step=hasLast?position-lastPosition:Vector2.zero;float travelled=step.magnitude;
            if(travelled>.6f){travelled=0;step=Vector2.zero;} // a teleport (new point, practice placement) is not a stride
            lastPosition=position;hasLast=true;
            PlayerBody body=rally!=null&&rally.Active?(human?rally.Human:rally.Rival):null;
            // A frame without time (pause) keeps the last velocity: nothing derived from it may move.
            var velocity=body!=null?body.Velocity:(dt>0?step/dt:lastVelocity);float speed=velocity.magnitude;
            var bodyPhase=body!=null?body.Phase:BodyPhase.Waiting;
            float blend=1-Mathf.Exp(-dt*12),slow=1-Mathf.Exp(-dt*6);
            if(travelled>1e-4f&&dt>0)
            {
                var turned=Vector2.Lerp(direction,step/travelled,1-Mathf.Exp(-dt*16));
                direction=turned.sqrMagnitude>1e-4f?turned.normalized:step/travelled;
            }
            // Away from the front wall (+z) the player backpedals: short quick steps, facing the wall, instead of running with the back to the ball.
            backpedal=Mathf.Lerp(backpedal,speed>.25f?Mathf.Clamp01(velocity.y/speed*1.4f-.2f):0,slow);
            // Above 3 m/s the cadence rises instead of the stride: the step never gets longer than at a run.
            float stride=Mathf.Lerp(Mathf.Clamp(.9f+.2f*speed,.9f,MaxStride),Mathf.Clamp(.62f+.13f*speed,.62f,1.05f),backpedal);
            runPhase+=travelled*(2*Mathf.PI/stride);
            move=Mathf.MoveTowards(move,Mathf.Clamp01(speed/1.4f),dt*7);idle+=dt;
            if(dt>0)
            {
                var accel=Vector2.ClampMagnitude((velocity-lastVelocity)/dt*.9f,9);
                accelLean=Vector2.Lerp(accelLean,accel,slow);lastVelocity=velocity;
            }
            // Stroke clocks: the result and power of the human's stroke come from the stroke effects; the rival's power too.
            if(fx!=null)
            {
                if(human&&fx.HumanStrokes!=seenHuman){seenHuman=fx.HumanStrokes;strokeClock=0;strokeKind=fx.HumanKind;strokePower=fx.HumanPower;}
                if(!human&&fx.RivalStrokes!=seenRival){seenRival=fx.RivalStrokes;strokePower=fx.LastPower;}
            }
            if(strokeClock>=0){strokeClock+=dt;if(strokeClock>.9f)strokeClock=-1;}
            if(hopClock>=0){hopClock+=dt;if(hopClock>=SplitStepSeconds)hopClock=-1;}
            float hop=hopClock>=0?Mathf.Sin(Mathf.PI*hopClock/SplitStepSeconds):0;
            bool swingDriven=human&&swing!=null;var swingPhase=swingDriven?swing.Phase:SwingPhase.Idle;
            if(contactAge>=0){if(firstContactFrame)firstContactFrame=false;else contactAge+=dt;}
            bool contactLock=swingDriven&&swingPhase==SwingPhase.Impact&&contactAge>=0&&contactAge<=ContactWindowEnd+1e-5f;
            bool intentionReference=swingDriven&&swingPhase==SwingPhase.Intention;
            if(swingDriven&&!swingHidden&&swing.Rig!=null)
            {
                // SwingView keeps computing its racket path (the figure follows it); its two boxes are no longer drawn.
                swingHidden=true;foreach(var part in swing.Rig.GetComponentsInChildren<MeshRenderer>(true))part.enabled=false;
            }
            // Which side holds the racket: the side of the planned contact.
            if(swingDriven&&swingPhase!=SwingPhase.Idle)racketSign=swing.ShoulderSign;
            else if(human&&controller!=null&&controller.Snapshot!=null&&controller.Plan.Exists)
            {
                var frame=controller.Snapshot.Body;racketSign=frame.IdealContactPoint(controller.Plan.Side).x-frame.PositionXZ.x<0?-1:1;
            }
            bool prepare=false;
            if(human)
            {
                // Wind-up follows the pull of the finger while it aims; in Demo the body prepares when it plants.
                bool aiming=preview!=null&&preview.M24!=null&&preview.M24.Active&&preview.M24.HasAim;
                bool planted=body!=null&&rally.Phase==RallyPhase.HumanReception&&(bodyPhase==BodyPhase.Set||bodyPhase==BodyPhase.Approaching&&Vector2.Distance(body.Position,body.Target)<1.1f);
                prepare=swingPhase==SwingPhase.Idle&&(aiming||planted);
                if(swingPhase==SwingPhase.Idle)wind=Mathf.Lerp(wind,aiming?.5f+.5f*Mathf.Clamp01(preview.M24.P):planted?.8f:0,blend);
            }
            else
            {
                prepare=body!=null&&swingClock<0&&rally.Phase==RallyPhase.RivalReception&&
                    (bodyPhase==BodyPhase.Set||bodyPhase==BodyPhase.Approaching&&Vector2.Distance(body.Position,body.Target)<1.1f);
                if(prepare&&host.ActiveConfig!=null)
                {
                    float x=host.RenderPosition.x-world.x;
                    if(Mathf.Abs(x)>.2f)racketSign=Mathf.Sign(x);
                }
                if(swingClock<0)wind=Mathf.Lerp(wind,prepare?.6f:0,blend);
            }
            float sign=racketSign,power=Mathf.Clamp01(strokePower);
            // The human keeps the chosen hand on either contact side: left on +x, right on -x (AX-1).
            // Across the body, that arm reaches over for a backhand. The rival is always right-handed.
            float holds=human&&Handedness.Left?1:-1;bool across=holds!=sign;
            bool striking=swingDriven?swingPhase==SwingPhase.Impact||swingPhase==SwingPhase.AirFollowThrough:swingClock>=0;
            bool missed=striking&&strokeKind==StrokeKind.Failed;
            bool winding=swingDriven?swingPhase==SwingPhase.Intention||prepare:prepare;
            // Reaching across, the hitting shoulder turns toward the ball instead of opening away from it.
            float twist=striking?(across?-(missed?40:48+8*power):missed?58:26+20*power)*sign:winding?-(across?30+18*wind:14+18*wind)*sign:0;
            // The body turns toward where it runs; moving back it turns only side-on.
            float travelYaw=0;
            if(speed>.2f)
            {
                float theta=Mathf.Atan2(-velocity.x,-velocity.y)*Mathf.Rad2Deg,a=Mathf.Abs(theta);
                travelYaw=Mathf.Sign(theta)*(a<=90?Mathf.Min(a*.85f,62):(180-a)*.69f)*move*(bodyPhase==BodyPhase.Set?.35f:1);
            }
            yaw=Mathf.Lerp(yaw,(striking||winding?travelYaw*.3f:travelYaw)+twist,blend);
            float ready=body!=null&&bodyPhase==BodyPhase.Waiting?1:0;
            crouch=Mathf.MoveTowards(crouch,body==null?.04f:bodyPhase==BodyPhase.Set?.13f:bodyPhase==BodyPhase.Waiting?.09f:.06f+.03f*backpedal,dt*.6f);
            var along=new Vector3(direction.x,0,direction.y);
            // The stroke carries the body: a short lunge toward the front wall that grows with power; a miss stumbles across.
            float lunge=strokeClock>=0?Mathf.Sin(Mathf.PI*Mathf.Clamp01(strokeClock/.42f)):0;
            var carry=missed?new Vector3(-sign*.09f,-.05f,-.04f)*lunge:new Vector3(sign*.03f,-(.03f+.05f*power),-(.05f+.11f*power))*lunge;
            float bob=Mathf.Abs(Mathf.Sin(runPhase))*.035f*move*(1-.5f*backpedal)+Mathf.Sin(idle*2.4f)*.008f*(1-move);
            var hip=new Vector3(0,HipHeight-crouch+bob+hop*.055f,0)+carry;
            var turn=Quaternion.Euler(0,yaw,0);
            var lean=Quaternion.AngleAxis(Mathf.Min(speed*2.8f,13)*move*(1-backpedal),Vector3.Cross(Vector3.up,along));
            float push=accelLean.magnitude;
            if(push>.05f)lean=Quaternion.AngleAxis(push,Vector3.Cross(Vector3.up,new Vector3(accelLean.x,0,accelLean.y)/push))*lean;
            if(missed)lean=Quaternion.AngleAxis(9*lunge,Vector3.forward*sign)*lean;
            var spine=lean*Quaternion.Euler(-8-crouch*45-6*lunge*(missed?0:power),yaw,0);
            torso.localPosition=hip;torso.localRotation=spine;

            var headAt=hip+spine*new Vector3(0,.745f,0);
            float look=0;
            if(host!=null&&host.ActiveConfig!=null)
            {
                var to=transform.InverseTransformPoint(host.RenderPosition)-headAt;
                look=Mathf.Clamp(Mathf.Atan2(-to.x,-to.z)*Mathf.Rad2Deg,-70,70);
            }
            headYaw=Mathf.Lerp(headYaw,look,1-Mathf.Exp(-dt*10));
            head.localPosition=headAt;head.localRotation=Quaternion.Euler(missed?-4+16*lunge:-4,headYaw,0);

            // Legs: the stride is bounded (it gets quicker, not longer, at top speed) and shortens going back.
            footSwing=Mathf.Min(MaxFootSwing,stride*.18f)*Mathf.Lerp(1,.8f,backpedal);
            float lift=Mathf.Lerp(.17f,.07f,backpedal),stance=Mathf.Lerp(Mathf.Lerp(.22f,.12f,move),.20f,backpedal)+.05f*ready*(1-move)+.05f*hop;
            for(int i=0;i<2;i++)
            {
                float s=i==0?-1:1,phase=runPhase+(i==0?Mathf.PI:0);
                var joint=hip+turn*new Vector3(s*.095f,0,0);
                var foot=new Vector3(s*stance,.05f+Mathf.Max(0,Mathf.Cos(phase))*lift*move,.02f)+along*(Mathf.Sin(phase)*footSwing*move);
                // The foot on the racket side steps into the stroke.
                if(s==sign&&!missed)foot.z-=(.10f+.14f*power)*lunge;
                var knee=VisualKit.Joint(joint,foot+Vector3.up*.03f,Thigh,Shin,new Vector3(s*.25f,0,-1),out var ankle);
                VisualKit.Bone(thigh[i],joint,knee);VisualKit.Bone(shin[i],knee,ankle);
                shoe[i].localPosition=ankle+new Vector3(0,-.03f,-.05f);
            }

            // Hand-specific preparation and finish are visual only. The contact interval bypasses smoothing
            // and offsets completely; its target is transformed from the untouched SwingView rig.
            int racketArm=holds>0?1:0;RacketHand=racketArm==1?-1:1;
            shoulderRacket=hip+spine*new Vector3(holds*.205f,.50f,0);
            Vector3 target;float rate;
            var rest=body==null?(human?ReadyPose(holds):RelaxedPose(holds)):bodyPhase==BodyPhase.Waiting?ReadyPose(holds):RelaxedPose(holds);
            // Reaching across, the racket is taken back close to the far shoulder, where the arm can hold it.
            var back=across?new Vector3(sign*.30f,1.30f,.30f)+new Vector3(sign*.03f,.10f,.08f)*wind:BackPose(sign)+new Vector3(sign*.05f,.16f,.12f)*wind;
            // After the contact: hard strokes finish high and across the body, soft ones short and low; a miss wraps around.
            float through=strokeClock>=0?Smooth((strokeClock-.06f)/.16f):0;
            // (Reaching across, the follow-through opens outward, away from the body.)
            var cleanFollow=across?new Vector3(sign*(.46f+.10f*power),1.18f+.42f*power,-.42f-.12f*power):
                FollowPose(sign)+new Vector3(-sign*.22f,.36f,-.14f)*(power-.35f);
            var finish=missed?new Vector3(-sign*.30f,-.34f,.02f):new Vector3((across?sign:-sign)*.22f,.36f,-.14f)*(power-.35f);
            if(swingDriven&&swingPhase!=SwingPhase.Idle)
            {
                rate=60;
                var reference=transform.InverseTransformPoint(swing.Rig.TransformPoint(swing.RacketLocalPosition));
                if(contactLock){target=reference;contactExit=reference;}
                // A delivery may interrupt Intention at any time, including before its Back key is reached.
                // Keep its head on the source throughout Intention; own preparation stays in aiming/planted Idle
                // and in the shoulder pose. There is no unmatched offset to snap away at the contact boundary.
                else if(intentionReference)target=reference;
                else if(swingPhase==SwingPhase.Impact)target=contactAge<0?reference:Vector3.Lerp(contactExit,cleanFollow,Smooth((contactAge-ContactWindowEnd)/HumanFollowSeconds));
                else target=reference+finish*through;
            }
            else if(!human&&swingClock>=0)
            {
                swingClock+=dt;
                if(swingClock<.07f){target=HitPose(sign);rate=45;}
                else if(swingClock<.55f){target=missed?FollowPose(sign)+finish*through:cleanFollow;rate=16;}
                else{swingClock=-1;target=rest;rate=7;}
            }
            else if(prepare){target=Vector3.Lerp(rest,back,human?wind:1);rate=9;}
            else{target=rest;rate=7;}
            racketPosition=contactLock||intentionReference?target:Vector3.Lerp(racketPosition,target,1-Mathf.Exp(-dt*rate));
            var reach=racketPosition-shoulderRacket;float length=reach.magnitude;
            var aim=length>1e-4f?reach/length:Vector3.forward;
            // The strings face the camera behind the court, so the racket never shows edge-on at the contact.
            var face=ToCamera-aim*Vector3.Dot(ToCamera,aim);
            racket.localPosition=racketPosition;
            racket.localRotation=Quaternion.LookRotation(aim,face.sqrMagnitude>.02f?face:Vector3.up);
            float pop=!human&&strokeClock>=0&&!missed?1-Smooth(strokeClock/.16f):0;
            racket.localScale=RacketScale*(contactLock?1:1+.25f*pop);

            for(int i=0;i<2;i++)
            {
                float s=i==0?-1:1;
                var shoulder=hip+spine*new Vector3(s*.205f,.50f,0);
                Vector3 hand,hint;
                if(i==racketArm)
                {
                    var toHead=racketPosition-shoulder;float distance=Mathf.Max(toHead.magnitude,1e-4f);
                    hand=racketPosition-toHead/distance*GripBack;hint=new Vector3(s*.7f,-.6f,.5f);
                }
                else
                {
                    float phase=runPhase+(i==0?0:Mathf.PI);
                    var running=hip+new Vector3(s*.27f,.20f,-.06f)+along*(Mathf.Sin(phase)*.24f);
                    var balance=hip+new Vector3(s*.40f,.26f,-.10f);
                    var waiting=hip+new Vector3(s*.34f,.30f,-.20f);
                    hand=Vector3.Lerp(waiting,Vector3.Lerp(running,balance,backpedal),move);
                    // The free arm counters the stroke.
                    hand+=new Vector3(s*.10f,.10f,.14f)*lunge*(missed?1:power);hint=new Vector3(s*.6f,-.4f,.7f);
                }
                var elbow=VisualKit.Joint(shoulder,hand,UpperArm,Forearm,hint,out var wrist);
                if(i==racketArm)GripGap=(wrist-hand).magnitude;
                VisualKit.Bone(upper[i],shoulder,elbow);VisualKit.Bone(lower[i],elbow,wrist);
            }
            PaintTrail(dt,striking||swingDriven&&swingPhase==SwingPhase.Intention,missed,power,contactLock||!human&&swingClock>=0&&swingClock<=.13f);
        }
        private void PaintTrail(float dt,bool swinging,bool missed,float power,bool clearContact)
        {
            // No opaque trail over the ball while the head is at contact. The open racket rim remains visible.
            if(clearContact){trail.enabled=false;trailLive=false;return;}
            if(dt<=0)return;
            trailAlpha=Mathf.MoveTowards(trailAlpha,swinging?1:0,dt/(swinging?.03f:TrailFadeSeconds));
            if(trailAlpha<=0){if(trail.enabled)trail.enabled=false;trailLive=false;return;}
            var headAt=transform.TransformPoint(racketPosition);
            if(!trailLive){trailLive=true;for(int i=0;i<TrailPoints;i++)trailPoints[i]=headAt;}
            else{for(int i=0;i<TrailPoints-1;i++)trailPoints[i]=trailPoints[i+1];trailPoints[TrailPoints-1]=headAt;}
            trail.SetPositions(trailPoints);
            bool perfect=human&&strokeClock>=0&&strokeKind==StrokeKind.Perfect;
            var colour=missed?MissTrail:perfect?PerfectTrail:human?HumanTrail:RivalTrail;
            trail.widthMultiplier=perfect?.21f:.15f+.05f*power;
            trail.startColor=new Color(colour.r,colour.g,colour.b,0);trail.endColor=new Color(colour.r,colour.g,colour.b,(missed?.45f:.7f)*trailAlpha);
            if(!trail.enabled)trail.enabled=true;
        }
        private void OnDestroy()
        {
            if(subscribed){if(rally!=null)rally.RivalSwing-=OnRivalSwing;if(host!=null){host.ShotApplied-=OnShotApplied;host.Launched-=OnLaunched;}
                if(human&&controller!=null){controller.SwingAccepted-=OnAccepted;controller.PresentationDelivered-=OnDelivered;}}
            if(trailMaterial!=null)Destroy(trailMaterial);
            foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);
            meshes.Clear();
        }
    }
}
