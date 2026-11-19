using System;
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
        public RivalStyle(RivalStyleId id,double longW,double awayW,double timeW,double dropW,double risk,double defense,double dropSlack,double dropZone,
            double attackBase,double attackAhead,double volleyBias,double volleyMinK,double volleySteal,double wait,double cover)
        {Id=id;LongWeight=longW;AwayWeight=awayW;TimeWeight=timeW;DropWeight=dropW;RiskAversion=risk;DefenseSlackS=defense;DropMinSlackS=dropSlack;DropZoneAhead=dropZone;
         AttackBase=attackBase;AttackAhead=attackAhead;VolleyBias=volleyBias;VolleyMinKS=volleyMinK;VolleySteal=volleySteal;WaitBehindT=wait;CoverDepth=cover;}
        public static RivalStyle For(RallyParams p,RivalStyleId id)
        {
            switch(id)
            {
                case RivalStyleId.Definidor:return new RivalStyle(id,p.StyleShooterLongWeight,p.StyleShooterAwayWeight,p.StyleShooterTimeWeight,p.StyleShooterDropWeight,p.StyleShooterRiskAversion,
                    p.StyleShooterDefenseSlackS,p.StyleShooterDropMinSlackS,p.StyleShooterDropZoneAhead,p.StyleShooterAttackBase,p.StyleShooterAttackAhead,
                    p.StyleShooterVolleyBias,p.StyleShooterVolleyMinKS,p.StyleShooterVolleySteal,p.StyleShooterWaitBehindT,p.StyleShooterCoverDepth);
                case RivalStyleId.Completo:return new RivalStyle(id,p.StyleAllRoundLongWeight,p.StyleAllRoundAwayWeight,p.StyleAllRoundTimeWeight,p.StyleAllRoundDropWeight,p.StyleAllRoundRiskAversion,
                    p.StyleAllRoundDefenseSlackS,p.StyleAllRoundDropMinSlackS,p.StyleAllRoundDropZoneAhead,p.StyleAllRoundAttackBase,p.StyleAllRoundAttackAhead,
                    p.StyleAllRoundVolleyBias,p.StyleAllRoundVolleyMinKS,p.StyleAllRoundVolleySteal,p.StyleAllRoundWaitBehindT,p.StyleAllRoundCoverDepth);
                default:return new RivalStyle(id,p.FondoLongWeight,p.FondoAwayWeight,p.FondoTimeWeight,p.FondoDropWeight,p.FondoRiskAversion,
                    p.FondoDefenseSlackS,p.FondoDropMinSlackS,double.NegativeInfinity,0,0,p.StyleFondoVolleyBias,p.StyleFondoVolleyMinKS,0,p.RivalWaitBehindT,p.StyleFondoCoverDepth);
            }
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
    public sealed class RivalAi : IRallyBrain,IRallyPositioning
    {
        private readonly RallyContext context;
        private readonly AiDifficulty difficulty;
        private readonly RallyRng random;
        private readonly double[] weights=new double[36];
        private readonly ShotOption[] all=new ShotOption[36];
        private readonly bool[] atVolley=new bool[36];
        public RivalStyle Style {get;}
        public RivalAi(RallyContext context,AiDifficulty difficulty,RivalStyleId style,long seed)
        {this.context=context??throw new ArgumentNullException(nameof(context));this.difficulty=difficulty;Style=RivalStyle.For(context.Params,style);random=new RallyRng(seed);}
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
        public bool VolleyWanted(in ReceptionPlan own)
        {
            if(context.Params.StyleRivalVolley==0||!own.Volley.Exists)return false;
            var v=own.Volley.Plan;return (v.KOut.Steps-v.KIn.Steps)/context.StepHz>=Style.VolleyMinKS;
        }
        public AiShot Decide(in AiObservation observation,in ReceptionPlan own)
        {
            bool bounce=own.Reachable&&own.Contact.Exists&&own.Contact.TStarStep>=0;
            bool volley=VolleyWanted(own);
            if(!bounce&&!volley)return default;
            var p=context.Params;bool defensive=own.SlackS<Style.DefenseSlackS;
            int count=0;double best=double.NegativeInfinity;var seen=observation;var mine=own;
            void Add(Vector3 at,bool isVolley,double bonus)
            {
                var options=ShotOptions.Build(context,at);
                for(int i=0;i<options.Count;i++)
                {
                    var o=options[i];bool allowed=!defensive||(o.Height==ShotHeight.High&&o.Power==ShotPower.Medium&&o.Line!=ShotLine.Drop);
                    all[count]=o;atVolley[count]=isVolley;
                    weights[count]=allowed?Utility(o,seen,mine,at)+bonus:double.NegativeInfinity;
                    if(weights[count]>best)best=weights[count];
                    count++;
                }
            }
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
            double total=0,temperature=Math.Max(1e-3,difficulty.Temperature);
            for(int i=0;i<count;i++){weights[i]=double.IsNegativeInfinity(weights[i])?0:Math.Exp((weights[i]-best)/temperature);total+=weights[i];}
            // Same draw as FondoAi: an option the AI itself predicts as a fault is dropped and the draw repeats.
            int chosen=-1,fallback=0;
            for(int i=1;i<count;i++)if(weights[i]>weights[fallback])fallback=i;
            for(int attempt=0;attempt<count&&total>0;attempt++)
            {
                double pick=random.NextDouble()*total;int candidate=-1;
                for(int i=0;i<count;i++){if(weights[i]<=0)continue;candidate=i;pick-=weights[i];if(pick<0)break;}
                if(candidate<0)break;
                var at=atVolley[candidate]?own.Volley.Plan.PStar:own.Contact.PStar;
                if(RallyContact.Forecast(context,at,all[candidate].Aim,all[candidate].P).Good){chosen=candidate;break;}
                total-=weights[candidate];weights[candidate]=0;
            }
            int index=chosen>=0?chosen:fallback;var option=all[index];bool useVolley=atVolley[index];
            var aim=ShotOptions.ClampAim(context,option.Aim+new Vector2((float)random.NextGaussian(difficulty.AimSigma),(float)random.NextGaussian(difficulty.AimSigma)));
            float power=Mathf.Clamp01(option.P+(float)random.NextGaussian(difficulty.PowerSigma));
            double delta=random.NextGaussian(difficulty.TimingSigmaMs);
            var plan=useVolley?own.Volley.Plan:own.Contact;
            double rate=context.StepHz*context.Tempo;
            double lower=(plan.KIn.Steps-plan.TStarStep)*1000/rate;
            double upper=(plan.KOut.Steps-plan.TStarStep)*1000/rate;
            if(context.Timing.ValidLimitOn)
            {lower=Math.Max(lower,-context.Timing.ValidMs);upper=Math.Min(upper,context.Timing.ValidMs);}
            delta=RivalReleaseTiming.Adjust(delta,lower,upper,difficulty.TimingSigmaMs,p.FondoReleaseMarginSigma);
            return new AiShot(true,option,aim,power,delta,defensive,useVolley);
        }
        // FondoAi.Utility plus the style's attack feature and drop zone. For Fondista both are zero/closed, so the value is FondoAi's.
        public double Utility(in ShotOption option,in AiObservation observation,in ReceptionPlan own,Vector3 at)
        {
            double away=Math.Min(1,Vector2.Distance(observation.OpponentPosition,option.Zone)/6);
            double need=Math.Min(1,Vector2.Distance(observation.SelfPosition,context.T)/4);
            double ahead=Math.Min(1,Math.Max(0,(context.T.y-at.z)/2));
            double time,risk;
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
