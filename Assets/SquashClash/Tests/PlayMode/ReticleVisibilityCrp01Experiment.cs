using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Shot;
using SquashClash.DevPanel;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;

namespace SquashClash.Runtime.Tests
{
    [Explicit("CRP-01 diagnosis experiment; instrumentation only"),Category("Experiments")]
    public sealed class ReticleVisibilityCrp01Experiment
    {
        private const int Width=1206,Height=2622;
        // Portrait iPhone 16 Pro fixture: 62 pt top / 34 pt bottom at native 3x.
        private static readonly Rect SafeArea=new Rect(0,102,Width,Height-186-102);
        private AppBootstrap app;private UnityEngine.Camera camera;
        private VisualElement root,bar;private DevPanelView panel;private ShotOverlay overlay;
        private AimReticleView reticle;private RenderTexture target,previousCameraTarget,previousPanelTarget;
        private PanelSettings settings;private float previousAspect;
#if UNITY_EDITOR
        private UnityEditor.EditorWindow gameView;private object sizeGroup;
        private PropertyInfo selectedSize;private int previousSize,customIndex=-1;
        private void UseNativeGameView()
        {
            var assembly=typeof(UnityEditor.Editor).Assembly;
            var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
            sizeGroup=sizesType.GetProperty("currentGroup").GetValue(sizes);
            var groupType=sizeGroup.GetType();
            customIndex=(int)groupType.GetMethod("GetTotalCount").Invoke(sizeGroup,null);
            var kind=assembly.GetType("UnityEditor.GameViewSizeType");
            var size=Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"),Enum.Parse(kind,"FixedResolution"),Width,Height,"S1 visibility test");
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
            Assert.That(Screen.width,Is.EqualTo(Width));Assert.That(Screen.height,Is.EqualTo(Height));
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(SquashClash.Core.Config.PresetCodec.FromSet(app.Tuning.Defaults,"Visibility fixture",app.Build.Label),new System.Collections.Generic.List<string>());
            app.Tuning.Set("practice.enabled",0);app.LaunchCurrent();app.Host.enabled=false;
            camera=UnityEngine.Camera.main;previousCameraTarget=camera.targetTexture;previousAspect=camera.aspect;
            target=new RenderTexture(Width,Height,24);target.Create();camera.targetTexture=target;camera.aspect=Width/(float)Height;
            var document=app.GetComponent<UIDocument>();settings=document.panelSettings;previousPanelTarget=settings.targetTexture;settings.targetTexture=target;
            root=document.rootVisualElement;panel=Object.FindFirstObjectByType<DevPanelView>();overlay=Object.FindFirstObjectByType<ShotOverlay>();
            reticle=app.GetComponentInChildren<AimReticleView>(true);
            yield return null;yield return null;
            Assert.That(target.width,Is.EqualTo(Width));Assert.That(target.height,Is.EqualTo(Height));
            bar=root.Q("dev-bar");
            // Place the actual DevBar in this explicit safe-area fixture; do not modify camera/defaults.
            bar.style.left=0;bar.style.right=0;bar.style.bottom=SafeArea.yMin/Height*root.layout.height;
            var toggle=root.Q("dev-toggle");toggle.style.top=186f/Height*root.layout.height+6;toggle.style.right=6;
            overlay.PlaceInSafeArea(SafeArea,new Vector2(Width,Height));yield return null;yield return null;
            TestContext.WriteLine($"FIXTURE {Width}x{Height} safe={SafeArea} root={root.worldBound} camera={camera.transform.position} fov={camera.fieldOfView}");
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(settings!=null)settings.targetTexture=previousPanelTarget;
            if(camera!=null){camera.targetTexture=previousCameraTarget;camera.aspect=previousAspect;}
            if(target!=null){target.Release();Object.Destroy(target);}
            yield return PlayModeTestWorld.ClearApp();
#if UNITY_EDITOR
            if(gameView!=null&&selectedSize!=null)selectedSize.SetValue(gameView,previousSize);
            if(sizeGroup!=null&&customIndex>=0)sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup,new object[]{customIndex});
#endif
        }

