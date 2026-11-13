using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class S2DefaultPromotionTests
    {
        private ParamSet Defaults=>ConfigFixtures.Default();
        [Test] public void RollbackHistory_HasApprovedDefaultAndRange()
        {Assert.That(ParamCatalog.TryGet("rollback.historyMs",out var def),Is.True);Assert.That(Defaults.Get("rollback.historyMs"),Is.EqualTo(250));Assert.That(def.Min,Is.EqualTo(100));Assert.That(def.Max,Is.EqualTo(500));}
        [Test] public void Defaults_SelectM24_Control2()=>Assert.That(Defaults.GetInt("gesture.control"),Is.EqualTo(2));
        [Test] public void DefaultChange_OnlyGestureControl_AmongExistingValues()
        {
            var json=File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Tests/Fixtures/pre-s2-defaults.json"));
            Assert.That(PresetCodec.TryParse(json,out var old,out var error),Is.True,error);
            var changed=old.values.Where(e=>Defaults.Get(e.key)!=e.value).Select(e=>e.key).ToArray();
            Assert.That(changed,Is.EqualTo(new[]{"gesture.control"}));
            Assert.That(Defaults.HashOf(ConfigFixtures.FirstBallPrefixes),Is.EqualTo("0d39e6016bd8"));
            Assert.That(EffectiveConfig.From(Defaults).SimulationKey,Is.EqualTo("166a96e319c7"));
        }
        [Test] public void M24DistanceSettings_Stay2And35_N2StrictSettings()
        {
            Assert.That(Defaults.Get("power.pullDeadZoneMm"),Is.EqualTo(2));Assert.That(Defaults.Get("power.pullFullMm"),Is.EqualTo(35));
            Assert.That(Defaults.Get("gesture.tapMaxMm"),Is.EqualTo(3));Assert.That(Defaults.Get("gesture.tapMaxMs"),Is.EqualTo(250));
        }
        [TestCase(0)][TestCase(1)] public void ExplicitAlternativePreset_RemainsSelectable(int control)
        {
            var f=new PresetFile{schemaVersion=1,name="explicit",values=new List<PresetEntry>{new PresetEntry{key="gesture.control",value=control}}};
            Assert.That(PresetCodec.Overlay(Defaults,f,new List<string>()).GetInt("gesture.control"),Is.EqualTo(control));
        }
        [Test] public void LegacyP0Overlay_UsesM24_WithoutRewritingPresetsOrSelection()
        {
            var dir=Path.Combine(Path.GetTempPath(),"s2-promotion-"+Guid.NewGuid().ToString("N"));
            try
            {
                var store=new PresetStore(dir);
                var legacy=PresetCodec.FromSet(Defaults,"P0-calib-1","fixture");
                legacy.values=legacy.values.Where(e=>ConfigFixtures.IsFirstBallKey(e.key)).ToList();
                legacy.values.Single(e=>e.key=="launcher.repeatSeconds").value=3;
                Assert.That(legacy.values.Count,Is.EqualTo(41));Assert.That(store.TrySave(legacy,out var error),Is.True,error);
                Assert.That(store.TrySave(PresetCodec.FromSet(Defaults.With("gesture.control",0),"Po-1","fixture"),out error),Is.True,error);
                Assert.That(store.TrySetLastActive("P0-calib-1",out error),Is.True,error);
                var before=Directory.GetFiles(dir).ToDictionary(p=>p,File.ReadAllBytes);
                var p0=new PresetStore(dir).LoadStartup(Defaults,new List<string>(),out var name,out _);
                Assert.That(p0.Hash,Is.EqualTo("b6914a349068"));Assert.That(name,Is.EqualTo("P0-calib-1"));Assert.That(p0.GetInt("gesture.control"),Is.EqualTo(2));
                Assert.That(p0.HashOf(ConfigFixtures.FirstBallPrefixes),Is.EqualTo("8de3f2ecb217"));
                Assert.That(EffectiveConfig.From(p0).SimulationKey,Is.EqualTo("166a96e319c7"));
                foreach(var pair in before)Assert.That(File.ReadAllBytes(pair.Key),Is.EqualTo(pair.Value),pair.Key);
                TestContext.WriteLine("S2_DEFAULT_HASH="+Defaults.Hash+" S2_P0_EXTENDED_HASH="+p0.Hash+" KEYS="+Defaults.Ordered.Count);
            }
            finally {if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
    }
}
