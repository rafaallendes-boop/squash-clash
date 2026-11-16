using System;
using System.IO;
using System.Text;
using SquashClash.Core.Config;
using UnityEngine;
namespace SquashClash.Core.Diagnostics
{
    public sealed class MetricsLog
    {
        [Serializable] private sealed class Record
        {
            public string ts,kind,phase,build,presetName,presetFileHash,effectiveHash,payload,installId;
            public int pending,seed;public bool metricsValid;public long droppedSinceRestart;
        }
        private readonly string directory,buildLabel;
        private string installId;
        private string phase="free";
        public string Phase
        {
            get=>phase;
            set { if(value!="free"&&value!="spike"&&value!="benchmark"&&value!="session")throw new ArgumentOutOfRangeException(nameof(value));phase=value; }
        }
        public string CurrentFilePath { get; }
        public string LastError { get; private set; }
        public string RetentionError { get; }
        public MetricsLog(string directory,BuildInfo build)
        {
            this.directory=Path.GetFullPath(directory??throw new ArgumentNullException(nameof(directory)));
            buildLabel=(build??throw new ArgumentNullException(nameof(build))).Label;
            CurrentFilePath=Path.Combine(this.directory,"metrics-"+DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff")+"-"+Guid.NewGuid().ToString("N")+".jsonl");
            RetentionError=LogRetention.Prune(this.directory,LogFileKind.Metrics,CurrentFilePath);
            LastError=RetentionError;
            if(RetentionError!=null)Debug.LogWarning("[SquashClash] Retención métricas: "+RetentionError);
        }
        public void Write(string kind,EffectiveConfig cfg,string presetName,string presetFileHash,int pendingCount,int seed,bool metricsValid,long droppedSinceRestart,string jsonPayload)
        {
            if(cfg==null)throw new ArgumentNullException(nameof(cfg));
            try
            {
                Directory.CreateDirectory(directory);
                if(installId==null)
                {
                    string path=Path.Combine(directory,"install-id.txt");
                    if(File.Exists(path)&&Guid.TryParse(File.ReadAllText(path).Trim(),out var existing))installId=existing.ToString("D");
                    else {string created=Guid.NewGuid().ToString("D");File.WriteAllText(path,created,new UTF8Encoding(false));installId=created;}
                }
                var record=new Record { ts=DateTime.UtcNow.ToString("O"),kind=kind,phase=phase,build=buildLabel,presetName=presetName,
                    presetFileHash=presetFileHash,effectiveHash=cfg.Hash,pending=pendingCount,seed=seed,metricsValid=metricsValid,
                    droppedSinceRestart=droppedSinceRestart,payload=jsonPayload??"{}",installId=installId };
                // Escaped payload keeps one physical line per record; identity is common to all event kinds.
                File.AppendAllText(CurrentFilePath,JsonUtility.ToJson(record)+"\n",new UTF8Encoding(false));LastError=null;
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is System.Security.SecurityException){LastError=e.Message;}
        }
    }
}
