using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using SquashClash.EditorTools;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace SquashClash.Core.Tests
{
    public sealed class IosReleaseLaneTests
    {
        private string directory;
        [SetUp] public void Setup(){directory=Path.Combine(Path.GetTempPath(),"squash-release-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);}
        [TearDown] public void Cleanup(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        [Test] public void IosIcon_IncludingMarketing1024_IsBoundToTheReleaseAsset()
        {
            var target=NamedBuildTarget.iOS;var kinds=PlayerSettings.GetSupportedIconKinds(target);
            var slots=kinds.SelectMany(k=>PlayerSettings.GetPlatformIcons(target,k)).ToArray();
            Assert.That(slots.Any(i=>i.width==1024&&i.height==1024),Is.True,"required marketing icon slot");
            foreach(var icon in slots)Assert.That(AssetDatabase.GetAssetPath(icon.GetTexture(0)),Is.EqualTo("Assets/SquashClash/Art/Release/icon-1024.png"),icon.width+"x"+icon.height);
        }
        [Test] public void LaunchScreen_IsBoundAtFullResolution_ForPhoneAndPad()
        {
            var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            foreach(string key in new[]{"iOSLaunchScreenPortrait","iOSLaunchScreeniPadImage"})
                Assert.That(AssetDatabase.GetAssetPath(settings.FindProperty(key).objectReferenceValue),Is.EqualTo("Assets/SquashClash/Art/Release/launch-screen.png"),key);
            Assert.That(settings.FindProperty("iOSLaunchScreenType").intValue,Is.EqualTo((int)iOSLaunchScreenType.ImageAndBackgroundRelative));
            Assert.That(settings.FindProperty("iOSLaunchScreeniPadType").intValue,Is.EqualTo((int)iOSLaunchScreenType.ImageAndBackgroundRelative));
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SquashClash/Art/Release/launch-screen.png");
            Assert.That(texture.height,Is.EqualTo(2796));
        }
        private void Git(string args)
        {
            var info=new ProcessStartInfo("git",args){WorkingDirectory=directory,UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true};
            using(var process=Process.Start(info)){string output=process.StandardOutput.ReadToEnd(),error=process.StandardError.ReadToEnd();process.WaitForExit();Assert.That(process.ExitCode,Is.Zero,output+error);}
        }
        [Test] public void BuildNumber_GrowsWithCommitCount_WithoutEditingProjectSettings()
        {
            string previous=PlayerSettings.iOS.buildNumber;
            Git("init -q");Git("-c user.name=ReleaseFixture -c user.email=fixture@example.invalid commit --allow-empty -qm first");
            Assert.That(IosReleaseSettings.GetBuildNumber(directory),Is.EqualTo("1"));
            Git("-c user.name=ReleaseFixture -c user.email=fixture@example.invalid commit --allow-empty -qm second");
            Assert.That(IosReleaseSettings.GetBuildNumber(directory),Is.EqualTo("2"));Assert.That(PlayerSettings.iOS.buildNumber,Is.EqualTo(previous));
        }
        [TestCase("ITSAppUsesNonExemptEncryption",false)][TestCase("UIFileSharingEnabled",true)][TestCase("LSSupportsOpeningDocumentsInPlace",true)]
        public void PlistPostprocess_SetsBooleanFlagsAndPreservesExistingData(string key,bool expected)
        {
            string path=Path.Combine(directory,"Info.plist");
            File.WriteAllText(path,"<?xml version=\"1.0\" encoding=\"UTF-8\"?><plist version=\"1.0\"><dict><key>CFBundleIdentifier</key><string>cl.test.squash</string><key>custom</key><string>keep</string></dict></plist>");
            IosReleaseSettings.UpdatePlist(directory);var root=XDocument.Load(path).Root.Element("dict");
            var entry=root.Elements("key").FirstOrDefault(e=>e.Value==key);Assert.That(entry,Is.Not.Null,"missing "+key);
            Assert.That(((XElement)entry.NextNode).Name.LocalName,Is.EqualTo(expected?"true":"false"));
            Assert.That(root.Elements("key").Single(e=>e.Value=="CFBundleIdentifier").NextNode.ToString(),Does.Contain("cl.test.squash"));
            Assert.That(root.Elements("key").Single(e=>e.Value=="custom").NextNode.ToString(),Does.Contain("keep"));
        }
    }
}
