using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Rally;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Fatigue test tools (PO, 18 nov): equal-width energy bars, and a DEV-only "test energy" for the next point.
    public sealed class ZFatigueTestToolsTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup(){yield return Boot(false);app.Clock.Paused=false;app.Tuning.Set("fatigue.enabled",1);}
        [UnityTearDown] public IEnumerator Cleanup()=>Shutdown();
        private MethodInfo Hook(){var m=typeof(RallyDirector).GetMethod("SetTestEnergy",BindingFlags.Public|BindingFlags.Instance);Assert.That(m,Is.Not.Null,"RallyDirector.SetTestEnergy(double) must exist");return m;}
        private void SetTest(double e)=>Hook().Invoke(rally,new object[]{e});
        private IEnumerator Layout(){for(int i=0;i<4;i++){Refresh();yield return null;}}
        [UnityTest] public IEnumerator Bars_HaveSameWidth_ForBothNames_AtEqualEnergy()
        {
            rally.StartMode();yield return Layout();
            foreach(var lowered in new[]{false,true})
            {
                if(lowered){for(int i=0;i<40;i++){rally.HumanEnergy.Strike(1);rally.RivalEnergy.Strike(1);}Refresh();for(int i=0;i<240;i++)hud.Tick(1/60f);yield return Layout();}
                var human=Find("rally-energy-human");var rival=Find("rally-energy-rival");
                Assert.That(float.IsNaN(human.resolvedStyle.width),Is.False,"layout resolved");Assert.That(human.resolvedStyle.width,Is.GreaterThan(10));
                Assert.That(rival.resolvedStyle.width,Is.EqualTo(human.resolvedStyle.width).Within(.5f),"TÚ and RIVAL bars have the same track width (lowered="+lowered+")");
                Assert.That(Find("rally-energy-rival-fill").resolvedStyle.width,Is.EqualTo(Find("rally-energy-human-fill").resolvedStyle.width).Within(.5f),"equal energy, equal fill");
            }
        }
        [UnityTest] public IEnumerator SetTestEnergy_NextPointStartsAtThatEnergy_ForBoth_AndBarFollows()
        {
            SetTest(40);rally.AutoRestart=false;rally.StartMatch();yield return Layout();
            Assert.That(rally.HumanEnergy.Energy,Is.EqualTo(40));Assert.That(rally.RivalEnergy.Energy,Is.EqualTo(40));
            for(int i=0;i<240;i++)hud.Tick(1/60f);Assert.That(hud.HumanEnergyShown,Is.EqualTo(.4f).Within(.01f));Assert.That(hud.RivalEnergyShown,Is.EqualTo(.4f).Within(.01f));
            Assert.That(Find("rally-energy-human").ClassListContains("sc-energy--amber"),Is.True,"40 is below onset 70, above 25");
        }
        [UnityTest] public IEnumerator SetTestEnergy_IsOneShot_AndClamped()
        {
            SetTest(10);rally.AutoRestart=false;rally.StartMatch();yield return null;
            Assert.That(rally.HumanEnergy.Energy,Is.EqualTo(10));
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver,90),Is.True);double after=rally.HumanEnergy.Energy;
            Assert.That(after,Is.GreaterThan(10),"recovery between points applies as usual");
            rally.SkipWait();Assert.That(Drive(()=>rally.Phase!=RallyPhase.PointOver,30),Is.True);
            Assert.That(rally.HumanEnergy.Energy,Is.GreaterThanOrEqualTo(after-1),"next point carries the energy normally, not the test value");
            SetTest(250);rally.Rematch();Assert.That(rally.HumanEnergy.Energy,Is.EqualTo(100),"clamped to 0..100");
            SetTest(-5);rally.Rematch();Assert.That(rally.RivalEnergy.Energy,Is.EqualTo(0));
        }
        // Control: without the hook, the starting energy is the normal one.
        [UnityTest] public IEnumerator Control_WithoutHook_StartsAtFull()
        {
            rally.StartMatch();yield return null;Assert.That(rally.HumanEnergy.Energy,Is.EqualTo(100));Assert.That(rally.RivalEnergy.Energy,Is.EqualTo(100));
        }
        [UnityTest] public IEnumerator DevButtons_CallTheHook_AndAreLogged()
        {
            yield return Layout();
            var b40=root.Q<Button>("fatigue-test-40");var b10=root.Q<Button>("fatigue-test-10");var b100=root.Q<Button>("fatigue-test-100");
            Assert.That(b40,Is.Not.Null,"DEV panel has «Energía 40»");Assert.That(b10,Is.Not.Null);Assert.That(b100,Is.Not.Null);
            Click("fatigue-test-40");rally.AutoRestart=false;rally.StartMatch();Assert.That(rally.HumanEnergy.Energy,Is.EqualTo(40));
            rally.StopMode();Click("fatigue-test-10");rally.StartMatch();Assert.That(rally.RivalEnergy.Energy,Is.EqualTo(10));
            var log=System.IO.File.ReadAllLines(app.Metrics.CurrentFilePath);
            Assert.That(log.Any(l=>l.Contains("fatigue-test-40")),Is.True,"logged as dev control");Assert.That(log.Any(l=>l.Contains("fatigue-test-10")),Is.True);
        }
    }
}
