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
        public static CausalShotResult Solve(RallyContext context,CausalReference reference,BodyFrame body,Vector3 point,Vector2 aim,float power)
            =>CausalQuality.Solve(reference,body,point,aim,power,context.Config,context.Factory);
        public static ShotSolution Solve(RallyContext context,Vector3 contactCenter,Vector2 aim,float power)
            =>ShotSolver.Solve(new ShotRequest(contactCenter,aim,power),context.VMin,context.VMax,context.VCap,context.Physics.Gravity,context.Physics.BallRadius,
                context.Court,context.Factory,context.Physics);
        // What the shared solver and predictor say about one's own intention from a contact point: does it reach the
        // front wall legally and bounce in court? Own shot, shared predictor, observed state only (spec 11.2).
        public static ShotForecast Forecast(RallyContext context,Vector3 contactCenter,Vector2 aim,float power)
        {
            var solution=Solve(context,contactCenter,aim,power);
            if(!solution.Solved)return new ShotForecast(false,PointEndCause.None,default,0);
            var rules=new PointRules(context.Court,(float)context.Params.FootprintRadius);rules.Begin(PlayerId.Human,PlayerId.Rival);
            using(var simulation=context.Factory.Create(context.Physics,context.Court))
            {
                simulation.Reset(new BallState{Position=contactCenter,Velocity=solution.Velocity});var events=new List<ContactEvent>(4);
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
