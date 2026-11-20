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
            for(int step=0;step<SquashClash.Runtime.Menu.FirstRunFlow.TutorialSteps;step++)
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
