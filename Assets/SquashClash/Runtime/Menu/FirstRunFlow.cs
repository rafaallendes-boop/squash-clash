using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Audio;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Menu
{
    public enum FirstRunScreen {None,Menu,Tutorial}
    // First run: what a player with no context sees between launching the app and playing a match. A start menu
    // (title, Jugar, difficulty, how to play, build label) laid over the court with the game paused. It adds no
    // mechanic: Jugar is RallyDirector.StartMatch at the chosen AiLevel, exactly what the "Partido" button does.
    // The first Jugar goes through a three-step tutorial (how to aim and strike with M2-4, how to read the timing ring),
    // shown once (PlayerPrefs) and reopened from the menu. Its text describes M24Model and TimingRingView as they are.
    // During a match the player's Pausa opens a card (Continuar / Salir al menú) and the result card's "Volver al inicio"
    // comes back here. Pauses made by the dev tools or by the system (ResumeOverlay) do not open the card.
    // The layers exist only while the game is paused (display none otherwise), so they never take a touch during play.
    // Batch mode keeps the previous boot (straight into practice), which is what the tests and AutoQA drive.
    public sealed class FirstRunFlow : MonoBehaviour
    {
        private static bool? openAtBoot;
        // A normal launch opens the menu. Tests set it to exercise that path under batch mode.
        public static bool OpenAtBoot {get=>openAtBoot??!Application.isBatchMode;set=>openAtBoot=value;}
        public static void ResetOpenAtBoot()=>openAtBoot=null;
        private static readonly string[] LevelNames={"easy","medium","hard"},LevelTexts={"Fácil","Media","Difícil"};
        private AppBootstrap app;private RallyDirector rally;private RallyHud hud;
        private VisualElement root,menu,header,pause;private Button play,practice,menuSound,pauseSound,menuMusic,pauseMusic,menuHand;private readonly Button[] levels=new Button[3];
        private int shownKey=-1;
        private SafeAreaLayout safeArea;private Label build;
        // Space between the safe area and the menu's content (the header's top edge is the reference at the top).
        private const float MenuTop=10,MenuBottom=23,BuildBottom=6;
        private void PlaceBuild(){if(build!=null&&safeArea!=null)build.style.bottom=Mathf.Ceil(safeArea.Bottom+BuildBottom);}
        public const int TutorialSteps=3;
        public const string TutorialSeenKey="sc.firstRun.tutorialSeen";
        public const string LevelKey="sc.firstRun.level";
        // M2-4 (M24Model): the aim is fixed where the finger lands on the front wall; pulling down sets the power;
        // lifting the finger is the stroke, and lifting it back at the anchor height cancels. Timing (TimingRingView,
        // ImpactCue): a ring converges on the fixed triangle at the planned contact; one attempt per ball.
        private static readonly string[] StepTitles={"Apunta","Carga y suelta","Suelta a tiempo"};
        private static readonly string[] StepBodies=
        {
            "Apoya el dedo en la pared del frente, justo donde quieres mandar la pelota. Ese punto queda fijo mientras no levantes el dedo.",
            "Sin levantar el dedo, tira hacia abajo: mientras más tiras, más fuerte sale el golpe. Suelta para pegar. Si te arrepientes, vuelve a subir el dedo hasta donde lo apoyaste y suelta: no pegas.",
            "Cuando la pelota viene hacia ti aparece un triángulo donde le vas a pegar y un anillo que se achica. Suelta justo cuando el anillo calce dentro del triángulo. Tienes un solo intento por pelota."
        };
        private VisualElement tutorial,tutorialArt,tutorialResults;private Label tutorialStep,tutorialTitle,tutorialBody;private Button tutorialNext;
        private bool tutorialThenPlay;
        public int TutorialStep {get;private set;}
        public static bool TutorialSeen
        {
            get=>PlayerPrefs.GetInt(TutorialSeenKey,0)==1;
            private set{PlayerPrefs.SetInt(TutorialSeenKey,value?1:0);PlayerPrefs.Save();}
        }
        public FirstRunScreen Screen {get;private set;}
        public AiLevel Level {get;private set;}=AiLevel.Medium;
        public void Initialize(AppBootstrap app,RallyDirector rally,RallyHud hud)
        {
            if(this.app!=null)return;this.app=app;this.rally=rally;this.hud=hud;
            int savedLevel=PlayerPrefs.GetInt(LevelKey,(int)AiLevel.Medium);
            Level=savedLevel>=(int)AiLevel.Easy&&savedLevel<=(int)AiLevel.Hard?(AiLevel)savedLevel:AiLevel.Medium;
            root=app.GetComponent<UIDocument>().rootVisualElement;
            header=root.Q("boot-header");
            BuildMenu();BuildTutorial();BuildPause();SelectLevel(Level);hud.MenuRequested+=OpenMenu;
            // Device robustness: the layers cover the whole screen; what they show stays inside the safe area.
            safeArea=app.GetComponent<SafeAreaLayout>();
            if(safeArea!=null)
            {
                safeArea.Fit(menu,MenuTop,MenuBottom);safeArea.Fit(tutorial,MenuTop,MenuBottom);safeArea.Fit(pause,float.NaN,0);
                safeArea.Changed+=PlaceBuild;PlaceBuild();
            }
            Refresh();
            if(OpenAtBoot)OpenMenu();
        }
        private void BuildMenu()
        {
            menu=new VisualElement{name="menu-layer"};menu.AddToClassList("sc-menu-layer");menu.style.display=DisplayStyle.None;root.Add(menu);
            var art=new VisualElement{name="menu-art",pickingMode=PickingMode.Ignore};art.AddToClassList("sc-menu-art");art.generateVisualContent+=DrawArt;menu.Add(art);
            var title=Text("SQUASH CLASH","sc-menu-title");title.name="menu-title";menu.Add(title);
            menu.Add(Text("Apunta, carga y suelta.","sc-menu-tagline"));
            menu.Add(Text("DIFICULTAD","sc-menu-label"));
            var row=new VisualElement{name="menu-levels",pickingMode=PickingMode.Ignore};row.AddToClassList("sc-level-row");menu.Add(row);
            for(int i=0;i<levels.Length;i++)
            {
                var level=(AiLevel)i;levels[i]=Add(row,"menu-level-"+LevelNames[i],LevelTexts[i],()=>SelectLevel(level));levels[i].AddToClassList("sc-level");
            }
            play=Add(menu,"menu-play","Jugar",Play);play.AddToClassList("sc-button--primary");play.AddToClassList("sc-menu-play");
            Add(menu,"menu-howto","¿Cómo se juega?",()=>OpenTutorial(false)).AddToClassList("sc-menu-wide");
            // Sound and music: two switches side by side.
            var audio=Row(menu,"menu-audio","sc-menu-pair");
            menuSound=Add(audio,"menu-sound","",ToggleSound);menuSound.AddToClassList("sc-menu-half");
            menuMusic=Add(audio,"menu-music","",ToggleMusic);menuMusic.AddToClassList("sc-menu-half");
            // Cosmetic: which hand the drawn player holds the racket in.
            menuHand=Add(menu,"menu-hand","",ToggleHand);menuHand.AddToClassList("sc-menu-wide");
            practice=Add(menu,"menu-practice","Práctica (dev)",ClosePractice);practice.AddToClassList("sc-menu-wide");
            build=Text("v"+app.Build.Label,"sc-menu-build");build.name="menu-build";menu.Add(build);
        }
        private Button Add(VisualElement parent,string name,string text,System.Action action)
        {
            var button=new Button(()=>{app.UserEvents?.Menu(MetricsScreen,name);action();}){name=name,text=text};button.AddToClassList("sc-button");parent.Add(button);return button;
        }
        private static VisualElement Row(VisualElement parent,string name,string style)
        {
            var row=new VisualElement{name=name,pickingMode=PickingMode.Ignore};row.AddToClassList(style);parent.Add(row);return row;
        }
        private static Label Text(string text,string style){var label=new Label(text){pickingMode=PickingMode.Ignore};label.AddToClassList(style);return label;}
        // Menu emblem: the front wall with its lines and a ball coming off it. Vector, drawn once per layout.
        private static void DrawArt(MeshGenerationContext context)
        {
            var r=context.visualElement.contentRect;if(r.width<=0||r.height<=0)return;
            var p=context.painter2D;float w=r.width,h=r.height;
            p.lineCap=LineCap.Round;p.lineJoin=LineJoin.Round;
            p.fillColor=new Color(.93f,.90f,.82f);p.BeginPath();p.MoveTo(new Vector2(0,0));p.LineTo(new Vector2(w,0));p.LineTo(new Vector2(w,h*.80f));p.LineTo(new Vector2(0,h*.80f));p.ClosePath();p.Fill();
            p.fillColor=new Color(.80f,.60f,.36f);p.BeginPath();p.MoveTo(new Vector2(0,h*.80f));p.LineTo(new Vector2(w,h*.80f));p.LineTo(new Vector2(w,h));p.LineTo(new Vector2(0,h));p.ClosePath();p.Fill();
            p.strokeColor=new Color(.84f,.16f,.12f);p.lineWidth=3;
            foreach(float y in new[]{.10f,.44f,.70f}){p.BeginPath();p.MoveTo(new Vector2(0,h*y));p.LineTo(new Vector2(w,h*y));p.Stroke();}
            p.strokeColor=new Color(1f,.82f,.25f,.9f);p.lineWidth=4;p.BeginPath();p.MoveTo(new Vector2(w*.24f,h*.30f));
            p.BezierCurveTo(new Vector2(w*.42f,h*.20f),new Vector2(w*.62f,h*.34f),new Vector2(w*.76f,h*.62f));p.Stroke();
            p.fillColor=new Color(.10f,.12f,.16f);p.BeginPath();p.Arc(new Vector2(w*.76f,h*.62f),h*.065f,0,360);p.Fill();
            p.fillColor=new Color(1f,.82f,.25f);p.BeginPath();p.Arc(new Vector2(w*.775f,h*.605f),h*.018f,0,360);p.Fill();
        }
        private void BuildTutorial()
        {
            tutorial=new VisualElement{name="tutorial-layer"};tutorial.AddToClassList("sc-menu-layer");tutorial.style.display=DisplayStyle.None;root.Add(tutorial);
            var card=new VisualElement{name="tutorial-card",pickingMode=PickingMode.Ignore};card.AddToClassList("sc-tutorial-card");tutorial.Add(card);
            tutorialStep=Text("","sc-menu-label");tutorialStep.name="tutorial-step";card.Add(tutorialStep);
            tutorialArt=new VisualElement{name="tutorial-art",pickingMode=PickingMode.Ignore};tutorialArt.AddToClassList("sc-tutorial-art");tutorialArt.generateVisualContent+=DrawStep;card.Add(tutorialArt);
            tutorialTitle=Text("","sc-tutorial-title");tutorialTitle.name="tutorial-title";card.Add(tutorialTitle);
            tutorialBody=Text("","sc-tutorial-body");tutorialBody.name="tutorial-body";card.Add(tutorialBody);
            tutorialResults=new VisualElement{name="tutorial-results",pickingMode=PickingMode.Ignore};tutorialResults.AddToClassList("sc-tutorial-results");card.Add(tutorialResults);
            Result("perfect","Perfecto: justo a tiempo. Destello dorado.");
            Result("valid","Válido: un poco antes o después. Destello blanco.");
            Result("failed","Fallido: demasiado antes o demasiado tarde. Sin destello: no le pegas.");
            var row=new VisualElement{pickingMode=PickingMode.Ignore};row.AddToClassList("sc-tutorial-buttons");card.Add(row);
            Add(row,"tutorial-skip","Saltar",SkipTutorial).AddToClassList("sc-tutorial-skip");
            tutorialNext=Add(row,"tutorial-next","Siguiente",NextStep);tutorialNext.AddToClassList("sc-button--primary");tutorialNext.AddToClassList("sc-tutorial-next");
        }
        private void Result(string kind,string text)
        {
            var row=new VisualElement{pickingMode=PickingMode.Ignore};row.AddToClassList("sc-tutorial-result");tutorialResults.Add(row);
            var chip=new VisualElement{pickingMode=PickingMode.Ignore};chip.AddToClassList("sc-tutorial-chip");chip.AddToClassList("sc-tutorial-chip--"+kind);row.Add(chip);
            var label=Text(text,"sc-tutorial-result-text");label.name="tutorial-result-"+kind;row.Add(label);
        }
        private static void Line(Painter2D p,Vector2 a,Vector2 b){p.BeginPath();p.MoveTo(a);p.LineTo(b);p.Stroke();}
        private static void Disc(Painter2D p,Vector2 c,float radius,bool fill){p.BeginPath();p.Arc(c,radius,0,360);if(fill)p.Fill();else p.Stroke();}
        // One drawing per step: where the finger lands, the pull, and the ring fitting the triangle.
        private void DrawStep(MeshGenerationContext context)
        {
            var r=context.visualElement.contentRect;if(r.width<=0||r.height<=0)return;
            var p=context.painter2D;float w=r.width,h=r.height;p.lineCap=LineCap.Round;p.lineJoin=LineJoin.Round;
            Color gold=new Color(1f,.82f,.25f),white=Color.white,red=new Color(.84f,.16f,.12f),finger=new Color(.47f,.75f,1f,.55f);
            if(TutorialStep<2)
            {
                // The front wall, as on the court: out line, service line and tin.
                float x0=TutorialStep==0?w*.12f:w*.08f,x1=TutorialStep==0?w*.88f:w*.62f,y0=h*.08f,y1=h*.92f;
                p.fillColor=new Color(.93f,.90f,.82f);p.BeginPath();p.MoveTo(new Vector2(x0,y0));p.LineTo(new Vector2(x1,y0));p.LineTo(new Vector2(x1,y1));p.LineTo(new Vector2(x0,y1));p.ClosePath();p.Fill();
                p.strokeColor=red;p.lineWidth=2;
                foreach(float y in new[]{.16f,.50f,.82f})Line(p,new Vector2(x0,h*y),new Vector2(x1,h*y));
                var aim=new Vector2(TutorialStep==0?w*.62f:w*.35f,h*.32f);
                p.strokeColor=new Color(.10f,.12f,.16f);p.lineWidth=3;Disc(p,aim,9,false);
                Line(p,aim+new Vector2(-15,0),aim+new Vector2(15,0));Line(p,aim+new Vector2(0,-15),aim+new Vector2(0,15));
                if(TutorialStep==0){p.fillColor=finger;Disc(p,aim+new Vector2(4,10),20,true);return;}
                // The pull: straight down from where the finger landed; the bar is the power.
                var end=new Vector2(aim.x,h*.80f);
                p.strokeColor=new Color(.12f,.42f,.86f);p.lineWidth=4;Line(p,aim+new Vector2(0,16),end+new Vector2(0,-24));
                Line(p,end+new Vector2(-9,-34),end+new Vector2(0,-24));Line(p,end+new Vector2(9,-34),end+new Vector2(0,-24));
                p.fillColor=finger;Disc(p,end,20,true);
                float bx=w*.80f,top=h*.12f,bottom=h*.88f,level=bottom-(bottom-top)*.72f;
                p.strokeColor=new Color(1,1,1,.22f);p.lineWidth=18;Line(p,new Vector2(bx,top),new Vector2(bx,bottom));
                p.strokeColor=gold;p.lineWidth=18;Line(p,new Vector2(bx,level),new Vector2(bx,bottom));
                return;
            }
            // The timing cue: the fixed triangle, the ring arriving (faint) and the ring at the right instant (gold).
            var c=new Vector2(w*.5f,h*.55f);float radius=h*.34f,cos=radius*.8660254f;
            p.strokeColor=white;p.lineWidth=3;p.BeginPath();p.MoveTo(c+new Vector2(0,-radius));p.LineTo(c+new Vector2(cos,radius*.5f));p.LineTo(c+new Vector2(-cos,radius*.5f));p.ClosePath();p.Stroke();
            p.strokeColor=new Color(1,1,1,.18f);Disc(p,c,radius*1.18f,false);
            p.strokeColor=new Color(1,1,1,.38f);Disc(p,c,radius*.84f,false);
            p.strokeColor=gold;p.lineWidth=4;Disc(p,c,radius*.5f,false);
        }
        private string MetricsScreen=>Screen==FirstRunScreen.Menu?"menu":Screen==FirstRunScreen.Tutorial?"tutorial":PauseVisible?"pause":"game";
        private void ShowStep()
        {
            tutorialStep.text=(TutorialStep+1)+" de "+TutorialSteps;tutorialTitle.text=StepTitles[TutorialStep];tutorialBody.text=StepBodies[TutorialStep];
            bool last=TutorialStep==TutorialSteps-1;
            tutorialResults.style.display=last?DisplayStyle.Flex:DisplayStyle.None;
            tutorialNext.text=!last?"Siguiente":tutorialThenPlay?"¡A jugar!":"Listo";
            tutorialArt.MarkDirtyRepaint();
        }
        // thenPlay: opened by the first Jugar (the match starts when it ends); otherwise opened from the menu (back to it).
        public void OpenTutorial(bool thenPlay)
        {
            if(app==null||Screen!=FirstRunScreen.Menu)return;
            tutorialThenPlay=thenPlay;TutorialStep=0;Screen=FirstRunScreen.Tutorial;
            app.UserEvents?.Menu("tutorial","open");app.UserEvents?.BeginTutorialStep(1);ShowStep();Refresh();
        }
        public void NextStep()
        {
            if(Screen!=FirstRunScreen.Tutorial)return;
            app.UserEvents?.EndTutorialStep(true,false);
            if(TutorialStep<TutorialSteps-1){TutorialStep++;app.UserEvents?.BeginTutorialStep(TutorialStep+1);ShowStep();}else FinishTutorial();
        }
        public void SkipTutorial(){if(Screen==FirstRunScreen.Tutorial){SkipTutorialMetrics();FinishTutorial();}}
        private void SkipTutorialMetrics()
        {
            app.UserEvents?.EndTutorialStep(false,true);
            for(int i=TutorialStep+1;i<TutorialSteps;i++){app.UserEvents?.BeginTutorialStep(i+1);app.UserEvents?.EndTutorialStep(false,true);}
        }
        private void FinishTutorial()
        {
            TutorialSeen=true;
            if(tutorialThenPlay&&rally.CanStart)StartMatch();else{Screen=FirstRunScreen.Menu;app.UserEvents?.Menu("menu","open");Refresh();}
        }
        private void BuildPause()
        {
            pause=new VisualElement{name="pause-layer"};pause.AddToClassList("sc-pause-layer");pause.style.display=DisplayStyle.None;root.Add(pause);
            var card=new VisualElement{name="pause-card",pickingMode=PickingMode.Ignore};card.AddToClassList("sc-result-card");card.AddToClassList("sc-pause-card");pause.Add(card);
            var title=Text("PAUSA","sc-pause-title");title.name="pause-title";card.Add(title);
            var resume=Add(card,"pause-continue","Continuar",Continue);resume.AddToClassList("sc-button--primary");resume.AddToClassList("sc-pause-button");
            Add(card,"pause-menu","Salir al menú",LeaveToMenu).AddToClassList("sc-pause-button");
            var audio=Row(card,"pause-audio","sc-pause-pair");
            pauseSound=Add(audio,"pause-sound","",ToggleSound);pauseSound.AddToClassList("sc-pause-half");
            pauseMusic=Add(audio,"pause-music","",ToggleMusic);pauseMusic.AddToClassList("sc-pause-half");
            ShowSound();
        }
        // "Sonido" (every sound effect of the game) and "Música" (the two loops): one switch each, in the menu and in the pause card.
        public void ToggleSound(){AudioScape.SoundOn=!AudioScape.SoundOn;ShowSound();}
        public void ToggleMusic(){MusicPlayer.MusicOn=!MusicPlayer.MusicOn;ShowSound();}
        // "Mano": only what the figure looks like (Handedness). Chosen in the menu.
        public void ToggleHand(){Handedness.Left=!Handedness.Left;ShowSound();}
        private void ShowSound()
        {
            string text=AudioScape.SoundOn?"Sonido: sí":"Sonido: no";if(menuSound!=null)menuSound.text=text;if(pauseSound!=null)pauseSound.text=text;
            text=MusicPlayer.MusicOn?"Música: sí":"Música: no";if(menuMusic!=null)menuMusic.text=text;if(pauseMusic!=null)pauseMusic.text=text;
            if(menuHand!=null)menuHand.text=Handedness.Left?"Mano: zurdo":"Mano: diestro";
        }
        // The pause card: only while a match is paused by the player's own button.
        public bool PauseVisible=>app!=null&&Screen==FirstRunScreen.None&&rally.Active&&rally.Phase!=RallyPhase.MatchOver&&app.Clock.Paused&&hud.PausedByPlayer;
        public void Continue(){if(PauseVisible){hud.TogglePause();Refresh();}}
        public void LeaveToMenu(){if(PauseVisible)OpenMenu();}
        // Device robustness: an interruption (home, call, notification centre, screen lock). True when this flow keeps
        // the game paused behind a screen of its own, so the system's "Toca para continuar" layer is not added on top.
        // A match in play pauses itself into the player's pause card and waits there for Continuar.
        public bool HoldSystemPause()
        {
            if(app==null)return false;
            if(Screen!=FirstRunScreen.None||PauseVisible)return true;
            if(!rally.Active||rally.Phase==RallyPhase.MatchOver||app.Clock.Paused)return false;
            hud.TogglePause("interruption");Refresh();
            return true;
        }
        public void OpenMenu()
        {
            if(app==null)return;
            if(Screen==FirstRunScreen.Tutorial)SkipTutorialMetrics();
            if(Screen!=FirstRunScreen.Menu)app.UserEvents?.Menu("menu","open");
            if(rally.Active)rally.StopMode();
            app.Clock.Paused=true;Screen=FirstRunScreen.Menu;Refresh();
        }
        public void SelectLevel(AiLevel level)
        {
            Level=level;
            PlayerPrefs.SetInt(LevelKey,(int)level);PlayerPrefs.Save();
            for(int i=0;i<levels.Length;i++)if(levels[i]!=null)levels[i].EnableInClassList("sc-level--selected",i==(int)level);
        }
        // Jugar: a match against the chosen level, through the same call as the "Partido" button.
        public void Play()
        {
            if(app==null||Screen!=FirstRunScreen.Menu||!rally.CanStart)return;
            if(!TutorialSeen){OpenTutorial(true);return;}
            StartMatch();
        }
        private void StartMatch()
        {
            Screen=FirstRunScreen.None;app.Clock.Paused=false;rally.Level=Level;
            if(!rally.StartMatch()){OpenMenu();return;}
            app.UserEvents?.Menu("game","open");Refresh();hud.Refresh();
        }
        // Dev only: leave the menu for the practice court and the developer tools.
        public void ClosePractice()
        {
            if(app==null||Screen!=FirstRunScreen.Menu)return;
            Screen=FirstRunScreen.None;app.Clock.Paused=false;app.UserEvents?.Menu("practice","open");Refresh();
        }
        // Called every frame: it touches the UI only when what it shows changed.
        public void Refresh()
        {
            if(app==null||menu==null)return;
            bool dev=app.DevTools,can=rally.CanStart;
            bool paused=PauseVisible;
            int key=(int)Screen|(dev?8:0)|(can?16:0)|(paused?32:0);
            if(key==shownKey)return;shownKey=key;
            menu.style.display=Screen==FirstRunScreen.Menu?DisplayStyle.Flex:DisplayStyle.None;
            tutorial.style.display=Screen==FirstRunScreen.Tutorial?DisplayStyle.Flex:DisplayStyle.None;
            pause.style.display=paused?DisplayStyle.Flex:DisplayStyle.None;
            // Like the result card, the pause card and its scrim start under the header, whose button stays usable.
            if(paused&&header!=null&&!float.IsNaN(header.worldBound.yMax))pause.style.top=header.worldBound.yMax-root.worldBound.yMin+8;
            practice.style.display=dev?DisplayStyle.Flex:DisplayStyle.None;
            play.SetEnabled(can);
        }
        private void Update()=>Refresh();
        private void OnDestroy()
        {
            if(hud!=null)hud.MenuRequested-=OpenMenu;
            if(safeArea!=null)safeArea.Changed-=PlaceBuild;
            menu?.RemoveFromHierarchy();tutorial?.RemoveFromHierarchy();pause?.RemoveFromHierarchy();
        }
    }
}
