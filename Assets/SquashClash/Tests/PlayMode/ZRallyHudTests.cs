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
    // Named to run last on purpose. ShotRollbackTests.ApplyShot_AfterFlightStopped_ResimulatesToRealPresentViaClockMap
    // depends on Unity's real clock when it starts: with a launch between about 64 and 128 s after startup its exact
    // multiples of a step round one step short. New fixtures that run before it push it into that band. That test and
    // the clock belong to the timing owner, so these fixtures simply stay out of its way (see the First Rally report).
    public sealed class ZRallyHudTests
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
            var size=Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"),Enum.Parse(kind,"FixedResolution"),Width,Height,"Rally HUD test");
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
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Rally HUD fixture",app.Build.Label),new List<string>());
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
        private Rect FrontisRect()
        {
            var d=app.Config.Court;
            var points=new[]{new Vector3(0,0,0),new Vector3(d.Width,0,0),new Vector3(0,d.FrontOutLower,0),new Vector3(d.Width,d.FrontOutLower,0)}
                .Select(p=>camera.WorldToViewportPoint(p)).Select(p=>new Vector2(root.worldBound.xMin+p.x*root.layout.width,root.worldBound.yMin+(1-p.y)*root.layout.height)).ToArray();
            return Rect.MinMaxRect(points.Min(p=>p.x),points.Min(p=>p.y),points.Max(p=>p.x),points.Max(p=>p.y));
        }
        [UnityTest] public IEnumerator AtBoot_OnlyTheRallyButtonShows_AndPracticeIsUntouched()
        {
            Assert.That(root.Q<Button>("rally-toggle").text,Is.EqualTo("Rally"));Assert.That(Shown(root.Q("rally-toggle")),Is.True);
            foreach(var name in new[]{"rally-status","rally-pause","rally-rematch","rally-demo"})Assert.That(Shown(root.Q(name)),Is.False,name);
            Assert.That(rally.Active,Is.False);Assert.That(app.Host.ActiveConfig.Hash,Is.EqualTo("9041c4aafc77"));
            Assert.That(root.Q<Label>("boot-title").text,Is.EqualTo("Squash Clash — First Ball"));
            yield return null;
        }
        [UnityTest] public IEnumerator RallyButton_StartsTheRally_AndTheLineShowsScoreStrokesAndTurn()
        {
            Click("rally-toggle");yield return null;
            Assert.That(rally.Active,Is.True);Assert.That(root.Q<Button>("rally-toggle").text,Is.EqualTo("Práctica"));
            var status=root.Q<Label>("rally-status");Assert.That(Shown(status),Is.True);
            Assert.That(status.text,Is.EqualTo("Tú 0 – 0 Rival · golpes 0 · turno: Tú"));
            foreach(var name in new[]{"rally-pause","rally-rematch","rally-demo"})Assert.That(Shown(root.Q(name)),Is.True,name);
        }
        [UnityTest] public IEnumerator PointOver_ShowsWinnerAndCause_ThenRematchClearsTheScore()
        {
            Click("rally-toggle");
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver),Is.True);
            Assert.That(root.Q<Label>("rally-status").text,Is.EqualTo("Punto Rival: doble bote (sin golpe) · 0 golpes · Tú 0 – 1 Rival"));
            Click("rally-rematch");hud.Refresh();
            Assert.That(rally.RivalPoints,Is.EqualTo(0));Assert.That(root.Q<Label>("rally-status").text,Is.EqualTo("Tú 0 – 0 Rival · golpes 0 · turno: Tú"));
            yield return null;
        }
        [UnityTest] public IEnumerator PauseButton_PausesAndResumesTheClock()
        {
            Click("rally-toggle");Drive(()=>app.Host.Current.Step>10,1);
            Click("rally-pause");Assert.That(app.Clock.Paused,Is.True);Assert.That(root.Q<Button>("rally-pause").text,Is.EqualTo("Reanudar"));
            long step=app.Host.Current.Step;Drive(()=>false,.5);Assert.That(app.Host.Current.Step,Is.EqualTo(step));
            Click("rally-pause");Assert.That(app.Clock.Paused,Is.False);Assert.That(root.Q<Button>("rally-pause").text,Is.EqualTo("Pausa"));
            Drive(()=>false,.2);Assert.That(app.Host.Current.Step,Is.GreaterThan(step));
            yield return null;
        }
        [UnityTest] public IEnumerator DemoButton_LetsTheBotPlayTheHumanSide()
        {
            Click("rally-toggle");Click("rally-demo");
            Assert.That(rally.Autopilot,Is.True);Assert.That(root.Q<Button>("rally-demo").text,Is.EqualTo("Demo: sí"));
            Click("rally-demo");Assert.That(rally.Autopilot,Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator MatchButton_StartsAMatch_AndTheLineShowsTheServer()
        {
            Assert.That(Shown(root.Q("rally-match")),Is.True);
            Click("rally-match");yield return null;
            Assert.That(rally.Match,Is.Not.Null);Assert.That(Shown(root.Q("rally-match")),Is.False);
            Assert.That(root.Q<Label>("rally-status").text,Is.EqualTo("Tú 0 – 0 Rival · saca Tú (der.) · golpes 0 · turno: Rival"));
            Assert.That(Shown(root.Q("rally-next")),Is.False);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,40),Is.True);yield return null;
            Assert.That(Shown(root.Q("rally-next")),Is.True,"the pause between points can be skipped");
            long launch=app.Host.LaunchId;Click("rally-next");Drive(()=>app.Host.LaunchId>launch,.2);
            Assert.That(app.Host.LaunchId,Is.EqualTo(launch+1));
            var front=FrontisRect();
            foreach(var e in root.Q("rally-hud").Query<VisualElement>().ToList().Where(Shown))
            {
                Assert.That(e.worldBound.yMax,Is.LessThanOrEqualTo(front.yMin),e.name);
                Assert.That(e.worldBound.xMax,Is.LessThanOrEqualTo(root.worldBound.xMax),e.name+" fits the screen width");
            }
        }
        // Art pass: score row and match result card.
        [UnityTest] public IEnumerator Score_ShowsThePoints_AndTheResultCardAppearsOnlyWhenTheMatchIsOver()
        {
            Assert.That(Shown(root.Q("rally-score")),Is.False);Assert.That(Shown(root.Q("match-result")),Is.False);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("rally-match");yield return null;
            Assert.That(Shown(root.Q("rally-score")),Is.True);Assert.That(Shown(root.Q("match-result")),Is.False);
            Assert.That(root.Q<Label>("rally-score-human").text,Is.EqualTo("0"));Assert.That(root.Q<Label>("rally-score-rival").text,Is.EqualTo("0"));
            Assert.That(Drive(()=>rally.HumanPoints+rally.RivalPoints>=1,200),Is.True);hud.Refresh();
            Assert.That(root.Q<Label>("rally-score-human").text,Is.EqualTo(rally.HumanPoints.ToString()));
            Assert.That(root.Q<Label>("rally-score-rival").text,Is.EqualTo(rally.RivalPoints.ToString()));
            Assert.That(Shown(root.Q("match-result")),Is.False,"no card while the match is being played");
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);hud.Refresh();yield return null;
            Assert.That(Shown(root.Q("match-result")),Is.True);
            Assert.That(root.Q<Label>("match-result-score").text,Is.EqualTo(rally.Match.Human+" – "+rally.Match.Rival));
            Assert.That(root.Q<Label>("match-result-title").text,Is.EqualTo(rally.Match.Winner==PlayerId.Human?"¡Ganaste el partido!":"Partido para Rival"));
            var card=root.Q("match-result-card").worldBound;
            Assert.That(card.xMin,Is.GreaterThanOrEqualTo(root.worldBound.xMin));Assert.That(card.xMax,Is.LessThanOrEqualTo(root.worldBound.xMax));
            Assert.That(card.yMin,Is.GreaterThanOrEqualTo(root.Q("boot-header").worldBound.yMax),"the card does not cover the header buttons");
            Click("match-result-rematch");hud.Refresh();yield return null;
            Assert.That(Shown(root.Q("match-result")),Is.False);Assert.That(rally.Phase,Is.Not.EqualTo(RallyPhase.MatchOver));
            Assert.That(root.Q<Label>("rally-score-human").text,Is.EqualTo("0"));Assert.That(root.Q<Label>("rally-score-rival").text,Is.EqualTo("0"));
        }
        [UnityTest] public IEnumerator PracticeButton_LeavesTheRally()
        {
            Click("rally-toggle");Click("rally-toggle");yield return null;
            Assert.That(rally.Active,Is.False);Assert.That(root.Q<Button>("rally-toggle").text,Is.EqualTo("Rally"));
            Assert.That(Shown(root.Q("rally-status")),Is.False);Assert.That(app.Host.ActiveConfig.Hash,Is.EqualTo("9041c4aafc77"));
        }
        [UnityTest] public IEnumerator Hud_StaysAboveTheProjectedFrontWall_InsideTheSafeWidth()
        {
            Assert.That(Screen.width,Is.EqualTo(Width));Assert.That(Screen.height,Is.EqualTo(Height));
            Click("rally-toggle");Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver),Is.True);yield return null;yield return null;
            var front=FrontisRect();var box=root.Q("rally-hud");var header=root.Q("boot-header");
            TestContext.WriteLine($"FRONTIS {front} HUD {box.worldBound} HEADER {header.worldBound} ROOT {root.worldBound}");
            foreach(var e in box.Query<VisualElement>().ToList().Prepend(box).Prepend(header).Where(Shown))
            {
                Assert.That(e.worldBound.Overlaps(front),Is.False,$"{e.name} {e.worldBound} covers the projected front wall {front}");
                Assert.That(e.worldBound.yMax,Is.LessThanOrEqualTo(front.yMin),e.name);
                Assert.That(e.worldBound.xMin,Is.GreaterThanOrEqualTo(root.worldBound.xMin));Assert.That(e.worldBound.xMax,Is.LessThanOrEqualTo(root.worldBound.xMax),e.name+" fits the screen width");
            }
            Assert.That(root.Q<Label>("rally-status").text,Does.Not.Contain("\n"));
        }
    }
}
