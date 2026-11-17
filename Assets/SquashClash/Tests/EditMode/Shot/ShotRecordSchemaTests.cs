using NUnit.Framework;
using SquashClash.Core.Shot;
namespace SquashClash.Core.Tests.Shot
{
    // F1-AUTOQA-01: the practice/SL/AutoQA shot event keeps the exact pre-F1 schema; f_r is a Rally/Match-only field.
    public sealed class ShotRecordSchemaTests
    {
        private const string MainPrefix="{\"AidLevel\":0,\"AidCutoffPlanVersion\":0,\"AidCutoff\":false,\"Solved\":false,";
        [Test] public void Practice_Json_OmitsFatigueField_AndMatchesMainSchemaByteForByte()
        {
            var r=new ShotRecord{Seed=7,Power=.5f,f_r=.4};
            string json=r.ToJson(false);
            Assert.That(json,Does.Not.Contain("f_r"));
            Assert.That(json,Does.StartWith(MainPrefix));
            string full=r.ToJson();
            Assert.That(full,Does.StartWith("{\"f_r\":"));
            Assert.That("{"+full.Substring(full.IndexOf(',')+1),Is.EqualTo(json),"only the f_r member differs");
        }
        [Test] public void Rally_Json_KeepsFatigueField()
        {
            Assert.That(new ShotRecord{f_r=.4}.ToJson(true),Does.Contain("\"f_r\":0.4"));
        }
    }
}
