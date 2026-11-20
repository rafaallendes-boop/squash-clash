using System.Collections.Generic;
using SquashClash.Core.Court;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    // Immutable view snapshot captured when Began is processed. No physics/input authority.
    public sealed class FrontisProjection
    {
        public readonly Matrix4x4 CameraToWorld, Projection;
        public readonly Rect Viewport, AdmissionArea;
        public readonly double ObservedAt;
        public readonly int Frame;
        public readonly CourtDimensions Court;
        public FrontisProjection(Matrix4x4 cameraToWorld,Matrix4x4 projection,Rect viewport,Rect area,CourtDimensions court,double observedAt,int frame)
        {
            CameraToWorld=cameraToWorld;Projection=projection;Viewport=viewport;AdmissionArea=area;
            Court=new CourtDimensions{Width=court.Width,Length=court.Length,WallHeight=court.WallHeight,TinTop=court.TinTop,FrontOutLower=court.FrontOutLower,LineWidth=court.LineWidth,BackOutLower=court.BackOutLower};
            ObservedAt=observedAt;Frame=frame;
        }
        public bool TryMap(Vector2 point,out Vector2 aim,out string reason)
        {
            aim=Vector2.zero;reason="non-finite-point";
            if(!Finite(point.x)||!Finite(point.y))return false;
            reason="outside-admission-area";
            if(!AdmissionArea.Contains(point)||!Viewport.Contains(point))return false;
            reason="invalid-ray";
            if(Viewport.width<=0||Viewport.height<=0||!Finite(Projection.determinant)||Projection.determinant==0||CameraToWorld.determinant==0)return false;
            float x=(point.x-Viewport.xMin)/Viewport.width*2-1,y=(point.y-Viewport.yMin)/Viewport.height*2-1;
            var inverse=Projection.inverse;
            Vector3 near=CameraToWorld.MultiplyPoint(inverse.MultiplyPoint(new Vector3(x,y,-1)));
            Vector3 far=CameraToWorld.MultiplyPoint(inverse.MultiplyPoint(new Vector3(x,y,1)));
            var delta=far-near;
            if(!Finite(delta.z)||delta.z>=0)return false;
            float t=-near.z/delta.z;
            if(!Finite(t)||t<0)return false;
            var hit=near+delta*t;
            if(!Finite(hit.x)||!Finite(hit.y))return false;
            reason="outside-front-wall";
            if(hit.x<0||hit.x>Court.Width||hit.y<0||hit.y>Court.WallHeight)return false;
            aim=new Vector2(hit.x,hit.y);reason="accepted";return true;
        }
        // Two-wall shot (design 2026-11-20-boast-predeclaration, section 1): a touch whose ray misses the front wall may still meet a
        // visible side wall: addendum 1, the whole length of the court (0 < z < Length), between the floor and the side out line. Same
        // captured view and the same admission checks as TryMap; TryMap itself is unchanged.
        public bool TryMapSide(Vector2 point,out SideAim side)
        {
            side=default;
            if(!Finite(point.x)||!Finite(point.y)||!AdmissionArea.Contains(point)||!Viewport.Contains(point))return false;
            if(Viewport.width<=0||Viewport.height<=0||!Finite(Projection.determinant)||Projection.determinant==0||CameraToWorld.determinant==0)return false;
            float x=(point.x-Viewport.xMin)/Viewport.width*2-1,y=(point.y-Viewport.yMin)/Viewport.height*2-1;
            var inverse=Projection.inverse;
            Vector3 near=CameraToWorld.MultiplyPoint(inverse.MultiplyPoint(new Vector3(x,y,-1)));
            Vector3 far=CameraToWorld.MultiplyPoint(inverse.MultiplyPoint(new Vector3(x,y,1)));
            var delta=far-near;
            if(!Finite(delta.x)||delta.x==0)return false;
            var wall=delta.x<0?SurfaceId.RightWall:SurfaceId.LeftWall;
            float t=((wall==SurfaceId.RightWall?0:Court.Width)-near.x)/delta.x;
            if(!Finite(t)||t<0)return false;
            var hit=near+delta*t;
            if(!Finite(hit.y)||!Finite(hit.z))return false;
            if(!(hit.z>0&&hit.z<Court.Length&&hit.y>0&&hit.y<SideAim.OutEdge(Court,hit.z)))return false;
            side=new SideAim(wall,hit.z,hit.y);return true;
        }
        public bool IsInvalid(Vector2 aim)
        {
            var touched=new List<LineId>(2);
            LineContactClassifier.Overlapping(CourtLines.Build(Court),SurfaceId.FrontWall,aim,.01f,touched);
            return aim.y<=Court.TinTop||aim.y>=Court.FrontOutLower||touched.Contains(LineId.Tin)||touched.Contains(LineId.FrontOut);
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    }
}
