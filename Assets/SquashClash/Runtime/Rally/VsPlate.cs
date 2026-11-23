using System;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Rally
{
    // «vs» plate (spec front 4, 2026-11-22 rev 2). The clock of the plate: real and unscaled, replaceable by tests. Its age is Now()-shownAt, never a sum of
    // the HUD's clipped dt (RallyHud.Update caps it at 0.05 s), so a long frame cannot stretch it. PointEndFreeze.Now is the precedent.
    public static class VsPlate
    {
        public static Func<double> Now=()=>Time.unscaledTimeAsDouble;
        public static void Reset(){Now=()=>Time.unscaledTimeAsDouble;}
    }
    // The plate is a pure view: it does not pause the game clock, delay the serve, or read or write any game state. It stands over the flight of the
    // first serve of a match started from the menu, on the empty band under the court. Concept «the centre line»: the two halves slide in from opposite
    // edges and meet on a red vertical line, the court's own centre line; that line is the «vs». It closes at whichever comes first:
    // tapped (the layer is touched), timeout (age reaches the total), phase (RivalReception ends) or pause. The first three cut without an exit animation.
    public sealed class VsPlateView
    {
        public const float Height=64,Margin=8,BandMargin=12,Fallback=6;
        private const double Total=RallyText.VsInSeconds+RallyText.VsHoldSeconds+RallyText.VsOutSeconds;
        private readonly AppBootstrap app;private readonly RallyDirector rally;private readonly VisualElement header;
        private readonly VisualElement layer,plate,left,right,line;private readonly Label you,mid,rivalTag,style;
        private SafeAreaLayout safe;private ShotNoteView noteView;
        private bool shown;private double shownAt;
        public bool Visible=>shown;
        public VsPlateView(AppBootstrap app,RallyDirector rally,VisualElement top,VisualElement header)
        {
            this.app=app;this.rally=rally;this.header=header;safe=app.GetComponent<SafeAreaLayout>();
            // The layer takes every touch under the header: a touch on the plate skips it and never opens a gesture (GesturePreview.OverUi knows its name).
            layer=new VisualElement{name="vs-plate-layer",pickingMode=PickingMode.Position};layer.AddToClassList("sc-vs-layer");layer.style.display=DisplayStyle.None;
            layer.RegisterCallback<PointerDownEvent>(_=>Close("tapped"));
            plate=new VisualElement{name="vs-plate",pickingMode=PickingMode.Ignore};plate.AddToClassList("sc-vs-plate");layer.Add(plate);
            left=new VisualElement{name="vs-plate-left",pickingMode=PickingMode.Ignore};left.AddToClassList("sc-vs-half");left.AddToClassList("sc-vs-half--human");plate.Add(left);
            right=new VisualElement{name="vs-plate-right",pickingMode=PickingMode.Ignore};right.AddToClassList("sc-vs-half");right.AddToClassList("sc-vs-half--rival");plate.Add(right);
            you=Text("TÚ","sc-vs-you","vs-plate-you");left.Add(you);
            mid=Text("vs","sc-vs-mid","vs-plate-mid");left.Add(mid);
            rivalTag=Text("RIVAL","sc-vs-rival","vs-plate-rival");right.Add(rivalTag);
            style=Text("","sc-vs-style","vs-plate-style");right.Add(style);
            line=new VisualElement{name="vs-plate-line",pickingMode=PickingMode.Ignore};line.AddToClassList("sc-vs-line");plate.Add(line);
            top.Add(layer);
            var input=app.GetComponent<TouchInputAdapter>();if(input!=null)input.SampleFiltered+=OnTouchFiltered;
        }
        private static Label Text(string text,string cls,string name){var label=new Label(text){name=name,pickingMode=PickingMode.Ignore};label.AddToClassList(cls);return label;}
        public void Show(RivalStyleId rivalStyle)
        {
            if(shown)return;
            shown=true;shownAt=VsPlate.Now();style.text=RallyText.VsStyle(rivalStyle);
            layer.style.display=DisplayStyle.Flex;Place();Animate(0);
            app.UserEvents?.Menu("vs","shown");
        }
        // One closing line per showing, with its own reason; a second call (a tap that also arrives as a pointer event) does nothing.
        public void Close(string reason)
        {
            if(!shown)return;
            shown=false;layer.style.display=DisplayStyle.None;
            app.UserEvents?.Menu("vs",reason);
        }
        // Every frame while shown: the three automatic causes, then the animation and the place.
        public void Update()
        {
            if(!shown)return;
            if(app.Clock==null||app.Clock.Paused){Close("pause");return;}
            if(!rally.Active||rally.Phase!=RallyPhase.RivalReception){Close("phase");return;}
            double age=VsPlate.Now()-shownAt;
            if(age>=Total){Close("timeout");return;}
            Place();Animate(age);
        }
        // The touch filter's side: a finger that lands on the layer is suppressed by OverUi and arrives here as filtered; that is the tap.
        public void OnTouchFiltered(TouchSample sample,string reason)
        {
            if(!shown||sample.Phase!=TouchPhaseKind.Began||reason!="suppressed-ui-or-contact"||layer.panel==null)return;
            var point=RuntimePanelUtils.ScreenToPanel(layer.panel,new Vector2(sample.PositionPx.x,Screen.height-sample.PositionPx.y));
            for(var picked=layer.panel.Pick(point);picked!=null;picked=picked.parent)
                if(picked==layer){Close("tapped");return;}
        }
        private static float Ease(float t){t=Mathf.Clamp01(t);return t*t*(3-2*t);}
        // Halves in from opposite edges (smoothstep), the line grows from its middle; the exit is a fade. During the hold everything is at rest.
        private void Animate(double age)
        {
            float k=Ease((float)(age/RallyText.VsInSeconds));
            left.style.translate=new Translate(Length.Percent(-(1-k)*100),0);right.style.translate=new Translate(Length.Percent((1-k)*100),0);
            line.style.scale=new Scale(new Vector3(1,k,1));
            double outAge=age-RallyText.VsInSeconds-RallyText.VsHoldSeconds;
            plate.style.opacity=outAge<=0?1f:1f-Ease((float)(outAge/RallyText.VsOutSeconds));
        }
        // Under the header, and on the empty band under the court that ShotNoteView.CourtBottomEdge measures (the same one the note and the serve hint use);
        // a band shorter than the plate (plus 12) leaves it resting on the safe bottom edge, like the note.
        private void Place()
        {
            if(safe==null)safe=app.GetComponent<SafeAreaLayout>();
            var panel=layer.parent;float panelHeight=panel!=null?panel.layout.height:float.NaN;
            float headerBottom=header!=null&&panel!=null&&!float.IsNaN(header.worldBound.yMax)?header.worldBound.yMax-panel.worldBound.yMin:0;
            layer.style.left=0;layer.style.right=0;layer.style.bottom=0;layer.style.top=headerBottom;
            float sl=safe!=null?safe.Left:0,sr=safe!=null?safe.Right:0,sb=safe!=null?safe.Bottom:0;
            if(noteView==null)noteView=app.GetComponent<ShotController>()?.GetComponent<ShotNoteView>();
            float floor=noteView!=null&&noteView.Floor>0?noteView.Floor:sb;
            float edge=noteView!=null?noteView.CourtBottomEdge():float.NaN;
            float bottom=float.IsNaN(edge)||edge-floor<Height+BandMargin?floor+Fallback:floor+(edge-floor-Height)/2;
            if(!float.IsNaN(panelHeight)&&panelHeight>0)bottom=Mathf.Min(bottom,Mathf.Max(floor,panelHeight-headerBottom-Height));
            plate.style.left=sl+Margin;plate.style.right=sr+Margin;plate.style.height=Height;plate.style.bottom=bottom;
        }
    }
}
