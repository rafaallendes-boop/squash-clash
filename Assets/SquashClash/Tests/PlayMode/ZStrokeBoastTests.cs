using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // First-match hints (spec front 2, section 1.6): ShotController.LastStrokeBoast describes the stroke that is leaving through ShotResolved.
    // True only for an applied two-wall shot whose AUTHORITATIVE solve used the boast solver and did not fall back to the front wall.
    // Named Z* to run after the timing fixtures.
    public sealed class ZStrokeBoastTests
    {
        private AppBootstrap app;private ShotController controller;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Stroke boast fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("fatigue.enabled",0);app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();app.LaunchCurrent();
            app.Host.enabled=app.enabled=controller.enabled=app.Rally.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){SquashClash.Runtime.Visual.PointEndFreeze.Reset();yield return PlayModeTestWorld.ClearApp();}
        private struct Outcome {public bool Resolved,InCallback,BeforeContact,Solved,FellBack;public string Result;}
        // One reception: the order is released at the planned contact and the host runs until ShotResolved.
        private Outcome Attempt(Func<Vector3,Vector2,SideAim?> side,double releaseOffset=0)
        {
            app.LaunchCurrent();
            Assert.That(controller.Plan.Exists,Is.True);Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var star),Is.True);
            for(int i=0;i<6000&&app.Host.ObservedRealTime+StepReal<star-1e-10;i++)app.Host.Advance(StepReal);
            double left=star-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);
            var p=controller.Plan.PStar;var o=new Outcome();var aim=side(p,new Vector2(p.x,p.z));
            double at=star+releaseOffset;
            var gesture=aim.HasValue?new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,at,true,GestureOutcome.Swing,aim.Value.UV,.8f,app.Host.ActiveConfig.Source,"fixture",at,side:aim.Value)
                :new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,at,true,GestureOutcome.Swing,new Vector2(3.2f,2.2f),.8f,app.Host.ActiveConfig.Source,"fixture",at);
            Action<ShotRecord> h=r=>{o.Resolved=true;o.InCallback=controller.LastStrokeBoast;o.Result=r.Result;};
            controller.ShotResolved+=h;
            controller.Enqueue(gesture);controller.ProcessPending();
            // An order whose contact is still ahead is solved by preview first: that solve claims nothing.
            o.BeforeContact=controller.WaitingForContact&&controller.LastStrokeBoast;
            for(int i=0;i<6000&&!o.Resolved;i++){app.Host.Advance(StepReal);controller.ProcessPending();}
            controller.ShotResolved-=h;o.Solved=controller.LastBoast.Solved;o.FellBack=controller.LastCausal.FellBack;return o;
        }
        private SideAim? Boast(Vector3 p,float dz,float y=1.4f){var wall=SideAim.NearWall(p.x,app.Host.ActiveConfig.Court);return new SideAim(wall,Mathf.Max(.5f,p.z+dz),y);}

        [UnityTest] public IEnumerator ASolvedBoastThatIsApplied_IsTrueInsideShotResolved_AndNotBeforeTheContact()
        {
            Outcome good=default;bool found=false;
            foreach(float dz in new[]{-.8f,-.5f,-1.1f,-1.4f,-.3f,-1.7f})
            {
                var o=Attempt((p,_)=>Boast(p,dz));
                Assert.That(o.Resolved,Is.True,"the stroke resolves");
                bool expected=o.Solved&&!o.FellBack&&(o.Result=="Perfect"||o.Result=="Valid");
                Assert.That(o.InCallback,Is.EqualTo(expected),"dz="+dz+" solved="+o.Solved+" fellBack="+o.FellBack+" result="+o.Result);
                Assert.That(o.BeforeContact,Is.False,"nothing is claimed before the authoritative contact (dz="+dz+")");
                if(expected&&!found){found=true;good=o;}
            }
            Assert.That(found,Is.True,"at least one side order must be a solved, applied two-wall shot");
            Assert.That(good.InCallback,Is.True);
            yield return null;
        }
        [UnityTest] public IEnumerator ABoastThatFallsBackToTheFrontWall_IsNotAStrokeBoast()
        {
            var o=Attempt((p,_)=>Boast(p,+.9f));
            Assert.That(o.Resolved,Is.True);Assert.That(o.FellBack,Is.True,"no valid boast behind the contact");Assert.That(o.InCallback,Is.False,"it counts as a front-wall stroke");
            yield return null;
        }
        [UnityTest] public IEnumerator AFrontWallStroke_IsNotAStrokeBoast()
        {
            var o=Attempt((p,_)=>null);
            Assert.That(o.Resolved,Is.True);Assert.That(o.Result,Is.EqualTo("Perfect").Or.EqualTo("Valid"),"the control stroke is applied");Assert.That(o.InCallback,Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator AMissedTwoWallOrder_IsNotAStrokeBoast()
        {
            var o=Attempt((p,_)=>Boast(p,-.8f),-1.0);
            Assert.That(o.Resolved,Is.True);Assert.That(o.Result,Is.Not.EqualTo("Perfect").And.Not.EqualTo("Valid"),"released a second early: not applied");Assert.That(o.InCallback,Is.False);
            yield return null;
        }
        // Round 2 of PR #103: an accepted order whose contact is still AHEAD. The release is a little early (inside the valid window), so the stroke
        // is accepted and scheduled; the preview solve runs now and must claim nothing. The contact then applies it and the mark is true inside ShotResolved.
        private struct Scheduled {public bool Waiting,ResolvedBefore,MarkBefore,Resolved,InCallback,Solved,FellBack;public string Result;}
        private Scheduled AttemptAhead(float dz,double lead)
        {
            app.LaunchCurrent();
            Assert.That(controller.Plan.Exists,Is.True);Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var star),Is.True);
            double at=star-lead;
            for(int i=0;i<6000&&app.Host.ObservedRealTime+StepReal<at-1e-10;i++)app.Host.Advance(StepReal);
            double left=at-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);
            var p=controller.Plan.PStar;var side=Boast(p,dz).Value;var o=new Scheduled();
            Action<ShotRecord> h=r=>{o.Resolved=true;o.InCallback=controller.LastStrokeBoast;o.Result=r.Result;};
            controller.ShotResolved+=h;
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,at,true,GestureOutcome.Swing,side.UV,.8f,app.Host.ActiveConfig.Source,"fixture",at,side:side));
            controller.ProcessPending();
            o.Waiting=controller.WaitingForContact;o.ResolvedBefore=o.Resolved;o.MarkBefore=controller.LastStrokeBoast;
            for(int i=0;i<6000&&!o.Resolved;i++){app.Host.Advance(StepReal);controller.ProcessPending();}
            controller.ShotResolved-=h;o.Solved=controller.LastBoast.Solved;o.FellBack=controller.LastCausal.FellBack;return o;
        }
        [UnityTest] public IEnumerator APreviewSolveBeforeTheContact_ClaimsNothing_AndTheContactClaimsItInsideShotResolved()
        {
            int scheduled=0,positives=0;
            foreach(double lead in new[]{.05,.07,.09})
                foreach(float dz in new[]{-.8f,-.5f,-1.1f,-1.4f,-.3f,-1.7f})
                {
                    var o=AttemptAhead(dz,lead);
                    if(!o.Waiting)continue;
                    scheduled++;
                    Assert.That(o.ResolvedBefore,Is.False,"no ShotResolved callback while the contact is still ahead (lead="+lead+", dz="+dz+")");
                    Assert.That(o.MarkBefore,Is.False,"the preview solve claims nothing (lead="+lead+", dz="+dz+", solved="+o.Solved+")");
                    Assert.That(o.Resolved,Is.True,"the contact arrives and the stroke resolves");
                    bool expected=o.Solved&&!o.FellBack&&(o.Result=="Perfect"||o.Result=="Valid");
                    Assert.That(o.InCallback,Is.EqualTo(expected),"inside ShotResolved (lead="+lead+", dz="+dz+", result="+o.Result+")");
                    if(expected)positives++;
                }
            Assert.That(scheduled,Is.GreaterThan(0),"at least one accepted order had a future contact: the probe is not vacuous");
            Assert.That(positives,Is.GreaterThan(0),"at least one of them was a solved, applied two-wall shot that claimed the mark at the contact");
            yield return null;
        }
        [UnityTest] public IEnumerator TheMarkIsClearedByTheNextLaunch()
        {
            bool seen=false;
            foreach(float dz in new[]{-.8f,-.5f,-1.1f,-1.4f,-.3f,-1.7f})
            {
                var o=Attempt((p,_)=>Boast(p,dz));
                if(o.InCallback){seen=true;Assert.That(controller.LastStrokeBoast,Is.True,"still readable after the callback");app.LaunchCurrent();Assert.That(controller.LastStrokeBoast,Is.False,"cleared on launch");break;}
            }
            Assert.That(seen,Is.True,"a solved two-wall shot was needed to check the clearing");
            yield return null;
        }
    }
}
