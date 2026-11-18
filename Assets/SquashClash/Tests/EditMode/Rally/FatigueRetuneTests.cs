using System;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Tests.Config;
namespace SquashClash.Core.Tests.Rally
{
    // Objectives O1/O2 of the 18 Nov 2026 retune, measured on the REAL per-point data of the phone
    // (metrics 18 Nov 00:42-01:08Z, 36 human points: meters, strong-stroke cost and quiet recovery at 0.5/s).
    public sealed class FatigueRetuneTests
    {
        // meters, strong cost, quiet recovery (energy) of each real human point, in recorded order
        private static readonly double[][] RealPoints=
        {
            new[]{5.361,0.000,0.458},new[]{24.855,0.944,2.904},new[]{5.412,0.000,0.471},new[]{27.131,1.171,3.100},new[]{8.269,0.000,0.704},new[]{29.584,1.184,3.550},
            new[]{13.724,0.661,1.579},new[]{37.307,2.967,3.112},new[]{2.453,0.000,0.667},new[]{14.954,1.945,1.192},new[]{22.306,1.098,1.954},new[]{8.148,0.371,3.733},
            new[]{23.277,1.131,1.946},new[]{14.955,0.605,1.150},new[]{18.277,0.908,1.650},new[]{27.261,0.546,4.283},new[]{2.453,0.000,0.667},new[]{18.211,0.265,1.633},
            new[]{19.351,1.468,3.858},new[]{9.971,0.000,2.042},new[]{20.311,2.039,2.558},new[]{31.337,0.662,3.929},new[]{14.561,1.343,1.483},new[]{5.063,0.000,0.437},
            new[]{19.683,2.304,1.800},new[]{35.670,3.339,2.346},new[]{48.556,4.782,5.050},new[]{31.198,3.180,4.004},new[]{19.198,0.844,2.450},new[]{8.379,1.605,1.083},
            new[]{9.009,0.000,1.017},new[]{11.343,0.412,3.204},new[]{17.377,4.962,3.496},new[]{54.538,5.695,5.192},new[]{51.184,10.709,7.238},new[]{2.300,0.000,0.704}
        };
        private const double RecordedRecoverPerS=0.5;
        private static FatigueSettings Settings()=>FatigueSettings.From(ConfigFixtures.Default());
        // Plays one point through the real FatigueState: movement, strong strokes, standing still, then the between-points recovery.
        private static void Play(FatigueState state,FatigueSettings s,double[] point)
        {
            state.BeginPoint();
            state.Step(point[0],BodyPhase.Approaching,1f,false);
            for(double rest=point[1];rest>1e-9;)
            {
                double cost=Math.Min(rest,s.StrongCost);rest-=cost;
                state.Strike(s.StrongFrom+cost/s.StrongCost*(1-s.StrongFrom));
            }
            int quiet=(int)Math.Round(point[2]*s.StepHz/RecordedRecoverPerS);
            for(int i=0;i<quiet;i++)state.Step(0,BodyPhase.Waiting,0f,false);
            state.ClosePoint();
        }
        private static double Median(System.Collections.Generic.IEnumerable<double> v){var a=v.OrderBy(x=>x).ToArray();return a.Length%2==1?a[a.Length/2]:(a[a.Length/2-1]+a[a.Length/2])/2;}
        // Median over 400 deterministic bootstrap games of 20 real points: energy at the start of each point.
        private static double[] MedianGame(int points)
        {
            var s=Settings();uint x=20261118;var runs=new double[400][];
            for(int r=0;r<runs.Length;r++)
            {
                var st=new FatigueState(s);runs[r]=new double[points];
                for(int i=0;i<points;i++)
                {
                    runs[r][i]=st.Energy;x=unchecked(x*1664525u+1013904223u);
                    Play(st,s,RealPoints[(int)((x>>16)%(uint)RealPoints.Length)]);
                }
            }
            return Enumerable.Range(0,points).Select(i=>Median(runs.Select(q=>q[i]))).ToArray();
        }
        [Test] public void O1_RealRallies_DropBelowOnsetAroundMidGame_AndEndBetween45And60()
        {
            var game=MedianGame(20);var onset=Settings().Onset;
            Assert.That(game[10],Is.LessThan(onset),"energy at the start of the 11th point (mid game): "+string.Join(" ",game.Select(v=>v.ToString("0.0"))));
            Assert.That(game[19],Is.InRange(45.0,60.0),"energy at the start of the 20th point");
        }
        [Test] public void O1_LongerRallies_TireMoreThanShortOnes()
        {
            var s=Settings();double End(double meters){var st=new FatigueState(s);for(int i=0;i<12;i++)Play(st,s,new[]{meters,0.9,2.0});return st.Energy;}
            Assert.That(End(30),Is.LessThan(End(18)-5));Assert.That(End(18),Is.LessThan(End(8)-5));
        }
        [Test] public void O2_AfterEnergy40_TheNextThreeMedianPointsStartAtOrBelow55()
        {
            var s=Settings();var st=new FatigueState(s,40);
            var median=new[]{Median(RealPoints.Select(p=>p[0])),Median(RealPoints.Select(p=>p[1])),Median(RealPoints.Select(p=>p[2]))};
            var starts=new double[4];
            for(int i=0;i<4;i++){starts[i]=st.Energy;Play(st,s,median);}
            for(int i=1;i<4;i++)Assert.That(starts[i],Is.LessThanOrEqualTo(55),"start of point "+(i+1)+": "+string.Join(" ",starts.Select(v=>v.ToString("0.0"))));
        }
        [Test] public void O3_LongSimulatedRallies_DoNotLeaveEnergyPinnedUnder10()
        {
            // Experiment B scale: about 36 m and 3 strong-stroke energy per point, 9 points per match (median length of the simulated matches).
            var s=Settings();var st=new FatigueState(s);int low=0;
            for(int i=0;i<9;i++){if(st.Energy<10)low++;Play(st,s,new[]{36.0,2.5,3.0});}
            Assert.That(low,Is.LessThan(5));
        }
        [Test] public void O4_ANewMatchStartsAtFullEnergy_AndNoSavedPresetOverridesFatigue()
        {
            Assert.That(new FatigueState(Settings()).Energy,Is.EqualTo(100));
        }
    }
}
