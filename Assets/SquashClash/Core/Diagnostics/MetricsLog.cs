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
            public string ts,kind,phase,build,presetName,presetFileHash,effectiveHash,payload;
            public int pending,seed;public bool metricsValid;public long droppedSinceRestart;
        }
        private readonly string directory,buildLabel;
        private string phase="free";
        public string Phase
        {
            get=>phase;
            set { if(value!="free"&&value!="spike"&&value!="benchmark"&&value!="session")throw new ArgumentOutOfRangeException(nameof(value));phase=value; }
        }
        public string CurrentFilePath { get; }
        public string LastError { get; private set; }
        public MetricsLog(string directory,BuildInfo build)
        {
            this.directory=Path.GetFullPath(directory??throw new ArgumentNullException(nameof(directory)));
            buildLabel=(build??throw new ArgumentNullException(nameof(build))).Label;
            CurrentFilePath=Path.Combine(this.directory,"metrics-"+DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff")+"-"+Guid.NewGuid().ToString("N")+".jsonl");
            LastError=LogRetention.Prune(this.directory,LogFileKind.Metrics,CurrentFilePath);
        }
        public void Write(string kind,EffectiveConfig cfg,string presetName,string presetFileHash,int pendingCount,int seed,bool metricsValid,long droppedSinceRestart,string jsonPayload)
        {
            if(cfg==null)throw new ArgumentNullException(nameof(cfg));
            var record=new Record { ts=DateTime.UtcNow.ToString("O"),kind=kind,phase=phase,build=buildLabel,presetName=presetName,
                presetFileHash=presetFileHash,effectiveHash=cfg.Hash,pending=pendingCount,seed=seed,metricsValid=metricsValid,
                droppedSinceRestart=droppedSinceRestart,payload=jsonPayload??"{}" };
            // Payload is an escaped JSON string. One physical line remains one complete record, even for multiline input.
            var line=JsonUtility.ToJson(record)+"\n";
            try { Directory.CreateDirectory(directory);File.AppendAllText(CurrentFilePath,line,new UTF8Encoding(false));LastError=null; }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is System.Security.SecurityException){LastError=e.Message;}
        }
    }
}
