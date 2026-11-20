using SquashClash.Runtime.App;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Serve kinds (design 2026-11-20-serve): «Saque nuevo» from the NEXT Rally/Match point, to
    // compare the lob and drive with today's automatic serve. Off by default; not a DefaultPreset key.
    public sealed class ServeDevSection
    {
        public VisualElement Root {get;}=new Foldout{name="serve-section",text="Saque",value=true};
        public ServeDevSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            var help=new Label("Sí: globo o drive. Tu saque: en la pausa toca el frontis, arriba globo y abajo drive. El rival elige según su estilo. No: el saque de hoy.");
            help.style.whiteSpace=WhiteSpace.Normal;Root.Add(help);
            Button button=null;
            string Text()=>"Saque nuevo: "+(app.Rally!=null&&app.Rally.NewServeNextPoint?"sí":"no");
            button=new Button(()=>DevControlLog.Run(app,"serve-new-toggle",()=>{app.Rally.SetNewServe(!app.Rally.NewServeNextPoint);button.text=Text();})){name="serve-new-toggle",text=Text()};
            button.style.minHeight=44;button.style.whiteSpace=WhiteSpace.Normal;Root.Add(button);
        }
    }
}
