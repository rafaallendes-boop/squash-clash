using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    public sealed class RallyDirectorTests
    {
        private AppBootstrap app;
        private ShotController controller;
        private PracticeMode practice;
        private RallyDirector rally;
        private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return PlayModeTestWorld.ClearApp();yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"Rally fixture",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);
            controller=app.GetComponent<ShotController>();practice=app.GetComponent<PracticeMode>();rally=app.Rally;
            Assert.That(rally,Is.Not.Null,"the app wires the rally director at startup");
            app.LaunchCurrent();
            // Deterministic drive: no component advances by itself; the test steps host, orders and director.
            app.Host.enabled=app.enabled=controller.enabled=rally.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return PlayModeTestWorld.ClearApp();}
        private bool Drive(System.Func<bool> until,double maxRealSeconds=60)
        {
            int limit=(int)(maxRealSeconds/StepReal);
            for(int i=0;i<limit;i++)
            {
                if(until())return true;
                app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();
            }
            return until();
        }
        private Vector2 Drawn(string name){var t=app.Players.transform.Find(name);return new Vector2(t.localPosition.x,t.localPosition.z);}

        [UnityTest] public IEnumerator StartMode_FeedsANeutralBallToTheHuman()
        {
            long before=app.Host.LaunchId;
            Assert.That(rally.StartMode(),Is.True);
            Assert.That(rally.Active,Is.True);Assert.That(rally.Phase,Is.EqualTo(RallyPhase.HumanReception));
            Assert.That(app.Host.LaunchId,Is.EqualTo(before+1));Assert.That(practice.enabled,Is.False,"practice must not relaunch during a rally");
            Assert.That(rally.HumanPlan.Reachable,Is.True,rally.HumanPlan.Reason);Assert.That(controller.Plan.Exists,Is.True);
            var source=app.Host.ActiveConfig.Source;
            Assert.That((float)source.Get("human.x"),Is.EqualTo(rally.HumanPlan.BodyAtContact.x));Assert.That((float)source.Get("human.z"),Is.EqualTo(rally.HumanPlan.BodyAtContact.y));
            Assert.That(app.Host.ActiveConfig.SimulationKey,Is.EqualTo("166a96e319c7"));
            Assert.That(Drawn("Human"),Is.EqualTo(rally.Human.Position),"the capsule shows the body, not the planned stance");
            Assert.That(rally.Human.Position,Is.Not.EqualTo(rally.HumanPlan.Stance));
            Assert.That(rally.Rules.ToHit,Is.EqualTo(PlayerId.Human));Assert.That(rally.Rules.Striker,Is.Null);
            yield return null;
        }
        [UnityTest] public IEnumerator HumanBody_RunsToTheStance_OnTheSimulationClock()
        {
            rally.StartMode();var start=rally.Human.Position;
            Assert.That(Drive(()=>rally.Human.Phase==BodyPhase.Set,5),Is.True);
            Assert.That(rally.Human.Position,Is.EqualTo(rally.HumanPlan.Stance));Assert.That(Drawn("Human"),Is.EqualTo(rally.HumanPlan.Stance));
            Assert.That(app.Host.Current.Step,Is.LessThanOrEqualTo(controller.Plan.TStarStep),"arrives before T*");
            Assert.That(start,Is.Not.EqualTo(rally.HumanPlan.Stance));
            yield return null;
        }
        [UnityTest] public IEnumerator WithoutASwing_TheRivalWinsOnTheSecondBounce_AndTheNextPointStarts()
        {
            rally.StartMode();long launch=app.Host.LaunchId;int ended=0;rally.PointEnded+=_=>ended++;
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,20),Is.True);
            var point=rally.LastPoint.Value;
            Assert.That(point.Winner,Is.EqualTo(PlayerId.Rival));Assert.That(point.Cause,Is.EqualTo(PointEndCause.DoubleBounce));
            Assert.That(point.Reception,Is.EqualTo(ReceptionResult.NoSwing));Assert.That(point.Shots,Is.EqualTo(0));
            Assert.That(rally.RivalPoints,Is.EqualTo(1));Assert.That(rally.HumanPoints,Is.EqualTo(0));Assert.That(ended,Is.EqualTo(1));
            Assert.That(controller.LastRecord.Result,Is.EqualTo("NoOrder"));
            Assert.That(app.Host.LaunchId,Is.EqualTo(launch),"no relaunch before the pause between points");
            Assert.That(Drive(()=>rally.Phase==RallyPhase.HumanReception,rally.Params.RestartSeconds+1),Is.True);
            Assert.That(app.Host.LaunchId,Is.EqualTo(launch+1));Assert.That(rally.PointsPlayed,Is.EqualTo(1));Assert.That(ended,Is.EqualTo(1),"closed once");
            yield return null;
        }
        [UnityTest] public IEnumerator Autopilot_ExchangesStrokes_ThroughTheNormalShotPath()
        {
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=15;rally.Level=AiLevel.Medium;rally.Seed=5;
            var parameters=new RallyParams{RestartSeconds=.2,MaxShots=24};rally.SetParams(parameters);
            int most=0,rivalSwings=0,resolved=0;rally.PointEnded+=p=>most=Mathf.Max(most,p.Shots);rally.RivalSwing+=(_,contact)=>{if(contact)rivalSwings++;};
            controller.ShotResolved+=r=>{if(r.Applied)resolved++;};
            rally.StartMode();long first=app.Host.LaunchId;float travelled=0;var last=rally.Rival.Position;
            Assert.That(Drive(()=>{travelled+=Vector2.Distance(rally.Rival.Position,last);last=rally.Rival.Position;return rally.PointsPlayed>=4;},600),Is.True,"three points must finish: phase="+rally.Phase+" points="+rally.PointsPlayed+" shots="+rally.Rules.Shots+" launch="+(app.Host.LaunchId-first)+" running="+app.Host.IsRunning+" step="+app.Host.Current.Step+" record="+controller.LastRecord?.Result+"/"+controller.LastRecord?.Reason+" consumed="+controller.AttemptConsumed+" waiting="+controller.WaitingForContact+" closed="+rally.Rules.Closed+" cause="+rally.Rules.Cause+" plan="+controller.Plan.Exists+"/"+controller.Plan.TStarStep);
            Assert.That(most,Is.GreaterThanOrEqualTo(3),"at least one real exchange");
            Assert.That(resolved,Is.GreaterThanOrEqualTo(2),"human strokes went through ShotController");Assert.That(rivalSwings,Is.GreaterThanOrEqualTo(1));
            // Every rival stroke and every new point is one host launch: the host stays the only ball authority.
            Assert.That(app.Host.LaunchId-first,Is.GreaterThanOrEqualTo(rivalSwings+2));
            Assert.That(rally.SystemFaults,Is.EqualTo(0));Assert.That(travelled,Is.GreaterThan(2f),"the rival has a real moving body");
            Assert.That(rally.HumanPoints+rally.RivalPoints,Is.LessThanOrEqualTo(4));
            Assert.That(Drawn("Rival"),Is.EqualTo(rally.Rival.Position));
            Assert.That(app.Host.ActiveConfig.SimulationKey,Is.EqualTo("166a96e319c7"));
            yield return null;
        }
        [UnityTest] public IEnumerator StrokeLimit_CutsAnEndlessRally_WithNoWinner()
        {
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Hard;rally.Seed=5;rally.AutoRestart=false;
            rally.SetParams(new RallyParams{MaxShots=6});rally.StartMode();
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,120),Is.True);
            var point=rally.LastPoint.Value;
            if(point.Shots>=6){Assert.That(point.Winner,Is.Null);Assert.That(rally.HumanPoints+rally.RivalPoints,Is.EqualTo(0));}
            else Assert.That(point.Winner,Is.Not.Null,"a shorter rally ended on a real terminal event");
            Assert.That(point.Shots,Is.LessThanOrEqualTo(6));
            long launch=app.Host.LaunchId;Drive(()=>false,3);
            Assert.That(app.Host.LaunchId,Is.EqualTo(launch),"AutoRestart off: the next point waits for the player");
            yield return null;
        }
        [UnityTest] public IEnumerator AForeignLaunch_HandsTheCourtBackToPractice()
        {
            rally.StartMode();Assert.That(Drive(()=>app.Host.Current.Step>20,2),Is.True);
            app.LaunchCurrent();
            Assert.That(rally.Active,Is.False);Assert.That(rally.Phase,Is.EqualTo(RallyPhase.Off));Assert.That(practice.enabled,Is.True);
            long launch=app.Host.LaunchId;Drive(()=>false,.5);
            Assert.That(app.Host.LaunchId,Is.EqualTo(launch),"an inactive director never launches");
            yield return null;
        }
        [UnityTest] public IEnumerator StopMode_ReturnsToThePracticeLaunch()
        {
            rally.StartMode();long launch=app.Host.LaunchId;var before=app.Tuning.Applied.Hash;
            rally.StopMode();
            Assert.That(rally.Active,Is.False);Assert.That(practice.enabled,Is.True);Assert.That(app.Host.LaunchId,Is.EqualTo(launch+1));
            Assert.That(app.Tuning.Applied.Hash,Is.EqualTo(before),"rally never edits the applied tuning");
            Assert.That(app.Host.ActiveConfig.Hash,Is.EqualTo("c0d8ec7050e0"),"back to the default practice launch");
            yield return null;
        }
        [UnityTest] public IEnumerator Pause_FreezesBallBodiesAndTheWaitBetweenPoints()
        {
            rally.SetParams(new RallyParams{RestartSeconds=.5});rally.StartMode();
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,20),Is.True);long launch=app.Host.LaunchId;
            app.Clock.Paused=true;var human=rally.Human.Position;
            Drive(()=>false,2);
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.PointOver));Assert.That(app.Host.LaunchId,Is.EqualTo(launch));Assert.That(rally.Human.Position,Is.EqualTo(human));
            app.Clock.Paused=false;
            Assert.That(Drive(()=>rally.Phase==RallyPhase.HumanReception,2),Is.True);
            yield return null;
        }
        [UnityTest] public IEnumerator Rematch_ClearsTheScore_AndStartsAtOnce()
        {
            rally.StartMode();Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,20),Is.True);Assert.That(rally.RivalPoints,Is.EqualTo(1));
            long launch=app.Host.LaunchId;rally.Rematch();
            Assert.That(rally.RivalPoints,Is.EqualTo(0));Assert.That(rally.PointsPlayed,Is.EqualTo(0));Assert.That(rally.Phase,Is.EqualTo(RallyPhase.HumanReception));Assert.That(app.Host.LaunchId,Is.EqualTo(launch+1));
            yield return null;
        }
    }
}
