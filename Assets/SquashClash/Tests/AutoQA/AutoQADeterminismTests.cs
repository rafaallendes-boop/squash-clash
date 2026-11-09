using NUnit.Framework;
namespace SquashClash.AutoQA
{
    public sealed class AutoQADeterminismTests
    {
        private PocObservation Snapshot(string exact="Scheduled=true",string extra="{}",double time=10)=>new PocObservation{exactShot=exact,additional=extra,relativeMs=new[]{time}};
        [Test] public void SemanticScheduledDifference_IsNeverExcluded()=>Assert.That(Determinism.Equivalent(Snapshot(),Snapshot("Scheduled=false")),Is.False);
        [Test] public void GeometryOrFeedbackDifference_IsExact()=>Assert.That(Determinism.Equivalent(Snapshot(),Snapshot(extra:"{\"point\":1}")),Is.False);
        [TestCase(0.0000001,true)][TestCase(0.000002,false)]
        public void RelativeTimes_UseOnlyPredeclaredTolerance(double change,bool expected)=>Assert.That(Determinism.Equivalent(Snapshot(),Snapshot(time:10+change)),Is.EqualTo(expected));
        [Test] public void NaNCannotCompareEqual()=>Assert.That(Determinism.Equivalent(Snapshot(),Snapshot(time:double.NaN)),Is.False);
        [Test] public void SchemaShapeCannotChange()=>Assert.That(Determinism.Equivalent(Snapshot(),new PocObservation{exactShot="Scheduled=true",additional="{}",relativeMs=new double[0]}),Is.False);
        [Test] public void UnknownRecordField_FailsClosed()=>Assert.Throws<AssertionException>(()=>PocObservation.ExactRecord("{\"unapproved\":true}"));
    }
}
