using System;
using SquashClash.Runtime.App;
using UnityEngine;
namespace SquashClash.Runtime.Input
{
    public enum DpiSource { Reported, NominalFallback, PhysicallyVerified }
    public static class ScreenMetrics
    {
        public static float MmPerPx(out DpiSource source)=>MmPerPx(Screen.dpi,(float)AppBootstrap.Instance.Tuning.Applied.Get("input.fallbackDpi"),out source);
        public static float MmPerPx(float reportedDpi,float fallbackDpi,out DpiSource source)
        {
            if(float.IsNaN(fallbackDpi)||float.IsInfinity(fallbackDpi)||fallbackDpi<=0)throw new ArgumentOutOfRangeException(nameof(fallbackDpi));
            bool plausible=!float.IsNaN(reportedDpi)&&!float.IsInfinity(reportedDpi)&&reportedDpi>=150&&reportedDpi<=700;
            // A reported or nominal density is never promoted to a verified physical measurement.
            source=plausible?DpiSource.Reported:DpiSource.NominalFallback;
            return 25.4f/(plausible?reportedDpi:fallbackDpi);
        }
    }
}
