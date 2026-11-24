using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    public readonly struct ShotForecast
    {
        public readonly bool Solved;public readonly PointEndCause Fault;public readonly Vector3 FirstBounce;public readonly double SecondsToBounce;
        public ShotForecast(bool solved,PointEndCause fault,Vector3 firstBounce,double seconds){Solved=solved;Fault=fault;FirstBounce=firstBounce;SecondsToBounce=seconds;}
        public bool Good=>Solved&&Fault==PointEndCause.None;
    }
    // The one path a non-touch player uses to hit: the same ContactContract and ShotSolver as the human (spec 17.A.9).
    public static class RallyContact
    {
        // deltaMs is the release offset from the T* the player was shown, in real perceived milliseconds.
        public static ContactDecision Resolve(RallyContext context,ClockMap clock,Trajectory trajectory,ContactPlan plan,double presentedAtReal,double deltaMs,double earlyAllowanceMs=0)
        {
            if(context==null||clock==null)throw new ArgumentNullException(nameof(context));
            var timing=context.Timing;double target=0;
            if(plan.Exists&&plan.TStarStep>=0&&!clock.TrySimToReal(plan.TStarStep,0,out target))
                return new ContactDecision(ShotResult.NotEvaluable,FailKind.None,0,-1,false,false,plan.Version,presentedAtReal,0,0,0,reason:"target-time-unmapped");
            var plans=new PresentedPlans();plans.Present(presentedAtReal,plan,target);
            // The contract subtracts the device offset from every release; a synthetic release adds it back.
            double release=Math.Max(presentedAtReal,target+deltaMs/1000+timing.LatencyCompMs/1000);
            return ContactContract.Decide(release,plans,clock,trajectory,timing,false,earlyAllowanceMs);
        }
        // A release aimed at the volley's T* (deltaMs from it), judged by OpportunityContract.Select exactly like the human's: at or
        // before S against the volley, later against the bounce. usedVolley tells which plan the decision belongs to.
        public static ContactDecision ResolveAimedVolley(RallyContext context,ClockMap clock,Trajectory trajectory,ContactPlan bounce,in VolleyOpportunity volley,double presentedAtReal,double deltaMs,out bool usedVolley,double earlyAllowanceMs=0)
        {
            if(context==null||clock==null)throw new ArgumentNullException(nameof(context));
            usedVolley=false;var timing=context.Timing;var v=volley.Plan;
            if(!volley.Exists)return Resolve(context,clock,trajectory,bounce,presentedAtReal,deltaMs,earlyAllowanceMs);
            if(!clock.TrySimToReal(v.TStarStep,0,out double target))
                return new ContactDecision(ShotResult.NotEvaluable,FailKind.None,0,-1,false,false,v.Version,presentedAtReal,0,0,0,reason:"target-time-unmapped");
            double release=Math.Max(presentedAtReal,target+deltaMs/1000+timing.LatencyCompMs/1000);
            usedVolley=OpportunityContract.Select(release,clock,timing,volley)==Opportunity.Volley;
            var plan=usedVolley?v:bounce;var plans=new PresentedPlans();
            double planTarget=target;
            if(!usedVolley&&plan.Exists&&plan.TStarStep>=0&&!clock.TrySimToReal(plan.TStarStep,0,out planTarget))
                return new ContactDecision(ShotResult.NotEvaluable,FailKind.None,0,-1,false,false,plan.Version,presentedAtReal,0,0,0,reason:"target-time-unmapped");
            plans.Present(presentedAtReal,plan,planTarget);
            return ContactContract.Decide(release,plans,clock,trajectory,timing,false,earlyAllowanceMs);
        }
        public static CausalShotResult Solve(RallyContext context,CausalReference reference,BodyFrame body,Vector3 point,Vector2 aim,float power)
            =>Solve(context,reference,body,point,aim,power,FatigueArm.None);
        public static CausalShotResult Solve(RallyContext context,CausalReference reference,BodyFrame body,Vector3 point,Vector2 aim,float power,in FatigueArm arm)
            =>CausalQuality.Solve(reference,body,point,aim,power,context.Config,context.Factory,arm,(float)context.Params.FootprintRadius);
        // The one execution path for a decided shot: a boast's side-wall point goes to the two-wall solve, everything else to Solve above.
        public static CausalShotResult Solve(RallyContext context,CausalReference reference,BodyFrame body,Vector3 point,in AiShot shot,in FatigueArm arm)
            =>shot.Side.Exists?CausalQuality.SolveBoast(reference,body,point,shot.Side,shot.P,context.Config,context.Factory,arm,(float)context.Params.FootprintRadius,out _)
                :Solve(context,reference,body,point,shot.Aim,shot.P,arm);
        // Own two-wall intention from a contact point: the same boast pace as the executed shot, without causal vectors.
        public static ShotSolution SolveBoastIntention(RallyContext context,Vector3 contactCenter,in SideAim side,float power,out BoastInfo info)
        {
            var pace=ShotPace.Boast(power,contactCenter,side,context.Court,context.Physics.BallRadius,context.Pace);
            return BoastSolver.Solve(contactCenter,side,pace.Speed,context.VMin,context.VCap,context.Court,context.Factory,context.Physics,(float)context.Params.FootprintRadius,out info);
        }
        // Forecast of an own boast: solved, then the same flight check as Forecast below.
        public static ShotForecast ForecastBoast(RallyContext context,Vector3 contactCenter,in SideAim side,float power)
        {
            var solution=SolveBoastIntention(context,contactCenter,side,power,out _);
            return solution.Solved?Fly(context,contactCenter,solution.Velocity):new ShotForecast(false,PointEndCause.None,default,0);
        }
        // Own intention from a contact point: the same pace curve as the human shot (without the causal layer), then the shared intention and solver.
        // Equal speed endpoints carry the curve speed unchanged, as CausalQuality.Solve does.
        public static ShotSolution SolveIntention(RallyContext context,Vector3 contactCenter,Vector2 aim,float power)
        {
            var pace=ShotPace.Requested(power,contactCenter,aim,context.Physics.BallRadius,context.Pace);
            // The same intention layer as the executed shot (G1 A2), without causal vectors.
            return ShotIntentSolver.Solve(new ShotRequest(contactCenter,aim,power),aim,pace.Speed,context.VCap,context.Pace,
                context.Court,context.Factory,context.Physics,(float)context.Params.FootprintRadius,out _);
        }
        // What the shared solver and predictor say about one's own intention from a contact point: does it reach the
        // front wall legally and bounce in court? Own shot, shared predictor, observed state only (spec 11.2).
        public static ShotForecast Forecast(RallyContext context,Vector3 contactCenter,Vector2 aim,float power)
        {
            var solution=SolveIntention(context,contactCenter,aim,power);
            if(!solution.Solved)return new ShotForecast(false,PointEndCause.None,default,0);
            return Fly(context,contactCenter,solution.Velocity);
        }
        private static ShotForecast Fly(RallyContext context,Vector3 contactCenter,Vector3 velocity)
        {
            var rules=new PointRules(context.Court,(float)context.Params.FootprintRadius);rules.Begin(PlayerId.Human,PlayerId.Rival);
            using(var simulation=context.Factory.Create(context.Physics,context.Court))
            {
                simulation.Reset(new BallState{Position=contactCenter,Velocity=velocity});var events=new List<ContactEvent>(4);
                int limit=context.StepHz*8;
                for(int step=1;step<=limit;step++)
                {
                    events.Clear();simulation.Step(events);
                    foreach(var e in events)
                    {
                        rules.OnBallContact(e);
                        if(rules.Closed)return new ShotForecast(true,rules.Cause,e.Point,step/(double)context.StepHz);
                        if(rules.FloorBounces==1)return new ShotForecast(true,PointEndCause.None,e.Point,step/(double)context.StepHz);
                    }
                    if(simulation.State.Position.y>context.Court.WallHeight)return new ShotForecast(true,PointEndCause.Out,simulation.State.Position,step/(double)context.StepHz);
                }
            }
            return new ShotForecast(true,PointEndCause.NotUp,default,8);
        }
    }
}
