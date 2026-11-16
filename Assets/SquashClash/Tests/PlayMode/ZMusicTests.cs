using System.Collections;
using NUnit.Framework;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Audio;
using SquashClash.Runtime.Feedback;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Device robustness, block B: original music. A loop for the menu and a very quiet one for the match, with a
    // switch of its own. Like the sound layer it never plays in batch mode, so these tests check what it asks for.
    public sealed class ZMusicTests : ZFirstRunFixture
    {
        private MusicPlayer music;
        private static void ResetSwitch(){MusicPlayer.MusicOn=true;PlayerPrefs.DeleteKey(MusicPlayer.MusicKey);}
        [UnitySetUp] public IEnumerator Setup()
        {
            ResetSwitch();yield return Boot(true);
            music=app.Music;Assert.That(music,Is.Not.Null,"the music player is created with the app");
            music.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return Shutdown();ResetSwitch();}
        private IEnumerator StartMatch()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
        }
        private void Ticks(float seconds){for(float t=0;t<seconds;t+=.02f)music.Tick(.02f);}
        private static float Peak(AudioClip clip,out float rms)
        {
            Assert.That(clip,Is.Not.Null);Assert.That(clip.loadState,Is.EqualTo(AudioDataLoadState.Loaded),clip.name);
            var data=new float[clip.samples*clip.channels];Assert.That(clip.GetData(data,0),Is.True,clip.name);
            float peak=0;double sum=0;foreach(float v in data){peak=Mathf.Max(peak,Mathf.Abs(v));sum+=v*v;}
            rms=Mathf.Sqrt((float)(sum/data.Length));return peak;
        }
        [UnityTest] public IEnumerator Clips_AreTheTwoSynthesizedLoops()
        {
            Assert.That(music.ClipsLoaded,Is.True,"both loops are in Resources/Audio and preloaded");
            var menu=music.ClipOf(MusicTrack.Menu);var match=music.ClipOf(MusicTrack.Match);
            Assert.That(menu.name,Is.EqualTo("music-menu"));Assert.That(match.name,Is.EqualTo("music-match"));
            Assert.That(menu.length,Is.EqualTo(19.2f).Within(.02f));Assert.That(match.length,Is.EqualTo(16f).Within(.02f));
            Assert.That(menu.channels,Is.EqualTo(1));Assert.That(match.channels,Is.EqualTo(1));
            yield return null;
        }
        [UnityTest] public IEnumerator Menu_PlaysTheMenuLoop_AndAMatchCrossesOverToTheMatchLoop()
        {
            music.Tick(.02f);
            Assert.That(music.Wanted,Is.EqualTo(MusicTrack.Menu));Assert.That(music.Sounding(MusicTrack.Menu),Is.True);Assert.That(music.Starts,Is.EqualTo(1));
            Ticks(MusicPlayer.FadeSeconds+.1f);
            Assert.That(music.Volume(MusicTrack.Menu),Is.EqualTo(MusicPlayer.MenuVolume).Within(.001f),"faded in");
            Click("menu-howto");Refresh();Ticks(.2f);
            Assert.That(music.Wanted,Is.EqualTo(MusicTrack.Menu),"the tutorial keeps the menu loop");Assert.That(music.Starts,Is.EqualTo(1));
            Click("tutorial-skip");Refresh();
            yield return StartMatch();music.Tick(.02f);
            Assert.That(music.Wanted,Is.EqualTo(MusicTrack.Match));Assert.That(music.Sounding(MusicTrack.Match),Is.True);Assert.That(music.Starts,Is.EqualTo(2));
            Assert.That(music.Sounding(MusicTrack.Menu),Is.True,"the menu loop fades out instead of cutting");
            Assert.That(music.Volume(MusicTrack.Menu),Is.LessThan(MusicPlayer.MenuVolume));
            Ticks(MusicPlayer.FadeSeconds+.1f);
            Assert.That(music.Sounding(MusicTrack.Menu),Is.False);Assert.That(music.Volume(MusicTrack.Menu),Is.EqualTo(0));
            Assert.That(music.Volume(MusicTrack.Match),Is.EqualTo(MusicPlayer.MatchVolume).Within(.001f));
            Assert.That(MusicPlayer.MatchVolume,Is.LessThan(MusicPlayer.MenuVolume*.3f),"the match loop is much quieter than the menu's");
        }
        [UnityTest] public IEnumerator Pause_SilencesTheMusicAtOnce_AndContinuePicksItUpWhereItWas()
        {
            yield return StartMatch();Ticks(MusicPlayer.FadeSeconds+.1f);int starts=music.Starts;
            Click("rally-pause");Refresh();music.Tick(.02f);
            Assert.That(music.Wanted,Is.EqualTo(MusicTrack.None));
            Assert.That(music.Sounding(MusicTrack.Match),Is.False,"silence in pause, with no fade tail");Assert.That(music.Suspended(MusicTrack.Match),Is.True);
            Assert.That(music.Sounding(MusicTrack.Menu),Is.False);
            Ticks(1f);Assert.That(music.Sounding(MusicTrack.Match),Is.False);
            Click("pause-continue");Refresh();music.Tick(.02f);
            Assert.That(music.Sounding(MusicTrack.Match),Is.True);Assert.That(music.Starts,Is.EqualTo(starts),"resumed, not restarted");Assert.That(music.Resumes,Is.EqualTo(1));
            // An interruption pauses the match into the same card: the same silence.
            app.gameObject.SendMessage("OnApplicationPause",true);Refresh();music.Tick(.02f);
            Assert.That(music.Sounding(MusicTrack.Match),Is.False);
            // Leaving to the menu from the pause card: the menu loop starts and the match loop is dropped.
            Click("pause-menu");Refresh();music.Tick(.02f);
            Assert.That(music.Wanted,Is.EqualTo(MusicTrack.Menu));Assert.That(music.Sounding(MusicTrack.Menu),Is.True);
            Assert.That(music.Suspended(MusicTrack.Match),Is.False);Assert.That(music.Sounding(MusicTrack.Match),Is.False);
        }
        [UnityTest] public IEnumerator Practice_HasNoMusic()
        {
            music.Tick(.02f);Click("menu-practice");Refresh();Ticks(MusicPlayer.FadeSeconds+.1f);
            Assert.That(music.Wanted,Is.EqualTo(MusicTrack.None));Assert.That(music.Sounding(MusicTrack.Menu),Is.False);Assert.That(music.Sounding(MusicTrack.Match),Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator MusicSwitch_IsItsOwn_InTheMenuAndInThePauseCard()
        {
            music.Tick(.02f);
            Assert.That(Find<Button>("menu-music").text,Is.EqualTo("Música: sí"));Assert.That(Find<Button>("menu-sound").text,Is.EqualTo("Sonido: sí"));
            Click("menu-music");Refresh();music.Tick(.02f);
            Assert.That(MusicPlayer.MusicOn,Is.False);Assert.That(PlayerPrefs.GetInt(MusicPlayer.MusicKey,1),Is.EqualTo(0),"remembered between launches");
            Assert.That(Find<Button>("menu-music").text,Is.EqualTo("Música: no"));Assert.That(Find<Button>("pause-music").text,Is.EqualTo("Música: no"));
            Assert.That(music.Wanted,Is.EqualTo(MusicTrack.None));Assert.That(music.Sounding(MusicTrack.Menu),Is.False,"off cuts at once");
            Assert.That(AudioScape.SoundOn,Is.True,"the sound switch did not move");Assert.That(AudioListener.volume,Is.EqualTo(1f));
            Assert.That(Find<Button>("menu-sound").text,Is.EqualTo("Sonido: sí"));
            Click("menu-music");Refresh();music.Tick(.02f);
            Assert.That(MusicPlayer.MusicOn,Is.True);Assert.That(music.Sounding(MusicTrack.Menu),Is.True);
            // Sound off, music on: the music still plays (its sources do not follow the listener volume).
            Click("menu-sound");Refresh();music.Tick(.02f);
            Assert.That(AudioScape.SoundOn,Is.False);Assert.That(MusicPlayer.MusicOn,Is.True);Assert.That(music.Wanted,Is.EqualTo(MusicTrack.Menu));
            var sources=app.transform.Find("Music").GetComponents<AudioSource>();Assert.That(sources.Length,Is.EqualTo(2));
            foreach(var source in sources){Assert.That(source.ignoreListenerVolume,Is.True);Assert.That(source.loop,Is.True);Assert.That(source.spatialBlend,Is.EqualTo(0));}
            Click("menu-sound");Refresh();
            // The same switch in the pause card.
            yield return StartMatch();Click("rally-pause");Refresh();yield return null;
            Assert.That(Find<Button>("pause-music").text,Is.EqualTo("Música: sí"));
            Click("pause-music");Refresh();Assert.That(MusicPlayer.MusicOn,Is.False);Assert.That(Find<Button>("pause-music").text,Is.EqualTo("Música: no"));
            Click("pause-continue");Refresh();Ticks(.5f);
            Assert.That(music.Wanted,Is.EqualTo(MusicTrack.None));Assert.That(music.Sounding(MusicTrack.Match),Is.False,"off stays off during the match");
        }
        // The loudest the match loop ever is, against the quietest thing the player's own stroke plays.
        [UnityTest] public IEnumerator Mix_TheMatchLoopStaysFarUnderThePlayersContact()
        {
            var feedback=app.GetComponent<ShotFeedback>();Assert.That(feedback.ClipsLoaded,Is.True);
            float contact=Mathf.Min(Peak(Resources.Load<AudioClip>("Audio/impact-perfect"),out _),Peak(Resources.Load<AudioClip>("Audio/impact-valid"),out _));
            float match=Peak(music.ClipOf(MusicTrack.Match),out float matchRms)*MusicPlayer.MatchVolume;
            Assert.That(match,Is.LessThanOrEqualTo(contact*.25f),"peak at least 12 dB under the quietest contact clip");
            Assert.That(matchRms*MusicPlayer.MatchVolume,Is.LessThanOrEqualTo(contact*.08f),"average level about 22 dB under it");
            float menu=Peak(music.ClipOf(MusicTrack.Menu),out _)*MusicPlayer.MenuVolume;
            Assert.That(menu,Is.LessThanOrEqualTo(.35f),"the menu loop leaves headroom");Assert.That(menu,Is.GreaterThan(match*3));
            yield return null;
        }
        [UnityTest] public IEnumerator Music_KeepsItsSourcesApart_FromTheContactAudio()
        {
            var feedback=app.GetComponent<ShotFeedback>();
            foreach(var source in app.GetComponents<AudioSource>())Assert.That(source.ignoreListenerVolume,Is.False,"sources on the app object are not the music's");
            var holder=app.transform.Find("Music");Assert.That(holder,Is.Not.Null,"music sources live on their own child");
            Assert.That(holder.GetComponents<AudioSource>().Length,Is.EqualTo(2));
            int sources=app.GetComponentsInChildren<AudioSource>(true).Length;
            yield return StartMatch();
            Assert.That(Drive(()=>{music.Tick((float)StepReal);return rally.HumanPoints+rally.RivalPoints>=2;},300),Is.True);
            Assert.That(app.GetComponentsInChildren<AudioSource>(true).Length,Is.EqualTo(sources),"no source is created while playing");
            Assert.That(feedback.ContactCount+feedback.SwishCount,Is.GreaterThan(0),"the contact audio kept being asked for");
        }
        [UnityTest] public IEnumerator AudioInterruption_AsksThePlayingLoopAgain()
        {
            music.Tick(.02f);Assert.That(music.Starts,Is.EqualTo(1));
            music.OnAudioConfigurationChanged(true);music.Tick(.02f);
            Assert.That(music.Starts,Is.EqualTo(2),"the menu loop is asked again after the audio system came back");
            MusicPlayer.MusicOn=false;music.Tick(.02f);music.OnAudioConfigurationChanged(true);music.Tick(.02f);
            Assert.That(music.Starts,Is.EqualTo(2),"nothing is asked while the music is off");
            yield return null;
        }
        // Performance ceiling: the player runs every frame and must cost no garbage.
        [UnityTest] public IEnumerator Tick_AllocatesNothing_InTheMenuAndDuringAMatch()
        {
            for(int i=0;i<10;i++)music.Tick(.016f);
            long before=System.GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<2000;i++)music.Tick(.016f);
            Assert.That(System.GC.GetAllocatedBytesForCurrentThread()-before,Is.EqualTo(0),"bytes for 2000 ticks in the menu");
            yield return StartMatch();Drive(()=>app.Host.Current.Step>50,2);
            for(int i=0;i<10;i++)music.Tick(.016f);
            before=System.GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<2000;i++)music.Tick(.016f);
            Assert.That(System.GC.GetAllocatedBytesForCurrentThread()-before,Is.EqualTo(0),"bytes for 2000 ticks during a match");
            Assert.That(music.Wanted,Is.EqualTo(MusicTrack.Match));
        }
    }
}
