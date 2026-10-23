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
  [TestCase(false)][TestCase(true)] public void Reports_C13MedianUsesConventionalEvenAndOddSamples(bool odd){
   var f=new FakeFactory{PositiveVelocities=true,FirstRatioUnavailable=odd};var run=Run(f);while(!run.Step(2)){}
   var row=run.Result.Criteria.Find(c=>c.Id=="C13");
   double low=(1-p.BallRadius)/(4*(p.StepSeconds-.001f)), high=(1-p.BallRadius)/(2*(p.StepSeconds-.001f));
   var match=System.Text.RegularExpressions.Regex.Match(row.Measured,@"mediana=([0-9.Ee+\-]+)");Assert.That(match.Success,Is.True);
   double actual=double.Parse(match.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture);
   Assert.That(actual,Is.EqualTo(odd?low:(low+high)/2).Within(.001));
   Assert.That(row.Evaluated%2,Is.EqualTo(odd?1:0));
  }
  [Test] public void C7_AbsentAfterCleanOverTop_IsNotApplicableOutsidePopulation(){
   var run=Run(new FakeFactory{OverTop=true,NoContacts=true});while(!run.Step(2)){}
   var c=run.Result.Criteria.Find(v=>v.Id=="C7");Assert.That(c.Expected,Is.Zero);Assert.That(c.NotEvaluable,Is.Zero);
   Assert.That(c.Measured,Does.Contain("Measured=0; NotApplicable=96; NotEvaluable=0"));Assert.That(c.Verdict,Is.EqualTo(Verdict.NotEvaluated),"zero eligible samples cannot approve C7");
  }
  [Test] public void C7_NoFirstContactAtCleanHorizon_IsNotApplicable_AndK30StillMeasured(){
   var run=Run(new FakeFactory{NoContacts=true});while(!run.Step(2)){}
   var c=run.Result.Criteria.Find(v=>v.Id=="C7");Assert.That(c.Expected,Is.EqualTo(48));Assert.That(c.Evaluated,Is.EqualTo(48));Assert.That(c.NotEvaluable,Is.Zero);
   Assert.That(c.Measured,Does.Contain("Measured=48; NotApplicable=48; NotEvaluable=0"));
  }
  [TestCase("nonfinite")][TestCase("crossing")][TestCase("steps")][TestCase("contact")]
  public void C7_AbsentAfterAnomaly_RemainsNotEvaluable(string fault){
   var run=Run(new FakeFactory{OverTop=true,NoContacts=fault!="contact",InvalidState=fault=="nonfinite",CrossesWall=fault=="crossing",BadSteps=fault=="steps",InvalidContact=fault=="contact"});while(!run.Step(2)){}
   var c=run.Result.Criteria.Find(v=>v.Id=="C7");Assert.That(c.NotEvaluable,Is.EqualTo(96));Assert.That(c.Expected,Is.EqualTo(96));
   Assert.That(c.Measured,Does.Contain("Measured=0; NotApplicable=0; NotEvaluable=96"));Assert.That(c.Verdict,Is.EqualTo(Verdict.NotEvaluated));
  }
  [Test] public void C7_EarlyOtherTerminationIsNotSilentlyExcluded(){
   var run=Run(new FakeFactory());while(!run.Step(2)){}var c=run.Result.Criteria.Find(v=>v.Id=="C7");
   Assert.That(c.Expected,Is.EqualTo(96));Assert.That(c.NotEvaluable,Is.EqualTo(16));Assert.That(c.Measured,Does.Contain("Measured=80; NotApplicable=0; NotEvaluable=16"));
  }
  [TestCase("C5")][TestCase("C6")][TestCase("C7")]
  public void Divergence_ReportsFirstStepStatesBitsAndContacts(string id){
   var run=Run(new FakeFactory{DivergentResets=true});while(!run.Step(2)){}
   string detail=run.Result.Details.Find(v=>v.StartsWith(id+" diagnóstico:"));Assert.That(detail,Is.Not.Null);
   foreach(var field in new[]{"primer paso divergente=","índice=1","previo A=","A=","B=","bits=","contactos A=","contactos B="})Assert.That(detail,Does.Contain(field));
  }
  [TestCase("C5",false)][TestCase("C6",false)][TestCase("C7",false)]
  [TestCase("C5",true)][TestCase("C6",true)][TestCase("C7",true)]
  public void Divergence_ClassifiesEveryFailure_WithoutCappingAtTwelve(string id,bool orderOnly){
   var run=Run(new FakeFactory{DivergentResets=!orderOnly,ReverseContactOrder=orderOnly});while(!run.Step(2)){}
   var row=run.Result.Criteria.Find(v=>v.Id==id);
   int failures=int.Parse(System.Text.RegularExpressions.Regex.Match(row.Measured,@"fallos=(\d+)").Groups[1].Value);
   var records=run.Result.Details.FindAll(v=>v.StartsWith(id+" clasificación:"));
   Assert.That(failures,Is.GreaterThan(0));Assert.That(records.Count,Is.EqualTo(failures));
   foreach(var record in records)Assert.That(record,Does.Contain(orderOnly?"solo orden de contactos":"estado/contactos distintos"));
   Assert.That(row.Verdict,Is.EqualTo(Verdict.Fail),"classification never changes the criterion");
  }
  [Test] public void C16_ReportsProvenance_WithoutChangingMaximum(){
   var run=Run(new FakeFactory{PenetratesFloor=true});while(!run.Step(2)){}
   var row=run.Result.Criteria.Find(v=>v.Id=="C16");
   Assert.That(row.Measured,Does.Contain("penetración máxima=10"));
   Assert.That(run.Result.Details,Has.Some.Contains("C16 máximo: S1 robustez; paso=1; superficie=Floor; tipo=impacto"));
  }
  private sealed class FakeFactory:IBallSimulationFactory {
   public string Name=>"fake";public int LiveInstances{get;private set;}public int Peak,Resets;public double DelayMs;public bool WarmupDelay,ThrowAtStep,InvalidState,TwoSurfaces,AlternateSurfaces,PositiveVelocities,FirstRatioUnavailable,OverTop,NoContacts,CrossesWall,BadSteps,InvalidContact,DivergentResets,ReverseContactOrder,PenetratesFloor;
   public IBallSimulation Create(PhysicsParams p,CourtDimensions d){LiveInstances++;Peak=Math.Max(Peak,LiveInstances);return new Fake(this);}
   private sealed class Fake:IBallSimulation {
    private readonly FakeFactory owner;private BallState state;private bool disposed;private int steps,lastReset;
    public Fake(FakeFactory owner){this.owner=owner;}
    public BallState State=>state;public BackendCapabilities Capabilities=>new BackendCapabilities{RunsInEditMode=true,Timing=ContactTiming.Exact,ImpactCapConfigurable=true};
    public void Reset(in BallState initial){state=initial;steps=0;int n=++owner.Resets;lastReset=n;double delay=owner.WarmupDelay?(n<=20?5:.1):owner.DelayMs;var watch=Stopwatch.StartNew();while(watch.Elapsed.TotalMilliseconds<delay){}}
    public void Step(List<ContactEvent> contacts){if(owner.ThrowAtStep)throw new InvalidOperationException("fake fault");contacts.Clear();state.Step++;steps++;state.Position=new Vector3(3,1,5);state.Velocity=Vector3.down;
     contacts.Add(new ContactEvent(SurfaceId.Floor,new Vector3(3,0,5),new Vector2(3,5),Vector3.up,state.Step,.001f,ContactTiming.Exact,Vector3.down,false));
     if(owner.AlternateSurfaces && steps==2){contacts.Clear();contacts.Add(new ContactEvent(SurfaceId.FrontWall,Vector3.zero,Vector2.zero,Vector3.forward,state.Step,.001f,ContactTiming.Exact,Vector3.back,false));}
     if(owner.TwoSurfaces)contacts.Add(new ContactEvent(SurfaceId.FrontWall,Vector3.zero,Vector2.zero,Vector3.forward,state.Step,.001f,ContactTiming.Exact,Vector3.back,false));
     if(owner.PositiveVelocities)state.Velocity=Vector3.up*(steps==1?2:4);
     if(owner.FirstRatioUnavailable&&owner.Resets==1&&steps==1)state.Velocity=Vector3.down;
     if(owner.DivergentResets)state.Position.z=lastReset%2==0?5.000001f:5f;
     if(owner.ReverseContactOrder){contacts.Add(new ContactEvent(SurfaceId.LeftWall,Vector3.zero,Vector2.zero,Vector3.left,state.Step,float.NaN,ContactTiming.StepOnly,Vector3.right,false));if(lastReset%2==0)contacts.Reverse();}
     if(owner.PenetratesFloor)state.Position.y=.01f;
     if(owner.OverTop)state.Position=new Vector3(3,7,5);
     if(owner.CrossesWall&&steps==1)state.Position=new Vector3(-1,1,5);
     if(owner.BadSteps)state.Step++;
     if(owner.NoContacts)contacts.Clear();
     if(owner.InvalidContact){contacts.Clear();contacts.Add(new ContactEvent(SurfaceId.Floor,new Vector3(float.NaN,0,0),Vector2.zero,Vector3.up,state.Step,.001f,ContactTiming.Exact,Vector3.down,false));}
     if(owner.InvalidState)state.Velocity=new Vector3(float.NaN,0,0);
    }
    public void Dispose(){if(disposed)return;disposed=true;owner.LiveInstances--;}
   }
  }
 }
}
