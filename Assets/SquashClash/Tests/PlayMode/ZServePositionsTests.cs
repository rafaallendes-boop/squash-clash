using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Serve positions (spec 2026-11-26-posiciones-saque): RallyDirector.BeginPoint places the simulated bodies, not only the figures.
    // Named Z* to run after the timing fixtures.
    public sealed class ZServePositionsTests
    {
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Serve positions fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();rally=app.Rally;
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;rally.AutoRestart=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){SquashClash.Runtime.Visual.PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}
        private RallyContext Context()=>new RallyContext(app.Host.ActiveConfig,rally.Params,new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
        private void Start(PlayerId server)
        {
            rally.StopMode();rally.Autopilot=false;rally.Level=AiLevel.Medium;rally.Seed=3;rally.SetParams(new RallyParams());
            Assert.That(rally.StartMatch(),Is.True);
            if(server==PlayerId.Human)return;
            // Test fixture only, as ZReceptionCaptionTests: the match is opened with the rival to serve and the point is begun again.
            var t=typeof(RallyDirector);
            t.GetProperty("Match").GetSetMethod(true).Invoke(rally,new object[]{new MatchScore((int)rally.Params.MatchTarget,(int)rally.Params.MatchMargin,PlayerId.Rival)});
            t.GetMethod("BeginPoint",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(rally,null);
        }
        [Test] public void HumanServes_TheRivalBodyStartsInTheOppositeBackQuarter()
        {
            Start(PlayerId.Human);Assert.That(rally.Match.Server,Is.EqualTo(PlayerId.Human));
            var context=Context();var box=rally.Match.Box;
            Assert.That(Vector2.Distance(rally.Rival.Position,ServePlanner.ReceiverPosition(context,box)),Is.LessThan(1e-5f),"the simulated rival body");
            Assert.That(Vector2.Distance(rally.Human.Position,ServePlanner.BoxPosition(context,box)),Is.LessThan(1e-5f),"the server in its corner");
        }
        [Test] public void RivalServes_TheHumanBodyStartsInTheOppositeBackQuarter()
        {
            Start(PlayerId.Rival);Assert.That(rally.Match.Server,Is.EqualTo(PlayerId.Rival));Assert.That(rally.Phase,Is.EqualTo(RallyPhase.HumanReception));
            var context=Context();var box=rally.Match.Box;
            Assert.That(Vector2.Distance(rally.Human.Position,ServePlanner.ReceiverPosition(context,box)),Is.LessThan(1e-5f),"the simulated human body");
        }
    }
}
