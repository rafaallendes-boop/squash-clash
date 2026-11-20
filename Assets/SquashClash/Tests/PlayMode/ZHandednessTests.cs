using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Device robustness, block C: "Mano: diestro / zurdo". Cosmetic only: the drawn player holds the racket in the left
    // hand. The gesture, the M2-4 aim, the physics, the timing and the rules do not read it.
    public sealed class ZHandednessTests : ZFirstRunFixture
    {
        private const float Frame=1f/60;
        private static void ResetHand(){Handedness.Left=false;PlayerPrefs.DeleteKey(Handedness.Key);}
        [UnitySetUp] public IEnumerator Setup(){ResetHand();yield return Boot(true);}
        [UnityTearDown] public IEnumerator Cleanup(){yield return Shutdown();ResetHand();}
        private static CharacterView Figure(Transform player)=>player.Find("Figure").GetComponent<CharacterView>();
        private IEnumerator StartMatch()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");Refresh();yield return null;
            Assert.That(rally.Match,Is.Not.Null,"the match must start");
        }
        // The wrist of an arm, in figure space: the forearm is a unit part stretched from the elbow to the wrist.
        private static Vector3 Wrist(Transform figure,string side){var forearm=figure.Find("Forearm"+side);return forearm.localPosition+forearm.localRotation*Vector3.up*forearm.localScale.y;}
        [UnityTest] public IEnumerator Menu_OffersTheHand_AndRemembersIt()
        {
            var button=Find<Button>("menu-hand");
            Assert.That(button.text,Is.EqualTo("Mano: diestro"),"right-handed unless chosen otherwise");Assert.That(Handedness.Left,Is.False);
            Click("menu-hand");Refresh();
            Assert.That(Handedness.Left,Is.True);Assert.That(button.text,Is.EqualTo("Mano: zurdo"));
            Assert.That(PlayerPrefs.GetInt(Handedness.Key,0),Is.EqualTo(1),"remembered between launches");
            Assert.That(button.worldBound.height,Is.GreaterThanOrEqualTo(44));
            Click("menu-hand");Refresh();
            Assert.That(Handedness.Left,Is.False);Assert.That(button.text,Is.EqualTo("Mano: diestro"));Assert.That(PlayerPrefs.GetInt(Handedness.Key,1),Is.EqualTo(0));
            yield return null;
        }
        // The player faces the front wall and the left wall is at +x (AX-1): the left hand is the arm on +x, whose parts
        // are named "...R" after the sign of x. In this fixture the planned contact is toward the right wall (-x).
        [UnityTest] public IEnumerator LeftHanded_TheHumanKeepsTheLeftHand_OnTheAlignedRestingHead_AndTheRivalDoesNotChange()
        {
            var player=app.Players.Human;var human=Figure(player);var rival=Figure(app.Players.Rival);human.enabled=rival.enabled=false;
            for(int i=0;i<40;i++){human.Animate(Frame);rival.Animate(Frame);}
            Assert.That(human.RacketSign,Is.LessThan(0),"fixture: the contact is planned on the player's right");
            Assert.That(app.Config.Court.Width,Is.GreaterThan(player.position.x),"fixture: the left wall is at +x");
            Assert.That(human.RacketHand,Is.EqualTo(1),"right-handed: the hand on the side of the contact, here the right one");
            var swing=app.GetComponent<SwingView>();var restingHead=swing.Rig.TransformPoint(swing.RacketLocalPosition);
            Assert.That(Vector3.Distance(human.transform.TransformPoint(human.RacketLocalPosition),restingHead),Is.LessThanOrEqualTo(.005f),"PO-authorized aligned Idle head, with the chosen hand still holding it");
            var rivalRacket=rival.RacketLocalPosition;int rivalHand=rival.RacketHand;
            Handedness.Left=true;
            for(int i=0;i<40;i++){human.Animate(Frame);rival.Animate(Frame);}
            Assert.That(human.RacketHand,Is.EqualTo(-1),"the left hand holds the racket");
            var grip=human.RacketLocalPosition;
            Assert.That(Vector3.Distance(human.transform.TransformPoint(grip),restingHead),Is.LessThanOrEqualTo(.005f),"switching hands keeps the resting head aligned with SwingView");
            Assert.That(Vector3.Distance(Wrist(human.transform,"R"),grip),Is.LessThan(Vector3.Distance(Wrist(human.transform,"L"),grip)),"the wrist of the +x arm is the one at the racket");
            Assert.That(human.GripGap,Is.LessThan(.02f),"the hand reaches the grip");
            Assert.That(rival.RacketHand,Is.EqualTo(rivalHand),"the rival is not the player");
            Assert.That(Vector3.Distance(rival.RacketLocalPosition,rivalRacket),Is.LessThan(.02f));
            yield return null;
        }
        [UnityTest] public IEnumerator LeftHanded_StrokesOnBothSidesStayInTheLeftHand_OnTheSwingViewPath()
        {
            Handedness.Left=true;
            var swing=app.GetComponent<SwingView>();var fx=app.GetComponent<StrokeFx>();var human=Figure(app.Players.Human);var rival=Figure(app.Players.Rival);
            yield return null;yield return null;
            swing.enabled=fx.enabled=human.enabled=rival.enabled=false;
            yield return StartMatch();
            int leftSide=0,rightSide=0;float worstPath=0,worstGap=0,closest=float.MaxValue;bool otherHand=false,inImpact=false;
            float dt=(float)StepReal;
            Drive(()=>
            {
                swing.Tick(dt);fx.Tick(dt);human.Animate(dt);
                if(human.RacketHand!=-1)otherHand=true;
                if(swing.Phase==SwingPhase.Impact)
                {
                    inImpact=true;if(swing.ShoulderSign<0)leftSide++;else rightSide++;
                    closest=Mathf.Min(closest,Vector3.Distance(human.RacketLocalPosition,swing.RacketLocalPosition+new Vector3(0,.9f,0)));
                    worstGap=Mathf.Max(worstGap,human.GripGap);
                }
                // Per stroke, as ZCharacterFeelTests measures it: the closest the drawn racket gets to the swing view's.
                else if(inImpact){inImpact=false;worstPath=Mathf.Max(worstPath,closest);closest=float.MaxValue;}
                return leftSide>60&&rightSide>60;
            },400);
            Assert.That(leftSide,Is.GreaterThan(0),"strokes on the left side were seen");Assert.That(rightSide,Is.GreaterThan(0),"strokes on the right side were seen");
            Assert.That(otherHand,Is.False,"the racket never changes hands");
            Assert.That(worstPath,Is.LessThan(.25f),"on every stroke the racket meets the swing view's path, on either side");
            Assert.That(worstGap,Is.LessThan(.12f),"the left hand stays on the grip, across the body too");
        }
        // Guard: nothing but the drawing reads the choice. The same seeded demo match ends up in the same state.
        [UnityTest] public IEnumerator RightHanded_StrokesOnBothSidesStayInTheRightHand_OnTheSwingViewPath()
        {
            Handedness.Left=false;
            var swing=app.GetComponent<SwingView>();var fx=app.GetComponent<StrokeFx>();var human=Figure(app.Players.Human);var rival=Figure(app.Players.Rival);
            yield return null;yield return null;
            swing.enabled=fx.enabled=human.enabled=rival.enabled=false;
            yield return StartMatch();
            int leftSide=0,rightSide=0;float worstPath=0,worstGap=0,closest=float.MaxValue;bool otherHand=false,inImpact=false;
            float dt=(float)StepReal;
            Assert.That(Drive(()=>
            {
                swing.Tick(dt);fx.Tick(dt);human.Animate(dt);
                if(human.RacketHand!=1)otherHand=true;
                if(swing.Phase==SwingPhase.Impact)
                {
                    inImpact=true;if(swing.ShoulderSign<0)rightSide++;else leftSide++;
                    closest=Mathf.Min(closest,Vector3.Distance(human.RacketLocalPosition,swing.RacketLocalPosition+new Vector3(0,.9f,0)));
                    worstGap=Mathf.Max(worstGap,human.GripGap);
                }
                else if(inImpact){inImpact=false;worstPath=Mathf.Max(worstPath,closest);closest=float.MaxValue;}
                return leftSide>60&&rightSide>60;
            },400),Is.True,"strokes on both sides must be observed");
            Assert.That(otherHand,Is.False,"the right hand (-x arm, AX-1) holds the racket on both sides");
            Assert.That(worstPath,Is.LessThan(.25f),"each stroke follows the swing view's path");
            Assert.That(worstGap,Is.LessThan(.12f),"the right hand reaches across the body too");
        }
        [UnityTest] public IEnumerator Handedness_ChangesNothingButTheDrawing()
        {
            long[] steps=new long[2];Vector3[] balls=new Vector3[2];int[] human=new int[2],rival=new int[2],shots=new int[2];long[] launches=new long[2];
            for(int run=0;run<2;run++)
            {
                if(run==1){yield return Shutdown();yield return Boot(true);}
                Handedness.Left=run==1;
                // The rival these points were written against (19 nov): a random style can draw the Definidor, whose rallies
                // against the autopilot outlast this budget. Handedness is what is tested, not the rival.
                rally.StyleOverride=SquashClash.Core.Rally.RivalStyleId.Fondista;
                yield return StartMatch();
                Assert.That(Drive(()=>rally.PointsPlayed>=3,400),Is.True);
                steps[run]=app.Host.Current.Step;balls[run]=app.Host.Current.Position;human[run]=rally.HumanPoints;rival[run]=rally.RivalPoints;
                shots[run]=rally.Rules.Shots;launches[run]=app.Host.LaunchId;
            }
            Assert.That(steps[1],Is.EqualTo(steps[0]));Assert.That(balls[1],Is.EqualTo(balls[0]));
            Assert.That(human[1],Is.EqualTo(human[0]));Assert.That(rival[1],Is.EqualTo(rival[0]));Assert.That(shots[1],Is.EqualTo(shots[0]));Assert.That(launches[1],Is.EqualTo(launches[0]));
        }
        // Performance ceiling: the left-handed figure costs no garbage per frame either.
        [UnityTest] public IEnumerator LeftHanded_Animate_AllocatesNothing()
        {
            Handedness.Left=true;
            var human=Figure(app.Players.Human);human.enabled=false;
            for(int i=0;i<10;i++)human.Animate(Frame);
            long before=System.GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<2000;i++)human.Animate(Frame);
            Assert.That(System.GC.GetAllocatedBytesForCurrentThread()-before,Is.EqualTo(0),"bytes for 2000 frames");
            Assert.That(human.RacketHand,Is.EqualTo(-1));
            yield return null;
        }
    }
}
