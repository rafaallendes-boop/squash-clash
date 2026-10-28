using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using SquashClash.DevPanel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Phase=UnityEngine.InputSystem.TouchPhase;
namespace SquashClash.Runtime.Tests
{
    public sealed class M24PreviewTests
    {
        private AppBootstrap app;private GesturePreview preview;private TouchInputAdapter input;private Touchscreen screen;
        private Vector2 anchor;private double start,now;private float mm;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"M24 fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("gesture.control",2);app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();app.Host.enabled=false;
            preview=app.GetComponent<GesturePreview>();input=app.GetComponent<TouchInputAdapter>();input.InputAreaOverride=new Rect(0,0,5000,5000);input.UiBlocker=_=>false;
            anchor=UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(3.2f,2.2f,0));start=now=Math.Ceiling(Time.realtimeSinceStartupAsDouble)+1;input.ReadRealTime=()=>now;
            mm=ScreenMetrics.MmPerPx(Screen.dpi,(float)app.Tuning.Applied.Get("input.fallbackDpi"),out _);screen=InputSystem.AddDevice<Touchscreen>();
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(screen!=null)InputSystem.RemoveDevice(screen);yield return PlayModeTestWorld.ClearApp();}
        private void Send(Phase phase,double dt,float down=0,int id=1,double age=0)
        {now=start+dt+age;InputSystem.QueueStateEvent(screen,new TouchState{touchId=id,phase=phase,position=anchor+Vector2.down*(down/mm)},start+dt);InputSystem.Update();}
        [Serializable] private class Record
        {
            public string attemptId,chainId,predecessorCanceledAttemptId,control,outcome,lockOrigin,taskKind,requestedPower;
            public bool correctionCandidate,sameRequestedTarget;
            public int o2Count,projectionFrame;
            public float aimX,aimY,power;
            public double lockObservedAt,beganTimeReal;
            public float[] projection,cameraToWorld;
            public Raw[] rawSamples;
        }
        [Serializable] private class Raw {public double timeReal,receivedReal;public float xPx,yPx;public bool o2;}
        private Record End(double t=.3,float d=18.5f){Send(Phase.Ended,t,d);preview.FlushRecords();return JsonUtility.FromJson<Record>(preview.LastRecordJson);}
        [UnityTest] public IEnumerator CapturedBegan_RecordsProjectionAndO2WithoutRetargetOrBallAuthority()
        {
            var state=app.Host.Current;var pred=app.Host.Prediction;
            Send(Phase.Began,0,age:.02);Assert.That(preview.M24,Is.Not.Null);Assert.That(preview.M24.HasAim,Is.True);var aim=preview.M24.Aim;
            Send(Phase.Moved,.01,5,age:.04);var data=End();
            Assert.That(preview.M24.Aim,Is.EqualTo(aim));Assert.That(data.control,Is.EqualTo("M2-4"));Assert.That(data.lockOrigin,Is.EqualTo("Began"));
            Assert.That(data.beganTimeReal,Is.EqualTo(start));Assert.That(data.lockObservedAt,Is.EqualTo(start+.02));Assert.That(data.o2Count,Is.EqualTo(1));
            Assert.That(data.rawSamples.Length,Is.EqualTo(3));Assert.That(data.rawSamples[1].o2,Is.True);Assert.That(data.projection.Length,Is.EqualTo(16));Assert.That(data.cameraToWorld.Length,Is.EqualTo(16));
            Assert.That(app.Host.Current,Is.EqualTo(state));Assert.That(app.Host.Prediction,Is.SameAs(pred));yield return null;
        }
        [UnityTest] public IEnumerator Reapply_LinksButNeverReplaces_AndContextBreaksChain()
        {
            Send(Phase.Began,0);Send(Phase.Moved,.1,18);var first=End(.3,0);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.CanceledReturnToDeadZone));
            start+=1;Send(Phase.Began,0);var second=End();Assert.That(second.predecessorCanceledAttemptId,Is.EqualTo(first.attemptId));Assert.That(second.chainId,Is.EqualTo(first.chainId));Assert.That(second.attemptId,Is.Not.EqualTo(first.attemptId));
            start+=1;Send(Phase.Began,0);End(.3,0);app.LaunchCurrent();start+=1;Send(Phase.Began,0);var next=End();Assert.That(next.predecessorCanceledAttemptId,Is.Null.Or.Empty);yield return null;
        }
        [UnityTest] public IEnumerator ContextChange_InvalidatesBeforeOldEnded_AndDuplicateFingerCannotReanchor()
        {
            Send(Phase.Began,0);Assert.That(preview.M24,Is.Not.Null);var aim=preview.M24.Aim;Send(Phase.Began,.02,4,id:2);Send(Phase.Ended,.03,35,id:2);
            Assert.That(preview.M24.Aim,Is.EqualTo(aim));Assert.That(preview.Active,Is.True);
            app.Tuning.Set("power.pullFullMm",40);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));Send(Phase.Ended,.3,35);
            Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled));Assert.That(double.IsNaN(preview.M24.ReleaseAt),Is.True);yield return null;
        }
        [UnityTest] public IEnumerator PauseFocusDevAndRelaunchCancelWithoutSyntheticR()
        {
            foreach(string cause in new[]{"pause","focus","dev","launch"})
            {
                Send(Phase.Began,0);Assert.That(preview.M24,Is.Not.Null);
                if(cause=="pause"){app.Clock.Paused=true;preview.Tick(now);app.Clock.Paused=false;}
                if(cause=="focus"){app.gameObject.SendMessage("OnApplicationFocus",false,SendMessageOptions.DontRequireReceiver);app.gameObject.SendMessage("OnApplicationFocus",true,SendMessageOptions.DontRequireReceiver);app.GetComponent<ResumeOverlay>().Continue();}
                if(cause=="dev"){var panel=UnityEngine.Object.FindFirstObjectByType<DevPanelView>();panel.Toggle();panel.Toggle();}
                if(cause=="launch")app.LaunchCurrent();
                Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.SystemCanceled),cause);Assert.That(double.IsNaN(preview.M24.ReleaseAt),Is.True,cause);Send(Phase.Ended,.3);start+=1;
            }
            yield return null;
        }
        [UnityTest] public IEnumerator OutsideFrontWall_IsRecordedAsRejectionWithoutFabricatingAim()
        {
            anchor=UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(-1,2.2f,0));Send(Phase.Began,0);preview.FlushRecords();
            Assert.That(preview.M24,Is.Not.Null);Assert.That(preview.M24.HasAim,Is.False);Assert.That(preview.Outcome,Is.EqualTo(GestureOutcome.Rejected));
            Assert.That(preview.LastRecordJson,Does.Contain("outside-front-wall"));yield return null;
        }
    }
}
