using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
namespace SquashClash.AutoQA
{
    [System.Serializable] internal sealed class PhysicsWindow
    {
        public double kInMs,kOutMs,sampleMs;
        internal static PhysicsWindow Measure(Trajectory incoming,ParamSet config,double targetStep)
            =>new PhysicsWindow { kInMs=double.NegativeInfinity,kOutMs=double.PositiveInfinity };
    }
}
