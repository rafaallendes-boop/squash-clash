using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class GestureCalibrationConfigTests
    {
        private static readonly string[] Added={"power.method","power.peakSmoothingMs","power.peakDecayMs","aim.freezeOnRelease"};
        [Test] public void NewKeys_DefaultToLegacyMethodAndReleaseDisabled_KeepAll58ExistingValues()
        {
            foreach(var key in Added)Assert.That(ParamCatalog.TryGet(key,out _),Is.True,key);
            var p=ConfigFixtures.Default();Assert.That(p.Get("power.method"),Is.Zero);Assert.That(p.Get("aim.freezeOnRelease"),Is.Zero);
            Assert.That(p.Get("power.peakSmoothingMs"),Is.EqualTo(20));Assert.That(p.Get("power.peakDecayMs"),Is.EqualTo(120));
            var entries=ConfigFixtures.PreSlingEntries(p).Where(x=>!Added.Contains(x.Key)).ToArray();Assert.That(entries.Length,Is.EqualTo(58));
            var canonical=string.Concat(entries.Select(x=>x.Key+"="+x.Value.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"\n"));
            using(var sha=SHA256.Create())Assert.That(BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical)),0,6).Replace("-","").ToLowerInvariant(),Is.EqualTo("00a501ad7066"));
            Assert.That(p.HashOf(ConfigFixtures.FirstBallPrefixes),Is.EqualTo("0d39e6016bd8"));Assert.That(EffectiveConfig.From(p).SimulationKey,Is.EqualTo("166a96e319c7"));
        }
        [Test] public void Defaults_ReproduceLegacyPowerAndAimForTheSameSamples()
        {
            Assert.That(ParamCatalog.TryGet("power.method",out _),Is.True);
            var p=ConfigFixtures.Default();var oldSettings=new PowerSettings(80,30,4,34,100,1500,1);
            var samples=Enumerable.Range(0,9).Select(i=>new TouchSample(1,i==0?TouchPhaseKind.Began:i==8?TouchPhaseKind.Ended:TouchPhaseKind.Moved,new Vector2(i*8,0),i*.01,i)).ToArray();
            var a=PowerEstimator.Estimate(samples,.08,1,oldSettings);var b=PowerEstimator.Estimate(samples,.08,1,PowerSettings.From(p));
            Assert.That(b.P,Is.EqualTo(a.P));Assert.That(b.NetSpeedMmPerS,Is.EqualTo(a.NetSpeedMmPerS));Assert.That(b.Conservative,Is.EqualTo(a.Conservative));Assert.That(b.CoveredMs,Is.EqualTo(a.CoveredMs));Assert.That(b.Reason,Is.EqualTo(a.Reason));
            var before=new AimModel(new AimSettings(.12f,2.2f,250,40,50,2),CourtDimensions.Standard(),3.2f);var after=new AimModel(AimSettings.From(p),CourtDimensions.Standard(),3.2f);
            foreach(var sample in samples){before.Update(sample,1);after.Update(sample,1);Assert.That(after.State,Is.EqualTo(before.State));Assert.That(after.Aim,Is.EqualTo(before.Aim));Assert.That(after.FreezeTimeReal,Is.EqualTo(before.FreezeTimeReal));}
            var slow=new[]{new TouchSample(1,TouchPhaseKind.Began,Vector2.zero,0,0),new TouchSample(1,TouchPhaseKind.Ended,Vector2.right,.2,1)};
            foreach(var sample in slow)after.Update(sample,1);Assert.That(after.State,Is.EqualTo(AimState.Aiming));
        }
    }
}
