using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Simulation {
    public sealed class TrajectoryRunnerTests {
        private sealed class FloorEveryTen : IBallSimulation {
            public BackendCapabilities Capabilities=>new BackendCapabilities();
            public BallState State {get;private set;}
            public void Reset(in BallState state) { State=state; }
            public void Step(List<ContactEvent> contacts) {
                contacts.Clear(); var s=State; s.Step++; s.Position+=s.Velocity; State=s;
                if(s.Step%10==0 || s.Step%10==5)
                    contacts.Add(new ContactEvent(SurfaceId.Floor,Vector3.zero,Vector2.zero,Vector3.up,s.Step,float.NaN,ContactTiming.StepOnly,Vector3.down,s.Step%10==5));
            }
            public void Dispose() { }
        }
        [Test] public void Run_SecondBounce_StopsAtTwenty_KeepingPersistentSamples() {
            using(var sim=new FloorEveryTen()) {
                var t=TrajectoryRunner.Run(sim,new BallState {Position=Vector3.one},100,StopCondition.SecondFloorBounce,CourtDimensions.Standard());
                Assert.That(t.States.Count,Is.EqualTo(21)); Assert.That(t.States[0].Step,Is.Zero);
                Assert.That(t.States[20].Step,Is.EqualTo(20)); Assert.That(t.Contacts.Count,Is.EqualTo(4));
                Assert.That(t.Contacts[0].Persistent,Is.True); Assert.That(t.Exit,Is.EqualTo(TrajectoryExit.None));
            }
        }
        [Test] public void Run_AboveWallHeight_RecordsOverTopWithoutInventingCeilingContact() {
            using(var sim=new FloorEveryTen()) {
                var t=TrajectoryRunner.Run(sim,new BallState {Position=new Vector3(3,5.9f,5),Velocity=new Vector3(0,0.2f,0)},25,StopCondition.MaxSteps,CourtDimensions.Standard());
                Assert.That(t.Exit,Is.EqualTo(TrajectoryExit.OverTop)); Assert.That(t.States.Count,Is.EqualTo(2)); Assert.That(t.Contacts,Is.Empty);
            }
        }
        [Test] public void Run_MaxSteps_DoesNotStopAtSecondBounce() {
            using(var sim=new FloorEveryTen()) {
                var t=TrajectoryRunner.Run(sim,new BallState(),25,StopCondition.MaxSteps,CourtDimensions.Standard());
                Assert.That(t.States.Count,Is.EqualTo(26));
            }
        }
        [Test] public void Run_ResetsToExactInitialState_AndZeroStepsStillHasInitialState() {
            using(var sim=new FloorEveryTen()) {
                var from=new BallState {Position=new Vector3(2,3,4),Velocity=new Vector3(1,2,3),Step=7};
                var t=TrajectoryRunner.Run(sim,from,0,StopCondition.MaxSteps,CourtDimensions.Standard());
                Assert.That(t.States.Count,Is.EqualTo(1)); Assert.That(t.States[0].Position,Is.EqualTo(from.Position));
                Assert.That(sim.State.Step,Is.EqualTo(7));
            }
        }
    }
}
