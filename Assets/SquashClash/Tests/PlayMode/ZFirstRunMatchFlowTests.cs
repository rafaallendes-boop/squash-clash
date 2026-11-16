using System.Collections;
using NUnit.Framework;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // First run, block C: pause (Continuar / Salir al menú) and the end of the match (Revancha / Volver al inicio).
    public sealed class ZFirstRunMatchFlowTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private IEnumerator StartMatch()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
        }
        [UnityTest] public IEnumerator Pause_ShowsTheCardUnderTheHeader_AndContinueResumes()
        {
            yield return StartMatch();Drive(()=>app.Host.Current.Step>10,1);
            Assert.That(Shown(Find("pause-layer")),Is.False,"nothing over the court while playing");
            Click("rally-pause");Refresh();yield return null;yield return null;
            Assert.That(app.Clock.Paused,Is.True);Assert.That(Shown(Find("pause-layer")),Is.True);
            Assert.That(Find<Label>("pause-title").text,Is.EqualTo("PAUSA"));
            Assert.That(Find<Button>("pause-continue").text,Is.EqualTo("Continuar"));
            Assert.That(Find<Button>("pause-menu").text,Is.EqualTo("Salir al menú"));
            var card=Find("pause-card").worldBound;
            Assert.That(card.yMin,Is.GreaterThanOrEqualTo(Find("boot-header").worldBound.yMax),"the card does not cover the header");
            Assert.That(card.xMin,Is.GreaterThanOrEqualTo(root.worldBound.xMin));Assert.That(card.xMax,Is.LessThanOrEqualTo(root.worldBound.xMax));
            Assert.That(card.yMax,Is.LessThanOrEqualTo(root.worldBound.yMax));
            foreach(var name in new[]{"pause-continue","pause-menu"})Assert.That(Find(name).worldBound.height,Is.GreaterThanOrEqualTo(44),name);
            long step=app.Host.Current.Step;Drive(()=>false,.3);Assert.That(app.Host.Current.Step,Is.EqualTo(step),"the game waits");
            Click("pause-continue");Refresh();yield return null;
            Assert.That(app.Clock.Paused,Is.False);Assert.That(Shown(Find("pause-layer")),Is.False);
            Assert.That(Find<Button>("rally-pause").text,Is.EqualTo("Pausa"));Assert.That(rally.Active,Is.True);
            Drive(()=>false,.2);Assert.That(app.Host.Current.Step,Is.GreaterThan(step));
        }
        [UnityTest] public IEnumerator Pause_LeaveToMenu_EndsTheMatch_AndTheNextOneStartsFromZero()
        {
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            yield return StartMatch();
            Assert.That(Drive(()=>rally.HumanPoints+rally.RivalPoints>=1,300),Is.True);
            Click("rally-pause");Refresh();yield return null;
            Click("pause-menu");Refresh();yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.Menu));Assert.That(rally.Active,Is.False);
            Assert.That(Shown(Find("menu-layer")),Is.True);Assert.That(Shown(Find("pause-layer")),Is.False);Assert.That(app.Clock.Paused,Is.True);
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null);Assert.That(rally.HumanPoints+rally.RivalPoints,Is.EqualTo(0));
            Assert.That(app.Clock.Paused,Is.False);Assert.That(Shown(Find("pause-layer")),Is.False,"the old pause does not leak into the new match");
            Assert.That(Find<Button>("rally-pause").text,Is.EqualTo("Pausa"));
        }
        [UnityTest] public IEnumerator ClockPausedByTheDevTools_ShowsNoCard()
        {
            yield return StartMatch();Drive(()=>app.Host.Current.Step>10,1);
            app.Clock.Paused=true;Refresh();yield return null;
            Assert.That(Shown(Find("pause-layer")),Is.False,"only the player's Pausa opens the card");
            app.Clock.Paused=false;Refresh();
        }
        [UnityTest] public IEnumerator MatchOver_OffersRematchAndBackToTheMenu()
        {
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            yield return StartMatch();
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);Refresh();
            // The fixture drives the HUD by hand: let the result card finish its fade-in before asking what is visible.
            hud.Tick(RallyHud.ResultInSeconds);yield return null;yield return null;
            Assert.That(Find("match-result-card").resolvedStyle.opacity,Is.EqualTo(1f).Within(.001f));
            Assert.That(Shown(Find("match-result")),Is.True);Assert.That(Shown(Find("pause-layer")),Is.False);
            Assert.That(Find<Button>("match-result-rematch").text,Is.EqualTo("Revancha"));
            Assert.That(Find<Button>("match-result-menu").text,Is.EqualTo("Volver al inicio"));
            Assert.That(Shown(Find("match-result-leave")),Is.True,"dev keeps the way back to practice");
            app.DevTools=false;Refresh();yield return null;yield return null;
            Assert.That(Shown(Find("match-result-leave")),Is.False,"practice is a developer destination");
            var card=Find("match-result-card").worldBound;
            foreach(var name in new[]{"match-result-rematch","match-result-menu"})
            {
                var b=Find(name).worldBound;Assert.That(Shown(Find(name)),Is.True,name);
                Assert.That(b.xMin,Is.GreaterThanOrEqualTo(card.xMin),name);Assert.That(b.xMax,Is.LessThanOrEqualTo(card.xMax),name);
                Assert.That(b.yMax,Is.LessThanOrEqualTo(card.yMax),name);Assert.That(b.height,Is.GreaterThanOrEqualTo(44),name);
            }
            Assert.That(card.yMin,Is.GreaterThanOrEqualTo(Find("boot-header").worldBound.yMax));Assert.That(card.yMax,Is.LessThanOrEqualTo(root.worldBound.yMax));
            Click("match-result-menu");Refresh();yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.Menu));Assert.That(rally.Active,Is.False);
            Assert.That(Shown(Find("match-result")),Is.False);Assert.That(Shown(Find("menu-layer")),Is.True);
        }
    }
}
