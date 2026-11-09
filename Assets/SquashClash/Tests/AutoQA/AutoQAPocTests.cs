using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Phase=UnityEngine.InputSystem.TouchPhase;
namespace SquashClash.AutoQA
{
    public sealed class AutoQAPocTests
    {
        [Serializable] private sealed class Line {public string kind,payload;}
        [Serializable] private sealed class PlanLine {public long launchId;public double kInReal,kOutReal,targetReal;}
        [UnityTest] public IEnumerator MainPerfect_ReachesRealPresentation()
        {
#if UNITY_EDITOR
            using(var view=new NativeGameView())
            {
                yield return null;yield return null;yield return null;
                Assert.That(Screen.width,Is.EqualTo(1206));Assert.That(Screen.height,Is.EqualTo(2622));
                string prior=null;
                for(int repetition=1;repetition<=3;repetition++)
                {
                    yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
                    var app=AppBootstrap.Instance;Touchscreen screen=null;
                    try
                    {
                        app.GetComponent<ResumeOverlay>().Continue();
                        app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"AutoQA defaults",app.Build.Label),new List<string>());
                        var expected=app.Tuning.Defaults.With("practice.launch",4).With("human.z",(double)1.6f).Hash;
                        TestContext.WriteLine("EXPECTED SL1="+app.Tuning.Defaults.Hash+" SL5="+expected);
                        app.Tuning.Set("practice.launch",4);app.LaunchCurrent();
                        var controller=app.GetComponent<ShotController>();var practice=app.GetComponent<PracticeMode>();
                        app.Host.enabled=app.enabled=controller.enabled=practice.enabled=false;
                        Assert.That(app.Host.ActiveConfig.Hash,Is.EqualTo(expected));Assert.That(app.Tuning.PendingKeys,Is.Empty);
                        var plan=File.ReadLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Line>).Where(x=>x.kind=="shot-plan").Select(x=>JsonUtility.FromJson<PlanLine>(x.payload)).Last();
                        TestContext.WriteLine(FormattableString.Invariant($"SL5 kInDeltaMs={(plan.kInReal-plan.targetReal)*1000:R} kOutDeltaMs={(plan.kOutReal-plan.targetReal)*1000:R}"));
                        var input=app.GetComponent<TouchInputAdapter>();var preview=app.GetComponent<GesturePreview>();
                        var anchor=(Vector2)UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(3.2f,2.2f,0));
                        Assert.That(Screen.safeArea.Contains(anchor),Is.True,"anchor in actual safe area");
                        Assert.That(input.UiBlocker,Is.Not.Null,"query the real UI blocker");
                        Assert.That(input.UiBlocker(anchor),Is.False,"real UI must not cover anchor");
                        Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var end),Is.True);
                        var mm=ScreenMetrics.MmPerPx(Screen.dpi,(float)app.Tuning.Applied.Get("input.fallbackDpi"),out var dpi);
                        double now=app.Host.ObservedRealTime,start=now,dt=1d/app.Tuning.Applied.Get("physics.stepHz")/app.Tuning.Applied.Get("pace.tempo");
                        input.ReadRealTime=()=>now;screen=InputSystem.AddDevice<Touchscreen>();
                        var times=new[]{end-.3,end-.1,end};var phases=new[]{Phase.Began,Phase.Moved,Phase.Ended};int sent=0;
                        for(int frame=1;frame<1500&&now<end+.15;frame++)
                        {
                            now=start+frame*dt;
                            while(sent<times.Length&&times[sent]<=now)
                            {
                                var point=anchor+Vector2.down*(sent==0?0:18.5f/mm);
                                InputSystem.QueueStateEvent(screen,new TouchState{touchId=101,phase=phases[sent],position=point},times[sent]);sent++;
                            }
                            InputSystem.Update();app.Host.Advance(dt);preview.Tick(now);practice.Tick(dt);controller.ProcessPending();preview.FlushRecords();
                        }
                        string path=Path.GetFullPath("Logs/autoqa/tdd/poc-game-"+repetition+".jsonl");File.Copy(app.Metrics.CurrentFilePath,path,true);
                        Assert.That(controller.LastRecord,Is.Not.Null,"real shot must be recorded");
                        TestContext.WriteLine("POC observed="+controller.LastRecord.ToJson());
                        Assert.That(controller.LastRecord.Result,Is.EqualTo("Perfect"));Assert.That(controller.LastRecord.Applied,Is.True);
                        Assert.That(controller.PresentationCount,Is.EqualTo(1));
                        Assert.That(File.ReadAllText(path),Does.Contain("shot-presentation"));
                        string observed=controller.LastRecord.Result+":"+controller.LastRecord.Applied+":"+controller.LastRecord.ContactStep+":"+controller.PresentationCount;
                        if(prior!=null)Assert.That(observed,Is.EqualTo(prior),"POC repetitions despite wall presentation time");prior=observed;
                    }
                    finally {if(screen!=null)InputSystem.RemoveDevice(screen);UnityEngine.Object.Destroy(app.gameObject);}
                    yield return null;
                }
            }
#endif
        }
    }
}
