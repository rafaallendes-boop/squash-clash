using System;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Menu
{
    // First-match hints (spec front 2, 2026-11-22): the adapter between the game and HintPicker. It reads what the rally and the stroke
    // already expose (events and read-only properties), keeps what the device remembers in PlayerPrefs (sc.hints.*) and writes the
    // «hint» and «serve-choice» metric lines. It never feeds anything back: no pause, order, parameter or hash of the game depends on it.
    // The HUD draws the rows and tells the adapter whether they were really visible (the two oracles of «seen»).
    public sealed class FirstMatchHints
    {
        public const string ServeKey = "sc.hints.serve", VolleyKey = "sc.hints.volley", BoastKey = "sc.hints.boast", BoastCrossKey = "sc.hints.boastCross", WindowKey = "sc.hints.window";
        private static readonly string[] Keys = { ServeKey, VolleyKey, BoastKey, BoastCrossKey, WindowKey };
        private static readonly string[] Names = { "serve", "volley", "boast", "boastCross" };
        // DEV switch (default on). Off: nothing is written and nothing is shown. Not remembered.
        public static bool Enabled = true;
        // Real time, unscaled, replaceable by tests (like PointEndFreeze.Now).
        public static Func<double> Now = () => Time.unscaledTimeAsDouble;
        // Test point: lets a match played by autopilot count as a window match. Only the new tests use it.
        public static bool ForceWindowForTests;
        public static string[] AllKeys() => (string[])Keys.Clone();
        public static void Clear()
        {
            foreach (var key in Keys) PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
        public static void ResetForTests()
        {
            Enabled = true;
            Now = () => Time.unscaledTimeAsDouble;
            ForceWindowForTests = false;
        }
        // Texts decided by SC-Lead (23 nov, vocab-93) under the text rule: real squash terms, neutral Spanish, «tú».
        // 24 nov (design 2026-11-24-two-wall-preview-hint): the boast hint warns that a high side-wall touch can leave the ball long.
        public static string Text(HintId id, HintWhere where)
        {
            switch (id)
            {
                case HintId.Serve:
                    return where == HintWhere.Guide
                        ? "Sacas tú: toca el frontis antes de que se acabe la barra. Si no tocas, sale un globo."
                        : "Cuando saques tú, toca el frontis durante la pausa: arriba sale un globo; abajo, un saque rápido.";
                case HintId.Volley: return SquashClash.Runtime.Shot.ReceptionRule.Full;
                case HintId.Boast: return "Dos paredes: si tocas alto en la lateral, puede quedar larga.";
                default: return "Pared lateral cerca de ti: la pelota cruza al otro lado.";
            }
        }

        private readonly AppBootstrap app;
        private readonly RallyDirector rally;
        private readonly ShotController controller;
        private HintPicker picker;
        private MatchScore hintMatch;
        private readonly int[] stored = new int[HintPicker.Count];
        private bool windowStored;
        // The pause being shown: chosen once when it starts, kept until it ends (a hint that just became seen stays on screen).
        private int pausePoint = -1;
        private HintPause pauseKind;
        private HintId? chosen, forced;
        private readonly GuideSeenClock guideClock = new GuideSeenClock();
        // A human reception: the amber ring must have shown in some frame, and the reception must end without a volley applied.
        private long receptionLaunch = -1;
        private bool receptionOpen, receptionHandled, volleyShown;
        // The pause before the human's own serve, for the «serve-choice» line.
        private bool servePauseSeen, hintSeenAtServePause;

        public FirstMatchHints(AppBootstrap app, RallyDirector rally, ShotController controller)
        {
            this.app = app;
            this.rally = rally;
            this.controller = controller;
            if (rally != null)
            {
                rally.RallyStarted += OnRallyStarted;
                rally.PointEnded += OnPointEnded;
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
                rally.ModeChanged -= OnModeChanged;
            }
            if (controller != null) controller.ShotResolved -= OnShotResolved;
        }

        // The current match counts for the window and is still the one the picker follows.
        public bool InWindowMatch => Enabled && picker != null && hintMatch != null && picker.InMatch && ReferenceEquals(rally.Match, hintMatch);
        // The hint the current pause shows, or null. «Where» says in which container: the serve guide only in the human's serve pause.
        public HintId? Shown => InWindowMatch ? forced ?? chosen : null;
        public HintWhere ShownWhere => pauseKind == HintPause.HumanServe ? HintWhere.Guide : HintWhere.Banner;
        public HintState State(HintId id) => picker != null ? picker.State(id) : HintState.Unseen;

        // The start menu began a match (FirstRunFlow.StartMatch). The practice court, the DEV «Partido», a rematch and a match that ever
        // ran on autopilot are not in the window.
        public void BeginMatch()
        {
            Abandon();
            if (!Enabled || rally == null || rally.Match == null || !rally.Active) return;
            if (rally.Autopilot && !ForceWindowForTests) return;
            var next = Load();
            if (!next.BeginMatch()) return;
            picker = next;
            hintMatch = rally.Match;
            pausePoint = -1;
            chosen = null;
            receptionLaunch = -1;
            receptionOpen = receptionHandled = volleyShown = false;
            guideClock.Reset();
        }

        private HintPicker Load()
        {
            for (int i = 0; i < HintPicker.Count; i++) stored[i] = Math.Max(0, Math.Min(2, PlayerPrefs.GetInt(Keys[i], 0)));
            windowStored = PlayerPrefs.GetInt(WindowKey, 0) == 1;
            return new HintPicker(stored[0], stored[1], stored[2], stored[3], windowStored);
        }
        // Writes what changed in the picker: the key, the «hint» line and a save, in the same frame as the change.
        private void Sync()
        {
            if (picker == null) return;
            bool save = false;
            for (int i = 0; i < HintPicker.Count; i++)
            {
                var id = (HintId)i;
                int value = picker.Stored(id);
                if (value == stored[i]) continue;
                stored[i] = value;
                PlayerPrefs.SetInt(Keys[i], value);
                save = true;
                if (value == 1) app?.UserEvents?.Hint(Names[i], "seen", picker.SeenWhere(id) == HintWhere.Guide ? "guide" : "banner");
                else if (value == 2) app?.UserEvents?.Hint(Names[i], "retired", "");
            }
            if (picker.WindowClosed && !windowStored)
            {
                windowStored = true;
                PlayerPrefs.SetInt(WindowKey, 1);
                save = true;
            }
            if (save) PlayerPrefs.Save();
        }
        // Armed hints go back to unseen, nothing else is written. The window stays as it is.
        private void Abandon()
        {
            if (picker != null && picker.InMatch)
            {
                picker.EndMatch(false);
                Sync();
            }
            hintMatch = null;
            chosen = null;
            pausePoint = -1;
            receptionOpen = false;
            guideClock.Reset();
        }

        // ----- events of the game -----
        private void OnRallyStarted()
        {
            WriteServeChoice();
            if (picker == null || hintMatch == null) return;
            if (!ReferenceEquals(rally.Match, hintMatch)) { Abandon(); return; }
            picker.EndPause();
            if (rally.Match.Server == PlayerId.Rival) picker.RivalServed();
            pausePoint = -1;
            chosen = null;
            guideClock.Reset();
            receptionOpen = receptionHandled = false;
        }
        private void OnModeChanged(bool active)
        {
            if (!active) Abandon();
        }
        private void OnPointEnded(PointResult result)
        {
            if (!InWindowMatch) return;
            // A point that closed a human reception with no stroke event at all: ring shown, no volley applied.
            OpenReception();
            if (receptionOpen && !receptionHandled)
            {
                picker.OnReceptionEnded(volleyShown, false);
                receptionHandled = true;
            }
            if (rally.Phase == RallyPhase.MatchOver)
            {
                picker.EndMatch(true);
                Sync();
                hintMatch = null;
                chosen = null;
                guideClock.Reset();
            }
            else Sync();
        }
        private void OnShotResolved(ShotRecord record)
        {
            if (!InWindowMatch) return;
            OpenReception();
            bool applied = record.Applied && (record.Result == nameof(ShotResult.Perfect) || record.Result == nameof(ShotResult.Valid));
            picker.OnStroke(applied, controller.SideAimOpen, controller.LastStrokeBoast);
            if (receptionOpen && !receptionHandled)
            {
                picker.OnReceptionEnded(volleyShown, applied && controller.LastOpportunity == Opportunity.Volley);
                receptionHandled = true;
            }
            Sync();
        }
        // Test points: arm a hint through the same picker entry its trigger uses.
        public void ArmForTests(HintId id)
        {
            if (picker == null) return;
            switch (id)
            {
                case HintId.Volley: picker.OnReceptionEnded(true, false); break;
                case HintId.Boast: picker.OnStroke(true, true, false); break;
                case HintId.BoastCross: picker.OnStroke(true, true, true); break;
            }
            Sync();
        }
        public void ForceShownForTests(HintId? id)
        {
            forced = id;
        }

        private void OpenReception()
        {
            if (rally == null || !rally.Active || rally.Phase != RallyPhase.HumanReception) { ObserveVolley(); return; }
            long launch = app.Host.LaunchId;
            if (launch != receptionLaunch)
            {
                receptionLaunch = launch;
                receptionOpen = true;
                receptionHandled = false;
                volleyShown = false;
            }
            ObserveVolley();
        }
        private void ObserveVolley()
        {
            if (receptionOpen && !receptionHandled && controller != null && controller.VolleyShown) volleyShown = true;
        }
        private void WriteServeChoice()
        {
            if (!Enabled || app?.UserEvents == null || rally.Match == null) { servePauseSeen = false; return; }
            if (rally.LastServeFromPause && rally.Match.Server == PlayerId.Human)
                app.UserEvents.ServeChoice(rally.LastServeChosen, rally.LastServeType.ToString(), servePauseSeen ? hintSeenAtServePause : PlayerPrefs.GetInt(ServeKey, 0) == 1);
            servePauseSeen = false;
        }

        // ----- the HUD, every frame -----
        // Observes the game and settles which hint the current pause shows. Called by RallyHud.Refresh before the rows are laid out.
        public void Observe()
        {
            if (!Enabled) { chosen = null; return; }
            if (rally.HumanServePause && !servePauseSeen)
            {
                servePauseSeen = true;
                hintSeenAtServePause = PlayerPrefs.GetInt(ServeKey, 0) == 1;
            }
            if (picker == null || hintMatch == null) return;
            if (!rally.Active || !ReferenceEquals(rally.Match, hintMatch) || (rally.Autopilot && !ForceWindowForTests)) { Abandon(); return; }
            OpenReception();
            bool pause = rally.Phase == RallyPhase.PointOver && rally.LastPoint.HasValue && !rally.Match.Finished;
            if (!pause)
            {
                if (pausePoint >= 0) { pausePoint = -1; chosen = null; guideClock.Reset(); }
                return;
            }
            var kind = rally.HumanServePause ? HintPause.HumanServe : HintPause.Ordinary;
            if (rally.PointsPlayed == pausePoint && kind == pauseKind) return;
            pausePoint = rally.PointsPlayed;
            pauseKind = kind;
            guideClock.Reset();
            chosen = picker.Choose(kind, rally.Match.Server == PlayerId.Rival);
        }
        // In the banner: the HUD's own fade and the visibility of the banner and of the row, from the HUD.
        public void ReportBanner(bool bannerVisible, bool rowVisible, float rowHeight, float opacity)
        {
            if (!InWindowMatch || forced.HasValue || !chosen.HasValue || ShownWhere != HintWhere.Banner) return;
            if (HintOracle.BannerSeen(opacity, bannerVisible, rowVisible, rowHeight) && picker.MarkSeen(chosen.Value, HintWhere.Banner)) Sync();
        }
        // In the serve guide: real time with the guide and the hint visible, a game that is not paused.
        public void ReportGuide(bool visible)
        {
            if (!InWindowMatch || forced.HasValue || !chosen.HasValue || ShownWhere != HintWhere.Guide) return;
            guideClock.Frame(Now(), visible, app.Clock != null && app.Clock.Paused);
            if (visible && guideClock.Reached(RallyHud.BannerInSeconds) && picker.MarkSeen(chosen.Value, HintWhere.Guide)) Sync();
        }
    }
}
