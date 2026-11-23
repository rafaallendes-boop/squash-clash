using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace SquashClash.Core.Tests
{
    public sealed class NoAirWordingTests
    {
        [TestCase("Runtime")]
        [TestCase("Core")]
        public void PlayerFacingLiteralsDoNotUseAirWording(string assemblyDirectory)
        {
            var directory = Path.Combine(Application.dataPath, "SquashClash", assemblyDirectory);
            const string airWording = @"""[^""
]*[Aa]l aire[^""
]*""";
            foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
                Assert.That(File.ReadAllText(file), Does.Not.Match(airWording), file);
        }
    }
}
