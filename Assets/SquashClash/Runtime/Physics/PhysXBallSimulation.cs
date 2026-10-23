using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace SquashClash.Runtime.Physics
{
    public sealed class PhysXBallSimulation : IBallSimulation
    {
        private readonly Scene scene;
        private readonly PhysicsScene physicsScene;
        private readonly PhysicsParams parameters;
        private readonly CourtDimensions court;
        private readonly PhysXVariant variant;
        private readonly List<PhysicsMaterial> materials = new List<PhysicsMaterial>();
        private readonly Action released, unloading, unloaded;
        private Rigidbody body;
        private long step;
        private bool disposed;
        private BallState beforeStep;
        private List<ContactEvent> currentContacts;
        public BackendCapabilities Capabilities => new BackendCapabilities { Timing = ContactTiming.Estimated,
            ImpactCapConfigurable = false, RunsInEditMode = false };
        public BallState State
        {
            get { CheckAlive(); return new BallState { Position = body.position, Velocity = body.linearVelocity, Step = step }; }
        }
        internal PhysXBallSimulation(PhysicsParams p, CourtDimensions d, PhysXVariant v, Action released, Action unloading, Action unloaded)
        {
            parameters = new PhysicsParams { Gravity = p.Gravity, BallRadius = p.BallRadius, BallMass = p.BallMass,
                StepHz = p.StepHz, MaxImpactsPerStep = p.MaxImpactsPerStep, Floor = Copy(p.Floor), FrontWall = Copy(p.FrontWall),
                SideWalls = Copy(p.SideWalls), BackWall = Copy(p.BackWall) };
            court = new CourtDimensions { Length = d.Length, Width = d.Width, WallHeight = d.WallHeight, LineWidth = d.LineWidth,
                FrontOutLower = d.FrontOutLower, ServiceLower = d.ServiceLower, BackOutLower = d.BackOutLower, TinTop = d.TinTop,
                ShortLineNearBackEdgeFromBack = d.ShortLineNearBackEdgeFromBack, ServiceBoxInner = d.ServiceBoxInner };
            variant = v.Copy(); this.released = released; this.unloading = unloading; this.unloaded = unloaded;
            scene = SceneManager.CreateScene("P1-PhysX-" + Guid.NewGuid().ToString("N"), new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            physicsScene = scene.GetPhysicsScene();
            try
            {
                var floor = Material("floor", parameters.Floor); var front = Material("front", parameters.FrontWall);
                var sides = Material("sides", parameters.SideWalls); var back = Material("back", parameters.BackWall);
                Box(SurfaceId.Floor, new Vector3(d.Width / 2, -0.5f, d.Length / 2), new Vector3(d.Width + 2, 1, d.Length + 2), floor);
                Box(SurfaceId.FrontWall, new Vector3(d.Width / 2, d.WallHeight / 2, -0.5f), new Vector3(d.Width + 2, d.WallHeight, 1), front);
                Box(SurfaceId.BackWall, new Vector3(d.Width / 2, d.WallHeight / 2, d.Length + 0.5f), new Vector3(d.Width + 2, d.WallHeight, 1), back);
                Box(SurfaceId.LeftWall, new Vector3(d.Width + 0.5f, d.WallHeight / 2, d.Length / 2), new Vector3(1, d.WallHeight, d.Length + 2), sides);
                Box(SurfaceId.RightWall, new Vector3(-0.5f, d.WallHeight / 2, d.Length / 2), new Vector3(1, d.WallHeight, d.Length + 2), sides);
                var ball = LocalObject("Ball");
                ball.transform.position = new Vector3(d.Width / 2, 1, d.Length / 2);
                var sphere = ball.AddComponent<SphereCollider>(); sphere.radius = parameters.BallRadius;
                sphere.contactOffset = variant.ContactOffset;
                sphere.sharedMaterial = Material("ball", new SurfaceMaterial { Restitution = 1, TangentialFriction = 1 });
                body = ball.AddComponent<Rigidbody>(); body.mass = parameters.BallMass;
                body.linearDamping = 0; body.angularDamping = 0; body.constraints = RigidbodyConstraints.FreezeRotation;
                body.collisionDetectionMode = variant.Ccd; body.interpolation = RigidbodyInterpolation.None; body.sleepThreshold = 0;
                body.solverIterations = variant.SolverIterations; body.solverVelocityIterations = variant.SolverVelocityIterations;
                ball.AddComponent<PhysXContactCollector>().Owner = this;
                UnityEngine.Physics.SyncTransforms();
            }
            catch
            {
                foreach (var material in materials) Object.Destroy(material);
                SceneManager.UnloadSceneAsync(scene);
                throw;
            }
        }
        private static SurfaceMaterial Copy(SurfaceMaterial m) => new SurfaceMaterial { Restitution = m.Restitution, TangentialFriction = m.TangentialFriction };
        private GameObject LocalObject(string name)
        {
            var item = new GameObject(name); SceneManager.MoveGameObjectToScene(item, scene); return item;
        }
        private void Box(SurfaceId surface, Vector3 position, Vector3 size, PhysicsMaterial material)
        {
            var item = LocalObject(surface.ToString()); item.transform.position = position;
            item.AddComponent<SurfaceTag>().Surface = surface;
            var collider = item.AddComponent<BoxCollider>(); collider.size = size; collider.contactOffset = variant.ContactOffset;
            collider.sharedMaterial = material;
        }
        private PhysicsMaterial Material(string name, SurfaceMaterial m)
        {
            var material = new PhysicsMaterial("P1-" + name) { bounciness = m.Restitution,
                dynamicFriction = m.TangentialFriction, staticFriction = m.TangentialFriction,
                bounceCombine = PhysicsMaterialCombine.Multiply, frictionCombine = PhysicsMaterialCombine.Multiply };
            materials.Add(material); return material;
        }
        public void Reset(in BallState initial)
        {
            CheckAlive(); body.position = initial.Position; body.rotation = Quaternion.identity;
            body.linearVelocity = initial.Velocity; body.angularVelocity = Vector3.zero; body.WakeUp(); step = initial.Step;
        }
        public void Step(List<ContactEvent> contactsThisStep)
        {
            CheckAlive();
            if (contactsThisStep == null) throw new ArgumentNullException(nameof(contactsThisStep));
            contactsThisStep.Clear(); beforeStep = State; currentContacts = contactsThisStep; step++;
            try { physicsScene.Simulate((float)parameters.StepSeconds); }
            finally { currentContacts = null; }
            if (contactsThisStep.Count > 1) contactsThisStep.Sort(PhysXContactOrder.Instance);
        }
        internal void Record(Collision collision, bool persistent)
        {
            if (currentContacts == null || collision.contactCount == 0 || !collision.collider.TryGetComponent<SurfaceTag>(out var tag)) return;
            var contact = collision.GetContact(0);
            var timing = ContactTiming.StepOnly; float time = float.NaN;
            if (!persistent && Ballistics.FirstPlaneHit(beforeStep, parameters.Gravity, parameters.BallRadius, court,
                parameters.StepSeconds, out var surface, out var t) && surface == tag.Surface)
            { timing = ContactTiming.Estimated; time = (float)t; }
            currentContacts.Add(new ContactEvent(tag.Surface, contact.point, court.ToSurfaceUV(tag.Surface, contact.point),
                contact.normal, step, time, timing, -collision.relativeVelocity, persistent));
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; currentContacts = null;
            if (scene.IsValid() && scene.isLoaded) foreach (var item in scene.GetRootGameObjects()) item.SetActive(false);
            foreach (var material in materials) Object.Destroy(material);
            materials.Clear(); released();
            if (!scene.IsValid() || !scene.isLoaded) return;
            var operation = SceneManager.UnloadSceneAsync(scene);
            if (operation == null) return;
            unloading(); operation.completed += _ => unloaded();
        }
        private void CheckAlive() { if (disposed) throw new ObjectDisposedException(nameof(PhysXBallSimulation)); }
    }
}
