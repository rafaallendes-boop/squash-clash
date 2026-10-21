using System;
using UnityEngine;

namespace SquashClash.Core.Diagnostics
{
    [Serializable]
    public sealed class BuildInfo
    {
        public string version, gitHash, profile, builtAtUtc, unityVersion;
        public bool gitDirty;

        public static BuildInfo FromJson(string json) => JsonUtility.FromJson<BuildInfo>(json);
        public string Label => $"{version}+{gitHash}{(gitDirty ? "*" : "")} {profile}";
    }
}
