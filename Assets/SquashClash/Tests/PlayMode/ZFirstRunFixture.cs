using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // First run: shared portrait fixture (iPhone 16 Pro, as in ZRallyHudTests) for the menu, tutorial, pause and sound tests.
    // Boots the real Main scene, loads the catalog defaults and hands the loop to the test (Drive).
    public abstract class ZFirstRunFixture
    {
        protected const int Width=1206,Height=2622;
        protected AppBootstrap app;protected ShotController controller;protected RallyDirector rally;protected RallyHud hud;protected FirstRunFlow flow;protected SquashClash.Runtime.Audio.AudioScape scape;protected VisualElement root;
        private UnityEngine.Camera camera;private RenderTexture target,previousCameraTarget,previousPanelTarget;private PanelSettings settings;private float previousAspect,previousVolume;
        protected double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
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
            var size=Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"),Enum.Parse(kind,"FixedResolution"),Width,Height,"First run test");
            groupType.GetMethod("AddCustomSize").Invoke(sizeGroup,new[]{size});
            var viewType=assembly.GetType("UnityEditor.GameView");gameView=UnityEditor.EditorWindow.GetWindow(viewType);
            selectedSize=viewType.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            previousSize=(int)selectedSize.GetValue(gameView);
            selectedSize.SetValue(gameView,(int)groupType.GetMethod("GetTotalCount").Invoke(sizeGroup,null)-1);gameView.Repaint();
        }
#endif
        // openAtBoot: what a normal launch does (true) or what batch mode does by default (null leaves the default).
        protected IEnumerator Boot(bool? openAtBoot,int? savedLevel=null)
        {
            ClearPrefs();previousVolume=AudioListener.volume;
            if(savedLevel.HasValue)PlayerPrefs.SetInt("sc.firstRun.level",savedLevel.Value);
            if(openAtBoot.HasValue)FirstRunFlow.OpenAtBoot=openAtBoot.Value;else FirstRunFlow.ResetOpenAtBoot();
            yield return PlayModeTestWorld.ClearApp();
#if UNITY_EDITOR
            UseNativeGameView();yield return null;yield return null;yield return null;
#endif
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"First run fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            controller=app.GetComponent<ShotController>();rally=app.Rally;hud=app.GetComponent<RallyHud>();flow=app.FirstRun;
            Assert.That(flow,Is.Not.Null,"the first-run flow is created with the app");
            scape=app.Scape;Assert.That(scape,Is.Not.Null,"the sound layer is created with the app");
            app.Host.enabled=app.enabled=controller.enabled=rally.enabled=hud.enabled=flow.enabled=scape.enabled=false;
            camera=UnityEngine.Camera.main;previousCameraTarget=camera.targetTexture;previousAspect=camera.aspect;
            target=new RenderTexture(Width,Height,24);target.Create();camera.targetTexture=target;camera.aspect=Width/(float)Height;
            var document=app.GetComponent<UIDocument>();settings=document.panelSettings;previousPanelTarget=settings.targetTexture;settings.targetTexture=target;
            root=document.rootVisualElement;yield return null;yield return null;
        }
        protected IEnumerator Shutdown()
        {
            if(settings!=null)settings.targetTexture=previousPanelTarget;
            if(camera!=null){camera.targetTexture=previousCameraTarget;camera.aspect=previousAspect;}
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}
            yield return PlayModeTestWorld.ClearApp();
            FirstRunFlow.ResetOpenAtBoot();ClearPrefs();AudioListener.volume=previousVolume;SquashClash.Runtime.Audio.AudioScape.SoundOn=true;ClearPrefs();
#if UNITY_EDITOR
            if(gameView!=null&&selectedSize!=null)selectedSize.SetValue(gameView,previousSize);
            if(sizeGroup!=null&&customIndex>=0)sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup,new object[]{customIndex});
#endif
        }
        private static void ClearPrefs(){foreach(var key in new[]{"sc.firstRun.tutorialSeen","sc.firstRun.sound","sc.firstRun.level","sc.firstRun.rivalMatches"})PlayerPrefs.DeleteKey(key);}
        // An element the screen must have: its absence is a failed assertion, never a null reference.
        protected T Find<T>(string name) where T:VisualElement
        {
            var element=root.Q<T>(name);Assert.That(element,Is.Not.Null,"missing element "+name);return element;
        }
        protected VisualElement Find(string name)=>Find<VisualElement>(name);
        protected void Click(string name){var button=Find<Button>(name);using(var click=NavigationSubmitEvent.GetPooled()){click.target=button;button.SendEvent(click);}}
        protected void Refresh(){hud.Refresh();flow.Refresh();}
        protected bool Drive(Func<bool> until,double maxRealSeconds=30)
        {
            int limit=(int)(maxRealSeconds/StepReal);
            for(int i=0;i<limit;i++){if(until())return true;app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();scape.Tick((float)StepReal);Refresh();}
            return until();
        }
        protected static bool Shown(VisualElement element)
        {
            if(element==null)return false;
            for(var e=element;e!=null;e=e.parent)
                if(e.resolvedStyle.display==DisplayStyle.None||e.resolvedStyle.visibility==Visibility.Hidden||e.resolvedStyle.opacity==0)return false;
            return element.worldBound.width>0&&element.worldBound.height>0;
        }
        protected List<VisualElement> ShownUnder(string name)=>Find(name).Query<VisualElement>().ToList().Where(Shown).ToList();
    }
}
