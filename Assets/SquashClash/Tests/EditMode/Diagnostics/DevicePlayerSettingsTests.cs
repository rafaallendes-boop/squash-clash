using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
namespace SquashClash.Core.Tests
{
    // Device robustness, block A (guard): the player settings every iPhone build is exported with. Portrait only, so
    // iOS itself never rotates the app; iOS 15 or later; full screen with the status bar hidden.
    public sealed class DevicePlayerSettingsTests
    {
        private static string Setting(string key)
        {
            string path=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"ProjectSettings","ProjectSettings.asset");
            Assert.That(File.Exists(path),Is.True,"missing "+path);
            var match=Regex.Match(File.ReadAllText(path),@"^  "+Regex.Escape(key)+@": (.*)$",RegexOptions.Multiline);
            Assert.That(match.Success,Is.True,"missing player setting "+key);
            return match.Groups[1].Value.Trim();
        }
        [Test] public void Orientation_IsFixedToPortrait()
        {
            // 0 = Portrait (1 upside down, 2 and 3 landscape, 4 auto rotation).
            Assert.That(Setting("defaultScreenOrientation"),Is.EqualTo("0"),"a fixed portrait orientation, not auto rotation");
        }
        [Test] public void Target_IsIPhoneFullScreen_FromIos15()
        {
            Assert.That(Setting("iOSTargetOSVersionString"),Is.EqualTo("15.0"));
            Assert.That(Setting("targetDevice"),Is.EqualTo("0"),"iPhone only");
            Assert.That(Setting("uIRequiresFullScreen"),Is.EqualTo("1"));
            Assert.That(Setting("uIStatusBarHidden"),Is.EqualTo("1"),"the safe-area insets assume no status bar");
        }
    }
}
