using NUnit.Framework;
using SquashClash.Core.Shot;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class ImpactCueTests
    {
        private const double Target=2;
        private static ImpactCueState At(double deltaMs, bool validLimitOn=true, bool mapped=true, bool consumed=false)
            =>ImpactCue.Evaluate(Target+deltaMs/1000,1,Target,new TimingSettings(40,validLimitOn,100),mapped,consumed);
        private static void AssertFinite(double value)
        { Assert.That(double.IsNaN(value),Is.False); Assert.That(double.IsInfinity(value),Is.False); }

        [Test] public void AtTarget_ApproachIsZero_AndAtOrAfterIdeal()
        {
            var at=At(0);
            Assert.That(at.Approach,Is.Zero);
            Assert.That(at.AtOrAfterIdeal,Is.True);
            Assert.That(at.SecondsToTarget,Is.Zero);
            Assert.That(at.DeltaMs,Is.Zero);
            Assert.That(at.Phase,Is.EqualTo(ImpactCuePhase.AtIdeal));
            Assert.That(at.InAssistWindow,Is.True);
            Assert.That(at.InValidWindow,Is.True);
            var after=At(10);
            Assert.That(after.Approach,Is.Zero);
            Assert.That(after.AtOrAfterIdeal,Is.True);
        }

        [Test] public void BeforeApproachWindow_ApproachStaysAtOne()
        {
            var before=At(-600);
            Assert.That(before.Approach,Is.EqualTo(1f));
            Assert.That(before.AtOrAfterIdeal,Is.False);
            Assert.That(before.Phase,Is.EqualTo(ImpactCuePhase.Approaching));
            Assert.That(At(-500).Approach,Is.EqualTo(1f));
        }

        [Test] public void ValidWindow_InclusiveBoundary()
        {
            Assert.That(At(-100).InValidWindow,Is.True);
            Assert.That(At(100).InValidWindow,Is.True);
            Assert.That(At(-101).InValidWindow,Is.False);
            Assert.That(At(101).InValidWindow,Is.False);
            Assert.That(At(-40).InAssistWindow,Is.True);
            Assert.That(At(40).InAssistWindow,Is.True);
            Assert.That(At(-41).InAssistWindow,Is.False);
            Assert.That(At(41).InAssistWindow,Is.False);
            foreach(double delta in new double[] {-100,-40,0,40,100})
                Assert.That(At(delta,validLimitOn:false).InValidWindow,Is.False);
        }

        [Test] public void ColorOrPhase_ChangesAtTarget_NotAfter()
        {
            var at=At(0);
            Assert.That(at.Phase,Is.EqualTo(ImpactCuePhase.AtIdeal));
            Assert.That(at.AtOrAfterIdeal,Is.True);
            var before=At(-1);
            Assert.That(before.Phase,Is.Not.EqualTo(ImpactCuePhase.AtIdeal));
            Assert.That(before.AtOrAfterIdeal,Is.False);
            var after=At(1);
            Assert.That(after.AtOrAfterIdeal,Is.True);
            Assert.That(after.Phase,Is.EqualTo(ImpactCuePhase.AfterIdeal));
        }

        [Test] public void ConsumedOrUnmapped_IsHidden()
        {
            Assert.That(At(-200,consumed:true).Phase,Is.EqualTo(ImpactCuePhase.Hidden));
            Assert.That(At(-200,mapped:false).Phase,Is.EqualTo(ImpactCuePhase.Hidden));
            Assert.That(At(-200).Phase,Is.Not.EqualTo(ImpactCuePhase.Hidden));
        }

        [Test] public void PresentedEqualsTarget_NoNaN()
        {
            foreach(double now in new double[] {Target-.2,Target,Target+.01})
            {
                var state=ImpactCue.Evaluate(now,Target,Target,new TimingSettings(40,true,100),true,false);
                AssertFinite(state.Approach);
                AssertFinite(state.SecondsToTarget);
                AssertFinite(state.DeltaMs);
            }
            var at=ImpactCue.Evaluate(Target,Target,Target,new TimingSettings(40,true,100),true,false);
            Assert.That(at.AtOrAfterIdeal,Is.True);
            Assert.That(at.Approach,Is.Zero);
            Assert.That(at.SecondsToTarget,Is.Zero);
            Assert.That(at.DeltaMs,Is.Zero);
            Assert.That(at.Phase,Is.EqualTo(ImpactCuePhase.AtIdeal));
        }
    }
}
