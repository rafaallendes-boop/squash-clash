using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Runtime.Tests
{
    public sealed class LaunchPresetsPhysicsTests
    {
        [TestCase("L1")][TestCase("L2")][TestCase("L3")][TestCase("L4")][TestCase("L5")][TestCase("L6")]
        public void DefaultP2_MeetsUnchangedLegibilityConditionBeforeSecondBounce(string id)
        {
            var cfg=EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
            Assert.That(cfg.Backend,Is.EqualTo("P2-Analytic"));var d=cfg.Court;
            var e=LaunchPresets.All(d,cfg.Physics.Gravity).Single(x=>x.Id==id);
            using(var sim=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold).Create(cfg.Physics,d))
            {
                var t=TrajectoryRunner.Run(sim,Launcher.ToState(e.Launch,cfg.LaunchScale),cfg.Physics.StepHz*10,StopCondition.SecondFloorBounce,d);
                var bounces=t.Contacts.Where(c=>c.Surface==SurfaceId.Floor&&!c.Persistent).ToArray();
                Assert.That(t.Exit,Is.EqualTo(TrajectoryExit.None));Assert.That(bounces.Length,Is.EqualTo(2),"Incomplete trajectory cannot pass");
                TestContext.WriteLine($"{id}: config={cfg.Hash} steps={t.States.Count-1} firstBounceZ={bounces[0].Point.z:R} secondBounceZ={bounces[1].Point.z:R} maxY={t.States.Max(s=>s.Position.y):R}");
                switch(id)
                {
                    case "L1":Assert.That(t.Contacts.Any(c=>c.Surface==SurfaceId.BackWall&&Before(c,bounces[1])),Is.True);break;
                    case "L2":
                        long front=t.Contacts.First(c=>c.Surface==SurfaceId.FrontWall).Step;
                        var after=t.States.Where(s=>s.Step>=front).ToArray();double fraction=(double)after.Count(s=>s.Position.x<=.30f)/after.Length;
                        TestContext.WriteLine("postFrontFraction="+fraction.ToString("R"));
                        Assert.That(fraction,Is.GreaterThanOrEqualTo(.5));Assert.That(t.States[(int)bounces[0].Step].Position.y,Is.LessThanOrEqualTo(1));break;
                    case "L3":Assert.That(bounces[1].Point.z,Is.LessThanOrEqualTo(2));break;
                    case "L4":Assert.That(t.States.Max(s=>s.Position.y),Is.GreaterThanOrEqualTo(4));Assert.That(bounces[0].Point.z,Is.GreaterThanOrEqualTo(7));break;
                    case "L5":
                        var camera=new Vector3(d.Width/2,(float)cfg.Source.Get("camera.height"),d.Length+(float)cfg.Source.Get("camera.backOffset"));
                        // A point on the finite camera-ball segment at the axis' z plane is a conservative distance witness.
                        int occluded=t.States.Where(s=>s.Step<bounces[1].Step).Count(s=>NearHumanAxis(camera,s.Position,e.HumanXZ));
                        TestContext.WriteLine("occludedSteps="+occluded);Assert.That(occluded,Is.GreaterThanOrEqualTo(12));break;
                    case "L6":
                        var corner=new Vector3(d.Width,0,d.Length);
                        var near=t.Contacts.Where(c=>Before(c,bounces[1])&&Vector3.Distance(c.Point,corner)<=1).ToArray();
                        Assert.That(near.Any(c=>c.Surface==SurfaceId.BackWall),Is.True,"BackWall near corner");
                        Assert.That(near.Any(c=>c.Surface==SurfaceId.LeftWall),Is.True,"Lateral near same corner");
                        foreach(var c in near.Where(c=>c.Surface==SurfaceId.BackWall||c.Surface==SurfaceId.LeftWall))
                            Assert.That(Vector3.Distance(c.Point,new Vector3(e.RivalXZ.x,.9f,e.RivalXZ.y)),Is.LessThanOrEqualTo(1));
                        TestContext.WriteLine("cornerContacts="+string.Join(";",near.Select(c=>$"{c.Surface}:{c.Point}")));break;
                }
            }
        }
        private static bool Before(ContactEvent a,ContactEvent b) => a.Step<b.Step||(a.Step==b.Step&&a.TimeInStep<b.TimeInStep);
        private static bool NearHumanAxis(Vector3 camera,Vector3 ball,Vector2 human)
        {
            if(ball.z==camera.z)return false;
            float u=(human.y-camera.z)/(ball.z-camera.z);if(u<0||u>1)return false;
            var p=Vector3.Lerp(camera,ball,u);
            var axis=new Vector3(human.x,Mathf.Clamp(p.y,.28f,1.52f),human.y);
            return Vector3.Distance(p,axis)<=.25f;
        }
    }
}
