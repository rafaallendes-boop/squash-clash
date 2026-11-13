using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class PlayerBodyTests
    {
        private static readonly MoveSettings Move=new MoveSettings(14,5.5f,18,.01f,120);
        [Test] public void Body_ReachesTheTarget_AndStops()
        {
            var body=new PlayerBody(PlayerId.Human,new Vector2(3.2f,5.5f),Move);var target=new Vector2(1,8.5f);
            body.MoveTo(target,0,BodyPhase.Approaching,BodyPhase.Set);
            Assert.That(body.Phase,Is.EqualTo(BodyPhase.Approaching));
            float top=0;int steps=0;
            while(body.Phase!=BodyPhase.Set&&steps<2000){body.Step();steps++;top=Mathf.Max(top,body.Speed);}
            Assert.That(body.Position,Is.EqualTo(target));Assert.That(body.Velocity,Is.EqualTo(Vector2.zero));
            Assert.That(top,Is.LessThanOrEqualTo(Move.MaxSpeed+1e-4f));Assert.That(top,Is.GreaterThan(Move.MaxSpeed*.95f),"long run reaches cruise speed");
            Assert.That(steps,Is.InRange(100,200),"about one second for 3.7 m");
        }
        [Test] public void Acceleration_IsBounded()
        {
            var body=new PlayerBody(PlayerId.Human,Vector2.zero,Move);body.MoveTo(new Vector2(5,0),0,BodyPhase.Approaching,BodyPhase.Set);
            body.Step();Assert.That(body.Speed,Is.EqualTo(Move.Accel/Move.StepHz).Within(1e-5f));
            body.Step();Assert.That(body.Speed,Is.EqualTo(2*Move.Accel/Move.StepHz).Within(1e-5f));
        }
        [Test] public void PredictedArrival_MatchesExecution()
        {
            var body=new PlayerBody(PlayerId.Rival,new Vector2(3.6f,6),Move);var target=new Vector2(5.4f,2.1f);
            int predicted=body.StepsToReach(target,18,out _);
            body.MoveTo(target,18,BodyPhase.Approaching,BodyPhase.Set);
            int actual=0;while(body.Phase!=BodyPhase.Set&&actual<5000){body.Step();actual++;}
            Assert.That(predicted,Is.GreaterThan(18));Assert.That(actual,Is.EqualTo(predicted));
        }
        [Test] public void Reaction_DelaysTheNewTarget()
        {
            var start=new Vector2(3.2f,5.5f);var body=new PlayerBody(PlayerId.Human,start,Move);
            body.MoveTo(new Vector2(1,8),10,BodyPhase.Approaching,BodyPhase.Set);
            Assert.That(body.Reacting,Is.True);
            for(int i=0;i<9;i++)body.Step();
            Assert.That(body.Position,Is.EqualTo(start));Assert.That(body.Reacting,Is.True);
            body.Step();body.Step();
            Assert.That(body.Reacting,Is.False);Assert.That(body.Position,Is.Not.EqualTo(start));Assert.That(body.Phase,Is.EqualTo(BodyPhase.Approaching));
        }
        [Test] public void WhileReacting_TheBodyKeepsItsPreviousMove()
        {
            var body=new PlayerBody(PlayerId.Human,new Vector2(1,8),Move);var wait=new Vector2(3.2f,5.5f);
            body.MoveTo(wait,0,BodyPhase.Recovering,BodyPhase.Waiting);for(int i=0;i<20;i++)body.Step();
            var before=body.Position;body.MoveTo(new Vector2(5,2),12,BodyPhase.Approaching,BodyPhase.Set);
            for(int i=0;i<6;i++)body.Step();
            Assert.That(body.Phase,Is.EqualTo(BodyPhase.Recovering));
            Assert.That(Vector2.Distance(body.Position,wait),Is.LessThan(Vector2.Distance(before,wait)));
        }
        [Test] public void Recovery_ReturnsToTheWaitPosition()
        {
            var body=new PlayerBody(PlayerId.Human,new Vector2(.8f,8.8f),Move);var wait=new Vector2(3.2f,5.465f);
            body.MoveTo(wait,0,BodyPhase.Recovering,BodyPhase.Waiting);
            Assert.That(body.Phase,Is.EqualTo(BodyPhase.Recovering));
            for(int i=0;i<600&&body.Phase!=BodyPhase.Waiting;i++)body.Step();
            Assert.That(body.Phase,Is.EqualTo(BodyPhase.Waiting));Assert.That(body.Position,Is.EqualTo(wait));
        }
        [Test] public void PositionAfter_DoesNotMoveTheBody()
        {
            var body=new PlayerBody(PlayerId.Human,new Vector2(3,5),Move);
            var later=body.PositionAfter(new Vector2(3,9),0,60);
            Assert.That(body.Position,Is.EqualTo(new Vector2(3,5)));Assert.That(later.y,Is.GreaterThan(5.5f));Assert.That(later.y,Is.LessThan(9));
        }
    }
    public sealed class ReceptionPlannerTests
    {
        private static ReceptionPlan PlanFrom(RallyContext context,BallState ball,Vector2 bodyAt,PlayerId who=PlayerId.Human)
        {
            var trajectory=context.Predict(ball);
            return ReceptionPlanner.Plan(context,trajectory,RallyFixtures.Rules(context,who.Other(),who),RallyFixtures.Body(context,who,bodyAt),context.Steps(context.Params.ReactionS),1);
        }
        [Test] public void NeutralLaunch_IsReachableFromTheT_WithSlack()
        {
            var context=RallyFixtures.Context();
            var plan=PlanFrom(context,context.NeutralLaunch(PlayerId.Human),context.WaitPosition(PlayerId.Human));
            Assert.That(plan.Reachable,Is.True,plan.Reason);Assert.That(plan.Contact.Exists,Is.True);Assert.That(plan.Contact.TStarStep,Is.GreaterThan(0));
            Assert.That(plan.ArrivalStep,Is.LessThanOrEqualTo(plan.Contact.TStarStep));Assert.That(plan.SlackS,Is.GreaterThanOrEqualTo(0));
            Assert.That(plan.Quality,Is.InRange(0f,1f));Assert.That(plan.BodyAtContact,Is.EqualTo(plan.Stance));
            float margin=(float)context.Params.BodyWallMargin;
            Assert.That(plan.Stance.x,Is.InRange(margin,context.Court.Width-margin));Assert.That(plan.Stance.y,Is.InRange(margin,context.Court.Length-margin));
            Assert.That(Vector3.Distance(plan.Contact.PStar,new Vector3(plan.Stance.x,.9f,plan.Stance.y)),Is.LessThanOrEqualTo(plan.Contact.Body.Reach+1e-4f));
        }
        [Test] public void ShortBall_FromTheBackCorner_IsUnreachable()
        {
            var context=RallyFixtures.Context();
            var plan=PlanFrom(context,RallyFixtures.ShortBall(context),new Vector2(5.8f,9.2f));
            Assert.That(plan.Reachable,Is.False);Assert.That(plan.Quality,Is.EqualTo(0));
            Assert.That(plan.Stance,Is.Not.EqualTo(new Vector2(5.8f,9.2f)),"the body still makes its best effort");
        }
        [Test] public void ShortBall_FromNearby_IsReachable()
        {
            var context=RallyFixtures.Context();
            var plan=PlanFrom(context,RallyFixtures.ShortBall(context),new Vector2(1.8f,2.4f));
            Assert.That(plan.Reachable,Is.True,plan.Reason);
        }
        [Test] public void StartingFarther_LowersTheSlack()
        {
            var context=RallyFixtures.Context();var ball=context.NeutralLaunch(PlayerId.Human);
            var near=PlanFrom(context,ball,context.WaitPosition(PlayerId.Human));
            var far=PlanFrom(context,ball,new Vector2(5.9f,1.2f));
            Assert.That(near.Reachable,Is.True);
            Assert.That(!far.Reachable||far.SlackS<near.SlackS,Is.True);
        }
        [Test] public void NoExtraAssist_SlowerBodyHasLessSlack()
        {
            var slow=new RallyParams{MaxSpeed=2.2,Accel=6};
            var fast=RallyFixtures.Context();var limited=RallyFixtures.Context(slow);var ball=fast.NeutralLaunch(PlayerId.Human);var at=new Vector2(5.6f,3);
            var a=PlanFrom(fast,ball,at);var b=PlanFrom(limited,ball,at);
            Assert.That(a.Reachable,Is.True,a.Reason);Assert.That(!b.Reachable||b.SlackS<a.SlackS,Is.True);
        }
        [Test] public void HumanAndRival_GetTheSamePlan_FromTheSameSituation()
        {
            var context=RallyFixtures.Context();var ball=context.NeutralLaunch(PlayerId.Human);var at=new Vector2(4,6.2f);
            var human=PlanFrom(context,ball,at,PlayerId.Human);var rival=PlanFrom(context,ball,at,PlayerId.Rival);
            Assert.That(rival.Reachable,Is.EqualTo(human.Reachable));Assert.That(rival.Stance,Is.EqualTo(human.Stance));
            Assert.That(rival.ArrivalStep,Is.EqualTo(human.ArrivalStep));Assert.That(rival.Contact.TStarStep,Is.EqualTo(human.Contact.TStarStep));
            Assert.That(rival.Contact.KIn.Steps,Is.EqualTo(human.Contact.KIn.Steps));Assert.That(rival.Contact.KOut.Steps,Is.EqualTo(human.Contact.KOut.Steps));
            Assert.That(rival.Contact.Body.Reach,Is.EqualTo(human.Contact.Body.Reach));
        }
        [Test] public void FaultyTrajectory_HasNoLegalWindow()
        {
            var context=RallyFixtures.Context();var from=new Vector3(3.2f,1,5);
            var center=Ballistics.CenterForContact(Core.Court.SurfaceId.FrontWall,new Vector3(3.2f,.3f,0),context.Physics.BallRadius);
            var ball=new BallState{Position=from,Velocity=Ballistics.SolveCenterTo(from,center,14,context.Physics.Gravity)};
            var plan=PlanFrom(context,ball,context.WaitPosition(PlayerId.Human));
            Assert.That(plan.Reachable,Is.False);Assert.That(plan.Reason,Is.EqualTo("no-legal-window"));Assert.That(plan.Contact.Exists,Is.False);
        }
        [Test] public void Planning_DoesNotMoveTheBody_NorTheRules()
        {
            var context=RallyFixtures.Context();var body=RallyFixtures.Body(context,PlayerId.Human,context.WaitPosition(PlayerId.Human));
            var rules=RallyFixtures.Rules(context,PlayerId.Rival,PlayerId.Human);var before=body.Position;
            ReceptionPlanner.Plan(context,context.Predict(context.NeutralLaunch(PlayerId.Human)),rules,body,18,1);
            Assert.That(body.Position,Is.EqualTo(before));Assert.That(body.Phase,Is.EqualTo(BodyPhase.Waiting));
            Assert.That(rules.FrontWallReached,Is.False);Assert.That(rules.FloorBounces,Is.EqualTo(0));
        }
        [Test] public void WaitPositions_AreInsideTheCourt_AndRivalWaitsBehind()
        {
            var context=RallyFixtures.Context();
            Assert.That(context.WaitPosition(PlayerId.Rival).y-context.WaitPosition(PlayerId.Human).y,Is.EqualTo(.5f).Within(1e-5f));
            Assert.That(context.T.x,Is.EqualTo(3.2f).Within(1e-5f));Assert.That(context.T.y,Is.EqualTo(5.465f).Within(1e-4f));
        }
        [Test] public void Params_HaveAStableHash()
        {
            Assert.That(new RallyParams().Hash,Is.EqualTo(new RallyParams().Hash));
            Assert.That(new RallyParams{MaxSpeed=5}.Hash,Is.Not.EqualTo(new RallyParams().Hash));
        }
    }
}
