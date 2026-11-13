using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class TimingAidConfigTests
    {
        [Test] public void AidLevel_DefaultFull_LegacyOverlayAndInheritedValuesRemainIntact()
        {
            var p=ConfigFixtures.Default();
            Assert.That(ParamCatalog.TryGet("timing.aidLevel",out var def),Is.True);
            Assert.That(p.GetInt("timing.aidLevel"),Is.Zero);Assert.That(p.Ordered.Count,Is.EqualTo(108));
            Assert.That(def.Kind,Is.EqualTo(ParamKind.Choice));Assert.That(def.Choices.Select(c=>c.Value),Is.EqualTo(new double[]{0,1,2}));
            foreach(var level in new[]{0,1,2})Assert.That(p.With("timing.aidLevel",level).GetInt("timing.aidLevel"),Is.EqualTo(level));
            var canonical=new StringBuilder();var legacy=p.Ordered.Where(e=>ConfigFixtures.IsFirstBallKey(e.Key)).ToArray();
            Assert.That(legacy.Length,Is.EqualTo(41));
            foreach(var e in legacy)canonical.Append(e.Key).Append('=').Append(e.Value.ToString("R",System.Globalization.CultureInfo.InvariantCulture)).Append('\n');
            using(var sha=SHA256.Create())Assert.That(System.BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())),0,6).Replace("-","").ToLowerInvariant(),Is.EqualTo("0d39e6016bd8"));
            Assert.That(EffectiveConfig.From(p).SimulationKey,Is.EqualTo("166a96e319c7"));
            var p0File=PresetCodec.FromSet(p,"P0-calib-1","fixture");p0File.values=p0File.values.Where(e=>ConfigFixtures.IsFirstBallKey(e.key)).ToList();
            p0File.values.Single(e=>e.key=="launcher.repeatSeconds").value=3;
            var effective=PresetCodec.Overlay(p,p0File,new List<string>());
            Assert.That(effective.GetInt("timing.aidLevel"),Is.Zero);
            var path=System.Environment.GetEnvironmentVariable("S4_P0_PATH");
            if(!string.IsNullOrEmpty(path))
            {
                byte[] bytes=File.ReadAllBytes(path);
                using(var sha=SHA256.Create())Assert.That(System.BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant(),Is.EqualTo("2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd"));
                Assert.That(PresetCodec.TryParse(Encoding.UTF8.GetString(bytes),out var realP0,out var error),Is.True,error);
                var warnings=new List<string>();var realEffective=PresetCodec.Overlay(p,realP0,warnings);
                Assert.That(warnings,Is.Empty);Assert.That(realEffective.Hash,Is.EqualTo(effective.Hash));
                Assert.That(realEffective.HashOf(ConfigFixtures.FirstBallPrefixes),Is.EqualTo("8de3f2ecb217"));
                Assert.That(File.ReadAllBytes(path),Is.EqualTo(bytes));
                TestContext.WriteLine("S4_REAL_P0 effective="+realEffective.Hash+" inherited="+realEffective.HashOf(ConfigFixtures.FirstBallPrefixes)+" bytesUnchanged=true");
            }
            var sl5=p.With("practice.launch",4).With("human.z",(double)1.6f);
            TestContext.WriteLine("S4_HASH defaults="+p.Hash+" P0="+effective.Hash+" SL5="+sl5.Hash);
            Assert.That(sl5.Hash,Is.EqualTo("50efa6d33c0c"),"SL5 with complete existing scenario preparation");
        }
    }
}
