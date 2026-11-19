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
        // The striker's arm for one contact (spec 4): f_s from the striker's own energy at its contact step, before that stroke's cost.
        // One function for the human, the rival and RallySim. Off: no arm at all, so the shot stays bit for bit the old one.
        public FatigueArm Arm(double strikerEnergy)
        {
            if(!Enabled)return FatigueArm.None;
            double f=Factor(strikerEnergy);
            return new FatigueArm((float)(ArmSpeedMax*f),(float)(PositionGainMax*f),f);
        }
        // Energy bar tone (spec 6): 0 neutral (at or above onset), 1 amber, 2 red (strictly below 25).
        public int Tone(double energy)=>!Enabled||energy>=Onset?0:energy<RedBelow?2:1;
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
        // Extra energy charged for volley contacts in this point (RallyParams.VolleyEnergyCost each).
        public double VolleyCost;
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
        // A volley contact: its extra cost, charged at the contact itself after the stroke's own cost. Zero costs nothing.
        public void Volley(double cost)
        {
            if(!open||!settings.Enabled||cost<=0)return;
            Point.VolleyCost+=cost;Energy=Clamp(Energy-cost);Point.Minimum=Math.Min(Point.Minimum,Energy);Point.End=Point.AfterRecovery=Energy;
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
        public void StrikeAt(long step,double power)=>StrikeAt(step,power,0);
        // The same delayed contact, plus the volley cost at that step when the contact was a volley.
        public void StrikeAt(long step,double power,double volleyCost)
        {
            // Without a volley cost the condition is the old one: a soft stroke replays nothing.
            bool strong=power>settings.StrongFrom&&settings.StrongCost!=0;
            if(!open||!settings.Enabled||struck||!strong&&volleyCost<=0)return;
            int offset=checked((int)(step-firstStep));
            if(offset<0||offset>samples.Count)throw new ArgumentOutOfRangeException(nameof(step));
            struck=true;Energy=flightEnergy;Point=flightPoint.Copy();
            if(offset==0){Strike(power);Volley(volleyCost);}energies[0]=Energy;
            for(int i=0;i<samples.Count;i++)
            {
                var s=samples[i];Step(s.Meters,s.Phase,s.Speed,s.Reacting);
                if(i+1==offset){Strike(power);Volley(volleyCost);}energies[i+1]=Energy;
            }
        }
        private static double Clamp(double energy)=>Math.Max(0,Math.Min(100,energy));
    }
}
