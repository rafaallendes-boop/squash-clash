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
            TestContext.WriteLine("S4_HASH defaults="+p.Hash+" P0="+effective.Hash+" SL5="+p.With("practice.launch",4).Hash);
        }
    }
}
