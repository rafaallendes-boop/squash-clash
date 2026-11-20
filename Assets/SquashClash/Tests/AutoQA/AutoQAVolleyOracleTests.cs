using NUnit.Framework;
using SquashClash.Core.Config;
using UnityEngine;
namespace SquashClash.AutoQA
{
    // volley-k/1: windows in ms from the volley T*; synthetic numbers, independent of the game.
    public sealed class AutoQAVolleyOracleTests
    {
        private ParamSet Defaults=>PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text);
        private const double VKIn=-50,VKOut=60,Bounce=400,BKIn=-90,BKOut=200;
        private Expected E(double delta)=>Oracle.ExpectVolley(new Scenario{kind="swing",releaseOffsetMs=delta,launch=Scenario.VolleyLaunch,useDefaults=true},Defaults,VKIn,VKOut,Bounce,BKIn,BKOut);
        [TestCase(-80,"Failed","Air")][TestCase(0,"Perfect","None")][TestCase(55,"Valid","None")][TestCase(150,"Failed","Late")]
        [TestCase(250,"Failed","Air")][TestCase(400,"Perfect","None")][TestCase(700,"Failed","Late")]
        public void Table(double delta,string result,string fail){var e=E(delta);Assert.That(e.result,Is.EqualTo(result),e.rationale);Assert.That(e.fail,Is.EqualTo(fail));Assert.That(e.oracle,Is.EqualTo("volley-k/1"));}
        [Test] public void SplitIsTheMidpoint(){Assert.That(E(185).rationale,Does.StartWith("volley"));Assert.That(E(186).rationale,Does.StartWith("bounce"));}
        [Test] public void Scheduled_OnlyForPerfectAtOrBeforeTStar(){Assert.That(E(-10).scheduled,Is.True);Assert.That(E(10).scheduled,Is.False);Assert.That(E(390).scheduled,Is.True);}
        [TestCase(0,true)][TestCase(185,false)][TestCase(-45,false)][TestCase(250,true)]
        public void Margin(double delta,bool valid)=>Assert.That(Oracle.MarginValidVolley(new Scenario{kind="swing",releaseOffsetMs=delta,launch=Scenario.VolleyLaunch},Defaults,VKIn,VKOut,Bounce,BKIn,BKOut),Is.EqualTo(valid));
        [Test] public void OnlyLaunchSixIsTheVolleySuite(){Assert.That(new Scenario{launch=6}.Volley,Is.True);Assert.That(new Scenario{launch=4}.Volley,Is.False);Assert.That(new Scenario{launch=4,useDefaults=true}.ExpectedHash,Is.EqualTo("d09119d6321b"));}
    }
}
