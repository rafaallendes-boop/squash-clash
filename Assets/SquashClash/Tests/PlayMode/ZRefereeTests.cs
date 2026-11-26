using System.Collections;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Simple referee (spec 2026-11-26): the call between points of a match, read from the real HUD.
    public sealed class ZRefereeTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup(){RallyHud.RefereeOn=true;return Shutdown();}
        private IEnumerator StartMatch(int seed)
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=seed;
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
        }
        private IEnumerator Settle(){Refresh();yield return null;yield return null;Refresh();yield return null;}
        private bool InPlay=>rally.Phase==RallyPhase.HumanReception||rally.Phase==RallyPhase.RivalReception;
        // Plays every point of a match and checks the card at each pause, while playing and at the end. Counts what it saw.
        private IEnumerator PlayAndCheck(int[] counts)
        {
            int lastChecked=-1;
            while(true)
            {
                Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver||rally.Phase==RallyPhase.MatchOver,600),Is.True,"a point must end");
                yield return Settle();
                if(rally.Phase==RallyPhase.MatchOver)
                {
                    Assert.That(Shown(Find("rally-referee-card")),Is.False,"hidden at the end of the match");Assert.That(hud.RefereeShown,Is.EqualTo(""));break;
                }
                Assert.That(rally.PointsPlayed,Is.Not.EqualTo(lastChecked),"a new pause");lastChecked=rally.PointsPlayed;
                var last=rally.LastPoint.Value;var call=RefereeCall.Call(rally.Match,last.TinTouched);
                Assert.That(Shown(Find("rally-referee-card")),Is.True,"the card is shown between points");
                Assert.That(hud.RefereeShown,Is.EqualTo(call));
                Assert.That(Shown(Find("rally-referee-tin")),Is.EqualTo(last.TinTouched),"tin part");
                Assert.That(Shown(Find("rally-referee-matchball")),Is.EqualTo(rally.Match.MatchBall),"match ball part");
                Assert.That(Find<Label>("rally-referee-score").text,Is.EqualTo(RefereeCall.Score(rally.Match)));
                counts[0]++;if(last.TinTouched)counts[1]++;if(rally.Match.MatchBall)counts[2]++;if(last.TinTouched&&rally.Match.MatchBall)counts[3]++;
                Assert.That(Drive(()=>InPlay||rally.Phase==RallyPhase.MatchOver,60),Is.True,"play resumes");
                yield return Settle();
                if(InPlay){Assert.That(Shown(Find("rally-referee-card")),Is.False,"hidden while playing");Assert.That(hud.RefereeShown,Is.EqualTo(""));}
            }
        }
        // Fixed seed 9 (autopilot, sigma 30 ms, Medium), more matches by rematch until a match ball has been seen (the first match has one).
        // A tin point is NOT required (spec revision 2): the autopilot aims with the solver and the rival rarely errs, so no tin ever came up.
        // Searched without a tin: the 6 seed-9 matches of the first version of this test, plus 68 other matches (18 over level x sigma x
        // seed; 32 over autopilot line x power x new serve x sigma; 18 over rival style x level x seed): 0 tin points in all of them.
        // The tin part of the rule is still checked at every pause (shown iff LastPoint.TinTouched); the tin cause itself is covered by EditMode.
        [UnityTest] public IEnumerator AutopilotMatch_ShowsTheCallAtEveryPause_AndOnlyThere()
        {
            yield return StartMatch(9);var counts=new int[4];
            for(int match=0;match<6&&counts[2]<1;match++)
            {
                yield return PlayAndCheck(counts);
                if(counts[2]<1){rally.Rematch();yield return Settle();}
            }
            Debug.Log("[Referee] pauses="+counts[0]+" tin="+counts[1]+" matchBall="+counts[2]+" both="+counts[3]);
            Assert.That(counts[0],Is.GreaterThan(0));Assert.That(counts[2],Is.GreaterThanOrEqualTo(1),"at least one match ball");
        }
        [UnityTest] public IEnumerator TheDevSwitch_HidesTheCard()
        {
            yield return StartMatch(9);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,600),Is.True);yield return Settle();
            Assert.That(Shown(Find("rally-referee-card")),Is.True);
            RallyHud.RefereeOn=false;yield return Settle();
            Assert.That(Shown(Find("rally-referee-card")),Is.False);Assert.That(hud.RefereeShown,Is.EqualTo(""));
            RallyHud.RefereeOn=true;yield return Settle();
            Assert.That(Shown(Find("rally-referee-card")),Is.True);Assert.That(hud.RefereeShown,Is.Not.EqualTo(""));
        }
    }
    // Simple referee, layout: the longest call fits on every phone, beside the banner and beside the serve guide, without touching either.
    public sealed class ZRefereeLayoutTests : ZDeviceFixture
    {
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private IEnumerator Settle(){Refresh();yield return null;yield return null;Refresh();yield return null;}
        private static bool Apart(Rect a,Rect b)=>a.xMax<=b.xMin+.5f||b.xMax<=a.xMin+.5f||a.yMax<=b.yMin+.5f||b.yMax<=a.yMin+.5f;
        private IEnumerator ForceLongest()
        {
            Find("rally-referee-tin").style.display=DisplayStyle.Flex;Find("rally-referee-matchball").style.display=DisplayStyle.Flex;
            Find<Label>("rally-referee-score").text="99–98";yield return Settle();
        }
        private void AssertClear(string context,params string[] others)
        {
            var card=Find("rally-referee-card");Assert.That(Shown(card),Is.True,context+": the card is shown");
            Debug.Log("[Referee] "+context+" card x="+card.worldBound.x.ToString("F1")+" y="+card.worldBound.y.ToString("F1")+" width="+card.worldBound.width.ToString("F1")+" height="+card.worldBound.height.ToString("F1"));
            Assert.That(card.worldBound.height,Is.LessThanOrEqualTo(22.5f));
            foreach(var name in new[]{"boot-header"}.Concat(others))
            {
                var other=Find(name);if(!Shown(other))continue;
                Assert.That(Apart(card.worldBound,other.worldBound),Is.True,context+": the card overlaps "+name+" "+card.worldBound+" / "+other.worldBound);
            }
        }
        [UnityTest] public IEnumerator TheLongestCall_FitsBesideTheBanner([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,false);
            app.Tuning.Set("fatigue.enabled",0);app.Clock.Paused=false;rally.SetNewServe(false);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Easy;rally.Seed=3;
            Assert.That(rally.StartMatch(),Is.True);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver&&rally.Match.Server==PlayerId.Rival,900),Is.True,"a point the rival serves after");
            hud.Tick(RallyHud.BannerInSeconds);yield return Settle();yield return ForceLongest();
            Assert.That(Shown(Find("rally-banner-card")),Is.True,"the banner is shown");
            AssertFits("rally-referee",phone+" with the banner");
            AssertClear(phone+" with the banner","rally-banner-card");
        }
        [UnityTest] public IEnumerator TheLongestCall_FitsBesideTheServeGuide([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,false);
            app.Tuning.Set("fatigue.enabled",0);app.Clock.Paused=false;rally.SetNewServe(true);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Easy;rally.Seed=3;
            Assert.That(rally.StartMatch(),Is.True);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver&&rally.Match.Server==PlayerId.Human,900),Is.True);
            Assert.That(Drive(()=>root.Q("serve-guide")?.style.opacity.value>=1,.25),Is.True);yield return Settle();yield return ForceLongest();
            Assert.That(Shown(Find("serve-guide-lob")),Is.True,"the serve guide is shown");
            AssertFits("rally-referee",phone+" with the serve guide");
            AssertClear(phone+" with the serve guide","serve-guide-lob","serve-guide-drive","serve-guide-hint","rally-banner-card");
        }
    }
}
