using System;
using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Audio;
using SquashClash.Runtime.Feedback;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // First run, block E: the court sound layer. It listens to events that already exist and never plays in batch mode,
    // so these tests check what it asks for (which sound, how loud, what pitch), not what comes out of the speaker.
    public sealed class ZAudioScapeTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private IEnumerator StartMatch(bool autopilot)
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=autopilot;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
        }
        private int Sources()=>app.GetComponentsInChildren<AudioSource>(true).Length;
        [UnityTest] public IEnumerator Clips_AreLoaded_AndThePoolNeverGrows()
        {
            Assert.That(scape.ClipsLoaded,Is.True,"every scape clip is in Resources/Audio and preloaded");
            int sources=Sources();
            Assert.That(scape.GetComponentsInChildren<AudioSource>(true).Length,Is.GreaterThanOrEqualTo(AudioScape.Voices+1));
            for(int i=0;i<200;i++){scape.BallContact(SurfaceId.Floor,3+i%9);scape.BallContact(SurfaceId.FrontWall,20);scape.Click();scape.Tick(.016f);}
            Assert.That(scape.TotalRequests,Is.EqualTo(600));
            Assert.That(Sources(),Is.EqualTo(sources),"no AudioSource is created per event");
            yield return null;
        }
        [UnityTest] public IEnumerator BallContacts_AskForTheSurfaceSound_AndQuietBouncesStaySilent()
        {
            scape.BallContact(SurfaceId.Floor,6);Assert.That(scape.LastSound,Is.EqualTo(ScapeSound.BounceFloor));Assert.That(scape.Requests(ScapeSound.BounceFloor),Is.EqualTo(1));
            scape.BallContact(SurfaceId.FrontWall,18);Assert.That(scape.LastSound,Is.EqualTo(ScapeSound.FrontWall));Assert.That(scape.Requests(ScapeSound.FrontWall),Is.EqualTo(1));
            foreach(var wall in new[]{SurfaceId.LeftWall,SurfaceId.RightWall}){scape.BallContact(wall,8);Assert.That(scape.LastSound,Is.EqualTo(ScapeSound.BounceWall),wall.ToString());}
            Assert.That(scape.Requests(ScapeSound.BounceWall),Is.EqualTo(2));
            scape.BallContact(SurfaceId.BackWall,8);Assert.That(scape.LastSound,Is.EqualTo(ScapeSound.BounceGlass),"the back-wall glass has its own sound");
            Assert.That(scape.Requests(ScapeSound.BounceGlass),Is.EqualTo(1));Assert.That(scape.Requests(ScapeSound.BounceWall),Is.EqualTo(2));
            int before=scape.TotalRequests;scape.BallContact(SurfaceId.Floor,AudioScape.QuietSpeed*.5f);
            Assert.That(scape.TotalRequests,Is.EqualTo(before),"a rolling or dying ball makes no sound");
            yield return null;
        }
        [UnityTest] public IEnumerator GlassSwitch_Off_SendsTheBackWallBackToTheSideWallSound()
        {
            try
            {
                AudioScape.GlassSoundOn=false;
                scape.BallContact(SurfaceId.BackWall,8);Assert.That(scape.LastSound,Is.EqualTo(ScapeSound.BounceWall));
                Assert.That(scape.Requests(ScapeSound.BounceGlass),Is.EqualTo(0));Assert.That(scape.Requests(ScapeSound.BounceWall),Is.EqualTo(1));
                AudioScape.GlassSoundOn=true;
                scape.BallContact(SurfaceId.BackWall,8);Assert.That(scape.LastSound,Is.EqualTo(ScapeSound.BounceGlass));
            }
            finally{AudioScape.GlassSoundOn=true;}
            yield return null;
        }
        [UnityTest] public IEnumerator Glass_IsBrighterAndShorterThanTheSideWall()
        {
            var glass=scape.ClipOf(ScapeSound.BounceGlass);var wall=scape.ClipOf(ScapeSound.BounceWall);
            Assert.That(glass.length,Is.LessThan(wall.length),"a short toc");
            // Brightness: zero crossings per second of the synthesized clip (a plain proxy for pitch).
            Assert.That(Crossings(glass),Is.GreaterThan(Crossings(wall)*1.5f),"clearly higher and brighter than the side wall");
            yield return null;
        }
        [UnityTest] public IEnumerator DevPanel_GlassToggle_DefaultsOn_AndFlipsTheSwitch()
        {
            for(int i=0;i<4;i++){Refresh();yield return null;}
            var button=root.Q<Button>("audio-glass-toggle");Assert.That(button,Is.Not.Null,"DEV panel has the glass toggle");
            try
            {
                Assert.That(AudioScape.GlassSoundOn,Is.True);Assert.That(button.text,Does.Contain("sí"));
                Click("audio-glass-toggle");Assert.That(AudioScape.GlassSoundOn,Is.False);Assert.That(button.text,Does.Contain("no"));
                scape.BallContact(SurfaceId.BackWall,8);Assert.That(scape.LastSound,Is.EqualTo(ScapeSound.BounceWall));
                Click("audio-glass-toggle");Assert.That(AudioScape.GlassSoundOn,Is.True);
            }
            finally{AudioScape.GlassSoundOn=true;}
        }
        private static float Crossings(AudioClip clip)
        {
            var data=new float[clip.samples*clip.channels];clip.GetData(data,0);int n=0;
            for(int i=1;i<data.Length;i++)if((data[i-1]<0)!=(data[i]<0))n++;
            return n/clip.length;
        }
        [UnityTest] public IEnumerator Loudness_GrowsALittleWithPower_AndStaysUnderTheMixCeiling()
        {
            scape.BallContact(SurfaceId.FrontWall,5);float soft=scape.LastVolume;
            scape.BallContact(SurfaceId.FrontWall,26);float hard=scape.LastVolume;
            Assert.That(hard,Is.GreaterThan(soft));Assert.That(hard,Is.LessThanOrEqualTo(soft*2f),"slightly, not a different sound");
            foreach(var surface in new[]{SurfaceId.Floor,SurfaceId.FrontWall,SurfaceId.LeftWall,SurfaceId.BackWall})
                for(float speed=2;speed<=40;speed+=2){scape.BallContact(surface,speed);Assert.That(scape.LastVolume,Is.GreaterThan(0).And.LessThanOrEqualTo(AudioScape.MaxVolume),surface+" "+speed);}
            yield return null;
        }
        private static float Peak(AudioClip clip)
        {
            Assert.That(clip,Is.Not.Null);Assert.That(clip.loadState,Is.EqualTo(AudioDataLoadState.Loaded),clip.name);
            var data=new float[clip.samples*clip.channels];Assert.That(clip.GetData(data,0),Is.True,clip.name);
            float peak=0;foreach(float v in data)peak=Mathf.Max(peak,Mathf.Abs(v));return peak;
        }
        // "Nothing may cover the player's contact": the contact clips play at full volume from ShotFeedback, so every
        // court sound, at its loudest, must peak at no more than half of the quietest contact clip (6 dB under it).
        [UnityTest] public IEnumerator Mix_EveryCourtSoundPeaksUnderHalfThePlayersQuietestContact()
        {
            var perfect=Resources.Load<AudioClip>("Audio/impact-perfect");var valid=Resources.Load<AudioClip>("Audio/impact-valid");
            perfect.LoadAudioData();valid.LoadAudioData();
            float contact=Mathf.Min(Peak(perfect),Peak(valid));Assert.That(contact,Is.GreaterThan(.1f));
            foreach(ScapeSound sound in Enum.GetValues(typeof(ScapeSound)))
            {
                float loudest=Peak(scape.ClipOf(sound))*AudioScape.TopVolume(sound);
                Assert.That(AudioScape.TopVolume(sound),Is.LessThanOrEqualTo(AudioScape.MaxVolume),sound.ToString());
                Assert.That(loudest,Is.LessThanOrEqualTo(contact*.5f),sound+" peaks at "+loudest+" against a contact of "+contact);
            }
            Assert.That(Peak(scape.AmbienceClip)*AudioScape.AmbienceVolume,Is.LessThanOrEqualTo(contact*.15f),"ambience stays in the background");
            yield return null;
        }
        [UnityTest] public IEnumerator Repeats_VaryThePitchALittle()
        {
            float min=float.MaxValue,max=float.MinValue;
            for(int i=0;i<40;i++){scape.BallContact(SurfaceId.Floor,8);min=Mathf.Min(min,scape.LastPitch);max=Mathf.Max(max,scape.LastPitch);}
            Assert.That(max-min,Is.GreaterThan(.01f),"the same bounce does not repeat the same pitch");
            float bound=AudioScape.PitchSpread+AudioScape.PowerPitch;
            Assert.That(min,Is.GreaterThanOrEqualTo(1-bound));Assert.That(max,Is.LessThanOrEqualTo(1+bound),"but stays close to the original");
            // Harder contacts sit slightly higher on average.
            float low=0,high=0;
            for(int i=0;i<200;i++){scape.BallContact(SurfaceId.FrontWall,4);low+=scape.LastPitch;scape.BallContact(SurfaceId.FrontWall,26);high+=scape.LastPitch;}
            Assert.That(high,Is.GreaterThan(low));
            yield return null;
        }
        [UnityTest] public IEnumerator InAMatch_TheCourtIsHeard_AndThePlayersContactAudioIsUntouched()
        {
            var feedback=controller.GetComponent<ShotFeedback>();Assert.That(feedback,Is.Not.Null);
            int contacts=0,rivalHits=0,humanDeliveries=0;
            app.Host.ContactRaised+=c=>{if(!c.Persistent&&Mathf.Abs(Vector3.Dot(c.ApproachVelocity,c.Normal))>=AudioScape.QuietSpeed)contacts++;};
            rally.RivalSwing+=(_,contact)=>{if(contact)rivalHits++;};
            controller.PresentationDelivered+=_=>humanDeliveries++;
            yield return StartMatch(true);
            Assert.That(Drive(()=>rivalHits>=3&&humanDeliveries>=2,300),Is.True,"the demo rally must produce strokes");
            scape.Tick((float)StepReal);
            int heard=scape.Requests(ScapeSound.BounceFloor)+scape.Requests(ScapeSound.BounceWall)+scape.Requests(ScapeSound.FrontWall)+scape.Requests(ScapeSound.BounceGlass);
            Assert.That(heard,Is.EqualTo(contacts),"one request per audible ball contact");
            Assert.That(scape.Requests(ScapeSound.FrontWall),Is.GreaterThan(0));Assert.That(scape.Requests(ScapeSound.BounceFloor),Is.GreaterThan(0));
            Assert.That(scape.Requests(ScapeSound.RivalRacket),Is.EqualTo(rivalHits),"one request per rival contact");
            Assert.That(scape.Requests(ScapeSound.RivalStep),Is.GreaterThan(0),"the rival is heard running");
            // The player's contact audio: still exactly one request per delivery, from its own clips only.
            Assert.That(feedback.PerfectCount+feedback.ValidCount+feedback.SwishCount,Is.EqualTo(humanDeliveries));
            Assert.That(new[]{"impact-perfect","impact-valid","swish-provisional"},Does.Contain(feedback.LastClip));
            // The app object carries the contact audio's source and, with the dev panel, the latency calibration's; the scape adds none there.
            int own=1+(app.GetComponent<SquashClash.DevPanel.LatencyCalibration>()!=null?1:0);
            Assert.That(feedback.GetComponents<AudioSource>().Length,Is.EqualTo(own),"the scape's sources live on its own child object");
            Assert.That(scape.transform.Find("Audio scape").GetComponents<AudioSource>().Length,Is.EqualTo(AudioScape.Voices+1));
        }
        [UnityTest] public IEnumerator RivalSteps_FollowTheDistanceTheRivalRuns()
        {
            yield return StartMatch(true);
            float run=0;Vector2 last=rally.Rival.Position;
            Drive(()=>{var now=rally.Rival.Position;float d=(now-last).magnitude;if(d<.5f)run+=d;last=now;return run>=6;},120);
            Assert.That(run,Is.GreaterThanOrEqualTo(6f));
            int steps=scape.Requests(ScapeSound.RivalStep);
            Assert.That(steps,Is.InRange((int)(run/AudioScape.StepMeters)-2,(int)(run/AudioScape.StepMeters)+2),"about one step per "+AudioScape.StepMeters+" m, ran "+run);
        }
        [UnityTest] public IEnumerator PointEnd_AsksForWonOrLost_AShortMomentLater()
        {
            PointResult? ended=null;rally.PointEnded+=r=>{if(!ended.HasValue)ended=r;};
            yield return StartMatch(true);
            Assert.That(Drive(()=>ended.HasValue,300),Is.True);
            Assert.That(scape.Requests(ScapeSound.PointWon)+scape.Requests(ScapeSound.PointLost),Is.EqualTo(0),"not on top of the last bounce");
            for(float t=0;t<AudioScape.JingleDelay+.05f;t+=.02f)scape.Tick(.02f);
            bool won=ended.Value.Winner==PlayerId.Human;
            Assert.That(scape.Requests(ScapeSound.PointWon),Is.EqualTo(won?1:0));Assert.That(scape.Requests(ScapeSound.PointLost),Is.EqualTo(won?0:1));
        }
        [UnityTest] public IEnumerator Pause_SilencesTheCourt_AndAmbienceComesBackOnResume()
        {
            Assert.That(scape.AmbienceWanted,Is.False,"no ambience behind the start menu");
            yield return StartMatch(true);scape.Tick(.02f);
            Assert.That(scape.AmbienceWanted,Is.True,"court ambience while the match runs");
            Drive(()=>rally.Rival.Speed>1.5f,60);
            Click("rally-pause");Refresh();
            int before=scape.TotalRequests-scape.Requests(ScapeSound.UiClick);
            for(int i=0;i<120;i++)scape.Tick(.02f);
            Assert.That(scape.AmbienceWanted,Is.False,"silence while paused");
            Assert.That(scape.TotalRequests-scape.Requests(ScapeSound.UiClick),Is.EqualTo(before),"no steps, bounces or jingles while paused");
            Click("pause-continue");Refresh();scape.Tick(.02f);
            Assert.That(scape.AmbienceWanted,Is.True);
        }
        [UnityTest] public IEnumerator Buttons_Click_AndTheSoundSwitchMutesEverythingAndIsRemembered()
        {
            Assert.That(AudioScape.SoundOn,Is.True);Assert.That(Find<Button>("menu-sound").text,Is.EqualTo("Sonido: sí"));
            int clicks=scape.Requests(ScapeSound.UiClick);
            Click("menu-level-easy");Assert.That(scape.Requests(ScapeSound.UiClick),Is.EqualTo(clicks+1));
            Click("menu-sound");Refresh();
            Assert.That(AudioScape.SoundOn,Is.False);Assert.That(Find<Button>("menu-sound").text,Is.EqualTo("Sonido: no"));
            Assert.That(AudioListener.volume,Is.EqualTo(0f),"the switch mutes the whole game, the player's contact audio included");
            Assert.That(PlayerPrefs.GetInt(AudioScape.SoundKey,1),Is.EqualTo(0));
            yield return StartMatch(false);Drive(()=>app.Host.Current.Step>10,1);
            Click("rally-pause");Refresh();yield return null;
            Assert.That(Find<Button>("pause-sound").text,Is.EqualTo("Sonido: no"));
            Click("pause-sound");Refresh();
            Assert.That(AudioScape.SoundOn,Is.True);Assert.That(AudioListener.volume,Is.EqualTo(1f));
            Assert.That(Find<Button>("pause-sound").text,Is.EqualTo("Sonido: sí"));Assert.That(Find<Button>("menu-sound").text,Is.EqualTo("Sonido: sí"));
            Assert.That(PlayerPrefs.GetInt(AudioScape.SoundKey,0),Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator BatchMode_AsksButPlaysNothing()
        {
            Assume.That(Application.isBatchMode,Is.True,"only meaningful under scripts/test.sh");
            scape.BallContact(SurfaceId.FrontWall,20);scape.Click();scape.Tick(.02f);
            Assert.That(scape.TotalRequests,Is.EqualTo(2));Assert.That(scape.Played,Is.EqualTo(0));
            foreach(var source in scape.GetComponentsInChildren<AudioSource>(true))Assert.That(source.isPlaying,Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator Tick_AndRequests_AllocateNothing()
        {
            yield return StartMatch(true);Drive(()=>app.Host.Current.Step>200,10);
            for(int i=0;i<20;i++){scape.BallContact(SurfaceId.Floor,6);scape.Tick(.016f);}
            long before=GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<2000;i++){scape.Tick(.016f);if(i%7==0)scape.BallContact(SurfaceId.Floor,6+i%5);if(i%31==0)scape.Click();}
            long bytes=GC.GetAllocatedBytesForCurrentThread()-before;
            Assert.That(bytes,Is.EqualTo(0),"bytes allocated by 2000 ticks with requests");
        }
    }
}
