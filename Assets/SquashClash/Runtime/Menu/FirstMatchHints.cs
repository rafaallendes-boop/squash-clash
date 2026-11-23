using System;
using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Menu
{
    public sealed class FirstMatchHints
    {
        public const string ServeKey = "sc.hints.serve", VolleyKey = "sc.hints.volley", BoastKey = "sc.hints.boast", BoastCrossKey = "sc.hints.boastCross", WindowKey = "sc.hints.window";
        public static bool Enabled = true;
        public static Func<double> Now = () => Time.unscaledTimeAsDouble;
        public static bool ForceWindowForTests;
        public static string[] AllKeys() => new[] { ServeKey, VolleyKey, BoastKey, BoastCrossKey, WindowKey };
        public static void Clear() { foreach (var key in AllKeys()) PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
        public static void ResetForTests() { }
        public static string Text(HintId id, HintWhere where) => "";
        public FirstMatchHints(AppBootstrap app, RallyDirector rally, ShotController controller) { }
        public bool InWindowMatch => false;
        public HintId? Shown => null;
        public void BeginMatch() { }
        public HintState State(HintId id) => HintState.Unseen;
        public void ArmForTests(HintId id) { }
        public void ForceShownForTests(HintId? id) { }
    }
}
