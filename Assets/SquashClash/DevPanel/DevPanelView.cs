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
            panel.style.fontSize=15;
            heading=new Label {name="pending-changes"};heading.style.whiteSpace=WhiteSpace.Normal;panel.Add(heading);
            var apply=new Button(()=>{app.LaunchCurrent();Refresh();}){text="Aplicar y relanzar",name="apply-relaunch"};apply.style.minHeight=44;panel.Add(apply);
            var tabs=new VisualElement();tabs.style.flexDirection=FlexDirection.Row;panel.Add(tabs);
            var paramsScroll=new ScrollView {name="parameters-scroll"};var presetsScroll=new ScrollView();var diagnosticsScroll=new ScrollView();
            foreach(var scroll in new[]{paramsScroll,presetsScroll,diagnosticsScroll}){scroll.style.flexGrow=1;panel.Add(scroll);}
            parameters=new ParamSection(app,Refresh);presets=new PresetsSection(app,Refresh);diagnostics=new DiagnosticsSection(app);
            paramsScroll.Add(parameters.Root);presetsScroll.Add(presets.Root);diagnosticsScroll.Add(diagnostics.Root);
            void Show(ScrollView selected){foreach(var scroll in new[]{paramsScroll,presetsScroll,diagnosticsScroll})scroll.style.display=scroll==selected?DisplayStyle.Flex:DisplayStyle.None;}
            void Tab(string text,ScrollView scroll){var b=new Button(()=>{Show(scroll);Refresh();}){text=text};b.style.flexGrow=1;b.style.minHeight=44;tabs.Add(b);}
            Tab("Parámetros",paramsScroll);Tab("Presets",presetsScroll);Tab("Diagnóstico",diagnosticsScroll);Show(paramsScroll);
            toggle=new Button(Toggle){name="dev-toggle",text="DEV"};toggle.style.position=Position.Absolute;toggle.style.width=58;toggle.style.height=44;
            // Insert below ResumeOverlay. It remains the last layer and owns resume input.
            int index=root.IndexOf(root.Q("resume-overlay"));root.Insert(index<0?root.childCount:index,panel);
            index=root.IndexOf(root.Q("resume-overlay"));root.Insert(index<0?root.childCount:index,toggle);
            panel.style.display=DisplayStyle.None;root.RegisterCallback<GeometryChangedEvent>(OnGeometry);
            gesture=gameObject.AddComponent<ThreeFingerHold>();gesture.CanToggle=CanToggle;gesture.Held+=Toggle;
            Place();Refresh();
        }
        private bool CanToggle()=>app!=null&&!app.GetComponent<ResumeOverlay>().IsVisible&&!(GetComponent<SpikeSection>()?.IsRunning??false);
        public void Toggle(){if(!CanToggle())return;panel.style.display=IsOpen?DisplayStyle.None:DisplayStyle.Flex;Refresh();Place();}
        private void Refresh()
        {
            if(app==null||heading==null)return;
            heading.text=app.ActivePresetName+" · "+app.Config.Backend+"\n"+app.Tuning.PendingKeys.Count+(app.Tuning.PendingKeys.Count==1?" cambio pendiente":" cambios pendientes");
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
