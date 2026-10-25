using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Shot;
using UnityEngine;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests
{
    public sealed class AimReticleViewTests
    {
        [Test] public void Reticle_ShowsFrozenState_AndStaysAtFrozenPoint()
        {
            var go=new GameObject("Reticle test");
            try {var v=go.AddComponent<AimReticleView>();v.SetAim(new Vector2(2,2.2f),AimState.Frozen,CourtDimensions.Standard());Assert.That(v.State,Is.EqualTo(AimState.Frozen));Assert.That(v.Aim,Is.EqualTo(new Vector2(2,2.2f)));Assert.That(go.transform.position.x,Is.EqualTo(2));Assert.That(go.transform.position.y,Is.EqualTo(2.2f));Assert.That(go.GetComponentsInChildren<Collider>(),Is.Empty);}
            finally {Object.DestroyImmediate(go);}
        }
        [Test] public void Reticle_TinAndOutAreRed_UsingPhysicalFootprint()
        {
            var go=new GameObject("Reticle boundary test");
            try {var v=go.AddComponent<AimReticleView>();var court=CourtDimensions.Standard();foreach(var y in new[]{.48f,.485f,4.565f,4.9f}){v.SetAim(new Vector2(2,y),AimState.Aiming,court);Assert.That(v.IsInvalid,Is.True);Assert.That(v.Tint,Is.EqualTo(Color.red));}v.SetAim(new Vector2(2,2.2f),AimState.Aiming,court);Assert.That(v.IsInvalid,Is.False);Assert.That(v.Tint,Is.Not.EqualTo(Color.red));}
            finally {Object.DestroyImmediate(go);}
        }
    }
}
