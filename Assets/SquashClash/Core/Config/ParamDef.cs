using System;
using System.Linq;

namespace SquashClash.Core.Config
{
    public readonly struct ChoiceOption
    {
        public readonly double Value;
        public readonly string Label;
        public ChoiceOption(double value, string label) { Value = value; Label = label; }
    }

    public sealed class ParamDef
    {
        public string Key { get; }
        public string Label { get; }
        public string Unit { get; }
        public string Group { get; }
        public ParamKind Kind { get; }
        public double Min { get; }
        public double Max { get; }
        public double Step { get; }
        public ApplyClass Apply { get; }
        private readonly ChoiceOption[] choices;
        public ChoiceOption[] Choices => (ChoiceOption[])choices.Clone();

        internal ParamDef(string key, string label, string unit, string group, ParamKind kind,
            double min, double max, double step, ApplyClass apply, params ChoiceOption[] options)
        {
            Key = key; Label = label; Unit = unit; Group = group; Kind = kind;
            Min = min; Max = max; Step = step; Apply = apply;
            choices = (ChoiceOption[])options.Clone();
        }

        // P1 exposes the requested cap but PhysX does not let this adapter enforce it.
        public bool OperativeFor(string backend) => Key != "physics.maxImpactsPerStep" || (backend != "P1" && backend != "P1-PhysX");

        internal string Error(double value, bool checkRange = true)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return Key + ": value must be finite.";
            if ((Kind == ParamKind.Int || Kind == ParamKind.Bool) && value != Math.Truncate(value))
                return Key + ": value must be an integer.";
            if (Kind == ParamKind.Choice && !choices.Any(option => option.Value == value))
                return Key + ": value is not an allowed option.";
            if (checkRange && (value < Min || value > Max)) return Key + ": value is outside its range.";
            return null;
        }
    }
}
