using System.Collections.Generic;
using System.Linq;
using SquashClash.Core.Config;
namespace SquashClash.Core.Shot
{
    public static class ShotConfigValidator
    {
        public static bool TryValidate(ParamSet candidate,out string error)=>TryValidate(candidate.Ordered.ToDictionary(e=>e.Key,e=>e.Value),out error);
        public static bool TryValidate(IReadOnlyDictionary<string,double> candidate,out string error)
        {
            error=null;
            foreach(var pair in new[]{("power.pullDeadZoneMm","power.pullFullMm"),("power.deadZoneMmPerS","power.fullMmPerS"),("body.preferredMinHeight","body.preferredMaxHeight"),("legal.minHeight","legal.maxHeight")})
                if(candidate.TryGetValue(pair.Item1,out var a)&&candidate.TryGetValue(pair.Item2,out var b)&&a>=b)
                {error=pair.Item1+" must be below "+pair.Item2;return false;}
            foreach(var pair in new[]{("shot.vMin","shot.vMax"),("shot.vMax","shot.vCap"),("power.minCoverageMs","power.windowMs")})
                if(candidate.TryGetValue(pair.Item1,out var a)&&candidate.TryGetValue(pair.Item2,out var b)&&a>b)
                {error=pair.Item1+" must not exceed "+pair.Item2;return false;}
            if(candidate.TryGetValue("timing.validLimit",out var mode)&&mode==1&&candidate.TryGetValue("timing.assistMs",out var assist)&&candidate.TryGetValue("timing.validMs",out var valid)&&assist>=valid)
            {error="timing.assistMs must be below timing.validMs in V mode";return false;}
            return true;
        }
    }
}
