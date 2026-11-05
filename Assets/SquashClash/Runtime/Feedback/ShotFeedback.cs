using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Feedback
{
    public sealed class ShotFeedback : MonoBehaviour
    {
        private AudioClip contact,swish;
        private AudioSource source;
        public int ContactCount {get;private set;}
        public int SwishCount {get;private set;}
        public void Initialize()
        {
            contact=Resources.Load<AudioClip>("Audio/contact-provisional");swish=Resources.Load<AudioClip>("Audio/swish-provisional");
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;
            contact?.LoadAudioData();swish?.LoadAudioData();
        }
        public void Present(ShotPresentation p){if(p.Category==PresentationCategory.Failed)Swish();else Contact();}
        public void Contact(){ContactCount++;if(contact!=null&&!Application.isBatchMode)source.PlayOneShot(contact);}
        public void Swish(){SwishCount++;if(swish!=null&&!Application.isBatchMode)source.PlayOneShot(swish);}
    }
}
