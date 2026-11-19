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
    // First run: PNG of every screen a new player meets (menu, tutorial, match header, pause, result), as prod shows
    // them (developer tools off) plus the menu as dev shows it. Same rules as ZVisualCapture; run with
    // scripts/capture.sh <set> <dir> ZFirstRunCapture.
    public sealed class ZFirstRunCapture
    {
        private const int Width=1206,Height=2622,Scale=2;
        private AppBootstrap app;private ShotController controller;private RallyDirector rally;private RallyHud hud;private FirstRunFlow flow;private VisualElement root;
        private UnityEngine.Camera camera;private RenderTexture target;private PanelSettings settings;
        private string directory;private int index;private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        private readonly List<string> written=new List<string>();
        private IEnumerator Boot(bool devTools)
        {
            if(AppBootstrap.Instance!=null){UnityEngine.Object.Destroy(AppBootstrap.Instance.gameObject);yield return null;}
            FirstRunFlow.OpenAtBoot=true;
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.DevTools=devTools;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Capture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            controller=app.GetComponent<ShotController>();rally=app.Rally;hud=app.GetComponent<RallyHud>();flow=app.FirstRun;
            app.Host.enabled=controller.enabled=rally.enabled=false;
            camera=UnityEngine.Camera.main;target=new RenderTexture(Width,Height,24);target.Create();
            camera.targetTexture=target;camera.aspect=Width/(float)Height;
            var document=app.GetComponent<UIDocument>();settings=document.panelSettings;settings.targetTexture=target;root=document.rootVisualElement;
            yield return null;yield return null;
        }
        private IEnumerator Shutdown()
        {
            FirstRunFlow.ResetOpenAtBoot();
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
            // Developer overlays (created a frame after boot by the dev panel) are not part of what a player sees.
            if(!app.DevTools)foreach(var id in new[]{"dev-bar","dev-toggle","dev-panel","shot-overlay"}){var e=root.Q(id);if(e!=null)e.style.display=DisplayStyle.None;}
            yield return null;yield return null;
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
            string dir=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_DIR");
            if(string.IsNullOrEmpty(dir))dir=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Logs","capture");
            directory=Path.Combine(dir,set);Directory.CreateDirectory(directory);index=0;
            bool seen=PlayerPrefs.GetInt("sc.firstRun.tutorialSeen",0)==1;PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);

            // 1. Start menu as a player sees it (prod) and with the developer exit (dev).
            yield return Boot(false);yield return Shoot("menu-prod");
            Click("menu-level-hard");yield return Shoot("menu-level-hard");
            yield return Shutdown();
            yield return Boot(true);yield return Shoot("menu-dev");yield return Shutdown();

            // 1b. The tutorial, as the first Jugar opens it.
            PlayerPrefs.DeleteKey("sc.firstRun.tutorialSeen");
            yield return Boot(false);
            Click("menu-play");yield return Shoot("tutorial-1-aim");
            Click("tutorial-next");yield return Shoot("tutorial-2-pull");
            Click("tutorial-next");yield return Shoot("tutorial-3-timing");
            Click("tutorial-next");yield return Shoot("tutorial-4-volley");
            yield return Shutdown();
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);

            // 2. Jugar: the match header a player sees.
            yield return Boot(false);
            // Energy bars on, so the HUD shows everything a player can meet in a match.
            app.Tuning.Set("fatigue.enabled",1);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=30;rally.Seed=9;
            Click("menu-play");yield return Frames(40);yield return Shoot("match-prod");
            // 2b. Shot feedback: the first stroke's note under the court, then the point banner over the front wall.
            int guard=(int)(60/StepReal);
            for(int i=0;i<guard&&!(controller.LastRecord!=null&&(controller.LastRecord.Result=="Perfect"||controller.LastRecord.Result=="Valid"));i++)Step(1);
            yield return Frames(3);
            // The controller is off in this fixture; it is what applies timing.showNotes every frame in the game.
            controller.GetComponent<ShotNoteView>().SetVisible(app.Tuning.Applied.GetBool("timing.showNotes"));yield return null;
            var note=root.Q<Label>("shot-note");
            Debug.Log("[SquashClash] capture shot-note record="+(controller.LastRecord?.Result??"none")+" text='"+note?.text+"' display="+note?.resolvedStyle.display+" bounds="+note?.worldBound+" root="+root.worldBound);
            yield return Shoot("match-shot-note");
            for(int i=0;i<guard&&rally.Phase!=RallyPhase.PointOver;i++)Step(1);
            for(int i=0;i<16;i++)yield return null;
            yield return Shoot("match-banner");
            // 3. Pause card, then the end of the match.
            Click("rally-pause");yield return Shoot("pause-prod");
            Click("pause-continue");yield return null;
            int limit=(int)(1500/StepReal);
            for(int i=0;i<limit&&rally.Phase!=RallyPhase.MatchOver;i++)Step(1);
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.MatchOver),"the demo match must finish");
            for(int i=0;i<24;i++)yield return null;
            yield return Shoot("match-over-prod");
            yield return Shutdown();

            if(!seen)PlayerPrefs.DeleteKey("sc.firstRun.tutorialSeen");
            Assert.That(written.Count,Is.GreaterThanOrEqualTo(4));
            foreach(var path in written)Assert.That(new FileInfo(path).Length,Is.GreaterThan(2000),path);
            Debug.Log("[SquashClash] first-run capture set="+set+" files="+written.Count+" dir="+directory);
        }
    }
}
