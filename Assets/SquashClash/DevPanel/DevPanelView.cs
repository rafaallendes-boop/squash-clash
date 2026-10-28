using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    public sealed class DevPanelView : MonoBehaviour
    {
        private AppBootstrap app;
        private VisualElement root,panel;
        private Button toggle;
        private Label heading;
        private ParamSection parameters;
        private PresetsSection presets;
        private DiagnosticsSection diagnostics;
        private ThreeFingerHold gesture;
        private float elapsed;
        public bool IsOpen=>panel!=null&&panel.style.display.value!=DisplayStyle.None;
        public void Initialize(AppBootstrap app,VisualElement root)
        {
            this.app=app;this.root=root;
            panel=new VisualElement {name="dev-panel"};panel.style.position=Position.Absolute;
            panel.style.backgroundColor=new Color(.02f,.06f,.08f,.98f);panel.style.color=Color.white;
            panel.style.paddingLeft=panel.style.paddingRight=10;panel.style.paddingTop=panel.style.paddingBottom=6;
            panel.style.fontSize=15;panel.style.minWidth=0;panel.style.minHeight=0;panel.style.overflow=Overflow.Hidden;
            var header=new VisualElement {name="panel-header"};header.style.flexDirection=FlexDirection.Row;header.style.flexShrink=0;header.style.minHeight=52;panel.Add(header);
            heading=new Label {name="pending-changes"};heading.style.whiteSpace=WhiteSpace.Normal;heading.style.flexGrow=1;heading.style.flexBasis=0;heading.style.minWidth=0;
            heading.style.maxHeight=60;heading.style.overflow=Overflow.Hidden;header.Add(heading);
            var apply=new Button(()=>DevControlLog.Run(app,"apply-relaunch",()=>{app.LaunchCurrent();Refresh();})){text="Aplicar y relanzar",name="apply-relaunch"};apply.style.minHeight=48;apply.style.width=128;apply.style.flexShrink=0;apply.style.whiteSpace=WhiteSpace.Normal;header.Add(apply);
            var tabs=new VisualElement {name="panel-tabs"};tabs.style.flexDirection=FlexDirection.Row;tabs.style.flexShrink=0;tabs.style.height=44;panel.Add(tabs);
            var paramsScroll=new ScrollView {name="parameters-scroll"};var presetsScroll=new ScrollView {name="presets-scroll"};var diagnosticsScroll=new ScrollView {name="diagnostics-scroll"};
            foreach(var scroll in new[]{paramsScroll,presetsScroll,diagnosticsScroll})
            {
                // Only the viewport shrinks. Intrinsic content height must never shrink the tabs to zero.
                scroll.style.marginTop=4;scroll.style.flexGrow=1;scroll.style.flexShrink=1;scroll.style.flexBasis=0;scroll.style.minHeight=0;scroll.style.minWidth=0;
                scroll.contentContainer.style.flexShrink=0;scroll.contentContainer.style.width=Length.Percent(100);
                scroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;panel.Add(scroll);
            }
            parameters=new ParamSection(app,Refresh);presets=new PresetsSection(app,Refresh);diagnostics=new DiagnosticsSection(app);
            paramsScroll.Add(parameters.Root);presetsScroll.Add(presets.Root);diagnosticsScroll.Add(diagnostics.Root);
            void Show(ScrollView selected)
            {
                foreach(var scroll in new[]{paramsScroll,presetsScroll,diagnosticsScroll})scroll.style.display=scroll==selected?DisplayStyle.Flex:DisplayStyle.None;
                foreach(var button in tabs.Query<Button>().ToList())button.style.backgroundColor=button.userData==selected?new Color(.12f,.34f,.4f):new Color(.16f,.2f,.23f);
            }
            void Tab(string text,ScrollView scroll)
            {
                var b=new Button(()=>DevControlLog.Run(app,"dev-tab-"+scroll.name,()=>{Show(scroll);Refresh();})){name="dev-tab-"+scroll.name,text=text,userData=scroll};b.style.flexGrow=1;b.style.flexBasis=0;b.style.minWidth=0;b.style.height=44;b.style.flexShrink=0;
                b.style.marginTop=0;b.style.marginBottom=0;b.style.fontSize=14;b.style.whiteSpace=WhiteSpace.Normal;b.style.color=Color.white;b.style.overflow=Overflow.Hidden;tabs.Add(b);
            }
            Tab("Parámetros",paramsScroll);Tab("Presets",presetsScroll);Tab("Diagnóstico",diagnosticsScroll);Show(paramsScroll);
            toggle=new Button(Toggle){name="dev-toggle",text="DEV"};toggle.style.position=Position.Absolute;toggle.style.width=58;toggle.style.height=44;
            // Insert below ResumeOverlay. It remains the last layer and owns resume input.
            int index=root.IndexOf(root.Q("resume-overlay"));root.Insert(index<0?root.childCount:index,panel);
            index=root.IndexOf(root.Q("resume-overlay"));root.Insert(index<0?root.childCount:index,toggle);
            panel.style.display=DisplayStyle.None;root.RegisterCallback<GeometryChangedEvent>(OnGeometry);
            gesture=gameObject.AddComponent<ThreeFingerHold>();gesture.CanToggle=CanToggle;gesture.Held+=Toggle;
            Place();Refresh();
        }
        private bool CanToggle()=>app!=null&&!app.GetComponent<ResumeOverlay>().IsVisible&&!(GetComponent<SpikeSection>()?.IsRunning??false)&&!(GetComponent<PerformanceSession>()?.IsRunning??false);
        public void Toggle()
        {
            app.RecordDevControl("dev-toggle",true,"invoked","activation");
            if(!CanToggle()){app.RecordDevControl("dev-toggle",false,"dev-unavailable");return;}
            panel.style.display=IsOpen?DisplayStyle.None:DisplayStyle.Flex;
            var input=app.GetComponent<SquashClash.Runtime.Input.TouchInputAdapter>();
            if(input!=null)input.BlockedByUi=IsOpen;
            Refresh();Place();app.RecordDevControl("dev-toggle",true,"completed");
        }
        private void Refresh()
        {
            if(app==null||heading==null)return;
            heading.text=app.ActivePresetName+" · "+app.Config.Backend+"\n"+app.Tuning.Applied.Hash+" · "+(app.Clock.Paused?"Pausa":"Jugando")+"\n"+app.Tuning.PendingKeys.Count+(app.Tuning.PendingKeys.Count==1?" cambio pendiente":" cambios pendientes");
            parameters?.Refresh();presets?.Refresh();diagnostics?.Refresh();
        }
        private void OnGeometry(GeometryChangedEvent evt)=>Place();
        private void Place()
        {
            if(root?.panel==null)return;var safe=Screen.safeArea;
            var top=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(safe.xMin,Screen.height-safe.yMax));
            var bottom=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(safe.xMax,Screen.height-safe.yMin));
            toggle.style.top=top.y+6;toggle.style.right=Mathf.Max(0,root.layout.width-bottom.x)+6;
            panel.style.left=top.x+6;panel.style.right=Mathf.Max(0,root.layout.width-bottom.x)+6;
            panel.style.top=(top.y+bottom.y)*.5f;
            float barHeight=root.Q("dev-bar")?.resolvedStyle.height??112;if(float.IsNaN(barHeight))barHeight=112;
            panel.style.bottom=Mathf.Max(0,root.layout.height-bottom.y)+barHeight+6;
        }
        private void Update()
        {
            if(app==null)return;
            var available=CanToggle();toggle?.SetEnabled(available);panel?.SetEnabled(available);
            elapsed+=Time.unscaledDeltaTime;if(elapsed<.5f)return;elapsed=0;if(IsOpen)Refresh();
        }
        private void OnDestroy()
        {
            diagnostics?.Dispose();root?.UnregisterCallback<GeometryChangedEvent>(OnGeometry);panel?.RemoveFromHierarchy();toggle?.RemoveFromHierarchy();
            if(gesture!=null){gesture.Held-=Toggle;Destroy(gesture);}
        }
    }
}
