using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Rival serve ritual (spec 2026-11-24): a separate drawn ball dribbles inside the existing pause before the rival's serve. View only.
    public sealed class ZRivalServeRitualTests
    {
        private const float Hand=1.1f,Radius=.0285f;
        private static readonly Vector2 Box=new Vector2(3.2f,6.1f);
        private static Vector3 Serve=>new Vector3(Box.x,Hand,Box.y);
        private bool On(double pause,double elapsed,out Vector3 ball)=>RivalServeRitual.Sample(pause,elapsed,Box,Hand,Radius,out ball);
        [TearDown] public void Reset(){RivalServeRitual.Reset();}
        [Test] public void Lead_IsTheSumOfItsParts()
        {
            Assert.That(RivalServeRitual.Lead,Is.EqualTo(RivalServeRitual.Bounces*RivalServeRitual.BounceSeconds+RivalServeRitual.MoveSeconds+RivalServeRitual.StillSeconds).Within(1e-12));
            Assert.That(RivalServeRitual.Start(2.5),Is.EqualTo(1.15).Within(1e-12));
        }
        [Test] public void NotShownBeforeStart_ShownFromStart()
        {
            double start=RivalServeRitual.Start(2.5);
            Assert.That(On(2.5,start-.001,out var none),Is.False);Assert.That(none,Is.EqualTo(default(Vector3)));
            Assert.That(On(2.5,start,out var ball),Is.True);Assert.That(ball.y,Is.EqualTo(Hand).Within(1e-5f));
        }
        [Test] public void TouchesTheFloorAtQuarterAndThreeQuarters_AndIsAtHandAtStartAndHalf()
        {
            double start=RivalServeRitual.Start(2.5);
            Assert.That(On(2.5,start+.25,out var a),Is.True);Assert.That(a.y,Is.EqualTo(Radius).Within(1e-5f));
            Assert.That(On(2.5,start+.75,out var b),Is.True);Assert.That(b.y,Is.EqualTo(Radius).Within(1e-5f));
            Assert.That(On(2.5,start+.5,out var h),Is.True);Assert.That(h.y,Is.EqualTo(Hand).Within(1e-5f));
        }
        [Test] public void AboveTheFloorEverywhereElseInTheBounces_AndFastAtTheFloor()
        {
            double start=RivalServeRitual.Start(2.5);
            for(int i=0;i<100;i++)
            {
                double t=i*.01;if(Math.Abs(t-.25)<1e-6||Math.Abs(t-.75)<1e-6)continue;
                Assert.That(On(2.5,start+t,out var ball),Is.True);Assert.That(ball.y,Is.GreaterThan(Radius),"t="+t);Assert.That(ball.y,Is.LessThanOrEqualTo(Hand+1e-5f));
            }
            On(2.5,start+.25,out var floor);On(2.5,start+.27,out var floor2);On(2.5,start,out var hand);On(2.5,start+.02,out var hand2);
            Assert.That(Math.Abs(floor2.y-floor.y),Is.GreaterThan(Math.Abs(hand2.y-hand.y)),"faster near the floor than near the hand");
        }
        [Test] public void DribbleSpot_IsOnTheFreeHandSideAndTowardTheFrontWall()
        {
            double start=RivalServeRitual.Start(2.5);On(2.5,start+.25,out var ball);
            Assert.That(ball.x,Is.EqualTo(Box.x+RivalServeRitual.SideOffset).Within(1e-5f));Assert.That(ball.z,Is.EqualTo(Box.y-RivalServeRitual.FrontOffset).Within(1e-5f));
        }
        [Test] public void LastFloorTouch_IsWellBeforeTheServe_AndTheBallEndsStillOnTheServePoint()
        {
            double pause=2.5,start=RivalServeRitual.Start(pause);
            Assert.That(pause-(start+.75),Is.GreaterThanOrEqualTo(.35-1e-9));
            Vector3 previous=default;
            for(double e=pause-RivalServeRitual.StillSeconds;e<=pause+1e-12;e+=.005)
            {
                Assert.That(On(pause,Math.Min(e,pause),out var ball),Is.True,"e="+e);
                Assert.That(ball.x,Is.EqualTo(Serve.x).Within(1e-5f));Assert.That(ball.y,Is.EqualTo(Serve.y).Within(1e-5f));Assert.That(ball.z,Is.EqualTo(Serve.z).Within(1e-5f));
                if(e>pause-RivalServeRitual.StillSeconds+.01&&previous!=default)Assert.That(ball,Is.EqualTo(previous),"still");
                previous=ball;
            }
            On(pause,start+1.1,out var mid);Assert.That(mid.x,Is.GreaterThan(Box.x).And.LessThan(Box.x+RivalServeRitual.SideOffset),"moving between the spot and the serve point");
        }
        [Test] public void NeverShownAfterThePause_OrWithAShortPause_OrWhenDisabled()
        {
            Assert.That(On(2.5,2.5+.001,out _),Is.False);
            Assert.That(RivalServeRitual.Fits(1.6),Is.True);Assert.That(RivalServeRitual.Fits(1.0),Is.False);
            Assert.That(On(1.0,.9,out _),Is.False,"too short to fit");
            RivalServeRitual.Enabled=false;Assert.That(On(2.5,2.4,out _),Is.False);RivalServeRitual.Reset();Assert.That(RivalServeRitual.Enabled,Is.True);
        }

        // ---- real match ----
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;private double now;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        private IEnumerator Boot()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Ritual fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();rally=app.Rally;app.LaunchCurrent();
            app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;
            PointEndFreeze.Reset();now=1000;PointEndFreeze.Now=()=>now;
        }
        [UnityTearDown] public IEnumerator Cleanup(){PointEndFreeze.Reset();RivalServeRitual.Reset();return PlayModeTestWorld.ClearApp();}
        private void Step(RivalServeRitualView view)
        {
            app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();now+=StepReal;view.Refresh();
        }
        private void StartMatch()
        {
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Easy;rally.Seed=3;rally.AutoRestart=true;
            Assert.That(rally.StartMatch(),Is.True);
        }
        [UnityTest] public IEnumerator InARealMatch_TheRitualShowsInThePause_AndEndsAtTheServe()
        {
            yield return Boot();
            var view=UnityEngine.Object.FindFirstObjectByType<RivalServeRitualView>();Assert.That(view,Is.Not.Null,"wired in AppBootstrap");
            var ballView=UnityEngine.Object.FindFirstObjectByType<BallView>();
            StartMatch();
            int guard=0;while(!rally.RivalServePause&&guard++<300000)Step(view);
            Assert.That(rally.RivalServePause,Is.True,"the rival serves next");
            Assert.That(rally.WaitBetweenPoints,Is.EqualTo(2.5).Within(1e-9));
            bool everShowing=false,midChecked=false;double firstShown=-1;Vector3 last=default;long launch=app.Host.LaunchId;
            while(rally.Phase==RallyPhase.PointOver&&guard++<400000)
            {
                Step(view);
                if(rally.Phase!=RallyPhase.PointOver)break;
                if(view.Showing)
                {
                    if(!everShowing){everShowing=true;firstShown=rally.PauseElapsed;}
                    last=view.Ball.position;
                    if(!midChecked&&rally.PauseElapsed>2.0)
                    {
                        midChecked=true;Assert.That(ballView.Hidden,Is.True,"the real ball is not drawn");Assert.That(rally.RivalWaitingToServe,Is.True);
                        var drawn=app.Players.Rival.localPosition;var box=rally.RivalServeBox;
                        Assert.That(drawn.x,Is.EqualTo(box.x).Within(1e-4f));Assert.That(drawn.z,Is.EqualTo(box.y).Within(1e-4f));
                    }
                }
            }
            Assert.That(everShowing,Is.True,"the ritual ball was drawn during the pause");Assert.That(midChecked,Is.True);
            Assert.That(firstShown,Is.GreaterThanOrEqualTo(rally.WaitBetweenPoints-RivalServeRitual.Lead-StepReal-1e-9),"not before pause - lead");
            Assert.That(rally.Phase,Is.Not.EqualTo(RallyPhase.PointOver),"the serve started");Assert.That(app.Host.LaunchId,Is.EqualTo(launch+1));
            view.Refresh();Assert.That(view.Showing,Is.False);Assert.That(ballView.Hidden,Is.False);
            Assert.That(Vector3.Distance(app.Host.Current.Position,last),Is.LessThan(1e-4f),"the real ball starts where the ritual ball waited");
            Assert.That(view.Ball.gameObject.activeSelf,Is.False);
            foreach(var t in view.Ball.GetComponentsInChildren<Transform>(true))
            {
                Assert.That(t.GetComponent<Collider>(),Is.Null);Assert.That(t.GetComponent<Rigidbody>(),Is.Null);Assert.That(t.GetComponent<BallView>(),Is.Null);
            }
        }
        [UnityTest] public IEnumerator DuringTheRitual_TheRivalFigureStandsStill_FacingTheFrontWall()
        {
            yield return Boot();
            var view=UnityEngine.Object.FindFirstObjectByType<RivalServeRitualView>();Assert.That(view,Is.Not.Null);
            StartMatch();
            int guard=0;while(!rally.RivalServePause&&guard++<300000)Step(view);
            Assert.That(rally.RivalServePause,Is.True);
            while(rally.PauseElapsed<1.6&&rally.RivalServePause&&guard++<400000)Step(view);
            Assert.That(rally.RivalWaitingToServe,Is.True);
            var character=UnityEngine.Object.FindObjectsByType<CharacterView>(FindObjectsSortMode.None).First(c=>!c.IsHuman);
            for(int i=0;i<10;i++)character.Animate(1/60f);
            Assert.That(character.Yaw,Is.EqualTo(0).Within(1e-3f));Assert.That(character.Move,Is.EqualTo(0).Within(1e-6f));
        }

        // ---- the simulation does not know about it ----
        [Serializable] private sealed class Line {public string kind,payload;}
        // Fields that differ between ANY two boots (random match id, wall-clock offsets of the boot frames, planner cost), not between ritual on and off.
        private static string Mask(string payload)=>System.Text.RegularExpressions.Regex.Replace(payload,"\"(matchId|[A-Za-z]*CostMs|[A-Za-z]*Real[A-Za-z]*)\":(\"[^\"]*\"|[-0-9.eE+]+)","\"$1\":-");
        private IEnumerator Run(bool ritual,List<string> trace,List<string> metrics,int[] counts,bool[] flags)
        {
            yield return Boot();
            RivalServeRitual.Enabled=ritual;
            var view=UnityEngine.Object.FindFirstObjectByType<RivalServeRitualView>();Assert.That(view,Is.Not.Null);
            int contacts=0;app.Host.ContactRaised+=_=>contacts++;
            int before=File.ReadAllLines(app.Metrics.CurrentFilePath).Length;
            StartMatch();
            bool sawRivalPause=false;int after=-1,pauseSteps=0;
            for(int i=0;i<500000;i++)
            {
                Step(view);if(view.Showing)flags[0]=true;
                if(rally.Phase==RallyPhase.PointOver)pauseSteps++;
                var h=rally.Human.Position;var r=rally.Rival.Position;var p=app.Host.Current.Position;var v=app.Host.Current.Velocity;
                trace.Add(app.Host.Current.Step+"|"+app.Host.LaunchId+"|"+rally.Phase+"|"+h.x.ToString("R")+","+h.y.ToString("R")+"|"+r.x.ToString("R")+","+r.y.ToString("R")+"|"
                    +rally.HumanPoints+"-"+rally.RivalPoints+"|"+rally.Match.Server+"|"+p.x.ToString("R")+","+p.y.ToString("R")+","+p.z.ToString("R")+"|"
                    +v.x.ToString("R")+","+v.y.ToString("R")+","+v.z.ToString("R")+"|"+app.Clock.Paused);
                if(rally.RivalServePause)sawRivalPause=true;
                if(sawRivalPause&&rally.Phase!=RallyPhase.PointOver&&after<0)after=0;
                if(after>=0&&++after>40)break;
            }
            Assert.That(after,Is.GreaterThan(40),"a rival serve started and ran 40 steps");
            counts[0]=pauseSteps;counts[1]=contacts;
            metrics.AddRange(File.ReadAllLines(app.Metrics.CurrentFilePath).Skip(before).Select(JsonUtility.FromJson<Line>).Select(l=>l.kind+"|"+Mask(l.payload)));
        }
        [UnityTest] public IEnumerator Simulation_IsIdentical_WithTheRitualOnAndOff()
        {
            var onTrace=new List<string>();var onMetrics=new List<string>();var onCounts=new int[2];var onFlags=new bool[1];
            var offTrace=new List<string>();var offMetrics=new List<string>();var offCounts=new int[2];var offFlags=new bool[1];
            yield return Run(true,onTrace,onMetrics,onCounts,onFlags);
            yield return Run(false,offTrace,offMetrics,offCounts,offFlags);
            Assert.That(onFlags[0],Is.True,"the ON run really showed the ritual");Assert.That(offFlags[0],Is.False);
            Assert.That(onTrace.Count,Is.EqualTo(offTrace.Count));
            for(int i=0;i<onTrace.Count;i++)Assert.That(onTrace[i],Is.EqualTo(offTrace[i]),"step "+i);
            Assert.That(onCounts[0],Is.EqualTo(offCounts[0]),"same number of pause steps");Assert.That(onCounts[1],Is.EqualTo(offCounts[1]),"same contact events");
            Assert.That(onMetrics,Is.Not.Empty);Assert.That(onMetrics,Is.EqualTo(offMetrics),"same metrics kind+payload");
        }
    }
}
