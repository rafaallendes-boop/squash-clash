using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    // Volley v1 (design 2026-11-18): zero-bounce K, the volley planner and the opportunity split. Synthetic timing fixture:
    // 100 Hz, tempo 1, front wall during step 2, out of reach in states 6-8, first bounce in step 7 (+0.5), second in step 13 (+0.5).
    public sealed class VolleyContactTests
    {
        private static BodyFrame Body=>new BodyFrame(new Vector2(3.2f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f);
        private static readonly float[] Heights={1.6f,1.6f,1.6f,1.4f,1.1f,1f,1.2f,1.2f,1.2f,.3f,.6f,.6f,.6f,.6f,.6f,.6f};
        private static Trajectory Fixture()
        {
            var t=new Trajectory();
            for(int i=0;i<Heights.Length;i++)t.States.Add(new BallState{Step=i,Position=new Vector3(i>=6&&i<=8?6f:2.6f,Heights[i],5.5f),Velocity=Vector3.zero});
            t.Contacts.Add(Event(SurfaceId.FrontWall,3,0,1.6f));t.Contacts.Add(Event(SurfaceId.Floor,8,.005f,0f,6f));t.Contacts.Add(Event(SurfaceId.Floor,14,.005f,0f));
            return t;
        }
        private static ContactEvent Event(SurfaceId s,int step,float time,float height,float x=2.6f)
        {var n=CourtDimensions.InwardNormal(s);return new ContactEvent(s,new Vector3(x,height,5.5f)-n*.02f,Vector2.zero,n,step,time,ContactTiming.Exact,Vector3.zero,false);}
        private static LegalSettings BounceLegal=>new LegalSettings(.05f,2,100,0);
        private static LegalSettings VolleyLegal=>new LegalSettings(.6f,1.9f,100,0,.02f,null,0);
        private static VolleySettings Settings(double separation=.02,double window=.01,float min=.6f,float max=1.9f,bool enabled=true)=>new VolleySettings(enabled,min,max,separation,window);
        private static ContactPlan BouncePlan(Trajectory t)=>ContactPlanner.Plan(t,Body,CourtDimensions.Standard(),BounceLegal,1);
        private static VolleyOpportunity Volley(Trajectory t,VolleySettings s,System.Func<long,BodyFrame> body=null,long earliest=0,ContactPlan? bounce=null)
            =>VolleyPlanner.Plan(t,body??(_=>Body),CourtDimensions.Standard(),BounceLegal,s,bounce??BouncePlan(t),earliest,1,7);

        [Test] public void LegalSettings_DefaultsToOneBounce()=>Assert.That(BounceLegal.Bounces,Is.EqualTo(1));
        [Test] public void BouncePlan_IsUnchangedByTheVolleyMode()
        {
            var p=BouncePlan(Fixture());Assert.That(p.Exists,Is.True);
            Assert.That(p.KIn.Steps,Is.EqualTo(9));Assert.That(p.KOut.Steps,Is.EqualTo(13.5).Within(1e-6));Assert.That(p.TStarStep,Is.EqualTo(10));
        }
        [Test] public void ZeroBounces_PlansTheVolleyBeforeTheBounce()
        {
            var p=ContactPlanner.Plan(Fixture(),Body,CourtDimensions.Standard(),VolleyLegal,3);
            Assert.That(p.Exists,Is.True);Assert.That(p.Legal.Bounces,Is.Zero);
            Assert.That(p.KIn.Steps,Is.EqualTo(2));Assert.That(p.KOut.Steps,Is.EqualTo(6));Assert.That(p.TStarStep,Is.EqualTo(4));
            Assert.That(ContactPlanner.IsLegalStep(Fixture(),4,Body,VolleyLegal),Is.True);
            Assert.That(ContactPlanner.IsLegalStep(Fixture(),10,Body,VolleyLegal),Is.False,"after the bounce a volley is not legal");
            Assert.That(ContactPlanner.IsLegalStep(Fixture(),4,Body,BounceLegal),Is.False,"before the bounce the bounce plan is not legal");
        }
        [Test] public void PhaseLegal_IgnoresReach_AndFollowsTheRequiredBounces()
        {
            var phase=ContactPlanner.PhaseLegal(Fixture(),Body,VolleyLegal);
            Assert.That(Enumerable.Range(0,phase.Length).Where(i=>phase[i]).ToArray(),Is.EqualTo(new[]{3,4,5,6,7}));
        }
        [Test] public void Planner_OffersTheVolley_WithTheSplitBetweenWindows()
        {
            var v=Volley(Fixture(),Settings());
            Assert.That(v.Reason,Is.EqualTo("offered"));Assert.That(v.Exists,Is.True);
            Assert.That(v.Plan.TStarStep,Is.EqualTo(4));Assert.That(v.Plan.Version,Is.EqualTo(7));Assert.That(v.SplitSteps,Is.EqualTo(7.5));
        }
        [Test] public void Planner_HidesTheVolley_WhenTooCloseToTheBounceWindow()
            =>Assert.That(Volley(Fixture(),Settings(separation:.25)).Reason,Is.EqualTo("separation"));
        [Test] public void Planner_OffersTheVolley_WhenThereIsNoBouncePlan()
        {
            var v=Volley(Fixture(),Settings(separation:.25),bounce:default(ContactPlan));
            Assert.That(v.Exists,Is.True);Assert.That(double.IsPositiveInfinity(v.SplitSteps),Is.True);
        }
        [Test] public void Planner_RespectsHeightWindowEarliestAndSwitch()
        {
            Assert.That(Volley(Fixture(),Settings(min:1.7f)).Reason,Is.EqualTo("no-pass"));
            Assert.That(Volley(Fixture(),Settings(window:.1)).Reason,Is.EqualTo("window"));
            Assert.That(Volley(Fixture(),Settings(),earliest:5).Exists,Is.False);
            Assert.That(Volley(Fixture(),Settings(enabled:false)).Reason,Is.EqualTo("disabled"));
        }
        [Test] public void Planner_UsesTheMovingBody_AtEachStep()
        {
            var far=new BodyFrame(new Vector2(.5f,9f),true,1.3f,.6f,.4f,1.1f,.3f);
            var late=Volley(Fixture(),Settings(),s=>s<4?far:Body);
            Assert.That(late.Exists,Is.True);Assert.That(late.Plan.TStarStep,Is.EqualTo(4));
            Assert.That(Volley(Fixture(),Settings(),_=>far).Reason,Is.EqualTo("no-pass"));
            var again=Volley(Fixture(),Settings(),s=>s<4?far:Body);
            Assert.That(again.Plan.KIn.Steps,Is.EqualTo(late.Plan.KIn.Steps));Assert.That(again.Plan.KOut.Steps,Is.EqualTo(late.Plan.KOut.Steps));Assert.That(again.SplitSteps,Is.EqualTo(late.SplitSteps));
        }

        // Opportunity split (spec 7.1.3 case 14) on the unchanged contract. T3: no V limit, A=40 ms.
        private static readonly TimingSettings T3=new TimingSettings(40,false,100,false,0);
        private static ContactDecision Decide(double releaseSteps,out Opportunity used,bool withVolley=true)
        {
            var t=Fixture();var clock=new ClockMap();clock.Begin(1,0,0,1,1,100);
            var bounce=BouncePlan(t);var volley=withVolley?Volley(t,Settings()):default;
            var bouncePlans=new PresentedPlans();bouncePlans.Present(0,bounce,bounce.TStarStep/100d);
            PresentedPlans volleyPlans=null;
            if(withVolley){volleyPlans=new PresentedPlans();volleyPlans.Present(0,volley.Plan,volley.Plan.TStarStep/100d);}
            return OpportunityContract.Decide(releaseSteps/100,bouncePlans,volleyPlans,volley,clock,t,T3,false,out used);
        }
        [Test] public void Split_BeforeVolleyK_IsAir()
        {var d=Decide(1,out var used);Assert.That(used,Is.EqualTo(Opportunity.Volley));Assert.That(d.Result,Is.EqualTo(ShotResult.Failed));Assert.That(d.Fail,Is.EqualTo(FailKind.Air));}
        [Test] public void Split_InVolleyK_IsAContactBeforeTheBounce()
        {var d=Decide(4,out var used);Assert.That(used,Is.EqualTo(Opportunity.Volley));Assert.That(d.Result,Is.EqualTo(ShotResult.Perfect));Assert.That(d.ContactStep,Is.EqualTo(4));}
        [Test] public void Split_AfterVolleyK_UpToS_IsLate()
        {
            var d=Decide(7.5,out var used);Assert.That(used,Is.EqualTo(Opportunity.Volley));Assert.That(d.Fail,Is.EqualTo(FailKind.Late));
            d=Decide(6.5,out used);Assert.That(used,Is.EqualTo(Opportunity.Volley));Assert.That(d.Fail,Is.EqualTo(FailKind.Late));
        }
        [Test] public void Split_AfterS_BeforeBounceK_IsAir_NeverAnAccidentalVolley()
        {var d=Decide(8,out var used);Assert.That(used,Is.EqualTo(Opportunity.Bounce));Assert.That(d.Fail,Is.EqualTo(FailKind.Air));Assert.That(d.ContactStep,Is.EqualTo(-1));}
        [Test] public void Split_BounceWindow_IsTheBounceContract()
        {
            var d=Decide(10,out var used);Assert.That(used,Is.EqualTo(Opportunity.Bounce));Assert.That(d.Result,Is.EqualTo(ShotResult.Perfect));Assert.That(d.ContactStep,Is.EqualTo(10));
            d=Decide(14,out used);Assert.That(used,Is.EqualTo(Opportunity.Bounce));Assert.That(d.Fail,Is.EqualTo(FailKind.Late));
        }
        [Test] public void WithoutVolley_TheDecisionIsTheContractItself()
        {
            var t=Fixture();var clock=new ClockMap();clock.Begin(1,0,0,1,1,100);var bounce=BouncePlan(t);
            var plans=new PresentedPlans();plans.Present(0,bounce,bounce.TStarStep/100d);
            foreach(double r in new[]{1,4,6.5,8,9,10,11.3,13.5,14,double.NaN})
            {
                var a=OpportunityContract.Decide(r/100,plans,null,default,clock,t,T3,false,out var used);var b=ContactContract.Decide(r/100,plans,clock,t,T3,false);
                Assert.That(used,Is.EqualTo(Opportunity.Bounce));
                Assert.That((a.Result,a.Fail,a.ContactStep,a.DeltaMs,a.Scheduled,a.Reason),Is.EqualTo((b.Result,b.Fail,b.ContactStep,b.DeltaMs,b.Scheduled,b.Reason)),"release "+r);
            }
        }
        [Test] public void NoRelease_NeverVolleys()
        {var d=Decide(double.NaN,out var used);Assert.That(used,Is.EqualTo(Opportunity.Bounce));Assert.That(d.Result,Is.EqualTo(ShotResult.NoOrder));}
    }
}
