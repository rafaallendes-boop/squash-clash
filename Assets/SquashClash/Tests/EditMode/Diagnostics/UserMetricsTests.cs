using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Diagnostics;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests
{
    public sealed class UserMetricsTests
    {
        [Serializable] private sealed class Event
        {
            public string action,screen,difficulty,hand,result,reason,matchId;
            public double durationSeconds,sessionDurationSeconds;public int step,humanPoints,rivalPoints;
            public bool completed,skipped,abandoned;
        }
        [Serializable] private sealed class Line {public string installId,kind,payload;}
        private readonly List<(string kind,Event value)> rows=new List<(string,Event)>();
        private double now;private UserMetrics metrics;private string directory;
        [SetUp] public void Setup()
        {
            now=10;rows.Clear();directory=Path.Combine(Path.GetTempPath(),"squash-user-metrics-"+Guid.NewGuid().ToString("N"));
            metrics=new UserMetrics((kind,payload)=>rows.Add((kind,JsonUtility.FromJson<Event>(payload))),()=>now);
        }
        [TearDown] public void Cleanup(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        private Event Last(string kind)
        {
            Assert.That(rows.Count(r=>r.kind==kind),Is.GreaterThan(0),"missing event "+kind);
            return rows.Last(r=>r.kind==kind).value;
        }
        [Test] public void AppSession_DeduplicatesPauseAndFocus_AndRecordsDurations()
        {
            metrics.StartSession();now=12;metrics.SetFocused(false);metrics.SetPaused(true);
            now=17;metrics.SetFocused(true);metrics.SetPaused(false);
            now=20;metrics.EndSession();metrics.EndSession();
            var events=rows.Where(r=>r.kind=="app-session").Select(r=>r.value).ToArray();
            Assert.That(events,Has.Length.EqualTo(4));
            Assert.That(events.Select(e=>e.action),Is.EqualTo(new[]{"start","background","foreground","end"}));
            Assert.That(events.Select(e=>e.durationSeconds),Is.EqualTo(new double[]{0,2,5,3}));
            Assert.That(events.Last().sessionDurationSeconds,Is.EqualTo(10));
        }
        [Test] public void Menu_RecordsScreenAndAction()
        {metrics.Menu("menu","menu-play");var e=Last("menu");Assert.That(e.screen,Is.EqualTo("menu"));Assert.That(e.action,Is.EqualTo("menu-play"));}
        [Test] public void TutorialStep_RecordsElapsedTimeAndCompletionOrSkip()
        {
            metrics.BeginTutorialStep(1);now=12;metrics.EndTutorialStep(true,false);
            var completed=Last("tutorial-step");Assert.That(completed.step,Is.EqualTo(1));Assert.That(completed.completed,Is.True);Assert.That(completed.skipped,Is.False);Assert.That(completed.durationSeconds,Is.EqualTo(2));
            metrics.BeginTutorialStep(2);now=15;metrics.EndTutorialStep(false,true);
            var skipped=Last("tutorial-step");Assert.That(skipped.step,Is.EqualTo(2));Assert.That(skipped.completed,Is.False);Assert.That(skipped.skipped,Is.True);Assert.That(skipped.durationSeconds,Is.EqualTo(3));
        }
        [Test] public void MatchStart_CapturesDifficultyHandScoreAndIdentity()
        {
            metrics.BeginMatch("Hard","left");var e=Last("match-start");
            Assert.That(e.difficulty,Is.EqualTo("Hard"));Assert.That(e.hand,Is.EqualTo("left"));Assert.That(Guid.TryParse(e.matchId,out _),Is.True);
            Assert.That(e.humanPoints+e.rivalPoints,Is.Zero);Assert.That(e.result,Is.EqualTo("pending"));Assert.That(e.durationSeconds,Is.Zero);Assert.That(e.abandoned,Is.False);
        }
        [Test] public void MatchEnd_RecordsOneResultAndRetainsStartChoices()
        {
            metrics.BeginMatch("Easy","right");string id=Last("match-start").matchId;
            metrics.UpdateMatchScore(7,3);now=40;metrics.EndMatch(false);metrics.EndMatch(true);
            var e=Last("match-end");Assert.That(rows.Count(r=>r.kind=="match-end"),Is.EqualTo(1));
            Assert.That(e.matchId,Is.EqualTo(id));Assert.That(e.difficulty,Is.EqualTo("Easy"));Assert.That(e.hand,Is.EqualTo("right"));
            Assert.That(e.humanPoints,Is.EqualTo(7));Assert.That(e.rivalPoints,Is.EqualTo(3));Assert.That(e.result,Is.EqualTo("win"));Assert.That(e.durationSeconds,Is.EqualTo(30));Assert.That(e.abandoned,Is.False);
        }
        [Test] public void SessionEnd_RecordsAbandonmentWithoutDuplicatingPointMetrics()
        {
            metrics.StartSession();metrics.BeginMatch("Medium","right");metrics.UpdateMatchScore(1,2);now=14;metrics.EndSession();
            var e=Last("match-end");Assert.That(e.abandoned,Is.True);Assert.That(e.result,Is.EqualTo("abandoned"));Assert.That(e.humanPoints,Is.EqualTo(1));Assert.That(e.rivalPoints,Is.EqualTo(2));
            Assert.That(rows.Any(r=>r.kind=="rally-point"||r.kind=="session-start"||r.kind=="session-summary"),Is.False);
        }
        [TestCase("player")][TestCase("interruption")]
        public void Pause_RecordsTheReason(string reason)
        {metrics.Pause(reason);Assert.That(Last("pause").reason,Is.EqualTo(reason));}
        [TestCase("dev")][TestCase("prod")]
        public void InstallId_IsAnAnonymousGuidOnEveryLine_AndSurvivesLogDeletion(string profile)
        {
            var build=new BuildInfo{version="test",gitHash="metrics",profile=profile};var cfg=EffectiveConfig.From(ConfigFixtures.Default());
            var log=new MetricsLog(directory,build);log.Write("first",cfg,"Default","",0,0,true,0,"{}");log.Write("second",cfg,"Default","",0,0,true,0,"{}");
            var first=File.ReadAllLines(log.CurrentFilePath).Select(JsonUtility.FromJson<Line>).ToArray();
            Assert.That(Guid.TryParse(first[0].installId,out _),Is.True,"installId must be present without device information");
            Assert.That(first[1].installId,Is.EqualTo(first[0].installId));
            foreach(var file in Directory.GetFiles(directory,"metrics-*.jsonl"))File.Delete(file);
            var next=new MetricsLog(directory,build);next.Write("next",cfg,"Default","",0,0,true,0,"{}");
            Assert.That(JsonUtility.FromJson<Line>(File.ReadAllLines(next.CurrentFilePath)[0]).installId,Is.EqualTo(first[0].installId));
            Assert.That(File.Exists(Path.Combine(directory,"install-id.txt")),Is.True);
        }
    }
}
