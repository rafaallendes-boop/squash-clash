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
        public static bool Sample(double pause,double elapsed,Vector2 box,float handHeight,float radius,out Vector3 ball){ball=default;return false;}
    }
}
