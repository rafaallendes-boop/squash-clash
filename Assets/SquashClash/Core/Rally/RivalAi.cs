using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    // Rival styles (design docs/superpowers/specs/2026-11-19-rival-styles-design.md). Display names: Fondista, Definidor, Completo.
    public enum RivalStyleId { Fondista, Definidor, Completo }
    // How a style chooses, and nothing else: utility weights, risk aversion, defence threshold, when the drop is open, volley
    // willingness, wait W and recovery cover. Difficulty (AiDifficulty) stays a separate axis shared by every style (spec 11.5).
    public readonly struct RivalStyle
    {
        public readonly RivalStyleId Id;
        public readonly double LongWeight,AwayWeight,TimeWeight,DropWeight,RiskAversion,DefenseSlackS,DropMinSlackS;
        // Drop also open when the own contact P* is at most this far behind the T (z); -inf: only with the opponent back (Fondo).
        public readonly double DropZoneAhead;
        // Attack feature (drop or low shot): base + ahead * how far in front of the T the own contact is (0..1 over 2 m).
        public readonly double AttackBase,AttackAhead;
        public readonly double VolleyBias,VolleyMinKS,VolleySteal;
        public readonly double WaitBehindT;
        // Recovery after an own short shot: 0 none, < 0 to the T, > 0 that far behind the predicted bounce.
        public readonly double CoverDepth;
        // Two-wall shot (design 2026-11-20-boast-predeclaration, section 5): the boast's base utility; read only with the rival's boast on.
        public readonly double BoastWeight;
        public RivalStyle(RivalStyleId id,double longW,double awayW,double timeW,double dropW,double risk,double defense,double dropSlack,double dropZone,
            double attackBase,double attackAhead,double volleyBias,double volleyMinK,double volleySteal,double wait,double cover,double boast=0)
        {BoastWeight=boast;Id=id;LongWeight=longW;AwayWeight=awayW;TimeWeight=timeW;DropWeight=dropW;RiskAversion=risk;DefenseSlackS=defense;DropMinSlackS=dropSlack;DropZoneAhead=dropZone;
         AttackBase=attackBase;AttackAhead=attackAhead;VolleyBias=volleyBias;VolleyMinKS=volleyMinK;VolleySteal=volleySteal;WaitBehindT=wait;CoverDepth=cover;}
        public static RivalStyle For(RallyParams p,RivalStyleId id)
        {
            switch(id)
            {
                case RivalStyleId.Definidor:return new RivalStyle(id,p.StyleShooterLongWeight,p.StyleShooterAwayWeight,p.StyleShooterTimeWeight,p.StyleShooterDropWeight,p.StyleShooterRiskAversion,
                    p.StyleShooterDefenseSlackS,p.StyleShooterDropMinSlackS,p.StyleShooterDropZoneAhead,p.StyleShooterAttackBase,p.StyleShooterAttackAhead,
                    p.StyleShooterVolleyBias,p.StyleShooterVolleyMinKS,p.StyleShooterVolleySteal,p.StyleShooterWaitBehindT,p.StyleShooterCoverDepth,p.BoastWeightShooter);
                case RivalStyleId.Completo:return new RivalStyle(id,p.StyleAllRoundLongWeight,p.StyleAllRoundAwayWeight,p.StyleAllRoundTimeWeight,p.StyleAllRoundDropWeight,p.StyleAllRoundRiskAversion,
                    p.StyleAllRoundDefenseSlackS,p.StyleAllRoundDropMinSlackS,p.StyleAllRoundDropZoneAhead,p.StyleAllRoundAttackBase,p.StyleAllRoundAttackAhead,
                    p.StyleAllRoundVolleyBias,p.StyleAllRoundVolleyMinKS,p.StyleAllRoundVolleySteal,p.StyleAllRoundWaitBehindT,p.StyleAllRoundCoverDepth,p.BoastWeightAllRound);
                default:return new RivalStyle(id,p.FondoLongWeight,p.FondoAwayWeight,p.FondoTimeWeight,p.FondoDropWeight,p.FondoRiskAversion,
                    p.FondoDefenseSlackS,p.FondoDropMinSlackS,double.NegativeInfinity,0,0,p.StyleFondoVolleyBias,p.StyleFondoVolleyMinKS,0,p.RivalWaitBehindT,p.StyleFondoCoverDepth,p.BoastWeightFondo);
            }
        }
        // The noise factor of a (style, level): it scales the level's five execution/perception noises, never the temperature.
        public static double NoiseK(RallyParams p,RivalStyleId id,AiLevel level)
        {
            switch(id)
            {
                case RivalStyleId.Definidor:return level==AiLevel.Easy?p.StyleNoiseKShooterEasy:level==AiLevel.Hard?p.StyleNoiseKShooterHard:p.StyleNoiseKShooterMedium;
                case RivalStyleId.Completo:return level==AiLevel.Easy?p.StyleNoiseKAllRoundEasy:level==AiLevel.Hard?p.StyleNoiseKAllRoundHard:p.StyleNoiseKAllRoundMedium;
                default:return level==AiLevel.Easy?p.StyleNoiseKFondoEasy:level==AiLevel.Hard?p.StyleNoiseKFondoHard:p.StyleNoiseKFondoMedium;
            }
        }
        public static AiDifficulty Difficulty(RallyParams p,RivalStyleId id,AiLevel level)
        {
            var d=AiDifficulty.For(p,level);double k=NoiseK(p,id,level);
            return new AiDifficulty(d.PerceptionS*k,d.StanceSigma*k,d.AimSigma*k,d.PowerSigma*k,d.TimingSigmaMs*k,d.Temperature);
        }
        public static RivalStyleId FromSeed(long seed)=>(RivalStyleId)(int)(new RallyRng(seed).NextBits()%3);
        public static string Name(RivalStyleId id)=>id==RivalStyleId.Definidor?"Definidor":id==RivalStyleId.Completo?"Completo":"Fondista";
    }
    // Optional for a brain: where it waits and where it recovers after its own stroke. A brain without it uses RallyContext.WaitPosition.
    public interface IRallyPositioning
    {
        Vector2 Wait {get;}
        // afterOwnStroke: the observable state right after the brain's own contact (the ball as just struck).
        Vector2 Recovery(in AiObservation afterOwnStroke);
    }
    // The rival with a style (spec 11.3-11.4). FondoAi stays the historical reference; Fondista with the volley and cover off decides
    // exactly like it (same options, same draws in the same order).
    public sealed class RivalAi : IRallyBrain,IRallyPositioning,IRallyServer
    {
        private readonly RallyContext context;
        private readonly AiDifficulty difficulty;
        private readonly RallyRng random;
        // 18 front-wall options plus at most 4 boasts, for the bounce and the volley contact.
        private readonly double[] weights=new double[44];
        private readonly ShotOption[] all=new ShotOption[44];
        private readonly bool[] atVolley=new bool[44];
        public RivalStyle Style {get;}
        // Observable diagnostic for R3: no legal defensive intention gave the opponent a reachable bounce plan.
        public bool LastStretchedFallback {get;private set;}
        // The game's rival: the level's shared noise scaled by the (style, level) factor.
        public static RivalAi For(RallyContext context,AiLevel level,RivalStyleId style,long seed)
            =>new RivalAi(context,RivalStyle.Difficulty(context.Params,style,level),style,seed);
        public RivalAi(RallyContext context,AiDifficulty difficulty,RivalStyleId style,long seed)
        {this.context=context??throw new ArgumentNullException(nameof(context));this.difficulty=difficulty;Style=RivalStyle.For(context.Params,style);random=new RallyRng(seed);serveRandom=new RallyRng(unchecked(seed*7919L+20261120L));}
        public int PerceptionSteps=>context.Steps(difficulty.PerceptionS);
        public Vector2 StanceNoise()=>new Vector2((float)random.NextGaussian(difficulty.StanceSigma),(float)random.NextGaussian(difficulty.StanceSigma));
        public Vector2 Wait=>context.ClampBody(context.T+new Vector2((float)context.Params.WaitLateralOffset,(float)Style.WaitBehindT));
        public Vector2 Recovery(in AiObservation afterOwnStroke)
        {
            var wait=Wait;double depth=Style.CoverDepth;
            if(depth==0)return wait;
            // The shared predictor from the ball just struck (symmetric with the human's planner); no opponent intention is read.
            var trajectory=context.Predict(afterOwnStroke.Ball);
            foreach(var c in trajectory.Contacts)
            {
                if(c.Surface!=SurfaceId.Floor)continue;
                if(c.Point.z>=context.Params.StyleCoverBounceZ)return wait;
                float x=Mathf.Lerp(context.T.x,c.Point.x,.3f);
                return context.ClampBody(new Vector2(x,depth<0?context.T.y:Mathf.Min(wait.y,c.Point.z+(float)depth)));
            }
            return wait;
        }
        // Serve kinds (design 2026-11-20-serve): the style's drive share, drawn from its own stream so its strokes never change.
        public double ServeDriveShare=>Style.Id==RivalStyleId.Definidor?context.Params.ServeKindDriveShooter:Style.Id==RivalStyleId.Completo?context.Params.ServeKindDriveAllRound:context.Params.ServeKindDriveFondo;
        public ServeType ChooseServe(ServiceBox box)=>serveRandom.NextDouble()<ServeDriveShare?ServeType.Drive:ServeType.Lob;
        private readonly RallyRng serveRandom;
        public bool VolleyWanted(in ReceptionPlan own)
        {
            if(context.Params.StyleRivalVolley==0||!own.Volley.Exists)return false;
            var v=own.Volley.Plan;return (v.KOut.Steps-v.KIn.Steps)/context.StepHz>=Style.VolleyMinKS;
        }
        public AiShot Decide(in AiObservation observation,in ReceptionPlan own)
        {
            LastStretchedFallback=false;
            bool bounce=own.Reachable&&own.Contact.Exists&&own.Contact.TStarStep>=0;
            bool volley=VolleyWanted(own);
            if(!bounce&&!volley)return default;
            var p=context.Params;bool stretched=own.Reason=="stretched",defensive=stretched||own.SlackS<Style.DefenseSlackS;
            int count=0;double best=double.NegativeInfinity;var seen=observation;var mine=own;
            void Add(Vector3 at,bool isVolley,double bonus)
            {
                var options=ShotOptions.Build(context,at);
                if(context.BoastOn)ShotOptions.AddBoasts(context,at,options);
                for(int i=0;i<options.Count;i++)
                {
                    var o=options[i];bool allowed=stretched
                        ?o.Height==ShotHeight.High&&o.Power==ShotPower.Medium&&(o.Line==ShotLine.Straight||o.Line==ShotLine.Cross)
                        :!defensive||(o.Height==ShotHeight.High&&o.Power==ShotPower.Medium&&o.Line!=ShotLine.Drop)||o.Line==ShotLine.Boast;
                    all[count]=o;atVolley[count]=isVolley;
                    weights[count]=allowed?Utility(o,seen,mine,at)+bonus:double.NegativeInfinity;
                    if(weights[count]>best)best=weights[count];
                    count++;
                }
            }
            // A planned low-slack reception still allows the back-corner boast; stretched receptions use pure defence.
            if(bounce)Add(own.Contact.PStar,false,0);
            if(volley)
            {
                var v=own.Volley.Plan;double kv=(v.KOut.Steps-v.KIn.Steps),bonus=Style.VolleyBias;
                if(bounce)
                {
                    double kb=Math.Max(1e-9,own.Contact.KOut.Steps-own.Contact.KIn.Steps);
                    bonus+=Style.VolleySteal*Math.Min(1,Math.Max(0,(own.Contact.TStarStep-v.TStarStep)/(double)context.StepHz))-Style.RiskAversion*(1-Math.Min(1,kv/kb));
                }
                Add(v.PStar,true,bonus);
            }
            int forced=stretched?FilterStretched(count,observation,own,out best):-1;
            LastStretchedFallback=forced>=0;
            double total=0,temperature=Math.Max(1e-3,difficulty.Temperature);
            for(int i=0;i<count;i++){weights[i]=double.IsNegativeInfinity(weights[i])?0:Math.Exp((weights[i]-best)/temperature);total+=weights[i];}
            // Same draw as FondoAi: an option the AI itself predicts as a fault is dropped and the draw repeats.
            int chosen=forced,fallback=0;
            for(int i=1;i<count;i++)if(weights[i]>weights[fallback])fallback=i;
            for(int attempt=0;chosen<0&&attempt<count&&total>0;attempt++)
            {
                double pick=random.NextDouble()*total;int candidate=-1;
                for(int i=0;i<count;i++){if(weights[i]<=0)continue;candidate=i;pick-=weights[i];if(pick<0)break;}
                if(candidate<0)break;
                var at=atVolley[candidate]?own.Volley.Plan.PStar:own.Contact.PStar;
                var forecast=all[candidate].Side.Exists?RallyContact.ForecastBoast(context,at,all[candidate].Side,all[candidate].P):RallyContact.Forecast(context,at,all[candidate].Aim,all[candidate].P);
                if(forecast.Good){chosen=candidate;break;}
                total-=weights[candidate];weights[candidate]=0;
            }
            int index=chosen>=0?chosen:fallback;var option=all[index];bool useVolley=atVolley[index];
            // The same two aim draws for every line; a boast spreads them over its side-wall point (z, y).
            var noise=new Vector2((float)random.NextGaussian(difficulty.AimSigma),(float)random.NextGaussian(difficulty.AimSigma));
            var side=option.Side.Exists?ShotOptions.ClampSide(context,option.Side.With(option.Side.Z+noise.x,option.Side.Y+noise.y)):default;
            var aim=option.Side.Exists?side.UV:ShotOptions.ClampAim(context,option.Aim+noise);
            float power=Mathf.Clamp01(option.P+(float)random.NextGaussian(difficulty.PowerSigma));
            double delta=random.NextGaussian(difficulty.TimingSigmaMs);
            var plan=useVolley?own.Volley.Plan:own.Contact;
            double rate=context.StepHz*context.Tempo;
            double lower=(plan.KIn.Steps-plan.TStarStep)*1000/rate;
            double upper=(plan.KOut.Steps-plan.TStarStep)*1000/rate;
            if(context.Timing.ValidLimitOn)
            {lower=Math.Max(lower,-context.Timing.ValidMs);upper=Math.Min(upper,context.Timing.ValidMs);}
            delta=RivalReleaseTiming.Adjust(delta,lower,upper,difficulty.TimingSigmaMs,p.FondoReleaseMarginSigma);
            return new AiShot(true,option,aim,power,delta,defensive,useVolley,side);
        }
        // Public evaluation seam for B3d. This prototype retains the old direct forecast until its RED is recorded.
        public StretchedEvaluation[] EvaluateStretched(in AiObservation observation,in ReceptionPlan own)
        {
            var result=new List<StretchedEvaluation>(12);var opponentPosition=observation.OpponentPosition;
            void Add(Vector3 at)
            {
                var options=ShotOptions.Build(context,at);
                if(context.BoastOn)ShotOptions.AddBoasts(context,at,options);
                foreach(var option in options)
                {
                    bool allowed=option.Line==ShotLine.Boast||
                        (option.Height==ShotHeight.High&&option.Power==ShotPower.Medium&&(option.Line==ShotLine.Straight||option.Line==ShotLine.Cross));
                    if(!allowed)continue;
                    var forecast=RallyContact.Forecast(context,at,option.Aim,option.P);
                    bool reachable=false;long delay=long.MaxValue;
                    if(forecast.Good)
                    {
                        var solution=StretchedIntention(at,option);
                        var trajectory=context.Predict(new BallState{Position=at,Velocity=solution.Velocity});
                        var rules=context.NewRules();rules.Begin(PlayerId.Rival,PlayerId.Human);
                        var opponent=new PlayerBody(PlayerId.Human,opponentPosition,context.Move);
                        var plan=ReceptionPlanner.Plan(context,trajectory,rules,opponent,context.Steps(context.Params.ReactionS),1);
                        long until=plan.Contact.Exists&&plan.Contact.TStarStep>=0?plan.Contact.TStarStep:trajectory.States[trajectory.States.Count-1].Step;
                        reachable=plan.Reachable;delay=plan.ArrivalStep-until;
                    }
                    result.Add(new StretchedEvaluation(forecast.Good,reachable,delay,option.Line==ShotLine.Boast));
                }
            }
            if(own.Reachable&&own.Contact.Exists&&own.Contact.TStarStep>=0)Add(own.Contact.PStar);
            if(VolleyWanted(own))Add(own.Volley.Plan.PStar);
            return result.ToArray();
        }
        // Only stretched receptions are filtered. The ordinary policy and its random draws stay untouched.
        private int FilterStretched(int count,in AiObservation observation,in ReceptionPlan own,out double best)
        {
            best=double.NegativeInfinity;int firstAllowed=-1,leastLate=-1;long smallestDelay=long.MaxValue;bool reachable=false;
            for(int i=0;i<count;i++)
            {
                if(double.IsNegativeInfinity(weights[i]))continue;
                if(firstAllowed<0)firstAllowed=i;
                var option=all[i];var at=atVolley[i]?own.Volley.Plan.PStar:own.Contact.PStar;
                if(RallyContact.Forecast(context,at,option.Aim,option.P).Good)
                {
                    // Forecast supplies the fault guard; the same intention supplies the full shared prediction.
                    var solution=StretchedIntention(at,option);
                    var trajectory=context.Predict(new BallState{Position=at,Velocity=solution.Velocity});
                    var rules=context.NewRules();rules.Begin(PlayerId.Rival,PlayerId.Human);
                    var opponent=new PlayerBody(PlayerId.Human,observation.OpponentPosition,context.Move);
                    var plan=ReceptionPlanner.Plan(context,trajectory,rules,opponent,context.Steps(context.Params.ReactionS),1);
                    // With no contact window, lateness is measured against the end of that predicted bounce flight.
                    long until=plan.Contact.Exists&&plan.Contact.TStarStep>=0?plan.Contact.TStarStep:trajectory.States[trajectory.States.Count-1].Step;
                    long delay=plan.ArrivalStep-until;
                    if(leastLate<0||delay<smallestDelay){leastLate=i;smallestDelay=delay;}
                    if(plan.Reachable){reachable=true;if(weights[i]>best)best=weights[i];continue;}
                }
                weights[i]=double.NegativeInfinity;
            }
            // No safe option: retain a defensive intention and flag the exception instead of drawing a winner at random.
            return reachable?-1:leastLate>=0?leastLate:firstAllowed;
        }
        // Existing direct intention, extracted without a policy change before the path-B RED.
        private ShotSolution StretchedIntention(Vector3 at,ShotOption option)
            =>RallyContact.SolveIntention(context,at,option.Aim,option.P);
        // FondoAi.Utility plus the style's attack feature and drop zone. For Fondista both are zero/closed, so the value is FondoAi's.
        public double Utility(in ShotOption option,in AiObservation observation,in ReceptionPlan own,Vector3 at)
        {
            double away=Math.Min(1,Vector2.Distance(observation.OpponentPosition,option.Zone)/6);
            double need=Math.Min(1,Vector2.Distance(observation.SelfPosition,context.T)/4);
            double ahead=Math.Min(1,Math.Max(0,(context.T.y-at.z)/2));
            double time,risk;
            if(option.Line==ShotLine.Boast)
            {
                // Two-wall shot: away from the opponent (front opposite corner), little time, more risk when low or strong.
                risk=.35+(option.Height==ShotHeight.Low?.2:0)+(option.Power==ShotPower.Strong?.1:0);
                return Style.BoastWeight+Style.AwayWeight*away+Style.TimeWeight*.4*need-Style.RiskAversion*risk;
            }
            if(option.Line==ShotLine.Drop)
            {
                bool open=(observation.OpponentPosition.y>context.T.y+.5f&&own.SlackS>=Style.DropMinSlackS)||at.z<=context.T.y+Style.DropZoneAhead;
                time=.5;risk=.55+(option.Height==ShotHeight.Low?.2:0)+(option.Power==ShotPower.Strong?.1:0);
                double drop=(open?Style.DropWeight:-4)+Style.AwayWeight*away+Style.TimeWeight*time*need-Style.RiskAversion*risk;
                return Style.AttackBase==0&&Style.AttackAhead==0?drop:drop+Style.AttackBase+Style.AttackAhead*ahead;
            }
            time=(option.Height==ShotHeight.High?1:option.Height==ShotHeight.Mid?.6:.3)*(option.Power==ShotPower.Strong?.7:1);
            risk=option.Height==ShotHeight.Low?.35+.3*(1-own.Quality):option.Height==ShotHeight.Mid?.1:(option.Power==ShotPower.Strong?.45:.15);
            double length=option.Long?1:.6;
            double u=Style.LongWeight*length+Style.AwayWeight*away+Style.TimeWeight*time*need-Style.RiskAversion*risk;
            return option.Height==ShotHeight.Low&&(Style.AttackBase!=0||Style.AttackAhead!=0)?u+Style.AttackBase+Style.AttackAhead*ahead:u;
        }
    }
}
