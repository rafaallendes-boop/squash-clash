using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Tests
{
    public sealed class LaunchPresetsTests
    {
        [Test] public void Catalog_HasExactlyL1ThroughL6()
        {
            Assert.That(LaunchPresets.All(CourtDimensions.Standard(),9.81f).Select(e=>e.Id),Is.EqualTo(new[]{"L1","L2","L3","L4","L5","L6"}));
        }
        [TestCase("L1")][TestCase("L2")][TestCase("L3")][TestCase("L4")][TestCase("L5")][TestCase("L6")]
        public void Launch_StartsStrictlyInsideAndAimsAtFrontWithBallistics(string id)
        {
            var d=CourtDimensions.Standard();float r=new PhysicsParams().BallRadius;
            foreach(float gravity in new[]{5f,9.81f,15f})
            {
                var e=LaunchPresets.All(d,gravity).Single(x=>x.Id==id);var p=e.Launch.Position;
                Assert.That(p.x,Is.GreaterThan(r).And.LessThan(d.Width-r));Assert.That(p.y,Is.GreaterThan(r).And.LessThan(d.WallHeight));
                Assert.That(p.z,Is.GreaterThan(r).And.LessThan(d.Length-r));
                Assert.That(e.Label,Is.Not.Empty);
                foreach(var player in new[]{e.HumanXZ,e.RivalXZ}) {Assert.That(player.x,Is.InRange(.3f,d.Width-.3f));Assert.That(player.y,Is.InRange(.3f,d.Length-.3f));}
                var initial=Launcher.ToState(e.Launch,1);
                Assert.That(Ballistics.FirstPlaneHit(initial,gravity,r,d,10,out var surface,out var time),Is.True);
                Assert.That(surface,Is.EqualTo(SurfaceId.FrontWall));
                var point=Ballistics.PositionAt(initial,gravity,time);Assert.That(point.y,Is.InRange(r,d.WallHeight));
            }
        }
    }
}
