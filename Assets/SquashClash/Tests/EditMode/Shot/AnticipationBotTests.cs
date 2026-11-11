using System;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class AnticipationBotTests
    {
        private static ContactPlan Plan(int version)=>new ContactPlan(true,new SimTime(100),new SimTime(200),150,
            new Vector3(2.6f,.6f,5.5f),StrokeSide.Drive,version,
            new BodyFrame(new Vector2(3.2f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f),new LegalSettings(.05f,2,100,0),CourtDimensions.Standard());
        [TestCase(0)] [TestCase(-60)] [TestCase(-100)] [TestCase(-150)]
        public void Bot_ReleasesAtFixedOffset_FromVisibleTStar(double offset)
        {
            var plans=new PresentedPlans();plans.Present(1,Plan(1),5);plans.Present(2,Plan(2),6);
            var bot=new AnticipationBot(17,offset);
            Assert.That(bot.TrySchedule(plans,1.5,new TimingSettings(),3,out var r),Is.True);
            Assert.That(r.EventAtReal,Is.EqualTo(5+offset/1000));Assert.That(r.TargetAtReal,Is.EqualTo(5));
            Assert.That(r.PresentedAtReal,Is.EqualTo(1));Assert.That(r.PlanVersion,Is.EqualTo(1));Assert.That(r.Seed,Is.EqualTo(17));
        }
        [Test] public void Bot_IsDeterministicPerSeed()
        {
            var a=new AnticipationBot(9,-100);var b=new AnticipationBot(9,-100);var other=new AnticipationBot(10,-100);
            bool differs=false;
            for(int i=0;i<30;i++)
            {
                var x=a.Intent(i,new Vector2(3.2f,2.2f),.2f);var y=b.Intent(i,new Vector2(3.2f,2.2f),.2f);
                Assert.That(x,Is.EqualTo(y));differs|=x!=other.Intent(i,new Vector2(3.2f,2.2f),.2f);
                Assert.That(new[]{.25f,.5f,.75f},Does.Contain(x.power));Assert.That(Math.Abs(x.aim.x-3.2f),Is.LessThanOrEqualTo(.200001f));
            }
            Assert.That(differs,Is.True);
        }
        [Test] public void Report_SeparatesContactAndNoContactPopulations()
        {
            var r=new AnticipationReport(false);
            r.Add(false,ShotResult.Valid,FailKind.None,true,true,new Vector3(2,4,6));
            r.Add(false,ShotResult.Perfect,FailKind.None,true,false,new Vector3(4,6,8));
            r.Add(false,ShotResult.Failed,FailKind.Air,false,null,null);r.Add(false,ShotResult.Failed,FailKind.Late,false,null,null);
            r.Add(false,ShotResult.NoOrder,FailKind.None,false,null,null);
            Assert.That(r.Total,Is.EqualTo(5));Assert.That(r.Contacts,Is.EqualTo(2));Assert.That(r.Air,Is.EqualTo(1));Assert.That(r.Late,Is.EqualTo(1));Assert.That(r.Other,Is.EqualTo(1));
            Assert.That(r.VectorSamples,Is.EqualTo(2));Assert.That(r.MeanCausal,Is.EqualTo(new Vector3(3,5,7)));
            Assert.That(r.ZoneRateAmongObserved,Is.EqualTo(.5));
        }
        [Test] public void Report_SeparatesVEffectFromT3Geometry()
        {
            var t=new Trajectory();for(int i=0;i<231;i++)t.States.Add(new BallState{Step=i,Position=new Vector3(2.6f,.6f,5.5f)});
            foreach(var e in new[]{(SurfaceId.FrontWall,1),(SurfaceId.Floor,101),(SurfaceId.Floor,201)})
            {var n=CourtDimensions.InwardNormal(e.Item1);t.Contacts.Add(new ContactEvent(e.Item1,new Vector3(2.6f,.6f,5.5f)-n*.02f,Vector2.zero,n,e.Item2,0,ContactTiming.Exact,Vector3.zero,false));}
            var plans=new PresentedPlans();plans.Present(0,Plan(1),1.5);var clock=new ClockMap();clock.Begin(1,0,0,1,1,100);
            foreach(double at in new[]{1.44,1.4,1.35})
            {
                var t3=ContactContract.Decide(at,plans,clock,t,new TimingSettings(40,false,100),false);
                var v=ContactContract.Decide(at,plans,clock,t,new TimingSettings(40,true,100),false);
                if(at==1.35){Assert.That(t3.Result,Is.EqualTo(ShotResult.Valid));Assert.That(v.Result,Is.EqualTo(ShotResult.Failed));}
                else {Assert.That(v.Result,Is.EqualTo(t3.Result));Assert.That(v.ContactStep,Is.EqualTo(t3.ContactStep));}
            }
            var report=new AnticipationReport(false);report.Add(false,ShotResult.Valid,FailKind.None,true,true,new Vector3(0,2,-1));
            Assert.That(report.MeanCausal,Is.EqualTo(new Vector3(0,2,-1)),"V is a contact gate, not a synthetic geometric penalty");
        }
        [Test] public void UnavailableCausalVectorsStayNullAndNeverBecomeZeroSamples()
        {
            var r=new AnticipationReport(true);r.Add(true,ShotResult.Valid,FailKind.None,true,null,null);
            Assert.That(r.Contacts,Is.EqualTo(1));Assert.That(r.VectorSamples,Is.Zero);Assert.That(r.MeanCausal,Is.Null);Assert.That(r.ZoneRateAmongObserved,Is.Null);
        }
        [Test] public void ReportsRejectMixingModesAndNoContactVectors()
        {
            var r=new AnticipationReport(true);
            Assert.Throws<ArgumentException>(()=>r.Add(false,ShotResult.Valid,FailKind.None,true,true,null));
            Assert.Throws<ArgumentException>(()=>r.Add(true,ShotResult.Failed,FailKind.Air,false,null,Vector3.zero));
            Assert.That(r.Total,Is.Zero);
        }
        [Test] public void BotNeverFabricatesAPlanOrSchedulesAnAlreadyPastRelease()
        {
            var bot=new AnticipationBot(1,-150);Assert.That(bot.TrySchedule(new PresentedPlans(),0,new TimingSettings(),0,out _),Is.False);
            var plans=new PresentedPlans();plans.Present(0,Plan(1),1);
            Assert.That(bot.TrySchedule(plans,.9,new TimingSettings(),0,out _),Is.False);
        }
    }
}
