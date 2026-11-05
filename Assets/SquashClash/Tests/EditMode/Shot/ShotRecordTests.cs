using NUnit.Framework;
using SquashClash.Core.Shot;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class ShotRecordTests
    {
        [Test] public void ShotRecord_ContainsFieldsOfSection11()
        {
            var r=new ShotRecord{LaunchId=7,AttemptId="touch-a",Version=2,PlanVersion=3,HasRelease=true,ReleaseEventAtReal=1.5,ReleaseCorrectedAtReal=1.48,
                PlanPresentedAtReal=1,TargetContactAtReal=1.48,Power=.6f,Result="Perfect",LaunchHash="captured",GestureHash="began",Build="editor",Seed=5};
            var json=r.ToJson();foreach(var field in new[]{"LaunchId","PlanVersion","PlanPresentedAtReal","TargetContactAtReal","ReleaseEventAtReal","ReleaseCorrectedAtReal","DeltaMs","Power","ContactStep","TargetUsedX","JumpMeters","JumpPixels","JumpBallDiameters","ObsoleteEffects","Preset","LaunchHash","GestureHash","Build","Seed","O2Count","ProjectionObservedAt","LockObservedAt","PredecessorId","QuantizationErrorMsSigned","ReconstructionCostMs","PredictionCostMs","ReconciliationCostMs"})Assert.That(json,Does.Contain("\""+field+"\""));
            var restored=JsonUtility.FromJson<ShotRecord>(json);Assert.That(restored.LaunchId,Is.EqualTo(7));Assert.That(restored.ReleaseEventAtReal,Is.EqualTo(1.5));Assert.That(restored.Version,Is.EqualTo(2));
        }
        [Test] public void ReceptionAndGestureSnapshots_AreImmutableAndSeparate()
        {
            var p=ConfigFixtures.Default();var cfg=new ShotConfig(p);var changed=p.With("timing.validLimit",0).With("human.x",4).With("power.pullFullMm",40);
            var gesture=cfg.WithGesture(changed);Assert.That(gesture.Reception,Is.SameAs(p));Assert.That(gesture.Gesture,Is.SameAs(changed));
            Assert.That(gesture.Timing.ValidLimitOn,Is.True);Assert.That(gesture.Body.PositionXZ,Is.EqualTo(new Vector2(3.2f,5.5f)));Assert.That(gesture.VMin,Is.EqualTo(6));
            Assert.That(gesture.Reception.Get("power.pullFullMm"),Is.EqualTo(35));Assert.That(gesture.Gesture.Get("power.pullFullMm"),Is.EqualTo(40));
        }
    }
}
