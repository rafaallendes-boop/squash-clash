using System;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    // Two-wall preview (design 2026-11-24-two-wall-preview-hint): a forecast beside the stroke, never the stroke. From the planned contact
    // P*, the boast pace and the unchanged boast solver, without the causal layer (the same intention path the rival forecasts with,
    // RallyContact.SolveBoastIntention). It decides nothing: the authoritative solve stays in ShotController.Solve.
    public static class BoastPreview
    {
        public static ShotSolution Estimate(Vector3 contact,in SideAim side,float power,EffectiveConfig config,IBallSimulationFactory factory,float footprintRadius,out BoastInfo info)
        {
            if(config==null){info=default;return default;}
            var pace=ShotPaceSettings.From(config.Source);
            float speed=ShotPace.Boast(power,contact,side,config.Court,config.Physics.BallRadius,pace).Speed;
            return BoastSolver.Solve(contact,side,speed,pace.VMin,(float)config.Source.Get("shot.vCap"),config.Court,factory,config.Physics,footprintRadius,out info);
        }
    }
    // When the preview recomputes: at once for a new touch, contact or side point; afterwards at most once per interval of real time, and
    // only if the power moved. A finger held still costs nothing. Each solve keeps the solver's own fixed budget (BoastSolver.Budget).
    // The interval is a view parameter, not a game one: no DefaultPreset key and no hash.
    public sealed class BoastPreviewSchedule
    {
        public const double MinIntervalSeconds=.10;
        public int SolveCount {get;private set;}
        private bool has;private long launch;private Vector3 contact;private SideAim side;private float power;private double at;
        public bool ShouldSolve(double now,long launchId,Vector3 contactCenter,in SideAim aim,float p)
        {
            bool fresh=!has||launchId!=launch||contactCenter!=contact||!aim.Equals(side);
            if(!fresh&&(p==power||now<at+MinIntervalSeconds))return false;
            has=true;launch=launchId;contact=contactCenter;side=aim;power=p;at=now;SolveCount++;
            return true;
        }
        public void Reset()=>has=false;
    }
}
