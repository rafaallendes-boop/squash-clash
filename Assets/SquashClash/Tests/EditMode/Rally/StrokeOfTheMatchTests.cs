using NUnit.Framework;
using SquashClash.Core.Rally;
namespace SquashClash.Core.Tests.Rally
{
    // Stroke of the match (spec 2026-11-25): the one line of the result card, chosen by a fixed priority at 3 or more.
    public sealed class StrokeOfTheMatchTests
    {
        [Test] public void ExactlyThreeOfOneKind_GivesThatLine()
        {
            Assert.That(StrokeOfTheMatch.Line(3,0,0),Is.EqualTo("Golpe del partido: 3 Perfectos"));
            Assert.That(StrokeOfTheMatch.Line(0,3,0),Is.EqualTo("Golpe del partido: 3 dos paredes ganadores"));
            Assert.That(StrokeOfTheMatch.Line(0,0,3),Is.EqualTo("Golpe del partido: 3 voleas"));
        }
        [Test] public void Two_GivesNothing_AloneOrAllAtOnce()
        {
            Assert.That(StrokeOfTheMatch.Line(2,0,0),Is.Empty);Assert.That(StrokeOfTheMatch.Line(0,2,0),Is.Empty);
            Assert.That(StrokeOfTheMatch.Line(0,0,2),Is.Empty);Assert.That(StrokeOfTheMatch.Line(2,2,2),Is.Empty);
        }
        [Test] public void NoStrokes_GivesNoLine()
        {
            Assert.That(StrokeOfTheMatch.Line(0,0,0),Is.Empty);Assert.That(new StrokeOfTheMatch().Line(),Is.Empty);
        }
        [Test] public void PriorityDecidesTies_AndALowerKindNeverBeatsAHigherOneAtThree()
        {
            Assert.That(StrokeOfTheMatch.Line(3,3,3),Is.EqualTo("Golpe del partido: 3 Perfectos"));
            Assert.That(StrokeOfTheMatch.Line(0,3,3),Is.EqualTo("Golpe del partido: 3 dos paredes ganadores"));
            Assert.That(StrokeOfTheMatch.Line(2,5,5),Is.EqualTo("Golpe del partido: 5 dos paredes ganadores"));
            Assert.That(StrokeOfTheMatch.Line(3,9,9),Is.EqualTo("Golpe del partido: 3 Perfectos"));
            Assert.That(StrokeOfTheMatch.Line(2,2,7),Is.EqualTo("Golpe del partido: 7 voleas"));
        }
        [Test] public void TextsAreExact_WithTwoDigitNumbers()
        {
            Assert.That(StrokeOfTheMatch.Line(12,0,0),Is.EqualTo("Golpe del partido: 12 Perfectos"));
            Assert.That(StrokeOfTheMatch.Line(0,99,0),Is.EqualTo("Golpe del partido: 99 dos paredes ganadores"));
            Assert.That(StrokeOfTheMatch.Line(0,0,10),Is.EqualTo("Golpe del partido: 10 voleas"));
        }
        [Test] public void ABoastThenAWonPoint_CountsOneBoastWinner()
        {
            var s=new StrokeOfTheMatch();s.OnStroke(true,false,false,true);s.OnPointEnded(true);
            Assert.That(s.BoastWinners,Is.EqualTo(1));Assert.That(s.Perfects,Is.EqualTo(0));Assert.That(s.Volleys,Is.EqualTo(0));
        }
        [Test] public void ABoastThenALostOrWinnerlessPoint_CountsNothing()
        {
            var s=new StrokeOfTheMatch();s.OnStroke(true,false,false,true);s.OnPointEnded(false);Assert.That(s.BoastWinners,Is.EqualTo(0));
            s.OnStroke(true,false,false,true);s.OnPointEnded(false);s.OnPointEnded(true);Assert.That(s.BoastWinners,Is.EqualTo(0),"the pending boast was cleared by the first point end");
        }
        [Test] public void ABoastThenAFrontWallStrokeThenAWonPoint_CountsNothing()
        {
            var s=new StrokeOfTheMatch();s.OnStroke(true,false,false,true);s.OnStroke(true,false,false,false);s.OnPointEnded(true);
            Assert.That(s.BoastWinners,Is.EqualTo(0));
        }
        [Test] public void AStrokeThatWasNotApplied_CountsNothing_AndClearsTheBoast()
        {
            var s=new StrokeOfTheMatch();s.OnStroke(false,true,true,true);s.OnPointEnded(true);
            Assert.That(s.Perfects+s.Volleys+s.BoastWinners,Is.EqualTo(0));
            s.OnStroke(true,false,false,true);s.OnStroke(false,false,false,false);s.OnPointEnded(true);Assert.That(s.BoastWinners,Is.EqualTo(0));
        }
        [Test] public void APerfectVolley_CountsAsBoth()
        {
            var s=new StrokeOfTheMatch();s.OnStroke(true,true,true,false);
            Assert.That(s.Perfects,Is.EqualTo(1));Assert.That(s.Volleys,Is.EqualTo(1));
        }
        [Test] public void APerfectBoastThatWinsThePoint_CountsAsPerfectAndBoastWinner()
        {
            var s=new StrokeOfTheMatch();s.OnStroke(true,true,false,true);s.OnPointEnded(true);
            Assert.That(s.Perfects,Is.EqualTo(1));Assert.That(s.BoastWinners,Is.EqualTo(1));
        }
        [Test] public void Reset_LeavesEverythingAtZero_PendingBoastIncluded()
        {
            var s=new StrokeOfTheMatch();s.OnStroke(true,true,true,true);s.OnPointEnded(true);s.OnStroke(true,false,false,true);
            s.Reset();s.OnPointEnded(true);
            Assert.That(s.Perfects,Is.EqualTo(0));Assert.That(s.BoastWinners,Is.EqualTo(0));Assert.That(s.Volleys,Is.EqualTo(0));
        }
        [Test] public void TheInstanceLine_ReadsItsOwnCounts()
        {
            var s=new StrokeOfTheMatch();for(int i=0;i<3;i++)s.OnStroke(true,true,false,false);
            Assert.That(s.Line(),Is.EqualTo("Golpe del partido: 3 Perfectos"));
        }
    }
}
