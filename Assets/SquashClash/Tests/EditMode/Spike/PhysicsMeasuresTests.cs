using System;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Core.Spike;
using UnityEngine;
namespace SquashClash.Core.Tests.Spike {
 public sealed class PhysicsMeasuresTests {
  private static BallState State(long step,Vector3 position,Vector3 velocity=default)=>new BallState{Step=step,Position=position,Velocity=velocity};
  private static Trajectory Path(params BallState[] states) {var t=new Trajectory();t.States.AddRange(states);return t;}
  private static ContactEvent Contact(SurfaceId face,long step,bool persistent=false,float time=.004f,ContactTiming timing=ContactTiming.Exact,Vector3? point=null,float speed=1)=>new ContactEvent(face,point??Vector3.zero,Vector2.zero,CourtDimensions.InwardNormal(face),step,time,timing,-CourtDimensions.InwardNormal(face)*speed,persistent);
  private static PhysicsParams Params()=>new PhysicsParams{Gravity=10,BallMass=1,BallRadius=.02f,StepHz=100};
  [Test] public void Crossings_DetectsCenterBeyondFrontWallEvenWithEvent() {
   var t=Path(State(0,new Vector3(3,1,.5f)),State(1,new Vector3(3,1,-.3f)));
   Assert.That(PhysicsMeasures.Crossings(t,CourtDimensions.Standard(),out var detail),Is.EqualTo(1));Assert.That(detail,Does.Contain("FrontWall"));
   t.Contacts.Add(Contact(SurfaceId.FrontWall,1));Assert.That(PhysicsMeasures.Crossings(t,CourtDimensions.Standard(),out _),Is.EqualTo(1));
  }
  [Test] public void Crossings_DetectsSegmentCrossingWithoutContact() {
   var t=Path(State(30,new Vector3(-.05f,1,.05f)),State(31,new Vector3(.05f,1,.05f)));
   Assert.That(PhysicsMeasures.Crossings(t,CourtDimensions.Standard(),out _),Is.EqualTo(1));
   t.Contacts.Add(Contact(SurfaceId.RightWall,31));Assert.That(PhysicsMeasures.Crossings(t,CourtDimensions.Standard(),out _),Is.Zero);
  }
  [Test] public void Crossings_IgnoresExitOverTopButNotEarlierEscape() {
   var t=Path(State(0,new Vector3(3,5.8f,.2f)),State(1,new Vector3(3,6.5f,-.2f)));
   Assert.That(PhysicsMeasures.Crossings(t,CourtDimensions.Standard(),out _),Is.Zero);Assert.That(PhysicsMeasures.ExitedOverTop(t,CourtDimensions.Standard()),Is.True);
   t=Path(State(0,new Vector3(3,1,.2f)),State(1,new Vector3(3,1,-.2f)),State(2,new Vector3(3,7,-.2f)));
   Assert.That(PhysicsMeasures.Crossings(t,CourtDimensions.Standard(),out _),Is.EqualTo(1));
  }
  [Test] public void Penetration_IsNotCrossing() {
   var t=Path(State(0,new Vector3(3,1,.5f)),State(1,new Vector3(3,1,.015f)));
   Assert.That(PhysicsMeasures.Crossings(t,CourtDimensions.Standard(),out _),Is.Zero);
   Assert.That(PhysicsMeasures.MaxPenetration(t,CourtDimensions.Standard(),.02f),Is.EqualTo(.005f).Within(1e-6));
  }
  [Test] public void Crossings_FindsFloorAndBothSidesOfAnEscapedCorner() {
   var d=CourtDimensions.Standard();var t=Path(State(0,new Vector3(3,1,5)),State(1,new Vector3(7,1,10)));
   Assert.That(PhysicsMeasures.Crossings(t,d,out _),Is.EqualTo(2));
   t=Path(State(0,new Vector3(3,.1f,5)),State(1,new Vector3(3,-.01f,5)));
   Assert.That(PhysicsMeasures.Crossings(t,d,out _),Is.EqualTo(1));
  }
  [Test] public void AllFinite_FindsBadStateAndRejectsNoSamples() {
   var t=Path(State(0,Vector3.one),State(1,new Vector3(1,float.NaN,1)));
   Assert.That(PhysicsMeasures.AllFinite(t,out var i),Is.False);Assert.That(i,Is.EqualTo(1));
   Assert.That(PhysicsMeasures.AllFinite(new Trajectory(),out _),Is.False);
   t=Path(State(0,Vector3.one));t.Contacts.Add(Contact(SurfaceId.Floor,0,time:float.NaN,timing:ContactTiming.StepOnly));
   Assert.That(PhysicsMeasures.AllFinite(t,out _),Is.True,"StepOnly NaN timing is not a nonfinite ball state");
  }
  [Test] public void EnergyGain_IgnoresGravityWork() {
   var t=Path(State(0,new Vector3(1,1,1)),State(1,new Vector3(1,.95f,1),Vector3.down));t.Contacts.Add(Contact(SurfaceId.Floor,1));
   Assert.That(PhysicsMeasures.MaxRelativeEnergyGain(t,Params(),false,out _),Is.LessThanOrEqualTo(1e-7f));
  }
  [Test] public void EnergyGain_DetectsContactGain() {
   var p=Params();p.Gravity=0;
   var t=Path(State(0,Vector3.one,new Vector3(10,0,0)),State(1,Vector3.one,new Vector3(10.1f,0,0)));
   t.Contacts.Add(Contact(SurfaceId.FrontWall,1));t.Contacts.Add(Contact(SurfaceId.RightWall,1));
   Assert.That(PhysicsMeasures.MaxRelativeEnergyGain(t,p,true,out var step),Is.EqualTo(.0201f).Within(1e-6));Assert.That(step,Is.EqualTo(1));
  }
  [Test] public void EnergyGain_WithNoEligibleContactIsUnavailable() {
   var t=Path(State(0,Vector3.one),State(1,Vector3.one));
   Assert.That(float.IsNaN(PhysicsMeasures.MaxRelativeEnergyGain(t,Params(),true,out _)),Is.True);
  }
  [Test] public void CornerEnergy_NoCornerIsNotApplicable_NotInvalid() {
   var t=Path(State(0,Vector3.one),State(1,Vector3.one));t.Contacts.Add(Contact(SurfaceId.Floor,1));
   var value=PhysicsMeasures.MaxRelativeEnergyGain(t,Params(),EnergyStepPopulation.Corner);
   Assert.That(value.Status,Is.EqualTo(MeasurementStatus.NotApplicable));Assert.That(value.Step,Is.EqualTo(-1));
  }
  [Test] public void CornerEnergy_InvalidDataIsNotEvaluable_EvenWithoutCorner() {
   var invalid=Path(State(0,new Vector3(float.NaN,1,1)));
   Assert.That(PhysicsMeasures.MaxRelativeEnergyGain(invalid,Params(),EnergyStepPopulation.Corner).Status,Is.EqualTo(MeasurementStatus.NotEvaluable));
   var missing=Path(State(0,Vector3.one),State(2,Vector3.one));missing.Contacts.Add(Contact(SurfaceId.Floor,2));missing.Contacts.Add(Contact(SurfaceId.FrontWall,2));
   Assert.That(PhysicsMeasures.MaxRelativeEnergyGain(missing,Params(),EnergyStepPopulation.Corner).Status,Is.EqualTo(MeasurementStatus.NotEvaluable));
  }
  [Test] public void CornerEnergy_SplitsImpactAndPersistentOnlyWithoutChangingOverallMaximum() {
   var p=Params();p.Gravity=0;
   var t=Path(State(0,Vector3.one,Vector3.right*10),State(1,Vector3.one,Vector3.right*11),State(2,Vector3.one,Vector3.right*12));
   t.Contacts.Add(Contact(SurfaceId.Floor,1));t.Contacts.Add(Contact(SurfaceId.FrontWall,1,true));
   t.Contacts.Add(Contact(SurfaceId.Floor,2,true));t.Contacts.Add(Contact(SurfaceId.FrontWall,2,true));
   var impact=PhysicsMeasures.MaxRelativeEnergyGain(t,p,EnergyStepPopulation.CornerWithImpact);
   var stay=PhysicsMeasures.MaxRelativeEnergyGain(t,p,EnergyStepPopulation.CornerPersistentOnly);
   var all=PhysicsMeasures.MaxRelativeEnergyGain(t,p,EnergyStepPopulation.Corner);
   Assert.That(impact.Status,Is.EqualTo(MeasurementStatus.Measured));Assert.That(impact.Step,Is.EqualTo(1));Assert.That(impact.SpeedBefore,Is.EqualTo(10));Assert.That(impact.SpeedAfter,Is.EqualTo(11));
   Assert.That(stay.Status,Is.EqualTo(MeasurementStatus.Measured));Assert.That(stay.Step,Is.EqualTo(2));Assert.That(stay.SpeedBefore,Is.EqualTo(11));Assert.That(stay.SpeedAfter,Is.EqualTo(12));
   Assert.That(all.RelativeGain,Is.EqualTo(.21f).Within(1e-6));Assert.That(all.RelativeGain,Is.EqualTo(PhysicsMeasures.MaxRelativeEnergyGain(t,p,true,out _)));
   t.States[2]=State(2,Vector3.one,Vector3.right*13);
   all=PhysicsMeasures.MaxRelativeEnergyGain(t,p,EnergyStepPopulation.Corner);
   Assert.That(all.Step,Is.EqualTo(2),"persistent-only remains in the unchanged C3 population");
  }
  [Test] public void IsCornerStep_RecognizesDistinctSurfacesInSameOrAdjacentSteps() {
   var t=Path(State(0,Vector3.one),State(1,Vector3.one),State(2,Vector3.one));
   t.Contacts.Add(Contact(SurfaceId.Floor,1));t.Contacts.Add(Contact(SurfaceId.FrontWall,2));
   Assert.That(PhysicsMeasures.IsCornerStep(t,1),Is.True);Assert.That(PhysicsMeasures.IsCornerStep(t,2),Is.True);Assert.That(PhysicsMeasures.IsCornerStep(t,3),Is.False);
  }
  [Test] public void RepeatedImpacts_IgnoresPersistentSamplesAndLowSpeed() {
   var t=new Trajectory();t.Contacts.Add(Contact(SurfaceId.Floor,10));t.Contacts.Add(Contact(SurfaceId.Floor,11,true));
   Assert.That(PhysicsMeasures.RepeatedImpacts(t,.5f),Is.Zero);Assert.That(PhysicsMeasures.PersistentSamples(t),Is.EqualTo(1));
   t.Contacts[1]=Contact(SurfaceId.Floor,11);Assert.That(PhysicsMeasures.RepeatedImpacts(t,.5f),Is.EqualTo(1));
   t.Contacts[1]=Contact(SurfaceId.Floor,11,speed:.1f);Assert.That(PhysicsMeasures.RepeatedImpacts(t,.5f),Is.Zero);
  }
  [TestCase(float.NaN)][TestCase(float.PositiveInfinity)]
  public void RepeatedImpacts_RejectsNonfiniteContactData(float invalid) {
   var t=Path(State(0,Vector3.one));
   foreach(long step in new[]{10L,11L})t.Contacts.Add(new ContactEvent(SurfaceId.Floor,Vector3.zero,Vector2.zero,Vector3.up,step,float.NaN,ContactTiming.StepOnly,new Vector3(0,invalid,0),false));
   Assert.That(PhysicsMeasures.AllFinite(t,out _),Is.True);
   Assert.Throws<ArgumentException>(()=>PhysicsMeasures.RepeatedImpacts(t,.5f));
   t.Contacts[0]=new ContactEvent(SurfaceId.Floor,Vector3.zero,Vector2.zero,new Vector3(0,invalid,0),10,0,ContactTiming.Exact,Vector3.down,false);
   Assert.Throws<ArgumentException>(()=>PhysicsMeasures.RepeatedImpacts(t,.5f));
  }
  [Test] public void RepeatedImpacts_RejectsZeroNormalAndAcceptsUnknownTime() {
   var t=new Trajectory();t.Contacts.Add(new ContactEvent(SurfaceId.Floor,Vector3.zero,Vector2.zero,Vector3.zero,10,0,ContactTiming.Exact,Vector3.down,false));
   Assert.Throws<ArgumentException>(()=>PhysicsMeasures.RepeatedImpacts(t,.5f));
   t.Contacts.Clear();t.Contacts.Add(Contact(SurfaceId.Floor,10,time:float.NaN,timing:ContactTiming.StepOnly));t.Contacts.Add(Contact(SurfaceId.Floor,11,time:float.NaN,timing:ContactTiming.StepOnly));
   Assert.That(PhysicsMeasures.RepeatedImpacts(t,.5f),Is.EqualTo(1));
  }
  [Test] public void Bitwise_FindsFirstDifferingIndexAndSignedZero() {
   var a=new Trajectory();var b=new Trajectory();for(int i=0;i<9;i++){a.States.Add(State(i,Vector3.one));b.States.Add(State(i,Vector3.one));}
   Assert.That(PhysicsMeasures.TrajectoriesBitwiseEqual(a,b,out _),Is.True);
   var s=b.States[7];s.Position.x=BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(1f)^1);b.States[7]=s;
   Assert.That(PhysicsMeasures.TrajectoriesBitwiseEqual(a,b,out var index),Is.False);Assert.That(index,Is.EqualTo(7));
   a=Path(State(0,Vector3.zero));b=Path(State(0,new Vector3(BitConverter.Int32BitsToSingle(unchecked((int)0x80000000)),0,0)));
   Assert.That(PhysicsMeasures.TrajectoriesBitwiseEqual(a,b,out _),Is.False);
  }
  [Test] public void Bitwise_ComparesContactsTooAndReportsStateIndexOfContact() {
   var a=new Trajectory();var b=new Trajectory();for(int i=0;i<8;i++){a.States.Add(State(i,Vector3.one));b.States.Add(State(i,Vector3.one));}
   a.Contacts.Add(Contact(SurfaceId.Floor,7));b.Contacts.Add(Contact(SurfaceId.FrontWall,7));
   Assert.That(PhysicsMeasures.TrajectoriesBitwiseEqual(a,b,out var firstDiff),Is.False);Assert.That(firstDiff,Is.EqualTo(7));
   b.Contacts.Clear();Assert.That(PhysicsMeasures.TrajectoriesBitwiseEqual(a,b,out _),Is.False);
  }
  [Test] public void MaxImpacts_ExcludesPersistentSamples() {
   var t=new Trajectory();t.Contacts.Add(Contact(SurfaceId.Floor,1));t.Contacts.Add(Contact(SurfaceId.FrontWall,1));t.Contacts.Add(Contact(SurfaceId.Floor,1,true));
   Assert.That(PhysicsMeasures.MaxImpactsInOneStep(t),Is.EqualTo(2));
  }
  [Test] public void ContactPrecision_UsesFirstImpactWithoutSkippingWrongSurface() {
   var s=new PhysicsScenario{ContactPoint=new Vector3(1,2,0),ContactSurface=SurfaceId.FrontWall};var t=new Trajectory();
   Assert.That(float.IsNaN(PhysicsMeasures.ContactPrecisionMm(t,s)),Is.True);
   t.Contacts.Add(Contact(SurfaceId.Floor,1,point:new Vector3(1,2.0005f,0)));
   t.Contacts.Add(Contact(SurfaceId.FrontWall,2,point:new Vector3(1,2,0)));
   Assert.That(PhysicsMeasures.ContactPrecisionMm(t,s),Is.EqualTo(.5f).Within(.001));
  }
  [Test] public void RemainingTime_FullAndStalled() {
   var t=Path(State(0,new Vector3(1,.03f,1),new Vector3(0,-2,0)),State(1,new Vector3(1,.032f,1),new Vector3(0,2,0)));
   t.Contacts.Add(Contact(SurfaceId.Floor,1,point:new Vector3(1,0,1)));
   Assert.That(PhysicsMeasures.RemainingTimeConsumed(t,Params(),0),Is.EqualTo(1).Within(.01));
   var s=t.States[1];s.Position.y=.02f;t.States[1]=s;Assert.That(PhysicsMeasures.RemainingTimeConsumed(t,Params(),0),Is.EqualTo(0).Within(.01));
  }
  [Test] public void RemainingTime_MissingTimingOrMultipleImpactsIsUnavailable() {
   var t=Path(State(0,Vector3.one),State(1,Vector3.one,new Vector3(0,2,0)));
   t.Contacts.Add(Contact(SurfaceId.Floor,1,time:float.NaN,timing:ContactTiming.StepOnly));
   Assert.That(float.IsNaN(PhysicsMeasures.RemainingTimeConsumed(t,Params(),0)),Is.True);
   t.Contacts[0]=Contact(SurfaceId.Floor,1);t.Contacts.Add(Contact(SurfaceId.FrontWall,1));
   Assert.That(float.IsNaN(PhysicsMeasures.RemainingTimeConsumed(t,Params(),0)),Is.True);
  }
 }
}
