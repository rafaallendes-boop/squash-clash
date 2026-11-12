using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Capture
{
    // Editor-only measurement for the art pass: a rally in Demo running freely (no frame cap) with the real components.
    // It reports frame time, render statistics and managed allocation per frame, and the cost of the art-pass components
    // when they exist (looked up by name so the same file also measures a build without them). Not a gate: the iPhone
    // p95 is measured on the device by the PO.
    public sealed class ZVisualPerf
    {
        [UnityTest] public IEnumerator Measure()
        {
            string set=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_SET");if(string.IsNullOrEmpty(set))set="scratch";
            string root=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_DIR");
            if(string.IsNullOrEmpty(root))root=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Logs","capture");
            Directory.CreateDirectory(root);
            if(AppBootstrap.Instance!=null){UnityEngine.Object.Destroy(AppBootstrap.Instance.gameObject);yield return null;}
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            var app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Perf",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            var rally=app.Rally;rally.Autopilot=true;rally.AutopilotTimingSigmaMs=15;rally.Level=AiLevel.Medium;rally.Seed=5;
            Assert.That(rally.StartMode(),Is.True);
            var camera=UnityEngine.Camera.main;var target=new RenderTexture(1206,2622,24);target.Create();
            camera.targetTexture=target;camera.aspect=1206f/2622;
            QualitySettings.vSyncCount=0;
            for(int i=0;i<120;i++)yield return null; // warm-up at the app's 60 fps cap
            Application.targetFrameRate=-1;
            for(int i=0;i<60;i++)yield return null;
            // Twenty seconds of real time, so whole points are played whatever the frame rate.
            const int capacity=400000;const double seconds=20;var ms=new double[capacity];int frames=0;long batches=0,setPass=0,triangles=0,maxBatches=0;
            long bytes=0,maxBytes=0;int gen0=GC.CollectionCount(0);
            var gc=Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Memory,"GC Allocated In Frame");
            double started=Time.realtimeSinceStartupAsDouble;
            while(frames<capacity&&Time.realtimeSinceStartupAsDouble-started<seconds)
            {
                yield return null;ms[frames++]=Time.unscaledDeltaTime*1000.0;
                long frameBytes=gc.Valid?gc.LastValue:-1;bytes+=frameBytes;maxBytes=Math.Max(maxBytes,frameBytes);
#if UNITY_EDITOR
                batches+=UnityEditor.UnityStats.batches;setPass+=UnityEditor.UnityStats.setPassCalls;triangles+=UnityEditor.UnityStats.triangles;
                maxBatches=Math.Max(maxBatches,UnityEditor.UnityStats.batches);
#endif
            }
            gc.Dispose();int collections=GC.CollectionCount(0)-gen0;
            Array.Resize(ref ms,frames);
            Array.Sort(ms);
            int renderers=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length;
            // Art-pass components, if present: direct cost of one pose update per figure.
            double figureMicros=-1;long figureBytes=-1;int figures=0;
            var type=Type.GetType("SquashClash.Runtime.Visual.CharacterView, SquashClash.Runtime");
            if(type!=null)
            {
                var animate=type.GetMethod("Animate");var views=UnityEngine.Object.FindObjectsByType(type,FindObjectsSortMode.None);figures=views.Length;
                var call=new List<Action<float>>();
                foreach(var view in views)call.Add((Action<float>)Delegate.CreateDelegate(typeof(Action<float>),view,animate));
                foreach(var c in call)c(1f/60);
                int gcBefore=GC.CollectionCount(0);long before=GC.GetTotalMemory(false);var watch=Stopwatch.StartNew();
                const int loops=2000;
                for(int i=0;i<loops;i++)foreach(var c in call)c(1f/60);
                watch.Stop();figureBytes=GC.CollectionCount(0)!=gcBefore?-2:Math.Max(0,GC.GetTotalMemory(false)-before)/loops;
                figureMicros=watch.Elapsed.TotalMilliseconds*1000/loops;
            }
            string F(double v)=>v.ToString("0.###",CultureInfo.InvariantCulture);
            string json="{\"set\":\""+set+"\",\"seconds\":"+F(seconds)+",\"frames\":"+frames+",\"frameMsP50\":"+F(ms[frames/2])+",\"frameMsP95\":"+F(ms[(int)(frames*.95)])+",\"frameMsP99\":"+F(ms[(int)(frames*.99)])+
                ",\"batchesAvg\":"+F(batches/(double)frames)+",\"batchesMax\":"+maxBatches+",\"setPassAvg\":"+F(setPass/(double)frames)+",\"trianglesAvg\":"+F(triangles/(double)frames)+
                ",\"renderers\":"+renderers+",\"gcBytesPerFrameAvg\":"+F(bytes/(double)frames)+",\"gcBytesPerFrameMax\":"+maxBytes+",\"gen0Collections\":"+collections+
                ",\"figures\":"+figures+",\"figurePoseMicrosPerFrame\":"+F(figureMicros)+",\"figurePoseBytesPerFrame\":"+figureBytes+
                ",\"pointsPlayed\":"+rally.PointsPlayed+",\"unity\":\""+Application.unityVersion+"\",\"device\":\""+SystemInfo.deviceModel+"\"}";
            File.WriteAllText(Path.Combine(root,"perf-"+set+".json"),json+"\n");
            UnityEngine.Debug.Log("[SquashClash] perf "+json);
            camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);
            string metrics=app.Metrics?.CurrentFilePath;UnityEngine.Object.Destroy(app.gameObject);yield return null;
            if(metrics!=null&&File.Exists(metrics))File.Delete(metrics);
            Application.targetFrameRate=60;
        }
    }
}
