using System.IO;
using NUnit.Framework;
using UnityEngine;
namespace SquashClash.Core.Tests
{
    // First run, block D: the release art that scripts/art/make_release_art.py writes. The release lane wires it up.
    public sealed class ReleaseArtTests
    {
        private static string PathOf(string name)=>Path.Combine(Application.dataPath,"SquashClash","Art","Release",name);
        // PNG signature (8 bytes), then the IHDR chunk: length, "IHDR", width, height, bit depth, colour type.
        private static void ReadHeader(string name,out int width,out int height,out int depth,out int colourType)
        {
            string path=PathOf(name);Assert.That(File.Exists(path),Is.True,"missing "+path);
            var b=File.ReadAllBytes(path);Assert.That(b.Length,Is.GreaterThan(33),name);
            Assert.That(new[]{b[0],b[1],b[2],b[3]},Is.EqualTo(new byte[]{0x89,0x50,0x4E,0x47}),name+" is a PNG");
            width=b[16]<<24|b[17]<<16|b[18]<<8|b[19];height=b[20]<<24|b[21]<<16|b[22]<<8|b[23];depth=b[24];colourType=b[25];
        }
        [Test] public void Icon_Is1024Square_WithNoAlphaChannel()
        {
            ReadHeader("icon-1024.png",out int width,out int height,out int depth,out int colourType);
            Assert.That(width,Is.EqualTo(1024));Assert.That(height,Is.EqualTo(1024));Assert.That(depth,Is.EqualTo(8));
            Assert.That(colourType,Is.EqualTo(2),"truecolour without alpha: no transparency and no rounded corners are possible");
            // tRNS would add transparency to an RGB image; the generator never writes it.
            Assert.That(System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(PathOf("icon-1024.png"))),Does.Not.Contain("tRNS"));
        }
        [Test] public void LaunchScreen_IsPortrait_WithNoAlphaChannel()
        {
            ReadHeader("launch-screen.png",out int width,out int height,out int depth,out int colourType);
            Assert.That(width,Is.EqualTo(1290));Assert.That(height,Is.EqualTo(2796));Assert.That(depth,Is.EqualTo(8));Assert.That(colourType,Is.EqualTo(2));
        }
        [Test] public void TheGeneratorScript_IsInTheRepository()
        {
            string script=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"scripts","art","make_release_art.py");
            Assert.That(File.Exists(script),Is.True,"missing "+script);
        }
    }
}
