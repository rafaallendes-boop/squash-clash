using System;
using System.Collections.Generic;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
namespace SquashClash.Core.Rally
{
    // Values come only from the effective preset; the same immutable settings serve both players.
    public sealed class FatigueSettings
    {
        public bool Enabled {get;private set;}
        public int StepHz {get;private set;}
        public double CostPerMeter,StrongFrom,StrongCost,RecoverBetween,RecoverPerS,Onset,LegsMax,ReactionMaxMs,ArmSpeedMax,PositionGainMax;
        private FatigueSettings(){}
        public static FatigueSettings From(ParamSet p)=>new FatigueSettings
        {
            Enabled=p.GetBool("fatigue.enabled"),StepHz=p.GetInt("physics.stepHz"),CostPerMeter=p.Get("fatigue.costPerMeter"),
            StrongFrom=p.Get("fatigue.strongFrom"),StrongCost=p.Get("fatigue.strongCost"),RecoverBetween=p.Get("fatigue.recoverBetween"),
            RecoverPerS=p.Get("fatigue.recoverPerS"),Onset=p.Get("fatigue.onset"),LegsMax=p.Get("fatigue.legsMax"),
            ReactionMaxMs=p.Get("fatigue.reactionMaxMs"),ArmSpeedMax=p.Get("fatigue.armSpeedMax"),PositionGainMax=p.Get("fatigue.positionGainMax")
        };
        public double Factor(double energy)=>Enabled?Math.Max(0,Math.Min(1,(Onset-energy)/Onset)):0;
        public const double RedBelow=25;
        // The striker's arm for one contact (spec 4): scaffold only.
        public FatigueArm Arm(double strikerEnergy)=>FatigueArm.None;
        // Energy bar tone (spec 6): 0 neutral, 1 amber, 2 red. Scaffold only.
        public int Tone(double energy)=>0;
    }
    public readonly struct FatigueEffects
    {
        public readonly float MovementFactor;public readonly int ExtraSteps;
        private FatigueEffects(float movement,int extra){MovementFactor=movement;ExtraSteps=extra;}
        public static FatigueEffects For(double reception,FatigueSettings p)
        {
            double f=p.Enabled?Math.Max(0,Math.Min(1,reception)):0;
            return new FatigueEffects((float)(1-p.LegsMax*f),(int)Math.Round(p.ReactionMaxMs*f/1000*p.StepHz));
        }
    }
    [Serializable] public sealed class FatiguePoint
    {
        public double Start,End,AfterRecovery,Meters,MovementCost,StrongCost,QuietRecovered,BetweenRecovered,Minimum;
        public FatiguePoint Copy()=>(FatiguePoint)MemberwiseClone();
    }
    // Energy advances on simulation steps only. The small flight history also accounts for delayed contacts
    // at their contact step (including clamping/recovery), without changing the ball or contact contract.
    public sealed class FatigueState
    {
        private readonly FatigueSettings settings;
        private bool open,struck;private long firstStep;
        private double flightEnergy;private FatiguePoint flightPoint;
        private readonly List<Sample> samples=new List<Sample>();
        private readonly List<double> energies=new List<double>();
        private readonly struct Sample
        {
            public readonly double Meters;public readonly BodyPhase Phase;public readonly float Speed;public readonly bool Reacting;
            public Sample(double meters,BodyPhase phase,float speed,bool reacting){Meters=meters;Phase=phase;Speed=speed;Reacting=reacting;}
        }
        public double Energy {get;private set;}
        public FatiguePoint Point {get;private set;}
        public FatigueState(FatigueSettings settings,double energy=100)
        {this.settings=settings??throw new ArgumentNullException(nameof(settings));Energy=Clamp(energy);Point=new FatiguePoint{Start=Energy,End=Energy,AfterRecovery=Energy,Minimum=Energy};}
        public void BeginPoint()
        {open=true;Point=new FatiguePoint{Start=Energy,End=Energy,AfterRecovery=Energy,Minimum=Energy};}
        public void Step(double meters,BodyPhase phase,float speed,bool reacting)
        {
            if(!open||!settings.Enabled)return;
            Point.Meters+=meters;double cost=settings.CostPerMeter*meters;Point.MovementCost+=cost;Energy=Clamp(Energy-cost);
            Point.Minimum=Math.Min(Point.Minimum,Energy);
            if((phase==BodyPhase.Waiting||phase==BodyPhase.Set)&&speed<.05f&&!reacting)
            {double before=Energy;Energy=Clamp(Energy+settings.RecoverPerS/settings.StepHz);Point.QuietRecovered+=Energy-before;}
            Point.End=Point.AfterRecovery=Energy;
        }
        public void Strike(double power)
        {
            if(!open||!settings.Enabled||power<=settings.StrongFrom)return;
            double cost=settings.StrongCost*(power-settings.StrongFrom)/(1-settings.StrongFrom);
            Point.StrongCost+=cost;Energy=Clamp(Energy-cost);Point.Minimum=Math.Min(Point.Minimum,Energy);Point.End=Point.AfterRecovery=Energy;
        }
        public void ClosePoint()
        {
            if(!open)return;open=false;
            Point.End=Energy;
            if(settings.Enabled){double before=Energy;Energy=Clamp(Energy+settings.RecoverBetween*(100-Energy));Point.BetweenRecovered=Energy-before;}
            Point.AfterRecovery=Energy;
        }
        public void BeginFlight(long step)
        {firstStep=step;flightEnergy=Energy;flightPoint=Point.Copy();samples.Clear();energies.Clear();energies.Add(Energy);struck=false;}
        public void Advance(long step,double meters,BodyPhase phase,float speed,bool reacting)
        {
            if(!open||!settings.Enabled)return;
            if(step!=firstStep+samples.Count+1)throw new InvalidOperationException("Noncontiguous fatigue steps");
            samples.Add(new Sample(meters,phase,speed,reacting));Step(meters,phase,speed,reacting);energies.Add(Energy);
        }
        public double EnergyAt(long step)
        {
            if(!settings.Enabled)return Energy;
            int offset=checked((int)(step-firstStep));
            if(offset<0||offset>=energies.Count)throw new ArgumentOutOfRangeException(nameof(step));
            return energies[offset];
        }
        public void StrikeAt(long step,double power)
        {
            if(!open||!settings.Enabled||struck||power<=settings.StrongFrom||settings.StrongCost==0)return;
            int offset=checked((int)(step-firstStep));
            if(offset<0||offset>samples.Count)throw new ArgumentOutOfRangeException(nameof(step));
            struck=true;Energy=flightEnergy;Point=flightPoint.Copy();
            if(offset==0)Strike(power);energies[0]=Energy;
            for(int i=0;i<samples.Count;i++)
            {
                var s=samples[i];Step(s.Meters,s.Phase,s.Speed,s.Reacting);
                if(i+1==offset)Strike(power);energies[i+1]=Energy;
            }
        }
        private static double Clamp(double energy)=>Math.Max(0,Math.Min(100,energy));
    }
}
