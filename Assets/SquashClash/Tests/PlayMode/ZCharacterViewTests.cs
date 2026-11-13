using System.Collections;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Art pass. Named to run last (see ZRallyDirectorTests): new fixtures stay out of the way of the clock-sensitive rollback test.
    public sealed class ZCharacterViewTests
    {
        private GameObject root;private PlaceholderPlayers players;
        [SetUp] public void SetUp()
        {
            root=new GameObject("Figure test");players=root.AddComponent<PlaceholderPlayers>();
            players.Apply(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
        }
        [UnityTearDown] public IEnumerator TearDown(){Object.Destroy(root);yield return null;}
        private CharacterView Figure(Transform player)=>player.Find("Figure").GetComponent<CharacterView>();

        [UnityTest] public IEnumerator BothPlayers_AreDrawnAsFigures_WithoutCollidersRigsOrRealtimeShadows()
        {
            yield return null;
            Assert.That(Figure(players.Human).IsHuman,Is.True);Assert.That(Figure(players.Rival).IsHuman,Is.False);
            foreach(var player in new[]{players.Human,players.Rival})
            {
                Assert.That(player.GetComponent<MeshRenderer>().forceRenderingOff,Is.True,"the reference capsule is kept but not drawn");
                var parts=player.Find("Figure").GetComponentsInChildren<MeshRenderer>(true);
                Assert.That(parts.Length,Is.GreaterThanOrEqualTo(14));
                Assert.That(parts.All(p=>p.shadowCastingMode==ShadowCastingMode.Off&&!p.receiveShadows),Is.True);
                Assert.That(parts.Count(p=>p.sharedMaterial==VisualKit.Toon),Is.EqualTo(parts.Length-1),"one shared opaque material plus the contact shadow");
                Assert.That(player.GetComponentsInChildren<Collider>(true),Is.Empty);Assert.That(player.GetComponentsInChildren<Rigidbody>(true),Is.Empty);
                Assert.That(player.GetComponentsInChildren<Animator>(true),Is.Empty);
                var bounds=parts.Where(p=>p.name!="Contact shadow"&&p.name!="Racket").Select(p=>p.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});
                Assert.That(bounds.min.y,Is.GreaterThan(-.01f),"feet on the floor");Assert.That(bounds.max.y,Is.InRange(1.45f,1.85f),"about the reference height");
                Assert.That(bounds.size.x,Is.LessThan(1.1f),"no wider than a player with arms out");
            }
        }

        [UnityTest] public IEnumerator Stride_FollowsDistanceTravelled_AndFreezesWithoutTimeOrTravel()
        {
            yield return null;
            var figure=Figure(players.Rival);var thigh=figure.transform.Find("ThighL");
            float phase=figure.RunPhase;
            for(int i=0;i<30;i++){players.Rival.position+=new Vector3(.05f,0,0);figure.Animate(1f/60);}
            Assert.That(figure.RunPhase-phase,Is.EqualTo(30*.05f*2*Mathf.PI/1.5f).Within(1e-3f),"one stride cycle per 1.5 m");
            Assert.That(figure.Move,Is.GreaterThan(.9f));
            var rotation=thigh.localRotation;var position=thigh.localPosition;
            for(int i=0;i<10;i++)figure.Animate(0);
            Assert.That(thigh.localRotation,Is.EqualTo(rotation));Assert.That(thigh.localPosition,Is.EqualTo(position));
            players.Rival.position+=new Vector3(3,0,0);figure.Animate(1f/60);
            Assert.That(figure.RunPhase-phase,Is.EqualTo(30*.05f*2*Mathf.PI/1.5f).Within(1e-3f),"a teleport is not a stride");
        }

        [UnityTest] public IEnumerator Figures_FollowPlaceholdersVisibilityAndPlacement()
        {
            yield return null;
            players.Place(new Vector2(1,2),new Vector2(5,8));
            Assert.That(players.Human.Find("Figure").position,Is.EqualTo(new Vector3(1,0,2)));
            Assert.That(players.Rival.Find("Figure").position,Is.EqualTo(new Vector3(5,0,8)));
            players.Apply(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text).With("placeholders.visible",0));
            Assert.That(players.Human.Find("Figure").gameObject.activeInHierarchy,Is.False);
        }
    }
}
