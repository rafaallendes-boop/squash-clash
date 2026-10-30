using System;
using System.Collections.Generic;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
namespace SquashClash.Core.Shot
{
    public enum ShotResult { Perfect, Valid, Failed, NoOrder, Unreachable, SystemFault, AttemptConsumed, NotEvaluable }
    public enum FailKind { None, Air, Late }
    public readonly struct TimingSettings
    {
        public readonly double AssistMs, ValidMs, LatencyCompMs;
        public readonly bool ValidLimitOn, EarlySoft;
        public TimingSettings(double assistMs=40, bool validLimitOn=true, double validMs=100, bool earlySoft=false, double latencyCompMs=0)
        { AssistMs=assistMs; ValidLimitOn=validLimitOn; ValidMs=validMs; EarlySoft=earlySoft; LatencyCompMs=latencyCompMs; }
        public static TimingSettings From(ParamSet p)=>new TimingSettings(p.Get("timing.assistMs"), p.GetInt("timing.validLimit")==1, p.Get("timing.validMs"), p.GetInt("timing.earlyPolicy")==1, p.Get("timing.latencyCompMs"));
    }
    public readonly struct ContactDecision
    {
        public readonly ShotResult Result;
        public readonly FailKind Fail;
        public readonly double DeltaMs, PlanPresentedAtReal, TargetContactAtReal, ReleaseEventAtReal, ReleaseCorrectedAtReal, QuantizationErrorMsSigned;
        public readonly long ContactStep;
        public readonly bool Scheduled, EarlyGeometry, UsedNeighborLegalStep, QuantizationAboveThirdOfA;
        public readonly int PlanVersionUsed;
        public readonly string Reason;
        public ContactDecision(ShotResult result, FailKind fail, double delta, long step, bool scheduled, bool early,
            int version, double presented, double target, double original, double corrected, double error=0, bool neighbor=false, bool coarse=false, string reason=null)
        { Result=result; Fail=fail; DeltaMs=delta; ContactStep=step; Scheduled=scheduled; EarlyGeometry=early; PlanVersionUsed=version;
          PlanPresentedAtReal=presented; TargetContactAtReal=target; ReleaseEventAtReal=original; ReleaseCorrectedAtReal=corrected;
          QuantizationErrorMsSigned=error; UsedNeighborLegalStep=neighbor; QuantizationAboveThirdOfA=coarse; Reason=reason; }
    }
    public sealed class PresentedPlans
    {
        private readonly struct Entry
        {
            public readonly double Presented, Target;
            public readonly ContactPlan Plan;
            public Entry(double time, ContactPlan plan, double target) { Presented=time; Plan=plan; Target=target; }
        }
        private readonly List<Entry> entries=new List<Entry>();
        public void Present(double planPresentedAtReal, ContactPlan plan, double targetContactAtReal)
        {
            if(double.IsNaN(planPresentedAtReal)||double.IsInfinity(planPresentedAtReal)||
                double.IsNaN(targetContactAtReal)||double.IsInfinity(targetContactAtReal)||
                (entries.Count>0&&planPresentedAtReal<entries[entries.Count-1].Presented))
                throw new ArgumentOutOfRangeException(nameof(planPresentedAtReal));
            entries.Add(new Entry(planPresentedAtReal,plan,targetContactAtReal));
        }
        public bool TryVisibleAt(double releaseEventAtReal, out ContactPlan plan, out double targetContactAtReal)
            =>TryVisibleAt(releaseEventAtReal,out plan,out targetContactAtReal,out _);
        public bool TryVisibleAt(double releaseEventAtReal, out ContactPlan plan, out double targetContactAtReal, out double presentedAt)
        {
            for(int i=entries.Count-1;i>=0;i--)
                if(entries[i].Presented<=releaseEventAtReal)
                { var e=entries[i]; plan=e.Plan; targetContactAtReal=e.Target; presentedAt=e.Presented; return true; }
            plan=default;targetContactAtReal=presentedAt=0;return false;
        }
    }
    public static class ContactContract
    {
        public static ContactDecision Decide(double releaseTimeReal, PresentedPlans plans, ClockMap clock, Trajectory authoritative, TimingSettings s, bool attemptConsumed)
        {
            double corrected=releaseTimeReal-s.LatencyCompMs/1000;
            ContactPlan plan=default;double target=0,presented=0,delta=0;
            ContactDecision Reject(ShotResult result, FailKind fail=FailKind.None, string reason=null) =>
                new ContactDecision(result,fail,delta,-1,false,false,plan.Version,presented,target,releaseTimeReal,corrected,reason:reason);
            if(attemptConsumed) return Reject(ShotResult.AttemptConsumed,reason:"attempt-consumed");
            if(double.IsNaN(releaseTimeReal)) return Reject(ShotResult.NoOrder,reason:"no-ended");
            if(clock==null||!clock.TryRealToSim(corrected,out long step,out double fraction))
                return Reject(ShotResult.NotEvaluable,reason:"release-outside-clock-history");
            if(plans==null||!plans.TryVisibleAt(releaseTimeReal,out plan,out target,out presented))
                return Reject(ShotResult.NotEvaluable,reason:"no-presented-plan-at-event");
            delta=(corrected-target)*1000;
            if(!plan.Exists) return Reject(ShotResult.Unreachable,reason:plan.Reason??"no-opportunity");
            if(plan.TStarStep<0) return Reject(ShotResult.NotEvaluable,reason:"no-legal-quantized-step");
            double position=step+fraction;
            bool early=position<plan.KIn.Steps;
            if(early&&!s.EarlySoft) return Reject(ShotResult.Failed,FailKind.Air,"before-k-in");
            if(position>plan.KOut.Steps) return Reject(ShotResult.Failed,FailKind.Late,"after-k-out");
            if(!early&&!ContactPlanner.IsLegalAt(authoritative,new SimTime(step,fraction),plan.Body,plan.Legal))
            {
                // The upper boundary can be inclusive (reach/height) or exclusive (second bounce).
                if(position==plan.KOut.Steps) return Reject(ShotResult.Failed,FailKind.Late,"k-out-exclusive");
                return Reject(ShotResult.SystemFault,reason:"authoritative-geometry-diverged");
            }
            // Compare time boundaries themselves: subtraction can round an exact A/V just beyond the limit.
            bool Within(double ms)=>corrected>=target-ms/1000&&corrected<=target+ms/1000;
            if(s.ValidLimitOn&&!Within(s.ValidMs))
                return Reject(ShotResult.Failed,corrected<target?FailKind.Air:FailKind.Late,"outside-v");
            bool perfect=!early&&Within(s.AssistMs);
            bool scheduled=perfect&&corrected<=target;
            long contact;bool neighbor=false;double exact=corrected;
            bool Legal(long at)=>at>=plan.KIn.Steps&&at<=plan.KOut.Steps&&ContactPlanner.IsLegalStep(authoritative,at,plan.Body,plan.Legal);
            if(scheduled)
            {
                contact=plan.TStarStep;
                if(!Legal(contact))return Reject(ShotResult.SystemFault,reason:"scheduled-step-illegal");
                if(!clock.TrySimToReal(contact,0,out exact))return Reject(ShotResult.NotEvaluable,reason:"scheduled-time-unmapped");
            }
            else
            {
                double at=early?plan.KIn.Steps:position;
                if(early&&!clock.TrySimToReal(plan.KIn.Step,plan.KIn.Fraction,out exact))
                    return Reject(ShotResult.NotEvaluable,reason:"k-in-time-unmapped");
                long previous=(long)Math.Floor(at),next=previous+1;
                contact=at-previous<=.5?previous:next;
                if(!Legal(contact))
                {
                    long other=contact==previous?next:previous;
                    if(!Legal(other)||Math.Abs(other-at)>1) return Reject(ShotResult.NotEvaluable,reason:"no-legal-neighbor");
                    contact=other;neighbor=true;
                }
            }
            if(!clock.TrySimToReal(contact,0,out double physical)||!clock.TrySimToReal(contact+1,0,out double following))
                return Reject(ShotResult.NotEvaluable,reason:"physical-time-unmapped");
            return new ContactDecision(perfect?ShotResult.Perfect:ShotResult.Valid,FailKind.None,delta,contact,scheduled,early,
                plan.Version,presented,target,releaseTimeReal,corrected,(physical-exact)*1000,neighbor,(following-physical)*500>s.AssistMs/3);
        }
    }
}
