using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class ContactPlannerTests
    {
        private static BodyFrame Body=>new BodyFrame(new Vector2(3.2f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f);
        // Timing-only metadata fixture, independent of the P2 bounce response. Real P2 recipes are tested separately.
        private static Trajectory TimingFixture(float height=.6f)
        {
            var t=new Trajectory();for(int i=0;i<=8;i++)t.States.Add(new BallState{Step=i,Position=new Vector3(2.6f,height,5.5f),Velocity=Vector3.zero});
            t.Contacts.Add(Event(SurfaceId.FrontWall,1,0,1.2f));t.Contacts.Add(Event(SurfaceId.Floor,3,1f/256,height));t.Contacts.Add(Event(SurfaceId.Floor,6,1f/256,height));return t;
        }
        private static ContactEvent Event(SurfaceId s,int step,float time,float height=.6f,bool persistent=false)
        {var n=CourtDimensions.InwardNormal(s);return new ContactEvent(s,new Vector3(2.6f,height,5.5f)-n*.02f,Vector2.zero,n,step,time,ContactTiming.Exact,Vector3.zero,persistent);}
        private static LegalSettings Legal=>new LegalSettings(.05f,2,128,0);
        [Test] public void NoK_BeforeFirstBounce_EvenIfReachable()
        {Assert.That(ContactPlanner.IsLegalAt(TimingFixture(),new SimTime(2,.49),Body,Legal),Is.False);Assert.That(ContactPlanner.IsLegalAt(TimingFixture(),new SimTime(3),Body,Legal),Is.True);}
        [Test] public void Legality_RequiresFrontWallThenExactlyOneBounce()
        {
            var t=TimingFixture();t.Contacts.RemoveAt(0);Assert.That(ContactPlanner.IsLegalStep(t,3,Body,Legal),Is.False);
            t=TimingFixture();t.Contacts.Insert(1,Event(SurfaceId.Floor,2,0,persistent:true));Assert.That(ContactPlanner.IsLegalStep(t,3,Body,Legal),Is.True);Assert.That(ContactPlanner.IsLegalStep(t,6,Body,Legal),Is.False);
        }
        [Test] public void K_BoundariesUseExactEventTimes_NotWholeSteps()
        {
            var p=ContactPlanner.Plan(TimingFixture(),Body,CourtDimensions.Standard(),Legal,9);Assert.That(p.Exists,Is.True);
            Assert.That(p.KIn.Steps,Is.EqualTo(2.5).Within(1e-9));Assert.That(p.KOut.Steps,Is.EqualTo(5.5).Within(1e-9));Assert.That(p.Version,Is.EqualTo(9));
        }
        [Test] public void LegalHeights_AreSeparateFromPreferredBand()
        {Assert.That(ContactPlanner.IsLegalStep(TimingFixture(1.8f),3,Body,Legal),Is.True);Assert.That(ContactPlanner.IsLegalStep(TimingFixture(2.1f),3,Body,Legal),Is.False);}
        [Test] public void K_IsContiguousLegalStepsWithinReach_ResolvedPerStep()
        {
            var t=TimingFixture();for(int i=4;i<t.States.Count;i++){var s=t.States[i];s.Position.x=6;t.States[i]=s;}
            var p=ContactPlanner.Plan(t,Body,CourtDimensions.Standard(),Legal,1);Assert.That(p.Exists,Is.True);Assert.That(p.TStarStep,Is.EqualTo(3));Assert.That(p.KOut.Steps,Is.LessThanOrEqualTo(4));
        }
        [Test] public void K_EndsAtExactSecondBounceTime()
        {Assert.That(ContactPlanner.IsLegalAt(TimingFixture(),new SimTime(5,.4999),Body,Legal),Is.True);Assert.That(ContactPlanner.IsLegalAt(TimingFixture(),new SimTime(5,.5),Body,Legal),Is.False);}
        [Test] public void TStar_MaximizesPreferredHeightAndWallClearance()
        {
            var t=TimingFixture(.1f);var s=t.States[4];s.Position.y=.7f;t.States[4]=s;
            var p=ContactPlanner.Plan(t,Body,CourtDimensions.Standard(),Legal,1);Assert.That(p.Exists,Is.True);Assert.That(p.TStarStep,Is.EqualTo(4));
            Assert.That(ContactPlanner.Plan(TimingFixture(),Body,CourtDimensions.Standard(),Legal,1).TStarStep,Is.EqualTo(3),"Tie resolves earlier");
        }
        [Test] public void NoReachableOpportunity_PlanDoesNotExist()
        {var b=new BodyFrame(new Vector2(0,0),true,1.3f,.6f,.4f,1.1f,.3f);Assert.That(ContactPlanner.Plan(TimingFixture(),b,CourtDimensions.Standard(),Legal,1).Exists,Is.False);}
        [Test] public void SideSelection_NearAxis_IsDeterministic()
        {
            Assert.That(Body.SideFor(new Vector3(3.2f,1,5)),Is.EqualTo(StrokeSide.Drive));Assert.That(Body.SideFor(new Vector3(3.21f,1,5)),Is.EqualTo(StrokeSide.Backhand));Assert.That(Body.SideFor(new Vector3(3.19f,1,5)),Is.EqualTo(StrokeSide.Drive));
            Assert.That(Body.IdealContactPoint(StrokeSide.Drive),Is.EqualTo(new Vector3(2.6f,.75f,5.5f)));
        }
        [Test] public void BodyAndLegalParameters_HaveApprovedInitialValues()
        {
            Assert.That(ParamCatalog.TryGet("body.reach",out _),Is.True);var b=BodyFrame.From(ConfigFixtures.Default());Assert.That(b.Reach,Is.EqualTo(1.3f));Assert.That(b.ReferenceHeight,Is.EqualTo(.9f));
            Assert.That(ConfigFixtures.Default().Get("legal.minHeight"),Is.EqualTo(.05));Assert.That(ConfigFixtures.Default().Get("legal.maxHeight"),Is.EqualTo(2));
        }
        [Test] public void FrontTinOrOut_DoesNotProduceAReceivingOpportunity()
        {
            foreach(float y in new[]{.1f,5f}){var t=TimingFixture();t.Contacts[0]=Event(SurfaceId.FrontWall,1,0,y);Assert.That(ContactPlanner.IsLegalStep(t,3,Body,Legal),Is.False);}
        }
        [Test] public void ChangingPreferredBand_DoesNotChangeRealReach()
        {
            var b=new BodyFrame(Body.PositionXZ,true,Body.Reach,.6f,1.5f,1.8f,.3f);
            Assert.That(ContactPlanner.IsLegalStep(TimingFixture(),3,b,Legal),Is.True);
        }
        [Test] public void SixPracticeLaunches_AreReachableFromTheirBodyPositions()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var definitions=PracticeLaunchDefinitions.Create(cfg);Assert.That(definitions.Count,Is.EqualTo(6));
            foreach(var d in definitions)
            {
                using(var sim=new AnalyticBallSimulationFactory(0).Create(cfg.Physics,cfg.Court))
                {
                    var t=TrajectoryRunner.Run(sim,d.Initial,cfg.Physics.StepHz*10,StopCondition.SecondFloorBounce,cfg.Court);
                    var legal=new LegalSettings(.05f,2,cfg.Physics.StepHz,cfg.Physics.Gravity,cfg.Physics.BallRadius);
                    var plan=ContactPlanner.Plan(t,d.Body,cfg.Court,legal,1);Assert.That(plan.Exists,Is.True,d.Id);Assert.That(ContactPlanner.IsLegalStep(t,plan.TStarStep,d.Body,legal),Is.True,d.Id);
                    var clock=new ClockMap();clock.Begin(1,0,0,cfg.Tempo,1,cfg.Physics.StepHz);
                    Assert.That(clock.TrySimToReal(plan.TStarStep,0,out var star),Is.True);
                    var front=t.Contacts.First(e=>e.Surface==SurfaceId.FrontWall);var floor=t.Contacts.First(e=>e.Surface==SurfaceId.Floor&&!e.Persistent);
                    Assert.That(clock.TrySimToReal(front.Step-1,front.TimeInStep*cfg.Physics.StepHz,out var frontReal),Is.True);
                    Assert.That(clock.TrySimToReal(floor.Step-1,floor.TimeInStep*cfg.Physics.StepHz,out var floorReal),Is.True);
                    Assert.That(clock.TrySimToReal(plan.KIn.Step,plan.KIn.Fraction,out var kin),Is.True);Assert.That(clock.TrySimToReal(plan.KOut.Step,plan.KOut.Fraction,out var kout),Is.True);
                    TestContext.WriteLine("SL_TIMING "+d.Id+" launch=0 front="+frontReal.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+" bounce="+floorReal.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+" kin="+kin.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+" star="+star.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+" kout="+kout.ToString("R",System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }
        [Test] public void LegibilityLaunchesL1toL6_AreUnchanged()
        {var c=EffectiveConfig.From(ConfigFixtures.Default());var l=LaunchPresets.All(c.Court,c.Physics.Gravity);Assert.That(l.Select(e=>e.Id),Is.EqualTo(new[]{"L1","L2","L3","L4","L5","L6"}));Assert.That(l[1].Launch.Position,Is.EqualTo(new Vector3(.3f,.35f,4)));Assert.That(l[1].HumanXZ,Is.EqualTo(new Vector2(3.2f,5.5f)));}
    }
}
