using NUnit.Framework;
using SquashClash.Core.Rally;
namespace SquashClash.Core.Tests.Rally
{
    // Simple referee (spec 2026-11-26): server-first score, match ball from the match's own target and margin, and the full call text.
    public sealed class RefereeCallTests
    {
        private static MatchScore Play(string winners,int target=7,int margin=2,PlayerId first=PlayerId.Human)
        {var score=new MatchScore(target,margin,first);foreach(char c in winners)score.PointWon(c=='H'?PlayerId.Human:PlayerId.Rival);return score;}
        // Scores human-rival with the last point won by lastWinner (so that player serves next), the other player's points first.
        private static MatchScore Make(int human,int rival,PlayerId lastWinner,int target=7,int margin=2)
        {
            var s=new MatchScore(target,margin,PlayerId.Human);
            int hFirst=lastWinner==PlayerId.Human?0:human,rFirst=lastWinner==PlayerId.Rival?0:rival;
            for(int i=0;i<hFirst;i++)s.PointWon(PlayerId.Human);for(int i=0;i<rFirst;i++)s.PointWon(PlayerId.Rival);
            for(int i=hFirst;i<human;i++)s.PointWon(PlayerId.Human);for(int i=rFirst;i<rival;i++)s.PointWon(PlayerId.Rival);
            return s;
        }
        [Test] public void Score_IsServerFirst()
        {
            Assert.That(RefereeCall.Score(3,1),Is.EqualTo("3–1"));
            var humanServes=Make(3,1,PlayerId.Human);Assert.That(humanServes.Server,Is.EqualTo(PlayerId.Human));Assert.That(RefereeCall.Score(humanServes),Is.EqualTo("3–1"));
            var rivalServes=Make(1,3,PlayerId.Rival);Assert.That(rivalServes.Server,Is.EqualTo(PlayerId.Rival));Assert.That(RefereeCall.Score(rivalServes),Is.EqualTo("3–1"));
            var humanServesBehind=Make(1,3,PlayerId.Human);Assert.That(humanServesBehind.Server,Is.EqualTo(PlayerId.Human));Assert.That(RefereeCall.Score(humanServesBehind),Is.EqualTo("1–3"));
        }
        [Test] public void MatchBall_SevenByTwo_WhenOnePointCloses([Values(PlayerId.Human,PlayerId.Rival)] PlayerId first)
        {
            for(int other=0;other<=5;other++)
            {
                var a=new MatchScore(7,2,first);var b=new MatchScore(7,2,first);
                for(int i=0;i<6;i++)a.PointWon(PlayerId.Human);for(int i=0;i<other;i++)a.PointWon(PlayerId.Rival);
                for(int i=0;i<6;i++)b.PointWon(PlayerId.Rival);for(int i=0;i<other;i++)b.PointWon(PlayerId.Human);
                Assert.That(a.MatchBall,Is.True,"6-"+other);Assert.That(b.MatchBall,Is.True,other+"-6");
            }
        }
        [Test] public void MatchBall_False_WhenNoOnePointClosesIt()
        {
            Assert.That(Play("HRHRHRHRHRHR").MatchBall,Is.False,"6-6");
            Assert.That(Play("HRHRHRHRHRHRH").MatchBall,Is.True,"7-6");
            Assert.That(Play("HRHRHRHRHRHRR").MatchBall,Is.True,"6-7");
            Assert.That(Play("HRHRHRHRHRHRHR").MatchBall,Is.False,"7-7");
            Assert.That(Play("HRHRHRHRHRHRHRH").MatchBall,Is.True,"8-7");
            Assert.That(Play("HRHRHRHRHR").MatchBall,Is.False,"5-5");
            Assert.That(Play("HHHHHRRRRRHH").Finished,Is.True);Assert.That(Play("HHHHHRRRRRHH").MatchBall,Is.False,"finished 7-5");
        }
        [Test] public void MatchBall_UsesTheMatchTargetAndMargin_NotFixedNumbers()
        {
            var tenEight=new MatchScore(11,2,PlayerId.Human);for(int i=0;i<10;i++)tenEight.PointWon(PlayerId.Human);for(int i=0;i<8;i++)tenEight.PointWon(PlayerId.Rival);
            Assert.That(tenEight.Finished,Is.False);Assert.That(tenEight.MatchBall,Is.True,"10-8 at 11 by 2");
            var sixFour=new MatchScore(11,2,PlayerId.Human);for(int i=0;i<6;i++)sixFour.PointWon(PlayerId.Human);for(int i=0;i<4;i++)sixFour.PointWon(PlayerId.Rival);
            Assert.That(sixFour.MatchBall,Is.False,"6-4 at 11 by 2");
            var sixAll=new MatchScore(7,1,PlayerId.Human);for(int i=0;i<6;i++){sixAll.PointWon(PlayerId.Human);sixAll.PointWon(PlayerId.Rival);}
            Assert.That(sixAll.MatchBall,Is.True,"6-6 at 7 by 1");
            Assert.That(sixAll.Target,Is.EqualTo(7));Assert.That(sixAll.Margin,Is.EqualTo(1));
        }
        [Test] public void Call_BuildsTheFourTexts()
        {
            var normal=Make(3,1,PlayerId.Human);Assert.That(RefereeCall.Call(normal,false),Is.EqualTo("3–1"));
            var tin=Make(4,2,PlayerId.Rival);Assert.That(tin.Server,Is.EqualTo(PlayerId.Rival));Assert.That(RefereeCall.Call(tin,true),Is.EqualTo("¡Tin! · 2–4"));
            var ball=Make(6,4,PlayerId.Human);Assert.That(ball.MatchBall,Is.True);Assert.That(RefereeCall.Call(ball,false),Is.EqualTo("Match ball · 6–4"));
            var both=Make(6,5,PlayerId.Human);Assert.That(RefereeCall.Call(both,true),Is.EqualTo("¡Tin! · Match ball · 6–5"));
            Assert.That(RefereeCall.Tin,Is.EqualTo("¡Tin!"));Assert.That(RefereeCall.MatchBall,Is.EqualTo("Match ball"));
        }
    }
}
