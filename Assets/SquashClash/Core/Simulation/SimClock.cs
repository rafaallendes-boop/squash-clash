using System;
namespace SquashClash.Core.Simulation
{
    public sealed class SimClock
    {
        public const int MaxStepsPerFrame = 8;
        private double accumulated, slowMotion = 1;
        private bool singleStep;
        public int StepHz { get; private set; }
        public double Tempo { get; private set; }
        public bool Paused { get; set; }
        public bool MetricsInvalid { get; private set; }
        public long DroppedSteps { get; private set; }
        public long DroppedSinceRestart { get; private set; }
        public float Alpha => Math.Min((float)accumulated, 0.99999994f);
        public double DebugSlowMotion
        {
            get => slowMotion;
            set
            {
                if (!Finite(value) || value <= 0 || value > 1) throw new ArgumentOutOfRangeException(nameof(value));
                slowMotion = value;
                if (value < 1) MetricsInvalid = true;
            }
        }
        public SimClock(int stepHz, double tempo) => Configure(stepHz, tempo);
        public void Configure(int stepHz, double tempo)
        {
            if (stepHz <= 0) throw new ArgumentOutOfRangeException(nameof(stepHz));
            if (!Finite(tempo) || tempo <= 0) throw new ArgumentOutOfRangeException(nameof(tempo));
            StepHz = stepHz; Tempo = tempo; accumulated = 0; singleStep = false;
        }
        public void Restart() { accumulated = 0; singleStep = false; DroppedSinceRestart = 0; }
        // Rebase the existing accumulator when a stopped flight is restored at the real present.
        // This does not advance time, execute steps, or change discard counters.
        public void RestoreFraction(double fraction)
        {
            if(!Finite(fraction)||fraction<0||fraction>=1)throw new ArgumentOutOfRangeException(nameof(fraction));
            accumulated=fraction;
        }
        public void RequestSingleStep() => singleStep = true;
        public int Advance(double realDeltaSeconds)
        {
            if (!Finite(realDeltaSeconds) || realDeltaSeconds < 0) throw new ArgumentOutOfRangeException(nameof(realDeltaSeconds));
            if (singleStep) { singleStep = false; return 1; }
            if (Paused) return 0;
            double next = accumulated + realDeltaSeconds * Tempo * slowMotion * StepHz;
            if (!Finite(next) || next >= long.MaxValue) throw new ArgumentOutOfRangeException(nameof(realDeltaSeconds));
            long steps = (long)Math.Floor(next + 1e-9);
            accumulated = Math.Max(0, next - steps);
            long dropped = Math.Max(0, steps - MaxStepsPerFrame);
            DroppedSteps = checked(DroppedSteps + dropped);
            DroppedSinceRestart = checked(DroppedSinceRestart + dropped);
            return (int)Math.Min(steps, MaxStepsPerFrame);
        }
        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
