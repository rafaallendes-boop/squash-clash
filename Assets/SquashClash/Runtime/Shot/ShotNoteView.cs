using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Shot
{
    // The judged stroke in a word, under the court. Same words and same moments as before; the look pass (18 nov) only
    // gave it a tone per result (Perfecto yellow like the ball's dots, Válido white, Fallido coral, informative notes
    // smaller) and a short pop when a stroke is judged. Its style lives in Visual/Hud.uss (.sc-shot-note).
    public sealed class ShotNoteView : MonoBehaviour
    {
        public const float PopSeconds=.22f,PopScale=1.3f;
        // Side margin, and the gap above the safe bottom edge (plus a visible developer bar) used when the band under the court is too short.
        private const float Side=8,Fallback=6;
        private static readonly string[] Tones={"perfect","valid","failed","info"};
        private Label label;
        private VisualElement root,bar;private SafeAreaLayout safe;private AppBootstrap app;private SquashClash.Core.Config.EffectiveConfig courtOf;private SquashClash.Core.Court.CourtDimensions court;
        private int tone=-1;private float popAge=-1;
        public string Text=>label?.text??string.Empty;
        public void Initialize(AppBootstrap app)
        {
            this.app=app;root=app.GetComponent<UIDocument>().rootVisualElement;safe=app.GetComponent<SafeAreaLayout>();
            label=new Label{name="shot-note",pickingMode=PickingMode.Ignore};label.AddToClassList("sc-shot-note");label.style.position=Position.Absolute;root.Insert(0,label);
        }
        // A volley reads the same words: since 19 nov (PO) it gets a figure at the contact (VolleyBurstView), not a «Volea · » prefix.
        public void Show(ShotRecord r)
        {
            string text=r.Result=="Perfect"?"Perfecto":r.Result=="Valid"?(r.DeltaMs<0?"Adelantado":"Atrasado"):
                r.Result=="Failed"?(r.Fail=="Air"?"¡Temprano!":"¡Tarde!"):r.Result=="Unreachable"?"No llega":r.Result=="NoOrder"?"Sin golpe":r.Result=="NotEvaluable"?"No evaluable":"Fallo del sistema";
            Present(text,r.Result=="Perfect"?0:r.Result=="Valid"?1:r.Result=="Failed"?2:3);
        }
        public void ShowText(string text)=>Present(text,3);
        private void Present(string text,int kind)
        {
            if(label==null)return;
            label.text=text;
            if(kind!=tone){tone=kind;for(int i=0;i<Tones.Length;i++)label.EnableInClassList("sc-shot-note--"+Tones[i],i==kind);}
            // A judged stroke pops once; an informative note just appears.
            popAge=kind<3?0:-1;SetScale(kind<3?PopScale:1);
        }
        private void SetScale(float s)=>label.style.scale=new Scale(new Vector3(s,s,1));
        // Eases the pop back to the note's own size. Pure presentation: it reads no clock of the game.
        public void Tick(float dt)
        {
            if(label==null||popAge<0)return;
            popAge+=Mathf.Max(0,dt);float k=Mathf.Clamp01(popAge/PopSeconds),rest=(1-k)*(1-k);
            SetScale(1+(PopScale-1)*rest);
            if(k>=1)popAge=-1;
        }
        public void SetVisible(bool visible){if(label!=null)label.style.display=visible&&!string.IsNullOrEmpty(label.text)?DisplayStyle.Flex:DisplayStyle.None;}
        private void LateUpdate()
        {
            if(root?.panel==null||label==null)return;
            Tick(Time.unscaledDeltaTime);
            if(bar==null)bar=root.Q("dev-bar");
            // A hidden DEV bar (closed panel) takes no room; a visible one is kept clear.
            float barHeight=0;
            if(bar!=null&&bar.resolvedStyle.display!=DisplayStyle.None&&bar.resolvedStyle.visibility==Visibility.Visible){barHeight=bar.resolvedStyle.height;if(float.IsNaN(barHeight))barHeight=0;}
            // In panel units from the safe area (SafeAreaLayout), so it sits where it should on any phone and in captures.
            float left=safe!=null?safe.Left:0,right=safe!=null?safe.Right:0,bottom=safe!=null?safe.Bottom:0;
            label.style.left=left+Side;label.style.right=right+Side;
            float h=label.resolvedStyle.height;if(float.IsNaN(h)||h<=0)h=34;
            label.style.bottom=BandBottom(bottom+barHeight,h);
        }
        // Where the court's near floor edge falls on screen, as panel units up from the panel's bottom (NaN when it cannot be known).
        public float CourtBottomEdge()
        {
            var cam=UnityEngine.Camera.main;float panelH=root!=null?root.layout.height:float.NaN;
            if(cam==null||app?.Config==null||float.IsNaN(panelH)||panelH<=0||Screen.height<=0)return float.NaN;
            if(!ReferenceEquals(courtOf,app.Config)){courtOf=app.Config;court=courtOf.Court;}var c=court;
            float a=cam.WorldToScreenPoint(new Vector3(0,0,c.Length)).y,b=cam.WorldToScreenPoint(new Vector3(c.Width,0,c.Length)).y;
            return Mathf.Max(0,Mathf.Min(a,b))/Screen.height*panelH;
        }
        // The note is centred in the empty band between the court's bottom edge and the safe bottom (plus a visible DEV bar);
        // if the band cannot hold it, it sits just above that floor.
        private float BandBottom(float floor,float h)
        {
            float edge=CourtBottomEdge();
            if(float.IsNaN(edge)||edge-floor<h+2*Fallback)return floor+Fallback;
            return floor+(edge-floor-h)/2;
        }
        private void OnDestroy()=>label?.RemoveFromHierarchy();
    }
}
