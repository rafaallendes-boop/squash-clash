using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // Serve positions (spec docs/superpowers/specs/2026-11-26-posiciones-saque.md): the server stands in the inner front corner of its
    // box and the receiver in the opposite back quarter, as the real start of the simulated bodies.
    public sealed class ServePositionsTests
    {
        // The 1.80 m reference capsule of PlaceholderPlayers.
        private const float CapsuleRadius=.28f,CapsuleHeight=1.8f;
        private sealed class StartProbe : IRallyBrain
        {
            private readonly IRallyBrain inner;public RallySim Sim;public Vector2? Start;
            public StartProbe(IRallyBrain inner){this.inner=inner;}
            public int PerceptionSteps=>inner.PerceptionSteps;public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {if(Start==null&&Sim!=null&&Sim.Rules.Serving&&o.Shots==0)Start=o.SelfPosition;return inner.Decide(o,own);}
        }
        [TestCase(ServiceBox.Right,1.30f,5.79f)]
        [TestCase(ServiceBox.Left,5.10f,5.79f)]
        public void Server_StandsInTheInnerFrontCornerOfItsBox(ServiceBox box,float x,float z)
        {
            var context=RallyFixtures.ContextWithoutFatigue();var at=ServePlanner.BoxPosition(context,box);
            Assert.That(at.x,Is.EqualTo(x).Within(1e-4f));Assert.That(at.y,Is.EqualTo(z).Within(1e-4f));
        }
        [TestCase(ServiceBox.Right,4.80f,7.65f)]
        [TestCase(ServiceBox.Left,1.60f,7.65f)]
        public void Receiver_StandsInTheOppositeBackQuarter(ServiceBox box,float x,float z)
        {
            var context=RallyFixtures.ContextWithoutFatigue();var at=ServePlanner.ReceiverPosition(context,box);
            Assert.That(at.x,Is.EqualTo(x).Within(1e-4f));Assert.That(at.y,Is.EqualTo(z).Within(1e-4f));
            Assert.That(context.ClampBody(at),Is.EqualTo(at),"inside the body margins");
        }
        [TestCase(PlayerId.Human,ServiceBox.Right)]
        [TestCase(PlayerId.Human,ServiceBox.Left)]
        [TestCase(PlayerId.Rival,ServiceBox.Right)]
        [TestCase(PlayerId.Rival,ServiceBox.Left)]
        public void PlayPoint_TheReceiverStartsTheServeReceptionFromItsPosition(PlayerId server,ServiceBox box)
        {
            var context=RallyFixtures.ContextWithoutFatigue();
            var human=new StartProbe(RallySimTests.Bot(context,3));var rival=new StartProbe(RallySimTests.Fondo(context,4));
            using(var sim=new RallySim(context,human,rival))
            {
                human.Sim=rival.Sim=sim;
                var outcome=sim.PlayPoint(server,box);
                var receiver=server==PlayerId.Human?rival:human;
                Assert.That(outcome.Receptions[0].Who,Is.EqualTo(server.Other()));
                Assert.That(receiver.Start.HasValue,Is.True,"the receiver decided on the serve");
                Assert.That(Vector2.Distance(receiver.Start.Value,ServePlanner.ReceiverPosition(context,box)),Is.LessThan(1e-5f));
            }
        }
        // The cross-court serve does not go through the receiver: before its first bounce the ball never enters the reference capsule
        // standing at the receiver position, for every serve of the product from both boxes.
        [TestCase(ServiceBox.Right,ServeType.Auto)]
        [TestCase(ServiceBox.Left,ServeType.Auto)]
        [TestCase(ServiceBox.Right,ServeType.Lob)]
        [TestCase(ServiceBox.Left,ServeType.Lob)]
        [TestCase(ServiceBox.Right,ServeType.Drive)]
        [TestCase(ServiceBox.Left,ServeType.Drive)]
        public void CrossCourtServe_DoesNotGoThroughTheReceiver(ServiceBox box,ServeType type)
        {
            var context=RallyFixtures.Context(new RallyParams{ServeKindOn=type==ServeType.Auto?0:1});
            var serve=context.Serve(box,type);Assert.That(ServePlanner.Forecast(context,serve,box,out _),Is.True,"a legal serve");
            var t=context.Predict(serve);var receiver=ServePlanner.ReceiverPosition(context,box);float r=context.Physics.BallRadius;
            long bounce=long.MaxValue;foreach(var c in t.Contacts)if(!c.Persistent&&c.Surface==SurfaceId.Floor){bounce=c.Step;break;}
            Assert.That(bounce,Is.LessThan(long.MaxValue),"the serve bounces");
            foreach(var s in t.States)
            {
                if(s.Step>bounce)break;
                if(s.Position.y-r>CapsuleHeight)continue;
                float d=Vector2.Distance(new Vector2(s.Position.x,s.Position.z),receiver);
                Assert.That(d,Is.GreaterThanOrEqualTo(CapsuleRadius+r),"step "+s.Step+" at height "+s.Position.y);
            }
        }
    }
}
