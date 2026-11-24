using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Phase=UnityEngine.InputSystem.TouchPhase;
namespace SquashClash.Runtime.Tests
{
    // Two-wall preview (design 2026-11-24-two-wall-preview-hint): a separate view that, while the finger rests on the side wall and pulls,
    // marks a dashed zone on the front wall at the boast forecast from P*; it hides on release, never shows with the frozen cross, only
    // reads the gesture and the controller, and recomputes on a bounded schedule. Named Z* to run after the timing fixtures.
    public sealed class ZBoastPreviewTests
    {
        private AppBootstrap app;private ShotController controller;private GesturePreview preview;private BoastPreviewView view;private TouchInputAdapter input;private Touchscreen screen;
        private double start,now;private float mm;
        private IEnumerator Boot()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Boast preview fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);app.Tuning.Set("gesture.control",2);
            controller=app.GetComponent<ShotController>();preview=app.GetComponent<GesturePreview>();view=app.GetComponent<BoastPreviewView>();app.LaunchCurrent();
            app.Host.enabled=app.enabled=controller.enabled=app.Rally.enabled=false;
            input=app.GetComponent<TouchInputAdapter>();input.InputAreaOverride=new Rect(0,0,5000,5000);input.UiBlocker=_=>false;
            start=now=app.Host.LaunchRealTime;input.ReadRealTime=()=>now;
            Assert.That(view,Is.Not.Null,"the app adds the preview view");view.Now=()=>now;
            mm=ScreenMetrics.MmPerPx(Screen.dpi,(float)app.Tuning.Applied.Get("input.fallbackDpi"),out _);screen=InputSystem.AddDevice<Touchscreen>();
            controller.SideAimGate=()=>true;
            Assert.That(controller.Available&&controller.Plan.Exists&&controller.Plan.TStarStep>=0,Is.True,"a planned contact to forecast from");
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(screen!=null)InputSystem.RemoveDevice(screen);yield return PlayModeTestWorld.ClearApp();}
        private void Send(Vector2 at,Phase phase,double dt,float down=0)
        {now=start+dt;InputSystem.QueueStateEvent(screen,new TouchState{touchId=1,phase=phase,position=at+Vector2.down*(down/mm)},start+dt);InputSystem.Update();}
        // A side-wall point ahead of the planned contact, on the wall nearest to it, low enough for a valid boast.
        private Vector2 SideTouch()
        {
            var p=controller.Plan.PStar;var court=app.Host.ActiveConfig.Court;var wall=SideAim.NearWall(p.x,court);
            var world=new Vector3(wall==SurfaceId.RightWall?0:court.Width,1.2f,Mathf.Clamp(p.z-1.5f,.8f,3f));
            return UnityEngine.Camera.main.WorldToScreenPoint(world);
        }
        private BoastInfo Expected()
        {
            BoastPreview.Estimate(controller.Plan.PStar,preview.M24.Side,preview.M24.P,app.Host.ActiveConfig,new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold),
                ShotIntentSolver.DefaultFootprintRadius,out var info);
            return info;
        }

        [UnityTest] public IEnumerator HeldSideTouch_MarksTheForecastZoneOnTheFrontWall_WithoutTheCross_AndReleaseHidesIt()
        {
            yield return Boot();var at=SideTouch();var reticle=app.GetComponentInChildren<AimReticleView>(true);
            Send(at,Phase.Began,0);view.Tick();
            Assert.That(preview.M24.Side.Exists,Is.True,"the touch is a side-wall aim");
            Assert.That(view.Visible,Is.False,"no pull yet: no stroke, no zone");
            Send(at,Phase.Moved,.1,12);view.Tick();
            Assert.That(preview.M24.P,Is.GreaterThan(0));
            var info=Expected();Assert.That(info.Solved,Is.True,"fixture: a valid boast from P* ("+info.Reason+")");
            Assert.That(view.Visible,Is.True,"held and pulling: the zone shows");
            Assert.That(view.Front,Is.EqualTo(info.Front),"at the forecast from P*, the unchanged boast solver");
            Assert.That(view.Center.x,Is.EqualTo(info.Front.x).Within(1e-4));Assert.That(view.Center.y,Is.EqualTo(info.Front.y).Within(1e-4));
            Assert.That(view.Center.z,Is.GreaterThan(0).And.LessThan(.1f),"on the front wall");
            // Not the cross: another shape (dashed oval) and another colour, and the cross is not shown while the finger rests.
            Assert.That(reticle.FeasibleVisible,Is.False,"the frozen cross appears only when the stroke is solved");
            Assert.That(view.DashCount,Is.GreaterThan(4));Assert.That(view.Tint,Is.Not.EqualTo(Color.white));
            Assert.That(view.HalfHeight,Is.GreaterThan(.12f),"a zone larger than the cross arms, not a point");
            // Discreet (SC-Lead, 24 nov): it must not play for the player. Thinner than the cross (0.035 m) and translucent.
            Assert.That(view.StrokeWidth,Is.LessThan(.035f),"thinner than the cross stroke");
            Assert.That(view.Tint.a,Is.LessThanOrEqualTo(.5f),"translucent");
            Assert.That(view.DashRenderers.Count,Is.EqualTo(view.DashCount));
            foreach(var d in view.DashRenderers){Assert.That(d.widthMultiplier,Is.EqualTo(view.StrokeWidth));for(int c=0;c<4;c++)Assert.That(d.startColor[c],Is.EqualTo(view.Tint[c]).Within(1.5f/255),"the renderer stores 8-bit colour");}
            Send(at,Phase.Ended,.3,12);view.Tick();
            Assert.That(view.Visible,Is.False,"released: the zone goes, the stroke's own reticle takes over");
            yield return null;
        }
        [UnityTest] public IEnumerator FrontWallTouch_NeverShowsTheZone()
        {
            yield return Boot();
            var front=(Vector2)UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(3.2f,2.2f,0));
            Send(front,Phase.Began,0);Send(front,Phase.Moved,.1,12);view.Tick();
            Assert.That(preview.M24.HasAim&&!preview.M24.Side.Exists,Is.True);Assert.That(preview.M24.P,Is.GreaterThan(0));
            Assert.That(view.Visible,Is.False);Assert.That(view.SolveCount,Is.EqualTo(0),"nothing computed for a front-wall aim");
            yield return null;
        }
        [UnityTest] public IEnumerator HeldStill_DoesNotRecompute_AndPullingRecomputesAtMostOncePerInterval()
        {
            yield return Boot();var at=SideTouch();
            Send(at,Phase.Began,0);Send(at,Phase.Moved,.1,8);view.Tick();
            Assert.That(view.SolveCount,Is.EqualTo(1),$"m24 active={preview.M24?.Active} aim={preview.M24?.HasAim} side={preview.M24?.Side} p={preview.M24?.P} phase={preview.M24?.Phase} consumed={controller.AttemptConsumed} visible={view.Visible}");
            for(int i=1;i<=20;i++){now=start+.1+i*.05;view.Tick();}
            Assert.That(view.SolveCount,Is.EqualTo(1),"finger still for a second: no recompute");
            // Pulling a little more every 20 ms for 0.2 s: at most one solve per interval.
            double t0=now-start;int before=view.SolveCount;
            for(int i=1;i<=10;i++){Send(at,Phase.Moved,t0+i*.02,8+i);view.Tick();}
            int allowed=1+(int)Math.Floor(.2/BoastPreviewSchedule.MinIntervalSeconds);
            Assert.That(view.SolveCount-before,Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(allowed));
            Assert.That(view.MaxCostMs,Is.GreaterThan(0));
            TestContext.WriteLine($"boast preview solve cost on this machine: last {view.LastCostMs:F2} ms, max {view.MaxCostMs:F2} ms over {view.SolveCount} solves");
            Send(at,Phase.Ended,t0+.3,18);view.Tick();
            yield return null;
        }
        [UnityTest] public IEnumerator TheView_OnlyReads_TheGestureAndTheController_AndWritesNoMetrics()
        {
            yield return Boot();var at=SideTouch();
            Send(at,Phase.Began,0);Send(at,Phase.Moved,.1,12);
            string path=app.Metrics?.CurrentFilePath;long length=path!=null&&File.Exists(path)?new FileInfo(path).Length:-1;
            var before=(preview.M24.Phase,preview.M24.P,preview.M24.Side,preview.M24.Aim,controller.AttemptConsumed,controller.LastRecord,controller.LastSolution.Reason,controller.LastBoast.Evaluations);
            for(int i=0;i<5;i++){now=start+.1+i*.2;view.Tick();}
            Assert.That(view.Visible,Is.True);
            var after=(preview.M24.Phase,preview.M24.P,preview.M24.Side,preview.M24.Aim,controller.AttemptConsumed,controller.LastRecord,controller.LastSolution.Reason,controller.LastBoast.Evaluations);
            Assert.That(after,Is.EqualTo(before));
            Assert.That(path!=null&&File.Exists(path)?new FileInfo(path).Length:-1,Is.EqualTo(length),"no metrics line");
            Send(at,Phase.Ended,.3,12);
            yield return null;
        }
    }
}
