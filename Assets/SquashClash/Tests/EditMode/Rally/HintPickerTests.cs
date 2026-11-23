using NUnit.Framework;
using SquashClash.Core.Rally;
namespace SquashClash.Core.Tests.Rally
{
    // First-match hints (spec front 2, 2026-11-22), the state machine of section 1.3 without Unity. Each test names the behaviour it pins.
    public sealed class HintPickerTests
    {
        private static HintPicker Fresh(){var p=new HintPicker(0,0,0,0,false);Assert.That(p.BeginMatch(),Is.True);return p;}
        private static void Seen(HintPicker p,HintId id,HintWhere where){Assert.That(p.MarkSeen(id,where),Is.True,"seen "+id);}
        [Test] public void ThereAreFourHints_AndEachStartsUnseen()
        {
            Assert.That(HintPicker.Count,Is.EqualTo(4));Assert.That(System.Enum.GetValues(typeof(HintId)).Length,Is.EqualTo(4));
            var p=Fresh();foreach(HintId id in System.Enum.GetValues(typeof(HintId)))Assert.That(p.State(id),Is.EqualTo(HintState.Unseen));
        }
        [Test] public void StoredValues_AreRestoredAsSeenAndRetired_AndAClosedWindowDoesNotBeginAMatch()
        {
            var p=new HintPicker(1,0,2,0,false);
            Assert.That(p.State(HintId.Serve),Is.EqualTo(HintState.Seen));Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Retired));
            Assert.That(p.Stored(HintId.Serve),Is.EqualTo(1));Assert.That(p.Stored(HintId.Boast),Is.EqualTo(2));Assert.That(p.Stored(HintId.Volley),Is.EqualTo(0));
            Assert.That(p.BeginMatch(),Is.True);
            var closed=new HintPicker(0,0,0,0,true);Assert.That(closed.WindowClosed,Is.True);Assert.That(closed.BeginMatch(),Is.False);Assert.That(closed.InMatch,Is.False);
        }
        [Test] public void AllFourSeenOrRetired_CloseTheWindow_AndNoMatchBegins()
        {
            var p=new HintPicker(1,1,2,1,false);Assert.That(p.AllDone,Is.True);Assert.That(p.BeginMatch(),Is.False);
        }
        // Triggers (section 1.2)
        [Test] public void Volley_ArmsWhenTheAmberRingShowedAndNoVolleyWasApplied_NotOtherwise()
        {
            var p=Fresh();p.OnReceptionEnded(false,false);Assert.That(p.State(HintId.Volley),Is.EqualTo(HintState.Unseen),"ring never shown");
            p.OnReceptionEnded(true,true);Assert.That(p.State(HintId.Volley),Is.EqualTo(HintState.Unseen),"the volley was applied");
            p.OnReceptionEnded(true,false);Assert.That(p.State(HintId.Volley),Is.EqualTo(HintState.Armed));
        }
        [Test] public void FrontWallStrokeWithSideAimOpen_ArmsBoast_ButNotWhenNotApplied_OrAfterATwoWallShot()
        {
            var p=Fresh();p.OnStroke(false,true,false);Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Unseen),"not applied");
            p.OnStroke(true,false,false);Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Unseen),"side aim closed");
            p.OnStroke(true,true,false);Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Armed));
            var q=Fresh();q.OnStroke(true,true,true);q.OnStroke(true,true,false);
            Assert.That(q.State(HintId.Boast),Is.EqualTo(HintState.Retired),"a two-wall shot already happened: no hint to arm again");
        }
        [Test] public void FirstTwoWallShot_ArmsTheCross_AndAFallbackDoesNotCountAsOne()
        {
            var p=Fresh();p.OnStroke(true,true,false);// a fallback reaches the picker as a front-wall stroke
            Assert.That(p.State(HintId.BoastCross),Is.EqualTo(HintState.Unseen));Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Armed));
            p.OnStroke(false,true,true);Assert.That(p.State(HintId.BoastCross),Is.EqualTo(HintState.Unseen),"not applied");
            p.OnStroke(true,true,true);Assert.That(p.State(HintId.BoastCross),Is.EqualTo(HintState.Armed));
        }
        [Test] public void TheCross_ArmsOnlyOnTheFirstTwoWallShotOfTheMatch()
        {
            var p=new HintPicker(0,0,0,1,false);p.BeginMatch();p.OnStroke(true,true,true);Assert.That(p.State(HintId.BoastCross),Is.EqualTo(HintState.Seen),"already seen stays seen");
            var q=Fresh();q.OnStroke(true,true,true);Seen(q,HintId.BoastCross,HintWhere.Banner);q.OnStroke(true,true,true);Assert.That(q.State(HintId.BoastCross),Is.EqualTo(HintState.Seen));
        }
        [Test] public void RetireBoast_NeedsNoVisibility_AndWorksFromUnseenOrArmed()
        {
            var p=Fresh();p.OnStroke(true,true,true);
            Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Retired));Assert.That(p.Stored(HintId.Boast),Is.EqualTo(2));
            var q=Fresh();q.OnStroke(true,true,false);Assert.That(q.State(HintId.Boast),Is.EqualTo(HintState.Armed));q.OnStroke(true,true,true);
            Assert.That(q.State(HintId.Boast),Is.EqualTo(HintState.Retired));Assert.That(q.State(HintId.BoastCross),Is.EqualTo(HintState.Armed),"the same shot arms the cross");
        }
        [Test] public void ASeenBoastIsNotRetired()
        {
            var p=Fresh();p.OnStroke(true,true,false);Seen(p,HintId.Boast,HintWhere.Banner);p.OnStroke(true,true,true);
            Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Seen));Assert.That(p.Stored(HintId.Boast),Is.EqualTo(1));
        }
        [Test] public void Serve_ArmsInTheHumanServePause_AndShowsAlone()
        {
            var p=Fresh();p.OnReceptionEnded(true,false);p.OnStroke(true,true,false);
            Assert.That(p.Choose(HintPause.HumanServe,false),Is.EqualTo(HintId.Serve),"only H0 in the guide even with H1 and H2 armed");
            Assert.That(p.State(HintId.Volley),Is.EqualTo(HintState.Armed));Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Armed));
            Assert.That(p.State(HintId.Serve),Is.EqualTo(HintState.Armed));
        }
        [Test] public void ServeInTheBanner_ArmsBeforeTheRivalsFirstServe_NotAfterItServed_NotWhenTheHumanServes()
        {
            var p=Fresh();Assert.That(p.Choose(HintPause.Ordinary,false),Is.Null,"the human serves next: no H0 in the banner");
            Assert.That(p.Choose(HintPause.Ordinary,true),Is.EqualTo(HintId.Serve));
            var q=Fresh();q.RivalServed();Assert.That(q.Choose(HintPause.Ordinary,true),Is.Null,"the rival already served in this match");
        }
        // Priority, one per pause, carry over, discard
        [Test] public void OrdinaryPause_ShowsOneHint_InTheOrderServeVolleyBoastCross()
        {
            var p=Fresh();p.OnStroke(true,true,true);p.OnReceptionEnded(true,false);p.OnStroke(true,true,false);// volley, cross armed; boast retired by the two-wall shot
            Assert.That(p.Choose(HintPause.Ordinary,false),Is.EqualTo(HintId.Volley));
            Seen(p,HintId.Volley,HintWhere.Banner);p.EndPause();
            Assert.That(p.Choose(HintPause.Ordinary,false),Is.EqualTo(HintId.BoastCross),"the next ordinary pause of the same match");
            var q=Fresh();q.OnReceptionEnded(true,false);q.OnStroke(true,true,false);
            Assert.That(q.Choose(HintPause.Ordinary,true),Is.EqualTo(HintId.Serve),"H0 in the banner goes first");
            q.EndPause();q.RivalServed();Assert.That(q.State(HintId.Serve),Is.EqualTo(HintState.Unseen));
            Assert.That(q.Choose(HintPause.Ordinary,false),Is.EqualTo(HintId.Volley),"H1 waited for the next ordinary pause");
            Seen(q,HintId.Volley,HintWhere.Banner);q.EndPause();Assert.That(q.Choose(HintPause.Ordinary,false),Is.EqualTo(HintId.Boast));
        }
        [Test] public void HumanServePause_LeavesTheOtherHintsArmed_AndTheyCannotBeSeenThere()
        {
            var p=Fresh();p.OnReceptionEnded(true,false);Assert.That(p.Choose(HintPause.HumanServe,false),Is.EqualTo(HintId.Serve));
            Assert.That(p.MarkSeen(HintId.Volley,HintWhere.Guide),Is.False,"the guide only holds H0");
            Assert.That(p.State(HintId.Volley),Is.EqualTo(HintState.Armed));
            p.EndPause();Assert.That(p.Choose(HintPause.Ordinary,false),Is.EqualTo(HintId.Volley));
        }
        [Test] public void MatchOver_ShowsNothing()
        {
            var p=Fresh();p.OnReceptionEnded(true,false);Assert.That(p.Choose(HintPause.MatchOver,false),Is.Null);
        }
        [Test] public void AnUnseenHintAtTheEndOfAPause_StaysArmedOnlyAsLongAsItsRuleSays()
        {
            var p=Fresh();Assert.That(p.Choose(HintPause.HumanServe,false),Is.EqualTo(HintId.Serve));p.EndPause();
            Assert.That(p.State(HintId.Serve),Is.EqualTo(HintState.Unseen),"H0 in the guide lives until the end of its pause");
            Assert.That(p.Choose(HintPause.HumanServe,false),Is.EqualTo(HintId.Serve),"and arms again in the next HumanServePause");
            var q=Fresh();Assert.That(q.Choose(HintPause.Ordinary,true),Is.EqualTo(HintId.Serve));q.EndPause();q.RivalServed();
            Assert.That(q.Choose(HintPause.Ordinary,true),Is.Null,"the banner trigger happens once per match");
            Assert.That(q.Choose(HintPause.HumanServe,false),Is.EqualTo(HintId.Serve),"but the guide can still show it");
        }
        // One key for both forms of H0
        [Test] public void ServeHasTwoFormsAndOneKey_TheFirstToBeSeenConsumesBoth()
        {
            var p=Fresh();Assert.That(p.Choose(HintPause.Ordinary,true),Is.EqualTo(HintId.Serve));
            Assert.That(p.MarkSeen(HintId.Serve,HintWhere.Guide),Is.False,"armed as the banner form: the guide cannot mark it");
            Seen(p,HintId.Serve,HintWhere.Banner);Assert.That(p.SeenWhere(HintId.Serve),Is.EqualTo(HintWhere.Banner));Assert.That(p.Stored(HintId.Serve),Is.EqualTo(1));
            p.EndPause();Assert.That(p.Choose(HintPause.HumanServe,false),Is.Null,"never in the guide after the banner");
            var q=Fresh();Assert.That(q.Choose(HintPause.HumanServe,false),Is.EqualTo(HintId.Serve));Seen(q,HintId.Serve,HintWhere.Guide);q.EndPause();q.RivalServed();
            Assert.That(q.Choose(HintPause.Ordinary,true),Is.Null,"never in the banner after the guide");Assert.That(q.SeenWhere(HintId.Serve),Is.EqualTo(HintWhere.Guide));
        }
        // Seen, window, discard
        [Test] public void OnlyAnArmedHintCanBeSeen()
        {
            var p=Fresh();Assert.That(p.MarkSeen(HintId.Volley,HintWhere.Banner),Is.False,"unseen is not armed");Assert.That(p.Stored(HintId.Volley),Is.EqualTo(0));
            p.OnReceptionEnded(true,false);Seen(p,HintId.Volley,HintWhere.Banner);Assert.That(p.MarkSeen(HintId.Volley,HintWhere.Banner),Is.False,"a seen hint is seen once");
        }
        [Test] public void ASeenOrRetiredHint_NeverComesBack()
        {
            var p=Fresh();p.OnReceptionEnded(true,false);Seen(p,HintId.Volley,HintWhere.Banner);p.OnReceptionEnded(true,false);
            Assert.That(p.State(HintId.Volley),Is.EqualTo(HintState.Seen));Assert.That(p.Choose(HintPause.Ordinary,false),Is.Null);
        }
        [Test] public void EndingAMatchDiscardsEveryArmedHint_AndWithoutRepeatingTheTriggerNothingReturns()
        {
            var p=Fresh();p.OnReceptionEnded(true,false);p.OnStroke(true,true,false);p.EndMatch(false);
            Assert.That(p.State(HintId.Volley),Is.EqualTo(HintState.Unseen));Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Unseen));
            Assert.That(p.WindowClosed,Is.False,"abandoning does not close the window");Assert.That(p.InMatch,Is.False);
            // The next match: restored from the stored values, nothing armed.
            var q=new HintPicker(p.Stored(HintId.Serve),p.Stored(HintId.Volley),p.Stored(HintId.Boast),p.Stored(HintId.BoastCross),p.WindowClosed);q.BeginMatch();
            Assert.That(q.Choose(HintPause.Ordinary,false),Is.Null,"the trigger did not happen again");
            q.OnReceptionEnded(true,false);Assert.That(q.Choose(HintPause.Ordinary,false),Is.EqualTo(HintId.Volley),"it arms when the trigger repeats");
        }
        [Test] public void ANormalMatchEnd_ClosesTheWindow_AnAbandonedOneDoesNot()
        {
            var p=Fresh();p.EndMatch(true);Assert.That(p.WindowClosed,Is.True);Assert.That(p.BeginMatch(),Is.False);
            var q=Fresh();q.EndMatch(false);Assert.That(q.WindowClosed,Is.False);Assert.That(q.BeginMatch(),Is.True);
        }
        [Test] public void TheLastHintSeenOrRetired_ClosesTheWindow()
        {
            var p=new HintPicker(1,1,0,1,false);p.BeginMatch();p.OnStroke(true,true,true);
            Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Retired));Assert.That(p.AllDone,Is.True);Assert.That(p.WindowClosed,Is.True);
        }
        [Test] public void NothingHappensOutsideAMatch()
        {
            var p=new HintPicker(0,0,0,0,false);
            p.OnReceptionEnded(true,false);p.OnStroke(true,true,true);
            Assert.That(p.State(HintId.Volley),Is.EqualTo(HintState.Unseen));Assert.That(p.State(HintId.Boast),Is.EqualTo(HintState.Unseen));
            Assert.That(p.Choose(HintPause.HumanServe,false),Is.Null);Assert.That(p.MarkSeen(HintId.Serve,HintWhere.Guide),Is.False);
        }
        // The two oracles of "seen"
        [Test] public void BannerOracle_NeedsFullOpacity_AVisibleBannerAndRow_AndALaidOutRow()
        {
            Assert.That(HintOracle.BannerSeen(1f,true,true,20f),Is.True);
            Assert.That(HintOracle.BannerSeen(.99f,true,true,20f),Is.False,"opacity below 1");
            Assert.That(HintOracle.BannerSeen(1f,false,true,20f),Is.False,"banner hidden");
            Assert.That(HintOracle.BannerSeen(1f,true,false,20f),Is.False,"row hidden");
            Assert.That(HintOracle.BannerSeen(1f,true,true,0f),Is.False,"no layout yet");
            Assert.That(HintOracle.BannerSeen(1f,true,true,float.NaN),Is.False,"unresolved layout");
        }
        [Test] public void GuideOracle_SumsRealTimeBetweenConsecutiveVisibleFrames()
        {
            var c=new GuideSeenClock();c.Frame(100,true,false);Assert.That(c.Reached(.18),Is.False,"the first visible frame adds nothing");
            c.Frame(100.1,true,false);Assert.That(c.Seconds,Is.EqualTo(.1).Within(1e-9));Assert.That(c.Reached(.18),Is.False);
            c.Frame(100.18,true,false);Assert.That(c.Reached(.18),Is.True,"0.1 + 0.08, with the tolerance of a double sum");
        }
        [Test] public void GuideOracle_AWholeLongFrameCountsAndASingleFrameOfPointOneEightIsEnough()
        {
            var c=new GuideSeenClock();c.Frame(50,true,false);c.Frame(50.18,true,false);Assert.That(c.Reached(.18),Is.True);
        }
        [Test] public void GuideOracle_HiddenFramesAndGamePauseCountNothing_AndTheGapIsNotAdded()
        {
            var c=new GuideSeenClock();c.Frame(10,true,false);c.Frame(10.05,true,false);
            c.Frame(10.5,false,false);c.Frame(11,true,false);Assert.That(c.Seconds,Is.EqualTo(.05).Within(1e-9),"a hidden frame breaks the run");
            c.Frame(11.1,true,true);c.Frame(12,true,true);c.Frame(12.05,true,false);Assert.That(c.Seconds,Is.EqualTo(.05).Within(1e-9),"paused game time is not visible time");
            c.Frame(12.1,true,false);Assert.That(c.Seconds,Is.EqualTo(.1).Within(1e-9));
        }
        [Test] public void GuideOracle_ResetStartsOver()
        {
            var c=new GuideSeenClock();c.Frame(1,true,false);c.Frame(2,true,false);Assert.That(c.Reached(.18),Is.True);c.Reset();Assert.That(c.Seconds,Is.Zero);Assert.That(c.Reached(.18),Is.False);
            c.Frame(5,true,false);Assert.That(c.Seconds,Is.Zero,"after a reset the first frame adds nothing");
        }
    }
}
