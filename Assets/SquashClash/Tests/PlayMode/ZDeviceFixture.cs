using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // A phone screen in pixels, portrait, with the safe-area insets iOS reports for it (status bar hidden).
    public struct PhoneScreen
    {
        public string Name;public int Width,Height,Top,Bottom;
        public PhoneScreen(string name,int width,int height,int top,int bottom){Name=name;Width=width;Height=height;Top=top;Bottom=bottom;}
        public Rect SafePixels=>new Rect(0,Bottom,Width,Height-Top-Bottom);
        public override string ToString()=>Name;
    }
    // Device robustness: boots the real Main scene at the size of a given phone, with its safe area simulated, and hands
    // the loop to the test. Same rules as ZFirstRunFixture, which stays at the iPhone 16 Pro size.
    public abstract class ZDeviceFixture
    {
        // From the smallest screen iOS 15 runs on (16:9, no notch) to the tallest Pro Max.
        public static readonly PhoneScreen[] Phones=
        {
            new PhoneScreen("SE-375x667",750,1334,0,0),
            new PhoneScreen("Mini-375x812",1125,2436,150,102),
            new PhoneScreen("Pro-402x874",1206,2622,186,102),
            new PhoneScreen("ProMax-430x932",1290,2796,177,102)
        };
        protected PhoneScreen phone;
        protected AppBootstrap app;protected ShotController controller;protected RallyDirector rally;protected RallyHud hud;protected FirstRunFlow flow;protected SquashClash.Runtime.Audio.AudioScape scape;protected VisualElement root;
        private UnityEngine.Camera camera;private RenderTexture target,previousCameraTarget,previousPanelTarget;private PanelSettings settings;private float previousAspect,previousVolume;
        protected double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
#if UNITY_EDITOR
        private UnityEditor.EditorWindow gameView;private object sizeGroup;private PropertyInfo selectedSize;private int previousSize,customIndex=-1;
        private void UseNativeGameView()
        {
            var assembly=typeof(UnityEditor.Editor).Assembly;var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
            sizeGroup=sizesType.GetProperty("currentGroup").GetValue(sizes);var groupType=sizeGroup.GetType();
            customIndex=(int)groupType.GetMethod("GetTotalCount").Invoke(sizeGroup,null);
            var kind=assembly.GetType("UnityEditor.GameViewSizeType");
            var size=Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"),Enum.Parse(kind,"FixedResolution"),phone.Width,phone.Height,"Device test");
            groupType.GetMethod("AddCustomSize").Invoke(sizeGroup,new[]{size});
            var viewType=assembly.GetType("UnityEditor.GameView");gameView=UnityEditor.EditorWindow.GetWindow(viewType);
            selectedSize=viewType.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            previousSize=(int)selectedSize.GetValue(gameView);
            selectedSize.SetValue(gameView,(int)groupType.GetMethod("GetTotalCount").Invoke(sizeGroup,null)-1);gameView.Repaint();
        }
#endif
        // openAtBoot: what a normal launch does (the start menu over the paused court).
        protected IEnumerator Boot(PhoneScreen screen,bool openAtBoot)
        {
            phone=screen;ClearPrefs();previousVolume=AudioListener.volume;
            FirstRunFlow.OpenAtBoot=openAtBoot;
            SafeAreaLayout.Simulate(new Vector2(phone.Width,phone.Height),phone.SafePixels);
            yield return PlayModeTestWorld.ClearApp();
#if UNITY_EDITOR
            UseNativeGameView();yield return null;yield return null;yield return null;
#endif
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Device fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            controller=app.GetComponent<ShotController>();rally=app.Rally;hud=app.GetComponent<RallyHud>();flow=app.FirstRun;scape=app.Scape;
            app.Host.enabled=app.enabled=controller.enabled=rally.enabled=hud.enabled=flow.enabled=scape.enabled=false;
            camera=UnityEngine.Camera.main;previousCameraTarget=camera.targetTexture;previousAspect=camera.aspect;
            target=new RenderTexture(phone.Width,phone.Height,24);target.Create();camera.targetTexture=target;camera.aspect=phone.Width/(float)phone.Height;
            var document=app.GetComponent<UIDocument>();settings=document.panelSettings;previousPanelTarget=settings.targetTexture;settings.targetTexture=target;
            root=document.rootVisualElement;yield return null;yield return null;
        }
        protected IEnumerator Shutdown()
        {
            SafeAreaLayout.StopSimulating();
            if(settings!=null)settings.targetTexture=previousPanelTarget;
            if(camera!=null){camera.targetTexture=previousCameraTarget;camera.aspect=previousAspect;}
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}
            yield return PlayModeTestWorld.ClearApp();
            FirstRunFlow.ResetOpenAtBoot();ClearPrefs();AudioListener.volume=previousVolume;SquashClash.Runtime.Audio.AudioScape.SoundOn=true;ClearPrefs();
#if UNITY_EDITOR
            if(gameView!=null&&selectedSize!=null)selectedSize.SetValue(gameView,previousSize);
            if(sizeGroup!=null&&customIndex>=0)sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup,new object[]{customIndex});
