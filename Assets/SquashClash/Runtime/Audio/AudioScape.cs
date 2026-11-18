using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Audio
{
    public enum ScapeSound {BounceFloor,BounceWall,FrontWall,RivalStep,RivalRacket,PointWon,PointLost,UiClick,BounceGlass}
    // First run, sound layer: what the court sounds like around the player's own stroke. It only listens to events that
    // already exist (ball contacts, the rival's swing, the end of a point, button presses) and asks a fixed pool of
    // sources to play a synthesized clip (scripts/audio/make_audioscape.py). The player's contact audio is not here:
    // ShotFeedback keeps its own source, clips, moment and order, and this view never subscribes to its delivery.
    // Mix: at its loudest every court sound peaks at half the quietest contact clip or less (a test measures it).
    // Each repeat shifts pitch a little and harder contacts are slightly louder and higher. No sound in batch mode,
    // none of the court's while the game is paused, and no allocation after start.
    public sealed class AudioScape : MonoBehaviour
    {
        public const int Voices=6,Sounds=9;
        public const string SoundKey="sc.firstRun.sound";
        // PitchSpread: random shift per repeat (+-). PowerPitch: total shift from the softest to the hardest contact.
        public const float PitchSpread=.05f,PowerPitch=.06f,MaxVolume=.35f,StepMeters=.9f,JingleDelay=.18f,QuietSpeed=1.2f,AmbienceVolume=.14f;
        private const float FullSpeed=24f,FullRun=4.8f,TeleportMeters=.5f;
        private static readonly string[] ClipNames={"scape-bounce-floor","scape-bounce-wall","scape-front-wall","scape-rival-step","scape-rival-racket","scape-point-won","scape-point-lost","scape-ui-click","scape-bounce-glass"};
        // Source volume at full power; the softest contact plays at 60 % of it.
        // Set against the quietest contact clip (impact-valid, peak 0.29 at full volume): each sound at its loudest
        // peaks at no more than half of that (front wall 0.14, floor 0.12, rival racket 0.11, walls 0.10, steps 0.06).
        private static readonly float[] BaseVolume={.18f,.21f,.20f,.16f,.33f,.32f,.30f,.30f,.19f};
        private static readonly string[] ClickPrefixes={"menu-","tutorial-","pause-","match-result-","rally-"};
        // DEV switch (default on): the back-wall glass has its own "toc"; off, it sounds like the side walls again. Not remembered.
        public static bool GlassSoundOn=true;
        private static bool? soundOn;
        // The "Sonido" switch: mutes the whole game (listener volume), remembered between launches.
        public static bool SoundOn
        {
            get=>soundOn??PlayerPrefs.GetInt(SoundKey,1)==1;
            set{soundOn=value;PlayerPrefs.SetInt(SoundKey,value?1:0);PlayerPrefs.Save();AudioListener.volume=value?1:0;}
        }
        private readonly AudioClip[] clips=new AudioClip[Sounds];private readonly int[] counts=new int[Sounds];
        private readonly AudioSource[] voices=new AudioSource[Voices];private AudioSource ambience;
        private AppBootstrap app;private BallSimulationHost host;private RallyDirector rally;private VisualElement root;
        private int nextVoice;private uint random=2891336453u;
        private bool rivalPending,ambienceOn,hasRival;private float jingleIn=-1,stepRun;private ScapeSound jingle;private Vector2 lastRival;
        public bool ClipsLoaded
        {
            get
            {
                if(ambience==null||ambience.clip==null||ambience.clip.loadState!=AudioDataLoadState.Loaded)return false;
                for(int i=0;i<Sounds;i++)if(clips[i]==null||clips[i].loadState!=AudioDataLoadState.Loaded)return false;
                return true;
            }
        }
        public int TotalRequests {get;private set;}
        // Requests that reached a source (always zero in batch mode).
        public int Played {get;private set;}
        public float LastVolume {get;private set;}
        public float LastPitch {get;private set;}
        public ScapeSound LastSound {get;private set;}
        // Court ambience: only while a rally or match is being played and the game is not paused.
        public bool AmbienceWanted=>app!=null&&rally!=null&&rally.Active&&app.Clock!=null&&!app.Clock.Paused;
        // How many times the ambience loop was asked to start (counted in batch mode too).
        public int AmbienceStarts {get;private set;}
        private bool audioReset;
        // Device robustness: the audio system came back from an interruption or a route change (call, alarm,
        // headphones): its sources may have been stopped without the game being paused, so the loop is asked for again.
        public void OnAudioConfigurationChanged(bool deviceWasChanged){audioReset=true;}
        public int Requests(ScapeSound sound)=>counts[(int)sound];
        public AudioClip ClipOf(ScapeSound sound)=>clips[(int)sound];
        public AudioClip AmbienceClip=>ambience!=null?ambience.clip:null;
        // The loudest a sound is ever asked to play (source volume).
        public static float TopVolume(ScapeSound sound)=>BaseVolume[(int)sound];
        public void Initialize(AppBootstrap app)
        {
            if(this.app!=null)return;this.app=app;host=app.Host;rally=app.Rally;
            // Sources live on a child so the app object keeps only the contact audio's own source.
            var holder=new GameObject("Audio scape");holder.transform.SetParent(transform,false);
            for(int i=0;i<Sounds;i++){clips[i]=Resources.Load<AudioClip>("Audio/"+ClipNames[i]);if(clips[i]!=null)clips[i].LoadAudioData();}
            for(int i=0;i<Voices;i++)voices[i]=Source(holder,false);
            ambience=Source(holder,true);ambience.clip=Resources.Load<AudioClip>("Audio/scape-ambience");ambience.volume=AmbienceVolume;
            if(ambience.clip!=null)ambience.clip.LoadAudioData();
            host.ContactRaised+=OnContact;
            if(rally!=null){rally.RivalSwing+=OnRivalSwing;rally.PointEnded+=OnPointEnded;}
            root=app.GetComponent<UIDocument>().rootVisualElement;
            root.RegisterCallback<ClickEvent>(OnUiClick,TrickleDown.TrickleDown);root.RegisterCallback<NavigationSubmitEvent>(OnUiSubmit,TrickleDown.TrickleDown);
            AudioListener.volume=SoundOn?1:0;
            AudioSettings.OnAudioConfigurationChanged+=OnAudioConfigurationChanged;
        }
        private static AudioSource Source(GameObject holder,bool loop)
        {
            var source=holder.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.loop=loop;return source;
        }
        private float Next(){random^=random<<13;random^=random>>17;random^=random<<5;return (random&0xFFFFFF)/(float)0x1000000;}
        private void Request(ScapeSound sound,float volume,float pitch)
        {
            int index=(int)sound;counts[index]++;TotalRequests++;LastSound=sound;LastVolume=volume;LastPitch=pitch;
            if(Application.isBatchMode||clips[index]==null)return;
            var voice=voices[nextVoice];nextVoice=(nextVoice+1)%Voices;
            voice.clip=clips[index];voice.volume=volume;voice.pitch=pitch;voice.Play();Played++;
        }
        // strength 0..1: how hard the contact was. Slightly louder and higher with it; a small random shift on every repeat.
        private void RequestScaled(ScapeSound sound,float strength)
        {
            strength=Mathf.Clamp01(strength);
            Request(sound,BaseVolume[(int)sound]*(.6f+.4f*strength),1+(Next()*2-1)*PitchSpread+PowerPitch*(strength-.5f));
        }
        // speed: how fast the ball met the surface, along its normal (m/s).
        public void BallContact(SurfaceId surface,float speed)
        {
            if(app==null||speed<QuietSpeed)return; // rolling and dying bounces stay quiet, as in ImpactFx
            RequestScaled(surface==SurfaceId.Floor?ScapeSound.BounceFloor:surface==SurfaceId.FrontWall?ScapeSound.FrontWall:ScapeSound.BounceWall,speed/FullSpeed);
        }
        public void Click(){if(app!=null)Request(ScapeSound.UiClick,BaseVolume[(int)ScapeSound.UiClick],1+(Next()*2-1)*PitchSpread*.5f);}
        private void OnContact(ContactEvent contact)
        {
            if(!contact.Persistent)BallContact(contact.Surface,Mathf.Abs(Vector3.Dot(contact.ApproachVelocity,contact.Normal)));
        }
        private void OnRivalSwing(StrokeSide side,bool contact){if(contact)rivalPending=true;}
        private void OnPointEnded(PointResult point)
        {
            if(!point.Winner.HasValue)return;
            jingle=point.Winner.Value==PlayerId.Human?ScapeSound.PointWon:ScapeSound.PointLost;jingleIn=JingleDelay;
        }
        private void OnUiClick(ClickEvent e)=>ClickFor(e.target as VisualElement);
        private void OnUiSubmit(NavigationSubmitEvent e)=>ClickFor(e.target as VisualElement);
        // Only the game's own buttons click; developer tools (the latency calibration among them) stay silent.
        private void ClickFor(VisualElement target)
        {
            for(var e=target;e!=null;e=e.parent)
            {
                if(!(e is Button)||string.IsNullOrEmpty(e.name))continue;
                for(int i=0;i<ClickPrefixes.Length;i++)if(e.name.StartsWith(ClickPrefixes[i],System.StringComparison.Ordinal)){Click();return;}
                return;
            }
        }
        public void Tick(float dt)
        {
            if(app==null)return;
            // The rival's stroke is judged once its velocity is in the ball (the director relaunches from the contact state).
            if(rivalPending)
            {
                rivalPending=false;
                if(host.ActiveConfig!=null)RequestScaled(ScapeSound.RivalRacket,host.Current.Velocity.magnitude/(float)host.ActiveConfig.Source.Get("shot.vMax"));
            }
            bool paused=app.Clock==null||app.Clock.Paused,want=AmbienceWanted&&ambience.clip!=null,audible=!Application.isBatchMode;
            if(audioReset){audioReset=false;ambienceOn=false;}
            if(want!=ambienceOn)
            {
                ambienceOn=want;
                if(want)AmbienceStarts++;
                if(audible){if(!want)ambience.Pause();else{ambience.UnPause();if(!ambience.isPlaying)ambience.Play();}}
            }
            // A source the system stopped behind the game's back starts again on its own.
            else if(want&&audible&&!ambience.isPlaying){AmbienceStarts++;ambience.Play();}
            if(paused)return;
            if(jingleIn>=0&&dt>0)
            {
                jingleIn-=dt;
                if(jingleIn<=0){jingleIn=-1;Request(jingle,BaseVolume[(int)jingle],1);}
            }
            // Rival footsteps: one per StepMeters actually run; a jump (the serve places the body) is not a run.
            var body=rally!=null&&rally.Active?rally.Rival:null;
            if(body==null){hasRival=false;stepRun=0;return;}
            var at=body.Position;
            if(!hasRival){hasRival=true;lastRival=at;return;}
            float moved=(at-lastRival).magnitude;lastRival=at;
            if(moved>TeleportMeters){stepRun=0;return;}
            stepRun+=moved;
            if(stepRun>=StepMeters){stepRun-=StepMeters;RequestScaled(ScapeSound.RivalStep,body.Speed/FullRun);}
        }
        private void LateUpdate()=>Tick(Mathf.Min(Time.unscaledDeltaTime,.05f));
        private void OnDestroy()
        {
            AudioSettings.OnAudioConfigurationChanged-=OnAudioConfigurationChanged;
            if(host!=null)host.ContactRaised-=OnContact;
            if(rally!=null){rally.RivalSwing-=OnRivalSwing;rally.PointEnded-=OnPointEnded;}
            if(root!=null){root.UnregisterCallback<ClickEvent>(OnUiClick,TrickleDown.TrickleDown);root.UnregisterCallback<NavigationSubmitEvent>(OnUiSubmit,TrickleDown.TrickleDown);}
        }
    }
}
