using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Rally
{
    // Loose-drive hint between points (spec 2026-11-28). Reads the rally and the stroke and tells the HUD what the
    // pause may show. It never feeds anything back: no pause, order, parameter or hash depends on it.
    // Once per match, and only when the banner was really seen. Nothing is stored in PlayerPrefs.
    public sealed class LooseDriveHint
    {
        // Default on. Not remembered. The DEV switch and the tests are the only writers.
        public static bool Enabled = true;
        // Lets a test match, which plays itself, count. Off, an autopilot match never shows the hint and never writes it.
        public static bool ForceForTests;
        public static void ResetForTests()
        {
            Enabled = true;
            ForceForTests = false;
        }

        private readonly AppBootstrap app;
        private readonly RallyDirector rally;
        private readonly ShotController controller;
        private MatchScore tracked;
        private bool usedThisMatch;
        private bool haveApplied, volley, sideAimed;
        private float power, aimY;
        private int rivalReturns;
        private Vector3? bounce;
        private bool armed, seenThisPause, occupiedByFirstMatch;
        private LooseDriveKind armedKind;
        private float bounceX, bounceZ;
        private string matchId = "";
        private bool injectPending;
        private LooseDriveKind injectKind;
        private float injectX, injectZ;

        // What the current pause shows. None hides the row. It does not carry into the next pause.
        public LooseDriveKind Shown { get; private set; }

        public LooseDriveHint(AppBootstrap app, RallyDirector rally, ShotController controller)
        {
            this.app = app;
            this.rally = rally;
            this.controller = controller;
            if (rally != null)
            {
                rally.RallyStarted += OnRallyStarted;
                rally.PointEnded += OnPointEnded;
                rally.RivalSwing += OnRivalSwing;
                rally.ModeChanged += OnModeChanged;
            }
            if (controller != null) controller.ShotResolved += OnShotResolved;
        }
        public void Dispose()
        {
            if (rally != null)
            {
                rally.RallyStarted -= OnRallyStarted;
                rally.PointEnded -= OnPointEnded;
                rally.RivalSwing -= OnRivalSwing;
                rally.ModeChanged -= OnModeChanged;
            }
            if (controller != null) controller.ShotResolved -= OnShotResolved;
        }

        // A qualified point for tests. Ignored unless ForceForTests is on (and the hint itself is on).
        // Applied on the next pause that is allowed to show one and does not already have one open.
        public void QualifyForTests(LooseDriveKind kind, float bounceX, float bounceZ)
        {
            if (!ForceForTests || !Enabled || kind == LooseDriveKind.None) return;
            injectKind = kind;
            injectX = bounceX;
            injectZ = bounceZ;
            injectPending = true;
        }

        // Called by the HUD every refresh, after the first-match hints have settled.
        // firstMatchBanner: that adapter already chose a banner hint for this pause, so this one stays hidden and does not count.
        public void Observe(bool firstMatchBanner)
        {
            if (!Enabled)
            {
                Shown = LooseDriveKind.None;
                return;
            }
            NoteMatch();
            if (injectPending && Showable() && !armed)
            {
                injectPending = false;
                Arm(injectKind, injectX, injectZ);
            }
            if (!Showable() || !armed)
            {
                Shown = LooseDriveKind.None;
                return;
            }
            if (firstMatchBanner) occupiedByFirstMatch = true;
            Shown = firstMatchBanner || usedThisMatch ? LooseDriveKind.None : armedKind;
        }

        // The HUD's own fade and the row that is really on screen. Same oracle as the first-match banner hints.
        public void ReportBanner(bool bannerVisible, bool rowVisible, float rowHeight, float opacity)
        {
            if (Shown == LooseDriveKind.None || !armed) return;
            if (HintOracle.BannerSeen(opacity, bannerVisible, rowVisible, rowHeight)) seenThisPause = true;
        }

        private void OnShotResolved(ShotRecord record)
        {
            bool applied = record.Applied && (record.Result == nameof(ShotResult.Perfect) || record.Result == nameof(ShotResult.Valid));
            if (!applied || controller == null) return;
            haveApplied = true;
            volley = controller.LastOpportunity == Opportunity.Volley;
            sideAimed = controller.LastStrokeSideAimed;
            power = record.Power;
            aimY = record.AimY;
            rivalReturns = 0;
            bounce = null;
        }
        private void OnRivalSwing(StrokeSide side, bool contact)
        {
            if (!contact || !haveApplied) return;
            rivalReturns++;
            if (rivalReturns == 1) bounce = rally.LastReturnedBounce;
        }
        private void OnPointEnded(PointResult result)
        {
            NoteMatch();
            var kind = LooseDrive.Judge(
                haveApplied, volley, sideAimed, power, aimY, (float)rally.Params.ServeKindLobMinAimY,
                haveApplied, rivalReturns,
                result.Winner, result.Cause, result.Reception, bounce, CourtDimensions.Standard(),
                rally.HumanServePause, rally.Phase == RallyPhase.MatchOver, rally.Match != null,
                rally.Autopilot, Enabled, ForceForTests);
            ClearStroke();
            if (kind == LooseDriveKind.None || armed) return;
            var point = bounce ?? Vector3.zero;
            Arm(kind, point.x, point.z);
        }
        private void OnRallyStarted()
        {
            ClosePause();
            ClearStroke();
            NoteMatch();
        }
        private void OnModeChanged(bool active)
        {
            if (active) return;
            ClosePause();
            ClearStroke();
        }

        private bool Showable()
            => rally.Active && rally.Phase == RallyPhase.PointOver && !rally.HumanServePause && rally.Match != null && !rally.Match.Finished;

        private void Arm(LooseDriveKind kind, float x, float z)
        {
            armed = true;
            armedKind = kind;
            bounceX = x;
            bounceZ = z;
            matchId = app.UserEvents != null ? app.UserEvents.OpenMatchId : "";
            seenThisPause = false;
            occupiedByFirstMatch = false;
        }
        // The pause ended (the next point started, or the mode stopped). One line for a qualified point, then nothing carries over.
        private void ClosePause()
        {
            Shown = LooseDriveKind.None;
            if (!armed) return;
            armed = false;
            bool seen = seenThisPause;
            bool occupied = occupiedByFirstMatch;
            seenThisPause = false;
            occupiedByFirstMatch = false;
            if (!Enabled || app.UserEvents == null) return;
            string reason;
            if (occupied) reason = "firstMatchHint";
            else if (usedThisMatch) reason = "limit";
            else if (seen) { reason = ""; usedThisMatch = true; }
            else reason = "notSeen";
            app.UserEvents.DriveHint(matchId, armedKind.ToString(), bounceX, bounceZ, reason.Length == 0, reason);
        }
        private void NoteMatch()
        {
            if (ReferenceEquals(tracked, rally.Match)) return;
            tracked = rally.Match;
            usedThisMatch = false;
            injectPending = false;
        }
        private void ClearStroke()
        {
            haveApplied = volley = sideAimed = false;
            power = aimY = 0;
            rivalReturns = 0;
            bounce = null;
        }
    }
}
