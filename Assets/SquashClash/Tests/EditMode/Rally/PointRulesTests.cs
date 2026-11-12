using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class PointRulesTests
    {
        private const float R=.02f;
        private static readonly CourtDimensions Court=CourtDimensions.Standard();
        private static PointRules New(PlayerId? striker=PlayerId.Rival,PlayerId receiver=PlayerId.Human)
        {var rules=new PointRules(Court,R);rules.Begin(striker,receiver);return rules;}
        internal static ContactEvent Hit(SurfaceId surface,float a,float b,bool persistent=false,long step=1)
        {
            Vector3 point;
            switch(surface)
            {
                case SurfaceId.Floor:point=new Vector3(a,0,b);break;
                case SurfaceId.FrontWall:point=new Vector3(a,b,0);break;
                case SurfaceId.BackWall:point=new Vector3(a,b,Court.Length);break;
                case SurfaceId.RightWall:point=new Vector3(0,b,a);break;
                default:point=new Vector3(Court.Width,b,a);break;
            }
            return new ContactEvent(surface,point,Court.ToSurfaceUV(surface,point),CourtDimensions.InwardNormal(surface),step,0,ContactTiming.Exact,Vector3.zero,persistent);
        }
        private static ContactEvent Front(float y=2)=>Hit(SurfaceId.FrontWall,3.2f,y);
        private static ContactEvent Floor()=>Hit(SurfaceId.Floor,3,6);

        [Test] public void GoodReturn_ThenSecondBounce_ReceiverLoses()
        {
            var rules=New();rules.OnBallContact(Front());rules.OnBallContact(Floor());
            Assert.That(rules.Closed,Is.False);Assert.That(rules.FloorBounces,Is.EqualTo(1));Assert.That(rules.FrontWallReached,Is.True);
            rules.OnBallContact(Floor());
            Assert.That(rules.Closed,Is.True);Assert.That(rules.Cause,Is.EqualTo(PointEndCause.DoubleBounce));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Rival));
        }
        [Test] public void FloorBeforeFrontWall_IsNotUp_StrikerLoses()
        {
            var rules=New();rules.OnBallContact(Floor());
            Assert.That(rules.Closed,Is.True);Assert.That(rules.Cause,Is.EqualTo(PointEndCause.NotUp));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Human));
        }
        [Test] public void SideAndBackWallBeforeFrontWall_AreAllowed()
        {
            var rules=New();rules.OnBallContact(Hit(SurfaceId.LeftWall,5,1));rules.OnBallContact(Hit(SurfaceId.BackWall,3,1));rules.OnBallContact(Front());
            Assert.That(rules.Closed,Is.False);Assert.That(rules.FrontWallReached,Is.True);
        }
        [Test] public void Tin_StrikerLoses()
        {
            var rules=New();rules.OnBallContact(Front(.3f));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.Tin));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Human));
            Assert.That(rules.TerminalSurface,Is.EqualTo(SurfaceId.FrontWall));Assert.That(rules.TerminalPoint.y,Is.EqualTo(.3f));
        }
        [TestCase(SurfaceId.FrontWall,3.2f,4.7f)]
        [TestCase(SurfaceId.BackWall,3.2f,2.5f)]
        [TestCase(SurfaceId.LeftWall,5f,4f)]
        [TestCase(SurfaceId.RightWall,5f,4f)]
        public void Out_OnEachWall_StrikerLoses(SurfaceId surface,float a,float b)
        {
            var rules=New();rules.OnBallContact(Hit(surface,a,b));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.Out));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Human));
        }
        [Test] public void OverTheTop_IsOut()
        {
            var rules=New();rules.OnOverTop();
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.Out));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Human));
        }
        [Test] public void PointClosesOnce_LaterEventsAreIgnoredAndCounted()
        {
            var rules=New();rules.OnBallContact(Front(.3f));
            rules.OnBallContact(Floor());rules.OnBallContact(Floor());rules.OnBallContact(Front(4.9f));rules.OnOverTop();
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.Tin));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Human));
            Assert.That(rules.IgnoredAfterClose,Is.EqualTo(4));Assert.That(rules.RacketContact(PlayerId.Human),Is.False);
        }
        [Test] public void RacketContact_ResetsBounces_AndSwitchesTurn()
        {
            var rules=New();rules.OnBallContact(Front());rules.OnBallContact(Floor());
            Assert.That(rules.CanHit(PlayerId.Human),Is.True);Assert.That(rules.CanHit(PlayerId.Rival),Is.False);
            Assert.That(rules.RacketContact(PlayerId.Human),Is.True);
            Assert.That(rules.FloorBounces,Is.EqualTo(0));Assert.That(rules.FrontWallReached,Is.False);
            Assert.That(rules.Striker,Is.EqualTo(PlayerId.Human));Assert.That(rules.ToHit,Is.EqualTo(PlayerId.Rival));Assert.That(rules.Shots,Is.EqualTo(1));
        }
        [Test] public void Turn_OnlyTheReceiverMayHit_AndOnlyOnce()
        {
            var rules=New();rules.OnBallContact(Front());rules.OnBallContact(Floor());
            Assert.That(rules.RacketContact(PlayerId.Rival),Is.False);
            Assert.That(rules.RacketContact(PlayerId.Human),Is.True);
            Assert.That(rules.RacketContact(PlayerId.Human),Is.False);
            Assert.That(rules.RejectedContacts,Is.EqualTo(2));Assert.That(rules.Shots,Is.EqualTo(1));
        }
        [Test] public void Contact_BeforeFrontWall_IsRejected()
        {
            var rules=New();
            Assert.That(rules.CanHit(PlayerId.Human),Is.False);Assert.That(rules.RacketContact(PlayerId.Human),Is.False);
        }
        [Test] public void WithoutVolley_ContactBeforeTheBounce_IsRejected()
        {
            var rules=New();rules.OnBallContact(Front());
            Assert.That(rules.CanHit(PlayerId.Human),Is.False);
            var volley=new PointRules(Court,R,true);volley.Begin(PlayerId.Rival,PlayerId.Human);volley.OnBallContact(Front());
            Assert.That(volley.CanHit(PlayerId.Human),Is.True);
        }
        [Test] public void LosingStrokeAfterAnExchange_GivesThePointToTheOpponent()
        {
            var rules=New();rules.OnBallContact(Front());rules.OnBallContact(Floor());rules.RacketContact(PlayerId.Human);
            rules.OnBallContact(Front());rules.OnBallContact(Floor());rules.RacketContact(PlayerId.Rival);
            rules.OnBallContact(Front(.47f));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.Tin));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Human));Assert.That(rules.Shots,Is.EqualTo(2));
        }
        [Test] public void RollingBallAfterOneBounce_ClosesAsDoubleBounce()
        {
            var rules=New();rules.OnBallContact(Front());rules.OnBallContact(Floor());rules.OnBallContact(Hit(SurfaceId.Floor,3,6,true));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.DoubleBounce));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Rival));
        }
        [Test] public void RollingBallBeforeFrontWall_IsNotUp()
        {
            var rules=New();rules.OnBallContact(Hit(SurfaceId.Floor,3,6,true));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.NotUp));
        }
        [Test] public void TinIsNotJudged_AfterTheFirstBounce()
        {
            var rules=New();rules.OnBallContact(Front());rules.OnBallContact(Floor());rules.OnBallContact(Front(.2f));
            Assert.That(rules.Closed,Is.False);
        }
        // WSF Rules of Singles Squash, rule 6.2.4 and the definition of OUT: a return that goes out is not good, bounced or not.
        [Test] public void OutAfterTheFirstBounce_StrikerLoses_Wsf()
        {
            var rules=New();rules.OnBallContact(Front());rules.OnBallContact(Floor());rules.OnBallContact(Hit(SurfaceId.BackWall,3,2.6f));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.Out));Assert.That(rules.OutAfterBounce,Is.True);Assert.That(rules.Winner,Is.EqualTo(PlayerId.Human));
        }
        [Test] public void OverTheTopAfterTheFirstBounce_StrikerLoses_Wsf()
        {
            var rules=New();rules.OnBallContact(Front());rules.OnBallContact(Floor());rules.OnOverTop();
            Assert.That(rules.FloorBounces,Is.EqualTo(1));
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.Out));Assert.That(rules.OutAfterBounce,Is.True);Assert.That(rules.Winner,Is.EqualTo(PlayerId.Human));
        }
        [Test] public void NeutralLaunch_OutAfterTheFirstBounce_HasNoWinner()
        {
            var wall=New(null);wall.OnBallContact(Front());wall.OnBallContact(Floor());wall.OnBallContact(Hit(SurfaceId.BackWall,3,2.6f));
            var top=New(null);top.OnBallContact(Front());top.OnBallContact(Floor());top.OnOverTop();
            foreach(var rules in new[]{wall,top})
            {Assert.That(rules.Closed,Is.True);Assert.That(rules.Cause,Is.EqualTo(PointEndCause.LaunchFault));Assert.That(rules.Winner,Is.Null);Assert.That(rules.OutAfterBounce,Is.True);}
        }
        [Test] public void NeutralLaunchFault_HasNoWinner()
        {
            var rules=New(null);rules.OnBallContact(Front(.3f));
            Assert.That(rules.Closed,Is.True);Assert.That(rules.Cause,Is.EqualTo(PointEndCause.LaunchFault));Assert.That(rules.Winner,Is.Null);
        }
        [Test] public void NeutralLaunch_DoubleBounce_ReceiverLoses()
        {
            var rules=New(null);rules.OnBallContact(Front());rules.OnBallContact(Floor());rules.OnBallContact(Floor());
            Assert.That(rules.Cause,Is.EqualTo(PointEndCause.DoubleBounce));Assert.That(rules.Winner,Is.EqualTo(PlayerId.Rival));
        }
        [Test] public void Clone_IsIndependent()
        {
            var rules=New();rules.OnBallContact(Front());var copy=rules.Clone();copy.OnBallContact(Floor());copy.OnBallContact(Floor());
            Assert.That(copy.Closed,Is.True);Assert.That(rules.Closed,Is.False);Assert.That(rules.FloorBounces,Is.EqualTo(0));
        }
        [Test] public void Begin_ResetsAClosedPoint()
        {
            var rules=New();rules.OnBallContact(Front(.3f));rules.Begin(PlayerId.Human,PlayerId.Rival);
            Assert.That(rules.Closed,Is.False);Assert.That(rules.Cause,Is.EqualTo(PointEndCause.None));Assert.That(rules.Winner,Is.Null);
            Assert.That(rules.ToHit,Is.EqualTo(PlayerId.Rival));Assert.That(rules.Shots,Is.EqualTo(0));Assert.That(rules.IgnoredAfterClose,Is.EqualTo(0));
        }
    }
    // Spec 8.7.6 / 17.A.8: below, grazing and exactly on the edge, on every surface judged in the return.
    public sealed class ReturnLineJudgeTests
    {
        private const float R=.02f,Mm=.001f;
        private static readonly CourtDimensions Court=CourtDimensions.Standard();
        private static readonly ReturnLineJudge Judge=new ReturnLineJudge(Court,R);
        private static LineVerdict Front(float y)=>Judge.Judge(SurfaceId.FrontWall,new Vector3(3.2f,y,0));
        private static LineVerdict Back(float y)=>Judge.Judge(SurfaceId.BackWall,new Vector3(3.2f,y,Court.Length));
        private static float SideLine(float z)=>Mathf.Lerp(Court.FrontOutLower,Court.BackOutLower,z/Court.Length);
        [Test] public void Tin_Edges()
        {
            Assert.That(Front(Court.TinTop+R+Mm),Is.EqualTo(LineVerdict.Good),"clear above");
            Assert.That(Front(Court.TinTop+R-Mm),Is.EqualTo(LineVerdict.Tin),"grazing");
            Assert.That(Front(Court.TinTop+R),Is.EqualTo(LineVerdict.Tin),"exactly on the edge");
            Assert.That(Front(.1f),Is.EqualTo(LineVerdict.Tin),"below the visible band");
        }
        [Test] public void FrontOut_Edges()
        {
            Assert.That(Front(Court.FrontOutLower-R-Mm),Is.EqualTo(LineVerdict.Good));
            Assert.That(Front(Court.FrontOutLower-R+Mm),Is.EqualTo(LineVerdict.Out));
            Assert.That(Front(Court.FrontOutLower-R),Is.EqualTo(LineVerdict.Out));
            Assert.That(Front(5.5f),Is.EqualTo(LineVerdict.Out),"above the band");
        }
        [Test] public void ServiceLine_IsNotJudgedInTheReturn()
        {
            Assert.That(Front(Court.ServiceLower+.02f),Is.EqualTo(LineVerdict.Good));
        }
        [Test] public void BackOut_Edges()
        {
            Assert.That(Back(Court.BackOutLower-R-Mm),Is.EqualTo(LineVerdict.Good));
            Assert.That(Back(Court.BackOutLower-R+Mm),Is.EqualTo(LineVerdict.Out));
            Assert.That(Back(Court.BackOutLower-R),Is.EqualTo(LineVerdict.Out));
            Assert.That(Back(4f),Is.EqualTo(LineVerdict.Out));
        }
        [TestCase(SurfaceId.LeftWall)]
        [TestCase(SurfaceId.RightWall)]
        public void SideOut_Edges(SurfaceId side)
        {
            float x=side==SurfaceId.LeftWall?Court.Width:0,z=4;
            // The band is perpendicular to the diagonal; measure the footprint along its normal.
            var growth=new Vector2(Court.FrontOutLower-Court.BackOutLower,Court.Length).normalized;
            Vector3 At(float distanceBelowEdge){var uv=new Vector2(z,SideLine(z))-growth*distanceBelowEdge;return new Vector3(x,uv.y,uv.x);}
            Assert.That(Judge.Judge(side,At(R+Mm)),Is.EqualTo(LineVerdict.Good));
            Assert.That(Judge.Judge(side,At(R-Mm)),Is.EqualTo(LineVerdict.Out));
            Assert.That(Judge.Judge(side,At(R)),Is.EqualTo(LineVerdict.Out));
            Assert.That(Judge.Judge(side,new Vector3(x,5.5f,z)),Is.EqualTo(LineVerdict.Out));
            Assert.That(Judge.Judge(side,new Vector3(x,1f,z)),Is.EqualTo(LineVerdict.Good));
        }
        [Test] public void Floor_IsNeverJudgedInTheReturn()
        {
            float shortLine=Court.Length-Court.ShortLineNearBackEdgeFromBack-.02f;
            Assert.That(Judge.Judge(SurfaceId.Floor,new Vector3(3.2f,0,shortLine)),Is.EqualTo(LineVerdict.Good));
        }
        [Test] public void ZeroFootprint_JudgesTheContactPointOnly()
        {
            var point=new ReturnLineJudge(Court,0);
            Assert.That(point.Judge(SurfaceId.FrontWall,new Vector3(3.2f,Court.TinTop+Mm,0)),Is.EqualTo(LineVerdict.Good));
            Assert.That(point.Judge(SurfaceId.FrontWall,new Vector3(3.2f,Court.TinTop,0)),Is.EqualTo(LineVerdict.Tin));
        }
    }
}
