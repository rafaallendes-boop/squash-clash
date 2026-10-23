using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
using SquashClash.Core.Spike;
using SquashClash.Runtime.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using UPhysics = UnityEngine.Physics;

namespace SquashClash.Runtime.Tests
{
    public sealed class CcdDiagnosticContacts : MonoBehaviour
    {
        public int Step;
        public readonly List<int> ImpactSteps = new List<int>();
        private void OnCollisionEnter(Collision collision) => ImpactSteps.Add(Step);
    }

    public sealed class CcdDiagnosticsTests
    {
        [UnitySetUp] public IEnumerator SetUp() => PlayModeTestWorld.ClearApp();

        [UnityTest] public IEnumerator RawSphere_LocalScene_CcdPreventsCrossingAndReportsImpactStep() => RawSphere(true);
        [UnityTest] public IEnumerator RawSphere_DefaultScene_CcdPreventsCrossingAndReportsImpactStep() => RawSphere(false);

        private IEnumerator RawSphere(bool local)
        {
            var savedMode = UPhysics.simulationMode;
            UPhysics.simulationMode = SimulationMode.Script;
            var scene = SceneManager.CreateScene("CcdMinimal-" + Guid.NewGuid().ToString("N"),
                new CreateSceneParameters(local ? LocalPhysicsMode.Physics3D : LocalPhysicsMode.None));
            var physicsScene = scene.GetPhysicsScene();
            var trace = new StringBuilder("step,z,vz,contacts\n");
            float minimum = float.PositiveInfinity;
            int[] impacts = null;
            try
            {
                var wall = new GameObject("Wall-1m"); SceneManager.MoveGameObjectToScene(wall, scene);
                wall.transform.position = new Vector3(0, 0, -.5f);
                var box = wall.AddComponent<BoxCollider>(); box.size = new Vector3(4, 4, 1); box.contactOffset = .001f;
                var ball = new GameObject("Sphere-radius20mm"); SceneManager.MoveGameObjectToScene(ball, scene);
                ball.transform.position = new Vector3(0, 0, .3f);
                var sphere = ball.AddComponent<SphereCollider>(); sphere.radius = .02f; sphere.contactOffset = .001f;
                var body = ball.AddComponent<Rigidbody>(); body.useGravity = false; body.linearDamping = 0;
                body.constraints = RigidbodyConstraints.FreezeRotation;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.linearVelocity = new Vector3(0, 0, -25);
                var collector = ball.AddComponent<CcdDiagnosticContacts>();
                UPhysics.SyncTransforms();
                for (int step = 1; step <= 6; step++)
                {
                    collector.Step = step; physicsScene.Simulate(1f / 120);
                    minimum = Mathf.Min(minimum, body.position.z);
                    trace.AppendLine(FormattableString.Invariant($"{step},{body.position.z:R},{body.linearVelocity.z:R},{string.Join(";", collector.ImpactSteps.Where(s => s == step))}"));
                }
                impacts = collector.ImpactSteps.ToArray();
            }
            finally
            {
                foreach (var root in scene.GetRootGameObjects()) root.SetActive(false);
                SceneManager.UnloadSceneAsync(scene); UPhysics.simulationMode = savedMode;
                Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/ccd-minimal-" + (local ? "local" : "default") + ".csv", trace.ToString());
            }
            yield return null;
            Assert.That(minimum, Is.GreaterThanOrEqualTo(0), "center must never cross z=0");
            Assert.That(impacts, Does.Contain(2), "impact is in step ceil((0.3-0.02)/(25/120))=2");
        }

        [UnityTest] public IEnumerator S1_BaselineAndSpeculative_WriteSteps30To36()
        {
            var config = EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
            var scenario = PhysicsScenarios.Fixed(config.Court, config.Physics).Single(s => s.Id == "S1");
            var savedGravity = UPhysics.gravity;
            float minimumBaselineZ = float.PositiveInfinity; long firstBaselineImpact = -1;
            foreach (bool speculative in new[] { false, true })
            {
                var variant = PhysXVariant.Baseline;
                if (speculative) variant.Ccd = CollisionDetectionMode.ContinuousSpeculative;
                var factory = new PhysXBallSimulationFactory(variant);
                var trace = new StringBuilder("step,position,velocity,contacts\n");
                try
                {
                    using (var sim = factory.Create(config.Physics, config.Court))
                    {
                        sim.Reset(scenario.Start); var contacts = new List<ContactEvent>();
                        for (int i = 1; i <= 36; i++)
                        {
                            sim.Step(contacts);
                            if (!speculative) {
                                minimumBaselineZ = Mathf.Min(minimumBaselineZ, sim.State.Position.z);
                                if (firstBaselineImpact < 0 && contacts.Any(c => c.Surface == SquashClash.Core.Court.SurfaceId.FrontWall && !c.Persistent)) firstBaselineImpact = sim.State.Step;
                            }
                            if (i >= 30) trace.AppendLine(FormattableString.Invariant($"{sim.State.Step},\"{sim.State.Position.ToString("F9", CultureInfo.InvariantCulture)}\",\"{sim.State.Velocity.ToString("F9", CultureInfo.InvariantCulture)}\",\"{string.Join("; ", contacts.Select(c => $"{c.Surface} persistent={c.Persistent} timing={c.Timing} point={c.Point.ToString("F9", CultureInfo.InvariantCulture)}"))}\""));
                        }
                    }
                }
                finally { UPhysics.gravity = savedGravity; }
                Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/ccd-s1-" + (speculative ? "speculative" : "baseline") + ".csv", trace.ToString());
                for (int i = 0; i < 120 && factory.PendingUnloads != 0; i++) yield return null;
                Assert.That(factory.PendingUnloads, Is.Zero);
            }
            Assert.That(minimumBaselineZ, Is.GreaterThanOrEqualTo(0), "S1 baseline center must not pass behind the frontis");
            Assert.That(firstBaselineImpact, Is.EqualTo(33), "S1 sweep impact must be reported in step 33");
        }
    }
}
