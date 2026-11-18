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
        public string ExpectedHash=>useDefaults?(launch==4?"d09119d6321b":"207da8f2c636"):(launch==4?"8ec4deffc6c1":"28e63d17dfbf");
    }
    [Serializable] internal sealed class ScenarioCatalog {public Scenario[] scenarios;}
    [Serializable] public sealed class Expected
    {
        public string oracle="timing-window/1",closure,result,fail="None",rationale;
        public bool applied,scheduled,checkScheduled;
        public int presentations;
    }
}
