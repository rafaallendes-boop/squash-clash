using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
using SquashClash.DevPanel;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests
{
    public sealed class DevPanelViewTests
    {
        private string presetName,activePath,activeBackup;private bool hadSelection;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();
            activePath=Path.Combine(Application.persistentDataPath,"presets","last-active.txt");hadSelection=File.Exists(activePath);activeBackup=hadSelection?File.ReadAllText(activePath):null;
            presetName="i5b-test-"+Guid.NewGuid().ToString("N");yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return PlayModeTestWorld.ClearApp();File.Delete(Path.Combine(Application.persistentDataPath,"presets",presetName+".json"));
            if(hadSelection)File.WriteAllText(activePath,activeBackup);else File.Delete(activePath);
        }
        [UnityTest] public IEnumerator PanelToggleKeepsBarAndResumeOverlayAboveControls()
        {
            var app=AppBootstrap.Instance;var root=app.GetComponent<UIDocument>().rootVisualElement;var view=Object.FindFirstObjectByType<DevPanelView>();
            Assert.That(view.IsOpen,Is.False);Assert.That(root.Q<Button>("dev-toggle"),Is.Not.Null);view.Toggle();Assert.That(view.IsOpen,Is.True);
            Assert.That(root.Q("dev-bar").resolvedStyle.display,Is.Not.EqualTo(DisplayStyle.None));
            app.GetComponent<ResumeOverlay>().Suspend();view.Toggle();Assert.That(view.IsOpen,Is.True,"Gesture cannot bypass resume overlay");
            Assert.That(root.Children().Last().name,Is.EqualTo("resume-overlay"));app.GetComponent<ResumeOverlay>().Continue();view.Toggle();Assert.That(view.IsOpen,Is.False);yield return null;
        }
        [UnityTest] public IEnumerator GeneratedControlsEditPendingAndImmediateAndFlagP1ImpactCap()
        {
            var app=AppBootstrap.Instance;string error;var section=new ParamSection(app,()=>{});
            app.GetComponent<UIDocument>().rootVisualElement.Add(section.Root);yield return null;
            Assert.That(section.Root.Q<Slider>("physics.floor.restitution"),Is.Not.Null);
            section.Root.Q<Slider>("physics.floor.restitution").value=.6f;Assert.That(app.Tuning.PendingKeys,Does.Contain("physics.floor.restitution"));
            section.Root.Q<Slider>("camera.fovDeg").value=70;Assert.That(UnityEngine.Camera.main.fieldOfView,Is.EqualTo(70));
            Assert.That(section.TrySet("physics.backend",.5,out error),Is.False);Assert.That(error,Is.Not.Empty);
            Assert.That(section.TrySet("camera.height",double.NaN,out error),Is.False);Assert.That(error,Is.Not.Empty);
            section.Root.Q<DropdownField>("physics.backend").value="P1-PhysX";app.LaunchCurrent();section.Refresh();
            Assert.That(section.Root.Q<SliderInt>("physics.maxImpactsPerStep").enabledInHierarchy,Is.False);
            Assert.That(section.Root.Query<Label>().ToList().Any(l=>l.text.Contains("no operativo con P1")),Is.True);
            app.Tuning.Set("physics.backend",1);app.LaunchCurrent();section.Refresh();Assert.That(section.Root.Q<SliderInt>("physics.maxImpactsPerStep").enabledInHierarchy,Is.True);yield return null;
        }
        [UnityTest] public IEnumerator PresetsSaveVisibleRecoverOnBootExportImportAndRejectReservedName()
        {
            var app=AppBootstrap.Instance;var section=new PresetsSection(app,()=>{});app.Tuning.Set("physics.floor.restitution",.6);
            Assert.That(section.SaveAs("Default"),Is.False);Assert.That(section.Status,Is.Not.Empty);
            Assert.That(section.SaveAs(presetName),Is.True,section.Status);Assert.That(app.Tuning.PendingKeys,Does.Contain("physics.floor.restitution"));
            var exported=section.Export();Assert.That(PresetCodec.TryParse(exported,out var f,out _),Is.True);Assert.That(f.values.Single(e=>e.key=="physics.floor.restitution").value,Is.EqualTo(.6));
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;Assert.That(app.ActivePresetName,Is.EqualTo(presetName));Assert.That(app.Config.Physics.Floor.Restitution,Is.EqualTo(.6f));Assert.That(app.Tuning.PendingKeys,Is.Empty);
            section=new PresetsSection(app,()=>{});string before=app.Tuning.Visible.Hash;Assert.That(section.Import("bad-json"),Is.False);Assert.That(app.Tuning.Visible.Hash,Is.EqualTo(before));
            f.values.Add(new PresetEntry{key="future.key",value=5});Assert.That(section.Import(PresetCodec.ToJson(f)),Is.True);Assert.That(section.Status,Does.Contain("future.key"));
            Assert.That(section.Load("Default"),Is.True);app.LaunchCurrent();Assert.That(app.Config.Hash,Is.EqualTo(app.Tuning.Defaults.Hash));Assert.That(app.Presets.LastActiveName,Is.Null);yield return null;
        }
        [UnityTest] public IEnumerator ExpandedGroupsStayBelowTabsAndPresetsCanScrollToSave() { yield return CheckResponsivePanel(390,304); }
        [UnityTest] public IEnumerator CompactPanelKeepsTabsAndSaveAccessible() { yield return CheckResponsivePanel(320,200); }
        private IEnumerator CheckResponsivePanel(float width,float height)
        {
            var app=AppBootstrap.Instance;var root=app.GetComponent<UIDocument>().rootVisualElement;
            var view=Object.FindFirstObjectByType<DevPanelView>();view.Toggle();
            // Allocate a phone-width lower panel independently of the batch Game view dimensions.
            root.style.width=width;root.style.height=844;yield return null;yield return null;
            var panel=root.Q("dev-panel");panel.style.top=844-120-height;panel.style.bottom=120;panel.style.left=6;panel.style.right=6;
            foreach(var fold in panel.Query<Foldout>().ToList())fold.value=true;
            yield return null;yield return null;
            var tabs=panel.Query<Button>().ToList().Where(b=>b.text=="Parámetros"||b.text=="Presets"||b.text=="Diagnóstico").ToArray();
            var scroll=panel.Q<ScrollView>("parameters-scroll");
            TestContext.WriteLine($"LAYOUT tabsBottom={tabs.Max(t=>t.worldBound.yMax)} viewportTop={scroll.contentViewport.worldBound.yMin} viewportBottom={scroll.contentViewport.worldBound.yMax} panelBottom={panel.worldBound.yMax}");
            Assert.That(scroll.contentViewport.worldBound.height,Is.GreaterThan(40),"Expanded parameters need a bounded, usable viewport");
            Assert.That(scroll.contentViewport.worldBound.yMax,Is.LessThanOrEqualTo(panel.worldBound.yMax+1),"Content cannot paint over the fixed bar");
            Assert.That(scroll.contentViewport.worldBound.yMin,Is.GreaterThanOrEqualTo(tabs.Max(t=>t.worldBound.yMax)-1),"Scroll content must start below tabs, without stealing their input");
            var presetsTab=tabs.Single(b=>b.text=="Presets");
            var picked=root.panel.Pick(presetsTab.worldBound.center);
            Assert.That(picked==presetsTab||picked.GetFirstAncestorOfType<Button>()==presetsTab,Is.True,"Presets tab must receive pointer input");
            presetsTab.Focus();
            using(var e=NavigationSubmitEvent.GetPooled())presetsTab.SendEvent(e);
            yield return null;yield return null;
            var name=panel.Q<TextField>("preset-name");var save=panel.Q<Button>("preset-save");
            Assert.That(name.visible&&name.resolvedStyle.display!=DisplayStyle.None,Is.True);
            var presetScroll=name.GetFirstAncestorOfType<ScrollView>();Assert.That(presetScroll.resolvedStyle.display,Is.EqualTo(DisplayStyle.Flex));presetScroll.ScrollTo(name);yield return null;yield return null;
            Assert.That(name.worldBound.xMin,Is.GreaterThanOrEqualTo(panel.worldBound.xMin));Assert.That(name.worldBound.xMax,Is.LessThanOrEqualTo(panel.worldBound.xMax));
            Assert.That(name.worldBound.yMin,Is.GreaterThanOrEqualTo(presetScroll.contentViewport.worldBound.yMin-1));
            Assert.That(name.worldBound.yMax,Is.LessThanOrEqualTo(presetScroll.contentViewport.worldBound.yMax+1),"Save name must be reachable");
            presetScroll.ScrollTo(save);yield return null;yield return null;
            Assert.That(save.worldBound.yMax,Is.LessThanOrEqualTo(presetScroll.contentViewport.worldBound.yMax+1),"Save button must be reachable");
            name.value=presetName;save.Focus();using(var e=NavigationSubmitEvent.GetPooled())save.SendEvent(e);yield return null;
            Assert.That(app.Presets.List(),Does.Contain(presetName),"The visible save button must execute its action");
        }
        [UnityTest] public IEnumerator NumericInputIsNotOverwrittenDuringEditing()
        {
            var app=AppBootstrap.Instance;var root=app.GetComponent<UIDocument>().rootVisualElement;
            var section=new ParamSection(app,()=>{});root.Add(section.Root);
            foreach(var fold in section.Root.Query<Foldout>().ToList())fold.value=true;
            yield return null;
            var slider=section.Root.Q<Slider>("physics.floor.restitution");var field=slider.Q<TextField>();field.Focus();yield return null;
            field.SelectAll();
            foreach(char c in "0.")
            {
                using(var e=KeyDownEvent.GetPooled(new Event {type=EventType.KeyDown,character=c,keyCode=KeyCode.None}))field.Q("unity-text-input").SendEvent(e);
            }
            section.Refresh();
            TestContext.WriteLine("EDIT text="+field.text+" value="+field.value+" applied="+app.Tuning.Visible.Get("physics.floor.restitution"));
            Assert.That(app.Tuning.Visible.Get("physics.floor.restitution"),Is.EqualTo(.45),"Typing an unfinished decimal must not alter tuning");
            Assert.That(field.text,Is.EqualTo("0."),"Refresh must preserve unfinished text");
            foreach(char c in "60")
            {
                using(var e=KeyDownEvent.GetPooled(new Event {type=EventType.KeyDown,character=c,keyCode=KeyCode.None}))field.Q("unity-text-input").SendEvent(e);
            }
            using(var e=KeyDownEvent.GetPooled(new Event {type=EventType.KeyDown,keyCode=KeyCode.Return,character='\n'}))field.Q("unity-text-input").SendEvent(e);
            field.Blur();yield return null;section.Refresh();
            Assert.That(app.Tuning.Visible.Get("physics.floor.restitution"),Is.EqualTo(.6));
        }
        [UnityTest] public IEnumerator ThreeFingerHoldFiresOnceAfterOneSecondAndResetsOnReplacementOrFocus()
        {
            var go=new GameObject("gesture test");var gesture=go.AddComponent<ThreeFingerHold>();gesture.enabled=false;
            Assert.That(gesture.Sample(3,.9,0),Is.False);Assert.That(gesture.Sample(3,1,0),Is.True);Assert.That(gesture.Sample(3,2,0),Is.False);
            gesture.Sample(2,2,0);Assert.That(gesture.Sample(3,2.9,2),Is.False);Assert.That(gesture.Sample(3,3,2),Is.True);
            gesture.Sample(3,4,2,false);Assert.That(gesture.Sample(3,4,3.5),Is.False);Assert.That(gesture.Sample(3,4.5,3.5),Is.True);
            gesture.Sample(0,5,0);gesture.Sample(3,5.9,5);Assert.That(gesture.Sample(3,6,5.5),Is.False);Assert.That(gesture.Sample(3,6.5,5.5),Is.True);
            Object.Destroy(go);yield return null;
        }
        [UnityTest] public IEnumerator DiagnosticsKeepsLastEightContactsAndShowsGeometryTimingAndInvalidMetrics()
        {
            var app=AppBootstrap.Instance;using(var section=new DiagnosticsSection(app))
            {
                for(int i=0;i<10;i++)section.Record(new ContactEvent(SurfaceId.FrontWall,Vector3.zero,new Vector2(3.2f,app.Config.Court.TinTop),Vector3.back,i,.001f,ContactTiming.Exact,Vector3.forward,false));
                app.Tuning.Set("debug.slowMotion",.5);section.Refresh();
                Assert.That(section.ContactText.Split('\n').Length,Is.EqualTo(8));Assert.That(section.ContactText,Does.Contain("Tin").And.Contain("Exact").And.Contain("UV"));
                Assert.That(section.ContactText,Does.Not.Contain("#0 ").And.Not.Contain("#1 "));
                Assert.That(section.Summary,Does.Contain("MÉTRICAS NO VÁLIDAS").And.Contain("CPU").And.Contain("GPU").And.Contain("unknown").And.Contain("p99"));
            }
            yield return null;
        }
    }
}
