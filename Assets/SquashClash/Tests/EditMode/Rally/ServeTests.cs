using System;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // Serve kinds (design docs/superpowers/specs/2026-11-20-serve-design.md, revision 2): lob and drive, chosen by the server, behind the
    // DEV «Saque nuevo» switch (off by default, today's serve bit for bit).
    public sealed class ServeTests
    {
        private static RallyContext Context(bool on){var p=new RallyParams{ServeKindOn=on?1:0};return RallyFixtures.Context(p);}
        private struct Flight {public float FrontY,BounceX,BounceZ;public bool SideBefore;}
        private static Flight Fly(RallyContext ctx,BallState serve)
        {
            var f=new Flight{FrontY=float.NaN,BounceZ=float.NaN};bool front=false;
            foreach(var c in ctx.Predict(serve).Contacts)
            {
                if(c.Persistent)continue;
                if(!front){if(c.Surface==SurfaceId.FrontWall){front=true;f.FrontY=c.Point.y;}continue;}
                if(c.Surface==SurfaceId.Floor){f.BounceX=c.Point.x;f.BounceZ=c.Point.z;break;}
                if(c.Surface==SurfaceId.LeftWall||c.Surface==SurfaceId.RightWall)f.SideBefore=true;
            }
            return f;
        }
        private static bool Same(BallState a,BallState b)=>a.Position==b.Position&&a.Velocity==b.Velocity&&a.Step==b.Step;

        [TestCase(ServiceBox.Right)][TestCase(ServiceBox.Left)]
        public void Lob_IsHighOnTheFrontWall_TouchesTheSideWall_AndDiesInTheBackCorner(ServiceBox box)
        {
            var ctx=Context(true);var serve=ctx.Serve(box,ServeType.Lob);
            Assert.That(ServePlanner.Forecast(ctx,serve,box,out _),Is.True,"a legal serve");
            var f=Fly(ctx,serve);
            Assert.That(f.FrontY,Is.GreaterThanOrEqualTo(3.4f));Assert.That(f.SideBefore,Is.True,"side wall between the front wall and the bounce");
            Assert.That(f.BounceZ,Is.GreaterThanOrEqualTo(9f),"bounces in the back corner");
            float wall=box==ServiceBox.Right?ctx.Court.Width-f.BounceX:f.BounceX;Assert.That(wall,Is.LessThan(.5f),"next to the receiver's side wall");
        }
        [TestCase(ServiceBox.Right)][TestCase(ServiceBox.Left)]
        public void Drive_IsLowAndFast_WithoutTheSideWall_AndLandsShorterThanTheLob(ServiceBox box)
        {
            var ctx=Context(true);var drive=ctx.Serve(box,ServeType.Drive);var lob=ctx.Serve(box,ServeType.Lob);
            Assert.That(ServePlanner.Forecast(ctx,drive,box,out _),Is.True,"a legal serve");
            var d=Fly(ctx,drive);var l=Fly(ctx,lob);
            Assert.That(d.FrontY,Is.LessThanOrEqualTo(2.6f));Assert.That(d.SideBefore,Is.False);
            Assert.That(d.BounceZ,Is.LessThan(l.BounceZ-.4f),"two landing zones");
            float hd=new Vector2(drive.Velocity.x,drive.Velocity.z).magnitude,hl=new Vector2(lob.Velocity.x,lob.Velocity.z).magnitude;
            Assert.That(hd,Is.GreaterThan(hl),"the drive is the faster one");
        }
        [Test] public void HumanServePauseDefaultsToThreeSeconds()
        {
            var pause = typeof(RallyParams).GetField("ServeKindHumanPauseSeconds");
            Assert.That(pause, Is.Not.Null);
            Assert.That(pause.GetValue(new RallyParams()), Is.EqualTo(3d));
            Assert.That(RivalAiTests.Projection(f => !f.Name.StartsWith("ServeKind", StringComparison.Ordinal)),
                Is.EqualTo("b2311f168c30"));
        }

        [Test] public void Off_EveryKindIsTodaysAutomaticServe_AndNothingIsChosen()
        {
            var ctx=Context(false);var rival=RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Definidor,3);
            Assert.That(new RallyParams().ServeKindOn,Is.EqualTo(0),"off by default");
            foreach(ServiceBox box in Enum.GetValues(typeof(ServiceBox)))
            {
                foreach(ServeType type in Enum.GetValues(typeof(ServeType)))Assert.That(Same(ctx.Serve(box,type),ctx.Serve(box)),Is.True,box+" "+type);
                Assert.That(ctx.ServeTypeOf(rival,box),Is.EqualTo(ServeType.Auto));
            }
        }
        [Test] public void On_TheServerBrainChooses_AndABrainWithoutAChoiceServesTheLob()
        {
            var ctx=Context(true);var bot=new BotBrain(ctx,30,1);
            Assert.That(ctx.ServeTypeOf(bot,ServiceBox.Right),Is.EqualTo(ServeType.Lob));
            var lob=ctx.Serve(ServiceBox.Right,ServeType.Lob);var drive=ctx.Serve(ServiceBox.Right,ServeType.Drive);
            Assert.That(Same(lob,drive),Is.False);Assert.That(Same(lob,ctx.Serve(ServiceBox.Right)),Is.False,"not today's serve");
            Assert.That(Same(ctx.Serve(ServiceBox.Right,ServeType.Auto),ctx.Serve(ServiceBox.Right)),Is.True);
        }
        [TestCase(RivalStyleId.Fondista,.25)][TestCase(RivalStyleId.Completo,.5)][TestCase(RivalStyleId.Definidor,.75)]
        public void Rival_DriveShare_FollowsItsStyle(RivalStyleId style,double share)
        {
            var ctx=Context(true);var ai=RivalAi.For(ctx,AiLevel.Medium,style,11);
            Assert.That(ai.ServeDriveShare,Is.EqualTo(share));
            int drives=0;const int n=4000;
            for(int i=0;i<n;i++){var t=ctx.ServeTypeOf(ai,i%2==0?ServiceBox.Right:ServiceBox.Left);Assert.That(t,Is.Not.EqualTo(ServeType.Auto));if(t==ServeType.Drive)drives++;}
            Assert.That(drives/(double)n,Is.EqualTo(share).Within(.03));
        }
        [Test] public void Rival_ServeChoice_UsesItsOwnStream_SoItsStrokeDrawsNeverChange()
        {
            var ctx=Context(true);var a=RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Completo,5);var b=RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Completo,5);
            for(int i=0;i<200;i++){if(i%3==0)b.ChooseServe(ServiceBox.Left);Assert.That(b.StanceNoise(),Is.EqualTo(a.StanceNoise()),"draw "+i);}
        }
        [Test] public void RallySim_On_RecordsEachServeKind_AndTheBotServesTheLob()
        {
            var ctx=Context(true);int lob=0,drive=0;
            using(var sim=new RallySim(ctx,new BotBrain(ctx,30,2),RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Definidor,3)))
            {
                var match=sim.PlayMatch(PlayerId.Rival);
                foreach(var p in match.Points)
                {
                    if(p.Server==PlayerId.Human)Assert.That(p.ServeType,Is.EqualTo(ServeType.Lob));
                    else if(p.ServeType==ServeType.Drive)drive++;else if(p.ServeType==ServeType.Lob)lob++;
                    Assert.That(p.Cause,Is.Not.EqualTo(PointEndCause.ServiceFault),"the fixed serves are legal");
                }
            }
            Assert.That(lob,Is.GreaterThan(0));Assert.That(drive,Is.GreaterThan(0));
        }
        [Test] public void RallySim_Off_EveryServeIsAuto()
        {
            var ctx=Context(false);
            using(var sim=new RallySim(ctx,new BotBrain(ctx,30,2),RivalAi.For(ctx,AiLevel.Medium,RivalStyleId.Definidor,3)))
                Assert.That(sim.PlayMatch(PlayerId.Rival).Points.All(p=>p.ServeType==ServeType.Auto),Is.True);
        }
    }
}
