using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Shot
{
    public sealed class ShotNoteView : MonoBehaviour
    {
        private Label label;
        private VisualElement root,bar;
        public string Text=>label?.text??string.Empty;
        public void Initialize(AppBootstrap app)
        {
            root=app.GetComponent<UIDocument>().rootVisualElement;
            label=new Label{name="shot-note",pickingMode=PickingMode.Ignore};label.style.position=Position.Absolute;label.style.height=24;label.style.fontSize=14;
            label.style.color=Color.white;label.style.backgroundColor=new Color(.02f,.06f,.08f,.86f);root.Insert(0,label);
        }
        public void Show(ShotRecord r)
        {
            string text=r.Result=="Perfect"?"Perfecto":r.Result=="Valid"?(r.DeltaMs<0?"Adelantado":"Atrasado"):
                r.Result=="Failed"?(r.Fail=="Air"?"¡Al aire!":"¡Tarde!"):r.Result=="Unreachable"?"No llega":r.Result=="NoOrder"?"Sin golpe":r.Result=="NotEvaluable"?"No evaluable":"Fallo del sistema";
            ShowText(text);
        }
        public void ShowText(string text){if(label!=null)label.text=text;}
        public void SetVisible(bool visible){if(label!=null)label.style.display=visible&&!string.IsNullOrEmpty(label.text)?DisplayStyle.Flex:DisplayStyle.None;}
        private void LateUpdate()
        {
            if(root?.panel==null||label==null)return;
            if(bar==null)bar=root.Q("dev-bar");var safe=Screen.safeArea;
            var bottom=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(safe.xMin,Screen.height-safe.yMin));
            float barHeight=bar?.resolvedStyle.height??0;if(float.IsNaN(barHeight))barHeight=0;
            label.style.left=bottom.x+8;label.style.right=bottom.x+8;label.style.top=bottom.y-barHeight-84;
        }
        private void OnDestroy()=>label?.RemoveFromHierarchy();
    }
}
