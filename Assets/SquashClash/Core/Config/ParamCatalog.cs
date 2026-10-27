using System;
using System.Collections.Generic;
using System.Linq;

namespace SquashClash.Core.Config
{
    public static class ParamCatalog
    {
        private static readonly IReadOnlyList<ParamDef> definitions = Array.AsReadOnly(new[]
        {
            new ParamDef("gesture.control", "Gesture control (experimental)", "", "First Shot / sling", ParamKind.Choice, 0, 1, 1, ApplyClass.Immediate, new ChoiceOption(0, "A′: flick"), new ChoiceOption(1, "S1-sling")),
            new ParamDef("aim.holdMs", "Quiet duration", "ms", "First Shot / sling", ParamKind.Float, 120, 150, 1, ApplyClass.Immediate),
            new ParamDef("aim.holdMaxSpeedMmPerS", "Quiet speed", "mm/s", "First Shot / sling", ParamKind.Float, 0, 100, 1, ApplyClass.Immediate),
            new ParamDef("aim.holdRadiusMm", "Quiet radius", "mm", "First Shot / sling", ParamKind.Float, 0, 5, 0.1, ApplyClass.Immediate),
            new ParamDef("aim.holdMaxGapMs", "Quiet maximum gap", "ms", "First Shot / sling", ParamKind.Float, 10, 100, 1, ApplyClass.Immediate),
            new ParamDef("aim.holdMinSamples", "Quiet samples", "", "First Shot / sling", ParamKind.Int, 3, 12, 1, ApplyClass.Immediate),
            new ParamDef("power.pullDeadZoneMm", "Pull dead zone", "mm", "First Shot / sling", ParamKind.Float, 0, 10, 0.1, ApplyClass.Immediate),
            new ParamDef("power.pullFullMm", "Full pull", "mm", "First Shot / sling", ParamKind.Float, 30, 40, 1, ApplyClass.Immediate),
            new ParamDef("gesture.slingCancelAboveMm", "Cancel above", "mm", "First Shot / sling", ParamKind.Float, 6, 30, 1, ApplyClass.Immediate),
            new ParamDef("gesture.slingCancelLateralMm", "Cancel lateral", "mm", "First Shot / sling", ParamKind.Float, 10, 40, 1, ApplyClass.Immediate),
            new ParamDef("gesture.slingCancelMinDepthMm", "Minimum cancel depth", "mm", "First Shot / sling", ParamKind.Float, 4, 15, 1, ApplyClass.Immediate),
            new ParamDef("gesture.slingPreferLeft", "Left hand declared", "", "First Shot / sling", ParamKind.Bool, 0, 1, 1, ApplyClass.Immediate),
            new ParamDef("aim.lockFlashMs", "Lock flash", "ms", "First Shot / sling", ParamKind.Float, 50, 400, 10, ApplyClass.Immediate),
            new ParamDef("gesture.slingTarget1X", "T1 screen X", "ratio", "First Shot / protocol targets", ParamKind.Float, 0.05, 0.95, 0.01, ApplyClass.Immediate),
            new ParamDef("gesture.slingTarget1Y", "T1 screen Y", "ratio", "First Shot / protocol targets", ParamKind.Float, 0.05, 0.95, 0.01, ApplyClass.Immediate),
            new ParamDef("gesture.slingTarget2X", "T2 screen X", "ratio", "First Shot / protocol targets", ParamKind.Float, 0.05, 0.95, 0.01, ApplyClass.Immediate),
            new ParamDef("gesture.slingTarget2Y", "T2 screen Y", "ratio", "First Shot / protocol targets", ParamKind.Float, 0.05, 0.95, 0.01, ApplyClass.Immediate),
            new ParamDef("gesture.slingTarget3X", "T3 screen X", "ratio", "First Shot / protocol targets", ParamKind.Float, 0.05, 0.95, 0.01, ApplyClass.Immediate),
            new ParamDef("gesture.slingTarget3Y", "T3 screen Y", "ratio", "First Shot / protocol targets", ParamKind.Float, 0.05, 0.95, 0.01, ApplyClass.Immediate),
            new ParamDef("gesture.slingTarget4X", "T4 screen X", "ratio", "First Shot / protocol targets", ParamKind.Float, 0.05, 0.95, 0.01, ApplyClass.Immediate),
            new ParamDef("gesture.slingTarget4Y", "T4 screen Y", "ratio", "First Shot / protocol targets", ParamKind.Float, 0.05, 0.95, 0.01, ApplyClass.Immediate),
            new ParamDef("gesture.slingTarget5X", "T5 screen X", "ratio", "First Shot / protocol targets", ParamKind.Float, 0.05, 0.95, 0.01, ApplyClass.Immediate),
            new ParamDef("gesture.slingTarget5Y", "T5 screen Y", "ratio", "First Shot / protocol targets", ParamKind.Float, 0.05, 0.95, 0.01, ApplyClass.Immediate),
            new ParamDef("aim.freezeOnRelease", "Freeze on release", "", "First Shot / aim", ParamKind.Bool, 0, 1, 1, ApplyClass.Immediate),
            new ParamDef("power.method", "Power method (experimental)", "", "First Shot / power", ParamKind.Choice, 0, 1, 1, ApplyClass.Immediate,
                new ChoiceOption(0, "A: temporal window"), new ChoiceOption(1, "B: decaying peak")),
            new ParamDef("power.peakSmoothingMs", "Peak smoothing", "ms", "First Shot / power", ParamKind.Float, 5, 80, 1, ApplyClass.Immediate),
            new ParamDef("power.peakDecayMs", "Peak decay", "ms", "First Shot / power", ParamKind.Float, 20, 300, 1, ApplyClass.Immediate),
            new ParamDef("aim.gainMPerMm", "Gain", "m/mm", "First Shot / aim", ParamKind.Float, 0.03, 0.3, 0.01, ApplyClass.Immediate),
            new ParamDef("aim.startHeight", "Start height", "m", "First Shot / aim", ParamKind.Float, 0.5, 4.5, 0.1, ApplyClass.Immediate),
            new ParamDef("aim.flickMmPerS", "Flick threshold", "mm/s", "First Shot / aim", ParamKind.Float, 50, 1000, 10, ApplyClass.Immediate),
            new ParamDef("aim.flickHysteresisMmPerS", "Flick hysteresis", "mm/s", "First Shot / aim", ParamKind.Float, 0, 200, 5, ApplyClass.Immediate),
            new ParamDef("aim.freezeLookbackMs", "Freeze lookback", "ms", "First Shot / aim", ParamKind.Float, 0, 100, 5, ApplyClass.Immediate),
            new ParamDef("aim.flickConfirmSamples", "Flick confirmation", "samples", "First Shot / aim", ParamKind.Int, 1, 4, 1, ApplyClass.Immediate),
            new ParamDef("power.windowMs", "Power window", "ms", "First Shot / power", ParamKind.Float, 60, 100, 5, ApplyClass.Immediate),
            new ParamDef("power.minCoverageMs", "Minimum coverage", "ms", "First Shot / power", ParamKind.Float, 10, 60, 5, ApplyClass.Immediate),
            new ParamDef("power.minSamples", "Minimum samples", "", "First Shot / power", ParamKind.Int, 2, 8, 1, ApplyClass.Immediate),
            new ParamDef("power.maxGapMs", "Maximum sample gap", "ms", "First Shot / power", ParamKind.Float, 10, 60, 1, ApplyClass.Immediate),
            new ParamDef("power.deadZoneMmPerS", "Soft threshold", "mm/s", "First Shot / power", ParamKind.Float, 0, 400, 10, ApplyClass.Immediate),
            new ParamDef("power.fullMmPerS", "Full power", "mm/s", "First Shot / power", ParamKind.Float, 300, 4000, 50, ApplyClass.Immediate),
            new ParamDef("power.curveExponent", "Power curve", "", "First Shot / power", ParamKind.Float, 0.5, 3, 0.1, ApplyClass.Immediate),
            new ParamDef("gesture.tapMaxMm", "Tap distance", "mm", "First Shot / gesture", ParamKind.Float, 1, 6, 0.1, ApplyClass.Immediate),
            new ParamDef("gesture.tapMaxMs", "Tap duration", "ms", "First Shot / gesture", ParamKind.Float, 100, 400, 10, ApplyClass.Immediate),
            new ParamDef("gesture.cancelRadiusMm", "Cancel radius", "mm", "First Shot / gesture", ParamKind.Float, 3, 12, 0.5, ApplyClass.Immediate),
            new ParamDef("input.fallbackDpi", "Nominal fallback DPI", "dpi", "First Shot / input", ParamKind.Float, 100, 600, 1, ApplyClass.Immediate),
            new ParamDef("pace.tempo", "Tempo", "", "Pace", ParamKind.Float, 0.3, 1, 0.01, ApplyClass.NextLaunch),
            new ParamDef("pace.launchScale", "Launch scale", "", "Pace", ParamKind.Float, 0.5, 1.5, 0.01, ApplyClass.NextLaunch),
            new ParamDef("physics.gravity", "Gravity", "m/s²", "Physics", ParamKind.Float, 5, 15, 0.01, ApplyClass.NextLaunch),
            new ParamDef("physics.stepHz", "Step rate", "Hz", "Physics", ParamKind.Choice, 60, 240, 60, ApplyClass.NextLaunch,
                new ChoiceOption(60, "60"), new ChoiceOption(120, "120"), new ChoiceOption(240, "240")),
            new ParamDef("physics.maxImpactsPerStep", "Impact cap", "", "Physics", ParamKind.Int, 1, 8, 1, ApplyClass.NextLaunch),
            new ParamDef("physics.floor.restitution", "floor restitution", "", "Materials", ParamKind.Float, 0, 1, 0.01, ApplyClass.NextLaunch),
            new ParamDef("physics.floor.friction", "floor friction", "", "Materials", ParamKind.Float, 0, 1, 0.01, ApplyClass.NextLaunch),
            new ParamDef("physics.front.restitution", "front restitution", "", "Materials", ParamKind.Float, 0, 1, 0.01, ApplyClass.NextLaunch),
            new ParamDef("physics.front.friction", "front friction", "", "Materials", ParamKind.Float, 0, 1, 0.01, ApplyClass.NextLaunch),
            new ParamDef("physics.side.restitution", "side restitution", "", "Materials", ParamKind.Float, 0, 1, 0.01, ApplyClass.NextLaunch),
            new ParamDef("physics.side.friction", "side friction", "", "Materials", ParamKind.Float, 0, 1, 0.01, ApplyClass.NextLaunch),
            new ParamDef("physics.back.restitution", "back restitution", "", "Materials", ParamKind.Float, 0, 1, 0.01, ApplyClass.NextLaunch),
            new ParamDef("physics.back.friction", "back friction", "", "Materials", ParamKind.Float, 0, 1, 0.01, ApplyClass.NextLaunch),
            new ParamDef("physics.backend", "Backend", "", "Physics", ParamKind.Choice, 0, 1, 1, ApplyClass.NextLaunch, new ChoiceOption(0, "P1-PhysX"), new ChoiceOption(1, "P2-Analytic")),
            new ParamDef("launcher.repeatSeconds", "Repeat interval", "s", "Launcher", ParamKind.Float, 0, 10, 0.1, ApplyClass.Immediate),
            new ParamDef("debug.slowMotion", "Slow motion", "", "Debug", ParamKind.Float, 0.1, 1, 0.1, ApplyClass.Immediate),
            new ParamDef("launcher.x", "Launch x", "m", "Launcher", ParamKind.Float, 0.3, 6.1, 0.1, ApplyClass.NextLaunch),
            new ParamDef("launcher.y", "Launch y", "m", "Launcher", ParamKind.Float, 0.1, 5, 0.1, ApplyClass.NextLaunch),
            new ParamDef("launcher.z", "Launch z", "m", "Launcher", ParamKind.Float, 0.3, 9.45, 0.1, ApplyClass.NextLaunch),
            new ParamDef("launcher.yawDeg", "Launch yawDeg", "deg", "Launcher", ParamKind.Float, -80, 80, 0.1, ApplyClass.NextLaunch),
            new ParamDef("launcher.pitchDeg", "Launch pitchDeg", "deg", "Launcher", ParamKind.Float, -30, 80, 0.1, ApplyClass.NextLaunch),
            new ParamDef("launcher.speed", "Launch speed", "m/s", "Launcher", ParamKind.Float, 2, 40, 0.1, ApplyClass.NextLaunch),
            new ParamDef("court.tinTop", "Tin", "m", "Court", ParamKind.Choice, 0.43, 0.48, 0.05, ApplyClass.NextLaunch,
                new ChoiceOption(0.48, "WSF"), new ChoiceOption(0.43, "PSA")),
            new ParamDef("camera.height", "Height", "m", "Camera", ParamKind.Float, 2, 8, 0.1, ApplyClass.Immediate),
            new ParamDef("camera.backOffset", "Back offset", "m", "Camera", ParamKind.Float, 0, 5, 0.1, ApplyClass.Immediate),
            new ParamDef("camera.lookAtHeight", "Look height", "m", "Camera", ParamKind.Float, 0, 3, 0.1, ApplyClass.Immediate),
            new ParamDef("camera.fovDeg", "Field of view", "deg", "Camera", ParamKind.Float, 40, 90, 1, ApplyClass.Immediate),
            new ParamDef("camera.mode", "Mode", "", "Camera", ParamKind.Choice, 0, 1, 1, ApplyClass.Immediate,
                new ChoiceOption(0, "fija"), new ChoiceOption(1, "seguimiento")),
            new ParamDef("camera.followDamping", "Follow damping", "s", "Camera", ParamKind.Float, 0, 1, 0.01, ApplyClass.Immediate),
            new ParamDef("camera.followMaxOffset", "Follow max offset", "m", "Camera", ParamKind.Float, 0, 3, 0.05, ApplyClass.Immediate),
            new ParamDef("aids.shadow", "Shadow", "", "Aids", ParamKind.Bool, 0, 1, 1, ApplyClass.Immediate),
            new ParamDef("aids.trail", "Trail", "", "Aids", ParamKind.Bool, 0, 1, 1, ApplyClass.Immediate),
            new ParamDef("aids.impactMarks", "Impact marks", "", "Aids", ParamKind.Bool, 0, 1, 1, ApplyClass.Immediate),
            new ParamDef("aids.prediction", "Prediction", "", "Aids", ParamKind.Bool, 0, 1, 1, ApplyClass.Immediate),
            new ParamDef("aids.trailSeconds", "Trail duration", "s", "Aids", ParamKind.Float, 0, 0.5, 0.01, ApplyClass.Immediate),
            new ParamDef("aids.ballVisualScale", "Ball visual scale", "", "Aids", ParamKind.Float, 1, 2.5, 0.1, ApplyClass.Immediate),
            new ParamDef("placeholders.visible", "Reference players", "", "Players", ParamKind.Bool, 0, 1, 1, ApplyClass.Immediate),
            new ParamDef("human.x", "Human x", "m", "Players", ParamKind.Float, 0.3, 6.1, 0.1, ApplyClass.Immediate),
            new ParamDef("human.z", "Human z", "m", "Players", ParamKind.Float, 0.3, 9.45, 0.1, ApplyClass.Immediate),
            new ParamDef("rival.x", "Rival x", "m", "Players", ParamKind.Float, 0.3, 6.1, 0.1, ApplyClass.Immediate),
            new ParamDef("rival.z", "Rival z", "m", "Players", ParamKind.Float, 0.3, 9.45, 0.1, ApplyClass.Immediate)
        });
        private static readonly Dictionary<string, ParamDef> byKey = definitions.ToDictionary(d => d.Key, StringComparer.Ordinal);
        public static IReadOnlyList<ParamDef> All => definitions;
        public static bool TryGet(string key, out ParamDef def)
        {
            if (key == null) { def = null; return false; }
            return byKey.TryGetValue(key, out def);
        }
    }
}
