using NUnit.Framework;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class ShotPresentationGateTests
    {
        private ShotPresentationGate gate;
        [SetUp] public void Setup(){gate=new ShotPresentationGate();gate.Reset(5);}
        private static ShotRecord Rec(string result,bool applied,string id="a1",long launch=5,int version:1,string fail="None")
            =>new ShotRecord{LaunchId=launch,AttemptId=id,Version=version,Result=result,Applied=applied,Fail=fail,ContactStep=42,ContactX=1.5f,ContactY=.7f,ContactZ=3.1f,Side="Right",DeltaMs=-12.5};
        private bool Try(ShotRecord r,out ShotPresentation p,long current=5)=>gate.TryDeliver(r,current,out p,out _);
        [Test] public void PerfectApplied_DeliversOneContactPresentation()
        {
            Assert.That(Try(Rec("Perfect",true),out var p),Is.True);
            Assert.That(p.Category,Is.EqualTo(PresentationCategory.Perfect));Assert.That(p.HasContact,Is.True);Assert.That(p.ContactStep,Is.EqualTo(42));
            Assert.That(p.ContactPosition,Is.EqualTo(new Vector3(1.5f,.7f,3.1f)));Assert.That(p.Side,Is.EqualTo("Right"));Assert.That(p.LaunchId,Is.EqualTo(5));Assert.That(p.AttemptId,Is.EqualTo("a1"));
        }
        [Test] public void ValidApplied_DeliversContactWithDelta()
        {
            Assert.That(Try(Rec("Valid",true),out var p),Is.True);Assert.That(p.Category,Is.EqualTo(PresentationCategory.Valid));Assert.That(p.HasContact,Is.True);Assert.That(p.DeltaMs,Is.EqualTo(-12.5));
        }
        [TestCase("Air")][TestCase("Late")]
        public void Failed_DeliversWithoutContactData(string fail)
        {
            Assert.That(Try(Rec("Failed",false,fail:fail),out var p),Is.True);Assert.That(p.Category,Is.EqualTo(PresentationCategory.Failed));Assert.That(p.HasContact,Is.False);Assert.That(p.Fail,Is.EqualTo(fail));
            Assert.That(p.ContactPosition,Is.EqualTo(Vector3.zero));Assert.That(p.ContactStep,Is.EqualTo(0));
        }
        [TestCase("NoOrder",false)][TestCase("Unreachable",false)][TestCase("SystemFault",false)][TestCase("NotEvaluable",false)][TestCase("Discarded",false)][TestCase("Closure",false)][TestCase("Perfect",false)][TestCase("Valid",false)]
        public void IneligibleRecords_NeverDeliver(string result,bool applied)
        {
            Assert.That(Try(Rec(result,applied),out _),Is.False);Assert.That(gate.DiscrepancyCount,Is.Zero);
            // The identity was not reserved: a later applied result for it still delivers.
            Assert.That(Try(Rec("Perfect",true),out _),Is.True);
        }
        [Test] public void SameIdentityNewVersion_NoSecondDelivery_AndNoDiscrepancy()
        {
            Assert.That(Try(Rec("Perfect",true,version:1),out _),Is.True);
            Assert.That(Try(Rec("Perfect",true,version:2),out _),Is.False);Assert.That(gate.DiscrepancyCount,Is.Zero);
        }
        [Test] public void SameIdentityDifferentResult_NoDeliveryAndOneDiscrepancy()
        {
            Assert.That(Try(Rec("Perfect",true),out _),Is.True);
            Assert.That(Try(Rec("Failed",false,version:2,fail:"Air"),out _),Is.False);
            Assert.That(gate.DiscrepancyCount,Is.EqualTo(1));Assert.That(gate.LastDiscrepancy,Does.Contain("Perfect").And.Contain("Failed"));
        }
        [Test] public void OtherLaunch_NeverDelivers_AndResetOpensNewLaunch()
        {
            Assert.That(Try(Rec("Perfect",true,launch:4),out _),Is.False);
            Assert.That(Try(Rec("Perfect",true),out _),Is.True);
            gate.Reset(6);Assert.That(Try(Rec("Perfect",true),out _,current:6),Is.False,"old launch record against the new launch");
            Assert.That(Try(Rec("Perfect",true,launch:6),out _,current:6),Is.True);
        }
        [Test] public void DifferentAttemptsInSameLaunch_AreIndependent()
        {
            Assert.That(Try(Rec("Perfect",true,"a1"),out _),Is.True);Assert.That(Try(Rec("Valid",true,"a2"),out var p),Is.True);Assert.That(p.AttemptId,Is.EqualTo("a2"));
        }
    }
}
