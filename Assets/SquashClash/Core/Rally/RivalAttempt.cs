using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    // Visual attempt only: never changes a reception or the point's rules.
    public readonly struct RivalAttempt
    {
        public readonly long Step;
        public readonly StrokeSide Side;
        public RivalAttempt(long step,StrokeSide side){Step=step;Side=side;}
        public static RivalAttempt StepFor(RallyContext context,Trajectory trajectory,PointRules rulesAtStart,Vector2[] path)
            =>new RivalAttempt(-1,default);
    }
}
