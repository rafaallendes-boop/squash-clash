using System;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.UIElements;

namespace SquashClash.DevPanel
{
    // Diagnostic controls are excluded from prod by this assembly's define constraint.
    public sealed class ShotOverlay : MonoBehaviour
    {
        private AppBootstrap app;
        private GesturePreview preview;
        private TouchInputAdapter input;
        private VisualElement root,layer,details,bar;
        private Label compact,state,power,sampling,status;
        private DevPanelView panel;
        private AimReticleView reticle;
        private float elapsed;
        public void Initialize(AppBootstrap app,VisualElement root)
        {
            this.app=app;this.root=root;preview=app.GetComponent<GesturePreview>();input=app.GetComponent<TouchInputAdapter>();
            panel=GetComponent<DevPanelView>();reticle=app.GetComponentInChildren<AimReticleView>(true);bar=root.Q("dev-bar");
            layer=new VisualElement {name="shot-overlay",pickingMode=PickingMode.Ignore};
            layer.style.position=Position.Absolute;layer.style.height=24;
            layer.style.backgroundColor=new Color(.02f,.06f,.08f,.86f);layer.style.color=Color.white;layer.style.paddingLeft=8;layer.style.paddingRight=8;
            compact=new Label {name="shot-compact",pickingMode=PickingMode.Ignore};compact.style.fontSize=14;compact.style.whiteSpace=WhiteSpace.NoWrap;layer.Add(compact);
            details=new VisualElement {name="shot-protocol"};details.style.flexShrink=0;
            state=AddLabel("shot-preview-state");power=AddLabel("shot-preview-power");sampling=AddLabel("shot-preview-sampling");status=AddLabel("shot-preview-status");
            var row=new VisualElement {pickingMode=PickingMode.Ignore};row.style.flexDirection=FlexDirection.Row;details.Add(row);
            AddButton(row,"s1-sampling","2 min",()=>preview.BeginSamplingWindow());
            AddButton(row,"s1-ruler","Regla 50 mm",()=>preview.ArmRuler());
            AddButton(row,"s1-soft","10 suaves",()=>preview.ArmFlicks(false));
            AddButton(row,"s1-strong","10 fuertes",()=>preview.ArmFlicks(true));
            root.Q<ScrollView>("parameters-scroll").contentContainer.Insert(0,details);
            root.Insert(0,layer);root.RegisterCallback<GeometryChangedEvent>(OnGeometry);bar.RegisterCallback<GeometryChangedEvent>(OnGeometry);Place();Refresh();
        }
        private Label AddLabel(string name)
        {
            var label=new Label {name=name,pickingMode=PickingMode.Ignore};label.style.fontSize=12;label.style.whiteSpace=WhiteSpace.Normal;details.Add(label);return label;
        }
        private void AddButton(VisualElement row,string name,string text,Action action)
        {
            var button=new Button(()=>{if(panel.IsOpen)panel.Toggle();action();}) {name=name,text=text};button.style.flexGrow=1;button.style.flexBasis=0;button.style.minWidth=0;button.style.height=44;
            button.style.fontSize=12;button.style.whiteSpace=WhiteSpace.Normal;row.Add(button);
        }
        private static string F(double n,string format="F1")=>double.IsNaN(n)||double.IsInfinity(n)?"?":n.ToString(format,System.Globalization.CultureInfo.InvariantCulture);
        private void Refresh()
        {
            var model=preview.Model;var p=preview.Power;var s=input.Stats;
            string aimState=reticle.IsInvalid?"Inválida":reticle.State==SquashClash.Core.Shot.AimState.Frozen?"Fijada":"Apuntando";
            compact.text=$"{aimState} · P {p.P*100:F0}%";
            if(!panel.IsOpen)return;
            state.text=model==null?"S1 · mira esperando gesto":$"Mira ({model.Aim.x:F2}, {model.Aim.y:F2}) m · {model.State} · {preview.Outcome} · salto {model.FreezeJumpMeters:F3} m";
            power.text=$"P {p.P*100:F0}% · {p.NetSpeedMmPerS:F0} mm/s · cobertura {p.CoveredMs:F1} ms · n {p.DistinctSamples} · hueco {p.MaxGapMs:F1} ms · {(p.Conservative?"conservadora ":"")}{p.Reason}\nDPI {Screen.dpi:F0} · {preview.DensitySource} · {preview.MmPerPx:F5} mm/px · sin verificar con regla";
            sampling.text=$"Eventos {s.Samples} · {F(s.DeliveredHz)} Hz entregados · Δ p50/p95 {F(s.IntervalP50Ms)}/{F(s.IntervalP95Ms)} ms · máx {F(s.MaxGapMs)} ms\nEdad p50/p95/máx {F(s.AgeP50Ms)}/{F(s.AgeP95Ms)}/{F(s.MaxAgeMs)} ms · edad<0 {s.NegativeAges} · dup {s.DuplicateTimes} · desorden {s.OutOfOrder} · máx/frame {s.MaxSamplesPerFrame}";
            status.text=preview.Status+" · serie "+preview.TrialCount+"/10";
        }
        private void Update()
        {
            if(app==null)return;
            elapsed+=Time.unscaledDeltaTime;if(elapsed<.1f)return;elapsed=0;Refresh();
        }
        public void PlaceInSafeArea(Rect safeArea,Vector2 screenSize)
        {
            if(root?.panel==null||screenSize.x<=0||screenSize.y<=0)return;
            float width=root.layout.width,height=root.layout.height;
            float safeTop=(screenSize.y-safeArea.yMax)/screenSize.y*height;
            float safeBottom=(screenSize.y-safeArea.yMin)/screenSize.y*height;
            float barTop=bar.worldBound.yMin-root.worldBound.yMin;
            layer.style.left=safeArea.xMin/screenSize.x*width+8;
            layer.style.right=(screenSize.x-safeArea.xMax)/screenSize.x*width+8;
            layer.style.top=Mathf.Max(safeTop,Mathf.Min(safeBottom,barTop)-24-6);
        }
        private void OnGeometry(GeometryChangedEvent e)=>Place();
        private void Place()
        {
            PlaceInSafeArea(Screen.safeArea,new Vector2(Screen.width,Screen.height));
        }
        private void OnDestroy(){root?.UnregisterCallback<GeometryChangedEvent>(OnGeometry);bar?.UnregisterCallback<GeometryChangedEvent>(OnGeometry);layer?.RemoveFromHierarchy();details?.RemoveFromHierarchy();}
    }
}
