using NUnit.Framework;
using SquashClash.Core.Config;
using UnityEngine;
namespace SquashClash.AutoQA
{
    public sealed class AutoQAOracleTests
    {
        private ParamSet Defaults=>PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text);
        [TestCase(-10,"Perfect",true,true)][TestCase(10,"Perfect",true,false)][TestCase(-70,"Valid",true,false)][TestCase(170,"Failed",false,false)]
        public void TimingTable_ComesFromConfig(double delta,string result,bool applied,bool scheduled)
        {var e=Oracle.Expect(new Scenario{kind="swing",releaseOffsetMs=delta},Defaults);Assert.That(e.result,Is.EqualTo(result));Assert.That(e.applied,Is.EqualTo(applied));Assert.That(e.scheduled,Is.EqualTo(scheduled));if(delta==170)Assert.That(e.fail,Is.EqualTo("Late"));}
        [TestCase(0,-268.143726,6.086025,false)][TestCase(-70,-268.143726,6.086025,true)][TestCase(170,-268.143726,6.086025,true)]
        [TestCase(-10,-239.529058,245.883248,true)][TestCase(10,-239.529058,245.883248,true)][TestCase(-39,-239.529058,245.883248,false)]
        [TestCase(-.5,-239.529058,245.883248,false)]
        public void Margin_AllExternalBoundariesAndInternalOneMs(double delta,double kin,double kout,bool valid)
        =>Assert.That(Oracle.MarginValid(new Scenario{kind="swing",releaseOffsetMs=delta},Defaults,kin,kout),Is.EqualTo(valid));
        [TestCase(true,false,false,false,0,0,300,"Rejected")][TestCase(false,true,false,false,0,0,300,"InvalidAim")]
        [TestCase(false,false,true,false,0,0,300,"SystemCanceled")][TestCase(false,false,false,false,0,0,100,"Tap")]
        [TestCase(false,false,false,false,0,0,300,"AbandonedNoPull")][TestCase(false,false,false,true,0,10,300,"CanceledReturnToDeadZone")]
        [TestCase(false,false,false,true,18.5f,18.5f,300,"Swing")]
        public void ClosureTable(bool rejected,bool invalid,bool canceled,bool pulled,float pull,float max,double duration,string closure)
        =>Assert.That(Oracle.Closure(rejected,invalid,canceled,pulled,pull,max,duration,Defaults),Is.EqualTo(closure));
        [TestCase("nopull","AbandonedNoPull")][TestCase("rejected","Rejected")]
        public void NonSwing_DoesNotConsumeOrPresent(string kind,string closure)
        {var e=Oracle.Expect(new Scenario{kind=kind,pullMm=0},Defaults);Assert.That(e.closure,Is.EqualTo(closure));Assert.That(e.result,Is.EqualTo("NoOrder"));Assert.That(e.presentations,Is.Zero);}
    }
}
