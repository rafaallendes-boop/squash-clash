using SquashClash.Core.Simulation;
namespace SquashClash.Core.Shot
{
    public enum Opportunity { Bounce, Volley }
    // Spec 7.1.3 case 14: choose the opportunity whose segment holds the release, forwarding its per-reception allowance.
    // Selection itself is unchanged; without a volley this reads the bounce plans.
    public static class OpportunityContract
    {
        // The corrected release, in simulation steps, at or before S chooses the volley; anything else (later, unmapped) the bounce.
        public static Opportunity Select(double releaseTimeReal,ClockMap clock,TimingSettings s,in VolleyOpportunity volley)
        {
            if(!volley.Exists||clock==null||double.IsNaN(releaseTimeReal))return Opportunity.Bounce;
            if(!clock.TryRealToSim(releaseTimeReal-s.LatencyCompMs/1000,out long step,out double fraction))return Opportunity.Bounce;
            return step+fraction<=volley.SplitSteps?Opportunity.Volley:Opportunity.Bounce;
        }
        public static ContactDecision Decide(double releaseTimeReal,PresentedPlans bounce,PresentedPlans volleyPlans,in VolleyOpportunity volley,
            ClockMap clock,Trajectory authoritative,TimingSettings s,bool attemptConsumed,out Opportunity used,double earlyAllowanceMs=0)
        {
            used=volleyPlans==null?Opportunity.Bounce:Select(releaseTimeReal,clock,s,volley);
            return ContactContract.Decide(releaseTimeReal,used==Opportunity.Volley?volleyPlans:bounce,clock,authoritative,s,attemptConsumed,earlyAllowanceMs);
        }
    }
}
