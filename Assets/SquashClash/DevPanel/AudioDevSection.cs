using SquashClash.Runtime.App;
using SquashClash.Runtime.Audio;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Switch for the back-wall glass "toc" (default on). Off: the glass sounds like the side walls.
    // Not remembered between launches and not a DefaultPreset key.
    public sealed class AudioDevSection
    {
        public VisualElement Root {get;}=new Foldout{name="audio-dev-section",text="Audio de prueba",value=true};
        public AudioDevSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            Button button=null;
            string Text()=>"Vidrio de fondo propio: "+(AudioScape.GlassSoundOn?"sí":"no");
            button=new Button(()=>DevControlLog.Run(app,"audio-glass-toggle",()=>{AudioScape.GlassSoundOn=!AudioScape.GlassSoundOn;button.text=Text();})){name="audio-glass-toggle",text=Text()};
            button.style.minHeight=44;button.style.whiteSpace=WhiteSpace.Normal;Root.Add(button);
        }
    }
}
