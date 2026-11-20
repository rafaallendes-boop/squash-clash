using System.Globalization;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
namespace SquashClash.Core.Tests.Shot
{
    // VL1, the practice recipe of the AutoQA volley suite. Rule fixed before data (design 2026-11-18, step 8): both opportunities,
    // separation >= 0.25 s real between volley K-out and bounce K-in, volley K >= 0.1 s of simulation. The six SL recipes stay as they are.
    public sealed class VolleyRecipeTests
    {
        [Test] public void Vl1_OffersBothOpportunities_UnderThePredeclaredRule()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var p=new RallyParams();
            Assert.That(PracticeLaunchDefinitions.Create(cfg).Count,Is.EqualTo(6),"SL1-SL6 unchanged");
            var vl1=PracticeLaunchDefinitions.At(cfg,PracticeLaunchDefinitions.VolleyIndex);Assert.That(vl1.Id,Is.EqualTo("VL1"));Assert.That(vl1.Volley,Is.True);
            Trajectory t;using(var sim=new AnalyticBallSimulationFactory(0).Create(cfg.Physics,cfg.Court))t=TrajectoryRunner.Run(sim,vl1.Initial,cfg.Physics.StepHz*10,StopCondition.SecondFloorBounce,cfg.Court);
            var legal=new LegalSettings((float)cfg.Source.Get("legal.minHeight"),(float)cfg.Source.Get("legal.maxHeight"),cfg.Physics.StepHz,cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court);
            var bounce=ContactPlanner.Plan(t,vl1.Body,cfg.Court,legal,1);
            var settings=new VolleySettings(true,(float)p.VolleyMinHeight,(float)p.VolleyMaxHeight,p.VolleyMinSeparationS,p.VolleyMinWindowS);
            var volley=VolleyPlanner.Plan(t,_=>vl1.Body,cfg.Court,legal,settings,bounce,(long)System.Math.Round(p.ReactionS*cfg.Physics.StepHz),cfg.Tempo,1);
            Assert.That(bounce.Exists&&bounce.TStarStep>=0,Is.True,"bounce opportunity");
            Assert.That(volley.Exists,Is.True,"volley opportunity: "+volley.Reason);
            double realStep=1d/cfg.Physics.StepHz/cfg.Tempo;
            Assert.That((bounce.KIn.Steps-volley.Plan.KOut.Steps)*realStep,Is.GreaterThanOrEqualTo(.25));
            Assert.That((volley.Plan.KOut.Steps-volley.Plan.KIn.Steps)/cfg.Physics.StepHz,Is.GreaterThanOrEqualTo(.1));
            var anchor=ConfigFixtures.Default().With("practice.launch",PracticeLaunchDefinitions.VolleyIndex).With("human.x",(double)vl1.Body.PositionXZ.x).With("human.z",(double)vl1.Body.PositionXZ.y);
            var c=CultureInfo.InvariantCulture;
            TestContext.WriteLine("VL1 hash="+anchor.Hash+" volley kIn="+volley.Plan.KIn.Steps.ToString("R",c)+" kOut="+volley.Plan.KOut.Steps.ToString("R",c)+" tStar="+volley.Plan.TStarStep+
                " y="+volley.Plan.PStar.y.ToString("R",c)+" split="+volley.SplitSteps.ToString("R",c)+" bounce kIn="+bounce.KIn.Steps.ToString("R",c)+" kOut="+bounce.KOut.Steps.ToString("R",c)+" tStar="+bounce.TStarStep+
                " realStepMs="+(realStep*1000).ToString("R",c));
        }
    }
}
