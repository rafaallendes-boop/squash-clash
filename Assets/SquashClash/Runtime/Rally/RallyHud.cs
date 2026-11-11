using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Rally
{
    // Minimal playable HUD (R5): one status line, mode, pause/resume, rematch and demo. No art: plain UI Toolkit
    // elements inside the boot header, which sits above the projected front wall. Present in dev and prod.
    public sealed class RallyHud : MonoBehaviour
    {
        private AppBootstrap app;
        private RallyDirector rally;
        private Label status;
        private Button toggle,pause,rematch,demo;
        private string shown;
        private int human=-1,rival=-1,shots=-1,played=-1;
        private PlayerId toHit;
        private RallyPhase phase=(RallyPhase)(-1);
        public void Initialize(AppBootstrap app,RallyDirector rally,VisualElement parent)
        {
            this.app=app;this.rally=rally;
            var box=new VisualElement{name="rally-hud",pickingMode=PickingMode.Ignore};
            box.style.alignItems=Align.Center;box.style.marginTop=6;parent.Add(box);
            status=new Label{name="rally-status",pickingMode=PickingMode.Ignore};
            status.style.fontSize=15;status.style.color=Color.white;status.style.whiteSpace=WhiteSpace.NoWrap;status.style.height=22;status.style.display=DisplayStyle.None;
            box.Add(status);
            var row=new VisualElement{name="rally-buttons",pickingMode=PickingMode.Ignore};row.style.flexDirection=FlexDirection.Row;row.style.marginTop=4;box.Add(row);
            toggle=Add(row,"rally-toggle","Rally",ToggleMode);
            pause=Add(row,"rally-pause","Pausa",TogglePause);
            rematch=Add(row,"rally-rematch","Revancha",()=>rally.Rematch());
            demo=Add(row,"rally-demo","Demo: no",ToggleDemo);
            Refresh();
        }
        private static Button Add(VisualElement row,string name,string text,System.Action action)
        {
            var button=new Button(action){name=name,text=text};
            button.style.height=44;button.style.minWidth=76;button.style.fontSize=15;button.style.marginLeft=3;button.style.marginRight=3;
            row.Add(button);return button;
        }
        public void ToggleMode(){if(rally.Active)rally.StopMode();else rally.StartMode();Refresh();}
        public void TogglePause(){app.Clock.Paused=!app.Clock.Paused;Refresh();}
        public void ToggleDemo(){rally.Autopilot=!rally.Autopilot;Refresh();}
        private void Update()=>Refresh();
        public void Refresh()
        {
            if(rally==null||toggle==null)return;
            bool active=rally.Active;
            toggle.text=active?"Práctica":"Rally";toggle.SetEnabled(active||rally.CanStart);
            var visible=active?DisplayStyle.Flex:DisplayStyle.None;
            pause.style.display=rematch.style.display=demo.style.display=status.style.display=visible;
            if(!active){shown=null;phase=(RallyPhase)(-1);return;}
            pause.text=app.Clock.Paused?"Reanudar":"Pausa";demo.text=rally.Autopilot?"Demo: sí":"Demo: no";
            var rules=rally.Rules;
            // Rebuild the line only when something it shows changed.
            if(shown!=null&&phase==rally.Phase&&human==rally.HumanPoints&&rival==rally.RivalPoints&&shots==rules.Shots&&toHit==rules.ToHit&&played==rally.PointsPlayed)return;
            phase=rally.Phase;human=rally.HumanPoints;rival=rally.RivalPoints;shots=rules.Shots;toHit=rules.ToHit;played=rally.PointsPlayed;
            var last=rally.LastPoint;
            shown=phase==RallyPhase.PointOver&&last.HasValue
                ?RallyText.Point(last.Value.Winner,last.Value.Cause,last.Value.Reception,last.Value.Shots,human,rival)
                :RallyText.Live(human,rival,shots,toHit);
            status.text=shown;
        }
    }
}
