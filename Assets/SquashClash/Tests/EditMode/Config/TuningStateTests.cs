using System;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
namespace SquashClash.Core.Tests.Config
{
    public sealed class TuningStateTests
    {
        private static TuningState NewState() { var p=ConfigFixtures.Default();return new TuningState(p,p); }
        [Test] public void NextLaunch_StaysPendingUntilApplyAndCanBeReverted()
        {
            var s=NewState();var initial=s.Applied; s.Set("physics.floor.restitution",.6);
            Assert.That(s.Applied,Is.SameAs(initial));Assert.That(s.Visible.Get("physics.floor.restitution"),Is.EqualTo(.6));
            Assert.That(s.PendingKeys,Does.Contain("physics.floor.restitution"));
            s.Set("physics.floor.restitution",.45);Assert.That(s.PendingKeys,Is.Empty);
            s.Set("physics.floor.restitution",.6);Assert.That(s.ApplyPending().Get("physics.floor.restitution"),Is.EqualTo(.6));
            Assert.That(s.PendingKeys,Is.Empty);Assert.That(s.Visible.Hash,Is.EqualTo(s.Applied.Hash));
        }
        [Test] public void Immediate_AppliesAndNotifiesWithoutApplyingUnrelatedPending()
        {
            var s=NewState();var changed=new List<string>();s.Changed+=changed.Add;
            s.Set("launcher.speed",20);s.Set("camera.fovDeg",70);s.Set("camera.fovDeg",70);
            Assert.That(s.Applied.Get("camera.fovDeg"),Is.EqualTo(70));Assert.That(s.Applied.Get("launcher.speed"),Is.EqualTo(15));
            Assert.That(s.Visible.Get("launcher.speed"),Is.EqualTo(20));Assert.That(changed,Is.EqualTo(new[]{"camera.fovDeg"}));
        }
        [Test] public void Set_ClampsFiniteRangesAndRejectsInvalidValuesWithoutMutation()
        {
            var s=NewState();s.Set("launcher.speed",99);Assert.That(s.Visible.Get("launcher.speed"),Is.EqualTo(40));
            foreach(var pair in new[]{("physics.stepHz",100d),("physics.maxImpactsPerStep",2.5),("placeholders.visible",.5),("camera.fovDeg",double.NaN),("camera.height",double.PositiveInfinity)})
            { var hash=s.Visible.Hash;Assert.Throws<ArgumentOutOfRangeException>(()=>s.Set(pair.Item1,pair.Item2));Assert.That(s.Visible.Hash,Is.EqualTo(hash)); }
            Assert.Throws<ArgumentException>(()=>s.Set("unknown",0));
        }
        [Test] public void LoadPreset_PartialUsesDefaultsAndReplacesOldPending()
        {
            var d=ConfigFixtures.Default();var s=new TuningState(d,d.With("camera.fovDeg",60).With("physics.gravity",12));
            s.Set("launcher.yawDeg",20);var warnings=new List<string>();
            s.LoadPreset(new PresetFile{schemaVersion=1,name="partial",values=new List<PresetEntry>{new PresetEntry{key="launcher.speed",value=20}}},warnings);
            Assert.That(s.Applied.Get("camera.fovDeg"),Is.EqualTo(d.Get("camera.fovDeg")));
            Assert.That(s.Applied.Get("physics.gravity"),Is.EqualTo(12));Assert.That(s.Visible.Get("physics.gravity"),Is.EqualTo(9.81));
            Assert.That(s.Visible.Get("launcher.yawDeg"),Is.Zero);Assert.That(s.Visible.Get("launcher.speed"),Is.EqualTo(20));Assert.That(warnings,Is.Empty);
            Assert.That(s.ApplyPending().Hash,Is.EqualTo(d.With("launcher.speed",20).Hash));
        }
        [Test] public void SavedPreset_ContainsVisiblePendingValues()
        {
            var s=NewState();s.Set("launcher.speed",20);var f=PresetCodec.FromSet(s.Visible,"visible","test");
            Assert.That(PresetCodec.LoadDefault(PresetCodec.ToJson(f)).Get("launcher.speed"),Is.EqualTo(20));
            Assert.That(s.Applied.Get("launcher.speed"),Is.EqualTo(15));
        }
        [Test] public void InvalidLoad_DoesNotChangeStateAndOverlayReportsWarnings()
        {
            var s=NewState();var hash=s.Visible.Hash;
            Assert.Throws<System.IO.InvalidDataException>(()=>s.LoadPreset(new PresetFile{schemaVersion=99},new List<string>()));
            Assert.That(s.Visible.Hash,Is.EqualTo(hash));var warnings=new List<string>();
            s.LoadPreset(new PresetFile{schemaVersion=1,values=new List<PresetEntry>{new PresetEntry{key="unknown",value=1}}},warnings);
            Assert.That(warnings,Has.Count.EqualTo(1));Assert.That(s.Visible.Hash,Is.EqualTo(s.Defaults.Hash));
        }
    }
}
