using SquashClash.Runtime.App;
using SquashClash.Runtime.Shot;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    public sealed class ShotSection
    {
        private readonly AppBootstrap app;
        private readonly Label captured=new Label{name="shot-captured"};
        public VisualElement Root {get;}=new Foldout{name="first-shot-section",text="First Shot · S2",value=false};
        public ShotSection(AppBootstrap app)
        {
            this.app=app;Root.style.flexShrink=0;captured.style.whiteSpace=WhiteSpace.Normal;Root.Add(captured);
            var help=new Label("Cuerpo, timing y solver se capturan al lanzar. Mira/potencia se capturan al apoyar y al soltar. V/T3 puro y T3/T3-suave son controles independientes. Los parámetros están debajo.");
            help.style.whiteSpace=WhiteSpace.Normal;Root.Add(help);
            var calibration=app.gameObject.AddComponent<LatencyCalibration>();calibration.Initialize(app);Root.Add(calibration.Root);Refresh();
        }
        public void Refresh()
        {
            var shot=app.GetComponent<ShotController>();if(shot?.Snapshot==null){captured.text="Sin recepción";return;}
            var timing=shot.Snapshot.Timing;var next=app.Tuning.Visible;
            captured.text="Actual: "+(timing.ValidLimitOn?"V "+timing.ValidMs+" ms":"T3 puro")+(timing.EarlySoft?" · T3-suave":" · T3")+
                "\nPróximo: "+(next.GetInt("timing.validLimit")==1?"V "+next.Get("timing.validMs")+" ms":"T3 puro")+
                "\nLanzamiento "+app.Host.LaunchId+" · "+shot.Snapshot.Reception.Hash+" · plan v"+shot.Plan.Version+
                (shot.Available?"":("\n"+shot.UnavailableReason));
        }
    }
}
