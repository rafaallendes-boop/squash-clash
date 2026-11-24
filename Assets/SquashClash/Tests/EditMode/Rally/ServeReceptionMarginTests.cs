using System;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class ServeReceptionMarginTests
    {
        [Test] public void DefaultMargin_IsChosenValue_AndItsProjectionPreservesEveryPriorParameter()
        {
            Assert.That(new RallyParams().HumanServeEarlyMs,Is.EqualTo(50d));
            Assert.That(RivalAiTests.Projection(f=>f.Name!="HumanServeEarlyMs"),Is.EqualTo("1063d0f6cf66"));
        }
        [TestCase(25d)] [TestCase(50d)]
        public void Context_SnapshotsMargin_AndGrantsItOnlyToTheHumanAtTheRivalServeLaunch(double margin)
        {
            var parameters=new RallyParams{HumanServeEarlyMs=margin};var ctx=RallyFixtures.Context(parameters);
            parameters.HumanServeEarlyMs=0;
            Assert.That(ctx.HumanServeEarlyMs,Is.EqualTo(margin));
            var rules=ctx.NewRules();rules.BeginServe(PlayerId.Rival,ServiceBox.Right);
            Assert.That(ctx.ServeReceptionEarlyMs(PlayerId.Human,rules),Is.EqualTo(margin));
            Assert.That(ctx.ServeReceptionEarlyMs(PlayerId.Rival,rules),Is.Zero);
            rules.BeginServe(PlayerId.Human,ServiceBox.Left);
            Assert.That(ctx.ServeReceptionEarlyMs(PlayerId.Human,rules),Is.Zero);
            Assert.That(ctx.ServeReceptionEarlyMs(PlayerId.Rival,rules),Is.Zero);
            rules.Begin(null,PlayerId.Human);Assert.That(ctx.ServeReceptionEarlyMs(PlayerId.Human,rules),Is.Zero);
            rules.Begin(PlayerId.Rival,PlayerId.Human);Assert.That(ctx.ServeReceptionEarlyMs(PlayerId.Human,rules),Is.Zero,"a non-serve feed is not a return of serve");
        }
        [TestCase(-1d)] [TestCase(51d)] [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)]
        public void Context_RejectsAnInvalidMargin(double margin)
        {Assert.Throws<ArgumentOutOfRangeException>(()=>RallyFixtures.Context(new RallyParams{HumanServeEarlyMs=margin}));}
        private sealed class EarlyBot : IRallyBrain
        {
            private readonly RallyContext ctx;
            public EarlyBot(RallyContext ctx){this.ctx=ctx;}
            public int PerceptionSteps=>0;
            public Vector2 StanceNoise()=>Vector2.zero;
            public AiShot Decide(in AiObservation observation,in ReceptionPlan own)
            {
                if(!own.Reachable||!own.Contact.Exists||own.Contact.TStarStep<0)return default;
                var at=own.Contact.PStar;var options=ShotOptions.Build(ctx,at);
                var selected=options.FirstOrDefault(o=>RallyContact.Forecast(ctx,at,o.Aim,o.P).Good);
                double delta=(own.Contact.KIn.Steps-own.Contact.TStarStep)*1000/(ctx.StepHz*ctx.Tempo)-12.5;
                return new AiShot(true,selected,selected.Aim,selected.P,delta,false);
            }
        }
        [TestCase(0d,ReceptionResult.Air)] [TestCase(25d,ReceptionResult.Contact)] [TestCase(50d,ReceptionResult.Contact)]
        public void Sim_HumanEarlyReturnOfServe_UsesTheBoundedAllowance(double margin,ReceptionResult expected)
        {
            var ctx=RallyFixtures.Context(new RallyParams{HumanServeEarlyMs=margin});
            using(var sim=new RallySim(ctx,new EarlyBot(ctx),new BotBrain(ctx,0,5)))
            {
                var point=sim.PlayPoint(PlayerId.Rival,ServiceBox.Right);
                Assert.That(point.Receptions.First().Who,Is.EqualTo(PlayerId.Human));
                Assert.That(point.Receptions.First().Result,Is.EqualTo(expected));
            }
        }
        [TestCase(0d)] [TestCase(25d)] [TestCase(50d)]
        public void Sim_RivalEarlyReturnOfTheHumanServe_IsStillAir(double margin)
        {
            var ctx=RallyFixtures.Context(new RallyParams{HumanServeEarlyMs=margin});
            using(var sim=new RallySim(ctx,new BotBrain(ctx,0,5),new EarlyBot(ctx)))
            {
                var point=sim.PlayPoint(PlayerId.Human,ServiceBox.Right);
                Assert.That(point.Receptions.First().Who,Is.EqualTo(PlayerId.Rival));
                Assert.That(point.Receptions.First().Result,Is.EqualTo(ReceptionResult.Air));
            }
        }
        [Test] public void Sim_HumanEarlyRallyReturn_HasNoAllowance()
        {
            var ctx=RallyFixtures.Context(new RallyParams{HumanServeEarlyMs=25});
            using(var sim=new RallySim(ctx,new EarlyBot(ctx),new BotBrain(ctx,0,5)))
            {
                var point=sim.PlayPoint(PlayerId.Human,ServiceBox.Right);
                var reception=point.Receptions.First(r=>r.Who==PlayerId.Human);
                Assert.That(reception.Result,Is.EqualTo(ReceptionResult.Air));
            }
        }
    }
}
