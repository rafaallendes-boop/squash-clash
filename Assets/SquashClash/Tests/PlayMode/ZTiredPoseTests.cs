using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Fatigue feel, section 3 (design 2026-11-20): below 25 energy the figure stands lower and heavier. View only, DEV toggle off by default.
    public sealed class ZTiredPoseTests
    {
        private const float Frame=1f/60;
        private AppBootstrap app;private RallyDirector rally;
        private static CharacterView Figure(Transform player)=>player.Find("Figure").GetComponent<CharacterView>();
        [UnitySetUp] public IEnumerator Setup()
        {
            TiredPose.Enabled=false;
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Tired pose fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",1);app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();rally=app.Rally;
            yield return null;yield return null;
            app.Host.enabled=app.enabled=app.GetComponent<ShotController>().enabled=rally.enabled=false;
            Figure(app.Players.Human).enabled=Figure(app.Players.Rival).enabled=false;rally.AutoRestart=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){TiredPose.Enabled=false;SquashClash.Runtime.Visual.PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}

        [Test] public void Amount_IsZeroAtOrAbove25_OneAtZero_AndOffIsZero()
        {
            Assert.That(TiredPose.Amount(true,100),Is.EqualTo(0));Assert.That(TiredPose.Amount(true,25),Is.EqualTo(0));
            Assert.That(TiredPose.Amount(true,12.5),Is.EqualTo(.5f).Within(1e-6));Assert.That(TiredPose.Amount(true,0),Is.EqualTo(1));
            Assert.That(TiredPose.Amount(false,0),Is.EqualTo(0),"off by default: no change");
        }
        [Test] public void Exhausted_TheFigureStandsLower_OnlyWithTheToggle_AndTheSimulationIsUntouched()
        {
            Assert.That(TiredPose.Enabled,Is.False,"off by default");
            rally.SetTestEnergy(5);Assert.That(rally.StartMode(),Is.True);
            var figure=Figure(app.Players.Human);var torso=app.Players.Human.Find("Figure/Torso");
            var body=rally.Human.Position;double energy=rally.HumanEnergy.Energy;
            for(int i=0;i<180;i++)figure.Animate(Frame);float upright=torso.localPosition.y;
            Assert.That(figure.Tired,Is.EqualTo(0),"toggle off: no tired pose");
            TiredPose.Enabled=true;for(int i=0;i<180;i++)figure.Animate(Frame);
            Assert.That(figure.Tired,Is.GreaterThan(.7f),"energy 5: clearly tired");
            Assert.That(torso.localPosition.y,Is.LessThan(upright-.03f),"the hips drop");
            Assert.That(rally.Human.Position,Is.EqualTo(body));Assert.That(rally.HumanEnergy.Energy,Is.EqualTo(energy),"the view changes nothing in the simulation");
            rally.SetTestEnergy(100);rally.Rematch();for(int i=0;i<180;i++)figure.Animate(Frame);
            Assert.That(figure.Tired,Is.LessThan(.05f),"fresh again: the pose goes away");
        }
    }
}
