using System;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    // F2: the striker's arm (spec 4, A5, A4). The vector «fatigue» only slows the shot; position vectors grow by (1+gain); timing and aim stay.
    public sealed class FatigueArmTests
    {
        private readonly BodyFrame body=new BodyFrame(new Vector2(3.2f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f);
        private readonly Vector3 contact=new Vector3(2.6f,.6f,5.5f);private readonly Vector2 aim=new Vector2(2.6f,1.5f);
        private static readonly AnalyticBallSimulationFactory Factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
        private CausalReference Reference()
        {
            var flight=new Trajectory();
            for(int i=0;i<=230;i++)flight.States.Add(new BallState{Step=i,Position=new Vector3(2.6f,.6f,5.5f+(i-150)*.01f),Velocity=new Vector3(0,0,1)});
            Func<SurfaceId,long,float,ContactEvent> ev=(surface,step,height)=>{var n=CourtDimensions.InwardNormal(surface);return new ContactEvent(surface,new Vector3(2.6f,height,5.5f)-n*.02f,Vector2.zero,n,step,0,ContactTiming.Exact,Vector3.zero,false);};
            flight.Contacts.Add(ev(SurfaceId.FrontWall,1,1.2f));flight.Contacts.Add(ev(SurfaceId.Floor,101,.02f));flight.Contacts.Add(ev(SurfaceId.Floor,201,.02f));
            var plan=new ContactPlan(true,new SimTime(100),new SimTime(200),150,flight.States[150].Position,StrokeSide.Drive,1,body,new LegalSettings(.05f,2,100,0),CourtDimensions.Standard());
            var clock=new ClockMap();clock.Begin(1,0,0,1,1,100);
            return CausalQuality.Prepare(flight,plan,clock,new TimingSettings(40,true,100,false,0),0,1);
        }
        private static FatigueSettings Settings(Func<ParamSet,ParamSet> edit=null)=>FatigueSettings.From(edit==null?ConfigFixtures.Default():edit(ConfigFixtures.Default()));
        [Test] public void A5_ExhaustedButWellPlaced_KeepsTheAim_AndOnlyLosesArmSpeed()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var r=Reference();
            var plain=CausalQuality.Solve(r,body,contact,aim,.5f,cfg,Factory);
            Assert.That(plain.Vectors.All(v=>v.DeltaHeight==0&&v.DeltaSpeedFraction==0),Is.True,"precondition: well placed and on time");
            Assert.That(plain.Solution.Solved,Is.True,"precondition");Assert.That(plain.Solution.SpeedRaised,Is.False,"precondition");
            var settings=Settings();var arm=settings.Arm(0);
            var tired=CausalQuality.Solve(r,body,contact,aim,.5f,cfg,Factory,arm);
            var fatigue=tired.Vectors.Where(v=>v.Cause=="fatigue").ToArray();
            Assert.That(fatigue,Has.Length.EqualTo(1),"exactly one fatigue vector at E=0");
            Assert.That(tired.Vectors.Where(v=>v.Cause!="fatigue").All(v=>v.DeltaHeight==0&&v.DeltaSpeedFraction==0),Is.True,"no other vector");
            Assert.That(fatigue[0].DeltaHeight,Is.Zero);Assert.That(fatigue[0].DeltaLateral,Is.Zero);Assert.That(fatigue[0].DeltaSpeedFraction,Is.EqualTo(-(float)settings.ArmSpeedMax).Within(1e-7));
            Assert.That(tired.Input.Aim,Is.EqualTo(aim));Assert.That(tired.Input.UnclampedAim,Is.EqualTo(aim));Assert.That(tired.Input.Clamped,Is.False);
            Assert.That(tired.Input.Speed,Is.EqualTo(plain.Pace.Speed*(1-(float)settings.ArmSpeedMax)).Within(1e-4));
            Assert.That(tired.Solution.Solved,Is.True);Assert.That(tired.Solution.SpeedRaised,Is.False,"the solver does not absorb the loss");
            Assert.That(tired.Solution.Speed,Is.LessThan(plain.Solution.Speed));
        }
        [Test] public void PositionVectors_GrowByOnePlusGain_TimingAndAimStay()
        {
            var timing=new CausalVector(.3f,-.1f,"timing");var stretch=new CausalVector(.4f,-.2f,"stretch");var jam=new CausalVector(.3f,-.15f,"jam");
            var input=new[]{timing,stretch,jam};var arm=new FatigueArm(.04f,.25f,1);
            var output=CausalQuality.ApplyFatigue(input,arm);
            Assert.That(output,Is.Not.SameAs(input));Assert.That(input[1].DeltaHeight,Is.EqualTo(.4f),"input is never mutated");
            Assert.That(output[0],Is.EqualTo(timing));
            Assert.That(output[1].DeltaHeight,Is.EqualTo(.4f*1.25f).Within(1e-6));Assert.That(output[1].DeltaSpeedFraction,Is.EqualTo(-.2f*1.25f).Within(1e-6));
            Assert.That(output[2].DeltaHeight,Is.EqualTo(.3f*1.25f).Within(1e-6));Assert.That(output[2].DeltaSpeedFraction,Is.EqualTo(-.15f*1.25f).Within(1e-6));
            Assert.That(output.Last().Cause,Is.EqualTo("fatigue"));Assert.That(output.Last().DeltaSpeedFraction,Is.EqualTo(-.04f));Assert.That(output.Length,Is.EqualTo(4));
        }
        [Test] public void NoFatigue_ReturnsTheSameArray()
        {
            var input=new[]{new CausalVector(.3f,-.1f,"timing"),new CausalVector(.4f,-.2f,"stretch")};
            Assert.That(CausalQuality.ApplyFatigue(input,FatigueArm.None),Is.SameAs(input));
            Assert.That(CausalQuality.ApplyFatigue(input,Settings().Arm(100)),Is.SameAs(input),"E at or above onset is f=0");
            Assert.That(CausalQuality.ApplyFatigue(input,Settings().Arm(70)),Is.SameAs(input));
        }
        [Test] public void Arm_ScalesWithFactor_AndIsNoneWhenOffOrZero()
        {
            var s=Settings();var half=s.Arm(35);
            Assert.That(half.Factor,Is.EqualTo(.5).Within(1e-12));Assert.That(half.SpeedLoss,Is.EqualTo((float)(s.ArmSpeedMax*.5)));Assert.That(half.PositionGain,Is.EqualTo((float)(s.PositionGainMax*.5)));
            var full=s.Arm(0);Assert.That(full.SpeedLoss,Is.EqualTo((float)s.ArmSpeedMax));Assert.That(full.PositionGain,Is.EqualTo((float)s.PositionGainMax));Assert.That(full.Active,Is.True);
            Assert.That(Settings(p=>p.With("fatigue.enabled",0)).Arm(0).Active,Is.False);
            Assert.That(Settings(p=>p.With("fatigue.armSpeedMax",0).With("fatigue.positionGainMax",0)).Arm(0).Active,Is.False,"zero keys are inactive");
        }
        [Test] public void SaturatedCaps_DecideTheShot_FatigueAddsNothingPastThem()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var r=Reference();var far=new Vector3(5.6f,.6f,5.5f);
            var plain=CausalQuality.Solve(r,body,far,aim,.5f,cfg,Factory);
            Assert.That(plain.Vectors.First(v=>v.Cause=="timing").DeltaSpeedFraction,Is.EqualTo(-.4f),"precondition: timing already at its speed cap");
            var tired=CausalQuality.Solve(r,body,far,aim,.5f,cfg,Factory,Settings().Arm(0));
            Assert.That(tired.Input.Speed,Is.EqualTo(plain.Input.Speed));Assert.That(tired.Input.Aim,Is.EqualTo(plain.Input.Aim));
        }
        [Test] public void HumanAndRival_SharedPath_SameResultWithTheSameArm()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var context=new RallyContext(cfg,new RallyParams(),Factory);var r=Reference();
            var arm=Settings().Arm(20);Assert.That(arm.Active,Is.True);
            var human=CausalQuality.Solve(r,body,new Vector3(2f,.9f,5.5f),aim,.5f,cfg,Factory,arm);var rival=RallyContact.Solve(context,r,body,new Vector3(2f,.9f,5.5f),aim,.5f,arm);
            Assert.That(human.Vectors.Any(v=>v.Cause=="fatigue"),Is.True);Assert.That(human.Vectors.Any(v=>v.Cause=="stretch"),Is.True);
            Assert.That(rival.Vectors,Is.EqualTo(human.Vectors));Assert.That(rival.Input.Aim,Is.EqualTo(human.Input.Aim));Assert.That(rival.Input.Speed,Is.EqualTo(human.Input.Speed));Assert.That(rival.Solution.Velocity,Is.EqualTo(human.Solution.Velocity));
        }
        [Test] public void SevenArgumentSolve_IsTheNoFatigueSolve()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var r=Reference();var p=new Vector3(2f,.9f,5.5f);
            var a=CausalQuality.Solve(r,body,p,aim,.5f,cfg,Factory);var b=CausalQuality.Solve(r,body,p,aim,.5f,cfg,Factory,FatigueArm.None);
            Assert.That(b.Vectors,Is.EqualTo(a.Vectors));Assert.That(b.Input.Speed,Is.EqualTo(a.Input.Speed));Assert.That(b.Solution.Velocity,Is.EqualTo(a.Solution.Velocity));
            Assert.That(a.Vectors.Any(v=>v.Cause=="fatigue"),Is.False);
        }
        [Test] public void Tone_Edges_NeutralAmberRed()
        {
            var s=Settings();
            Assert.That(s.Tone(100),Is.EqualTo(0));Assert.That(s.Tone(70),Is.EqualTo(0),"at onset nothing is noticed");
            Assert.That(s.Tone(69.99),Is.EqualTo(1));Assert.That(s.Tone(25),Is.EqualTo(1),"red is strictly below 25");
            Assert.That(s.Tone(24.99),Is.EqualTo(2));Assert.That(s.Tone(0),Is.EqualTo(2));
            Assert.That(Settings(p=>p.With("fatigue.enabled",0)).Tone(0),Is.EqualTo(0));
        }
    }
}
