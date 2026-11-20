using SquashClash.Runtime.App;
using SquashClash.Runtime.Shot;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    // DEV only (asmdef SQUASH_DEV || UNITY_EDITOR). Front deep drive (design 2026-11-20): the human side-wall guard on or off from
    // the next stroke, to compare a deep drive from in front of the T with and without it. Default on; not remembered between
    // launches and not a DefaultPreset key.
    public sealed class FrontDeepDriveDevSection
    {
        public VisualElement Root {get;}=new Foldout{name="front-deep-drive-section",text="Drive desde adelante",value=true};
        public FrontDeepDriveDevSection(AppBootstrap app)
        {
            Root.style.flexShrink=0;
            var help=new Label("Golpe fuerte delante de la T hacia el fondo: sí = no se va a la lateral antes del bote; no = como antes.");
            help.style.whiteSpace=WhiteSpace.Normal;Root.Add(help);
            Button button=null;
            string Text()=>"Drive adelante: "+(ShotController.FrontDeepDriveOn?"sí":"no");
            button=new Button(()=>DevControlLog.Run(app,"front-deep-drive-toggle",()=>{ShotController.FrontDeepDriveOn=!ShotController.FrontDeepDriveOn;button.text=Text();})){name="front-deep-drive-toggle",text=Text()};
            button.style.minHeight=44;button.style.whiteSpace=WhiteSpace.Normal;Root.Add(button);
        }
    }
}
