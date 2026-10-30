using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    public readonly struct ShotRequest
    {
        public readonly Vector3 ContactCenter;
        public readonly Vector2 AimOnFrontWall;
        public readonly float P;
        public ShotRequest(Vector3 center, Vector2 aim, float power) { ContactCenter=center; AimOnFrontWall=aim; P=power; }
    }
    public readonly struct ShotSolution
    {
        public readonly bool Solved, SpeedRaised, TargetLowered;
        public readonly Vector3 Velocity;
        public readonly float Speed;
        public readonly Vector2 TargetUsed;
        public readonly string Reason;
        public ShotSolution(bool solved, Vector3 velocity, float speed, Vector2 target, bool raised, bool lowered, string reason)
        { Solved=solved; Velocity=velocity; Speed=speed; TargetUsed=target; SpeedRaised=raised; TargetLowered=lowered; Reason=reason; }
    }
    public static class ShotSolver
    {
        public static ShotSolution Solve(in ShotRequest r, float vMin, float vMax, float vCap, float gravity, float radius,
            CourtDimensions court, IBallSimulationFactory verifier, PhysicsParams physics)
        {
            var from=r.ContactCenter; var aim=r.AimOnFrontWall;
            ShotSolution Fail(string reason)=>new ShotSolution(false,Vector3.zero,0,aim,false,false,reason);
            if(court==null||physics==null||verifier==null||!Finite(from.x)||!Finite(from.y)||!Finite(from.z)||
                !Finite(aim.x)||!Finite(aim.y)||!Finite(r.P)||r.P<0||r.P>1||!Finite(vMin)||!Finite(vMax)||!Finite(vCap)||
                vMin<=0||vMin>vMax||vMax>vCap||!Finite(gravity)||gravity<=0||radius<=0||gravity!=physics.Gravity||radius!=physics.BallRadius)
                return Fail("invalid-request-or-physics");
            if(from.x<radius||from.x>court.Width-radius||from.y<radius||from.z<radius||from.z>court.Length-radius||
                aim.x<radius||aim.x>court.Width-radius||aim.y<=court.TinTop||aim.y>=court.FrontOutLower)
                return Fail("target-or-contact-outside-court");
            double dx=(double)aim.x-from.x,dz=(double)radius-from.z,d=Math.Sqrt(dx*dx+dz*dz);
            if(d==0) return Fail("zero-horizontal-distance");
            float requested=vMin+r.P*(vMax-vMin);
            double h=(double)aim.y-from.y;
            double required=Math.Sqrt(gravity*(h+Math.Sqrt(d*d+h*h)));
            float speed=requested;
            if(required>requested) speed=Math.Min(vCap,CeilingFloat(required));
            bool Candidate(float y,out Vector3 velocity)
            {
                velocity=Vector3.zero;
                double height=(double)y-from.y,v2=(double)speed*speed;
                double discriminant=v2*v2-gravity*(gravity*d*d+2*height*v2);
                if(discriminant<0) return false;
                // Rationalized low root avoids cancellation near a horizontal/low target.
                double timeSquared=2*(d*d+height*height)/(v2-gravity*height+Math.Sqrt(discriminant));
                if(!(timeSquared>0)||double.IsNaN(timeSquared)||double.IsInfinity(timeSquared))return false;
                double time=Math.Sqrt(timeSquared);
                velocity=new Vector3((float)(dx/time),(float)(height/time+.5*gravity*time),(float)(dz/time));
                return Verify(from,velocity,new Vector2(aim.x,y),time,court,verifier,physics);
            }
            if(Candidate(aim.y,out var output))
                return new ShotSolution(true,output,speed,aim,speed>requested,false,"solved");
            // The lower legal center must clear the tin with the physical ball radius.
            float low=CeilingFloat((double)court.TinTop+radius),high=aim.y;
            if(low>=high||!Candidate(low,out var best))return Fail("no-feasible-low-arc-even-at-lower-target");
            float used=low;
            for(int i=0;i<16;i++)
            {
                float mid=(low+high)*.5f;
                if(Candidate(mid,out var value)){low=mid;used=mid;best=value;}else high=mid;
            }
            return new ShotSolution(true,best,speed,new Vector2(aim.x,used),speed>requested,true,"target-lowered");
        }
        private static bool Verify(Vector3 from,Vector3 velocity,Vector2 target,double time,CourtDimensions court,IBallSimulationFactory factory,PhysicsParams physics)
        {
            using(var simulation=factory.Create(physics,court))
            {
                simulation.Reset(new BallState{Position=from,Velocity=velocity});
                var events=new List<ContactEvent>(4);
                int steps=checked((int)Math.Ceiling(time*physics.StepHz)+2);
                for(int i=0;i<steps;i++)
                {
                    events.Clear();simulation.Step(events);
                    foreach(var hit in events)
                    {
                        if(hit.Persistent)continue;
                        if(hit.Surface!=SurfaceId.FrontWall)return false;
                        return Vector2.Distance(new Vector2(hit.Point.x,hit.Point.y),target)<=.05f;
                    }
                    if(simulation.State.Position.y>court.WallHeight)return false;
                }
            }
            return false;
        }
        private static float CeilingFloat(double value)
        {
            float rounded=(float)value;
            return rounded<value?BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(rounded)+1):rounded;
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    }
}
