using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
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
        [UnityTest] public IEnumerator Early10_ReachesRealPresentation()=>Run(-10,true);
        [UnityTest] public IEnumerator Late10_ReachesRealPresentation()=>Run(10,false);
        private IEnumerator Run(double offsetMs,bool scheduled)
        {
#if UNITY_EDITOR
            using(var view=new NativeGameView())
            {
                yield return null;yield return null;yield return null;
                Assert.That(Screen.width,Is.EqualTo(1206));Assert.That(Screen.height,Is.EqualTo(2622));
                PocObservation prior=null;
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
                        end+=offsetMs/1000;
                        var mm=ScreenMetrics.MmPerPx(Screen.dpi,(float)app.Tuning.Applied.Get("input.fallbackDpi"),out var dpi);
                        double now=app.Host.ObservedRealTime,start=now,dt=1d/app.Tuning.Applied.Get("physics.stepHz")/app.Tuning.Applied.Get("pace.tempo");
                        input.ReadRealTime=()=>now;screen=InputSystem.AddDevice<Touchscreen>();
                        var times=new[]{end-.3,end-.1,end};
                        var phases=new[]{Phase.Began,Phase.Moved,Phase.Ended};int sent=0;
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
                        string dir=Environment.GetEnvironmentVariable("AUTOQA_POC_DIR");
                        Assert.That(dir,Is.Not.Null.And.Not.Empty,"fresh POC evidence directory required");
                        dir=Path.GetFullPath(dir);Assert.That(dir.StartsWith(Path.GetFullPath("Logs/autoqa")+Path.DirectorySeparatorChar),Is.True);Directory.CreateDirectory(dir);
                        string path=Path.Combine(dir,(scheduled?"early10":"late10")+"-"+repetition+".jsonl");Assert.That(File.Exists(path),Is.False,"never overwrite historical POC");File.Copy(app.Metrics.CurrentFilePath,path);
                        Assert.That(controller.LastRecord,Is.Not.Null,"real shot must be recorded");
                        TestContext.WriteLine("POC observed="+controller.LastRecord.ToJson());
                        Assert.That(controller.LastRecord.Result,Is.EqualTo("Perfect"));Assert.That(controller.LastRecord.Applied,Is.True);
                        Assert.That(controller.PresentationCount,Is.EqualTo(1));
                        Assert.That(File.ReadAllText(path),Does.Contain("shot-presentation"));
                        Assert.That(controller.LastRecord.Scheduled,Is.EqualTo(scheduled));
                        Assert.That(controller.LastRecord.DeltaMs,Is.EqualTo(offsetMs).Within(1e-6));
                        Assert.That(app.Host.ActiveConfig.SimulationKey,Is.EqualTo("166a96e319c7"));
                        Assert.That(app.Host.PredictionMatchesBitwise,Is.True);
                        var front=app.Host.Prediction.Contacts.First();Assert.That(front.Surface,Is.EqualTo(SurfaceId.FrontWall));
                        Assert.That(Vector2.Distance(new Vector2(front.Point.x,front.Point.y),new Vector2(controller.LastRecord.TargetUsedX,controller.LastRecord.TargetUsedY)),Is.LessThanOrEqualTo(.05));
                        var observed=PocObservation.Capture(app,File.ReadAllLines(path),anchor);
                        File.WriteAllText(Path.Combine(dir,(scheduled?"early10":"late10")+"-"+repetition+"-observed.json"),JsonUtility.ToJson(observed,true));
                        if(prior!=null)prior.AssertIdentical(observed);prior=observed;
                    }
                    finally {if(screen!=null)InputSystem.RemoveDevice(screen);UnityEngine.Object.Destroy(app.gameObject);}
                    yield return null;
                }
            }
#endif
        }
    }
}
