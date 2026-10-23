using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using UnityEngine;
namespace SquashClash.Runtime.App
{
    [DefaultExecutionOrder(-200)]
    public sealed class ThermalMonitor : MonoBehaviour
    {
        private readonly Dictionary<string,double> seconds=new Dictionary<string,double>(StringComparer.Ordinal);
        private IReadOnlyDictionary<string,double> view;
        private bool paused,focused=true,discardResumeFrame;
        public string Current { get; private set; }="unknown";
        public string WorstThisSession { get; private set; }="unknown";
        public IReadOnlyDictionary<string,double> SecondsByState=>view??(view=new ReadOnlyDictionary<string,double>(seconds));
        private void Awake()=>ResetSession();
        private void Update()
        {
            if(paused||!focused)return;
            if(discardResumeFrame){discardResumeFrame=false;return;}
            Observe(ReadSystem(),Time.unscaledDeltaTime);
        }
        public void Observe(string state,double elapsedSeconds)
        {
            int rank=Rank(state);
            if(double.IsNaN(elapsedSeconds)||double.IsInfinity(elapsedSeconds)||elapsedSeconds<0)throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            Current=state;if(rank>Rank(WorstThisSession))WorstThisSession=state;
            seconds.TryGetValue(state,out var previous);seconds[state]=previous+elapsedSeconds;
        }
        public void ResetSession(){seconds.Clear();Current=WorstThisSession=ReadSystem();}
        private void OnApplicationPause(bool value){paused=value;discardResumeFrame=true;}
        private void OnApplicationFocus(bool value){focused=value;discardResumeFrame=true;}
        private static int Rank(string state)=>state switch {"unknown"=>-1,"nominal"=>0,"fair"=>1,"serious"=>2,"critical"=>3,_=>throw new ArgumentOutOfRangeException(nameof(state))};
        private static string ReadSystem()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return SquashThermalState() switch {0=>"nominal",1=>"fair",2=>"serious",3=>"critical",_=>"unknown"};
#else
            return "unknown";
#endif
        }
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int SquashThermalState();
#endif
    }
}
