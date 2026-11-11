using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    public enum PlayerId { Human=0, Rival=1 }
    public enum PointEndCause { None, DoubleBounce, Tin, Out, NotUp, LaunchFault }
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
        private readonly LineBand tin,frontOut,backOut,leftOut,rightOut;
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
                }
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
            Closed=false;Winner=null;Cause=PointEndCause.None;Striker=striker;ToHit=receiver;FloorBounces=0;FrontWallReached=OutAfterBounce=false;
            Shots=IgnoredAfterClose=RejectedContacts=0;TerminalSurface=default;TerminalPoint=default;
        }
        public bool CanHit(PlayerId who)=>!Closed&&who==ToHit&&FrontWallReached&&(volleyAllowed?FloorBounces<=1:FloorBounces==1);
        public bool RacketContact(PlayerId who)
        {
            if(!CanHit(who)){RejectedContacts++;return false;}
            Striker=who;ToHit=who.Other();FloorBounces=0;FrontWallReached=false;Shots++;return true;
        }
        public void OnBallContact(in ContactEvent e)
        {
            if(Closed){IgnoredAfterClose++;return;}
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
            // [ASSUMPTION] After a good return has bounced once, a ball leaving the court ends the rally against the receiver.
            if(verdict==LineVerdict.Out){OutAfterBounce=true;Close(PointEndCause.Out,ToHit,e);}
        }
        public void OnOverTop()
        {
            if(Closed){IgnoredAfterClose++;return;}
            if(FloorBounces>0)OutAfterBounce=true;
            Close(PointEndCause.Out,FloorBounces==0?Striker:ToHit,default);
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
            copy.RejectedContacts=RejectedContacts;copy.TerminalSurface=TerminalSurface;copy.TerminalPoint=TerminalPoint;return copy;
        }
    }
}
