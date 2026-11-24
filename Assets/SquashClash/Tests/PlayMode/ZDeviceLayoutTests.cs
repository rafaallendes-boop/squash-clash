using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Device robustness, block A: on every phone from the 16:9 SE to the Pro Max, nothing a player reads or touches
    // (menu, tutorial, HUD, point banner, pause card, result card) sits under the notch, the Dynamic Island or the home
    // indicator, leaves the screen, gets squashed or overlaps another button.
    public sealed class ZDeviceLayoutTests : ZDeviceFixture
    {
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private IEnumerator Settle(){Refresh();yield return null;yield return null;Refresh();yield return null;}
        private IEnumerator StartMatch()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");yield return Settle();
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
        }
        [UnityTest] public IEnumerator ServeGuideLabelsFitInsideTheSafeFrontWall([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen, false);
            app.Tuning.Set("fatigue.enabled", 0);
            app.Clock.Paused = false;
            rally.SetNewServe(true);
            rally.Autopilot = true;
            rally.AutopilotTimingSigmaMs = 0;
            rally.Level = AiLevel.Easy;
            rally.Seed = 3;
            Assert.That(rally.StartMatch(), Is.True);
            Assert.That(Drive(() => rally.Phase == RallyPhase.PointOver && rally.Match.Server == PlayerId.Human, 900), Is.True);
            Assert.That(Drive(() => root.Q("serve-guide")?.style.opacity.value >= 1, .25), Is.True);
            yield return Settle();

            var camera = UnityEngine.Camera.main;
            var court = app.Host.ActiveConfig.Court;
            Vector2 Panel(float x, float y)
            {
                var p = camera.WorldToScreenPoint(new Vector3(x, y, 0));
                return new Vector2(p.x / phone.Width * root.layout.width, (1 - p.y / phone.Height) * root.layout.height);
            }
            var topAtZero = Panel(0, court.WallHeight);
            var topAtWidth = Panel(court.Width, court.WallHeight);
            var bottom = Panel(court.Width, 0);
            float left = Mathf.Min(topAtZero.x, topAtWidth.x);
            float right = Mathf.Max(topAtZero.x, topAtWidth.x);
            AssertFits("serve-guide", phone.ToString());
            var labels = new[] { Find("serve-guide-lob"), Find("serve-guide-drive") };
            foreach (var label in labels)
            {
                Assert.That(label.worldBound.xMin, Is.GreaterThanOrEqualTo(left - 1));
                Assert.That(label.worldBound.xMax, Is.LessThanOrEqualTo(right + 1));
                Assert.That(label.worldBound.yMin, Is.GreaterThanOrEqualTo(topAtZero.y - 1));
                Assert.That(label.worldBound.yMax, Is.LessThanOrEqualTo(bottom.y + 1));
            }
            var line = Find("serve-guide-line").worldBound;
            Assert.That(labels[0].worldBound.yMax, Is.EqualTo(line.center.y - 7.5f).Within(1));
            Assert.That(labels[1].worldBound.yMin, Is.EqualTo(line.center.y + 7.5f).Within(1));
        }

        [UnityTest] public IEnumerator SafeArea_IsMeasuredInPanelUnits_AndTheHeaderStartsBelowIt([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,true);yield return Settle();
            var layout=app.GetComponent<SafeAreaLayout>();Assert.That(layout,Is.Not.Null,"the app owns the safe-area layout");
            float unit=root.worldBound.height/phone.Height;
            Assert.That(layout.Top,Is.EqualTo(phone.Top*unit).Within(.5f),"top inset in panel units");
            Assert.That(layout.Bottom,Is.EqualTo(phone.Bottom*unit).Within(.5f),"bottom inset in panel units");
            float edge=Mathf.Max(SafeAreaLayout.MinTop,phone.Top*unit);
            Assert.That(root.resolvedStyle.paddingTop,Is.InRange(edge-.06f,edge+1f),"the HUD starts right under the inset, or at a small margin without one");
            Assert.That(Find("boot-header").worldBound.yMin,Is.GreaterThanOrEqualTo(Safe.yMin-.5f));
        }
        // The mechanism, not a real phone: no 16:9 iPhone has insets. With the player's menu (the developer one has a
        // button more and would not fit a 16:9 screen that also had a notch and a home indicator).
        [UnityTest] public IEnumerator SafeArea_FollowsAChangeOfInsets()
        {
            yield return Boot(Phones[0],true);app.DevTools=false;yield return Settle();
            float before=Find("boot-header").worldBound.yMin;
            phone=new PhoneScreen("SE with insets",phone.Width,phone.Height,100,60);
            SafeAreaLayout.Simulate(new Vector2(phone.Width,phone.Height),phone.SafePixels);yield return Settle();
            Assert.That(Find("boot-header").worldBound.yMin,Is.GreaterThan(before+30),"the header moved down with the inset");
            AssertFits("menu-layer","after the insets changed");
        }
        [UnityTest] public IEnumerator Menu_FitsInsideTheSafeArea([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,true);
            foreach(bool dev in new[]{true,false})
            {
                app.DevTools=dev;yield return Settle();
                AssertFits("menu-layer",phone+(dev?" dev":" prod"));
            }
        }
        // Menu redesign (19 nov): the credits screen, from the 16:9 SE to the Pro Max.
        [UnityTest] public IEnumerator Credits_FitInsideTheSafeArea([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,true);app.DevTools=false;yield return Settle();
            Click("menu-credits");yield return Settle();
            AssertFits("credits-layer",phone+" credits");
        }
        // Personal records (20 nov): the "Tus récords" screen, and the menu with its third link, from the 16:9 SE to the Pro Max.
        [UnityTest] public IEnumerator Records_FitInsideTheSafeArea([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,true);app.DevTools=false;yield return Settle();
            Click("menu-records");yield return Settle();
            AssertFits("records-layer",phone+" records");
        }
        [UnityTest] public IEnumerator Tutorial_EveryStepFitsInsideTheSafeArea([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,true);
            Click("menu-howto");
            for(int step=0;step<6;step++)
            {
                yield return Settle();
                Assert.That(flow.TutorialStep,Is.EqualTo(step));
                AssertFits("tutorial-layer",phone+" step "+(step+1));
                Click("tutorial-next");
            }
        }
        [UnityTest] public IEnumerator Match_HeaderBannerPauseAndResultFitInsideTheSafeArea([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,true);
            rally.SetNewServe(false); // Exercise the ordinary point banner. The serve guide has its own layout test.
            // The developer header carries the most buttons; the player's header must fit as well.
            yield return StartMatch();Drive(()=>app.Host.Current.Step>10,1);yield return Settle();
            AssertFits("boot-header",phone+" dev header");
            app.DevTools=false;yield return Settle();
            AssertFits("boot-header",phone+" player header");
            var header=Find("boot-header");

            Click("rally-pause");yield return Settle();
            AssertFits("pause-layer",phone+" pause");
            Assert.That(Find("pause-card").worldBound.yMin,Is.GreaterThanOrEqualTo(header.worldBound.yMax),"the pause card does not cover the header");
            Click("pause-continue");yield return Settle();

            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,300),Is.True,"a point must end");
            hud.Tick(RallyHud.BannerInSeconds);yield return Settle();
            AssertFits("rally-banner",phone+" banner");
            AssertFits("boot-header",phone+" header between points");
            Assert.That(Find("rally-banner-card").worldBound.yMin,Is.GreaterThanOrEqualTo(header.worldBound.yMax),"the banner does not cover the header");

            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True,"the demo match must finish");
            hud.Tick(RallyHud.ResultInSeconds);yield return Settle();
            AssertFits("match-result",phone+" result");
            Assert.That(Find("match-result-card").worldBound.yMin,Is.GreaterThanOrEqualTo(header.worldBound.yMax),"the result card does not cover the header");
        }
        // «vs» plate (front 4, criterion 8): inside the safe area on every phone, under the header, with no text cut or crossing the centre line.
        [UnityTest] public IEnumerator VsPlate_FitsInsideTheSafeArea_AndDoesNotCoverTheHeader([ValueSource(nameof(Phones))] PhoneScreen screen,[Values(RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo)] RivalStyleId style)
        {
            yield return Boot(screen,true);
            double clock=100;VsPlate.Now=()=>clock;
            try
            {
                rally.Seed=9;rally.Autopilot=false;rally.StyleOverride=style;
                PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);Click("menu-play");yield return Settle();
                Assert.That(rally.Style,Is.EqualTo(style));Assert.That(rally.Phase,Is.EqualTo(RallyPhase.RivalReception));
                clock+=RallyText.VsInSeconds+.25;yield return Settle();
                var plate=Find("vs-plate");var header=Find("boot-header");
                AssertFits("vs-plate",phone+" "+style);
                Assert.That(Find<Label>("vs-plate-style").text,Is.EqualTo(RivalStyle.Name(style)));
                Assert.That(plate.worldBound.yMin,Is.GreaterThanOrEqualTo(header.worldBound.yMax),phone+": the plate does not cover the header");
                var safe=Safe;var box=plate.worldBound;
                Assert.That(box.xMin,Is.GreaterThanOrEqualTo(safe.xMin-.5f));Assert.That(box.xMax,Is.LessThanOrEqualTo(safe.xMax+.5f));Assert.That(box.yMax,Is.LessThanOrEqualTo(safe.yMax+.5f),"not under the home indicator");
                Assert.That(box.height,Is.EqualTo(64f).Within(1f),"64 px tall");
                var line=Find("vs-plate-line").worldBound;
                foreach(var name in new[]{"vs-plate-you","vs-plate-mid","vs-plate-rival","vs-plate-style"})
                    Assert.That(Find(name).worldBound.Overlaps(line),Is.False,phone+": "+name+" crosses the centre line");
                Assert.That(Find("vs-plate-you").worldBound.Overlaps(Find("vs-plate-mid").worldBound),Is.False,phone+": TÚ and vs overlap");
                Assert.That(Find("vs-plate-rival").worldBound.Overlaps(Find("vs-plate-style").worldBound),Is.False,phone+": RIVAL and the style overlap");
            }
            finally{VsPlate.Reset();}
        }
        // Performance ceiling: the layout is checked every frame and must cost no garbage while nothing changes.
        [UnityTest] public IEnumerator Apply_AllocatesNothing_WhileNothingChanges()
        {
            yield return Boot(Phones[3],true);yield return Settle();
            var layout=app.GetComponent<SafeAreaLayout>();Assert.That(layout,Is.Not.Null);
            for(int i=0;i<10;i++)layout.Apply();
            long before=System.GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<2000;i++)layout.Apply();
            Assert.That(System.GC.GetAllocatedBytesForCurrentThread()-before,Is.EqualTo(0),"bytes for 2000 checks");
        }
    }
}
