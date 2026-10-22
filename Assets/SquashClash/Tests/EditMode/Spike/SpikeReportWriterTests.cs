using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Spike;
namespace SquashClash.Core.Tests.Spike {
 public sealed class SpikeReportWriterTests {
  private static SpikeResult Passing(bool iphone=false) {
   var r=new SpikeResult{Backend="P1",Variant="baseline",Device=iphone?"iPhone 16 Pro":"editor",Build="test",ConfigHash="abc",ProjectPhysics="gravity=9.81",Complete=true,Criteria=SpikeCriteria.CreateResults(iphone)};
   foreach(var c in r.Criteria){c.Verdict=c.Gating?Verdict.Pass:Verdict.Info;if(c.Expected==0)c.Expected=c.Id=="C2"?10053:c.Id=="C3"?39:c.Id=="C7"?20032:1;c.Evaluated=c.Expected;c.CoverageComplete=true;c.Measured="observado";}
   return r;
  }
  [TestCase(Verdict.NotEvaluated)][TestCase(Verdict.Fail)][TestCase(Verdict.Info)]
  public void Report_NotEvaluatedIsNeverPass_AndListsTraceability(Verdict verdict) {
   var r=Passing();r.Criteria.Single(c=>c.Id=="C7").Verdict=verdict;
   Assert.That(r.AllGatingPass,Is.False);
   var md=SpikeReportWriter.ToMarkdown(r);
   foreach(var field in new[]{"Requisito","Escenarios","Medición","Umbral","Veredicto","evaluados/no evaluables","C7",verdict.ToString(),"abc","gravity=9.81"})Assert.That(md,Does.Contain(field));
  }
  [Test] public void Report_IncompleteRunIsNeverPass() {
   var r=Passing();r.Complete=false;Assert.That(r.AllGatingPass,Is.False);Assert.That(SpikeReportWriter.ToMarkdown(r),Does.Contain("INCOMPLETO"));
  }
  [Test] public void Criteria_CoverageAndGateFlagsMatchApprovedDecisions() {
   var rows=SpikeCriteria.CreateResults(true);Assert.That(rows.Count,Is.EqualTo(18));Assert.That(rows.Select(c=>c.Id).Distinct().Count(),Is.EqualTo(18));
   Assert.That(rows.Single(c=>c.Id=="C7").Gating,Is.True);Assert.That(rows.Single(c=>c.Id=="C9").Gating,Is.False);
   foreach(var c in rows)Assert.That(c.Verdict,Is.EqualTo(Verdict.NotEvaluated));
   Assert.That(rows.Single(c=>c.Id=="C10").Threshold,Does.Contain("1 mm"));
   Assert.That(rows.Single(c=>c.Id=="C17").Threshold,Does.Contain("p95"));
  }
  [Test] public void Report_PerformanceIsGatingOnlyOnIphone() {
   var r=Passing();Assert.That(r.AllGatingPass,Is.True);Assert.That(r.Criteria.Single(c=>c.Id=="C17").Gating,Is.False);
   r=Passing(true);Assert.That(r.AllGatingPass,Is.True);r.Criteria.Single(c=>c.Id=="C17").Verdict=Verdict.NotEvaluated;Assert.That(r.AllGatingPass,Is.False);
  }
  [Test] public void Report_RejectsMissingDuplicateOrDisabledGateCriterion() {
   var r=Passing();r.Criteria.RemoveAt(0);Assert.That(r.AllGatingPass,Is.False);
   r=Passing();r.Criteria[1]=r.Criteria[0];Assert.That(r.AllGatingPass,Is.False);
   r=Passing();r.Criteria.Single(c=>c.Id=="C1").Gating=false;Assert.That(r.AllGatingPass,Is.False);
   r=new SpikeResult{Complete=true};Assert.That(r.AllGatingPass,Is.False);
  }
  [Test] public void Report_ZeroOrPartialCoverageIsNotPass() {
   var r=Passing();r.Criteria[0].Evaluated=0;Assert.That(r.AllGatingPass,Is.False);
   r=Passing();r.Criteria[0].NotEvaluable=1;Assert.That(r.AllGatingPass,Is.False);
  }
  [Test] public void Report_CannotApproveOneOfNineS9PrecisionCases() {
   var r=Passing();var c=r.Criteria.Single(v=>v.Id=="C10");c.Evaluated=1;
   Assert.That(r.AllGatingPass,Is.False);Assert.That(SpikeReportWriter.ToMarkdown(r),Does.Contain("1/0/9"));
   c.Expected=1;Assert.That(r.AllGatingPass,Is.False,"fixed catalog coverage cannot be lowered");
  }
  [Test] public void Report_ReducedS8BatchCannotApproveRobustness() {
   var r=Passing();var c=r.Criteria.Single(v=>v.Id=="C1");c.Evaluated=153;c.Expected=153;
   Assert.That(r.AllGatingPass,Is.False,"100 random +17 fixed+36 directed is not the approved 10000 random campaign");
  }
  [Test] public void Report_EligibleCoverageMustBeDeclaredAndClosed() {
   var r=Passing();var c=r.Criteria.Single(v=>v.Id=="C3");c.CoverageComplete=false;
   Assert.That(r.AllGatingPass,Is.False);c.CoverageComplete=true;c.Expected=0;Assert.That(r.AllGatingPass,Is.False);
  }
  [Test] public void Report_MissingRowsAreShownAsNotEvaluatedAndMarkdownIsEscaped() {
   var r=Passing();r.Criteria.RemoveAt(0);r.Backend="P1|test";r.Criteria[0].Measured="uno|dos\ntres";
   var md=SpikeReportWriter.ToMarkdown(r);
   Assert.That(md,Does.Contain("C1"));Assert.That(md,Does.Contain("NotEvaluated"));Assert.That(md,Does.Contain("P1\\|test"));Assert.That(md,Does.Contain("uno\\|dos<br>tres"));
  }
 }
}
