using System;
using SquashClash.Core.Court;

namespace SquashClash.Core.Config
{
    // I2 snapshot. Ball/clock/launch members are added only in their authorized increments.
    public sealed class EffectiveConfig
    {
        public ParamSet Source { get; }
        public string Hash => Source.Hash;
        public string SimulationKey { get; }
        // CourtDimensions is mutable for geometry authoring; consumers receive their own copy.
        public CourtDimensions Court => CourtDimensions.WithTinTop((float)Source.Get("court.tinTop"));

        private EffectiveConfig(ParamSet source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            SimulationKey = source.HashOf("physics.", "court.");
        }
        public static EffectiveConfig From(ParamSet source) => new EffectiveConfig(source);
    }
}
