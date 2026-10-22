using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Core.Spike
{
    public static class PhysicsScenarios
    {
        public static IReadOnlyList<PhysicsScenario> Fixed(CourtDimensions d, PhysicsParams p)
        {
            var result = new List<PhysicsScenario>(17);
            result.Add(Target("S1", "Drive paralelo rápido, tres contactos", new Vector3(1,.9f,7.5f), SurfaceId.FrontWall, new Vector3(.6f,2.2f,0),28,3,p));
            result.Add(Target("S2", "Drop bajo, cinco centímetros sobre el tin",new Vector3(4.5f,.6f,3),SurfaceId.FrontWall,new Vector3(5.5f,d.TinTop+.05f,0),6,1,p));
            float angle=8*Mathf.Deg2Rad;
            result.Add(Directed("S3","Rasante en la lateral derecha",new Vector3(.8f,1,8),new Vector3(-Mathf.Sin(angle),0,-Mathf.Cos(angle)),20,1,p));
            result.Add(Target("S4","Nick lateral derecha y suelo",new Vector3(1.5f,1,4),SurfaceId.RightWall,new Vector3(0,p.BallRadius,2.5f),12,2,p));
            result.Add(Target("S5","Frontis cerca de la esquina frontal derecha",new Vector3(2,1.5f,3),SurfaceId.FrontWall,new Vector3(p.BallRadius+.005f,1.5f,0),15,2,p));
            result.Add(Target("S6","Esquina trasera izquierda, dos contactos",new Vector3(3.2f,1,6),SurfaceId.BackWall,new Vector3(d.Width-p.BallRadius,1,d.Length),25,2,p));
            result.Add(Directed("S7","Sale de la pared trasera",new Vector3(3.2f,1.2f,6),new Vector3(0,.2f,1),15,1,p));
            var lines=new[]{LineId.FrontOut,LineId.Tin,LineId.Service};
            float[] edge={d.FrontOutLower-.01f,d.TinTop+.01f,d.ServiceLower-.01f};
            for(int line=0;line<3;line++) for(int side=0;side<3;side++)
            {
                float offset=(side-1)*.003f*(line==1?-1:1);
                int index=line*3+side;
                var scenario=Target("S9"+(char)('a'+index),"Borde de "+lines[line]+": "+new[]{"fuera","exacto","dentro"}[side],new Vector3(3.2f,1,5),SurfaceId.FrontWall,new Vector3(3.2f,edge[line]+offset,0),15,1,p);
                scenario.Line=lines[line];scenario.ExpectTouch=side==1?(bool?)null:side==2;
                result.Add(scenario);
            }
            var rest=Directed("S10","Se asienta en el suelo",new Vector3(3.2f,.3f,5),Vector3.down,1,1,p);
            rest.MaxSteps=6*p.StepHz;rest.Stop=StopCondition.MaxSteps;result.Add(rest);
            return result.AsReadOnly();
        }

        public static IEnumerable<PhysicsScenario> Random(CourtDimensions d, PhysicsParams p, int count, int seed)
        {
            var random=new System.Random(seed);
            for(int i=0;i<count;i++)
            {
                var position=new Vector3((float)(.3+random.NextDouble()*5.8),(float)(.3+random.NextDouble()*2.2),(float)(.3+random.NextDouble()*9.15));
                double y=2*random.NextDouble()-1, phi=2*Math.PI*random.NextDouble(), horizontal=Math.Sqrt(1-y*y);
                var direction=new Vector3((float)(horizontal*Math.Cos(phi)),(float)y,(float)(horizontal*Math.Sin(phi)));
                var scenario=Directed("S8-"+i,"Lanzamiento aleatorio",position,direction,(float)(2+33*random.NextDouble()),0,p);
                // Robustness observes the full horizon, including after two bounces.
                scenario.Stop=StopCondition.MaxSteps;
                yield return scenario;
            }
        }

        public static IReadOnlyList<PhysicsScenario> EdgeDirected(CourtDimensions d, PhysicsParams p)
        {
            float r=p.BallRadius;
            var start=new Vector3(d.Width/2,1,d.Length/2);
            var centers=new List<Vector3>();
            foreach(float x in new[]{r,d.Width-r}) foreach(float z in new[]{r,d.Length-r}) centers.Add(new Vector3(x,1,z));
            foreach(float x in new[]{r,d.Width-r}) foreach(float z in new[]{r,d.Length-r}) centers.Add(new Vector3(x,r,z));
            centers.Add(new Vector3(r,r,d.Length/2));centers.Add(new Vector3(d.Width-r,r,d.Length/2));
            centers.Add(new Vector3(d.Width/2,r,r));centers.Add(new Vector3(d.Width/2,r,d.Length-r));
            var result=new List<PhysicsScenario>(36);
            foreach(float speed in new[]{10f,25f,40f}) for(int i=0;i<centers.Count;i++)
            {
                var center=centers[i];var face=i>=4?SurfaceId.Floor:(center.z<d.Length/2?SurfaceId.FrontWall:SurfaceId.BackWall);
                var s=Basic("S8d-"+result.Count,"Cobertura dirigida de aristas y vértices",start,p,i>=4&&i<8?3:2);
                s.ContactSurface=face;s.ContactPoint=center-CourtDimensions.InwardNormal(face)*r;
                // S8d specifies total speed. Use the low arc to hit the prescribed center.
                double distanceSquared=(center-start).sqrMagnitude, dy=(double)center.y-start.y, g=p.Gravity;
                double a=speed*speed-g*dy, discriminant=a*a-g*g*distanceSquared;
                if(discriminant<0)throw new InvalidOperationException("Unreachable S8d target at its specified speed.");
                double time=Math.Sqrt(2*distanceSquared/(a+Math.Sqrt(discriminant)));
                double horizontalDistance=Math.Sqrt(Math.Pow((double)center.x-start.x,2)+Math.Pow((double)center.z-start.z,2));
                s.Start.Velocity=Ballistics.SolveCenterTo(start,center,(float)(horizontalDistance/time),p.Gravity);
                s.Stop=StopCondition.MaxSteps; result.Add(s);
            }
            return result.AsReadOnly();
        }

        private static PhysicsScenario Basic(string id,string description,Vector3 start,PhysicsParams p,int minimum) => new PhysicsScenario {
            Id=id,Description=description,Start=new BallState {Position=start,Step=0},MaxSteps=3*p.StepHz,Stop=StopCondition.SecondFloorBounce,MinContactsBeforeStop=minimum
        };
        private static PhysicsScenario Target(string id,string description,Vector3 start,SurfaceId face,Vector3 point,float horizontalSpeed,int minimum,PhysicsParams p)
        {
            var s=Basic(id,description,start,p,minimum);s.ContactSurface=face;s.ContactPoint=point;
            s.Start.Velocity=Ballistics.SolveCenterTo(start,Ballistics.CenterForContact(face,point,p.BallRadius),horizontalSpeed,p.Gravity);return s;
        }
        private static PhysicsScenario Directed(string id,string description,Vector3 start,Vector3 direction,float speed,int minimum,PhysicsParams p)
        {
            var s=Basic(id,description,start,p,minimum);s.Start.Velocity=direction.normalized*speed;return s;
        }
    }
}
