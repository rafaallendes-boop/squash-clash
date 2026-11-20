using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.Ball;
using UnityEngine;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Back-glass audio delivery, task 2: does an impact indicator sit where the ball really meets the wall? Measured, not eyeballed.
    // Two indicators: the wall-impact disc (LegibilityAids) against the contact event's point, and the timing cue's fixed triangle
    // (TimingRingView, drawn at the plan's P*) against where the ball really is at T*, split by the walls the ball met on the way.
    // The numbers are printed (see the report); the assertions only guard against a visible misalignment (2 cm).
    public sealed class ZImpactMarkerAlignmentTests : ZFirstRunFixture
    {
        private const float Tolerance=.02f;
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private sealed class Stat
        {
            public readonly List<float> Errors=new List<float>();
            public void Add(float e)=>Errors.Add(e);
            public override string ToString()=>Errors.Count==0?"n=0":$"n={Errors.Count} max={Errors.Max()*100:F3} cm mean={Errors.Average()*100:F3} cm";
        }
        // Scripted launches at each wall from several heights and sides (the autopilot demo almost never meets the side walls).
        [UnityTest] public IEnumerator WallMarks_SitOnTheContactPoint_ForEveryWall_AndPredictionIsTheBall()
        {
            var aids=app.GetComponent<LegibilityAids>();Assert.That(aids,Is.Not.Null);app.Clock.Paused=false;
            var court=app.Config.Court;var mark=new Dictionary<SurfaceId,Stat>();var plane=new Dictionary<SurfaceId,Stat>();var tilt=new Dictionary<SurfaceId,Stat>();var follow=new Dictionary<SurfaceId,Stat>();
            foreach(SurfaceId s in new[]{SurfaceId.FrontWall,SurfaceId.LeftWall,SurfaceId.RightWall,SurfaceId.BackWall}){mark[s]=new Stat();plane[s]=new Stat();tilt[s]=new Stat();follow[s]=new Stat();}
            SurfaceId? firstWall=null;
            app.Host.ContactRaised+=c=>
            {
                if(c.Persistent||!mark.ContainsKey(c.Surface))return;
                Transform last=null;foreach(Transform t in aids.transform)if(t.name=="Wall impact"&&t.gameObject.activeSelf)last=t;
                if(last==null)return;
                mark[c.Surface].Add((last.position-(c.Point+c.Normal*.002f)).magnitude);
                tilt[c.Surface].Add(Vector3.Angle(last.forward,c.Normal)/100f); // degrees/100, printed as "cm" below only for uniformity: see the report
                float dist=c.Surface==SurfaceId.FrontWall?Mathf.Abs(c.Point.z):c.Surface==SurfaceId.BackWall?Mathf.Abs(c.Point.z-court.Length):c.Surface==SurfaceId.RightWall?Mathf.Abs(c.Point.x):Mathf.Abs(c.Point.x-court.Width);
                plane[c.Surface].Add(dist);if(!firstWall.HasValue)firstWall=c.Surface;
            };
            var cases=new List<(SurfaceId wall,Vector3 pos,Vector3 vel)>();
            foreach(float y in new[]{.8f,1.6f,2.6f})
            {
                foreach(float x in new[]{1.0f,3.2f,5.4f})foreach(float vx in new[]{-3f,0f,3f}){cases.Add((SurfaceId.BackWall,new Vector3(x,y,6),new Vector3(vx,1,14)));cases.Add((SurfaceId.FrontWall,new Vector3(x,y,6),new Vector3(vx,1,-14)));}
                foreach(float z in new[]{3f,6f,8f})foreach(float vz in new[]{-4f,4f}){cases.Add((SurfaceId.LeftWall,new Vector3(3.2f,y,z),new Vector3(14,1,vz)));cases.Add((SurfaceId.RightWall,new Vector3(3.2f,y,z),new Vector3(-14,1,vz)));}
            }
            foreach(var c in cases)
            {
                firstWall=null;
                app.Host.Launch(app.Config,new BallState{Position=c.pos,Velocity=c.vel,Step=0},app.Config.Physics.StepHz*3,StopCondition.MaxSteps);
                float worst=0;
                for(int i=0;i<150;i++)
                {
                    app.Host.Advance(StepReal);
                    long index=app.Host.Current.Step-app.Host.Prediction.States[0].Step;
                    if(index>=0&&index<app.Host.Prediction.States.Count)worst=Mathf.Max(worst,(app.Host.Current.Position-app.Host.Prediction.States[(int)index].Position).magnitude);
                }
                Assert.That(firstWall.HasValue,Is.True,"scripted launch never met a wall: "+c.wall+" "+c.pos+" "+c.vel);
                foreach(var f in follow.Values)f.Add(worst);
            }
            foreach(var s in mark.Keys)TestContext.Out.WriteLine($"WALLMARK {s}: disc vs contact point {mark[s]}; contact point vs wall plane {plane[s]}; disc facing error (deg/100 in the max column) {tilt[s].Errors.Max()*100:F4} deg; prediction vs ball along the whole path {follow[s]}");
            foreach(var s in mark.Keys)
            {
                Assert.That(mark[s].Errors.Count,Is.GreaterThanOrEqualTo(12),s+" too few samples");
                Assert.That(mark[s].Errors.Max(),Is.LessThan(Tolerance),s+" disc is misaligned");Assert.That(plane[s].Errors.Max(),Is.LessThan(Tolerance),s+" contact point is off its wall plane");
                Assert.That(follow[s].Errors.Max(),Is.LessThan(Tolerance),s+" the ball left its predicted path");
            }
            yield return null;
        }
        [UnityTest] public IEnumerator TimingTriangle_SitsWhereTheBallIsAtTStar_AlsoAfterTheGlass()
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=21;
            var groups=new Dictionary<string,Stat>{{"none",new Stat()},{"side",new Stat()},{"glass",new Stat()},{"glass+side",new Stat()}};
            var ringVsPlan=new Stat();var cueGap=new Stat();
            long launch=-1;var met=new HashSet<SurfaceId>();long lastPlanVersionSeen=-1;
            app.Host.ContactRaised+=c=>{if(!c.Persistent)met.Add(c.Surface);};
            app.Host.Launched+=_=>met.Clear();bool replaced=false;app.Host.PredictionReplaced+=_=>replaced=true;
            Vector3 pStar=default;long tStar=-1;bool have=false,done=false;
            Click("menu-play");Refresh();yield return null;
            bool Step()
            {
                if(app.Host.LaunchId!=launch){launch=app.Host.LaunchId;have=false;done=false;replaced=false;}
                var plan=controller.Plan;
                if(plan.Exists&&plan.TStarStep>=0&&!done&&!replaced){pStar=plan.PStar;tStar=plan.TStarStep;have=true;lastPlanVersionSeen=plan.Version;
                    var cue=controller.transform.Find("First Shot timing cue");if(cue!=null)ringVsPlan.Add((cue.position-controller.ShownPlan.PStar).magnitude);} // the cue sits at the shown opportunity: the volley one first (volley v1)
                if(have&&!done&&app.Host.Current.Step>=tStar)
                {
                    done=true;if(replaced)return false; // the player's own stroke changed the ball before T*: not a plan error
                    float e=(app.Host.Current.Position-pStar).magnitude;
                    bool glass=met.Contains(SurfaceId.BackWall),side=met.Contains(SurfaceId.LeftWall)||met.Contains(SurfaceId.RightWall);
                    groups[glass&&side?"glass+side":glass?"glass":side?"side":"none"].Add(e);
                }
                return groups["glass"].Errors.Count+groups["glass+side"].Errors.Count>=6&&groups.Values.Sum(g=>g.Errors.Count)>=30;
            }
            Drive(Step,1500);
            foreach(var g in groups)TestContext.Out.WriteLine($"TRIANGLE {g.Key}: planned P* vs ball at T* {g.Value}");
            TestContext.Out.WriteLine($"TRIANGLE drawn position vs plan.PStar {ringVsPlan}; PredictionMatchesBitwise={app.Host.PredictionMatchesBitwise}");
            Assert.That(groups.Values.Sum(g=>g.Errors.Count),Is.GreaterThanOrEqualTo(10),"enough receptions measured");
            foreach(var g in groups)if(g.Value.Errors.Count>0)Assert.That(g.Value.Errors.Max(),Is.LessThan(Tolerance),g.Key+" triangle misaligned");
            if(ringVsPlan.Errors.Count>0)Assert.That(ringVsPlan.Errors.Max(),Is.LessThan(Tolerance));
        }
    }
}
