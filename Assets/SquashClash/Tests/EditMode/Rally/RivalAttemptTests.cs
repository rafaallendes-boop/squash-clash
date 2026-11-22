using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class RivalAttemptTests
    {
        private static ContactEvent Contact(SurfaceId surface,Vector3 point,long step)
            =>new ContactEvent(surface,point,default,Vector3.up,step,0,ContactTiming.Exact,Vector3.down,false);
        private static Trajectory ShortBall()
        {
            var t=new Trajectory();
            for(int i=0;i<6;i++)t.States.Add(new BallState{Step=100+i,Position=new Vector3(1,.2f,1+Mathf.Abs(i-3))});
            t.Contacts.Add(Contact(SurfaceId.FrontWall,new Vector3(1,1,0),100));
            t.Contacts.Add(Contact(SurfaceId.Floor,new Vector3(1,0,3),101));
            t.Contacts.Add(Contact(SurfaceId.Floor,new Vector3(1,0,3),105));
            return t;
        }
        [Test] public void RivalAttempt_StepFor_IsClosestApproachAfterBounce()
        {
            var c=RallyFixtures.Context();var t=ShortBall();var path=new Vector2[6];
            for(int i=0;i<path.Length;i++)path[i]=new Vector2(2,1);
            var rules=RallyFixtures.Rules(c,PlayerId.Human,PlayerId.Rival);
            var attempt=RivalAttempt.StepFor(c,t,rules,path);
            Assert.That(attempt.Step,Is.EqualTo(103),"R1: the missed reception must still have a visible attempt at closest approach");
            Assert.That(attempt.Step,Is.LessThanOrEqualTo(105));
            Assert.That(attempt.Side,Is.EqualTo(StrokeSide.Drive));
            Assert.That(rules.Closed,Is.False,"the visual attempt never mutates live rules");
        }
        [Test] public void RivalAttempt_StepFor_NoneWhenPointClosedBeforeBounce()
        {
            var c=RallyFixtures.Context();var t=ShortBall();
            t.Contacts[0]=Contact(SurfaceId.FrontWall,new Vector3(1,.1f,0),100);
            Assert.That(RivalAttempt.StepFor(c,t,RallyFixtures.Rules(c,PlayerId.Human,PlayerId.Rival),new Vector2[6]).Step,Is.EqualTo(-1));
        }
    }
}
