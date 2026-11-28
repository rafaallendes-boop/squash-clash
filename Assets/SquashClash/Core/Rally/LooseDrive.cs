using SquashClash.Core.Court;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    public enum LooseDriveKind { None, Short, Wide, ShortAndWide }

    // Loose drive between points (spec 2026-11-28). Pure: the cuts are constants of this class or of the court,
    // never RallyParams fields. This revision answers nothing, so the EditMode tests fail by assertion.
    public static class LooseDrive
    {
        public const float MinPower = .5f;

        public static bool IsDrive(bool applied, bool volley, bool sideAimed, float power, float aimY, float lobMinAimY) => false;

        public static LooseDriveKind Classify(Vector3? bounce, CourtDimensions court) => LooseDriveKind.None;

        public static LooseDriveKind Judge(
            bool applied, bool volley, bool sideAimed, float power, float aimY, float lobMinAimY,
            bool lastAppliedHumanStroke, int rivalRacketContacts,
            PlayerId? winner, PointEndCause cause, ReceptionResult? reception,
            Vector3? bounce, CourtDimensions court,
            bool humanServePause, bool matchOver, bool inMatch, bool autopilot, bool enabled, bool forceForTests)
            => LooseDriveKind.None;

        public static string Text(LooseDriveKind kind) => "";
    }
}
