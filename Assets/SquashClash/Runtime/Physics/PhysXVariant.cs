using System.Globalization;
using UnityEngine;
namespace SquashClash.Runtime.Physics
{
    public sealed class PhysXVariant
    {
        public CollisionDetectionMode Ccd = CollisionDetectionMode.ContinuousDynamic;
        public float ContactOffset = 0.001f;
        public int SolverIterations = 6, SolverVelocityIterations = 1;
        public string Name => (Ccd == CollisionDetectionMode.ContinuousDynamic ? "ccdDyn" : Ccd.ToString()) +
            "-co" + (ContactOffset * 1000).ToString("R", CultureInfo.InvariantCulture) + "mm-it" + SolverIterations + "-" + SolverVelocityIterations;
        public static PhysXVariant Baseline => new PhysXVariant();
        internal PhysXVariant Copy() => new PhysXVariant { Ccd = Ccd, ContactOffset = ContactOffset,
            SolverIterations = SolverIterations, SolverVelocityIterations = SolverVelocityIterations };
    }
}
