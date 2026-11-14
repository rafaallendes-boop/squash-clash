using System;
using System.Collections.Generic;
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
    // Distance-aware pace curve (design 2026-11-14, section 5 and 9.2). One shared function; no Delta, category or time input.
    public sealed class ShotPaceTests
    {
        private const float R=.02f;
        private static readonly ShotPaceSettings S=new ShotPaceSettings(6,28,2,1.5f,6);
        private static readonly Vector2 Aim=new Vector2(2.6f,1.2f);
        // A contact straight behind the aimed point at 3D distance d, so D is exactly d.
        private static Vector3 At(float d)=>new Vector3(Aim.x,Aim.y,R+d);
        private static float Linear(float p,ShotPaceSettings s)=>s.VMin+p*(s.VMax-s.VMin);
        private static readonly float[] Distances={.5f,1.5f,2f,3.7f,5.99f,6f,7.5f,9f};

        [Test] public void Extremes_P0IsVMin_P1IsVMax_AtAnyDistance()
        {
            foreach(float d in Distances)
            {
                Assert.That(ShotPace.Requested(0,At(d),Aim,R,S).Speed,Is.EqualTo(6f),"P=0 d="+d);
                Assert.That(ShotPace.Requested(1,At(d),Aim,R,S).Speed,Is.EqualTo(28f),"P=1 d="+d);
            }
        }
        [Test] public void Monotonic_InPower_AtEachDistance()
        {
            foreach(float d in Distances)
            {
                float last=-1;
                for(int k=0;k<=20;k++){float v=ShotPace.Requested(k/20f,At(d),Aim,R,S).Speed;Assert.That(v,Is.GreaterThan(last),"d="+d+" k="+k);last=v;}
            }
        }
        [Test] public void Monotonic_InDistance_AtEachInteriorPower()
        {
            for(int k=1;k<20;k++)
            {
                float last=-1;
                for(float d=.5f;d<=9f;d+=.25f){float v=ShotPace.Requested(k/20f,At(d),Aim,R,S).Speed;Assert.That(v,Is.GreaterThanOrEqualTo(last),"k="+k+" d="+d);last=v;}
            }
        }
        [Test] public void GammaMax_AtOrBelowNear_And_LinearAtOrBeyondFull()
        {
            Assert.That(ShotPace.Requested(.5f,At(1.5f),Aim,R,S).Gamma,Is.EqualTo(2f).Within(1e-5));
            Assert.That(ShotPace.Requested(.5f,At(.6f),Aim,R,S).Gamma,Is.EqualTo(2f).Within(1e-5));
            Assert.That(ShotPace.Requested(.5f,At(6f),Aim,R,S).Gamma,Is.EqualTo(1f));
            Assert.That(ShotPace.Requested(.5f,At(8f),Aim,R,S).Gamma,Is.EqualTo(1f));
            Assert.That(ShotPace.Requested(.5f,At(1.5f),Aim,R,S).Speed,Is.EqualTo(6+.25f*22).Within(1e-4),"P^2 at the near distance");
        }
        [Test] public void Continuous_AtNearAndFullDistance()
        {
            foreach(float edge in new[]{1.5f,6f})
                foreach(float p in new[]{.2f,.5f,.8f})
                {
                    float below=ShotPace.Requested(p,At(edge-1e-3f),Aim,R,S).Speed,above=ShotPace.Requested(p,At(edge+1e-3f),Aim,R,S).Speed;
                    Assert.That(Math.Abs(below-above),Is.LessThan(.01f),"edge="+edge+" p="+p);
                }
            foreach(float d in new[]{3f,3.7f})
            {
                float last=ShotPace.Requested(0,At(d),Aim,R,S).Speed;
                for(int k=1;k<=200;k++){float v=ShotPace.Requested(k/200f,At(d),Aim,R,S).Speed;Assert.That(v-last,Is.LessThan(.5f),"p step d="+d);last=v;}
            }
        }
        [Test] public void FarFromFront_AndGammaMaxOne_AreBitIdenticalToTheLinearFormula()
        {
            var off=new ShotPaceSettings(6,28,1,1.5f,6);
            for(int k=0;k<=40;k++)
            {
                float p=k/40f;
                foreach(float d in Distances.Where(x=>x>=6f))
                {var r=ShotPace.Requested(p,At(d),Aim,R,S);Assert.That(BitConverter.SingleToInt32Bits(r.Speed),Is.EqualTo(BitConverter.SingleToInt32Bits(Linear(p,S))),"far k="+k+" d="+d);Assert.That(r.Speed,Is.EqualTo(r.LinearSpeed));}
                foreach(float d in Distances)
                {var r=ShotPace.Requested(p,At(d),Aim,R,off);Assert.That(BitConverter.SingleToInt32Bits(r.Speed),Is.EqualTo(BitConverter.SingleToInt32Bits(Linear(p,off))),"gamma1 k="+k+" d="+d);}
            }
        }
        [Test] public void Deterministic_SameInputsSameBits_AndLinearSpeedIsAlwaysTheCurrentFormula()
        {
            var a=ShotPace.Requested(.37f,At(2.3f),Aim,R,S);var b=ShotPace.Requested(.37f,At(2.3f),Aim,R,S);
            Assert.That(a.Speed,Is.EqualTo(b.Speed));Assert.That(a.Gamma,Is.EqualTo(b.Gamma));Assert.That(a.DistanceM,Is.EqualTo(b.DistanceM));
            Assert.That(a.LinearSpeed,Is.EqualTo(Linear(.37f,S)));Assert.That(a.DistanceM,Is.EqualTo(2.3f).Within(1e-5));
            Assert.That(a.Speed,Is.LessThan(a.LinearSpeed),"the curve softens a mid power near the front wall");
        }
        [Test] public void DistanceIs3D_IncludesHeightAndDirection()
        {
            var contact=new Vector3(1.6f,.6f,R+1.2f);var aim=new Vector2(2.6f,1.4f);
            double expected=Math.Sqrt(1.0*1.0+.8*.8+1.2*1.2);
            Assert.That(ShotPace.Requested(.5f,contact,aim,R,S).DistanceM,Is.EqualTo((float)expected).Within(1e-4));
        }
        [Test] public void SettingsFromParamSet_ReadTheThreeNewKeys_AndDefaultsAreTheDesignValues()
        {
            var p=ConfigFixtures.Default();
            Assert.That(p.Get("shot.paceGammaMax"),Is.EqualTo(2));Assert.That(p.Get("shot.paceNearM"),Is.EqualTo(1.5));Assert.That(p.Get("shot.paceFullM"),Is.EqualTo(6));
            foreach(var key in new[]{"shot.paceGammaMax","shot.paceNearM","shot.paceFullM"})
            {Assert.That(ParamCatalog.TryGet(key,out var def),Is.True,key);Assert.That(def.Label,Does.Contain("EXPERIMENT"));}
            ParamCatalog.TryGet("shot.paceGammaMax",out var g);Assert.That(g.Min,Is.EqualTo(1));Assert.That(g.Max,Is.EqualTo(3));
            ParamCatalog.TryGet("shot.paceNearM",out var n);Assert.That(n.Min,Is.EqualTo(.5));Assert.That(n.Max,Is.EqualTo(4));
            ParamCatalog.TryGet("shot.paceFullM",out var f);Assert.That(f.Min,Is.EqualTo(2));Assert.That(f.Max,Is.EqualTo(9.75));
            var s=ShotPaceSettings.From(p);Assert.That(s.GammaMax,Is.EqualTo(2f));Assert.That(s.NearM,Is.EqualTo(1.5f));Assert.That(s.FullM,Is.EqualTo(6f));Assert.That(s.VMin,Is.EqualTo(6f));Assert.That(s.VMax,Is.EqualTo(28f));
        }
        [TestCase(1.5,6,true)][TestCase(2,6,true)][TestCase(6,6,false)][TestCase(6,2,false)][TestCase(4,3,false)]
        public void Validator_RequiresNearBelowFull(double near,double full,bool valid)
            =>Assert.That(ShotConfigValidator.TryValidate(new Dictionary<string,double>{{"shot.paceNearM",near},{"shot.paceFullM",full}},out _),Is.EqualTo(valid));

        // Routing: one shared function for the human, the rival, the AI forecast, and the order curve -> causal -> bound -> solver.
        private BodyFrame body=new BodyFrame(new Vector2(3.2f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f);
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
        [Test] public void HumanRivalAndForecast_UseTheSameCurve_CurveThenCausalThenBoundThenSolver()
        {
            var p=ConfigFixtures.Default();var cfg=EffectiveConfig.From(p);var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var context=new RallyContext(cfg,new RallyParams(),factory);var reference=Reference();
            var contact=new Vector3(2.7f,.6f,2.1f);var aim=new Vector2(2.6f,1.3f);float power=.5f;
            var expected=ShotPace.Requested(power,contact,aim,cfg.Physics.BallRadius,p);
            Assert.That(expected.Speed,Is.LessThan(expected.LinearSpeed),"contact is near the front wall");
            var human=CausalQuality.Solve(reference,body,contact,aim,power,cfg,factory);var rival=RallyContact.Solve(context,reference,body,contact,aim,power);
            Assert.That(human.Pace.Speed,Is.EqualTo(expected.Speed));Assert.That(human.Pace.Gamma,Is.EqualTo(expected.Gamma));Assert.That(human.Pace.DistanceM,Is.EqualTo(expected.DistanceM));
            Assert.That(human.Pace.LinearSpeed,Is.EqualTo(expected.LinearSpeed));Assert.That(rival.Pace.Speed,Is.EqualTo(human.Pace.Speed));
            // The distance uses the aim requested BEFORE the causal layer, never the composed one.
            Assert.That(human.Input.UnclampedAim.y,Is.GreaterThan(aim.y));Assert.That(human.Pace.DistanceM,Is.EqualTo(expected.DistanceM).Within(0));
            var settings=CausalSettings.From(p);
            Assert.That(human.Input.Speed,Is.EqualTo(CausalQuality.Apply(aim,expected.Speed,human.Vectors,settings).Speed),"causal fraction applies to the curve speed");
            Assert.That(human.Input.Speed,Is.Not.EqualTo(CausalQuality.Apply(aim,expected.LinearSpeed,human.Vectors,settings).Speed));
            Assert.That(human.Solution.Solved,Is.True);Assert.That(human.Solution.SpeedRaised,Is.False);Assert.That(human.Solution.Speed,Is.EqualTo(human.Input.Speed),"the solver receives the composed speed unchanged");
            var forecast=RallyContact.SolveIntention(context,contact,aim,power);
            Assert.That(forecast.Solved,Is.True);Assert.That(forecast.Speed,Is.EqualTo(expected.Speed),"AI forecast uses the same curve speed");
            Assert.That(factory.LiveInstances,Is.Zero);
        }
        [Test] public void FarContact_RoutesIdenticallyToTheOldLinearSpeed()
        {
            var p=ConfigFixtures.Default();var cfg=EffectiveConfig.From(p);var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var context=new RallyContext(cfg,new RallyParams(),factory);
            var contact=new Vector3(2.7f,.6f,8.2f);var aim=new Vector2(2.6f,1.2f);
            var pace=ShotPace.Requested(.5f,contact,aim,cfg.Physics.BallRadius,p);Assert.That(pace.Gamma,Is.EqualTo(1f));
            var old=ShotSolver.Solve(new ShotRequest(contact,aim,.5f),context.VMin,context.VMax,context.VCap,cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court,factory,cfg.Physics);
            var now=RallyContact.SolveIntention(context,contact,aim,.5f);
            Assert.That(now.Solved,Is.EqualTo(old.Solved));Assert.That(now.Speed,Is.EqualTo(old.Speed));Assert.That(now.Velocity,Is.EqualTo(old.Velocity));Assert.That(now.TargetUsed,Is.EqualTo(old.TargetUsed));
        }
        [Test] public void Trace_And_Record_CarryTheFourPaceFields()
        {
            var p=ConfigFixtures.Default();var cfg=EffectiveConfig.From(p);var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var reference=Reference();var result=CausalQuality.Solve(reference,body,new Vector3(2.7f,.6f,2.1f),new Vector2(2.6f,1.3f),.5f,cfg,factory);
            var trace=CausalShotTrace.Create("Human",1,"a",150,reference,body,result);
            Assert.That(trace.paceDistanceM,Is.EqualTo(result.Pace.DistanceM));Assert.That(trace.paceGamma,Is.EqualTo(result.Pace.Gamma));
            Assert.That(trace.requestedSpeedLinear,Is.EqualTo(result.Pace.LinearSpeed));Assert.That(trace.requestedSpeed,Is.EqualTo(result.Pace.Speed));
            Assert.That(trace.paceDistanceM,Is.GreaterThan(0));Assert.That(trace.paceGamma,Is.GreaterThan(1));Assert.That(trace.requestedSpeed,Is.LessThan(trace.requestedSpeedLinear));
            var json=JsonUtility.ToJson(trace);foreach(var f in new[]{"paceDistanceM","paceGamma","requestedSpeedLinear","requestedSpeed"})Assert.That(json,Does.Contain("\""+f+"\""));
            var rj=new ShotRecord{PaceDistanceM=1.5f,PaceGamma=2,RequestedSpeedLinear=17,RequestedSpeed=12}.ToJson();
            foreach(var f in new[]{"PaceDistanceM","PaceGamma","RequestedSpeedLinear","RequestedSpeed"})Assert.That(rj,Does.Contain("\""+f+"\""));
            var back=JsonUtility.FromJson<ShotRecord>(rj);Assert.That(back.RequestedSpeed,Is.EqualTo(12f));Assert.That(back.PaceGamma,Is.EqualTo(2f));
        }
    }
}
