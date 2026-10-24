using System.Collections;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Core.Spike;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Court;
using SquashClash.Runtime.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests {
    public sealed class AppBootstrapTests {
        [UnitySetUp] public IEnumerator SetUp() {yield return PlayModeTestWorld.ClearApp();}
        [UnityTearDown] public IEnumerator TearDown() {yield return PlayModeTestWorld.ClearApp();}
        [UnityTest] public IEnumerator Main_DefaultHostUsesP2ForGameAndPredictionAndBounces()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;app.Host.enabled=false;
            app.GetComponent<ResumeOverlay>().Continue();app.Host.Launch(EffectiveConfig.From(app.Params));
            var cfg=app.Config;
            Assert.That(cfg.Backend,Is.EqualTo("P2-Analytic"));
            Assert.That(cfg.Hash,Is.EqualTo(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text).Hash));
            Assert.That(Object.FindObjectsByType<SurfaceTag>(FindObjectsSortMode.None).Where(t=>t.GetComponent<BoxCollider>()!=null),Is.Empty,"P2 does not create PhysX simulation scenes.");
            var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            Trajectory expected;
            using(var simulation=factory.Create(cfg.Physics,cfg.Court))
                expected=TrajectoryRunner.Run(simulation,Launcher.ToState(cfg.Launch,cfg.LaunchScale),cfg.Physics.StepHz*10,StopCondition.SecondFloorBounce,cfg.Court);
            Assert.That(PhysicsMeasures.TrajectoriesBitwiseEqual(app.Host.Prediction,expected,out _),Is.True);
            Assert.That(expected.Contacts,Is.Not.Empty);
            Assert.That(expected.Contacts.All(c=>c.Timing==ContactTiming.Exact),Is.True);
            int bounces=0;app.Host.ContactRaised+=c=>{if(c.Surface==SurfaceId.Floor&&!c.Persistent)bounces++;};
            for(int i=0;i<1000&&app.Host.IsRunning;i++)app.Host.Advance(1.0/60);
            Assert.That(bounces,Is.EqualTo(2));Assert.That(app.Host.IsRunning,Is.False);
            Assert.That(app.Host.PredictionMatchesBitwise,Is.True);
            Assert.That(app.Host.Current.Step,Is.EqualTo(expected.States[expected.States.Count-1].Step));
            TestContext.WriteLine("DEFAULT_CONFIG_HASH="+cfg.Hash);
        }
        [UnityTest] public IEnumerator Main_ConfiguredP1RemainsAvailableAndCanReturnToP2()
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;app.Host.enabled=false;var defaults=app.Config;
            Assert.That(defaults.Backend,Is.EqualTo("P2-Analytic"));
            int generation=app.Host.SimulationGeneration;
            app.Host.Launch(EffectiveConfig.From(defaults.Source.With("physics.backend",0)));
            Assert.That(app.Host.ActiveConfig.Backend,Is.EqualTo("P1-PhysX"));
            Assert.That(app.Host.SimulationGeneration,Is.EqualTo(generation+1));
            Assert.That(app.Host.Prediction.Contacts.Any(c=>c.Timing==ContactTiming.Estimated),Is.True);
            Assert.That(Object.FindObjectsByType<SurfaceTag>(FindObjectsSortMode.None).Count(t=>t.Surface==SurfaceId.Floor&&t.GetComponent<BoxCollider>()!=null),Is.EqualTo(2));
            app.Host.Launch(defaults);
            Assert.That(app.Host.ActiveConfig.Backend,Is.EqualTo("P2-Analytic"));
            Assert.That(app.Host.SimulationGeneration,Is.EqualTo(generation+2));
            Assert.That(app.Host.Prediction.Contacts.All(c=>c.Timing==ContactTiming.Exact),Is.True);
            yield return null;
        }
        [UnityTest] public IEnumerator Launch_UsesEffectiveConfigInEveryConsumer_P1()=>LaunchUsesConfig(0);
        [UnityTest] public IEnumerator Launch_UsesEffectiveConfigInEveryConsumer_P2()=>LaunchUsesConfig(1);
        private IEnumerator LaunchUsesConfig(int backend) {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;app.Host.enabled=false;int generation=app.Host.SimulationGeneration;
            var cfg=EffectiveConfig.From(app.Params.With("physics.backend",backend).With("physics.floor.restitution",0.6).With("physics.stepHz",240).With("pace.tempo",0.8).With("court.tinTop",0.43));
            app.Host.Launch(cfg);
            Assert.That(app.Host.SimulationGeneration,Is.EqualTo(generation+1));Assert.That(app.Host.Clock.StepHz,Is.EqualTo(240));Assert.That(app.Host.Clock.Tempo,Is.EqualTo(0.8));
            Assert.That(app.Host.ActiveConfig.Physics.Floor.Restitution,Is.EqualTo(0.6f));
            var floors=Object.FindObjectsByType<SurfaceTag>(FindObjectsSortMode.None).Where(t=>t.Surface==SurfaceId.Floor&&t.GetComponent<BoxCollider>()!=null).ToArray();
            // Disposed scenes are disabled immediately; only the new pair is active.
            Assert.That(floors.Length,Is.EqualTo(backend==0?2:0));foreach(var f in floors)Assert.That(f.GetComponent<BoxCollider>().sharedMaterial.bounciness,Is.EqualTo(0.6f));
            if(backend==1)
            {
                var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
                using(var sim=factory.Create(cfg.Physics,cfg.Court))
                    Assert.That(PhysicsMeasures.TrajectoriesBitwiseEqual(app.Host.Prediction,TrajectoryRunner.Run(sim,Launcher.ToState(cfg.Launch,cfg.LaunchScale),cfg.Physics.StepHz*10,StopCondition.SecondFloorBounce,cfg.Court),out _),Is.True);
            }
            var court=Object.FindFirstObjectByType<CourtView>();
            var tin=court.transform.Find("Geometry/TinPanel").GetComponent<MeshFilter>().sharedMesh;
            Assert.That(tin.vertices.Max(v=>v.y),Is.EqualTo(0.43f));Assert.That(app.Config.Hash,Is.EqualTo(cfg.Hash));
        }
        [UnityTest] public IEnumerator ManualPause_SurvivesSystemResume_P1()=>ManualPauseSurvives(0);
        [UnityTest] public IEnumerator ManualPause_SurvivesSystemResume_P2()=>ManualPauseSurvives(1);
        private IEnumerator ManualPauseSurvives(int backend)
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;app.Host.enabled=false;var overlay=app.GetComponent<ResumeOverlay>();overlay.Continue();
            app.Host.Launch(EffectiveConfig.From(app.Params.With("physics.backend",backend)));app.Host.Clock.Paused=true;
            var state=app.Host.Current;var shown=app.Host.RenderPosition;var dropped=app.Host.Clock.DroppedSteps;
            app.SendMessage("OnApplicationPause",true);app.SendMessage("OnApplicationFocus",false);app.SendMessage("OnApplicationPause",true);
            overlay.Continue();Assert.That(overlay.IsVisible,Is.False);Assert.That(app.Host.Clock.Paused,Is.True,"System resume must preserve the prior manual pause.");
            app.Host.Advance(30);Assert.That(app.Host.Current.Step,Is.EqualTo(state.Step));Assert.That(app.Host.RenderPosition,Is.EqualTo(shown));Assert.That(app.Host.Clock.DroppedSteps,Is.EqualTo(dropped));
            overlay.Continue();Assert.That(app.Host.Clock.Paused,Is.True,"An idle overlay must not clear manual pause.");
        }
        [UnityTest] public IEnumerator InvalidInjectedState_IsRejectedBeforeReset_P1()=>InvalidState(0);
        [UnityTest] public IEnumerator InvalidInjectedState_IsRejectedBeforeReset_P2()=>InvalidState(1);
        private IEnumerator InvalidState(int backend)
        {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;app.Host.enabled=false;var cfg=EffectiveConfig.From(app.Params.With("physics.backend",backend));app.Host.Launch(cfg);
            var current=app.Host.Current;int generation=app.Host.SimulationGeneration;var bad=current;
            foreach(var p in new[]{new Vector3(-.01f,1,5),new Vector3(6.41f,1,5),new Vector3(3.2f,0,5),new Vector3(3.2f,1,-1),new Vector3(float.NaN,1,5)})
            {bad.Position=p;Assert.Throws<System.ArgumentOutOfRangeException>(()=>app.Host.Launch(cfg,bad,120,StopCondition.MaxSteps));}
            bad=current;bad.Velocity.x=float.PositiveInfinity;Assert.Throws<System.ArgumentOutOfRangeException>(()=>app.Host.Launch(cfg,bad,120,StopCondition.MaxSteps));
            bad=current;bad.Step=-1;Assert.Throws<System.ArgumentOutOfRangeException>(()=>app.Host.Launch(cfg,bad,120,StopCondition.MaxSteps));
            Assert.That(app.Host.Current.Position,Is.EqualTo(current.Position));Assert.That(app.Host.Current.Step,Is.EqualTo(current.Step));Assert.That(app.Host.SimulationGeneration,Is.EqualTo(generation));
        }
        [UnityTest] public IEnumerator SystemPause_ShowsResumeOverlay_WithoutDevPanelDependency() {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;app.Host.enabled=false;var overlay=app.GetComponent<ResumeOverlay>();
            Assert.That(typeof(ResumeOverlay).Assembly.GetName().Name,Is.EqualTo("SquashClash.Runtime"));
            overlay.Continue();app.Host.Launch(EffectiveConfig.From(app.Params));
            long dropsBeforePause=app.Host.Clock.DroppedSteps;
            app.SendMessage("OnApplicationPause",true);Assert.That(app.Host.Clock.Paused,Is.True);Assert.That(overlay.IsVisible,Is.True);
            app.Host.Advance(30);Assert.That(app.Host.Current.Step,Is.Zero);Assert.That(app.Host.Clock.DroppedSteps,Is.EqualTo(dropsBeforePause));
            app.SendMessage("OnApplicationPause",false);app.SendMessage("OnApplicationFocus",true);
            Assert.That(app.Host.Clock.Paused,Is.True);
            using(var tap=PointerDownEvent.GetPooled()) {var layer=app.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("resume-overlay");tap.target=layer;layer.SendEvent(tap);}
            Assert.That(app.Host.Clock.Paused,Is.False);Assert.That(overlay.IsVisible,Is.False);
            app.Host.Clock.Paused=true;yield return null;Assert.That(overlay.IsVisible,Is.False,"Manual pause must not show system overlay.");
        }
        [UnityTest] public IEnumerator SystemResume_DiscardsDeltaOfResumeFrame() {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var app=AppBootstrap.Instance;app.Host.enabled=false;app.GetComponent<ResumeOverlay>().Continue();
            app.Host.Launch(EffectiveConfig.From(app.Params));
            app.Host.Clock.Advance(0.99/(120*0.65));
            long dropsBeforePause=app.Host.Clock.DroppedSteps;
            app.SendMessage("OnApplicationPause",true);app.Host.Advance(30);
            var layer=app.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("resume-overlay");
            using(var tap=PointerDownEvent.GetPooled()){tap.target=layer;layer.SendEvent(tap);}
            app.Host.SendMessage("Update");
            Assert.That(app.Host.Current.Step,Is.Zero,"The resume frame must discard time measured while paused.");
            Assert.That(app.Host.Clock.Alpha,Is.EqualTo(0.99f).Within(1e-6));Assert.That(app.Host.Clock.DroppedSteps,Is.EqualTo(dropsBeforePause));
        }
        [UnityTest] public IEnumerator EditorDevToolbar_FourActionsControlHost() {
            Assert.That(System.Type.GetType("SquashClash.DevPanel.DevPanelBootstrap, SquashClash.DevPanel"),Is.Not.Null);
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;yield return null;
            var app=AppBootstrap.Instance;app.Host.enabled=false;app.GetComponent<ResumeOverlay>().Continue();
            app.Host.Launch(EffectiveConfig.From(app.Params));var root=app.GetComponent<UIDocument>().rootVisualElement;
            foreach(var name in new[]{"launch","pause","step","replay"})Assert.That(root.Q<Button>(name),Is.Not.Null,name);
            using(var click=NavigationSubmitEvent.GetPooled()) {click.target=root.Q<Button>("pause");root.Q<Button>("pause").SendEvent(click);}
            yield return null;
            Assert.That(app.Host.Clock.Paused,Is.True);
            using(var click=NavigationSubmitEvent.GetPooled()) {click.target=root.Q<Button>("step");root.Q<Button>("step").SendEvent(click);}
            yield return null;
            app.Host.Advance(0);Assert.That(app.Host.Current.Step,Is.EqualTo(1));
            using(var click=NavigationSubmitEvent.GetPooled()) {click.target=root.Q<Button>("replay");root.Q<Button>("replay").SendEvent(click);}
            yield return null;
            Assert.That(app.Host.Current.Step,Is.Zero);
            using(var click=NavigationSubmitEvent.GetPooled()) {click.target=root.Q<Button>("launch");root.Q<Button>("launch").SendEvent(click);}
            yield return null;
            Assert.That(app.Host.Current.Step,Is.Zero);
        }
        [UnityTest] public IEnumerator Main_BallViewUsesPhysicalRadiusAndHasNoCollider() {
            yield return SceneManager.LoadSceneAsync("Main",LoadSceneMode.Single);yield return null;
            var view=Object.FindFirstObjectByType<BallView>();Assert.That(view,Is.Not.Null);
            Assert.That(view.GetComponentsInChildren<Collider>(),Is.Empty);
            Assert.That(view.GetComponent<MeshRenderer>().bounds.size.x,Is.EqualTo(0.04f).Within(1e-5));
            Assert.That(Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.gameObject.scene==view.gameObject.scene),Is.Empty);
        }
    }
}
