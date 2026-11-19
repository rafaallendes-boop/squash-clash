using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Rival styles in the real game (design 2026-11-19): one style per match, random from the start menu, fixed by the DEV
    // override, shown under «RIVAL» and at the start of the match, written to the metrics; the rival volleys and recovers by style.
    public sealed class ZRivalStyleTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        [Serializable] private sealed class Line {public string kind,payload;}
        [Serializable] private sealed class PointLine {public string style,level;}
        private void StepOnce(){app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();}
        private bool Until(Func<bool> done,double seconds){int limit=(int)(seconds/StepReal);for(int i=0;i<limit&&!done();i++)StepOnce();return done();}
        [UnityTest] public IEnumerator MenuMatch_PicksAStyle_ShowsIt_AndTheMetricsRecordIt()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.StyleOverride=RivalStyleId.Definidor;
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null);Assert.That(rally.Style,Is.EqualTo(RivalStyleId.Definidor));
            Refresh();
            Assert.That(Find<Label>("rally-rival-style").text,Is.EqualTo("Definidor"));Assert.That(Shown(Find("rally-rival-style")),Is.True,"the style is on the scoreboard");
            Assert.That(Find<Label>("rally-status").text,Does.StartWith("Rival: Definidor"),"announced at the start of the match");
            rally.Autopilot=true;
            Assert.That(Until(()=>rally.PointsPlayed>0,120),Is.True,"a point ends");
            var line=File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Line>).Last(l=>l.kind=="rally-point");
            Assert.That(JsonUtility.FromJson<PointLine>(line.payload).style,Is.EqualTo("Definidor"));
            Refresh();Assert.That(Find<Label>("rally-status").text,Does.Not.StartWith("Rival:"),"only during the first point");
        }
        [UnityTest] public IEnumerator RandomStyles_VaryBetweenMatches_TheOverrideWins_AndTheDefaultIsFondista()
        {
            yield return null;
            Assert.That(rally.StartMatch(),Is.True);Assert.That(rally.Style,Is.EqualTo(RivalStyleId.Fondista),"without the menu the match is deterministic");
            rally.RandomStyles=true;var seen=new System.Collections.Generic.HashSet<RivalStyleId>();
            for(int i=0;i<15;i++){Assert.That(rally.StartMatch(),Is.True);seen.Add(rally.Style);}
            Assert.That(seen.Count,Is.GreaterThan(1),"random per match");
            rally.StyleOverride=RivalStyleId.Completo;Assert.That(rally.StartMatch(),Is.True);Assert.That(rally.Style,Is.EqualTo(RivalStyleId.Completo));
            rally.Rematch();Assert.That(rally.Style,Is.EqualTo(RivalStyleId.Completo),"a rematch keeps the rival");
        }
        [UnityTest] public IEnumerator Definidor_VolleysInTheRealGame()
        {
            yield return null;
            app.Clock.Paused=false;rally.Autopilot=true;rally.AutoRestart=true;rally.Seed=7;rally.StyleOverride=RivalStyleId.Definidor;
            // Strong cross drives reach the rival at volley height; the plain autopilot drive never does (as bot30 in RallySim).
            rally.AutopilotLine=ShotLine.Cross;rally.AutopilotPower=ShotPower.Strong;
            Assert.That(rally.StartMatch(),Is.True);
            Assert.That(Until(()=>rally.RivalVolleys>0,900),Is.True,"the Definidor takes a volley within fifteen minutes of autopilot play");
        }
    }
}
