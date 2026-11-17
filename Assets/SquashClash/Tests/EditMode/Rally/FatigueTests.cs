using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class FatigueTests
    {
        private static Type Feature(string name)
        {var t=typeof(PlayerBody).Assembly.GetType("SquashClash.Core.Rally."+name);Assert.That(t,Is.Not.Null,"F1 requires "+name);return t;}
        private static object Settings(ParamSet p=null)=>Feature("FatigueSettings").GetMethod("From").Invoke(null,new object[]{p??ConfigFixtures.Default()});
        private static object State(double energy=100,ParamSet p=null)=>Activator.CreateInstance(Feature("FatigueState"),Settings(p),energy);
        private static object Call(object o,string method,params object[] args)=>o.GetType().GetMethod(method).Invoke(o,args);
        private static double Energy(object o)=>Convert.ToDouble(o.GetType().GetProperty("Energy").GetValue(o));
        private static double Field(object o,string name)=>Convert.ToDouble(o.GetType().GetField(name).GetValue(o));
        private static object Point(object o)=>o.GetType().GetProperty("Point").GetValue(o);
        [Test] public void DefaultsAndRanges_AreTheElevenApprovedSoftValues()
        {
            string[] keys={"enabled","costPerMeter","strongFrom","strongCost","recoverBetween","recoverPerS","onset","legsMax","reactionMaxMs","armSpeedMax","positionGainMax"};
            double[] values={1,.4,.7,1,.25,.5,70,.10,40,.04,.25},min={0,0,.5,0,0,0,10,0,0,0,0},max={1,2,1,5,1,5,100,.4,200,.2,1};
            var p=ConfigFixtures.Default();Assert.That(p.Ordered.Count(e=>e.Key.StartsWith("fatigue.",StringComparison.Ordinal)),Is.EqualTo(11));
            for(int i=0;i<keys.Length;i++)
            {Assert.That(ParamCatalog.TryGet("fatigue."+keys[i],out var d),Is.True);Assert.That(p.Get(d.Key),Is.EqualTo(values[i]));Assert.That(d.Min,Is.EqualTo(min[i]));Assert.That(d.Max,Is.EqualTo(max[i]));}
        }
        [Test] public void Projection_ExcludesOnlyFatigue_AndRetainsAllFiveHistoricalHashes()
        {
            var t3=ConfigFixtures.Default();var v=t3.With("timing.validLimit",1);
            Assert.That(t3.Ordered.Count,Is.EqualTo(132));
            string Project(ParamSet p)
            {
                var entries=p.Ordered.Where(e=>!e.Key.StartsWith("fatigue.",StringComparison.Ordinal)).ToArray();Assert.That(entries.Length,Is.EqualTo(121));
                string text=string.Concat(entries.Select(e=>e.Key+"="+e.Value.ToString("R",CultureInfo.InvariantCulture)+"\n"));
                using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)),0,6).Replace("-","").ToLowerInvariant();
            }
            var sl5v=v.With("practice.launch",4).With("human.z",(double)1.6f);var sl5t3=t3.With("practice.launch",4).With("human.z",(double)1.6f);var p0=t3.With("launcher.repeatSeconds",3);
            Assert.That(Project(t3),Is.EqualTo("430da2faa818"));Assert.That(Project(v),Is.EqualTo("06fed4bd4ac9"));
            Assert.That(Project(sl5v),Is.EqualTo("8e22575422f2"));Assert.That(Project(sl5t3),Is.EqualTo("e89daab41c3e"));Assert.That(Project(p0),Is.EqualTo("dcc7c8cc0688"));
            Assert.That(EffectiveConfig.From(t3).SimulationKey,Is.EqualTo("166a96e319c7"));
            TestContext.WriteLine("F1_HASH T3="+t3.Hash+" V="+v.Hash+" SL5V="+sl5v.Hash+" SL5T3="+sl5t3.Hash+" P0="+p0.Hash+" keys="+t3.Ordered.Count);
        }
        [Test] public void RunningAndStrongIntentions_AreCharged_AndServeZeroCostsNothing()
        {
            var s=State();Call(s,"BeginPoint");Call(s,"Step",10d,BodyPhase.Recovering,1f,false);
            Assert.That(Energy(s),Is.EqualTo(96).Within(1e-12));Call(s,"Strike",0d);Call(s,"Strike",.7d);Assert.That(Energy(s),Is.EqualTo(96).Within(1e-12));
            Call(s,"Strike",1d);Assert.That(Energy(s),Is.EqualTo(95).Within(1e-12));
            Assert.That(Field(Point(s),"Meters"),Is.EqualTo(10));Assert.That(Field(Point(s),"MovementCost"),Is.EqualTo(4));Assert.That(Field(Point(s),"StrongCost"),Is.EqualTo(1));
        }
        [TestCase(BodyPhase.Waiting,.049f,false,true)] [TestCase(BodyPhase.Set,0f,false,true)]
        [TestCase(BodyPhase.Approaching,0f,false,false)] [TestCase(BodyPhase.Recovering,0f,false,false)]
        [TestCase(BodyPhase.Set,.05f,false,false)] [TestCase(BodyPhase.Waiting,0f,true,false)]
        public void QuietRecovery_RequiresAllThreeConditions(BodyPhase phase,float speed,bool reacting,bool recovers)
        {
            var s=State(50);Call(s,"BeginPoint");Call(s,"Step",0d,phase,speed,reacting);
            Assert.That(Energy(s),Is.EqualTo(recovers?50+.5/120:50).Within(1e-12));
        }
        [Test] public void BetweenPoints_RecoversExactlyOnce_AndNothingRunsOutsidePoint()
        {
            var s=State(40);Call(s,"Step",10d,BodyPhase.Waiting,0f,false);Assert.That(Energy(s),Is.EqualTo(40));
            Call(s,"BeginPoint");Call(s,"ClosePoint");Assert.That(Energy(s),Is.EqualTo(61));Call(s,"ClosePoint");
            Call(s,"Step",10d,BodyPhase.Waiting,0f,false);Call(s,"Strike",1d);Assert.That(Energy(s),Is.EqualTo(61));
            Assert.That(Field(Point(s),"End"),Is.EqualTo(40));Assert.That(Field(Point(s),"AfterRecovery"),Is.EqualTo(61));
        }
        [Test] public void Energy_IsClamped_ButMovementAccountingIsNotTruncated()
        {
            var s=State(1);Call(s,"BeginPoint");Call(s,"Step",1000d,BodyPhase.Approaching,2f,false);
            Assert.That(Energy(s),Is.Zero);Assert.That(Field(Point(s),"MovementCost"),Is.EqualTo(400));
            var full=State(100);Call(full,"BeginPoint");Call(full,"Step",0d,BodyPhase.Set,0f,false);Assert.That(Energy(full),Is.EqualTo(100));
        }
        [Test] public void Disabled_IsIdentity_AndDoesNotCalculateEnergy()
        {
            var s=State(100,ConfigFixtures.Default().With("fatigue.enabled",0));Call(s,"BeginPoint");Call(s,"Step",100d,BodyPhase.Approaching,3f,false);Call(s,"Strike",1d);Call(s,"ClosePoint");
            Assert.That(Energy(s),Is.EqualTo(100));Assert.That(Field(Point(s),"Meters"),Is.Zero);
        }
        [TestCase(100,1f,0)] [TestCase(70,1f,0)] [TestCase(35,.95f,2)] [TestCase(0,.9f,5)]
        public void SharedEffects_AreEqualForBothPlayers_AndUseSimulationSteps(double energy,float movement,int extra)
        {
            var p=Settings();double f=Convert.ToDouble(Call(p,"Factor",energy));
            object a=Feature("FatigueEffects").GetMethod("For").Invoke(null,new[]{(object)f,p});
            object b=Feature("FatigueEffects").GetMethod("For").Invoke(null,new[]{(object)f,p});
            Assert.That(Field(a,"MovementFactor"),Is.EqualTo(movement));Assert.That(Field(a,"ExtraSteps"),Is.EqualTo(extra));Assert.That(a,Is.EqualTo(b));
        }
        [TestCase(0f)] [TestCase(.25f)] [TestCase(.5f)] [TestCase(1f)]
        public void PredictionCopies_InheritFrozenMovementFactor_AtEveryStep(float f)
        {
            var context=RallyFixtures.Context();var body=new PlayerBody(PlayerId.Human,new Vector2(2,4),context.Move);
            var property=typeof(PlayerBody).GetProperty("MovementFactor");Assert.That(property,Is.Not.Null,"prediction must inherit fatigue");
            property.SetValue(body,1f-.1f*f);var target=new Vector2(4,7);int delay=22+(int)Math.Round(40*f/1000*context.StepHz);
            int arrive=body.StepsToReach(target,delay,out var at);var path=body.PositionAfter(target,delay,arrive);
            body.MoveTo(target,delay,BodyPhase.Approaching,BodyPhase.Set);var positions=body.PositionsAfter(arrive+1);
            for(int i=0;i<=arrive;i++){Assert.That(body.Position.Equals(positions[i]),Is.True,"step "+i);if(i<arrive)body.Step();}
            Assert.That(body.Position.Equals(at),Is.True);Assert.That(body.Position.Equals(path),Is.True);Assert.That(body.Phase,Is.EqualTo(BodyPhase.Set));
        }
    }
}
