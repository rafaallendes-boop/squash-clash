using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Rally
{
    // Playable HUD (R5): one status line, mode, pause/resume, rematch and demo, inside the boot header, which sits above
    // the projected front wall. The art pass added the style sheet, the score row and the match result card. Dev and prod.
    public sealed class RallyHud : MonoBehaviour
    {
        private AppBootstrap app;
        private RallyDirector rally;
        private Label status,humanPoints,rivalPoints,resultTitle,resultScore,resultKicker;
        private VisualElement score,humanServe,rivalServe,result;
        private Button toggle,match,pause,next,rematch,demo;
        private int scoreHuman=-1,scoreRival=-1,serveShown=-1,resultShown=-1;
        private string shown;
        private int human=-1,rival=-1,shots=-1,played=-1,buttons=-1;
        private PlayerId toHit;
        private RallyPhase phase=(RallyPhase)(-1);
        public void Initialize(AppBootstrap app,RallyDirector rally,VisualElement parent)
        {
            this.app=app;this.rally=rally;
            // Art pass: palette and type scale live in one style sheet; the elements and their names are the same.
            var sheet=Resources.Load<StyleSheet>("Visual/Hud");
            var top=parent.parent??parent;if(sheet!=null)top.styleSheets.Add(sheet);
            parent.AddToClassList("sc-header");parent.style.backgroundColor=StyleKeyword.Null;
            var title=parent.Q<Label>("boot-title");if(title!=null){title.style.fontSize=StyleKeyword.Null;title.style.color=StyleKeyword.Null;title.AddToClassList("sc-title");}
            foreach(var name in new[]{"build-label","fps-label"})
            {var label=parent.Q<Label>(name);if(label!=null){label.style.fontSize=StyleKeyword.Null;label.style.color=StyleKeyword.Null;label.AddToClassList("sc-meta");}}
            var box=new VisualElement{name="rally-hud",pickingMode=PickingMode.Ignore};
            box.style.alignItems=Align.Center;box.style.marginTop=6;box.style.maxWidth=Length.Percent(100);box.style.flexShrink=1;parent.Add(box);
            // Score: big digits between the two shirt colours; the yellow dot marks the server during a match.
            score=new VisualElement{name="rally-score",pickingMode=PickingMode.Ignore};score.AddToClassList("sc-score");score.style.display=DisplayStyle.None;box.Add(score);
            score.Add(Chip("sc-chip--human"));score.Add(Text("TÚ","sc-score-name"));humanServe=Chip("sc-serve");score.Add(humanServe);
            humanPoints=Text("0","sc-score-points");humanPoints.name="rally-score-human";score.Add(humanPoints);score.Add(Text("–","sc-score-dash"));
            rivalPoints=Text("0","sc-score-points");rivalPoints.name="rally-score-rival";score.Add(rivalPoints);
            rivalServe=Chip("sc-serve");score.Add(rivalServe);score.Add(Text("RIVAL","sc-score-name"));score.Add(Chip("sc-chip--rival"));
            status=new Label{name="rally-status",pickingMode=PickingMode.Ignore};
            status.AddToClassList("sc-status");status.style.whiteSpace=WhiteSpace.NoWrap;status.style.height=22;status.style.display=DisplayStyle.None;
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
            toggle.AddToClassList("sc-button--primary");match.AddToClassList("sc-button--primary");
            // Match result: a card over the court, shown only when the match is over.
            result=new VisualElement{name="match-result",pickingMode=PickingMode.Ignore};result.AddToClassList("sc-result-layer");result.style.display=DisplayStyle.None;top.Add(result);
            var card=new VisualElement{name="match-result-card"};card.AddToClassList("sc-result-card");result.Add(card);
            resultKicker=Text("FIN DEL PARTIDO","sc-result-kicker");card.Add(resultKicker);
            resultTitle=Text("","sc-result-title");resultTitle.name="match-result-title";card.Add(resultTitle);
            resultScore=Text("","sc-result-score");resultScore.name="match-result-score";card.Add(resultScore);
            card.Add(Text("PAR a 7 · diferencia de 2","sc-result-note"));
            var actions=new VisualElement{pickingMode=PickingMode.Ignore};actions.AddToClassList("sc-result-buttons");card.Add(actions);
            Add(actions,"match-result-rematch","Revancha",()=>{rally.Rematch();Refresh();}).AddToClassList("sc-button--primary");
            Add(actions,"match-result-leave","Práctica",()=>{rally.StopMode();Refresh();});
            Refresh();
        }
        private static Button Add(VisualElement row,string name,string text,System.Action action)
        {
            var button=new Button(action){name=name,text=text};
            button.AddToClassList("sc-button");
            row.Add(button);return button;
        }
        private static Label Text(string text,string style){var label=new Label(text){pickingMode=PickingMode.Ignore};label.AddToClassList(style);return label;}
        private static VisualElement Chip(string style){var chip=new VisualElement{pickingMode=PickingMode.Ignore};chip.AddToClassList("sc-chip");chip.AddToClassList(style);return chip;}
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
                pause.style.display=rematch.style.display=demo.style.display=status.style.display=score.style.display=visible;
                match.style.display=active?DisplayStyle.None:DisplayStyle.Flex;match.SetEnabled(canStart);
                // "Seguir" skips the pause between points (spec 5.2: the pause can be skipped).
                next.style.display=active&&rally.Phase==RallyPhase.PointOver?DisplayStyle.Flex:DisplayStyle.None;
                pause.text=paused?"Reanudar":"Pausa";demo.text=auto?"Demo: sí":"Demo: no";
            }
            // Score, server dot and result card: touched only when their value changes.
            int over=active&&rally.Phase==RallyPhase.MatchOver&&rally.Match!=null&&rally.Match.Winner.HasValue?(rally.Match.Winner.Value==PlayerId.Human?1:2):0;
            if(over!=resultShown)
            {
                resultShown=over;result.style.display=over!=0?DisplayStyle.Flex:DisplayStyle.None;
                if(over!=0)
                {
                    // The card and its scrim start under the header, whose buttons stay usable.
                    var header=result.parent.Q("boot-header");
                    if(header!=null&&!float.IsNaN(header.worldBound.yMax))result.style.top=header.worldBound.yMax-result.parent.worldBound.yMin+8;
                    resultTitle.text=over==1?"¡Ganaste el partido!":"Partido para Rival";
                    resultScore.text=rally.Match.Human+" – "+rally.Match.Rival;
                }
            }
            if(!active){shown=null;phase=(RallyPhase)(-1);return;}
            if(scoreHuman!=rally.HumanPoints){scoreHuman=rally.HumanPoints;humanPoints.text=scoreHuman.ToString();}
            if(scoreRival!=rally.RivalPoints){scoreRival=rally.RivalPoints;rivalPoints.text=scoreRival.ToString();}
            int serve=rally.Match==null?0:rally.Match.Server==PlayerId.Human?1:2;
            if(serve!=serveShown)
            {
                serveShown=serve;
                humanServe.style.visibility=serve==1?Visibility.Visible:Visibility.Hidden;rivalServe.style.visibility=serve==2?Visibility.Visible:Visibility.Hidden;
            }
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
