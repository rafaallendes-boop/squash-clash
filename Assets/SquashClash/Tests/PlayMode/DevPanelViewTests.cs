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
