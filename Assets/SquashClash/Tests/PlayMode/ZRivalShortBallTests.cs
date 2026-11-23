using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    public sealed class ZRivalShortBallTests
    {
        private AppBootstrap app;private RallyDirector rally;private ShotController controller;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Rival short fixture",app.Build.Label),new List<string>());
            controller=app.GetComponent<ShotController>();rally=app.Rally;app.LaunchCurrent();
            app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;rally.AutoRestart=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){SquashClash.Runtime.Visual.PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}
        private void Drive(int seconds)
        {
            double step=1d/app.Config.Physics.StepHz/app.Config.Tempo;
            for(int i=0;i<seconds/step;i++){app.Host.Advance(step);controller.ProcessPending();rally.Tick();}
        }
        [Test] public void Director_UnreachableRivalReception_RaisesOneFailedSwing()
        {
            rally.StartMode();rally.Rival.Teleport(new Vector2(6,9));
            // Test-only access to the existing launch path: no production hooks or alternate ball authority.
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var rules=(PointRules)typeof(RallyDirector).GetField("committed",flags).GetValue(rally);
            rules.Begin(PlayerId.Human,PlayerId.Rival);
            var c=new RallyContext(app.Host.ActiveConfig,rally.Params,new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
            var from=new Vector3(1.2f,.6f,2.2f);
            var center=Ballistics.CenterForContact(Core.Court.SurfaceId.FrontWall,new Vector3(.8f,.75f,0),c.Physics.BallRadius);
            var ball=new BallState{Position=from,Velocity=Ballistics.SolveCenterTo(from,center,5,c.Physics.Gravity)};
            int failed=0,contacts=0,ended=0;bool failedBeforeEnd=false;
            rally.RivalSwing+=(_,hit)=>{if(hit)contacts++;else{failed++;failedBeforeEnd=ended==0;}};
            rally.PointEnded+=_=>ended++;
            typeof(RallyDirector).GetMethod("LaunchToRival",flags).Invoke(rally,new object[]{ball});
            Assert.That(rally.RivalPlan.Reason,Is.EqualTo("unreachable"),"required deterministic unreachable reception");
            Drive(12);
            Assert.That(failed,Is.EqualTo(1),"R1: a missed short ball still draws one attempt");
            Assert.That(failedBeforeEnd,Is.True);Assert.That(contacts,Is.Zero);Assert.That(ended,Is.EqualTo(1));
            Assert.That(rally.LastPoint.Value.Reception,Is.EqualTo(ReceptionResult.Unreachable));
            Drive(2);Assert.That(failed,Is.EqualTo(1),"the visual attempt is consumed once");
        }
        [Test] public void Director_ReceptionWithSwing_RaisesNoExtraAttempt()
        {
            int swings=0;rally.RivalSwing+=(_,hit)=>swings++;
            rally.StartMatch();Assert.That(rally.RivalPlan.Reachable,Is.True);
            Drive(12);Assert.That(swings,Is.EqualTo(1));
        }
    }
}
