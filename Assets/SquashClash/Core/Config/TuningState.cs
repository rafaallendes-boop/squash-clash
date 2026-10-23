using System;
using System.Collections.Generic;
using System.Linq;
namespace SquashClash.Core.Config
{
    public sealed class TuningState
    {
        private readonly HashSet<string> pending = new HashSet<string>(StringComparer.Ordinal);
        public ParamSet Defaults { get; }
        public ParamSet Applied { get; private set; }
        public ParamSet Visible { get; private set; }
        public IReadOnlyCollection<string> PendingKeys => Array.AsReadOnly(pending.OrderBy(k=>k,StringComparer.Ordinal).ToArray());
        public event Action<string> Changed;
        public TuningState(ParamSet defaults, ParamSet applied)
        {
            Defaults=defaults??throw new ArgumentNullException(nameof(defaults));
            Applied=Visible=applied??throw new ArgumentNullException(nameof(applied));
        }
        public void Set(string key, double value)
        {
            if(!ParamCatalog.TryGet(key,out var def))throw new ArgumentException("Unknown parameter: "+key,nameof(key));
            var error=def.Error(value,false);
            if(error!=null)throw new ArgumentOutOfRangeException(nameof(value),error);
            value=Math.Max(def.Min,Math.Min(def.Max,value));
            Visible=Visible.With(key,value);
            if(def.Apply==ApplyClass.Immediate)
            {
                if(Applied.Get(key)==value)return;
                Applied=Applied.With(key,value);Changed?.Invoke(key);
            }
            else if(Applied.Get(key)==value)pending.Remove(key);else pending.Add(key);
        }
        public ParamSet ApplyPending()
        {
            var changed=PendingKeys;Applied=Visible;pending.Clear();
            foreach(var key in changed)Changed?.Invoke(key);
            return Applied;
        }
        public void LoadPreset(PresetFile f, List<string> warnings)
        {
            if(warnings==null)throw new ArgumentNullException(nameof(warnings));
            var loaded=PresetCodec.Overlay(Defaults,f,warnings);
            pending.Clear();Visible=Applied;
            foreach(var entry in loaded.Ordered)Set(entry.Key,entry.Value);
        }
    }
}
