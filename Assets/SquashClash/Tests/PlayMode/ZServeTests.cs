using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Serve kinds (design 2026-11-20-serve, revision 2): the human chooses with a touch on the front wall in the pause before its serve,
    // the rival by its style, with the DEV «Saque nuevo» switch on by default. Named Z* to run after the timing fixtures.
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

        private void ReachHumanServePause(bool newServe)
        {
            rally.Autopilot = true;
            rally.AutopilotTimingSigmaMs = 0;
            rally.Level = AiLevel.Easy;
            rally.Seed = 3;
            rally.SetParams(new RallyParams { ServeKindOn = newServe ? 1 : 0 });
            Assert.That(rally.StartMatch(), Is.True);
            Assert.That(Drive(() => rally.Phase == RallyPhase.PointOver && rally.Match.Server == PlayerId.Human, 900),
                Is.True, "the human wins a point");
        }

        private Vector2 ServiceBoxPosition()
        {
            var context = new RallyContext(app.Host.ActiveConfig, rally.Params, new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold));
            return ServePlanner.BoxPosition(context, rally.Match.Box);
        }

        [UnityTest] public IEnumerator On_TheHumanWaitsInTheBoxAfterTheFreeze_ThenServesALobAtThreeSeconds()
        {
            double now = 10;
            PointEndFreeze.Now = () => now;
            ReachHumanServePause(true);
            Assert.That(rally.WaitBetweenPoints, Is.EqualTo(3d));
            Assert.That(PointEndFreeze.Active, Is.True);
            var box = ServiceBoxPosition();
            Assert.That(rally.Human.Position, Is.Not.EqualTo(box), "keep the point-end pose through the freeze");
            double pointEnd = app.Host.ObservedRealTime;
            // The freeze the simulation sees runs on the game clock: advance the host until it has passed.
            Assert.That(Drive(() => rally.HumanServePauseElapsed >= PointEndFreeze.Seconds + .001, 1), Is.True);
            Assert.That(rally.Human.Position, Is.EqualTo(box));
            Assert.That(rally.Human.Velocity, Is.EqualTo(Vector2.zero));
            Assert.That(rally.Human.Phase, Is.EqualTo(BodyPhase.Waiting));

            long launch = app.Host.LaunchId;
            Assert.That(Drive(() => app.Host.ObservedRealTime >= pointEnd + 2.97, 3), Is.True);
            Assert.That(app.Host.LaunchId, Is.EqualTo(launch));
            Assert.That(rally.Human.Position, Is.EqualTo(box), "the server stays still until the serve");
            // Launch resets ObservedRealTime. Record the elapsed time before the step that launches.
            double elapsedBeforeLaunch = 0;
            Assert.That(Drive(() =>
            {
                if (app.Host.LaunchId != launch) return true;
                elapsedBeforeLaunch = app.Host.ObservedRealTime - pointEnd;
                return false;
            }, .1), Is.True);
            Assert.That(app.Host.LaunchId, Is.EqualTo(launch + 1));
            Assert.That(elapsedBeforeLaunch + StepReal, Is.EqualTo(3d).Within(StepReal + 1e-8));
            Assert.That(rally.LastServeType, Is.EqualTo(ServeType.Lob));
            yield return null;
        }

        // The point-end freeze the simulation sees runs on the game clock (the pause's own elapsed host time), never on the real clock
        // the views read: with PointEndFreeze.Now stopped, the human reaches the box as soon as the game clock passes the freeze.
        [UnityTest] public IEnumerator On_ThePointEndFreezeFollowsTheGameClock_NotTheRealClock()
        {
            double now = 10;
            PointEndFreeze.Now = () => now;
            ReachHumanServePause(true);
            double freeze = System.Math.Min(PointEndFreeze.Seconds, rally.WaitBetweenPoints);
            var box = ServiceBoxPosition();
            Assert.That(rally.Human.Position, Is.Not.EqualTo(box), "keep the point-end pose at the start of the pause");
            Assert.That(Drive(() => rally.HumanServePauseElapsed >= freeze - 2 * StepReal, 1), Is.True);
            Assert.That(rally.HumanServePauseElapsed, Is.LessThan(freeze), "the game clock has not passed the freeze yet");
            Assert.That(rally.Human.Position, Is.Not.EqualTo(box), "still in the freeze: the game clock has not passed it");
            Assert.That(Drive(() => rally.HumanServePauseElapsed >= freeze + StepReal, 1), Is.True);
            Assert.That(rally.Human.Position, Is.EqualTo(box), "the game clock passed the freeze: the human is in the box with the real clock stopped");
            Assert.That(rally.Human.Phase, Is.EqualTo(BodyPhase.Waiting));
            yield return null;
        }

        [UnityTest] public IEnumerator On_TheServeStartsWithIdenticalStateWithAndWithoutEarlyPlacement()
        {
            var placement = typeof(RallyDirector).GetField("PlaceHumanInServeBoxEarly",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(placement, Is.Not.Null);
            var starts = new List<(BallState ball, Vector2 human, Vector2 rival, double humanEnergy, double rivalEnergy)>();
            foreach (bool early in new[] { true, false })
            {
                double now = 10;
                PointEndFreeze.Now = () => now;
                placement.SetValue(rally, early);
                ReachHumanServePause(true);
                var before = rally.Human.Position;
                Assert.That(Drive(() => rally.HumanServePauseElapsed >= PointEndFreeze.Seconds + .001, 1), Is.True);
                Assert.That(rally.Human.Position, Is.EqualTo(early ? ServiceBoxPosition() : before));
                rally.SetTestEnergy(72);
                long launch = app.Host.LaunchId;
                rally.SkipWait();
                rally.Tick();
                Assert.That(app.Host.LaunchId, Is.EqualTo(launch + 1));
                starts.Add((app.Host.Current, rally.Human.Position, rally.Rival.Position, rally.HumanEnergy.Energy, rally.RivalEnergy.Energy));
            }
            Assert.That(starts[1], Is.EqualTo(starts[0]));
            yield return null;
        }

        [UnityTest] public IEnumerator On_TheGuideReplacesTheBannerAndCountsDown_AndALowTouchServesNow()
        {
            var root = app.GetComponent<UIDocument>().rootVisualElement;
            Assert.That(root.Q("serve-guide"), Is.Not.Null);
            var guide = root.Q("serve-guide");
            Assert.That(guide.style.display.value, Is.EqualTo(DisplayStyle.None), "no guide before a match");
            ReachHumanServePause(true);
            var hud = app.GetComponent<RallyHud>();
            hud.Refresh();
            yield return null;
            yield return null;
            Assert.That(guide.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(root.Q("rally-banner").style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(root.Q("rally-banner-serve-hint").style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(root.Q<Label>("rally-status").text, Does.StartWith("Punto Tú:"));
            var line = guide.Q("serve-guide-line");
            Assert.That(line, Is.Not.Null);
            var camera = UnityEngine.Camera.main;
            var court = app.Host.ActiveConfig.Court;
            var atZero = camera.WorldToScreenPoint(new Vector3(0, 3, 0));
            var atWidth = camera.WorldToScreenPoint(new Vector3(court.Width, 3, 0));
            float panelPerPixel = root.layout.width / Screen.width;
            float fullWidth = Mathf.Abs(atWidth.x - atZero.x) * panelPerPixel;
            float center = (atZero.x + atWidth.x) / 2 * panelPerPixel;
            Assert.That(line.worldBound.center.y, Is.EqualTo((1f - atZero.y / Screen.height) * root.layout.height).Within(1));
            Assert.That(line.worldBound.width, Is.EqualTo(fullWidth).Within(1));
            Assert.That(line.worldBound.center.x, Is.EqualTo(center).Within(1));
            double start = app.Host.ObservedRealTime;
            Assert.That(Drive(() => app.Host.ObservedRealTime >= start + 1, 1.1), Is.True);
            hud.Refresh();
            yield return null;
            yield return null;
            float remaining = (float)(3 - (app.Host.ObservedRealTime - start)) / 3f;
            Assert.That(line.worldBound.width, Is.EqualTo(fullWidth * remaining).Within(1 + fullWidth * StepReal / 3));
            Assert.That(line.worldBound.center.x, Is.EqualTo(center).Within(1));
            var input = app.GetComponent<TouchInputAdapter>();
            var low = camera.WorldToScreenPoint(new Vector3(3.2f, 2.7f, 0));
            Assert.That(input.UiBlocker(low), Is.False);
            long launch = app.Host.LaunchId;
            controller.Enqueue(Touch(new Vector2(3.2f, 2.7f), "serve-guide-low"));
            controller.ProcessPending();
            hud.Refresh();
            Assert.That(app.Host.LaunchId, Is.EqualTo(launch + 1));
            Assert.That(rally.LastServeType, Is.EqualTo(ServeType.Drive));
            Assert.That(guide.style.display.value, Is.EqualTo(DisplayStyle.None));
        }

        [UnityTest] public IEnumerator RivalServeAndDisabledHumanServeKeepTheOrdinaryPauseAndBanner()
        {
            rally.SetParams(new RallyParams { ServeKindOn = 1 });
            Assert.That(rally.StartMatch(), Is.True);
            Assert.That(Drive(() => rally.Phase == RallyPhase.PointOver && rally.Match.Server == PlayerId.Rival, 120), Is.True);
            Assert.That(rally.WaitBetweenPoints, Is.EqualTo(2.5d));
            var root = app.GetComponent<UIDocument>().rootVisualElement;
            var hud = app.GetComponent<RallyHud>();
            hud.Refresh();
            Assert.That(root.Q("rally-banner").style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(root.Q("serve-guide")?.style.display.value, Is.Not.EqualTo(DisplayStyle.Flex));

            double now = 10;
            PointEndFreeze.Now = () => now;
            ReachHumanServePause(false);
            Assert.That(rally.WaitBetweenPoints, Is.EqualTo(2.5d));
            var before = rally.Human.Position;
            now += PointEndFreeze.Seconds + .001;
            rally.Tick();
            hud.Refresh();
            Assert.That(rally.Human.Position, Is.EqualTo(before));
            Assert.That(root.Q("rally-banner").style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(root.Q("serve-guide")?.style.display.value, Is.Not.EqualTo(DisplayStyle.Flex));
            yield return null;
        }

        [UnityTest] public IEnumerator Off_TheMatchServeIsTodaysAutomaticServe()
        {
            rally.SetParams(new RallyParams{MatchPauseSeconds=.3,ServeKindOn=0});
            Assert.That(rally.NewServeNextPoint,Is.False,"explicitly use the original serve");
            Assert.That(rally.StartMatch(),Is.True);
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
        [UnityTest] public IEnumerator DevPanel_NewServeToggle_DefaultsOn_AndSwitchesTheNextPoint()
        {
            for(int i=0;i<4;i++){Refresh();yield return null;}
            var button=root.Q<Button>("serve-new-toggle");Assert.That(button,Is.Not.Null,"DEV panel has the «Saque nuevo» toggle");
            Assert.That(rally.NewServeNextPoint,Is.True);Assert.That(button.text,Does.Contain("sí"));
            Click("serve-new-toggle");Assert.That(rally.NewServeNextPoint,Is.False);Assert.That(button.text,Does.Contain("no"));
            Click("serve-new-toggle");Assert.That(rally.NewServeNextPoint,Is.True);
            Assert.That(root.Q<Label>("rally-banner-serve-hint"),Is.Not.Null,"the pause shows how to choose the serve");
        }
    }
}
