using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Diagnostics;
using UnityEngine;
namespace SquashClash.Core.Tests
{
    // First-match hints (spec front 2, section 1.4 and 1.5): the two new lines are additive and the current ones do not change.
    // A file of its own so that the existing UserMetricsTests stay as they are.
    public sealed class UserMetricsHintTests
    {
        [Serializable] private sealed class Event {public string matchId,id,action,where,kind,difficulty,hand,result;public bool chosen,hintSeen,abandoned;public int humanPoints,rivalPoints;}
        private readonly List<(string kind,string payload)> rows=new List<(string,string)>();
        private UserMetrics metrics;
        [SetUp] public void Setup(){rows.Clear();metrics=new UserMetrics((kind,payload)=>rows.Add((kind,payload)),()=>10);}
        private Event Only(string kind)
        {
            var matches=rows.Where(r=>r.kind==kind).ToArray();Assert.That(matches.Length,Is.EqualTo(1),"exactly one "+kind+" line");
            return JsonUtility.FromJson<Event>(matches[0].payload);
        }
        [Test] public void Hint_Seen_WritesOneLineWithTheMatchIdTheIdAndWhere()
        {
            metrics.BeginMatch("Medium","right");string match=Only("match-start").matchId;
            metrics.Hint("serve","seen","guide");
            var e=Only("hint");Assert.That(e.matchId,Is.EqualTo(match));Assert.That(e.id,Is.EqualTo("serve"));Assert.That(e.action,Is.EqualTo("seen"));Assert.That(e.where,Is.EqualTo("guide"));
        }
        [Test] public void Hint_Retired_HasNoWhere()
        {
            metrics.BeginMatch("Medium","right");metrics.Hint("boast","retired","");
            var e=Only("hint");Assert.That(e.id,Is.EqualTo("boast"));Assert.That(e.action,Is.EqualTo("retired"));Assert.That(e.where,Is.Empty);
        }
        [Test] public void ServeChoice_WritesChosenKindAndHintSeen_ForTheOpenMatch()
        {
            metrics.BeginMatch("Easy","left");string match=Only("match-start").matchId;
            metrics.ServeChoice(true,"Drive",true);
            var e=Only("serve-choice");Assert.That(e.matchId,Is.EqualTo(match));Assert.That(e.chosen,Is.True);Assert.That(e.kind,Is.EqualTo("Drive"));Assert.That(e.hintSeen,Is.True);
        }
        [Test] public void ServeChoice_WithoutATouch_IsAnAutomaticLob()
        {
            metrics.BeginMatch("Easy","left");metrics.ServeChoice(false,"Lob",false);
            var e=Only("serve-choice");Assert.That(e.chosen,Is.False);Assert.That(e.kind,Is.EqualTo("Lob"));Assert.That(e.hintSeen,Is.False);
        }
        [Test] public void CurrentLines_StayTheSame()
        {
            metrics.BeginMatch("Medium","right");metrics.UpdateMatchScore(3,2);metrics.EndMatch(false);
            var start=rows.First(r=>r.kind=="match-start");var end=rows.First(r=>r.kind=="match-end");
            Assert.That(rows.Select(r=>r.kind),Is.EqualTo(new[]{"match-start","match-end"}),"the hint lines are written only when asked");
            Assert.That(start.payload,Does.Contain("\"result\":\"pending\""));Assert.That(end.payload,Does.Contain("\"result\":\"win\""));Assert.That(end.payload,Does.Contain("\"humanPoints\":3"));
        }
    }
}
