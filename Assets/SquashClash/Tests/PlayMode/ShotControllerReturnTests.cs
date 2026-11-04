using System;
using NUnit.Framework;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Shot;
using UnityEngine;
using Phase=UnityEngine.InputSystem.TouchPhase;
namespace SquashClash.Runtime.Tests
{
    public sealed partial class ShotControllerTests
    {
        private void EnableReturn()
        {app.Tuning.Set("practice.launch",4);app.Tuning.Set("shot.returnEnabled",1);app.LaunchCurrent();}
        [Test] public void ReturnPreviewAndLaunch_SameInputSameResolution_AndPreviewIsIsolated()
        {
            EnableReturn();double end=Star();At(end);var state=app.Host.Current;var prediction=app.Host.Prediction;
            Assert.That(controller.TryPreviewReturn(new Vector2(3.2f,2.2f),1,end,out var before),Is.True);
            Assert.That(before.State,Is.EqualTo(ReturnRuleState.Changed));Assert.That(app.Host.Current.Position,Is.EqualTo(state.Position));Assert.That(app.Host.Current.Velocity,Is.EqualTo(state.Velocity));Assert.That(app.Host.Prediction,Is.SameAs(prediction));
            controller.Enqueue(Order(end,p:1));controller.ProcessPending();var record=controller.LastRecord;
            Assert.That(record.Applied,Is.True,record.Reason);Assert.That(record.ReturnRuleState,Is.EqualTo("Changed"));Assert.That(record.ReturnRequestedY,Is.EqualTo(before.AdjustedRequest.AimOnFrontWall.y));Assert.That(record.TargetUsedY,Is.EqualTo(before.Solution.TargetUsed.y));
            foreach(var pair in new[]{Tuple.Create(record.VelocityX,before.Solution.Velocity.x),Tuple.Create(record.VelocityY,before.Solution.Velocity.y),Tuple.Create(record.VelocityZ,before.Solution.Velocity.z)})Assert.That(BitConverter.SingleToInt32Bits(pair.Item1),Is.EqualTo(BitConverter.SingleToInt32Bits(pair.Item2)));
            Assert.That(record.ReturnSolverCalls,Is.Zero,"same reception resolver should reuse the exact preview request");
        }
        [Test] public void ReturnAdjustedPoint_IsVisibleWhileAiming_BeforeEnded()
        {
            EnableReturn();double end=Star();At(end);Send(Phase.Began,end-.02);Send(Phase.Moved,end,35);preview.Tick(now);
            Assert.That(preview.Active,Is.True);Assert.That(controller.LastRecord,Is.Null);
            Assert.That(controller.LastPreviewReturn.HasValue,Is.True);Assert.That(controller.LastPreviewReturn.Value.State,Is.EqualTo(ReturnRuleState.Changed));
            var reticle=app.GetComponentInChildren<AimReticleView>(true);Assert.That(reticle.FeasibleVisible,Is.True);Assert.That(reticle.FeasibleAim.y,Is.LessThan(reticle.Aim.y));
        }
        [Test] public void ReturnNoSolution_RecordsFallbackAndAppliesOriginalSolverResult()
        {
            EnableReturn();app.Tuning.Set("shot.returnTinMargin",6);app.LaunchCurrent();var record=Submit(Star(),p:1);
            Assert.That(record.ReturnRuleEnabled,Is.True);Assert.That(record.ReturnRuleState,Is.EqualTo("NoSolution"));Assert.That(record.Applied,Is.True,record.Reason);Assert.That(record.ReturnRequestedY,Is.EqualTo(record.AimY));Assert.That(record.TargetUsedY,Is.EqualTo(record.AimY));
        }
    }
}
