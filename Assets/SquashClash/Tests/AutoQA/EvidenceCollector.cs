using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace SquashClash.AutoQA
{
    internal static class EvidenceCollector
    {
        internal static string Sha(byte[] data){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(data)).Replace("-","").ToLowerInvariant();}
        internal static string ShaText(string text)=>Sha(Encoding.UTF8.GetBytes(text));
        internal static string HashFile(string path)=>Sha(File.ReadAllBytes(path));
        internal static string PresetsSnapshot(string directory)=>Directory.Exists(directory)?ShaText(string.Join("\n",Directory.GetFiles(directory,"*",SearchOption.AllDirectories).OrderBy(p=>p,StringComparer.Ordinal).Select(p=>p.Substring(directory.Length)+":"+HashFile(p)))):ShaText("absent");
        internal static ArtifactRef Copy(string source,string runDir,string name)
        {
            string relative="game-metrics/"+name+".jsonl",target=Path.Combine(runDir,relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target));if(File.Exists(target))throw new IOException("Evidence destination already exists");
            File.Copy(source,target);return new ArtifactRef{gameMetrics=relative,sha256=HashFile(target)};
        }
    }
}
