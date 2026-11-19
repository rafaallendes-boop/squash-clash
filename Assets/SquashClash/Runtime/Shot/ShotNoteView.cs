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
        // Distance from the safe bottom edge (plus the developer bar, when there is one) to the note's lower edge.
        private const float Above=55,Side=8;
        private static readonly string[] Tones={"perfect","valid","failed","info"};
        private Label label;
        private VisualElement root,bar;private SafeAreaLayout safe;
        private int tone=-1;private float popAge=-1;
        public string Text=>label?.text??string.Empty;
        public void Initialize(AppBootstrap app)
        {
            root=app.GetComponent<UIDocument>().rootVisualElement;safe=app.GetComponent<SafeAreaLayout>();
            label=new Label{name="shot-note",pickingMode=PickingMode.Ignore};label.AddToClassList("sc-shot-note");label.style.position=Position.Absolute;root.Insert(0,label);
        }
        public void Show(ShotRecord r)
        {
            string text=r.Result=="Perfect"?"Perfecto":r.Result=="Valid"?(r.DeltaMs<0?"Adelantado":"Atrasado"):
                r.Result=="Failed"?(r.Fail=="Air"?"¡Al aire!":"¡Tarde!"):r.Result=="Unreachable"?"No llega":r.Result=="NoOrder"?"Sin golpe":r.Result=="NotEvaluable"?"No evaluable":"Fallo del sistema";
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
            float barHeight=bar?.resolvedStyle.height??0;if(float.IsNaN(barHeight))barHeight=0;
            // In panel units from the safe area (SafeAreaLayout), so it sits where it should on any phone and in captures.
            float left=safe!=null?safe.Left:0,right=safe!=null?safe.Right:0,bottom=safe!=null?safe.Bottom:0;
            label.style.left=left+Side;label.style.right=right+Side;label.style.bottom=bottom+barHeight+Above;
        }
        private void OnDestroy()=>label?.RemoveFromHierarchy();
    }
}
