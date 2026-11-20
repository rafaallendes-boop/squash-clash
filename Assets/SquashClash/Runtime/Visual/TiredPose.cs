namespace SquashClash.Runtime.Visual
{
    // Fatigue feel, section 3 (design 2026-11-20): low energy readable on the figure. View only: CharacterView is its one reader; the
    // simulation, the bodies, the rules and the energy never know it exists. DEV toggle, off by default.
    public static class TiredPose
    {
        public static bool Enabled;
        // Below the red zone of the energy bar (FatigueSettings.RedBelow).
        public const double From=25;
        public const float HipDrop=.07f,PitchDeg=8f,BreathAmplitude=.022f;
        // 0 at or above 25 energy (or off), 1 at energy 0, linear between.
        public static float Amount(bool enabled,double energy)=>!enabled||!(energy<From)?0:(float)System.Math.Min(1,(From-System.Math.Max(0,energy))/From);
    }
}
