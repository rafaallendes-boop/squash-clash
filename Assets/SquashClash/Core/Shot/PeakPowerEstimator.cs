using System;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    // One causal update per raw sample, constant memory/time; all decay uses real timestamps.
    public sealed class PeakPowerEstimator
    {
        private readonly PowerSettings settings;
        private readonly double smoothingSeconds,decaySeconds;
        private TouchSample previous;
        private double runStart,runGap,smooth,peak,peakAt,supportCovered,supportGap;
        private int runCount,supportCount;
        private bool hasSample,hasSupport;
        private string invalid;
        public PeakPowerEstimator(PowerSettings settings,double smoothingMs,double decayMs)
        {this.settings=settings;smoothingSeconds=smoothingMs/1000;decaySeconds=decayMs/1000;}
        public PowerResult Push(in TouchSample sample,float mmPerPx)
        {
            if(sample.Phase==TouchPhaseKind.Began)
            {hasSample=hasSupport=false;invalid=null;peak=smooth=runGap=0;runCount=supportCount=0;supportCovered=supportGap=0;}
            if(invalid!=null)return Result(sample.TimeReal);
            if(!Finite(sample.TimeReal)||!Finite(sample.PositionPx.x)||!Finite(sample.PositionPx.y)||!Finite(mmPerPx)||mmPerPx<=0||
                !Finite(smoothingSeconds)||!Finite(decaySeconds)||smoothingSeconds<=0||decaySeconds<=0||settings.FullMmPerS<=settings.DeadZoneMmPerS)
            {invalid="invalid-input";return Result(sample.TimeReal);}
            if(!hasSample)
            {previous=sample;runStart=sample.TimeReal;runCount=1;hasSample=true;return Result(sample.TimeReal);}
            double dt=sample.TimeReal-previous.TimeReal;
            if(dt<=0)
            {
                if(dt==0&&sample.Phase==TouchPhaseKind.Stationary&&sample.PositionPx==previous.PositionPx)return Result(sample.TimeReal);
                invalid=dt<0?"out-of-order":"repeated-time";return Result(sample.TimeReal);
            }
            peak*=Math.Exp(-dt/decaySeconds);
            double speed=Vector2.Distance(sample.PositionPx,previous.PositionPx)*mmPerPx/dt;
            if(!Finite(speed)){invalid="non-finite-speed";return Result(sample.TimeReal);}
            if(dt*1000<=settings.MaxGapMs+1e-6)
            {
                runCount++;runGap=Math.Max(runGap,dt*1000);
                smooth+=(1-Math.Exp(-dt/smoothingSeconds))*(speed-smooth);
                double covered=(sample.TimeReal-runStart)*1000;
                if(runCount>=settings.MinSamples&&covered+1e-6>=settings.MinCoverageMs)
                {
                    hasSupport=true;
                    if(smooth>=peak)
                    {peak=smooth;peakAt=sample.TimeReal;supportCovered=covered;supportCount=runCount;supportGap=runGap;}
                }
            }
            else {runCount=1;runStart=sample.TimeReal;runGap=0;smooth=0;}
            previous=sample;return Result(sample.TimeReal);
        }
        private PowerResult Result(double now)
        {
            string reason=invalid??(hasSupport?"measured":"no-supported-run");bool conservative=invalid!=null||!hasSupport;
            float speed=conservative?0:(float)peak;
            float p=conservative?0:Mathf.Pow(Mathf.Clamp01((speed-settings.DeadZoneMmPerS)/(settings.FullMmPerS-settings.DeadZoneMmPerS)),settings.CurveExponent);
            return new PowerResult(p,speed,supportCovered,supportCount,supportGap,conservative,reason,"decayed-peak",hasSupport?Math.Max(0,(now-peakAt)*1000):0);
        }
        private static bool Finite(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value);
    }
}
