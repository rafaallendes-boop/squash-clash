using System;
using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Audio;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // 19 nov: the rival audibly puffs when tired, so the player can hear its fatigue. Audio only (view layer): the rules read the
    // same energy and tone the HUD bar reads. Like the rest of the scape these check what is asked for, not what comes out of the speaker.
    public sealed class ZRivalBreathTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup(){yield return Boot(false);app.Clock.Paused=false;app.Tuning.Set("fatigue.enabled",1);AudioScape.RivalBreathOn=true;}
        [UnityTearDown] public IEnumerator Cleanup(){AudioScape.RivalBreathOn=true;return Shutdown();}
        private static bool IsBreath(ScapeSound s)=>s==ScapeSound.RivalBreath1||s==ScapeSound.RivalBreath2||s==ScapeSound.RivalBreath3;
        private int Breaths=>scape.Requests(ScapeSound.RivalBreath1)+scape.Requests(ScapeSound.RivalBreath2)+scape.Requests(ScapeSound.RivalBreath3);
        private IEnumerator StartRally(){rally.StartMode();Refresh();yield return null;yield return null;}
        // Drains the energy with real strong strokes until it sits just under the wanted level.
        private static void DrainTo(FatigueState state,double level){for(int i=0;i<600&&state.Energy>level;i++)state.Strike(1);}
        private void TickFor(float seconds){for(float t=0;t<seconds;t+=.02f)scape.Tick(.02f);}
        [UnityTest] public IEnumerator BelowOnset_APuffFollowsThePoint_AboveOnsetNothing()
        {
            yield return StartRally();var onset=rally.FatigueSettings.Onset;
            DrainTo(rally.RivalEnergy,onset+1.5);Assert.That(rally.RivalEnergy.Energy,Is.GreaterThanOrEqualTo(onset),"fixture: still at or above onset");
            scape.BreathAfterPoint();TickFor(1.5f);Assert.That(Breaths,Is.EqualTo(0),"at or above onset the rival is silent");
            DrainTo(rally.RivalEnergy,onset-5);Assert.That(rally.FatigueSettings.Tone(rally.RivalEnergy.Energy),Is.EqualTo(1),"fixture: amber");
            scape.BreathAfterPoint();Assert.That(Breaths,Is.EqualTo(0),"in the pause after the point, not on top of it");
            TickFor(1.5f);Assert.That(Breaths,Is.EqualTo(1),"one exhale");Assert.That(IsBreath(scape.LastSound),Is.True);
            Assert.That(scape.BreathRequests,Is.EqualTo(1));
            Assert.That(scape.LastVolume,Is.GreaterThan(0).And.LessThanOrEqualTo(AudioScape.TopVolume(scape.LastSound)));
        }
        [UnityTest] public IEnumerator Amber_NeverPuffsAfterAStroke_OnlyTheRedZoneDoes_WithAThreeSecondCooldown()
        {
            yield return StartRally();
            DrainTo(rally.RivalEnergy,rally.FatigueSettings.Onset-10);Assert.That(rally.FatigueSettings.Tone(rally.RivalEnergy.Energy),Is.EqualTo(1));
            scape.BreathAfterRivalStroke();TickFor(1f);Assert.That(Breaths,Is.EqualTo(0),"amber: only after the point");
            DrainTo(rally.RivalEnergy,15);Assert.That(rally.FatigueSettings.Tone(rally.RivalEnergy.Energy),Is.EqualTo(2),"fixture: red");
            scape.BreathAfterRivalStroke();TickFor(1f);Assert.That(Breaths,Is.EqualTo(1),"red: after the stroke");
            scape.BreathAfterRivalStroke();TickFor(1f);Assert.That(Breaths,Is.EqualTo(1),"at most one every 3 s (2 s since the last)");
            TickFor(1.5f);scape.BreathAfterRivalStroke();TickFor(1f);Assert.That(Breaths,Is.EqualTo(2),"after the cooldown it puffs again");
        }
        [UnityTest] public IEnumerator NothingWhileTheHumanSwings_AndItComesBackAfter()
        {
            yield return StartRally();DrainTo(rally.RivalEnergy,40);
            bool busy=true;scape.SwingBusyProbe=()=>busy;
            scape.BreathAfterPoint();TickFor(1.5f);Assert.That(Breaths,Is.EqualTo(0),"suppressed during the human's swing window");Assert.That(scape.BreathSuppressed,Is.EqualTo(1));
            busy=false;scape.BreathAfterPoint();TickFor(1.5f);Assert.That(Breaths,Is.EqualTo(1));Assert.That(scape.BreathSuppressed,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator TheRealSwingStateCountsAsBusy_AndANewMatchIsNotBusy()
        {
            yield return StartRally();DrainTo(rally.RivalEnergy,40);
            Assert.That(scape.HumanSwingBusy,Is.False,"idle human");
            yield return null;
        }
        [UnityTest] public IEnumerator NeverForTheHuman_OnlyTheRivalsEnergyCounts()
        {
            yield return StartRally();DrainTo(rally.HumanEnergy,5);
            Assert.That(rally.RivalEnergy.Energy,Is.EqualTo(100));
            scape.BreathAfterPoint();scape.BreathAfterRivalStroke();TickFor(2f);Assert.That(Breaths,Is.EqualTo(0),"a tired human makes no rival sound");
            DrainTo(rally.RivalEnergy,40);scape.BreathAfterPoint();TickFor(1.5f);Assert.That(Breaths,Is.EqualTo(1),"the rival's own energy does");
        }
        [UnityTest] public IEnumerator FatigueOff_NeverPlays()
        {
            rally.StopMode();app.Tuning.Set("fatigue.enabled",0);yield return StartRally();
            Assert.That(rally.FatigueSettings.Enabled,Is.False);
            DrainTo(rally.RivalEnergy,5);
            scape.BreathAfterPoint();scape.BreathAfterRivalStroke();TickFor(2f);Assert.That(Breaths,Is.EqualTo(0),"fatigue off: silent");
        }
        [UnityTest] public IEnumerator SoundSwitchAndDevSwitch_Off_AreSilent_AndOffIsExactlyToday()
        {
            yield return StartRally();DrainTo(rally.RivalEnergy,10);
            AudioScape.RivalBreathOn=false;int before=scape.TotalRequests;
            scape.BreathAfterPoint();scape.BreathAfterRivalStroke();TickFor(2f);Assert.That(Breaths,Is.EqualTo(0));Assert.That(scape.TotalRequests,Is.EqualTo(before),"DEV off: no request at all");
            AudioScape.RivalBreathOn=true;AudioScape.SoundOn=false;
            scape.BreathAfterPoint();TickFor(2f);Assert.That(Breaths,Is.EqualTo(0),"the Sonido switch off: no breath asked");
            AudioScape.SoundOn=true;scape.BreathAfterPoint();TickFor(2f);Assert.That(Breaths,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator PausedGame_StaysSilent()
        {
            yield return StartRally();DrainTo(rally.RivalEnergy,10);
            scape.BreathAfterPoint();app.Clock.Paused=true;TickFor(2f);Assert.That(Breaths,Is.EqualTo(0));
            app.Clock.Paused=false;TickFor(1f);Assert.That(Breaths,Is.EqualTo(1),"it plays once the game resumes");
        }
        [UnityTest] public IEnumerator DevPanel_Toggle_DefaultsOn_AndFlipsTheSwitch()
        {
            for(int i=0;i<4;i++){Refresh();yield return null;}
            var button=root.Q<Button>("audio-breath-toggle");Assert.That(button,Is.Not.Null,"DEV panel has the breath toggle");
            Assert.That(AudioScape.RivalBreathOn,Is.True);Assert.That(button.text,Does.Contain("Resoplido del rival").And.Contain("sí"));
            Click("audio-breath-toggle");Assert.That(AudioScape.RivalBreathOn,Is.False);Assert.That(button.text,Does.Contain("no"));
            Click("audio-breath-toggle");Assert.That(AudioScape.RivalBreathOn,Is.True);
        }
        private static float Peak(AudioClip clip)
        {
            Assert.That(clip,Is.Not.Null);clip.LoadAudioData();var data=new float[clip.samples*clip.channels];Assert.That(clip.GetData(data,0),Is.True,clip.name);
            float peak=0;foreach(float v in data)peak=Mathf.Max(peak,Mathf.Abs(v));return peak;
        }
        [UnityTest] public IEnumerator Clips_AreThreeDifferentQuietExhales_UnderTheContactAndTheRivalRacket()
        {
            var valid=Peak(Resources.Load<AudioClip>("Audio/impact-valid"));var racket=Peak(scape.ClipOf(ScapeSound.RivalRacket))*AudioScape.TopVolume(ScapeSound.RivalRacket);
            var seen=new System.Collections.Generic.HashSet<int>();
            foreach(var sound in new[]{ScapeSound.RivalBreath1,ScapeSound.RivalBreath2,ScapeSound.RivalBreath3})
            {
                var clip=scape.ClipOf(sound);Assert.That(clip,Is.Not.Null,sound.ToString());
                float inGame=Peak(clip)*AudioScape.TopVolume(sound);
                Assert.That(inGame,Is.GreaterThan(.02f).And.LessThanOrEqualTo(valid*.25f),sound+" is audible but quiet");
                Assert.That(inGame,Is.LessThan(racket),sound+" stays under the rival's racket");
                Assert.That(clip.length,Is.InRange(.25f,.7f),"a short exhale");seen.Add(Mathf.RoundToInt(clip.length*1000));
            }
            Assert.That(seen.Count,Is.EqualTo(3),"three distinct variants");
            yield return null;
        }
        [UnityTest] public IEnumerator Variants_AreAllUsed_WithoutRepeatingTheSameOneTwice()
        {
            yield return StartRally();DrainTo(rally.RivalEnergy,30);
            var used=new System.Collections.Generic.HashSet<ScapeSound>();ScapeSound? last=null;
            for(int i=0;i<30;i++)
            {
                scape.BreathAfterPoint();TickFor(1f);Assert.That(IsBreath(scape.LastSound),Is.True);
                if(last.HasValue)Assert.That(scape.LastSound,Is.Not.EqualTo(last.Value),"never the same exhale twice in a row");
                last=scape.LastSound;used.Add(scape.LastSound);
            }
            Assert.That(used.Count,Is.EqualTo(3));Assert.That(Breaths,Is.EqualTo(30));
        }
        [UnityTest] public IEnumerator InATiredMatch_TheRivalPuffs_AndNeverWhileTheHumanSwings()
        {
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;rally.SetTestEnergy(20);
            rally.StartMatch();Refresh();yield return null;Assert.That(rally.Match,Is.Not.Null);
            int whileBusy=0,ended=0;rally.PointEnded+=_=>ended++;
            int seenBreaths=0;
            Assert.That(Drive(()=>{if(scape.HumanSwingBusy&&scape.BreathRequests>seenBreaths)whileBusy++;seenBreaths=scape.BreathRequests;return ended>=2&&scape.BreathRequests>=2;},400),Is.True,"a tired rival puffs in a real match");
            Assert.That(whileBusy,Is.EqualTo(0),"no breath asked while the human swings");
            Assert.That(rally.FatigueSettings.Tone(rally.RivalEnergy.Energy),Is.GreaterThan(0));
        }
    }
}
