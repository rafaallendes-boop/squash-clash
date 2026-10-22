using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
namespace SquashClash.Core.Simulation
{
    public static class TrajectoryRunner
    {
        public static Trajectory Run(IBallSimulation sim, in BallState from, int maxSteps, StopCondition stop, CourtDimensions d)
        {
            if (sim == null) throw new ArgumentNullException(nameof(sim));
            if (maxSteps < 0) throw new ArgumentOutOfRangeException(nameof(maxSteps));
            var trajectory = new Trajectory();
            var contacts = new List<ContactEvent>();
            sim.Reset(from);
            trajectory.States.Add(sim.State);
            int bounces = 0;
            for (int i = 0; i < maxSteps; i++)
            {
                contacts.Clear(); sim.Step(contacts);
                var state = sim.State;
                trajectory.States.Add(state); trajectory.Contacts.AddRange(contacts);
                foreach (var contact in contacts) if (contact.Surface == SurfaceId.Floor && !contact.Persistent) bounces++;
                if (state.Position.y > d.WallHeight) { trajectory.Exit = TrajectoryExit.OverTop; break; }
                if (stop == StopCondition.SecondFloorBounce && bounces >= 2) break;
            }
            return trajectory;
        }
    }
}
