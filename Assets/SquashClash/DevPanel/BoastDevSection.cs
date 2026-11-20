using SquashClash.Runtime.App;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Two-wall shot (design 2026-11-20-boast-predeclaration): «Dos paredes» switches the
    // RIVAL's boast from the NEXT Rally/Match point. The human's side-wall aim (touch a side wall) is always on. Not a DefaultPreset key.
    public sealed class BoastDevSection
    {
        public VisualElement Root {get;}=new Foldout{name="boast-section",text="Dos paredes",value=true};
        public BoastDevSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            var help=new Label("Tú: toca una pared lateral (mitad delantera) para tirar a dos paredes. El rival: sí, también lo usa desde el fondo; no, no lo usa.");
            help.style.whiteSpace=WhiteSpace.Normal;Root.Add(help);
            Button button=null;
            string Text()=>"Dos paredes (rival): "+(app.Rally!=null&&app.Rally.RivalBoastNextPoint?"sí":"no");
            button=new Button(()=>DevControlLog.Run(app,"boast-rival-toggle",()=>{app.Rally.SetRivalBoast(!app.Rally.RivalBoastNextPoint);button.text=Text();})){name="boast-rival-toggle",text=Text()};
            button.style.minHeight=44;button.style.whiteSpace=WhiteSpace.Normal;Root.Add(button);
        }
    }
}
