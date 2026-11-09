using NUnit.Framework;
namespace SquashClash.AutoQA
{
    public sealed class AutoQAReportTests
    {
        [TestCase("PASS","FAIL",true,true,null,"PASS",0,1)]
        [TestCase("FAIL","FAIL",true,true,null,"FAIL",1,2)]
        [TestCase("INCONCLUSIVE","FAIL",true,true,null,"INCONCLUSIVE",2,2)]
        [TestCase("NOT_EVALUATED","FAIL",true,true,null,"NOT_EVALUATED",2,2)]
        [TestCase("PASS","PASS",true,true,null,"FAIL",3,3)]
        [TestCase("PASS","INCONCLUSIVE",true,true,null,"FAIL",3,3)]
        [TestCase("PASS",null,true,true,null,"FAIL",3,3)]
        [TestCase("INCONCLUSIVE","FAIL",false,true,null,"FAIL",3,3)]
        [TestCase("PASS","FAIL",true,false,null,"FAIL",1,2)]
        [TestCase("PASS",null,true,true,"environment","ERROR",4,2)]
        [TestCase("FAIL",null,true,true,"environment","ERROR",4,2)]
        [TestCase("PASS","PASS",true,true,"environment","ERROR",3,3)]
        [TestCase("PASS",null,false,true,"environment","ERROR",3,3)]
        public void DeterministicTriageAndExitPrecedence(string verdict,string canary,bool integrity,bool determinism,string error,string gate,int exitCode,int level)
        {var d=Report.Triage(new[]{verdict},canary,integrity,determinism,error);Assert.That(d.gate,Is.EqualTo(gate));Assert.That(d.exitCode,Is.EqualTo(exitCode));Assert.That(d.escalationLevel,Is.EqualTo(level));Assert.That(d.escalationReasons,Is.Not.Null);}
        [Test] public void NoAttemptsCannotPass()=>Assert.That(Report.Triage(new string[0],"FAIL",true,true).gate,Is.EqualTo("NOT_EVALUATED"));
        [Test] public void SummarySchemaIsVersioned()=>Assert.That(Report.SchemaHeader(),Is.EqualTo("autoqa-summary/1"));
    }
}
