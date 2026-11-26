using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    // Serve kinds (design 2026-11-20-serve). Auto: today's single automatic serve (the new serve is off, nothing is chosen).
    public enum ServeType { Auto, Lob, Drive }
    // Optional for a brain: which serve it plays from a box when the new serve is on. A brain without it serves the lob.
    public interface IRallyServer { ServeType ChooseServe(ServiceBox box); }
    // Automatic serve (spec 8.7.5: automatic placement in the box, no foot fault). It searches a small fixed grid for a
    // serve that the serve rules accept, so the same box always gives the same serve.
    public static class ServePlanner
    {
        // Serve positions (spec 2026-11-26-posiciones-saque): court geometry, not tuning.
        // The server stands at the front of its box, as close to the inner front corner as the current serve aims allow: from 0.30 m
        // the drive is a fault; ServePositionsSearch chose the nearest position where Drive, Lob and Auto bounce at least 0.3 m inside
        // the required quarter in both boxes. The serve leaves from here.
        public const float ServerFromInnerLine=.7f,ServerBehindShortLine=.3f,ReceiverFromSide=1.6f,ReceiverFromBack=2.1f;
        public static Vector2 BoxPosition(RallyContext context,ServiceBox box)
        {
            var court=context.Court;float x=court.ServiceBoxInner-ServerFromInnerLine;
            return new Vector2(box==ServiceBox.Right?x:court.Width-x,court.Length-court.ShortLineNearBackEdgeFromBack+ServerBehindShortLine);
        }
        // The receiver stands in the back quarter opposite the server's box: the real start of its simulated body.
        public static Vector2 ReceiverPosition(RallyContext context,ServiceBox box)
        {
            var court=context.Court;
            return new Vector2(box==ServiceBox.Right?court.Width-ReceiverFromSide:ReceiverFromSide,court.Length-ReceiverFromBack);
        }
        public static BallState Find(RallyContext context,ServiceBox box)=>Find(context,box,BoxPosition(context,box));
        // The same search from any server position (the serve positions search measures candidate corners with it).
        public static BallState Find(RallyContext context,ServiceBox box,Vector2 at)
        {
            var p=context.Params;var court=context.Court;
            var from=new Vector3(at.x,(float)p.ServeHandHeight,at.y);
            float landX=box==ServiceBox.Right?court.Width-(float)p.ServeLandingFromWall:(float)p.ServeLandingFromWall,depth=(float)p.ServeLandingDepth;
            bool found=false;float best=float.PositiveInfinity;BallState chosen=default;
            foreach(float landZ in new[]{depth,depth-.6f,depth+.6f,depth-1.2f})
                foreach(float height in new[]{2.4f,2.8f,3.2f,2.1f,3.6f})
                    foreach(float speed in new[]{11f,12,13,10,14,15,9})
                    {
                        // Mirror the landing spot across the front wall to get where the serve must strike it.
                        float aimX=from.x+(landX-from.x)*from.z/(from.z+landZ);
                        var center=Ballistics.CenterForContact(SurfaceId.FrontWall,new Vector3(aimX,height,0),context.Physics.BallRadius);
                        var serve=new BallState{Position=from,Velocity=Ballistics.SolveCenterTo(from,center,speed,context.Physics.Gravity)};
                        if(!Forecast(context,serve,box,out var bounce))continue;
                        float score=Mathf.Abs(bounce.z-depth)+Mathf.Abs(bounce.x-landX);
                        if(score<best){best=score;chosen=serve;found=true;}
                    }
            if(!found)throw new InvalidOperationException("No valid automatic serve for this court and physics.");
            return chosen;
        }
        // The fixed serve of a kind from a box (RallyParams ServeKind*), checked against the serve rules like Find.
        public static BallState Typed(RallyContext context,ServiceBox box,ServeType type)=>Typed(context,box,type,BoxPosition(context,box));
        public static BallState Typed(RallyContext context,ServiceBox box,ServeType type,Vector2 at)
        {
            if(type!=ServeType.Lob&&type!=ServeType.Drive)throw new ArgumentOutOfRangeException(nameof(type));
            var p=context.Params;var from=new Vector3(at.x,(float)p.ServeHandHeight,at.y);
            bool right=box==ServiceBox.Right,lob=type==ServeType.Lob;
            double x=lob?(right?p.ServeKindLobRightAimX:p.ServeKindLobLeftAimX):(right?p.ServeKindDriveRightAimX:p.ServeKindDriveLeftAimX);
            double y=lob?(right?p.ServeKindLobRightAimY:p.ServeKindLobLeftAimY):(right?p.ServeKindDriveRightAimY:p.ServeKindDriveLeftAimY);
            double speed=lob?(right?p.ServeKindLobRightSpeed:p.ServeKindLobLeftSpeed):(right?p.ServeKindDriveRightSpeed:p.ServeKindDriveLeftSpeed);
            var center=Ballistics.CenterForContact(SurfaceId.FrontWall,new Vector3((float)x,(float)y,0),context.Physics.BallRadius);
            var serve=new BallState{Position=from,Velocity=Ballistics.SolveCenterTo(from,center,(float)speed,context.Physics.Gravity)};
            if(!Forecast(context,serve,box,out _))throw new InvalidOperationException("The "+type+" serve from the "+box+" box is not valid for this court and physics.");
            return serve;
        }
        // Replays the serve rules over an isolated prediction until the first bounce is judged.
        public static bool Forecast(RallyContext context,BallState serve,ServiceBox box,out Vector3 firstBounce)
        {
            firstBounce=default;
            var rules=new PointRules(context.Court,(float)context.Params.FootprintRadius);rules.BeginServe(PlayerId.Human,box);
            using(var simulation=context.Factory.Create(context.Physics,context.Court))
            {
                simulation.Reset(serve);var events=new List<ContactEvent>(4);
                for(int step=0;step<context.StepHz*8;step++)
                {
                    events.Clear();simulation.Step(events);
                    foreach(var e in events)
                    {
                        rules.OnBallContact(e);
                        if(rules.Closed)return false;
                        if(!rules.Serving){firstBounce=e.Point;return true;}
                    }
                    if(simulation.State.Position.y>context.Court.WallHeight)return false;
                }
            }
            return false;
        }
    }
}
