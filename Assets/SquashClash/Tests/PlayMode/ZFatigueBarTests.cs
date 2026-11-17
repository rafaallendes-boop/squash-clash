using System;
using System.Collections;
using System.Collections.Generic;
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
    // F2, spec 6: a 3 pt energy line under each name inside the score row. It takes no touches, adds no button, renames nothing,
    // changes neither the score row nor the U3 events, and its red is not the rival chip's.
    public sealed class ZFatigueBarTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup(){yield return Boot(false);app.Clock.Paused=false;app.Tuning.Set("fatigue.enabled",1);}
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private VisualElement Bar(string who)=>Find("rally-energy-"+who);
        private VisualElement Fill(string who)=>Find("rally-energy-"+who+"-fill");
        private void Lower(PlayerId who,int strikes){var e=who==PlayerId.Human?rally.HumanEnergy:rally.RivalEnergy;for(int i=0;i<strikes;i++)e.Strike(1);}
        [UnityTest] public IEnumerator Bar_Is3Pt_TakesNoTouch_HasNoButtonOrLabel()
        {
            rally.StartMode();Refresh();yield return null;yield return null;
            foreach(var who in new[]{"human","rival"})
            {
                var bar=Bar(who);// 3 pt, snapped by the panel to a whole physical pixel.
                Assert.That(bar.resolvedStyle.height,Is.EqualTo(3).Within(1f/root.panel.scaledPixelsPerPoint+.01f),who);
                var all=bar.Query<VisualElement>().ToList().Prepend(bar).Concat(Find("rally-score").Query<VisualElement>().ToList()).ToList();
                Assert.That(all.All(e=>e.pickingMode==PickingMode.Ignore),Is.True,"everything in the score row ignores touches");
                Assert.That(bar.Query<Button>().ToList(),Is.Empty);Assert.That(bar.Query<Label>().ToList(),Is.Empty);
                var picked=root.panel.Pick(bar.worldBound.center);Assert.That(picked==null||!bar.Contains(picked),Is.True,"the panel never picks the bar");
            }
        }
        [UnityTest] public IEnumerator Bar_ShowsOnlyInRallyAndMatch_AndOnlyWithFatigueOn()
        {
            Refresh();yield return null;Assert.That(Shown(Bar("human")),Is.False,"not in a rally");
            rally.StartMode();Refresh();yield return null;yield return null;Assert.That(Shown(Bar("human")),Is.True);Assert.That(Shown(Bar("rival")),Is.True);
            rally.StopMode();Refresh();yield return null;Assert.That(Shown(Bar("human")),Is.False);
            rally.StartMatch();Refresh();yield return null;yield return null;Assert.That(Shown(Bar("human")),Is.True,"in a match");
            rally.StopMode();app.Tuning.Set("fatigue.enabled",0);rally.StartMode();Refresh();yield return null;yield return null;
            Assert.That(Shown(Bar("human")),Is.False,"off: nothing");Assert.That(Shown(Bar("rival")),Is.False);
        }
        [UnityTest] public IEnumerator Bar_Tone_FollowsEnergy_PerPlayer_AndRedIsNotTheRivalChip()
        {
            rally.StartMode();Refresh();yield return null;yield return null;
            Assert.That(Bar("human").ClassListContains("sc-energy--neutral"),Is.True,"E=100");
            Lower(PlayerId.Human,40);Refresh();yield return null;
            Assert.That(rally.HumanEnergy.Energy,Is.LessThan(rally.FatigueSettings.Onset).And.GreaterThanOrEqualTo(25));
            Assert.That(Bar("human").ClassListContains("sc-energy--amber"),Is.True,"below onset");Assert.That(Bar("rival").ClassListContains("sc-energy--neutral"),Is.True,"the other bar is its own");
            Lower(PlayerId.Human,40);Refresh();yield return null;yield return null;
            Assert.That(rally.HumanEnergy.Energy,Is.LessThan(25));Assert.That(Bar("human").ClassListContains("sc-energy--red"),Is.True,"below 25");
            Assert.That(Bar("human").ClassListContains("sc-energy--amber"),Is.False);
            var color=(Color32)Fill("human").resolvedStyle.backgroundColor;
            Assert.That(color,Is.Not.EqualTo(new Color32(255,77,51,255)),"red is not the rival chip's colour");Assert.That(color.r,Is.GreaterThan(color.g+60),"it is a red");
            var chip=(Color32)Find("rally-score").Q(className:"sc-chip--rival").resolvedStyle.backgroundColor;Assert.That(color,Is.Not.EqualTo(chip));
        }
        [UnityTest] public IEnumerator Bar_Width_MovesMonotonically_NeverOscillates_AndWaitsInPause()
        {
            rally.StartMode();Refresh();yield return null;
            Assert.That(hud.HumanEnergyShown,Is.EqualTo(1).Within(1e-4f),"starts full");
            Lower(PlayerId.Human,60);Refresh();double target=rally.HumanEnergy.Energy/100;
            float previous=hud.HumanEnergyShown;var seen=new List<float>();
            for(int i=0;i<240;i++){hud.Tick(1/60f);float now=hud.HumanEnergyShown;Assert.That(now,Is.LessThanOrEqualTo(previous+1e-6f),"never goes back up while falling");Assert.That(now,Is.GreaterThanOrEqualTo((float)target-1e-4f),"never overshoots below its target");previous=now;seen.Add(now);}
            Assert.That(seen[0],Is.LessThan(1),"it moved");Assert.That(seen[0],Is.GreaterThan((float)target+1e-3f),"smoothly, not a jump");Assert.That(previous,Is.EqualTo((float)target).Within(.01f),"and it arrives");
            rally.HumanEnergy.ClosePoint();rally.HumanEnergy.BeginPoint();Lower(PlayerId.Human,0);
            Refresh();float held=hud.HumanEnergyShown;app.Clock.Paused=true;Lower(PlayerId.Human,20);Refresh();
            for(int i=0;i<30;i++)hud.Tick(1/60f);Assert.That(hud.HumanEnergyShown,Is.EqualTo(held),"nothing moves in pause");
        }
        private IEnumerator Script(int fatigue,List<string> events,List<string> names,List<float> heights)
        {
            yield return Shutdown();yield return Boot(true);app.Clock.Paused=false;app.Tuning.Set("fatigue.enabled",fatigue);
            Click("menu-play");Click("tutorial-skip");Refresh();yield return null;yield return null;
            heights.Add(Find("rally-score").layout.height);
            Click("rally-pause");Refresh();Click("rally-pause");Refresh();Click("rally-pause");Refresh();Click("pause-menu");Refresh();
            events.AddRange(File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Line>).Where(l=>l.kind=="menu"||l.kind=="pause").Select(l=>l.kind+"|"+JsonUtility.FromJson<Payload>(l.payload).screen+"|"+JsonUtility.FromJson<Payload>(l.payload).action+"|"+JsonUtility.FromJson<Payload>(l.payload).reason));
            names.AddRange(root.Query<Button>().ToList().Select(b=>b.name));
        }
        [Serializable] private sealed class Line {public string kind,payload;}
        [Serializable] private sealed class Payload {public string screen,action,reason;}
        [UnityTest] public IEnumerator U3Events_ButtonNames_AndScoreRowHeight_AreTheSameWithAndWithoutFatigue()
        {
            var offEvents=new List<string>();var offNames=new List<string>();var offHeights=new List<float>();
            var onEvents=new List<string>();var onNames=new List<string>();var onHeights=new List<float>();
            yield return Script(0,offEvents,offNames,offHeights);yield return Script(1,onEvents,onNames,onHeights);
            Assert.That(offEvents,Is.Not.Empty);Assert.That(onEvents,Is.EqualTo(offEvents),"same U3 menu/pause events");
            Assert.That(onNames,Is.EqualTo(offNames),"same buttons, same names");Assert.That(onNames,Does.Contain("rally-pause"));
            Assert.That(onHeights[0],Is.EqualTo(offHeights[0]).Within(.01f),"the score row does not grow");Assert.That(onHeights[0],Is.GreaterThan(0));
        }
    }
}
