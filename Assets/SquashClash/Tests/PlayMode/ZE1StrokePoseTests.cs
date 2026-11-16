using System.Collections;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // E1: drawing only. Pose the figure without advancing a ball, director, controller or SwingView.
    public sealed class ZE1StrokePoseTests
    {
        private const float Frame=1f/60;
        private GameObject root;private PlaceholderPlayers players;private bool wasLeft;
        [SetUp] public void Setup()
        {
            wasLeft=Handedness.Left;Handedness.Left=false;
            root=new GameObject("E1 visual pose test");players=root.AddComponent<PlaceholderPlayers>();
            players.Apply(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
            foreach(var figure in root.GetComponentsInChildren<CharacterView>())figure.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){Object.Destroy(root);Handedness.Left=wasLeft;yield return null;}
        private CharacterView Figure(Transform player)=>player.Find("Figure").GetComponent<CharacterView>();
        private static void Set(CharacterView figure,string name,object value)=>typeof(CharacterView).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(figure,value);
        private static void Stroke(CharacterView figure,float contactSign)
        {
            Set(figure,"racketSign",contactSign);Set(figure,"swingClock",.07f);Set(figure,"strokeClock",0f);Set(figure,"strokePower",.6f);Set(figure,"strokeKind",StrokeKind.Rival);
            Set(figure,"racketPosition",new Vector3(contactSign*.62f,.95f,-.30f));
        }
        [UnityTest] public IEnumerator Rival_KeepsTheRightHand_OnBothContactSides()
        {
            var figure=Figure(players.Rival);
            foreach(float side in new[]{-1f,1f})
            {
                Stroke(figure,side);
                for(int i=0;i<20;i++)
                {
                    figure.Animate(Frame);
                    Assert.That(figure.RacketHand,Is.EqualTo(1),"the rival is right-handed even across the body");
                }
            }
            yield return null;
        }
        [UnityTest] public IEnumerator Rival_BackhandOpensOutward_WhileForehandFinishesAcross()
        {
            var figure=Figure(players.Rival);
            Stroke(figure,1);
            for(int i=0;i<20;i++)figure.Animate(Frame);
            Assert.That(figure.RacketLocalPosition.x,Is.GreaterThan(.20f),"backhand: from across the body out toward the contact side");
            Stroke(figure,-1);
            for(int i=0;i<20;i++)figure.Animate(Frame);
            Assert.That(figure.RacketLocalPosition.x,Is.GreaterThan(.10f),"forehand: from the right side toward the centre and across");
            yield return null;
        }
        [UnityTest] public IEnumerator HumanHandChoice_LeavesTheRivalRightHanded_WithoutAddingCollision()
        {
            var human=Figure(players.Human);var rival=Figure(players.Rival);
            foreach(bool left in new[]{false,true})
            {
                Handedness.Left=left;Stroke(rival,1);
                for(int i=0;i<20;i++){human.Animate(Frame);rival.Animate(Frame);}
                Assert.That(human.RacketHand,Is.EqualTo(left?-1:1));
                Assert.That(rival.RacketHand,Is.EqualTo(1),"human preference must never switch the rival's hand");
            }
            Assert.That(root.GetComponentsInChildren<Collider>(true),Is.Empty);
            Assert.That(root.GetComponentsInChildren<Rigidbody>(true),Is.Empty);
            yield return null;
        }
    }
}
