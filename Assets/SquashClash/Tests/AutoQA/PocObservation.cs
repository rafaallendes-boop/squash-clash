using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Feedback;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using UnityEngine;
namespace SquashClash.AutoQA
{
    [Serializable] internal sealed class MetricLine {public string kind,payload,effectiveHash;public int pending;}
    [Serializable] internal sealed class PlanMetric {public long launchId;public double kInReal,kOutReal,targetReal;}
    [Serializable] internal sealed class GestureMetric {public string attemptId,outcome,gestureConfigHash;}
    [Serializable] internal sealed class PresentationMetric {public long launchId,contactStep;public string attemptId,category;}
    [Serializable] internal sealed class PocObservation
    {
        public string exactShot,additional;
        public double[] relativeMs;
        internal const double TimeToleranceMs=1e-6;
        private static readonly string[] Excluded=("AttemptId,PredecessorId,BeganAtReal,ReleaseEventAtReal,ReleaseCorrectedAtReal,InputReceivedAtReal,ProcessingAtReal,PlanPresentedAtReal,TargetContactAtReal,AidCutoffTarget,ProjectionObservedAt,LockObservedAt,PlannerCostMs,SolverCostMs,ReconstructionCostMs,PredictionCostMs,ReconciliationCostMs").Split(',');
        private static readonly string[] Relative={"DeltaMs","QuantizationErrorMsSigned","OrderAgeMs"};
        private static readonly string[] Exact=("Schema,Version,Seed,PlanVersion,Pending,O2Count,FingerId,StepsResimulated,ObsoleteEffects,LaunchId,ContactStep,TStarStep,GestureOutcome,Reason,Result,Fail,LaunchHash,GestureHash,Preset,FileHash,Build,Side,HasJumpPixels,HasRelease,Scheduled,EarlyGeometry,UsedNeighborLegalStep,QuantizationAboveThirdOfA,ValidLimitOn,EarlySoft,Applied,SpeedRaised,TargetLowered,KInSteps,KOutSteps,AssistMs,ValidMs,LatencyCompMs,Power,AimX,AimY,ContactX,ContactY,ContactZ,TargetUsedX,TargetUsedY,Speed,VelocityX,VelocityY,VelocityZ,JumpMeters,JumpPixels,JumpBallDiameters,AidLevel,AidCutoff,AidCutoffPlanVersion,CuePhase,Solved").Split(',');
        internal static IEnumerable<string> KnownFields=>Exact.Concat(Excluded).Concat(Relative);
        // ShotRecord is a flat JSON object. Parse its scalar fields without reflection on game types.
        internal static SortedDictionary<string,string> Fields(string json)
        {
            var fields=new SortedDictionary<string,string>(StringComparer.Ordinal);
            foreach(Match m in Regex.Matches(json,"\"(?<key>[^\"]+)\":(?<value>\"(?:[^\"\\\\]|\\\\.)*\"|[^,{}]+)"))fields.Add(m.Groups["key"].Value,m.Groups["value"].Value);
            return fields;
        }
        internal static string ExactRecord(string json)
        {
            var fields=Fields(json);var known=new HashSet<string>(Exact.Concat(Excluded).Concat(Relative));
            Assert.That(fields.Keys.Except(known),Is.Empty,"Unknown ShotRecord field not predeclared");
            Assert.That(known.Except(fields.Keys),Is.Empty,"Missing ShotRecord field");
            return string.Join("\n",fields.Where(p=>Exact.Contains(p.Key)).Select(p=>p.Key+"="+p.Value));
        }
        internal static PocObservation Capture(AppBootstrap app,string[] jsonl,Vector2 anchor)
        {
            var lines=jsonl.Select(JsonUtility.FromJson<MetricLine>).ToArray();long launch=app.Host.LaunchId;
            var shot=lines.Where(x=>x.kind=="shot").Select(x=>JsonUtility.FromJson<ShotRecord>(x.payload)).Last(x=>x.LaunchId==launch);
            string raw=lines.Where(x=>x.kind=="shot").Last(x=>JsonUtility.FromJson<ShotRecord>(x.payload).LaunchId==launch).payload;
            var plan=lines.Where(x=>x.kind=="shot-plan").Select(x=>JsonUtility.FromJson<PlanMetric>(x.payload)).Last(x=>x.launchId==launch);
            var gesture=lines.Where(x=>x.kind=="m24-gesture").Select(x=>JsonUtility.FromJson<GestureMetric>(x.payload)).Last();
            var presentations=lines.Where(x=>x.kind=="shot-presentation").Select(x=>JsonUtility.FromJson<PresentationMetric>(x.payload)).Where(x=>x.launchId==launch).ToArray();
            var closure=lines.Where(x=>x.kind=="shot-gesture").Select(x=>JsonUtility.FromJson<ShotRecord>(x.payload)).Last(x=>x.LaunchId==launch);
            Assert.That(gesture.attemptId,Is.EqualTo(closure.AttemptId));
            Assert.That(presentations.All(p=>p.attemptId==closure.AttemptId),Is.True);
            var c=app.GetComponent<ShotController>();var feedback=app.GetComponent<ShotFeedback>();var input=app.GetComponent<TouchInputAdapter>();
            float mm=ScreenMetrics.MmPerPx(Screen.dpi,(float)app.Tuning.Applied.Get("input.fallbackDpi"),out var dpi);
            var first=app.Host.Prediction.Contacts.First();
            float error=Vector2.Distance(new Vector2(first.Point.x,first.Point.y),new Vector2(shot.TargetUsedX,shot.TargetUsedY));
            var extra=new Additional {closure=gesture.outcome,launchId=launch,identityMatches=true,presentations=presentations.Length,
                category=presentations.Length==1?presentations[0].category:null,presentationStep=presentations.Length==1?presentations[0].contactStep:0,
                discrepancies=lines.Count(x=>x.kind=="shot-presentation-discrepancy"),attemptConsumed=c.AttemptConsumed,
                clip=feedback.LastClip,perfect=feedback.PerfectCount,valid=feedback.ValidCount,swish=feedback.SwishCount,contact=feedback.ContactCount,
                predictionMatches=app.Host.PredictionMatchesBitwise,hash=app.Host.ActiveConfig.Hash,gestureHash=gesture.gestureConfigHash,
                simulationKey=app.Host.ActiveConfig.SimulationKey,backend=app.Host.ActiveConfig.Backend,pending=app.Tuning.PendingKeys.Count,
                width=Screen.width,height=Screen.height,safeArea=Screen.safeArea,dpi=Screen.dpi,dpiSource=dpi.ToString(),mmPerPx=mm,uiBlocked=input.UiBlocker(anchor),
                current=app.Host.Current,firstSurface=first.Surface.ToString(),firstPoint=first.Point,firstStep=first.Step,frontError=error};
            double target=shot.TargetContactAtReal;
            double RelativeTime(double value)=>target==0?0:(value-target)*1000;
            return new PocObservation {exactShot=ExactRecord(raw),additional=JsonUtility.ToJson(extra),relativeMs=new[]{
                shot.DeltaMs,shot.QuantizationErrorMsSigned,shot.OrderAgeMs,RelativeTime(shot.BeganAtReal),
                RelativeTime(shot.ReleaseEventAtReal),RelativeTime(shot.ReleaseCorrectedAtReal),RelativeTime(shot.InputReceivedAtReal),
                RelativeTime(shot.ProcessingAtReal),RelativeTime(shot.ProjectionObservedAt),RelativeTime(shot.LockObservedAt),
                (plan.kInReal-plan.targetReal)*1000,(plan.kOutReal-plan.targetReal)*1000}};
        }
        internal void AssertIdentical(PocObservation other)
        {
            Assert.That(other.exactShot,Is.EqualTo(exactShot),"Exact semantic ShotRecord fields");
            Assert.That(other.additional,Is.EqualTo(additional),"Exact additional observables");
            Assert.That(other.relativeMs.Length,Is.EqualTo(relativeMs.Length));
            for(int i=0;i<relativeMs.Length;i++)
            {
                Assert.That(double.IsNaN(other.relativeMs[i])||double.IsInfinity(other.relativeMs[i]),Is.False);
                Assert.That(other.relativeMs[i],Is.EqualTo(relativeMs[i]).Within(TimeToleranceMs),"Predeclared relativeMs["+i+"]");
            }
        }
        [Serializable] private sealed class Additional
        {
            public string closure,category,clip,hash,gestureHash,simulationKey,backend,dpiSource,firstSurface;
            public long launchId,presentationStep,firstStep;
            public int presentations,discrepancies,perfect,valid,swish,contact,pending,width,height;
            public bool identityMatches,attemptConsumed,predictionMatches,uiBlocked;
            public float dpi,mmPerPx,frontError;public Rect safeArea;
            public SquashClash.Core.Simulation.BallState current;public Vector3 firstPoint;
        }
    }
}
