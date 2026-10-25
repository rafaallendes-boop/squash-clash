using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using SquashClash.Core.Shot;

namespace SquashClash.Core.Config
{
    public static class PresetCodec
    {
        public const int SupportedSchema = 1;

        public static string ToJson(PresetFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            var json = JsonUtility.ToJson(file, true);
            if (!TryParse(json, out _, out var error)) throw new InvalidDataException(error);
            return json;
        }

        public static bool TryParse(string json, out PresetFile file, out string error)
        {
            file = null;
            error = null;
            try
            {
                // JsonUtility maps the DTO; this guard prevents omitted/mistyped fields from becoming valid zeroes.
                PresetJsonShape.Validate(json);
                var parsed = JsonUtility.FromJson<PresetFile>(json);
                error = StructuralError(parsed);
                if (error != null) return false;
                file = parsed;
                return true;
            }
            catch (Exception exception)
            {
                error = "Invalid preset JSON: " + exception.Message;
                return false;
            }
        }

        public static ParamSet LoadDefault(string json)
        {
            if (!TryParse(json, out var file, out var error)) throw new InvalidDataException(error);
            var errors = new List<string>();
            var values = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var entry in file.values)
            {
                if (!ParamCatalog.TryGet(entry.key, out var def)) errors.Add("Unknown key: " + entry.key);
                else
                {
                    error = def.Error(entry.value);
                    if (error != null) errors.Add(error);
                    values[entry.key] = entry.value;
                }
            }
            foreach (var def in ParamCatalog.All)
                if (!values.ContainsKey(def.Key)) errors.Add("Missing key: " + def.Key);
            if (errors.Count != 0) throw new InvalidDataException(string.Join("\n", errors));
            var result=new ParamSet(values);
            if(!ShotConfigValidator.TryValidate(result,out error))throw new InvalidDataException(error);
            return result;
        }

        public static ParamSet Overlay(ParamSet baseSet, PresetFile file, List<string> warnings)
        {
            var structuralError = StructuralError(file);
            if (structuralError != null) throw new InvalidDataException(structuralError);
            var values = baseSet.Ordered.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
            foreach (var entry in file.values)
            {
                if (!ParamCatalog.TryGet(entry.key, out var def)) { warnings.Add("Unknown key: " + entry.key); continue; }
                var error = def.Error(entry.value, false);
                if (error != null) { warnings.Add(error); continue; }
                var value = Math.Max(def.Min, Math.Min(def.Max, entry.value));
                if (value != entry.value) warnings.Add(entry.key + ": clamped to range.");
                values[entry.key] = value;
            }
            var result=new ParamSet(values);
            if(!ShotConfigValidator.TryValidate(result,out var validationError))throw new InvalidDataException(validationError);
            return result;
        }

        public static PresetFile FromSet(ParamSet set, string name, string build) => new PresetFile
        {
            schemaVersion = SupportedSchema, name = name, createdBuild = build,
            values = set.Ordered.Select(entry => new PresetEntry { key = entry.Key, value = entry.Value }).ToList()
        };

        private static string StructuralError(PresetFile file)
        {
            if (file == null || file.schemaVersion != SupportedSchema) return "Unsupported preset schemaVersion.";
            if (file.values == null) return "Missing values array.";
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in file.values)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.key)) return "Missing parameter key.";
                if (!keys.Add(entry.key)) return "Duplicate parameter key: " + entry.key;
            }
            return null;
        }
    }
}
