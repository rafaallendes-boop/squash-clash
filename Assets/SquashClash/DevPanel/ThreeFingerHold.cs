using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch=UnityEngine.InputSystem.EnhancedTouch.Touch;
namespace SquashClash.DevPanel
{
    public sealed class ThreeFingerHold : MonoBehaviour
    {
        private bool fired,focused=true,paused,awaitRelease;
        private double start=double.NaN;
        public Func<bool> CanToggle {get;set;}
        public event Action Held;
        private void OnEnable()=>EnhancedTouchSupport.Enable();
        private void OnDisable(){EnhancedTouchSupport.Disable();start=double.NaN;fired=false;}
        // The latest start belongs to the last finger joining the chord, and changes if any finger is replaced.
        public bool Sample(int fingers,double now,double latestStart,bool allowed=true)
        {
            if(!allowed||fingers!=3){start=double.NaN;fired=false;return false;}
            if(start!=latestStart){start=latestStart;fired=false;}
            if(fired||now-start<1)return false;
            fired=true;return true;
        }
        private void Update()
        {
            int count=0;double latest=0;
            foreach(var finger in Touch.activeFingers){var touch=finger.currentTouch;if(!touch.isInProgress)continue;count++;latest=Math.Max(latest,touch.startTime);}
            bool allowed=focused&&!paused&&(CanToggle?.Invoke()??true);
            if(!allowed)awaitRelease=true;if(count==0)awaitRelease=false;
            if(Sample(count,Time.realtimeSinceStartupAsDouble,latest,allowed&&!awaitRelease))Held?.Invoke();
        }
        private void OnApplicationFocus(bool value){focused=value;if(!value)awaitRelease=true;}
        private void OnApplicationPause(bool value){paused=value;if(value)awaitRelease=true;}
    }
}
