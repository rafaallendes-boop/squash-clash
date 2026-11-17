using System;
using System.Collections;
using System.Collections.Generic;
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
    // F2: the arm is the striker's own energy at its own contact step, for the human (through ShotController's authoritative
    // callback) and the rival, and never depends on how the frame loop slices time.
    public sealed class ZFatigueArmTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup(){yield return Boot(false);Prepare();}
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private void Prepare(bool constant=true)
        {
            app.Clock.Paused=false;app.Tuning.Set("fatigue.enabled",1);
            if(constant){app.Tuning.Set("fatigue.costPerMeter",0);app.Tuning.Set("fatigue.recoverPerS",0);}
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.AutoRestart=false;
        }
        // Puts a fresh energy at the start of the current flight; nothing has advanced yet, so it is the energy at launch.
        private FatigueState Replace(PlayerId who,double energy)
        {
            var state=new FatigueState(rally.FatigueSettings,energy);state.BeginPoint();state.BeginFlight(app.Host.Current.Step);
            typeof(RallyDirector).GetProperty(who==PlayerId.Human?"HumanEnergy":"RivalEnergy").SetValue(rally,state);return state;
        }
        [Serializable] private sealed class Line {public string kind,payload;}
        private CausalShotTrace[] Traces(string player)=>File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Line>).Where(l=>l.kind=="rally-causal")
            .Select(l=>JsonUtility.FromJson<CausalShotTrace>(l.payload)).Where(t=>t.player==player).ToArray();
        private static CausalVector[] Fatigue(CausalVector[] vectors)=>(vectors??new CausalVector[0]).Where(v=>v.Cause=="fatigue").ToArray();
        private CausalVector[] FirstHumanVectors(Func<bool> extra,double maxSeconds=60,int slice=1)
        {
            CausalVector[] captured=null;
            Action<SquashClash.Core.Shot.ShotRecord> handler=r=>{if(captured==null&&r.Applied)captured=controller.LastCausal.Vectors;};
            controller.ShotResolved+=handler;
            try
            {
                int limit=(int)(maxSeconds/StepReal/slice);
                for(int i=0;i<limit&&captured==null;i++){app.Host.Advance(StepReal*slice);controller.ProcessPending();rally.Tick();}
            }
            finally{controller.ShotResolved-=handler;}
            return captured;
        }
        [Test] public void A7_HumanStrike_UsesItsOwnEnergyAtTheContactStep()
        {
            rally.StartMode();Replace(PlayerId.Human,40);
            var vectors=FirstHumanVectors(null);Assert.That(vectors,Is.Not.Null,"the human struck");
            var fatigue=Fatigue(vectors);Assert.That(fatigue,Has.Length.EqualTo(1),"the fatigue vector reaches the authoritative human solve");
            double f=(rally.FatigueSettings.Onset-40)/rally.FatigueSettings.Onset;
            Assert.That(fatigue[0].DeltaSpeedFraction,Is.EqualTo(-(float)(rally.FatigueSettings.ArmSpeedMax*f)).Within(1e-7));
        }
        [Test] public void A7_HumanStrike_IgnoresTheRivalsEnergy()
        {
            rally.StartMode();Replace(PlayerId.Rival,40);
            var vectors=FirstHumanVectors(null);Assert.That(vectors,Is.Not.Null);
            Assert.That(Fatigue(vectors),Is.Empty,"a rested striker is not tired by its opponent");
        }
        [Test] public void A7_RivalStrike_UsesItsOwnEnergyAtTheContactStep()
        {
            rally.StartMode();Replace(PlayerId.Rival,40);
            Assert.That(Drive(()=>Traces("Rival").Length>0,90),Is.True,"the rival struck");
            var fatigue=Fatigue(Traces("Rival")[0].vectors);Assert.That(fatigue,Has.Length.EqualTo(1));
            double f=(rally.FatigueSettings.Onset-40)/rally.FatigueSettings.Onset;
            Assert.That(fatigue[0].DeltaSpeedFraction,Is.EqualTo(-(float)(rally.FatigueSettings.ArmSpeedMax*f)).Within(1e-7));
            Assert.That(Fatigue(Traces("Human").SelectMany(t=>t.vectors).ToArray()),Is.Empty,"the rested human was not changed by the tired rival");
        }
        [UnityTest] public IEnumerator HumanFactor_DoesNotDependOnTheFrameSliceOfAdvance()
        {
            var results=new List<float>();
            foreach(int slice in new[]{1,5})
            {
                yield return Shutdown();yield return Boot(false);Prepare(false);
                rally.StartMode();Replace(PlayerId.Human,40);
                var vectors=FirstHumanVectors(null,60,slice);Assert.That(vectors,Is.Not.Null,"slice "+slice);
                var fatigue=Fatigue(vectors);Assert.That(fatigue,Has.Length.EqualTo(1),"slice "+slice);results.Add(fatigue[0].DeltaSpeedFraction);
            }
            Assert.That(results[0],Is.LessThan(0));Assert.That(results[1],Is.EqualTo(results[0]),"same contact step, same energy at it");
        }
        [Test] public void PracticeAfterStopMode_HasNoFatigueVector()
        {
            rally.StartMode();Replace(PlayerId.Human,40);rally.StopMode();
            Assert.That(rally.Active,Is.False);var plan=controller.Plan;Assert.That(plan.Exists,Is.True);
            Assert.That(app.Host.ClockMap.TrySimToReal(plan.TStarStep,0,out double target),Is.True);
            double release=target+controller.Snapshot.Timing.LatencyCompMs/1000;
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,"practice-1",0,Math.Max(app.Host.LaunchRealTime,release-.15),release,true,GestureOutcome.Swing,new Vector2(2.6f,1.5f),.5f,controller.Snapshot.Reception,"fixture",app.Host.ObservedRealTime));
            Assert.That(Drive(()=>controller.LastRecord!=null&&controller.LastRecord.Applied,60),Is.True);
            Assert.That(controller.LastCausal.Vectors,Is.Not.Null);Assert.That(Fatigue(controller.LastCausal.Vectors),Is.Empty);
        }
    }
}
