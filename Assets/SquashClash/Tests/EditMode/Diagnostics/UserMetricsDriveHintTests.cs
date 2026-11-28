using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SquashClash.Core.Diagnostics;
using UnityEngine;
namespace SquashClash.Core.Tests
{
    // Loose-drive hint (spec 2026-11-28, P13). One additive line. The lines that already exist stay identical.
    public sealed class UserMetricsDriveHintTests
    {
        [Serializable] private sealed class Event
        {
            public string matchId,kind,reason,id,action,where,difficulty,hand,result;
            public float bounceX,bounceZ;
            public bool seen,chosen,hintSeen,abandoned;
            public int humanPoints,rivalPoints;
        }
        private readonly List<(string kind,string payload)> rows=new List<(string,string)>();
        private UserMetrics metrics;
        [SetUp] public void Setup(){rows.Clear();metrics=new UserMetrics((kind,payload)=>rows.Add((kind,payload)),()=>10);}
        private Event Only(string kind)
        {
            var matches=rows.Where(r=>r.kind==kind).ToArray();
            Assert.That(matches.Length,Is.EqualTo(1),"exactly one "+kind+" line");
            return JsonUtility.FromJson<Event>(matches[0].payload);
        }
        private static string[] Keys(string json)=>Regex.Matches(json,"\"([A-Za-z0-9_]+)\":").Cast<Match>().Select(m=>m.Groups[1].Value).ToArray();

        [Test]
        public void DriveHint_WritesOneLineForTheNamedMatch_WithTheBounceAndWhetherItWasSeen()
        {
            metrics.BeginMatch("Medium","right");
            string match=Only("match-start").matchId;
            metrics.DriveHint(match,"Short",1.25f,5.5f,true,"");
            var e=Only("drive-hint");
            Assert.That(e.matchId,Is.EqualTo(match));
            Assert.That(e.kind,Is.EqualTo("Short"));
            Assert.That(e.bounceX,Is.EqualTo(1.25f));
            Assert.That(e.bounceZ,Is.EqualTo(5.5f));
            Assert.That(e.seen,Is.True);
            Assert.That(e.reason,Is.EqualTo(""));
            Assert.That(Keys(rows.Last(r=>r.kind=="drive-hint").payload),Is.EqualTo(new[]{"matchId","kind","bounceX","bounceZ","seen","reason"}));
        }

        [TestCase("limit")]
        [TestCase("firstMatchHint")]
        [TestCase("notSeen")]
        public void DriveHint_RecordsWhyItWasNotSeen(string reason)
        {
            metrics.BeginMatch("Medium","right");
            metrics.DriveHint(metrics.OpenMatchId,"Wide",0.4f,8f,false,reason);
            var e=Only("drive-hint");
            Assert.That(e.seen,Is.False);
            Assert.That(e.reason,Is.EqualTo(reason));
            Assert.That(e.kind,Is.EqualTo("Wide"));
        }

        [Test]
        public void DriveHint_UsesTheMatchIdItIsGiven_NotALaterMatch()
        {
            metrics.BeginMatch("Medium","right");
            string first=metrics.OpenMatchId;
            metrics.BeginMatch("Hard","left");
            Assert.That(metrics.OpenMatchId,Is.Not.EqualTo(first));
            metrics.DriveHint(first,"ShortAndWide",3.2f,5.4f,false,"notSeen");
            Assert.That(Only("drive-hint").matchId,Is.EqualTo(first));
        }

        [Test]
        public void CurrentLines_StayTheSame_WhenADriveHintIsWritten()
        {
            metrics.BeginMatch("Medium","right");
            metrics.UpdateMatchScore(3,2);
            metrics.Hint("serve","seen","banner");
            metrics.ServeChoice(true,"Drive",false);
            metrics.EndMatch(false);
            var before=rows.Select(r=>(r.kind,r.payload)).ToArray();
            Assert.That(before.Select(r=>r.kind),Is.EqualTo(new[]{"match-start","hint","serve-choice","match-end"}));
            Assert.That(Keys(before[0].payload),Is.EqualTo(new[]{"matchId","difficulty","hand","result","humanPoints","rivalPoints","durationSeconds","abandoned"}));
            Assert.That(Keys(before[1].payload),Is.EqualTo(new[]{"matchId","id","action","where"}));
            Assert.That(Keys(before[2].payload),Is.EqualTo(new[]{"matchId","kind","chosen","hintSeen"}));
            Assert.That(before[0].payload,Does.Contain("\"result\":\"pending\""));
            Assert.That(before[3].payload,Does.Contain("\"result\":\"win\"").And.Contain("\"humanPoints\":3"));
            metrics.DriveHint(metrics.OpenMatchId,"Short",1f,2f,true,"");
            Assert.That(rows.Take(before.Length).Select(r=>(r.kind,r.payload)),Is.EqualTo(before),"the lines already written are byte for byte the same");
            Assert.That(rows.Count(r=>r.kind=="drive-hint"),Is.EqualTo(1));
        }
    }
}
