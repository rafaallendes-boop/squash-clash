using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Serve kinds (design 2026-11-20-serve, revision 2): the human chooses with a touch on the front wall in the pause before its serve,
    // the rival by its style, all behind the DEV «Saque nuevo» switch (off by default). Named Z* to run after the timing fixtures.
    public sealed class ZServeTests
    {
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Serve fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();rally=app.Rally;app.LaunchCurrent();
            app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){SquashClash.Runtime.Visual.PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}
        private bool Drive(System.Func<bool> until,double maxRealSeconds=60)
        {
            int limit=(int)(maxRealSeconds/StepReal);
            for(int i=0;i<limit;i++){if(until())return true;app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();}
            return until();
        }
        private ClosedGesture Touch(Vector2 aim,string id)
        {double now=app.Host.ObservedRealTime;return new ClosedGesture(app.Host.LaunchId,id,0,now-.2,now,true,GestureOutcome.Swing,aim,.5f,controller.Snapshot.Reception,"test",now);}

        [UnityTest] public IEnumerator Off_TheMatchServeIsTodaysAutomaticServe()
        {
            Assert.That(rally.NewServeNextPoint,Is.False,"off by default");
            rally.SetParams(new RallyParams{MatchPauseSeconds=.3});Assert.That(rally.StartMatch(),Is.True);
            Assert.That(rally.LastServeType,Is.EqualTo(ServeType.Auto));
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,40),Is.True);Assert.That(controller.ServeAim,Is.Null,"no serve choice with the switch off");
            yield return null;
        }
        [UnityTest] public IEnumerator On_TheHumansFirstServeIsTheLob_AndTheRivalServesItsOwnChoice()
        {
            rally.StyleOverride=RivalStyleId.Definidor;
            rally.SetParams(new RallyParams{ServeKindOn=1,MatchPauseSeconds=.3});rally.StartMatch();
            Assert.That(rally.LastServeType,Is.EqualTo(ServeType.Lob),"first serve of the match, no pause before it");
            // The human never swings: the rival wins a point and serves within a few points.
            Assert.That(Drive(()=>rally.Phase==RallyPhase.HumanReception&&rally.Rules.Serving,120),Is.True);
            Assert.That(rally.LastServeType,Is.Not.EqualTo(ServeType.Auto),"the rival chose a lob or a drive");
            yield return null;
        }
        [UnityTest] public IEnumerator On_ALowTouchInThePauseBeforeTheHumansServe_ServesTheDriveAtOnce()
        {
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Easy;rally.Seed=3;
            rally.SetParams(new RallyParams{ServeKindOn=1,MatchPauseSeconds=30});rally.StartMatch();
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver&&!rally.Match.Finished&&rally.Match.Server==PlayerId.Human,900),Is.True,"the human wins a point");
            Assert.That(controller.ServeAim,Is.Not.Null,"the touch chooses the serve in this pause");
            Assert.That(controller.CanBegin(app.Host.ObservedRealTime,out var reason),Is.True,reason);
            long launch=app.Host.LaunchId;
            controller.Enqueue(Touch(new Vector2(3.2f,1.2f),"serve-low"));
            Assert.That(Drive(()=>app.Host.LaunchId==launch+1,.5),Is.True,"the serve goes when the touch is released");
            Assert.That(rally.LastServeType,Is.EqualTo(ServeType.Drive));Assert.That(controller.ServeAim,Is.Null,"closed once the point starts");
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.RivalReception));
            yield return null;
        }
    }
    public sealed class ZServeDevTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        [UnityTest] public IEnumerator DevPanel_NewServeToggle_DefaultsOff_AndSwitchesTheNextPoint()
        {
            for(int i=0;i<4;i++){Refresh();yield return null;}
            var button=root.Q<Button>("serve-new-toggle");Assert.That(button,Is.Not.Null,"DEV panel has the «Saque nuevo» toggle");
            Assert.That(rally.NewServeNextPoint,Is.False);Assert.That(button.text,Does.Contain("no"));
            Click("serve-new-toggle");Assert.That(rally.NewServeNextPoint,Is.True);Assert.That(button.text,Does.Contain("sí"));
            Click("serve-new-toggle");Assert.That(rally.NewServeNextPoint,Is.False);
            Assert.That(root.Q<Label>("rally-banner-serve-hint"),Is.Not.Null,"the pause shows how to choose the serve");
        }
    }
}
