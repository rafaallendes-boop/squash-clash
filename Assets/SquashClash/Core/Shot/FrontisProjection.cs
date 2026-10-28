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
            Court=new CourtDimensions{Width=court.Width,Length=court.Length,WallHeight=court.WallHeight,TinTop=court.TinTop,FrontOutLower=court.FrontOutLower,LineWidth=court.LineWidth};
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
        public bool IsInvalid(Vector2 aim)
        {
            var touched=new List<LineId>(2);
            LineContactClassifier.Overlapping(CourtLines.Build(Court),SurfaceId.FrontWall,aim,.01f,touched);
            return aim.y<=Court.TinTop||aim.y>=Court.FrontOutLower||touched.Contains(LineId.Tin)||touched.Contains(LineId.FrontOut);
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    }
}
