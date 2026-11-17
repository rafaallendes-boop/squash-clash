using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    public sealed class ZUserMetricsFlowTests : ZFirstRunFixture
    {
        [Serializable] private sealed class Line {public string kind,installId,payload;}
        [Serializable] private sealed class Payload
        {
            public string action,screen,reason,matchId,difficulty,hand,result;
            public int step,humanPoints,rivalPoints;public bool completed,skipped,abandoned;public double durationSeconds;
        }
        private bool originalHand;
        [UnitySetUp] public IEnumerator Setup(){originalHand=Handedness.Left;yield return Boot(true);}
        [UnityTearDown] public IEnumerator Cleanup(){Handedness.Left=originalHand;yield return Shutdown();}
        private Line[] Lines()=>File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Line>).ToArray();
        private Payload[] Events(string kind)=>Lines().Where(r=>r.kind==kind).Select(r=>JsonUtility.FromJson<Payload>(r.payload)).ToArray();
        private Payload Last(string kind){var rows=Events(kind);Assert.That(rows.Length,Is.GreaterThan(0),"missing "+kind);return rows.Last();}
        [UnityTest] public IEnumerator MenuTutorialMatchEnd_RecordsTheRealFlowAndOneMatchResult()
        {
            Assert.That(Events("app-session").Count(r=>r.action=="start"),Is.EqualTo(1));
            Click("menu-level-hard");Handedness.Left=true;rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");Click("tutorial-next");Click("tutorial-next");Click("tutorial-next");Refresh();yield return null;
            var steps=Events("tutorial-step");Assert.That(steps.Select(r=>r.step),Is.EqualTo(new[]{1,2,3}));Assert.That(steps.All(r=>r.completed&&!r.skipped&&r.durationSeconds>=0),Is.True);
            var start=Last("match-start");Assert.That(start.difficulty,Is.EqualTo("Hard"));Assert.That(start.hand,Is.EqualTo("left"));
            Handedness.Left=false;
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);Refresh();
            var end=Last("match-end");Assert.That(Events("match-end"),Has.Length.EqualTo(1));Assert.That(end.matchId,Is.EqualTo(start.matchId));Assert.That(end.hand,Is.EqualTo("left"));
            Assert.That(end.humanPoints,Is.EqualTo(rally.HumanPoints));Assert.That(end.rivalPoints,Is.EqualTo(rally.RivalPoints));Assert.That(end.abandoned,Is.False);
            Assert.That(end.result,Is.EqualTo(rally.Match.Winner==PlayerId.Human?"win":"loss"));Assert.That(end.durationSeconds,Is.GreaterThan(0));
            Assert.That(Events("menu").Any(r=>r.screen=="menu"&&r.action=="menu-play"),Is.True);
            var lines=Lines();Assert.That(lines.All(r=>Guid.TryParse(r.installId,out _)),Is.True);Assert.That(lines.Select(r=>r.installId).Distinct().Count(),Is.EqualTo(1));
            Assert.That(lines.Count(r=>r.kind=="rally-point"),Is.EqualTo(rally.PointsPlayed));
            Click("match-result-menu");Refresh();Assert.That(Events("match-end"),Has.Length.EqualTo(1));
        }
        [UnityTest] public IEnumerator TutorialSkipAndMenuExit_RecordSkippedStepsAndAbandonment()
        {
            Click("menu-play");Click("tutorial-next");Click("tutorial-skip");Refresh();yield return null;
            var steps=Events("tutorial-step");Assert.That(steps,Has.Length.EqualTo(3));Assert.That(steps[0].completed,Is.True);Assert.That(steps.Skip(1).All(r=>r.skipped&&!r.completed),Is.True);
            Click("rally-pause");Refresh();Assert.That(Last("pause").reason,Is.EqualTo("player"));
            Click("pause-menu");Refresh();var end=Last("match-end");Assert.That(end.abandoned,Is.True);Assert.That(end.result,Is.EqualTo("abandoned"));
            Assert.That(Events("match-end"),Has.Length.EqualTo(1));
        }
        [UnityTest] public IEnumerator Interruption_RecordsOnePauseAndDeduplicatesAppCallbacks()
        {
            Click("menu-play");Click("tutorial-skip");Refresh();yield return null;
            app.SendMessage("OnApplicationFocus",false);app.SendMessage("OnApplicationPause",true);
            app.SendMessage("OnApplicationFocus",true);app.SendMessage("OnApplicationPause",false);Refresh();
            Assert.That(Events("pause"),Has.Length.EqualTo(1));Assert.That(Last("pause").reason,Is.EqualTo("interruption"));
            Assert.That(Events("app-session").Count(r=>r.action=="background"),Is.EqualTo(1));Assert.That(Events("app-session").Count(r=>r.action=="foreground"),Is.EqualTo(1));
            Assert.That(app.Clock.Paused,Is.True,"metrics must not resume the match");
        }
    }
}
