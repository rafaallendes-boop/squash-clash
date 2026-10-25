using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.DevPanel;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace SquashClash.Runtime.Tests
{
    public sealed class PresetsSectionValidationTests
    {
        private AppBootstrap app;
        private PresetsSection section;
        private string savedName,badName,activePath,presetDirectory;
        private byte[] originalSelection;
        private bool hadSelection;
        private int notices;

        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();
            presetDirectory=Path.Combine(Application.persistentDataPath,"presets");activePath=Path.Combine(presetDirectory,"last-active.txt");
            hadSelection=File.Exists(activePath);originalSelection=hadSelection?File.ReadAllBytes(activePath):null;
            string suffix=Guid.NewGuid().ToString("N");savedName="s1-valid-"+suffix;badName="s1-invalid-"+suffix;
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.Host.enabled=false;
            section=new PresetsSection(app,()=>notices++);
            app.GetComponent<UIDocument>().rootVisualElement.Add(section.Root);
            app.Tuning.Set("physics.floor.restitution",app.Tuning.Applied.Get("physics.floor.restitution")==.6?.7:.6);
            app.Tuning.Set("camera.fovDeg",app.Tuning.Applied.Get("camera.fovDeg")==70?75:70);
            Assert.That(section.SaveAs(savedName),Is.True,section.Status);notices=0;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return PlayModeTestWorld.ClearApp();
            File.Delete(Path.Combine(presetDirectory,savedName+".json"));File.Delete(Path.Combine(presetDirectory,badName+".json"));
            if(hadSelection)File.WriteAllBytes(activePath,originalSelection);else File.Delete(activePath);
        }
        private PresetFile Incoherent(double full)
        {
            var set=app.Tuning.Visible.With("power.deadZoneMmPerS",400).With("power.fullMmPerS",full);
            return PresetCodec.FromSet(set,badName,"validation-test");
        }
        private Action AssertStateUnchangedAfter()
        {
            var visible=app.Tuning.Visible;var applied=app.Tuning.Applied;var pending=app.Tuning.PendingKeys.ToArray();
            string name=app.ActivePresetName,hash=app.ActivePresetFileHash,selection=app.Presets.LastActiveName;
            byte[] selectedBytes=File.ReadAllBytes(activePath),savedBytes=File.ReadAllBytes(Path.Combine(presetDirectory,savedName+".json"));
            string selectedControl=section.Root.Q<DropdownField>("preset-list").value;
            var config=app.Config;var flight=app.Host.ActiveConfig;var prediction=app.Host.Prediction;
            int changed=0;app.Tuning.Changed+=_=>changed++;
            return ()=>
            {
                Assert.That(app.Tuning.Visible,Is.SameAs(visible));Assert.That(app.Tuning.Applied,Is.SameAs(applied));
                Assert.That(app.Tuning.PendingKeys,Is.EquivalentTo(pending));Assert.That(changed,Is.Zero);
                Assert.That(app.ActivePresetName,Is.EqualTo(name));Assert.That(app.ActivePresetFileHash,Is.EqualTo(hash));
                Assert.That(app.Presets.LastActiveName,Is.EqualTo(selection));Assert.That(File.ReadAllBytes(activePath),Is.EqualTo(selectedBytes));
                Assert.That(File.ReadAllBytes(Path.Combine(presetDirectory,savedName+".json")),Is.EqualTo(savedBytes));
                Assert.That(section.Root.Q<DropdownField>("preset-list").value,Is.EqualTo(selectedControl));
                Assert.That(app.Config,Is.SameAs(config));Assert.That(app.Host.ActiveConfig,Is.SameAs(flight));Assert.That(app.Host.Prediction,Is.SameAs(prediction));
                Assert.That(section.Root.Q<Label>("preset-status").text,Is.EqualTo(section.Status));Assert.That(notices,Is.EqualTo(1));
            };
        }
        [UnityTest] public IEnumerator Import_IncoherentPreset_ShowsErrorAndPreview_WithoutMutatingStateOrSelection()
        {
            string json=PresetCodec.ToJson(Incoherent(300));var unchanged=AssertStateUnchangedAfter();bool result=true;
            Assert.DoesNotThrow(()=>result=section.Import(json));
            Assert.That(result,Is.False);Assert.That(section.Status,Does.Contain("No se importó").And.Contain("power.deadZoneMmPerS").And.Contain("power.fullMmPerS"));
            AssertImportPreview(json);unchanged();yield return null;
        }
        [UnityTest] public IEnumerator Load_IncoherentPreset_ShowsError_WithoutMutatingStateOrSelection()
        {
            Assert.That(app.Presets.TrySave(Incoherent(400),out var error),Is.True,error);
            var unchanged=AssertStateUnchangedAfter();bool result=true;
            Assert.DoesNotThrow(()=>result=section.Load(badName));
            Assert.That(result,Is.False);Assert.That(section.Status,Does.Contain("No se cargó").And.Contain("power.deadZoneMmPerS").And.Contain("power.fullMmPerS"));
            unchanged();yield return null;
        }
        [UnityTest] public IEnumerator Import_MalformedText_ShowsLengthAndBoundedEscapedPrefix_WithoutMutatingState()
        {
            string text="texto pegado que no es JSON\r\n\t"+new string('x',100)+"END-NOT-IN-PREVIEW";
            var unchanged=AssertStateUnchangedAfter();Assert.That(section.Import(text),Is.False);
            AssertImportPreview(text);Assert.That(section.Status,Does.Not.Contain("END-NOT-IN-PREVIEW"));unchanged();yield return null;
        }
        private void AssertImportPreview(string text)
        {
            string prefix=text.Substring(0,Math.Min(80,text.Length)).Replace("\r","\\r").Replace("\n","\\n").Replace("\t","\\t");
            Assert.That(section.Status,Does.Contain("longitud: "+text.Length).And.Contain("inicio: «"+prefix+"»"));
        }
    }
}
