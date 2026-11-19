using SquashClash.Runtime.App;
using SquashClash.Runtime.Audio;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Switches (default on): back-wall glass "toc" (off: like the side walls) and the player's own drier Perfect contact (off: impact-perfect as before).
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
            Button own=null;
            string OwnText()=>"Golpe Perfecto propio: "+(AudioScape.PerfectOwnOn?"sí":"no");
            own=new Button(()=>DevControlLog.Run(app,"audio-perfect-toggle",()=>{AudioScape.PerfectOwnOn=!AudioScape.PerfectOwnOn;own.text=OwnText();})){name="audio-perfect-toggle",text=OwnText()};
            own.style.minHeight=44;own.style.whiteSpace=WhiteSpace.Normal;Root.Add(own);
        }
    }
}
