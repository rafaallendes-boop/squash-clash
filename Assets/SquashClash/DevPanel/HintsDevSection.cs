using SquashClash.Runtime.App;
using SquashClash.Runtime.Menu;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). First-match hints: a switch (default on; off writes nothing and shows nothing) and
    // «Reiniciar pistas», which removes only the keys of FirstMatchHints.AllKeys(). Not remembered and not a DefaultPreset key.
    public sealed class HintsDevSection
    {
        public VisualElement Root {get;}=new Foldout{name="hints-dev-section",text="Pistas del primer partido",value=true};
        public HintsDevSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            Button toggle=null;
            string Text()=>"Pistas: "+(FirstMatchHints.Enabled?"sí":"no");
            toggle=new Button(()=>DevControlLog.Run(app,"hints-toggle",()=>{FirstMatchHints.Enabled=!FirstMatchHints.Enabled;toggle.text=Text();})){name="hints-toggle",text=Text()};
            toggle.style.minHeight=44;toggle.style.whiteSpace=WhiteSpace.Normal;Root.Add(toggle);
            var reset=new Button(()=>DevControlLog.Run(app,"hints-reset",FirstMatchHints.Clear)){name="hints-reset",text="Reiniciar pistas"};
            reset.style.minHeight=44;reset.style.whiteSpace=WhiteSpace.Normal;Root.Add(reset);
        }
    }
}
