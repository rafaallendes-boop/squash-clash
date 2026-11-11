using System;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    public readonly struct AnticipationRelease
    {
        public readonly double EventAtReal,TargetAtReal,PresentedAtReal;
        public readonly int PlanVersion,Seed,Attempt;
        public AnticipationRelease(double at,double target,double presented,int version,int seed,int attempt)
        {EventAtReal=at;TargetAtReal=target;PresentedAtReal=presented;PlanVersion=version;Seed=seed;Attempt=attempt;}
    }
    public sealed class AnticipationBot
    {
        private readonly int seed;
        private readonly double offsetMs;
        public AnticipationBot(int seed,double offsetMs)
        {if(!Finite(offsetMs))throw new ArgumentOutOfRangeException(nameof(offsetMs));this.seed=seed;this.offsetMs=offsetMs;}
        public bool TrySchedule(PresentedPlans plans,double observedAtReal,TimingSettings timing,int attempt,out AnticipationRelease release)
        {
            release=default;
            if(attempt<0||!Finite(observedAtReal)||plans==null||!plans.TryVisibleAt(observedAtReal,out var plan,out double target,out double presented)||!plan.Exists||plan.TStarStep<0)return false;
            var cue=ImpactCue.Evaluate(observedAtReal,presented,target,timing,true,false);
            double at=target+offsetMs/1000;
            if(cue.Phase==ImpactCuePhase.Hidden||cue.Phase==ImpactCuePhase.Closed||at<observedAtReal)return false;
            release=new AnticipationRelease(at,target,presented,plan.Version,seed,attempt);return true;
        }
        // Seed changes intention only. The fixed release offset never receives random jitter.
        public (Vector2 aim,float power) Intent(int attempt,Vector2 center,float variation)
        {
            if(attempt<0||!Finite(variation)||variation<0||!Finite(center.x)||!Finite(center.y))throw new ArgumentOutOfRangeException(nameof(attempt));
            uint key=unchecked((uint)seed*747796405u+(uint)attempt*2891336453u+277803737u);
            uint Next(){key^=key<<13;key^=key>>17;key^=key<<5;return key;}
            float x=((Next()%10001)/5000f-1)*variation,y=((Next()%10001)/5000f-1)*variation;
            return (center+new Vector2(x,y),.25f*(1+Next()%3));
        }
        private static bool Finite(double n)=>!double.IsNaN(n)&&!double.IsInfinity(n);
    }
    public sealed class AnticipationReport
    {
        private readonly bool validLimitOn;
        private double x,y,z;
        public int Total {get;private set;} public int Contacts {get;private set;} public int Air {get;private set;} public int Late {get;private set;} public int Other {get;private set;}
        public int ZoneObserved {get;private set;} public int ZoneHits {get;private set;} public int VectorSamples {get;private set;}
        public Vector3? MeanCausal=>VectorSamples==0?(Vector3?)null:new Vector3((float)(x/VectorSamples),(float)(y/VectorSamples),(float)(z/VectorSamples));
        public double? ZoneRateAmongObserved=>ZoneObserved==0?(double?)null:ZoneHits/(double)ZoneObserved;
        public AnticipationReport(bool validLimitOn) {this.validLimitOn=validLimitOn;}
        public void Add(bool validLimitOn,ShotResult result,FailKind fail,bool applied,bool? zoneHit,Vector3? causalVector)
        {
            bool contact=applied&&(result==ShotResult.Valid||result==ShotResult.Perfect);
            if(this.validLimitOn!=validLimitOn||(!contact&&(zoneHit.HasValue||causalVector.HasValue))||
                (applied&&!contact)||(contact&&fail!=FailKind.None)||
                (causalVector.HasValue&&(!Finite(causalVector.Value.x)||!Finite(causalVector.Value.y)||!Finite(causalVector.Value.z))))
                throw new ArgumentException("Do not mix timing modes or substitute no-contact vectors/zone samples.");
            Total++;
            if(contact)
            {
                Contacts++;if(zoneHit.HasValue){ZoneObserved++;if(zoneHit.Value)ZoneHits++;}
                if(causalVector.HasValue){var v=causalVector.Value;VectorSamples++;x+=v.x;y+=v.y;z+=v.z;}
            }
            else if(result==ShotResult.Failed&&fail==FailKind.Air)Air++;
            else if(result==ShotResult.Failed&&fail==FailKind.Late)Late++;
            else Other++;
        }
        private static bool Finite(float n)=>!float.IsNaN(n)&&!float.IsInfinity(n);
    }
}
