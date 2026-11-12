using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class MatchScoreTests
    {
        private static MatchScore Play(string winners,PlayerId first=PlayerId.Human)
        {var score=new MatchScore(7,2,first);foreach(char c in winners)score.PointWon(c=='H'?PlayerId.Human:PlayerId.Rival);return score;}
        [Test] public void FirstToSeven_WithTwoClear_Wins()
        {
            var score=Play("HHHHHHH");
            Assert.That(score.Finished,Is.True);Assert.That(score.Winner,Is.EqualTo(PlayerId.Human));Assert.That(score.Human,Is.EqualTo(7));Assert.That(score.Rival,Is.EqualTo(0));
            Assert.That(Play("HHHHHH").Finished,Is.False);Assert.That(Play("HHHHHRRRRRHH").Finished,Is.True,"7-5");
        }
        [Test] public void FromSixAll_TheGameNeedsADifferenceOfTwo_WithNoCap()
        {
            const string sixAll="HRHRHRHRHRHR";
            Assert.That(Play(sixAll+"H").Finished,Is.False,"7-6");Assert.That(Play(sixAll+"HR").Finished,Is.False,"7-7");
            var long_=Play(sixAll+"HRHRHRRR");Assert.That(long_.Finished,Is.True);Assert.That(long_.Winner,Is.EqualTo(PlayerId.Rival));
            Assert.That(long_.Rival,Is.EqualTo(11));Assert.That(long_.Human,Is.EqualTo(9));
        }
        [Test] public void PointsAfterTheEnd_AreIgnored()
        {
            var score=Play("HHHHHHH");
            Assert.That(score.PointWon(PlayerId.Rival),Is.False);Assert.That(score.Rival,Is.EqualTo(0));Assert.That(score.PointsPlayed,Is.EqualTo(7));
        }
        [Test] public void TheWinnerServes_AlternatingBoxes_AndANewServerStartsFromTheRight()
        {
            var score=new MatchScore(7,2,PlayerId.Human);
            Assert.That(score.Server,Is.EqualTo(PlayerId.Human));Assert.That(score.Box,Is.EqualTo(ServiceBox.Right));
            score.PointWon(PlayerId.Human);Assert.That(score.Server,Is.EqualTo(PlayerId.Human));Assert.That(score.Box,Is.EqualTo(ServiceBox.Left));
            score.PointWon(PlayerId.Human);Assert.That(score.Box,Is.EqualTo(ServiceBox.Right));
            score.PointWon(PlayerId.Rival);Assert.That(score.Server,Is.EqualTo(PlayerId.Rival));Assert.That(score.Box,Is.EqualTo(ServiceBox.Right));
            score.PointWon(PlayerId.Rival);Assert.That(score.Server,Is.EqualTo(PlayerId.Rival));Assert.That(score.Box,Is.EqualTo(ServiceBox.Left));
            score.PointWon(PlayerId.Human);Assert.That(score.Server,Is.EqualTo(PlayerId.Human));Assert.That(score.Box,Is.EqualTo(ServiceBox.Right));
        }
        [Test] public void RallyCount_MatchesTheSpecArithmetic()
        {
            Assert.That(Play("HHHHHHH").PointsPlayed,Is.EqualTo(7));Assert.That(Play("HHHHHRRRRRHH").PointsPlayed,Is.EqualTo(12));
            Assert.That(Play("HRHRHRHRHRHRHH").PointsPlayed,Is.EqualTo(14));
        }
    }
    public sealed class ServeRulesTests
    {
        private const float R=.02f,Mm=.001f;
        private static readonly CourtDimensions Court=CourtDimensions.Standard();
        private static readonly float ShortFar=Court.Length-Court.ShortLineNearBackEdgeFromBack,Mid=Court.Width*.5f;
        private static PointRules Serve(ServiceBox box=ServiceBox.Right,PlayerId server=PlayerId.Rival)
        {var rules=new PointRules(Court,R);rules.BeginServe(server,box);return rules;}
        private static ContactEvent Front(float y=2.5f)=>PointRulesTests.Hit(SurfaceId.FrontWall,3.2f,y);
        private static ContactEvent Floor(float x,float z)=>PointRulesTests.Hit(SurfaceId.Floor,x,z);
        private static void Fault(PointRules rules,PlayerId server=PlayerId.Rival)
        {Assert.That(rules.Closed,Is.True);Assert.That(rules.Cause,Is.EqualTo(PointEndCause.ServiceFault));Assert.That(rules.Winner,Is.EqualTo(server.Other()));}
        [Test] public void GoodServe_BecomesANormalRally()
        {
            var rules=Serve();Assert.That(rules.Serving,Is.True);Assert.That(rules.ToHit,Is.EqualTo(PlayerId.Human));
            rules.OnBallContact(Front());rules.OnBallContact(PointRulesTests.Hit(SurfaceId.LeftWall,7,1.5f));rules.OnBallContact(Floor(Mid+1.5f,8));
            Assert.That(rules.Closed,Is.False);Assert.That(rules.Serving,Is.False);Assert.That(rules.FloorBounces,Is.EqualTo(1));Assert.That(rules.CanHit(PlayerId.Human),Is.True);
            Assert.That(rules.RacketContact(PlayerId.Human),Is.True);
            rules.OnBallContact(Front(1));rules.OnBallContact(Floor(1,3));rules.OnBallContact(Floor(1,4));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.DoubleBounce));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Human),"the service line is not judged in the return");
        }
        [Test] public void UnreturnedGoodServe_WinsThePointForTheServer()
        {
            var rules=Serve();rules.OnBallContact(Front());rules.OnBallContact(Floor(Mid+1.5f,8));rules.OnBallContact(Floor(Mid+1.6f,9));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.DoubleBounce));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Rival));
        }
        [Test] public void SideWallBeforeTheFrontWall_IsAFault(){var rules=Serve();rules.OnBallContact(PointRulesTests.Hit(SurfaceId.LeftWall,2,2.5f));Fault(rules);}
        [Test] public void FloorBeforeTheFrontWall_IsAFault(){var rules=Serve();rules.OnBallContact(Floor(3,3));Fault(rules);}
        [Test] public void ServiceLine_Edges()
        {
            float top=Court.ServiceLower+Court.LineWidth;
            var above=Serve();above.OnBallContact(Front(top+R+Mm));Assert.That(above.Closed,Is.False);Assert.That(above.FrontWallReached,Is.True);
            var grazing=Serve();grazing.OnBallContact(Front(top+R-Mm));Fault(grazing);
            var onEdge=Serve();onEdge.OnBallContact(Front(top+R));Fault(onEdge);
            var below=Serve();below.OnBallContact(Front(1.2f));Fault(below);
        }
        [Test] public void TinAndOut_AreFaults()
        {
            var tin=Serve();tin.OnBallContact(Front(.3f));Fault(tin);
            var outFront=Serve();outFront.OnBallContact(Front(4.7f));Fault(outFront);
            var outSide=Serve();outSide.OnBallContact(Front());outSide.OnBallContact(PointRulesTests.Hit(SurfaceId.LeftWall,7,4.2f));Fault(outSide);
            var outBack=Serve();outBack.OnBallContact(Front());outBack.OnBallContact(PointRulesTests.Hit(SurfaceId.BackWall,5,2.4f));Fault(outBack);
            var over=Serve();over.OnBallContact(Front());over.OnOverTop();Fault(over);
        }
        [Test] public void Landing_MustBeInTheOppositeBackQuarter()
        {
            var sameSide=Serve(ServiceBox.Right);sameSide.OnBallContact(Front());sameSide.OnBallContact(Floor(Mid-1.5f,8));Fault(sameSide);
            var shortBall=Serve(ServiceBox.Right);shortBall.OnBallContact(Front());shortBall.OnBallContact(Floor(Mid+1.5f,4));Fault(shortBall);
            var fromLeft=Serve(ServiceBox.Left,PlayerId.Human);fromLeft.OnBallContact(Front());fromLeft.OnBallContact(Floor(Mid-1.5f,8));Assert.That(fromLeft.Closed,Is.False);
            var fromLeftWrong=Serve(ServiceBox.Left,PlayerId.Human);fromLeftWrong.OnBallContact(Front());fromLeftWrong.OnBallContact(Floor(Mid+1.5f,8));Fault(fromLeftWrong,PlayerId.Human);
        }
        [Test] public void FloorLines_Edges()
        {
            float halfEdge=Mid+Court.LineWidth*.5f;
            var clearShort=Serve();clearShort.OnBallContact(Front());clearShort.OnBallContact(Floor(Mid+1.5f,ShortFar+R+Mm));Assert.That(clearShort.Closed,Is.False);
            var grazeShort=Serve();grazeShort.OnBallContact(Front());grazeShort.OnBallContact(Floor(Mid+1.5f,ShortFar+R-Mm));Fault(grazeShort);
            var onShort=Serve();onShort.OnBallContact(Front());onShort.OnBallContact(Floor(Mid+1.5f,ShortFar+R));Fault(onShort);
            var clearHalf=Serve();clearHalf.OnBallContact(Front());clearHalf.OnBallContact(Floor(halfEdge+R+Mm,8));Assert.That(clearHalf.Closed,Is.False);
            var grazeHalf=Serve();grazeHalf.OnBallContact(Front());grazeHalf.OnBallContact(Floor(halfEdge+R-Mm,8));Fault(grazeHalf);
            var onHalf=Serve();onHalf.OnBallContact(Front());onHalf.OnBallContact(Floor(halfEdge+R,8));Fault(onHalf);
        }
        [Test] public void FloorLines_AreNotJudgedInTheRally()
        {
            var rules=new PointRules(Court,R);rules.Begin(PlayerId.Rival,PlayerId.Human);
            rules.OnBallContact(Front(1));rules.OnBallContact(Floor(Mid,ShortFar-.02f));
            Assert.That(rules.Closed,Is.False);Assert.That(rules.FloorBounces,Is.EqualTo(1));
        }
        [Test] public void ReceiverCannotHit_BeforeTheServeLands_WithoutVolley()
        {
            var rules=Serve();rules.OnBallContact(Front());
            Assert.That(rules.CanHit(PlayerId.Human),Is.False);Assert.That(rules.CanHit(PlayerId.Rival),Is.False);
        }
        [Test] public void Clone_KeepsTheServeState()
        {
            var rules=Serve();rules.OnBallContact(Front());var copy=rules.Clone();
            Assert.That(copy.Serving,Is.True);copy.OnBallContact(Floor(Mid-1.5f,8));Fault(copy);Assert.That(rules.Closed,Is.False);
        }
    }
    public sealed class ServePlayTests
    {
        [TestCase(ServiceBox.Right)]
        [TestCase(ServiceBox.Left)]
        public void AutomaticServe_IsValid_AndLandsInTheReceiversQuarter(ServiceBox box)
        {
            var context=RallyFixtures.Context();var serve=context.Serve(box);float mid=context.Court.Width*.5f;
            Assert.That(ServePlanner.Forecast(context,serve,box,out var bounce),Is.True);
            Assert.That(box==ServiceBox.Right?bounce.x>mid:bounce.x<mid,Is.True);Assert.That(bounce.z,Is.GreaterThan(context.T.y+.5f));
            var at=ServePlanner.BoxPosition(context,box);
            Assert.That(box==ServiceBox.Right?at.x<context.Court.ServiceBoxInner:at.x>context.Court.Width-context.Court.ServiceBoxInner,Is.True);
            Assert.That(at.y,Is.GreaterThan(context.T.y));Assert.That(serve.Position.x,Is.EqualTo(at.x));Assert.That(serve.Position.z,Is.EqualTo(at.y));
            Assert.That(context.Serve(box).Velocity,Is.EqualTo(serve.Velocity),"same box, same serve");
        }
        [Test] public void ServedPoint_IsPlayedToATerminalEvent()
        {
            var context=RallyFixtures.Context();
            using(var sim=new RallySim(context,RallySimTests.Bot(context,3),RallySimTests.Fondo(context,4)))
            {
                var outcome=sim.PlayPoint(PlayerId.Human,ServiceBox.Right);
                Assert.That(outcome.End,Is.EqualTo(RallyEnd.Point));Assert.That(outcome.Winner,Is.Not.Null);Assert.That(outcome.Server,Is.EqualTo(PlayerId.Human));
                Assert.That(outcome.Cause,Is.Not.EqualTo(PointEndCause.ServiceFault),"the automatic serve is valid");
                Assert.That(outcome.Receptions[0].Who,Is.EqualTo(PlayerId.Rival),"the receiver takes the first reception");
            }
        }
        [Test,Timeout(600000)] public void Matches_FinishWithAValidScore()
        {
            const int Matches=60;var context=RallyFixtures.Context();var outcomes=new List<MatchOutcome>();
            using(var sim=new RallySim(context,RallySimTests.Bot(context,77),RallySimTests.Fondo(context,78)))
                for(int i=0;i<Matches;i++)outcomes.Add(sim.PlayMatch(i%2==0?PlayerId.Human:PlayerId.Rival));
            foreach(var match in outcomes)
            {
                Assert.That(match.Aborted,Is.False);Assert.That(match.Winner,Is.Not.Null);
                int high=Mathf.Max(match.Human,match.Rival),low=Mathf.Min(match.Human,match.Rival);
                Assert.That(high,Is.GreaterThanOrEqualTo(7));Assert.That(high-low,Is.GreaterThanOrEqualTo(2));
                Assert.That(high==7||high-low==2,Is.True,"ends as soon as the margin is reached");
                Assert.That(match.Points.Count-match.Replays,Is.EqualTo(match.Human+match.Rival));
                Assert.That(match.Points.All(p=>p.End==RallyEnd.Point||p.End==RallyEnd.ShotCap),Is.True);
                Assert.That(match.Points.Count(p=>p.Cause==PointEndCause.ServiceFault),Is.EqualTo(0));
            }
            var points=outcomes.Select(m=>(double)(m.Human+m.Rival)).OrderBy(v=>v).ToList();var minutes=outcomes.Select(m=>m.RealSeconds/60).OrderBy(v=>v).ToList();
            TestContext.WriteLine("MATCH_SIM matches="+Matches+" humanWins="+outcomes.Count(m=>m.Winner==PlayerId.Human)+" pointsMedian="+points[points.Count/2]+" pointsMax="+points.Last()
                +" minutesMedian="+minutes[minutes.Count/2].ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+" minutesP90="+minutes[(int)(minutes.Count*.9)].ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)
                +" replays="+outcomes.Sum(m=>m.Replays));
        }
    }
}
