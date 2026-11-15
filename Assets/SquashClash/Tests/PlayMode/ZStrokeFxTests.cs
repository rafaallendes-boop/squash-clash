using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Camera;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Feel pass, block 1: what a stroke looks like, by result and power. Named to run last (see ZRallyDirectorTests).
    public sealed class ZStrokeFxTests
    {
        private AppBootstrap app;private StrokeFx fx;private ImpactFx impacts;private CourtCameraRig rig;private ShotController controller;private UnityEngine.Camera view;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Stroke fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            fx=app.GetComponent<StrokeFx>();impacts=app.GetComponent<ImpactFx>();view=UnityEngine.Camera.main;rig=view.GetComponent<CourtCameraRig>();controller=app.GetComponent<ShotController>();
            Assert.That(fx,Is.Not.Null,"the app wires the stroke effects at startup");
            app.Host.enabled=app.enabled=controller.enabled=fx.enabled=impacts.enabled=rig.enabled=false;
            if(app.Rally!=null)app.Rally.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return PlayModeTestWorld.ClearApp();}
        // One practice reception struck by a scripted release at T* + deltaMs (the same route the rally autopilot uses).
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
                    controller.Enqueue(new ClosedGesture(app.Host.LaunchId,"stroke-"+app.Host.LaunchId,0,Math.Max(app.Host.LaunchRealTime,release-.15),release,true,GestureOutcome.Swing,
                        aim,power,controller.Snapshot.Reception,"test",app.Host.ObservedRealTime));
                }
                if(queued&&controller.PresentationCount>0)return controller.LastRecord;
            }
            Assert.Fail("the scripted stroke was never delivered");return null;
        }

        [UnityTest] public IEnumerator PerfectValidAndFailed_LookDifferent()
        {
            Assert.That(fx.ActiveQuads,Is.Zero);Assert.That(fx.HumanKind,Is.EqualTo(StrokeKind.None));
            var perfect=Strike(0,.95f);Assert.That(perfect.Result,Is.EqualTo("Perfect"));
            fx.Tick(.01f);
            Assert.That(fx.HumanKind,Is.EqualTo(StrokeKind.Perfect));Assert.That(fx.HumanPower,Is.EqualTo(perfect.Power).Within(1e-4f));
            int perfectQuads=fx.ActiveQuads;Assert.That(perfectQuads,Is.GreaterThanOrEqualTo(10),"two rings and a fan of sparks");
            Assert.That(fx.StreakVisible,Is.True,"a Perfect stroke leaves a streak behind the ball");
            fx.Tick(1);Assert.That(fx.ActiveQuads,Is.Zero);Assert.That(fx.StreakVisible,Is.False);

            var valid=Strike(-90,.5f);Assert.That(valid.Result,Is.EqualTo("Valid"));
            fx.Tick(.01f);
            Assert.That(fx.HumanKind,Is.EqualTo(StrokeKind.Valid));
            Assert.That(fx.ActiveQuads,Is.InRange(2,perfectQuads-4),"a Valid stroke is clearly smaller than a Perfect one");
            Assert.That(fx.StreakVisible,Is.False,"no streak for an ordinary Valid stroke");
            fx.Tick(1);

            var failed=Strike(170,.7f);Assert.That(failed.Result,Is.EqualTo("Failed"));
            fx.Tick(.01f);
            Assert.That(fx.HumanKind,Is.EqualTo(StrokeKind.Failed));Assert.That(fx.ActiveQuads,Is.Zero,"a failed swing has no contact burst");Assert.That(fx.StreakVisible,Is.False);
            Assert.That(fx.HumanStrokes,Is.EqualTo(3));
            yield return null;
        }

        [UnityTest] public IEnumerator Burst_GrowsWithPower()
        {
            Strike(0,.2f);fx.Tick(.01f);int soft=fx.ActiveQuads;fx.Tick(1);
            Strike(0,1f);fx.Tick(.01f);int strong=fx.ActiveQuads;
            Assert.That(strong,Is.GreaterThan(soft),"more sparks for a harder stroke");
            yield return null;
        }

        [UnityTest] public IEnumerator Effects_FreezeWithTheClock_Expire_AndCreateNothingAfterStart()
        {
            // The effects live on the app object, which other views share: count this view's own objects by name.
            int Own(string name){int n=0;foreach(Transform child in fx.transform)if(child.name==name)n++;return n;}
            Assert.That(Own("Stroke fx"),Is.EqualTo(1));Assert.That(Own("Stroke streak"),Is.EqualTo(1));
            Strike(0,.9f);fx.Tick(.02f);
            int active=fx.ActiveQuads;var corner=fx.VertexAt(StrokeFx.Rings*4);Assert.That(active,Is.GreaterThan(0));
            app.Clock.Paused=true;fx.Tick(1);
            Assert.That(fx.ActiveQuads,Is.EqualTo(active),"effects wait for the clock");Assert.That(fx.VertexAt(StrokeFx.Rings*4),Is.EqualTo(corner));Assert.That(fx.StreakVisible,Is.True);
            app.Clock.Paused=false;fx.Tick(.02f);
            Assert.That(fx.VertexAt(StrokeFx.Rings*4),Is.Not.EqualTo(corner),"and move again once it runs");
            fx.Tick(1);Assert.That(fx.ActiveQuads,Is.Zero);
            for(int i=0;i<6;i++){Strike(0,1f);fx.Tick(.01f);}
            Assert.That(fx.ActiveQuads,Is.LessThanOrEqualTo(StrokeFx.MaxQuads));
            Assert.That(Own("Stroke fx"),Is.EqualTo(1),"pooled: one mesh for every burst");Assert.That(Own("Stroke streak"),Is.EqualTo(1),"and one line for every streak");
            Assert.That(fx.GetComponentsInChildren<Collider>(true),Is.Empty);
            yield return null;
        }

        [UnityTest] public IEnumerator StrongStroke_PunchesTheFieldOfView_OnlyInTheRenderedFrame()
        {
            float fov=view.fieldOfView;var projection=view.projectionMatrix;
            float vMax=(float)app.Host.ActiveConfig.Source.Get("shot.vMax");
            for(int i=0;i<(int)(.3/StepReal);i++)app.Host.Advance(StepReal);
            Assert.That(app.Host.ApplyShot(app.Host.LaunchId,app.Host.Current.Step,new Vector3(0,2,-.5f*vMax)).Applied,Is.True);
            impacts.Tick(0);Assert.That(rig.Punching,Is.False,"an ordinary stroke leaves the lens alone");
            app.LaunchCurrent();for(int i=0;i<(int)(.3/StepReal);i++)app.Host.Advance(StepReal);
            Assert.That(app.Host.ApplyShot(app.Host.LaunchId,app.Host.Current.Step,new Vector3(0,2,-.95f*vMax)).Applied,Is.True);
            impacts.Tick(0);
            Assert.That(rig.Punching,Is.True);Assert.That(rig.RenderFovOffset,Is.InRange(-2.5f,-.3f),"a brief, small push in");
            Assert.That(view.fieldOfView,Is.EqualTo(fov));Assert.That(view.projectionMatrix,Is.EqualTo(projection),"the projection gameplay reads is the placed one");
            rig.TickShake(.5f);Assert.That(rig.Punching,Is.False);Assert.That(rig.RenderFovOffset,Is.Zero);Assert.That(view.fieldOfView,Is.EqualTo(fov));
            yield return null;
        }
    }
}
