using System;
using System.Collections.Generic;
using SquashClash.Core.Config;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    public readonly struct PowerSettings
    {
        public readonly float WindowMs,MinCoverageMs,MaxGapMs,DeadZoneMmPerS,FullMmPerS,CurveExponent;
        public readonly int MinSamples;
        public PowerSettings(float window,float coverage,int samples,float gap,float dead,float full,float exponent)
        {WindowMs=window;MinCoverageMs=coverage;MinSamples=samples;MaxGapMs=gap;DeadZoneMmPerS=dead;FullMmPerS=full;CurveExponent=exponent;}
        public static PowerSettings From(ParamSet p)=>new PowerSettings((float)p.Get("power.windowMs"),(float)p.Get("power.minCoverageMs"),p.GetInt("power.minSamples"),(float)p.Get("power.maxGapMs"),(float)p.Get("power.deadZoneMmPerS"),(float)p.Get("power.fullMmPerS"),(float)p.Get("power.curveExponent"));
    }
    public readonly struct PowerResult
    {
        public readonly float P,NetSpeedMmPerS;
        public readonly double CoveredMs,MaxGapMs;
        public readonly int DistinctSamples;
        public readonly bool Conservative;
        public readonly string Reason,SpeedBasis;
        public readonly float PowerSpeedMmPerS;
        public readonly double SupportAgeMs;
        public PowerResult(float p,float speed,double covered,int count,double gap,bool conservative,string reason,string speedBasis="net-window",double supportAgeMs=0)
        {P=p;NetSpeedMmPerS=speedBasis=="net-window"?speed:0;PowerSpeedMmPerS=speed;SpeedBasis=speedBasis;SupportAgeMs=supportAgeMs;CoveredMs=covered;DistinctSamples=count;MaxGapMs=gap;Conservative=conservative;Reason=reason;}
    }
    public static class PowerEstimator
    {
        public static PowerResult Estimate(IReadOnlyList<TouchSample> samples,double releaseTimeReal,float mmPerPx,PowerSettings s,double? dragStartedAtReal=null)
        {
            if(samples==null||samples.Count==0)return Soft("no-samples");
            if(!Finite(releaseTimeReal)||!Finite(mmPerPx)||mmPerPx<=0||s.FullMmPerS<=s.DeadZoneMmPerS)return Soft("invalid-input");
            double start=Math.Max(releaseTimeReal-s.WindowMs/1000d,samples[0].TimeReal);
            if(dragStartedAtReal.HasValue&&!double.IsNaN(dragStartedAtReal.Value))start=Math.Max(start,dragStartedAtReal.Value);
            double covered=Math.Max(0,(releaseTimeReal-start)*1000),gap=0;
            int count=0,previous=-1;
            for(int i=0;i<samples.Count;i++)
            {
                var sample=samples[i];
                if(!Finite(sample.TimeReal)||!Finite(sample.PositionPx.x)||!Finite(sample.PositionPx.y))return Soft("non-finite");
                if(previous>=0)
                {
                    var before=samples[previous];double dt=sample.TimeReal-before.TimeReal;
                    if(dt<0)return Soft("out-of-order");
                    if(dt==0)
                    {
                        if(sample.Phase==TouchPhaseKind.Stationary&&sample.PositionPx==before.PositionPx)continue;
                        return Soft("repeated-time");
                    }
                    // Only an interval overlapping the window interior can limit interpolation.
                    // Exact endpoints are real samples: a preceding hold or a later event is outside W'.
                    // Use the same timestamp tolerance as At/count below (0.1 ns), not a new physical threshold.
                    if(before.TimeReal<releaseTimeReal-1e-10&&sample.TimeReal>start+1e-10)gap=Math.Max(gap,dt*1000);
                }
                if(sample.TimeReal>=start-1e-10&&sample.TimeReal<=releaseTimeReal+1e-10)count++;
                previous=i;
            }
            string reason=covered+1e-6<s.MinCoverageMs?"short-coverage":count<s.MinSamples?"few-samples":gap>s.MaxGapMs+1e-6?"sample-gap":null;
            if(reason!=null)return new PowerResult(0,0,covered,count,gap,true,reason);
            if(!At(samples,start,out var first)||!At(samples,releaseTimeReal,out var last))
                return new PowerResult(0,0,covered,count,gap,true,"unbracketed-endpoint");
            float speed=(float)(Vector2.Distance(first,last)*mmPerPx/(covered/1000));
            if(!Finite(speed))return Soft("non-finite-speed");
            float p=Mathf.Pow(Mathf.Clamp01((speed-s.DeadZoneMmPerS)/(s.FullMmPerS-s.DeadZoneMmPerS)),s.CurveExponent);
            return new PowerResult(p,speed,covered,count,gap,false,"measured");
        }
        private static bool At(IReadOnlyList<TouchSample> samples,double t,out Vector2 point)
        {
            point=default;
            for(int i=0;i<samples.Count;i++)
            {
                var b=samples[i];
                if(Math.Abs(t-b.TimeReal)<1e-10){point=b.PositionPx;return true;}
                if(i==0)continue;var a=samples[i-1];
                if(t>a.TimeReal&&t<b.TimeReal)
                {point=Vector2.Lerp(a.PositionPx,b.PositionPx,(float)((t-a.TimeReal)/(b.TimeReal-a.TimeReal)));return true;}
            }
            return false;
        }
        private static PowerResult Soft(string why)=>new PowerResult(0,0,0,0,0,true,why);
        private static bool Finite(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value);
    }
}
