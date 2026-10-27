using System;
using NUnit.Framework;
using SquashClash.Core.Shot;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class SlingProtocolTests
    {
        [Test] public void B1_HoldsEachAttemptForAnExplicitAnswer_WithoutCreatingAnotherAttempt()
        {
            var p=new SlingProtocol();Assert.That(p.Arm("B1"),Is.True);
            Assert.That(p.TargetId,Is.EqualTo(1));p.CompleteAttempt();
            Assert.That(p.Completed,Is.EqualTo(1));Assert.That(p.AwaitingFeedback,Is.True);
            Assert.That(p.PendingTrial,Is.EqualTo(1));Assert.That(p.Arm("B3"),Is.False);
            p.CompleteAttempt();Assert.That(p.Completed,Is.EqualTo(1));
            Assert.That(p.Respond(true),Is.True);Assert.That(p.Respond(false),Is.False);
            Assert.That(p.TargetId,Is.EqualTo(2));Assert.That(p.Completed,Is.EqualTo(1));
        }
        [Test] public void B1_FinalAnswerRemainsPending_AndAllTenTargetsKeepTheirOrder()
        {
            var p=new SlingProtocol();p.Arm("B1");
            for(int i=0;i<10;i++){Assert.That(p.TargetId,Is.EqualTo(i%5+1));p.CompleteAttempt();Assert.That(p.AwaitingFeedback,Is.True);p.Respond(false);}
            Assert.That(p.Running,Is.False);Assert.That(p.Completed,Is.EqualTo(10));
        }
        [Test] public void B3_AlternatesLowTargets_AndNeverInfersHumanIntent()
        {
            var p=new SlingProtocol();p.Arm("B3");
            for(int i=0;i<10;i++){Assert.That(p.TargetId,Is.EqualTo(i%2+1));Assert.That(p.RequestedPower,Is.EqualTo("medium"));p.CompleteAttempt();p.Respond(false);}
        }
        [Test] public void B5_UsesPredeclaredTargetAndPowerSequences()
        {
            var p=new SlingProtocol();p.Arm("B5");var power=new[]{"soft","medium","strong"};
            for(int i=0;i<10;i++){Assert.That(p.TargetId,Is.EqualTo(i%5+1));Assert.That(p.RequestedPower,Is.EqualTo(power[i%3]));p.CompleteAttempt();}
            Assert.That(p.Running,Is.False);
        }
        [TestCase("B2-soft",10,"soft")] [TestCase("B2-medium",10,"medium")] [TestCase("B2-strong",10,"strong")] [TestCase("B4",5,"cancel")]
        public void FormalBlocksKeepEveryAttempt_AndDoNotReplaceCancellation(string block,int count,string power)
        {
            var p=new SlingProtocol();p.Arm(block);Assert.That(p.RequestedPower,Is.EqualTo(power));
            for(int i=0;i<count;i++)p.CompleteAttempt();Assert.That(p.Completed,Is.EqualTo(count));Assert.That(p.Running,Is.False);
        }
        [Test] public void PracticeIsSeparate_AndRearmingGetsANewSessionId()
        {
            var p=new SlingProtocol();p.Arm("B0");int first=p.Session;for(int i=0;i<80;i++)p.CompleteAttempt();
            Assert.That(p.Running,Is.True);Assert.That(p.Completed,Is.EqualTo(80));p.Arm("B1");Assert.That(p.Session,Is.GreaterThan(first));Assert.That(p.Completed,Is.Zero);
        }
    }
}
