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
    public sealed class ReticleVisibilityTests
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
            app.LaunchCurrent();app.Host.enabled=false;
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
        private Rect FrontisRect()
        {
            var d=app.Config.Court;
            var points=new[]{new Vector3(0,0,0),new Vector3(d.Width,0,0),new Vector3(0,d.FrontOutLower,0),new Vector3(d.Width,d.FrontOutLower,0)}
                .Select(p=>camera.WorldToViewportPoint(p)).Select(p=>new Vector2(root.worldBound.xMin+p.x*root.layout.width,root.worldBound.yMin+(1-p.y)*root.layout.height)).ToArray();
            return Rect.MinMaxRect(points.Min(p=>p.x),points.Min(p=>p.y),points.Max(p=>p.x),points.Max(p=>p.y));
        }
        private static bool Shown(VisualElement element)
        {
            for(var e=element;e!=null;e=e.parent)
                if(e.resolvedStyle.display==DisplayStyle.None||e.resolvedStyle.visibility==Visibility.Hidden||e.resolvedStyle.opacity==0)return false;
            return element.worldBound.width>0&&element.worldBound.height>0;
        }
        [UnityTest] public IEnumerator DevClosed_AllShotOverlayElements_ClearWholeProjectedFrontis()
        {
            Assert.That(panel.IsOpen,Is.False);var front=FrontisRect();var layer=root.Q("shot-overlay");
            var visible=layer.Query<VisualElement>().ToList().Prepend(layer).Where(Shown).Distinct().ToArray();
            Assert.That(visible,Is.Not.Empty);
            TestContext.WriteLine($"FRONTIS {front} OVERLAY {layer.worldBound}");
            foreach(var e in visible)Assert.That(e.worldBound.Overlaps(front),Is.False,$"{e.name} {e.worldBound} covers projected frontis {front}");
            yield return null;
        }
        [UnityTest] public IEnumerator AimingCenter_AtDefaultCamera_Has60PixelDiameterAnd6PixelStroke()
        {
            reticle.gameObject.SetActive(true);reticle.SetAim(new Vector2(app.Config.Court.Width/2,2.2f),AimState.Aiming,app.Config.Court);
            yield return null;
            var line=reticle.GetComponent<LineRenderer>();var points=Enumerable.Range(0,line.positionCount).Select(i=>camera.WorldToScreenPoint(reticle.transform.TransformPoint(line.GetPosition(i)))).ToArray();
            float diameterX=points.Max(p=>p.x)-points.Min(p=>p.x),diameterY=points.Max(p=>p.y)-points.Min(p=>p.y);
            float diameter=Mathf.Min(diameterX,diameterY);
            var center=reticle.transform.position;float stroke=(camera.WorldToScreenPoint(center+camera.transform.right*line.widthMultiplier/2)-camera.WorldToScreenPoint(center-camera.transform.right*line.widthMultiplier/2)).magnitude;
            TestContext.WriteLine($"RETICLE diameterX={diameterX:R} diameterY={diameterY:R} diameterMinPx={diameter:R} strokePx={stroke:R}");
            Assert.That(diameter>=60&&stroke>=6,Is.True,$"Native projection: diameter {diameter:R} px must be >=60; stroke {stroke:R} px must be >=6");
        }
        [UnityTest] public IEnumerator CircleDiamondAndCyanYellowRed_ArePreserved()
        {
            reticle.gameObject.SetActive(true);reticle.SetAim(new Vector2(app.Config.Court.Width/2,2.2f),AimState.Aiming,app.Config.Court);
            var line=reticle.GetComponent<LineRenderer>();Assert.That(line.positionCount,Is.EqualTo(33));Assert.That(line.startColor,Is.EqualTo(Color.cyan));
            yield return Capture("aiming");
            reticle.SetAim(reticle.Aim,AimState.Frozen,app.Config.Court);Assert.That(line.positionCount,Is.EqualTo(5));Assert.That(line.startColor,Is.EqualTo(Color.yellow));
            yield return Capture("frozen");
            reticle.SetAim(new Vector2(3.2f,4.8f),AimState.Aiming,app.Config.Court);Assert.That(line.startColor,Is.EqualTo(Color.red));Assert.That(reticle.IsInvalid,Is.True);
            yield return Capture("invalid");
        }
        private IEnumerator Capture(string state)
        {
            yield return new WaitForSecondsRealtime(.2f);yield return null;
            var previous=RenderTexture.active;var image=new Texture2D(Width,Height,TextureFormat.RGB24,false);
            try
            {
                RenderTexture.active=target;image.ReadPixels(new Rect(0,0,Width,Height),0,0);image.Apply();
                var directory=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Logs","evidence","reticle-visibility"));Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory,"editor-"+state+".png"),image.EncodeToPNG());
            }
            finally {RenderTexture.active=previous;Object.Destroy(image);}
        }
        [UnityTest] public IEnumerator DevClosed_OnlyCompactStateAndPower_AreInsideSafeAreaAboveBar()
        {
            var layer=root.Q("shot-overlay");
            Assert.That(layer.Query<Button>().ToList().Any(Shown),Is.False,"Protocol buttons belong only to open DEV");
            Assert.That(layer.Query<Label>().ToList().Count(Shown),Is.EqualTo(1),"One compact line when DEV is closed");
            var safeUi=Rect.MinMaxRect(root.worldBound.xMin,root.worldBound.yMin+186f/Height*root.layout.height,root.worldBound.xMax,root.worldBound.yMax-102f/Height*root.layout.height);
            Assert.That(layer.worldBound.yMax,Is.LessThanOrEqualTo(bar.worldBound.yMin));
            Assert.That(safeUi.Contains(layer.worldBound.min)&&safeUi.Contains(layer.worldBound.max-Vector2.one*.01f),Is.True);
            yield return null;
        }
        [UnityTest] public IEnumerator SlingFeedbackAndStatus_StayOutsideFrontis_AndTargetMatchesScreenSide()
        {
            var preview=app.GetComponent<GesturePreview>();
            app.Tuning.Set("gesture.control",1);app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            Assert.That(preview.ArmSlingBlock("B1"),Is.True);preview.SlingTrials.CompleteAttempt();
            yield return new WaitForSecondsRealtime(.25f);yield return null;
            var front=FrontisRect();
            foreach(var name in new[]{"sling-status","shot-overlay","sling-ok","sling-premature"})
            {
                var e=root.Q(name);Assert.That(e,Is.Not.Null,name);Assert.That(Shown(e),Is.True,name);
                Assert.That(e.worldBound.Overlaps(front),Is.False,name+" must clear the whole frontis");
                Assert.That(e.worldBound.yMax,Is.LessThanOrEqualTo(bar.worldBound.yMin));
            }
            var t1=preview.TargetPosition(1,app.Tuning.Applied);var t2=preview.TargetPosition(2,app.Tuning.Applied);
            Assert.That(camera.WorldToScreenPoint(new Vector3(t1.x,t1.y,0)).x,Is.LessThan(camera.WorldToScreenPoint(new Vector3(t2.x,t2.y,0)).x));
            Assert.That(t1.y,Is.GreaterThan(app.Config.Court.TinTop));Assert.That(t1.y,Is.LessThan(app.Config.Court.FrontOutLower));
            yield return Capture("sling-feedback");
        }
        [UnityTest] public IEnumerator RejectedBlockNoticeAfterInterruptedB0_IsContainedAndClearsFrontis()
        {
            var preview=app.GetComponent<GesturePreview>();app.Tuning.Set("gesture.control",1);app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            Assert.That(preview.ArmSlingBlock("B0"),Is.True);app.Clock.Paused=true;preview.Tick(Time.realtimeSinceStartupAsDouble);
            Assert.That(preview.ArmSlingBlock("B1"),Is.False);yield return new WaitForSecondsRealtime(.25f);yield return null;
            overlay.PlaceInSafeArea(SafeArea,new Vector2(Width,Height));yield return null;yield return null;
            var layer=root.Q("shot-overlay");var guide=root.Q<Label>("sling-protocol-guide");Assert.That(guide.text,Does.Contain("Continuar"));
            Assert.That(guide.worldBound.yMax,Is.LessThanOrEqualTo(layer.worldBound.yMax+.5f),"Rejection remedy must fit inside its reserved overlay height");
            foreach(var e in layer.Query<VisualElement>().ToList().Prepend(layer).Where(Shown))Assert.That(e.worldBound.Overlaps(FrontisRect()),Is.False,e.name);
        }
        [UnityTest] public IEnumerator LocalCancelLimitAndState_AreVisibleAndNeverCoverFrontis()
        {
            var preview=app.GetComponent<GesturePreview>();app.Tuning.Set("gesture.control",1);app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            Assert.That(preview.ArmSlingBlock("B0"),Is.True);yield return new WaitForSecondsRealtime(.25f);yield return null;
            foreach(var name in new[]{"sling-protocol-timer","sling-protocol-guide"})
            {var e=root.Q(name);Assert.That(Shown(e),Is.True,name);Assert.That(e.worldBound.Overlaps(FrontisRect()),Is.False,name);}
            Assert.That(root.Q("sling-protocol-guide").worldBound.yMax,Is.LessThanOrEqualTo(root.Q("shot-overlay").worldBound.yMax),"Guide must fit its visible background");
            root.Q<Label>("build-label").text="Editor · D2 sintético";
            float mm=SquashClash.Runtime.Input.ScreenMetrics.MmPerPx(Screen.dpi,460,out _);
            float y=camera.WorldToScreenPoint(new Vector3(3.2f,2.2f,0)).y-12/mm;
            var model=new SquashClash.Core.Shot.SlingModel(SquashClash.Core.Shot.SlingSettings.From(app.Tuning.Applied),app.Config.Court,3.2f,SafeArea,mm);
            System.Action<double,float,SquashClash.Core.Shot.TouchPhaseKind> send=(t,py,phase)=>model.Push(new SquashClash.Core.Shot.TouchSample(1,phase,new Vector2(Width/2,py),t,0));
            send(0,y-3/mm,SquashClash.Core.Shot.TouchPhaseKind.Began);send(.02,y,SquashClash.Core.Shot.TouchPhaseKind.Moved);send(.04,y,SquashClash.Core.Shot.TouchPhaseKind.Stationary);send(.06,y,SquashClash.Core.Shot.TouchPhaseKind.Stationary);model.Tick(.15);Assert.That(model.Locked,Is.True);
            var indicator=new SquashClash.Runtime.Shot.SlingCancelIndicator(root);indicator.Show(model,app.Tuning.Applied,app.Config.Court,SafeArea,mm);yield return null;yield return null;
            var label=root.Q<Label>("sling-cancel-local");Assert.That(label,Is.Not.Null);Assert.That(Shown(label),Is.True);Assert.That(label.text,Does.Contain("Límite").And.Contain("FUERA"));
            var front=FrontisRect();var visible=root.Query<VisualElement>().ToList().Where(e=>e.name.StartsWith("sling-cancel-")&&Shown(e)).ToArray();
            Assert.That(visible.Length,Is.GreaterThanOrEqualTo(2),"Local card plus a real limit segment");foreach(var e in visible)Assert.That(e.worldBound.Overlaps(front),Is.False,e.name);
            send(.2,y+13/mm,SquashClash.Core.Shot.TouchPhaseKind.Moved);indicator.Show(model,app.Tuning.Applied,app.Config.Court,SafeArea,mm);Assert.That(label.text,Does.Contain("DENTRO").And.Contain("suelta"));
            yield return Capture("sling-local-cancel");indicator.Dispose();
        }
        [UnityTest] public IEnumerator DevOpen_ProtocolActionsRemainUsable_AndCloseDevBeforeArming()
        {
            var preview=app.GetComponent<GesturePreview>();
            foreach(var name in new[]{"s1-sampling","s1-ruler","s1-soft","s1-strong"})
            {
                panel.Toggle();yield return null;yield return null;
                var button=root.Q<Button>(name);Assert.That(Shown(button),Is.True,name+" available in DEV");
                var scroll=button.GetFirstAncestorOfType<ScrollView>();Assert.That(scroll,Is.Not.Null,"Protocols remain reachable in the DEV scroll view");
                scroll.ScrollTo(button);yield return null;yield return null;
                Assert.That(button.worldBound.yMin,Is.GreaterThanOrEqualTo(scroll.contentViewport.worldBound.yMin-1));
                Assert.That(button.worldBound.yMax,Is.LessThanOrEqualTo(scroll.contentViewport.worldBound.yMax+1));
                button.Focus();using(var e=NavigationSubmitEvent.GetPooled())button.SendEvent(e);yield return null;
                Assert.That(panel.IsOpen,Is.False,"Starting a protocol closes DEV so capture can start");
                if(name=="s1-sampling")Assert.That(preview.Sampling,Is.True);
                else Assert.That(preview.Status,Does.Contain(name=="s1-ruler"?"50 mm":name=="s1-soft"?"suaves":"fuertes"));
            }
        }
        [UnityTest] public IEnumerator M24FeedbackAndPlainCancelGuide_ClearProjectedFrontisAtNativeViewport()
        {
            var preview=app.GetComponent<GesturePreview>();app.Tuning.Set("gesture.control",2);app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            Assert.That(preview.ArmSlingBlock("B0"),Is.True);yield return new WaitForSecondsRealtime(.25f);
            Assert.That(root.Q<Label>("sling-protocol-guide").text,Does.Contain("apoyo"));
            Assert.That(preview.ArmSlingBlock("B1"),Is.True);preview.SlingTrials.CompleteAttempt();yield return new WaitForSecondsRealtime(.25f);yield return null;
            foreach(string name in new[]{"shot-overlay","sling-status","sling-ok","sling-premature","sling-failed"})
            {
                var e=root.Q(name);Assert.That(Shown(e),Is.True,name);Assert.That(e.worldBound.Overlaps(FrontisRect()),Is.False,name);
                Assert.That(e.worldBound.yMax,Is.LessThanOrEqualTo(bar.worldBound.yMin),name);
            }
            Assert.That(root.Q<Button>("sling-premature").text,Is.EqualTo("Apoyo sin querer"));
            yield return Capture("m24-three-responses");
        }
    }
}
