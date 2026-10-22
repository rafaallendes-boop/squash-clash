using System;
using System.Collections.Generic;
namespace SquashClash.Core.Config
{
    [Serializable] public sealed class PresetEntry { public string key; public double value; }
    [Serializable] public sealed class PresetFile
    {
        public int schemaVersion = 1;
        public string name, createdBuild;
        public List<PresetEntry> values = new List<PresetEntry>();
    }
}
