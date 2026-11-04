using System;
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
        { Enabled=enabled;TinMargin=margin;SearchStep=step; }
        public static ReturnRuleSettings From(ParamSet p)=>new ReturnRuleSettings(false);
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
    public sealed class ReturnShotResolver
    {
        private readonly float min,max,cap;private readonly CourtDimensions court;private readonly PhysicsParams physics;private readonly IBallSimulationFactory factory;
        public ReturnShotResolver(float min,float max,float cap,CourtDimensions court,PhysicsParams physics,IBallSimulationFactory factory,ReturnRuleSettings settings)
        {this.min=min;this.max=max;this.cap=cap;this.court=court;this.physics=physics;this.factory=factory;}
        public ReturnResolution Resolve(ShotRequest request)
        {
            var s=ShotSolver.Solve(request,min,max,cap,physics.Gravity,physics.BallRadius,court,factory,physics);
            return new ReturnResolution(request,request,s,s,ReturnRuleState.Disabled,1);
        }
    }
}
