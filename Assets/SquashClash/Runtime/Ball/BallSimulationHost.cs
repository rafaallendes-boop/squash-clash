using System;
using System.Collections.Generic;
using System.Linq;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Runtime.Ball
{
    public sealed partial class BallSimulationHost : MonoBehaviour
    {
        private IReadOnlyList<IBallSimulationFactory> backends;
        private IBallSimulation game, predictor;
        private BallState previous, current;
        private readonly List<ContactEvent> contacts = new List<ContactEvent>();
        private int floorBounces, maxSteps;
        private int discardDeltaFrame = -1;
        private float wallHeight;
        private BallState launchState;
        private StopCondition stopCondition;
        private int launchMaxSteps;
        public SimClock Clock { get; private set; }
        public EffectiveConfig ActiveConfig { get; private set; }
        public int SimulationGeneration { get; private set; }
        public BallState Current => current;
        public Vector3 RenderPosition { get; private set; }
        public Trajectory Prediction { get; private set; }
        public bool PredictionMatchesBitwise { get; private set; }
        public bool IsRunning { get; private set; }
        public event Action<ContactEvent> ContactRaised;
        public event Action<EffectiveConfig> Launched;
        public void Initialize(IReadOnlyList<IBallSimulationFactory> backends, SimClock clock)
        {
            if (game != null) throw new InvalidOperationException("Cannot reinitialize a live host.");
            this.backends = backends?.ToArray() ?? throw new ArgumentNullException(nameof(backends));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }
        public void Launch(EffectiveConfig cfg)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));
            Launch(cfg, Launcher.ToState(cfg.Launch, cfg.LaunchScale), cfg.Physics.StepHz * 10, StopCondition.SecondFloorBounce);
        }
        public void Launch(EffectiveConfig cfg, BallState initial, int horizonSteps, StopCondition stop)
        {
            if (horizonSteps <= 0) throw new ArgumentOutOfRangeException(nameof(horizonSteps));
            if (backends == null) throw new InvalidOperationException("Initialize the host before launch.");
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));
            var physics = cfg.Physics; var court = cfg.Court;
            // Store derived limits as float fields, just like BallState, before comparison (Mono may retain arithmetic precision).
            var maximumCenter=new Vector3(court.Width-physics.BallRadius,0,court.Length-physics.BallRadius);
            // P2 Reset assumes a valid center inside the floor/lateral bounds. Validate before mutating the live host.
            if(!Finite(initial.Position)||!Finite(initial.Velocity)||initial.Step<0||initial.Step>int.MaxValue-horizonSteps||
                initial.Position.x<physics.BallRadius||initial.Position.x>maximumCenter.x||
                initial.Position.z<physics.BallRadius||initial.Position.z>maximumCenter.z||initial.Position.y<physics.BallRadius)
                throw new ArgumentOutOfRangeException(nameof(initial),"Launch state must be finite and inside the court center bounds.");
            if (game == null || ActiveConfig.SimulationKey != cfg.SimulationKey)
            {
                DisposeSimulations();
                var factory = backends.Single(b => b.Name == cfg.Backend);
                try { game = factory.Create(physics, court); predictor = factory.Create(physics, court); }
                catch { DisposeSimulations(); throw; }
                SimulationGeneration++;
            }
            Clock.Configure(physics.StepHz, cfg.Tempo); Clock.Restart();
            launchState = initial; launchMaxSteps = horizonSteps; stopCondition = stop;
            game.Reset(initial); predictor.Reset(initial);
            // Bounded preview, not a physics-quality or performance acceptance criterion.
            maxSteps = checked((int)initial.Step + horizonSteps); wallHeight = court.WallHeight;
            Prediction = TrajectoryRunner.Run(predictor, initial, horizonSteps, stop, court);
            previous = current = initial; RenderPosition = initial.Position;
            floorBounces = 0; PredictionMatchesBitwise = true; IsRunning = true;
            ActiveConfig = cfg; BeginShotLaunch(); Launched?.Invoke(cfg);
        }
        public void Replay() { if (ActiveConfig != null) Replay(ActiveConfig); }
        public void Replay(EffectiveConfig cfg) { if(ActiveConfig!=null)Launch(cfg,launchState,launchMaxSteps,stopCondition); }
        public void ReleaseForExperiment() { Clock.Paused = true; DisposeSimulations(); }
        public void ResumeAfterSystemPause()
        {
            Clock.Paused = false;
            // The input callback can run before Update on the first resumed frame.
            discardDeltaFrame = Time.frameCount;
        }
        private void Update() => AdvanceCore(discardDeltaFrame == Time.frameCount ? 0 : Time.unscaledDeltaTime, Time.realtimeSinceStartupAsDouble);
        public void Advance(double realDeltaSeconds)
            => AdvanceCore(realDeltaSeconds,null);
        private void AdvanceCore(double realDeltaSeconds,double? observedAt)
        {
            if(game==null)return;
            if(double.IsNaN(realDeltaSeconds)||double.IsInfinity(realDeltaSeconds)||realDeltaSeconds<0)throw new ArgumentOutOfRangeException(nameof(realDeltaSeconds));
            int count=IsRunning?Clock.Advance(realDeltaSeconds):0;
            ObserveClock(realDeltaSeconds,count,observedAt);
            if(!IsRunning)return;
            for (int i = 0; i < count; i++)
            {
                previous = current; game.Step(contacts); current = game.State;
                long index = current.Step - Prediction.States[0].Step;
                if (index < 0 || index >= Prediction.States.Count || !SameBits(current, Prediction.States[(int)index])) PredictionMatchesBitwise = false;
                foreach (var contact in contacts)
                {
                    if (contact.Surface == SurfaceId.Floor && !contact.Persistent) floorBounces++;
                    if(!hasShot&&contact.Surface==SurfaceId.Floor&&!contact.Persistent)ReceptionBounces++;
                    canonicalContacts.Add(contact);
                    ContactRaised?.Invoke(contact);
                }
                RecordHistorical(current);
                if(scheduledVelocity!=null&&current.Step==scheduledStep)
                {
                    var callback=scheduledVelocity;scheduledVelocity=null;
                    var velocity=callback(current);
                    var result=ApplyShotCore(LaunchId,current.Step,velocity,scheduledRelease,true);
                    if(!result.Applied)ShotApplied?.Invoke(result);
                    // ApplyShotCore already reconstructed the remaining full steps to this frame's present.
                    if(result.Applied)break;
                }
                if ((stopCondition == StopCondition.SecondFloorBounce && floorBounces >= 2) || current.Position.y > wallHeight || current.Step >= maxSteps)
                { IsRunning = false; break; }
            }
            if (!IsRunning) RenderPosition = current.Position;
            else if (Clock.Paused)
            {
                // No step: freeze the last interpolated image. A requested step
                // presents current and rebases history so Continue cannot rewind.
                if (count > 0) { previous = current; RenderPosition = current.Position; }
            }
            else RenderPosition = Vector3.Lerp(previous.Position, current.Position, Clock.Alpha);
        }
        private static bool Finite(Vector3 v)=>Finite(v.x)&&Finite(v.y)&&Finite(v.z);
        private static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
        private static bool SameBits(BallState a, BallState b) => a.Step == b.Step &&
            Same(a.Position.x,b.Position.x) && Same(a.Position.y,b.Position.y) && Same(a.Position.z,b.Position.z) &&
            Same(a.Velocity.x,b.Velocity.x) && Same(a.Velocity.y,b.Velocity.y) && Same(a.Velocity.z,b.Velocity.z);
        private static bool Same(float a, float b) => BitConverter.SingleToInt32Bits(a) == BitConverter.SingleToInt32Bits(b);
        private void DisposeSimulations() { game?.Dispose(); predictor?.Dispose(); game = null; predictor = null; IsRunning = false; }
        private void OnDestroy() => DisposeSimulations();
    }
}
