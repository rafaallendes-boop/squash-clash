using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
namespace SquashClash.AutoQA
{
    internal static partial class Report
    {
        [Serializable] private sealed class TextBox {public string value;}
        internal static string Q(string value){string json=JsonUtility.ToJson(new TextBox{value=value});return json.Substring(9,json.Length-10);}
        internal static Decision Write(string dir,string started,List<Attempt> attempts,Integrity integrity,Dictionary<string,bool> determinism,string error)
        {
            var normal=attempts.Where(a=>!a.scenario.canary).ToArray();var canary=attempts.SingleOrDefault(a=>a.scenario.canary);
            var counts=new Counts{PASS=normal.Count(a=>a.verdict=="PASS"),FAIL=normal.Count(a=>a.verdict=="FAIL"),INCONCLUSIVE=normal.Count(a=>a.verdict=="INCONCLUSIVE"),NOT_EVALUATED=normal.Count(a=>a.verdict=="NOT_EVALUATED")};
            foreach(var a in attempts.Where(a=>a.artifacts!=null))
                integrity.evidenceSha256Ok&=EvidenceCollector.HashFile(Path.Combine(dir,a.artifacts.gameMetrics))==a.artifacts.sha256;
            var verdicts=normal.Select(a=>a.verdict).ToList();if(normal.Length!=18)verdicts.Add("NOT_EVALUATED");
            var d=Triage(verdicts.ToArray(),canary?.verdict,integrity.All,determinism.Values.All(x=>x),error);
            File.WriteAllText(Path.Combine(dir,"attempts.jsonl"),string.Join("\n",attempts.Select(a=>JsonUtility.ToJson(a)))+"\n",new UTF8Encoding(false));
            var report=new StringBuilder("# AutoQA MVP\n\ngate: "+d.gate+"\n\nDriver: editor-deterministic.\n\n");
            report.AppendLine("| Escenario | Repetición | Esperado | Observado | Veredicto |\n| --- | ---: | --- | --- | --- |");
            foreach(var a in attempts)report.AppendLine($"| {a.scenario.id} | {a.scenario.repetition} | {a.expected?.result} | {a.observed?.result} | {a.verdict} |");
            report.AppendLine("\nEscalamiento: "+d.escalationLevel+". Razones: "+string.Join(", ",d.escalationReasons)+".");
            report.AppendLine("\nNo verifica digitalizador, UIKit, IL2CPP, audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación ni diversión.");
            File.WriteAllText(Path.Combine(dir,"report.md"),report.ToString(),new UTF8Encoding(false));
            string git=System.Environment.GetEnvironmentVariable("AUTOQA_GIT_HASH"),command=System.Environment.GetEnvironmentVariable("AUTOQA_COMMAND");
            string summary="{\"schema\":"+Q(SchemaHeader())+",\"runId\":"+Q(Path.GetFileName(dir))+",\"gitHash\":"+Q(git)+",\"dirty\":"+(System.Environment.GetEnvironmentVariable("AUTOQA_DIRTY")=="true"?"true":"false")+
                ",\"command\":"+Q(command)+",\"startedAt\":"+Q(started)+",\"finishedAt\":"+Q(DateTime.UtcNow.ToString("O"))+",\"gate\":"+Q(d.gate)+",\"counts\":"+JsonUtility.ToJson(counts)+
                ",\"canary\":{\"expectedFail\":true,\"observed\":"+(canary==null?"null":Q(canary.verdict))+"},\"determinism\":{"+
                string.Join(",",determinism.OrderBy(x=>x.Key,StringComparer.Ordinal).Select(x=>Q(x.Key)+":{\"identical\":"+(x.Value?"true":"false")+"}"))+"},\"integrity\":"+JsonUtility.ToJson(integrity)+
                ",\"executionError\":"+(error==null?"null":"{\"kind\":\"execution\",\"message\":"+Q(error)+"}")+",\"artifacts\":{\"attemptsSha256\":"+Q(EvidenceCollector.HashFile(Path.Combine(dir,"attempts.jsonl")))+
                ",\"reportSha256\":"+Q(EvidenceCollector.HashFile(Path.Combine(dir,"report.md")))+",\"gameMetrics\":["+string.Join(",",attempts.Where(a=>a.artifacts!=null).Select(a=>JsonUtility.ToJson(new EvidenceRef{path=a.artifacts.gameMetrics,sha256=a.artifacts.sha256})))+
                "]},\"escalationLevel\":"+d.escalationLevel+",\"escalationReasons\":["+string.Join(",",d.escalationReasons.Select(Q))+"],\"exitCode\":"+d.exitCode+"}";
            File.WriteAllText(Path.Combine(dir,"summary.json"),summary+"\n",new UTF8Encoding(false));return d;
        }
    }
}
