using SquashClash.Runtime.App;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Volley v1 on or off from the NEXT Rally/Match point, to compare with and without it.
    public sealed class VolleyTestSection
    {
        public VisualElement Root {get;}=new Foldout{name="volley-test-section",text="Volea",value=true};
        public VolleyTestSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            var help=new Label("Activa o desactiva la volea desde el PRÓXIMO punto de Rally/Partido.");
            help.style.whiteSpace=WhiteSpace.Normal;Root.Add(help);
            var row=new VisualElement();row.style.flexDirection=FlexDirection.Row;row.style.flexShrink=0;Root.Add(row);
            Add(app,row,"volley-on","Volea sí",()=>app.Rally.SetVolley(true));Add(app,row,"volley-off","Volea no",()=>app.Rally.SetVolley(false));
            // Volley v2 (design 2026-11-20): chosen by touch (v2) or v1, and the human's return-of-serve volley, to compare.
            var v2=new VisualElement();v2.style.flexDirection=FlexDirection.Row;v2.style.flexShrink=0;Root.Add(v2);
            Add(app,v2,"volley-v2-on","Volea v2",()=>app.Rally.SetVolleyV2(true));Add(app,v2,"volley-v2-off","Volea v1",()=>app.Rally.SetVolleyV2(false));
            var serve=new VisualElement();serve.style.flexDirection=FlexDirection.Row;serve.style.flexShrink=0;Root.Add(serve);
            Add(app,serve,"serve-volley-on","Volea al saque sí",()=>app.Rally.SetServeVolley(true));Add(app,serve,"serve-volley-off","Volea al saque no",()=>app.Rally.SetServeVolley(false));
        }
        private static void Add(AppBootstrap app,VisualElement row,string name,string text,System.Action action)
        {
            var button=new Button(()=>DevControlLog.Run(app,name,action)){name=name,text=text};
            button.style.minHeight=44;button.style.flexGrow=1;button.style.flexBasis=0;button.style.minWidth=0;button.style.whiteSpace=WhiteSpace.Normal;row.Add(button);
        }
    }
}
