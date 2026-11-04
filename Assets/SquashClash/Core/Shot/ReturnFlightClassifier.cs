using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    // Diagnostic intersection, not point scoring. Uses an isolated P2 instance.
    internal sealed class ReturnFlightClassifier
    {
        private readonly CourtDimensions court;private readonly PhysicsParams physics;private readonly IBallSimulationFactory factory;
        private readonly IReadOnlyList<LineBand> bands;private readonly List<ContactEvent> hits=new List<ContactEvent>(4);
        public ReturnFlightClassifier(CourtDimensions court,PhysicsParams physics,IBallSimulationFactory factory)
        {this.court=court;this.physics=physics;this.factory=factory;bands=CourtLines.Build(court);}
        public bool Within(Vector3 position,ShotSolution solution)
        {
            if(!solution.Solved)return false;
            using(var sim=factory.Create(physics,court))
            {
                sim.Reset(new BallState{Position=position,Velocity=solution.Velocity,Step=0});
                double floor=double.NaN,outAt=double.PositiveInfinity,tinAt=double.PositiveInfinity,escapeAt=double.PositiveInfinity;
                bool first=true,front=false,inside=false;
                for(int step=1;step<=physics.StepHz*10;step++)
                {
                    sim.Step(hits);
                    foreach(var h in hits)
                    {
                        if(h.Persistent)continue;
                        double time=(h.Step-1)/(double)physics.StepHz+h.TimeInStep;
                        if(first){first=false;front=h.Surface==SurfaceId.FrontWall;}
                        if(IsOut(h,bands))outAt=Math.Min(outAt,time);
                        if(IsTin(h,court,bands))tinAt=Math.Min(tinAt,time);
                        if(h.Surface==SurfaceId.Floor&&double.IsNaN(floor))
                        {floor=time;inside=h.Point.x>=0&&h.Point.x<=court.Width&&h.Point.z>=0&&h.Point.z<=court.Length;}
                    }
                    var s=sim.State;
                    if(s.Position.y>court.WallHeight||s.Position.x<0||s.Position.x>court.Width||s.Position.z<0||s.Position.z>court.Length)
                        escapeAt=Math.Min(escapeAt,s.Step/(double)physics.StepHz);
                    if(!double.IsNaN(floor))return front&&inside&&outAt>floor&&tinAt>floor&&escapeAt>floor;
                }
            }
            return false;
        }
 static bool OutBand(LineId id)=>id==LineId.FrontOut||id==LineId.BackOut||id==LineId.LeftSideOut||id==LineId.RightSideOut;
 static bool FiniteWall(ContactEvent h,CourtDimensions court) =>
  (h.Surface==SurfaceId.FrontWall||h.Surface==SurfaceId.BackWall)?h.Point.x>=0&&h.Point.x<=court.Width:
  (h.Surface==SurfaceId.LeftWall||h.Surface==SurfaceId.RightWall)?h.Point.z>=0&&h.Point.z<=court.Length:true;
 static bool IsOut(ContactEvent h,IReadOnlyList<LineBand> bands) {
  foreach(var b in bands)if(b.Surface==h.Surface&&OutBand(b.Id)) {
   var e=b.B-b.A;float len=e.magnitude;float along=Vector2.Dot(h.UV-b.A,e/len);
   if((along>=0&&along<=len&&Vector2.Dot(h.UV-b.A,b.Growth)>=0)||LineContactClassifier.Touches(in b,h.UV,.01f))return true;
  }
  return false;
 }
 static bool IsTin(ContactEvent h,CourtDimensions court,IReadOnlyList<LineBand> bands) {
  if(h.Surface!=SurfaceId.FrontWall||!FiniteWall(h,court))return false;
  if(h.Point.y<=court.TinTop)return true;
  foreach(var b in bands)if(b.Id==LineId.Tin&&LineContactClassifier.Touches(in b,h.UV,.01f))return true;
  return false;
 }
    }
}
