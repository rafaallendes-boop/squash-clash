using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace SquashClash.Core.Config
{
    public sealed class PresetStore
    {
        private readonly string directory;
        private string ActivePath => Path.Combine(directory,"last-active.txt");
        public PresetStore(string directory) { this.directory=Path.GetFullPath(directory??throw new ArgumentNullException(nameof(directory))); }
        public string LastActiveName
        {
            get { try { var name=File.ReadAllText(ActivePath);return ValidName(name)?name:null; }catch{return null;} }
        }
        public bool TrySave(PresetFile f,out string error)
        {
            try
            {
                if(f==null||!ValidName(f.name))throw new ArgumentException("Invalid preset name.");
                AtomicWrite(Path.Combine(directory,f.name+".json"),PresetCodec.ToJson(f));error=null;return true;
            }
            catch(Exception e){error=e.Message;return false;}
        }
        public bool TryLoad(string name,out PresetFile f,out string error) => Read(name,out f,out _,out error);
        public IReadOnlyList<string> List()
        {
            try { return Array.AsReadOnly(Directory.EnumerateFiles(directory,"*.json").Select(Path.GetFileNameWithoutExtension).Where(ValidName).OrderBy(n=>n,StringComparer.Ordinal).ToArray()); }
            catch{return Array.Empty<string>();}
        }
        public bool TrySetLastActive(string name,out string error)
        {
            if(!TryLoad(name,out _,out error))return false;
            try { AtomicWrite(ActivePath,name);error=null;return true; }
            catch(Exception e){error=e.Message;return false;}
        }
        public ParamSet LoadStartup(ParamSet defaults,List<string> warnings,out string name,out string fileHash)
        {
            if(defaults==null||warnings==null)throw new ArgumentNullException(defaults==null?nameof(defaults):nameof(warnings));
            name="Default";fileHash=null;
            // A first launch has no selection and needs no warning. I/O errors remain distinguishable.
            try
            {
                if(!File.Exists(ActivePath)&&!Directory.Exists(ActivePath))return defaults;
                var selected=File.ReadAllText(ActivePath);
                if(!Read(selected,out var f,out var hash,out var error))throw new IOException(error);
                var applied=PresetCodec.Overlay(defaults,f,warnings);name=selected;fileHash=hash;return applied;
            }
            catch(Exception e){warnings.Add("Last active preset unavailable; using defaults: "+e.Message);return defaults;}
        }
        private bool Read(string name,out PresetFile f,out string hash,out string error)
        {
            f=null;hash=null;
            try
            {
                if(!ValidName(name))throw new ArgumentException("Invalid preset name.");
                var bytes=File.ReadAllBytes(Path.Combine(directory,name+".json"));
                if(!PresetCodec.TryParse(Encoding.UTF8.GetString(bytes),out f,out error))return false;
                using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();
                error=null;return true;
            }
            catch(Exception e){error=e.Message;return false;}
        }
        private void AtomicWrite(string path,string text)
        {
            Directory.CreateDirectory(directory);var temporary=path+".tmp";
            using(var stream=new FileStream(temporary,FileMode.Create,FileAccess.Write,FileShare.None))
            {
                var bytes=new UTF8Encoding(false).GetBytes(text);stream.Write(bytes,0,bytes.Length);stream.Flush(true);
            }
            // Same directory/volume. Existing destination remains intact until the complete temp file is renamed.
            if(File.Exists(path))File.Replace(temporary,path,null);else File.Move(temporary,path);
        }
        private static bool ValidName(string name) => !string.IsNullOrWhiteSpace(name)&&name==name.Trim()&&name!="."&&name!=".."&&
            !name.Any(c=>c<32||"/\\:*?\"<>|".IndexOf(c)>=0||Path.GetInvalidFileNameChars().Contains(c));
    }
}
