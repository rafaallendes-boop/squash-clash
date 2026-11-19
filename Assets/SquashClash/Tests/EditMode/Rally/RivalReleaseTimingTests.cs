using System;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class RivalReleaseTimingTests
    {
        [TestCase(-13.25,-1,217,38,42.75)]
        [TestCase(8,-200,2,38,-47)]
        [TestCase(8,-7,14,38,11.5)]
        public void UnsafeWindow_ShiftsOnlyTheMean(double sample,double lower,double upper,double sigma,double expected)
            =>Assert.That(RivalReleaseTiming.Adjust(sample,lower,upper,sigma,1.5),Is.EqualTo(expected).Within(1e-12));

        [Test] public void SafeWindow_IncludingEqualityAndSignedZero_ReturnsIdenticalBits()
        {
            double negativeZero=BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000UL));
            foreach(double sample in new[]{negativeZero,0d,-123.75,84.125,double.Epsilon})
                foreach(var edges in new[]{(-57d,57d),(-100d,200d)})
                    Assert.That(BitConverter.DoubleToInt64Bits(RivalReleaseTiming.Adjust(sample,edges.Item1,edges.Item2,38,1.5)),
                        Is.EqualTo(BitConverter.DoubleToInt64Bits(sample)));
        }
        [Test] public void ExactlyThreeSigma_UsesTheUniqueFeasibleMean()
            =>Assert.That(RivalReleaseTiming.Adjust(9,-10,104,38,1.5),Is.EqualTo(56));
        [Test] public void ZeroSigmaOrZeroMargin_DoesNotChangeTheSample()
        {
            Assert.That(RivalReleaseTiming.Adjust(-4,-1,200,0,1.5),Is.EqualTo(-4));
            Assert.That(RivalReleaseTiming.Adjust(-4,-1,200,38,0),Is.EqualTo(-4));
        }
        [Test] public void MarginDefault_IsOnePointFive_OutsideDefaultPreset()
        {
            var field=typeof(RallyParams).GetField("FondoReleaseMarginSigma");
            Assert.That(field,Is.Not.Null);
            Assert.That(field.GetValue(new RallyParams()),Is.EqualTo(1.5));
            var context=RallyFixtures.Context();
            Assert.That(context.Config.Hash,Is.EqualTo("207da8f2c636"));
            Assert.That(context.Config.Source.Ordered.Count,Is.EqualTo(132));
            Assert.That(context.Config.SimulationKey,Is.EqualTo("166a96e319c7"));
        }
        private static RallyParams Parameters(double factor)
        {
            var p=new RallyParams();var field=typeof(RallyParams).GetField("FondoReleaseMarginSigma");
            Assert.That(field,Is.Not.Null,"The only new policy parameter belongs to RallyParams.");field.SetValue(p,factor);return p;
        }
        private static ReceptionPlan Plan(RallyContext context,bool safe)
        {
            var stance=new Vector2(2.2f,6.5f);long star=240;
            double rate=context.StepHz*context.Tempo;
            double lower=safe?-200:-1,upper=safe?200:217;
            var plan=new ContactPlan(true,SimTime.FromSteps(star+lower*rate/1000),SimTime.FromSteps(star+upper*rate/1000),star,
                new Vector3(1.6f,.8f,6.5f),StrokeSide.Drive,1,context.FrameAt(stance),context.Legal,context.Court);
            return new ReceptionPlan(true,stance,stance,plan,100,.35,1,"planned");
        }
        private static AiObservation Observation(RallyContext context)
            =>new AiObservation(default,context.WaitPosition(PlayerId.Rival),Vector2.zero,context.WaitPosition(PlayerId.Human),Vector2.zero,3,0,false);
        [TestCase(true)] [TestCase(false)]
        public void Fondo_UsesTheSameRandomStream_AndOnlyShiftsUnsafeRelease(bool safe)
        {
            var oldContext=RallyFixtures.Context(Parameters(0));var nextContext=RallyFixtures.Context(Parameters(1.5));
            var oldAi=new FondoAi(oldContext,AiDifficulty.For(oldContext.Params,AiLevel.Medium),41);
            var nextAi=new FondoAi(nextContext,AiDifficulty.For(nextContext.Params,AiLevel.Medium),41);
            var own=Plan(nextContext,safe);var observation=Observation(nextContext);
            for(int i=0;i<32;i++)
            {
                var old=oldAi.Decide(observation,own);var next=nextAi.Decide(observation,own);
                Assert.That((next.Swing,next.Defensive,next.Option.Line,next.Option.Height,next.Option.Power,next.Aim,next.P),
                    Is.EqualTo((old.Swing,old.Defensive,old.Option.Line,old.Option.Height,old.Option.Power,old.Aim,old.P)));
                if(safe)Assert.That(BitConverter.DoubleToInt64Bits(next.DeltaMs),Is.EqualTo(BitConverter.DoubleToInt64Bits(old.DeltaMs)));
                else Assert.That(next.DeltaMs-old.DeltaMs,Is.EqualTo(56).Within(1e-9));
                Assert.That(nextAi.StanceNoise(),Is.EqualTo(oldAi.StanceNoise()),"No extra RNG draw after the timing sample.");
            }
        }
        [Test] public void HumanBot_DecisionsRemainBitwiseIdentical()
        {
            var before=RallyFixtures.Context(Parameters(0));var after=RallyFixtures.Context(Parameters(1.5));
            var a=new BotBrain(before,30,17);var b=new BotBrain(after,30,17);
            var own=Plan(after,false);var observation=Observation(after);
            for(int i=0;i<32;i++)
            {
                var first=a.Decide(observation,own);var second=b.Decide(observation,own);
                Assert.That((second.Swing,second.Aim,second.P),Is.EqualTo((first.Swing,first.Aim,first.P)));
                Assert.That(BitConverter.DoubleToInt64Bits(second.DeltaMs),Is.EqualTo(BitConverter.DoubleToInt64Bits(first.DeltaMs)));
            }
        }
    }
}
