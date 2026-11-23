using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Shot
{
    // «Espera el bote», on the white (bounce) ring of the human's reception of the rival's serve (PO, 23 nov). Pure view: it reads the
    // ring and the controller and never touches a clock, a timing window, a contact or the ball. Its look lives in Visual/Hud.uss
    // (.sc-ring-caption): a small glass pill above the ring's outer circle, kept inside the panel.
    public sealed class ReceptionCaptionView : MonoBehaviour
    {
        public const string BounceText="Espera el bote";
        private const float Gap=.05f,Side=8,FallbackHalfWidth=60;
        private ShotController controller;private TimingRingView ring;private VisualElement root;private Label label;
        public string Text=>BounceText;
        public bool IsShown {get;private set;}
        public void Initialize(AppBootstrap app,ShotController controller,TimingRingView ring)
        {
            if(this.controller!=null)return;this.controller=controller;this.ring=ring;
            root=app.GetComponent<UIDocument>().rootVisualElement;
            label=new Label(BounceText){name="ring-caption",pickingMode=PickingMode.Ignore};label.AddToClassList("sc-ring-caption");
            label.style.display=DisplayStyle.None;root.Add(label);
        }
        // The white ring only (the amber one carries no text), only in the serve reception, only while the ring itself is on the court.
        public void Tick()
        {
            if(controller==null||label==null)return;
            IsShown=controller.ServeReception&&ring.Visible&&ring.Tint!=ShotController.VolleyTint;
            label.style.display=IsShown?DisplayStyle.Flex:DisplayStyle.None;
            if(IsShown)Place();
        }
        private void LateUpdate()=>Tick();
        private void Place()
        {
            var camera=UnityEngine.Camera.main;float panelW=root.layout.width,panelH=root.layout.height;
            if(camera==null||float.IsNaN(panelW)||float.IsNaN(panelH)||panelW<=0||panelH<=0||Screen.width<=0||Screen.height<=0)return;
            var top=ring.CuePosition+camera.transform.up*(TimingRingView.OuterRadius+Gap);
            var screen=camera.WorldToScreenPoint(top);if(screen.z<=0)return;
            float half=label.resolvedStyle.width;half=float.IsNaN(half)||half<=0?FallbackHalfWidth:half/2;
            float x=Mathf.Clamp(screen.x/Screen.width*panelW,half+Side,Mathf.Max(half+Side,panelW-half-Side));
            label.style.left=x;label.style.top=(Screen.height-screen.y)/Screen.height*panelH;
            label.style.translate=new Translate(Length.Percent(-50),Length.Percent(-100));
        }
        private void OnDestroy()=>label?.RemoveFromHierarchy();
    }
}
