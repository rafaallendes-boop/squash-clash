using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Audio;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Menu
{
    public enum FirstRunScreen {None,Menu,Tutorial,Credits}
    // First run: what a player with no context sees between launching the app and playing a match. A start menu
    // (title, Jugar, difficulty, how to play, build label) laid over the court with the game paused. It adds no
    // mechanic: Jugar is RallyDirector.StartMatch at the chosen AiLevel, exactly what the "Partido" button does.
    // The first Jugar goes through a four-step tutorial (how to aim and strike with M2-4, how to read the timing ring, the volley),
    // shown once (PlayerPrefs) and reopened from the menu. Its text describes M24Model and TimingRingView as they are.
    // During a match the player's Pausa opens a card (Continuar / Salir al menú) and the result card's "Volver al inicio"
    // comes back here. A Créditos screen (font licence and audio provenance) opens from the menu and goes back to it.
    // Pauses made by the dev tools or by the system (ResumeOverlay) do not open the card.
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
        private VisualElement root,menu,header,pause,credits;private Button play,practice,menuSound,pauseSound,menuMusic,pauseMusic,menuHand;private readonly Button[] levels=new Button[3];
        private int shownKey=-1;
        private SafeAreaLayout safeArea;private Label build;
        // Space between the safe area and the menu's content (the header's top edge is the reference at the top).
        private const float MenuTop=10,MenuBottom=23,BuildBottom=6;
        private void PlaceBuild(){if(build!=null&&safeArea!=null)build.style.bottom=Mathf.Ceil(safeArea.Bottom+BuildBottom);}
        public const int TutorialSteps=4;
        // The step that lists Perfecto / Válido / Fallido: the timing ring's.
        private const int TimingStep=2;
        public const string TutorialSeenKey="sc.firstRun.tutorialSeen";
        public const string LevelKey="sc.firstRun.level";
        // M2-4 (M24Model): the aim is fixed where the finger lands on the front wall; pulling down sets the power;
        // lifting the finger is the stroke, and lifting it back at the anchor height cancels. Timing (TimingRingView,
        // ImpactCue): a ring converges on the fixed triangle at the planned contact; one attempt per ball. Volley (19 nov, PO):
        // an amber ring before the bounce (ShotController.VolleyTint) can be struck in the air; it costs a little more energy.
        private static readonly string[] StepTitles={"Apunta","Carga y suelta","Suelta a tiempo","Volea"};
        private static readonly string[] StepBodies=
        {
            "Apoya el dedo en la pared del frente, justo donde quieres mandar la pelota. Ese punto queda fijo mientras no levantes el dedo.",
            "Sin levantar el dedo, tira hacia abajo: mientras más tiras, más fuerte sale el golpe. Suelta para pegar. Si te arrepientes, vuelve a subir el dedo hasta donde lo apoyaste y suelta: no pegas.",
            "Cuando la pelota viene hacia ti aparece un triángulo donde le vas a pegar y un anillo que se achica. Suelta justo cuando el anillo calce dentro del triángulo. Tienes un solo intento por pelota.",
            "Si el anillo sale ámbar, puedes pegarle en el aire, antes del bote. Suelta cuando calce, como siempre: volear cansa un poco más."
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
            BuildMenu();BuildTutorial();BuildCredits();BuildPause();SelectLevel(Level);hud.MenuRequested+=OpenMenu;
            // Device robustness: the layers cover the whole screen; what they show stays inside the safe area.
            safeArea=app.GetComponent<SafeAreaLayout>();
            if(safeArea!=null)
            {
                safeArea.Fit(menu,MenuTop,MenuBottom);safeArea.Fit(tutorial,MenuTop,MenuBottom);safeArea.Fit(credits,MenuTop,MenuBottom);safeArea.Fit(pause,float.NaN,0);
                safeArea.Changed+=PlaceBuild;PlaceBuild();
            }
            Refresh();
            if(OpenAtBoot)OpenMenu();
        }
        // Menu redesign (19 nov): the screen is the front wall at scale, read top to bottom as on a court: the out line, the
        // wordmark in the playing area above the service line, the choices below it within thumb reach, the tin at the bottom.
        private void BuildMenu()
        {
            menu=new VisualElement{name="menu-layer"};menu.AddToClassList("sc-menu-layer");menu.AddToClassList("sc-menu-home");menu.AddToClassList("sc-wall");menu.style.display=DisplayStyle.None;root.Add(menu);
            menu.Add(Decor("menu-outline","sc-menu-outline"));
            var hero=Row(menu,"menu-hero","sc-menu-hero");
            // The wall keeps the marks of every ball that hit it; the wordmark sits just above the service line, and the
            // ball squashes against the wall right after "CLASH".
            var marks=Decor("menu-marks","sc-menu-marks");marks.generateVisualContent+=DrawMarks;hero.Add(marks);
            var wordmark=Row(hero,"menu-wordmark","sc-menu-wordmark");
            var title=Text("SQUASH CLASH","sc-menu-title");title.name="menu-title";wordmark.Add(title);
            var art=new VisualElement{name="menu-art",pickingMode=PickingMode.Ignore};art.AddToClassList("sc-menu-art");art.generateVisualContent+=DrawArt;wordmark.Add(art);
            hero.Add(Text("Apunta, carga y suelta.","sc-menu-tagline"));
            menu.Add(Decor("menu-rule","sc-menu-rule"));menu.Add(Decor("menu-tin","sc-menu-tin"));
            var controls=Row(menu,"menu-controls","sc-menu-controls");
            controls.Add(Text("Nivel del rival","sc-menu-label"));
            var row=Row(controls,"menu-levels","sc-level-row");
            for(int i=0;i<levels.Length;i++)
            {
                var level=(AiLevel)i;levels[i]=Add(row,"menu-level-"+LevelNames[i],LevelTexts[i],()=>SelectLevel(level));levels[i].AddToClassList("sc-level");
                if(i>0)levels[i].AddToClassList("sc-level--divided");
            }
            play=Add(controls,"menu-play","Jugar",Play);play.AddToClassList("sc-button--primary");play.AddToClassList("sc-menu-play");
            var links=Row(controls,"menu-links","sc-menu-links");
            var howto=Add(links,"menu-howto","¿Cómo se juega?",()=>OpenTutorial(false));howto.AddToClassList("sc-menu-link");howto.AddToClassList("sc-menu-link--first");
            Add(links,"menu-credits","Créditos",OpenCredits).AddToClassList("sc-menu-link");
            // Sound, music and the hand the drawn player holds the racket in (cosmetic): three switches in a row.
            var audio=Row(controls,"menu-audio","sc-menu-switches");
            menuSound=Add(audio,"menu-sound","",ToggleSound);menuSound.AddToClassList("sc-menu-switch");
            menuMusic=Add(audio,"menu-music","",ToggleMusic);menuMusic.AddToClassList("sc-menu-switch");
            menuHand=Add(audio,"menu-hand","",ToggleHand);menuHand.AddToClassList("sc-menu-switch");menuHand.AddToClassList("sc-menu-switch--last");
            practice=Add(controls,"menu-practice","Práctica (dev)",ClosePractice);practice.AddToClassList("sc-menu-dev");
            build=Text("v"+app.Build.Label,"sc-menu-build");build.name="menu-build";menu.Add(build);
        }
        // Credits (19 nov), owed before publishing: the font and its licence, as the font's own name table states them, and
        // where the sound comes from (Resources/Audio/PROVENANCE.md).
        public const string RubikCopyright="Copyright (c) 2015 by Hubert & Fischer. All rights reserved. Hebrew characters (c) 2016 by Meir Sadan.";
        private void BuildCredits()
        {
            credits=new VisualElement{name="credits-layer"};credits.AddToClassList("sc-menu-layer");credits.AddToClassList("sc-credits");credits.AddToClassList("sc-wall");credits.style.display=DisplayStyle.None;root.Add(credits);
            credits.Add(Decor("credits-outline","sc-menu-outline"));
            var title=Text("Créditos","sc-credits-title");title.name="credits-title";credits.Add(title);
            credits.Add(Decor("credits-rule","sc-menu-rule"));
            var body=Row(credits,"credits-body","sc-credits-body");
            body.Add(Text("Tipografía","sc-credits-section"));
            Named(body,"credits-font","Rubik, de Hubert & Fischer.","sc-credits-lead");
            Named(body,"credits-font-copyright",RubikCopyright,"sc-credits-fine");
            Named(body,"credits-font-license","Se usa con la licencia SIL Open Font License 1.1 (scripts.sil.org/OFL).","sc-credits-text");
            body.Add(Text("Sonido y música","sc-credits-section"));
            Named(body,"credits-audio","Los efectos y la música se compusieron y generaron con código para Squash Clash en 2026, sin grabaciones, samples ni bibliotecas de sonido externas.","sc-credits-text");
            var back=Add(credits,"credits-back","Volver",CloseCredits);back.AddToClassList("sc-button--primary");back.AddToClassList("sc-credits-back");
            credits.Add(Decor("credits-tin","sc-menu-tin"));
        }
        private static void Named(VisualElement parent,string name,string text,string style){var label=Text(text,style);label.name=name;parent.Add(label);}
        public void OpenCredits()
        {
            if(app==null||Screen!=FirstRunScreen.Menu)return;
            Screen=FirstRunScreen.Credits;app.UserEvents?.Menu("credits","open");Refresh();
        }
        public void CloseCredits()
        {
            if(Screen!=FirstRunScreen.Credits)return;
            Screen=FirstRunScreen.Menu;app.UserEvents?.Menu("menu","open");Refresh();
        }
        private Button Add(VisualElement parent,string name,string text,System.Action action)
        {
            var button=new Button(()=>{app.UserEvents?.Menu(MetricsScreen,name);action();}){name=name,text=text};button.AddToClassList("sc-button");parent.Add(button);return button;
        }
        private static VisualElement Row(VisualElement parent,string name,string style)
        {
            var row=new VisualElement{name=name,pickingMode=PickingMode.Ignore};row.AddToClassList(style);parent.Add(row);return row;
        }
        private static VisualElement Decor(string name,string style){var e=new VisualElement{name=name,pickingMode=PickingMode.Ignore};e.AddToClassList(style);return e;}
        private static Label Text(string text,string style){var label=new Label(text){pickingMode=PickingMode.Ignore};label.AddToClassList(style);return label;}
        // Plaster, ink and line red, as in Visual/Hud.uss; the tutorial drawings use the same three.
        private static readonly Color Plaster=new Color(.93f,.945f,.937f),Ink=new Color(.094f,.133f,.173f),LineRed=new Color(.808f,.125f,.176f),Dot=new Color(1f,.82f,.25f),Amber=new Color(1f,.72f,.16f);
        // Menu emblem: the ball (black, with the two yellow dots of a match ball) squashing flat against the front wall, with
        // the burst of the hit around it. No stroke on the side facing the wordmark, so it never reads as a hyphen after
        // "CLASH". Vector, drawn once per layout.
        private static void DrawArt(MeshGenerationContext context)
        {
            var r=context.visualElement.contentRect;if(r.width<=0||r.height<=0)return;
            var p=context.painter2D;float w=r.width,h=r.height,radius=Mathf.Min(w,h)*.30f;var c=new Vector2(w*.5f,h*.5f);
            p.lineCap=LineCap.Round;p.lineJoin=LineJoin.Round;
            p.strokeColor=LineRed;p.lineWidth=3.5f;
            for(int i=0;i<8;i++)
            {
                if(i==4)continue;
                float angle=i*45*Mathf.Deg2Rad;var d=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                var from=c+new Vector2(d.x*radius*1.55f,d.y*radius*1.10f);Line(p,from,from+d*radius*.42f);
            }
            // The ball itself, squashed: wider than tall, as it is for the instant it touches the wall.
            p.fillColor=Ink;Oval(p,c,radius*1.22f,radius*.80f);p.Fill();
            p.fillColor=Dot;float d0=radius*.2f;
            Disc(p,c+new Vector2(-radius*.38f,-radius*.22f),d0,true);Disc(p,c+new Vector2(radius*.38f,-radius*.22f),d0,true);
        }
        private static void Oval(Painter2D p,Vector2 c,float rx,float ry)
        {
            const float k=.5522848f;p.BeginPath();p.MoveTo(c+new Vector2(rx,0));
            p.BezierCurveTo(c+new Vector2(rx,ry*k),c+new Vector2(rx*k,ry),c+new Vector2(0,ry));
            p.BezierCurveTo(c+new Vector2(-rx*k,ry),c+new Vector2(-rx,ry*k),c+new Vector2(-rx,0));
            p.BezierCurveTo(c+new Vector2(-rx,-ry*k),c+new Vector2(-rx*k,-ry),c+new Vector2(0,-ry));
            p.BezierCurveTo(c+new Vector2(rx*k,-ry),c+new Vector2(rx,-ry*k),c+new Vector2(rx,0));p.ClosePath();
        }
        // Ball marks on the plaster: a fixed scatter (fractions of the wall above the wordmark), small and faint, a loose
        // cluster where shots land most. Behind everything, never touched.
        private static readonly Vector3[] Marks=
        {
            new Vector3(.14f,.30f,.6f),new Vector3(.33f,.12f,.4f),new Vector3(.61f,.22f,.7f),new Vector3(.86f,.10f,.35f),new Vector3(.74f,.44f,.9f),
            new Vector3(.22f,.58f,.8f),new Vector3(.48f,.40f,.6f),new Vector3(.90f,.64f,.75f),new Vector3(.57f,.70f,1f),new Vector3(.08f,.80f,.55f),
            new Vector3(.38f,.86f,.85f)
        };
        // The marks stay above the wordmark and the tagline (two 72 pt lines, the tagline and the hero's padding).
        private const float WordmarkClear=230;
        private static void DrawMarks(MeshGenerationContext context)
        {
            var r=context.visualElement.contentRect;if(r.width<=0||r.height<=0)return;
            var p=context.painter2D;float top=r.height*.04f,band=r.height-WordmarkClear-top;
            // A short wall (a 16:9 phone) has no room above the wordmark: no marks rather than a row of them.
            if(band<60)return;
            foreach(var m in Marks)
            {
                float size=2.5f+3f*m.z;var c=new Vector2(r.width*m.x,top+band*m.y);
                p.fillColor=new Color(Ink.r,Ink.g,Ink.b,.05f+.05f*m.z);Oval(p,c,size*1.3f,size*.85f);p.Fill();
            }
        }
        private void BuildTutorial()
        {
            tutorial=new VisualElement{name="tutorial-layer"};tutorial.AddToClassList("sc-menu-layer");tutorial.AddToClassList("sc-wall");tutorial.style.display=DisplayStyle.None;root.Add(tutorial);
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
        // One drawing per step: where the finger lands, the pull, the ring fitting the triangle, and the volley in the air.
        private void DrawStep(MeshGenerationContext context)
        {
            var r=context.visualElement.contentRect;if(r.width<=0||r.height<=0)return;
            var p=context.painter2D;float w=r.width,h=r.height;p.lineCap=LineCap.Round;p.lineJoin=LineJoin.Round;
            Color gold=Dot,white=Color.white,red=LineRed,finger=new Color(.47f,.75f,1f,.55f);
            if(TutorialStep==3){DrawVolley(p,w,h);return;}
            if(TutorialStep<2)
            {
                // The front wall, as on the court: out line, service line and tin.
                float x0=TutorialStep==0?w*.12f:w*.08f,x1=TutorialStep==0?w*.88f:w*.62f,y0=h*.08f,y1=h*.92f;
                p.fillColor=Plaster;p.BeginPath();p.MoveTo(new Vector2(x0,y0));p.LineTo(new Vector2(x1,y0));p.LineTo(new Vector2(x1,y1));p.LineTo(new Vector2(x0,y1));p.ClosePath();p.Fill();
                p.strokeColor=red;p.lineWidth=2;
                foreach(float y in new[]{.16f,.50f,.82f})Line(p,new Vector2(x0,h*y),new Vector2(x1,h*y));
                var aim=new Vector2(TutorialStep==0?w*.62f:w*.35f,h*.32f);
                p.strokeColor=Ink;p.lineWidth=3;Disc(p,aim,9,false);
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
        // The volley: the ball's path towards a bounce that never comes (faint), the amber triangle and ring at the ball in the air,
        // the burst of the contact around it (VolleyBurstView), and the ball going back to the front wall.
        private static void DrawVolley(Painter2D p,float w,float h)
        {
            Vector2 a=new Vector2(w*.06f,h*.16f),c=new Vector2(w*.56f,h*.20f),b=new Vector2(w*.90f,h*.86f);
            Vector2 At(float t)=>(1-t)*(1-t)*a+2*t*(1-t)*c+t*t*b;
            p.strokeColor=new Color(1,1,1,.3f);p.lineWidth=2;Line(p,new Vector2(w*.04f,h*.86f),new Vector2(w*.96f,h*.86f));
            for(int i=0;i<=16;i++)
            {
                float t=i/16f;if(Mathf.Abs(t-.5f)<.1f)continue;
                p.fillColor=new Color(1,1,1,t<.5f?.55f:.16f);Disc(p,At(t),t<.5f?2.2f:1.8f,true);
            }
            // Where it would have bounced.
            p.strokeColor=new Color(1,1,1,.22f);p.lineWidth=2;Line(p,b+new Vector2(-7,0),b+new Vector2(7,0));Line(p,b+new Vector2(0,-6),b+new Vector2(0,-1));
            var ball=At(.5f);float radius=h*.2f,cos=radius*.8660254f;
            // The ball going back to the front wall.
            p.strokeColor=new Color(1,1,1,.7f);p.lineWidth=3;var back=new Vector2(w*.12f,h*.52f);var from=ball+new Vector2(-radius*.7f,radius*.25f);
            Line(p,from,back);var dir=(back-from).normalized;var side=new Vector2(-dir.y,dir.x);
            Line(p,back,back-dir*9+side*6);Line(p,back,back-dir*9-side*6);
            p.strokeColor=Amber;p.lineWidth=3;p.BeginPath();p.MoveTo(ball+new Vector2(0,-radius));p.LineTo(ball+new Vector2(cos,radius*.5f));p.LineTo(ball+new Vector2(-cos,radius*.5f));p.ClosePath();p.Stroke();
            p.lineWidth=4;Disc(p,ball,radius*.5f,false);
            // The burst of the contact: short rays around the ring.
            p.strokeColor=new Color(Amber.r,Amber.g,Amber.b,.85f);p.lineWidth=3;
            for(int i=0;i<8;i++)
            {
                float angle=(i*45+22.5f)*Mathf.Deg2Rad;var d=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                Line(p,ball+d*radius*1.1f,ball+d*radius*1.35f);
            }
            p.fillColor=Plaster;Disc(p,ball,6,true);
        }
        private string MetricsScreen=>Screen==FirstRunScreen.Menu?"menu":Screen==FirstRunScreen.Tutorial?"tutorial":Screen==FirstRunScreen.Credits?"credits":PauseVisible?"pause":"game";
        private void ShowStep()
        {
            tutorialStep.text=(TutorialStep+1)+" de "+TutorialSteps;tutorialTitle.text=StepTitles[TutorialStep];tutorialBody.text=StepBodies[TutorialStep];
            bool last=TutorialStep==TutorialSteps-1;
            tutorialResults.style.display=TutorialStep==TimingStep?DisplayStyle.Flex:DisplayStyle.None;
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
            var card=new VisualElement{name="pause-card",pickingMode=PickingMode.Ignore};card.AddToClassList("sc-result-card");card.AddToClassList("sc-pause-card");card.AddToClassList("sc-wall");pause.Add(card);
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
            bool sound=AudioScape.SoundOn,music=MusicPlayer.MusicOn;
            Switch(menuSound,sound,"Sonido: sí","Sonido: no");Switch(pauseSound,sound,"Sonido: sí","Sonido: no");
            Switch(menuMusic,music,"Música: sí","Música: no");Switch(pauseMusic,music,"Música: sí","Música: no");
            if(menuHand!=null)menuHand.text=Handedness.Left?"Mano: zurdo":"Mano: diestro";
        }
        // A switch says its state in words and in look: an off switch is marked (sc-toggle--off).
        private static void Switch(Button button,bool on,string onText,string offText)
        {
            if(button==null)return;button.text=on?onText:offText;button.EnableInClassList("sc-toggle--off",!on);
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
            credits.style.display=Screen==FirstRunScreen.Credits?DisplayStyle.Flex:DisplayStyle.None;
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
            menu?.RemoveFromHierarchy();tutorial?.RemoveFromHierarchy();credits?.RemoveFromHierarchy();pause?.RemoveFromHierarchy();
        }
    }
}
