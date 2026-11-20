using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using UnityEditor;
namespace SquashClash.Core.Tests
{
    // Store preparation (2026-11-21): app-level privacy manifest and no Unity data submission.
    public sealed class StorePrepTests
    {
        private const string ManifestPath="Assets/Plugins/iOS/PrivacyInfo.xcprivacy";
        private static XElement Manifest()
        {
            Assert.That(File.Exists(ManifestPath),Is.True,"missing "+ManifestPath);
            return XDocument.Load(ManifestPath).Root.Element("dict");
        }
        private static XElement ValueOf(XElement dict,string key)=>(XElement)dict.Elements("key").Single(e=>e.Value==key).NextNode;
        [Test] public void PrivacyManifest_DeclaresNoTrackingAndNoCollectedData()
        {
            var dict=Manifest();
            Assert.That(ValueOf(dict,"NSPrivacyTracking").Name.LocalName,Is.EqualTo("false"));
            Assert.That(ValueOf(dict,"NSPrivacyTrackingDomains").Elements().Count(),Is.Zero);
            Assert.That(ValueOf(dict,"NSPrivacyCollectedDataTypes").Elements().Count(),Is.Zero);
        }
        [Test] public void PrivacyManifest_DeclaresExactlyTheRequiredReasonApisTheAppUses()
        {
            var types=ValueOf(Manifest(),"NSPrivacyAccessedAPITypes").Elements("dict")
                .ToDictionary(d=>ValueOf(d,"NSPrivacyAccessedAPIType").Value,
                    d=>string.Join(",",ValueOf(d,"NSPrivacyAccessedAPITypeReasons").Elements("string").Select(s=>s.Value)));
            // PlayerPrefs (UserDefaults) and file size metadata of the app's own Documents (LogRetention).
            Assert.That(types.Keys.OrderBy(k=>k),Is.EqualTo(new[]{"NSPrivacyAccessedAPICategoryFileTimestamp","NSPrivacyAccessedAPICategoryUserDefaults"}));
            Assert.That(types["NSPrivacyAccessedAPICategoryUserDefaults"],Is.EqualTo("CA92.1"));
            Assert.That(types["NSPrivacyAccessedAPICategoryFileTimestamp"],Is.EqualTo("C617.1"));
        }
        [Test] public void PrivacyManifest_IsAnIosOnlyPlugin()
        {
            var importer=AssetImporter.GetAtPath(ManifestPath) as PluginImporter;
            Assert.That(importer,Is.Not.Null,"plugin importer");
            Assert.That(importer.GetCompatibleWithPlatform(BuildTarget.iOS),Is.True);
            Assert.That(importer.GetCompatibleWithAnyPlatform(),Is.False);
            Assert.That(importer.GetCompatibleWithEditor(),Is.False);
        }
        [Test] public void ShippedCode_HasNoNetworkOrAnalyticsApis()
        {
            string[] banned={"UnityWebRequest","HttpClient","WebClient","System.Net.","UnityEngine.Analytics","Unity.Services","CrashReport","Application.OpenURL"};
            foreach(var file in Directory.GetFiles("Assets/SquashClash","*.cs",SearchOption.AllDirectories).Where(f=>!f.Contains("/Tests/")&&!f.Contains("/Editor/")))
            {var text=File.ReadAllText(file);foreach(var token in banned)Assert.That(text,Does.Not.Contain(token),file);}
        }
        [Test] public void UnityDataSubmission_IsDisabled()
        {
            var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            Assert.That(settings.FindProperty("submitAnalytics").boolValue,Is.False,"submitAnalytics");
            Assert.That(settings.FindProperty("enableCrashReportAPI").boolValue,Is.False,"enableCrashReportAPI");
        }
    }
}
