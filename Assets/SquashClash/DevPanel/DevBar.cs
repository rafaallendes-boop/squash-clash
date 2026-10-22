using System.Collections;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    public sealed class DevBar : MonoBehaviour
    {
        private AppBootstrap app;
        private VisualElement bar, root;
        private Button pause;
        private IEnumerator Start()
        {
            while (AppBootstrap.Instance == null || AppBootstrap.Instance.Host == null || AppBootstrap.Instance.Host.ActiveConfig == null) yield return null;
            app = AppBootstrap.Instance;
            root = app.GetComponent<UIDocument>().rootVisualElement;
            bar = new VisualElement { name="dev-bar" };
            bar.style.position=Position.Absolute; bar.style.flexDirection=FlexDirection.Row;
            bar.style.paddingTop=6; bar.style.paddingBottom=6; bar.style.backgroundColor=new Color(0.02f,0.06f,0.08f,0.95f);
            Add("launch","Lanzar",()=>app.Host.Launch(EffectiveConfig.From(app.Params)));
            pause=Add("pause","Pausa",()=>app.Host.Clock.Paused=!app.Host.Clock.Paused);
            Add("step","Paso",()=>{app.Host.Clock.Paused=true;app.Host.Clock.RequestSingleStep();});
            Add("replay","Repetir",()=>app.Host.Replay());
            // Insert below the runtime resume overlay so it blocks all controls.
            root.Insert(0,bar);root.RegisterCallback<GeometryChangedEvent>(_=>PlaceInSafeArea());PlaceInSafeArea();
        }
        private Button Add(string name,string text,System.Action action)
        {
            var button=new Button(action) {name=name,text=text};
            button.style.flexGrow=1;button.style.flexBasis=0;button.style.height=48;button.style.fontSize=15;
            bar.Add(button);return button;
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
            if(pause!=null)pause.text=app.Host.Clock.Paused?"Continuar":"Pausa";
        }
        private void OnDestroy() => bar?.RemoveFromHierarchy();
    }
}
