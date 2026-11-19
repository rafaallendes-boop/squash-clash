using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Volley v1 end to end on the human path (design 2026-11-18): the VL1 practice recipe (fixed body, both opportunities) and the rally hook.
    // Named after the rally fixtures on purpose (see ZRallyDirectorTests).
    public sealed class ZVolleyTests
    {
        private AppBootstrap app;private ShotController controller;private PracticeMode practice;private RallyDirector rally;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Volley fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.Tuning.Set("fatigue.enabled",0);
            controller=app.GetComponent<ShotController>();practice=app.GetComponent<PracticeMode>();rally=app.Rally;
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=practice.enabled=rally.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return PlayModeTestWorld.ClearApp();}
        private void LaunchVl1(){app.Tuning.Set("practice.launch",PracticeLaunchDefinitions.VolleyIndex);app.LaunchCurrent();}
        private void At(double target)
        {
            for(int i=0;i<3000&&app.Host.ObservedRealTime+StepReal<target-1e-10;i++){app.Host.Advance(StepReal);controller.ProcessPending();}
            double left=target-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);controller.ProcessPending();
        }
        private double Real(double steps){var t=SimTime.FromSteps(steps);Assert.That(app.Host.ClockMap.TrySimToReal(t.Step,t.Fraction,out var real),Is.True);return real;}
        private ShotRecord Submit(double time)
        {
            At(time);
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,time,true,GestureOutcome.Swing,new Vector2(3.2f,2.2f),.5f,app.Host.ActiveConfig.Source,"fixture",time));
            controller.ProcessPending();Assert.That(controller.LastRecord,Is.Not.Null);return controller.LastRecord;
        }
        private long FirstBounceStep(){var e=app.Host.Prediction.Contacts.First(c=>c.Surface==SurfaceId.Floor&&!c.Persistent);return e.Step;}
        // Metric kinds written for the current launch only (the metrics file can outlive a fixture).
        private string[] Kinds()=>File.ReadLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Line>)
            .Where(l=>l.kind!=null&&l.kind.StartsWith("shot-opportunity",StringComparison.Ordinal)&&JsonUtility.FromJson<Payload>(l.payload).launchId==app.Host.LaunchId).Select(l=>l.kind).ToArray();
        [Serializable] private sealed class Line {public string kind,payload;}
        [Serializable] private sealed class Payload {public long launchId;}

        [Test] public void Vl1_PresentsTheVolleyRingFirst_TintedAtItsPoint()
        {
            LaunchVl1();var ring=app.GetComponent<TimingRingView>();
            Assert.That(controller.Volley.Exists,Is.True,controller.Volley.Reason);Assert.That(controller.Plan.Exists,Is.True);
            Assert.That(controller.VolleyShown,Is.True);Assert.That(controller.ShownPlan.TStarStep,Is.EqualTo(controller.Volley.Plan.TStarStep));
            Assert.That(ring.Tint,Is.EqualTo(ShotController.VolleyTint));
            Assert.That(controller.Volley.Plan.TStarStep,Is.LessThan(FirstBounceStep()),"the volley contact is before the bounce");
            Assert.That(controller.VolleyCausalReference,Is.Not.Null);Assert.That(controller.VolleyCausalReference,Is.Not.SameAs(controller.CausalReference));
            Assert.That(Kinds(),Does.Contain("shot-opportunity"));
        }
        [Test] public void Vl1_ReleaseAtVolleyTStar_HitsBeforeTheBounce()
        {
            LaunchVl1();long bounce=FirstBounceStep();var r=Submit(Real(controller.Volley.Plan.TStarStep));
            Assert.That(controller.LastOpportunity,Is.EqualTo(Opportunity.Volley));
            Assert.That(r.Result,Is.EqualTo("Perfect"),r.Reason);
            Assert.That(r.TStarStep,Is.EqualTo(controller.Volley.Plan.TStarStep));Assert.That(r.KOutSteps,Is.EqualTo(controller.Volley.Plan.KOut.Steps));
            At(app.Host.ObservedRealTime+.3);
            Assert.That(controller.LastRecord.Applied,Is.True,controller.LastRecord.Reason);Assert.That(controller.LastRecord.ContactStep,Is.LessThan(bounce));
            Assert.That(controller.UsedCausalReference,Is.SameAs(controller.VolleyCausalReference));
            // 19 nov (PO): no «Volea · » text over the play; the note is the usual result and the volley gets a figure instead.
            Assert.That(app.GetComponent<ShotNoteView>().Text,Is.EqualTo("Perfecto"));
            Assert.That(app.GetComponent<SwingView>().VolleyPose,Is.True);
            Assert.That(Kinds(),Does.Contain("shot-opportunity-order"));
        }
        [Test] public void Vl1_VolleyContact_BurstsTheAmberRingAtTheContact_ThenFades()
        {
            LaunchVl1();var burst=app.GetComponent<VolleyBurstView>();Assert.That(burst,Is.Not.Null,"the volley figure exists with the shot views");
            Assert.That(burst.IsVisible,Is.False);
            // 20 ms early (inside Perfect): the contact is scheduled ahead, so nothing shows until the contact itself.
            var order=Submit(Real(controller.Volley.Plan.TStarStep)-.02);Assert.That(order.Result,Is.EqualTo("Perfect"),order.Reason);
            Assert.That(controller.WaitingForContact,Is.True);Assert.That(burst.ShowCount,Is.Zero,"nothing before the contact itself");
            At(app.Host.ObservedRealTime+.3);var r=controller.LastRecord;Assert.That(r.Applied,Is.True,r.Reason);
            Assert.That(burst.ShowCount,Is.EqualTo(1));Assert.That(burst.IsVisible,Is.True);
            Assert.That(Vector3.Distance(burst.Burst.position,new Vector3(r.ContactX,r.ContactY,r.ContactZ)),Is.LessThan(1e-4f),"at the contact point");
            Assert.That(burst.Tint,Is.EqualTo(ShotController.VolleyTint));
            Assert.That(burst.Radius,Is.EqualTo(VolleyBurstView.StartRadius).Within(1e-6f));Assert.That(burst.Alpha,Is.EqualTo(1).Within(1e-6f));
            app.Clock.Paused=true;burst.Tick(.1);Assert.That(burst.Radius,Is.EqualTo(VolleyBurstView.StartRadius).Within(1e-6f),"a pause freezes the figure");
            app.Clock.Paused=false;burst.Tick(.1);
            Assert.That(burst.Radius,Is.GreaterThan(VolleyBurstView.StartRadius));Assert.That(burst.Radius,Is.LessThan(VolleyBurstView.EndRadius));
            Assert.That(burst.Alpha,Is.LessThan(1));Assert.That(burst.Alpha,Is.GreaterThan(0));
            burst.Tick(VolleyBurstView.DurationSeconds);Assert.That(burst.IsVisible,Is.False,"gone after a quarter of a second");
            Assert.That(burst.Burst.gameObject.activeSelf,Is.False);
            Assert.That(app.GetComponent<ShotNoteView>().Text,Does.Not.Contain("Volea"));
        }
        [Test] public void Vl1_BounceContact_HasNoVolleyFigure()
        {
            LaunchVl1();var burst=app.GetComponent<VolleyBurstView>();Assert.That(burst,Is.Not.Null);
            var r=Submit(Real(controller.Plan.TStarStep));Assert.That(controller.LastOpportunity,Is.EqualTo(Opportunity.Bounce));Assert.That(r.Result,Is.EqualTo("Perfect"),r.Reason);
            At(app.Host.ObservedRealTime+.3);Assert.That(controller.LastRecord.Applied,Is.True,controller.LastRecord.Reason);
            Assert.That(burst.ShowCount,Is.Zero);Assert.That(burst.IsVisible,Is.False);
            Assert.That(app.GetComponent<ShotNoteView>().Text,Is.EqualTo("Perfecto"));
        }
        [Test] public void Vl1_ReleaseAfterTheSplit_IsTheBounceOpportunity_NeverAnAccidentalVolley()
        {
            LaunchVl1();double s=controller.Volley.SplitSteps,b=controller.Plan.KIn.Steps;
            var r=Submit(Real(s+(b-s)*.5));
            Assert.That(controller.LastOpportunity,Is.EqualTo(Opportunity.Bounce));Assert.That(r.Result,Is.EqualTo("Failed"));Assert.That(r.Fail,Is.EqualTo("Air"));
            Assert.That(app.GetComponent<ShotNoteView>().Text,Does.Not.StartWith("Volea"));
        }
        [Test] public void Vl1_ReleaseBetweenVolleyKOutAndSplit_IsLateOnTheVolley()
        {
            LaunchVl1();double k=controller.Volley.Plan.KOut.Steps,s=controller.Volley.SplitSteps;
            var r=Submit(Real(k+(s-k)*.5));
            Assert.That(controller.LastOpportunity,Is.EqualTo(Opportunity.Volley));Assert.That(r.Fail,Is.EqualTo("Late"));
        }
        [Test] public void Vl1_WithoutRelease_TheRingSwitchesToTheBounceAtTheSplit()
        {
            LaunchVl1();var ring=app.GetComponent<TimingRingView>();
            At(Real(controller.Volley.SplitSteps)-StepReal*2);Assert.That(controller.VolleyShown,Is.True);
            At(Real(controller.Volley.SplitSteps)+StepReal);
            Assert.That(controller.VolleyShown,Is.False);Assert.That(controller.ShownPlan.TStarStep,Is.EqualTo(controller.Plan.TStarStep));
            Assert.That(ring.Tint,Is.EqualTo(Color.white));
            var r=Submit(Real(controller.Plan.TStarStep));Assert.That(controller.LastOpportunity,Is.EqualTo(Opportunity.Bounce));Assert.That(r.Result,Is.EqualTo("Perfect"),r.Reason);
        }
        [Test] public void Sl1_HasNoVolley_AndKeepsTheSinglePlan()
        {
            app.Tuning.Set("practice.launch",0);app.LaunchCurrent();var ring=app.GetComponent<TimingRingView>();
            Assert.That(controller.Volley.Exists,Is.False);Assert.That(controller.ShownPlan.TStarStep,Is.EqualTo(controller.Plan.TStarStep));
            Assert.That(ring.Tint,Is.EqualTo(Color.white));Assert.That(Kinds(),Does.Not.Contain("shot-opportunity"));
        }
        [Test] public void Rally_OffersTheHookToTheHuman_AndTheDevSwitchAppliesNextPoint()
        {
            rally.AutoRestart=false;rally.StartMode();
            Assert.That(rally.Rules.VolleyAllowed,Is.True);Assert.That(Kinds(),Does.Contain("shot-opportunity"),"every human launch reports its volley opportunity");
            Assert.That(rally.VolleyNextPoint,Is.True);rally.SetVolley(false);Assert.That(rally.VolleyNextPoint,Is.False);
            rally.Rematch();Assert.That(rally.Params.VolleyEnabled,Is.EqualTo(0));Assert.That(rally.Rules.VolleyAllowed,Is.False);
            Assert.That(controller.Volley.Reason,Is.Null,"no hook without the volley");
        }
    }
}
