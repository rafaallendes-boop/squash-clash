using UnityEngine;
namespace SquashClash.AutoQA
{
    internal static class ScriptedPlayer
    {
        internal static TouchScript Build(Scenario s,Vector2 anchor,double presentedTarget,float mmPerPx)
        {
            double end=presentedTarget+s.releaseOffsetMs/1000,begin=end-s.durationMs/1000;
            TouchPoint Sample(string phase,double t,float down)=>new TouchPoint{phase=phase,t=t,x=anchor.x,y=anchor.y-down/mmPerPx};
            return new TouchScript{samples=new[]{Sample("Began",begin,0),Sample("Moved",end-.1,s.pullMm),Sample("Ended",end,s.pullMm)}};
        }
    }
}
