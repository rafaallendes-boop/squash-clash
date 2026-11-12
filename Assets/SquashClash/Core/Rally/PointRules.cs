using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    public enum PlayerId { Human=0, Rival=1 }
    public enum PointEndCause { None, DoubleBounce, Tin, Out, NotUp, LaunchFault, ServiceFault }
    public enum LineVerdict { Good, Tin, Out }
    public static class PlayerIds
    {
        public static PlayerId Other(this PlayerId who)=>who==PlayerId.Human?PlayerId.Rival:PlayerId.Human;
    }
    // Return-phase line consequences (spec 8.7.4, rally column), judged by contact footprint.
    // The service line and every floor line are not judged in the return.
    public sealed class ReturnLineJudge
    {
        private readonly CourtDimensions court;
        private readonly float radius;
        private readonly LineBand tin,frontOut,backOut,leftOut,rightOut,service,shortLine,halfCourt;
        public ReturnLineJudge(CourtDimensions court,float footprintRadius)
        {
            this.court=court??throw new ArgumentNullException(nameof(court));
            if(float.IsNaN(footprintRadius)||float.IsInfinity(footprintRadius)||footprintRadius<0)throw new ArgumentOutOfRangeException(nameof(footprintRadius));
            radius=footprintRadius;
            foreach(var band in CourtLines.Build(court))
                switch(band.Id)
                {
                    case LineId.Tin:tin=band;break;
                    case LineId.FrontOut:frontOut=band;break;
                    case LineId.BackOut:backOut=band;break;
                    case LineId.LeftSideOut:leftOut=band;break;
                    case LineId.RightSideOut:rightOut=band;break;
                    case LineId.Service:service=band;break;
                    case LineId.ShortLine:shortLine=band;break;
                    case LineId.HalfCourt:halfCourt=band;break;
                }
        }
        // Serve column of spec 8.7.4: above the service line (touching it is a fault), clear of tin and out.
        public bool ServeFrontWallGood(Vector3 point)
        {
            var uv=court.ToSurfaceUV(SurfaceId.FrontWall,point);
            return Judge(SurfaceId.FrontWall,point)==LineVerdict.Good&&uv.y>court.ServiceLower+court.LineWidth&&!LineContactClassifier.Touches(in service,uv,radius);
        }
        // First bounce of a serve: the back quarter opposite to the server's box, clear of the short and half-court lines.
        public bool ServeLandsInQuarter(Vector3 point,ServiceBox from)
        {
            var uv=court.ToSurfaceUV(SurfaceId.Floor,point);
            if(uv.y<=court.Length-court.ShortLineNearBackEdgeFromBack||LineContactClassifier.Touches(in shortLine,uv,radius)||LineContactClassifier.Touches(in halfCourt,uv,radius))return false;
            // The right box is beside the right wall (x = 0); its serve lands on the other half.
            return from==ServiceBox.Right?uv.x>court.Width*.5f:uv.x<court.Width*.5f;
        }
        public LineVerdict Judge(SurfaceId surface,Vector3 point)
        {
            var uv=court.ToSurfaceUV(surface,point);
            switch(surface)
            {
                case SurfaceId.FrontWall:
                    // The tin spans the floor up to and including its top line (WSF 5.02 as recorded in spec 8.7.2).
                    if(uv.y<=court.TinTop||LineContactClassifier.Touches(in tin,uv,radius))return LineVerdict.Tin;
                    return uv.y>=court.FrontOutLower||LineContactClassifier.Touches(in frontOut,uv,radius)?LineVerdict.Out:LineVerdict.Good;
                case SurfaceId.BackWall:
                    return uv.y>=court.BackOutLower||LineContactClassifier.Touches(in backOut,uv,radius)?LineVerdict.Out:LineVerdict.Good;
                case SurfaceId.LeftWall:
                case SurfaceId.RightWall:
                    var band=surface==SurfaceId.LeftWall?leftOut:rightOut;
                    float edge=Mathf.Lerp(court.FrontOutLower,court.BackOutLower,uv.x/court.Length);
                    return uv.y>=edge||LineContactClassifier.Touches(in band,uv,radius)?LineVerdict.Out:LineVerdict.Good;
                default:return LineVerdict.Good;
            }
        }
    }
    // Pure point state for one rally: terminal events, single closure, bounce count and turn (spec 8.5).
    // It never moves the ball; it only reads contacts reported by the single ball authority.
    public sealed class PointRules
    {
        private readonly CourtDimensions court;
        private readonly float radius;
        private readonly bool volleyAllowed;
        private readonly ReturnLineJudge judge;
        public bool Closed {get;private set;}
        public PlayerId? Winner {get;private set;}
        public PointEndCause Cause {get;private set;}
        // Null while the ball in flight comes from the neutral launch.
        public PlayerId? Striker {get;private set;}
        public PlayerId ToHit {get;private set;}
        public int FloorBounces {get;private set;}
        public bool FrontWallReached {get;private set;}
        public bool OutAfterBounce {get;private set;}
        public int Shots {get;private set;}
        public int IgnoredAfterClose {get;private set;}
        public int RejectedContacts {get;private set;}
        public SurfaceId TerminalSurface {get;private set;}
        public Vector3 TerminalPoint {get;private set;}
        public PointRules(CourtDimensions court,float footprintRadius,bool volleyAllowed=false)
        {
            this.court=court??throw new ArgumentNullException(nameof(court));radius=footprintRadius;this.volleyAllowed=volleyAllowed;
            judge=new ReturnLineJudge(court,footprintRadius);
        }
        public void Begin(PlayerId? striker,PlayerId receiver)
        {
            if(striker.HasValue&&striker.Value==receiver)throw new ArgumentException("The receiver cannot be the striker.",nameof(receiver));
            Closed=false;Winner=null;Cause=PointEndCause.None;Striker=striker;ToHit=receiver;FloorBounces=0;FrontWallReached=OutAfterBounce=false;Serving=false;
            Shots=IgnoredAfterClose=RejectedContacts=0;TerminalSurface=default;TerminalPoint=default;
        }
        // True from the serve until its first bounce is judged (or the receiver strikes it).
        public bool Serving {get;private set;}
        public ServiceBox ServeBox {get;private set;}
        // One attempt: any fault gives the point to the receiver (spec 8.5).
        public void BeginServe(PlayerId server,ServiceBox from){Begin(server,server.Other());Serving=true;ServeBox=from;}
        private void OnServeContact(in ContactEvent e)
        {
            if(e.Surface==SurfaceId.Floor)
            {
                if(!FrontWallReached||e.Persistent||!judge.ServeLandsInQuarter(e.Point,ServeBox)){Close(PointEndCause.ServiceFault,Striker,e);return;}
                FloorBounces=1;Serving=false;return;
            }
            if(e.Persistent)return;
            if(!FrontWallReached)
            {
                // Straight to the front wall: any other wall first is a fault whatever the line.
                if(e.Surface!=SurfaceId.FrontWall||!judge.ServeFrontWallGood(e.Point)){Close(PointEndCause.ServiceFault,Striker,e);return;}
                FrontWallReached=true;return;
            }
            if(judge.Judge(e.Surface,e.Point)!=LineVerdict.Good)Close(PointEndCause.ServiceFault,Striker,e);
        }
        public bool CanHit(PlayerId who)=>!Closed&&who==ToHit&&FrontWallReached&&(volleyAllowed?FloorBounces<=1:FloorBounces==1);
        public bool RacketContact(PlayerId who)
        {
            if(!CanHit(who)){RejectedContacts++;return false;}
            Striker=who;ToHit=who.Other();FloorBounces=0;FrontWallReached=false;Serving=false;Shots++;return true;
        }
        public void OnBallContact(in ContactEvent e)
        {
            if(Closed){IgnoredAfterClose++;return;}
            if(Serving){OnServeContact(e);return;}
            if(e.Surface==SurfaceId.Floor)
            {
                if(!FrontWallReached){Close(PointEndCause.NotUp,Striker,e);return;}
                // A ball that settles on the floor emits no further bounces: it is dead (documented deviation).
                if(e.Persistent){if(FloorBounces>=1)Close(PointEndCause.DoubleBounce,ToHit,e);return;}
                if(++FloorBounces>=2)Close(PointEndCause.DoubleBounce,ToHit,e);
                return;
            }
            if(e.Persistent)return;
            var verdict=judge.Judge(e.Surface,e.Point);
            if(FloorBounces==0)
            {
                if(verdict==LineVerdict.Tin)Close(PointEndCause.Tin,Striker,e);
                else if(verdict==LineVerdict.Out)Close(PointEndCause.Out,Striker,e);
                else if(e.Surface==SurfaceId.FrontWall)FrontWallReached=true;
                return;
            }
            // WSF Rules of Singles Squash, rule 6.2.4 and the definition of OUT: a return that goes out is not good,
            // bounced or not. The striker loses (spec 8.5, 8.7.4).
            if(verdict==LineVerdict.Out){OutAfterBounce=true;Close(PointEndCause.Out,Striker,e);}
        }
        public void OnOverTop()
        {
            if(Closed){IgnoredAfterClose++;return;}
            if(Serving){Close(PointEndCause.ServiceFault,Striker,default);return;}
            if(FloorBounces>0)OutAfterBounce=true;
            Close(PointEndCause.Out,Striker,default);
        }
        private void Close(PointEndCause cause,PlayerId? loser,in ContactEvent e)
        {
            Closed=true;TerminalSurface=e.Surface;TerminalPoint=e.Point;
            // The neutral launch belongs to nobody: its own fault replays the point.
            if(!loser.HasValue){Cause=PointEndCause.LaunchFault;Winner=null;return;}
            Cause=cause;Winner=loser.Value.Other();
        }
        public PointRules Clone()
        {
            var copy=new PointRules(court,radius,volleyAllowed);
            copy.Closed=Closed;copy.Winner=Winner;copy.Cause=Cause;copy.Striker=Striker;copy.ToHit=ToHit;copy.FloorBounces=FloorBounces;
            copy.FrontWallReached=FrontWallReached;copy.OutAfterBounce=OutAfterBounce;copy.Shots=Shots;copy.IgnoredAfterClose=IgnoredAfterClose;
            copy.RejectedContacts=RejectedContacts;copy.Serving=Serving;copy.ServeBox=ServeBox;copy.TerminalSurface=TerminalSurface;copy.TerminalPoint=TerminalPoint;return copy;
        }
    }
}
