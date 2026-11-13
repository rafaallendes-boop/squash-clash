using System;
using System.Collections.Generic;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    [Serializable] public struct CausalVector
    {
        public float DeltaHeight,DeltaLateral,DeltaSpeedFraction;public string Cause;
        public CausalVector(float height,float speed,string cause){DeltaHeight=height;DeltaLateral=0;DeltaSpeedFraction=speed;Cause=cause;}
    }
    public readonly struct CausalSettings
    {
        public readonly float TimingHeight,TimingSpeed,StretchStart,JamDistance,StretchHeight,StretchSpeed,JamHeight,JamSpeed,CapHeight,CapSpeed;
        public CausalSettings(float timingHeight=.6f,float timingSpeed=-.25f,float stretchStart=.85f,float jamDistance=.35f,float stretchHeight=.4f,float stretchSpeed=-.2f,float jamHeight=.3f,float jamSpeed=-.15f,float capHeight=1,float capSpeed=-.4f)
        {TimingHeight=timingHeight;TimingSpeed=timingSpeed;StretchStart=stretchStart;JamDistance=jamDistance;StretchHeight=stretchHeight;StretchSpeed=stretchSpeed;JamHeight=jamHeight;JamSpeed=jamSpeed;CapHeight=capHeight;CapSpeed=capSpeed;}
    }
    public sealed class CausalReference
    {
        public IReadOnlyList<long> PerfectSteps=>Array.Empty<long>();
        public double ArcRadius=>0;
        public double Distance(Vector3 point,BodyFrame body)=>0;
    }
    public readonly struct CausalInput
    {
        public readonly Vector2 Aim;public readonly float Speed;
        public CausalInput(Vector2 aim,float speed){Aim=aim;Speed=speed;}
    }
    public static class CausalQuality
    {
        public static CausalReference Prepare(Trajectory trajectory,ContactPlan plan,ClockMap clock,TimingSettings timing,double presented,double tempo,Func<long,BodyFrame> bodyAt=null)=>new CausalReference();
        public static CausalVector[] Evaluate(CausalReference reference,BodyFrame body,Vector3 contact,CausalSettings settings)=>Array.Empty<CausalVector>();
        public static CausalInput Apply(Vector2 aim,float speed,IReadOnlyList<CausalVector> vectors,CausalSettings settings)=>new CausalInput(aim,speed);
    }
}
