using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Camera;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Capture
{
    // E1 evidence only: actual seeded demo strokes, the unchanged court camera, at phone resolution.
    // Separate capture assembly: never contributes to regression counts or product builds.
    public sealed class ZE1StrokeCapture
    {
        private const int Width=1206,Height=2622,TileWidth=320,TileHeight=450;
        private static readonly string[] Names={"human-forehand","human-backhand","rival-forehand","rival-backhand"};
        private AppBootstrap app;private ShotController controller;private SwingView swing;private StrokeFx fx;private RallyDirector rally;
        private CharacterView human,rival;private UnityEngine.Camera camera;private RenderTexture target;private PanelSettings panel;
        private readonly float[] age={-1,-1,-1,-1};private readonly bool[] done=new bool[4];private readonly Texture2D[] contacts=new Texture2D[4];
        private string directory;private bool wasLeft;private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        private void Begin(int index){if(!done[index]&&age[index]<0)age[index]=0;}
        private IEnumerator Boot()
        {
            wasLeft=Handedness.Left;Handedness.Left=false;FirstRunFlow.OpenAtBoot=false;
            if(AppBootstrap.Instance!=null){UnityEngine.Object.Destroy(AppBootstrap.Instance.gameObject);yield return null;}
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;app.DevTools=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"E1 capture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            controller=app.GetComponent<ShotController>();swing=app.GetComponent<SwingView>();fx=app.GetComponent<StrokeFx>();rally=app.Rally;
            human=app.Players.Human.Find("Figure").GetComponent<CharacterView>();rival=app.Players.Rival.Find("Figure").GetComponent<CharacterView>();
            yield return null;yield return null;
            app.enabled=app.Host.enabled=controller.enabled=swing.enabled=fx.enabled=rally.enabled=human.enabled=rival.enabled=false;
            app.GetComponent<ImpactFx>().enabled=false;
            camera=UnityEngine.Camera.main;camera.GetComponent<CourtCameraRig>().enabled=false;
            target=new RenderTexture(Width,Height,24);target.Create();camera.targetTexture=target;camera.aspect=Width/(float)Height;
            var document=app.GetComponent<UIDocument>();panel=document.panelSettings;panel.targetTexture=target;
            var root=document.rootVisualElement;root.Q("dev-bar")?.RemoveFromHierarchy();
            foreach(var id in new[]{"dev-toggle","dev-panel","shot-overlay"}){var e=root.Q(id);if(e!=null)e.style.display=DisplayStyle.None;}
            controller.PresentationDelivered+=p=>{if(p.HasContact)Begin(swing.ShoulderSign<0?0:1);};
            rally.RivalSwing+=(side,contact)=>{if(contact)Begin(side==StrokeSide.Drive?2:3);};
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Medium;rally.Seed=9;
            Assert.That(rally.StartMode(),Is.True);
            yield return null;yield return null;
        }
        private Texture2D Read(RectInt region)
        {
            var previous=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(region.width,region.height,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(region.x,region.y,region.width,region.height),0,0);texture.Apply();RenderTexture.active=previous;return texture;
        }
        private Texture2D Tile(int index)
        {
            var player=index<2?app.Players.Human:app.Players.Rival;
            var centre=camera.WorldToViewportPoint(new Vector3(player.position.x,.95f,player.position.z));
            return Read(new RectInt(Mathf.Clamp((int)(centre.x*Width)-TileWidth/2,0,Width-TileWidth),
                Mathf.Clamp((int)(centre.y*Height)-TileHeight/2,0,Height-TileHeight),TileWidth,TileHeight));
        }
        private void Save(int index)
        {
            var full=Read(new RectInt(0,0,Width,Height));File.WriteAllBytes(Path.Combine(directory,Names[index]+"-phone.png"),full.EncodeToPNG());UnityEngine.Object.Destroy(full);
            var follow=Tile(index);const int gap=4;
            var strip=new Texture2D(TileWidth*2+gap,TileHeight,TextureFormat.RGB24,false);
            var pixels=new Color32[strip.width*strip.height];for(int i=0;i<pixels.Length;i++)pixels[i]=new Color32(255,255,255,255);strip.SetPixels32(pixels);
            strip.SetPixels(0,0,TileWidth,TileHeight,contacts[index].GetPixels());strip.SetPixels(TileWidth+gap,0,TileWidth,TileHeight,follow.GetPixels());strip.Apply();
            File.WriteAllBytes(Path.Combine(directory,Names[index]+"-contact-follow.png"),strip.EncodeToPNG());
            UnityEngine.Object.Destroy(follow);UnityEngine.Object.Destroy(strip);UnityEngine.Object.Destroy(contacts[index]);contacts[index]=null;
            done[index]=true;age[index]=-1;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(panel!=null)panel.targetTexture=null;if(camera!=null)camera.targetTexture=null;
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}
            for(int i=0;i<contacts.Length;i++)if(contacts[i]!=null)UnityEngine.Object.Destroy(contacts[i]);
            var metrics=app!=null?app.Metrics?.CurrentFilePath:null;
            if(app!=null)UnityEngine.Object.Destroy(app.gameObject);yield return null;
            if(metrics!=null&&File.Exists(metrics))File.Delete(metrics);
            Handedness.Left=wasLeft;FirstRunFlow.ResetOpenAtBoot();
        }
        [UnityTest] public IEnumerator CaptureBothStrokesForBothPlayers()
        {
            string set=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_SET")??"e1";
            string root=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_DIR")??Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Logs","capture");
            directory=Path.Combine(root,set);Directory.CreateDirectory(directory);
            yield return Boot();
            int count=0,limit=(int)(400/StepReal);
            for(int frame=0;frame<limit&&count<4;frame++)
            {
                float dt=(float)StepReal;
                app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();
                swing.Tick(dt);fx.Tick(dt);human.Animate(dt);rival.Animate(dt);
                for(int i=0;i<age.Length;i++)
                {
                    if(age[i]<0)continue;age[i]+=dt;
                    if(contacts[i]==null&&age[i]>=SwingView.ReachSeconds)
                    {
                        app.GetComponent<RallyHud>().Refresh();yield return null;contacts[i]=Tile(i);
                    }
                    if(age[i]>=.26f)
                    {
                        app.GetComponent<RallyHud>().Refresh();yield return null;Save(i);count++;
                    }
                }
                if(frame%120==0)yield return null;
            }
            Assert.That(count,Is.EqualTo(4),"the real seeded demo must show both stroke sides for both players");
            foreach(var name in Names)
            {
                Assert.That(new FileInfo(Path.Combine(directory,name+"-phone.png")).Length,Is.GreaterThan(2000));
                Assert.That(new FileInfo(Path.Combine(directory,name+"-contact-follow.png")).Length,Is.GreaterThan(2000));
            }
            Debug.Log("[SquashClash] E1 capture: four real strokes, contact then follow-through, camera unchanged; "+directory);
        }
    }
}
