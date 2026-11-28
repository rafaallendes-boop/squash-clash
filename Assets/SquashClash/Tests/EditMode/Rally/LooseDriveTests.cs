using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // Loose drive (spec 2026-11-28, criteria 1 and 2). The court lines come from CourtDimensions, not from handwritten cuts.
    public sealed class LooseDriveTests
    {
        private static CourtDimensions Court => CourtDimensions.Standard();
        private static float Lob => (float)new RallyParams().ServeKindLobMinAimY;
        private static Vector3 At(float x, float z) => new Vector3(x, 0f, z);

        [Test]
        public void Classify_UsesTheCourtLines_NotHandwrittenCuts()
        {
            var court = Court;
            Assert.That(LooseDrive.Classify(At(3.2f, 5.40f), court), Is.EqualTo(LooseDriveKind.ShortAndWide));
            Assert.That(LooseDrive.Classify(At(0.8f, 5.40f), court), Is.EqualTo(LooseDriveKind.Short));
            Assert.That(LooseDrive.Classify(At(3.2f, 8.0f), court), Is.EqualTo(LooseDriveKind.Wide));
            Assert.That(LooseDrive.Classify(At(0.8f, 8.0f), court), Is.EqualTo(LooseDriveKind.None));
            Assert.That(LooseDrive.Classify(At(5.6f, 8.0f), court), Is.EqualTo(LooseDriveKind.None));
        }

        [Test]
        public void Classify_ReadsTheShortLineAndTheBoxWidthFromTheCourt()
        {
            var moved = CourtDimensions.Standard();
            moved.ShortLineNearBackEdgeFromBack = moved.Length - 5.40f;
            Assert.That(LooseDrive.Classify(At(3.2f, 5.40f), moved), Is.EqualTo(LooseDriveKind.Wide), "z on the moved short line is not short");
            var wider = CourtDimensions.Standard();
            wider.ServiceBoxInner = 3.2f;
            Assert.That(LooseDrive.Classify(At(3.2f, 5.40f), wider), Is.EqualTo(LooseDriveKind.Short), "exactly the box width is not wide");
        }

        [Test]
        public void Classify_OnTheShortLine_IsNotShort_AndOnTheServiceBoxWidth_IsNotWide()
        {
            var court = Court;
            float line = court.Length - court.ShortLineNearBackEdgeFromBack;
            float box = court.ServiceBoxInner;
            Assert.That(LooseDrive.Classify(At(0.5f, line), court), Is.EqualTo(LooseDriveKind.None), "z on the short line is not short");
            Assert.That(LooseDrive.Classify(At(box, line - 0.09f), court), Is.EqualTo(LooseDriveKind.Short), "exactly the box width, and short");
            Assert.That(LooseDrive.Classify(At(court.Width - box, line - 0.09f), court), Is.EqualTo(LooseDriveKind.Short), "the other side wall");
            Assert.That(LooseDrive.Classify(At(box, line + 1f), court), Is.EqualTo(LooseDriveKind.None), "on the width and behind the line");
            Assert.That(LooseDrive.Classify(null, court), Is.EqualTo(LooseDriveKind.None), "no bounce");
            Assert.That(LooseDrive.Classify(At(3.2f, 5.40f), null), Is.EqualTo(LooseDriveKind.None));
        }

        [Test]
        public void IsDrive_PowerAndAimAreStrict_AndVolleySideAndMissAreNot()
        {
            float lob = Lob;
            Assert.That(LooseDrive.IsDrive(true, false, false, LooseDrive.MinPower, 1f, lob), Is.False, "P 0.5 is not a drive");
            Assert.That(LooseDrive.IsDrive(true, false, false, LooseDrive.MinPower + 0.01f, 1f, lob), Is.True, "P 0.51 is a drive");
            Assert.That(LooseDrive.IsDrive(true, false, false, 0.8f, lob, lob), Is.False, "AimY at the lob line is not a drive");
            Assert.That(LooseDrive.IsDrive(true, false, false, 0.8f, lob - 0.01f, lob), Is.True, "AimY just under the lob line is a drive");
            Assert.That(LooseDrive.IsDrive(true, true, false, 0.8f, 1f, lob), Is.False, "a volley");
            Assert.That(LooseDrive.IsDrive(true, false, true, 0.8f, 1f, lob), Is.False, "the order aimed at the side wall");
            Assert.That(LooseDrive.IsDrive(false, false, false, 0.8f, 1f, lob), Is.False, "not applied");
        }

        [Test]
        public void Text_IsTheThreeSentences_AndNoneIsEmpty()
        {
            Assert.That(LooseDrive.Text(LooseDriveKind.Short), Is.EqualTo("Tu drive botó corto: el rival lo aprovechó."));
            Assert.That(LooseDrive.Text(LooseDriveKind.Wide), Is.EqualTo("Tu drive quedó lejos de la pared lateral: el rival lo aprovechó."));
            Assert.That(LooseDrive.Text(LooseDriveKind.ShortAndWide), Is.EqualTo("Tu drive botó corto y lejos de la pared lateral: el rival lo aprovechó."));
            Assert.That(LooseDrive.Text(LooseDriveKind.None), Is.EqualTo(""));
        }

        private sealed class Facts
        {
            public bool Applied = true, Volley, SideAimed, LastApplied = true;
            public float Power = 0.8f, AimY = 1f;
            public int Rivals = 1;
            public PlayerId? Winner = PlayerId.Rival;
            public PointEndCause Cause = PointEndCause.DoubleBounce;
            public ReceptionResult? Reception = ReceptionResult.Unreachable;
            public Vector3? Bounce = new Vector3(3.2f, 0f, 5.40f);
            public bool HumanServePause, MatchOver, Autopilot, Force;
            public bool InMatch = true, Enabled = true;
            public LooseDriveKind Judge() => LooseDrive.Judge(
                Applied, Volley, SideAimed, Power, AimY, (float)new RallyParams().ServeKindLobMinAimY,
                LastApplied, Rivals, Winner, Cause, Reception, Bounce, CourtDimensions.Standard(),
                HumanServePause, MatchOver, InMatch, Autopilot, Enabled, Force);
        }

        [Test]
        public void ALooseDrive_TheRivalReturnedOnce_AndTheHumanCouldNotReach_IsItsKind()
        {
            Assert.That(new Facts().Judge(), Is.EqualTo(LooseDriveKind.ShortAndWide));
            Assert.That(new Facts { Bounce = new Vector3(0.8f, 0f, 5.40f) }.Judge(), Is.EqualTo(LooseDriveKind.Short));
            Assert.That(new Facts { Bounce = new Vector3(3.2f, 0f, 8f) }.Judge(), Is.EqualTo(LooseDriveKind.Wide));
        }

        [Test]
        public void TwoWallOrder_AndAFallbackToTheDrive_AreNone_BecauseTheOrderAimedAtTheSide()
        {
            Assert.That(new Facts { SideAimed = true }.Judge(), Is.EqualTo(LooseDriveKind.None), "a side aim is not a drive, including a boast that fell back");
        }

        [Test]
        public void AVolley_IsNone()
        {
            Assert.That(new Facts { Volley = true }.Judge(), Is.EqualTo(LooseDriveKind.None));
        }

        [Test]
        public void PowerAtOrBelowTheCut_IsNone()
        {
            Assert.That(new Facts { Power = LooseDrive.MinPower }.Judge(), Is.EqualTo(LooseDriveKind.None), "P 0.5");
            Assert.That(new Facts { Power = 0.12f }.Judge(), Is.EqualTo(LooseDriveKind.None), "a drop");
        }

        [Test]
        public void AimAtOrAboveTheLobLine_IsNone()
        {
            Assert.That(new Facts { AimY = Lob }.Judge(), Is.EqualTo(LooseDriveKind.None));
            Assert.That(new Facts { AimY = Lob + 1f }.Judge(), Is.EqualTo(LooseDriveKind.None));
        }

        [Test]
        public void NotApplied_IsNone()
        {
            Assert.That(new Facts { Applied = false }.Judge(), Is.EqualTo(LooseDriveKind.None));
        }

        [Test]
        public void AHumanServe_HasNoPendingDrive_AndIsNone()
        {
            Assert.That(new Facts { LastApplied = false, Applied = false }.Judge(), Is.EqualTo(LooseDriveKind.None));
        }

        [Test]
        public void TheHumanWins_OrThePointHasNoWinner_IsNone()
        {
            Assert.That(new Facts { Winner = PlayerId.Human }.Judge(), Is.EqualTo(LooseDriveKind.None), "the human won");
            Assert.That(new Facts { Winner = null, Cause = PointEndCause.LaunchFault }.Judge(), Is.EqualTo(LooseDriveKind.None), "launch fault");
            Assert.That(new Facts { Winner = null, Cause = PointEndCause.None }.Judge(), Is.EqualTo(LooseDriveKind.None), "stroke cap");
        }

        [TestCase(PointEndCause.Tin)]
        [TestCase(PointEndCause.Out)]
        [TestCase(PointEndCause.NotUp)]
        [TestCase(PointEndCause.ServiceFault)]
        public void TheHumanLosesByHisOwnFault_IsNone(PointEndCause cause)
        {
            Assert.That(new Facts { Cause = cause }.Judge(), Is.EqualTo(LooseDriveKind.None), cause.ToString());
        }

        [TestCase(ReceptionResult.Air)]
        [TestCase(ReceptionResult.Late)]
        [TestCase(ReceptionResult.NoSwing)]
        public void TheHumanMissedByTimingOrDidNotSwing_IsNone(ReceptionResult reception)
        {
            Assert.That(new Facts { Reception = reception }.Judge(), Is.EqualTo(LooseDriveKind.None), reception.ToString());
        }

        [Test]
        public void TheRivalDidNotReturnExactlyOnce_IsNone()
        {
            Assert.That(new Facts { Rivals = 0 }.Judge(), Is.EqualTo(LooseDriveKind.None), "no return");
            Assert.That(new Facts { Rivals = 2 }.Judge(), Is.EqualTo(LooseDriveKind.None), "the human hit again, or the rival returned twice");
        }

        [Test]
        public void ADriveThatIsNotTheLastAppliedHumanStroke_IsNone()
        {
            Assert.That(new Facts { LastApplied = false }.Judge(), Is.EqualTo(LooseDriveKind.None));
        }

        [Test]
        public void ABounceBehindTheShortLineAndInsideTheBoxWidth_IsNone()
        {
            Assert.That(new Facts { Bounce = new Vector3(0.8f, 0f, 8f) }.Judge(), Is.EqualTo(LooseDriveKind.None));
            Assert.That(new Facts { Bounce = new Vector3(5.6f, 0f, 8f) }.Judge(), Is.EqualTo(LooseDriveKind.None));
        }

        [Test]
        public void NoBounce_TheRivalVolleyed_IsNone()
        {
            Assert.That(new Facts { Bounce = null }.Judge(), Is.EqualTo(LooseDriveKind.None));
        }

        [Test]
        public void HumanServePause_MatchOver_PracticeAndRallyWithoutAMatch_AreNone()
        {
            Assert.That(new Facts { HumanServePause = true }.Judge(), Is.EqualTo(LooseDriveKind.None), "human serve pause");
            Assert.That(new Facts { MatchOver = true }.Judge(), Is.EqualTo(LooseDriveKind.None), "match over");
            Assert.That(new Facts { InMatch = false }.Judge(), Is.EqualTo(LooseDriveKind.None), "practice, or a rally without a match");
        }

        [Test]
        public void Autopilot_IsNone_UnlessTheTestPointForcesIt()
        {
            Assert.That(new Facts { Autopilot = true }.Judge(), Is.EqualTo(LooseDriveKind.None));
            Assert.That(new Facts { Autopilot = true, Force = true }.Judge(), Is.EqualTo(LooseDriveKind.ShortAndWide));
        }

        [Test]
        public void Disabled_IsNone()
        {
            Assert.That(new Facts { Enabled = false }.Judge(), Is.EqualTo(LooseDriveKind.None));
        }
    }
}
