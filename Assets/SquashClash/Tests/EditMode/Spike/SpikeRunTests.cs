using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Core.Spike;
using UnityEngine;
namespace SquashClash.Core.Tests.Spike {
 public sealed class SpikeRunTests {
  private PhysicsParams p;private CourtDimensions d;
  [SetUp] public void Setup(){p=EffectiveConfig.From(Config.ConfigFixtures.Default()).Physics;d=CourtDimensions.Standard();}
  private SpikeRun Run(FakeFactory f)=>new SpikeRun(f,p,d,new SpikeOptions{RandomCount=32,Build="test",ConfigHash="test",Variant="fake"});
  [Test] public void Step_RespectsBudget_AndCompletesOverManyCalls(){
   var f=new FakeFactory{DelayMs=.1};var run=Run(f);int before=f.Resets;
   Assert.That(run.Step(2),Is.False);Assert.That(f.Resets-before,Is.LessThanOrEqualTo(25));
   int calls=1;while(!run.Step(2)&&calls++<1000){}Assert.That(run.Result.Complete,Is.True);Assert.That(calls,Is.GreaterThan(1));Assert.That(run.Progress,Is.EqualTo(1));
   Assert.That(run.Result.AllGatingPass,Is.False,"reduced random population cannot certify gate");
  }
  [Test] public void Cancel_ProducesIncompleteResult(){var f=new FakeFactory();var run=Run(f);run.Step(.001);run.Cancel();Assert.That(run.Step(2),Is.True);Assert.That(run.Result.Complete,Is.False);Assert.That(f.LiveInstances,Is.Zero);}
  [Test] public void Run_KeepsAtMostThreeLiveInstances_AndReleasesAll(){var f=new FakeFactory();var run=Run(f);while(!run.Step(1)){}Assert.That(f.Peak,Is.LessThanOrEqualTo(3));Assert.That(f.LiveInstances,Is.Zero);Assert.That(run.Result.Complete,Is.True);}
  [Test] public void Exception_ReleasesAllInstancesAndLeavesIncompleteResult(){var f=new FakeFactory{ThrowAtStep=true};var run=Run(f);Assert.Throws<InvalidOperationException>(()=>run.Step(1));Assert.That(run.Result.Complete,Is.False);Assert.That(f.LiveInstances,Is.Zero);}
  [Test] public void Benchmark_DiscardsWarmup(){var f=new FakeFactory{WarmupDelay=true};var run=new PredictionBenchmarkRun(f,p,d);while(!run.Step(2)){}Assert.That(f.Resets,Is.EqualTo(220));Assert.That(run.ResultMs.max,Is.LessThan(1));Assert.That(f.LiveInstances,Is.Zero);}
  [Test] public void Benchmark_CancelReleasesInstanceAndRejectsPartialStatistics(){var f=new FakeFactory();var run=new PredictionBenchmarkRun(f,p,d);run.Step(.001);run.Cancel();Assert.That(f.LiveInstances,Is.Zero);Assert.Throws<InvalidOperationException>(()=>{var stats=run.ResultMs;});}
  [Test] public void Reports_EnergyNotApplicableSeparately_AndAllEighteenRows(){var f=new FakeFactory();var run=Run(f);while(!run.Step(2)){}Assert.That(run.Result.Criteria.Count,Is.EqualTo(18));Assert.That(run.Result.Criteria.Find(c=>c.Id=="C3").Measured,Does.Contain("no aplica"));}
  [Test] public void Reports_InvalidEnergyIsExplicitInEachDiagnosticPopulation(){var f=new FakeFactory{InvalidState=true,TwoSurfaces=true};var run=Run(f);while(!run.Step(2)){}var row=run.Result.Criteria.Find(c=>c.Id=="C3");Assert.That(row.NotEvaluable,Is.GreaterThan(0));Assert.That(row.Measured,Does.Contain("NotEvaluable="));}
  [Test] public void Reports_C15IncludesBothDistinctSurfaceSteps(){var f=new FakeFactory{AlternateSurfaces=true};var run=Run(f);while(!run.Step(2)){}Assert.That(run.Result.Details,Has.Some.Contains("mismo paso=False"));}
  private sealed class FakeFactory:IBallSimulationFactory {
   public string Name=>"fake";public int LiveInstances{get;private set;}public int Peak,Resets;public double DelayMs;public bool WarmupDelay,ThrowAtStep,InvalidState,TwoSurfaces,AlternateSurfaces;
   public IBallSimulation Create(PhysicsParams p,CourtDimensions d){LiveInstances++;Peak=Math.Max(Peak,LiveInstances);return new Fake(this);}
   private sealed class Fake:IBallSimulation {
    private readonly FakeFactory owner;private BallState state;private bool disposed;private int steps;
    public Fake(FakeFactory owner){this.owner=owner;}
    public BallState State=>state;public BackendCapabilities Capabilities=>new BackendCapabilities{RunsInEditMode=true,Timing=ContactTiming.Exact,ImpactCapConfigurable=true};
    public void Reset(in BallState initial){state=initial;steps=0;int n=++owner.Resets;double delay=owner.WarmupDelay?(n<=20?5:.1):owner.DelayMs;var watch=Stopwatch.StartNew();while(watch.Elapsed.TotalMilliseconds<delay){}}
    public void Step(List<ContactEvent> contacts){if(owner.ThrowAtStep)throw new InvalidOperationException("fake fault");contacts.Clear();state.Step++;steps++;state.Position=new Vector3(3,1,5);state.Velocity=Vector3.down;
     contacts.Add(new ContactEvent(SurfaceId.Floor,new Vector3(3,0,5),new Vector2(3,5),Vector3.up,state.Step,.001f,ContactTiming.Exact,Vector3.down,false));
     if(owner.AlternateSurfaces && steps==2){contacts.Clear();contacts.Add(new ContactEvent(SurfaceId.FrontWall,Vector3.zero,Vector2.zero,Vector3.forward,state.Step,.001f,ContactTiming.Exact,Vector3.back,false));}
     if(owner.TwoSurfaces)contacts.Add(new ContactEvent(SurfaceId.FrontWall,Vector3.zero,Vector2.zero,Vector3.forward,state.Step,.001f,ContactTiming.Exact,Vector3.back,false));
     if(owner.InvalidState)state.Velocity=new Vector3(float.NaN,0,0);
    }
    public void Dispose(){if(disposed)return;disposed=true;owner.LiveInstances--;}
   }
  }
 }
}
