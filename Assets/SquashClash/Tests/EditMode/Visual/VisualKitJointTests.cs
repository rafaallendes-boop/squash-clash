using NUnit.Framework;
using SquashClash.Runtime.Visual;
using UnityEngine;

namespace SquashClash.Core.Tests.Visual
{
    public sealed class VisualKitJointTests
    {
        // Arm lengths of CharacterView (UpperArm .27, Forearm .26) and a typical bend hint.
        private const float L1=.27f,L2=.26f;
        private static readonly Vector3 Root=new Vector3(.0609f,1.1968f,-.2781f),Hint=new Vector3(.3f,-1f,-.4f);

        [TestCase(0f)]
        [TestCase(5e-5f)]
        public void TargetOnTheRoot_KeepsTheChainWithinItsLength(float offset)
        {
            var target=Root+new Vector3(0,offset,0);
            var elbow=VisualKit.Joint(Root,target,L1,L2,Hint,out var end);
            Assert.That((end-Root).magnitude,Is.LessThanOrEqualTo(L1+L2),"end");
            Assert.That((elbow-Root).magnitude,Is.LessThanOrEqualTo(L1+L2),"elbow");
        }
    }
}
