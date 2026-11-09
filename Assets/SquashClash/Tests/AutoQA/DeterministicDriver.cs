using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
namespace SquashClash.AutoQA
{
    internal sealed class DeterministicDriver : IDisposable
    {
        internal AppBootstrap App {get;private set;}
        internal ShotController Controller {get;private set;}
        internal Vector2 Anchor {get;private set;}
        internal double Target {get;private set;}
        internal EnvironmentMeta Environment {get;private set;}
        private TouchInputAdapter input;private Touchscreen screen;private Func<double> previousTime;
        private double now,dt;private PracticeMode practice;private GesturePreview preview;
        internal IEnumerator Prepare(Scenario scenario)
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            App=AppBootstrap.Instance;App.GetComponent<ResumeOverlay>().Continue();
            App.Tuning.LoadPreset(PresetCodec.FromSet(App.Tuning.Defaults,"AutoQA defaults",App.Build.Label),new List<string>());
            App.Tuning.Set("practice.launch",scenario.launch);App.LaunchCurrent();
            Controller=App.GetComponent<ShotController>();practice=App.GetComponent<PracticeMode>();
            App.Host.enabled=App.enabled=Controller.enabled=practice.enabled=false;
            input=App.GetComponent<TouchInputAdapter>();preview=App.GetComponent<GesturePreview>();
            now=App.Host.ObservedRealTime;previousTime=input.ReadRealTime;input.ReadRealTime=()=>now;screen=InputSystem.AddDevice<Touchscreen>();
            Anchor=(Vector2)UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(scenario.aimX,scenario.aimY,0));
            App.Host.ClockMap.TrySimToReal(Controller.Plan.TStarStep,0,out var target);Target=target;
            float mm=ScreenMetrics.MmPerPx(Screen.dpi,(float)App.Tuning.Applied.Get("input.fallbackDpi"),out var dpi);
            var safe=Screen.safeArea;
            Environment=new EnvironmentMeta {screen=new[]{Screen.width,Screen.height},safeArea=new[]{safe.x,safe.y,safe.width,safe.height},
                dpiSource=dpi.ToString(),mmPerPx=mm,uiBlockedAtAnchor=input.UiBlocker==null||input.UiBlocker(Anchor),anchorInSafeArea=safe.Contains(Anchor)};
            dt=1d/App.Tuning.Applied.Get("physics.stepHz")/App.Tuning.Applied.Get("pace.tempo");
        }
        internal void Pump(Scenario scenario,TouchScript script)
        {
            double start=now,end=script.samples[script.samples.Length-1].t;int sent=0;
            long receptionEnd=App.Host.Prediction.States[App.Host.Prediction.States.Count-1].Step;
            bool complete=false;
            for(int frame=1;frame<=1500;frame++)
            {
                now=start+frame*dt;
                while(sent<script.samples.Length&&script.samples[sent].t<=now)
                {
                    var p=script.samples[sent++];
                    InputSystem.QueueStateEvent(screen,new TouchState{touchId=101,phase=(UnityEngine.InputSystem.TouchPhase)Enum.Parse(typeof(UnityEngine.InputSystem.TouchPhase),p.phase),position=new Vector2(p.x,p.y)},p.t);
                }
                InputSystem.Update();App.Host.Advance(dt);preview.Tick(now);practice.Tick(dt);Controller.ProcessPending();preview.FlushRecords();
                if(scenario.kind=="swing"?now>=end+.15&&(Controller.LastRecord?.Applied!=true||App.Host.Current.Step>=App.Host.Prediction.Contacts[0].Step):sent==script.samples.Length&&App.Host.Current.Step>=receptionEnd)
                {complete=true;break;}
            }
            if(!complete)throw new TimeoutException("virtual-frame-watchdog");
        }
        public void Dispose()
        {
            if(input!=null)input.ReadRealTime=previousTime;
            if(screen!=null)InputSystem.RemoveDevice(screen);
            if(App!=null)UnityEngine.Object.Destroy(App.gameObject);
        }
    }
}
