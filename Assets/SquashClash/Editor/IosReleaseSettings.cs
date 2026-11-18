using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace SquashClash.EditorTools
{
    public static class IosReleaseSettings
    {
        private const string IconPath = "Assets/SquashClash/Art/Release/icon-1024.png";
        private const string LaunchPath = "Assets/SquashClash/Art/Release/launch-screen.png";

        [MenuItem("Squash Clash/Connect iOS Release Art")]
        public static void ConnectReleaseArt()
        {
            var icon = ImportArt(IconPath, 1024);
            var launch = ImportArt(LaunchPath, 4096);
            var target = NamedBuildTarget.iOS;
            foreach (var kind in PlayerSettings.GetSupportedIconKinds(target))
            {
                var slots = PlayerSettings.GetPlatformIcons(target, kind);
                foreach (var slot in slots) slot.SetTexture(icon, 0);
                PlayerSettings.SetPlatformIcons(target, kind, slots);
            }
            PlayerSettings.iOS.SetiPhoneLaunchScreenType(iOSLaunchScreenType.ImageAndBackgroundRelative);
            PlayerSettings.iOS.SetiPadLaunchScreenType(iOSLaunchScreenType.ImageAndBackgroundRelative);
            PlayerSettings.iOS.SetLaunchScreenImage(launch, iOSLaunchScreenImageType.iPhonePortraitImage);
            PlayerSettings.iOS.SetLaunchScreenImage(launch, iOSLaunchScreenImageType.iPhoneLandscapeImage);
            PlayerSettings.iOS.SetLaunchScreenImage(launch, iOSLaunchScreenImageType.iPadImage);
            AssetDatabase.SaveAssets();
        }

        private static Texture2D ImportArt(string path, int maximumSize)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing release art: " + path);
            importer.maxTextureSize = maximumSize;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static string GetBuildNumber(string root)
        {
            var count = BuildScript.Git(root, "rev-list --count HEAD");
            if (!int.TryParse(count, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number < 1)
                throw new InvalidOperationException("Git must provide a positive commit count for CFBundleVersion.");
            return number.ToString(CultureInfo.InvariantCulture);
        }

        [PostProcessBuild(1000)]
        private static void AfterBuild(BuildTarget target, string projectPath)
        {
            if (target == BuildTarget.iOS) UpdatePlist(projectPath);
        }

        public static void UpdatePlist(string projectPath)
        {
            var path = Path.Combine(projectPath, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(path);
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            plist.root.SetBoolean("UIFileSharingEnabled", true);
            plist.root.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);
            // Xcode compiles this named asset catalog into the final icon resources.
            foreach (var key in new[] { "CFBundleIcons", "CFBundleIcons~ipad" })
            {
                var icons = plist.root.values.TryGetValue(key, out var value) ? value.AsDict() : plist.root.CreateDict(key);
                var primary = icons.values.TryGetValue("CFBundlePrimaryIcon", out var existing)
                    ? existing.AsDict() : icons.CreateDict("CFBundlePrimaryIcon");
                primary.SetString("CFBundleIconName", "AppIcon");
            }
            plist.WriteToFile(path);
        }
    }
}
