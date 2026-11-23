using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Feel pass, block 3: what the HUD does between points. Same portrait fixture as ZRallyHudTests; named to run last.
    public sealed class ZRallyHudFeelTests
    {
        // Portrait iPhone 16 Pro fixture, as in the reticle visibility tests: 62 pt top / 34 pt bottom at native 3x.
        private const int Width=1206,Height=2622;
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;private RallyHud hud;private VisualElement root;
        private UnityEngine.Camera camera;private RenderTexture target,previousCameraTarget,previousPanelTarget;private PanelSettings settings;private float previousAspect;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
#if UNITY_EDITOR
        private UnityEditor.EditorWindow gameView;private object sizeGroup;private PropertyInfo selectedSize;private int previousSize,customIndex=-1;
        private void UseNativeGameView()
        {
            var assembly=typeof(UnityEditor.Editor).Assembly;var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
            sizeGroup=sizesType.GetProperty("currentGroup").GetValue(sizes);var groupType=sizeGroup.GetType();
            customIndex=(int)groupType.GetMethod("GetTotalCount").Invoke(sizeGroup,null);
            var kind=assembly.GetType("UnityEditor.GameViewSizeType");
            var size=Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"),Enum.Parse(kind,"FixedResolution"),Width,Height,"Rally HUD feel test");
            groupType.GetMethod("AddCustomSize").Invoke(sizeGroup,new[]{size});
            var viewType=assembly.GetType("UnityEditor.GameView");gameView=UnityEditor.EditorWindow.GetWindow(viewType);
            selectedSize=viewType.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            previousSize=(int)selectedSize.GetValue(gameView);
            selectedSize.SetValue(gameView,(int)groupType.GetMethod("GetTotalCount").Invoke(sizeGroup,null)-1);gameView.Repaint();
        }
#endif
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();
#if UNITY_EDITOR
            UseNativeGameView();yield return null;yield return null;yield return null;
#endif
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Rally HUD feel fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            controller=app.GetComponent<ShotController>();rally=app.Rally;hud=app.GetComponent<RallyHud>();
            app.Host.enabled=app.enabled=controller.enabled=rally.enabled=hud.enabled=false;
            camera=UnityEngine.Camera.main;previousCameraTarget=camera.targetTexture;previousAspect=camera.aspect;
            target=new RenderTexture(Width,Height,24);target.Create();camera.targetTexture=target;camera.aspect=Width/(float)Height;
            var document=app.GetComponent<UIDocument>();settings=document.panelSettings;previousPanelTarget=settings.targetTexture;settings.targetTexture=target;
            root=document.rootVisualElement;yield return null;yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(settings!=null)settings.targetTexture=previousPanelTarget;
            if(camera!=null){camera.targetTexture=previousCameraTarget;camera.aspect=previousAspect;}
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}
            yield return PlayModeTestWorld.ClearApp();
#if UNITY_EDITOR
            if(gameView!=null&&selectedSize!=null)selectedSize.SetValue(gameView,previousSize);
            if(sizeGroup!=null&&customIndex>=0)sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup,new object[]{customIndex});
