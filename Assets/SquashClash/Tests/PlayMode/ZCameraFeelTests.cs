using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Camera;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    // Feel pass, block 0. The camera shake is presentation only: it waits for the clock and it never moves the pose that
    // gameplay reads (the M2-4 reticle snapshots the camera matrices when the finger lands). Named to run last.
    public sealed class ZCameraFeelTests
    {
        private AppBootstrap app;private CourtCameraRig rig;private UnityEngine.Camera view;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Camera feel fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            view=UnityEngine.Camera.main;rig=view.GetComponent<CourtCameraRig>();
            app.Host.enabled=false;if(app.Rally!=null)app.Rally.enabled=false;
            yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return PlayModeTestWorld.ClearApp();}

        [UnityTest] public IEnumerator Shake_WaitsWhileTheClockIsPaused_AndFinishesAfterResume()
        {
            rig.Shake(.028f,.18f);app.Clock.Paused=true;
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(rig.Shaking,Is.True,"a paused game keeps the shake where it was");
            app.Clock.Paused=false;
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(rig.Shaking,Is.False,"the shake ends once the clock runs again");
        }

        [UnityTest] public IEnumerator Shake_IsDrawnButNeverMovesThePoseTheReticleReads()
        {
            // A batch run has no game window: the camera renders only into a target, as in the capture fixture.
            var target=new RenderTexture(96,208,16);target.Create();view.targetTexture=target;yield return null;
            var pose=rig.transform.position;var rotation=rig.transform.rotation;var toWorld=view.cameraToWorldMatrix;var projection=view.projectionMatrix;
            float drawn=0;
            Action<ScriptableRenderContext,UnityEngine.Camera> probe=(_,camera)=>{if(camera==view)drawn=Mathf.Max(drawn,Vector3.Distance(camera.transform.position,pose));};
            RenderPipelineManager.beginCameraRendering+=probe;
            try
            {
                rig.Shake(.028f,.18f);
                float seen=0;bool sameMatrices=true;
                for(int i=0;i<12&&rig.Shaking;i++)
                {
                    yield return null; // resumes in the next frame's update phase, where gestures are read
                    seen=Mathf.Max(seen,Vector3.Distance(rig.transform.position,pose));
                    sameMatrices&=view.cameraToWorldMatrix==toWorld&&view.projectionMatrix==projection&&rig.transform.rotation==rotation;
                }
                Assert.That(seen,Is.Zero,"outside rendering the camera stays on its placed pose");
                Assert.That(sameMatrices,Is.True,"the matrices a touch is projected with are the placed ones");
                Assert.That(drawn,Is.InRange(1e-4f,.028f*1.5f),"the shake still shows in the rendered frames");
            }
            finally{RenderPipelineManager.beginCameraRendering-=probe;view.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);}
        }
    }
}
