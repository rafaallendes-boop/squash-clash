using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Core.Tests.Config;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class ShotConfigValidatorTests
    {
        [Test] public void DeadZoneNotBelowFull_IsRejectedAndPreviousConfigKept()
        {
            var d=ConfigFixtures.Default();var t=new TuningState(d,d);t.Set("power.fullMmPerS",300);var before=t.Applied;
            Assert.Throws<System.ArgumentException>(()=>t.Set("power.deadZoneMmPerS",400));
            Assert.That(t.Applied,Is.SameAs(before));Assert.That(t.Visible,Is.SameAs(before));
        }
        [TestCase(15,10,32,false)][TestCase(6,28,10,false)][TestCase(10,10,10,true)]
        public void SpeedOrderViolated_IsRejected(double min,double max,double cap,bool valid)
        {Assert.That(ShotConfigValidator.TryValidate(new Dictionary<string,double>{{"shot.vMin",min},{"shot.vMax",max},{"shot.vCap",cap}},out _),Is.EqualTo(valid));}
        [Test] public void InvalidImport_IsRejectedAtomically_NoPartialApply()
        {
            var d=ConfigFixtures.Default();var t=new TuningState(d,d);var before=t.Applied;
            var f=new PresetFile{schemaVersion=1,values=new List<PresetEntry>{new PresetEntry{key="aim.gainMPerMm",value=.2},new PresetEntry{key="power.fullMmPerS",value=300},new PresetEntry{key="power.deadZoneMmPerS",value=400}}};
            Assert.Throws<InvalidDataException>(()=>t.LoadPreset(f,new List<string>()));
            Assert.That(t.Applied,Is.SameAs(before));Assert.That(t.Visible,Is.SameAs(before));Assert.That(t.PendingKeys,Is.Empty);
        }
    }
}
