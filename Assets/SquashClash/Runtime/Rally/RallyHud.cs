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
        private Button toggle,match,pause,next,rematch,demo;
        private string shown;
        private int human=-1,rival=-1,shots=-1,played=-1,buttons=-1;
        private PlayerId toHit;
        private RallyPhase phase=(RallyPhase)(-1);
        public void Initialize(AppBootstrap app,RallyDirector rally,VisualElement parent)
        {
            this.app=app;this.rally=rally;
            var box=new VisualElement{name="rally-hud",pickingMode=PickingMode.Ignore};
            box.style.alignItems=Align.Center;box.style.marginTop=6;box.style.maxWidth=Length.Percent(100);box.style.flexShrink=1;parent.Add(box);
            status=new Label{name="rally-status",pickingMode=PickingMode.Ignore};
            status.style.fontSize=13;status.style.color=Color.white;status.style.whiteSpace=WhiteSpace.NoWrap;status.style.height=22;status.style.display=DisplayStyle.None;
            // One line always: a text longer than the screen is cut with an ellipsis, never wrapped or spilled.
            status.style.maxWidth=Length.Percent(100);status.style.overflow=Overflow.Hidden;status.style.textOverflow=TextOverflow.Ellipsis;
            box.Add(status);
            var row=new VisualElement{name="rally-buttons",pickingMode=PickingMode.Ignore};row.style.flexDirection=FlexDirection.Row;row.style.flexWrap=Wrap.Wrap;row.style.justifyContent=Justify.Center;row.style.maxWidth=Length.Percent(100);row.style.marginTop=4;box.Add(row);
            toggle=Add(row,"rally-toggle","Rally",ToggleMode);
            match=Add(row,"rally-match","Partido",StartMatch);
            pause=Add(row,"rally-pause","Pausa",TogglePause);
            next=Add(row,"rally-next","Seguir",()=>rally.SkipWait());
            rematch=Add(row,"rally-rematch","Revancha",()=>rally.Rematch());
            demo=Add(row,"rally-demo","Demo: no",ToggleDemo);
            Refresh();
        }
        private static Button Add(VisualElement row,string name,string text,System.Action action)
        {
            var button=new Button(action){name=name,text=text};
            button.style.height=44;button.style.minWidth=56;button.style.fontSize=14;button.style.marginLeft=2;button.style.marginRight=2;button.style.paddingLeft=6;button.style.paddingRight=6;
            row.Add(button);return button;
        }
        public void ToggleMode(){if(rally.Active)rally.StopMode();else rally.StartMode();Refresh();}
        public void StartMatch(){rally.StartMatch();Refresh();}
        public void TogglePause(){app.Clock.Paused=!app.Clock.Paused;Refresh();}
        public void ToggleDemo(){rally.Autopilot=!rally.Autopilot;Refresh();}
        private void Update()=>Refresh();
        // Called every frame: it touches the UI only when what it shows changed, so an idle HUD costs nothing.
        public void Refresh()
        {
            if(rally==null||toggle==null)return;
            bool active=rally.Active,canStart=active||rally.CanStart,paused=app.Clock!=null&&app.Clock.Paused,auto=rally.Autopilot;
            int key=(active?1:0)|(canStart?2:0)|(paused?4:0)|(auto?8:0)|((int)rally.Phase<<4);
            if(key!=buttons)
            {
                buttons=key;
                toggle.text=active?"Práctica":"Rally";toggle.SetEnabled(canStart);
                var visible=active?DisplayStyle.Flex:DisplayStyle.None;
                pause.style.display=rematch.style.display=demo.style.display=status.style.display=visible;
                match.style.display=active?DisplayStyle.None:DisplayStyle.Flex;match.SetEnabled(canStart);
                // "Seguir" skips the pause between points (spec 5.2: the pause can be skipped).
                next.style.display=active&&rally.Phase==RallyPhase.PointOver?DisplayStyle.Flex:DisplayStyle.None;
                pause.text=paused?"Reanudar":"Pausa";demo.text=auto?"Demo: sí":"Demo: no";
            }
            if(!active){shown=null;phase=(RallyPhase)(-1);return;}
            var rules=rally.Rules;
            // Rebuild the line only when something it shows changed.
            if(shown!=null&&phase==rally.Phase&&human==rally.HumanPoints&&rival==rally.RivalPoints&&shots==rules.Shots&&toHit==rules.ToHit&&played==rally.PointsPlayed)return;
            phase=rally.Phase;human=rally.HumanPoints;rival=rally.RivalPoints;shots=rules.Shots;toHit=rules.ToHit;played=rally.PointsPlayed;
            var last=rally.LastPoint;var game=rally.Match;
            shown=phase==RallyPhase.MatchOver&&game!=null&&game.Winner.HasValue?RallyText.MatchOver(game.Winner.Value,game.Human,game.Rival)
                :phase==RallyPhase.PointOver&&last.HasValue?RallyText.Point(last.Value.Winner,last.Value.Cause,last.Value.Reception,last.Value.Shots,human,rival)
                :game!=null?RallyText.LiveMatch(human,rival,shots,toHit,game.Server,game.Box)
                :RallyText.Live(human,rival,shots,toHit);
            status.text=shown;
        }
    }
}
