using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Stroke of the match, layout (spec 2026-11-25 section 5.5): the longest line fits the card in two lines at most, on every phone,
    // alone and together with the personal-record line. The label is forced visible with fixed texts: this is layout, not counting.
    public sealed class ZStrokeOfTheMatchLayoutTests : ZDeviceFixture
    {
        private const string Longest="Golpe del partido: 99 dos paredes ganadores",Record="¡Nuevo récord! Rally más largo: 23 golpes · Mayor diferencia: 7 puntos";
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private IEnumerator Settle(){Refresh();yield return null;yield return null;Refresh();yield return null;}
        private static float Width(Label label,string text)=>label.MeasureTextSize(text,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined).x;
        // Baseline for the worst case below: the record line alone, with the highlight line hidden (today's card plus a forced record line).
        [UnityTest] public IEnumerator TheRecordLineAlone_FitsInsideTheSafeArea([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,true);rally.SetNewServe(false);
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");yield return Settle();Assert.That(rally.Match,Is.Not.Null);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);
            hud.Tick(RallyHud.ResultInSeconds);yield return Settle();
            Find<Label>("match-result-highlight").style.display=DisplayStyle.None;
            var record=Find<Label>("match-result-record");record.style.display=DisplayStyle.Flex;record.text=Record;yield return Settle();
            AssertFits("match-result",phone+" record line only");
        }
        // Diagnostic only (logs, no criterion): how far the card's last button ends below the safe area for each text plus the record line.
        [UnityTest] public IEnumerator Diagnostic_OverflowOfTheCardWithEachTextAndTheRecordLine([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,true);rally.SetNewServe(false);
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");yield return Settle();Assert.That(rally.Match,Is.Not.Null);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);
            hud.Tick(RallyHud.ResultInSeconds);yield return Settle();
            var label=Find<Label>("match-result-highlight");var record=Find<Label>("match-result-record");var rematch=Find("match-result-rematch");var menu=Find("match-result-menu");
            record.style.display=DisplayStyle.Flex;record.text=Record;
            foreach(var text in new[]{"","Golpe del partido: 3 voleas","Golpe del partido: 3 Perfectos","Golpe del partido: 3 dos paredes ganadores",Longest})
            {
                label.style.display=text.Length>0?DisplayStyle.Flex:DisplayStyle.None;label.text=text;yield return Settle();
                float bottom=Mathf.Max(rematch.worldBound.yMax,menu.worldBound.yMax);
                Debug.Log("[StrokeOfTheMatch] "+phone+" record+'"+text+"' overflowBelowSafe="+(bottom-Safe.yMax).ToString("F1")+" labelHeight="+label.worldBound.height.ToString("F1"));
            }
        }
        [UnityTest] public IEnumerator TheLongestLine_FitsTheCardInTwoLines_AlongsideTheRecordLine([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,true);rally.SetNewServe(false);
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");yield return Settle();Assert.That(rally.Match,Is.Not.Null);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True,"the demo match must finish");
            hud.Tick(RallyHud.ResultInSeconds);yield return Settle();
            var label=Find<Label>("match-result-highlight");var card=Find("match-result-card");var record=Find<Label>("match-result-record");
            label.style.display=DisplayStyle.Flex;label.text="Golpe del partido: 3 voleas";yield return Settle();
            float oneLine=label.worldBound.height;Assert.That(oneLine,Is.GreaterThan(0));
            foreach(var text in new[]{"Golpe del partido: 3 Perfectos","Golpe del partido: 3 dos paredes ganadores","Golpe del partido: 3 voleas"})
                Debug.Log("[StrokeOfTheMatch] "+phone+" text='"+text+"' width="+Width(label,text).ToString("F1")+" labelContentWidth="+label.contentRect.width.ToString("F1")+" cardContentWidth="+card.contentRect.width.ToString("F1"));
            label.text=Longest;yield return Settle();
            Debug.Log("[StrokeOfTheMatch] "+phone+" longest width="+Width(label,Longest).ToString("F1")+" height="+label.worldBound.height.ToString("F1")+" oneLine="+oneLine.ToString("F1"));
            var c=card.worldBound;var b=label.worldBound;
            Assert.That(b.xMin,Is.GreaterThanOrEqualTo(c.xMin-.5f));Assert.That(b.xMax,Is.LessThanOrEqualTo(c.xMax+.5f));Assert.That(b.yMin,Is.GreaterThanOrEqualTo(c.yMin-.5f));Assert.That(b.yMax,Is.LessThanOrEqualTo(c.yMax+.5f),"inside the card");
            Assert.That(b.height,Is.LessThanOrEqualTo(2*oneLine+2),"two lines at most");
            AssertFits("match-result",phone+" highlight alone");
            record.style.display=DisplayStyle.Flex;record.text=Record;yield return Settle();
            b=label.worldBound;c=card.worldBound;
            Assert.That(b.yMax,Is.LessThanOrEqualTo(c.yMax+.5f),"inside the card with the record line");Assert.That(b.height,Is.LessThanOrEqualTo(2*oneLine+2));
            AssertFits("match-result",phone+" highlight and record");
        }
    }
}
