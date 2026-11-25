using System.Collections;
using NUnit.Framework;
using SquashClash.Runtime.Menu;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // The tutorial shown once before the first match and reopened from the menu.
    public sealed class ZFirstRunTutorialTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private bool Seen=>PlayerPrefs.GetInt("sc.firstRun.tutorialSeen",0)==1;
        [UnityTest] public IEnumerator FirstPlay_ShowsTheTutorialOnce_ThenStartsTheMatch()
        {
            Assert.That(Seen,Is.False);
            Click("menu-play");Refresh();yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.Tutorial));Assert.That(Shown(Find("tutorial-layer")),Is.True);
            Assert.That(Shown(Find("menu-layer")),Is.False);
            Assert.That(rally.Active,Is.False,"the match waits for the tutorial");Assert.That(app.Clock.Paused,Is.True);
            Assert.That(Find<Label>("tutorial-step").text,Is.EqualTo("1 de 6"));
            Assert.That(Find<Button>("tutorial-next").text,Is.EqualTo("Siguiente"));
            Click("tutorial-next");Refresh();Assert.That(Find<Label>("tutorial-step").text,Is.EqualTo("2 de 6"));
            Click("tutorial-next");Refresh();Assert.That(Find<Label>("tutorial-step").text,Is.EqualTo("3 de 6"));
            Assert.That(Find<Button>("tutorial-next").text,Is.EqualTo("Siguiente"));
            Click("tutorial-next");Refresh();Assert.That(Find<Label>("tutorial-step").text,Is.EqualTo("4 de 6"));
            Assert.That(Find<Button>("tutorial-next").text,Is.EqualTo("Siguiente"));
            Click("tutorial-next");Refresh();Assert.That(Find<Label>("tutorial-step").text,Is.EqualTo("5 de 6"));
            Assert.That(Find<Button>("tutorial-next").text,Is.EqualTo("Siguiente"));
            Click("tutorial-next");Refresh();Assert.That(Find<Label>("tutorial-step").text,Is.EqualTo("6 de 6"));
            Assert.That(Find<Label>("tutorial-title").text,Is.EqualTo("Saque"));
            Assert.That(Find<Button>("tutorial-next").text,Is.EqualTo("¡A jugar!"));
            Assert.That(Seen,Is.False,"it counts as seen only once finished or skipped");
            Click("tutorial-next");Refresh();yield return null;
            Assert.That(Seen,Is.True);Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.None));
            Assert.That(rally.Active,Is.True);Assert.That(rally.Match,Is.Not.Null);Assert.That(app.Clock.Paused,Is.False);
            Assert.That(Shown(Find("tutorial-layer")),Is.False,"no tutorial element stays on screen during play");
            // Second time: straight to the match.
            flow.OpenMenu();Refresh();Click("menu-play");Refresh();yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.None));Assert.That(rally.Match,Is.Not.Null);
            Assert.That(Shown(Find("tutorial-layer")),Is.False);
        }
        [UnityTest] public IEnumerator Skip_StartsTheMatch_AndCountsAsSeen()
        {
            Click("menu-play");Refresh();Click("tutorial-next");Refresh();
            Assert.That(Find<Button>("tutorial-skip").text,Is.EqualTo("Saltar"));
            Click("tutorial-skip");Refresh();yield return null;
            Assert.That(Seen,Is.True);Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.None));Assert.That(rally.Match,Is.Not.Null);
        }
        [UnityTest] public IEnumerator HowToPlay_ReopensTheTutorial_AndReturnsToTheMenu()
        {
            rally.SetNewServe(false);
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            Click("menu-howto");Refresh();yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.Tutorial));Assert.That(Find<Label>("tutorial-step").text,Is.EqualTo("1 de 5"));
            Click("tutorial-next");Click("tutorial-next");Click("tutorial-next");Click("tutorial-next");Refresh();
            Assert.That(Find<Button>("tutorial-next").text,Is.EqualTo("Listo"));
            Click("tutorial-next");Refresh();yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.Menu));Assert.That(Shown(Find("menu-layer")),Is.True);
            Assert.That(rally.Active,Is.False);Assert.That(app.Clock.Paused,Is.True);
            // Skipping from the menu's tutorial also goes back to the menu.
            Click("menu-howto");Refresh();Click("tutorial-skip");Refresh();yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.Menu));Assert.That(rally.Active,Is.False);
        }
        [UnityTest] public IEnumerator Steps_CoverAimingPullingTheTimingRingAndTheVolley()
        {
            Assert.That(FirstRunFlow.TutorialSteps,Is.EqualTo(5));
            Click("menu-howto");Refresh();yield return null;
            var title=Find<Label>("tutorial-title");var body=Find<Label>("tutorial-body");var results=Find("tutorial-results");
            Assert.That(title.text,Is.EqualTo("Apunta"));
            Assert.That(body.text,Does.Contain("Apoya el dedo").And.Contain("pared del frente"));
            Assert.That(Shown(results),Is.False);
            Click("tutorial-next");Refresh();yield return null;
            Assert.That(title.text,Is.EqualTo("Carga y suelta"));
            Assert.That(body.text,Does.Contain("hacia abajo").And.Contain("Suelta"));
            Click("tutorial-next");Refresh();yield return null;
            Assert.That(title.text,Is.EqualTo("Suelta a tiempo"));
            Assert.That(body.text,Does.Contain("anillo").And.Contain("triángulo"));
            Assert.That(Shown(results),Is.True);
            Assert.That(Find<Label>("tutorial-result-perfect").text,Does.StartWith("Perfecto"));
            Assert.That(Find<Label>("tutorial-result-valid").text,Does.StartWith("Válido"));
            Assert.That(Find<Label>("tutorial-result-failed").text,Does.StartWith("Fallido"));
            // 19 nov (PO): the volley, last: the amber ring before the bounce.
            Click("tutorial-next");Refresh();yield return null;
            Assert.That(title.text,Is.EqualTo("Volea"));
            Assert.That(body.text,Does.Contain("ámbar").And.Contain("bote").And.Contain("aire"));
            Assert.That(Shown(results),Is.False,"the result list belongs to the timing step");
            // 21 nov: the two-wall shot (boast), last: the visible side wall, near where you will hit, not on the serve.
            Click("tutorial-next");Refresh();yield return null;
            Assert.That(title.text,Is.EqualTo("Dos paredes"));
            Assert.That(body.text,Does.Contain("pared lateral").And.Contain("frente").And.Contain("saque").And.Contain("Tócala bajo").And.Contain("frontis").And.Contain("más o menos"));
            Assert.That(Shown(results),Is.False);
        }
        [UnityTest] public IEnumerator NewServeAddsTheSixthStepWhenTheTutorialOpens()
        {
            rally.SetNewServe(true);
            Click("menu-howto");
            Assert.That(Find<Label>("tutorial-step").text, Is.EqualTo("1 de 6"));
            rally.SetNewServe(false);
            for (int i = 0; i < 5; i++) Click("tutorial-next");
            Assert.That(Find<Label>("tutorial-step").text, Is.EqualTo("6 de 6"));
            Assert.That(Find<Label>("tutorial-title").text, Is.EqualTo("Saque"));
            Assert.That(Find<Label>("tutorial-body").text, Is.EqualTo("Empiezas sacando tú. Toca el frontis: arriba de la línea, globo; abajo, drive. Si no tocas, globo. Cada rally es un punto y saca quien lo gana. Partido a 7, con 2 de ventaja."));
            Assert.That(Find<Button>("tutorial-next").text, Is.EqualTo("Listo"));
            Click("tutorial-next");
            Click("menu-howto");
            Assert.That(Find<Label>("tutorial-step").text, Is.EqualTo("1 de 5"));
            yield return null;
        }
        // The target and the margin in the serve step come from the rally parameters, not from the text.
        [UnityTest] public IEnumerator ServeStepTakesTheTargetAndMarginFromTheParameters()
        {
            rally.SetNewServe(true);
            var p = rally.Params.Clone(); p.MatchTarget = 11; p.MatchMargin = 3; rally.SetParams(p);
            Click("menu-howto");
            for (int i = 0; i < 5; i++) Click("tutorial-next");
            Assert.That(Find<Label>("tutorial-body").text, Does.EndWith("Partido a 11, con 3 de ventaja."));
            yield return null;
        }

        [UnityTest] public IEnumerator EveryStep_FitsThePortraitScreen_WithFingerSizedButtons()
        {
            const int steps=6;
            Click("menu-howto");Refresh();
            for(int step=0;step<steps;step++)
            {
                yield return null;yield return null;
                var card=Find("tutorial-card").worldBound;
                foreach(var e in ShownUnder("tutorial-layer"))
                {
                    Assert.That(e.worldBound.xMin,Is.GreaterThanOrEqualTo(root.worldBound.xMin-.5f),step+" "+e.name);Assert.That(e.worldBound.xMax,Is.LessThanOrEqualTo(root.worldBound.xMax+.5f),step+" "+e.name);
                    Assert.That(e.worldBound.yMin,Is.GreaterThanOrEqualTo(root.worldBound.yMin-.5f),step+" "+e.name);Assert.That(e.worldBound.yMax,Is.LessThanOrEqualTo(root.worldBound.yMax+.5f),step+" "+e.name);
                }
                var text=Find("tutorial-body").worldBound;
                Assert.That(text.yMax,Is.LessThanOrEqualTo(Find("tutorial-next").worldBound.yMin+.5f),"step "+step+": the text does not run under the buttons");
                Assert.That(text.xMin,Is.GreaterThanOrEqualTo(card.xMin));Assert.That(text.xMax,Is.LessThanOrEqualTo(card.xMax));
                foreach(var name in new[]{"tutorial-next","tutorial-skip"})Assert.That(Find(name).worldBound.height,Is.GreaterThanOrEqualTo(44),name);
                if(step<steps-1){Click("tutorial-next");Refresh();}
            }
        }
    }
}
