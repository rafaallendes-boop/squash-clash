using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using SquashClash.Core.Config;
namespace SquashClash.Core.Tests.Config
{
    public sealed class PresetStoreTests
    {
        private string directory;private PresetStore store;
        [SetUp] public void Setup(){directory=Path.Combine(Path.GetTempPath(),"squash-presets-"+Guid.NewGuid().ToString("N"));store=new PresetStore(directory);}
        [TearDown] public void Cleanup(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        [Test] public void SaveReloadAndReplace_RestoresLastActiveAndEffectiveValues()
        {
            var p=ConfigFixtures.Default();Assert.That(store.TrySave(PresetCodec.FromSet(p,"local","test"),out var error),Is.True,error);
            Assert.That(store.TrySetLastActive("local",out error),Is.True,error);
            var next=new PresetStore(directory);Assert.That(next.LastActiveName,Is.EqualTo("local"));
            Assert.That(next.TrySave(PresetCodec.FromSet(p.With("launcher.speed",20),"local","test"),out error),Is.True,error);
            Assert.That(next.TryLoad("local",out var file,out error),Is.True,error);
            Assert.That(PresetCodec.Overlay(p,file,new List<string>()).Get("launcher.speed"),Is.EqualTo(20));
            Assert.That(next.List(),Is.EqualTo(new[]{"local"}));Assert.That(File.Exists(Path.Combine(directory,"local.json.tmp")),Is.False);
        }
        [Test] public void InterruptedWrite_KeepsPreviousPresetAndLastActive()
        {
            var p=ConfigFixtures.Default();Assert.That(store.TrySave(PresetCodec.FromSet(p,"good","test"),out _),Is.True);
            Assert.That(store.TrySetLastActive("good",out _),Is.True);
            File.WriteAllText(Path.Combine(directory,"good.json.tmp"),"{not complete");File.WriteAllText(Path.Combine(directory,"last-active.txt.tmp"),"bad");
            Assert.That(store.TryLoad("good",out var f,out _),Is.True);Assert.That(store.LastActiveName,Is.EqualTo("good"));
            Assert.That(store.List(),Is.EqualTo(new[]{"good"}));Assert.That(PresetCodec.LoadDefault(PresetCodec.ToJson(f)).Hash,Is.EqualTo(p.Hash));
        }
        [TestCase("../escape")][TestCase("/escape")][TestCase("a/b")][TestCase("a\\b")][TestCase("")][TestCase("..")]
        public void NamesCannotEscapeDirectory(string name)
        {
            Assert.That(store.TrySave(PresetCodec.FromSet(ConfigFixtures.Default(),name,"test"),out var error),Is.False);Assert.That(error,Is.Not.Empty);
            Assert.That(store.TryLoad(name,out _,out _),Is.False);Assert.That(store.TrySetLastActive(name,out _),Is.False);
        }
        [Test] public void WriteFailure_IsReturnedWithoutThrowingOrDestroyingExistingFile()
        {
            Directory.CreateDirectory(directory);var blocker=Path.Combine(directory,"file");File.WriteAllText(blocker,"keep");var broken=new PresetStore(blocker);
            Assert.That(broken.TrySave(PresetCodec.FromSet(ConfigFixtures.Default(),"bad","test"),out var error),Is.False);Assert.That(error,Is.Not.Empty);
            Assert.That(File.ReadAllText(blocker),Is.EqualTo("keep"));
            Assert.That(store.TrySetLastActive("missing",out _),Is.False);
        }
        [Test] public void Boot_CorruptLastActive_FallsBackToDefaultsWithOneWarning()
        {
            Directory.CreateDirectory(directory);File.WriteAllText(Path.Combine(directory,"last-active.txt"),"bad");File.WriteAllText(Path.Combine(directory,"bad.json"),"{not json");
            var warnings=new List<string>();var d=ConfigFixtures.Default();var loaded=store.LoadStartup(d,warnings,out var name,out var hash);
            Assert.That(loaded.Hash,Is.EqualTo(d.Hash));Assert.That(warnings,Has.Count.EqualTo(1));Assert.That(name,Is.EqualTo("Default"));Assert.That(hash,Is.Null);
        }
        [Test] public void Boot_ValidPartialOverlayUsesDefaultsAndHasNoPending()
        {
            var p=ConfigFixtures.Default();var f=new PresetFile{schemaVersion=1,name="partial",values=new List<PresetEntry>{new PresetEntry{key="launcher.speed",value=20}}};
            Assert.That(store.TrySave(f,out var error),Is.True,error);Assert.That(store.TrySetLastActive("partial",out error),Is.True,error);
            var warnings=new List<string>();var applied=store.LoadStartup(p,warnings,out var name,out var hash);var tuning=new TuningState(p,applied);
            Assert.That(applied.Hash,Is.EqualTo(p.With("launcher.speed",20).Hash));Assert.That(tuning.PendingKeys,Is.Empty);Assert.That(name,Is.EqualTo("partial"));Assert.That(warnings,Is.Empty);
            using(var sha=SHA256.Create())Assert.That(hash,Is.EqualTo(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(directory,"partial.json")))).Replace("-","").ToLowerInvariant()));
        }
    }
}
