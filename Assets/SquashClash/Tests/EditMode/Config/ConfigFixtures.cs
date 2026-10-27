using System.IO;
using SquashClash.Core.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Config
{
    internal static class ConfigFixtures
    {
        // R2-3: retain the accepted First Ball projection while checking new keys separately.
        internal static readonly string[] FirstBallPrefixes={"court.","camera.","placeholders.","human.","rival.","pace.","physics.","launcher.","debug.","aids."};
        internal static bool IsFirstBallKey(string key)=>System.Array.Exists(FirstBallPrefixes,p=>key.StartsWith(p,System.StringComparison.Ordinal));
        internal static string Json => System.IO.File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json"));
        internal static ParamSet Default() => PresetCodec.LoadDefault(Json);
        internal static System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,double>> PreSlingEntries(ParamSet current)
        {
            var old=JsonUtility.FromJson<PresetFile>(System.IO.File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Tests/Fixtures/pre-sling-defaults.json")));
            var keys=new System.Collections.Generic.HashSet<string>(old.values.ConvertAll(e=>e.key));
            foreach(var e in current.Ordered)if(keys.Contains(e.Key))yield return e;
        }
        internal static string PreSlingHash(ParamSet current)
        {
            var canonical=new System.Text.StringBuilder();
            foreach(var e in PreSlingEntries(current))canonical.Append(e.Key).Append('=').Append(e.Value.ToString("R",System.Globalization.CultureInfo.InvariantCulture)).Append('\n');
            using(var sha=System.Security.Cryptography.SHA256.Create())return System.BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(canonical.ToString())),0,6).Replace("-","").ToLowerInvariant();
        }
        internal static PresetFile File() { NUnit.Framework.Assert.That(PresetCodec.TryParse(Json, out var file, out var error), NUnit.Framework.Is.True, error); return file; }
    }
}
