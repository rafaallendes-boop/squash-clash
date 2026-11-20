using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Rival style for the NEXT match: Auto (random per match) or a fixed one.
    public sealed class RivalStyleSection
    {
        public VisualElement Root {get;}=new Foldout{name="rival-style-section",text="Estilo del rival",value=true};
        public RivalStyleSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            var help=new Label("Fija el estilo del rival desde el PRÓXIMO partido. Auto: al azar en cada partido.");
            help.style.whiteSpace=WhiteSpace.Normal;Root.Add(help);
            var row=new VisualElement();row.style.flexDirection=FlexDirection.Row;row.style.flexShrink=0;row.style.flexWrap=Wrap.Wrap;Root.Add(row);
            Add(app,row,"rival-style-auto","Auto",null);
            foreach(RivalStyleId id in System.Enum.GetValues(typeof(RivalStyleId)))Add(app,row,"rival-style-"+RivalStyle.Name(id).ToLowerInvariant(),RivalStyle.Name(id),id);
        }
        private static void Add(AppBootstrap app,VisualElement row,string name,string text,RivalStyleId? style)
        {
            var button=new Button(()=>DevControlLog.Run(app,name,()=>app.Rally.StyleOverride=style)){name=name,text=text};
            button.style.minHeight=44;button.style.flexGrow=1;button.style.flexBasis=0;button.style.minWidth=70;button.style.whiteSpace=WhiteSpace.Normal;row.Add(button);
        }
    }
}