#endif
        }
        private static void ClearPrefs(){foreach(var key in new[]{"sc.firstRun.tutorialSeen","sc.firstRun.sound","sc.music","sc.hand"})PlayerPrefs.DeleteKey(key);}
        protected T Find<T>(string name) where T:VisualElement
        {
            var element=root.Q<T>(name);Assert.That(element,Is.Not.Null,"missing element "+name);return element;
        }
        protected VisualElement Find(string name)=>Find<VisualElement>(name);
        protected void Click(string name){var button=Find<Button>(name);using(var click=NavigationSubmitEvent.GetPooled()){click.target=button;button.SendEvent(click);}}
        protected void Refresh(){app.GetComponent<SafeAreaLayout>()?.Apply();hud.Refresh();flow.Refresh();}
        protected bool Drive(Func<bool> until,double maxRealSeconds=30)
        {
            int limit=(int)(maxRealSeconds/StepReal);
            for(int i=0;i<limit;i++){if(until())return true;app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();scape.Tick((float)StepReal);Refresh();}
            return until();
        }
        protected static bool Shown(VisualElement element)
        {
            if(element==null)return false;
            for(var e=element;e!=null;e=e.parent)
                if(e.resolvedStyle.display==DisplayStyle.None||e.resolvedStyle.visibility==Visibility.Hidden||e.resolvedStyle.opacity==0)return false;
            return element.worldBound.width>0&&element.worldBound.height>0;
        }
        // The safe area of the simulated phone, in the panel's own units.
        protected Rect Safe
        {
            get
            {
                var all=root.worldBound;float unit=all.height/phone.Height;
                return Rect.MinMaxRect(all.xMin,all.yMin+phone.Top*unit,all.xMax,all.yMax-phone.Bottom*unit);
            }
        }
        private static bool Inside(Rect inner,Rect outer)=>inner.xMin>=outer.xMin-.5f&&inner.xMax<=outer.xMax+.5f&&inner.yMin>=outer.yMin-.5f&&inner.yMax<=outer.yMax+.5f;
        // Everything a player reads or touches under a layer: inside the safe area, not squashed, not cut, not overlapping.
        protected void AssertFits(string layer,string context)
        {
            var safe=Safe;var buttons=new List<Button>();int seen=0;
            Assert.That(Shown(Find(layer)),Is.True,context+": "+layer+" is shown");
            foreach(var e in Find(layer).Query<VisualElement>().ToList().Where(Shown))
            {
                bool label=e is Label,button=e is Button,card=!string.IsNullOrEmpty(e.name)&&(e.name.EndsWith("-card")||e.name.EndsWith("-art")||e.name=="boot-header");
                if(!label&&!button&&!card)continue;
                seen++;string what=context+": "+layer+"/"+(string.IsNullOrEmpty(e.name)?(e as TextElement)?.text:e.name);
                Assert.That(Inside(e.worldBound,safe),Is.True,what+" "+e.worldBound+" is outside the safe area "+safe);
                if(button){Assert.That(e.worldBound.height,Is.GreaterThanOrEqualTo(43.5f),what+" is finger sized");buttons.Add((Button)e);}
                if(e is TextElement text&&!string.IsNullOrEmpty(text.text))
                {
                    Assert.That(e.worldBound.height,Is.GreaterThanOrEqualTo(e.resolvedStyle.fontSize),what+" is not squashed");
                    if(e.resolvedStyle.whiteSpace==WhiteSpace.NoWrap&&e.resolvedStyle.textOverflow!=TextOverflow.Ellipsis)
                    {
                        var need=text.MeasureTextSize(text.text,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined);
                        Assert.That(need.x,Is.LessThanOrEqualTo(e.contentRect.width+1.5f),what+" is cut");
                    }
                }
            }
            Assert.That(seen,Is.GreaterThan(0),context+": "+layer+" shows something");
            for(int i=0;i<buttons.Count;i++)for(int j=i+1;j<buttons.Count;j++)
            {
                var a=buttons[i].worldBound;var b=buttons[j].worldBound;
                bool apart=a.xMax<=b.xMin+.5f||b.xMax<=a.xMin+.5f||a.yMax<=b.yMin+.5f||b.yMax<=a.yMin+.5f;
                Assert.That(apart,Is.True,context+": "+buttons[i].name+" overlaps "+buttons[j].name);
            }
        }
    }
}
