using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SquashClash.Core.Simulation;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    public sealed class DevBar : MonoBehaviour
    {
        private AppBootstrap app;
        private SquashClash.Runtime.Shot.GesturePreview preview;
        private VisualElement bar, root;
        private Button pause, scenario;
        private SpikeSection spike;
        private PerformanceSession session;
        private Button sessionButton,spikeButton;
        private DropdownField legibility;
        private ParamSet selectedLaunch;
        private static readonly string[] LaunchKeys = { "launcher.x","launcher.y","launcher.z","launcher.yawDeg","launcher.pitchDeg","launcher.speed",
            "human.x","human.z","rival.x","rival.z" };
        private VisualElement actions;
        private IEnumerator Start()
        {
            while (AppBootstrap.Instance == null || AppBootstrap.Instance.Host == null || AppBootstrap.Instance.Host.ActiveConfig == null) yield return null;
            app = AppBootstrap.Instance;preview=app.GetComponent<SquashClash.Runtime.Shot.GesturePreview>();
            root = app.GetComponent<UIDocument>().rootVisualElement;
            spike = gameObject.AddComponent<SpikeSection>(); spike.Initialize(app, root);
            session=gameObject.AddComponent<PerformanceSession>();session.Initialize(app,root,index=>{legibility.SetValueWithoutNotify(legibility.choices[index]);SelectLaunch(index);});
            bar = new VisualElement { name="dev-bar" };
            bar.style.position=Position.Absolute; bar.style.flexDirection=FlexDirection.Column;
            actions = new VisualElement(); actions.style.flexDirection=FlexDirection.Row; bar.Add(actions);
            bar.style.paddingTop=6; bar.style.paddingBottom=6; bar.style.backgroundColor=new Color(0.02f,0.06f,0.08f,0.95f);
            Add("launch","Lanzar",()=>app.LaunchCurrent());
            pause=Add("pause","Pausa",()=>app.Host.Clock.Paused=!app.Host.Clock.Paused);
            Add("step","Paso",()=>{app.Host.Clock.Paused=true;app.Host.Clock.RequestSingleStep();});
            Add("replay","Repetir",()=>app.ReplayCurrent());
            var options = new List<string> { "Manual" };
            options.AddRange(LaunchPresets.All(app.Config.Court,app.Config.Physics.Gravity).Select(e=>e.Id+" · "+e.Label));
            legibility = new DropdownField("Legibilidad",options,0) { name="legibility-launch" };
            legibility.style.height=44;legibility.style.fontSize=15;
            legibility.labelElement.style.minWidth=80;legibility.labelElement.style.width=90;
            legibility.RegisterValueChangedCallback(e=>DevControlLog.Run(app,"legibility-launch",()=>SelectLaunch(options.IndexOf(e.newValue))));
            bar.Add(legibility);
            var tests = new VisualElement(); tests.style.flexDirection=FlexDirection.Row; bar.Add(tests);
            scenario=new Button(()=>ActivateExperiment("scenario")) {text="Escenario",name="scenario"};
            scenario.style.flexGrow=1;scenario.style.height=44;tests.Add(scenario);
            var test=new Button(()=>ActivateExperiment("physics-spike")) {text="Benchmark P2 (dev)",name="physics-spike"};
            test.style.flexGrow=1;test.style.height=44;tests.Add(test);
            spikeButton=test;
            sessionButton=new Button(()=>ActivateExperiment("performance-session")) {name="performance-session",text="Sesión H17"};
            sessionButton.style.flexGrow=1;sessionButton.style.height=44;sessionButton.style.whiteSpace=WhiteSpace.Normal;tests.Add(sessionButton);
            // Insert below the runtime resume overlay so it blocks all controls.
            root.Insert(0,bar);gameObject.AddComponent<DevPanelView>().Initialize(app,root);root.RegisterCallback<GeometryChangedEvent>(_=>PlaceInSafeArea());PlaceInSafeArea();
            gameObject.AddComponent<ShotOverlay>().Initialize(app,root);
        }
        public bool ActivateExperiment(string control)
        {
            app.RecordDevControl(control,true,"invoked","activation");
            if(preview!=null&&preview.ProtectSlingProtocol)
            {app.RecordDevControl(control,false,"sling-protocol-protected");return false;}
            if(spike.IsRunning||(session.IsRunning&&control!="performance-session"))
            {app.RecordDevControl(control,false,"experiment-running");return false;}
            bool accepted;
            switch(control)
            {
                case "scenario":spike.LaunchNextScenario();accepted=true;break;
                case "physics-spike":spike.Begin();accepted=spike.IsRunning;break;
                case "performance-session":if(session.IsRunning){session.Cancel();accepted=true;}else accepted=session.Begin();break;
                default:app.RecordDevControl(control,false,"unknown-control");return false;
            }
            app.RecordDevControl(control,accepted,accepted?"completed":"experiment-preconditions");return accepted;
        }
        private void SelectLaunch(int index)
        {
            if(index<=0) { selectedLaunch=null;return; }
            var cfg=EffectiveConfig.From(app.Tuning.Visible);
            var entry=LaunchPresets.All(cfg.Court,cfg.Physics.Gravity)[index-1];var launch=entry.Launch;
            var direction=launch.Direction.normalized;
            app.Tuning.Set("launcher.x",launch.Position.x);app.Tuning.Set("launcher.y",launch.Position.y);app.Tuning.Set("launcher.z",launch.Position.z);
            app.Tuning.Set("launcher.yawDeg",System.Math.Atan2(-direction.x,-direction.z)*180/System.Math.PI);
            app.Tuning.Set("launcher.pitchDeg",System.Math.Asin(direction.y)*180/System.Math.PI);
            app.Tuning.Set("launcher.speed",launch.SpeedRealMps);
            app.Tuning.Set("human.x",entry.HumanXZ.x);app.Tuning.Set("human.z",entry.HumanXZ.y);
            app.Tuning.Set("rival.x",entry.RivalXZ.x);app.Tuning.Set("rival.z",entry.RivalXZ.y);
            selectedLaunch=app.Tuning.Visible;
            app.LaunchCurrent();
        }
        private Button Add(string name,string text,System.Action action)
        {
            var button=new Button(()=>DevControlLog.Run(app,name,action)) {name=name,text=text};
            button.style.flexGrow=1;button.style.flexBasis=0;button.style.height=48;button.style.fontSize=15;
            actions.Add(button);return button;
        }
        private void PlaceInSafeArea()
        {
            if(root?.panel==null||bar==null)return;
            var safe=Screen.safeArea;
            var leftBottom=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(safe.xMin,Screen.height-safe.yMin));
            var rightBottom=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(safe.xMax,Screen.height-safe.yMin));
            bar.style.left=leftBottom.x;bar.style.right=Mathf.Max(0,root.layout.width-rightBottom.x);
            bar.style.bottom=Mathf.Max(0,root.layout.height-leftBottom.y);
        }
        private void Update()
        {
            if(app==null) {if(bar!=null)Destroy(gameObject);return;}
            bar?.SetEnabled(!spike.IsRunning);
            actions?.SetEnabled(!session.IsRunning);legibility?.SetEnabled(!session.IsRunning);
            bool protect=preview?.ProtectSlingProtocol??false;
            scenario?.SetEnabled(!session.IsRunning&&!protect);spikeButton?.SetEnabled(!session.IsRunning&&!protect);sessionButton?.SetEnabled(!protect);
            string protection=protect?"Deshabilitado mientras S1-sling está preparado o en curso.":string.Empty;
            if(scenario!=null)scenario.tooltip=protection;if(spikeButton!=null)spikeButton.tooltip=protection;
            if(sessionButton!=null){sessionButton.text=session.IsRunning?"Cancelar H17": "Sesión H17";sessionButton.tooltip=protect?protection:session.Status;}
            if(selectedLaunch!=null&&LaunchKeys.Any(k=>app.Tuning.Visible.Get(k)!=selectedLaunch.Get(k)))
            {selectedLaunch=null;legibility.SetValueWithoutNotify("Manual");}
            if(scenario!=null)scenario.text="Escenario "+spike.SelectedScenario;
            if(pause!=null)pause.text=app.Host.Clock.Paused?"Continuar":"Pausa";
        }
        private void OnDestroy() => bar?.RemoveFromHierarchy();
    }
}
