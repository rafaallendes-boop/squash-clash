using System;
using System.IO;
using System.Linq;
using System.Globalization;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Diagnostics;
using SquashClash.Core.Tests.Config;
namespace SquashClash.Core.Tests
{
    public sealed class LogRetentionTests
    {
        private string root,metrics,spike;
        [SetUp] public void Setup()
        {
            root=Path.Combine(Path.GetTempPath(),"squash-retention-"+Guid.NewGuid().ToString("N"));
            metrics=Path.Combine(root,"metrics");spike=Path.Combine(root,"spike");Directory.CreateDirectory(metrics);Directory.CreateDirectory(spike);
        }
        [TearDown] public void Cleanup(){if(Directory.Exists(root))Directory.Delete(root,true);}
        private string Metric(int index,long bytes=1)=>Make(metrics,"metrics-"+new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc).AddSeconds(index).ToString("yyyyMMdd'T'HHmmssfff",CultureInfo.InvariantCulture)+"-sample.jsonl",bytes);
        private static string Make(string dir,string name,long bytes=1)
        {var path=Path.Combine(dir,name);using(var f=File.Create(path))f.SetLength(bytes);return path;}
        [Test] public void BelowLimitsAndMissingDirectory_KeepFilesWithoutCreatingAnything()
        {
            var files=Enumerable.Range(0,12).Select(i=>Metric(i)).ToArray();
            Assert.That(LogRetention.Prune(metrics,LogFileKind.Metrics),Is.Null);Assert.That(files.All(File.Exists),Is.True);
            var missing=Path.Combine(root,"missing");Assert.That(LogRetention.Prune(missing,LogFileKind.Metrics),Is.Null);Assert.That(Directory.Exists(missing),Is.False);
        }
        [Test] public void OverThirty_DeletesOldestNameTimestamp_EvenWhenMtimeIsReversed()
        {
            var files=Enumerable.Range(0,33).Select(i=>Metric(i)).ToArray();
            for(int i=0;i<files.Length;i++)File.SetLastWriteTimeUtc(files[i],new DateTime(2030,1,1).AddDays(-i));
            Assert.That(LogRetention.Prune(metrics,LogFileKind.Metrics),Is.Null);
            Assert.That(files.Take(3).Any(File.Exists),Is.False);Assert.That(files.Skip(3).All(File.Exists),Is.True);
            Assert.That(Directory.GetFiles(metrics),Has.Length.EqualTo(30));
        }
        [Test] public void ByteLimit_KeepsNewestPrefixAndDoesNotFillHolesWithOlderSmallFiles()
        {
            var oldest=Metric(0,1_000_000);var middle=Metric(1,20_000_000);var newest=Metric(2,31_000_000);
            Assert.That(LogRetention.Prune(metrics,LogFileKind.Metrics),Is.Null);
            Assert.That(File.Exists(newest),Is.True);Assert.That(File.Exists(middle),Is.False);Assert.That(File.Exists(oldest),Is.False);
        }
        [Test] public void ExactByteAndCountLimits_AreInclusive()
        {
            var files=Enumerable.Range(0,30).Select(i=>Metric(i,i==29?49_999_971:1)).ToArray();
            Assert.That(LogRetention.MaxFiles,Is.EqualTo(30));Assert.That(LogRetention.MaxBytes,Is.EqualTo(50_000_000));
            Assert.That(LogRetention.Prune(metrics,LogFileKind.Metrics),Is.Null);Assert.That(files.All(File.Exists),Is.True);
        }
        [Test] public void CurrentOldestFile_IsProtectedAndCountsTowardBothBudgets()
        {
            var current=Metric(0,5_000_000);var old=Metric(1,1_000_000);var middle=Metric(2,20_000_000);var latest=Metric(3,30_000_000);
            Assert.That(LogRetention.Prune(metrics,LogFileKind.Metrics,Path.Combine(metrics,".",Path.GetFileName(current))),Is.Null);
            Assert.That(File.Exists(current),Is.True);Assert.That(File.Exists(latest),Is.True);Assert.That(File.Exists(middle),Is.False);Assert.That(File.Exists(old),Is.False);
            for(int i=4;i<40;i++)Metric(i);
            Assert.That(LogRetention.Prune(metrics,LogFileKind.Metrics,current),Is.Null);Assert.That(File.Exists(current),Is.True);Assert.That(Directory.GetFiles(metrics),Has.Length.EqualTo(30));
        }
        [Test] public void OversizedCurrentFile_IsNeverDeleted_OtherEligibleFilesAreRemoved()
        {
            var current=Metric(0,60_000_000);var other=Metric(1);
            Assert.That(LogRetention.Prune(metrics,LogFileKind.Metrics,current),Is.Null);
            Assert.That(File.Exists(current),Is.True);Assert.That(File.Exists(other),Is.False);
        }
        [Test] public void OnlyValidNamesInRequestedFolder_AreEligible_PresetsTmpAndForeignFilesStayIntact()
        {
            string presets=Path.Combine(root,"presets");Directory.CreateDirectory(presets);
            var protectedPaths=new[]{Make(presets,"P0-calib-1.json"),Make(presets,"last-active.txt"),
                Make(presets,"metrics-20260101T000000000-example.jsonl"),Make(metrics,"last-active"),Make(metrics,"last-active.txt"),
                Make(metrics,"metrics-20260101T000000000-example.jsonl.tmp"),Make(metrics,"P2-20260101-000000-000.md"),
                Make(metrics,"notes.jsonl"),Make(spike,"metrics-20260101T000000000-example.jsonl")};
            var nested=Path.Combine(metrics,"presets");Directory.CreateDirectory(nested);protectedPaths=protectedPaths.Append(Make(nested,"metrics-20260101T000000000-example.jsonl")).ToArray();
            foreach(var p in protectedPaths)File.WriteAllText(p,"preserve exactly");
            for(int i=0;i<35;i++)Metric(i);
            Assert.That(LogRetention.Prune(metrics,LogFileKind.Metrics),Is.Null);Assert.That(LogRetention.Prune(spike,LogFileKind.Spike),Is.Null);
            foreach(var p in protectedPaths)Assert.That(File.ReadAllText(p),Is.EqualTo("preserve exactly"),p);
            Assert.That(File.Exists(Path.Combine(metrics,"metrics-20260101T000000000-sample.jsonl")),Is.False);
        }
        [Test] public void MalformedOrImpossibleUtcNames_AreIgnored_AndDoNotConsumeTheBudget()
        {
            string[] names={"metrics-20260230T000000000-x.jsonl","metrics-20260101T240000000-x.jsonl","metrics-20260101T00000000-x.jsonl",
                "metrics-20260101T000000000-.jsonl","metrics-20260101T000000000-x.jsonl.bak","Metrics-20260101T000000000-x.jsonl",
                "P1-20260230-000000-000.md","P2-20260101-240000-000.md","P3-20260101-000000-000.md","P1-20260101-000000-00.md","P2-20260101-000000-000.md.tmp"};
            var bad=names.Select(n=>Make(n.StartsWith("P",StringComparison.Ordinal)?spike:metrics,n,51_000_000)).ToArray();
            var valid=Metric(1);
            Assert.That(LogRetention.Prune(metrics,LogFileKind.Metrics),Is.Null);Assert.That(LogRetention.Prune(spike,LogFileKind.Spike),Is.Null);
            Assert.That(bad.All(File.Exists),Is.True);Assert.That(File.Exists(valid),Is.True);
        }
        [Test] public void SpikePrefixesShareOneBudget_AndAreOrderedByUtcName()
        {
            var files=Enumerable.Range(0,33).Select(i=>Make(spike,(i%2==0?"P1-":"P2-")+new DateTime(2026,1,1).AddSeconds(i).ToString("yyyyMMdd-HHmmss-fff",CultureInfo.InvariantCulture)+".md")).ToArray();
            for(int i=0;i<files.Length;i++)File.SetLastWriteTimeUtc(files[i],new DateTime(2030,1,1).AddDays(-i));
            Assert.That(LogRetention.Prune(spike,LogFileKind.Spike,files[0]),Is.Null);
            Assert.That(File.Exists(files[0]),Is.True);Assert.That(files.Skip(1).Take(3).Any(File.Exists),Is.False);Assert.That(files.Skip(4).All(File.Exists),Is.True);
        }
        [Test] public void IoFailure_ReturnsErrorWithoutThrowing_AndLeavesDestinationIntact()
        {
            var blocked=Make(root,"blocked");File.WriteAllText(blocked,"do not alter");string error=null;
            Assert.DoesNotThrow(()=>error=LogRetention.Prune(blocked,LogFileKind.Metrics));Assert.That(error,Is.Not.Null.And.Not.Empty);Assert.That(File.ReadAllText(blocked),Is.EqualTo("do not alter"));
        }
        [TestCase("dev")][TestCase("prod")]
        public void MetricsConstructor_PrunesBeforeWritingInBothProfiles(string profile)
        {
            var files=Enumerable.Range(0,33).Select(i=>Metric(i)).ToArray();
            var log=new MetricsLog(metrics,new BuildInfo{version="test",gitHash="retention",profile=profile});
            Assert.That(log.LastError,Is.Null);Assert.That(files.Take(3).Any(File.Exists),Is.False);Assert.That(files.Skip(3).All(File.Exists),Is.True);
            Assert.That(File.Exists(log.CurrentFilePath),Is.False,"Retention does not write a session record");
        }
        [Test] public void MetricsConstructor_IoFailureSetsLastErrorWithoutThrowing()
        {
            var blocked=Make(root,"blocked");MetricsLog log=null;
            Assert.DoesNotThrow(()=>log=new MetricsLog(blocked,new BuildInfo{profile="prod"}));Assert.That(log.LastError,Is.Not.Null.And.Not.Empty);
        }
        [Test] public void Write_DoesNotPruneOrRotateDuringSession_EvenPastBudgets()
        {
            var log=new MetricsLog(metrics,new BuildInfo{profile="dev"});
            using(var f=File.Create(log.CurrentFilePath))f.SetLength(60_000_000);
            var files=Enumerable.Range(0,33).Select(i=>Metric(i)).ToArray();
            Assert.DoesNotThrow(()=>log.Write("frames",EffectiveConfig.From(ConfigFixtures.Default()),"Default","hash",0,0,true,0,"{}"));
            Assert.That(files.All(File.Exists),Is.True);Assert.That(new FileInfo(log.CurrentFilePath).Length,Is.GreaterThan(60_000_000));Assert.That(log.LastError,Is.Null);
        }
        [Test] public void SpikeInitialize_PrunesBeforeReportWriting_AndKeepsCurrentReport()
        {
            var files=Enumerable.Range(0,33).Select(i=>Make(spike,"P2-"+new DateTime(2026,1,1).AddSeconds(i).ToString("yyyyMMdd-HHmmss-fff",CultureInfo.InvariantCulture)+".md")).ToArray();
            WithSpike(section=>
            {
                var type=section.GetType();type.GetProperty("LastReportPath").SetValue(section,files[0]);
                Assert.DoesNotThrow(()=>type.GetMethod("Initialize").Invoke(section,new object[]{null,new UnityEngine.UIElements.VisualElement(),spike}));
                Assert.That(Directory.GetFiles(spike),Has.Length.EqualTo(30));Assert.That(File.Exists(files[0]),Is.True);
                Assert.That(files.Skip(1).Take(3).Any(File.Exists),Is.False);Assert.That(type.GetProperty("LastError").GetValue(section),Is.Null);
            });
        }
        [Test] public void SpikeInitialize_IoFailureSetsLastErrorWithoutThrowing()
        {
            var blocked=Make(root,"blocked");WithSpike(section=>
            {
                var type=section.GetType();Assert.DoesNotThrow(()=>type.GetMethod("Initialize").Invoke(section,new object[]{null,new UnityEngine.UIElements.VisualElement(),blocked}));
                var property=type.GetProperty("LastError");Assert.That(property,Is.Not.Null,"Spike retention errors need LastError");
                Assert.That(property.GetValue(section),Is.Not.Null.And.Not.Empty);Assert.That(File.Exists(blocked),Is.True);
            });
        }
        private static void WithSpike(Action<UnityEngine.Component> check)
        {
            // Load the dev-only UI for its startup I/O boundary, without launching a scene or experiment.
            var type=Type.GetType("SquashClash.DevPanel.SpikeSection, SquashClash.DevPanel",true);
            var go=new UnityEngine.GameObject("Spike retention startup test");
            try {check(go.AddComponent(type));}finally {UnityEngine.Object.DestroyImmediate(go);}
        }
    }
}
