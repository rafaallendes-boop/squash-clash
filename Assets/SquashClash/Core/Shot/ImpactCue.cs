namespace SquashClash.Core.Shot
{
    public enum ImpactCuePhase { Hidden, Approaching, InValid, InAssist, AtIdeal, AfterIdeal, Closed }
    public readonly struct ImpactCueState
    {
        public ImpactCueState(ImpactCuePhase phase, float approach, double secondsToTarget, double deltaMs,
            bool inValidWindow, bool inAssistWindow, bool atOrAfterIdeal)
        { Phase=phase; Approach=approach; SecondsToTarget=secondsToTarget; DeltaMs=deltaMs;
          InValidWindow=inValidWindow; InAssistWindow=inAssistWindow; AtOrAfterIdeal=atOrAfterIdeal; }
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
        {
            double seconds=targetContactAtReal-nowReal, delta=(nowReal-targetContactAtReal)*1000;
            if(!mapped||attemptConsumed)return new ImpactCueState(ImpactCuePhase.Hidden,0,seconds,delta,false,false,false);
            // Compare inclusive boundaries in clock units, avoiding round-trip millisecond rounding.
            bool assist=nowReal>=targetContactAtReal-timing.AssistMs/1000&&nowReal<=targetContactAtReal+timing.AssistMs/1000;
            bool valid=timing.ValidLimitOn&&nowReal>=targetContactAtReal-timing.ValidMs/1000&&nowReal<=targetContactAtReal+timing.ValidMs/1000;
            bool atOrAfter=nowReal>=targetContactAtReal;
            double start=System.Math.Max(presentedReal,targetContactAtReal-ApproachMs/1000);
            float approach=atOrAfter?0:nowReal<=start?1:(float)(seconds/(targetContactAtReal-start));
            // A positive clock interval must remain positive even when converted to float.
            if(!atOrAfter&&approach==0)approach=float.Epsilon;
            var phase=nowReal==targetContactAtReal?ImpactCuePhase.AtIdeal:
                !atOrAfter?(assist?ImpactCuePhase.InAssist:valid?ImpactCuePhase.InValid:ImpactCuePhase.Approaching):
                assist||valid?ImpactCuePhase.AfterIdeal:ImpactCuePhase.Closed;
            return new ImpactCueState(phase,approach,seconds,delta,valid,assist,atOrAfter);
        }
    }
}
