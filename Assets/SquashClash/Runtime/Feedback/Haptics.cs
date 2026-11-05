using System.Runtime.InteropServices;
using SquashClash.Core.Shot;
namespace SquashClash.Runtime.Feedback
{
    public interface IHapticsBackend {void Prepare();void Impact(HapticStyle style);}
    // Static facade (plan r2 3.2; Impact takes HapticStyle because the PO approved two styles). Native only on iOS player builds.
    public static class Haptics
    {
        private sealed class NullBackend:IHapticsBackend {public void Prepare(){}public void Impact(HapticStyle style){}}
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void SquashHapticsPrepare();
        [DllImport("__Internal")] private static extern void SquashHapticsImpact(int style);
        private sealed class NativeBackend:IHapticsBackend {public void Prepare()=>SquashHapticsPrepare();public void Impact(HapticStyle style)=>SquashHapticsImpact((int)style);}
        private static IHapticsBackend DefaultBackend()=>new NativeBackend();
#else
        private static IHapticsBackend DefaultBackend()=>new NullBackend();
#endif
        private static IHapticsBackend backend=DefaultBackend();
        // Per-launch switch feedback.haptics, set by ShotController when a launch snapshot is taken.
        public static bool Enabled {get;set;}=true;
        // Test seam: install a recording double; ResetBackend restores the default.
        public static IHapticsBackend Backend {get=>backend;set=>backend=value??DefaultBackend();}
        public static void ResetBackend(){backend=DefaultBackend();Enabled=true;}
        public static void Prepare(){if(Enabled)backend.Prepare();}
        public static void Impact(HapticStyle style){if(Enabled)backend.Impact(style);}
    }
}
