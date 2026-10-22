using UnityEngine;
namespace SquashClash.Runtime.Physics
{
    public sealed class PhysXContactCollector : MonoBehaviour
    {
        internal PhysXBallSimulation Owner;
        private void OnCollisionEnter(Collision collision) => Owner?.Record(collision, false);
        private void OnCollisionStay(Collision collision) => Owner?.Record(collision, true);
    }
}
