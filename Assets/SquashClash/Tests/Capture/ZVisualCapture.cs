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
        // Feel pass: strips. Several frames of one moment side by side, cropped around a world-space box, so short effects
        // (0.1-0.4 s) can be judged in still images.
        private readonly List<Texture2D> strip=new List<Texture2D>();
        // Full-resolution pixel box (the stroke strips are not reduced: the effects are a few pixels wide).
        private RectInt Region(Vector3 centre,float halfWidthMeters,float below,float above)
        {
            var c=camera.WorldToScreenPoint(centre);var side=camera.WorldToScreenPoint(centre+Vector3.right*halfWidthMeters);
            var low=camera.WorldToScreenPoint(centre+Vector3.down*below);var high=camera.WorldToScreenPoint(centre+Vector3.up*above);
            float half=Mathf.Abs(side.x-c.x);
            int x0=Mathf.Clamp((int)(c.x-half),0,Width-2),x1=Mathf.Clamp((int)(c.x+half),x0+2,Width);
            int y0=Mathf.Clamp((int)low.y,0,Height-2),y1=Mathf.Clamp((int)high.y,y0+2,Height);
            return new RectInt(x0,y0,x1-x0,y1-y0);
        }
        // region is in pixels of the target reduced by `scale` (1 = full resolution).
        private void Grab(RectInt region,int scale=Scale)
        {
            var small=RenderTexture.GetTemporary(Width/scale,Height/scale,0);Graphics.Blit(target,small);
            var previous=RenderTexture.active;RenderTexture.active=small;
            var image=new Texture2D(region.width,region.height,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(region.x,region.y,region.width,region.height),0,0);image.Apply();
            RenderTexture.active=previous;RenderTexture.ReleaseTemporary(small);strip.Add(image);
        }
        private void SaveStrip(string name)
        {
            if(strip.Count==0)return;
            const int gap=4;int w=strip[0].width,h=strip[0].height;
            var image=new Texture2D(strip.Count*(w+gap)-gap,h,TextureFormat.RGB24,false);
            var fill=new Color32[image.width*image.height];for(int i=0;i<fill.Length;i++)fill[i]=new Color32(255,255,255,255);image.SetPixels32(fill);
            for(int i=0;i<strip.Count;i++){image.SetPixels(i*(w+gap),0,w,h,strip[i].GetPixels());UnityEngine.Object.Destroy(strip[i]);}
            image.Apply();strip.Clear();
            string path=Path.Combine(directory,(++index).ToString("00")+"-"+name+".png");
            File.WriteAllBytes(path,image.EncodeToPNG());UnityEngine.Object.Destroy(image);written.Add(path);
        }
        // One practice reception struck by a scripted release at T* + deltaMs: seven frames, 50 ms apart, around the stroke.
        private IEnumerator StrokeStrip(string name,double deltaMs,float power)
        {
            yield return Boot();
            var court=app.Config.Court;var aim=new Vector2(court.Width*.5f,1.7f);
            var human=app.Players.Human.position;var region=Region(new Vector3(human.x,1.1f,human.z),1.7f,2.3f,1.5f);
            double release=double.NaN;bool queued=false;int since=0;
            for(int frame=0;frame<60*14&&strip.Count<7;frame++)
            {
                Step(2);
                if(double.IsNaN(release)&&controller.Plan.Exists&&controller.Plan.TStarStep>=0&&app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out double t))
                    release=Math.Max(app.Host.LaunchRealTime,t+deltaMs/1000+app.Host.ActiveConfig.Source.Get("timing.latencyCompMs")/1000);
                if(!double.IsNaN(release)&&!queued&&app.Host.ObservedRealTime>=release)
                {
                    queued=true;
                    controller.Enqueue(new SquashClash.Core.Shot.ClosedGesture(app.Host.LaunchId,"capture-1",0,Math.Max(app.Host.LaunchRealTime,release-.15),release,true,
                        SquashClash.Core.Shot.GestureOutcome.Swing,aim,power,controller.Snapshot.Reception,"capture",app.Host.ObservedRealTime));
                    controller.ProcessPending();
                }
                yield return null;
                if(!double.IsNaN(release)&&app.Host.ObservedRealTime>=release-.10&&since++%3==0)Grab(region,1);
            }
            Assert.That(queued,Is.True,"the scripted release must be queued for "+name);
            Debug.Log("[SquashClash] capture stroke "+name+" result="+(controller.LastRecord!=null?controller.LastRecord.Result:"none")+" power="+power);
            SaveStrip(name);
            yield return Shutdown();
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
            for(int i=0;i<24;i++)yield return null; // let the result card finish its entrance
            yield return Shoot("match-over");
            yield return Shutdown();

            // 4. Feel pass: strips of a Perfect, a Valid and a failed stroke in practice, and of a rally in Demo.
            yield return StrokeStrip("strip-perfect",0,.95f);
            yield return StrokeStrip("strip-valid",-90,.5f);
            yield return StrokeStrip("strip-failed",170,.7f);
            yield return Boot();
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=15;rally.Level=AiLevel.Medium;rally.Seed=5;
            Assert.That(rally.StartMode(),Is.True,"rally mode must start for the strip");
            // Whole court every 0.2 s, then each player's stroke at full resolution (six frames, 50 ms apart, from the event).
            var whole=new RectInt(0,Height/Scale/8,Width/Scale,Height/Scale*3/4);
            var rivalFrames=new List<Texture2D>();var humanFrames=new List<Texture2D>();int rivalLeft=-1,humanLeft=-1;RectInt rivalBox=default,humanBox=default;
            rally.RivalSwing+=(_,contact)=>{if(contact&&rivalLeft<0&&rivalFrames.Count==0){rivalLeft=0;var at=app.Players.Rival.position;rivalBox=Region(new Vector3(at.x,1.1f,at.z),1.9f,2.1f,1.7f);}};
            controller.PresentationDelivered+=p=>{if(p.HasContact&&humanLeft<0&&humanFrames.Count==0){humanLeft=0;var at=app.Players.Human.position;humanBox=Region(new Vector3(at.x,1.1f,at.z),1.9f,2.1f,1.7f);}};
            for(int frame=1;frame<=60*30&&(frame<=12*7||rivalFrames.Count<6||humanFrames.Count<6);frame++)
            {
                Step(2);yield return null;
                if(frame%12==0&&frame<=12*7)Grab(whole);
                if(rivalLeft>=0&&rivalFrames.Count<6&&rivalLeft++%3==0){var keep=new List<Texture2D>(strip);strip.Clear();Grab(rivalBox,1);rivalFrames.Add(strip[0]);strip.Clear();strip.AddRange(keep);}
                if(humanLeft>=0&&humanFrames.Count<6&&humanLeft++%3==0){var keep=new List<Texture2D>(strip);strip.Clear();Grab(humanBox,1);humanFrames.Add(strip[0]);strip.Clear();strip.AddRange(keep);}
            }
            SaveStrip("strip-rally-demo");
            strip.AddRange(rivalFrames);SaveStrip("strip-rival-stroke");strip.AddRange(humanFrames);SaveStrip("strip-human-stroke-demo");
            // The pause between points and what the HUD says about the point.
            for(int frame=0;frame<60*60&&rally.Phase!=RallyPhase.PointOver;frame++){Step(2);if(frame%4==0)yield return null;}
            for(int frame=1;frame<=48;frame++){Step(2);yield return null;if(frame%12==0)Grab(new RectInt(0,Height/Scale/2,Width/Scale,Height/Scale/2));}
            SaveStrip("strip-point-over");
            yield return Shutdown();

            Assert.That(written.Count,Is.GreaterThanOrEqualTo(8));
            foreach(var path in written)Assert.That(new FileInfo(path).Length,Is.GreaterThan(2000),path);
            Debug.Log("[SquashClash] capture set="+set+" files="+written.Count+" dir="+directory);
        }
    }
}
