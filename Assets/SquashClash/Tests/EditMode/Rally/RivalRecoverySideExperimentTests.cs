using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // [EXPERIMENTO] set17 (27 nov; local branch, never shipped): the rival recovers towards the side of its own deep ball.
    public class RivalRecoverySideExperimentTests
    {
        private static RallyContext Context(double shift=0){var p=new RallyParams{RivalRecoverySideShift=shift};return RallyFixtures.Context(p);}
        private static RivalAi Ai(RallyContext c,RivalStyleId style)=>new RivalAi(c,AiDifficulty.For(c.Params,AiLevel.Medium),style,1);
        private static AiObservation After(RallyContext c,BallState b)=>new AiObservation(b,c.WaitPosition(PlayerId.Rival),Vector2.zero,new Vector2(3,7),Vector2.zero,3,0,true);
        private static Vector3 Bounce(RallyContext c,BallState b)=>c.Predict(b).Contacts.First(x=>x.Surface==SurfaceId.Floor).Point;
        private static readonly RivalStyleId[] Styles={RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo};

        [Test] public void Default_IsZero_AndTheRecoveryAfterADeepBallIsTheStyleWait()
        {
            Assert.That(new RallyParams().RivalRecoverySideShift,Is.EqualTo(0));
            var c=Context();
            foreach(var ball in new[]{c.NeutralLaunch(PlayerId.Human),c.NeutralLaunch(PlayerId.Rival)})
            {
                Assert.That(Bounce(c,ball).z,Is.GreaterThanOrEqualTo((float)c.Params.StyleCoverBounceZ));
                foreach(var s in Styles){var ai=Ai(c,s);Assert.That(ai.Recovery(After(c,ball)),Is.EqualTo(ai.Wait),s.ToString());}
            }
        }
        [Test] public void WithShift_ADeepBallOnTheLeftOfTheT_RecoversToTheLeftAtTheStyleWaitDepth()
        {
            var c=Context(.8);var ball=c.NeutralLaunch(PlayerId.Human);var b=Bounce(c,ball);
            Assert.That(b.z,Is.GreaterThanOrEqualTo((float)c.Params.StyleCoverBounceZ));Assert.That(b.x,Is.LessThan(c.T.x));
            foreach(var s in Styles){var ai=Ai(c,s);Assert.That(ai.Recovery(After(c,ball)),Is.EqualTo(c.ClampBody(new Vector2(c.T.x-.8f,ai.Wait.y))),s.ToString());}
        }
        [Test] public void WithShift_ADeepBallOnTheRightOfTheT_RecoversToTheRightAtTheStyleWaitDepth()
        {
            var c=Context(.8);var ball=c.NeutralLaunch(PlayerId.Rival);var b=Bounce(c,ball);
            Assert.That(b.z,Is.GreaterThanOrEqualTo((float)c.Params.StyleCoverBounceZ));Assert.That(b.x,Is.GreaterThan(c.T.x));
            foreach(var s in Styles){var ai=Ai(c,s);Assert.That(ai.Recovery(After(c,ball)),Is.EqualTo(c.ClampBody(new Vector2(c.T.x+.8f,ai.Wait.y))),s.ToString());}
        }
        [Test] public void WithShift_AShortBall_RecoversExactlyAsWithoutShift()
        {
            var c0=Context();var c1=Context(.8);var ball=RallyFixtures.ShortBall(c1);
            Assert.That(Bounce(c1,ball).z,Is.LessThan((float)c1.Params.StyleCoverBounceZ));
            foreach(var s in Styles)Assert.That(Ai(c1,s).Recovery(After(c1,ball)),Is.EqualTo(Ai(c0,s).Recovery(After(c0,ball))),s.ToString());
        }
        [Test] public void TheHumanWaitPosition_IsUntouched()
            =>Assert.That(Context(.8).WaitPosition(PlayerId.Human),Is.EqualTo(Context().WaitPosition(PlayerId.Human)));
    }
}
