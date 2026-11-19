using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Switch for the extended match-result card (duration, longest rally, "Revancha 5–7"); default on.
    // Off: the card exactly as it was. Not remembered between launches and not a DefaultPreset key.
    public sealed class MatchCardDevSection
    {
        public VisualElement Root {get;}=new Foldout{name="match-card-dev-section",text="Tarjeta de resultado",value=true};
        public MatchCardDevSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            Button button=null;
            string Text()=>"Tarjeta extendida: "+(RallyHud.ExtendedCardOn?"sí":"no");
            button=new Button(()=>DevControlLog.Run(app,"match-card-toggle",()=>{RallyHud.ExtendedCardOn=!RallyHud.ExtendedCardOn;button.text=Text();})){name="match-card-toggle",text=Text()};
            button.style.minHeight=44;button.style.whiteSpace=WhiteSpace.Normal;Root.Add(button);
        }
    }
}
