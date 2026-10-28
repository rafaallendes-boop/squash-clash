using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class SlingConfigTests
    {
        [Test] public void SelectorDefaultsToReference_AndAllExisting62ValuesRemainIdentical()
        {
            Assert.That(ParamCatalog.TryGet("gesture.control",out _),Is.True);
            var p=ConfigFixtures.Default();Assert.That(p.Get("gesture.control"),Is.Zero);
            var old=JsonUtility.FromJson<PresetFile>(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Tests/Fixtures/pre-sling-defaults.json")));
            Assert.That(old.values.Count,Is.EqualTo(62));foreach(var e in old.values)Assert.That(p.Get(e.key),Is.EqualTo(e.value),e.key);
            Assert.That(p.HashOf(ConfigFixtures.FirstBallPrefixes),Is.EqualTo("0d39e6016bd8"));
            Assert.That(EffectiveConfig.From(p).SimulationKey,Is.EqualTo("166a96e319c7"));
        }
        [Test] public void QuietPullAndCancelParametersHaveTheApprovedInitialValues()
        {
            var p=ConfigFixtures.Default();
            foreach(var item in new[]{("aim.holdMs",120d),("aim.holdMaxSpeedMmPerS",15d),("aim.holdRadiusMm",1d),("aim.holdMaxGapMs",51d),("aim.holdMinSamples",3d),("power.pullDeadZoneMm",2d),("power.pullFullMm",35d),("gesture.slingCancelAboveMm",12d),("gesture.slingCancelLateralMm",20d),("gesture.slingCancelMinDepthMm",8d),("aim.lockFlashMs",120d)})
            {Assert.That(ParamCatalog.TryGet(item.Item1,out _),Is.True,item.Item1);Assert.That(p.Get(item.Item1),Is.EqualTo(item.Item2));}
        }
        [Test] public void IncoherentPullPairIsRejectedBeforeStateChanges()
        {
            var values=ConfigFixtures.Default().Ordered.ToDictionary(e=>e.Key,e=>e.Value);values["power.pullDeadZoneMm"]=35;values["power.pullFullMm"]=35;
            Assert.That(ShotConfigValidator.TryValidate(values,out var error),Is.False);Assert.That(error,Does.Contain("power.pullDeadZoneMm"));
        }
    }
}
