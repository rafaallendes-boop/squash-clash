using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Menu
{
    public enum FirstRunScreen {None,Menu,Tutorial}
    // First run: what a player with no context sees between launching the app and playing a match. A start menu
    // (title, Jugar, difficulty, how to play, build label) laid over the court with the game paused. It adds no
    // mechanic: Jugar is RallyDirector.StartMatch at the chosen AiLevel, exactly what the "Partido" button does.
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
        private VisualElement root,menu;private Button play,practice;private readonly Button[] levels=new Button[3];
        private int shownKey=-1;
        public FirstRunScreen Screen {get;private set;}
        public AiLevel Level {get;private set;}=AiLevel.Medium;
        public void Initialize(AppBootstrap app,RallyDirector rally,RallyHud hud)
        {
            if(this.app!=null)return;this.app=app;this.rally=rally;this.hud=hud;
            root=app.GetComponent<UIDocument>().rootVisualElement;
            BuildMenu();SelectLevel(Level);Refresh();
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
            Add(menu,"menu-howto","¿Cómo se juega?",()=>{}).AddToClassList("sc-menu-wide");
            practice=Add(menu,"menu-practice","Práctica (dev)",ClosePractice);practice.AddToClassList("sc-menu-wide");
            var build=Text("v"+app.Build.Label,"sc-menu-build");build.name="menu-build";menu.Add(build);
        }
        private static Button Add(VisualElement parent,string name,string text,System.Action action)
        {
            var button=new Button(action){name=name,text=text};button.AddToClassList("sc-button");parent.Add(button);return button;
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
        public void OpenMenu()
        {
            if(app==null)return;
            if(rally.Active)rally.StopMode();
            app.Clock.Paused=true;Screen=FirstRunScreen.Menu;Refresh();
        }
        public void SelectLevel(AiLevel level)
        {
            Level=level;
            for(int i=0;i<levels.Length;i++)if(levels[i]!=null)levels[i].EnableInClassList("sc-level--selected",i==(int)level);
        }
        // Jugar: a match against the chosen level, through the same call as the "Partido" button.
        public void Play()
        {
            if(app==null||Screen!=FirstRunScreen.Menu||!rally.CanStart)return;
            StartMatch();
        }
        private void StartMatch()
        {
            Screen=FirstRunScreen.None;app.Clock.Paused=false;rally.Level=Level;
            if(!rally.StartMatch()){OpenMenu();return;}
            Refresh();hud.Refresh();
        }
        // Dev only: leave the menu for the practice court and the developer tools.
        public void ClosePractice()
        {
            if(app==null||Screen!=FirstRunScreen.Menu)return;
            Screen=FirstRunScreen.None;app.Clock.Paused=false;Refresh();
        }
        // Called every frame: it touches the UI only when what it shows changed.
        public void Refresh()
        {
            if(app==null||menu==null)return;
            bool dev=app.DevTools,can=rally.CanStart;
            int key=(int)Screen|(dev?8:0)|(can?16:0);
            if(key==shownKey)return;shownKey=key;
            menu.style.display=Screen==FirstRunScreen.Menu?DisplayStyle.Flex:DisplayStyle.None;
            practice.style.display=dev?DisplayStyle.Flex:DisplayStyle.None;
            play.SetEnabled(can);
        }
        private void Update()=>Refresh();
        private void OnDestroy(){menu?.RemoveFromHierarchy();}
    }
}
