using UnityEngine;
namespace SquashClash.Runtime.Visual
{
    // Rival serve ritual (spec 2026-11-24): VIEW ONLY. Inside the existing pause before the rival's serve a separate drawn ball dribbles twice
    // from hand height to the floor, moves to the exact serve start point and holds still. Pure timeline; nothing here touches the simulation.
    public static class RivalServeRitual
    {
        public const double Lead=1.35,BounceSeconds=.5,MoveSeconds=.2,StillSeconds=.15;
        public const int Bounces=2;
        public const float SideOffset=.32f,FrontOffset=.10f;
        public static bool Enabled {get;set;}=true;
        public static void Reset(){Enabled=true;}
        public static double Start(double pause)=>pause-Lead;
        public static bool Fits(double pause)=>pause-Lead>=PointEndFreeze.Seconds-1e-9;
        // The drawn ball at `elapsed` seconds into a pause of `pause` seconds; false when there is no ritual (disabled, pause too short, outside the window).
        public static bool Sample(double pause,double elapsed,Vector2 box,float handHeight,float radius,out Vector3 ball)
        {
            ball=default;
            if(!Enabled||!Fits(pause)||elapsed<Start(pause)||elapsed>pause)return false;
            double t=elapsed-Start(pause);float x=box.x+SideOffset,z=box.y-FrontOffset;
            if(t<Bounces*BounceSeconds)
            {
                float u=(float)((t%BounceSeconds)/BounceSeconds),s=Mathf.Abs(2*u-1),q=1-s;
                ball=new Vector3(x,radius+(handHeight-radius)*(1-q*q),z);return true;
            }
            t-=Bounces*BounceSeconds;
            if(t<MoveSeconds){float k=(float)(t/MoveSeconds);k=k*k*(3-2*k);ball=new Vector3(Mathf.Lerp(x,box.x,k),handHeight,Mathf.Lerp(z,box.y,k));return true;}
            ball=new Vector3(box.x,handHeight,box.y);return true;
        }
    }
}
