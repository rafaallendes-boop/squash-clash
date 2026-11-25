using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // 24 nov: the amber ring closes over its tap window, the white ring pulses while it waits, one text for the rule. Presentation only.
    public sealed class ZRingClosePulseViewTests
    {
        private GameObject root;private TimingRingView view;private double target,p;private ContactPlan plan;
        private static readonly TimingSettings Settings=new TimingSettings(40,true,100);
        [SetUp] public void Setup()
        {
            root=new GameObject("close pulse");view=root.AddComponent<TimingRingView>();
            var body=new BodyFrame(new Vector2(3.2f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f);
            plan=new ContactPlan(true,new SimTime(100,0),new SimTime(200,0),150,new Vector3(2.6f,.6f,5.5f),StrokeSide.Drive,7,body,new LegalSettings(.05f,2,100,0),CourtDimensions.Standard());
        }
        [TearDown] public void Cleanup()=>UnityEngine.Object.DestroyImmediate(root);
        private LineRenderer Line(string name)=>root.GetComponentsInChildren<LineRenderer>(true).SingleOrDefault(r=>r.name==name);
        private float Radius(string name="Converging ring")=>Line(name).GetPosition(0).magnitude;
        private float CueRadius=>.1f+.35f*view.CueState.Approach;
        // Shows the ring far from its contact (the ring waits at the outer radius) and returns the moment it was shown.
        private void Show(Color tint,double secondsToTarget=10,double? from=null,double? until=null)
        {
            view.SetTint(tint);target=Time.realtimeSinceStartupAsDouble+secondsToTarget;
            if(from.HasValue)view.SetClosingWindow(from.Value,until.Value);
            view.Show(plan,target,true,Settings);p=Time.realtimeSinceStartupAsDouble;
        }
        private void Window(double fromAt,double untilAt){view.SetClosingWindow(p+fromAt,p+untilAt);}
        private float Alpha(string name="Converging ring")=>Line(name).startColor.a;
        [Test] public void A1_AmberRadiusFallsLinearlyFromOuterToTwiceTheFitRadiusOverTheWindow()
        {
            Show(ShotController.VolleyTint);Window(0,4);
            view.Tick(p);Assert.That(Radius(),Is.EqualTo(.45f).Within(1e-4f));
            view.Tick(p+2);Assert.That(Radius(),Is.EqualTo(.325f).Within(1e-4f));
            view.Tick(p+4);Assert.That(Radius(),Is.EqualTo(.2f).Within(1e-4f));
        }
        [Test] public void A2_TheHaloHasTheRadiusOfTheRingAtAllTimes()
        {
            Show(ShotController.VolleyTint,10);Window(0,4);
            for(double t=0;t<=6;t+=.25){view.Tick(p+t);Assert.That(Radius("Volley halo"),Is.EqualTo(Radius()).Within(1e-5f),"t="+t);}
            Assert.That(Radius(),Is.EqualTo(.2f).Within(1e-4f),"after the window it stays closed");
        }
        [Test] public void A3_WithoutAWindowOrWithAnInfiniteOne_TheAmberRadiusIsTheImpactCueOne()
        {
            Show(ShotController.VolleyTint,1);
            foreach(double t in new[]{0,.2,.6,.8,.95}){view.Tick(p+t);Assert.That(Radius(),Is.EqualTo(CueRadius).Within(1e-5f),"no window t="+t);}
            view.SetClosingWindow(p,double.PositiveInfinity);view.Tick(p+.2);Assert.That(Radius(),Is.EqualTo(.45f).Within(1e-5f),"infinite window");
            view.SetClosingWindow(p+1,p+1);view.Tick(p+.2);Assert.That(Radius(),Is.EqualTo(.45f).Within(1e-5f),"empty window");
        }
        [Test] public void A4_TheDrawnRadiusIsNeverAboveTheImpactCueOneNorBelowTwiceTheFitRadiusBecauseOfClosing()
        {
            Show(ShotController.VolleyTint,2);Window(0,1.2);
            bool closedWhileCueStillWide=false;
            for(double t=0;t<2;t+=.02)
            {
                view.Tick(p+t);float r=Radius();
                Assert.That(r,Is.LessThanOrEqualTo(CueRadius+1e-5f),"t="+t);
                if(CueRadius>=.2f)Assert.That(r,Is.GreaterThanOrEqualTo(.2f-1e-5f),"t="+t);
                if(CueRadius>.25f&&r<CueRadius-.02f)closedWhileCueStillWide=true;
            }
            Assert.That(closedWhileCueStillWide,Is.True,"the closing, not the cue, set the radius for a while");
            view.Tick(target+.001);Assert.That(Visible,Is.True);
        }
        private bool Visible=>view.Visible;
        // B1 (the white ring breathes at 2 Hz) was replaced by P1 on 25 nov: the breathing was replaced by one pulse on entry.
        [Test] public void P1_TheWaitingWhiteRingPulsesOnceOnEntryAndThenIsSteadyAtOne()
        {
            Show(Color.white);
            view.Tick(p);Assert.That(Alpha(),Is.EqualTo(1f).Within(.01f),"1 when it shows");
            view.Tick(p+.25);Assert.That(Alpha(),Is.EqualTo(.45f).Within(.01f),"0.45 at 0.25 s");
            view.Tick(p+.5);Assert.That(Alpha(),Is.EqualTo(1f).Within(.01f),"1 at 0.5 s");
            for(double t=0;t<3;t+=.013)
            {
                view.Tick(p+t);Assert.That(Alpha(),Is.InRange(.45f-1e-3f,1f+1e-3f),"t="+t);Assert.That(Line("Converging ring").enabled,Is.True,"never hidden: t="+t);
                if(t>=.5)Assert.That(Alpha(),Is.EqualTo(1f).Within(1e-5f),"steady after the pulse: t="+t);
            }
        }
        [UnityTest] public IEnumerator P2_AReplanOfTheWhiteRingInTheSameOpportunityDoesNotRestartThePulse()
        {
            Show(Color.white);
            yield return new WaitForSecondsRealtime(.3f);
            Show(Color.white);
            view.Tick(p+.25);Assert.That(Alpha(),Is.EqualTo(1f).Within(1e-5f),"0.25 s after the replan the pulse is long over");
        }
        [Test] public void P3_WhenTheAmberYieldsToWhiteThereIsANewPulse()
        {
            Show(ShotController.VolleyTint);view.Tick(p+.25);Assert.That(Alpha(),Is.EqualTo(1f).Within(1e-5f),"amber does not pulse");
            view.BeginOpportunity();Show(Color.white);
            view.Tick(p+.25);Assert.That(Alpha(),Is.EqualTo(.45f).Within(.01f),"0.45 at 0.25 s from the white entry");
            view.Tick(p+.75);Assert.That(Alpha(),Is.EqualTo(1f).Within(1e-5f),"1 at 0.75 s");
        }
        [Test] public void P4_AWhiteRingThatEntersAlreadyConvergingDoesNotPulse()
        {
            Show(Color.white,.3);
            view.Tick(p+.1);Assert.That(view.CueState.Approach,Is.LessThan(1f),"already converging");Assert.That(Alpha(),Is.EqualTo(1f).Within(1e-5f));
        }
        [Test] public void B2_TheConvergingWhiteRingIsSteadyAtOne()
        {
            Show(Color.white,2);
            foreach(double t in new[]{1.5,1.55,1.6,1.7,1.8,1.9,1.99}){view.Tick(p+t);Assert.That(view.CueState.Approach,Is.LessThan(1f));Assert.That(Alpha(),Is.EqualTo(1f).Within(1e-5f),"t="+t);}
            view.Tick(target+.01);Assert.That(Alpha(),Is.EqualTo(1f).Within(1e-5f),"after T*");
        }
        [Test] public void B3_TheAmberRingTheTriangleAndTheBandsDoNotPulse()
        {
            Show(ShotController.VolleyTint);
            view.Tick(p+.25);Assert.That(Alpha(),Is.EqualTo(1f).Within(1e-5f),"amber");
            Show(Color.white);view.Tick(p);float tri=Alpha("Fixed triangle"),band=Alpha("Perfect band"),valid=Alpha("Valid band");
            view.Tick(p+.25);Assert.That(Alpha("Converging ring"),Is.EqualTo(.45f).Within(.01f));
            Assert.That(Alpha("Fixed triangle"),Is.EqualTo(tri).Within(1e-5f));Assert.That(Alpha("Perfect band"),Is.EqualTo(band).Within(1e-5f));
            Assert.That(Alpha("Valid band"),Is.EqualTo(valid).Within(1e-5f));
        }
    }
    public sealed class ZRingClosePulseMatchTests
    {
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;private TimingRingView ring;private ReceptionCaptionView caption;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Close pulse fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();rally=app.Rally;ring=app.GetComponent<TimingRingView>();caption=app.GetComponentInChildren<ReceptionCaptionView>(true);
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;rally.AutoRestart=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){SquashClash.Runtime.Visual.PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}
        // The controller's per-frame path (LateUpdate): the closing window, then the ring's tick.
        private void Frame()
        {
            app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();caption?.Tick();
            controller.UpdateRingClosing();ring.Tick(app.Host.ObservedRealTime);
        }
        private void StartRivalServe(int seed=3)
        {
            rally.StopMode();rally.Autopilot=false;rally.Level=AiLevel.Medium;rally.Seed=seed;rally.SetParams(new RallyParams{ServeKindOn=1});
            Assert.That(rally.StartMatch(),Is.True);
            var t=typeof(RallyDirector);
            t.GetProperty("Match").GetSetMethod(true).Invoke(rally,new object[]{new MatchScore((int)rally.Params.MatchTarget,(int)rally.Params.MatchMargin,PlayerId.Rival)});
            t.GetMethod("BeginPoint",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(rally,null);
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.HumanReception));
        }
        private float Radius=>GameObjectLine().GetPosition(0).magnitude;
        private LineRenderer GameObjectLine()=>ring.GetComponentsInChildren<LineRenderer>(true).Single(r=>r.name=="Converging ring");
        private void StartRally()
        {
            Assert.That(rally.StartMode(),Is.True);Assert.That(controller.Volley.Exists,Is.True,controller.Volley.Reason);
        }
        // Presentation only (25 nov). Drives a reception without touching: the amber offer closes inside its window and, once white, the waiting ring is steady.
        private void RunUntouched(out List<float> amber,out int steadySamples)
        {
            amber=new List<float>();steadySamples=0;double? whiteSince=null;bool wasAmber=false;
            for(int i=0;i<1500&&!controller.AttemptConsumed;i++)
            {
                Frame();
                if(controller.VolleyCommitted)break;
                double now=app.Host.ObservedRealTime;
                if(ring.Visible&&ring.Tint==ShotController.VolleyTint&&controller.VolleyShown&&controller.TryVolleyWindow(out double from,out double until)&&!double.IsInfinity(until)&&now>=from&&now<=until)
                {amber.Add(Radius);wasAmber=true;}
                else if(wasAmber&&ring.Visible&&ring.Tint==Color.white&&!controller.VolleyShown)
                {
                    whiteSince=whiteSince??now;
                    if(ring.CueState.Approach>=1&&now-whiteSince.Value>.5){steadySamples++;Assert.That(GameObjectLine().startColor.a,Is.EqualTo(1f).Within(1e-5f),"steady white at "+(now-whiteSince.Value));}
                }
            }
        }
        private void AssertClosesMonotonically(List<float> radii)
        {
            Assert.That(radii.Count,Is.GreaterThanOrEqualTo(10),"the amber ring was offered with a finite window");
            for(int i=1;i<radii.Count;i++)Assert.That(radii[i],Is.LessThanOrEqualTo(radii[i-1]+1e-5f),"monotonic, sample "+i);
            Assert.That(radii[radii.Count-1],Is.LessThan(radii[0]-1e-3f),"it really closes");
        }
        [Test] public void U1a_RivalServeReceptionWithTheServeVolley_AmberClosesAndTheWhiteIsSteadyAfterItsPulse()
        {
            var seen=new List<string>();List<float> amber=null;int steady=0;
            for(int seed=1;seed<=24;seed++)
            {
                StartRivalServe(seed);RunUntouched(out amber,out steady);seen.Add(seed+":"+amber.Count);
                if(amber.Count>=10)break;
                rally.StopMode();
            }
            Assert.That(amber.Count,Is.GreaterThanOrEqualTo(10),"no seed gave an amber serve offer: "+string.Join(" ",seen));
            AssertClosesMonotonically(amber);
            Assert.That(steady,Is.GreaterThanOrEqualTo(3),"waiting-white samples past the pulse");
        }
        [Test] public void U1b_RallyReceptionWithAnAmberOffer_AmberClosesAndTheWhiteIsSteadyAfterItsPulse()
        {
            StartRally();
            RunUntouched(out var amber,out int steady);
            AssertClosesMonotonically(amber);
            Assert.That(steady,Is.GreaterThanOrEqualTo(3),"waiting-white samples past the pulse");
        }
        [Test] public void A5_TheUnchosenAmberRingNeverGrowsInsideItsWindow_AndAChosenOneFollowsTheImpactCue()
        {
            StartRally();var radii=new List<float>();bool chose=false;float afterChoice=-1,cueAfter=-1;
            for(int i=0;i<900&&!controller.AttemptConsumed;i++)
            {
                Frame();
                if(controller.VolleyCommitted)
                {
                    if(!chose){chose=true;}
                    afterChoice=Radius;cueAfter=.1f+.35f*ring.CueState.Approach;break;
                }
                if(ring.Visible&&ring.Tint==ShotController.VolleyTint&&controller.VolleyShown&&controller.TryVolleyWindow(out double from,out double until)&&!double.IsInfinity(until)&&app.Host.ObservedRealTime>=from&&app.Host.ObservedRealTime<=until)
                {
                    radii.Add(Radius);
                    if(radii.Count==20)controller.GestureBegan(app.Host.ObservedRealTime);
                }
            }
            Assert.That(radii.Count,Is.GreaterThanOrEqualTo(10),"the amber ring was offered with a finite window");
            for(int i=1;i<radii.Count;i++)Assert.That(radii[i],Is.LessThanOrEqualTo(radii[i-1]+1e-5f),"monotonic, sample "+i);
            Assert.That(radii[radii.Count-1],Is.LessThan(radii[0]-1e-3f),"it really closes");
            Assert.That(chose,Is.True,"the touch inside the window chose the volley");Assert.That(afterChoice,Is.EqualTo(cueAfter).Within(1e-4f),"chosen: the ImpactCue radius");
        }
    }
    public sealed class ZRingRuleTextTests : ZFirstRunFixture
    {
        private const string Literal="Anillo ámbar: toca y volea en el aire. Anillo blanco: espera el bote y golpea.";
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        [UnityTest] public IEnumerator C1_TutorialStep4_HintH1_AndTheCaptionAreTheSameRule()
        {
            Click("menu-howto");Refresh();yield return null;
            for(int i=0;i<3;i++){Click("tutorial-next");Refresh();yield return null;}
            Assert.That(Find<Label>("tutorial-title").text,Is.EqualTo("Volea"));
            string tutorial=Find<Label>("tutorial-body").text;
            Assert.That(tutorial,Is.EqualTo(ReceptionRule.Full));
            Assert.That(FirstMatchHints.Text(HintId.Volley,HintWhere.Banner),Is.EqualTo(ReceptionRule.Full));
            Assert.That(app.GetComponentInChildren<ReceptionCaptionView>(true).Text,Is.EqualTo(ReceptionRule.White));
            Assert.That(ReceptionRule.Full,Does.StartWith(ReceptionRule.Amber).And.EndWith(ReceptionRule.White));
            Assert.That(ReceptionRule.Full,Is.EqualTo(ReceptionRule.Amber+" "+ReceptionRule.White));
        }
        [Test] public void C2_TheRuleIsLiterallyTheApprovedText()
        {
            Assert.That(ReceptionRule.Amber,Is.EqualTo("Anillo ámbar: toca y volea en el aire."));
            Assert.That(ReceptionRule.White,Is.EqualTo("Anillo blanco: espera el bote y golpea."));
            Assert.That(ReceptionRule.Full,Is.EqualTo(Literal));
        }
    }
}
