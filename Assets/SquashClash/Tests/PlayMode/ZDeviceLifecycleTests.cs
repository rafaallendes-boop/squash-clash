using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Device robustness, block A: what an interruption does (home button, call, notification centre, screen lock).
    // iOS reports all of them as a pause, a loss of focus or both, in any order and sometimes twice. A match pauses
    // itself into the player's own pause card and resumes from exactly where it was; a screen that already holds the
    // game paused (menu, tutorial, pause card) gets no second layer; practice keeps the "Toca para continuar" overlay.
    public sealed class ZDeviceLifecycleTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private ResumeOverlay Overlay=>app.GetComponent<ResumeOverlay>();
        private IEnumerator StartMatch()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
        }
        private void Interrupt(string how)
        {
            if(how!="focus")app.gameObject.SendMessage("OnApplicationPause",true);
            if(how!="pause")app.gameObject.SendMessage("OnApplicationFocus",false);
            if(how=="twice"){app.gameObject.SendMessage("OnApplicationPause",true);app.gameObject.SendMessage("OnApplicationFocus",false);}
            Refresh();
        }
        private void ComeBack(string how)
        {
            if(how!="focus")app.gameObject.SendMessage("OnApplicationPause",false);
            if(how!="pause")app.gameObject.SendMessage("OnApplicationFocus",true);
            Refresh();
        }
        [UnityTest] public IEnumerator Interruption_MidRally_PausesIntoThePauseCard_AndContinueResumesTheSameRally([Values("pause","focus","both","twice")] string how)
        {
            yield return StartMatch();
            // Into the second point, with the ball in flight.
            Assert.That(Drive(()=>rally.HumanPoints+rally.RivalPoints>=1&&rally.Phase!=RallyPhase.PointOver&&rally.Rules.Shots>=2,300),Is.True);
            var ball=app.Host.Current;int human=rally.HumanPoints,rival=rally.RivalPoints,shots=rally.Rules.Shots;long launch=app.Host.LaunchId;
            var phase=rally.Phase;var server=rally.Match.Server;var toHit=rally.Rules.ToHit;
            Assert.That(scape.AmbienceWanted,Is.True);

            Interrupt(how);yield return null;
            Assert.That(app.Clock.Paused,Is.True,"the match pauses itself");
            Assert.That(Shown(Find("pause-layer")),Is.True,"into the player's pause card");
            Assert.That(Overlay.IsVisible,Is.False,"with no second layer on top of it");
            Assert.That(scape.AmbienceWanted,Is.False,"the court falls silent");
            Drive(()=>false,.5);
            Assert.That(app.Host.Current.Step,Is.EqualTo(ball.Step),"nothing advances while away");

            ComeBack(how);yield return null;
            Assert.That(app.Clock.Paused,Is.True,"coming back does not throw the player into a live ball");
            Assert.That(Shown(Find("pause-layer")),Is.True);Assert.That(Overlay.IsVisible,Is.False);
            Assert.That(app.Host.Current.Step,Is.EqualTo(ball.Step));Assert.That(app.Host.Current.Position,Is.EqualTo(ball.Position));Assert.That(app.Host.Current.Velocity,Is.EqualTo(ball.Velocity));

            Click("pause-continue");Refresh();yield return null;
            Assert.That(app.Clock.Paused,Is.False);Assert.That(Shown(Find("pause-layer")),Is.False);
            Assert.That(rally.Active,Is.True);Assert.That(rally.Phase,Is.EqualTo(phase));
            Assert.That(rally.HumanPoints,Is.EqualTo(human));Assert.That(rally.RivalPoints,Is.EqualTo(rival));
            Assert.That(rally.Rules.Shots,Is.EqualTo(shots));Assert.That(rally.Rules.ToHit,Is.EqualTo(toHit));Assert.That(rally.Match.Server,Is.EqualTo(server));
            Assert.That(app.Host.LaunchId,Is.EqualTo(launch),"the same flight, not a relaunch");
            Assert.That(app.Host.Current.Step,Is.EqualTo(ball.Step),"from the very step it stopped at");
            Assert.That(scape.AmbienceWanted,Is.True,"the court sounds again");
            Drive(()=>false,.1);
            Assert.That(app.Host.Current.Step,Is.GreaterThan(ball.Step));
            Assert.That(app.Host.Current.Step-ball.Step,Is.LessThanOrEqualTo((long)(.1/StepReal)+2),"no jump for the time spent away");
        }
        [UnityTest] public IEnumerator Interruption_BetweenPoints_KeepsTheScoreAndTheBanner()
        {
            rally.SetNewServe(false);
            yield return StartMatch();
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,300),Is.True);
            int human=rally.HumanPoints,rival=rally.RivalPoints,played=rally.PointsPlayed;
            Interrupt("both");yield return null;ComeBack("both");yield return null;
            Assert.That(Shown(Find("pause-layer")),Is.True);Assert.That(Overlay.IsVisible,Is.False);
            Click("pause-continue");Refresh();yield return null;
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.PointOver));Assert.That(rally.PointsPlayed,Is.EqualTo(played));
            Assert.That(rally.HumanPoints,Is.EqualTo(human));Assert.That(rally.RivalPoints,Is.EqualTo(rival));
            Assert.That(Shown(Find("rally-banner")),Is.True,"the banner of the finished point is still there");
        }
        [UnityTest] public IEnumerator Interruption_OnTheMenuOrTheTutorial_AddsNoSecondLayer()
        {
            Interrupt("both");yield return null;ComeBack("both");yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.Menu));Assert.That(Shown(Find("menu-layer")),Is.True);
            Assert.That(Overlay.IsVisible,Is.False,"the menu already holds the game: no extra tap");
            Assert.That(app.Clock.Paused,Is.True);
            Click("menu-howto");Refresh();yield return null;Click("tutorial-next");Refresh();
            Interrupt("both");yield return null;ComeBack("both");yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.Tutorial));Assert.That(flow.TutorialStep,Is.EqualTo(1),"the tutorial stays on its step");
            Assert.That(Overlay.IsVisible,Is.False);
            // And the menu still starts a match afterwards.
            Click("tutorial-skip");Refresh();yield return null;
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null);Assert.That(app.Clock.Paused,Is.False);
        }
        [UnityTest] public IEnumerator Interruption_WithThePauseCardOpen_LeavesItAsItWas()
        {
            yield return StartMatch();Drive(()=>app.Host.Current.Step>20,1);
            Click("rally-pause");Refresh();yield return null;
            long step=app.Host.Current.Step;
            Interrupt("both");yield return null;ComeBack("both");yield return null;
            Assert.That(Shown(Find("pause-layer")),Is.True);Assert.That(Overlay.IsVisible,Is.False);Assert.That(app.Clock.Paused,Is.True);
            Click("pause-continue");Refresh();yield return null;
            Assert.That(app.Clock.Paused,Is.False);Assert.That(app.Host.Current.Step,Is.EqualTo(step));
        }
        // Guards: behaviour that already existed and must stay.
        [UnityTest] public IEnumerator Interruption_InPractice_KeepsTheResumeOverlay()
        {
            Click("menu-practice");Refresh();yield return null;
            Assert.That(app.Clock.Paused,Is.False);Assert.That(rally.Active,Is.False);
            Interrupt("both");yield return null;
            Assert.That(app.Clock.Paused,Is.True);Assert.That(Overlay.IsVisible,Is.True);Assert.That(Shown(Find("pause-layer")),Is.False);
            ComeBack("both");Overlay.Continue();Refresh();yield return null;
            Assert.That(app.Clock.Paused,Is.False);Assert.That(Overlay.IsVisible,Is.False);
        }
        [UnityTest] public IEnumerator Interruption_AtTheEndOfTheMatch_ComesBackToTheResultCard()
        {
            yield return StartMatch();
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);Refresh();
            int human=rally.Match.Human,rival=rally.Match.Rival;
            Interrupt("both");yield return null;ComeBack("both");Overlay.Continue();Refresh();yield return null;
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.MatchOver));Assert.That(rally.Match.Human,Is.EqualTo(human));Assert.That(rally.Match.Rival,Is.EqualTo(rival));
            Assert.That(Find("match-result").resolvedStyle.display,Is.EqualTo(DisplayStyle.Flex));
            Assert.That(Shown(Find("pause-layer")),Is.False);
        }
        // An audio interruption (call, alarm, a route change) can stop every source without pausing the app: the court
        // ambience is asked to start again on the next frame instead of waiting for the next pause.
        [UnityTest] public IEnumerator AudioInterruption_AsksTheAmbienceToStartAgain()
        {
            yield return StartMatch();Drive(()=>app.Host.Current.Step>20,1);
            Assert.That(scape.AmbienceStarts,Is.EqualTo(1),"asked once when the match starts");
            Drive(()=>false,.2);Assert.That(scape.AmbienceStarts,Is.EqualTo(1),"and not again while it plays");
            scape.OnAudioConfigurationChanged(true);scape.Tick(.016f);
            Assert.That(scape.AmbienceStarts,Is.EqualTo(2),"asked again after the audio system came back");
            Click("rally-pause");Refresh();scape.Tick(.016f);
            scape.OnAudioConfigurationChanged(false);scape.Tick(.016f);
            Assert.That(scape.AmbienceStarts,Is.EqualTo(2),"never while paused");
        }
    }
}
