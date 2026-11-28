using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // After the clock/rollback fixtures; deterministic host steps, including an independent point-freeze clock.
    public sealed class ZServeReceptionMarginTests
    {
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;private double wall;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Serve margin fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();rally=app.Rally;app.LaunchCurrent();
            app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;
            wall=0;PointEndFreeze.Now=()=>wall;
        }
        [UnityTearDown] public IEnumerator Cleanup(){PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}
        private void Step(double seconds){wall+=seconds;app.Host.Advance(seconds);controller.ProcessPending();rally.Tick();}
        private bool Drive(Func<bool> until,double max=120)
        {for(int i=0;i<(int)(max/StepReal);i++){if(until())return true;Step(StepReal);}return until();}
        private void At(double time)
        {while(app.Host.ObservedRealTime+StepReal<time)Step(StepReal);double left=time-app.Host.ObservedRealTime;if(left>0)Step(left);}
        private double Real(SimTime time)
        {Assert.That(app.Host.ClockMap.TrySimToReal(time.Step,time.Fraction,out double real),Is.True);return real;}
        private void RivalServe(RivalStyleId style=RivalStyleId.Definidor)
        {
            rally.Autopilot=false;rally.RandomStyles=false;rally.Seed=1;rally.Level=AiLevel.Medium;
            rally.StyleOverride=style;
            // Keep the production pause so the receiver can recover before the next serve's volley offer.
            rally.SetParams(new RallyParams{HumanServeEarlyMs=25});
            Assert.That(rally.StartMatch(),Is.True);
            Assert.That(controller.ServeReceptionEarlyMs,Is.Zero,"the human's own serve grants no allowance");
            Assert.That(Drive(()=>rally.Phase==RallyPhase.HumanReception&&rally.Rules.Serving),Is.True);
            Assert.That(rally.Rules.Striker,Is.EqualTo(PlayerId.Rival));
        }
        private ShotRecord Release(double began,double at)
        {
            At(at);
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,"serve-margin",1,began,at,true,GestureOutcome.Swing,new Vector2(3.2f,2.2f),.5f,controller.Snapshot.Reception,"fixture",at));
            controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);return controller.LastRecord;
        }
        [Test] public void RivalServe_AfterFirstBounce_KeepsAllowanceUntilTheOrder_ThenResetsOnTheNextFlight()
        {
            // Keep K-in after the step that reports the bounce, so this specifically exercises Serving=false at release.
            // .25 stays below the real post-bounce peak (.2989 m); .3 left the plan unreachable.
            app.Tuning.Set("legal.minHeight",.25);
            // PO approval, 28 nov: this test plays against the Fondista; minHeight .25 and every assertion unchanged.
            RivalServe(RivalStyleId.Fondista);Assert.That(controller.ServeReceptionEarlyMs,Is.EqualTo(25));
            // Observe the actual first bounce, then read the currently presented bounce plan.
            Assert.That(Drive(()=>rally.Phase==RallyPhase.HumanReception&&!rally.Rules.Serving&&rally.Rules.FloorBounces==1),Is.True);
            var plan=controller.Plan;double kin=Real(plan.KIn);
            double release=Math.Max(app.Host.ObservedRealTime+.00001,kin-.0125);
            Assert.That(release,Is.LessThan(kin),"the tested release is after the bounce but before K-in");
            At(release);Assert.That(rally.Rules.Serving,Is.False,"the mutable serve flag ended at the bounce");
            var r=Release(Math.Max(app.Host.LaunchRealTime,release-.05),release);
            Assert.That(r.Result,Is.EqualTo("Valid"),r.Reason);Assert.That(r.EarlyGeometry,Is.True);Assert.That(r.Scheduled,Is.False);
            Assert.That(Drive(()=>r.Applied||r.Result=="SystemFault",5),Is.True);Assert.That(r.Applied,Is.True,r.Reason);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.HumanReception||rally.Phase==RallyPhase.PointOver,60),Is.True);
            if(rally.Phase==RallyPhase.HumanReception)Assert.That(controller.ServeReceptionEarlyMs,Is.Zero,"the rally return has no serve allowance");
            app.LaunchCurrent();Assert.That(controller.ServeReceptionEarlyMs,Is.Zero,"the per-launch value is consumed once");
        }
        [Test] public void RivalServe_ChosenVolleyKeepsTheSameAllowance_AndTheVolleyRemainsOptional()
        {
            RivalServe();Assert.That(controller.Volley.Exists,Is.True,controller.Volley.Reason);Assert.That(controller.VolleyCommitted,Is.False);
            Assert.That(controller.TryVolleyWindow(out double from,out double until),Is.True);
            double touch=Math.Min(until,from+.05);At(touch);controller.GestureBegan(touch);
            Assert.That(controller.VolleyCommitted,Is.True,controller.VolleyChoice);
            double release=Real(controller.Volley.Plan.KIn)-.0125;
            Assert.That(release,Is.GreaterThan(touch));
            var r=Release(touch,release);
            Assert.That(controller.LastOpportunity,Is.EqualTo(Opportunity.Volley));
            Assert.That(r.Result,Is.EqualTo("Valid"),r.Reason);Assert.That(r.EarlyGeometry,Is.True);
            Assert.That(Drive(()=>r.Applied||r.Result=="SystemFault",5),Is.True);Assert.That(r.Applied,Is.True,r.Reason);
        }
        [Test] public void NeutralPracticeLaunch_HasNoAllowance_EvenWithTheRallyParameterSet()
        {
            rally.SetParams(new RallyParams{HumanServeEarlyMs=25});rally.StartMode();
            Assert.That(controller.ServeReceptionEarlyMs,Is.Zero);
            double release=Real(controller.Plan.KIn)-.0125;
            var r=Release(Math.Max(app.Host.LaunchRealTime,release-.05),release);
            Assert.That(r.Result,Is.EqualTo("Failed"));Assert.That(r.Reason,Is.EqualTo("before-k-in"));
        }
    }
}
