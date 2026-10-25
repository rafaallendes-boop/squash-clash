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
        internal static PresetFile File() { NUnit.Framework.Assert.That(PresetCodec.TryParse(Json, out var file, out var error), NUnit.Framework.Is.True, error); return file; }
    }
}
