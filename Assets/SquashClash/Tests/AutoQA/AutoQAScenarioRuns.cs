using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Feedback;
namespace SquashClash.AutoQA
{
    public sealed class AutoQAScenarioRuns
    {
        private string dir,started,catalogText,presetsBefore,error;
        private ScenarioCatalog catalog;private bool blocked;
        private readonly List<Attempt> attempts=new List<Attempt>();
        private readonly Integrity integrity=new Integrity();
        private readonly Dictionary<string,PocObservation> prior=new Dictionary<string,PocObservation>();
        private readonly Dictionary<string,bool> deterministic=new Dictionary<string,bool>();
#if UNITY_EDITOR
        private NativeGameView view;
#endif
        [OneTimeSetUp] public void Begin()
        {
            dir=System.Environment.GetEnvironmentVariable("AUTOQA_RUN_DIR");Assert.That(dir,Is.Not.Null.And.Not.Empty);
            dir=Path.GetFullPath(dir);Assert.That(dir.StartsWith(Path.GetFullPath("Logs/autoqa")+Path.DirectorySeparatorChar),Is.True);
            Directory.CreateDirectory(dir);started=DateTime.UtcNow.ToString("O");
            catalogText=File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Tests/AutoQA/Scenarios/mvp.json"));
            catalog=JsonUtility.FromJson<ScenarioCatalog>(catalogText);
            presetsBefore=EvidenceCollector.PresetsSnapshot(Path.Combine(Application.persistentDataPath,"presets"));
            try {new AutoQAGuardTests().AutoQASources_RespectAuthorityContract();}catch(AssertionException){integrity.guardPassed=false;blocked=true;}
#if UNITY_EDITOR
            view=new NativeGameView();
#endif
        }
        [OneTimeTearDown] public void Finish()
        {
#if UNITY_EDITOR
            view?.Dispose();
#endif
            if(dir==null)return;
            string presetsAfter=EvidenceCollector.PresetsSnapshot(Path.Combine(Application.persistentDataPath,"presets"));
            integrity.presetsUnchanged=presetsBefore==presetsAfter;
            File.WriteAllText(Path.Combine(dir,"presets-integrity.json"),"{\"beforeSha256\":"+Report.Q(presetsBefore)+",\"afterSha256\":"+Report.Q(presetsAfter)+"}\n");
            Report.Write(dir,started,attempts,integrity,deterministic,error);
        }
        private IEnumerator Run(int index,int repetition)
        {
            var s=catalog.scenarios[index];
            string filter=System.Environment.GetEnvironmentVariable("AUTOQA_SCENARIO"),rep=System.Environment.GetEnvironmentVariable("AUTOQA_REPETITION");
            if(!s.canary&&(!string.IsNullOrEmpty(filter)&&filter!=s.id||!string.IsNullOrEmpty(rep)&&rep!=repetition.ToString()))yield break;
            var attempt=new Attempt{runId=Path.GetFileName(dir),scenario=new ScenarioRef{id=s.id,repetition=repetition,sha256=EvidenceCollector.ShaText(JsonUtility.ToJson(s)),canary=s.canary},
                build=new BuildMeta{gitHash=System.Environment.GetEnvironmentVariable("AUTOQA_GIT_HASH"),dirty=System.Environment.GetEnvironmentVariable("AUTOQA_DIRTY")=="true",unity=Application.unityVersion},
                repro="scripts/autoqa.sh mvp --scenario "+s.id+" --repetition "+repetition,verdict="NOT_EVALUATED"};
            attempts.Add(attempt);if(blocked){attempt.failure="stopped-after-prior-failure";yield break;}
            yield return null;yield return null;yield return null;
            using(var driver=new DeterministicDriver())
            {
                yield return driver.Prepare(s);
                try
                {
                    var app=driver.App;var cfg=app.Host.ActiveConfig;
                    attempt.config=new ConfigMeta {effectiveHash=cfg.Hash,simulationKey=cfg.SimulationKey,expectedHash=s.ExpectedHash,hashMatch=cfg.Hash==s.ExpectedHash,
                        gestureHash=cfg.Hash,preset=app.ActivePresetName,backend=cfg.Backend,pending=app.Tuning.PendingKeys.Count};
                    integrity.hashesVerified&=attempt.config.hashMatch&&cfg.SimulationKey=="166a96e319c7";
                    attempt.environment=driver.Environment;attempt.expected=Oracle.Expect(s,app.Tuning.Applied);
                    bool ready=attempt.config.hashMatch&&cfg.SimulationKey=="166a96e319c7"&&cfg.Backend=="P2-Analytic"&&app.Tuning.PendingKeys.Count==0&&app.Tuning.Applied.GetInt("gesture.control")==2&&
                        driver.Environment.screen[0]==1206&&driver.Environment.screen[1]==2622&&driver.Environment.anchorInSafeArea&&!driver.Environment.uiBlockedAtAnchor;
                    var plan=File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<MetricLine>).Where(x=>x.kind=="shot-plan").Select(x=>JsonUtility.FromJson<PlanMetric>(x.payload)).Last(x=>x.launchId==app.Host.LaunchId);
                    if(!ready){attempt.verdict="INCONCLUSIVE";attempt.failure="precondition-not-met";}
                    else if(!Oracle.MarginValid(s,app.Tuning.Applied,(plan.kInReal-plan.targetReal)*1000,(plan.kOutReal-plan.targetReal)*1000)){attempt.failure="scenario-invalid-margin";}
                    else
                    {
                        var script=ScriptedPlayer.Build(s,driver.Anchor,driver.Target,driver.Environment.mmPerPx);
                        attempt.input=new InputMeta{intent=s,samples=script.samples};
                        string incoming=JsonUtility.ToJson(new StateList{states=app.Host.Prediction.States.ToArray()});
                        driver.Pump(s,script);
                        attempt.artifacts=EvidenceCollector.Copy(app.Metrics.CurrentFilePath,dir,s.id+"-"+repetition);
                        var lines=File.ReadAllLines(Path.Combine(dir,attempt.artifacts.gameMetrics)).Select(JsonUtility.FromJson<MetricLine>).ToArray();
                        var shot=lines.Where(x=>x.kind=="shot").Select(x=>JsonUtility.FromJson<ShotRecord>(x.payload)).LastOrDefault(x=>x.LaunchId==app.Host.LaunchId);
                        var closure=lines.Where(x=>x.kind=="shot-gesture").Select(x=>JsonUtility.FromJson<ShotRecord>(x.payload)).LastOrDefault(x=>x.LaunchId==app.Host.LaunchId);
                        if(shot==null||closure==null){attempt.verdict="FAIL";attempt.failure="missing-real-shot-or-closure";}
                        else
                        {
                            var presentations=lines.Where(x=>x.kind=="shot-presentation").Select(x=>JsonUtility.FromJson<PresentationMetric>(x.payload)).Where(x=>x.launchId==app.Host.LaunchId).ToArray();
                            var front=app.Host.Prediction.Contacts.First();
                            var o=new Observed{closure=closure.GestureOutcome,result=shot.Result,fail=shot.Fail,applied=shot.Applied,scheduled=shot.Scheduled,
                                attemptConsumed=driver.Controller.AttemptConsumed,contactStep=shot.ContactStep,deltaMs=shot.DeltaMs,quantizationErrorMs=shot.QuantizationErrorMsSigned,
                                presentations=presentations.Length,presentationCategory=presentations.Length==1?presentations[0].category:null,discrepancies=driver.Controller.PresentationDiscrepancyCount,
                                clip=app.GetComponent<ShotFeedback>().LastClip,predictionBitwise=app.Host.PredictionMatchesBitwise,firstSurface=front.Surface.ToString(),firstPoint=front.Point,
                                frontError=Vector2.Distance(new Vector2(front.Point.x,front.Point.y),new Vector2(shot.TargetUsedX,shot.TargetUsedY)),
                                normalized=PocObservation.Capture(app,File.ReadAllLines(Path.Combine(dir,attempt.artifacts.gameMetrics)),driver.Anchor)};
                            attempt.observed=o;attempt.ids=new IdMeta{launchId=shot.LaunchId,attemptId=closure.AttemptId};attempt.metrics.solverCostMs=shot.SolverCostMs;
                            attempt.config.gestureHash=closure.GestureHash;
                            var e=attempt.expected;
                            bool prediction=o.applied||incoming==JsonUtility.ToJson(new StateList{states=app.Host.Prediction.States.ToArray()});
                            attempt.invariants=new[]{new Invariant{id="prediction-bitwise",pass=o.predictionBitwise},new Invariant{id="original-prediction-if-not-applied",pass=prediction},new Invariant{id="single-presentation",pass=o.presentations==e.presentations&&o.discrepancies==0}};
                            bool pass=o.closure==e.closure&&o.result==e.result&&o.applied==e.applied&&o.presentations==e.presentations&&o.discrepancies==0&&o.attemptConsumed==(e.closure=="Swing")&&
                                (e.result!="Failed"||o.fail==e.fail)&&(!e.checkScheduled||o.scheduled==e.scheduled)&&
                                (e.presentations==0||o.presentationCategory==e.result)&&
                                (e.result!="Perfect"||o.firstSurface=="FrontWall"&&o.frontError<=.05f)&&attempt.invariants.All(x=>x.pass);
                            attempt.verdict=pass?"PASS":"FAIL";attempt.failure=pass?null:"oracle-mismatch";
                            if(!s.canary)
                            {
                                bool same=!prior.ContainsKey(s.id)||Determinism.Equivalent(prior[s.id],o.normalized);
                                deterministic[s.id]=same&&(!deterministic.ContainsKey(s.id)||deterministic[s.id]);
                                if(!same){attempt.verdict="FAIL";attempt.failure="determinism-mismatch";}
                                prior[s.id]=o.normalized;
                            }
                        }
                    }
                    if(attempt.artifacts==null)attempt.artifacts=EvidenceCollector.Copy(app.Metrics.CurrentFilePath,dir,s.id+"-"+repetition);
                    if(!s.canary&&attempt.verdict=="FAIL")blocked=true;
                }
                catch(Exception ex){error=ex.Message;attempt.verdict="NOT_EVALUATED";attempt.failure="execution-error: "+ex.Message;blocked=true;}
            }
            yield return null;
        }
        [Serializable] private sealed class StateList {public SquashClash.Core.Simulation.BallState[] states;}
        [UnityTest] public IEnumerator Case01()=>Run(0,1);
        [UnityTest] public IEnumerator Case02()=>Run(0,2);
        [UnityTest] public IEnumerator Case03()=>Run(0,3);
        [UnityTest] public IEnumerator Case11()=>Run(1,1);
        [UnityTest] public IEnumerator Case12()=>Run(1,2);
        [UnityTest] public IEnumerator Case13()=>Run(1,3);
        [UnityTest] public IEnumerator Case21()=>Run(2,1);
        [UnityTest] public IEnumerator Case22()=>Run(2,2);
        [UnityTest] public IEnumerator Case23()=>Run(2,3);
        [UnityTest] public IEnumerator Case31()=>Run(3,1);
        [UnityTest] public IEnumerator Case32()=>Run(3,2);
        [UnityTest] public IEnumerator Case33()=>Run(3,3);
        [UnityTest] public IEnumerator Case41()=>Run(4,1);
        [UnityTest] public IEnumerator Case42()=>Run(4,2);
        [UnityTest] public IEnumerator Case43()=>Run(4,3);
        [UnityTest] public IEnumerator Case51()=>Run(5,1);
        [UnityTest] public IEnumerator Case52()=>Run(5,2);
        [UnityTest] public IEnumerator Case53()=>Run(5,3);
        [UnityTest] public IEnumerator ZCanary()=>Run(6,1);
    }
}
