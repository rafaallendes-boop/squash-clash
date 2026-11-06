using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class S3FeedbackConfigTests
    {
        [Test] public void HapticsKey_IsBoolDefaultOn_Immediate()
        {
            Assert.That(ParamCatalog.TryGet("feedback.haptics",out var def),Is.True);Assert.That(def.Kind,Is.EqualTo(ParamKind.Bool));
            Assert.That(def.Min,Is.EqualTo(0));Assert.That(def.Max,Is.EqualTo(1));Assert.That(def.Step,Is.EqualTo(1));Assert.That(def.Apply,Is.EqualTo(ApplyClass.Immediate));
            Assert.That(def.Group,Is.EqualTo("First Shot / feedback"));Assert.That(ConfigFixtures.Default().Get("feedback.haptics"),Is.EqualTo(1));
        }
        [Test] public void OnlyDifference_FromPreS3Fixture_IsHapticsKey()
        {
            var json=File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Tests/Fixtures/pre-s3-defaults.json"));
            Assert.That(PresetCodec.TryParse(json,out var old,out var error),Is.True,error);Assert.That(old.values.Count,Is.EqualTo(106));
            var now=ConfigFixtures.Default();Assert.That(now.Ordered.Count,Is.EqualTo(107));
            foreach(var e in old.values)Assert.That(now.Get(e.key),Is.EqualTo(e.value),e.key);
            var added=now.Ordered.Select(e=>e.Key).Except(old.values.Select(e=>e.key)).ToArray();
            Assert.That(added,Is.EqualTo(new[]{"feedback.haptics"}));
            Assert.That(EffectiveConfig.From(now).SimulationKey,Is.EqualTo("166a96e319c7"));Assert.That(now.HashOf(ConfigFixtures.FirstBallPrefixes),Is.EqualTo("0d39e6016bd8"));
        }
        [Test] public void EveryClip_HasProvenanceEntry()
        {
            var dir=Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/Audio");var text=File.ReadAllText(Path.Combine(dir,"PROVENANCE.md"));
            var wavs=Directory.GetFiles(dir,"*.wav");Assert.That(wavs.Length,Is.GreaterThanOrEqualTo(4));
            foreach(var w in wavs)Assert.That(text,Does.Contain(Path.GetFileName(w)),w);
        }
        [TestCase(PresentationCategory.Perfect,true,true,HapticStyle.Firm)][TestCase(PresentationCategory.Valid,true,true,HapticStyle.Discreet)]
        [TestCase(PresentationCategory.Failed,true,false,HapticStyle.Firm)][TestCase(PresentationCategory.Perfect,false,false,HapticStyle.Firm)][TestCase(PresentationCategory.Valid,false,false,HapticStyle.Firm)]
        public void HapticMapping(PresentationCategory category,bool enabled,bool expected,HapticStyle style)
        {Assert.That(FeedbackMap.TryHaptic(category,enabled,out var s),Is.EqualTo(expected));if(expected)Assert.That(s,Is.EqualTo(style));}
    }
}
