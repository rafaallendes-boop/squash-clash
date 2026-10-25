using System;
using System.Collections.Generic;
using System.Linq;
using SquashClash.Core.Shot;
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
            var candidate=Visible.With(key,value);
            if(!ShotConfigValidator.TryValidate(candidate,out error))throw new ArgumentException(error,nameof(value));
            Visible=candidate;
            if(def.Apply==ApplyClass.Immediate)
            {
                if(Applied.Get(key)==value)return;
                Applied=Applied.With(key,value);
                if(pending.Count==0)Visible=Applied;
                Changed?.Invoke(key);
            }
            else if(Applied.Get(key)==value)pending.Remove(key);else pending.Add(key);
        }
        public ParamSet ApplyPending()
        {
            if(!ShotConfigValidator.TryValidate(Visible,out var error))throw new ArgumentException(error);
            var changed=PendingKeys;Applied=Visible;pending.Clear();
            foreach(var key in changed)Changed?.Invoke(key);
            return Applied;
        }
        public void LoadPreset(PresetFile f, List<string> warnings)
        {
            if(warnings==null)throw new ArgumentNullException(nameof(warnings));
            var loaded=PresetCodec.Overlay(Defaults,f,warnings);
            // Validate the complete candidate before publishing any key or event.
            if(!ShotConfigValidator.TryValidate(loaded,out var error))throw new System.IO.InvalidDataException(error);
            var nextApplied=Applied;var nextPending=new List<string>();var changed=new List<string>();
            foreach(var entry in loaded.Ordered)
            {
                ParamCatalog.TryGet(entry.Key,out var def);
                if(def.Apply==ApplyClass.Immediate)
                {
                    if(Applied.Get(entry.Key)!=entry.Value)changed.Add(entry.Key);
                    nextApplied=nextApplied.With(entry.Key,entry.Value);
                }
                else if(Applied.Get(entry.Key)!=entry.Value)nextPending.Add(entry.Key);
            }
            Applied=nextApplied;Visible=nextPending.Count==0?Applied:loaded;
            pending.Clear();foreach(var key in nextPending)pending.Add(key);
            foreach(var key in changed)Changed?.Invoke(key);
        }
    }
}
