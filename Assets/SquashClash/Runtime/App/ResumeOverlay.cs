using SquashClash.Core.Simulation;
using SquashClash.Runtime.Ball;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.App
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class ResumeOverlay : MonoBehaviour
    {
        private SimClock clock;
        private VisualElement layer;
        private bool needsResume, wasManuallyPaused;
        public bool IsVisible => layer != null && layer.style.display.value != DisplayStyle.None;
        public void Initialize(SimClock clock)
        {
            this.clock = clock;
            layer?.RemoveFromHierarchy();
            layer = new VisualElement { name="resume-overlay" };
            layer.style.position = Position.Absolute; layer.style.left=0; layer.style.right=0; layer.style.top=0; layer.style.bottom=0;
            layer.style.justifyContent=Justify.Center; layer.style.alignItems=Align.Center;
            layer.style.backgroundColor=new Color(0.015f,0.03f,0.04f,0.85f);
            var label = new Label("Toca para continuar") { pickingMode=PickingMode.Ignore };
            label.style.color=Color.white; label.style.fontSize=24; layer.Add(label);
            layer.RegisterCallback<PointerDownEvent>(evt => { Continue(); evt.StopPropagation(); });
            GetComponent<UIDocument>().rootVisualElement.Add(layer);
            if (needsResume) Suspend(); else layer.style.display=DisplayStyle.None;
        }
        public void Suspend()
        {
            if(!needsResume&&clock!=null)wasManuallyPaused=clock.Paused;
            needsResume=true;
            if (clock != null) clock.Paused=true;
            if (layer != null) { layer.style.display=DisplayStyle.Flex; layer.BringToFront(); }
        }
        public void Continue()
        {
            if(needsResume)
            {
                if(TryGetComponent<BallSimulationHost>(out var host)&&host.Clock!=null)host.ResumeAfterSystemPause();
                if(clock!=null)clock.Paused=wasManuallyPaused;
                needsResume=false;
            }
            if (layer != null) layer.style.display=DisplayStyle.None;
        }
    }
}
