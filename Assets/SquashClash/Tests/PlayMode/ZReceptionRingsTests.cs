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
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Look of the two reception rings (PO, 23 nov): the amber volley ring reads apart from the white bounce ring, and «Espera el bote»
    // sits on the white ring of the rival's serve. Presentation only: when each ring shows is not touched (ZVolleyV2Tests keeps that).
    public sealed class ZReceptionRingsLookTests
    {
        private GameObject root;private TimingRingView view;private double target;private ContactPlan plan;
        private static readonly TimingSettings Settings=new TimingSettings(40,true,100);
        [SetUp] public void Setup()
        {
            root=new GameObject("rings look");view=root.AddComponent<TimingRingView>();target=Time.realtimeSinceStartupAsDouble+1;
            var body=new BodyFrame(new Vector2(3.2f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f);
            plan=new ContactPlan(true,new SimTime(100,0),new SimTime(200,0),150,new Vector3(2.6f,.6f,5.5f),StrokeSide.Drive,7,body,new LegalSettings(.05f,2,100,0),CourtDimensions.Standard());
        }
        [TearDown] public void Cleanup()=>UnityEngine.Object.DestroyImmediate(root);
        private LineRenderer Line(string name)=>root.GetComponentsInChildren<LineRenderer>(true).SingleOrDefault(r=>r.name==name);
        private void ShowRing(Color tint,double inSeconds=.4)
        {
            view.SetTint(tint);view.Show(plan,target,true,Settings);view.Tick(target-inSeconds);
        }
        [Test] public void AmberRing_IsDrawnThickerThanTheWhiteOne()
        {
            ShowRing(ShotController.VolleyTint);float amber=Line("Converging ring").widthMultiplier;
            view.SetTint(Color.white);view.Tick(target-.4);float white=Line("Converging ring").widthMultiplier;
            Assert.That(amber,Is.GreaterThanOrEqualTo(white*1.8f),"amber 4 px against white 2 px");
        }
        [Test] public void AmberRing_HasAHaloAtTheSameRadius_SoftAndWiderThanTheRing()
        {
            ShowRing(ShotController.VolleyTint);var halo=Line("Volley halo");
            Assert.That(halo,Is.Not.Null,"the halo exists for the amber ring");Assert.That(halo.enabled,Is.True);
            Assert.That(halo.GetPosition(0).magnitude,Is.EqualTo(Line("Converging ring").GetPosition(0).magnitude).Within(1e-5f));
            Assert.That(halo.widthMultiplier,Is.GreaterThan(Line("Converging ring").widthMultiplier));
            Assert.That(halo.startColor.a,Is.InRange(.2f,.5f),"soft");
            Assert.That(halo.startColor.r,Is.EqualTo(ShotController.VolleyTint.r).Within(.01f));Assert.That(halo.startColor.g,Is.EqualTo(ShotController.VolleyTint.g).Within(.01f));
        }
        [Test] public void WhiteRing_HasNoHalo()
        {
            ShowRing(Color.white);var halo=Line("Volley halo");
            Assert.That(halo==null||!halo.enabled,Is.True);
        }
        [Test] public void Halo_FollowsTheRingWhenItClosesAndWhenTheAidHidesIt()
        {
            ShowRing(ShotController.VolleyTint);Assert.That(Line("Volley halo"),Is.Not.Null);Assert.That(Line("Volley halo").enabled,Is.True);
            float before=Line("Volley halo").GetPosition(0).magnitude;view.Tick(target-.1);
            Assert.That(Line("Volley halo").GetPosition(0).magnitude,Is.LessThan(before),"it converges with the ring");
            view.BeginReception(2);view.Show(plan,target,true,Settings);view.Tick(target-.2);
            Assert.That(Line("Volley halo")==null||!Line("Volley halo").enabled,Is.True,"aid level 2 shows no ring, so no halo");
        }
        [Test] public void AmberAndWhiteBands_KeepTheirStroke()
        {
            ShowRing(ShotController.VolleyTint);float amber=Line("Perfect band").widthMultiplier;
            view.SetTint(Color.white);view.Tick(target-.4);Assert.That(Line("Perfect band").widthMultiplier,Is.EqualTo(amber).Within(1e-6f),"only the converging ring is thickened");
        }
    }
    public sealed class ZReceptionCaptionTests
    {
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;private TimingRingView ring;private ReceptionCaptionView caption;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Rings fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();rally=app.Rally;ring=app.GetComponent<TimingRingView>();caption=app.GetComponentInChildren<ReceptionCaptionView>(true);
            app.LaunchCurrent();app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;rally.AutoRestart=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){SquashClash.Runtime.Visual.PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}
        private void At(double target)
        {
            for(int i=0;i<6000&&app.Host.ObservedRealTime+StepReal<target-1e-10;i++){app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();caption.Tick();}
            double left=target-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);controller.ProcessPending();rally.Tick();caption.Tick();
        }
        private void Step(){app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();caption?.Tick();}
        // Test fixture only: the match starts with the rival to serve (the director always opens with the human), then the point is begun
        // again, so the human receives a real serve without playing a point first.
        private void StartRivalServe()
        {
            rally.StopMode();rally.Autopilot=false;rally.Level=AiLevel.Medium;rally.Seed=3;rally.SetParams(new RallyParams{ServeKindOn=1});
            Assert.That(rally.StartMatch(),Is.True);
            var t=typeof(RallyDirector);
            t.GetProperty("Match").GetSetMethod(true).Invoke(rally,new object[]{new MatchScore((int)rally.Params.MatchTarget,(int)rally.Params.MatchMargin,PlayerId.Rival)});
            t.GetMethod("BeginPoint",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(rally,null);
            Assert.That(rally.Match.Server,Is.EqualTo(PlayerId.Rival));Assert.That(rally.Phase,Is.EqualTo(RallyPhase.HumanReception));
        }
        [Test] public void TheCaptionViewExists_AndIsHiddenBeforeAnyReception()
        {
            Assert.That(caption,Is.Not.Null,"added by the controller");Assert.That(caption.IsShown,Is.False);Assert.That(caption.Text,Is.EqualTo("Espera el bote"));
        }
        [Test] public void ServeReception_IsTrueOnlyForTheRivalsServe()
        {
            Assert.That(rally.StartMode(),Is.True);Assert.That(controller.ServeReception,Is.False,"practice and rally balls are not serves");rally.StopMode();
            StartRivalServe();Assert.That(controller.ServeReception,Is.True);
        }
        [Test] public void Caption_ShowsOnTheWhiteRing_NeverOnTheAmberOne()
        {
            Assert.That(caption,Is.Not.Null);StartRivalServe();bool sawAmber=false,sawWhite=false;
            for(int i=0;i<600&&!(sawAmber&&sawWhite)&&!controller.AttemptConsumed;i++)
            {
                At(app.Host.ObservedRealTime+StepReal);
                if(!ring.Visible){Assert.That(caption.IsShown,Is.False,"no ring, no caption");continue;}
                if(ring.Tint==ShotController.VolleyTint){sawAmber=true;Assert.That(caption.IsShown,Is.False,"the amber ring carries no text");}
                else{sawWhite=true;Assert.That(caption.IsShown,Is.True,"the white ring carries «Espera el bote»");Assert.That(caption.Text,Is.EqualTo("Espera el bote"));}
            }
            Assert.That(sawWhite,Is.True,"the white ring showed at some point of the reception");
        }
        [Test] public void Caption_IsHiddenInAReceptionThatIsNotTheServe()
        {
            Assert.That(caption,Is.Not.Null);Assert.That(rally.StartMode(),Is.True);Assert.That(controller.ServeReception,Is.False);
            for(int i=0;i<300;i++){At(app.Host.ObservedRealTime+StepReal);Assert.That(caption.IsShown,Is.False);}
        }
        [Test] public void Caption_IsHiddenWhenTheAidHidesTheRing()
        {
            Assert.That(caption,Is.Not.Null);app.Tuning.Set("timing.aidLevel",2);StartRivalServe();
            for(int i=0;i<300;i++){At(app.Host.ObservedRealTime+StepReal);Assert.That(caption.IsShown,Is.False,"aid 2: no ring on the court, no caption either");}
        }
    }
}
