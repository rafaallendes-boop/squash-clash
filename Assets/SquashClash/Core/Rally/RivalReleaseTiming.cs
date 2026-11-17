using System;
namespace SquashClash.Core.Rally
{
    public static class RivalReleaseTiming
    {
        // Real corrected milliseconds relative to T*. Never resamples or clamps the Gaussian draw.
        public static double Adjust(double sampleMs,double lowerMs,double upperMs,double sigmaMs,double marginSigma)
        {
            if(sigmaMs<=0||marginSigma<=0)return sampleMs;
            double margin=sigmaMs*marginSigma;
            // Return the original double, including signed zero, when no adjustment is necessary.
            if(lowerMs<=-margin&&upperMs>=margin)return sampleMs;
            double mean=upperMs-lowerMs<2*margin
                ?lowerMs+(upperMs-lowerMs)/2
                :Math.Max(lowerMs+margin,Math.Min(0,upperMs-margin));
            return mean==0?sampleMs:sampleMs+mean;
        }
    }
}
