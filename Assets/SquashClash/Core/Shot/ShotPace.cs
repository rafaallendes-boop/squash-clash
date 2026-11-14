using System;
using SquashClash.Core.Config;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    // What the shared pace curve produced for one request, kept for the shot record and the causal trace.
    [Serializable] public struct ShotPaceResult
    {
        public float DistanceM,Gamma,LinearSpeed,Speed;
        public ShotPaceResult(float distance,float gamma,float linear,float speed){DistanceM=distance;Gamma=gamma;LinearSpeed=linear;Speed=speed;}
    }
    public readonly struct ShotPaceSettings
    {
        public readonly float VMin,VMax,GammaMax,NearM,FullM;
        public ShotPaceSettings(float vMin,float vMax,float gammaMax,float nearM,float fullM){VMin=vMin;VMax=vMax;GammaMax=gammaMax;NearM=nearM;FullM=fullM;}
        public static ShotPaceSettings From(ParamSet p)=>new ShotPaceSettings((float)p.Get("shot.vMin"),(float)p.Get("shot.vMax"),
            (float)p.Get("shot.paceGammaMax"),(float)p.Get("shot.paceNearM"),(float)p.Get("shot.paceFullM"));
    }
    // The one place that turns the gesture power P into a requested speed (distance-aware pace curve, design 2026-11-14).
    public static class ShotPace
    {
        public static ShotPaceResult Requested(float power,Vector3 contact,Vector2 aim,float ballRadius,ParamSet source)
            =>Requested(power,contact,aim,ballRadius,ShotPaceSettings.From(source));
        public static ShotPaceResult Requested(float power,Vector3 contact,Vector2 aim,float ballRadius,ShotPaceSettings s)
        {
            float linear=s.VMin+power*(s.VMax-s.VMin);
            return new ShotPaceResult(0,1,linear,linear);
        }
    }
}
