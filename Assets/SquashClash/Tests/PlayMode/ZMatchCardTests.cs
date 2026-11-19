using System;
using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Result card, extended: match duration (m:ss), longest rally in strokes, and "Revancha 5–7" only after a loss.
    // View only: everything is read from what the HUD already sees (the phase, the points, PointResult.Shots).
    public sealed class ZMatchCardTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup(){RallyHud.ExtendedCardOn=true;return Shutdown();}
        private IEnumerator StartMatch()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
        }
        // Plays a whole match by hand, feeding the HUD the same real time the test counts: only while the match is not over.
        private bool Play(out float seconds,out int longest)
        {
            seconds=0;longest=0;int counted=0;float dt=(float)StepReal;
            for(int i=0;i<(int)(1500/StepReal);i++)
            {
                if(rally.Phase==RallyPhase.MatchOver)return true;
                app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();scape.Tick(dt);Refresh();
                if(rally.PointsPlayed!=counted){counted=rally.PointsPlayed;if(rally.LastPoint.HasValue)longest=Mathf.Max(longest,rally.LastPoint.Value.Shots);}
                if(rally.Phase!=RallyPhase.MatchOver){seconds+=dt;hud.Tick(dt);}
            }
            return rally.Phase==RallyPhase.MatchOver;
        }
        [Test] public void Formats_DurationAsMinutesAndSeconds_AndRematchOnlyAfterALoss()
        {
            Assert.That(RallyHud.DurationText(0f),Is.EqualTo("0:00"));Assert.That(RallyHud.DurationText(59.9f),Is.EqualTo("0:59"));
            Assert.That(RallyHud.DurationText(61f),Is.EqualTo("1:01"));Assert.That(RallyHud.DurationText(754f),Is.EqualTo("12:34"));
            Assert.That(RallyHud.RematchText(false,5,7),Is.EqualTo("Revancha 5–7"),"human score first, an en dash");
            Assert.That(RallyHud.RematchText(true,7,5),Is.EqualTo("Revancha"),"a win keeps the plain label");
            Assert.That(RallyHud.StrokesText(1),Is.EqualTo("1 golpe"));Assert.That(RallyHud.StrokesText(14),Is.EqualTo("14 golpes"));
        }
        [UnityTest] public IEnumerator Card_ShowsDurationAndLongestRally_AndRematchNamesTheScoreOnlyOnALoss()
        {
            yield return StartMatch();
            Assert.That(Play(out var seconds,out var longest),Is.True);Refresh();hud.Tick(RallyHud.ResultInSeconds);yield return null;
            Assert.That(seconds,Is.GreaterThan(30f),"a real match took some time");Assert.That(longest,Is.GreaterThan(0));
            var duration=Find<Label>("match-result-duration");var strokes=Find<Label>("match-result-longest");
            Assert.That(Shown(duration),Is.True);Assert.That(Shown(strokes),Is.True);
            Assert.That(duration.text,Is.EqualTo("Duración: "+(int)seconds/60+":"+((int)seconds%60).ToString("D2")));
            Assert.That(strokes.text,Is.EqualTo("Rally más largo: "+(longest==1?"1 golpe":longest+" golpes")));
            bool won=rally.Match.Winner==PlayerId.Human;
            Assert.That(Find<Button>("match-result-rematch").text,Is.EqualTo(won?"Revancha":"Revancha "+rally.Match.Human+"–"+rally.Match.Rival));
            var card=Find("match-result-card").worldBound;
            foreach(var name in new[]{"match-result-duration","match-result-longest","match-result-rematch"})
            {var b=Find(name).worldBound;Assert.That(b.yMin,Is.GreaterThanOrEqualTo(card.yMin));Assert.That(b.yMax,Is.LessThanOrEqualTo(card.yMax),name+" inside the card");}
        }
        [UnityTest] public IEnumerator Card_SwitchOff_IsExactlyTheCardOfToday()
        {
            RallyHud.ExtendedCardOn=false;
            yield return StartMatch();
            Assert.That(Play(out _,out _),Is.True);Refresh();hud.Tick(RallyHud.ResultInSeconds);yield return null;
            Assert.That(Shown(Find("match-result")),Is.True);
            Assert.That(Shown(Find("match-result-duration")),Is.False);Assert.That(Shown(Find("match-result-longest")),Is.False);
            Assert.That(Find<Button>("match-result-rematch").text,Is.EqualTo("Revancha"),"never the score");
        }
        [UnityTest] public IEnumerator Rematch_StartsTheClockAndTheLongestRallyOver()
        {
            yield return StartMatch();
            Assert.That(Play(out var first,out var firstLongest),Is.True);Refresh();
            Click("match-result-rematch");Refresh();yield return null;
            Assert.That(rally.Phase,Is.Not.EqualTo(RallyPhase.MatchOver));
            Assert.That(Play(out var second,out var secondLongest),Is.True);Refresh();hud.Tick(RallyHud.ResultInSeconds);yield return null;
            Assert.That(Find<Label>("match-result-duration").text,Is.EqualTo("Duración: "+(int)second/60+":"+((int)second%60).ToString("D2")),"the second match's own time, not both");
            Assert.That(Find<Label>("match-result-longest").text,Does.Contain(secondLongest==1?"1 golpe":secondLongest+" golpes"));
        }
        [UnityTest] public IEnumerator PausedTime_IsNotCounted()
        {
            yield return StartMatch();
            Assert.That(Drive(()=>rally.PointsPlayed>=1,300),Is.True);
            float start=hud.MatchSeconds;hud.Tick(1f);Assert.That(hud.MatchSeconds,Is.EqualTo(start+1f).Within(.001f),"counts while playing");
            app.Clock.Paused=true;float before=hud.MatchSeconds;
            for(int i=0;i<600;i++)hud.Tick(.05f);
            Assert.That(hud.MatchSeconds,Is.EqualTo(before),"30 s of pause did not add up");
            app.Clock.Paused=false;
        }
        [UnityTest] public IEnumerator DevPanel_CardToggle_DefaultsOn_AndFlipsTheSwitch()
        {
            for(int i=0;i<4;i++){Refresh();yield return null;}
            var button=root.Q<Button>("match-card-toggle");Assert.That(button,Is.Not.Null,"DEV panel has the card toggle");
            Assert.That(RallyHud.ExtendedCardOn,Is.True);Assert.That(button.text,Does.Contain("sí"));
            Click("match-card-toggle");Assert.That(RallyHud.ExtendedCardOn,Is.False);Assert.That(button.text,Does.Contain("no"));
            Click("match-card-toggle");Assert.That(RallyHud.ExtendedCardOn,Is.True);
        }
    }
}
