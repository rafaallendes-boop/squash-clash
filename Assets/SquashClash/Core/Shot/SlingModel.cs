using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Config;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    public enum SlingPhase { Aiming, Locked, Pulling, CancelZone }
    public enum SlingCancelRegion { None, Above, Left, Right }
    public readonly struct SlingTransition
    {
        public readonly string Kind; public readonly double TimeReal;
        public SlingTransition(string kind,double time){Kind=kind;TimeReal=time;}
    }
    public readonly struct SlingSettings
    {
        public readonly float Gain,StartHeight,HoldMs,MaxSpeed,Radius,MaxGapMs,DeadMm,FullMm,AboveMm,LateralMm,MinDepthMm,TapMm,TapMs;
        public readonly int MinSamples;
        public readonly bool PreferLeft;
        public SlingSettings(float holdMs=120,float speed=15,float radius=1,float gap=51,int samples=3,
            float full=35,float dead=2,float above=12,float lateral=20,float depth=8,bool left=true,
            float gain=.12f,float height=2.2f,float tapMm=3,float tapMs=250)
        {HoldMs=holdMs;MaxSpeed=speed;Radius=radius;MaxGapMs=gap;MinSamples=samples;FullMm=full;DeadMm=dead;AboveMm=above;LateralMm=lateral;MinDepthMm=depth;PreferLeft=left;Gain=gain;StartHeight=height;TapMm=tapMm;TapMs=tapMs;}
        public static SlingSettings From(ParamSet p)=>new SlingSettings(
            (float)p.Get("aim.holdMs"),(float)p.Get("aim.holdMaxSpeedMmPerS"),(float)p.Get("aim.holdRadiusMm"),
            (float)p.Get("aim.holdMaxGapMs"),p.GetInt("aim.holdMinSamples"),(float)p.Get("power.pullFullMm"),
            (float)p.Get("power.pullDeadZoneMm"),(float)p.Get("gesture.slingCancelAboveMm"),(float)p.Get("gesture.slingCancelLateralMm"),
            (float)p.Get("gesture.slingCancelMinDepthMm"),p.GetBool("gesture.slingPreferLeft"),(float)p.Get("aim.gainMPerMm"),
            (float)p.Get("aim.startHeight"),(float)p.Get("gesture.tapMaxMm"),(float)p.Get("gesture.tapMaxMs"));

    }
    // The sling consumes the existing raw stream; it owns no clock, ball or input source.
    public sealed class SlingModel
    {
        private readonly SlingSettings settings;
        private readonly Rect area;
        private readonly float scale,width,initialX;
        private readonly List<SlingTransition> transitions=new List<SlingTransition>(16);
        private Vector2 origin,previous,quietAnchor;
        private double previousTime,quietAt;
        private int finger,quietSamples,pullSamples,sampleCount;
        private float maxDisplacement;
        private bool pulled,invalid;
        public SlingModel(SlingSettings settings,CourtDimensions court,float initialX,Rect area,float mmPerPx)
        {
            if(court==null||!Finite(mmPerPx)||mmPerPx<=0||settings.HoldMs<=0||settings.MaxGapMs<=0||
                settings.MinSamples<3||settings.FullMm<=settings.DeadMm||settings.DeadMm<0||
                settings.Radius<0||settings.MaxSpeed<0||settings.AboveMm<=0||settings.LateralMm<=0||settings.MinDepthMm<=0)
                throw new ArgumentOutOfRangeException(nameof(settings));
            this.settings=settings;this.area=area;scale=mmPerPx;width=court.Width;this.initialX=initialX;
            Aim=new Vector2(initialX,settings.StartHeight);
        }
        public bool Active {get;private set;}
        public bool Locked {get;private set;}
        public bool LockBlocked {get;private set;}
        public bool Conservative=>invalid;
        public string Reason {get;private set;}="aiming";
        public SlingPhase Phase {get;private set;}
        public SlingCancelRegion Region {get;private set;}
        public Vector2 Aim {get;private set;}
        public Vector2 LockPositionPx {get;private set;}
        public float P {get;private set;}
        public float DistanceMm {get;private set;}
        public float MaxDistanceMm {get;private set;}
        public float LateralMm {get;private set;}
        public double BeganAt {get;private set;}
        public double LockAt {get;private set;}=double.NaN;
        public double LastOutsideAt {get;private set;}=double.NaN;
        public double ReleaseAt {get;private set;}=double.NaN;
        public IReadOnlyList<SlingTransition> Transitions=>transitions;
        public GestureOutcome Outcome {get;private set;}
        private static bool Finite(double x)=>!double.IsNaN(x)&&!double.IsInfinity(x);
        private void Event(string kind,double t)=>transitions.Add(new SlingTransition(kind,t));
        public void Push(in TouchSample s)
        {
            if(s.Phase==TouchPhaseKind.Began)
            {
                if(Active)return;
                Active=true;Locked=LockBlocked=pulled=invalid=false;Outcome=GestureOutcome.None;Region=SlingCancelRegion.None;
                Phase=SlingPhase.Aiming;finger=s.FingerId;origin=previous=quietAnchor=s.PositionPx;
                BeganAt=previousTime=quietAt=s.TimeReal;LockAt=LastOutsideAt=ReleaseAt=double.NaN;
                quietSamples=1;pullSamples=sampleCount=0;maxDisplacement=P=DistanceMm=MaxDistanceMm=LateralMm=0;
                Aim=new Vector2(Mathf.Clamp(initialX,0,width),Mathf.Clamp(settings.StartHeight,0,5));
                Reason="aiming";transitions.Clear();Event("Began",s.TimeReal);
                if(!Valid(s)){invalid=true;Reason="invalid-input";}
                return;
            }
            if(!Active||s.FingerId!=finger)return;
            if(++sampleCount>32768){Reason="sample-limit";Cancel(s.TimeReal);return;}
            if(s.Phase==TouchPhaseKind.Canceled){Cancel(s.TimeReal);return;}
            double dt=s.TimeReal-previousTime;
            bool same=dt==0&&s.PositionPx==previous;
            if(!Valid(s)||dt<0||dt==0&&!(same&&(s.Phase==TouchPhaseKind.Ended||s.Phase==TouchPhaseKind.Stationary)))
            {invalid=true;P=0;Reason=!Valid(s)?"invalid-input":dt<0?"out-of-order":"repeated-time";}
            if(invalid)
            {
                if(s.Phase==TouchPhaseKind.Ended){ReleaseAt=s.TimeReal;Event("Ended",s.TimeReal);Active=false;Outcome=GestureOutcome.SystemCanceled;}
                return;
            }
            if(dt==0)
            {
                if(s.Phase==TouchPhaseKind.Ended)Finish(s);
                return; // No new velocity, support, duration or position.
            }
            maxDisplacement=Mathf.Max(maxDisplacement,Vector2.Distance(origin,s.PositionPx)*scale);
            if(!Locked)
            {
                var delta=(s.PositionPx-origin)*scale*settings.Gain;
                Aim=new Vector2(Mathf.Clamp(initialX-delta.x,0,width),Mathf.Clamp(settings.StartHeight+delta.y,0,5));
                double speed=Vector2.Distance(previous,s.PositionPx)*scale/dt;
                bool outside=speed>settings.MaxSpeed+1e-6||Vector2.Distance(quietAnchor,s.PositionPx)*scale>settings.Radius+1e-6;
                if(outside)LastOutsideAt=s.TimeReal;
                if(outside||dt*1000>settings.MaxGapMs+1e-6||maxDisplacement<settings.TapMm)
                {quietAt=s.TimeReal;quietAnchor=s.PositionPx;quietSamples=1;}
                else quietSamples++;
                if(s.Phase!=TouchPhaseKind.Ended&&maxDisplacement>=settings.TapMm&&quietSamples>=settings.MinSamples&&
                    (s.TimeReal-quietAt)*1000+1e-6>=settings.HoldMs)
                {
                    Region=ChooseRegion(s.PositionPx);LockBlocked=Region==SlingCancelRegion.None;
                    if(!LockBlocked)
                    {Locked=true;LockAt=s.TimeReal;LockPositionPx=s.PositionPx;pullSamples=1;Phase=SlingPhase.Locked;Reason="distance";Event("Locked",s.TimeReal);}
                }
            }
            else
            {
                if(s.PositionPx!=previous)pullSamples++;
                DistanceMm=Mathf.Max(0,(LockPositionPx.y-s.PositionPx.y)*scale);
                MaxDistanceMm=Mathf.Max(MaxDistanceMm,DistanceMm);
                LateralMm=(s.PositionPx.x-LockPositionPx.x)*scale;
                P=pullSamples<2?0:Mathf.Clamp01((DistanceMm-settings.DeadMm)/(settings.FullMm-settings.DeadMm));
                if(!pulled&&DistanceMm>settings.DeadMm&&pullSamples>=2){pulled=true;Event("PullStarted",s.TimeReal);}
                bool inside=InsideCancel(s.PositionPx);
                if(inside&&Phase!=SlingPhase.CancelZone)Event("CancelEntered",s.TimeReal);
                if(!inside&&Phase==SlingPhase.CancelZone)Event("CancelExited",s.TimeReal);
                Phase=inside?SlingPhase.CancelZone:pulled?SlingPhase.Pulling:SlingPhase.Locked;
            }
            previous=s.PositionPx;previousTime=s.TimeReal;
            if(s.Phase==TouchPhaseKind.Ended)Finish(s);
        }
        private bool Valid(in TouchSample s)=>Finite(s.TimeReal)&&Finite(s.PositionPx.x)&&Finite(s.PositionPx.y);
        private SlingCancelRegion ChooseRegion(Vector2 p)
        {
            if(!area.Contains(p))return SlingCancelRegion.None;
            float depth=settings.MinDepthMm/scale;
            if(area.yMax-p.y-settings.AboveMm/scale+1e-5>=depth)return SlingCancelRegion.Above;
            bool left=p.x-area.xMin-settings.LateralMm/scale+1e-5>=depth;
            bool right=area.xMax-p.x-settings.LateralMm/scale+1e-5>=depth;
            if(settings.PreferLeft&&left)return SlingCancelRegion.Left;
            if(!settings.PreferLeft&&right)return SlingCancelRegion.Right;
            return left?SlingCancelRegion.Left:right?SlingCancelRegion.Right:SlingCancelRegion.None;
        }
        public bool InsideCancel(Vector2 p)
        {
            if(!Locked||!area.Contains(p))return false;
            switch(Region)
            {
                case SlingCancelRegion.Above:return (p.y-LockPositionPx.y)*scale+1e-5>=settings.AboveMm;
                case SlingCancelRegion.Left:return (LockPositionPx.x-p.x)*scale+1e-5>=settings.LateralMm;
                case SlingCancelRegion.Right:return (p.x-LockPositionPx.x)*scale+1e-5>=settings.LateralMm;
                default:return false;
            }
        }
        private void Finish(in TouchSample s)
        {
            ReleaseAt=s.TimeReal;Active=false;Event("Ended",s.TimeReal);
            Outcome=maxDisplacement<settings.TapMm&&(s.TimeReal-BeganAt)*1000<settings.TapMs
                ?GestureOutcome.Tap:!Locked?GestureOutcome.AbandonedBeforeFix:InsideCancel(s.PositionPx)
                ?GestureOutcome.ZoneCanceled:GestureOutcome.Swing;
        }
        public void Cancel(double now)
        {if(!Active)return;Active=false;Outcome=GestureOutcome.SystemCanceled;Event("SystemCanceled",now);}
    }
}
