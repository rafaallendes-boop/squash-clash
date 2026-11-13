using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
using SquashClash.Core.Config;
namespace SquashClash.Runtime.Shot
{
    [Serializable] public sealed class PracticeTaskRequest
    {
        public int FrontColumn,FrontRow,PowerThird,BounceColumn,BounceDepth;
        public PracticeTaskRequest(int frontColumn,int frontRow,int powerThird,int bounceColumn,int bounceDepth)
        {FrontColumn=frontColumn;FrontRow=frontRow;PowerThird=powerThird;BounceColumn=bounceColumn;BounceDepth=bounceDepth;}
    }
    [Serializable] public sealed class PracticeTaskRecord
    {
        public PracticeTaskRequest Requested;
        public long LaunchId,ContactStep;
        public string AttemptId,Stage,CaptureFailure,FrontFailure,BounceFailure,PriorSurfaces,LineTouches;
        public bool Applied,HasFrozenAim,HasFrozenPower,HasFeasibleAim,HasFrontImpact,HasFirstBounce,AimZoneHit,PowerBandHit,PowerSaturated,FirstBounceZoneHit,OutObserved,TinObserved;
        public Vector2 RequestedAim,FrozenAim,FeasibleAim;
        public Vector3 FrontImpact,FirstBounce;
        public float RequestedPower,FrozenPower,AimErrorCm=-1,FeasibleErrorCm=-1,FrontErrorCm=-1;
        public int ActualPowerThird=-1,ActualBounceColumn=-1,ActualBounceDepth=-1;
    }
    public sealed class PracticeTask : MonoBehaviour
    {
        private AppBootstrap app;
        private ShotController controller;
        private PracticeTaskRequest current,next;
        private ShotRecord observedShot;
        private EffectiveConfig receptionConfig;
        private Label label;
        private LineRenderer zone;
        private Material material;
        private string lastJson,preset,fileHash;
        private int pending,seed,lastContactCount=-1;
        private bool lastRunning;
        private ShotRecord lastObservedShot;
        public PracticeTaskRequest CurrentRequest=>Copy(current);
        public PracticeTaskRequest NextRequest=>Copy(next);
        public PracticeTaskRecord LastRecord {get;private set;}
        public void Initialize(AppBootstrap app,ShotController controller)
        {
            this.app=app;this.controller=controller;
            app.Host.Launched+=OnLaunch;app.Host.ContactRaised+=OnContact;
            controller.GestureClosed+=OnGesture;controller.ShotResolved+=OnGesture;
            var root=app.GetComponent<UIDocument>().rootVisualElement;
            label=new Label{name="practice-task",pickingMode=PickingMode.Ignore};
            label.style.maxWidth=Length.Percent(94);label.style.marginTop=6;label.style.paddingLeft=label.style.paddingRight=6;
            label.style.whiteSpace=WhiteSpace.Normal;label.style.color=Color.white;label.style.fontSize=14;
            label.style.backgroundColor=new Color(.02f,.06f,.08f,.86f);
            int header=root.IndexOf(root.Q("boot-header"));root.Insert(header<0?0:header+1,label);
            OnLaunch(app.Host.ActiveConfig);
        }
        public void Select(PracticeTaskRequest request)
        {if(request!=null)Validate(request);next=Copy(request);}
        private void OnLaunch(EffectiveConfig cfg)
        {
            if(LastRecord!=null&&LastRecord.Stage!="complete")
            {LastRecord.Stage="reception-replaced";Write(LastRecord);}
            current=cfg.Source.GetBool("practice.enabled")&&cfg.Backend=="P2-Analytic"?Copy(next):null;
            receptionConfig=cfg;preset=app.ActivePresetName;fileHash=app.ActivePresetFileHash;pending=app.Tuning.PendingKeys.Count;seed=app.MetricsSeed;
            observedShot=lastObservedShot=null;lastContactCount=-1;LastRecord=null;lastJson=null;ShowRequest();
            if(current!=null)
            {LastRecord=Observe(current,new ShotRecord{LaunchId=app.Host.LaunchId,Result="requested",Reason="awaiting-capture"});Write(LastRecord);}
        }
        private void OnGesture(ShotRecord shot)
        {if(current==null||shot.LaunchId!=app.Host.LaunchId)return;observedShot=shot;Tick();}
        private void OnContact(ContactEvent contact)
        {if(observedShot?.Applied==true)Tick();}
        public void Tick()
        {
            if(current==null||observedShot==null)return;
            int count=app.Host.CanonicalContacts.Count;bool running=app.Host.IsRunning;
            if(ReferenceEquals(lastObservedShot,observedShot)&&lastContactCount==count&&lastRunning==running)return;
            lastObservedShot=observedShot;lastContactCount=count;lastRunning=running;
            var r=Observe(current,observedShot);
            ObserveContacts(r,app.Host.CanonicalContacts,receptionConfig.Court,receptionConfig.Physics.BallRadius);
            if(observedShot.Applied&&!app.Host.IsRunning)r.Stage="complete";
            LastRecord=r;Write(r);
        }
        private void Write(PracticeTaskRecord r)
        {
            string json=JsonUtility.ToJson(r);if(json==lastJson)return;lastJson=json;
            app.Metrics?.Write("practice-task",receptionConfig,preset,fileHash,
                pending,seed,!app.Clock.MetricsInvalid,app.Clock.DroppedSinceRestart,json);
        }
        public static string Describe(PracticeTaskRequest r)
        {
            if(r==null)return "Sin tarea";
            string[] x={"[0,20;2,20)","[2,20;4,20)","[4,20;6,20]"};
            string[] y={"[0,60;1,60)","[1,60;2,60)","[2,60;3,60]"};
            string[] power={"suave [0;1/3)","media [1/3;2/3)","fuerte [2/3;1]"};
            string[] depth={"corto [0,20;3,20)","medio [3,20;6,40)","profundo [6,40;9,55]"};
            return "Observación provisional\nFrontis X "+x[r.FrontColumn]+" / Y "+y[r.FrontRow]+" m · P "+power[r.PowerThird]+"\nPrimer bote X "+x[r.BounceColumn]+" m / Z "+depth[r.BounceDepth]+" m";
        }
        private void ShowRequest()
        {
            label.text=Describe(current);label.style.display=current==null?DisplayStyle.None:DisplayStyle.Flex;
            if(current==null){if(zone!=null)zone.enabled=false;return;}
            if(zone==null)
            {
                var go=new GameObject("Practice task front rectangle");go.transform.SetParent(transform,false);
                zone=go.AddComponent<LineRenderer>();zone.useWorldSpace=true;zone.loop=true;zone.positionCount=4;zone.widthMultiplier=.008f;
                zone.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;zone.receiveShadows=false;
                material=new Material(Resources.Load<Shader>("Legibility"));material.SetColor("_BaseColor",new Color(.3f,1,.6f,.7f));zone.sharedMaterial=material;
            }
            float x=.2f+current.FrontColumn*2,y=.6f+current.FrontRow;
            zone.SetPositions(new[]{new Vector3(x,y,.025f),new Vector3(x+2,y,.025f),new Vector3(x+2,y+1,.025f),new Vector3(x,y+1,.025f)});zone.enabled=true;
        }
        private void LateUpdate()=>Tick();
        private void OnDestroy()
        {
            if(app!=null){app.Host.Launched-=OnLaunch;app.Host.ContactRaised-=OnContact;}
            if(controller!=null){controller.GestureClosed-=OnGesture;controller.ShotResolved-=OnGesture;}
            label?.RemoveFromHierarchy();if(zone!=null)Destroy(zone.gameObject);if(material!=null)Destroy(material);
        }
        private static PracticeTaskRequest Copy(PracticeTaskRequest r)
            =>r==null?null:new PracticeTaskRequest(r.FrontColumn,r.FrontRow,r.PowerThird,r.BounceColumn,r.BounceDepth);
        private static void Validate(PracticeTaskRequest r)
        {
            if(r==null)throw new ArgumentNullException(nameof(r));
            foreach(int band in new[]{r.FrontColumn,r.FrontRow,r.PowerThird,r.BounceColumn,r.BounceDepth})
                if(band<0||band>2)throw new ArgumentOutOfRangeException(nameof(r));
        }
        public static PracticeTaskRecord Observe(PracticeTaskRequest request,ShotRecord shot)
        {
            Validate(request);if(shot==null)throw new ArgumentNullException(nameof(shot));
            var r=new PracticeTaskRecord{Requested=Copy(request),LaunchId=shot.LaunchId,AttemptId=shot.AttemptId,ContactStep=shot.ContactStep,
                RequestedAim=new Vector2(1.2f+request.FrontColumn*2,1.1f+request.FrontRow),RequestedPower=(request.PowerThird+.5f)/3,
                FrontFailure=shot.Applied?"front-not-observed":"no-applied-contact",BounceFailure=shot.Applied?"bounce-not-observed":"no-applied-contact",
                PriorSurfaces="",LineTouches="",Stage=shot.Result};
            bool frozen=shot.GestureOutcome=="Swing"&&shot.HasRelease;
            r.HasFrozenAim=frozen&&Finite(shot.AimX)&&Finite(shot.AimY);
            r.HasFrozenPower=frozen&&PowerThird(shot.Power)>=0;
            r.CaptureFailure=r.HasFrozenAim&&r.HasFrozenPower?null:shot.Reason??shot.GestureOutcome??shot.Result??"no-capture";
            if(r.HasFrozenAim)
            {r.FrozenAim=new Vector2(shot.AimX,shot.AimY);r.AimErrorCm=Vector2.Distance(r.RequestedAim,r.FrozenAim)*100;r.AimZoneHit=FrontColumn(shot.AimX)==request.FrontColumn&&FrontRow(shot.AimY)==request.FrontRow;}
            if(r.HasFrozenPower)
            {r.FrozenPower=shot.Power;r.ActualPowerThird=PowerThird(shot.Power);r.PowerBandHit=r.ActualPowerThird==request.PowerThird;r.PowerSaturated=shot.Power==1;}
            r.Applied=shot.Applied;
            r.HasFeasibleAim=frozen&&shot.Solved&&Finite(shot.TargetUsedX)&&Finite(shot.TargetUsedY);
            if(r.HasFeasibleAim)
            {r.FeasibleAim=new Vector2(shot.TargetUsedX,shot.TargetUsedY);r.FeasibleErrorCm=Vector2.Distance(r.RequestedAim,r.FeasibleAim)*100;}
            return r;
        }
        public static void ObserveContacts(PracticeTaskRecord record,IReadOnlyList<ContactEvent> contacts,CourtDimensions court,float radius)
        {
            if(!record.Applied)return;
            var surfaces=new List<string>();var touches=new List<string>();
            var lines=CourtLines.Build(court);var overlaps=new List<LineId>();
            var judge=new SquashClash.Core.Rally.ReturnLineJudge(court,radius);
            foreach(var e in contacts)
            {
                if(e.Step<=record.ContactStep||e.Persistent)continue;
                if(!Finite(e.Point.x)||!Finite(e.Point.y)||!Finite(e.Point.z))continue;
                var verdict=judge.Judge(e.Surface,e.Point);
                record.OutObserved|=verdict==SquashClash.Core.Rally.LineVerdict.Out;
                record.TinObserved|=verdict==SquashClash.Core.Rally.LineVerdict.Tin;
                LineContactClassifier.Overlapping(lines,e.Surface,court.ToSurfaceUV(e.Surface,e.Point),radius,overlaps);
                foreach(var line in overlaps)touches.Add(e.Step+":"+line);
                if(!record.HasFrontImpact&&e.Surface==SurfaceId.FrontWall)
                {
                    record.HasFrontImpact=true;record.FrontImpact=e.Point;record.FrontFailure=null;
                    record.FrontErrorCm=Vector2.Distance(record.RequestedAim,new Vector2(e.Point.x,e.Point.y))*100;
                }
                if(!record.HasFirstBounce)
                {
                    if(e.Surface==SurfaceId.Floor)
                    {
                        record.HasFirstBounce=true;record.FirstBounce=e.Point+e.Normal*radius;record.BounceFailure=null;
                        record.ActualBounceColumn=FrontColumn(record.FirstBounce.x);record.ActualBounceDepth=BounceDepth(record.FirstBounce.z);
                        record.FirstBounceZoneHit=record.ActualBounceColumn==record.Requested.BounceColumn&&record.ActualBounceDepth==record.Requested.BounceDepth;
                    }
                    else surfaces.Add(e.Surface.ToString());
                }
            }
            record.PriorSurfaces=string.Join(",",surfaces);record.LineTouches=string.Join(",",touches);
        }
        private static bool Finite(float x)=>!float.IsNaN(x)&&!float.IsInfinity(x);
        private static int Band(float value,float min,float first,float second,float max)
            =>!Finite(value)||value<min||value>max?-1:value<first?0:value<second?1:2;
        public static int FrontColumn(float x)=>Band(x,.2f,2.2f,4.2f,6.2f);
        public static int FrontRow(float y)=>Band(y,.6f,1.6f,2.6f,3.6f);
        public static int PowerThird(float p)=>Band(p,0,1f/3,2f/3,1);
        public static int BounceDepth(float z)=>Band(z,.2f,3.2f,6.4f,9.55f);
    }
}
