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
        private VisualElement root,layer;
        private Label state,power,sampling,status;
        private float elapsed;
        public void Initialize(AppBootstrap app,VisualElement root)
        {
            this.app=app;this.root=root;preview=app.GetComponent<GesturePreview>();input=app.GetComponent<TouchInputAdapter>();
            layer=new VisualElement {name="shot-overlay",pickingMode=PickingMode.Ignore};
            layer.style.position=Position.Absolute;layer.style.left=8;layer.style.right=8;layer.style.top=130;
            layer.style.backgroundColor=new Color(.02f,.06f,.08f,.86f);layer.style.color=Color.white;layer.style.paddingLeft=8;layer.style.paddingRight=8;
            state=AddLabel("shot-preview-state");power=AddLabel("shot-preview-power");sampling=AddLabel("shot-preview-sampling");status=AddLabel("shot-preview-status");
            var row=new VisualElement {pickingMode=PickingMode.Ignore};row.style.flexDirection=FlexDirection.Row;layer.Add(row);
            AddButton(row,"s1-sampling","2 min",()=>preview.BeginSamplingWindow());
            AddButton(row,"s1-ruler","Regla 50 mm",()=>preview.ArmRuler());
            AddButton(row,"s1-soft","10 suaves",()=>preview.ArmFlicks(false));
            AddButton(row,"s1-strong","10 fuertes",()=>preview.ArmFlicks(true));
            root.Insert(0,layer);root.RegisterCallback<GeometryChangedEvent>(OnGeometry);Place();Refresh();
        }
        private Label AddLabel(string name)
        {
            var label=new Label {name=name,pickingMode=PickingMode.Ignore};label.style.fontSize=12;label.style.whiteSpace=WhiteSpace.Normal;layer.Add(label);return label;
        }
        private void AddButton(VisualElement row,string name,string text,Action action)
        {
            var button=new Button(action) {name=name,text=text};button.style.flexGrow=1;button.style.flexBasis=0;button.style.minWidth=0;button.style.height=44;
            button.style.fontSize=12;button.style.whiteSpace=WhiteSpace.Normal;row.Add(button);
        }
        private static string F(double n,string format="F1")=>double.IsNaN(n)||double.IsInfinity(n)?"?":n.ToString(format,System.Globalization.CultureInfo.InvariantCulture);
        private void Refresh()
        {
            var model=preview.Model;var p=preview.Power;var s=input.Stats;
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
        private void OnGeometry(GeometryChangedEvent e)=>Place();
        private void Place()
        {
            if(root?.panel==null)return;
            var safe=Screen.safeArea;var top=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(safe.xMin,Screen.height-safe.yMax));
            var right=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(safe.xMax,Screen.height-safe.yMax));
            layer.style.left=top.x+8;layer.style.right=Mathf.Max(0,root.layout.width-right.x)+8;layer.style.top=top.y+130;
        }
        private void OnDestroy(){root?.UnregisterCallback<GeometryChangedEvent>(OnGeometry);layer?.RemoveFromHierarchy();}
    }
}
