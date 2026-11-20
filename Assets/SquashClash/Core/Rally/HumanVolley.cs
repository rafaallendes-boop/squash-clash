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
    public static class HumanVolley
    {
        // The amber ring is visible from max(launch, T*v - the cue approach) on: the first step a touch may choose the volley.
        public static double AmberFromSteps(RallyContext context,long startStep,long tStarStep)
            =>Math.Max(startStep,tStarStep-ImpactCue.ApproachMs/1000*context.StepHz*context.Tempo);
        // The offer shown at launch: the intercept from the body as it starts its reception, with S = the last step a touch still reaches it.
        public static VolleyOpportunity Offer(RallyContext context,Trajectory trajectory,PointRules rulesAtStart,PlayerBody bodyAtStart,int delaySteps,int version)
        {
            if(context==null||bodyAtStart==null)throw new ArgumentNullException(nameof(context));
            if(trajectory==null||trajectory.States.Count<2)return VolleyOpportunity.None("no-trajectory");
            long start=trajectory.States[0].Step;
            var p=Intercept(context,trajectory,rulesAtStart,bodyAtStart,start,delaySteps,start+delaySteps,version);
            if(!p.Exists)return VolleyOpportunity.None(p.Reason);
            long deadline=Deadline(context,bodyAtStart,start,delaySteps,p);
            if(deadline<AmberFromSteps(context,start,p.Plan.TStarStep))return VolleyOpportunity.None("commit-window");
            return new VolleyOpportunity(p.Plan,deadline,"offered");
        }
        // The touch at bodyStep: the intercept recomputed from the body as it really is now (the reaction delay still applies).
        public static VolleyIntercept Commit(RallyContext context,Trajectory trajectory,PointRules rulesAtStart,PlayerBody bodyNow,long bodyStep,int delaySteps,int version)
        {
            if(context==null||bodyNow==null)throw new ArgumentNullException(nameof(context));
            if(trajectory==null||trajectory.States.Count<2)return VolleyIntercept.None("no-trajectory");
            long start=trajectory.States[0].Step;int remaining=(int)Math.Max(0,start+delaySteps-bodyStep);
            return Intercept(context,trajectory,rulesAtStart,bodyNow,bodyStep,remaining,start+delaySteps,version);
        }
        // The stance with the longest fixed-frame volley window among those the body reaches before its T*.
        public static VolleyIntercept Intercept(RallyContext context,Trajectory t,PointRules rulesAtStart,PlayerBody body,long bodyStep,int delaySteps,long earliestStep,int version)
        {
            if(!context.HumanVolleyV2)return VolleyIntercept.None("disabled");
            if(rulesAtStart!=null&&rulesAtStart.Serving&&rulesAtStart.ServeVolley!=PlayerId.Human)return VolleyIntercept.None("serve");
            var p=context.Params;var l=context.Legal;
            var legal=new LegalSettings((float)p.VolleyMinHeight,(float)p.VolleyMaxHeight,l.StepHz,l.Gravity,l.Radius,context.Court,0);
            var frame=context.FrameAt(body.Position);var phase=ContactPlanner.PhaseLegal(t,frame,legal);
            long start=t.States[0].Step,from=Math.Max(earliestStep,bodyStep);int interval=Math.Max(1,context.Steps(p.CandidateIntervalS));
            double minWindow=p.HumanVolleyMinWindowS*context.StepHz;
            var best=VolleyIntercept.None("no-intercept");double bestK=double.NegativeInfinity;
            for(int i=(int)Math.Max(0,from-start);i<phase.Length;i++)
            {
                if(!phase[i]||i%interval!=0)continue;var ball=t.States[i].Position;
                for(int s=-1;s<=1;s+=2)
                {
                    var stance=context.ClampBody(new Vector2(ball.x+s*frame.IdealLateral,ball.z));
                    var plan=ContactPlanner.Plan(t,context.FrameAt(stance),context.Court,legal,version);
                    if(!plan.Exists||plan.TStarStep<0||plan.KIn.Steps<earliestStep||plan.TStarStep<bodyStep)continue;
                    double k=plan.KOut.Steps-plan.KIn.Steps;if(k<minWindow||k<=bestK)continue;
                    int arrival=body.StepsToReach(stance,delaySteps,out _,(int)(plan.TStarStep-bodyStep)+1);
                    if(arrival==int.MaxValue||bodyStep+arrival>plan.TStarStep)continue;
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
