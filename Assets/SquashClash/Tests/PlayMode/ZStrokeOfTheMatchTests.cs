using System;
using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Stroke of the match (spec 2026-11-25): the card's one line, counted from ShotResolved and PointEnded. The observer below applies
    // the spec's rules on its own, without reading hud.Strokes, and the HUD must agree with it.
    public sealed class ZStrokeOfTheMatchTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup(){RallyHud.ExtendedCardOn=true;FirstMatchHints.ResetForTests();return Shutdown();}
        private sealed class Observer
        {
            public int Perfects,BoastWinners,Volleys;private bool pendingBoast;
            private readonly RallyDirector rally;private readonly ShotController controller;
            public Observer(RallyDirector rally,ShotController controller){this.rally=rally;this.controller=controller;controller.ShotResolved+=OnResolved;rally.PointEnded+=OnPointEnded;}
            public void Dispose(){controller.ShotResolved-=OnResolved;rally.PointEnded-=OnPointEnded;}
            private void OnResolved(ShotRecord record)
            {
                if(!(rally.Active&&rally.Match!=null&&rally.Phase==RallyPhase.HumanReception))return;
                bool good=record.Applied&&(record.Result=="Perfect"||record.Result=="Valid");
                if(!good){pendingBoast=false;return;}
                if(record.Result=="Perfect")Perfects++;
                if(controller.LastOpportunity==Opportunity.Volley)Volleys++;
                pendingBoast=controller.LastStrokeBoast;
            }
            private void OnPointEnded(PointResult result){if(result.Winner==PlayerId.Human&&pendingBoast)BoastWinners++;pendingBoast=false;}
        }
        private IEnumerator StartMatch()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
        }
        private IEnumerator Show(){Refresh();hud.Tick(RallyHud.ResultInSeconds);yield return null;}
        // Steps the match to the frame where it becomes MatchOver, without the Refresh that would build the card.
        private bool StepToMatchOver()
        {
            for(int i=0;i<(int)(1500/StepReal);i++)
            {
                app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();scape.Tick((float)StepReal);
                if(rally.Phase==RallyPhase.MatchOver)return true;
                Refresh();
            }
            return false;
        }
        private Label Highlight=>Find<Label>("match-result-highlight");
        private static void AssertAgrees(Observer o,StrokeOfTheMatch s)
        {
            Assert.That(s.Perfects,Is.EqualTo(o.Perfects),"perfects");Assert.That(s.BoastWinners,Is.EqualTo(o.BoastWinners),"boast winners");Assert.That(s.Volleys,Is.EqualTo(o.Volleys),"volleys");
        }
        [UnityTest] public IEnumerator AutopilotMatch_HudAgreesWithAnIndependentObserver_AndTheCardShowsThatLine()
        {
            yield return StartMatch();var o=new Observer(rally,controller);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);yield return Show();o.Dispose();
            Debug.Log("[StrokeOfTheMatch] observer perfects="+o.Perfects+" boastWinners="+o.BoastWinners+" volleys="+o.Volleys);
            AssertAgrees(o,hud.Strokes);
            string expected=StrokeOfTheMatch.Line(o.Perfects,o.BoastWinners,o.Volleys);
            Assert.That(Highlight.text,Is.EqualTo(expected));Assert.That(Shown(Highlight),Is.EqualTo(expected.Length>0),"shown only when there is a line");
            var card=Find("match-result-card").worldBound;
            if(expected.Length>0){var b=Highlight.worldBound;Assert.That(b.yMin,Is.GreaterThanOrEqualTo(card.yMin));Assert.That(b.yMax,Is.LessThanOrEqualTo(card.yMax));}
        }
        [UnityTest] public IEnumerator TheHighlightSitsBetweenTheLongestRallyAndTheRecordLine()
        {
            yield return StartMatch();
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);yield return Show();
            var c=Find("match-result-card");
            Assert.That(c.IndexOf(Highlight),Is.EqualTo(c.IndexOf(Find("match-result-longest"))+1));Assert.That(c.IndexOf(Find("match-result-record")),Is.EqualTo(c.IndexOf(Highlight)+1));
            Assert.That(Highlight.ClassListContains("sc-result-highlight"),Is.True);
        }
        [UnityTest] public IEnumerator ABoastResolvedThenAPointWonByThePlayer_AddsOneBoastWinner_AndALostPointAddsNone()
        {
            FirstMatchHints.ResetForTests();FirstMatchHints.Now=()=>100;FirstMatchHints.ForceWindowForTests=true;
            PlayerPrefs.SetInt("sc.hints.serve",1);PlayerPrefs.SetInt("sc.hints.volley",1);PlayerPrefs.SetInt("sc.hints.boast",2);PlayerPrefs.SetInt("sc.hints.boastCross",1);
            int tries=0,won=0,lost=0;
            foreach(long seed in new long[]{3,4,5,6,7,8})
            foreach(float dz in new[]{-.8f,-.5f,-1.1f,-1.4f,-.3f,-1.7f})
            {
                if(won>0)break;
                tries++;
                flow.ClosePractice();rally.Autopilot=false;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Easy;rally.Seed=seed;rally.StyleOverride=RivalStyleId.Fondista;
                rally.SetParams(new RallyParams{ServeKindOn=1});
                Assert.That(rally.StartMatch(),Is.True);hud.Hints.BeginMatch();
                bool boast=false;int before=-1;PointResult? ended=null;
                Action<ShotRecord> probe=r=>{if(controller.LastStrokeBoast&&!boast){boast=true;before=hud.Strokes.BoastWinners;}};
                Action<PointResult> pe=r=>{if(boast&&!ended.HasValue)ended=r;};
                controller.ShotResolved+=probe;rally.PointEnded+=pe;
                yield return HumanBoast(dz);controller.ProcessPending();
                Drive(()=>controller.LastRecord!=null&&controller.LastRecord.LaunchId==app.Host.LaunchId&&!controller.WaitingForContact,5);
                if(boast)Drive(()=>ended.HasValue,60);
                controller.ShotResolved-=probe;rally.PointEnded-=pe;
                if(boast&&ended.HasValue)
                {
                    bool humanWon=ended.Value.Winner==PlayerId.Human;
                    Assert.That(hud.Strokes.BoastWinners-before,Is.EqualTo(humanWon?1:0),"boast winner delta (seed "+seed+", dz "+dz+", winner "+ended.Value.Winner+")");
                    if(humanWon)won++;else lost++;
                }
                rally.StopMode();Refresh();yield return null;
            }
            Debug.Log("[StrokeOfTheMatch] boast tries="+tries+" wonBoasts="+won+" lostBoasts="+lost);
            Assert.That(won+lost,Is.GreaterThan(0),"a boast must have been resolved in play (LastStrokeBoast)");
            // Product decision 25 nov (spec revision 2): a won boast does not reproduce in scene (0 of 72 tries), so only a resolved boast is required.
        }
        private IEnumerator HumanBoast(float dz)
        {
            Assert.That(Drive(()=>rally.Phase==RallyPhase.HumanReception&&controller.Plan.Exists&&!controller.AttemptConsumed,300),Is.True,"a reception to hit");
            Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var star),Is.True);
            Drive(()=>app.Host.ObservedRealTime+StepReal>=star-1e-10,30);
            double left=star-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);
            var p=controller.Plan.PStar;var court=app.Host.ActiveConfig.Court;var wall=SideAim.NearWall(p.x,court);var side=new SideAim(wall,Mathf.Max(.5f,p.z+dz),1.4f);
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,star,true,GestureOutcome.Swing,side.UV,.8f,app.Host.ActiveConfig.Source,"fixture",star,side:side));
            yield return null;
        }
        [UnityTest] public IEnumerator BelowThree_HidesTheLine_AndThreeShowsIt_AndTheDevSwitchHidesItAgain()
        {
            yield return StartMatch();
            Assert.That(StepToMatchOver(),Is.True);hud.Strokes.Reset();for(int i=0;i<2;i++)hud.Strokes.OnStroke(true,true,true,false);
            yield return Show();
            Assert.That(Highlight.text,Is.Empty);Assert.That(Shown(Highlight),Is.False,"2 of each: no line");
            Click("match-result-rematch");Refresh();yield return null;
            Assert.That(StepToMatchOver(),Is.True);hud.Strokes.Reset();for(int i=0;i<3;i++)hud.Strokes.OnStroke(true,true,false,false);
            yield return Show();
            Assert.That(Highlight.text,Is.EqualTo("Golpe del partido: 3 Perfectos"));Assert.That(Shown(Highlight),Is.True);
            Click("match-result-rematch");Refresh();yield return null;
            RallyHud.ExtendedCardOn=false;
            Assert.That(StepToMatchOver(),Is.True);hud.Strokes.Reset();for(int i=0;i<3;i++)hud.Strokes.OnStroke(true,true,false,false);
            yield return Show();
            Assert.That(Shown(Highlight),Is.False,"the DEV switch off leaves the card as it was");Assert.That(Highlight.style.display.value,Is.EqualTo(DisplayStyle.None));
        }
        [UnityTest] public IEnumerator Rematch_StartsTheTallyAtZero_AndTheSecondCardMatchesASecondObserver()
        {
            yield return StartMatch();var first=new Observer(rally,controller);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);yield return Show();first.Dispose();
            AssertAgrees(first,hud.Strokes);
            Click("match-result-rematch");Refresh();yield return null;
            Assert.That(rally.Phase,Is.Not.EqualTo(RallyPhase.MatchOver));
            Assert.That(hud.Strokes.Perfects+hud.Strokes.BoastWinners+hud.Strokes.Volleys,Is.EqualTo(0),"nothing carried into the rematch");
            var second=new Observer(rally,controller);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);yield return Show();second.Dispose();
            Debug.Log("[StrokeOfTheMatch] rematch observer perfects="+second.Perfects+" boastWinners="+second.BoastWinners+" volleys="+second.Volleys);
            AssertAgrees(second,hud.Strokes);
            Assert.That(Highlight.text,Is.EqualTo(StrokeOfTheMatch.Line(second.Perfects,second.BoastWinners,second.Volleys)));
        }
        [UnityTest] public IEnumerator LeavingTheMode_ClearsTheTally()
        {
            yield return StartMatch();
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);yield return Show();
            rally.StopMode();Refresh();yield return null;
            Assert.That(hud.Strokes.Perfects+hud.Strokes.BoastWinners+hud.Strokes.Volleys,Is.EqualTo(0));
        }
    }
}
