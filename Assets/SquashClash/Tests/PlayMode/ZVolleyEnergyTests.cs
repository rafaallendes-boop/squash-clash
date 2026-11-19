using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Volley energy on the human path (PO, 19 nov): a volley in the rally costs RallyParams.VolleyEnergyCost at its own contact step,
    // on top of the stroke, and the point's fatigue line records it. Running and quiet recovery are switched off so the only change is
    // the strokes'. The autopilot plays the bounces until the first offered volley, which the test then takes.
    public sealed class ZVolleyEnergyTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return Boot(false);
            app.Clock.Paused=false;app.Tuning.Set("fatigue.enabled",1);app.Tuning.Set("fatigue.costPerMeter",0);app.Tuning.Set("fatigue.recoverPerS",0);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.AutoRestart=true;rally.Seed=7;
        }
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        [Serializable] private sealed class Line {public string kind,payload;}
        [Serializable] private sealed class FatigueLine {public int point;public FatiguePoint human,rival;}
        private void StepOnce(){app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();}
        private bool Until(Func<bool> done,double seconds){int limit=(int)(seconds/StepReal);for(int i=0;i<limit&&!done();i++)StepOnce();return done();}
        [Test] public void HumanVolleyInTheRally_CostsTheVolleyEnergy_AtItsContact_AndTheMetricsShowIt()
        {
            rally.StartMode();
            Assert.That(Until(()=>rally.Phase==RallyPhase.HumanReception&&controller.Volley.Exists&&!controller.AttemptConsumed,600),Is.True,"an offered volley within ten minutes of autopilot rally");
            rally.Autopilot=false;long launch=app.Host.LaunchId;var energy=rally.HumanEnergy;
            double before=energy.Energy,volleyBefore=energy.Point.VolleyCost,strongBefore=energy.Point.StrongCost;
            Assert.That(app.Host.ClockMap.TrySimToReal(controller.Volley.Plan.TStarStep,0,out double target),Is.True);
            for(int i=0;i<3000&&app.Host.ObservedRealTime+StepReal<target-1e-10;i++)StepOnce();
            double left=target-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);controller.ProcessPending();rally.Tick();
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.HumanReception));Assert.That(app.Host.LaunchId,Is.EqualTo(launch));
            controller.Enqueue(new ClosedGesture(launch,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,target,true,GestureOutcome.Swing,new Vector2(3.2f,2.2f),.5f,controller.Snapshot.Reception,"fixture",target));
            controller.ProcessPending();
            Assert.That(controller.LastOpportunity,Is.EqualTo(Opportunity.Volley));Assert.That(controller.LastRecord.Result,Is.EqualTo("Perfect"),controller.LastRecord.Reason);
            Assert.That(Until(()=>rally.Phase!=RallyPhase.HumanReception,3),Is.True);
            Assert.That(controller.LastRecord.Applied,Is.True,controller.LastRecord.Reason);Assert.That(rally.Phase,Is.EqualTo(RallyPhase.RivalReception),"the volley went back to the rival");
            Assert.That(energy.Point.StrongCost,Is.EqualTo(strongBefore),"a soft stroke has no cost of its own");
            Assert.That(energy.Point.VolleyCost,Is.EqualTo(volleyBefore+2.0),"the volley costs two energy points");
            Assert.That(energy.Energy,Is.EqualTo(before-2.0).Within(1e-9));
            Assert.That(rally.Params.VolleyEnergyCost,Is.EqualTo(2.0));
            int point=rally.PointsPlayed;
            Assert.That(Until(()=>rally.PointsPlayed>point,60),Is.True,"the point ends");
            var last=File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Line>).Where(l=>l.kind=="fatigue").Select(l=>JsonUtility.FromJson<FatigueLine>(l.payload)).Last();
            Assert.That(last.human.VolleyCost,Is.GreaterThanOrEqualTo(volleyBefore+2.0),"the fatigue line of the point shows the volley cost");
            Assert.That(last.rival.VolleyCost,Is.Zero);
        }
    }
}
