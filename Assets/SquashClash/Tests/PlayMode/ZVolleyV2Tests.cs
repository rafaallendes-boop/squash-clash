using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Feedback;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Volley v2 end to end in the rally (design 2026-11-20): «Anillo ámbar visible: toca y voleas. Espera al anillo blanco: golpeas
    // después del bote.» Named after the rally fixtures on purpose (see ZRallyDirectorTests).
    public sealed class ZVolleyV2Tests
    {
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;private RecordingHaptics haptics;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Volley v2 fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();rally=app.Rally;haptics=new RecordingHaptics();Haptics.Backend=haptics;
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;
            rally.AutoRestart=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){Haptics.ResetBackend();SquashClash.Runtime.Visual.PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}
        private void At(double target)
        {
            for(int i=0;i<6000&&app.Host.ObservedRealTime+StepReal<target-1e-10;i++){app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();}
            double left=target-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);controller.ProcessPending();rally.Tick();
        }
        private double Real(double steps){var t=SimTime.FromSteps(steps);Assert.That(app.Host.ClockMap.TrySimToReal(t.Step,t.Fraction,out var real),Is.True);return real;}
        private ShotRecord Release(double began,double time)
        {
            At(time);
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,began,time,true,GestureOutcome.Swing,new Vector2(3.2f,2.2f),.5f,app.Host.ActiveConfig.Source,"fixture",time));
            controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);return controller.LastRecord;
        }
        private long FirstBounceStep()=>app.Host.Prediction.Contacts.First(c=>c.Surface==SurfaceId.Floor&&!c.Persistent).Step;
        private void StartRally()
        {
            Assert.That(rally.StartMode(),Is.True);
            Assert.That(controller.VolleyChoiceByTouch,Is.True,"v2 is the default for the human");
            Assert.That(controller.Volley.Exists,Is.True,controller.Volley.Reason);
        }

        [Test] public void AmberRing_IsOfferedAtLaunch_AndTicksOnceWhenItShows()
        {
            StartRally();var ring=app.GetComponent<TimingRingView>();
            Assert.That(controller.VolleyShown,Is.True);Assert.That(ring.Tint,Is.EqualTo(ShotController.VolleyTint));
            Assert.That(controller.Volley.Plan.TStarStep,Is.LessThan(FirstBounceStep()));
            Assert.That(controller.TryVolleyWindow(out double from,out double until),Is.True);Assert.That(until,Is.GreaterThanOrEqualTo(from));
            if(from>app.Host.LaunchRealTime+2*StepReal){At(from-2*StepReal);Assert.That(controller.VolleyHapticCount,Is.Zero,"not before the ring shows");}
            At(from+StepReal);
            Assert.That(controller.VolleyHapticCount,Is.EqualTo(1));Assert.That(haptics.Discreet,Is.EqualTo(1),"the lightest impact");
            At(from+5*StepReal);Assert.That(controller.VolleyHapticCount,Is.EqualTo(1),"once per reception");
        }
        [Test] public void TouchWhileAmberShows_ChoosesTheVolley_TheBodyRunsToTheIntercept_AndHitsBeforeTheBounce()
        {
            StartRally();long bounce=FirstBounceStep();Assert.That(controller.TryVolleyWindow(out double from,out double until),Is.True);
            double touch=Math.Min(until,from+.05);At(touch);controller.GestureBegan(touch);
            Assert.That(controller.VolleyCommitted,Is.True,controller.VolleyChoice);Assert.That(controller.VolleyChoice,Is.EqualTo("committed"));
            Assert.That(double.IsPositiveInfinity(controller.Volley.SplitSteps),Is.True,"the chosen volley is the only opportunity left");
            Assert.That(controller.VolleyCausalReference,Is.Not.Null);
            double star=Real(controller.Volley.Plan.TStarStep);At(star-2*StepReal);
            Assert.That(rally.Human.Target,Is.EqualTo(controller.Volley.Plan.Body.PositionXZ),"the body runs to the intercept stance");
            var r=Release(touch,star);
            Assert.That(controller.LastOpportunity,Is.EqualTo(Opportunity.Volley));Assert.That(r.Result,Is.EqualTo("Perfect"),r.Reason);
            At(app.Host.ObservedRealTime+.3);
            Assert.That(controller.LastRecord.Applied,Is.True,controller.LastRecord.Reason);Assert.That(controller.LastRecord.ContactStep,Is.LessThan(bounce));
            Assert.That(rally.SystemFaults,Is.Zero);
        }
        [Test] public void TouchBeforeTheAmberRingShows_GivesTheVolleyUp_AndPlaysTheBounce()
        {
            StartRally();Assert.That(controller.TryVolleyWindow(out double from,out _),Is.True);
            Assert.That(from,Is.GreaterThan(app.Host.LaunchRealTime+.02),"the neutral feed shows the amber ring after the launch");
            var stance=rally.HumanPlan.Stance;double touch=app.Host.LaunchRealTime+.01;At(touch);controller.GestureBegan(touch);
            Assert.That(controller.VolleyCommitted,Is.False);Assert.That(controller.VolleyChoice,Is.EqualTo("touch-outside-amber"));
            Assert.That(controller.Volley.Exists,Is.False);Assert.That(app.GetComponent<TimingRingView>().Tint,Is.EqualTo(Color.white));
            Assert.That(rally.Human.Target,Is.EqualTo(stance),"the body keeps its path to the bounce");
            var r=Release(touch,Real(controller.Plan.TStarStep));
            Assert.That(controller.LastOpportunity,Is.EqualTo(Opportunity.Bounce));Assert.That(r.Result,Is.EqualTo("Perfect"),r.Reason);
        }
        [Test] public void WithoutTouch_TheRingTurnsWhiteAtS_AndAnOrderWithoutTouchNeverVolleys()
        {
            StartRally();var ring=app.GetComponent<TimingRingView>();Assert.That(controller.TryVolleyWindow(out double from,out double until),Is.True);
            // An order with no touch (autopilot) inside the amber window is still the bounce one: only a touch chooses the volley.
            var early=Release(app.Host.LaunchRealTime+.01,Math.Max(from,Real(controller.Volley.Plan.TStarStep)-.001));
            Assert.That(controller.LastOpportunity,Is.EqualTo(Opportunity.Bounce));Assert.That(early.Result,Is.Not.EqualTo("Perfect"));
            Assert.That(controller.VolleyCommitted,Is.False);
            At(until+StepReal);
            Assert.That(controller.VolleyShown,Is.False);Assert.That(ring.Tint,Is.EqualTo(Color.white));
        }
        [Test] public void DevSwitches_BringBackV1AndTurnTheServeVolleyOff_FromTheNextPoint()
        {
            StartRally();Assert.That(rally.VolleyV2NextPoint,Is.True);Assert.That(rally.ServeVolleyNextPoint,Is.True);
            Assert.That(rally.Rules.ServeVolley,Is.EqualTo(PlayerId.Human));
            rally.SetVolleyV2(false);rally.SetServeVolley(false);Assert.That(rally.VolleyV2NextPoint,Is.False);Assert.That(rally.ServeVolleyNextPoint,Is.False);
            Assert.That(controller.VolleyChoiceByTouch,Is.True,"the current point keeps v2");
            rally.Rematch();
            Assert.That(rally.Params.HumanVolleyMode,Is.EqualTo(1));Assert.That(rally.Params.HumanVolleyServe,Is.EqualTo(0));
            Assert.That(controller.VolleyChoiceByTouch,Is.False,"v1 again: no touch choice");Assert.That(rally.Rules.ServeVolley,Is.Null);
        }
    }
}
