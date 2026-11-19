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
        public string ExpectedHash=>useDefaults?(launch==4?"d09119d6321b":launch==VolleyLaunch?"24531a50618a":"207da8f2c636"):(launch==4?"8ec4deffc6c1":"28e63d17dfbf");
        // VL1 (practice.launch 6): the volley suite. Its offsets are relative to the volley T*.
        public const int VolleyLaunch=6;
        public bool Volley=>launch==VolleyLaunch;
    }
    [Serializable] internal sealed class ScenarioCatalog {public Scenario[] scenarios;}
    [Serializable] public sealed class Expected
    {
        public string oracle="timing-window/1",closure,result,fail="None",rationale;
        public bool applied,scheduled,checkScheduled;
        public int presentations;
    }
}
