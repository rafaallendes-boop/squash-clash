using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Rival volley pose (2026-11-20): the rival's figure shows the volley it plays (short backswing, contact at the planned height,
    // high = racket over the shoulder, low = open at mid height, side from the plan). Visual only: the same seed gives the same rally
    // with the pose on and off. Named to run with the other Z fixtures (see ZRallyDirectorTests).
    public sealed class ZRivalVolleyPoseTests : ZFirstRunFixture
    {
        private const float Frame=1f/60;
        [UnitySetUp] public IEnumerator Setup(){RivalVolleyPose.Enabled=true;return Boot(true);}
        [UnityTearDown] public IEnumerator Cleanup(){RivalVolleyPose.Enabled=true;return Shutdown();}
        private void StepOnce(){app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();}
        private bool Until(Func<bool> done,double seconds){int limit=(int)(seconds/StepReal);for(int i=0;i<limit&&!done();i++)StepOnce();return done();}
        private CharacterView Rival=>app.Players.Rival.Find("Figure").GetComponent<CharacterView>();
        private CharacterView RivalFigureAfterWarmup()
        {
            var figure=Rival;figure.enabled=false;return figure;
        }
        private static void Run(CharacterView figure,int frames){for(int i=0;i<frames;i++)figure.Animate(Frame);}
        // The racket head at contact (swing clock 0.04 s, inside the .07 s contact pose), in figure space.
        private Vector3 ContactHead(CharacterView figure,StrokeSide side,bool volley,float height)
        {
            figure.ShowRivalSwing(side,true,volley,height);Run(figure,3);return figure.RacketLocalPosition;
        }
        [UnityTest] public IEnumerator HighAndLowVolleys_DrawTheRacketAtTheirHeight_OverTheShoulderOrOpenAtMidHeight()
        {
            yield return null;yield return null;var figure=RivalFigureAfterWarmup();
            figure.ShowRivalSwing(StrokeSide.Drive,true,true,1.7f);
            Assert.That(figure.VolleyPose,Is.True);Assert.That(figure.HighVolley,Is.True,"1.7 m is a high volley");
            Run(figure,4);var high=figure.RacketLocalPosition;
            Assert.That(high.y,Is.GreaterThan(1.45f),"the racket is over the shoulder");
            figure.ShowRivalSwing(StrokeSide.Drive,true,true,.8f);
            Assert.That(figure.HighVolley,Is.False,"0.8 m is a low volley");Run(figure,60);
            figure.ShowRivalSwing(StrokeSide.Drive,true,true,.8f);Run(figure,4);var low=figure.RacketLocalPosition;
            Assert.That(low.y,Is.LessThan(1.15f),"the racket is open at mid height");
            Assert.That(high.y-low.y,Is.GreaterThan(.5f),"high and low volleys are told apart");
        }
        [UnityTest] public IEnumerator VolleySide_FollowsThePlan_ForehandAndBackhand()
        {
            yield return null;yield return null;var figure=RivalFigureAfterWarmup();
            var forehand=ContactHead(figure,StrokeSide.Drive,true,1.1f);Run(figure,80);
            var backhand=ContactHead(figure,StrokeSide.Backhand,true,1.1f);
            Assert.That(figure.RacketSign,Is.EqualTo(1f));Assert.That(forehand.x,Is.LessThan(0),"drive: the racket on -x (AX-1)");Assert.That(backhand.x,Is.GreaterThan(0),"backhand: +x");
        }
        [UnityTest] public IEnumerator Volley_PunchesShort_NotTheFullGroundStrokeFollowThrough()
        {
            yield return null;yield return null;var figure=RivalFigureAfterWarmup();
            figure.ShowRivalSwing(StrokeSide.Drive,true,false,0);Run(figure,18);var stroke=figure.RacketLocalPosition;Run(figure,80);
            figure.ShowRivalSwing(StrokeSide.Drive,true,true,1.1f);Assert.That(figure.VolleyPose,Is.True);Run(figure,18);var volley=figure.RacketLocalPosition;
            Assert.That(figure.VolleyPose,Is.True,"still inside the short volley swing");
            Assert.That(volley.y,Is.LessThan(stroke.y-.1f),"no big high finish: the volley follow-through is short");
        }
        [UnityTest] public IEnumerator GroundStroke_IsUnchanged_AndTheToggleTurnsThePoseOff()
        {
            yield return null;yield return null;var figure=RivalFigureAfterWarmup();
            figure.ShowRivalSwing(StrokeSide.Drive,true,false,0);Assert.That(figure.VolleyPose,Is.False,"a ground stroke never shows the volley pose");
            Run(figure,80);RivalVolleyPose.Enabled=false;
            figure.ShowRivalSwing(StrokeSide.Drive,true,true,1.7f);Assert.That(figure.VolleyPose,Is.False,"toggle off: the old swing");
            Run(figure,80);RivalVolleyPose.Enabled=true;
            figure.ShowRivalSwing(StrokeSide.Drive,true,true,1.7f);Assert.That(figure.VolleyPose,Is.True,"toggle on (default)");
        }
        [UnityTest] public IEnumerator DevToggle_IsOnByDefault_AndFlipsThePose()
        {
            yield return null;
            UnityEngine.Object.FindFirstObjectByType<SquashClash.DevPanel.DevPanelView>().Toggle();yield return null;
            var button=Find<Button>("rival-volley-pose");Assert.That(RivalVolleyPose.Enabled,Is.True,"default on");Assert.That(button.text,Is.EqualTo("Pose de volea del rival: sí"));
            Click("rival-volley-pose");
            Assert.That(RivalVolleyPose.Enabled,Is.False);Assert.That(button.text,Is.EqualTo("Pose de volea del rival: no"));
        }
        // The real game: the Definidor volleys; its figure gets the plan's side and height at that very swing.
        [UnityTest] public IEnumerator RealRivalVolley_ReachesTheFigure_WithTheHeightOfItsPlan()
        {
            yield return null;yield return null;
            app.Clock.Paused=false;rally.Autopilot=true;rally.AutoRestart=true;rally.Seed=7;rally.StyleOverride=RivalStyleId.Definidor;
            rally.AutopilotLine=ShotLine.Cross;rally.AutopilotPower=ShotPower.Strong;
            bool seen=false,armedBefore=false,figureVolley=false;float planned=0,figureHeight=0;StrokeSide side=StrokeSide.Drive;
            var figure=Rival;
            rally.RivalSwing+=(s,contact)=>{if(contact&&rally.RivalSwingVolley&&!seen){seen=true;side=s;planned=rally.RivalSwingHeight;figureVolley=figure.VolleyPose;figureHeight=figure.VolleyHeight;}};
            Assert.That(rally.StartMatch(),Is.True);
            Assert.That(Until(()=>{if(rally.RivalVolleyIntended)armedBefore=true;return seen;},900),Is.True,"the rival volleys");
            Assert.That(rally.RivalVolleys,Is.GreaterThan(0));Assert.That(figureVolley,Is.True,"the figure took the volley pose at the swing");
            Assert.That(figureHeight,Is.EqualTo(planned).Within(1e-4f));Assert.That(planned,Is.GreaterThan(0));
            Assert.That(armedBefore,Is.True,"the intent is known before the swing, for the short backswing");
        }
        // Same seed, same rally: the pose on and off give the identical rival positions, swings, points and volleys.
        private IEnumerator Play(bool pose,List<string> log)
        {
            RivalVolleyPose.Enabled=pose;yield return null;yield return null;
            app.Clock.Paused=false;rally.Autopilot=true;rally.AutoRestart=true;rally.Seed=7;rally.StyleOverride=RivalStyleId.Definidor;
            rally.AutopilotLine=ShotLine.Cross;rally.AutopilotPower=ShotPower.Strong;
            int step=0;rally.RivalSwing+=(s,c)=>log.Add($"swing {step} {s} {c}");
            Assert.That(rally.StartMatch(),Is.True);
            int limit=(int)(240/StepReal);
            for(int i=0;i<limit;i++)
            {
                step=i;StepOnce();Rival.Animate(Frame);
                var r=rally.Rival.Position;var h=rally.Human.Position;log.Add($"{i} {r.x:R} {r.y:R} {h.x:R} {h.y:R} {rally.PointsPlayed} {rally.HumanPoints}-{rally.RivalPoints} {rally.RivalVolleys}");
            }
        }
        [UnityTest] public IEnumerator SameSeed_GivesTheSameRally_WithThePoseOnAndOff()
        {
            var on=new List<string>();var off=new List<string>();
            yield return Play(true,on);
            yield return Shutdown();yield return Boot(true);
            yield return Play(false,off);
            Assert.That(on.Count,Is.GreaterThan(1000));Assert.That(rally.RivalVolleys,Is.GreaterThan(0),"the comparison includes rival volleys");
            Assert.That(off.Count,Is.EqualTo(on.Count));
            for(int i=0;i<on.Count;i++)Assert.That(off[i],Is.EqualTo(on[i]),"line "+i);
        }
    }
}
