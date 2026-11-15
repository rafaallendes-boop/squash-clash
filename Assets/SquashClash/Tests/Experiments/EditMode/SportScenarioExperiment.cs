using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Experiments.EditMode
{
    // Descriptive annex C experiment. No sport assertion is part of regression or AutoQA.
    public sealed class SportScenarioExperiment
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        internal static readonly string[] FirstBallPrefixes={"court.","camera.","placeholders.","human.","rival.","pace.","physics.","launcher.","debug.","aids."};
        private static readonly float[] AimX = { .02f,.10f,.20f,.35f,.60f,1.20f,1.60f,2f,2.60f,3.20f,3.80f,4.40f,4.80f,5.20f,5.80f,6.05f,6.20f,6.30f,6.38f };

        [Serializable] internal sealed class Hit
        {
            public string surface, verdict;
            public long step;
            public float timeInStep;
            public bool persistent;
            public Vector3 point, normal;
        }
        [Serializable] internal sealed class Contact
        {
            public string id;
            public Vector3 center, incoming = new Vector3(0,-1,3);
            public Vector2 body;
            public int tStarStep = 120;
            public float deltaMs;
        }
        [Serializable] internal sealed class Trial
        {
            public int id, contactIndex, xi, yi, pi;
            public string contact, reason, terminal;
            public Vector3 center, incoming, velocity, first, second;
            public Vector2 aim, target;
            public float power, speed, frontMargin, sideHeight, sideDepth, paceDistance, paceGamma, speedLinear, requested, t1, t2;
            public bool solved, speedRaised, targetLowered, goodFront, legal, firstObserved, secondObserved;
            public bool sideBeforeFirst, backBeforeFirst, oppositeAfterFirst, settled;
            public double sideMin, sideMax;
            public List<Hit> hits = new List<Hit>();
        }
        [Serializable] private sealed class Scenario
        {
            public string scenario, contact, status;
            public int found, sampled, witness = -1;
            public double minimumSideDistance = -1;
        }
        [Serializable] private sealed class Relation
        {
            public string id;
            public int evaluated, increasing, equal, counterexamples, notEvaluated;
            public double fractionIncreasing;
        }
        [Serializable] private sealed class TempoPair
        {
            public string recipe, status, reason;
            public Vector2 body;
            public Vector3 pStar;
            public double kInSteps, kOutSteps, slowSeconds, fastSeconds, slowTStar, fastTStar;
        }
        [Serializable] private sealed class P0Pair
        {
            public int front, rear;
            public string status;
            public float z2Front, z2Rear, t2Front, t2Rear, speedFront, speedRear;
            public bool raisedFront, raisedRear;
        }
        [Serializable] private sealed class Summary
        {
            public string source, defaultsHash, defaultsSha256, simulationKey, p0Hash;
            public bool gitDirty;
            public float bounceThreshold, footprint;
            public int trialCount, solved, legalTwoBounces, unsolved, incomplete;
            public long impactCaps;
            public string random = "off", energy = "fixed", timingAid = "complete", contactMode = "synthetic PStar at TStar, delta=0";
            public string deferred = "R4,C5:E3; X6,X7,R5,R8:out of current scope; R6:random layer; layer4:integrated iPhone playtest";
            public Contact[] contacts;
            public List<Scenario> scenarios = new List<Scenario>();
            public List<Relation> relations = new List<Relation>();
            public List<TempoPair> tempoPairs = new List<TempoPair>();
            public List<P0Pair> r3P0 = new List<P0Pair>();
            public string variant;
            public float paceGammaMax, paceNearM, paceFullM;
        }
        [Serializable] private sealed class Witnesses { public List<Trial> trials = new List<Trial>(); }

        [Test, Explicit, Category("Experiments"), Timeout(1200000)]
        public void WriteAnnexCSportReport()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            string run = Environment.GetEnvironmentVariable("SC_SPORT_RUN");
            if(run != "run01" && run != "run02") throw new InvalidDataException("Set SC_SPORT_RUN=run01 or run02; outputs are never overwritten.");
            string directory = Path.Combine(root,"Logs/experiments/sport",run);
            string presetFile = Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json");
            Harness(directory,PresetCodec.LoadDefault(File.ReadAllText(presetFile)),File.ReadAllBytes(presetFile),"defaults");
        }

        // The Annex C harness for one effective configuration. Speeds go through the shared ShotPace function, then the unchanged solver.
        internal static void Harness(string directory,ParamSet preset,byte[] presetBytes,string variant)
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            if(Directory.Exists(directory)) throw new IOException("Experiment output already exists: "+directory);
            var cfg = EffectiveConfig.From(preset);
            if(preset.Ordered.Count!=121 || cfg.SimulationKey!="166a96e319c7" || preset.HashOf(FirstBallPrefixes)!="0d39e6016bd8" ||
                cfg.Backend!="P2-Analytic" || preset.GetInt("timing.aidLevel")!=0 || cfg.LaunchScale!=1)
                throw new InvalidDataException("Protected base configuration changed.");
            var summary = new Summary {
                source=Git(root,"rev-parse HEAD"), gitDirty=Git(root,"status --porcelain").Length!=0,
                defaultsHash=preset.Hash, defaultsSha256=Sha(presetBytes), simulationKey=cfg.SimulationKey, variant=variant,
                paceGammaMax=(float)preset.Get("shot.paceGammaMax"), paceNearM=(float)preset.Get("shot.paceNearM"), paceFullM=(float)preset.Get("shot.paceFullM"),
                p0Hash=preset.With("launcher.repeatSeconds",3).Hash, bounceThreshold=Physics.bounceThreshold,
                footprint=(float)new RallyParams().FootprintRadius, contacts=Contacts(preset)
            };
            if(summary.gitDirty) throw new InvalidDataException("Run from a clean committed source.");
            var court=cfg.Court;
            var physics=cfg.Physics;
            var factory=new AnalyticBallSimulationFactory(summary.bounceThreshold);
            var heights = new List<float> { court.TinTop+.05f,court.TinTop+.10f,court.TinTop+.15f };
            var dropHeights = new HashSet<float>(heights); // Store float nodes once; avoid comparing stored floats with unrounded expressions.
            for(int i=0;i<=12;i++) heights.Add(Mathf.Lerp(court.TinTop+physics.BallRadius,court.FrontOutLower-physics.BallRadius,i/12f));
            float[] aimY=heights.Distinct().OrderBy(v=>v).ToArray();
            var rows=new List<Trial>();
            var lookup=new Dictionary<(int,int,int,int),Trial>();
            Directory.CreateDirectory(directory);
            using(var trials=new StreamWriter(Path.Combine(directory,"trials.tsv"),false,Utf8))
            using(var pairs=new StreamWriter(Path.Combine(directory,"pairs.tsv"),false,Utf8))
            {
                trials.NewLine=pairs.NewLine="\n";
                trials.WriteLine("id\tcontact\tcx\tcy\tcz\taimX\taimY\tpower\tsolved\tspeedRaised\ttargetLowered\ttargetX\ttargetY\tspeed\tvx\tvy\tvz\tgoodFront\tlegal\tterminal\tfirstObserved\tx1\ty1\tz1\tsecondObserved\tx2\ty2\tz2\tfrontMargin\tsideBeforeFirst\tbackBeforeFirst\toppositeAfterFirst\tsideHeight\tsideDepth\tsideMin\tsideMax\tsettled\treason\tpaceDistance\tpaceGamma\tspeedLinear\trequested\tt1\tt2");
                pairs.WriteLine("relation\ta\tb\tstatus\treason\tvalueA\tvalueB");
                for(int ci=0;ci<summary.contacts.Length;ci++)
                for(int yi=0;yi<aimY.Length;yi++)
                for(int pi=0;pi<=20;pi++)
                for(int xi=0;xi<AimX.Length;xi++)
                {
                    var row=Run(rows.Count,ci,xi,yi,pi,summary.contacts[ci],new Vector2(AimX[xi],aimY[yi]),preset,cfg,factory,summary.footprint);
                    rows.Add(row); lookup.Add((ci,xi,yi,pi),row); WriteTrial(trials,row);
                }
                summary.trialCount=rows.Count; summary.solved=rows.Count(t=>t.solved);
                summary.unsolved=rows.Count-summary.solved;
                summary.legalTwoBounces=rows.Count(t=>t.legal);
                summary.incomplete=rows.Count(t=>t.solved&&!t.secondObserved&&t.terminal=="Horizon");
                for(int ci=0;ci<summary.contacts.Length;ci++)
                {
                    var contact=summary.contacts[ci]; var subset=rows.Where(r=>r.contactIndex==ci).ToArray();
                    if(contact.id.StartsWith("B",StringComparison.Ordinal))
                    {
                        AddScenario(summary,"X1",contact,subset,r=>r.legal&&r.first.z>court.Length-court.ShortLineNearBackEdgeFromBack&&r.sideMax<.5,
                            r=>r.sideMax);
                        AddScenario(summary,"X2",contact,subset,r=>r.legal&&r.oppositeAfterFirst,r=>r.id);
                        AddScenario(summary,"X4",contact,subset,r=>r.goodFront,r=>r.id);
                    }
                    else AddScenario(summary,"X3",contact,subset,r=>r.legal&&r.second.z<court.Length/3,r=>r.second.z);
                    AddScenario(summary,"X5",contact,subset,r=>r.goodFront,r=>r.id);
                }
                var r1=new Relation{id="R1"}; var r2=new Relation{id="R2"}; var r3=new Relation{id="R3"};
                var r3a=new Relation{id="R3a"}; var r3b=new Relation{id="R3b"}; var r3p0=new Relation{id="R3-P0"};
                summary.relations.AddRange(new[]{r1,r2,r3,r3a,r3b,r3p0});
                for(int ci=0;ci<summary.contacts.Length;ci++)
                for(int xi=0;xi<AimX.Length;xi++)
                for(int yi=0;yi<aimY.Length;yi++)
                for(int pi=0;pi<=20;pi++)
                {
                    var a=lookup[(ci,xi,yi,pi)];
                    if(pi<20)
                    {
                        var b=lookup[(ci,xi,yi,pi+1)];
                        string excluded=CommonPair(a,b);
                        if(!Middle(a.aim.y,court)) excluded="aim-outside-middle-band";
                        else if(a.sideBeforeFirst||b.sideBeforeFirst) excluded="side-wall-before-first-bounce";
                        else if(a.backBeforeFirst) excluded="lower-power-back-wall-before-first-bounce";
                        // A higher-power back-wall return is the explicit exception: depth is categorical.
                        bool deeperBack=excluded==null && b.backBeforeFirst;
                        if(excluded==null && (!a.firstObserved||(!b.firstObserved&&!deeperBack))) excluded="first-bounce-not-observed";
                        Record(pairs,r1,a,b,excluded,a.first.z,deeperBack?court.Length:b.first.z,deeperBack);
                    }
                    if(yi+1<aimY.Length)
                    {
                        var b=lookup[(ci,xi,yi+1,pi)]; string excluded=CommonPair(a,b);
                        if(!Middle(a.aim.y,court)||!Middle(b.aim.y,court)) excluded="aim-outside-middle-band";
                        else if(!a.firstObserved||!b.firstObserved) excluded="first-bounce-not-observed";
                        Record(pairs,r2,a,b,excluded,a.first.z,b.first.z);
                    }
                    // F1/B1 and F2/B2; a is the front contact, b the rear.
                    if(ci>=4 && a.pi<=6 && (a.aim.x==a.center.x) &&
                        dropHeights.Contains(a.aim.y))
                    {
                        var b=lookup[(ci-3,xi,yi,pi)];
                        string excluded=null;
                        if(!a.solved||!b.solved) excluded="unsolved";
                        else if(!a.legal||!b.legal) excluded="not-a-legal-two-bounce-return";
                        else if(a.target.x!=b.target.x||a.target.y!=b.target.y) excluded="effective-target-differs";
                        // R3 keeps the original definition over every pair (traceability); R3a is the PO trend over valid pairs with P>0,
                        // the P=0 pairs are reported apart with their behaviour; R3b compares time to the second bounce (physical indicator).
                        Record(pairs,r3,a,b,excluded,a.second.z,b.second.z);
                        Record(pairs,a.pi>0?r3a:r3p0,a,b,excluded,a.second.z,b.second.z);
                        Record(pairs,r3b,a,b,excluded,a.t2,b.t2);
                        if(a.pi==0)summary.r3P0.Add(new P0Pair{front=a.id,rear=b.id,status=excluded??(b.second.z>a.second.z?"Increasing":b.second.z==a.second.z?"Equal":"Counterexample"),
                            z2Front=a.second.z,z2Rear=b.second.z,t2Front=a.t2,t2Rear=b.t2,speedFront=a.speed,speedRear=b.speed,raisedFront=a.speedRaised,raisedRear=b.speedRaised});
                    }
                }
                foreach(var relation in summary.relations)
                    relation.fractionIncreasing=relation.evaluated==0?0:relation.increasing/(double)relation.evaluated;
                RunTempoPairs(summary,cfg,factory);
            }
            summary.impactCaps=factory.ImpactCapReachedCount;
            if(factory.LiveInstances!=0||factory.ImpactCapReachedCount!=0) throw new InvalidDataException("P2 resource leak or impact cap: experiment invalid.");
            var witnesses=new Witnesses();
            foreach(int id in summary.scenarios.Where(s=>s.witness>=0).Select(s=>s.witness).Distinct().OrderBy(i=>i)) witnesses.trials.Add(rows[id]);
            File.WriteAllText(Path.Combine(directory,"summary.json"),JsonUtility.ToJson(summary,true)+"\n",Utf8);
            File.WriteAllText(Path.Combine(directory,"witnesses.json"),JsonUtility.ToJson(witnesses,true)+"\n",Utf8);
            TestContext.WriteLine("SPORT_DATA="+directory);
            // A successful test invocation means complete acquisition, not sport acceptance.
        }

        private static Contact[] Contacts(ParamSet p)
        {
            var result=new List<Contact>();
            float[] xs={.35f,2.6f,3.8f,6.05f};
            for(int i=0;i<4;i++) result.Add(MakeContact("B"+i,xs[i],7.5f,p));
            result.Add(MakeContact("F1",2.6f,1.5f,p)); result.Add(MakeContact("F2",3.8f,1.5f,p));
            return result.ToArray();
        }
        private static Contact MakeContact(string id,float x,float z,ParamSet p)
        {
            var court=EffectiveConfig.From(p).Court;
            float lateral=(float)p.Get("body.idealLateral");
            var body=new Vector2(x+(x<court.Width/2?lateral:-lateral),z);
            var frame=BodyFrame.From(p,body);
            var center=new Vector3(x,(frame.PreferredMinHeight+frame.PreferredMaxHeight)*.5f,z);
            var reference=new Vector3(body.x,frame.ReferenceHeight,body.y);
            if(center.y<p.Get("legal.minHeight")||center.y>p.Get("legal.maxHeight")||Vector3.Distance(center,reference)>frame.Reach||
                Mathf.Min(x,court.Width-x,z,court.Length-z)<frame.WallClearance||Mathf.Min(body.x,court.Width-body.x)<frame.WallClearance)
                throw new InvalidDataException("Synthetic contact violates declared good position: "+id);
            return new Contact{id=id,center=center,body=body};
        }
        internal static Trial Run(int id,int ci,int xi,int yi,int pi,Contact contact,Vector2 aim,ParamSet preset,EffectiveConfig cfg,AnalyticBallSimulationFactory factory,float footprint)
        {
            var court=cfg.Court; var physics=cfg.Physics;
            // The shared pace curve first, then the unchanged solver with equal endpoints (as CausalQuality.Solve does).
            var pace=ShotPace.Requested(pi/20f,contact.center,aim,physics.BallRadius,preset);
            var solution=ShotSolver.Solve(new ShotRequest(contact.center,aim,pi/20f),pace.Speed,pace.Speed,(float)preset.Get("shot.vCap"),
                physics.Gravity,physics.BallRadius,court,factory,physics);
            var row=new Trial { id=id,contactIndex=ci,contact=contact.id,xi=xi,yi=yi,pi=pi,center=contact.center,incoming=contact.incoming,
                aim=aim,power=pi/20f,solved=solution.Solved,speedRaised=solution.SpeedRaised,targetLowered=solution.TargetLowered,
                velocity=solution.Velocity,target=solution.TargetUsed,speed=solution.Speed,reason=solution.Reason,terminal="Unsolved",sideMin=-1,sideMax=-1,
                paceDistance=pace.DistanceM,paceGamma=pace.Gamma,speedLinear=pace.LinearSpeed,requested=pace.Speed };
            if(!solution.Solved) return row;
            var rules=new PointRules(court,footprint); rules.Begin(PlayerId.Human,PlayerId.Rival);
            var judge=new ReturnLineJudge(court,footprint);
            bool ownAtZero=contact.center.x<court.Width*.5f;
            bool stop=false; int floors=0;
            row.terminal="Horizon";
            void SampleSide(Vector3 center)
            {
                double distance=ownAtZero?center.x:court.Width-center.x;
                row.sideMin=row.sideMin<0?distance:Math.Min(row.sideMin,distance);
                row.sideMax=Math.Max(row.sideMax,distance);
            }
            using(var sim=factory.Create(physics,court))
            {
                sim.Reset(new BallState{Step=contact.tStarStep,Position=contact.center,Velocity=solution.Velocity});
                var events=new List<ContactEvent>(4);
                for(int step=0;step<physics.StepHz*10&&!stop;step++)
                {
                    events.Clear(); sim.Step(events);
                    foreach(var hit in events)
                    {
                        var verdict=judge.Judge(hit.Surface,hit.Point);
                        row.hits.Add(new Hit{surface=hit.Surface.ToString(),verdict=verdict.ToString(),step=hit.Step,timeInStep=hit.TimeInStep,
                            persistent=hit.Persistent,point=hit.Point,normal=hit.Normal});
                        if(floors==1) SampleSide(hit.Point+hit.Normal*physics.BallRadius);
                        if(!hit.Persistent)
                        {
                            if(hit.Surface==SurfaceId.FrontWall && !row.goodFront && verdict==LineVerdict.Good && floors==0)
                            { row.goodFront=true; row.frontMargin=hit.Point.y-court.TinTop; }
                            if(hit.Surface==SurfaceId.Floor)
                            {
                                floors++;
                                if(floors==1){row.firstObserved=true;row.first=hit.Point;row.t1=step/(float)physics.StepHz+hit.TimeInStep;SampleSide(hit.Point+hit.Normal*physics.BallRadius);}
                                if(floors==2){row.secondObserved=true;row.second=hit.Point;row.t2=step/(float)physics.StepHz+hit.TimeInStep;}
                            }
                            if(hit.Surface==SurfaceId.LeftWall||hit.Surface==SurfaceId.RightWall)
                            {
                                if(floors==0) row.sideBeforeFirst=true;
                                bool opposite=ownAtZero?hit.Surface==SurfaceId.LeftWall:hit.Surface==SurfaceId.RightWall;
                                if(floors==1&&opposite&&!row.oppositeAfterFirst)
                                {row.oppositeAfterFirst=true;row.sideHeight=hit.Point.y;row.sideDepth=hit.Point.z;}
                            }
                            if(floors==0&&hit.Surface==SurfaceId.BackWall) row.backBeforeFirst=true;
                        }
                        if(hit.Persistent&&hit.Surface==SurfaceId.Floor&&floors==1) row.settled=true;
                        rules.OnBallContact(hit);
                        if(rules.Closed)
                        {
                            row.terminal=rules.Cause.ToString(); row.legal=row.secondObserved&&rules.Cause==PointEndCause.DoubleBounce;
                            stop=true; break;
                        }
                    }
                    if(!stop && sim.State.Position.y>court.WallHeight){rules.OnOverTop();row.terminal=rules.Cause.ToString();stop=true;}
                    if(!stop && floors==1) SampleSide(sim.State.Position);
                }
            }
            return row;
        }
        private static void AddScenario(Summary summary,string id,Contact contact,Trial[] subset,Func<Trial,bool> matches,Func<Trial,double> order)
        {
            var found=subset.Where(matches).OrderBy(order).ThenBy(r=>r.id).ToArray();
            var side=subset.Where(r=>r.legal&&r.first.z>9.75f-4.26f).ToArray();
            summary.scenarios.Add(new Scenario{scenario=id,contact=contact.id,status=found.Length>0?"Exists":"NotFoundInGrid",sampled=subset.Length,
                found=found.Length,witness=found.Length>0?found[0].id:-1,minimumSideDistance=side.Length>0?side.Min(r=>r.sideMin):-1});
        }
        private static bool Middle(float y,CourtDimensions court)
        { return y>=1.6f&&y<2.6f; } // Existing PracticeTask.FrontRow == 1 (T4.2 D2), no new band.
        private static string CommonPair(Trial a,Trial b)
        {
            if(!a.solved||!b.solved) return "unsolved";
            if(a.speedRaised||b.speedRaised||a.targetLowered||b.targetLowered) return "solver-adjustment";
            if(!a.legal||!b.legal) return "not-a-legal-two-bounce-return";
            return null;
        }
        private static void Record(StreamWriter writer,Relation relation,Trial a,Trial b,string excluded,double valueA,double valueB,bool categorical=false)
        {
            string status;
            if(excluded!=null){relation.notEvaluated++;status="NotEvaluated";}
            else
            {
                relation.evaluated++;
                if(categorical||valueB>valueA){relation.increasing++;status="Increasing";}
                else if(valueB==valueA){relation.equal++;status="Equal";}
                else {relation.counterexamples++;status="Counterexample";}
            }
            writer.WriteLine(string.Join("\t",relation.id,a.id,b.id,status,excluded??(categorical?"higher-power-back-wall-exception":"comparable"),F(valueA),F(valueB)));
        }
        private static void RunTempoPairs(Summary summary,EffectiveConfig cfg,AnalyticBallSimulationFactory factory)
        {
            foreach(var recipe in PracticeLaunchDefinitions.Create(cfg))
            {
                Trajectory trajectory;
                using(var sim=factory.Create(cfg.Physics,cfg.Court))
                    trajectory=TrajectoryRunner.Run(sim,recipe.Initial,cfg.Physics.StepHz*10,StopCondition.SecondFloorBounce,cfg.Court);
                var legal=new LegalSettings((float)cfg.Source.Get("legal.minHeight"),(float)cfg.Source.Get("legal.maxHeight"),
                    cfg.Physics.StepHz,cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court);
                var plan=ContactPlanner.Plan(trajectory,recipe.Body,cfg.Court,legal,1);
                var pair=new TempoPair{recipe=recipe.Id,body=recipe.Body.PositionXZ,pStar=plan.PStar,status="NotEvaluated",reason=plan.Reason,
                    kInSteps=plan.KIn.Steps,kOutSteps=plan.KOut.Steps};
                if(plan.Exists&&plan.TStarStep>=0)
                {
                    var slow=new ClockMap();slow.Begin(1,0,0,.5,1,cfg.Physics.StepHz);
                    var fast=new ClockMap();fast.Begin(1,0,0,1,1,cfg.Physics.StepHz);
                    if(slow.TrySimToReal(plan.KIn.Step,plan.KIn.Fraction,out double slowIn)&&slow.TrySimToReal(plan.KOut.Step,plan.KOut.Fraction,out double slowOut)&&
                        fast.TrySimToReal(plan.KIn.Step,plan.KIn.Fraction,out double fastIn)&&fast.TrySimToReal(plan.KOut.Step,plan.KOut.Fraction,out double fastOut)&&
                        slow.TrySimToReal(plan.TStarStep,0,out pair.slowTStar)&&fast.TrySimToReal(plan.TStarStep,0,out pair.fastTStar))
                    {
                        pair.slowSeconds=slowOut-slowIn;pair.fastSeconds=fastOut-fastIn;
                        pair.status=pair.fastSeconds<pair.slowSeconds?"Shorter":"Counterexample";pair.reason="identical-P2-geometry-lambda1-tempo-only";
                    }
                }
                summary.tempoPairs.Add(pair);
            }
        }
        internal static void WriteTrial(StreamWriter w,Trial t)
        {
            w.WriteLine(string.Join("\t",new[]{t.id.ToString(Invariant),t.contact,F(t.center.x),F(t.center.y),F(t.center.z),F(t.aim.x),F(t.aim.y),F(t.power),
                B(t.solved),B(t.speedRaised),B(t.targetLowered),F(t.target.x),F(t.target.y),F(t.speed),F(t.velocity.x),F(t.velocity.y),F(t.velocity.z),B(t.goodFront),B(t.legal),t.terminal,
                B(t.firstObserved),F(t.first.x),F(t.first.y),F(t.first.z),B(t.secondObserved),F(t.second.x),F(t.second.y),F(t.second.z),F(t.frontMargin),
                B(t.sideBeforeFirst),B(t.backBeforeFirst),B(t.oppositeAfterFirst),F(t.sideHeight),F(t.sideDepth),F(t.sideMin),F(t.sideMax),B(t.settled),t.reason,F(t.paceDistance),F(t.paceGamma),F(t.speedLinear),F(t.requested),F(t.t1),F(t.t2)}));
        }
        internal static string F(double value)=>value.ToString("R",Invariant);
        internal static string B(bool value)=>value?"1":"0";
        private static string Sha(byte[] bytes)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        private static string Git(string root,string arguments)
        {
            using(var p=Process.Start(new ProcessStartInfo("git",arguments){WorkingDirectory=root,RedirectStandardOutput=true,UseShellExecute=false,CreateNoWindow=true}))
            {string output=p.StandardOutput.ReadToEnd().Trim();p.WaitForExit();if(p.ExitCode!=0)throw new IOException("Git metadata failed");return output;}
        }
    }
}
