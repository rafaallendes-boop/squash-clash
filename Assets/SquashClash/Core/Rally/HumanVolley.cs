using System;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    // A chosen-volley intercept (volley v2): the fixed-frame contact plan (zero bounces) at the stance the body runs to.
    public readonly struct VolleyIntercept
    {
        public readonly ContactPlan Plan;public readonly Vector2 Stance;public readonly string Reason;
        public VolleyIntercept(ContactPlan plan,Vector2 stance,string reason){Plan=plan;Stance=stance;Reason=reason;}
        public bool Exists=>Reason=="offered"&&Plan.Exists&&Plan.TStarStep>=0;
        public static VolleyIntercept None(string reason)=>new VolleyIntercept(default,default,reason);
    }
    // Volley v2, human only (design 2026-11-20). The player chooses the volley: a touch while the amber ring shows sends the body to an
    // intercept stance (V3, W chosen by the player, so D24 holds), and the bounce of that reception is given up. Without that touch the
    // body keeps its M1 path and nothing changes. Pure: reads the isolated prediction and copies of the body, never moves either.
    public readonly struct VolleyOfferDetail
    {
        public readonly VolleyOpportunity Offer;public readonly VolleyIntercept Intercept;public readonly long S,SPrime;
        public VolleyOfferDetail(VolleyOpportunity offer,VolleyIntercept intercept,long s,long sPrime){Offer=offer;Intercept=intercept;S=s;SPrime=sPrime;}
        public static VolleyOfferDetail None(string reason)=>new VolleyOfferDetail(VolleyOpportunity.None(reason),VolleyIntercept.None(reason),-1,-1);
    }
    public static class HumanVolley
    {
        // The amber ring shows from its presentation at the launch (ImpactCue keeps it at its outer radius until the last ApproachMs, then it
        // converges), so a touch may choose the volley from the launch on.
        public static double AmberFromSteps(RallyContext context,long startStep,long tStarStep)=>startStep;
        // The offer shown at launch. A touch can only choose it once the amber ring shows, so each candidate must be reachable by a body that
        // follows its M1 path until that moment and only then turns (today: the launch itself); S is the last step a touch still reaches it.
        // v2.1: S' is the last touch a sprint still converts; the amber ring (SplitSteps) lasts until S'.
        public static VolleyOpportunity Offer(RallyContext context,Trajectory trajectory,PointRules rulesAtStart,PlayerBody bodyAtStart,int delaySteps,int version)
            =>OfferDetail(context,trajectory,rulesAtStart,bodyAtStart,delaySteps,version).Offer;
        public static VolleyOfferDetail OfferDetail(RallyContext context,Trajectory trajectory,PointRules rulesAtStart,PlayerBody bodyAtStart,int delaySteps,int version)
        {
            if(context==null||bodyAtStart==null)throw new ArgumentNullException(nameof(context));
            if(trajectory==null||trajectory.States.Count<2)return VolleyOfferDetail.None("no-trajectory");
            long start=trajectory.States[0].Step;var path=new System.Collections.Generic.List<PlayerBody>();var walk=bodyAtStart.Clone();
            PlayerBody At(long step){while(path.Count<=step-start){path.Add(walk.Clone());walk.Step();}return path[(int)(step-start)];}
            var p=Intercept(context,trajectory,rulesAtStart,start+delaySteps,start+delaySteps,version,plan=>
            {long c=Math.Max(start,(long)Math.Ceiling(AmberFromSteps(context,start,plan.TStarStep)));return (At(c),c);});
            if(!p.Exists)return VolleyOfferDetail.None(p.Reason);
            long deadline=Deadline(context,bodyAtStart,start,delaySteps,p);
            if(deadline<AmberFromSteps(context,start,p.Plan.TStarStep))return VolleyOfferDetail.None("commit-window");
            float sprint=context.HumanVolleySprint;
            long late=sprint>1?Math.Max(deadline,SprintDeadline(context,bodyAtStart,start,delaySteps,p,sprint)):deadline;
            return new VolleyOfferDetail(new VolleyOpportunity(p.Plan,late,"offered"),p,deadline,late);
        }
        // The touch at bodyStep: the intercept recomputed from the body as it really is now (the reaction delay still applies), running at
        // the given sprint factor from the touch on (1: the normal body).
        public static VolleyIntercept Commit(RallyContext context,Trajectory trajectory,PointRules rulesAtStart,PlayerBody bodyNow,long bodyStep,int delaySteps,int version,float sprint=1)
        {
            if(context==null||bodyNow==null)throw new ArgumentNullException(nameof(context));
            if(trajectory==null||trajectory.States.Count<2)return VolleyIntercept.None("no-trajectory");
            long start=trajectory.States[0].Step;var body=bodyNow;
            if(sprint>1){body=bodyNow.Clone();body.Sprint=sprint;}
            // Candidates from launch + reaction, like the offer (VOLLEY-V2-COMMIT-01); Intercept keeps only those whose T* is still ahead.
            return Intercept(context,trajectory,rulesAtStart,start+delaySteps,start+delaySteps,version,_=>(body,bodyStep));
        }
        // How late a sprint touch is: 0 at S, 1 at S' (clamped).
        public static double Lateness(long s,long sPrime,long touchStep)=>sPrime<=s?(touchStep>s?1:0):Math.Max(0,Math.Min(1,(touchStep-s)/(double)(sPrime-s)));
        // The last touch step at which the body, following its M1 path until then and sprinting from then on, still reaches the stance by T*.
        public static long SprintDeadline(RallyContext context,PlayerBody bodyAtStart,long startStep,int delaySteps,in VolleyIntercept p,float sprint)
        {
            var copy=bodyAtStart.Clone();long last=startStep-1;long star=p.Plan.TStarStep;
            for(long c=startStep;c<=star;c++)
            {
                var runner=copy.Clone();runner.Sprint=sprint;
                int remaining=(int)Math.Max(0,startStep+delaySteps-c);
                int arrival=runner.StepsToReach(p.Stance,remaining,out _,(int)(star-c)+1);
                if(arrival!=int.MaxValue&&c+arrival<=star)last=c;
                copy.Step();
            }
            return last;
        }
        // The stance with the longest fixed-frame volley window among those reachable before its T* by the body departing as departure says
        // (the reaction delay counted from the reception start still applies to a departure inside it).
        public static VolleyIntercept Intercept(RallyContext context,Trajectory t,PointRules rulesAtStart,long fromStep,long earliestStep,int version,
            Func<ContactPlan,(PlayerBody body,long step)> departure)
        {
            if(!context.HumanVolleyV2)return VolleyIntercept.None("disabled");
            if(rulesAtStart!=null&&rulesAtStart.Serving&&rulesAtStart.ServeVolley!=PlayerId.Human)return VolleyIntercept.None("serve");
            var p=context.Params;var l=context.Legal;
            var legal=new LegalSettings((float)p.VolleyMinHeight,(float)p.VolleyMaxHeight,l.StepHz,l.Gravity,l.Radius,context.Court,0);
            var frame=context.FrameAt(context.T);var phase=ContactPlanner.PhaseLegal(t,frame,legal);
            long start=t.States[0].Step;int interval=Math.Max(1,context.Steps(p.CandidateIntervalS));
            double minWindow=p.HumanVolleyMinWindowS*context.StepHz;
            var best=VolleyIntercept.None("no-intercept");double bestK=double.NegativeInfinity;
            for(int i=(int)Math.Max(0,fromStep-start);i<phase.Length;i++)
            {
                if(!phase[i]||i%interval!=0)continue;var ball=t.States[i].Position;
                for(int s=-1;s<=1;s+=2)
                {
                    var stance=context.ClampBody(new Vector2(ball.x+s*frame.IdealLateral,ball.z));
                    var plan=ContactPlanner.Plan(t,context.FrameAt(stance),context.Court,legal,version);
                    if(!plan.Exists||plan.TStarStep<0||plan.KIn.Steps<earliestStep)continue;
                    double k=plan.KOut.Steps-plan.KIn.Steps;if(k<minWindow||k<=bestK)continue;
                    var (body,step)=departure(plan);if(plan.TStarStep<step)continue;
                    int remaining=(int)Math.Max(0,earliestStep-step);
                    int arrival=body.StepsToReach(stance,remaining,out _,(int)(plan.TStarStep-step)+1);
                    if(arrival==int.MaxValue||step+arrival>plan.TStarStep)continue;
                    bestK=k;best=new VolleyIntercept(plan,stance,"offered");
                }
            }
            return best;
        }
        // The last step at which a touch still sends the body (following its M1 path until then) to the stance by T*.
        public static long Deadline(RallyContext context,PlayerBody bodyAtStart,long startStep,int delaySteps,in VolleyIntercept p)
        {
            var copy=bodyAtStart.Clone();long last=startStep-1;long star=p.Plan.TStarStep;
            for(long c=startStep;c<=star;c++)
            {
                int remaining=(int)Math.Max(0,startStep+delaySteps-c);
                int arrival=copy.StepsToReach(p.Stance,remaining,out _,(int)(star-c)+1);
                if(arrival==int.MaxValue||c+arrival>star)break;
                last=c;copy.Step();
            }
            return last;
        }
    }
}
