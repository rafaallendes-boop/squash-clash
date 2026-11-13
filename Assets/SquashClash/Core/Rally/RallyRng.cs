using System;
namespace SquashClash.Core.Rally
{
    // Small explicit generator (SplitMix64): the same seed gives the same sequence on every runtime.
    public sealed class RallyRng
    {
        private ulong state;
        private bool hasSpare;
        private double spare;
        public RallyRng(long seed){state=unchecked((ulong)seed*0x9E3779B97F4A7C15UL+0xD1B54A32D192ED03UL);}
        public ulong NextBits()
        {
            ulong z=unchecked(state+=0x9E3779B97F4A7C15UL);
            z=unchecked((z^(z>>30))*0xBF58476D1CE4E5B9UL);z=unchecked((z^(z>>27))*0x94D049BB133111EBUL);
            return z^(z>>31);
        }
        // Uniform in [0, 1).
        public double NextDouble()=>(NextBits()>>11)*(1.0/9007199254740992.0);
        public double NextGaussian(double sigma=1)
        {
            if(hasSpare){hasSpare=false;return spare*sigma;}
            double u=1-NextDouble(),v=NextDouble(),radius=Math.Sqrt(-2*Math.Log(u)),angle=2*Math.PI*v;
            spare=radius*Math.Sin(angle);hasSpare=true;return radius*Math.Cos(angle)*sigma;
        }
    }
}
