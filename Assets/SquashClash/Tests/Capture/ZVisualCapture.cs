using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Capture
{
    // Visual evidence only: renders the real Main scene (camera + UI Toolkit) to PNG at the iPhone portrait size.
    // Lives in its own assembly so neither scripts/test.sh nor AutoQA runs it; use scripts/capture.sh <set>.
    // The rally is driven step by step (two physics steps per rendered frame) with a fixed seed, so two runs of
    // different builds show the same simulated instants.
    public sealed class ZVisualCapture
    {
        private const int Width=1206,Height=2622,Scale=2;
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;
        private UnityEngine.Camera camera;private RenderTexture target;private PanelSettings settings;
        private string directory;private int index;private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        private readonly List<string> written=new List<string>();
        private IEnumerator Boot()
        {
            if(AppBootstrap.Instance!=null){UnityEngine.Object.Destroy(AppBootstrap.Instance.gameObject);yield return null;}
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Capture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            controller=app.GetComponent<ShotController>();rally=app.Rally;
            app.Host.enabled=controller.enabled=rally.enabled=false;
            camera=UnityEngine.Camera.main;target=new RenderTexture(Width,Height,24);target.Create();
            camera.targetTexture=target;camera.aspect=Width/(float)Height;
            settings=app.GetComponent<UIDocument>().panelSettings;settings.targetTexture=target;
            yield return null;yield return null;
        }
        private IEnumerator Shutdown()
        {
            if(settings!=null)settings.targetTexture=null;
            if(camera!=null)camera.targetTexture=null;
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);target=null;}
            string metrics=app!=null?app.Metrics?.CurrentFilePath:null;
            if(app!=null)UnityEngine.Object.Destroy(app.gameObject);
            yield return null;
            if(metrics!=null&&File.Exists(metrics))File.Delete(metrics);
        }
        private void Step(int steps){for(int i=0;i<steps;i++){app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();}}
        private IEnumerator Frames(int frames){for(int i=0;i<frames;i++){Step(2);yield return null;}}
        private IEnumerator Shoot(string name)
        {
            yield return null; // one more render with the final state
            var small=RenderTexture.GetTemporary(Width/Scale,Height/Scale,0);Graphics.Blit(target,small);
            var previous=RenderTexture.active;RenderTexture.active=small;
            var image=new Texture2D(Width/Scale,Height/Scale,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,Width/Scale,Height/Scale),0,0);image.Apply();
            RenderTexture.active=previous;RenderTexture.ReleaseTemporary(small);
            string path=Path.Combine(directory,(++index).ToString("00")+"-"+name+".png");
            File.WriteAllBytes(path,image.EncodeToPNG());UnityEngine.Object.Destroy(image);written.Add(path);
        }
        [UnityTest] public IEnumerator CaptureAll()
        {
            Application.targetFrameRate=60;
            string set=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_SET");if(string.IsNullOrEmpty(set))set="scratch";
            string root=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_DIR");
            if(string.IsNullOrEmpty(root))root=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Logs","capture");
            directory=Path.Combine(root,set);Directory.CreateDirectory(directory);index=0;

            // 1. Practice: the launcher ball in flight toward the human.
            yield return Boot();
            yield return Frames(40);yield return Shoot("practice-flight");
            yield return Frames(40);yield return Shoot("practice-arrival");
            yield return Shutdown();

            // 2. Rally in Demo: fixed seed, frames at regular instants plus the moments around each stroke and the point end.
            yield return Boot();
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=15;rally.Level=AiLevel.Medium;rally.Seed=5;
            int pendingRival=-1,pendingHuman=-1,pendingPoint=-1,rivalShots=0,humanShots=0,points=0;
            rally.RivalSwing+=(_,contact)=>{if(contact&&rivalShots<2&&pendingRival<0)pendingRival=4;};
            controller.SwingAccepted+=_=>{if(humanShots<2&&pendingHuman<0)pendingHuman=8;};
            rally.PointEnded+=_=>{if(points<1&&pendingPoint<0)pendingPoint=30;};
            Assert.That(rally.StartMode(),Is.True,"rally mode must start for the capture");
            int regular=0;
            for(int frame=1;frame<=60*40&&(points<1||regular<8);frame++)
            {
                Step(2);yield return null;
                if(pendingRival>=0&&pendingRival--==0){rivalShots++;yield return Shoot("rally-rival-stroke-"+rivalShots);}
                if(pendingHuman>=0&&pendingHuman--==0){humanShots++;yield return Shoot("rally-human-stroke-"+humanShots);}
                if(pendingPoint>=0&&pendingPoint--==0){points++;yield return Shoot("rally-point-over");}
                if(frame%45==0&&regular<8){regular++;yield return Shoot("rally-t"+(frame/60f).ToString("0.00",System.Globalization.CultureInfo.InvariantCulture));}
            }
            yield return Shutdown();

            // 3. Match: the serve, then fast-forward to the result.
            yield return Boot();
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Level=AiLevel.Medium;rally.Seed=9;
            Assert.That(rally.StartMatch(),Is.True,"match mode must start for the capture");
            yield return Frames(30);yield return Shoot("match-serve");
            int limit=(int)(1500/StepReal);
            for(int i=0;i<limit&&rally.Phase!=RallyPhase.MatchOver;i++)Step(1);
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.MatchOver),"the demo match must finish");
            yield return null;yield return null;yield return Shoot("match-over");
            yield return Shutdown();

            Assert.That(written.Count,Is.GreaterThanOrEqualTo(8));
            foreach(var path in written)Assert.That(new FileInfo(path).Length,Is.GreaterThan(2000),path);
            Debug.Log("[SquashClash] capture set="+set+" files="+written.Count+" dir="+directory);
        }
    }
}
