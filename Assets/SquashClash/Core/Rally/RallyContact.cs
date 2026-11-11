using System;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    // The one path a non-touch player uses to hit: the same ContactContract and ShotSolver as the human (spec 17.A.9).
    public static class RallyContact
    {
        // deltaMs is the release offset from the T* the player was shown, in real perceived milliseconds.
        public static ContactDecision Resolve(RallyContext context,ClockMap clock,Trajectory trajectory,ContactPlan plan,double presentedAtReal,double deltaMs)
        {
            if(context==null||clock==null)throw new ArgumentNullException(nameof(context));
            var timing=context.Timing;double target=0;
            if(plan.Exists&&plan.TStarStep>=0&&!clock.TrySimToReal(plan.TStarStep,0,out target))
                return new ContactDecision(ShotResult.NotEvaluable,FailKind.None,0,-1,false,false,plan.Version,presentedAtReal,0,0,0,reason:"target-time-unmapped");
            var plans=new PresentedPlans();plans.Present(presentedAtReal,plan,target);
            // The contract subtracts the device offset from every release; a synthetic release adds it back.
            double release=Math.Max(presentedAtReal,target+deltaMs/1000+timing.LatencyCompMs/1000);
            return ContactContract.Decide(release,plans,clock,trajectory,timing,false);
        }
        public static ShotSolution Solve(RallyContext context,Vector3 contactCenter,Vector2 aim,float power)
            =>ShotSolver.Solve(new ShotRequest(contactCenter,aim,power),context.VMin,context.VMax,context.VCap,context.Physics.Gravity,context.Physics.BallRadius,
                context.Court,context.Factory,context.Physics);
    }
}
