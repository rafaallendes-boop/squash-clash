using System;
using System.IO;
using System.Collections.Generic;
using SquashClash.Core.Config;
using SquashClash.Core.Tests.Config;
using NUnit.Framework;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class PeakSelectionTests
    {
        [TestCase(300,0,60,true)]
        [TestCase(400,0,120,false)]
        [TestCase(400,0,180,false)]
        [TestCase(400,10,180,false)]
        public void PredeclaredCandidates_MandatoryRiskIsEliminatory(float full,float dead,double decay,bool expectedPass)
        {
            bool allPass=true;
            foreach(var profile in new[]{(speed:400f,duration:.20),(speed:850f,duration:.10),(speed:1000f,duration:.06)})
            {
                var e=new PeakPowerEstimator(new PowerSettings(80,30,3,51,dead,full,1),20,decay);
                int intervals=(int)Math.Round(profile.duration/.02);
                for(int i=0;i<=intervals;i++)e.Push(new TouchSample(1,TouchPhaseKind.Moved,new Vector2(0,(float)(i*.02)*profile.speed),i*.02,i),1);
                var r=e.Push(new TouchSample(1,TouchPhaseKind.Ended,new Vector2(0,(float)profile.duration*profile.speed),profile.duration+.15,intervals+1),1);
                bool pass=!r.Conservative&&r.P<.6f;allPass&=pass;
                TestContext.WriteLine($"RISK|{full:R}|{dead:R}|{decay:R}|{profile.speed:R}|{profile.duration:R}|{r.P:R}|{pass}");
            }
            Assert.That(allPass,Is.EqualTo(expectedPass));
        }
        [TestCase(.025)]
        [TestCase(.050)]
        public void SelectedCandidate_StrongFlickStopped25To50ms_StaysStrong(double pause)
        {
            var e=new PeakPowerEstimator(new PowerSettings(80,30,3,51,0,300,1),20,60);
            for(int i=0;i<=5;i++)e.Push(new TouchSample(1,TouchPhaseKind.Moved,new Vector2(i*10,0),i*.02,i),1);
            var r=e.Push(new TouchSample(1,TouchPhaseKind.Ended,new Vector2(50,0),.10+pause,6),1);
            TestContext.WriteLine($"STOPPED-FLICK|{pause:R}|{r.P:R}");
            Assert.That(r.Conservative,Is.False);Assert.That(r.P,Is.GreaterThanOrEqualTo(.6f));
        }
        [Test] public void AcceptedCost_500mmPerSFlickStopped100ms_LosesMorePowerWithShortMemory()
        {
            float[] powers=new float[2];int index=0;
            foreach(var setting in new[]{(full:400f,decay:120d),(full:300f,decay:60d)})
            {
                var e=new PeakPowerEstimator(new PowerSettings(80,30,3,51,0,setting.full,1),20,setting.decay);
                for(int i=0;i<=5;i++)e.Push(new TouchSample(1,TouchPhaseKind.Moved,new Vector2(i*10,0),i*.02,i),1);
                powers[index++]=e.Push(new TouchSample(1,TouchPhaseKind.Ended,new Vector2(50,0),.20,6),1).P;
            }
            TestContext.WriteLine($"ACCEPTED-COST|{powers[0]:R}|{powers[1]:R}");
            Assert.That(powers[0],Is.EqualTo(.54f).Within(.01));Assert.That(powers[1],Is.EqualTo(.31f).Within(.01));
        }
        [Test] public void ExperimentalPreset_MatchesRiskWinner_LeavesDefaultsUnchanged()
        {
            var defaults=ConfigFixtures.Default();
            string json=File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"docs/validation/presets/S1-calib-gesture.json"));
            Assert.That(PresetCodec.TryParse(json,out var preset,out var error),Is.True,error);
            var candidate=PresetCodec.Overlay(defaults,preset,new List<string>());
            Assert.That(candidate.Get("power.fullMmPerS"),Is.EqualTo(300));Assert.That(candidate.Get("power.peakDecayMs"),Is.EqualTo(60));
            Assert.That(candidate.Get("power.peakSmoothingMs"),Is.EqualTo(20));Assert.That(candidate.Get("power.method"),Is.EqualTo(1));
            Assert.That(candidate.Get("aim.freezeOnRelease"),Is.EqualTo(1));Assert.That(candidate.Get("launcher.repeatSeconds"),Is.Zero);
            Assert.That(candidate.Hash,Is.EqualTo("08b6fe90889b"));Assert.That(defaults.Hash,Is.EqualTo("6095c0b38614"));
        }
    }
}
