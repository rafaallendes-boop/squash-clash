using System;
using System.Collections.Generic;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    public enum AiLevel { Easy, Medium, Hard }
    // Spec 11.5: difficulty changes these and nothing else (never physics, rules, attributes or information).
    public readonly struct AiDifficulty
    {
        public readonly double PerceptionS,StanceSigma,AimSigma,PowerSigma,TimingSigmaMs,Temperature;
        public AiDifficulty(double perceptionS,double stanceSigma,double aimSigma,double powerSigma,double timingSigmaMs,double temperature)
        {PerceptionS=perceptionS;StanceSigma=stanceSigma;AimSigma=aimSigma;PowerSigma=powerSigma;TimingSigmaMs=timingSigmaMs;Temperature=temperature;}
        public static AiDifficulty For(RallyParams p,AiLevel level)
        {
            switch(level)
            {
                case AiLevel.Easy:return new AiDifficulty(p.EasyPerceptionS,p.EasyStanceSigma,p.EasyAimSigma,p.EasyPowerSigma,p.EasyTimingSigmaMs,p.EasyTemperature);
                case AiLevel.Hard:return new AiDifficulty(p.HardPerceptionS,p.HardStanceSigma,p.HardAimSigma,p.HardPowerSigma,p.HardTimingSigmaMs,p.HardTemperature);
                default:return new AiDifficulty(p.MediumPerceptionS,p.MediumStanceSigma,p.MediumAimSigma,p.MediumPowerSigma,p.MediumTimingSigmaMs,p.MediumTemperature);
            }
        }
    }
    // Spec 11.2 "observable" row, and nothing else. No touch, gesture, aim, power, delta, rings or planner objects of the opponent.
    public readonly struct AiObservation
    {
        public readonly BallState Ball;
        public readonly Vector2 SelfPosition,SelfVelocity,OpponentPosition,OpponentVelocity;
        public readonly int Shots,FloorBounces,ScoreSelf,ScoreOpponent;
        public readonly bool FrontWallReached;
        public AiObservation(BallState ball,Vector2 selfPosition,Vector2 selfVelocity,Vector2 opponentPosition,Vector2 opponentVelocity,
            int shots,int floorBounces,bool frontWallReached,int scoreSelf=0,int scoreOpponent=0)
        {Ball=ball;SelfPosition=selfPosition;SelfVelocity=selfVelocity;OpponentPosition=opponentPosition;OpponentVelocity=opponentVelocity;
         Shots=shots;FloorBounces=floorBounces;FrontWallReached=frontWallReached;ScoreSelf=scoreSelf;ScoreOpponent=scoreOpponent;}
    }
    public readonly struct AiShot
    {
        public readonly bool Swing,Defensive;
        public readonly ShotOption Option;
        public readonly Vector2 Aim;public readonly float P;
        // Release offset from T* in real perceived milliseconds (spec 11.3 step 3).
        public readonly double DeltaMs;
        public AiShot(bool swing,ShotOption option,Vector2 aim,float p,double deltaMs,bool defensive){Swing=swing;Option=option;Aim=aim;P=p;DeltaMs=deltaMs;Defensive=defensive;}
    }
    public interface IRallyBrain
    {
        // Extra delay before the body reacts (d_perc); zero for a brain that needs none.
        int PerceptionSteps {get;}
        // Prediction noise (sigma_pred) applied to where the body stands; it can only make the plan worse.
        Vector2 StanceNoise();
        // One call per reception: a single attempt, like the human.
        AiShot Decide(in AiObservation observation,in ReceptionPlan own);
    }
    // Basic "Fondo" policy (spec 11.3-11.4): perception -> utility/softmax -> noisy intention and release.
    public sealed class FondoAi : IRallyBrain
    {
        private readonly RallyContext context;
        private readonly AiDifficulty difficulty;
        private readonly RallyRng random;
        private readonly double[] weights=new double[18];
        public FondoAi(RallyContext context,AiDifficulty difficulty,long seed)
        {this.context=context??throw new ArgumentNullException(nameof(context));this.difficulty=difficulty;random=new RallyRng(seed);}
        public int PerceptionSteps=>context.Steps(difficulty.PerceptionS);
        public Vector2 StanceNoise()=>new Vector2((float)random.NextGaussian(difficulty.StanceSigma),(float)random.NextGaussian(difficulty.StanceSigma));
        public AiShot Decide(in AiObservation observation,in ReceptionPlan own)
        {
            if(!own.Reachable||!own.Contact.Exists||own.Contact.TStarStep<0)return default;
            var p=context.Params;var options=ShotOptions.Build(context,own.Contact.PStar);
            bool defensive=own.SlackS<p.FondoDefenseSlackS;
            double best=double.NegativeInfinity;
            for(int i=0;i<options.Count;i++)
            {
                var o=options[i];
                bool allowed=!defensive||(o.Height==ShotHeight.High&&o.Power==ShotPower.Medium&&o.Line!=ShotLine.Drop);
                weights[i]=allowed?Utility(o,observation,own):double.NegativeInfinity;
                if(weights[i]>best)best=weights[i];
            }
            double total=0,temperature=Math.Max(1e-3,difficulty.Temperature);
            for(int i=0;i<options.Count;i++){weights[i]=double.IsNegativeInfinity(weights[i])?0:Math.Exp((weights[i]-best)/temperature);total+=weights[i];}
            double pick=random.NextDouble()*total;int chosen=options.Count-1;
            for(int i=0;i<options.Count;i++){pick-=weights[i];if(pick<0){chosen=i;break;}}
            var option=options[chosen];
            var aim=ShotOptions.ClampAim(context,option.Aim+new Vector2((float)random.NextGaussian(difficulty.AimSigma),(float)random.NextGaussian(difficulty.AimSigma)));
            float power=Mathf.Clamp01(option.P+(float)random.NextGaussian(difficulty.PowerSigma));
            return new AiShot(true,option,aim,power,random.NextGaussian(difficulty.TimingSigmaMs),defensive);
        }
        // U(option) = sum(style weight * feature) - risk aversion * estimated risk (spec 11.3 step 2).
        public double Utility(in ShotOption option,in AiObservation observation,in ReceptionPlan own)
        {
            var p=context.Params;
            double away=Math.Min(1,Vector2.Distance(observation.OpponentPosition,option.Zone)/6);
            double need=Math.Min(1,Vector2.Distance(observation.SelfPosition,context.T)/4);
            double time,risk;
            if(option.Line==ShotLine.Drop)
            {
                // Low weight; only with the opponent behind the T and plenty of own slack (spec 11.4).
                bool open=observation.OpponentPosition.y>context.T.y+.5f&&own.SlackS>=p.FondoDropMinSlackS;
                time=.5;risk=.55+(option.Height==ShotHeight.Low?.2:0)+(option.Power==ShotPower.Strong?.1:0);
                return (open?p.FondoDropWeight:-4)+p.FondoAwayWeight*away+p.FondoTimeWeight*time*need-p.FondoRiskAversion*risk;
            }
            time=(option.Height==ShotHeight.High?1:option.Height==ShotHeight.Mid?.6:.3)*(option.Power==ShotPower.Strong?.7:1);
            // Tin margin for low shots; out line at the back for high and hard ones; pressure makes low shots riskier.
            risk=option.Height==ShotHeight.Low?.35+.3*(1-own.Quality):option.Height==ShotHeight.Mid?.1:(option.Power==ShotPower.Strong?.45:.15);
            double length=option.Long?1:.6;
            return p.FondoLongWeight*length+p.FondoAwayWeight*away+p.FondoTimeWeight*time*need-p.FondoRiskAversion*risk;
        }
    }
}
