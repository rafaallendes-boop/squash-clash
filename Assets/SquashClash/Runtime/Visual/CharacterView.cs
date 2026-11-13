using System;
using System.Collections.Generic;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Visual
{
    // Art pass: the drawn player. A figure of simple vertex-coloured parts posed in code every frame (two-bone IK for
    // legs and arms). It only reads state: the transform gameplay places, the rally bodies (speed, phase), the swing
    // view and the rival's swing event. It owns no gameplay state, colliders, rigs or Animator.
    public sealed class CharacterView : MonoBehaviour
    {
        public struct Palette { public Color Shirt,Accent,Shorts,Hair,Skin,Shoes,Socks; }
        public static Palette HumanPalette=>new Palette{Shirt=new Color(.10f,.50f,1f),Accent=new Color(.97f,.98f,1f),Shorts=new Color(.07f,.13f,.36f),
            Hair=new Color(.24f,.15f,.10f),Skin=VisualKit.Skin,Shoes=new Color(.97f,.97f,.97f),Socks=new Color(.97f,.97f,.97f)};
        public static Palette RivalPalette=>new Palette{Shirt=new Color(1f,.30f,.20f),Accent=new Color(.17f,.18f,.24f),Shorts=new Color(.96f,.96f,.98f),
            Hair=new Color(.09f,.08f,.08f),Skin=new Color(.82f,.60f,.45f),Shoes=new Color(1f,.82f,.18f),Socks=new Color(.17f,.18f,.24f)};
        private const float Thigh=.40f,Shin=.40f,UpperArm=.27f,Forearm=.26f,HipHeight=.80f,GripBack=.32f,CentreHeight=.9f;
        private static readonly Vector3 RacketScale=new Vector3(.22f,.03f,.30f);
        private readonly List<Mesh> meshes=new List<Mesh>();
        private readonly Transform[] thigh=new Transform[2],shin=new Transform[2],shoe=new Transform[2],upper=new Transform[2],lower=new Transform[2];
        private Transform torso,head,racket;
        private bool human,built,hasLast,racketShown=true;
        private int hiddenArm=-1;
        private Vector2 lastPosition,direction=new Vector2(0,-1);
        private float runPhase,move,yaw,crouch=.04f,idle,headYaw,swingClock=-1,racketSign=1;
        private Vector3 racketPosition,shoulderRacket;
        private RallyDirector rally;private SwingView swing;private AppBootstrap app;private int lookup;
        private Func<float,Vector3> anchor;
        public bool IsHuman=>human;
        public float RunPhase=>runPhase;
        public float Move=>move;
        public float RacketSign=>racketSign;
        public bool Swinging=>swingClock>=0;
        public Vector3 RacketLocalPosition=>racketPosition;
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
            head=VisualKit.Part("Head",transform,Own(VisualKit.Sphere("Figure head",.165f,palette.Skin)),toon).transform;
            var hair=VisualKit.Part("Hair",head,Own(VisualKit.Sphere("Figure hair",.172f,palette.Hair)),toon).transform;
            hair.localPosition=new Vector3(0,.035f,.045f);hair.localScale=new Vector3(1,.93f,1);
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
            racket=VisualKit.Part("Racket",transform,Own(VisualKit.Racket("Figure racket",isHuman?new Color(.98f,.86f,.18f):new Color(.30f,.95f,.85f),VisualKit.Ink,new Color(.93f,.96f,1f))),toon).transform;
            racket.localScale=RacketScale;
            var shadow=VisualKit.Part("Contact shadow",transform,Own(VisualKit.FloorQuad("Figure shadow")),VisualKit.ContactShadow).transform;
            shadow.localPosition=new Vector3(0,.004f,.03f);shadow.localScale=new Vector3(.95f,1,.68f);
            racketPosition=IdlePose(1);anchor=ShoulderForSwing;
            Animate(0);
        }
        private Mesh Own(Mesh mesh){meshes.Add(mesh);return mesh;}
        // Racket-head centres in figure space (x lateral, y from the floor, -z toward the front wall).
        private static Vector3 IdlePose(float s)=>new Vector3(s*.30f,.68f,-.12f);
        private static Vector3 BackPose(float s)=>new Vector3(s*.52f,1.38f,.42f);
        private static Vector3 HitPose(float s)=>new Vector3(s*.62f,.95f,-.30f);
        private static Vector3 FollowPose(float s)=>new Vector3(-s*.28f,1.40f,-.52f);
        private Vector3 ShoulderForSwing(float sign)=>shoulderRacket-new Vector3(0,CentreHeight,0);
        private void Resolve()
        {
            if(app!=null&&(human?swing!=null:rally!=null)&&rally!=null)return;
            if(lookup>0){lookup--;return;}
            lookup=30;
            if(app==null)app=AppBootstrap.Instance;if(app==null)return;
            if(rally==null)
            {
                rally=app.Rally;
                if(rally!=null&&!human)rally.RivalSwing+=OnRivalSwing;
            }
            if(human&&swing==null)
            {
                swing=app.GetComponent<SwingView>();
                if(swing!=null)swing.ShoulderAnchor=anchor;
            }
        }
        private void OnRivalSwing(StrokeSide side,bool contact)
        {
            if(app!=null&&app.Host.ActiveConfig!=null)
            {
                var ball=transform.InverseTransformPoint(app.Host.RenderPosition);
                if(Mathf.Abs(ball.x)>.1f)racketSign=Mathf.Sign(ball.x);
                // The racket meets the ball where the contact happened, when that is within reach of the drawn arm.
                if(contact)racketPosition=new Vector2(ball.x,ball.z).magnitude<1.1f&&ball.y<1.9f?ball:HitPose(racketSign);
            }
            swingClock=contact?.07f:0;
        }
        private void LateUpdate()
        {
            Resolve();
            bool paused=app!=null&&app.Clock!=null&&app.Clock.Paused;
            Animate(paused?0:Mathf.Min(Time.unscaledDeltaTime,.05f));
        }
        // dt drives only smoothing and the idle breath; the stride follows the distance actually travelled, so feet
        // do not slide and a paused clock (dt 0, no travel) freezes the figure.
        public void Animate(float dt)
        {
            if(!built)return;
            var parent=transform.parent;var world=parent!=null?parent.position:transform.position;
            var position=new Vector2(world.x,world.z);
            float travelled=hasLast?Vector2.Distance(position,lastPosition):0;
            if(travelled>.6f)travelled=0; // a teleport (new point, practice placement) is not a stride
            if(travelled>1e-4f)direction=(position-lastPosition)/travelled;
            lastPosition=position;hasLast=true;
            PlayerBody body=rally!=null&&rally.Active?(human?rally.Human:rally.Rival):null;
            float speed=body!=null?body.Speed:(dt>0?travelled/dt:0),lateral=body!=null?body.Velocity.x:0;
            var bodyPhase=body!=null?body.Phase:BodyPhase.Waiting;
            runPhase+=travelled*(2*Mathf.PI/1.5f);
            move=Mathf.MoveTowards(move,Mathf.Clamp01(speed/1.4f),dt*7);idle+=dt;
            crouch=Mathf.MoveTowards(crouch,body==null?.04f:bodyPhase==BodyPhase.Set?.12f:.07f,dt*.5f);
            var along=new Vector3(direction.x,0,direction.y);
            float sign=human&&swing!=null?swing.ShoulderSign:racketSign;
            float twist=0;bool prepare=false;
            if(human&&swing!=null)twist=swing.Phase==SwingPhase.Intention?-24*sign:swing.Phase==SwingPhase.Impact||swing.Phase==SwingPhase.AirFollowThrough?30*sign:0;
            else if(!human)
            {
                prepare=body!=null&&swingClock<0&&rally.Phase==RallyPhase.RivalReception&&
                    (bodyPhase==BodyPhase.Set||bodyPhase==BodyPhase.Approaching&&Vector2.Distance(body.Position,body.Target)<1.1f);
                if(prepare&&app!=null&&app.Host.ActiveConfig!=null)
                {
                    float x=app.Host.RenderPosition.x-world.x;
                    if(Mathf.Abs(x)>.2f)racketSign=Mathf.Sign(x);
                }
                sign=racketSign;
                twist=swingClock>=0?30*sign:prepare?-24*sign:0;
            }
            float blend=1-Mathf.Exp(-dt*12);
            yaw=Mathf.Lerp(yaw,-Mathf.Clamp(lateral*9,-28,28)*move+twist,blend);
            float bob=Mathf.Abs(Mathf.Sin(runPhase))*.035f*move+Mathf.Sin(idle*2.4f)*.008f*(1-move);
            var hip=new Vector3(0,HipHeight-crouch+bob,0);
            var turn=Quaternion.Euler(0,yaw,0);
            var lean=Quaternion.AngleAxis(Mathf.Min(speed*3.2f,14)*move,Vector3.Cross(Vector3.up,along));
            var spine=lean*Quaternion.Euler(-8-crouch*45,yaw,0);
            torso.localPosition=hip;torso.localRotation=spine;

            var headAt=hip+spine*new Vector3(0,.745f,0);
            float look=0;
            if(app!=null&&app.Host.ActiveConfig!=null)
            {
                var to=transform.InverseTransformPoint(app.Host.RenderPosition)-headAt;
                look=Mathf.Clamp(Mathf.Atan2(-to.x,-to.z)*Mathf.Rad2Deg,-70,70);
            }
            headYaw=Mathf.Lerp(headYaw,look,1-Mathf.Exp(-dt*10));
            head.localPosition=headAt;head.localRotation=Quaternion.Euler(-4,headYaw,0);

            for(int i=0;i<2;i++)
            {
                float s=i==0?-1:1,phase=runPhase+(i==0?Mathf.PI:0);
                var joint=hip+turn*new Vector3(s*.095f,0,0);
                var foot=new Vector3(s*Mathf.Lerp(.22f,.12f,move),.05f+Mathf.Max(0,Mathf.Cos(phase))*.17f*move,.02f)+along*(Mathf.Sin(phase)*.30f*move);
                var knee=VisualKit.Joint(joint,foot+Vector3.up*.03f,Thigh,Shin,new Vector3(s*.25f,0,-1),out var ankle);
                VisualKit.Bone(thigh[i],joint,knee);VisualKit.Bone(shin[i],knee,ankle);
                shoe[i].localPosition=ankle+new Vector3(0,-.03f,-.05f);
            }

            // The racket: the swing view draws it for the human; the figure draws it for the rival (and for a human without a swing view).
            bool external=human&&swing!=null;
            int racketArm=sign>0?1:0;
            shoulderRacket=hip+spine*new Vector3(sign*.205f,.50f,0);
            if(!external)
            {
                Vector3 target;float rate;
                if(swingClock>=0)
                {
                    swingClock+=dt;
                    if(swingClock<.07f){target=HitPose(sign);rate=45;}
                    else if(swingClock<.55f){target=FollowPose(sign);rate=16;}
                    else{swingClock=-1;target=IdlePose(sign);rate=7;}
                }
                else if(prepare){target=BackPose(sign);rate=9;}
                else{target=IdlePose(sign);rate=7;}
                racketPosition=Vector3.Lerp(racketPosition,target,1-Mathf.Exp(-dt*rate));
                var reach=racketPosition-shoulderRacket;float length=reach.magnitude;
                var aim=length>1e-4f?reach/length:Vector3.forward;
                racket.localPosition=racketPosition;
                racket.localRotation=Quaternion.LookRotation(aim,Mathf.Abs(aim.z)>.95f?Vector3.up:Vector3.back);
            }
            if(racketShown==external){racketShown=!external;racket.gameObject.SetActive(racketShown);}
            int hide=external?racketArm:-1;
            if(hide!=hiddenArm)
            {
                hiddenArm=hide;
                for(int i=0;i<2;i++){upper[i].gameObject.SetActive(i!=hide);lower[i].gameObject.SetActive(i!=hide);}
            }
            for(int i=0;i<2;i++)
            {
                if(i==hiddenArm)continue;
                float s=i==0?-1:1;
                var shoulder=hip+spine*new Vector3(s*.205f,.50f,0);
                Vector3 hand;
                if(i==racketArm)
                {
                    var reach=racketPosition-shoulder;float length=Mathf.Max(reach.magnitude,1e-4f);
                    hand=racketPosition-reach/length*GripBack;
                }
                else
                {
                    float phase=runPhase+(i==0?0:Mathf.PI);
                    var running=hip+new Vector3(s*.27f,.20f,-.06f)+along*(Mathf.Sin(phase)*.24f);
                    var ready=hip+new Vector3(s*.34f,.30f,-.20f);
                    hand=Vector3.Lerp(ready,running,move);
                }
                var elbow=VisualKit.Joint(shoulder,hand,UpperArm,Forearm,new Vector3(s*.6f,-.4f,.7f),out var wrist);
                VisualKit.Bone(upper[i],shoulder,elbow);VisualKit.Bone(lower[i],elbow,wrist);
            }
            if(external)swing.Repose();
        }
        private void OnDestroy()
        {
            if(rally!=null&&!human)rally.RivalSwing-=OnRivalSwing;
            if(swing!=null&&swing.ShoulderAnchor==anchor)swing.ShoulderAnchor=null;
            foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);
            meshes.Clear();
        }
    }
}
