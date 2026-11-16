using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using UnityEngine;

namespace SquashClash.Experiments.EditMode
{
    // First Shot 4.5: expected effective hash of every block condition, computed with the product code (ParamSet,
    // PresetCodec, PracticeLaunchDefinitions) from the real DefaultPreset.json and the P0-calib-1 backup.
    // It mirrors what the DEV panel and PracticeMode do on the phone: Tuning.Set (clamped to the catalog range),
    // practice.launch, and the body reassignment of PracticeMode.TryLaunchCurrent (only when the float differs).
    // Not part of regression or AutoQA. Read-only: it writes only under Logs/experiments/session45.
    public sealed class Session45HashExperiment
    {
        private static readonly CultureInfo C = CultureInfo.InvariantCulture;
        private static readonly (string id, int validLimit, int earlyPolicy)[] Modes = { ("V", 1, 0), ("T3", 0, 0), ("T3suave", 0, 1) };

        private static ParamSet defaultsSet;
        // TuningState.Set: the same validation, clamping and ShotConfigValidator check the DEV panel goes through.
        private static ParamSet Set(ParamSet p, string key, double value)
        {
            var tuning = new TuningState(defaultsSet, p);
            tuning.Set(key, value);
            return tuning.ApplyPending();
        }

        // PracticeMode.TryLaunchCurrent: the recipe positions the body only when the float value differs.
        private static ParamSet Launch(ParamSet p, int index)
        {
            p = Set(p, "practice.launch", index);
            var recipe = PracticeLaunchDefinitions.Create(EffectiveConfig.From(p))[index];
            if ((float)p.Get("human.x") != recipe.Body.PositionXZ.x) p = Set(p, "human.x", recipe.Body.PositionXZ.x);
            if ((float)p.Get("human.z") != recipe.Body.PositionXZ.y) p = Set(p, "human.z", recipe.Body.PositionXZ.y);
            return p;
        }

        [Test, Explicit, Category("Experiments")]
        public void ExpectedHashesPerCondition()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string p0Path = Environment.GetEnvironmentVariable("SC_P0_PATH");
            Assert.That(File.Exists(p0Path), Is.True, "SC_P0_PATH must point to the P0-calib-1 backup: " + p0Path);
            var offsets = (Environment.GetEnvironmentVariable("SC_LATENCY_MS") ?? "0").Split(',').Select(s => double.Parse(s.Trim(), C)).ToArray();

            var defaults = defaultsSet = PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(root, "Assets/SquashClash/Runtime/Resources/DefaultPreset.json")));
            Assert.That(PresetCodec.TryParse(File.ReadAllText(p0Path), out var p0File, out var error), Is.True, error);
            var warnings = new List<string>();
            var p0 = PresetCodec.Overlay(defaults, p0File, warnings);
            var sb = new StringBuilder();
            sb.Append("defaults\t").Append(defaults.Hash).Append('\t').Append(defaults.Ordered.Count).Append('\n');
            sb.Append("p0\t").Append(p0.Hash).Append("\twarnings=").Append(warnings.Count).Append('\n');
            sb.Append("p0.timing\tvalidLimit=").Append(p0.GetInt("timing.validLimit")).Append(" earlyPolicy=").Append(p0.GetInt("timing.earlyPolicy"))
              .Append(" aidLevel=").Append(p0.GetInt("timing.aidLevel")).Append(" latencyCompMs=").Append(p0.Get("timing.latencyCompMs").ToString("R", C))
              .Append(" practice.enabled=").Append(p0.GetInt("practice.enabled")).Append(" practice.launch=").Append(p0.GetInt("practice.launch")).Append('\n');
            sb.Append("offsetMs\tmode\tSL\tfreshFromP0\tround1\tround2\n");
            foreach (var offset in offsets)
            foreach (var mode in Modes)
            {
                var start = Set(Set(Set(Set(p0, "timing.latencyCompMs", offset), "timing.validLimit", mode.validLimit), "timing.earlyPolicy", mode.earlyPolicy), "timing.aidLevel", 0);
                // Two rounds SL1..SL6 in session order, plus each SL from a fresh P0 start. The body reassignment (float vs double)
                // makes the hash depend on the path: round2 is the steady value once every SL has been launched once.
                var seq = start; var round = new string[2, 6];
                for (int r = 0; r < 2; r++) for (int i = 0; i < 6; i++) { seq = Launch(seq, i); round[r, i] = seq.Hash; }
                for (int i = 0; i < 6; i++)
                {
                    string fresh = Launch(start, i).Hash;
                    sb.Append(offset.ToString("R", C)).Append('\t').Append(mode.id).Append("\tSL").Append(i + 1).Append('\t').Append(fresh).Append('\t').Append(round[0, i]).Append('\t').Append(round[1, i]).Append('\n');
                }
            }
            string outDir = Path.Combine(root, "Logs/experiments/session45");
            Directory.CreateDirectory(outDir);
            File.WriteAllText(Path.Combine(outDir, "expected-hashes.tsv"), sb.ToString(), new UTF8Encoding(false));
            Debug.Log("[Session45] " + sb);
        }
    }
}