        private static bool Shown(VisualElement element)
        {
            for(var e=element;e!=null;e=e.parent)
                if(e.resolvedStyle.display==DisplayStyle.None||e.resolvedStyle.visibility==Visibility.Hidden||e.resolvedStyle.opacity==0)return false;
            return element.worldBound.width>0&&element.worldBound.height>0;
        }
        private static string LogPath(string name)
        {
            string dir=Path.Combine(Path.GetDirectoryName(Application.dataPath),"Logs","evidence","crp01");Directory.CreateDirectory(dir);return Path.Combine(dir,name);
        }
        private static readonly string[] Labels={"shot-preview-state","shot-preview-power","shot-preview-sampling","shot-preview-status"};
        private string Sample(string tag,int cycle,string name,int frame,Button button,ScrollView scroll)
        {
            var vp=scroll.contentViewport.worldBound;var b=button.worldBound;
            float labels=0;string lens="";
            foreach(var l in Labels){var e=root.Q<Label>(l);labels+=e.layout.height;lens+=(lens.Length>0?"/":"")+e.text.Length+"@"+e.layout.height.ToString("F1");}
            bool bad=b.yMax>vp.yMax+1||b.yMin<vp.yMin-1;
            return $"{tag} cycle={cycle} btn={name} f={frame} t={Time.realtimeSinceStartup:F3} dt={Time.unscaledDeltaTime*1000f:F1}ms fc={Time.frameCount} btnY=[{b.yMin:F2},{b.yMax:F2}] vpY=[{vp.yMin:F2},{vp.yMax:F2}] contentH={scroll.contentContainer.layout.height:F2} vpH={scroll.contentViewport.layout.height:F2} scrollY={scroll.scrollOffset.y:F2} labelsH={labels:F1} lens={lens} {(bad?"OUT":"in")}";
        }
        private IEnumerator Run(string tag,int cycles,float preWait,string file)
        {
            var preview=app.GetComponent<GesturePreview>();int wouldFail=0,total=0;
            for(int c=0;c<cycles;c++)
            foreach(var name in new[]{"s1-sampling","s1-ruler","s1-soft","s1-strong"})
            {
                var lines=new System.Collections.Generic.List<string>();
                panel.Toggle();yield return null;yield return null;
                var button=root.Q<Button>(name);Assert.That(Shown(button),Is.True,name+" available in DEV");
                var scroll=button.GetFirstAncestorOfType<ScrollView>();
                if(preWait>0)yield return new WaitForSecondsRealtime(preWait);
                lines.Add(Sample(tag,c,name,-1,button,scroll));
                scroll.ScrollTo(button);lines.Add(Sample(tag,c,name,0,button,scroll));
                yield return null;lines.Add(Sample(tag,c,name,1,button,scroll));
                yield return null;lines.Add(Sample(tag,c,name,2,button,scroll));
                var vp=scroll.contentViewport.worldBound;var b=button.worldBound;
                bool fail=b.yMin<vp.yMin-1||b.yMax>vp.yMax+1;total++;if(fail)wouldFail++;
                yield return null;lines.Add(Sample(tag,c,name,3,button,scroll));
                yield return null;lines.Add(Sample(tag,c,name,4,button,scroll));
                yield return new WaitForSecondsRealtime(.3f);lines.Add(Sample(tag,c,name,99,button,scroll));
                lines.Add($"{tag} RESULT cycle={c} btn={name} {(fail?"WOULD_FAIL":"ok")}");
                File.AppendAllLines(LogPath(file),lines);
                button.Focus();using(var e=NavigationSubmitEvent.GetPooled())button.SendEvent(e);yield return null;
                Assert.That(panel.IsOpen,Is.False);
            }
            File.AppendAllText(LogPath(file),$"{tag} SUMMARY total={total} wouldFail={wouldFail}\n");
            TestContext.WriteLine($"{tag} SUMMARY total={total} wouldFail={wouldFail}");
        }
        // Same cadence as the original test (Toggle, 2 frames, ScrollTo, 2 frames), repeated.
        [UnityTest] public IEnumerator A_OriginalCadence_Repeated(){yield return Run("A",25,0,"exp-A.log");}
        // Contrast for H1: let ShotOverlay/DevPanel refresh timers fire before ScrollTo.
        [UnityTest] public IEnumerator B_WaitBeforeScroll_Repeated(){yield return Run("B",25,.35f,"exp-B.log");}
    }
}
