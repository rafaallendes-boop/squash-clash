using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
namespace SquashClash.Core.Tests.Rally
{
    // Simple referee (spec 2026-11-26 section 6.2): TinTouched is derived from the closure already recorded and never changes a decision.
    public sealed class PointRulesTinTouchedTests
    {
        private const float R=.02f;
        private static readonly CourtDimensions Court=CourtDimensions.Standard();
        private static PointRules Rally(){var rules=new PointRules(Court,R);rules.Begin(PlayerId.Rival,PlayerId.Human);return rules;}
        private static PointRules Serve(){var rules=new PointRules(Court,R);rules.BeginServe(PlayerId.Rival,ServiceBox.Right);return rules;}
        private static ContactEvent Front(float y)=>PointRulesTests.Hit(SurfaceId.FrontWall,3.2f,y);
        [Test] public void RallyShotIntoTheTin_IsTinTouched()
        {
            var rules=Rally();Assert.That(rules.TinTouched,Is.False,"open point");rules.OnBallContact(Front(.3f));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.Tin));Assert.That(rules.TinTouched,Is.True);
        }
        [Test] public void ServeIntoTheTin_IsAServiceFault_AndTinTouched()
        {
            var rules=Serve();rules.OnBallContact(Front(.3f));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.ServiceFault));Assert.That(rules.TinTouched,Is.True);
        }
        [Test] public void ServeBelowTheServiceLineButAboveTheTin_IsNotTinTouched()
        {
            var rules=Serve();rules.OnBallContact(Front(1.2f));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.ServiceFault));Assert.That(rules.TinTouched,Is.False);
        }
        [Test] public void RallyShotOut_IsNotTinTouched()
        {
            var rules=Rally();rules.OnBallContact(Front(4.7f));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.Out));Assert.That(rules.TinTouched,Is.False);
        }
        [Test] public void DoubleBounce_IsNotTinTouched()
        {
            var rules=Rally();rules.OnBallContact(Front(2));rules.OnBallContact(PointRulesTests.Hit(SurfaceId.Floor,3,6));rules.OnBallContact(PointRulesTests.Hit(SurfaceId.Floor,3,6));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.DoubleBounce));Assert.That(rules.TinTouched,Is.False);
        }
    }
}
