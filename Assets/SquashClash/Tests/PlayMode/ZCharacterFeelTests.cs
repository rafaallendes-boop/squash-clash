using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Feel pass, block 2: how the figures move and strike. Named to run last (see ZRallyDirectorTests).
    public sealed class ZCharacterFeelTests
    {
        private const float Frame=1f/60;
        private GameObject root;private PlaceholderPlayers players;
        private AppBootstrap app;private ShotController controller;private SwingView swing;private StrokeFx fx;private RallyDirector rally;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        private static CharacterView Figure(Transform player)=>player.Find("Figure").GetComponent<CharacterView>();
        private void Standalone()
        {
            root=new GameObject("Figure feel test");players=root.AddComponent<PlaceholderPlayers>();
            players.Apply(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
        }
        private IEnumerator Main()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Figure feel fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            controller=app.GetComponent<ShotController>();swing=app.GetComponent<SwingView>();fx=app.GetComponent<StrokeFx>();rally=app.Rally;
            yield return null;yield return null; // the figures find the app, the swing view and the stroke effects
            app.Host.enabled=app.enabled=controller.enabled=swing.enabled=fx.enabled=rally.enabled=false;
            Figure(app.Players.Human).enabled=Figure(app.Players.Rival).enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(root!=null){UnityEngine.Object.Destroy(root);root=null;yield return null;}
            yield return PlayModeTestWorld.ClearApp();
        }
        private void Run(CharacterView figure,Transform player,Vector3 velocity,int frames)
        {
            for(int i=0;i<frames;i++){player.position+=velocity*Frame;figure.Animate(Frame);}
        }
        private ShotRecord Strike(double deltaMs,float power)
        {
            app.LaunchCurrent();
            var aim=new Vector2(app.Config.Court.Width*.5f,1.7f);double release=double.NaN;bool queued=false;
            for(int i=0;i<(int)(12/StepReal);i++)
            {
                app.Host.Advance(StepReal);controller.ProcessPending();
                if(double.IsNaN(release)&&controller.Plan.Exists&&controller.Plan.TStarStep>=0&&app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out double t))
                    release=Math.Max(app.Host.LaunchRealTime,t+deltaMs/1000+app.Host.ActiveConfig.Source.Get("timing.latencyCompMs")/1000);
                if(!double.IsNaN(release)&&!queued&&app.Host.ObservedRealTime>=release)
                {
                    queued=true;
                    controller.Enqueue(new ClosedGesture(app.Host.LaunchId,"figure-"+app.Host.LaunchId,0,Math.Max(app.Host.LaunchRealTime,release-.15),release,true,GestureOutcome.Swing,
                        aim,power,controller.Snapshot.Reception,"test",app.Host.ObservedRealTime));
                }
                if(queued&&controller.PresentationCount>0)return controller.LastRecord;
            }
            Assert.Fail("the scripted stroke was never delivered");return null;
        }
        // Half a second of presentation after a delivery: the swing view, the effects and the figure, at 60 fps.
        private void Present(CharacterView figure,out float highest,out float widestYaw)
        {
            highest=0;widestYaw=0;
            for(int i=0;i<30;i++)
            {
                swing.Tick(Frame);fx.Tick(Frame);figure.Animate(Frame);
                highest=Mathf.Max(highest,figure.RacketLocalPosition.y);widestYaw=Mathf.Max(widestYaw,Mathf.Abs(figure.Yaw));
            }
        }

        [UnityTest] public IEnumerator Body_TurnsTowardWhereItRuns_AndBackpedalsFacingTheFrontWall()
        {
            Standalone();yield return null;
            var figure=Figure(players.Rival);var player=players.Rival;
            Run(figure,player,new Vector3(3,0,0),40);
            Assert.That(figure.Yaw,Is.LessThan(-40),"running toward +x the body turns that way");
            Run(figure,player,new Vector3(-3,0,0),40);
            Assert.That(figure.Yaw,Is.GreaterThan(40),"and turns around when the direction changes");
            Run(figure,player,new Vector3(0,0,-3),40);
            Assert.That(Mathf.Abs(figure.Yaw),Is.LessThan(10));Assert.That(figure.Backpedal,Is.LessThan(.1f),"toward the front wall it simply runs");
            Run(figure,player,new Vector3(0,0,3),40);
            Assert.That(figure.Backpedal,Is.GreaterThan(.8f),"away from the front wall it backpedals");
            Assert.That(Mathf.Abs(figure.Yaw),Is.LessThan(10),"still facing the front wall");
        }

        [UnityTest] public IEnumerator Stride_IsBoundedAtTopSpeed_AndShorterGoingBack()
        {
            Standalone();yield return null;
            var figure=Figure(players.Rival);var player=players.Rival;
            Run(figure,player,new Vector3(-3,0,0),30);float running=figure.FootSwing;float phase=figure.RunPhase;
            Run(figure,player,new Vector3(-4.8f,0,0),30);
            Assert.That(figure.FootSwing,Is.LessThanOrEqualTo(CharacterView.MaxFootSwing+1e-4f),"the foot never reaches further than the bound");
            Assert.That(figure.FootSwing,Is.EqualTo(running).Within(1e-3f),"top speed is a quicker cadence, not a longer step");
            Assert.That(figure.RunPhase-phase,Is.EqualTo(30*4.8f*Frame*2*Mathf.PI/CharacterView.MaxStride).Within(1e-2f));
            Run(figure,player,new Vector3(0,0,3),60);
            Assert.That(figure.FootSwing,Is.LessThan(running*.9f),"backpedalling takes shorter steps");
        }

        [UnityTest] public IEnumerator Human_DrawsItsOwnArmWithAnElbowAndTheRacket_AlongTheSwingViewPath()
        {
            yield return Main();
            var human=app.Players.Human;var figure=Figure(human);
            figure.Animate(Frame);
            foreach(var name in new[]{"UpperArmL","UpperArmR","ForearmL","ForearmR","Racket"})
                Assert.That(human.Find("Figure/"+name).gameObject.activeInHierarchy,Is.True,name+" is drawn by the figure");
            Assert.That(swing.Rig.GetComponentsInChildren<MeshRenderer>(true).All(r=>!r.enabled),Is.True,"the swing view's own two parts are no longer drawn");
            Assert.That(swing.Rig.gameObject.activeInHierarchy,Is.True,"but the swing view still runs");
            // An elbow: upper arm and forearm of the racket arm are not one straight segment while the racket rests near the body.
            string side=figure.RacketSign>0?"R":"L";
            var upperArm=human.Find("Figure/UpperArm"+side);var forearm=human.Find("Figure/Forearm"+side);
            Assert.That(Vector3.Angle(upperArm.up,forearm.up),Is.GreaterThan(15));
            Strike(0,.8f);
            float closest=float.MaxValue;
            for(int i=0;i<12;i++)
            {
                swing.Tick(Frame);fx.Tick(Frame);figure.Animate(Frame);
                if(swing.Phase==SwingPhase.Impact)closest=Mathf.Min(closest,Vector3.Distance(figure.RacketLocalPosition,swing.RacketLocalPosition+new Vector3(0,.9f,0)));
            }
            Assert.That(closest,Is.LessThan(.25f),"the figure's racket follows the swing view's racket path");
            Assert.That(figure.TrailVisible,Is.True,"the racket head leaves an arc while it swings");
        }

        [UnityTest] public IEnumerator FollowThrough_GrowsWithPower_AndAMissLooksDifferent_AndAllOfItFreezesOnPause()
        {
            yield return Main();
            var figure=Figure(app.Players.Human);
            Strike(0,.1f);Present(figure,out float softHeight,out _);
            for(int i=0;i<60;i++){swing.Tick(Frame);fx.Tick(Frame);figure.Animate(Frame);}
            Strike(0,1f);Present(figure,out float hardHeight,out float hitYaw);
            Assert.That(hardHeight,Is.GreaterThan(softHeight+.12f),"a hard stroke finishes higher than a soft one");
            for(int i=0;i<60;i++){swing.Tick(Frame);fx.Tick(Frame);figure.Animate(Frame);}
            Strike(170,1f);Assert.That(fx.HumanKind,Is.EqualTo(StrokeKind.Failed));
            for(int i=0;i<6;i++){swing.Tick(Frame);fx.Tick(Frame);figure.Animate(Frame);}
            // Paused mid-swing: the figure, its racket and its trail hold still for as long as the clock is stopped.
            var racket=figure.RacketLocalPosition;float yaw=figure.Yaw;bool trail=figure.TrailVisible;
            app.Clock.Paused=true;figure.enabled=true;
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(figure.RacketLocalPosition,Is.EqualTo(racket));Assert.That(figure.Yaw,Is.EqualTo(yaw));Assert.That(figure.TrailVisible,Is.EqualTo(trail));
            figure.enabled=false;app.Clock.Paused=false;
            Present(figure,out _,out float missYaw);
            Assert.That(missYaw,Is.GreaterThan(hitYaw+6),"a miss over-rotates past a clean stroke");
        }

        [UnityTest] public IEnumerator InARally_TheHumanPreparesWhenPlanted_AndSplitStepsWhenTheRivalHits()
        {
            yield return Main();
            var figure=Figure(app.Players.Human);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Medium;rally.Seed=5;
            Assert.That(rally.StartMode(),Is.True);
            bool rivalHit=false,split=false;float wound=0,raised=0;
            rally.RivalSwing+=(_,contact)=>{if(contact)rivalHit=true;};
            for(int i=0;i<(int)(40/StepReal)&&!(split&&wound>.4f);i++)
            {
                app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();
                if(i%4!=0)continue;
                swing.Tick(Frame);fx.Tick(Frame);figure.Animate(Frame);
                if(rally.Phase==RallyPhase.HumanReception&&rally.Human.Phase==BodyPhase.Set&&swing.Phase==SwingPhase.Idle){wound=Mathf.Max(wound,figure.WindUp);raised=Mathf.Max(raised,figure.RacketLocalPosition.y);}
                if(rivalHit&&figure.SplitStepping)split=true;
            }
            Assert.That(wound,Is.GreaterThan(.4f),"planted for the ball, the human winds the racket up before the swing");
            Assert.That(raised,Is.GreaterThan(1.15f),"racket above the shoulder");
            Assert.That(split,Is.True,"the human takes a split step when the rival strikes");
        }
    }
}
