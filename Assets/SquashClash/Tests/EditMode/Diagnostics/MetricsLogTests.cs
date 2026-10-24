using System;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Diagnostics;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests
{
    public sealed class MetricsLogTests
    {
        [Serializable] private sealed class Record { public string ts,kind,phase,build,presetName,presetFileHash,effectiveHash,payload;public int pending,seed;public bool metricsValid;public long droppedSinceRestart; }
        private string directory;
        [SetUp] public void Setup(){directory=Path.Combine(Path.GetTempPath(),"squash-metrics-"+Guid.NewGuid().ToString("N"));}
        [TearDown] public void Cleanup(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        private static BuildInfo Build()=>new BuildInfo{version="test",gitHash="abc",profile="dev"};
        [Test] public void EveryLineCarriesTraceabilityAndPayloadCannotInjectLines()
        {
            var b=Build();var log=new MetricsLog(directory,b);var cfg=EffectiveConfig.From(ConfigFixtures.Default());
            string payload="{\n\"message\":\"quoted \\\"text\\\"\"\n}";
            log.Write("launch",cfg,"preset\nname","filehash",2,123,true,0,payload);
            var changed=EffectiveConfig.From(cfg.Source.With("physics.gravity",12));log.Phase="benchmark";
            log.Write("frames",changed,"preset","second",0,456,false,5,"{}");
            var lines=File.ReadAllLines(log.CurrentFilePath);Assert.That(lines,Has.Length.EqualTo(2));
            var a=JsonUtility.FromJson<Record>(lines[0]);var c=JsonUtility.FromJson<Record>(lines[1]);
            Assert.That(DateTimeOffset.TryParse(a.ts,out _),Is.True);Assert.That(a.kind,Is.EqualTo("launch"));Assert.That(a.phase,Is.EqualTo("free"));
            Assert.That(a.build,Is.EqualTo(b.Label));Assert.That(a.presetName,Is.EqualTo("preset\nname"));Assert.That(a.presetFileHash,Is.EqualTo("filehash"));
            Assert.That(a.effectiveHash,Is.EqualTo(cfg.Hash));Assert.That(a.pending,Is.EqualTo(2));Assert.That(a.seed,Is.EqualTo(123));Assert.That(a.metricsValid,Is.True);Assert.That(a.payload,Is.EqualTo(payload));
            Assert.That(c.effectiveHash,Is.EqualTo(changed.Hash));Assert.That(c.phase,Is.EqualTo("benchmark"));Assert.That(c.metricsValid,Is.False);Assert.That(c.droppedSinceRestart,Is.EqualTo(5));
            Assert.That(log.LastError,Is.Null);
        }
        [Test] public void FilesAreDistinctAndInvalidPhaseIsRejected()
        {
            var a=new MetricsLog(directory,Build());var b=new MetricsLog(directory,Build());
            Assert.That(a.CurrentFilePath,Is.Not.Null.And.Not.EqualTo(b.CurrentFilePath));
            Assert.Throws<ArgumentOutOfRangeException>(()=>a.Phase="unknown");
        }
        [Test] public void UnwritableDestinationDoesNotCrashGameAndReportsError()
        {
            Directory.CreateDirectory(directory);var file=Path.Combine(directory,"not-a-directory");File.WriteAllText(file,"keep");
            var log=new MetricsLog(file,Build());
            Assert.DoesNotThrow(()=>log.Write("launch",EffectiveConfig.From(ConfigFixtures.Default()),"Default","hash",0,0,true,0,"{}"));
            Assert.That(log.LastError,Is.Not.Empty);Assert.That(File.ReadAllText(file),Is.EqualTo("keep"));
        }
    }
}
