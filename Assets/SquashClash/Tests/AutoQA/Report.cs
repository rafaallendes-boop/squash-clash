using System;
using System.Collections.Generic;
using System.Linq;
namespace SquashClash.AutoQA
{
    [Serializable] internal sealed class Decision {public string gate;public int exitCode,escalationLevel;public string[] escalationReasons;}
    internal static partial class Report
    {
        internal static Decision Triage(string[] verdicts,string canary,bool integrity,bool deterministic,string executionError=null)
        {
            var reasons=new List<string>();bool error=executionError!=null;
            bool badCanary=canary=="PASS"||(!error&&canary!="FAIL");
            bool badIntegrity=!integrity||badCanary;
            bool fail=verdicts.Contains("FAIL")||!deterministic;
            string gate=error?"ERROR":badIntegrity||fail?"FAIL":verdicts.Contains("INCONCLUSIVE")?"INCONCLUSIVE":verdicts.Length==0||verdicts.Contains("NOT_EVALUATED")?"NOT_EVALUATED":"PASS";
            int exit=badIntegrity?3:error?4:fail?1:gate!="PASS"?2:0;
            if(!integrity)reasons.Add("integrity-check-failed");if(badCanary)reasons.Add("canary-not-failed");
            if(error)reasons.Add("execution-error");if(!deterministic)reasons.Add("determinism-mismatch");
            if(verdicts.Any(v=>v!="PASS")||verdicts.Length==0)reasons.Add("non-pass-attempts");
            return new Decision{gate=gate,exitCode=exit,escalationLevel=badIntegrity?3:exit!=0?2:1,escalationReasons=reasons.ToArray()};
        }
        internal static string SchemaHeader()=>"autoqa-summary/1";
    }
}
