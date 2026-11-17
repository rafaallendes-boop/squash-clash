using System;
namespace SquashClash.AutoQA
{
    [Serializable] public sealed class Scenario
    {
        public string id,kind,deliberateExpectedResult;
        public int launch;
        public float aimX=3.2f,aimY=2.2f,pullMm=18.5f;
        public double releaseOffsetMs,durationMs=300;
        public bool canary,useDefaults;
        public string ExpectedHash=>useDefaults?(launch==4?"750c407c8a2f":"2ec0c953786a"):(launch==4?"95a3f24313fe":"e8df60f37d4b");
    }
    [Serializable] internal sealed class ScenarioCatalog {public Scenario[] scenarios;}
    [Serializable] public sealed class Expected
    {
        public string oracle="timing-window/1",closure,result,fail="None",rationale;
        public bool applied,scheduled,checkScheduled;
        public int presentations;
    }
}
