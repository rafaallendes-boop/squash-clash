using System;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class ContactContractTests
    {
        private Trajectory trajectory;
        private ContactPlan plan;
        private PresentedPlans plans;
        private ClockMap clock;
        private TimingSettings settings;
        private readonly BodyFrame body=new BodyFrame(new Vector2(3.2f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f);
        [SetUp] public void Setup()
        {
            trajectory=new Trajectory();
            for(int i=0;i<=230;i++) trajectory.States.Add(new BallState{Step=i,Position=new Vector3(2.6f,.6f,5.5f)});
            trajectory.Contacts.Add(Event(SurfaceId.FrontWall,1,0,1.2f));
            trajectory.Contacts.Add(Event(SurfaceId.Floor,101,0));
            trajectory.Contacts.Add(Event(SurfaceId.Floor,201,0));
            clock=new ClockMap();clock.Begin(1,0,0,1,1,100);
            settings=new TimingSettings(40,true,100);
            SetPlan(100,200,150,1);
        }
        private ContactEvent Event(SurfaceId surface,long step,float time,float height=.6f)
        {var normal=CourtDimensions.InwardNormal(surface);return new ContactEvent(surface,new Vector3(2.6f,height,5.5f)-normal*.02f,Vector2.zero,normal,step,time,ContactTiming.Exact,Vector3.zero,false);}
        private void SetPlan(double kin,double kout,long star,int version,double presented=0,double? target=null)
        {
            plan=new ContactPlan(true,SimTime.FromSteps(kin),SimTime.FromSteps(kout),star,new Vector3(2.6f,.6f,5.5f),StrokeSide.Drive,version,body,new LegalSettings(.05f,2,100,0),CourtDimensions.Standard());
            plans=new PresentedPlans();plans.Present(presented,plan,target??star/100d);
        }
        private ContactDecision Decide(double r, bool consumed=false)=>ContactContract.Decide(r,plans,clock,trajectory,settings,consumed);
        private void IsContact(ContactDecision d,ShotResult result,long step)
        {Assert.That(d.Result,Is.EqualTo(result));Assert.That(d.ContactStep,Is.EqualTo(step));}
        [Test] public void Case1_WithinA_BeforeTStar_IsPerfectScheduledAtTStar()
        {var d=Decide(1.48);IsContact(d,ShotResult.Perfect,150);Assert.That(d.Scheduled,Is.True);}
        [Test] public void Case1_WithinA_AfterTStar_IsPerfectAtR()
        {var d=Decide(1.52);IsContact(d,ShotResult.Perfect,152);Assert.That(d.Scheduled,Is.False);}
        [Test] public void DeltaExactlyA_IsPerfect()
        {Assert.That(Decide(1.54).Result,Is.EqualTo(ShotResult.Perfect));Assert.That(Decide(1.46).Result,Is.EqualTo(ShotResult.Perfect));Assert.That(Decide(1.540001).Result,Is.EqualTo(ShotResult.Valid));}
        [Test] public void Case2_EarlyBeyondA_InK_IsValidAtR()=>IsContact(Decide(1.43),ShotResult.Valid,143);
        [Test] public void Case3_LateBeyondA_InK_IsValidAtR()=>IsContact(Decide(1.57),ShotResult.Valid,157);
        [Test] public void DeltaExactlyV_IsValid_BeyondV_IsFailedOnlyInVMode()
        {Assert.That(Decide(1.6).Result,Is.EqualTo(ShotResult.Valid));Assert.That(Decide(1.4).Result,Is.EqualTo(ShotResult.Valid));Assert.That(Decide(1.600001).Result,Is.EqualTo(ShotResult.Failed));}
        [Test] public void PureT3_BeyondV_InK_IsValid()
        {settings=new TimingSettings(40,false,100);IsContact(Decide(1.75),ShotResult.Valid,175);}
        [Test] public void Case4_BeforeKIn_IsAir_EvenWithinA()
        {SetPlan(100,200,102,1);var d=Decide(.99);Assert.That(d.Result,Is.EqualTo(ShotResult.Failed));Assert.That(d.Fail,Is.EqualTo(FailKind.Air));Assert.That(d.ContactStep,Is.EqualTo(-1));}
        [Test] public void T3Soft_BeforeKIn_ContactsAtKInWithEarlyGeometry()
        {settings=new TimingSettings(40,false,100,true);var d=Decide(.9);IsContact(d,ShotResult.Valid,100);Assert.That(d.EarlyGeometry,Is.True);Assert.That(d.QuantizationErrorMsSigned,Is.Zero.Within(1e-9));}
        [Test] public void Case5_AfterKOut_IsLate()
        {var d=Decide(2.01);Assert.That(d.Result,Is.EqualTo(ShotResult.Failed));Assert.That(d.Fail,Is.EqualTo(FailKind.Late));}
        [Test] public void Case6_NoRelease_IsNoOrder()=>Assert.That(Decide(double.NaN).Result,Is.EqualTo(ShotResult.NoOrder));
        [Test] public void Case7_NoPlan_IsUnreachable()
        {plans=new PresentedPlans();plans.Present(0,default,0);Assert.That(Decide(1.5).Result,Is.EqualTo(ShotResult.Unreachable));}
        [Test] public void Case8_PlanChangedBeforeR_UsesPlanVisibleAtR()
        {var old=plan;SetPlan(100,200,160,2);var next=plan;plans=new PresentedPlans();plans.Present(0,old,1.5);plans.Present(1.55,next,1.6);
         Assert.That(Decide(1.52).PlanVersionUsed,Is.EqualTo(1));var d=Decide(1.6);Assert.That(d.PlanVersionUsed,Is.EqualTo(2));Assert.That(d.DeltaMs,Is.Zero);}
        [Test] public void Delta_UsesTargetContactTime_NotPresentationTime()
        {clock.Begin(1,9,0,1,1,100);SetPlan(100,200,150,1,10,10.5);var d=Decide(10.5);Assert.That(d.DeltaMs,Is.Zero);Assert.That(d.PlanPresentedAtReal,Is.EqualTo(10));Assert.That(d.TargetContactAtReal,Is.EqualTo(10.5));}
        [Test] public void PlanChangeBetweenCorrectedAndEventTime_UsesPlanAtEventTime()
        {var old=plan;SetPlan(100,200,153,2);var next=plan;plans=new PresentedPlans();plans.Present(0,old,1.5);plans.Present(1.55,next,1.53);settings=new TimingSettings(40,true,100,false,70);var d=Decide(1.6);Assert.That(d.PlanVersionUsed,Is.EqualTo(2));Assert.That(d.DeltaMs,Is.Zero.Within(1e-9));}
        [Test] public void Case9_AuthoritativeLeavesK_IsSystemFault()
        {for(int i=0;i<trajectory.States.Count;i++){var s=trajectory.States[i];s.Position.x=6;trajectory.States[i]=s;}Assert.That(Decide(1.5).Result,Is.EqualTo(ShotResult.SystemFault));}
        [Test] public void Case11_AttemptAlreadyConsumed_NoSwing()=>Assert.That(Decide(1.5,true).Result,Is.EqualTo(ShotResult.AttemptConsumed));
        [Test] public void Case13_AtSecondBounce_NoContact()
        {settings=new TimingSettings(0,false,100);var d=Decide(2);Assert.That(d.ContactStep,Is.EqualTo(-1));Assert.That(d.Fail,Is.EqualTo(FailKind.Late));}
        [Test] public void DeltaIsInRealMs_NotSimulationMs()
        {clock.Begin(1,0,0,.65,1,100);plans=new PresentedPlans();plans.Present(0,plan,150/65d);var d=Decide(150/65d+.05);Assert.That(d.DeltaMs,Is.EqualTo(50).Within(1e-9));Assert.That(d.Result,Is.EqualTo(ShotResult.Valid));}
        [Test] public void LatencyCompensation_IsAppliedOnce_BeforeMapping()
        {settings=new TimingSettings(40,true,100,false,50);var d=Decide(1.57);IsContact(d,ShotResult.Perfect,152);Assert.That(d.ReleaseEventAtReal,Is.EqualTo(1.57));Assert.That(d.ReleaseCorrectedAtReal,Is.EqualTo(1.52).Within(1e-12));Assert.That(d.DeltaMs,Is.EqualTo(20).Within(1e-9));}
        [Test] public void ContactStep_IsNearestStepToR_AndSignedErrorIsReported()
        {settings=new TimingSettings(0,false,100);var d=Decide(1.563);IsContact(d,ShotResult.Valid,156);Assert.That(d.QuantizationErrorMsSigned,Is.EqualTo(-3).Within(1e-9));}
        [Test] public void ExactTieBetweenSteps_UsesEarlierStep()
        {clock.Begin(1,0,0,1,1,128);plans=new PresentedPlans();plans.Present(0,plan,150/128d);settings=new TimingSettings(0,false,100);var d=Decide(155.5/128);IsContact(d,ShotResult.Valid,155);}
        [Test] public void LegalRNearSecondBounce_NearestStepIllegal_UsesLegalNeighbor()
        {settings=new TimingSettings(0,false,100);var d=Decide(1.998);IsContact(d,ShotResult.Valid,199);Assert.That(d.UsedNeighborLegalStep,Is.True);Assert.That(d.QuantizationErrorMsSigned,Is.EqualTo(-8).Within(1e-9));}
        [Test] public void LegalRNearKOut_NearestStepOutOfReach_UsesLegalNeighbor()
        {SetPlan(100,175.6,150,1);settings=new TimingSettings(0,false,100);var d=Decide(1.754);IsContact(d,ShotResult.Valid,175);Assert.That(d.UsedNeighborLegalStep,Is.False);
         d=Decide(1.7555);Assert.That(d.ContactStep,Is.EqualTo(175));Assert.That(d.UsedNeighborLegalStep,Is.True);}
        [Test] public void IllegalRAfterSecondBounce_NoContactEvenIfNeighborLegal()
        {settings=new TimingSettings(0,false,100);Assert.That(Decide(2.001).ContactStep,Is.EqualTo(-1));}
        [Test] public void PerfectEarly_SchedulesAtTStarOfVisiblePlan_NotNearestToR()
        {var d=Decide(1.465);IsContact(d,ShotResult.Perfect,150);Assert.That(d.QuantizationErrorMsSigned,Is.Zero);}
        [Test] public void T3Soft_KInContact_RespectsSameBoundaryRules()
        {SetPlan(100.25,200,150,1);settings=new TimingSettings(40,false,100,true);var d=Decide(.9);IsContact(d,ShotResult.Valid,101);Assert.That(d.UsedNeighborLegalStep,Is.True);Assert.That(d.QuantizationErrorMsSigned,Is.EqualTo(7.5).Within(1e-9));}
        [Test] public void HalfStepAboveThirdOfA_IsFlagged()
        {clock.Begin(1,0,0,.3,1,60);plans=new PresentedPlans();plans.Present(0,plan,150/18d);var d=Decide(150/18d);Assert.That(d.QuantizationAboveThirdOfA,Is.True);}
        [Test] public void ReleaseOlderThanClockMap_IsNotEvaluable()
        {clock.Begin(1,1,0,1,1,100);Assert.That(Decide(.5).Result,Is.EqualTo(ShotResult.NotEvaluable));}
        [Test] public void NewTimingKeys_AreRecordedWithApprovedDefaults()
        {var p=ConfigFixtures.Default();Assert.That(p.Ordered.Count,Is.EqualTo(101));Assert.That(TimingSettings.From(p).AssistMs,Is.EqualTo(40));Assert.That(TimingSettings.From(p).ValidLimitOn,Is.True);Assert.That(TimingSettings.From(p).ValidMs,Is.EqualTo(100));Assert.That(TimingSettings.From(p).EarlySoft,Is.False);Assert.That(TimingSettings.From(p).LatencyCompMs,Is.Zero);}
    }
}
