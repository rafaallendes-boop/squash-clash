using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Feedback
{
    public sealed class ShotFeedback : MonoBehaviour
    {
        private AudioClip perfect,perfectOwn,valid,swish;
        private AudioSource source;
        public int PerfectCount {get;private set;}
        public int ValidCount {get;private set;}
        public int SwishCount {get;private set;}
        // Total impact requests (Perfect + Valid).
        public int ContactCount=>PerfectCount+ValidCount;
        public string LastClip {get;private set;}
        // The file actually requested (LastClip names the category's clip and does not change).
        public string LastFile {get;private set;}
        public bool ClipsLoaded=>perfect!=null&&valid!=null&&swish!=null&&perfect.loadState==AudioDataLoadState.Loaded&&valid.loadState==AudioDataLoadState.Loaded&&swish.loadState==AudioDataLoadState.Loaded;
        public void Initialize()
        {
            perfect=Resources.Load<AudioClip>("Audio/impact-perfect");perfectOwn=Resources.Load<AudioClip>("Audio/scape-perfect-own");valid=Resources.Load<AudioClip>("Audio/impact-valid");swish=Resources.Load<AudioClip>("Audio/swish-provisional");
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;
            perfect?.LoadAudioData();perfectOwn?.LoadAudioData();valid?.LoadAudioData();swish?.LoadAudioData();
        }
        // Only consumer of the single final delivery: one audio request and, when the launch switch is on, one haptic request.
        public void Present(ShotPresentation p)
        {
            if(p.Category==PresentationCategory.Failed){SwishCount++;Play(swish,"swish-provisional");return;}
            if(p.Category==PresentationCategory.Perfect){PerfectCount++;if(SquashClash.Runtime.Audio.AudioScape.PerfectOwnOn&&perfectOwn!=null){Play(perfectOwn,"impact-perfect","scape-perfect-own");}else Play(perfect,"impact-perfect");}else{ValidCount++;Play(valid,"impact-valid");}
            if(FeedbackMap.TryHaptic(p.Category,Haptics.Enabled,out var style))Haptics.Impact(style);
        }
        private void Play(AudioClip clip,string name,string file=null){LastClip=name;LastFile=file??name;if(clip!=null&&!Application.isBatchMode)source.PlayOneShot(clip);}
    }
}
