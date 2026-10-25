using System;
using System.Collections.Generic;
using SquashClash.Core.Shot;
namespace SquashClash.Runtime.Input
{
    public sealed class TouchSamplingStats
    {
        // Upper bucket edge, 0.1 ms resolution, matching the existing diagnostic convention.
        private readonly int[] intervalBins=new int[10001],ageBins=new int[10001];
        private readonly Dictionary<int,double> lastByTouch=new Dictionary<int,double>();
        private long intervals,ages;private double intervalSum;
        private int frame=-1,frameSamples;
        public long Samples {get;private set;}
        public long DistinctTimes {get;private set;}
        public long DuplicateTimes {get;private set;}
        public long OutOfOrder {get;private set;}
        public long NegativeAges {get;private set;}
        public long IntervalOverflow {get;private set;}
        public long AgeOverflow {get;private set;}
        public int MaxSamplesPerFrame {get;private set;}
        public double MaxGapMs {get;private set;}
        public double MaxAgeMs {get;private set;}
        public double DeliveredHz=>intervalSum>0?intervals*1000/intervalSum:double.NaN;
        public double IntervalP50Ms=>Percentile(intervalBins,intervals,.5);
        public double IntervalP95Ms=>Percentile(intervalBins,intervals,.95);
        public double AgeP50Ms=>Percentile(ageBins,ages,.5);
        public double AgeP95Ms=>Percentile(ageBins,ages,.95);
        public void Record(in TouchSample sample,double receiptReal,int frameNumber)
        {
            Samples++;
            if(frame!=frameNumber){frame=frameNumber;frameSamples=0;}
            MaxSamplesPerFrame=Math.Max(MaxSamplesPerFrame,++frameSamples);
            double age=(receiptReal-sample.TimeReal)*1000;
            if(age<0)NegativeAges++;
            else {ages++;MaxAgeMs=Math.Max(MaxAgeMs,age);if(Add(ageBins,age))AgeOverflow++;}
            bool first=sample.Phase==TouchPhaseKind.Began||!lastByTouch.TryGetValue(sample.FingerId,out _);
            if(first){DistinctTimes++;lastByTouch[sample.FingerId]=sample.TimeReal;}
            else
            {
                double previous=lastByTouch[sample.FingerId],dt=(sample.TimeReal-previous)*1000;
                if(dt==0)DuplicateTimes++;
                else if(dt<0)OutOfOrder++;
                else
                {
                    DistinctTimes++;intervals++;intervalSum+=dt;MaxGapMs=Math.Max(MaxGapMs,dt);
                    if(Add(intervalBins,dt))IntervalOverflow++;
                    lastByTouch[sample.FingerId]=sample.TimeReal;
                }
            }
            if(sample.Phase==TouchPhaseKind.Ended||sample.Phase==TouchPhaseKind.Canceled)lastByTouch.Remove(sample.FingerId);
        }
        public void Clear()
        {
            Array.Clear(intervalBins,0,intervalBins.Length);Array.Clear(ageBins,0,ageBins.Length);lastByTouch.Clear();
            Samples=DistinctTimes=DuplicateTimes=OutOfOrder=NegativeAges=IntervalOverflow=AgeOverflow=intervals=ages=0;
            intervalSum=MaxGapMs=MaxAgeMs=0;frame=-1;frameSamples=MaxSamplesPerFrame=0;
        }
        private static bool Add(int[] bins,double ms)
        {int i=ms>=1000?10000:Math.Max(0,(int)(ms*10));bins[i]++;return i==10000;}
        private static double Percentile(int[] bins,long count,double p)
        {
            if(count==0)return double.NaN;
            long rank=(long)Math.Ceiling(count*p),sum=0;
            for(int i=0;i<bins.Length;i++){sum+=bins[i];if(sum>=rank)return i==10000?double.PositiveInfinity:(i+1)/10d;}
            return double.NaN;
        }
    }
}
