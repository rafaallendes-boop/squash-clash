using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
namespace SquashClash.Runtime.Tests
{
    public sealed class ZFatigueFlowTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup(){yield return Boot(false);app.Clock.Paused=false;app.Tuning.Set("fatigue.enabled",1);}
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private FatigueState Energy(PlayerId who)
        {var p=typeof(RallyDirector).GetProperty(who==PlayerId.Human?"HumanEnergy":"RivalEnergy");Assert.That(p,Is.Not.Null,"RallyDirector must integrate fatigue");return (FatigueState)p.GetValue(rally);}
        private double Reception(PlayerId who)
        {var p=typeof(RallyDirector).GetProperty(who==PlayerId.Human?"HumanReceptionFatigue":"RivalReceptionFatigue");Assert.That(p,Is.Not.Null);return (double)p.GetValue(rally);}
        [Serializable] private sealed class Line {public string kind,payload;}
        [Serializable] private sealed class FatigueLine {public FatiguePoint human,rival;public int point;}
        private Line[] Lines()=>File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Line>).ToArray();
        [Test] public void PointEnergyAndMetrics_CloseOnce_AndDoNotAdvanceBetweenPoints()
        {
            rally.AutoRestart=false;rally.StartMatch();var human=Energy(PlayerId.Human);var rival=Energy(PlayerId.Rival);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,90),Is.True);
            Assert.That(human.Point.Meters+rival.Point.Meters,Is.GreaterThan(0));
            var lines=Lines().Where(l=>l.kind=="fatigue").Select(l=>JsonUtility.FromJson<FatigueLine>(l.payload)).ToArray();
            Assert.That(lines,Has.Length.EqualTo(1));Assert.That(lines[0].human.AfterRecovery,Is.EqualTo(human.Energy));
            Assert.That(human.Point.AfterRecovery,Is.EqualTo(human.Point.End+.08*(100-human.Point.End)).Within(1e-10));
            double h=human.Energy,r=rival.Energy;for(int i=0;i<200;i++){app.Host.Advance(.01);rally.Tick();}
            Assert.That(human.Energy,Is.EqualTo(h));Assert.That(rival.Energy,Is.EqualTo(r));Assert.That(Lines().Count(l=>l.kind=="fatigue"),Is.EqualTo(1));
            rally.Rematch();Assert.That(Energy(PlayerId.Human).Energy,Is.EqualTo(100));Assert.That(Energy(PlayerId.Rival).Energy,Is.EqualTo(100));
            rally.StopMode();Assert.That(controller.LastRecord,Is.Null);Assert.That(rally.Active,Is.False);
        }
        [Test] public void HumanAndRival_FreezeReceptionEffects_BeforeSharedPlanning()
        {
            app.Tuning.Set("fatigue.costPerMeter",0);app.Tuning.Set("fatigue.recoverPerS",0);
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.AutoRestart=false;rally.StartMode();
            var human=Energy(PlayerId.Human);var rival=Energy(PlayerId.Rival);
            for(int i=0;i<65;i++){human.Strike(1);rival.Strike(1);}
            Assert.That(Reception(PlayerId.Human),Is.Zero,"initial reception stays frozen despite later consumption");
            Assert.That(Drive(()=>rally.Phase==RallyPhase.RivalReception,90),Is.True);
            Assert.That(Reception(PlayerId.Rival),Is.EqualTo(.5));Assert.That(rally.Rival.MovementFactor,Is.EqualTo(.95f));
            double frozen=Reception(PlayerId.Rival);rival.Strike(1);
            typeof(RallyDirector).GetMethod("PlanRival",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(rally,null);
            Assert.That(Reception(PlayerId.Rival),Is.EqualTo(frozen));Assert.That(rally.Rival.MovementFactor,Is.EqualTo(.95f));
            Assert.That(Drive(()=>rally.Phase==RallyPhase.HumanReception||rally.Phase==RallyPhase.PointOver,90),Is.True);
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.HumanReception));Assert.That(Reception(PlayerId.Human),Is.EqualTo(.5));Assert.That(rally.Human.MovementFactor,Is.EqualTo(.95f));
            Assert.That(typeof(ShotRecord).GetField("f_r"),Is.Not.Null,"shot metrics retain the frozen receiver factor");
            Assert.That(Drive(()=>controller.LastRecord!=null,90),Is.True);
            Assert.That((double)typeof(ShotRecord).GetField("f_r").GetValue(controller.LastRecord),Is.EqualTo(.5));
            Assert.That(Lines().Where(l=>l.kind=="shot").All(l=>l.payload.Contains("\"f_r\":")),Is.True,"rally shot events carry f_r");
            Assert.That(Lines().Any(l=>l.kind=="shot"),Is.True);
            rally.StopMode();controller.CloseReception();
            if(controller.LastRecord!=null)Assert.That((double)typeof(ShotRecord).GetField("f_r").GetValue(controller.LastRecord),Is.Zero,"practice does not inherit rally fatigue");
        }
    }
}
