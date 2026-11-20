using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Fatigue feel (design 2026-11-20-fatigue-feel) in the rally: the energy-limited sprint and the DEV «Cansancio fuerte».
    public sealed class ZFatigueFeelTests
    {
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Fatigue feel fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",1);app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();rally=app.Rally;
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;
            rally.AutoRestart=false;
        }
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        private void At(double target)
        {
            for(int i=0;i<6000&&app.Host.ObservedRealTime+StepReal<target-1e-10;i++){app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();}
            double left=target-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);controller.ProcessPending();rally.Tick();
        }
        [UnityTearDown] public IEnumerator Cleanup(){SquashClash.Runtime.Visual.PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}

        [Test] public void Exhausted_TheOfferHasNoSprintStretch_FreshItDoes()
        {
            rally.SetTestEnergy(100);Assert.That(rally.StartMode(),Is.True);var fresh=rally.LastHumanOffer;
            Assert.That(fresh.Offer.Exists,Is.True,fresh.Offer.Reason);Assert.That(fresh.SPrime,Is.GreaterThan(fresh.S),"control: fresh, the sprint stretch exists");
            rally.StopMode();rally.SetTestEnergy(0);Assert.That(rally.StartMode(),Is.True);var empty=rally.LastHumanOffer;
            Assert.That(rally.HumanReceptionFatigue,Is.EqualTo(1));Assert.That(empty.Offer.Exists,Is.True,empty.Offer.Reason);
            Assert.That(empty.SPrime,Is.EqualTo(empty.S),"at energy 0 the amber ring ends at S: no sprint volley");
            Assert.That(controller.TryVolleyWindow(out _,out double until),Is.True);
            At(until);controller.GestureBegan(until);Assert.That(controller.VolleyCommitted,Is.True,controller.VolleyChoice);Assert.That(rally.Human.Sprint,Is.EqualTo(1f),"no sprint at energy 0");
        }
        [Test] public void DevToggle_SprintFatigueOff_BringsBackTheFullSprint_FromTheNextPoint()
        {
            Assert.That(rally.VolleySprintFatigueNextPoint,Is.True,"on by default");
            rally.SetVolleySprintFatigue(false);Assert.That(rally.VolleySprintFatigueNextPoint,Is.False);
            rally.SetTestEnergy(0);Assert.That(rally.StartMode(),Is.True);
            Assert.That(rally.Params.HumanVolleySprintFatigue,Is.EqualTo(0));var d=rally.LastHumanOffer;
            Assert.That(d.SPrime,Is.GreaterThan(d.S),"DEV off: v2.2 as shipped, even exhausted");
            rally.SetVolleySprintFatigue(true);rally.Rematch();Assert.That(rally.Params.HumanVolleySprintFatigue,Is.EqualTo(1));
        }
        [Test] public void DevStrongFatigue_MultipliesBothPlayersLegsAndReaction_FromTheNextPoint_WithoutTouchingTheTuning()
        {
            Assert.That(rally.StrongFatigueNextPoint,Is.False,"off by default");
            rally.SetStrongFatigue(true);Assert.That(rally.StrongFatigueNextPoint,Is.True);Assert.That(rally.StartMatch(),Is.True);
            Assert.That(rally.FatigueSettings.LegsMax,Is.EqualTo(.3).Within(1e-12));Assert.That(rally.FatigueSettings.ReactionMaxMs,Is.EqualTo(80).Within(1e-12));
            Assert.That(app.Tuning.Applied.Get("fatigue.legsMax"),Is.EqualTo(.1),"the tuning and DefaultPreset values stay");
            Assert.That(app.Tuning.Applied.Get("fatigue.reactionMaxMs"),Is.EqualTo(40));
            rally.SetStrongFatigue(false);rally.Rematch();
            Assert.That(rally.FatigueSettings.LegsMax,Is.EqualTo(.1));Assert.That(rally.FatigueSettings.ReactionMaxMs,Is.EqualTo(40));
        }
    }
}
