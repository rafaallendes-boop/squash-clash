using SquashClash.Core.Config;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    public enum StrokeSide { Drive, Backhand }
    public readonly struct BodyFrame
    {
        public readonly Vector2 PositionXZ;
        public readonly bool RightHanded;
        public readonly float Reach,IdealLateral,PreferredMinHeight,PreferredMaxHeight,WallClearance,ReferenceHeight;
        public BodyFrame(Vector2 position,bool rightHanded,float reach,float lateral,float min,float max,float clearance,float referenceHeight=.9f)
        {PositionXZ=position;RightHanded=rightHanded;Reach=reach;IdealLateral=lateral;PreferredMinHeight=min;PreferredMaxHeight=max;WallClearance=clearance;ReferenceHeight=referenceHeight;}
        public static BodyFrame From(ParamSet source,Vector2? position=null)=>new BodyFrame(
            position??new Vector2((float)source.Get("human.x"),(float)source.Get("human.z")),true,
            (float)source.Get("body.reach"),(float)source.Get("body.idealLateral"),(float)source.Get("body.preferredMinHeight"),
            (float)source.Get("body.preferredMaxHeight"),(float)source.Get("body.wallClearance"));
        public StrokeSide SideFor(Vector3 ball)=>(RightHanded?ball.x<=PositionXZ.x:ball.x>=PositionXZ.x)?StrokeSide.Drive:StrokeSide.Backhand;
        public Vector3 IdealContactPoint(StrokeSide side)=>new Vector3(
            PositionXZ.x+((side==StrokeSide.Drive)==RightHanded?-IdealLateral:IdealLateral),
            (PreferredMinHeight+PreferredMaxHeight)*.5f,PositionXZ.y);
    }
}
