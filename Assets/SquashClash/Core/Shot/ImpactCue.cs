namespace SquashClash.Core.Shot
{
    public enum ImpactCuePhase { Hidden, Approaching, InValid, InAssist, AtIdeal, AfterIdeal, Closed }
    public readonly struct ImpactCueState
    {
        public ImpactCuePhase Phase { get; }
        public float Approach { get; }
        public double SecondsToTarget { get; }
        public double DeltaMs { get; }
        public bool InValidWindow { get; }
        public bool InAssistWindow { get; }
        public bool AtOrAfterIdeal { get; }
    }
    public static class ImpactCue
    {
        public const double ApproachMs = 500;
        public static ImpactCueState Evaluate(
            double nowReal, double presentedReal, double targetContactAtReal,
            TimingSettings timing, bool mapped, bool attemptConsumed)
        { return default; }
    }
}
