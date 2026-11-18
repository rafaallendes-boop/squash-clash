using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // First run, block A: the start menu is the normal way from launch to a match. Named to run last (see ZRallyHudTests).
    public sealed class ZFirstRunMenuTests : ZFirstRunFixture
    {
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private void MarkTutorialSeen(){PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);}
        [UnityTest] public IEnumerator SlingStatus_DisplayFollowsTheCompiledDevDefine_WithoutStoppingUpdates()
        {
            yield return Boot(false);
            var status=Find<Label>("sling-status");
#if SQUASH_DEV
            Assert.That(status.resolvedStyle.display,Is.EqualTo(DisplayStyle.Flex));
            Assert.That(Shown(status),Is.True);
#else
            Assert.That(status.resolvedStyle.display,Is.EqualTo(DisplayStyle.None),"prod has no technical line");
            Assert.That(status.worldBound.height,Is.EqualTo(0),"prod reserves no space");
#endif
        }
        [UnityTest] public IEnumerator LevelSelection_IsSaved_AndRestoredOnANewLaunch()
        {
            yield return Boot(true);
            foreach(var name in new[]{"easy","hard","medium"})
            {
                Click("menu-level-"+name);
                var selected=flow.Level;
                Assert.That(PlayerPrefs.GetInt("sc.firstRun.level",-1),Is.EqualTo((int)selected),"selection is saved");
                int saved=PlayerPrefs.GetInt("sc.firstRun.level",-1);
                yield return Shutdown();yield return Boot(true,saved);
                Assert.That(flow.Level,Is.EqualTo(selected),"a new flow reads the saved choice");
                Assert.That(Find<Button>("menu-level-"+name).ClassListContains("sc-level--selected"),Is.True);
                MarkTutorialSeen();Click("menu-play");Refresh();yield return null;
                Assert.That(rally.Level,Is.EqualTo(selected),"Jugar uses the restored level");
                flow.OpenMenu();Refresh();
            }
        }
        [UnityTest] public IEnumerator InvalidSavedLevel_FallsBackToMedium()
        {
            bool booted=false;
            foreach(int invalid in new[]{-1,3,int.MinValue,int.MaxValue})
            {
                if(booted)yield return Shutdown();
                yield return Boot(true,invalid);
                booted=true;
                Assert.That(flow.Level,Is.EqualTo(AiLevel.Medium));
                Assert.That(Find<Button>("menu-level-medium").ClassListContains("sc-level--selected"),Is.True);
            }
        }
        [UnityTest] public IEnumerator NormalLaunch_OpensTheMenu_WithTheGamePausedAndNoMatch()
        {
            yield return Boot(true);
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.Menu));
            Assert.That(Shown(Find("menu-layer")),Is.True);
            Assert.That(app.Clock.Paused,Is.True,"nothing plays behind the menu");Assert.That(rally.Active,Is.False);
            Assert.That(Find<Label>("menu-title").text,Is.EqualTo("SQUASH CLASH"));
            Assert.That(Find<Button>("menu-play").text,Is.EqualTo("Jugar"));
            Assert.That(Find<Button>("menu-howto").text,Is.EqualTo("¿Cómo se juega?"));
            Assert.That(Find<Label>("menu-build").text,Is.EqualTo("v"+app.Build.Label));
            Assert.That(flow.Level,Is.EqualTo(AiLevel.Medium));
            Assert.That(Find<Button>("menu-level-medium").ClassListContains("sc-level--selected"),Is.True);
            Assert.That(Find<Button>("menu-level-easy").ClassListContains("sc-level--selected"),Is.False);
            Assert.That(Find<Button>("menu-level-hard").ClassListContains("sc-level--selected"),Is.False);
        }
        [UnityTest] public IEnumerator BatchLaunch_KeepsTheOldBoot_SoAutomationIsUntouched()
        {
            Assume.That(Application.isBatchMode,Is.True,"only meaningful under scripts/test.sh");
            yield return Boot(null);
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.None));Assert.That(app.Clock.Paused,Is.False);
            Assert.That(Shown(Find("menu-layer")),Is.False);
            Assert.That(app.Host.ActiveConfig.Hash,Is.EqualTo("3262f9b89156"));
            Assert.That(Find<Label>("boot-title").text,Is.EqualTo("Squash Clash — First Ball"));
            Assert.That(Shown(Find("rally-toggle")),Is.True);
        }
        [UnityTest] public IEnumerator Play_StartsAMatchAgainstTheChosenLevel_AndTheMenuIsGone()
        {
            MarkTutorialSeen();yield return Boot(true);MarkTutorialSeen();
            Click("menu-level-hard");Refresh();
            Assert.That(flow.Level,Is.EqualTo(AiLevel.Hard));
            Assert.That(Find<Button>("menu-level-hard").ClassListContains("sc-level--selected"),Is.True);
            Assert.That(Find<Button>("menu-level-medium").ClassListContains("sc-level--selected"),Is.False);
            Click("menu-play");Refresh();yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.None));
            Assert.That(rally.Active,Is.True);Assert.That(rally.Match,Is.Not.Null,"Jugar starts a match, not an open rally");
            Assert.That(rally.Level,Is.EqualTo(AiLevel.Hard));Assert.That(app.Clock.Paused,Is.False);
            Assert.That(Shown(Find("menu-layer")),Is.False);
            // During play no first-run element is on screen, so none can take a touch.
            foreach(var name in new[]{"menu-layer","menu-play","menu-howto","menu-level-easy"})Assert.That(Shown(Find(name)),Is.False,name);
            long step=app.Host.Current.Step;Drive(()=>false,.2);Assert.That(app.Host.Current.Step,Is.GreaterThan(step),"the match is running");
        }
        [UnityTest] public IEnumerator EachLevelButton_SelectsItsLevel()
        {
            yield return Boot(true);
            Click("menu-level-easy");Assert.That(flow.Level,Is.EqualTo(AiLevel.Easy));
            Click("menu-level-hard");Assert.That(flow.Level,Is.EqualTo(AiLevel.Hard));
            Click("menu-level-medium");Assert.That(flow.Level,Is.EqualTo(AiLevel.Medium));
            Assert.That(Find<Button>("menu-level-easy").text,Is.EqualTo("Fácil"));
            Assert.That(Find<Button>("menu-level-medium").text,Is.EqualTo("Media"));
            Assert.That(Find<Button>("menu-level-hard").text,Is.EqualTo("Difícil"));
        }
        [UnityTest] public IEnumerator OpenMenu_DuringAMatch_StopsItAndPauses()
        {
            MarkTutorialSeen();yield return Boot(true);MarkTutorialSeen();
            Click("menu-play");Refresh();Drive(()=>app.Host.Current.Step>10,1);
            flow.OpenMenu();Refresh();yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.Menu));Assert.That(rally.Active,Is.False);
            Assert.That(app.Clock.Paused,Is.True);Assert.That(Shown(Find("menu-layer")),Is.True);
        }
        [UnityTest] public IEnumerator Prod_ShowsOnlyThePlayersControls_DevKeepsDemoAndPractice()
        {
            yield return Boot(true);
            // Editor and dev builds carry the developer tools.
            Assert.That(app.DevTools,Is.True);
            Assert.That(Shown(Find("menu-practice")),Is.True,"dev can leave the menu for practice");
            app.DevTools=false;Refresh();yield return null;
            Assert.That(Shown(Find("menu-practice")),Is.False);
            Assert.That(Find<Label>("boot-title").text,Is.EqualTo("Squash Clash"));
            Assert.That(Shown(Find("fps-label")),Is.False);
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null);
            foreach(var name in new[]{"rally-toggle","rally-demo","rally-rematch","rally-match"})Assert.That(Shown(Find(name)),Is.False,name+" is a developer control");
            foreach(var name in new[]{"rally-pause","rally-score","rally-status"})Assert.That(Shown(Find(name)),Is.True,name);
            app.DevTools=true;Refresh();yield return null;
            foreach(var name in new[]{"rally-toggle","rally-demo","rally-rematch"})Assert.That(Shown(Find(name)),Is.True,name);
            Assert.That(Find<Label>("boot-title").text,Is.EqualTo("Squash Clash — First Ball"));
        }
        [UnityTest] public IEnumerator DevPracticeButton_ClosesTheMenu_IntoTheOldPractice()
        {
            yield return Boot(true);
            Click("menu-practice");Refresh();yield return null;
            Assert.That(flow.Screen,Is.EqualTo(FirstRunScreen.None));Assert.That(app.Clock.Paused,Is.False);
            Assert.That(rally.Active,Is.False);Assert.That(Shown(Find("menu-layer")),Is.False);
            Assert.That(app.Host.ActiveConfig.Hash,Is.EqualTo("3262f9b89156"));
        }
        // Performance ceiling: the flow runs every frame and must cost no garbage, with the menu open and during a match.
        [UnityTest] public IEnumerator Refresh_AllocatesNothing_InTheMenuAndDuringAMatch()
        {
            MarkTutorialSeen();yield return Boot(true);MarkTutorialSeen();
            for(int i=0;i<10;i++)flow.Refresh();
            long before=System.GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<2000;i++)flow.Refresh();
            Assert.That(System.GC.GetAllocatedBytesForCurrentThread()-before,Is.EqualTo(0),"bytes for 2000 refreshes with the menu open");
            Click("menu-play");Refresh();Drive(()=>app.Host.Current.Step>50,2);
            Assert.That(rally.Active,Is.True);
            before=System.GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<2000;i++)flow.Refresh();
            Assert.That(System.GC.GetAllocatedBytesForCurrentThread()-before,Is.EqualTo(0),"bytes for 2000 refreshes during a match");
        }
        [UnityTest] public IEnumerator Menu_FitsThePortraitScreen_WithFingerSizedButtons()
        {
            yield return Boot(true);yield return null;
            Assert.That(Screen.width,Is.EqualTo(Width));Assert.That(Screen.height,Is.EqualTo(Height));
            var layer=Find("menu-layer");
            Assert.That(layer.worldBound.width,Is.EqualTo(root.worldBound.width).Within(1));Assert.That(layer.worldBound.height,Is.EqualTo(root.worldBound.height).Within(1));
            foreach(var e in ShownUnder("menu-layer"))
            {
                Assert.That(e.worldBound.xMin,Is.GreaterThanOrEqualTo(root.worldBound.xMin-.5f),e.name);Assert.That(e.worldBound.xMax,Is.LessThanOrEqualTo(root.worldBound.xMax+.5f),e.name);
                Assert.That(e.worldBound.yMin,Is.GreaterThanOrEqualTo(root.worldBound.yMin-.5f),e.name);Assert.That(e.worldBound.yMax,Is.LessThanOrEqualTo(root.worldBound.yMax+.5f),e.name);
            }
            foreach(var name in new[]{"menu-play","menu-howto","menu-level-easy","menu-level-medium","menu-level-hard"})
                Assert.That(Find(name).worldBound.height,Is.GreaterThanOrEqualTo(44),name+" is at least 44 pt tall");
        }
    }
}
