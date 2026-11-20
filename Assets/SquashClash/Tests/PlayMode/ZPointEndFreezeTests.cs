using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Camera;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Point-end freeze (PO, 19 nov): for 100 ms of unscaled real time after a point closes, only the rendered pose is held, inside the pause that already exists.
    public sealed class ZPointEndFreezeTests : ZFirstRunFixture
    {
        private double t;
        [UnitySetUp] public IEnumerator Setup(){yield return Boot(false);app.Clock.Paused=false;PointEndFreeze.Reset();t=1000;PointEndFreeze.Now=()=>t;yield return null;}
        [UnityTearDown] public IEnumerator Cleanup(){PointEndFreeze.Reset();return Shutdown();}
        private static object Call(object target,string method){var m=target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);Assert.That(m,Is.Not.Null,method);return m.Invoke(target,null);}
        [Test] public void Default_IsOn_AndOffMeansNeverActive()
        {
            Assert.That(PointEndFreeze.Enabled,Is.True,"default ON");
            PointEndFreeze.Enabled=false;PointEndFreeze.Begin(1.6,()=>true);Assert.That(PointEndFreeze.Active,Is.False,"off = today's behaviour");
        }
        [Test] public void Begin_HoldsExactly100ms_OfUnscaledTime()
        {
            PointEndFreeze.Begin(1.6,()=>true);Assert.That(PointEndFreeze.Active,Is.True);
            t+=.099;Assert.That(PointEndFreeze.Active,Is.True);t+=.002;Assert.That(PointEndFreeze.Active,Is.False,"released after 100 ms");
            t+=.5;Assert.That(PointEndFreeze.Active,Is.False,"never comes back");
        }
        [Test] public void Begin_ClampsToAShorterPause()
        {
            PointEndFreeze.Begin(.05,()=>true);Assert.That(PointEndFreeze.Duration,Is.EqualTo(.05).Within(1e-9),"never longer than the pause");
            t+=.06;Assert.That(PointEndFreeze.Active,Is.False);
            PointEndFreeze.Begin(0,()=>true);Assert.That(PointEndFreeze.Active,Is.False,"no pause, no freeze");
        }
        [Test] public void Freeze_EndsAtOnce_WhenThePauseEnds()
        {
            bool inPause=true;PointEndFreeze.Begin(1.6,()=>inPause);Assert.That(PointEndFreeze.Active,Is.True);
            inPause=false;Assert.That(PointEndFreeze.Active,Is.False,"the next point never starts frozen");
        }
        [UnityTest] public IEnumerator BallView_HoldsItsPose_ThenResumes()
        {
            var view=Object.FindFirstObjectByType<BallView>();view.enabled=false;
            view.transform.position=new Vector3(9,9,9);PointEndFreeze.Begin(1.6,()=>true);
            Call(view,"LateUpdate");Assert.That(view.transform.position,Is.EqualTo(new Vector3(9,9,9)),"held while frozen");
            t+=.11;Call(view,"LateUpdate");Assert.That(view.transform.position,Is.EqualTo(app.Host.RenderPosition),"live again after 100 ms");
            yield return null;
        }
        [UnityTest] public IEnumerator Players_HoldTheirDrawnPose_ThenResume()
        {
            var human=app.Players.Human;app.Players.Place(new Vector2(1,1),new Vector2(2,2));var held=human.localPosition;
            PointEndFreeze.Begin(1.6,()=>true);app.Players.Place(new Vector2(3,3),new Vector2(4,4));
            Assert.That(human.localPosition,Is.EqualTo(held),"bodies do not move on screen while frozen");
            t+=.11;app.Players.Place(new Vector2(3,3),new Vector2(4,4));Assert.That(human.localPosition.x,Is.EqualTo(3f),"placed again after 100 ms");
            yield return null;
        }
        [UnityTest] public IEnumerator CharacterViews_HoldTheirAnimation_ThenResume()
        {
            yield return null;var views=Object.FindObjectsByType<CharacterView>(FindObjectsSortMode.None);Assert.That(views.Length,Is.EqualTo(2));
            var idle=typeof(CharacterView).GetField("idle",BindingFlags.Instance|BindingFlags.NonPublic);Assert.That(idle,Is.Not.Null);
            foreach(var view in views)view.enabled=false;
            foreach(var view in views)Call(view,"LateUpdate");var before=new List<float>();foreach(var view in views)before.Add((float)idle.GetValue(view));
            PointEndFreeze.Begin(1.6,()=>true);foreach(var view in views)Call(view,"LateUpdate");
            for(int i=0;i<views.Length;i++)Assert.That((float)idle.GetValue(views[i]),Is.EqualTo(before[i]),"pose clock held");
            t+=.11;foreach(var view in views)Call(view,"LateUpdate");
            for(int i=0;i<views.Length;i++)Assert.That((float)idle.GetValue(views[i]),Is.GreaterThan(before[i]),"animating again");
        }
        [UnityTest] public IEnumerator CameraShake_HoldsItsOffset_ThenResumes()
        {
            yield return null;var rig=Object.FindFirstObjectByType<CourtCameraRig>();rig.enabled=false;
            rig.Shake(.05f,1f);Call(rig,"LateUpdate");var live=rig.RenderOffset;Assert.That(live,Is.Not.EqualTo(Vector3.zero),"shaking");
            PointEndFreeze.Begin(1.6,()=>true);Call(rig,"LateUpdate");Assert.That(rig.RenderOffset,Is.EqualTo(live),"shake pose held");
            t+=.11;Call(rig,"LateUpdate");Assert.That(rig.RenderOffset,Is.Not.EqualTo(live),"shake continues after the hold");
        }
        [UnityTest] public IEnumerator PointEnd_StartsTheFreeze_InsideThePause_AndTheNextPointEndsIt()
        {
            rally.Autopilot=true;rally.Seed=5;rally.AutoRestart=false;rally.SetParams(new RallyParams{MaxShots=4,RestartSeconds=1.6});rally.StartMode();
            Assert.That(PointEndFreeze.Active,Is.False,"nothing frozen while the ball is live");
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,120),Is.True);
            Assert.That(PointEndFreeze.Active,Is.True,"point closed: frozen");Assert.That(PointEndFreeze.Duration,Is.EqualTo(.1).Within(1e-9));
            t+=.099;Assert.That(PointEndFreeze.Active,Is.True);t+=.002;Assert.That(PointEndFreeze.Active,Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator SkippingThePause_CancelsTheFreeze()
        {
            rally.Autopilot=true;rally.Seed=5;rally.SetParams(new RallyParams{MaxShots=4,RestartSeconds=1.6});rally.StartMode();
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,120),Is.True);Assert.That(PointEndFreeze.Active,Is.True);
            rally.SkipWait();Assert.That(Drive(()=>rally.Phase!=RallyPhase.PointOver,10),Is.True);
            Assert.That(PointEndFreeze.Active,Is.False,"the next point starts live, same real time");
            yield return null;
        }
        [UnityTest] public IEnumerator PauseShorterThan100ms_FreezesOnlyThePause()
        {
            rally.Autopilot=true;rally.Seed=5;rally.AutoRestart=false;rally.SetParams(new RallyParams{MaxShots=4,RestartSeconds=.05});rally.StartMode();
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,120),Is.True);
            Assert.That(PointEndFreeze.Duration,Is.EqualTo(.05).Within(1e-9),"clamped to the pause");
            yield return null;
        }
        [UnityTest] public IEnumerator DevSection_HasTheToggle_DefaultOn()
        {
            for(int i=0;i<4;i++){Refresh();yield return null;}
            var on=root.Q<Button>("point-freeze-on");var off=root.Q<Button>("point-freeze-off");
            Assert.That(on,Is.Not.Null,"DEV panel has «Congelar fin de punto sí»");Assert.That(off,Is.Not.Null,"and «no»");
            Assert.That(PointEndFreeze.Enabled,Is.True);
            Click("point-freeze-off");Assert.That(PointEndFreeze.Enabled,Is.False);Click("point-freeze-on");Assert.That(PointEndFreeze.Enabled,Is.True);
        }
    }
    // The freeze is view-only: the simulation sequence, the pause and the next point are identical with it on and off.
    public sealed class ZPointEndFreezeSimulationTests
    {
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        private IEnumerator Boot()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Freeze fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();rally=app.Rally;app.LaunchCurrent();
            app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){PointEndFreeze.Reset();return PlayModeTestWorld.ClearApp();}
        // One point plus its pause plus the first ticks of the next: every observable of the simulation, per step.
        private List<string> Trace(bool freeze,out int pauseSteps)
        {
            PointEndFreeze.Reset();PointEndFreeze.Enabled=freeze;
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Seed=5;rally.SetParams(new RallyParams{MaxShots=4,RestartSeconds=.5});rally.StartMode();
            var lines=new List<string>();bool over=false;pauseSteps=0;
            for(int i=0;i<40000;i++)
            {
                app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();
                if(rally.Phase==RallyPhase.PointOver){over=true;pauseSteps++;}
                lines.Add(app.Host.Current.Step+"|"+app.Host.LaunchId+"|"+rally.Phase+"|"+rally.Human.Position.x.ToString("R")+","+rally.Human.Position.y.ToString("R")+"|"
                    +rally.Rival.Position.x.ToString("R")+","+rally.Rival.Position.y.ToString("R")+"|"+rally.HumanPoints+"-"+rally.RivalPoints+"|"+app.Clock.Paused+"|"+Time.timeScale+"|"+Time.fixedDeltaTime.ToString("R"));
                if(over&&rally.Phase!=RallyPhase.PointOver&&lines.Count>0){for(int k=0;k<20;k++){app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();lines.Add(app.Host.Current.Step+"|"+app.Host.LaunchId+"|"+rally.Phase);}break;}
            }
            Assert.That(over,Is.True,"a point closed");return lines;
        }
        [UnityTest] public IEnumerator SimulationPauseAndNextPoint_AreIdentical_WithFreezeOnAndOff()
        {
            yield return Boot();var on=Trace(true,out int pauseOn);
            yield return Boot();var off=Trace(false,out int pauseOff);
            Assert.That(pauseOn,Is.EqualTo(pauseOff),"the pause lasts the same number of steps");
            Assert.That(pauseOn*StepReal,Is.GreaterThanOrEqualTo(.1),"the 100 ms freeze fits inside the pause");
            Assert.That(on.Count,Is.EqualTo(off.Count),"the next point starts at the same step");
            for(int i=0;i<on.Count;i++)Assert.That(on[i],Is.EqualTo(off[i]),"step "+i);
        }
    }
}
