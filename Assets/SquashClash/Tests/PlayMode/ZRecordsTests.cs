using System.Collections;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Personal records (20 nov): the bests of the player's own menu matches, kept in PlayerPrefs (sc.records.*).
    // The rules, pure: no scene needed.
    public sealed class ZPersonalRecordsRulesTests
    {
        [SetUp] public void Clean(){PersonalRecords.Enabled=true;PersonalRecords.Clear();}
        [TearDown] public void Restore(){PersonalRecords.Enabled=true;PersonalRecords.Clear();}
        private static MatchSummary Win(int rival=3,int rally=4,float seconds=300,AiLevel level=AiLevel.Medium)=>new MatchSummary(true,level,7,rival,rally,seconds);
        private static MatchSummary Loss(int human=3,int rally=4,float seconds=300)=>new MatchSummary(false,AiLevel.Medium,human,7,rally,seconds);
        [Test] public void FirstMatch_SetsTheLongestRally_AndSaysSo()
        {
            var broken=PersonalRecords.Apply(Loss(rally:5));
            Assert.That(broken,Is.EqualTo(new[]{"Rally más largo: 5 golpes"}));
            Assert.That(PersonalRecords.LongestRally,Is.EqualTo(5));Assert.That(PersonalRecords.Streak,Is.EqualTo(0));
            Assert.That(PersonalRecords.BestMargin,Is.EqualTo(0),"a loss has no margin");Assert.That(PersonalRecords.FastestWin,Is.EqualTo(0f));
        }
        [Test] public void ATieOrAShorterRally_IsNotARecord()
        {
            PersonalRecords.Apply(Loss(rally:9));
            Assert.That(PersonalRecords.Apply(Loss(rally:9)),Is.Empty);Assert.That(PersonalRecords.Apply(Loss(rally:8)),Is.Empty);
            Assert.That(PersonalRecords.LongestRally,Is.EqualTo(9));
        }
        [Test] public void WinStreak_CountsInARow_OnlyCelebratesFromTwo_AndALossResetsTheCurrentOnly()
        {
            Assert.That(PersonalRecords.Apply(Win(rally:2)).Any(l=>l.StartsWith("Mejor racha")),Is.False,"a first win is a streak of one");
            Assert.That(PersonalRecords.Apply(Win(rally:2)),Does.Contain("Mejor racha: 2 victorias"));
            Assert.That(PersonalRecords.Apply(Win(rally:2)),Does.Contain("Mejor racha: 3 victorias"));
            PersonalRecords.Apply(Loss(rally:2));
            Assert.That(PersonalRecords.Streak,Is.EqualTo(0));Assert.That(PersonalRecords.BestStreak,Is.EqualTo(3));
            PersonalRecords.Apply(Win(rally:2));PersonalRecords.Apply(Win(rally:2));
            Assert.That(PersonalRecords.BestStreak,Is.EqualTo(3),"two in a row do not beat three");Assert.That(PersonalRecords.Streak,Is.EqualTo(2));
        }
        [Test] public void Wins_AreCountedPerLevel_AndLossesNeverCount()
        {
            PersonalRecords.Apply(Win(level:AiLevel.Easy));PersonalRecords.Apply(Win(level:AiLevel.Hard));PersonalRecords.Apply(Win(level:AiLevel.Hard));
            PersonalRecords.Apply(Loss());
            Assert.That(PersonalRecords.Wins(AiLevel.Easy),Is.EqualTo(1));Assert.That(PersonalRecords.Wins(AiLevel.Medium),Is.EqualTo(0));Assert.That(PersonalRecords.Wins(AiLevel.Hard),Is.EqualTo(2));
        }
        [Test] public void BestMargin_IsOnlyForWins_AndMustBeBeaten()
        {
            Assert.That(PersonalRecords.Apply(Win(rival:4)),Does.Contain("Mayor diferencia: 3 puntos"));
            Assert.That(PersonalRecords.Apply(Win(rival:4)).Any(l=>l.StartsWith("Mayor diferencia")),Is.False,"a tie is not a record");
            Assert.That(PersonalRecords.Apply(Win(rival:6)).Any(l=>l.StartsWith("Mayor diferencia")),Is.False);
            Assert.That(PersonalRecords.Apply(Win(rival:6-5)),Does.Contain("Mayor diferencia: 6 puntos"));
            Assert.That(PersonalRecords.BestMargin,Is.EqualTo(6));
        }
        [Test] public void FastestWin_IsOnlyForWins_AndMustBeBeaten()
        {
            Assert.That(PersonalRecords.Apply(Loss(rally:1,seconds:10)).Any(l=>l.StartsWith("Victoria más rápida")),Is.False,"a loss is never a fast win");
            Assert.That(PersonalRecords.Apply(Win(seconds:300)),Does.Contain("Victoria más rápida: 5:00"));
            Assert.That(PersonalRecords.Apply(Win(seconds:400)).Any(l=>l.StartsWith("Victoria más rápida")),Is.False);
            Assert.That(PersonalRecords.Apply(Win(seconds:251)),Does.Contain("Victoria más rápida: 4:11"));
            Assert.That(PersonalRecords.FastestWin,Is.EqualTo(251f));
        }
        [Test] public void SeveralRecords_ShareOneLine_InOrder()
        {
            PersonalRecords.Apply(Win(rival:6,rally:3,seconds:500));
            var broken=PersonalRecords.Apply(Win(rival:2,rally:23,seconds:200));
            Assert.That(PersonalRecords.CardLine(broken),Is.EqualTo("¡Nuevo récord! Rally más largo: 23 golpes · Mejor racha: 2 victorias · Mayor diferencia: 5 puntos · Victoria más rápida: 3:20"));
            Assert.That(PersonalRecords.CardLine(new System.Collections.Generic.List<string>()),Is.EqualTo(""),"nothing broken, no line");
        }
        [Test] public void SwitchedOff_WritesNothing_AndReportsNothing()
        {
            PersonalRecords.Enabled=false;
            Assert.That(PersonalRecords.Apply(Win(rally:30)),Is.Empty);
            foreach(var key in PersonalRecords.AllKeys())Assert.That(PlayerPrefs.HasKey(key),Is.False,key);
        }
        [Test] public void Clear_RemovesEveryKey_UnderTheRecordsPrefix()
        {
            PersonalRecords.Apply(Win(rally:6));PersonalRecords.Apply(Win(rally:7));
            Assert.That(PersonalRecords.AllKeys().Any(PlayerPrefs.HasKey),Is.True);
            PersonalRecords.Clear();
            foreach(var key in PersonalRecords.AllKeys()){Assert.That(key,Does.StartWith("sc.records."));Assert.That(PlayerPrefs.HasKey(key),Is.False,key);}
        }
    }
    // The card line, the menu screen, the DEV switches and the matches that must not count.
    public sealed class ZRecordsFlowTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup(){PersonalRecords.Enabled=true;return Shutdown();}
        private static bool AnyKey()=>PersonalRecords.AllKeys().Any(PlayerPrefs.HasKey);
        // A passive human loses its match quickly: enough to reach the result card through the real flow.
        private IEnumerator MenuMatchToTheEnd()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);rally.Seed=9;
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True,"the match must finish");
            Refresh();hud.Tick(RallyHud.ResultInSeconds);yield return null;
        }
        [UnityTest] public IEnumerator MenuMatch_ShowsOneLineWhenARecordFalls_AndWritesIt()
        {
            yield return MenuMatchToTheEnd();
            var line=Find<Label>("match-result-record");
            Assert.That(PersonalRecords.LongestRally,Is.EqualTo(hud.LongestRally),"the longest rally of the match is stored");
            Assert.That(Shown(line),Is.True);Assert.That(line.text,Does.StartWith("¡Nuevo récord! Rally más largo: "));
            var card=Find("match-result-card").worldBound;var b=line.worldBound;
            Assert.That(b.yMin,Is.GreaterThanOrEqualTo(card.yMin));Assert.That(b.yMax,Is.LessThanOrEqualTo(card.yMax),"inside the card");
            Assert.That(PersonalRecords.Streak,Is.EqualTo(rally.Match.Winner==PlayerId.Human?1:0));
        }
        [UnityTest] public IEnumerator NoRecordBroken_HidesTheLine()
        {
            PlayerPrefs.SetInt(PersonalRecords.LongestRallyKey,500);PlayerPrefs.SetInt(PersonalRecords.StreakKey,0);
            yield return MenuMatchToTheEnd();
            Assert.That(Shown(Find("match-result-record")),Is.False);Assert.That(PersonalRecords.LongestRally,Is.EqualTo(500));
        }
        [UnityTest] public IEnumerator Autopilot_NeverWritesRecords()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");Refresh();yield return null;
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);Refresh();hud.Tick(RallyHud.ResultInSeconds);yield return null;
            Assert.That(AnyKey(),Is.False,"a demo match is nobody's record");Assert.That(Shown(Find("match-result-record")),Is.False);
        }
        [UnityTest] public IEnumerator AMatchStartedOutsideTheMenu_NeverWritesRecords()
        {
            rally.Seed=9;Assert.That(rally.StartMatch(),Is.True);app.Clock.Paused=false;
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);Refresh();yield return null;
            Assert.That(AnyKey(),Is.False,"the dev Partido button and practice are not menu matches");
        }
        [UnityTest] public IEnumerator AnAbandonedMatch_NeverWritesRecords()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);rally.Seed=9;
            Click("menu-play");Refresh();yield return null;
            Assert.That(Drive(()=>rally.PointsPlayed>=2,300),Is.True);
            Assert.That(rally.Phase,Is.Not.EqualTo(RallyPhase.MatchOver),"still in the match");
            flow.OpenMenu();Refresh();yield return null;
            Assert.That(AnyKey(),Is.False,"leaving mid-match writes nothing");
        }
        [UnityTest] public IEnumerator SwitchedOff_TheCardIsAsItWas()
        {
            PersonalRecords.Enabled=false;
            yield return MenuMatchToTheEnd();
            Assert.That(AnyKey(),Is.False);Assert.That(Shown(Find("match-result-record")),Is.False);
        }
        [UnityTest] public IEnumerator Menu_HasARecordsScreen_ThatListsTheBests_AndVolverComesBack()
        {
            var open=Find<Button>("menu-records");Assert.That(open.text,Is.EqualTo("Récords"));
            Click("menu-records");Refresh();yield return null;
            Assert.That(flow.Screen.ToString(),Is.EqualTo("Records"));
            Assert.That(Shown(Find("records-layer")),Is.True);Assert.That(Shown(Find("menu-layer")),Is.False);
            Assert.That(app.Clock.Paused,Is.True);Assert.That(rally.Active,Is.False);
            Assert.That(Find<Label>("records-title").text,Is.EqualTo("Tus récords"));
            foreach(var name in new[]{"records-rally","records-streak","records-wins","records-margin","records-fastest"})
                Assert.That(Find<Label>(name+"-value").text,Is.EqualTo("—"),name+" is empty before any match");
            Click("records-back");Refresh();yield return null;
            Assert.That(flow.Screen.ToString(),Is.EqualTo("Menu"));
            PlayerPrefs.SetInt(PersonalRecords.LongestRallyKey,23);PlayerPrefs.SetInt(PersonalRecords.BestStreakKey,5);PlayerPrefs.SetInt(PersonalRecords.StreakKey,2);
            PlayerPrefs.SetInt(PersonalRecords.WinsKeyPrefix+"easy",3);PlayerPrefs.SetInt(PersonalRecords.WinsKeyPrefix+"medium",2);
            PlayerPrefs.SetInt(PersonalRecords.BestMarginKey,6);PlayerPrefs.SetFloat(PersonalRecords.FastestWinKey,252f);
            Click("menu-records");Refresh();yield return null;
            Assert.That(Find<Label>("records-rally-value").text,Is.EqualTo("23 golpes"));
            Assert.That(Find<Label>("records-streak-value").text,Is.EqualTo("Mejor 5 · Actual 2"));
            Assert.That(Find<Label>("records-wins-value").text,Is.EqualTo("Fácil 3 · Media 2 · Difícil 0"));
            Assert.That(Find<Label>("records-margin-value").text,Is.EqualTo("6 puntos"));
            Assert.That(Find<Label>("records-fastest-value").text,Is.EqualTo("4:12"));
            foreach(var b in ShownUnder("records-layer").OfType<Button>()){Assert.That(b.worldBound.height,Is.GreaterThanOrEqualTo(44),b.name);Assert.That(b.worldBound.width,Is.GreaterThanOrEqualTo(44),b.name);}
        }
        [UnityTest] public IEnumerator DevPanel_RecordsSwitch_FlipsIt_AndBorrarClearsTheKeys()
        {
            for(int i=0;i<4;i++){Refresh();yield return null;}
            var toggle=root.Q<Button>("records-toggle");Assert.That(toggle,Is.Not.Null,"DEV panel has the records switch");
            Assert.That(PersonalRecords.Enabled,Is.True);Assert.That(toggle.text,Does.Contain("sí"));
            Click("records-toggle");Assert.That(PersonalRecords.Enabled,Is.False);Assert.That(toggle.text,Does.Contain("no"));
            Click("records-toggle");Assert.That(PersonalRecords.Enabled,Is.True);
            var clear=root.Q<Button>("records-clear");Assert.That(clear,Is.Not.Null);Assert.That(clear.text,Is.EqualTo("Borrar récords"));
            PersonalRecords.Apply(new MatchSummary(true,AiLevel.Hard,7,2,12,200));Assert.That(AnyKey(),Is.True);
            Click("records-clear");Assert.That(AnyKey(),Is.False);
        }
        [UnityTest] public IEnumerator TheFixtureLeavesNoRecordKeysBehind()
        {
            PlayerPrefs.SetInt(PersonalRecords.LongestRallyKey,9);PlayerPrefs.SetInt(PersonalRecords.WinsKeyPrefix+"hard",2);
            yield return Shutdown();
            Assert.That(AnyKey(),Is.False,"ClearPrefs removes sc.records.*");
            yield return Boot(true);
        }
    }
}
