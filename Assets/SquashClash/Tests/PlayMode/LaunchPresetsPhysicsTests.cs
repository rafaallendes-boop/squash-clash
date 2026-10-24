using System;
using System.Collections.Generic;
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
        public void DefaultP2_MeetsApprovedLegibilityConditionBeforeSecondBounce(string id)
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
                        Assert.That(fraction,Is.GreaterThanOrEqualTo(.5));
                        double maxHeight=MaxHeightAfterFrontBeforeFirstBounce(t,cfg.Physics.StepSeconds,cfg.Physics.Gravity,cfg.Physics.BallRadius);
                        TestContext.WriteLine(FormattableString.Invariant($"postFrontPreBounceMaxY={maxHeight:R}"));
                        Assert.That(maxHeight,Is.LessThanOrEqualTo(1));break;
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
        [Test] public void LowInterval_RejectsHighFrontEvenWhenFirstBounceIsLow()
        {
            var cfg=EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));
            var start=LaunchPresets.All(cfg.Court,cfg.Physics.Gravity).Single(e=>e.Id=="L2").Launch.Position;
            var target=Ballistics.CenterForContact(SurfaceId.FrontWall,new Vector3(.3f,1.2f,0),cfg.Physics.BallRadius);
            var velocity=Ballistics.SolveCenterTo(start,target,18,cfg.Physics.Gravity);
            using(var sim=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold).Create(cfg.Physics,cfg.Court))
            {
                var t=TrajectoryRunner.Run(sim,Launcher.ToState(new LaunchSpec(start,velocity.normalized,velocity.magnitude),cfg.LaunchScale),1200,StopCondition.SecondFloorBounce,cfg.Court);
                var bounce=t.Contacts.First(c=>c.Surface==SurfaceId.Floor&&!c.Persistent);
                Assert.That(t.States[(int)bounce.Step].Position.y,Is.LessThanOrEqualTo(1),"The old literal accepts this high-front flight");
                Assert.That(MaxHeightAfterFrontBeforeFirstBounce(t,cfg.Physics.StepSeconds,cfg.Physics.Gravity,cfg.Physics.BallRadius),Is.GreaterThan(1));
            }
        }
        [Test] public void LowInterval_DetectsApexBetweenStepSamples()
        {
            const double dt=1.0/120,g=9.81,r=.02,start=.0001,height=.99995;
            double vy=g*(dt-start)/2;
            double flight=(vy+Math.Sqrt(vy*vy+2*g*(height-r)))/g,end=start+flight;
            int floorStep=(int)Math.Ceiling(end/dt);var t=new Trajectory();
            // Reference interval with zero tangential friction; apex is halfway from front to the first sample.
            // Both endpoints and every tick sample are below 1 m, but the interior apex exceeds it.
            for(int i=0;i<=floorStep;i++)
            {
                double elapsed=i*dt-start;
                double y=height+vy*elapsed-.5*g*elapsed*elapsed,v=vy-g*elapsed;
                if(i*dt>=end) {elapsed=i*dt-end;v=-(vy-g*flight)*.45;y=r+v*elapsed-.5*g*elapsed*elapsed;v-=g*elapsed;}
                double z=i*dt<start?r+start-i*dt:r+.55*(i*dt-start);
                t.States.Add(new BallState {Step=i,Position=new Vector3(.3f,(float)y,(float)z),Velocity=new Vector3(0,(float)v,i*dt<start?-1:.55f)});
            }
            t.Contacts.Add(new ContactEvent(SurfaceId.FrontWall,new Vector3(.3f,(float)height,0),Vector2.zero,Vector3.forward,1,(float)start,ContactTiming.Exact,new Vector3(0,(float)vy,-1),false));
            t.Contacts.Add(new ContactEvent(SurfaceId.Floor,new Vector3(.3f,0,(float)(r+.55*flight)),Vector2.zero,Vector3.up,floorStep,(float)(end-(floorStep-1)*dt),ContactTiming.Exact,new Vector3(0,(float)(vy-g*flight),.55f),false));
            Assert.That(t.States.Max(s=>s.Position.y),Is.LessThanOrEqualTo(1));
            Assert.That(MaxHeightAfterFrontBeforeFirstBounce(t,dt,g,r),Is.GreaterThan(1));
        }
        private static double MaxHeightAfterFrontBeforeFirstBounce(Trajectory t,double stepSeconds,double gravity,double radius)
        {
            double Time(ContactEvent c)=>(c.Step-1)*stepSeconds+c.TimeInStep;
            var front=t.Contacts.First(c=>c.Surface==SurfaceId.FrontWall);
            var bounce=t.Contacts.First(c=>c.Surface==SurfaceId.Floor&&!c.Persistent);
            double start=Time(front),end=Time(bounce);
            Assert.That(end,Is.GreaterThanOrEqualTo(start),"First floor bounce must follow the front contact");
            var nodes=new List<(double Time,double Height,double Vy,int Order)>();
            foreach(var c in t.Contacts)
                nodes.Add((Time(c),c.Point.y+c.Normal.y*radius,c.ApproachVelocity.y,0));
            foreach(var state in t.States)
                nodes.Add((state.Step*stepSeconds,state.Position.y,state.Velocity.y,1));
            // Exact contact endpoints, then tick states; a simultaneous post-impact state has zero duration.
            var interval=nodes.Where(n=>n.Time>=start&&n.Time<=end).OrderBy(n=>n.Time).ThenBy(n=>n.Order).ToArray();
            double maximum=interval.Max(n=>n.Height);
            for(int i=1;i<interval.Length;i++)
            {
                var last=interval[i];double duration=last.Time-interval[i-1].Time;
                // There is no collision inside adjacent nodes. Reconstruct the ballistic apex backwards
                // from the incoming endpoint velocity; ticks alone can miss a maximum above 1 m.
                if(gravity>0&&duration>0)
                {
                    double beforeEnd=-last.Vy/gravity;
                    if(beforeEnd>=0&&beforeEnd<=duration)maximum=Math.Max(maximum,last.Height+last.Vy*last.Vy/(2*gravity));
                }
            }
            return maximum;
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
