using SquashClash.Core.Court;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    public enum LooseDriveKind { None, Short, Wide, ShortAndWide }

    // Loose drive between points (spec 2026-11-28). Pure: the power cut is a constant of this class.
    // The short line and the side-wall distance come from the court. The lob line is the caller's
    // ServeKindLobMinAimY. Nothing here is a RallyParams field.
    public static class LooseDrive
    {
        public const float MinPower = .5f;

        public static bool IsDrive(bool applied, bool volley, bool sideAimed, float power, float aimY, float lobMinAimY)
            => applied && !volley && !sideAimed && power > MinPower && aimY < lobMinAimY;

        public static LooseDriveKind Classify(Vector3? bounce, CourtDimensions court)
        {
            if (bounce == null || court == null) return LooseDriveKind.None;
            float shortLine = court.Length - court.ShortLineNearBackEdgeFromBack;
            bool shortBall = bounce.Value.z < shortLine;
            float nearestWall = System.Math.Min(bounce.Value.x, court.Width - bounce.Value.x);
            bool wide = nearestWall > court.ServiceBoxInner;
            if (shortBall && wide) return LooseDriveKind.ShortAndWide;
            if (shortBall) return LooseDriveKind.Short;
            if (wide) return LooseDriveKind.Wide;
            return LooseDriveKind.None;
        }

        public static LooseDriveKind Judge(
            bool applied, bool volley, bool sideAimed, float power, float aimY, float lobMinAimY,
            bool lastAppliedHumanStroke, int rivalRacketContacts,
            PlayerId? winner, PointEndCause cause, ReceptionResult? reception,
            Vector3? bounce, CourtDimensions court,
            bool humanServePause, bool matchOver, bool inMatch, bool autopilot, bool enabled, bool forceForTests)
        {
            if (!enabled || !inMatch || matchOver || humanServePause) return LooseDriveKind.None;
            if (autopilot && !forceForTests) return LooseDriveKind.None;
            if (!lastAppliedHumanStroke) return LooseDriveKind.None;
            if (!IsDrive(applied, volley, sideAimed, power, aimY, lobMinAimY)) return LooseDriveKind.None;
            if (rivalRacketContacts != 1) return LooseDriveKind.None;
            if (winner != PlayerId.Rival || cause != PointEndCause.DoubleBounce || reception != ReceptionResult.Unreachable) return LooseDriveKind.None;
            return Classify(bounce, court);
        }

        public static string Text(LooseDriveKind kind)
        {
            switch (kind)
            {
                case LooseDriveKind.Short: return "Tu drive botó corto: el rival lo aprovechó.";
                case LooseDriveKind.Wide: return "Tu drive quedó lejos de la pared lateral: el rival lo aprovechó.";
                case LooseDriveKind.ShortAndWide: return "Tu drive botó corto y lejos de la pared lateral: el rival lo aprovechó.";
                default: return "";
            }
        }
    }
}
