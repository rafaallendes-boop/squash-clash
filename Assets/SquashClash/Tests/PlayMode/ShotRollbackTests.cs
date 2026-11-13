using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.Ball;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests
{
    public sealed class ShotRollbackTests
    {
        private readonly List<GameObject> roots=new List<GameObject>();
        private EffectiveConfig config;
        private double StepReal=>1d/config.Physics.StepHz/config.Tempo;
        [UnitySetUp] public IEnumerator Setup(){yield return PlayModeTestWorld.ClearApp();config=EffectiveConfig.From(PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text));}
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var root in roots)Object.Destroy(root);roots.Clear();yield return null;}
        private BallSimulationHost Host(bool incoming=false)
        {
            var root=new GameObject("Rollback test");roots.Add(root);var host=root.AddComponent<BallSimulationHost>();host.enabled=false;
            host.Initialize(new IBallSimulationFactory[]{new AnalyticBallSimulationFactory(0)},new SimClock(config.Physics.StepHz,config.Tempo));host.ConfigureShotHistory(250);
            if(incoming)host.Launch(config);else host.Launch(config,new BallState{Position=new Vector3(3.2f,1.2f,3),Velocity=new Vector3(0,0,-6)},1200,StopCondition.SecondFloorBounce);
            return host;
        }
        private BallSimulationHost HostAtRealTime(double realBase)
        {
            var h=Host();
            // Rebase this disabled test host before any advance. Keep the map and initial history
            // on the same fixed epoch without adding a production clock or waiting for Unity uptime.
            typeof(BallSimulationHost).GetProperty(nameof(h.LaunchRealTime)).SetValue(h,realBase);
            typeof(BallSimulationHost).GetProperty(nameof(h.ObservedRealTime)).SetValue(h,realBase);
            h.ClockMap.Begin(h.LaunchId,realBase,h.Current.Step,h.Clock.Tempo,h.Clock.DebugSlowMotion,h.Clock.StepHz);
            typeof(BallSimulationHost).GetMethod("RecordHistorical",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(h,new object[]{h.Current});
            return h;
        }
        private void To(BallSimulationHost host,long step){for(int i=0;i<1500&&host.Current.Step<step&&host.IsRunning;i++)host.Advance(StepReal);Assert.That(host.Current.Step,Is.EqualTo(step));}
        private static void Bits(BallState a,BallState b)
        {
            Assert.That(a.Step,Is.EqualTo(b.Step));
            foreach(var pair in new[]{(a.Position.x,b.Position.x),(a.Position.y,b.Position.y),(a.Position.z,b.Position.z),(a.Velocity.x,b.Velocity.x),(a.Velocity.y,b.Velocity.y),(a.Velocity.z,b.Velocity.z)})
                Assert.That(BitConverter.SingleToInt32Bits(pair.Item1),Is.EqualTo(BitConverter.SingleToInt32Bits(pair.Item2)));
        }
        private static IEnumerable<string> Keys(BallSimulationHost h)=>h.CanonicalContacts.Select(e=>e.Step+":"+e.Surface+":"+e.TimeInStep.ToString("R",System.Globalization.CultureInfo.InvariantCulture));
        [Test] public void ApplyShot_PunctualVsDelayed_SameBitsAndSameCanonicalEvents()
        {
            var punctual=Host();var delayed=Host();To(punctual,30);Assert.That(punctual.ApplyShot(punctual.LaunchId,30,new Vector3(0,4,-18)).Applied,Is.True);To(punctual,36);
            To(delayed,36);var d=delayed.ApplyShot(delayed.LaunchId,30,new Vector3(0,4,-18));Assert.That(d.Applied,Is.True,d.Reason);Bits(punctual.Current,delayed.Current);Assert.That(Keys(delayed),Is.EqualTo(Keys(punctual)));Assert.That(d.StepsResimulated,Is.EqualTo(6));
        }
        [Test] public void ApplyShot_PredictionMatchesGameBitwiseAfterShot()
        {var h=Host();To(h,30);Assert.That(h.ApplyShot(h.LaunchId,28,new Vector3(0,5,-15)).Applied,Is.True);for(int i=0;i<200&&h.IsRunning;i++){h.Advance(StepReal);Assert.That(h.PredictionMatchesBitwise,Is.True);var s=h.Prediction.States.Single(e=>e.Step==h.Current.Step);Bits(h.Current,s);}}
        [Test] public void ApplyShot_DoesNotAdvanceClockAgain()
        {var h=Host();To(h,30);h.Advance(StepReal*.4);float alpha=h.Clock.Alpha;long dropped=h.Clock.DroppedSteps;Assert.That(h.ApplyShot(h.LaunchId,28,new Vector3(0,5,-15)).Applied,Is.True);Assert.That(h.Clock.Alpha,Is.EqualTo(alpha));Assert.That(h.Clock.DroppedSteps,Is.EqualTo(dropped));}
        [Test] public void ApplyShot_ResetsOutgoingBounces_KeepsReceptionLegality()
        {var h=Host(true);long floor=h.Prediction.Contacts.First(e=>e.Surface==SurfaceId.Floor&&!e.Persistent).Step;To(h,floor+3);Assert.That(h.ReceptionBounces,Is.EqualTo(1));Assert.That(h.ApplyShot(h.LaunchId,floor+1,new Vector3(0,5,-15)).Applied,Is.True);Assert.That(h.ReceptionBounces,Is.EqualTo(1));Assert.That(h.OutgoingBounces,Is.Zero);}
        [Test] public void ApplyShot_FromPreviousLaunchId_IsRejected()
        {var h=Host();long old=h.LaunchId;h.Replay();Assert.That(h.LaunchId,Is.EqualTo(old+1));var before=h.Current;var d=h.ApplyShot(old,0,Vector3.up);Assert.That(d.Applied,Is.False);Assert.That(d.Reason,Is.EqualTo("wrong-launch"));Bits(h.Current,before);}
        [Test] public void ApplyShot_OlderThanHistory_IsNotEvaluable()
        {var h=Host();To(h,40);var before=h.Current;Assert.That(h.TryGetHistoricalState(0,out _),Is.False);var d=h.ApplyShot(h.LaunchId,0,Vector3.up);Assert.That(d.Applied,Is.False);Assert.That(d.Reason,Is.EqualTo("history-expired"));Bits(h.Current,before);}
        [Test] public void ApplyShot_LateDeliveryAfterSecondBounce_ResolvesByEventTime()
        {var h=Host();long last=h.Prediction.States.Last().Step;To(h,last);Assert.That(h.IsRunning,Is.False);var d=h.ApplyShot(h.LaunchId,last-3,new Vector3(0,6,-10));Assert.That(d.Applied,Is.True,d.Reason);Assert.That(d.ContactStep,Is.EqualTo(last-3));Assert.That(h.IsRunning,Is.True);}
        [Test] public void ApplyShot_DoesNotReemitCommonPrefixEvents_AndCountsObsolete()
        {var h=Host();int callbacks=0;h.ContactRaised+=e=>callbacks++;long last=h.Prediction.States.Last().Step;To(h,last);int old=callbacks;var d=h.ApplyShot(h.LaunchId,last-3,new Vector3(0,6,-10));Assert.That(d.Applied,Is.True,d.Reason);Assert.That(callbacks,Is.EqualTo(old));Assert.That(d.ObsoleteEffects,Is.GreaterThanOrEqualTo(1));Assert.That(h.CanonicalContacts.Count(e=>e.Surface==SurfaceId.Floor&&!e.Persistent),Is.EqualTo(1));}
        [Test] public void ApplyShot_InvalidatesStaleTrailAndMarks()
        {
            var h=Host();var ball=new GameObject("Ball");ball.transform.SetParent(h.transform);var view=ball.AddComponent<BallView>();view.Initialize(h,.02f);
            var go=new GameObject("Aids");go.transform.SetParent(h.transform);var aids=go.AddComponent<LegibilityAids>();aids.enabled=false;var p=config.Source;aids.Initialize(h,new TuningState(p,p),view);
            To(h,30);aids.Tick(h.RenderPosition,.01f);aids.Tick(h.RenderPosition+Vector3.one,.01f);Assert.That(go.transform.Find("Ball trail").GetComponent<LineRenderer>().positionCount,Is.GreaterThan(0));
            Assert.That(h.ApplyShot(h.LaunchId,28,new Vector3(0,5,-15)).Applied,Is.True);Assert.That(go.transform.Find("Ball trail").GetComponent<LineRenderer>().positionCount,Is.Zero);
        }
        [Test] public void ApplyShot_MeasuresJumpAtSameVisualInstantAndCamera()
        {
            var camera=UnityEngine.Camera.main;
            if(camera==null){var cameraObject=new GameObject("Rollback camera");roots.Add(cameraObject);cameraObject.tag="MainCamera";camera=cameraObject.AddComponent<UnityEngine.Camera>();camera.transform.position=new Vector3(3,8,-5);camera.transform.LookAt(new Vector3(3,1,5));}
            var h=Host();To(h,30);h.Advance(StepReal*.4);var before=h.RenderPosition;var beforePx=camera.WorldToScreenPoint(before);var d=h.ApplyShot(h.LaunchId,25,new Vector3(0,6,-15));
            Assert.That(d.Applied,Is.True,d.Reason);Assert.That(d.JumpMeters,Is.EqualTo(Vector3.Distance(before,h.RenderPosition)).Within(1e-6));Assert.That(d.JumpMeters,Is.GreaterThan(0));Assert.That(d.JumpPixels,Is.EqualTo(Vector2.Distance(beforePx,camera.WorldToScreenPoint(h.RenderPosition))).Within(.01));Assert.That(d.JumpBallDiameters,Is.EqualTo(d.JumpMeters/.04f).Within(.0001));
        }
        [Test] public void ScheduleShot_AppliesAtTStarStep_AndFiresFeedbackAtContact()
        {var h=Host();To(h,20);int calls=0;h.ShotApplied+=d=>{Assert.That(h.Current.Step,Is.EqualTo(25));Assert.That(d.Applied,Is.True);calls++;};Assert.That(h.ScheduleShot(h.LaunchId,25,s=>new Vector3(0,6,-15)),Is.True);To(h,24);Assert.That(calls,Is.Zero);To(h,25);Assert.That(calls,Is.EqualTo(1));Assert.That(h.Current.Velocity,Is.EqualTo(new Vector3(0,6,-15)));}
        [Test] public void PauseStep_AndSlowMotion_DoNotBreakRollback()
        {var h=Host();To(h,30);h.Clock.Paused=true;h.Advance(.2);h.Clock.RequestSingleStep();h.Advance(0);Assert.That(h.Current.Step,Is.EqualTo(31));Assert.That(h.ApplyShot(h.LaunchId,30,new Vector3(0,6,-15)).Applied,Is.True);h.Clock.Paused=false;h.Clock.DebugSlowMotion=.5;h.Advance(StepReal*2);Assert.That(h.Current.Step,Is.EqualTo(32));Assert.That(h.PredictionMatchesBitwise,Is.True);}
        [Test] public void ApplyShot_AfterFlightStopped_ResimulatesToRealPresentViaClockMap()
        {TestContext.WriteLine($"ROLLBACK_TEST_START unityReal={Time.realtimeSinceStartupAsDouble:R} utc={DateTime.UtcNow:O}");var h=Host();long last=h.Prediction.States.Last().Step;To(h,last);h.Advance(3*StepReal);var d=h.ApplyShot(h.LaunchId,last-3,new Vector3(0,6,-10));Assert.That(d.Applied,Is.True,d.Reason);Assert.That(d.StepsResimulated,Is.EqualTo(6));Assert.That(h.Current.Step,Is.EqualTo(last+3));}
        [TestCase(10d)][TestCase(70d)][TestCase(100d)][TestCase(200d)]
        public void ApplyShot_AfterFlightStopped_ExactStepsAtFixedRealBase(double realBase)
        {
            TestContext.WriteLine($"ROLLBACK_FIXED_START base={realBase:R} unityReal={Time.realtimeSinceStartupAsDouble:R} utc={DateTime.UtcNow:O}");
            var h=HostAtRealTime(realBase);long last=h.Prediction.States.Last().Step;To(h,last);
            h.Advance(3*StepReal);
            var d=h.ApplyShot(h.LaunchId,last-3,new Vector3(0,6,-10));
            Assert.That(d.Applied,Is.True,d.Reason);Assert.That(d.StepsResimulated,Is.EqualTo(6));
            Assert.That(h.Current.Step,Is.EqualTo(last+3));Assert.That(h.Clock.Alpha,Is.Zero);
            h.Advance(StepReal);Assert.That(h.Current.Step,Is.EqualTo(last+4));
        }
        [Test] public void AfterStoppedRollback_FractionalPresentAndNextFrameRemainContinuous()
        {var h=Host();long last=h.Prediction.States.Last().Step;To(h,last);h.Advance(3.4*StepReal);var d=h.ApplyShot(h.LaunchId,last-3,new Vector3(0,6,-10));Assert.That(d.Applied,Is.True,d.Reason);Assert.That(h.Current.Step,Is.EqualTo(last+3));Assert.That(h.Clock.Alpha,Is.EqualTo(.4f).Within(.00001));h.Advance(StepReal);Assert.That(h.Current.Step,Is.EqualTo(last+4));Assert.That(h.Clock.Alpha,Is.EqualTo(.4f).Within(.00001));}
        [Test] public void ApplyShot_RecordsPerPartCosts()
        {var h=Host();To(h,30);var d=h.ApplyShot(h.LaunchId,25,new Vector3(0,6,-15));Assert.That(d.Applied,Is.True);Assert.That(d.ReconstructionCostMs,Is.GreaterThanOrEqualTo(0));Assert.That(d.PredictionCostMs,Is.GreaterThan(0));Assert.That(d.ReconciliationCostMs,Is.GreaterThanOrEqualTo(0));Assert.That(d.OrderAgeMs,Is.GreaterThan(0));}
        [Test] public void ScheduledContactInsideMultiStepFrame_DoesNotAdvanceRemainderTwice()
        {var h=Host();To(h,23);Assert.That(h.ScheduleShot(h.LaunchId,25,s=>new Vector3(0,6,-15)),Is.True);h.Advance(5*StepReal);Assert.That(h.Current.Step,Is.EqualTo(28));Assert.That(h.PredictionMatchesBitwise,Is.True);}
        [Test] public void LaunchClockMap_UsesSlowMotionFromThisLaunchSnapshot()
        {var h=Host();h.Clock.DebugSlowMotion=.5;h.Launch(config);Assert.That(h.ClockMap.TrySimToReal(1,0,out var one),Is.True);Assert.That(one-h.LaunchRealTime,Is.EqualTo(StepReal).Within(1e-9));}
        [Test] public void ScheduledJump_UsesOldTrajectoryAtSameFramePresent()
        {var h=Host();To(h,23);var oldAtPresent=h.Prediction.States[27].Position;ShotApplyResult result=default;h.ShotApplied+=r=>result=r;Assert.That(h.ScheduleShot(h.LaunchId,25,s=>new Vector3(0,6,-15)),Is.True);h.Advance(5*StepReal);Assert.That(result.Applied,Is.True);Assert.That(result.JumpMeters,Is.EqualTo(Vector3.Distance(oldAtPresent,h.RenderPosition)).Within(1e-6));}
        [Test] public void FuturePerfect_ReservesOnce_NoSecondOrder()
        {var h=Host();To(h,20);Assert.That(h.ScheduleShot(h.LaunchId,25,s=>new Vector3(0,6,-15)),Is.True);Assert.That(h.ScheduleShot(h.LaunchId,26,s=>Vector3.up),Is.False);Assert.That(h.ApplyShot(h.LaunchId,20,Vector3.up).Reason,Is.EqualTo("attempt-consumed"));To(h,25);Assert.That(h.Current.Velocity,Is.EqualTo(new Vector3(0,6,-15)));}
        [Test] public void DuplicateEnded_AndReentrantCallback_ConsumeOnce()
        {var h=Host();To(h,30);int calls=0;h.ShotApplied+=r=>{calls++;Assert.That(h.ApplyShot(h.LaunchId,30,Vector3.up).Reason,Is.EqualTo("attempt-consumed"));};Assert.That(h.ApplyShot(h.LaunchId,28,new Vector3(0,6,-15)).Applied,Is.True);Assert.That(h.ApplyShot(h.LaunchId,28,Vector3.up).Applied,Is.False);Assert.That(calls,Is.EqualTo(1));}
        [TestCase(false)][TestCase(true)]
        public void ClockMap_StableStoppedOrPausedFlight_DoesNotRetainOneSegmentPerFrame(bool paused)
        {
            config=EffectiveConfig.From(config.Source.With("practice.enabled",0));var h=Host();
            if(paused){To(h,30);h.Clock.Paused=true;h.Advance(1d/60);}else To(h,h.Prediction.States.Last().Step);
            // Diagnostic introspection only: no new runtime parameter, time window, or acceptance threshold.
            var field=typeof(SquashClash.Core.Shot.ClockMap).GetField("segments",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Assert.That(field,Is.Not.Null);var segments=(System.Collections.ICollection)field.GetValue(h.ClockMap);
            int before=segments.Count;var state=h.Current;
            for(int i=0;i<120;i++)h.Advance(1d/60);
            int after=segments.Count;
            TestContext.WriteLine("S2_CLOCK_RETENTION paused="+paused+" frames=120 before="+before+" after="+after+" sameState="+state.Equals(h.Current));
            Bits(h.Current,state);
            Assert.That(after,Is.LessThanOrEqualTo(before+1),"Unchanged stopped/paused state must not retain every frame indefinitely.");
        }
        [Test] public void LongPause_ExpiresOldHistory_ThenResumeAllowsFreshRollback()
        {
            var h=Host();To(h,30);h.Clock.Paused=true;h.Advance(0);
            double pausedAt=h.ObservedRealTime;var before=h.Current;
            for(int i=0;i<3600;i++)h.Advance(1d/60);
            Bits(h.Current,before);
            Assert.That(h.ClockMap.TryRealToSim(pausedAt+30,out var step,out var fraction),Is.True);
            Assert.That(step,Is.EqualTo(30));Assert.That(fraction,Is.Zero);
            Assert.That(h.ApplyShot(h.LaunchId,30,Vector3.up).Reason,Is.EqualTo("history-expired"));
            h.Clock.Paused=false;h.Advance(0);To(h,36);
            Assert.That(h.ClockMap.TrySimToReal(34,0,out var originalEnded),Is.True);
            var result=h.ApplyShot(h.LaunchId,34,new Vector3(0,6,-15),originalEnded);
            Assert.That(result.Applied,Is.True,result.Reason);Assert.That(result.StepsResimulated,Is.EqualTo(2));
            Assert.That(result.OrderAgeMs,Is.EqualTo(2*StepReal*1000).Within(.0001));
            Assert.That(h.PredictionMatchesBitwise,Is.True);
            h.Advance(StepReal);Assert.That(h.Current.Step,Is.EqualTo(37));
        }
        [Test] public void NoShot_FirstBallBehaviourUnchanged()
        {var h=Host();var prediction=h.Prediction;long last=prediction.States.Last().Step;To(h,last);Bits(h.Current,prediction.States.Last());Assert.That(h.IsRunning,Is.False);Assert.That(h.PredictionMatchesBitwise,Is.True);h.Advance(1);Bits(h.Current,prediction.States.Last());}
    }
}
