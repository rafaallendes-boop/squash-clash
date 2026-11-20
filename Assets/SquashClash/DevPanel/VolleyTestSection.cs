using SquashClash.Runtime.App;
using SquashClash.Runtime.Visual;
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
            Add(app,v2,"volley-mode-1","Volea v1",()=>app.Rally.SetVolleyMode(1));Add(app,v2,"volley-mode-2","Volea v2",()=>app.Rally.SetVolleyMode(2));
            Add(app,v2,"volley-mode-3","Volea v2.2",()=>app.Rally.SetVolleyMode(3));
            var serve=new VisualElement();serve.style.flexDirection=FlexDirection.Row;serve.style.flexShrink=0;Root.Add(serve);
            Add(app,serve,"serve-volley-on","Volea al saque sí",()=>app.Rally.SetServeVolley(true));Add(app,serve,"serve-volley-off","Volea al saque no",()=>app.Rally.SetServeVolley(false));
            // «Pose de volea del rival» (2026-11-20): cosmetic, default on, takes effect at the rival's next swing.
            var pose=new VisualElement();pose.style.flexDirection=FlexDirection.Row;pose.style.flexShrink=0;Root.Add(pose);
            Button toggle=null;
            toggle=Add(app,pose,"rival-volley-pose","",()=>{RivalVolleyPose.Enabled=!RivalVolleyPose.Enabled;toggle.text=PoseText();});
            toggle.text=PoseText();
        }
        private static string PoseText()=>"Pose de volea del rival: "+(RivalVolleyPose.Enabled?"sí":"no");
        private static Button Add(AppBootstrap app,VisualElement row,string name,string text,System.Action action)
        {
            var button=new Button(()=>DevControlLog.Run(app,name,action)){name=name,text=text};
            button.style.minHeight=44;button.style.flexGrow=1;button.style.flexBasis=0;button.style.minWidth=0;button.style.whiteSpace=WhiteSpace.Normal;row.Add(button);return button;
        }
    }
}
