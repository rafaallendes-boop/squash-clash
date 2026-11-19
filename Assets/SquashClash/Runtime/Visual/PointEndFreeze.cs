using System;
using UnityEngine;
namespace SquashClash.Runtime.Visual
{
    // Point-end freeze (PO, 19 nov): STUB, replaced by the real thing in the next commit.
    public static class PointEndFreeze
    {
        public const double Seconds=.1;
        public static bool Enabled {get;set;}=true;
        public static Func<double> Now=()=>Time.unscaledTimeAsDouble;
        public static bool Active=>false;
        public static double Duration {get;private set;}
        public static void Begin(double pauseSeconds,Func<bool> inPause){}
        public static void Reset(){Enabled=true;Now=()=>Time.unscaledTimeAsDouble;Duration=0;}
    }
}
