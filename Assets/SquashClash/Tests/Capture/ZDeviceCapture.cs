using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Capture
{
    // Device robustness: PNG of every screen a player meets, as prod shows them, at the smallest and the largest phone
    // (iPhone SE 375x667 pt and Pro Max 430x932 pt), with the safe area of each simulated. The Dynamic Island and the
    // home indicator are drawn on top as flat shapes so what they would hide can be judged by eye.
    // Run with scripts/capture.sh <set> <dir> ZDeviceCapture.
    public sealed class ZDeviceCapture
    {
        private struct Phone
        {
            public string Name;public int Width,Height,Top,Bottom,Scale;public bool Island;
            public Phone(string name,int width,int height,int top,int bottom,int scale,bool island){Name=name;Width=width;Height=height;Top=top;Bottom=bottom;Scale=scale;Island=island;}
        }
        private static readonly Phone[] Phones=
        {
            new Phone("se-375x667",750,1334,0,0,1,false),
            new Phone("promax-430x932",1290,2796,177,102,2,true)
        };
        private Phone phone;
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;private RallyHud hud;private FirstRunFlow flow;private VisualElement root;
        private UnityEngine.Camera camera;private RenderTexture target;private PanelSettings settings;
        private string directory;private int index;private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        private readonly List<string> written=new List<string>();
        private IEnumerator Boot()
        {
            if(AppBootstrap.Instance!=null){UnityEngine.Object.Destroy(AppBootstrap.Instance.gameObject);yield return null;}
            FirstRunFlow.OpenAtBoot=true;
            SafeAreaLayout.Simulate(new Vector2(phone.Width,phone.Height),new Rect(0,phone.Bottom,phone.Width,phone.Height-phone.Top-phone.Bottom));
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.DevTools=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Capture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            controller=app.GetComponent<ShotController>();rally=app.Rally;hud=app.GetComponent<RallyHud>();flow=app.FirstRun;
            app.Host.enabled=controller.enabled=rally.enabled=false;
            camera=UnityEngine.Camera.main;target=new RenderTexture(phone.Width,phone.Height,24);target.Create();
            camera.targetTexture=target;camera.aspect=phone.Width/(float)phone.Height;
            var document=app.GetComponent<UIDocument>();settings=document.panelSettings;settings.targetTexture=target;root=document.rootVisualElement;
            yield return null;yield return null;
            DrawChrome();
        }
        // The parts of the phone that cover the screen, in panel units (points x pixel scale x panel units per pixel).
        private void DrawChrome()
        {
            if(!phone.Island)return;
            float unit=root.layout.height/phone.Height*3;
            Shape("capture-island",126*unit,37*unit,11*unit,float.NaN,18.5f*unit);
            Shape("capture-home",139*unit,5*unit,float.NaN,8*unit,2.5f*unit);
        }
        private void Shape(string name,float width,float height,float top,float bottom,float radius)
        {
            var e=new VisualElement{name=name,pickingMode=PickingMode.Ignore};e.style.position=Position.Absolute;
            e.style.width=width;e.style.height=height;e.style.left=(root.layout.width-width)*.5f;
            if(!float.IsNaN(top))e.style.top=top;else e.style.bottom=bottom;
            e.style.backgroundColor=float.IsNaN(top)?new Color(1,1,1):Color.black;
            e.style.borderTopLeftRadius=e.style.borderTopRightRadius=e.style.borderBottomLeftRadius=e.style.borderBottomRightRadius=radius;
            root.Add(e);
        }
        private IEnumerator Shutdown()
        {
            FirstRunFlow.ResetOpenAtBoot();SafeAreaLayout.StopSimulating();
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
        private void Click(string name){var button=root.Q<Button>(name);Assert.That(button,Is.Not.Null,name);using(var click=NavigationSubmitEvent.GetPooled()){click.target=button;button.SendEvent(click);}}
        private IEnumerator Shoot(string name)
        {
            // Developer overlays are not part of what a player sees. The bar is taken out of the tree, as in a prod build
            // (which has none): the status lines that anchor to it then sit where prod puts them, above the bottom edge.
            root.Q("dev-bar")?.RemoveFromHierarchy();
            foreach(var id in new[]{"dev-toggle","dev-panel","shot-overlay"}){var e=root.Q(id);if(e!=null){e.style.display=DisplayStyle.None;e.style.opacity=0;}}
            foreach(var id in new[]{"capture-island","capture-home"})root.Q(id)?.BringToFront();
            yield return null;yield return null;
            int w=phone.Width/phone.Scale,h=phone.Height/phone.Scale;
            var small=RenderTexture.GetTemporary(w,h,0);Graphics.Blit(target,small);
            var previous=RenderTexture.active;RenderTexture.active=small;
            var image=new Texture2D(w,h,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,w,h),0,0);image.Apply();
            RenderTexture.active=previous;RenderTexture.ReleaseTemporary(small);
            string path=Path.Combine(directory,phone.Name+"-"+(++index).ToString("00")+"-"+name+".png");
            File.WriteAllBytes(path,image.EncodeToPNG());UnityEngine.Object.Destroy(image);written.Add(path);
        }
        [UnityTest] public IEnumerator CaptureAll()
        {
            Application.targetFrameRate=60;
            string set=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_SET");if(string.IsNullOrEmpty(set))set="scratch";
            string dir=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_DIR");
            if(string.IsNullOrEmpty(dir))dir=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Logs","capture");
            directory=Path.Combine(dir,set);Directory.CreateDirectory(directory);
            bool seen=PlayerPrefs.GetInt("sc.firstRun.tutorialSeen",0)==1;
            foreach(var each in Phones)
            {
                phone=each;index=0;
                // Menu and tutorial, as the first Jugar opens it.
                PlayerPrefs.DeleteKey("sc.firstRun.tutorialSeen");
                yield return Boot();yield return Shoot("menu");
                Click("menu-play");yield return Shoot("tutorial-1");
                Click("tutorial-next");yield return Shoot("tutorial-2");
                Click("tutorial-next");yield return Shoot("tutorial-3");
                yield return Shutdown();
                // Match header, point banner, pause card and the end of the match.
                PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
                yield return Boot();
                rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
                Click("menu-play");yield return Frames(40);yield return Shoot("match");
                int limit=(int)(300/StepReal);
                for(int i=0;i<limit&&rally.Phase!=RallyPhase.PointOver;i++)Step(1);
                Assert.That(rally.Phase,Is.EqualTo(RallyPhase.PointOver),"a point must end");
                for(int i=0;i<20;i++)yield return null;
                yield return Shoot("banner");
                Click("rally-pause");yield return Shoot("pause");
                Click("pause-continue");yield return null;
                limit=(int)(1500/StepReal);
                for(int i=0;i<limit&&rally.Phase!=RallyPhase.MatchOver;i++)Step(1);
                Assert.That(rally.Phase,Is.EqualTo(RallyPhase.MatchOver),"the demo match must finish");
                for(int i=0;i<24;i++)yield return null;
                yield return Shoot("result");
                yield return Shutdown();
            }
            if(seen)PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);else PlayerPrefs.DeleteKey("sc.firstRun.tutorialSeen");
            Assert.That(written.Count,Is.EqualTo(Phones.Length*8));
            foreach(var path in written)Assert.That(new FileInfo(path).Length,Is.GreaterThan(2000),path);
            Debug.Log("[SquashClash] device capture set="+set+" files="+written.Count+" dir="+directory);
        }
    }
}
