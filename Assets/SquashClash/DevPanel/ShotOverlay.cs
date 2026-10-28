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
        private VisualElement root,layer,details,bar,feedback;
        private LineRenderer targetLine;private Material targetMaterial;
        private Label compact,state,power,sampling,status,timer,guide;
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
            timer=new Label {name="sling-protocol-timer",pickingMode=PickingMode.Ignore};timer.style.fontSize=14;timer.style.whiteSpace=WhiteSpace.Normal;layer.Add(timer);
            guide=new Label {name="sling-protocol-guide",pickingMode=PickingMode.Ignore};guide.style.fontSize=12;guide.style.whiteSpace=WhiteSpace.Normal;guide.style.height=48;layer.Add(guide);
            foreach(var label in new[]{compact,timer,guide})
            {label.style.flexShrink=0;label.style.marginTop=label.style.marginBottom=0;label.style.paddingTop=label.style.paddingBottom=0;}
            compact.style.height=timer.style.height=24;
            feedback=new VisualElement {name="sling-feedback",pickingMode=PickingMode.Ignore};feedback.style.flexDirection=FlexDirection.Row;
            var ok=new Button(()=>DevControlLog.Try(app,"sling-ok",()=>preview.RespondSling(false))) {name="sling-ok",text="OK"};
            var premature=new Button(()=>DevControlLog.Try(app,"sling-premature",()=>preview.RespondSling(true))) {name="sling-premature",text="Se fijó antes de tiempo"};
            foreach(var button in new[]{ok,premature}){button.style.height=44;button.style.fontSize=13;button.style.flexGrow=1;feedback.Add(button);}
            feedback.style.display=DisplayStyle.None;layer.Add(feedback);
            details=new VisualElement {name="shot-protocol"};details.style.flexShrink=0;
            state=AddLabel("shot-preview-state");power=AddLabel("shot-preview-power");sampling=AddLabel("shot-preview-sampling");status=AddLabel("shot-preview-status");
            var row=new VisualElement {pickingMode=PickingMode.Ignore};row.style.flexDirection=FlexDirection.Row;details.Add(row);
            AddButton(row,"s1-sampling","2 min",()=>preview.BeginSamplingWindow());
            AddButton(row,"s1-ruler","Regla 50 mm",()=>preview.ArmRuler());
            AddButton(row,"s1-soft","10 suaves",()=>preview.ArmFlicks(false));
            AddButton(row,"s1-strong","10 fuertes",()=>preview.ArmFlicks(true));
            var calibrationRow=new VisualElement {pickingMode=PickingMode.Ignore};calibrationRow.style.flexDirection=FlexDirection.Row;details.Add(calibrationRow);
            AddButton(calibrationRow,"s1-medium","10 medios",()=>preview.ArmSeries("medium"));
            AddButton(calibrationRow,"s1-aim-soft","10 apuntar rápido y soltar suave",()=>preview.ArmSeries("aim-soft"));
            var slingHeading=AddLabel("sling-heading");slingHeading.text="S1-sling · protocolo experimental (mano izquierda)";
            foreach(string block in new[]{"B0","B1","B2-soft","B2-medium","B2-strong","B3","B4","B5"})
            {
                var protocolRow=new VisualElement();protocolRow.style.flexDirection=FlexDirection.Row;details.Add(protocolRow);
                string captured=block;
                string title=block=="B0"?"B0 · 2 min práctica":block=="B1"?"B1 · 10 apuntar y soltar suave":block=="B2-soft"?"B2 · 10 suaves":block=="B2-medium"?"B2 · 10 medios":block=="B2-strong"?"B2 · 10 fuertes":block=="B3"?"B3 · 10 medios a objetivo bajo":block=="B4"?"B4 · 5 cancelaciones":"B5 · 10 gestos a ritmo cómodo";
                AddButton(protocolRow,"sling-"+block,title,()=>preview.ArmSlingBlock(captured));
            }
            var targetObject=new GameObject("Sling protocol target");targetObject.transform.SetParent(app.transform,false);
            targetLine=targetObject.AddComponent<LineRenderer>();targetLine.useWorldSpace=false;targetLine.loop=true;targetLine.positionCount=4;targetLine.widthMultiplier=.015f;
            targetLine.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;targetLine.receiveShadows=false;
            targetMaterial=new Material(Resources.Load<Shader>("Legibility"));targetMaterial.SetColor("_BaseColor",Color.white);targetLine.sharedMaterial=targetMaterial;
            targetLine.startColor=targetLine.endColor=Color.black;
            for(int i=0;i<4;i++){float a=i*Mathf.PI/2;targetLine.SetPosition(i,new Vector3(Mathf.Cos(a)*.10f,Mathf.Sin(a)*.10f,0));}
            targetLine.gameObject.SetActive(false);
            root.Q<ScrollView>("parameters-scroll").contentContainer.Insert(0,details);
            root.Insert(0,layer);root.RegisterCallback<GeometryChangedEvent>(OnGeometry);bar.RegisterCallback<GeometryChangedEvent>(OnGeometry);Place();Refresh();
        }
        private Label AddLabel(string name)
        {
            var label=new Label {name=name,pickingMode=PickingMode.Ignore};label.style.fontSize=12;label.style.whiteSpace=WhiteSpace.Normal;details.Add(label);return label;
        }
        private void AddButton(VisualElement row,string name,string text,Action action)
        {
            var button=new Button(()=>{app.RecordDevControl(name,true,"clicked","ui-activation");if(panel.IsOpen)panel.Toggle();if(name.StartsWith("sling-",StringComparison.Ordinal))action();else DevControlLog.Run(app,name,action);}) {name=name,text=text};button.style.flexGrow=1;button.style.flexBasis=0;button.style.minWidth=0;button.style.height=44;
            button.style.fontSize=12;button.style.whiteSpace=WhiteSpace.Normal;row.Add(button);
        }
        private static string F(double n,string format="F1")=>double.IsNaN(n)||double.IsInfinity(n)?"?":n.ToString(format,System.Globalization.CultureInfo.InvariantCulture);
        private void Refresh()
        {
            var model=preview.Model;var p=preview.Power;var s=input.Stats;
            string aimState=reticle.IsInvalid?"Inválida":reticle.State==SquashClash.Core.Shot.AimState.Frozen?"Fijada":"Apuntando";
            bool sling=preview.Active?preview.Sling!=null:app.Tuning.Applied.GetInt("gesture.control")==1;
            var trials=preview.SlingTrials;
            bool showProtocol=trials.AwaitingFeedback||sling&&(trials.Running||preview.HasSlingProtocolStatus);
            bool notice=preview.SlingArmRejection.Length>0;
            feedback.style.display=showProtocol&&trials.AwaitingFeedback?DisplayStyle.Flex:DisplayStyle.None;
            layer.style.display=DisplayStyle.Flex;
            timer.style.display=showProtocol&&!notice?DisplayStyle.Flex:DisplayStyle.None;timer.text=preview.ProtocolTimingText;
            guide.style.display=notice||showProtocol&&preview.SlingGuidanceText.Length>0?DisplayStyle.Flex:DisplayStyle.None;guide.text=notice?preview.SlingArmRejection:preview.SlingGuidanceText;
            layer.style.height=ProtocolHeight(showProtocol);
            if(notice)compact.text="No se inició el bloque";
            else if(showProtocol)
            {
                string requested=trials.RequestedPower=="soft"?"suave":trials.RequestedPower=="medium"?"media":trials.RequestedPower=="strong"?"fuerte":trials.RequestedPower=="cancel"?"cancelar":"práctica";
                compact.text=trials.Block=="B0"?"B0 · práctica guiada":trials.AwaitingFeedback?$"{trials.Block} · intento {trials.PendingTrial}: responde":$"{trials.Block} · {trials.Completed+1} · T{trials.TargetId} · {requested}";
            }
            else compact.text=sling?preview.SlingReadinessText:$"{aimState} · P {p.P*100:F0}%";
            bool targetShown=showProtocol&&trials.TargetId>0;
            targetLine.gameObject.SetActive(targetShown);
            if(targetShown){var point=preview.ProtocolTarget;targetLine.transform.position=new Vector3(point.x,point.y,.045f);}
            Place();
            if(!panel.IsOpen)return;
            state.text=preview.Sling!=null?$"S1-sling · {preview.Sling.Phase} · {preview.Outcome} · d {preview.Sling.DistanceMm:F1} mm · max {preview.Sling.MaxDistanceMm:F1} mm · {preview.Sling.Region}":model==null?"S1 · mira esperando gesto":$"Mira ({model.Aim.x:F2}, {model.Aim.y:F2}) m · {model.State} · {preview.Outcome} · salto {model.FreezeJumpMeters:F3} m";
            power.text=$"P {p.P*100:F0}% · {p.PowerSpeedMmPerS:F0} mm/s ({p.SpeedBasis}) · soporte {p.CoveredMs:F1} ms · n {p.DistinctSamples} · hueco {p.MaxGapMs:F1} ms · {(p.Conservative?"conservadora ":"")}{p.Reason}\nDPI {Screen.dpi:F0} · {preview.DensitySource} · {preview.MmPerPx:F5} mm/px · sin verificar con regla";
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
            bool sling=app.Tuning.Applied.GetInt("gesture.control")==1||preview.SlingTrials.AwaitingFeedback;
            float lineHeight=ProtocolHeight(sling&&(preview.HasSlingProtocolStatus||preview.SlingTrials.Running)||preview.SlingTrials.AwaitingFeedback);
            layer.style.top=Mathf.Max(safeTop,Mathf.Min(safeBottom,barTop)-(sling?30+lineHeight:24)-6);
        }
        private float ProtocolHeight(bool shown)=>preview.SlingArmRejection.Length>0?72+(preview.SlingTrials.AwaitingFeedback?48:0):!shown?24:24+24+(preview.SlingGuidanceText.Length>0?48:0)+(preview.SlingTrials.AwaitingFeedback?48:0);
        private void OnGeometry(GeometryChangedEvent e)=>Place();
        private void Place()
        {
            PlaceInSafeArea(Screen.safeArea,new Vector2(Screen.width,Screen.height));
        }
        private void OnDestroy(){root?.UnregisterCallback<GeometryChangedEvent>(OnGeometry);bar?.UnregisterCallback<GeometryChangedEvent>(OnGeometry);layer?.RemoveFromHierarchy();details?.RemoveFromHierarchy();if(targetLine!=null)Destroy(targetLine.gameObject);if(targetMaterial!=null)Destroy(targetMaterial);}
    }
}
