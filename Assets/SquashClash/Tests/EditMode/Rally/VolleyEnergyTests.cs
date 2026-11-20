using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    // Volley energy (PO, 19 nov: «debería desgastar más la volea ya que vas anticipando»): a contact before the bounce costs the
    // striker RallyParams.VolleyEnergyCost on top of the stroke, at the contact step, only with fatigue on, recorded in FatiguePoint.
    public sealed class VolleyEnergyTests
    {
        private static FatigueSettings Settings(bool on=true)=>FatigueSettings.From(ConfigFixtures.Default().With("fatigue.enabled",on?1:0));
        private static ContactEvent At(SurfaceId surface,Vector3 point)=>new ContactEvent(surface,point,Vector2.zero,CourtDimensions.InwardNormal(surface),1,0,ContactTiming.Exact,Vector3.zero,false);
        private static string Projection(Func<FieldInfo,bool> keep)
        {
            var canonical=new StringBuilder();
            foreach(var field in typeof(RallyParams).GetFields(BindingFlags.Public|BindingFlags.Instance).Where(keep).OrderBy(f=>f.Name,StringComparer.Ordinal))
                canonical.Append(field.Name).Append('=').Append(((double)field.GetValue(new RallyParams())).ToString("R",CultureInfo.InvariantCulture)).Append('\n');
            using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())),0,6).Replace("-","").ToLowerInvariant();
        }

        [Test] public void RallyParams_VolleyEnergyCost_IsTwoPoints_AndOnlyTheRallyParamsHashMoves()
        {
            Assert.That(new RallyParams().VolleyEnergyCost,Is.EqualTo(2.0));
            // Rival styles (19 nov) add the Style* fields; without them the hashes below are the ones this test always fixed.
            Assert.That(Projection(f=>f.Name!="VolleyEnergyCost"&&!f.Name.StartsWith("Style",StringComparison.Ordinal)&&!f.Name.StartsWith("HumanVolley",StringComparison.Ordinal)),Is.EqualTo("6a7b329a7b85"),"without the new field the hash is the volley v1 one");
            Assert.That(Projection(f=>!f.Name.StartsWith("Style",StringComparison.Ordinal)&&!f.Name.StartsWith("HumanVolley",StringComparison.Ordinal)),Is.EqualTo("5cdbb01e4621"));
            var defaults=ConfigFixtures.Default();
            Assert.That(defaults.Hash,Is.EqualTo("207da8f2c636"));Assert.That(defaults.Ordered.Count,Is.EqualTo(132));
            Assert.That(EffectiveConfig.From(defaults).SimulationKey,Is.EqualTo("166a96e319c7"));
            TestContext.WriteLine("VOLLEY_ENERGY_RALLYPARAMS_HASH="+new RallyParams().Hash+" projection="+Projection(f=>f.Name!="VolleyEnergyCost")+" defaults="+defaults.Hash);
        }
        [Test] public void Volley_ChargesItsCostOnTopOfTheStroke_AndRecordsIt()
        {
            var s=new FatigueState(Settings(),50);s.BeginPoint();
            s.Strike(.5);Assert.That(s.Energy,Is.EqualTo(50),"a soft stroke costs nothing by itself");
            s.Volley(2);
            Assert.That(s.Energy,Is.EqualTo(48));Assert.That(s.Point.VolleyCost,Is.EqualTo(2));Assert.That(s.Point.Minimum,Is.EqualTo(48));Assert.That(s.Point.End,Is.EqualTo(48));
            s.Volley(2);Assert.That(s.Energy,Is.EqualTo(46));Assert.That(s.Point.VolleyCost,Is.EqualTo(4),"one cost per volley of the point");
            s.ClosePoint();s.BeginPoint();Assert.That(s.Point.VolleyCost,Is.Zero,"each point records its own volleys");
        }
        [Test] public void Volley_CostsNothing_WithFatigueOff_OutsideAPoint_OrWithZeroCost()
        {
            var off=new FatigueState(Settings(false),50);off.BeginPoint();off.Volley(2);
            Assert.That(off.Energy,Is.EqualTo(50));Assert.That(off.Point.VolleyCost,Is.Zero);
            var closed=new FatigueState(Settings(),50);closed.Volley(2);Assert.That(closed.Energy,Is.EqualTo(50));Assert.That(closed.Point.VolleyCost,Is.Zero);
            var zero=new FatigueState(Settings(),50);zero.BeginPoint();zero.Volley(0);Assert.That(zero.Energy,Is.EqualTo(50));Assert.That(zero.Point.VolleyCost,Is.Zero);
        }
        [Test] public void DelayedVolleyContact_ChargesAtItsStep_AndReplaysTheRecoveryAfterIt([Values(.5,1)] double power)
        {
            var p=Settings();var expected=new FatigueState(p,40);var delayed=new FatigueState(p,40);
            expected.BeginPoint();delayed.BeginPoint();delayed.BeginFlight(0);
            for(int step=1;step<=120;step++)
            {expected.Step(0,BodyPhase.Set,0,false);if(step==10){expected.Strike(power);expected.Volley(2);}delayed.Advance(step,0,BodyPhase.Set,0,false);}
            delayed.StrikeAt(10,power,2);
            Assert.That(delayed.Energy,Is.EqualTo(expected.Energy));Assert.That(delayed.Point.VolleyCost,Is.EqualTo(2));
            Assert.That(delayed.Point.QuietRecovered,Is.EqualTo(expected.Point.QuietRecovered));Assert.That(delayed.Point.StrongCost,Is.EqualTo(expected.Point.StrongCost));
            Assert.That(delayed.EnergyAt(9),Is.EqualTo(40+9*p.RecoverPerS/p.StepHz).Within(1e-10),"before the contact nothing changes");
            Assert.That(delayed.EnergyAt(10),Is.LessThan(delayed.EnergyAt(9)),"the cost lands on the contact step");
            delayed.StrikeAt(10,power,2);Assert.That(delayed.Energy,Is.EqualTo(expected.Energy),"a duplicate callback never charges twice");
        }
        [Test] public void StrikeAt_WithoutVolleyCost_IsTheOldStrike_BitForBit([Values(.5,1)] double power)
        {
            var p=Settings();var old=new FatigueState(p,40);var now=new FatigueState(p,40);
            old.BeginPoint();now.BeginPoint();old.BeginFlight(0);now.BeginFlight(0);
            for(int step=1;step<=60;step++){old.Advance(step,.01,BodyPhase.Set,0,false);now.Advance(step,.01,BodyPhase.Set,0,false);}
            old.StrikeAt(10,power);now.StrikeAt(10,power,0);
            Assert.That(now.Energy,Is.EqualTo(old.Energy));Assert.That(now.Point.Minimum,Is.EqualTo(old.Point.Minimum));Assert.That(now.Point.VolleyCost,Is.Zero);
            for(long step=0;step<=60;step++)Assert.That(now.EnergyAt(step),Is.EqualTo(old.EnergyAt(step)),"step "+step);
        }
        [Test] public void Context_ChargesTheVolleyCost_OnlyForAContactBeforeTheBounce()
        {
            var context=RallyFixtures.Context();var rules=context.NewRules();rules.Begin(PlayerId.Rival,PlayerId.Human);
            rules.OnBallContact(At(SurfaceId.FrontWall,new Vector3(3.2f,2.5f,0)));
            Assert.That(rules.CanHit(PlayerId.Human),Is.True);Assert.That(context.VolleyEnergyCost(rules),Is.EqualTo(2.0),"before the bounce: a volley");
            rules.OnBallContact(At(SurfaceId.Floor,new Vector3(3.2f,0,6)));
            Assert.That(rules.FloorBounces,Is.EqualTo(1));Assert.That(context.VolleyEnergyCost(rules),Is.Zero,"after the bounce: the usual stroke");
        }
        [Test] public void RallySim_EveryVolleyContact_ChargesItsStriker_AndNobodyElse()
        {
            var context=RallyFixtures.Context();int volleys=0;
            for(int i=0;i<3;i++)
                using(var sim=new RallySim(context,new BotBrain(context,30,20261118+2*i,takeVolley:true),new FondoAi(context,AiDifficulty.For(context.Params,AiLevel.Medium),20261119+2*i)))
                    foreach(var point in sim.PlayMatch(i%2==0?PlayerId.Human:PlayerId.Rival).Points)
                    {
                        int n=point.Receptions.Count(r=>r.Who==PlayerId.Human&&r.Volley&&r.Result==ReceptionResult.Contact);volleys+=n;
                        Assert.That(point.HumanFatigue.VolleyCost,Is.EqualTo(n*2.0),"one cost per human volley contact of the point");
                        Assert.That(point.RivalFatigue.VolleyCost,Is.Zero,"the Fondo rival does not volley in v1");
                    }
            Assert.That(volleys,Is.GreaterThan(0),"a bot that takes every offered volley volleys in three games");
        }
        [Test] public void RallySim_NobodyVolleys_NoVolleyCostIsEverCharged()
        {
            var context=RallyFixtures.Context();
            using(var sim=new RallySim(context,new BotBrain(context,30,20261117),new FondoAi(context,AiDifficulty.For(context.Params,AiLevel.Medium),20261118)))
                foreach(var point in sim.PlayMatch(PlayerId.Human).Points)
                {Assert.That(point.HumanFatigue.VolleyCost,Is.Zero);Assert.That(point.RivalFatigue.VolleyCost,Is.Zero);}
        }
    }
}
