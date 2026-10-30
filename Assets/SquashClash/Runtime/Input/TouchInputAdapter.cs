using System;
using System.Collections.Generic;
using SquashClash.Core.Shot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace SquashClash.Runtime.Input
{
    public sealed class TouchInputAdapter : MonoBehaviour
    {
        private readonly HashSet<int> suppressed=new HashSet<int>(),active=new HashSet<int>();
        private InputSettings boundSettings;private bool mergingBefore,blocked,focused=true,paused;
#if UNITY_IOS && !UNITY_EDITOR
        private UnityEngine.iOS.SystemGestureDeferMode gesturesBefore;
#endif
        private double canceledBefore=double.NegativeInfinity;private long sequence;
        public event Action<TouchSample> SampleReceived;
        public event Action<TouchSample,string> SampleFiltered;
        public event Action SystemCanceled;
        public TouchSamplingStats Stats {get;}=new TouchSamplingStats();
        public bool BlockedByUi
        {
            get=>blocked;
            set {if(blocked==value)return;blocked=value;if(value)CancelSystem();}
        }
        public Rect? InputAreaOverride {get;set;}
        public Func<Vector2,bool> UiBlocker {get;set;}
        public Func<double> ReadRealTime {get;set;}=()=>Time.realtimeSinceStartupAsDouble;
        public bool IsPointAllowed(Vector2 point)=>(InputAreaOverride??Screen.safeArea).Contains(point);
        private void OnEnable()
        {
            boundSettings=InputSystem.settings;mergingBefore=boundSettings.disableRedundantEventsMerging;
            // Touchscreen.MergeForward otherwise removes consecutive move records before onEvent.
            // This is a runtime flag; no settings asset or persistent PlayerSetting is saved.
            boundSettings.disableRedundantEventsMerging=true;
#if UNITY_IOS && !UNITY_EDITOR
            gesturesBefore=UnityEngine.iOS.Device.deferSystemGesturesMode;
            UnityEngine.iOS.Device.deferSystemGesturesMode=gesturesBefore|UnityEngine.iOS.SystemGestureDeferMode.BottomEdge;
#endif
            InputSystem.onEvent+=OnInput;
        }
        private void OnDisable()
        {
            InputSystem.onEvent-=OnInput;CancelSystem();
            if(boundSettings!=null)boundSettings.disableRedundantEventsMerging=mergingBefore;
#if UNITY_IOS && !UNITY_EDITOR
            UnityEngine.iOS.Device.deferSystemGesturesMode=gesturesBefore;
#endif
        }
        private void OnInput(InputEventPtr e,InputDevice device)
        {
            if(!(device is Touchscreen)||!e.IsA<StateEvent>())return;
            TouchState raw;
            try {raw=StateEvent.GetState<TouchState>(e);}catch(InvalidOperationException){return;}
            if(raw.touchId==0||!Finite(e.time)||!Finite(raw.position.x)||!Finite(raw.position.y))return;
            TouchPhaseKind phase;
            switch(raw.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:phase=TouchPhaseKind.Began;break;
                case UnityEngine.InputSystem.TouchPhase.Moved:phase=TouchPhaseKind.Moved;break;
                case UnityEngine.InputSystem.TouchPhase.Stationary:phase=TouchPhaseKind.Stationary;break;
                case UnityEngine.InputSystem.TouchPhase.Ended:phase=TouchPhaseKind.Ended;break;
                case UnityEngine.InputSystem.TouchPhase.Canceled:phase=TouchPhaseKind.Canceled;break;
                default:return;
            }
            var sample=new TouchSample(raw.touchId,phase,raw.position,e.time,++sequence);
            Stats.Record(sample,ReadRealTime(),Time.frameCount);
            if(phase==TouchPhaseKind.Began)
            {
                active.Add(raw.touchId);
                if(blocked||!focused||paused||e.time<canceledBefore||!IsPointAllowed(raw.position)||(UiBlocker?.Invoke(raw.position)??false))suppressed.Add(raw.touchId);
                else suppressed.Remove(raw.touchId);
            }
            bool ending=phase==TouchPhaseKind.Ended||phase==TouchPhaseKind.Canceled;
            bool ignore=suppressed.Contains(raw.touchId)||blocked||!focused||paused||e.time<canceledBefore;
            if(ending){active.Remove(raw.touchId);suppressed.Remove(raw.touchId);}
            if(!ignore)SampleReceived?.Invoke(sample);
            else SampleFiltered?.Invoke(sample,blocked?"dev":!focused?"focus":paused?"pause":!IsPointAllowed(raw.position)?"outside-safe-area":"suppressed-ui-or-contact");
        }
        public void CancelSystem()
        {
            foreach(var id in active)suppressed.Add(id);
            canceledBefore=ReadRealTime();SystemCanceled?.Invoke();
        }
        private void OnApplicationFocus(bool value){focused=value;if(!value)CancelSystem();}
        private void OnApplicationPause(bool value){paused=value;if(value)CancelSystem();}
        private static bool Finite(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value);
    }
}
