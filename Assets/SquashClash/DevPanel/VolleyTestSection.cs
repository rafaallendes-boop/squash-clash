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
            Add(app,row,"volley-on","Volea sí",true);Add(app,row,"volley-off","Volea no",false);
        }
        private static void Add(AppBootstrap app,VisualElement row,string name,string text,bool on)
        {
            var button=new Button(()=>DevControlLog.Run(app,name,()=>app.Rally.SetVolley(on))){name=name,text=text};
            button.style.minHeight=44;button.style.flexGrow=1;button.style.flexBasis=0;button.style.minWidth=0;button.style.whiteSpace=WhiteSpace.Normal;row.Add(button);
        }
    }
}
