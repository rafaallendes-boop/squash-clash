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
    // Backhand pose: drawing only. The backhand (the racket arm reaching across the body) must read as a backhand from
    // the court camera: side-on shoulders, the free arm opening backwards for balance. The forehand stays as E1 drew it.
    public sealed class ZBackhandPoseTests
    {
        private const float Frame=1f/60;
        private GameObject root;private PlaceholderPlayers players;private bool wasLeft;
        [SetUp] public void Setup()
        {
            wasLeft=Handedness.Left;Handedness.Left=false;
            root=new GameObject("backhand pose test");players=root.AddComponent<PlaceholderPlayers>();
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
        private static Vector3 Wrist(Transform figure,string side){var forearm=figure.Find("Forearm"+side);return forearm.localPosition+forearm.localRotation*Vector3.up*forearm.localScale.y;}
        private struct Peaks{public float Yaw,FreeBack,FreeAway,ElbowAhead;}
        // Strokes the figure and records the extremes over the stroke. The racket arm is "L" (-x) unless the figure holds it in the left hand.
        private static Peaks Measure(CharacterView figure,float contactSign,bool leftHand)
        {
            string racketSide=leftHand?"R":"L",freeSide=leftHand?"L":"R";
            Stroke(figure,contactSign);
            var peaks=new Peaks{ElbowAhead=float.MinValue};var torso=figure.transform.Find("Torso");
            for(int i=0;i<30;i++)
            {
                figure.Animate(Frame);
                peaks.Yaw=Mathf.Max(peaks.Yaw,Mathf.Abs(figure.Yaw));
                var free=Wrist(figure.transform,freeSide)-torso.localPosition;
                peaks.FreeBack=Mathf.Max(peaks.FreeBack,free.z);peaks.FreeAway=Mathf.Max(peaks.FreeAway,new Vector2(free.x,free.z).magnitude);
                peaks.ElbowAhead=Mathf.Max(peaks.ElbowAhead,torso.localPosition.z-figure.transform.Find("Forearm"+racketSide).localPosition.z);
            }
            return peaks;
        }
        [UnityTest] public IEnumerator Backhand_TurnsTheShouldersSideOn_MoreThanTheForehand()
        {
            var figure=Figure(players.Rival);
            var forehand=Measure(figure,-1,false);var backhand=Measure(figure,1,false);
            Assert.That(backhand.Yaw,Is.GreaterThanOrEqualTo(62f),"backhand: the shoulders turn side-on to the ball");
            Assert.That(forehand.Yaw,Is.LessThanOrEqualTo(45f),"control: the forehand keeps its E1 turn");
            Assert.That(backhand.Yaw-forehand.Yaw,Is.GreaterThanOrEqualTo(20f));
            yield return null;
        }
        [UnityTest] public IEnumerator Backhand_FreeArmOpensBackwardsForBalance_ForehandDoesNot()
        {
            var figure=Figure(players.Rival);
            var forehand=Measure(figure,-1,false);var backhand=Measure(figure,1,false);
            Assert.That(backhand.FreeBack,Is.GreaterThanOrEqualTo(.15f),"backhand: the free hand goes behind the hip");
            Assert.That(backhand.FreeAway,Is.GreaterThanOrEqualTo(.45f),"backhand: and well away from the torso, the arm opened");
            Assert.That(forehand.FreeBack,Is.LessThan(.15f),"control: the forehand free arm stays in front as in E1");
            yield return null;
        }
        [UnityTest] public IEnumerator Backhand_TheElbowLeadsTheRacket()
        {
            var figure=Figure(players.Rival);
            var backhand=Measure(figure,1,false);
            Assert.That(backhand.ElbowAhead,Is.GreaterThanOrEqualTo(.20f),"backhand: the elbow is in front of the body while the hand trails");
            yield return null;
        }
        [UnityTest] public IEnumerator LeftHandedHuman_DrawsTheMirroredBackhand_OnTheOtherContactSide()
        {
            var human=Figure(players.Human);Handedness.Left=true;
            // A left-hander's backhand is the contact on the right (-x): the same arm reaching across, mirrored.
            var backhand=Measure(human,-1,true);
            Assert.That(backhand.Yaw,Is.GreaterThanOrEqualTo(62f));
            Assert.That(backhand.FreeBack,Is.GreaterThanOrEqualTo(.15f));
            Assert.That(backhand.FreeAway,Is.GreaterThanOrEqualTo(.45f));
            Assert.That(backhand.ElbowAhead,Is.GreaterThanOrEqualTo(.20f));
            var forehand=Measure(human,1,true);
            Assert.That(forehand.Yaw,Is.LessThanOrEqualTo(45f),"control: the left-hander's forehand is the E1 one");
            yield return null;
        }
    }
}
