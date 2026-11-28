using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Loose-drive hint (spec 2026-11-28), criteria 3 to 6, plus the two read-only fields.
    // The hint is on. ForceForTests is set only by the tests that play a match on autopilot.
    public sealed class ZLooseDriveHintTests : ZFirstRunFixture
    {
        private const string ShortText = "Tu drive botó corto: el rival lo aprovechó.";
        private const string WideText = "Tu drive quedó lejos de la pared lateral: el rival lo aprovechó.";
        private const string BothText = "Tu drive botó corto y lejos de la pared lateral: el rival lo aprovechó.";
        [Serializable] private sealed class Line { public string kind, payload; }
        [Serializable] private sealed class DriveLine { public string matchId, kind, reason; public float bounceX, bounceZ; public bool seen; }

        [UnitySetUp] public IEnumerator Setup()
        {
            LooseDriveHint.ResetForTests();
            FirstMatchHints.ResetForTests();
            yield return Boot(true);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            LooseDriveHint.ResetForTests();
            FirstMatchHints.ResetForTests();
            PointEndFreeze.Reset();
            yield return Shutdown();
        }

        private bool Ordinary() => rally.Phase == RallyPhase.PointOver && !rally.HumanServePause && rally.Match != null && !rally.Match.Finished;
        private bool ReachOrdinary() => Drive(() => Ordinary(), 900);
        private void Step()
        {
            app.Host.Advance(StepReal);
            controller.ProcessPending();
            rally.Tick();
            scape.Tick((float)StepReal);
            Refresh();
            hud.Tick((float)StepReal);
        }
        private bool LeavePause()
        {
            Click("rally-next");
            return Drive(() => rally.Phase != RallyPhase.PointOver, 5);
        }
        private IEnumerator Inject(LooseDriveKind kind, float x, float z, float fade)
        {
            hud.Drive.QualifyForTests(kind, x, z);
            Refresh();
            yield return null;
            yield return null;
            hud.Tick(fade);
        }
        private DriveLine[] DriveLines()
        {
            string path = app.Metrics.CurrentFilePath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return new DriveLine[0];
            return File.ReadAllLines(path).Select(JsonUtility.FromJson<Line>).Where(r => r != null && r.kind == "drive-hint").Select(r => JsonUtility.FromJson<DriveLine>(r.payload)).ToArray();
        }
        private static bool DriveSentence(string text) => text == ShortText || text == WideText || text == BothText;
        private bool RowOn() => Find("rally-banner-hint").style.display.value == DisplayStyle.Flex;
        private string RowText() => RowOn() ? Find<Label>("rally-banner-hint-text").text : "";
        // First non-persistent floor after the human stroke. The flight is still the human's at RivalSwing(true).
        private Vector3? FirstFloorAfter(long contactStep)
        {
            foreach (var e in app.Host.CanonicalContacts)
            {
                if (e.Step <= contactStep) continue;
                if (e.Surface == SurfaceId.Floor && !e.Persistent) return e.Point;
            }
            return null;
        }
        private void StartLoose(long seed, AiLevel level, double sigma, ShotPower power, bool autopilot = true)
        {
            LooseDriveHint.ResetForTests();
            LooseDriveHint.ForceForTests = true;
            FirstMatchHints.ResetForTests();
            flow.ClosePractice();
            rally.Autopilot = autopilot;
            rally.AutopilotTimingSigmaMs = sigma;
            rally.Level = level;
            rally.Seed = seed;
            rally.RandomStyles = false;
            rally.StyleOverride = RivalStyleId.Fondista;
            rally.AutopilotPower = power;
            Assert.That(rally.StartMatch(), Is.True);
        }
        // AppBootstrap writes the score on PointEnded without a null check. The same-match rally start does not.
        // Drop those two handlers so a missing metric can be observed through the next pause.
        private void DropUserEvents()
        {
            UnhookBootstrap(typeof(RallyDirector).GetField(nameof(RallyDirector.PointEnded), BindingFlags.Instance | BindingFlags.NonPublic));
            UnhookBootstrap(typeof(RallyDirector).GetField(nameof(RallyDirector.ModeChanged), BindingFlags.Instance | BindingFlags.NonPublic));
            var set = typeof(AppBootstrap).GetProperty(nameof(AppBootstrap.UserEvents)).GetSetMethod(true);
            Assert.That(set, Is.Not.Null, "UserEvents has no setter to clear");
            set.Invoke(app, new object[] { null });
            Assert.That(app.UserEvents, Is.Null);
        }
        private void UnhookBootstrap(FieldInfo field)
        {
            Assert.That(field, Is.Not.Null, "missing rally event field");
            var del = field.GetValue(rally) as Delegate;
            if (del == null) return;
            foreach (var handler in del.GetInvocationList())
                if (handler.Target is AppBootstrap)
                    field.SetValue(rally, Delegate.Remove((Delegate)field.GetValue(rally), handler));
        }

        private sealed class Attempt
        {
            public bool Resolved, Waiting, SideBefore, SideInCallback, BoastInCallback, FellBack;
            public string Result;
        }
        private Attempt Swing(Func<Vector3, SideAim?> side)
        {
            app.LaunchCurrent();
            Assert.That(controller.Plan.Exists, Is.True);
            Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep, 0, out var star), Is.True);
            for (int i = 0; i < 6000 && app.Host.ObservedRealTime + StepReal < star - 1e-10; i++) app.Host.Advance(StepReal);
            double left = star - app.Host.ObservedRealTime;
            if (left > 0) app.Host.Advance(left);
            var p = controller.Plan.PStar;
            var aim = side(p);
            var o = new Attempt();
            double at = star;
            var gesture = aim.HasValue
                ? new ClosedGesture(app.Host.LaunchId, Guid.NewGuid().ToString("N"), 1, app.Host.LaunchRealTime + .01, at, true, GestureOutcome.Swing, aim.Value.UV, .8f, app.Host.ActiveConfig.Source, "fixture", at, side: aim.Value)
                : new ClosedGesture(app.Host.LaunchId, Guid.NewGuid().ToString("N"), 1, app.Host.LaunchRealTime + .01, at, true, GestureOutcome.Swing, new Vector2(3.2f, 2.2f), .8f, app.Host.ActiveConfig.Source, "fixture", at);
            Action<ShotRecord> onShot = r => { o.Resolved = true; o.SideInCallback = controller.LastStrokeSideAimed; o.BoastInCallback = controller.LastStrokeBoast; o.Result = r.Result; };
            controller.ShotResolved += onShot;
            controller.Enqueue(gesture);
            controller.ProcessPending();
            o.Waiting = controller.WaitingForContact;
            o.SideBefore = controller.LastStrokeSideAimed;
            for (int i = 0; i < 6000 && !o.Resolved; i++) { app.Host.Advance(StepReal); controller.ProcessPending(); }
            controller.ShotResolved -= onShot;
            o.FellBack = controller.LastCausal.FellBack;
            return o;
        }
        private SideAim Boast(Vector3 p, float dz)
        {
            var wall = SideAim.NearWall(p.x, app.Host.ActiveConfig.Court);
            return new SideAim(wall, Mathf.Max(.5f, p.z + dz), 1.4f);
        }

        [UnityTest] public IEnumerator LastStrokeSideAimed_IsTheOrdersSideInsideShotResolved_AndClearsOnTheNextLaunch()
        {
            // A (SC-PM, diagnosis set30): close the menu so the clock is running; otherwise TrySimToReal fails.
            flow.ClosePractice();
            var front = Swing(_ => null);
            Assert.That(front.Resolved, Is.True);
            Assert.That(front.Result, Is.EqualTo("Perfect").Or.EqualTo("Valid"));
            Assert.That(front.SideInCallback, Is.False, "a front-wall order is not side aimed");
            if (front.Waiting) Assert.That(front.SideBefore, Is.False, "a preview does not publish the mark");

            bool sideSeen = false;
            foreach (float dz in new[] { -.8f, -.5f, -1.1f, .9f })
            {
                var o = Swing(p => Boast(p, dz));
                Assert.That(o.Resolved, Is.True, "dz=" + dz);
                Assert.That(o.SideInCallback, Is.True, "the order aimed at a side wall (dz=" + dz + ", fellBack=" + o.FellBack + ")");
                if (o.Waiting) Assert.That(o.SideBefore, Is.False, "preview claims nothing (dz=" + dz + ")");
                if (o.FellBack) Assert.That(o.BoastInCallback, Is.False, "a fallback is not a solved boast, and the side aim still stands");
                sideSeen = true;
                Assert.That(controller.LastStrokeSideAimed, Is.True);
                app.LaunchCurrent();
                Assert.That(controller.LastStrokeSideAimed, Is.False, "cleared on the next launch");
                break;
            }
            Assert.That(sideSeen, Is.True);
            var fallback = Swing(p => Boast(p, .9f));
            Assert.That(fallback.Resolved, Is.True);
            Assert.That(fallback.FellBack, Is.True, "the boast behind the contact falls back to a drive");
            Assert.That(fallback.SideInCallback, Is.True, "the order still aimed at the side wall");
            Assert.That(fallback.BoastInCallback, Is.False);
            yield return null;
        }

        [UnityTest] public IEnumerator LastReturnedBounce_IsTheFirstFloorAfterTheHumanStroke_AndClearsOnTheNextPoint()
        {
            StartLoose(11, AiLevel.Medium, 0, ShotPower.Strong);
            bool human = false;
            long contact = 0;
            int nonNull = 0;
            bool cleared = false;
            Action<ShotRecord> onShot = r =>
            {
                if (r.Applied && (r.Result == "Perfect" || r.Result == "Valid")) { human = true; contact = r.ContactStep; }
            };
            Action<StrokeSide, bool> onSwing = (_, made) =>
            {
                if (!made || !human) return;
                human = false;
                var found = FirstFloorAfter(contact);
                if (!found.HasValue) Assert.That(rally.LastReturnedBounce.HasValue, Is.False);
                else
                {
                    Assert.That(rally.LastReturnedBounce.HasValue, Is.True);
                    Assert.That(rally.LastReturnedBounce.Value.x, Is.EqualTo(found.Value.x));
                    Assert.That(rally.LastReturnedBounce.Value.y, Is.EqualTo(found.Value.y));
                    Assert.That(rally.LastReturnedBounce.Value.z, Is.EqualTo(found.Value.z));
                    nonNull++;
                }
            };
            Action onStart = () =>
            {
                human = false;
                if (nonNull > 0) { Assert.That(rally.LastReturnedBounce.HasValue, Is.False); cleared = true; }
            };
            controller.ShotResolved += onShot;
            rally.RivalSwing += onSwing;
            rally.RallyStarted += onStart;
            try
            {
                for (int i = 0; i < 400000 && !cleared && rally.Phase != RallyPhase.MatchOver; i++)
                {
                    Step();
                    if (i % 30 == 0) yield return null;
                }
            }
            finally
            {
                controller.ShotResolved -= onShot;
                rally.RivalSwing -= onSwing;
                rally.RallyStarted -= onStart;
            }
            Assert.That(nonNull, Is.GreaterThan(0), "seed 11 / Medium / Strong / Fondista produced no returned floor bounce. Stop. Do not loosen.");
            Assert.That(cleared, Is.True, "BeginPoint did not run after the bounce, so the clear was not observed");
        }

        // Criterion 3. The banner text matches an independent Judge. At least one pause must qualify.
        [UnityTest] public IEnumerator Scene_BannerMatchesAnIndependentLooseDrive_AndShowsAtLeastOne()
        {
            // E (PO, 28 nov 15:21): first seed from 1 to 24 with a natural loose drive (Medium, sigma 0, Strong).
            int chosen = 0;
            for (int seed = 1; seed <= 24 && chosen == 0; seed++)
            {
                rally.StopMode();
                StartLoose(seed, AiLevel.Medium, 0, ShotPower.Strong);
                bool have = false, volley = false, side = false;
                float power = 0, aimY = 0;
                int rivals = 0;
                long contact = 0;
                Vector3? bounce = null;
                bool found = false;
                Action<ShotRecord> scanShot = r =>
                {
                    bool applied = r.Applied && (r.Result == nameof(ShotResult.Perfect) || r.Result == nameof(ShotResult.Valid));
                    if (!applied) return;
                    have = true;
                    volley = controller.LastOpportunity == Opportunity.Volley;
                    side = controller.LastStrokeSideAimed;
                    power = r.Power;
                    aimY = r.AimY;
                    rivals = 0;
                    bounce = null;
                    contact = r.ContactStep;
                };
                Action<StrokeSide, bool> scanSwing = (_, made) =>
                {
                    if (!made || !have) return;
                    rivals++;
                    if (rivals == 1) bounce = FirstFloorAfter(contact);
                };
                Action<PointResult> scanPoint = result =>
                {
                    var kind = LooseDrive.Judge(
                        have, volley, side, power, aimY, (float)rally.Params.ServeKindLobMinAimY,
                        have, rivals, result.Winner, result.Cause, result.Reception, bounce, CourtDimensions.Standard(),
                        rally.HumanServePause, rally.Phase == RallyPhase.MatchOver, rally.Match != null,
                        rally.Autopilot, true, true);
                    if (kind != LooseDriveKind.None) found = true;
                    have = false;
                    rivals = 0;
                    bounce = null;
                };
                controller.ShotResolved += scanShot;
                rally.RivalSwing += scanSwing;
                rally.PointEnded += scanPoint;
                try
                {
                    for (int i = 0; i < 400000 && rally.Phase != RallyPhase.MatchOver && !found; i++)
                    {
                        Step();
                        if (i % 12 == 0) yield return null;
                    }
                }
                finally
                {
                    controller.ShotResolved -= scanShot;
                    rally.RivalSwing -= scanSwing;
                    rally.PointEnded -= scanPoint;
                }
                if (found) chosen = seed;
            }
            Assert.That(chosen, Is.GreaterThan(0),
                "no seed from 1 to 24 has a natural loose drive (Medium / sigma 0 / Strong / Fondista). Stop. Do not loosen the criterion.");
            rally.StopMode();
            StartLoose(chosen, AiLevel.Medium, 0, ShotPower.Strong);
            bool have = false, volley = false, side = false;
            float power = 0, aimY = 0;
            int rivals = 0;
            long contact = 0;
            Vector3? bounce = null;
            LooseDriveKind expected = LooseDriveKind.None;
            bool seenEarlier = false, sawThisPause = false;
            int positives = 0, pauses = 0;
            bool wasOrdinary = false;
            Action<ShotRecord> onShot = r =>
            {
                bool applied = r.Applied && (r.Result == nameof(ShotResult.Perfect) || r.Result == nameof(ShotResult.Valid));
                if (!applied) return;
                have = true;
                volley = controller.LastOpportunity == Opportunity.Volley;
                side = controller.LastStrokeSideAimed;
                power = r.Power;
                aimY = r.AimY;
                rivals = 0;
                bounce = null;
                contact = r.ContactStep;
            };
            Action<StrokeSide, bool> onSwing = (_, made) =>
            {
                if (!made || !have) return;
                rivals++;
                if (rivals == 1) bounce = FirstFloorAfter(contact);
            };
            Action<PointResult> onPoint = result =>
            {
                expected = LooseDrive.Judge(
                    have, volley, side, power, aimY, (float)rally.Params.ServeKindLobMinAimY,
                    have, rivals, result.Winner, result.Cause, result.Reception, bounce, CourtDimensions.Standard(),
                    rally.HumanServePause, rally.Phase == RallyPhase.MatchOver, rally.Match != null,
                    rally.Autopilot, true, true);
                have = false;
                rivals = 0;
                bounce = null;
            };
            controller.ShotResolved += onShot;
            rally.RivalSwing += onSwing;
            rally.PointEnded += onPoint;
            try
            {
                for (int i = 0; i < 400000 && rally.Phase != RallyPhase.MatchOver; i++)
                {
                    Step();
                    bool ordinary = Ordinary();
                    if (ordinary && !wasOrdinary) pauses++;
                    if (!ordinary && wasOrdinary && sawThisPause) seenEarlier = true;
                    if (!ordinary) sawThisPause = false;
                    wasOrdinary = ordinary;
                    string want = ordinary && expected != LooseDriveKind.None && !seenEarlier ? LooseDrive.Text(expected) : "";
                    if (want.Length == 0)
                        Assert.That(RowOn(), Is.False, "no loose-drive row in " + rally.Phase + " serve=" + rally.HumanServePause + " kind=" + expected);
                    else
                    {
                        Assert.That(RowText(), Is.EqualTo(want));
                        Assert.That(Find("rally-banner").style.display.value, Is.EqualTo(DisplayStyle.Flex));
                        Assert.That(Find("rally-banner-hint-icon").style.display.value, Is.EqualTo(DisplayStyle.None));
                        positives++;
                    }
                    float height = Find("rally-banner-hint").layout.height;
                    bool bannerOn = Find("rally-banner").style.display.value == DisplayStyle.Flex;
                    if (ordinary && want.Length > 0 && HintOracle.BannerSeen(hud.BannerOpacity, bannerOn, RowOn(), height)) sawThisPause = true;
                    if (ordinary && i % 6 == 0) yield return null;
                }
            }
            finally
            {
                controller.ShotResolved -= onShot;
                rally.RivalSwing -= onSwing;
                rally.PointEnded -= onPoint;
            }
            Assert.That(rally.Phase, Is.EqualTo(RallyPhase.MatchOver));
            Assert.That(pauses, Is.GreaterThan(0));
            Assert.That(positives, Is.GreaterThan(0), "seed " + chosen + " / Medium / sigma 0 / Strong / Fondista: no loose drive after it was selected. Stop. Do not loosen the criterion.");
        }

        [UnityTest] public IEnumerator OncePerMatch_ASeenHintBlocksTheNext_AndARematchAllowsIt()
        {
            // B (PO, 28 nov 15:21): injection tests play with autopilot off, so the rival wins and an ordinary pause arrives.
            StartLoose(3, AiLevel.Easy, 0, ShotPower.Medium, autopilot: false);
            Assert.That(ReachOrdinary(), Is.True);
            string match = app.UserEvents.OpenMatchId;
            yield return Inject(LooseDriveKind.Short, .8f, 5.4f, RallyHud.BannerInSeconds);
            Assert.That(hud.BannerOpacity, Is.EqualTo(1f));
            Assert.That(Find("rally-banner-hint").layout.height, Is.GreaterThan(0f));
            Assert.That(RowText(), Is.EqualTo(ShortText));
            Assert.That(hud.Drive.Shown, Is.EqualTo(LooseDriveKind.Short));
            Assert.That(LeavePause(), Is.True);
            Assert.That(ReachOrdinary(), Is.True);
            yield return Inject(LooseDriveKind.Wide, 3.2f, 8f, RallyHud.BannerInSeconds);
            Assert.That(hud.Drive.Shown, Is.EqualTo(LooseDriveKind.None));
            Assert.That(DriveSentence(RowText()), Is.False, "the second qualified pause of the match does not show it");
            Assert.That(LeavePause(), Is.True);
            var lines = DriveLines();
            Assert.That(lines.Length, Is.EqualTo(2));
            Assert.That(lines[0].matchId, Is.EqualTo(match));
            Assert.That(lines[0].kind, Is.EqualTo("Short"));
            Assert.That(lines[0].bounceX, Is.EqualTo(.8f));
            Assert.That(lines[0].bounceZ, Is.EqualTo(5.4f));
            Assert.That(lines[0].seen, Is.True);
            Assert.That(lines[0].reason, Is.EqualTo(""));
            Assert.That(lines[1].matchId, Is.EqualTo(match));
            Assert.That(lines[1].kind, Is.EqualTo("Wide"));
            Assert.That(lines[1].seen, Is.False);
            Assert.That(lines[1].reason, Is.EqualTo("limit"));
            rally.Rematch();
            Refresh();
            Assert.That(ReachOrdinary(), Is.True);
            Assert.That(app.UserEvents.OpenMatchId, Is.Not.EqualTo(match));
            yield return Inject(LooseDriveKind.ShortAndWide, 3.2f, 5.4f, RallyHud.BannerInSeconds);
            Assert.That(RowText(), Is.EqualTo(BothText), "a new match can show it again");
            Assert.That(hud.Drive.Shown, Is.EqualTo(LooseDriveKind.ShortAndWide));
        }

        // O1. The once-per-match cap is the view, not the metric line. UserEvents is null for the view and the next point.
        [UnityTest] public IEnumerator SeenOnce_BlocksTheNextPoint_WhenUserEventsIsNull()
        {
            // B (PO, 28 nov 15:21): injection tests play with autopilot off.
            StartLoose(3, AiLevel.Easy, 0, ShotPower.Medium, autopilot: false);
            Assert.That(ReachOrdinary(), Is.True);
            var match = rally.Match;
            DropUserEvents();
            yield return Inject(LooseDriveKind.Short, .8f, 5.4f, RallyHud.BannerInSeconds);
            Assert.That(hud.BannerOpacity, Is.EqualTo(1f));
            Assert.That(Find("rally-banner-hint").layout.height, Is.GreaterThan(0f));
            Assert.That(RowText(), Is.EqualTo(ShortText));
            Assert.That(hud.Drive.Shown, Is.EqualTo(LooseDriveKind.Short));
            Assert.That(LeavePause(), Is.True);
            Assert.That(ReachOrdinary(), Is.True);
            Assert.That(rally.Match, Is.SameAs(match), "still the same match");
            Assert.That(app.UserEvents, Is.Null);
            yield return Inject(LooseDriveKind.Wide, 3.2f, 8f, RallyHud.BannerInSeconds);
            Assert.That(hud.Drive.Shown, Is.EqualTo(LooseDriveKind.None));
            Assert.That(DriveSentence(RowText()), Is.False, "a seen hint blocks the next qualified point even when UserEvents is null");
        }

        [UnityTest] public IEnumerator SeguirBeforeOpacityOne_DoesNotCount_AndTheNextPauseCanShowIt()
        {
            // B (PO, 28 nov 15:21): injection tests play with autopilot off.
            StartLoose(4, AiLevel.Easy, 0, ShotPower.Medium, autopilot: false);
            Assert.That(ReachOrdinary(), Is.True);
            yield return Inject(LooseDriveKind.Wide, 3.2f, 8f, .05f);
            Assert.That(hud.BannerOpacity, Is.LessThan(1f));
            Assert.That(RowText(), Is.EqualTo(WideText));
            Assert.That(LeavePause(), Is.True);
            var skipped = DriveLines();
            Assert.That(skipped.Length, Is.EqualTo(1));
            Assert.That(skipped[0].seen, Is.False);
            Assert.That(skipped[0].reason, Is.EqualTo("notSeen"));
            Assert.That(skipped[0].kind, Is.EqualTo("Wide"));
            Assert.That(ReachOrdinary(), Is.True);
            yield return Inject(LooseDriveKind.Short, .8f, 5.4f, RallyHud.BannerInSeconds);
            Assert.That(RowText(), Is.EqualTo(ShortText));
            Assert.That(hud.Drive.Shown, Is.EqualTo(LooseDriveKind.Short));
            Assert.That(Find("rally-banner-hint").layout.height, Is.GreaterThan(0f));
            Assert.That(LeavePause(), Is.True);
            var lines = DriveLines();
            Assert.That(lines.Length, Is.EqualTo(2));
            Assert.That(lines[1].reason, Is.EqualTo(""));
            Assert.That(lines[1].seen, Is.True);
            Assert.That(lines[1].kind, Is.EqualTo("Short"));
        }

        [UnityTest] public IEnumerator FirstMatchHint_WinsThePause_AndTheDriveDoesNotCarry()
        {
            // B (PO, 28 nov 15:21): injection tests play with autopilot off.
            StartLoose(5, AiLevel.Easy, 0, ShotPower.Medium, autopilot: false);
            FirstMatchHints.ForceWindowForTests = true;
            FirstMatchHints.Now = () => 100;
            hud.Hints.BeginMatch();
            hud.Hints.ForceShownForTests(HintId.Volley);
            Assert.That(ReachOrdinary(), Is.True);
            yield return Inject(LooseDriveKind.Short, .8f, 5.4f, RallyHud.BannerInSeconds);
            Assert.That(Find<Label>("rally-banner-hint-text").text, Is.EqualTo(FirstMatchHints.Text(HintId.Volley, HintWhere.Banner)));
            Assert.That(hud.Drive.Shown, Is.EqualTo(LooseDriveKind.None));
            Assert.That(Find("rally-banner-hint-icon").style.display.value, Is.Not.EqualTo(DisplayStyle.None));
            Assert.That(LeavePause(), Is.True);
            var lines = DriveLines();
            Assert.That(lines.Length, Is.EqualTo(1));
            Assert.That(lines[0].reason, Is.EqualTo("firstMatchHint"));
            Assert.That(lines[0].seen, Is.False);
            Assert.That(lines[0].kind, Is.EqualTo("Short"));
            hud.Hints.ForceShownForTests(null);
            FirstMatchHints.Enabled = false;
            Assert.That(ReachOrdinary(), Is.True);
            Assert.That(hud.Drive.Shown, Is.EqualTo(LooseDriveKind.None));
            Assert.That(DriveSentence(RowText()), Is.False, "the drive hint does not carry into the next pause");
            Assert.That(DriveLines().Length, Is.EqualTo(1), "the next pause was not a new qualified point");
        }

        [UnityTest] public IEnumerator NeverOutsideAnOrdinaryPause_AndOffShowsAndWritesNothing()
        {
            // C (PO, 28 nov 15:21): autopilot on until a human-serve pause, then off; require at least one such pause.
            StartLoose(6, AiLevel.Easy, 0, ShotPower.Medium);
            hud.Drive.QualifyForTests(LooseDriveKind.Short, .8f, 5.4f);
            Refresh();
            bool ever = false, wasHumanServe = false, turnedOff = false;
            int frames = 0, servePauses = 0;
            for (int i = 0; i < 400000 && rally.Phase != RallyPhase.MatchOver; i++)
            {
                Step();
                frames++;
                bool humanServe = rally.Phase == RallyPhase.PointOver && rally.HumanServePause;
                if (humanServe && !wasHumanServe) servePauses++;
                wasHumanServe = humanServe;
                if (servePauses >= 1 && !turnedOff)
                {
                    rally.Autopilot = false;
                    turnedOff = true;
                }
                bool driveText = DriveSentence(RowText());
                if (driveText) ever = true;
                if (!Ordinary()) Assert.That(driveText, Is.False, "drive text in " + rally.Phase + " humanServe=" + rally.HumanServePause);
                if (i % 8 == 0) yield return null;
            }
            Assert.That(frames, Is.GreaterThan(100));
            Assert.That(servePauses, Is.GreaterThanOrEqualTo(1), "never walked a human-serve pause, so the walk is vacuous");
            Assert.That(ever, Is.True, "the injected hint never appeared, so the walk is vacuous");
            int written = DriveLines().Length;

            LooseDriveHint.ResetForTests();
            LooseDriveHint.ForceForTests = true;
            LooseDriveHint.Enabled = false;
            rally.StopMode();
            flow.ClosePractice();
            rally.Autopilot = false;
            rally.AutopilotTimingSigmaMs = 0;
            rally.Level = AiLevel.Easy;
            rally.Seed = 6;
            rally.RandomStyles = false;
            rally.StyleOverride = RivalStyleId.Fondista;
            rally.AutopilotPower = ShotPower.Medium;
            Assert.That(rally.StartMatch(), Is.True);
            int pauses = 0;
            bool inPause = false;
            for (int i = 0; i < 400000 && pauses < 3 && rally.Phase != RallyPhase.MatchOver; i++)
            {
                Step();
                if (Ordinary())
                {
                    if (!inPause)
                    {
                        inPause = true;
                        pauses++;
                        hud.Drive.QualifyForTests(LooseDriveKind.Wide, 3.2f, 8f);
                        Refresh();
                    }
                }
                else inPause = false;
                Assert.That(DriveSentence(RowText()), Is.False);
                Assert.That(hud.Drive.Shown, Is.EqualTo(LooseDriveKind.None));
                if (i % 8 == 0) yield return null;
            }
            Assert.That(pauses, Is.GreaterThanOrEqualTo(1));
            Assert.That(DriveLines().Length, Is.EqualTo(written), "Enabled false writes nothing");
        }

        // Criterion 6. Same rally inputs as front-2 criterion 9. ForceForTests is on in both runs; only Enabled changes.
        // O3: the on run must actually show the hint. The seed and the cuts stay as they are. No positive means fail.
        private sealed class Trace
        {
            public readonly List<string> Points = new List<string>(), Waits = new List<string>(), Starts = new List<string>();
            public int LooseDrives;
            public bool HintShown;
        }
        private Trace RunCompare(bool enabled)
        {
            LooseDriveHint.Enabled = enabled;
            LooseDriveHint.ForceForTests = true;
            rally.StopMode();
            var trace = new Trace();
            long steps = 0;
            rally.Autopilot = true;
            rally.AutopilotTimingSigmaMs = 25;
            rally.Level = AiLevel.Easy;
            rally.Seed = 5;
            rally.RandomStyles = false;
            rally.StyleOverride = RivalStyleId.Fondista;
            rally.SetParams(new RallyParams { ServeKindOn = 1 });
            bool have = false, volley = false, side = false;
            float power = 0, aimY = 0;
            int rivals = 0;
            long contact = 0;
            Vector3? bounce = null;
            Action<ShotRecord> onShot = r =>
            {
                bool applied = r.Applied && (r.Result == nameof(ShotResult.Perfect) || r.Result == nameof(ShotResult.Valid));
                if (!applied) return;
                have = true;
                volley = controller.LastOpportunity == Opportunity.Volley;
                side = controller.LastStrokeSideAimed;
                power = r.Power;
                aimY = r.AimY;
                rivals = 0;
                bounce = null;
                contact = r.ContactStep;
            };
            Action<StrokeSide, bool> onSwing = (_, made) =>
            {
                if (!made || !have) return;
                rivals++;
                if (rivals == 1) bounce = FirstFloorAfter(contact);
            };
            Action<PointResult> ended = r =>
            {
                trace.Points.Add(r.Winner + "/" + r.Cause + "/" + r.Shots + "/" + r.Reception);
                trace.Waits.Add(rally.WaitBetweenPoints.ToString("R"));
                var kind = LooseDrive.Judge(
                    have, volley, side, power, aimY, (float)rally.Params.ServeKindLobMinAimY,
                    have, rivals, r.Winner, r.Cause, r.Reception, bounce, CourtDimensions.Standard(),
                    rally.HumanServePause, rally.Phase == RallyPhase.MatchOver, rally.Match != null,
                    rally.Autopilot, true, true);
                if (kind != LooseDriveKind.None) trace.LooseDrives++;
                have = false;
                rivals = 0;
                bounce = null;
            };
            Action started = () => trace.Starts.Add(steps + "/" + rally.LastServeType + "/" + rally.Match.Server);
            controller.ShotResolved += onShot;
            rally.RivalSwing += onSwing;
            rally.PointEnded += ended;
            rally.RallyStarted += started;
            try
            {
                Assert.That(rally.StartMatch(), Is.True);
                // D (PO, 28 nov 15:21): inject a qualified point in both runs; the off run ignores it by design.
                hud.Drive.QualifyForTests(LooseDriveKind.Short, .8f, 5.4f);
                Refresh();
                while (steps < 400000 && rally.PointsPlayed < 6 && rally.Phase != RallyPhase.MatchOver)
                {
                    app.Host.Advance(StepReal);
                    controller.ProcessPending();
                    rally.Tick();
                    scape.Tick((float)StepReal);
                    Refresh();
                    if (enabled && Ordinary() && DriveSentence(RowText())) trace.HintShown = true;
                    steps++;
                }
            }
            finally
            {
                controller.ShotResolved -= onShot;
                rally.RivalSwing -= onSwing;
                rally.PointEnded -= ended;
                rally.RallyStarted -= started;
            }
            return trace;
        }
        [UnityTest] public IEnumerator NoAddedTime_OnMatchesOff()
        {
            flow.ClosePractice();
            var on = RunCompare(true);
            var off = RunCompare(false);
            Assert.That(on.HintShown, Is.True, "the on run never showed the injected loose-drive hint, so the comparison is vacuous");
            Assert.That(on.Points.Count, Is.GreaterThanOrEqualTo(5));
            Assert.That(on.Starts.Count, Is.GreaterThanOrEqualTo(5));
            Assert.That(on.Points, Is.EqualTo(off.Points));
            Assert.That(on.Waits, Is.EqualTo(off.Waits));
            Assert.That(on.Starts, Is.EqualTo(off.Starts));
            yield return null;
        }

        [UnityTest] public IEnumerator DevButton_TogglesTheDriveHint()
        {
            for (int i = 0; i < 4; i++) { Refresh(); yield return null; }
            var button = root.Q<Button>("drive-hint-toggle");
            Assert.That(button, Is.Not.Null);
            Assert.That(button.text, Is.EqualTo("Pista del drive: sí"));
            Assert.That(LooseDriveHint.Enabled, Is.True);
            Click("drive-hint-toggle");
            Assert.That(LooseDriveHint.Enabled, Is.False);
            Assert.That(button.text, Is.EqualTo("Pista del drive: no"));
            Click("drive-hint-toggle");
            Assert.That(LooseDriveHint.Enabled, Is.True);
            Assert.That(button.text, Is.EqualTo("Pista del drive: sí"));
            var reset = root.Q<Button>("hints-reset");
            Assert.That(reset.text, Is.EqualTo("Reiniciar pistas"));
        }
    }
}
