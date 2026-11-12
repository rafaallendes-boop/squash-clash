using NUnit.Framework;
using SquashClash.Core.Rally;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class RallyTextTests
    {
        [Test] public void Live_ShowsScoreStrokesAndTurn_InOneLine()
        {
            Assert.That(RallyText.Live(2,1,5,PlayerId.Rival),Is.EqualTo("Tú 2 – 1 Rival · golpes 5 · turno: Rival"));
            Assert.That(RallyText.Live(0,0,0,PlayerId.Human),Is.EqualTo("Tú 0 – 0 Rival · golpes 0 · turno: Tú"));
            Assert.That(RallyText.Live(2,1,5,PlayerId.Rival),Does.Not.Contain("\n"));
        }
        [TestCase(PointEndCause.DoubleBounce,ReceptionResult.Air,"doble bote (al aire)")]
        [TestCase(PointEndCause.DoubleBounce,ReceptionResult.Late,"doble bote (tarde)")]
        [TestCase(PointEndCause.DoubleBounce,ReceptionResult.Unreachable,"doble bote (no llega)")]
        [TestCase(PointEndCause.DoubleBounce,ReceptionResult.NoSwing,"doble bote (sin golpe)")]
        [TestCase(PointEndCause.DoubleBounce,ReceptionResult.SystemFault,"doble bote (falla del sistema)")]
        [TestCase(PointEndCause.Tin,null,"tin")]
        [TestCase(PointEndCause.Out,null,"fuera")]
        [TestCase(PointEndCause.NotUp,null,"no llegó al frontis")]
        [TestCase(PointEndCause.LaunchFault,null,"lanzamiento nulo")]
        public void Cause_NamesEveryTerminalEvent(PointEndCause cause,ReceptionResult? reception,string expected)
            =>Assert.That(RallyText.Cause(cause,reception),Is.EqualTo(expected));
        [Test] public void Match_ShowsServerAndBox_AndTheFinalResult()
        {
            Assert.That(RallyText.LiveMatch(3,2,4,PlayerId.Human,PlayerId.Rival,ServiceBox.Left),Is.EqualTo("Tú 3 – 2 Rival · saca Rival (izq.) · golpes 4 · turno: Tú"));
            Assert.That(RallyText.LiveMatch(0,0,0,PlayerId.Rival,PlayerId.Human,ServiceBox.Right),Is.EqualTo("Tú 0 – 0 Rival · saca Tú (der.) · golpes 0 · turno: Rival"));
            Assert.That(RallyText.MatchOver(PlayerId.Rival,5,7),Is.EqualTo("Partido para Rival: Tú 5 – 7 Rival · Revancha para jugar otro"));
            Assert.That(RallyText.Cause(PointEndCause.ServiceFault,null),Is.EqualTo("saque fallado"));
        }
        [Test] public void Point_ShowsWinnerCauseStrokesAndScore_InOneLine()
        {
            Assert.That(RallyText.Point(PlayerId.Rival,PointEndCause.DoubleBounce,ReceptionResult.Late,5,2,2),Is.EqualTo("Punto Rival: doble bote (tarde) · 5 golpes · Tú 2 – 2 Rival"));
            Assert.That(RallyText.Point(PlayerId.Human,PointEndCause.Tin,null,1,1,0),Is.EqualTo("Punto Tú: tin · 1 golpe · Tú 1 – 0 Rival"));
            Assert.That(RallyText.Point(null,PointEndCause.LaunchFault,null,0,1,0),Is.EqualTo("Sin punto: lanzamiento nulo · 0 golpes · Tú 1 – 0 Rival"));
            Assert.That(RallyText.Point(null,PointEndCause.None,null,200,1,0),Is.EqualTo("Sin punto: tope de golpes · 200 golpes · Tú 1 – 0 Rival"));
        }
    }
}
