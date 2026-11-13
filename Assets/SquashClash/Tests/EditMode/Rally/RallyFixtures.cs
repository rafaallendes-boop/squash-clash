using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    internal static class RallyFixtures
    {
        internal static RallyContext Context(RallyParams parameters=null)
            =>new RallyContext(EffectiveConfig.From(ConfigFixtures.Default()),parameters??new RallyParams(),new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
        internal static PointRules Rules(RallyContext context,PlayerId? striker,PlayerId receiver)
        {var rules=new PointRules(context.Court,(float)context.Params.FootprintRadius);rules.Begin(striker,receiver);return rules;}
        internal static PlayerBody Body(RallyContext context,PlayerId id,Vector2 at)=>new PlayerBody(id,at,context.Move);
        // Slow low ball that dies near the front-right corner.
        internal static BallState ShortBall(RallyContext context)
        {
            var from=new Vector3(1.2f,.6f,2.2f);
            var center=Ballistics.CenterForContact(Core.Court.SurfaceId.FrontWall,new Vector3(.8f,.75f,0),context.Physics.BallRadius);
            return new BallState{Position=from,Velocity=Ballistics.SolveCenterTo(from,center,5,context.Physics.Gravity)};
        }
    }
}
