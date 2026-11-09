using NUnit.Framework;
namespace SquashClash.AutoQA
{
    public sealed class AutoQAFlowTests
    {
        [TestCase(false,false,true,null,true)]
        [TestCase(false,true,true,null,true)]
        [TestCase(true,false,true,null,false)]
        [TestCase(true,true,true,null,true)]
        [TestCase(true,true,false,null,false)]
        [TestCase(true,true,true,"execution",false)]
        [TestCase(true,false,true,"execution",false)]
        public void StopPolicy_RetainsCanaryOnlyWhenStateIsReliable(bool blocked,bool canary,bool guardPassed,string executionError,bool expected)
        =>Assert.That(AutoQAScenarioRuns.CanExecute(blocked,canary,guardPassed,executionError),Is.EqualTo(expected));
        [TestCase("FAIL",true)][TestCase("PASS",false)]
        public void RegressionOrDeterminismStop_StillChecksCanaryAndUsesLevelTwo(string verdict,bool deterministic)
        {
            bool ran=AutoQAScenarioRuns.CanExecute(true,true,true,null);
            var d=Report.Triage(new[]{verdict},ran?"FAIL":"NOT_EVALUATED",true,deterministic);
            Assert.That(ran,Is.True,"Canary must run after a normal regression stop");
            Assert.That(d.gate,Is.EqualTo("FAIL"));Assert.That(d.exitCode,Is.EqualTo(1));Assert.That(d.escalationLevel,Is.EqualTo(2));
        }
    }
}
