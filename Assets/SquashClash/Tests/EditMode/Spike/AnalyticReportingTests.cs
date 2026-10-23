using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Core.Spike;
using UnityEngine;
namespace SquashClash.Core.Tests.Spike
{
    public sealed class AnalyticReportingTests
    {
        private static EffectiveConfig Config()=>EffectiveConfig.From(Core.Tests.Config.ConfigFixtures.Default());
        [Test] public void FactoryCapCounter_SurvivesResetAndDisposedInstances()
        {
            var cfg=Config();var p=cfg.Physics;p.MaxImpactsPerStep=1;var f=new AnalyticBallSimulationFactory(.1f);
            var from=new BallState {Position=new Vector3(.07f,2,5),Velocity=new Vector3(-10,0,0)};
            using(var sim=f.Create(p,cfg.Court)) {sim.Reset(from);sim.Step(new List<ContactEvent>());Assert.That(f.ImpactCapReachedCount,Is.EqualTo(1));sim.Reset(from);sim.Step(new List<ContactEvent>());Assert.That(f.ImpactCapReachedCount,Is.EqualTo(2));}
            Assert.That(f.LiveInstances,Is.Zero);Assert.That(f.ImpactCapReachedCount,Is.EqualTo(2));
        }
        [Test] public void C14_ReportsConfiguredCapAndEveryReachedCapInCampaign()
        {
            var cfg=Config();var p=cfg.Physics;p.MaxImpactsPerStep=1;var f=new AnalyticBallSimulationFactory(.1f);
            var run=new SpikeRun(f,p,cfg.Court,new SpikeOptions {RandomCount=0,Variant="test-P2"});
            try {while(!run.Step(8)){}var text=run.Result.Criteria.Single(c=>c.Id=="C14").Measured;Assert.That(f.ImpactCapReachedCount,Is.GreaterThan(0));Assert.That(text,Does.Contain("maxImpactsPerStep=1"));Assert.That(text,Does.Contain("veces que se alcanzó el tope="+f.ImpactCapReachedCount));Assert.That(text,Does.Not.Contain("no operativo"));}
            finally {run.Cancel();}Assert.That(f.LiveInstances,Is.Zero);
        }
        [Test] public void Benchmark_PreservesMeasuredSamplesInAcquisitionOrderAndRejectsReducedCoverage()
        {
            var cfg=Config();var benchmark=new PredictionBenchmarkRun(new AnalyticBallSimulationFactory(.1f),cfg.Physics,cfg.Court,2,3);
            try {
                while(!benchmark.Step(8)){}var result=new SpikeResult {Criteria=SpikeCriteria.CreateResults(true)};benchmark.ApplyTo(result);benchmark.ApplyTo(result);
                Assert.That(result.PredictionSamplesMs.Count,Is.EqualTo(3));Assert.That(result.PredictionSamplesMs.All(x=>x>0&&!double.IsNaN(x)&&!double.IsInfinity(x)),Is.True);
                Assert.That(result.PredictionSamplesMs.Max(),Is.EqualTo(benchmark.ResultMs.p95));Assert.That(result.Criteria.Single(c=>c.Id=="C17").Verdict,Is.EqualTo(Verdict.NotEvaluated));
            } finally {benchmark.Cancel();}
        }
        [Test] public void Report_PersistsAll200IndividualSamplesWithRoundTripPrecision()
        {
            var result=new SpikeResult {Device="editor",Criteria=SpikeCriteria.CreateResults(false)};
            for(int i=0;i<200;i++)result.PredictionSamplesMs.Add((201-i)/7.0);
            string report=SpikeReportWriter.ToMarkdown(result);var lines=report.Split('\n').Where(x=>x.StartsWith("- C17 muestra ")).ToArray();Assert.That(lines.Length,Is.EqualTo(200));
            for(int i=0;i<200;i++){string expected=result.PredictionSamplesMs[i].ToString("R",System.Globalization.CultureInfo.InvariantCulture);Assert.That(lines[i],Is.EqualTo($"- C17 muestra {i+1:D3}: {expected} ms"));}
        }
    }
}
