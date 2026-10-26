using System;
using System.Collections.Generic;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    public enum AimState { Aiming, Frozen }
    public enum AimFreezeCause { None, Flick, Release }
    public readonly struct AimSettings
    {
        public readonly float GainMPerMm, StartHeight, FlickMmPerS, FlickHysteresisMmPerS, FreezeLookbackMs;
        public readonly int FlickConfirmSamples;
        public readonly bool FreezeOnRelease;
        public AimSettings(float gain,float height,float flick,float hysteresis,float lookback,int confirm,bool freezeOnRelease=false)
        {GainMPerMm=gain;StartHeight=height;FlickMmPerS=flick;FlickHysteresisMmPerS=hysteresis;FreezeLookbackMs=lookback;FlickConfirmSamples=confirm;FreezeOnRelease=freezeOnRelease;}
        public static AimSettings From(ParamSet p)=>new AimSettings((float)p.Get("aim.gainMPerMm"),(float)p.Get("aim.startHeight"),(float)p.Get("aim.flickMmPerS"),(float)p.Get("aim.flickHysteresisMmPerS"),(float)p.Get("aim.freezeLookbackMs"),p.GetInt("aim.flickConfirmSamples"));
    }
    public sealed class AimModel
    {
        private readonly AimSettings settings;
        private readonly float width;
        private readonly Vector2 initial;
        private readonly List<(double time,Vector2 aim)> history=new List<(double,Vector2)>(128);
        private Vector2 origin,previousPosition;
        private double previousTime,crossingTime;
        private float scale;
        private int consecutive;
        private bool started;
        public AimModel(AimSettings s,CourtDimensions court,bool ballOnLeftSide)
            :this(s,court,court.Width*(ballOnLeftSide ? .75f : .25f)) { }
        public AimModel(AimSettings s,CourtDimensions court,float initialX)
        {
            if(court==null)throw new ArgumentNullException(nameof(court));
            settings=s;width=court.Width;initial=new Vector2(Mathf.Clamp(initialX,0,width),Mathf.Clamp(s.StartHeight,0,5));Aim=initial;
        }
        public Vector2 Aim {get;private set;}
        public AimState State {get;private set;}
        public AimFreezeCause FreezeCause {get;private set;}
        public double FreezeTimeReal {get;private set;}
        public bool FreezeUsedFirstValidPoint {get;private set;}
        public float FreezeJumpMeters {get;private set;}
        public void Update(in TouchSample sample,float mmPerPx)
        {
            if(sample.Phase==TouchPhaseKind.Began)
            {
                if(!Finite(sample.TimeReal)||!Finite(mmPerPx)||mmPerPx<=0)throw new ArgumentOutOfRangeException(nameof(mmPerPx));
                origin=previousPosition=sample.PositionPx;previousTime=sample.TimeReal;scale=mmPerPx;
                Aim=initial;State=AimState.Aiming;FreezeCause=AimFreezeCause.None;consecutive=0;crossingTime=double.NaN;started=true;history.Clear();history.Add((sample.TimeReal,Aim));
                FreezeTimeReal=0;FreezeUsedFirstValidPoint=false;FreezeJumpMeters=0;return;
            }
            if(!started||State==AimState.Frozen||sample.TimeReal<=previousTime||!Finite(sample.TimeReal))return;
            var delta=(sample.PositionPx-origin)*scale*settings.GainMPerMm;
            // AX-1: the rear camera projects -X towards screen right. Camera motion is not input.
            Aim=new Vector2(Mathf.Clamp(initial.x-delta.x,0,width),Mathf.Clamp(initial.y+delta.y,0,5));
            history.Add((sample.TimeReal,Aim));
            double speed=Vector2.Distance(sample.PositionPx,previousPosition)*scale/(sample.TimeReal-previousTime);
            if(speed>settings.FlickMmPerS)
            {
                if(double.IsNaN(crossingTime))crossingTime=sample.TimeReal;
                consecutive++;
                if(consecutive>=settings.FlickConfirmSamples)
                {
                    double wanted=crossingTime-settings.FreezeLookbackMs/1000d;
                    var held=history[0].aim;FreezeUsedFirstValidPoint=wanted<history[0].time;
                    for(int i=1;i<history.Count;i++)
                    {
                        var a=history[i-1];var b=history[i];
                        if(wanted>=a.time&&wanted<=b.time)
                        {held=Vector2.Lerp(a.aim,b.aim,(float)((wanted-a.time)/(b.time-a.time)));break;}
                    }
                    FreezeJumpMeters=Vector2.Distance(Aim,held);Aim=held;State=AimState.Frozen;FreezeCause=AimFreezeCause.Flick;FreezeTimeReal=crossingTime;
                }
            }
            else
            {
                // Confirmation requires consecutive intervals above the high threshold.
                consecutive=0;
                if(speed<settings.FlickMmPerS-settings.FlickHysteresisMmPerS)crossingTime=double.NaN;
            }
            if(sample.Phase==TouchPhaseKind.Ended&&settings.FreezeOnRelease&&State==AimState.Aiming)
            {State=AimState.Frozen;FreezeCause=AimFreezeCause.Release;FreezeTimeReal=sample.TimeReal;FreezeJumpMeters=0;FreezeUsedFirstValidPoint=false;}
            previousPosition=sample.PositionPx;previousTime=sample.TimeReal;
        }
        private static bool Finite(double x)=>!double.IsNaN(x)&&!double.IsInfinity(x);
    }
}
