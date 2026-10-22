namespace SquashClash.Core.Spike
{
    public enum MeasurementStatus { Measured, NotApplicable, NotEvaluable }
    public enum EnergyStepPopulation { AllContacts, Corner, CornerWithImpact, CornerPersistentOnly }
    public readonly struct EnergyGainSample
    {
        public readonly MeasurementStatus Status;
        public readonly float RelativeGain, SpeedBefore, SpeedAfter;
        public readonly long Step;
        public EnergyGainSample(MeasurementStatus status, float gain = float.NaN, long step = -1, float speedBefore = float.NaN, float speedAfter = float.NaN)
        { Status = status; RelativeGain = gain; Step = step; SpeedBefore = speedBefore; SpeedAfter = speedAfter; }
    }
}
