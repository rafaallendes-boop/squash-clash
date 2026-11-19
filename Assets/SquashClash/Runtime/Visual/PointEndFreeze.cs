using System;
using UnityEngine;
namespace SquashClash.Runtime.Visual
{
    // Point-end freeze (PO, 19 nov): for 100 ms of unscaled real time after a point closes, the views hold the pose they last drew,
    // inside the pause between points that already exists. It is VIEW ONLY: it changes no simulation state, clock, timeScale,
    // fixed step, pause length or start of the next point. Views read Active in LateUpdate (and PlaceholderPlayers.Place) and skip
    // their own update; with no views (AutoQA, headless) nothing reads it and it is a no-op. DEV toggle: Enabled (default on).
    public static class PointEndFreeze
    {
        public const double Seconds=.1;
        public static bool Enabled {get;set;}=true;
        // Real time source, replaceable by tests.
        public static Func<double> Now=()=>Time.unscaledTimeAsDouble;
        private static double until;
        private static Func<bool> inPause;
        // Length of the last freeze: the smaller of 100 ms and the pause it sits in.
        public static double Duration {get;private set;}
        public static bool Active
        {
            get
            {
                if(!Enabled||inPause==null)return false;
                if(Now()>=until||!inPause()){inPause=null;return false;}
                return true;
            }
        }
        // pauseSeconds: the pause between points that is starting; stillInPause: false once the next point has begun.
        public static void Begin(double pauseSeconds,Func<bool> stillInPause)
        {
            Duration=Math.Max(0,Math.Min(Seconds,pauseSeconds));
            if(!Enabled||Duration<=0||stillInPause==null){inPause=null;return;}
            until=Now()+Duration;inPause=stillInPause;
        }
        public static void Reset(){Enabled=true;Now=()=>Time.unscaledTimeAsDouble;Duration=0;inPause=null;until=0;}
    }
}
