using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using UnityEngine;
namespace SquashClash.Core.Simulation
{
    public sealed class AnalyticBallSimulation : IBallSimulation
    {
        private readonly PhysicsParams physics;
        private readonly CourtDimensions court;
        private readonly float bounceThreshold;
        private readonly Action released, capReached;
        private BallState state;
        private bool disposed;
        public BackendCapabilities Capabilities => new BackendCapabilities { Timing = ContactTiming.Exact, ImpactCapConfigurable = true, RunsInEditMode = true };
        public BallState State { get { CheckAlive(); return state; } }
        public long ImpactCapReachedCount { get; private set; }
        internal AnalyticBallSimulation(PhysicsParams p, CourtDimensions d, float threshold, Action released, Action capReached)
        {
            physics=new PhysicsParams {Gravity=p.Gravity,BallRadius=p.BallRadius,BallMass=p.BallMass,StepHz=p.StepHz,MaxImpactsPerStep=p.MaxImpactsPerStep,
                Floor=Copy(p.Floor),FrontWall=Copy(p.FrontWall),SideWalls=Copy(p.SideWalls),BackWall=Copy(p.BackWall)};
            court=new CourtDimensions {Width=d.Width,Length=d.Length,WallHeight=d.WallHeight,LineWidth=d.LineWidth,TinTop=d.TinTop,
                FrontOutLower=d.FrontOutLower,ServiceLower=d.ServiceLower,BackOutLower=d.BackOutLower,ShortLineNearBackEdgeFromBack=d.ShortLineNearBackEdgeFromBack,ServiceBoxInner=d.ServiceBoxInner};
            bounceThreshold=threshold;this.released=released;this.capReached=capReached;
        }
        private static SurfaceMaterial Copy(SurfaceMaterial m)=>new SurfaceMaterial {Restitution=m.Restitution,TangentialFriction=m.TangentialFriction};
        public void Reset(in BallState initial) { CheckAlive(); state=initial; ImpactCapReachedCount=0; }
        public void Step(List<ContactEvent> contacts)
        {
            CheckAlive(); if(contacts==null)throw new ArgumentNullException(nameof(contacts)); contacts.Clear();
            state.Step++; double remaining=physics.StepSeconds,elapsed=0;int impacts=0;
            bool persistentFloor=Supported(state);
            if(persistentFloor)AddContact(contacts,SurfaceId.Floor,0,state.Velocity,true);
            while(remaining>0)
            {
                double time=FindHits(remaining);
                if(double.IsPositiveInfinity(time)) { Advance(remaining);break; }
                Advance(time);elapsed+=time;remaining-=time;
                var incoming=state.Velocity;var outgoing=incoming;var tangent=incoming;double frictionImpulse=0;
                for(int i=0;i<5;i++) if(hitTimes[i]<=time+SimultaneousEpsilon)
                {
                    var surface=(SurfaceId)i;var normal=CourtDimensions.InwardNormal(surface);
                    Snap(surface);float vn=Vector3.Dot(incoming,normal);
                    float reflected=-vn<bounceThreshold?0:-Material(surface).Restitution*vn;
                    outgoing+=(reflected-vn)*normal;
                    tangent-=vn*normal;
                    frictionImpulse+=(double)Material(surface).TangentialFriction*(1+Material(surface).Restitution)*-vn;
                }
                // Simultaneous normals are resolved jointly. Friction acts in their common tangent space.
                double tangentSpeed=Math.Sqrt((double)tangent.x*tangent.x+(double)tangent.y*tangent.y+(double)tangent.z*tangent.z);
                if(tangentSpeed>0)outgoing-=tangent*(float)Math.Min(1,frictionImpulse/tangentSpeed);
                for(int i=0;i<5;i++) if(hitTimes[i]<=time+SimultaneousEpsilon)
                {
                    AddContact(contacts,(SurfaceId)i,elapsed,incoming,false);
                }
                state.Velocity=outgoing;
                if(!persistentFloor&&Supported(state)) {AddContact(contacts,SurfaceId.Floor,elapsed,outgoing,true);persistentFloor=true;}
                // A simultaneous set is one atomic collision instant. Never integrate unchecked after the cap.
                if(++impacts>=physics.MaxImpactsPerStep) {ImpactCapReachedCount++;capReached();break;}
            }
        }
        private const double SimultaneousEpsilon=1e-6;
        private readonly double[] hitTimes=new double[5];
        private double FindHits(double limit)
        {
            for(int i=0;i<5;i++)hitTimes[i]=double.PositiveInfinity;
            var p=state.Position;var v=state.Velocity;double h=(double)p.y-physics.BallRadius;
            if(!Supported(state)&&physics.Gravity>0)
            {
                double disc=(double)v.y*v.y+2*physics.Gravity*h;
                if(disc>=0)Consider(SurfaceId.Floor,(v.y+Math.Sqrt(disc))/physics.Gravity,limit);
            }
            else if(!Supported(state)&&v.y<0)Consider(SurfaceId.Floor,-h/v.y,limit);
            if(v.x<0)Consider(SurfaceId.RightWall,WallTime((double)physics.BallRadius-p.x,v.x),limit);
            if(v.x>0)Consider(SurfaceId.LeftWall,WallTime((double)(court.Width-physics.BallRadius)-p.x,v.x),limit);
            if(v.z<0)Consider(SurfaceId.FrontWall,WallTime((double)physics.BallRadius-p.z,v.z),limit);
            if(v.z>0)Consider(SurfaceId.BackWall,WallTime((double)(court.Length-physics.BallRadius)-p.z,v.z),limit);
            double best=double.PositiveInfinity;for(int i=0;i<5;i++)best=Math.Min(best,hitTimes[i]);return best;
        }
        private void Consider(SurfaceId surface,double time,double limit)
        {
            if(double.IsNaN(time)||time<0||time>limit)return;
            var hit=Integrated(state,time);var p=hit.Position;var v=hit.Velocity;
            if(Vector3.Dot(v,CourtDimensions.InwardNormal(surface))>=0)return;
            // The approved planar simplification clips walls by center height only; the floor is unbounded.
            bool finite=surface==SurfaceId.Floor||(p.y>=0&&p.y<=court.WallHeight);
            if(finite)hitTimes[(int)surface]=time;
        }
        private void Snap(SurfaceId surface)
        {
            var p=state.Position;
            switch(surface) {
                case SurfaceId.Floor:p.y=physics.BallRadius;break;
                case SurfaceId.FrontWall:p.z=physics.BallRadius;break;
                case SurfaceId.BackWall:p.z=court.Length-physics.BallRadius;break;
                case SurfaceId.RightWall:p.x=physics.BallRadius;break;
                case SurfaceId.LeftWall:p.x=court.Width-physics.BallRadius;break;
            }
            state.Position=p;
        }
        private bool Supported(in BallState s) => s.Position.y==physics.BallRadius&&s.Velocity.y==0;
        private double WallTime(double delta,double component)
        {
            if(!Supported(state))return delta/component;
            double speed=Math.Sqrt((double)state.Velocity.x*state.Velocity.x+(double)state.Velocity.z*state.Velocity.z);
            double distance=delta*speed/component, a=(double)physics.Floor.TangentialFriction*physics.Gravity;
            if(distance<0)return double.PositiveInfinity;
            if(a==0)return delta/component;
            double disc=speed*speed-2*a*distance;
            return disc<0?double.PositiveInfinity:2*distance/(speed+Math.Sqrt(disc));
        }
        private BallState Integrated(BallState from,double time)
        {
            if(!Supported(from))return new BallState {Position=Ballistics.PositionAt(from,physics.Gravity,time),Velocity=Ballistics.VelocityAt(from,physics.Gravity,time),Step=from.Step};
            double vx=from.Velocity.x,vz=from.Velocity.z,speed=Math.Sqrt(vx*vx+vz*vz);
            if(speed==0)return from;
            double a=(double)physics.Floor.TangentialFriction*physics.Gravity;
            double duration=a>0?Math.Min(time,speed/a):time;
            double distance=speed*duration-.5*a*duration*duration;
            double after=Math.Max(0,speed-a*time);
            from.Position=new Vector3((float)(from.Position.x+vx/speed*distance),physics.BallRadius,(float)(from.Position.z+vz/speed*distance));
            from.Velocity=new Vector3((float)(vx/speed*after),0,(float)(vz/speed*after));return from;
        }
        private void Advance(double time) => state=Integrated(state,time);
        private void AddContact(List<ContactEvent> contacts,SurfaceId surface,double time,Vector3 incoming,bool persistent)
        {
            var normal=CourtDimensions.InwardNormal(surface);var point=state.Position-normal*physics.BallRadius;
            switch(surface) {
                case SurfaceId.Floor:point.y=0;break;case SurfaceId.FrontWall:point.z=0;break;
                case SurfaceId.BackWall:point.z=court.Length;break;case SurfaceId.RightWall:point.x=0;break;
                case SurfaceId.LeftWall:point.x=court.Width;break;
            }
            contacts.Add(new ContactEvent(surface,point,court.ToSurfaceUV(surface,point),normal,state.Step,(float)time,ContactTiming.Exact,incoming,persistent));
        }
        private SurfaceMaterial Material(SurfaceId surface) => surface==SurfaceId.Floor?physics.Floor:
            surface==SurfaceId.FrontWall?physics.FrontWall:surface==SurfaceId.BackWall?physics.BackWall:physics.SideWalls;
        public void Dispose() { if(disposed)return; disposed=true; released(); }
        private void CheckAlive() { if(disposed)throw new ObjectDisposedException(nameof(AnalyticBallSimulation)); }
    }
}
