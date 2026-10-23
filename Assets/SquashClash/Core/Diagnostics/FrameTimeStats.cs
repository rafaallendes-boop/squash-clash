using System;
namespace SquashClash.Core.Diagnostics
{
    public sealed class FrameTimeStats
    {
        private readonly long[] histogram=new long[2502]; // 0, upper edges 0.1..250 ms, then overflow.
        private readonly double[] window;
        private int next,count;
        private double elapsedMs;
        public long SessionFrames { get; private set; }
        public double SessionMinutes=>elapsedMs/60000;
        public FrameTimeStats(int windowCapacity=3600)
        {
            if(windowCapacity<1)throw new ArgumentOutOfRangeException(nameof(windowCapacity));window=new double[windowCapacity];
        }
        public void Add(double ms)
        {
            if(!Finite(ms)||ms<0||!Finite(elapsedMs+ms))throw new ArgumentOutOfRangeException(nameof(ms));
            histogram[ms>250?2501:(int)Math.Ceiling(ms*10)]++;
            elapsedMs+=ms;SessionFrames++;window[next]=ms;next=(next+1)%window.Length;count=Math.Min(count+1,window.Length);
        }
        public void ResetSession() { Array.Clear(histogram,0,histogram.Length);Array.Clear(window,0,window.Length);next=count=0;elapsedMs=0;SessionFrames=0; }
        public double SessionPercentile(double p)
        {
            ValidatePercentile(p);if(SessionFrames==0)return double.NaN;
            long rank=Math.Max(1,(long)Math.Ceiling(p/100*SessionFrames)),sum=0;
            for(int i=0;i<histogram.Length;i++){sum+=histogram[i];if(sum>=rank)return i==2501?double.PositiveInfinity:i/10d;}
            throw new InvalidOperationException("Histogram coverage mismatch.");
        }
        public double WindowPercentile(double p)
        {
            ValidatePercentile(p);if(count==0)return double.NaN;
            var sorted=new double[count];Array.Copy(window,sorted,count);Array.Sort(sorted);
            return sorted[Math.Max(1,(int)Math.Ceiling(p/100*count))-1];
        }
        // Exact at histogram edges (including the approved >33 ms threshold); 0.1 ms resolution otherwise.
        public double SessionLongFramesPerMinute(double overMs)
        {
            if(!Finite(overMs)||overMs<0)throw new ArgumentOutOfRangeException(nameof(overMs));
            if(elapsedMs==0)return double.NaN;
            if(overMs>250)return histogram[2501]>0?double.NaN:0;
            long total=0;for(int i=(int)Math.Floor(overMs*10)+1;i<histogram.Length;i++)total+=histogram[i];
            return total/SessionMinutes;
        }
        private static void ValidatePercentile(double p){if(!Finite(p)||p<0||p>100)throw new ArgumentOutOfRangeException(nameof(p));}
        private static bool Finite(double v)=>!double.IsNaN(v)&&!double.IsInfinity(v);
    }
}
