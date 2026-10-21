using System.IO;
using SquashClash.Core.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Config
{
    internal static class ConfigFixtures
    {
        internal static string Json => System.IO.File.ReadAllText(Path.Combine(Application.dataPath, "SquashClash/Runtime/Resources/DefaultPreset.json"));
        internal static ParamSet Default() => PresetCodec.LoadDefault(Json);
        internal static PresetFile File() { NUnit.Framework.Assert.That(PresetCodec.TryParse(Json, out var file, out var error), NUnit.Framework.Is.True, error); return file; }
    }
}