#endif
        }
        private void Click(string name){var button=root.Q<Button>(name);using(var click=NavigationSubmitEvent.GetPooled()){click.target=button;button.SendEvent(click);}}
        private bool Drive(Func<bool> until,double maxRealSeconds=30)
        {
            int limit=(int)(maxRealSeconds/StepReal);
            for(int i=0;i<limit;i++){if(until())return true;app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();hud.Refresh();}
            return until();
        }
        private static bool Shown(VisualElement element)
        {
            for(var e=element;e!=null;e=e.parent)
                if(e.resolvedStyle.display==DisplayStyle.None||e.resolvedStyle.visibility==Visibility.Hidden||e.resolvedStyle.opacity==0)return false;
            return element.worldBound.width>0&&element.worldBound.height>0;
        }
        private static readonly string[] KeptNames={"boot-header","rally-hud","rally-status","rally-toggle","rally-pause","rally-rematch","rally-demo"};

        [UnityTest] public IEnumerator PointOver_ShowsABannerWithWinnerAndCause_OnlyDuringThePauseBetweenPoints()
        {
            foreach(var name in KeptNames)Assert.That(root.Q(name),Is.Not.Null,name+" keeps its name");
            var banner=root.Q("rally-banner");Assert.That(banner,Is.Not.Null,"the banner exists from the start");
            Assert.That(root.Q("rally-hud").Contains(banner),Is.False,"it is not one more row of the header");
            Assert.That(Shown(banner),Is.False);
            Click("rally-toggle");hud.Refresh();hud.Tick(.3f);yield return null;
            Assert.That(Shown(banner),Is.False,"nothing over the court while the ball is in play");
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver),Is.True);hud.Refresh();hud.Tick(.3f);yield return null;
            Assert.That(Shown(banner),Is.True);
            Assert.That(root.Q<Label>("rally-banner-title").text,Is.EqualTo("PUNTO RIVAL"));
            Assert.That(root.Q<Label>("rally-banner-cause").text,Is.EqualTo("Tú: doble bote (sin golpe) · 0 golpes"));
            Assert.That(Shown(root.Q("rally-banner-next")),Is.False,"no server outside a match");
            var card=root.Q("rally-banner-card").worldBound;
            Assert.That(card.yMin,Is.GreaterThanOrEqualTo(root.Q("boot-header").worldBound.yMax),"below the header buttons");
            Assert.That(card.xMin,Is.GreaterThanOrEqualTo(root.worldBound.xMin));Assert.That(card.xMax,Is.LessThanOrEqualTo(root.worldBound.xMax));
            // The pause between points is Core's and is not stretched for the banner.
            Assert.That(rally.WaitBetweenPoints,Is.EqualTo(1.6));Assert.That(RallyHud.BannerInSeconds,Is.LessThan(.3f));
            long launch=app.Host.LaunchId;Assert.That(Drive(()=>app.Host.LaunchId>launch,rally.WaitBetweenPoints+.2),Is.True,"the next point starts on Core's schedule");
            hud.Refresh();hud.Tick(.05f);yield return null;
            Assert.That(Shown(banner),Is.False,"gone as soon as the next point starts");
        }

        [UnityTest] public IEnumerator InAMatch_TheBannerSaysWhoServesNext_AndTheScorePulses()
        {
            rally.SetNewServe(false);
            Click("rally-match");hud.Refresh();yield return null;
            Assert.That(hud.PulseRemaining,Is.Zero);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,60),Is.True);hud.Refresh();
            Assert.That(hud.PulseRemaining,Is.EqualTo(RallyHud.PulseSeconds),"the winner's digit starts its pulse with the point");
            hud.Tick(.1f);yield return null;
            var points=root.Q<Label>(rally.LastPoint.Value.Winner==PlayerId.Human?"rally-score-human":"rally-score-rival");
            Assert.That(points.resolvedStyle.scale.value.x,Is.GreaterThan(1.1f),"the digit grows while it pulses");
            Assert.That(Shown(root.Q("rally-banner-next")),Is.True);
            Assert.That(root.Q<Label>("rally-banner-next").text,Is.EqualTo("Saca "+RallyText.Player(rally.Match.Server)+" ("+RallyText.Box(rally.Match.Box)+")"));
            Assert.That(rally.WaitBetweenPoints,Is.EqualTo(2.5));
            hud.Tick(1);yield return null;
            Assert.That(hud.PulseRemaining,Is.Zero);Assert.That(points.resolvedStyle.scale.value.x,Is.EqualTo(1).Within(1e-3f),"and returns to its size");
        }

        [UnityTest] public IEnumerator BannerAndPulse_FreezeWhileTheGameIsPaused()
        {
            Click("rally-toggle");
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver),Is.True);hud.Refresh();hud.Tick(.05f);
            float opacity=hud.BannerOpacity,pulse=hud.PulseRemaining;
            Assert.That(opacity,Is.InRange(.01f,.99f),"still fading in");Assert.That(pulse,Is.InRange(.01f,RallyHud.PulseSeconds));
            app.Clock.Paused=true;hud.Refresh();hud.Tick(1);
            Assert.That(hud.BannerOpacity,Is.EqualTo(opacity));Assert.That(hud.PulseRemaining,Is.EqualTo(pulse),"nothing animates under a pause");
            hud.enabled=true;yield return new WaitForSecondsRealtime(.3f);hud.enabled=false;
            Assert.That(hud.BannerOpacity,Is.EqualTo(opacity),"not even on real frames");Assert.That(hud.PulseRemaining,Is.EqualTo(pulse));
            app.Clock.Paused=false;hud.Tick(1);
            Assert.That(hud.BannerOpacity,Is.EqualTo(1));Assert.That(hud.PulseRemaining,Is.Zero);
        }

        [UnityTest] public IEnumerator MatchOver_TheCardNamesTheResultAndTheLastPoint()
        {
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("rally-match");yield return null;
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);hud.Refresh();hud.Tick(1);yield return null;
            bool won=rally.Match.Winner==PlayerId.Human;
            var kicker=root.Q<Label>("match-result-kicker");Assert.That(kicker,Is.Not.Null,"the kicker has a name");
            Assert.That(Shown(root.Q("match-result")),Is.True);Assert.That(Shown(root.Q("rally-banner")),Is.False,"the card replaces the banner");
            Assert.That(kicker.text,Is.EqualTo(won?"VICTORIA":"DERROTA"));
            Assert.That(root.Q("match-result-card").ClassListContains(won?"sc-result-card--won":"sc-result-card--lost"),Is.True);
            var last=root.Q<Label>("match-result-last");Assert.That(last,Is.Not.Null);Assert.That(Shown(last),Is.True);
            Assert.That(last.text,Does.StartWith("Último punto: "));Assert.That(last.text,Does.Contain(RallyText.Cause(rally.LastPoint.Value.Cause,rally.LastPoint.Value.Reception)));
            Assert.That(root.Q<Label>("match-result-title").text,Is.EqualTo(won?"¡Ganaste el partido!":"Partido para Rival"),"texts the earlier tests fix are unchanged");
            var card=root.Q("match-result-card").worldBound;
            Assert.That(card.yMin,Is.GreaterThanOrEqualTo(root.Q("boot-header").worldBound.yMax));Assert.That(card.yMax,Is.LessThanOrEqualTo(root.worldBound.yMax));
        }
    }
}
