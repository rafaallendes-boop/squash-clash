using SquashClash.Runtime.App;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using UnityEngine;
namespace SquashClash.Runtime.Audio
{
    public enum MusicTrack {None,Menu,Match}
    // Device robustness, block B: original music, synthesized by scripts/audio/make_music.py. One loop for the start
    // menu and its tutorial, and a quiet one while a rally or match is being played. It only reads state (which
    // screen is open, whether a rally is active, whether the clock is paused): it subscribes to no gameplay event and
    // never touches the contact audio (ShotFeedback) or the court sounds (AudioScape).
    // Silence in pause: the playing loop is paused on the spot, with no fade tail, and picked up where it was.
    // Between menu and match the loops cross-fade. The "Música" switch is its own: the two sources ignore the listener
    // volume, which is what the "Sonido" switch drives. No sound in batch mode and no allocation after start.
    public sealed class MusicPlayer : MonoBehaviour
    {
        public const string MusicKey="sc.music";
        // Source volume at full fade. The match loop is a bed whose average level sits about 16 dB under the peak of the
        // player's quietest contact clip, with its own peak at half of that peak or less (a test measures it against the clips).
        public const float MenuVolume=.5f,MatchVolume=.24f,FadeSeconds=.6f;
        private const int Off=0,On=1,Held=2;
        private static readonly string[] ClipNames={"music-menu","music-match"};
        private static readonly float[] FullVolume={MenuVolume,MatchVolume};
        private static bool? musicOn;
        // The "Música" switch, remembered between launches.
        public static bool MusicOn
        {
            get=>musicOn??PlayerPrefs.GetInt(MusicKey,1)==1;
            set{musicOn=value;PlayerPrefs.SetInt(MusicKey,value?1:0);PlayerPrefs.Save();}
        }
        private readonly AudioSource[] sources=new AudioSource[2];private readonly int[] state=new int[2];private readonly float[] gain=new float[2];
        private AppBootstrap app;private FirstRunFlow flow;private RallyDirector rally;private ResumeOverlay overlay;
        private bool audioReset;
        public bool ClipsLoaded
        {
            get
            {
                for(int i=0;i<2;i++)if(sources[i]==null||sources[i].clip==null||sources[i].clip.loadState!=AudioDataLoadState.Loaded)return false;
                return true;
            }
        }
        // How many times a loop was asked to play from its start, and to pick up after a pause (counted in batch mode too).
        public int Starts {get;private set;}
        public int Resumes {get;private set;}
        // The system layer or any pause holds the game: music waits in silence.
        private bool Waiting=>app.Clock==null||app.Clock.Paused||overlay!=null&&overlay.IsVisible;
        // What should be heard right now.
        public MusicTrack Wanted
        {
            get
            {
                if(app==null||!MusicOn)return MusicTrack.None;
                if(flow==null)flow=app.FirstRun;
                if(overlay!=null&&overlay.IsVisible)return MusicTrack.None;
                if(flow!=null&&flow.Screen!=FirstRunScreen.None)return MusicTrack.Menu;
                return rally!=null&&rally.Active&&!Waiting?MusicTrack.Match:MusicTrack.None;
            }
        }
        public bool Sounding(MusicTrack track)=>track!=MusicTrack.None&&state[(int)track-1]==On;
        public bool Suspended(MusicTrack track)=>track!=MusicTrack.None&&state[(int)track-1]==Held;
        public float Volume(MusicTrack track)=>track==MusicTrack.None?0:FullVolume[(int)track-1]*gain[(int)track-1];
        public AudioClip ClipOf(MusicTrack track)=>track==MusicTrack.None||sources[(int)track-1]==null?null:sources[(int)track-1].clip;
        public void Initialize(AppBootstrap app)
        {
            if(this.app!=null)return;this.app=app;rally=app.Rally;flow=app.FirstRun;overlay=app.GetComponent<ResumeOverlay>();
            // Sources live on their own child: the app object keeps only the contact audio's source.
            var holder=new GameObject("Music");holder.transform.SetParent(transform,false);
            for(int i=0;i<2;i++)
            {
                var source=holder.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.loop=true;source.volume=0;
                source.ignoreListenerVolume=true;source.priority=200;
                source.clip=Resources.Load<AudioClip>("Audio/"+ClipNames[i]);if(source.clip!=null)source.clip.LoadAudioData();
                sources[i]=source;
            }
            AudioSettings.OnAudioConfigurationChanged+=OnAudioConfigurationChanged;
        }
        // The audio system came back from an interruption or a route change: a playing loop may have been stopped.
        public void OnAudioConfigurationChanged(bool deviceWasChanged){audioReset=true;}
        public void Tick(float dt)
        {
            if(app==null)return;
            var want=Wanted;bool audible=!Application.isBatchMode,waiting=Waiting,on=MusicOn,reset=audioReset;audioReset=false;
            float step=dt>0?dt/FadeSeconds:0;
            for(int i=0;i<2;i++)
            {
                var source=sources[i];bool wanted=(int)want==i+1;
                if(wanted)
                {
                    if(state[i]==Off){state[i]=On;gain[i]=0;Starts++;if(audible&&source.clip!=null){source.volume=0;source.Play();}}
                    else if(state[i]==Held){state[i]=On;Resumes++;if(audible)source.UnPause();}
                    // A source the system stopped behind the game's back starts again on its own.
                    else if(reset||audible&&source.clip!=null&&!source.isPlaying){Starts++;if(audible&&source.clip!=null)source.Play();}
                    gain[i]=Mathf.MoveTowards(gain[i],1,step);
                }
                else if(state[i]==On)
                {
                    // Switched off: cut. Paused: hold in silence on the spot. Another screen: fade out, then stop.
                    if(!on||want!=MusicTrack.None&&gain[i]<=0){state[i]=Off;gain[i]=0;if(audible)source.Stop();}
                    else if(want==MusicTrack.None&&waiting){state[i]=Held;if(audible)source.Pause();}
                    else
                    {
                        gain[i]=Mathf.MoveTowards(gain[i],0,step);
                        if(gain[i]<=0){state[i]=Off;if(audible)source.Stop();}
                    }
                }
                // A loop held by a pause is dropped once the music is switched off or another one takes over.
                else if(state[i]==Held&&(!on||want!=MusicTrack.None)){state[i]=Off;gain[i]=0;if(audible)source.Stop();}
                source.volume=FullVolume[i]*gain[i];
            }
        }
        private void LateUpdate()=>Tick(Mathf.Min(Time.unscaledDeltaTime,.05f));
        private void OnDestroy()=>AudioSettings.OnAudioConfigurationChanged-=OnAudioConfigurationChanged;
    }
}
