using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Camera;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Art pass. Named to run last (see ZRallyDirectorTests).
    public sealed class ZImpactFxTests
    {
        private AppBootstrap app;private ImpactFx fx;private CourtCameraRig rig;private ShotController controller;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Impact fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            fx=app.GetComponent<ImpactFx>();rig=UnityEngine.Camera.main.GetComponent<CourtCameraRig>();controller=app.GetComponent<ShotController>();
            Assert.That(fx,Is.Not.Null,"the app wires the impact effects at startup");
            app.Host.enabled=app.enabled=controller.enabled=fx.enabled=rig.enabled=false;
            if(app.Rally!=null)app.Rally.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return PlayModeTestWorld.ClearApp();}
        private bool Drive(System.Func<bool> until,double seconds=10)
        {
            int limit=(int)(seconds/StepReal);
            for(int i=0;i<limit;i++){if(until())return true;app.Host.Advance(StepReal);}
            return until();
        }

        [UnityTest] public IEnumerator Bounce_SpawnsOnePooledEffect_ThatFadesFreezesOnPauseAndExpires()
        {
            Assert.That(fx.ActiveCount,Is.Zero);
            Assert.That(Drive(()=>fx.Spawned>0),Is.True,"the launcher ball must hit a surface");
            Assert.That(fx.Spawned,Is.EqualTo(1));Assert.That(fx.ActiveCount,Is.EqualTo(1));
            Assert.That(fx.GetComponentsInChildren<Collider>(true),Is.Empty);
            Assert.That(fx.transform.GetComponentsInChildren<MeshRenderer>(true).Length,Is.GreaterThanOrEqualTo(ImpactFx.PoolSize));
            app.Clock.Paused=true;fx.Tick(1);Assert.That(fx.ActiveCount,Is.EqualTo(1),"effects freeze with the clock");
            app.Clock.Paused=false;fx.Tick(.1f);Assert.That(fx.ActiveCount,Is.EqualTo(1));
            fx.Tick(.3f);Assert.That(fx.ActiveCount,Is.Zero);
            yield return null;
        }

        [UnityTest] public IEnumerator ManyBounces_NeverExceedThePool()
        {
            Drive(()=>fx.Spawned>=3,20);
            for(int i=0;i<40;i++){app.LaunchCurrent();Drive(()=>false,.6);}
            Assert.That(fx.ActiveCount,Is.LessThanOrEqualTo(ImpactFx.PoolSize));
            int pooled=0;foreach(Transform child in fx.transform)if(child.name=="Impact")pooled++;
            Assert.That(pooled,Is.EqualTo(ImpactFx.PoolSize),"no object is created after start");
            yield return null;
        }

        [UnityTest] public IEnumerator OnlyStrongStrokes_ShakeTheCamera_AndItReturnsExactly()
        {
            var pose=rig.transform.position;var rotation=rig.transform.rotation;
            float vMax=(float)app.Host.ActiveConfig.Source.Get("shot.vMax");
            Drive(()=>false,.3);
            var soft=app.Host.ApplyShot(app.Host.LaunchId,app.Host.Current.Step,new Vector3(0,2,-.5f*vMax));
            Assert.That(soft.Applied,Is.True,soft.Reason);
            fx.Tick(0);Assert.That(fx.Shakes,Is.Zero);Assert.That(rig.Shaking,Is.False);
            app.LaunchCurrent();Drive(()=>false,.3); // one stroke per reception: the strong one needs its own launch
            var strong=app.Host.ApplyShot(app.Host.LaunchId,app.Host.Current.Step,new Vector3(0,2,-.95f*vMax));
            Assert.That(strong.Applied,Is.True,strong.Reason);
            fx.Tick(0);Assert.That(fx.Shakes,Is.EqualTo(1));Assert.That(rig.Shaking,Is.True);
            rig.TickShake(.05f);
            Assert.That(Vector3.Distance(rig.transform.position,pose),Is.InRange(1e-4f,ImpactFx.ShakeMeters*1.5f),"a subtle nudge");
            Assert.That(rig.transform.rotation,Is.EqualTo(rotation),"the shake translates, it does not rotate");
            rig.TickShake(.2f);
            Assert.That(rig.Shaking,Is.False);Assert.That(rig.transform.position,Is.EqualTo(pose));Assert.That(rig.transform.rotation,Is.EqualTo(rotation));
            yield return null;
        }
    }
}
