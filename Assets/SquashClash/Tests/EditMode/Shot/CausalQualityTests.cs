using System;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Tests.Config;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class CausalQualityTests
    {
        private Trajectory flight;private ContactPlan plan;private ClockMap clock;private PresentedPlans plans;
        private BodyFrame body;private CausalSettings settings;private TimingSettings timing;
        [SetUp] public void Setup()
        {
            body=new BodyFrame(new Vector2(3.2f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f);
            flight=new Trajectory();
            for(int i=0;i<=230;i++)flight.States.Add(new BallState{Step=i,Position=new Vector3(2.6f,.6f,5.5f+(i-150)*.01f),Velocity=new Vector3(0,0,1)});
            flight.Contacts.Add(Event(SurfaceId.FrontWall,1,1.2f));flight.Contacts.Add(Event(SurfaceId.Floor,101,.02f));flight.Contacts.Add(Event(SurfaceId.Floor,201,.02f));
            plan=new ContactPlan(true,new SimTime(100),new SimTime(200),150,flight.States[150].Position,StrokeSide.Drive,1,body,new LegalSettings(.05f,2,100,0),CourtDimensions.Standard());
            clock=new ClockMap();clock.Begin(1,0,0,1,1,100);timing=new TimingSettings(40);settings=new CausalSettings(.6f);
            plans=new PresentedPlans();plans.Present(0,plan,1.5);
        }
        private ContactEvent Event(SurfaceId surface,long step,float height)
        {var n=CourtDimensions.InwardNormal(surface);return new ContactEvent(surface,new Vector3(2.6f,height,5.5f)-n*.02f,Vector2.zero,n,step,0,ContactTiming.Exact,Vector3.zero,false);}
        private CausalReference Reference(double tempo=1,double comp=0)
        {clock.Begin(1,0,0,tempo,1,100);return CausalQuality.Prepare(flight,plan,clock,new TimingSettings(40,true,100,false,comp),0,tempo);}
        private CausalVector Timing(CausalReference r,Vector3 q,BodyFrame? b=null)
        {var v=CausalQuality.Evaluate(r,b??body,q,settings);Assert.That(v.Any(x=>x.Cause=="timing"),Is.True);return v.First(x=>x.Cause=="timing");}
        [Test] public void AllReachablePerfectContacts_InsideZone_ButKeepStretchAndJam()
        {
            var r=Reference();Assert.That(r.PerfectSteps,Is.EquivalentTo(new long[]{150,151,152,153,154}));
            foreach(long j in r.PerfectSteps)Assert.That(Timing(r,flight.States[(int)j].Position).DeltaHeight,Is.Zero);
            var jam=new BodyFrame(new Vector2(2.6f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f);
            var v=CausalQuality.Evaluate(r,jam,new Vector3(2.6f,.9f,5.5f),settings);
            Assert.That(v.Any(x=>x.Cause=="jam"&&x.DeltaHeight>0),Is.True);
            var stretch=CausalQuality.Evaluate(r,body,new Vector3(2f,.9f,5.5f),settings);
            Assert.That(stretch.Any(x=>x.Cause=="stretch"&&x.DeltaHeight>0),Is.True);
        }
        [Test] public void TimingVector_GrowsContinuouslyWithDistanceToZone()
        {
            var r=Reference();var edge=flight.States[154].Position;
            Assert.That(r.Distance(edge,body),Is.Zero);
            foreach(float d in new[]{.00001f,.01f,.1f,.5f})
            {var q=edge+Vector3.right*d;var v=Timing(r,q);Assert.That(v.DeltaHeight,Is.EqualTo(.6f*d).Within(1e-6));Assert.That(v.DeltaSpeedFraction,Is.EqualTo(-.25f*d).Within(1e-6));}
        }
        [Test] public void NoVector_HasLateralComponent()
        {var v=CausalQuality.Evaluate(Reference(),body,new Vector3(2,.9f,5.5f),settings);Assert.That(v.Length,Is.GreaterThan(0));Assert.That(v.All(x=>x.DeltaLateral==0),Is.True);}
        [Test] public void SameGeometryDifferentDelta_SameShot()
        {
            clock.Begin(1,0,0,1,1,120);plans=new PresentedPlans();plans.Present(0,plan,150/120d);
            var a=ContactContract.Decide(150/120d+.039,plans,clock,flight,timing,false);var b=ContactContract.Decide(150/120d+.041,plans,clock,flight,timing,false);
            Assert.That(a.Result,Is.EqualTo(ShotResult.Perfect));Assert.That(b.Result,Is.EqualTo(ShotResult.Valid));Assert.That(a.ContactStep,Is.EqualTo(b.ContactStep));
            var r=CausalQuality.Prepare(flight,plan,clock,timing,0,1);Assert.That(r.PerfectSteps.Count,Is.GreaterThan(0));
            var va=CausalQuality.Evaluate(r,body,flight.States[(int)a.ContactStep].Position,settings);var vb=CausalQuality.Evaluate(r,body,flight.States[(int)b.ContactStep].Position,settings);
            Assert.That(va,Is.EqualTo(vb));Assert.That(CausalQuality.Apply(new Vector2(2.6f,1.5f),15,va,settings).Speed,Is.EqualTo(CausalQuality.Apply(new Vector2(2.6f,1.5f),15,vb,settings).Speed));
        }
        [Test] public void Caps_ArePerComponent()
        {
            var v=new[]{new CausalVector(.9f,-.3f,"timing"),new CausalVector(.4f,-.2f,"stretch")};var applied=CausalQuality.Apply(new Vector2(2,1.5f),20,v,settings);
            Assert.That(applied.Aim,Is.EqualTo(new Vector2(2,2.5f)));Assert.That(applied.Speed,Is.EqualTo(12).Within(1e-6));
            var timingVector=Timing(Reference(),new Vector3(5.6f,.6f,5.5f));Assert.That(timingVector.DeltaHeight,Is.EqualTo(1));Assert.That(timingVector.DeltaSpeedFraction,Is.EqualTo(-.4f));
        }
        [Test] public void MirroredSide_IsSymmetric()
        {
            var r=Reference();var q=new Vector3(2,.9f,5.5f);var v=CausalQuality.Evaluate(r,body,q,settings);
            var mirror=new Trajectory();foreach(var b in flight.States){var s=b;s.Position.x=6.4f-b.Position.x;s.Velocity.x=-b.Velocity.x;mirror.States.Add(s);}mirror.Contacts.AddRange(flight.Contacts.Select(e=>new ContactEvent(e.Surface,new Vector3(6.4f-e.Point.x,e.Point.y,e.Point.z),e.UV,e.Normal,e.Step,e.TimeInStep,e.Timing,e.ApproachVelocity,e.Persistent)));
            var mb=new BodyFrame(new Vector2(3.2f,5.5f),false,1.3f,.6f,.4f,1.1f,.3f);
            var mp=new ContactPlan(true,plan.KIn,plan.KOut,150,mirror.States[150].Position,StrokeSide.Drive,1,mb,plan.Legal,plan.Court);
            var mr=CausalQuality.Prepare(mirror,mp,clock,timing,0,1);var mv=CausalQuality.Evaluate(mr,mb,new Vector3(6.4f-q.x,q.y,q.z),settings);
            Assert.That(v.Length,Is.GreaterThan(0));Assert.That(mv.Length,Is.EqualTo(v.Length));for(int i=0;i<v.Length;i++){Assert.That(mv[i].DeltaHeight,Is.EqualTo(v[i].DeltaHeight).Within(1e-6));Assert.That(mv[i].DeltaSpeedFraction,Is.EqualTo(v[i].DeltaSpeedFraction).Within(1e-6));}
        }
        [Test] public void ZonePreparedOncePerReception_IndependentOfAttemptCategory()
        {var r=Reference();var before=r.PerfectSteps.ToArray();Assert.That(before.Length,Is.GreaterThan(0));var d=r.Distance(new Vector3(3,.7f,5.5f),body);clock.Mark(3,300,0,.3,1,false);flight.States.Clear();Assert.That(r.Distance(new Vector3(3,.7f,5.5f),body),Is.EqualTo(d));Assert.That(r.PerfectSteps,Is.EqualTo(before));}
        [Test] public void Determinism_SameInputsSameVectorAndShot()
        {var a=Reference();var b=Reference();Assert.That(a.PerfectSteps.Count,Is.GreaterThan(0));Assert.That(a.PerfectSteps,Is.EqualTo(b.PerfectSteps));Assert.That(CausalQuality.Evaluate(a,body,new Vector3(2.7f,.6f,5.5f),settings),Is.EqualTo(CausalQuality.Evaluate(b,body,new Vector3(2.7f,.6f,5.5f),settings)));}
        [Test] public void DifferentTempo_DifferenceDocumented_NotRequiredEqual()
        {var fast=Reference(1);var slow=Reference(.3);Assert.That(fast.ArcRadius,Is.GreaterThan(slow.ArcRadius));TestContext.WriteLine("fast L="+fast.ArcRadius+" slow L="+slow.ArcRadius);}
        [Test] public void Edges_ExactWindows_KOutNeighborAndEarlierTie()
        {
            var r=Reference();Assert.That(r.PerfectSteps,Does.Contain(154));Assert.That(r.PerfectSteps,Is.Not.Contains(155L));
            foreach(double delta in new[]{-.1,.1})Assert.That(ContactContract.Decide(1.5+delta,plans,clock,flight,timing,false).Result,Is.EqualTo(ShotResult.Valid));
            clock.Begin(1,0,0,1,1,128);plans=new PresentedPlans();plans.Present(0,plan,150/128d);
            var tie=ContactContract.Decide(155.5/128d,plans,clock,flight,new TimingSettings(0,false,100),false);Assert.That(tie.ContactStep,Is.EqualTo(155));
            var neighbor=ContactContract.Decide(199.8/128d,plans,clock,flight,new TimingSettings(0,false,100),false);Assert.That(neighbor.ContactStep,Is.EqualTo(199));Assert.That(neighbor.UsedNeighborLegalStep,Is.True);
        }

        [Test] public void CausalKeys_AreExperimentalReceptionDefaults_AndSnapshotValues()
        {
            var p=ConfigFixtures.Default();var keys=new[]{"timingHeightPerM","timingSpeedPerM","stretchStart","jamDistance","stretchHeight","stretchSpeed","jamHeight","jamSpeed","capHeight","capSpeed"};
            var values=new double[]{.6,-.25,.85,.35,.4,-.2,.3,-.15,1,-.4};
            for(int i=0;i<keys.Length;i++)
            {string key="causal."+keys[i];Assert.That(ParamCatalog.TryGet(key,out var def),Is.True,key);Assert.That(p.Get(key),Is.EqualTo(values[i]));Assert.That(def.Label,Does.Contain("EXPERIMENT"));Assert.That(def.Apply,Is.EqualTo(ApplyClass.NextLaunch));}
            Assert.That(p.Ordered.Count,Is.EqualTo(121));Assert.That(EffectiveConfig.From(p).SimulationKey,Is.EqualTo("166a96e319c7"));
        }
        [Test] public void AppliedBeforeSolver_NeverCorrectedAfter()
        {
            var p=ConfigFixtures.Default();var cfg=EffectiveConfig.From(p);var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var point=new Vector3(2.7f,.6f,5.5f);var aim=new Vector2(2.6f,1.5f);var r=Reference();
            var evaluated=CausalQuality.Solve(r,body,point,aim,.5f,cfg,factory);
            Assert.That(evaluated.Vectors,Is.Not.Null);Assert.That(evaluated.Input.Aim.y,Is.GreaterThan(aim.y));
            Assert.That(evaluated.Input.Speed,Is.LessThan(17));
            var direct=ShotSolver.Solve(new ShotRequest(point,evaluated.Input.Aim,.5f),evaluated.Input.Speed,evaluated.Input.Speed,32,cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court,factory,cfg.Physics);
            Assert.That(evaluated.Solution.Solved,Is.True);Assert.That(evaluated.Solution.Velocity,Is.EqualTo(direct.Velocity));Assert.That(evaluated.Solution.Speed,Is.EqualTo(direct.Speed));Assert.That(evaluated.Solution.TargetUsed,Is.EqualTo(direct.TargetUsed));
            Assert.That(factory.LiveInstances,Is.Zero);
        }

        [Test] public void HumanAndRival_SharedPath_SameResult()
        {
            var p=ConfigFixtures.Default();var cfg=EffectiveConfig.From(p);var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var context=new RallyContext(cfg,new RallyParams(),factory);var reference=Reference();var contact=new Vector3(2.7f,.6f,5.5f);var aim=new Vector2(2.6f,1.5f);
            var human=CausalQuality.Solve(reference,body,contact,aim,.5f,cfg,factory);var rival=RallyContact.Solve(context,reference,body,contact,aim,.5f);
            Assert.That(rival.Vectors,Is.EqualTo(human.Vectors));Assert.That(rival.Input.Aim,Is.EqualTo(human.Input.Aim));Assert.That(rival.Input.Speed,Is.EqualTo(human.Input.Speed));Assert.That(rival.Solution.Velocity,Is.EqualTo(human.Solution.Velocity));
        }
        [Test] public void EffectiveBody_IsFrozenAtEachContactStep()
        {
            var r=CausalQuality.Prepare(flight,plan,clock,timing,0,1,j=>new BodyFrame(new Vector2(3.2f+(j-150)*.002f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f));
            Assert.That(r.BodyAt(153).PositionXZ.x,Is.EqualTo(3.206f).Within(1e-6));Assert.That(r.BodyAt(153).Reach,Is.EqualTo(body.Reach));
            foreach(long j in r.PerfectSteps)Assert.That(r.Distance(flight.States[(int)j].Position,r.BodyAt(j)),Is.Zero);
        }
        private static float LegalFrontMax(EffectiveConfig cfg)=>cfg.Court.FrontOutLower-cfg.Physics.BallRadius;
        [Test] public void CausalHeightNearOutLine_IsBoundedToLegalFrontMax_AndSolves()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var r=Reference();var contact=new Vector3(2f,.9f,5.5f);var aim=new Vector2(2.6f,4.5f);
            var vectors=CausalQuality.Evaluate(r,body,contact,CausalSettings.From(cfg.Source));
            Assert.That(vectors.Sum(v=>v.DeltaHeight),Is.GreaterThan(.1f),"precondition: the penalty must push the aim past the legal bound");
            var plain=CausalQuality.Apply(aim,10,vectors,CausalSettings.From(cfg.Source));Assert.That(plain.Aim.y,Is.GreaterThan(cfg.Court.FrontOutLower),"precondition");
            var result=CausalQuality.Solve(r,body,contact,aim,0,cfg,factory);
            Assert.That(result.Input.Clamped,Is.True);Assert.That(result.Input.Aim.y,Is.EqualTo(LegalFrontMax(cfg)).Within(2e-6f));Assert.That(result.Input.Aim.y,Is.LessThan(cfg.Court.FrontOutLower));
            Assert.That(result.Input.UnclampedAim.y,Is.GreaterThan(cfg.Court.FrontOutLower));Assert.That(result.Input.Aim.x,Is.EqualTo(aim.x));
            Assert.That(result.Solution.Reason,Is.Not.EqualTo("target-or-contact-outside-court"));Assert.That(result.Solution.Solved,Is.True,result.Solution.Reason);
            Assert.That(factory.LiveInstances,Is.Zero);
        }
        [Test] public void CausalHeightWithinLegalBound_IsNotClamped_AndSpeedIsNeverChangedByTheBound()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var r=Reference();var point=new Vector3(2.7f,.6f,5.5f);var aim=new Vector2(2.6f,1.5f);
            var result=CausalQuality.Solve(r,body,point,aim,.5f,cfg,factory);var plain=CausalQuality.Apply(aim,ShotPace.Requested(.5f,point,aim,cfg.Physics.BallRadius,cfg.Source).Speed,result.Vectors,CausalSettings.From(cfg.Source));
            Assert.That(result.Input.Clamped,Is.False);Assert.That(result.Input.Aim,Is.EqualTo(plain.Aim));Assert.That(result.Input.UnclampedAim,Is.EqualTo(plain.Aim));Assert.That(result.Input.Speed,Is.EqualTo(plain.Speed));
            var clamped=CausalQuality.Solve(r,body,new Vector3(2f,.9f,5.5f),new Vector2(2.6f,4.5f),.5f,cfg,factory);
            var unbounded=CausalQuality.Apply(new Vector2(2.6f,4.5f),clamped.Input.Speed/(1+Mathf.Max(-.4f,clamped.Vectors.Sum(v=>v.DeltaSpeedFraction))),clamped.Vectors,CausalSettings.From(cfg.Source));
            Assert.That(clamped.Input.Clamped,Is.True);Assert.That(clamped.Input.Speed,Is.EqualTo(unbounded.Speed).Within(1e-5f));
        }
        [Test] public void RequestedAimAlreadyAboveLegalMax_IsNeverLoweredByTheBound()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            float above=cfg.Court.FrontOutLower-cfg.Physics.BallRadius*.5f;
            var result=CausalQuality.Solve(Reference(),body,new Vector3(2f,.9f,5.5f),new Vector2(2.6f,above),0,cfg,factory);
            Assert.That(result.Input.Aim.y,Is.EqualTo(above));Assert.That(result.Input.Clamped,Is.True);Assert.That(result.Input.UnclampedAim.y,Is.GreaterThan(above));
        }
        [Test] public void NearOutAim_WithAnyPenalty_NeverFaultsInsideTheCausalLayer()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);var r=Reference();
            var contacts=new[]{new Vector3(2f,.9f,5.5f),new Vector3(2.7f,.6f,5.5f),new Vector3(2.6f,.9f,5.5f),new Vector3(4.2f,1.1f,5.3f)};
            for(float y=3.9f;y<=LegalFrontMax(cfg);y+=.07f)
                foreach(var c in contacts)
                {
                    var result=CausalQuality.Solve(r,body,c,new Vector2(2.6f,y),0,cfg,factory);
                    Assert.That(result.Solution.Reason,Is.Not.EqualTo("target-or-contact-outside-court"),"y="+y+" contact="+c);
                }
        }
        [Test] public void Trace_RecordsPreviousAndBoundedAim_AndClampFlag()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);var r=Reference();
            var result=CausalQuality.Solve(r,body,new Vector3(2f,.9f,5.5f),new Vector2(2.6f,4.5f),0,cfg,factory);
            var trace=CausalShotTrace.Create("Human",1,"a",150,r,body,result);
            Assert.That(trace.aimClamped,Is.True);Assert.That(trace.aimY,Is.EqualTo(result.Input.Aim.y));Assert.That(trace.unclampedAimY,Is.EqualTo(result.Input.UnclampedAim.y));Assert.That(trace.unclampedAimY,Is.GreaterThan(trace.aimY));
        }
        [Test] public void RivalPath_UsesTheSameBound()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var context=new RallyContext(cfg,new RallyParams(),factory);var r=Reference();var contact=new Vector3(2f,.9f,5.5f);var aim=new Vector2(2.6f,4.5f);
            var human=CausalQuality.Solve(r,body,contact,aim,0,cfg,factory);var rival=RallyContact.Solve(context,r,body,contact,aim,0);
            Assert.That(human.Input.Clamped,Is.True);Assert.That(rival.Input.Clamped,Is.EqualTo(human.Input.Clamped));Assert.That(rival.Input.Aim,Is.EqualTo(human.Input.Aim));Assert.That(rival.Input.UnclampedAim,Is.EqualTo(human.Input.UnclampedAim));
            Assert.That(rival.Solution.Reason,Is.EqualTo(human.Solution.Reason));Assert.That(rival.Solution.Velocity,Is.EqualTo(human.Solution.Velocity));
        }
        [Test] public void MovingBodySnapshot_IsTheSameStepper_WithoutMutatingThePlayer()
        {
            var cfg=EffectiveConfig.From(ConfigFixtures.Default());var context=new RallyContext(cfg,new RallyParams(),new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
            var player=new PlayerBody(PlayerId.Rival,new Vector2(3,6),context.Move);var target=new Vector2(2,5);player.MoveTo(target,10,BodyPhase.Approaching,BodyPhase.Set);
            var positions=player.PositionsAfter(160);Assert.That(positions.Length,Is.EqualTo(160));
            for(int i=0;i<positions.Length;i++)Assert.That(positions[i],Is.EqualTo(player.PositionAfter(target,10,i)),"step "+i);
            Assert.That(player.Position,Is.EqualTo(new Vector2(3,6)));Assert.That(player.Velocity,Is.EqualTo(Vector2.zero));
        }
    }
}
