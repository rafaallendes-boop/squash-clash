using System;
using System.Collections.Generic;
using System.Linq;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.AutoQA
{
    // Numerical witness from physical samples, independent of production contact decisions and roots.
    [Serializable] internal sealed class PhysicsWindow
    {
        public double kInMs,kOutMs,sampleMs;
        internal static PhysicsWindow Measure(Trajectory incoming,ParamSet config,double targetStep)
        {
            if(incoming==null||incoming.States.Count<2)return null;
            var cfg=EffectiveConfig.From(config);var body=BodyFrame.From(config);var court=cfg.Court;
            int hz=cfg.Physics.StepHz;float radius=cfg.Physics.BallRadius;
            double realMs=1000d/hz/cfg.Tempo;
            var collisions=incoming.Contacts.OrderBy(e=>e.Step-1+(double)e.TimeInStep*hz).ToArray();int next=0,bounces=0;
            bool front=false,invalid=false,active=false;double start=0,last=0;
            var ranges=new List<(double start,double end)>();
            void Samples(double a,double b,Vector3 from,Vector3 to)
            {
                if(b<=a)return;
                for(int j=0;j<64;j++)
                {
                    float fraction=(j+.5f)/64;double at=a+(b-a)*fraction;var point=Vector3.LerpUnclamped(from,to,fraction);
                    double dx=(double)point.x-body.PositionXZ.x,dy=(double)point.y-body.ReferenceHeight,dz=(double)point.z-body.PositionXZ.y;
                    bool legal=front&&bounces==1&&!invalid&&point.y>=config.Get("legal.minHeight")&&point.y<=config.Get("legal.maxHeight")&&dx*dx+dy*dy+dz*dz<=(double)body.Reach*body.Reach;
                    if(legal){if(!active){start=at;active=true;}last=at;}
                    else if(active){ranges.Add((start,last));active=false;}
                }
            }
            foreach(var pair in incoming.States.Zip(incoming.States.Skip(1),(a,b)=>(a,b)))
            {
                double at=pair.a.Step;var point=pair.a.Position;
                while(next<collisions.Length&&collisions[next].Step-1<=pair.a.Step)
                {
                    var hit=collisions[next++];double time=hit.Step-1+Math.Max(0,Math.Min(1,(double)hit.TimeInStep*hz));
                    var center=hit.Point+hit.Normal*radius;
                    Samples(at,time,point,center);at=time;point=center;
                    if(hit.Surface==SurfaceId.FrontWall)
                    {
                        front=true;
                        invalid|=hit.Point.y<=court.TinTop+radius||hit.Point.y>=court.FrontOutLower-radius;
                    }
                    if(hit.Surface==SurfaceId.Floor&&!hit.Persistent)bounces++;
                    if(hit.Surface==SurfaceId.BackWall)invalid|=hit.Point.y>=court.BackOutLower-radius;
                    if(hit.Surface==SurfaceId.LeftWall||hit.Surface==SurfaceId.RightWall)
                        invalid|=hit.Point.y>=Mathf.Lerp(court.FrontOutLower,court.BackOutLower,hit.Point.z/court.Length)-radius;
                }
                Samples(at,pair.b.Step,point,pair.b.Position);
            }
            if(active)ranges.Add((start,last));
            foreach(var range in ranges)
                if(targetStep>=range.start-1d/64&&targetStep<=range.end+1d/64)
                    return new PhysicsWindow{kInMs=(range.start-targetStep)*realMs,kOutMs=(range.end-targetStep)*realMs,sampleMs=realMs/64};
            return null;
        }
    }
}
