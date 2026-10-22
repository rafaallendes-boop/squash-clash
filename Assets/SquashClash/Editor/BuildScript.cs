using System;
using System.IO;
using System.Text;
using SquashClash.Core.Diagnostics;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SquashClash.EditorTools
{
    public static class BuildScript
    {
        public static void BuildIos()
        {
            try
            {
                var profile = Environment.GetEnvironmentVariable("BUILD_PROFILE");
                if (profile != "dev" && profile != "prod" && profile != "debug")
                    throw new InvalidOperationException("BUILD_PROFILE must be dev, prod or debug.");
                var target = NamedBuildTarget.iOS;
                var bundle = Environment.GetEnvironmentVariable("BUNDLE_ID");
                if (string.IsNullOrWhiteSpace(bundle) || PlayerSettings.GetApplicationIdentifier(target) != bundle)
                    throw new InvalidOperationException("BUNDLE_ID must match the configured iOS application identifier.");
                var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                var output = Path.Combine(root, "Builds", "iOS-" + profile);
                if (Directory.Exists(output)) Directory.Delete(output, true);

                var info = new BuildInfo
                {
                    version = PlayerSettings.bundleVersion,
                    gitHash = Git(root, "rev-parse --short HEAD"),
                    gitDirty = Git(root, "status --porcelain").Length != 0,
                    profile = profile,
                    builtAtUtc = DateTime.UtcNow.ToString("O"),
                    unityVersion = Application.unityVersion
                };
                const string metadataPath = "Assets/SquashClash/Runtime/Resources/BuildInfo.json";
                var json = JsonUtility.ToJson(info, true);
                Directory.CreateDirectory(Path.GetDirectoryName(metadataPath));
                File.WriteAllText(metadataPath, json, new UTF8Encoding(false));
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.ImportAsset(metadataPath, ImportAssetOptions.ForceSynchronousImport);
                var imported = AssetDatabase.LoadAssetAtPath<TextAsset>(metadataPath);
                if (imported == null || imported.text != json)
                    throw new InvalidOperationException("Build metadata did not import synchronously.");

                var persistent = PlayerSettings.GetScriptingDefineSymbols(target).Split(';');
                if (profile == "prod" && Array.Exists(persistent, value => value.Trim() == "SQUASH_DEV"))
                    throw new InvalidOperationException("Production build rejected: persistent SQUASH_DEV define.");
                var debug = profile == "debug";
                PlayerSettings.SetIl2CppCompilerConfiguration(target,
                    debug ? Il2CppCompilerConfiguration.Debug : Il2CppCompilerConfiguration.Release);
                var options = new BuildPlayerOptions
                {
                    scenes = new[] { ProjectSetup.MainScene },
                    locationPathName = output,
                    target = BuildTarget.iOS,
                    options = debug ? BuildOptions.Development | BuildOptions.AllowDebugging : BuildOptions.None,
                    extraScriptingDefines = profile == "prod" ? Array.Empty<string>() : new[] { "SQUASH_DEV" }
                };
                Debug.Log($"[SquashClash] build {info.Label} development={debug} il2cpp={(debug ? "Debug" : "Release")} extraDefines={string.Join(",", options.extraScriptingDefines)}");
                var thermal = AssetImporter.GetAtPath("Assets/SquashClash/DevPanel/Native/ThermalState.mm") as PluginImporter;
                if (thermal == null) throw new InvalidOperationException("Missing native thermal diagnostic importer.");
                // Native .mm inclusion is explicit; assembly constraints alone cannot exclude it.
                thermal.SetIncludeInBuildDelegate(_ => profile != "prod");
                BuildReport report;
                try { report = BuildPipeline.BuildPlayer(options); }
                finally { thermal.SetIncludeInBuildDelegate(null); }
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException($"iOS export failed: {report.summary.result}, errors={report.summary.totalErrors}");
                Debug.Log($"[SquashClash] iOS export succeeded: {output}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static string Git(string root, string arguments)
        {
            var start = new System.Diagnostics.ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var process = System.Diagnostics.Process.Start(start))
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0) throw new InvalidOperationException("git " + arguments + ": " + error);
                return output.Trim();
            }
        }
    }
}
