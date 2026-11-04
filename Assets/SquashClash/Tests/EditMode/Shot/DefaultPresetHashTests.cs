using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Tests.Config;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class DefaultPresetHashTests
    {
        [Test] public void InheritedValues_AreUnchanged()
        {
            var p=ConfigFixtures.Default();
            Assert.That(p.Get("court.tinTop"),Is.EqualTo(0.48d),"court.tinTop");
            Assert.That(p.Get("camera.height"),Is.EqualTo(8d),"camera.height");
            Assert.That(p.Get("camera.backOffset"),Is.EqualTo(5d),"camera.backOffset");
            Assert.That(p.Get("camera.lookAtHeight"),Is.EqualTo(0.4d),"camera.lookAtHeight");
            Assert.That(p.Get("camera.fovDeg"),Is.EqualTo(90d),"camera.fovDeg");
            Assert.That(p.Get("placeholders.visible"),Is.EqualTo(1d),"placeholders.visible");
            Assert.That(p.Get("human.x"),Is.EqualTo(3.2d),"human.x");
            Assert.That(p.Get("human.z"),Is.EqualTo(5.5d),"human.z");
            Assert.That(p.Get("rival.x"),Is.EqualTo(4.5d),"rival.x");
            Assert.That(p.Get("rival.z"),Is.EqualTo(6.5d),"rival.z");
            Assert.That(p.Get("pace.tempo"),Is.EqualTo(0.65d),"pace.tempo");
            Assert.That(p.Get("pace.launchScale"),Is.EqualTo(1d),"pace.launchScale");
            Assert.That(p.Get("physics.gravity"),Is.EqualTo(9.81d),"physics.gravity");
            Assert.That(p.Get("physics.stepHz"),Is.EqualTo(120d),"physics.stepHz");
            Assert.That(p.Get("physics.maxImpactsPerStep"),Is.EqualTo(4d),"physics.maxImpactsPerStep");
            Assert.That(p.Get("physics.floor.restitution"),Is.EqualTo(0.45d),"physics.floor.restitution");
            Assert.That(p.Get("physics.floor.friction"),Is.EqualTo(0.3d),"physics.floor.friction");
            Assert.That(p.Get("physics.front.restitution"),Is.EqualTo(0.55d),"physics.front.restitution");
            Assert.That(p.Get("physics.front.friction"),Is.EqualTo(0.2d),"physics.front.friction");
            Assert.That(p.Get("physics.side.restitution"),Is.EqualTo(0.55d),"physics.side.restitution");
            Assert.That(p.Get("physics.side.friction"),Is.EqualTo(0.2d),"physics.side.friction");
            Assert.That(p.Get("physics.back.restitution"),Is.EqualTo(0.55d),"physics.back.restitution");
            Assert.That(p.Get("physics.back.friction"),Is.EqualTo(0.2d),"physics.back.friction");
            Assert.That(p.Get("physics.backend"),Is.EqualTo(1d),"physics.backend");
            Assert.That(p.Get("launcher.x"),Is.EqualTo(3.2d),"launcher.x");
            Assert.That(p.Get("launcher.y"),Is.EqualTo(1d),"launcher.y");
            Assert.That(p.Get("launcher.z"),Is.EqualTo(7d),"launcher.z");
            Assert.That(p.Get("launcher.yawDeg"),Is.EqualTo(0d),"launcher.yawDeg");
            Assert.That(p.Get("launcher.pitchDeg"),Is.EqualTo(10d),"launcher.pitchDeg");
            Assert.That(p.Get("launcher.speed"),Is.EqualTo(15d),"launcher.speed");
            Assert.That(p.Get("launcher.repeatSeconds"),Is.EqualTo(0d),"launcher.repeatSeconds");
            Assert.That(p.Get("debug.slowMotion"),Is.EqualTo(1d),"debug.slowMotion");
            Assert.That(p.Get("camera.mode"),Is.EqualTo(0d),"camera.mode");
            Assert.That(p.Get("camera.followDamping"),Is.EqualTo(0.3d),"camera.followDamping");
            Assert.That(p.Get("camera.followMaxOffset"),Is.EqualTo(1.5d),"camera.followMaxOffset");
            Assert.That(p.Get("aids.shadow"),Is.EqualTo(1d),"aids.shadow");
            Assert.That(p.Get("aids.trail"),Is.EqualTo(1d),"aids.trail");
            Assert.That(p.Get("aids.impactMarks"),Is.EqualTo(1d),"aids.impactMarks");
            Assert.That(p.Get("aids.prediction"),Is.EqualTo(0d),"aids.prediction");
            Assert.That(p.Get("aids.trailSeconds"),Is.EqualTo(0.15d),"aids.trailSeconds");
            Assert.That(p.Get("aids.ballVisualScale"),Is.EqualTo(1d),"aids.ballVisualScale");
        }
        [Test] public void SimulationKey_IsUnchanged()=>Assert.That(EffectiveConfig.From(ConfigFixtures.Default()).SimulationKey,Is.EqualTo("166a96e319c7"));
        [Test] public void NewHash_IsRecorded() {var p=ConfigFixtures.Default();Assert.That(p.Hash,Is.EqualTo("af2114f4cbc7"));Assert.That(p.Ordered.Count,Is.EqualTo(109));}
    }
}
