using System;
namespace SquashClash.AutoQA
{
    [Serializable] public sealed class Scenario
    {
        public string id,kind,deliberateExpectedResult;
        public int launch;
        public float aimX=3.2f,aimY=2.2f,pullMm=18.5f;
        public double releaseOffsetMs,durationMs=300;
        public bool canary;
        public string ExpectedHash=>launch==4?"e2ffa032a588":"9041c4aafc77";
    }
    [Serializable] internal sealed class ScenarioCatalog {public Scenario[] scenarios;}
    [Serializable] public sealed class Expected
    {
        public string oracle="timing-window/1",closure,result,fail="None",rationale;
        public bool applied,scheduled,checkScheduled;
        public int presentations;
    }
}
