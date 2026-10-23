using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Core.Spike;
using SquashClash.Runtime.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UPhysics=UnityEngine.Physics;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests
{
    public sealed class PhysXLifecycleTests
    {
        private Vector3 gravity;
        private readonly List<IBallSimulation> sims=new List<IBallSimulation>();
        private readonly List<PhysXBallSimulationFactory> factories=new List<PhysXBallSimulationFactory>();
        [UnitySetUp] public IEnumerator SetUp() { yield return PlayModeTestWorld.ClearApp(); gravity=UPhysics.gravity; }
        [UnityTearDown] public IEnumerator TearDown() {
            foreach(var s in sims) s.Dispose(); sims.Clear();
            for(int i=0;i<120 && factories.Any(f=>f.PendingUnloads!=0);i++) yield return null;
            Assert.That(factories.Sum(f=>f.LiveInstances),Is.Zero); Assert.That(factories.Sum(f=>f.PendingUnloads),Is.Zero);
            factories.Clear(); UPhysics.gravity=gravity;
        }
        private PhysXBallSimulationFactory Factory() { var f=new PhysXBallSimulationFactory(); factories.Add(f);return f; }
        private IBallSimulation Create(PhysXBallSimulationFactory f,PhysicsParams p=null,CourtDimensions d=null) {
            var s=f.Create(p??Params(),d??CourtDimensions.Standard());sims.Add(s);return s;
        }
        private static PhysicsParams Params() => new PhysicsParams {Gravity=9.81f,StepHz=120,MaxImpactsPerStep=4,
            Floor=new SurfaceMaterial {Restitution=0.45f,TangentialFriction=0.3f},
            FrontWall=new SurfaceMaterial {Restitution=0.55f,TangentialFriction=0.2f},
            SideWalls=new SurfaceMaterial {Restitution=0.55f,TangentialFriction=0.2f},
            BackWall=new SurfaceMaterial {Restitution=0.55f,TangentialFriction=0.2f}};
        private static Scene[] Scenes() => Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
        private static IEnumerable<T> Components<T>(Scene scene) where T:Component => scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true));
        private static Scene NewScene(HashSet<Scene> before) => Scenes().Single(s=>!before.Contains(s));
        [UnityTest] public IEnumerator Drop_HitsFloor_WithSurfaceUVAndHonestTiming() {
            var s=Create(Factory());s.Reset(new BallState {Position=new Vector3(1.2f,1,5.3f)});
            var contacts=new List<ContactEvent>(); ContactEvent? hit=null;
            BallState beforeHit=default;
            for(int i=0;i<240 && !hit.HasValue;i++) {
                var before=s.State;s.Step(contacts);
                foreach(var c in contacts) if(c.Surface==SurfaceId.Floor&&!c.Persistent) { hit=c;beforeHit=before; }
            }
            Assert.That(hit.HasValue,Is.True); var e=hit.Value;
            Assert.That(Vector2.Distance(e.UV,new Vector2(1.2f,5.3f)),Is.LessThanOrEqualTo(0.001f));
            Assert.That(Mathf.Abs(e.Point.y),Is.LessThanOrEqualTo(0.001f)); Assert.That(e.Normal.y,Is.GreaterThan(0.99f));
            Assert.That(e.ApproachVelocity.y,Is.LessThan(0));
            bool within=Ballistics.FirstPlaneHit(beforeHit,9.81f,0.02f,CourtDimensions.Standard(),1.0/120,out var plane,out var t)&&plane==SurfaceId.Floor;
            if(within) { Assert.That(e.Timing,Is.EqualTo(ContactTiming.Estimated));Assert.That(e.TimeInStep,Is.EqualTo((float)t)); }
            else { Assert.That(e.Timing,Is.EqualTo(ContactTiming.StepOnly));Assert.That(float.IsNaN(e.TimeInStep),Is.True); }
            Assert.That(s.Capabilities.Timing,Is.EqualTo(ContactTiming.Estimated)); Assert.That(s.Capabilities.ImpactCapConfigurable,Is.False);
            Assert.That(s.Capabilities.RunsInEditMode,Is.False); yield return null;
        }
        [UnityTest] public IEnumerator NearFloor_ContactWithinAnalyticStep_ReportsEstimatedTime() {
            var sim=Create(Factory());sim.Reset(new BallState {Position=new Vector3(3,0.021f,5),Velocity=Vector3.down});
            var contacts=new List<ContactEvent>();sim.Step(contacts);
            var hit=contacts.Single(c=>c.Surface==SurfaceId.Floor&&!c.Persistent);
            Assert.That(hit.Timing,Is.EqualTo(ContactTiming.Estimated));Assert.That(hit.TimeInStep,Is.InRange(0f,1f/120));yield return null;
        }
        [UnityTest] public IEnumerator Create_PutsAllObjectsInOwnScene_WithAx1Planes() {
            var active=SceneManager.GetActiveScene(); int roots=active.rootCount; var before=Scenes().ToHashSet();
            Create(Factory()); var scene=NewScene(before);
            Assert.That(SceneManager.GetActiveScene(),Is.EqualTo(active)); Assert.That(active.rootCount,Is.EqualTo(roots));
            Assert.That(scene.GetPhysicsScene(),Is.Not.EqualTo(UPhysics.defaultPhysicsScene));
            Assert.That(Components<Transform>(scene).All(t=>t.gameObject.scene==scene),Is.True);
            Assert.That(Components<Rigidbody>(scene).Count(),Is.EqualTo(1)); Assert.That(Components<BoxCollider>(scene).Count(),Is.EqualTo(5));
            var sides=Components<SurfaceTag>(scene).ToDictionary(t=>t.Surface);
            Assert.That(sides[SurfaceId.LeftWall].GetComponent<BoxCollider>().bounds.min.x,Is.EqualTo(6.4f).Within(1e-5));
            Assert.That(sides[SurfaceId.RightWall].GetComponent<BoxCollider>().bounds.max.x,Is.Zero.Within(1e-5));
            var ball=Components<Rigidbody>(scene).Single();
            Assert.That(ball.mass,Is.EqualTo(0.024f)); Assert.That(ball.GetComponent<SphereCollider>().radius,Is.EqualTo(0.02f));
            Assert.That(ball.constraints,Is.EqualTo(RigidbodyConstraints.FreezeRotation));
            Assert.That(ball.sleepThreshold,Is.Zero); Assert.That(ball.interpolation,Is.EqualTo(RigidbodyInterpolation.None)); yield return null;
        }
        [UnityTest] public IEnumerator Instances_DoNotInterfere() {
            var f=Factory();var a=Create(f);var b=Create(f);var start=new BallState {Position=new Vector3(3,1,5),Velocity=new Vector3(0,2,-8)};
            a.Reset(start);b.Reset(start);var snapshot=b.State;var contacts=new List<ContactEvent>();
            for(int i=0;i<100;i++) a.Step(contacts);
            Assert.That(b.State.Position,Is.EqualTo(snapshot.Position));Assert.That(b.State.Velocity,Is.EqualTo(snapshot.Velocity)); Assert.That(b.State.Step,Is.EqualTo(snapshot.Step));
            yield return null;
        }
        [UnityTest] public IEnumerator Create_WithOtherGravityWhileAlive_ThrowsAcrossFactories() {
            Create(Factory());var p=Params();p.Gravity=12;
            Assert.Throws<InvalidOperationException>(()=>Factory().Create(p,CourtDimensions.Standard())); yield return null;
        }
        [UnityTest] public IEnumerator Dispose_UnloadsScenesAndMaterials() {
            int count=SceneManager.sceneCount;var before=Resources.FindObjectsOfTypeAll<PhysicsMaterial>().Where(m=>m.name.StartsWith("P1-")).Select(m=>m.GetInstanceID()).ToHashSet();
            var f=Factory(); for(int i=0;i<20;i++) {var s=Create(f);s.Dispose();}
            for(int i=0;i<120 && f.PendingUnloads!=0;i++) yield return null;
            Assert.That(f.LiveInstances,Is.Zero);Assert.That(f.PendingUnloads,Is.Zero);Assert.That(SceneManager.sceneCount,Is.EqualTo(count));
            Assert.That(Resources.FindObjectsOfTypeAll<PhysicsMaterial>().Where(m=>m.name.StartsWith("P1-")).Select(m=>m.GetInstanceID()),Is.EquivalentTo(before));
        }
        [UnityTest] public IEnumerator Materials_FollowParams_AndCallerMutationCannotChangeInstance() {
            var p=Params();p.FrontWall.Restitution=0.3f;var before=Scenes().ToHashSet();
            var s=Create(Factory(),p);var scene=NewScene(before);
            var front=Components<SurfaceTag>(scene).Single(t=>t.Surface==SurfaceId.FrontWall).GetComponent<BoxCollider>();
            Assert.That(front.sharedMaterial.bounciness,Is.EqualTo(0.3f));Assert.That(front.sharedMaterial.bounceCombine,Is.EqualTo(PhysicsMaterialCombine.Multiply));
            Assert.That(front.sharedMaterial.dynamicFriction,Is.EqualTo(0.2f));Assert.That(front.sharedMaterial.frictionCombine,Is.EqualTo(PhysicsMaterialCombine.Multiply));
            p.StepHz=60;p.Gravity=12;p.FrontWall.Restitution=0.9f;
            s.Reset(new BallState {Position=new Vector3(3,3,5)});s.Step(new List<ContactEvent>());
            Assert.That(s.State.Velocity.y,Is.EqualTo(-9.81f/120).Within(1e-6));Assert.That(front.sharedMaterial.bounciness,Is.EqualTo(0.3f));yield return null;
        }
        [UnityTest] public IEnumerator Stay_RetainsPersistentContactSamples() {
            var p=Params();p.Floor.Restitution=0;var s=Create(Factory(),p);s.Reset(new BallState {Position=new Vector3(3,0.1f,5)});
            var contacts=new List<ContactEvent>();bool persistent=false;
            for(int i=0;i<120;i++) {s.Step(contacts);persistent|=contacts.Any(c=>c.Surface==SurfaceId.Floor&&c.Persistent);}
            Assert.That(persistent,Is.True);yield return null;
        }
        [UnityTest, Timeout(180000)] public IEnumerator DifferentHistories_SameCheckpoint_ReturnSameContactOrder() {
            var cfg=EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
            var f=Factory();var run=new SpikeRun(f,cfg.Physics,cfg.Court,new SpikeOptions {RandomCount=110,Seed=20261020,Variant="contact-order-regression"});
            // Replay the real job prefix through S8-109. The two persistent instances have different
            // histories (only game runs robustness); C7 resets BOTH to the same checkpoint state.
            // The isolated S8-109 history is insufficient: its earlier prefix matters to PhysX pair order.
            try {
                while(!run.Step(8))yield return null;
                Assert.That(run.Result.Complete,Is.True);
                var c7=run.Result.Criteria.Single(c=>c.Id=="C7");Assert.That(c7.Evaluated,Is.GreaterThan(0));
                foreach(var detail in run.Result.Details.Where(v=>v.StartsWith("C7 diagnóstico:")||v.StartsWith("C7 clasificación:")))Debug.Log("[ContactOrder] "+detail);
                var failure=run.Result.Details.Find(v=>v.StartsWith("C7 clasificación: S8-109 C7 postcontacto"));
                if(failure!=null) {
                    Assert.That(failure,Does.Contain("estadosBitsIguales=True; multiconjuntoContactosCompletosIgual=True"));
                    Assert.That(failure,Does.Contain("primerPaso=127; índice=77; checkpoint=50"));
                }
                Assert.That(c7.Measured,Does.StartWith("fallos=0;"),"ordered contacts from two real PhysX instances; pre-fix failure records prove identical state bits and full contact values");
            }
            finally {run.Cancel();}
        }
    }
}
