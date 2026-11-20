using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Phase=UnityEngine.InputSystem.TouchPhase;
namespace SquashClash.Runtime.Tests
{
    // Two-wall shot (design 2026-11-20-boast-predeclaration): the side-wall aim opens only in Rally/Match play and never while the serve is
    // chosen; the touch path, the reticle on the side wall with the front-wall point, and the controller's boast solve. Named Z* to run after
    // the timing fixtures.
    public sealed class ZBoastTests
    {
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;private GesturePreview preview;private TouchInputAdapter input;private Touchscreen screen;
        private double start,now;private float mm;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        private IEnumerator Boot(bool practiceOn)
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Boast fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);app.Tuning.Set("gesture.control",2);
            if(!practiceOn)app.Tuning.Set("practice.enabled",0);
            controller=app.GetComponent<ShotController>();rally=app.Rally;preview=app.GetComponent<GesturePreview>();app.LaunchCurrent();
            app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;
            input=app.GetComponent<TouchInputAdapter>();input.InputAreaOverride=new Rect(0,0,5000,5000);input.UiBlocker=_=>false;
            start=now=practiceOn?app.Host.LaunchRealTime:Math.Ceiling(Time.realtimeSinceStartupAsDouble)+1;input.ReadRealTime=()=>now;
            mm=ScreenMetrics.MmPerPx(Screen.dpi,(float)app.Tuning.Applied.Get("input.fallbackDpi"),out _);screen=InputSystem.AddDevice<Touchscreen>();
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(screen!=null)InputSystem.RemoveDevice(screen);SquashClash.Runtime.Visual.PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}
        private void Send(Vector2 at,Phase phase,double dt,float down=0)
        {now=start+dt;InputSystem.QueueStateEvent(screen,new TouchState{touchId=1,phase=phase,position=at+Vector2.down*(down/mm)},start+dt);InputSystem.Update();}

        [UnityTest] public IEnumerator SideAim_OpensOnlyInRallyOrMatchPlay_AndClosesWhileTheServeIsChosen()
        {
            yield return Boot(true);
            Assert.That(controller.SideAimOpen,Is.False,"practice: no side-wall aim");
            Assert.That(rally.StartMatch(),Is.True);Assert.That(controller.SideAimOpen,Is.True,"match play");
            controller.ServeAim=_=>true;Assert.That(controller.SideAimOpen,Is.False,"never while the human chooses the serve");
            controller.ServeAim=null;Assert.That(controller.SideAimOpen,Is.True);
            rally.StopMode();Assert.That(controller.SideAimOpen,Is.False,"back to practice");
        }
        [UnityTest] public IEnumerator SideWallTouch_WithTheAimOpen_IsASideAimOnTheSideWallReticle_AndClosedIsRejectedAsToday()
        {
            yield return Boot(false);
            var at=(Vector2)UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(0,2,3));
            Send(at,Phase.Began,0);Assert.That(preview.M24,Is.Not.Null);Assert.That(preview.M24.HasAim,Is.False,"closed: today's rejection");
            Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.Rejected));Send(at,Phase.Ended,.3);start+=1;
            controller.SideAimGate=()=>true;
            Send(at,Phase.Began,0);Assert.That(preview.M24.HasAim,Is.True);Assert.That(preview.M24.Side.Exists,Is.True);
            Assert.That(preview.M24.Side.Wall,Is.EqualTo(SurfaceId.RightWall));Assert.That(preview.M24.Side.Z,Is.EqualTo(3).Within(.02));Assert.That(preview.M24.Side.Y,Is.EqualTo(2).Within(.02));
            var reticle=app.GetComponentInChildren<AimReticleView>(true);
            Assert.That(reticle.Side.Exists,Is.True);Assert.That(reticle.transform.position.x,Is.EqualTo(.035f).Within(1e-4));Assert.That(reticle.transform.position.z,Is.EqualTo(3).Within(.02));
            Assert.That(reticle.IsInvalid,Is.False);
            Send(at,Phase.Moved,.1,12);Send(at,Phase.Ended,.3,12);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.Swing));
            // A front-wall touch afterwards puts the ring back on the front wall.
            start+=1;var front=(Vector2)UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(3.2f,2.2f,0));Send(front,Phase.Began,0);
            Assert.That(preview.M24.Side.Exists,Is.False);Assert.That(reticle.Side.Exists,Is.False);Assert.That(reticle.transform.rotation,Is.EqualTo(Quaternion.identity));
            yield return null;
        }
        [UnityTest] public IEnumerator SideOrder_IsSolvedByTheBoastSolver_AndTheFrontWallPointIsMarked()
        {
            yield return Boot(true);
            Assert.That(controller.Plan.Exists,Is.True);Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var star),Is.True);
            for(int i=0;i<1500&&app.Host.ObservedRealTime+StepReal<star-1e-10;i++)app.Host.Advance(StepReal);
            double left=star-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);now=star;
            var p=controller.Plan.PStar;var court=app.Host.ActiveConfig.Court;var wall=SideAim.NearWall(p.x,court);
            var side=new SideAim(wall,Mathf.Max(.5f,p.z-.8f),1.4f);
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,star,true,GestureOutcome.Swing,side.UV,.8f,app.Host.ActiveConfig.Source,"fixture",star,side:side));
            controller.ProcessPending();
            Assert.That(controller.LastRecord,Is.Not.Null);
            // The side order went to the boast solver: same wall and depth; the causal layer may lift the height, as for a front-wall aim.
            var asked=controller.LastBoast.Requested;
            Assert.That(asked.Exists&&asked.Wall==side.Wall&&asked.Z==side.Z,Is.True,"the side order went to the boast solver: "+asked);
            Assert.That(asked.Y,Is.GreaterThanOrEqualTo(side.Y));
            // Addendum 1: without a valid boast the stroke falls back to the front wall (UnsolvableSideOrder_... covers that path).
            Assert.That(controller.LastCausal.FellBack,Is.EqualTo(!controller.LastBoast.Solved));Assert.That(controller.LastSolution.Solved,Is.True);
            var reticle=app.GetComponentInChildren<AimReticleView>(true);
            if(controller.LastBoast.Solved)
            {
                Assert.That(reticle.Side,Is.EqualTo(side),"the ring on the side wall");
                Assert.That(controller.LastSolution.Reason,Does.StartWith("boast"));
                Assert.That(reticle.FeasibleVisible,Is.True);Assert.That(reticle.FeasibleAim,Is.EqualTo(controller.LastBoast.Front),"the front-wall point is marked");
            }
            else Assert.That(reticle.Aim,Is.EqualTo(controller.LastCausal.FallbackAim),"the ring on the front-wall shot that goes out");
            yield return null;
        }
        // Addendum 1: no valid boast at the contact sends the stroke to the front wall on the same side; the reticle shows that shot.
        [UnityTest] public IEnumerator UnsolvableSideOrder_FallsBackToTheFrontWall_AndTheReticleShowsTheShotThatGoesOut()
        {
            yield return Boot(true);
            Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var star),Is.True);
            for(int i=0;i<1500&&app.Host.ObservedRealTime+StepReal<star-1e-10;i++)app.Host.Advance(StepReal);
            double left=star-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);now=star;
            var p=controller.Plan.PStar;var court=app.Host.ActiveConfig.Court;var wall=SideAim.NearWall(p.x,court);
            var side=new SideAim(wall,p.z+.9f,1.4f);
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,star,true,GestureOutcome.Swing,side.UV,.8f,app.Host.ActiveConfig.Source,"fixture",star,side:side));
            controller.ProcessPending();
            Assert.That(controller.LastBoast.Solved,Is.False,"no valid boast behind the contact");
            Assert.That(controller.LastCausal.FellBack,Is.True);Assert.That(controller.LastSolution.Solved,Is.True,controller.LastSolution.Reason);
            Assert.That(controller.LastRecord.Result,Is.Not.EqualTo("SystemFault"),controller.LastRecord.Reason);
            var reticle=app.GetComponentInChildren<AimReticleView>(true);
            Assert.That(reticle.Side.Exists,Is.False,"the ring moves to the front wall");Assert.That(reticle.Aim,Is.EqualTo(controller.LastCausal.FallbackAim));
            var expected=BoastFallback.Aim(controller.LastRecord.ContactX==0?p:new Vector3(controller.LastRecord.ContactX,controller.LastRecord.ContactY,controller.LastRecord.ContactZ),wall,court);
            Assert.That(controller.LastCausal.FallbackAim,Is.EqualTo(expected));
            yield return null;
        }
    }
    public sealed class ZBoastDevTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        // Round two: on by default (M4 passed with new seeds).
        [UnityTest] public IEnumerator DevPanel_DosParedes_DefaultsOn_AndSwitchesTheRivalFromTheNextPoint()
        {
            for(int i=0;i<4;i++){Refresh();yield return null;}
            var button=root.Q<Button>("boast-rival-toggle");Assert.That(button,Is.Not.Null,"DEV panel has the «Dos paredes» toggle");
            Assert.That(rally.RivalBoastNextPoint,Is.True);Assert.That(button.text,Does.Contain("sí"));
            Click("boast-rival-toggle");Assert.That(rally.RivalBoastNextPoint,Is.False);Assert.That(button.text,Does.Contain("no"));
            Assert.That(rally.StartMatch(),Is.True);Assert.That(rally.Params.BoastRival,Is.EqualTo(0),"applied at the next point");
            Click("boast-rival-toggle");Assert.That(rally.RivalBoastNextPoint,Is.True);
        }
    }
}
