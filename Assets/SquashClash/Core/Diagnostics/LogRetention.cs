using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
namespace SquashClash.Core.Diagnostics
{
    public enum LogFileKind { Metrics, Spike }
    public static class LogRetention
    {
        public const int MaxFiles=30;
        public const long MaxBytes=50_000_000;
        // PO policy: decimal MB, independently for metrics and spike. Only a startup pass;
        // subsequent writes may exceed these budgets until the next startup (no rotation).
        private static readonly Regex MetricsName=new Regex(@"\Ametrics-([0-9]{8}T[0-9]{9})-[^\r\n]+\.jsonl\z",RegexOptions.CultureInvariant);
        private static readonly Regex SpikeName=new Regex(@"\AP[12]-([0-9]{8}-[0-9]{6}-[0-9]{3})\.md\z",RegexOptions.CultureInvariant);
        private sealed class Candidate { public string Path,Name;public DateTime Utc;public long Bytes; }
        public static string Prune(string directory,LogFileKind kind,string currentFilePath=null)
        {
            string error=null;
            try
            {
                if(File.Exists(directory))return "Log retention destination is a file, not a directory: "+directory;
                string[] paths;
                try { paths=Directory.GetFiles(directory); }
                catch(DirectoryNotFoundException){return null;}
                var pattern=kind==LogFileKind.Metrics?MetricsName:SpikeName;
                var format=kind==LogFileKind.Metrics?"yyyyMMdd'T'HHmmssfff":"yyyyMMdd-HHmmss-fff";
                var current=currentFilePath==null?null:System.IO.Path.GetFullPath(currentFilePath);
                var candidates=new List<Candidate>();
                // Top-level only: no traversal into presets or any other subdirectory.
                foreach(var path in paths)
                {
                    string name=System.IO.Path.GetFileName(path);var match=pattern.Match(name);
                    if(!match.Success||!DateTime.TryParseExact(match.Groups[1].Value,format,CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal|DateTimeStyles.AdjustToUniversal,out var utc))continue;
                    try { candidates.Add(new Candidate {Path=System.IO.Path.GetFullPath(path),Name=name,Utc=utc,Bytes=new FileInfo(path).Length}); }
                    catch(Exception e) when(IoError(e)){error=e.Message;} // Preserve files whose size could not be read.
                }
                var protectedFile=candidates.FirstOrDefault(f=>string.Equals(f.Path,current,StringComparison.Ordinal));
                int count=protectedFile==null?0:1;long bytes=protectedFile?.Bytes??0;bool full=false;
                foreach(var file in candidates.OrderByDescending(f=>f.Utc).ThenBy(f=>f.Name,StringComparer.Ordinal))
                {
                    if(file==protectedFile)continue;
                    if(!full&&count<MaxFiles&&file.Bytes<=MaxBytes-bytes){count++;bytes+=file.Bytes;continue;}
                    full=true; // Keep a newest prefix, never fill a size gap with older files.
                    try { File.Delete(file.Path); }
                    catch(Exception e) when(IoError(e)){error=e.Message;}
                }
            }
            catch(Exception e) when(IoError(e)){error=e.Message;}
            return error;
        }
        private static bool IoError(Exception e)=>e is IOException||e is UnauthorizedAccessException||e is System.Security.SecurityException;
    }
}
