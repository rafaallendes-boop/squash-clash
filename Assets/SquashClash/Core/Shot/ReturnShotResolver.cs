using System;
using System.Globalization;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    public enum ReturnRuleState { Disabled, Preserved, Changed, NoSolution }
    public readonly struct ReturnRuleSettings
    {
        public readonly bool Enabled;
        public readonly float TinMargin;
        public readonly double SearchStep;
        public ReturnRuleSettings(bool enabled,float margin=.05f,double step=.05)
        {
            if(float.IsNaN(margin)||float.IsInfinity(margin)||margin<0)throw new ArgumentOutOfRangeException(nameof(margin));
            if(double.IsNaN(step)||double.IsInfinity(step)||step<.01)throw new ArgumentOutOfRangeException(nameof(step));
            Enabled=enabled;TinMargin=margin;SearchStep=step;
        }
        public static ReturnRuleSettings From(ParamSet p)=>new ReturnRuleSettings(p.GetBool("shot.returnEnabled"),(float)p.Get("shot.returnTinMargin"),p.Get("shot.returnSearchStep"));
    }
    public readonly struct ReturnResolution
    {
        public readonly ShotRequest OriginalRequest,AdjustedRequest;
        public readonly ShotSolution BaseSolution,Solution;
        public readonly ReturnRuleState State;
        public readonly int SolverCalls;
        public ReturnResolution(ShotRequest original,ShotRequest adjusted,ShotSolution baseline,ShotSolution solution,ReturnRuleState state,int calls)
        {OriginalRequest=original;AdjustedRequest=adjusted;BaseSolution=baseline;Solution=solution;State=state;SolverCalls=calls;}
    }
    // One-entry cache, owned by a reception. It compares bits, never Vector equality tolerance.
    public sealed class ReturnShotResolver
    {
        private readonly float min,max,cap;
        private readonly CourtDimensions court;
        private readonly PhysicsParams physics;
        private readonly IBallSimulationFactory factory;
        private readonly ReturnRuleSettings settings;
        private readonly ReturnFlightClassifier flight;
        private bool cached;private ShotRequest previous;private ReturnResolution previousResult;
        public ReturnShotResolver(float min,float max,float cap,CourtDimensions court,PhysicsParams physics,IBallSimulationFactory factory,ReturnRuleSettings settings)
        {
            this.min=min;this.max=max;this.cap=cap;this.settings=settings;this.factory=factory;
            this.court=new CourtDimensions{Width=court.Width,Length=court.Length,WallHeight=court.WallHeight,LineWidth=court.LineWidth,TinTop=court.TinTop,
                FrontOutLower=court.FrontOutLower,ServiceLower=court.ServiceLower,BackOutLower=court.BackOutLower,ShortLineNearBackEdgeFromBack=court.ShortLineNearBackEdgeFromBack,ServiceBoxInner=court.ServiceBoxInner};
            this.physics=new PhysicsParams{Gravity=physics.Gravity,StepHz=physics.StepHz,MaxImpactsPerStep=physics.MaxImpactsPerStep,BallRadius=physics.BallRadius,BallMass=physics.BallMass,
                Floor=Copy(physics.Floor),FrontWall=Copy(physics.FrontWall),SideWalls=Copy(physics.SideWalls),BackWall=Copy(physics.BackWall)};
            if(settings.Enabled)flight=new ReturnFlightClassifier(this.court,this.physics,factory);
        }
        private static SurfaceMaterial Copy(SurfaceMaterial s)=>new SurfaceMaterial{Restitution=s.Restitution,TangentialFriction=s.TangentialFriction};
        public ReturnResolution Resolve(ShotRequest request)
        {
            if(settings.Enabled&&cached&&Same(previous,request))
                return new ReturnResolution(previousResult.OriginalRequest,previousResult.AdjustedRequest,previousResult.BaseSolution,previousResult.Solution,previousResult.State,0);
            var result=Uncached(request);if(settings.Enabled){previous=request;previousResult=result;cached=true;}return result;
        }
        private ReturnResolution Uncached(ShotRequest request)
        {
            ShotSolution Solve(ShotRequest r)=>ShotSolver.Solve(r,min,max,cap,physics.Gravity,physics.BallRadius,court,factory,physics);
            var baseline=Solve(request);int calls=1;
            if(!settings.Enabled)return new ReturnResolution(request,request,baseline,baseline,ReturnRuleState.Disabled,calls);
            if(flight.Within(request.ContactCenter,baseline))return new ReturnResolution(request,request,baseline,baseline,ReturnRuleState.Preserved,calls);
            // Invalid input is not a missed return. Preserve the solver's existing rejection;
            // in particular do not search an unbounded/illegal front-wall request.
            if(!baseline.Solved&&(baseline.Reason=="invalid-request-or-physics"||baseline.Reason=="target-or-contact-outside-court"||baseline.Reason=="zero-horizontal-distance"))
                return new ReturnResolution(request,request,baseline,baseline,ReturnRuleState.NoSolution,calls);
            if(float.IsNaN(request.AimOnFrontWall.y)||float.IsInfinity(request.AimOnFrontWall.y))
                return new ReturnResolution(request,request,baseline,baseline,ReturnRuleState.NoSolution,calls);
            // R gives the canonical decimal for a float input (the approved grid uses these decimals).
            double origin=double.Parse(request.AimOnFrontWall.y.ToString("R",CultureInfo.InvariantCulture),CultureInfo.InvariantCulture);
            double limit=(double)court.TinTop+(double)settings.TinMargin;
            float last=request.AimOnFrontWall.y;
            for(int k=1;;k++)
            {
                double value=origin-k*settings.SearchStep;if(value<=limit)break;float y=(float)value;
                if(y<=limit||Same(y,last))continue;last=y;
                var adjusted=new ShotRequest(request.ContactCenter,new Vector2(request.AimOnFrontWall.x,y),request.P);
                var candidate=Solve(adjusted);calls++;
                if(flight.Within(request.ContactCenter,candidate))return new ReturnResolution(request,adjusted,baseline,candidate,ReturnRuleState.Changed,calls);
            }
            float endpoint=(float)limit;if(endpoint<=limit)endpoint=BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(endpoint)+1);
            if(endpoint<request.AimOnFrontWall.y&&!Same(endpoint,last))
            {
                var adjusted=new ShotRequest(request.ContactCenter,new Vector2(request.AimOnFrontWall.x,endpoint),request.P);
                var candidate=Solve(adjusted);calls++;
                if(flight.Within(request.ContactCenter,candidate))return new ReturnResolution(request,adjusted,baseline,candidate,ReturnRuleState.Changed,calls);
            }
            return new ReturnResolution(request,request,baseline,baseline,ReturnRuleState.NoSolution,calls);
        }
        private static bool Same(float a,float b)=>BitConverter.SingleToInt32Bits(a)==BitConverter.SingleToInt32Bits(b);
        private static bool Same(ShotRequest a,ShotRequest b)=>Same(a.ContactCenter.x,b.ContactCenter.x)&&Same(a.ContactCenter.y,b.ContactCenter.y)&&Same(a.ContactCenter.z,b.ContactCenter.z)&&Same(a.AimOnFrontWall.x,b.AimOnFrontWall.x)&&Same(a.AimOnFrontWall.y,b.AimOnFrontWall.y)&&Same(a.P,b.P);
    }
}
