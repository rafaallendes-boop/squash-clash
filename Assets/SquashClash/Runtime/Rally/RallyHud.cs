using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Rally
{
    // Playable HUD (R5): one status line, mode, pause/resume, rematch and demo, inside the boot header, which sits above
    // the projected front wall. The art pass added the style sheet, the score row and the match result card. Dev and prod.
    // Feel pass: between points the winner's digit pulses and a short banner over the front wall says who won, why, and
    // who serves next; the result card names the outcome and the last point. All of it is driven by Tick, which waits
    // while the game is paused, and none of it changes the pause between points (that is Core's).
    public sealed class RallyHud : MonoBehaviour
    {
        private AppBootstrap app;
        private RallyDirector rally;
        private Label status,humanPoints,rivalPoints,resultTitle,resultScore,resultKicker,bootTitle,fps;
        public const string DevTitle="Squash Clash — First Ball",PlayerTitle="Squash Clash";
        private VisualElement score,humanServe,rivalServe,result;
        private Button toggle,match,pause,next,rematch,demo,leave;
        // First run: true only while the game is paused by the player's own Pausa button (not by the dev tools or the system).
        public bool PausedByPlayer {get;private set;}
        // First run: the result card's "Volver al inicio".
        public event System.Action MenuRequested;
        private int scoreHuman=-1,scoreRival=-1,serveShown=-1,resultShown=-1;
        private string shown;
        private int human=-1,rival=-1,shots=-1,played=-1,buttons=-1;
        private PlayerId toHit;
        public const float PulseSeconds=.45f,BannerInSeconds=.18f,ServePulseSeconds=.6f,ResultInSeconds=.28f;
        private static readonly Color Gold=new Color(1f,.82f,.25f);
        private VisualElement banner,bannerCard,resultCard;private Label bannerTitle,bannerCause,bannerNext,resultLast;
        private float bannerAge=-1,pulseHuman,pulseRival,servePulse,resultAge=-1;private int bannerPoint=-1,bannerTone=-1,resultTone=-1;
        // Read-outs for the tests: how far the banner has faded in, and how much of a score pulse is left.
        public float BannerOpacity=>bannerAge<0?0:Mathf.Clamp01(bannerAge/BannerInSeconds);
        public float PulseRemaining=>Mathf.Max(pulseHuman,pulseRival);
        private RallyPhase phase=(RallyPhase)(-1);
        public void Initialize(AppBootstrap app,RallyDirector rally,VisualElement parent)
        {
            this.app=app;this.rally=rally;
            // Art pass: palette and type scale live in one style sheet; the elements and their names are the same.
            var sheet=Resources.Load<StyleSheet>("Visual/Hud");
            var top=parent.parent??parent;if(sheet!=null)top.styleSheets.Add(sheet);
            parent.AddToClassList("sc-header");parent.style.backgroundColor=StyleKeyword.Null;
            var title=bootTitle=parent.Q<Label>("boot-title");fps=parent.Q<Label>("fps-label");if(title!=null){title.style.fontSize=StyleKeyword.Null;title.style.color=StyleKeyword.Null;title.AddToClassList("sc-title");}
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
            var card=new VisualElement{name="match-result-card"};card.AddToClassList("sc-result-card");result.Add(card);resultCard=card;
            app.GetComponent<SafeAreaLayout>()?.Fit(result,float.NaN,0);
            resultKicker=Text("FIN DEL PARTIDO","sc-result-kicker");resultKicker.name="match-result-kicker";card.Add(resultKicker);
            resultTitle=Text("","sc-result-title");resultTitle.name="match-result-title";card.Add(resultTitle);
            resultScore=Text("","sc-result-score");resultScore.name="match-result-score";card.Add(resultScore);
            resultLast=Text("","sc-result-last");resultLast.name="match-result-last";card.Add(resultLast);
            card.Add(Text("PAR a 7 · diferencia de 2","sc-result-note"));
            var actions=new VisualElement{pickingMode=PickingMode.Ignore};actions.AddToClassList("sc-result-buttons");card.Add(actions);
            Add(actions,"match-result-rematch","Revancha",()=>{rally.Rematch();Refresh();}).AddToClassList("sc-button--primary");
            Add(actions,"match-result-menu","Volver al inicio",()=>{MenuRequested?.Invoke();Refresh();});
            leave=Add(actions,"match-result-leave","Práctica",()=>{rally.StopMode();Refresh();});
            // Point banner: over the front wall, only during the pause between points (the ball is dead then).
            banner=new VisualElement{name="rally-banner",pickingMode=PickingMode.Ignore};banner.AddToClassList("sc-banner-layer");banner.style.display=DisplayStyle.None;top.Add(banner);
            bannerCard=new VisualElement{name="rally-banner-card",pickingMode=PickingMode.Ignore};bannerCard.AddToClassList("sc-banner");banner.Add(bannerCard);
            bannerTitle=Text("","sc-banner-title");bannerTitle.name="rally-banner-title";bannerCard.Add(bannerTitle);
            bannerCause=Text("","sc-banner-cause");bannerCause.name="rally-banner-cause";bannerCard.Add(bannerCause);
            bannerNext=Text("","sc-banner-next");bannerNext.name="rally-banner-next";bannerCard.Add(bannerNext);
            // The header grows and shrinks with its buttons ("Seguir" appears with the banner): follow it.
            header=parent;parent.RegisterCallback<GeometryChangedEvent>(_=>PlaceBanner());
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
        public void TogglePause(){app.Clock.Paused=!app.Clock.Paused;PausedByPlayer=app.Clock.Paused;Refresh();}
        public void ToggleDemo(){rally.Autopilot=!rally.Autopilot;Refresh();}
        private void Update(){Refresh();Tick(Mathf.Min(Time.unscaledDeltaTime,.05f));}
        private VisualElement header;
        private void PlaceBanner()
        {
            if(banner==null||header==null||banner.parent==null||float.IsNaN(header.worldBound.yMax))return;
            banner.style.top=header.worldBound.yMax-banner.parent.worldBound.yMin+28;
        }
        private static float Ease(float t){t=Mathf.Clamp01(t);return t*t*(3-2*t);}
        private static string Capital(string text)=>string.IsNullOrEmpty(text)?text:char.ToUpperInvariant(text[0])+text.Substring(1);
        // Who made the error and what it was, from the texts Core already has.
        private static string CauseLine(PointResult point)
        {
            string cause=RallyText.Cause(point.Cause,point.Reception);
            return point.Winner.HasValue?RallyText.Player(point.Winner.Value.Other())+": "+cause:Capital(cause);
        }
        private static void Tone(VisualElement element,string prefix,int tone)
        {
            element.EnableInClassList(prefix+"--human",tone==1);element.EnableInClassList(prefix+"--rival",tone==2);element.EnableInClassList(prefix+"--none",tone==0);
        }
        private static void Pulse(Label label,float remaining)
        {
            float k=remaining>0?Mathf.Sin(Mathf.PI*(1-remaining/PulseSeconds)):0,scale=1+.5f*k;
            label.style.scale=new Scale(new Vector3(scale,scale,1));
            if(remaining>0)label.style.color=Color.Lerp(Color.white,Gold,k);else label.style.color=StyleKeyword.Null;
        }
        // Advances the HUD's own animations. It waits while the game is paused, like everything else that moves.
        public void Tick(float dt)
        {
            if(rally==null||banner==null||dt<=0||app.Clock==null||app.Clock.Paused)return;
            if(bannerAge>=0&&bannerAge<BannerInSeconds)
            {
                bannerAge=Mathf.Min(BannerInSeconds,bannerAge+dt);float k=Ease(bannerAge/BannerInSeconds);
                bannerCard.style.opacity=k;bannerCard.style.translate=new Translate(0,(1-k)*-12);
            }
            if(pulseHuman>0){pulseHuman=Mathf.Max(0,pulseHuman-dt);Pulse(humanPoints,pulseHuman);}
            if(pulseRival>0){pulseRival=Mathf.Max(0,pulseRival-dt);Pulse(rivalPoints,pulseRival);}
            if(servePulse>0)
            {
                servePulse=Mathf.Max(0,servePulse-dt);float scale=1+1.4f*Mathf.Sin(Mathf.PI*(1-servePulse/ServePulseSeconds));
                var dot=serveShown==1?humanServe:rivalServe;dot.style.scale=new Scale(new Vector3(scale,scale,1));
            }
            if(resultAge>=0&&resultAge<ResultInSeconds)
            {
                resultAge=Mathf.Min(ResultInSeconds,resultAge+dt);float k=Ease(resultAge/ResultInSeconds),scale=.88f+.12f*k;
                resultCard.style.opacity=k;resultCard.style.scale=new Scale(new Vector3(scale,scale,1));
            }
        }
        // Called every frame: it touches the UI only when what it shows changed, so an idle HUD costs nothing.
        public void Refresh()
        {
            if(rally==null||toggle==null)return;
            bool active=rally.Active,canStart=active||rally.CanStart,paused=app.Clock!=null&&app.Clock.Paused,auto=rally.Autopilot,dev=app.DevTools;
            if(!paused)PausedByPlayer=false;
            int key=(active?1:0)|(canStart?2:0)|(paused?4:0)|(auto?8:0)|((int)rally.Phase<<4)|(dev?1<<10:0);
            if(key!=buttons)
            {
                buttons=key;
                toggle.text=active?"Práctica":"Rally";toggle.SetEnabled(canStart);
                var visible=active?DisplayStyle.Flex:DisplayStyle.None;
                pause.style.display=status.style.display=score.style.display=visible;
                // First run: mode switch, demo, mid-match rematch and the dev title are developer controls; a prod
                // player reaches the match from the start menu and sees only score, status, pause and "Seguir".
                toggle.style.display=dev?DisplayStyle.Flex:DisplayStyle.None;
                rematch.style.display=demo.style.display=active&&dev?DisplayStyle.Flex:DisplayStyle.None;
                match.style.display=active||!dev?DisplayStyle.None:DisplayStyle.Flex;match.SetEnabled(canStart);
                if(bootTitle!=null)bootTitle.text=dev?DevTitle:PlayerTitle;
                if(fps!=null)fps.style.display=dev?DisplayStyle.Flex:DisplayStyle.None;
                leave.style.display=dev?DisplayStyle.Flex:DisplayStyle.None;
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
                    resultKicker.text=over==1?"VICTORIA":"DERROTA";
                    resultCard.EnableInClassList("sc-result-card--won",over==1);resultCard.EnableInClassList("sc-result-card--lost",over==2);
                    bool hasLast=rally.LastPoint.HasValue;resultLast.style.display=hasLast?DisplayStyle.Flex:DisplayStyle.None;
                    if(hasLast)resultLast.text="Último punto: "+CauseLine(rally.LastPoint.Value);
                    resultAge=0;resultCard.style.opacity=0;
                }
            }
            // Banner: built once per finished point, hidden the moment play resumes or the match ends.
            int point=active&&rally.Phase==RallyPhase.PointOver&&rally.LastPoint.HasValue?rally.PointsPlayed:-1;
            if(point!=bannerPoint)
            {
                bannerPoint=point;
                if(point<0){bannerAge=-1;banner.style.display=DisplayStyle.None;}
                else
                {
                    var ended=rally.LastPoint.Value;int tone=!ended.Winner.HasValue?0:ended.Winner.Value==PlayerId.Human?1:2;
                    bannerTitle.text=tone==1?"PUNTO PARA TI":tone==2?"PUNTO RIVAL":"SIN PUNTO";
                    bannerCause.text=CauseLine(ended)+" · "+ended.Shots+(ended.Shots==1?" golpe":" golpes");
                    bool serves=rally.Match!=null&&!rally.Match.Finished;bannerNext.style.display=serves?DisplayStyle.Flex:DisplayStyle.None;
                    if(serves)bannerNext.text="Saca "+RallyText.Player(rally.Match.Server)+" ("+RallyText.Box(rally.Match.Box)+")";
                    if(tone!=bannerTone){bannerTone=tone;Tone(bannerCard,"sc-banner",tone);}
                    PlaceBanner();
                    bannerAge=0;bannerCard.style.opacity=0;bannerCard.style.translate=new Translate(0,-12);banner.style.display=DisplayStyle.Flex;
                }
            }
            if(!active)
            {
                shown=null;phase=(RallyPhase)(-1);scoreHuman=scoreRival=-1;
                if(pulseHuman>0||pulseRival>0){pulseHuman=pulseRival=0;Pulse(humanPoints,0);Pulse(rivalPoints,0);}
                return;
            }
            // A point just won makes its digit pulse; a reset to zero (rematch) does not.
            if(scoreHuman!=rally.HumanPoints){if(scoreHuman>=0&&rally.HumanPoints>scoreHuman)pulseHuman=PulseSeconds;scoreHuman=rally.HumanPoints;humanPoints.text=scoreHuman.ToString();}
            if(scoreRival!=rally.RivalPoints){if(scoreRival>=0&&rally.RivalPoints>scoreRival)pulseRival=PulseSeconds;scoreRival=rally.RivalPoints;rivalPoints.text=scoreRival.ToString();}
            int serve=rally.Match==null?0:rally.Match.Server==PlayerId.Human?1:2;
            if(serve!=serveShown)
            {
                serveShown=serve;
                humanServe.style.visibility=serve==1?Visibility.Visible:Visibility.Hidden;rivalServe.style.visibility=serve==2?Visibility.Visible:Visibility.Hidden;
            }
            var rules=rally.Rules;
            // Rebuild the line only when something it shows changed.
            if(shown!=null&&phase==rally.Phase&&human==rally.HumanPoints&&rival==rally.RivalPoints&&shots==rules.Shots&&toHit==rules.ToHit&&played==rally.PointsPlayed)return;
            // A new point in a match: the server's dot pulses once.
            if(phase==RallyPhase.PointOver&&rally.Phase!=RallyPhase.PointOver&&rally.Phase!=RallyPhase.MatchOver&&rally.Match!=null)servePulse=ServePulseSeconds;
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
