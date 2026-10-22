using System;
using System.Collections.Generic;
using System.Linq;

namespace SquashClash.Core.Config
{
    public static class ParamCatalog
    {
        private static readonly IReadOnlyList<ParamDef> definitions = Array.AsReadOnly(new[]
        {
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
            new ParamDef("physics.backend", "Candidate backend", "", "Physics", ParamKind.Choice, 0, 0, 1, ApplyClass.NextLaunch, new ChoiceOption(0, "P1-PhysX")),
            new ParamDef("court.tinTop", "Tin", "m", "Court", ParamKind.Choice, 0.43, 0.48, 0.05, ApplyClass.NextLaunch,
                new ChoiceOption(0.48, "WSF"), new ChoiceOption(0.43, "PSA")),
            new ParamDef("camera.height", "Height", "m", "Camera", ParamKind.Float, 2, 8, 0.1, ApplyClass.Immediate),
            new ParamDef("camera.backOffset", "Back offset", "m", "Camera", ParamKind.Float, 0, 5, 0.1, ApplyClass.Immediate),
            new ParamDef("camera.lookAtHeight", "Look height", "m", "Camera", ParamKind.Float, 0, 3, 0.1, ApplyClass.Immediate),
            new ParamDef("camera.fovDeg", "Field of view", "deg", "Camera", ParamKind.Float, 40, 90, 1, ApplyClass.Immediate),
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
