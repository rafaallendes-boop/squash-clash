using System;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    // Volley v1 limits (design 2026-11-18). Heights in metres; separation in real perceived seconds; window in simulation seconds.
    public readonly struct VolleySettings
    {
        public readonly bool Enabled;public readonly float MinHeight,MaxHeight;public readonly double MinSeparationS,MinWindowS;
        public VolleySettings(bool enabled,float minHeight,float maxHeight,double minSeparationS,double minWindowS)
        {Enabled=enabled;MinHeight=minHeight;MaxHeight=maxHeight;MinSeparationS=minSeparationS;MinWindowS=minWindowS;}
    }
    // One volley opportunity of a reception: its own contact plan (fixed body, zero bounces) and the split instant S that
    // separates it from the bounce opportunity (spec 7.1.3 case 14). Reason says why there is none.
    public readonly struct VolleyOpportunity
    {
        public readonly ContactPlan Plan;public readonly double SplitSteps;public readonly string Reason;
        public VolleyOpportunity(ContactPlan plan,double splitSteps,string reason){Plan=plan;SplitSteps=splitSteps;Reason=reason;}
        public bool Exists=>Plan.Exists&&Plan.TStarStep>=0&&Reason=="offered";
        public static VolleyOpportunity None(string reason)=>new VolleyOpportunity(default,double.NegativeInfinity,reason);
        public VolleyOpportunity WithVersion(int version)
        {var p=Plan;return new VolleyOpportunity(new ContactPlan(p.Exists,p.KIn,p.KOut,p.TStarStep,p.PStar,p.Side,version,p.Body,p.Legal,p.Court,p.Reason),SplitSteps,Reason);}
    }
    // Finds the volley opportunity on the path the body already follows towards its bounce plan (M1, spec 7.3.3).
    // Pure: it reads the isolated prediction and the frozen body path, and never moves the ball or the body.
    public static class VolleyPlanner
    {
        public static VolleyOpportunity Plan(Trajectory t,Func<long,BodyFrame> bodyAt,CourtDimensions court,LegalSettings bounceLegal,
            VolleySettings settings,ContactPlan bounce,long earliestStep,double tempo,int version)
        {
            if(!settings.Enabled)return VolleyOpportunity.None("disabled");
            if(t==null||t.States.Count<2||bodyAt==null)return VolleyOpportunity.None("no-trajectory");
            var legal=new LegalSettings(settings.MinHeight,settings.MaxHeight,bounceLegal.StepHz,bounceLegal.Gravity,bounceLegal.Radius,court,0);
            long start=t.States[0].Step;var phase=ContactPlanner.PhaseLegal(t,bodyAt(start),legal);
            // The first run of steps where the ball, before its bounce, passes within reach of the moving body.
            int first=-1,last=-1;
            for(int i=0;i<phase.Length;i++)
            {
                long step=start+i;bool ok=phase[i]&&step>=earliestStep&&Within(t.States[i].Position,bodyAt(step));
                if(ok){if(first<0)first=i;last=i;}
                else if(first>=0)break;
            }
            if(first<0)return VolleyOpportunity.None("no-pass");
            int best=first;float score=float.NegativeInfinity;
            for(int i=first;i<=last;i++)
            {
                float value=Feature(bodyAt(start+i),court,t.States[i].Position);
                if(value>score){score=value;best=i;}
            }
            var plan=ContactPlanner.Plan(t,bodyAt(start+best),court,legal,version);
            if(!plan.Exists||plan.TStarStep<0||plan.TStarStep<start+first||plan.TStarStep>start+last||plan.KIn.Steps<earliestStep)
                return VolleyOpportunity.None("fixed-frame");
            if((plan.KOut.Steps-plan.KIn.Steps)/legal.StepHz<settings.MinWindowS)return VolleyOpportunity.None("window");
            double split=double.PositiveInfinity;
            if(bounce.Exists&&bounce.TStarStep>=0)
            {
                double gap=(bounce.KIn.Steps-plan.KOut.Steps)/(legal.StepHz*tempo);
                if(gap<settings.MinSeparationS)return VolleyOpportunity.None("separation");
                split=(plan.KOut.Steps+bounce.KIn.Steps)*.5;
            }
            return new VolleyOpportunity(plan,split,"offered");
        }
        private static bool Within(Vector3 ball,BodyFrame body)
        {
            double dx=ball.x-body.PositionXZ.x,dy=ball.y-body.ReferenceHeight,dz=ball.z-body.PositionXZ.y;
            return dx*dx+dy*dy+dz*dz<=(double)body.Reach*body.Reach;
        }
        // The same preference ContactPlanner uses to pick T* inside a range.
        private static float Feature(BodyFrame body,CourtDimensions court,Vector3 ball)
        {
            float gap=ball.y<body.PreferredMinHeight?body.PreferredMinHeight-ball.y:ball.y>body.PreferredMaxHeight?ball.y-body.PreferredMaxHeight:0;
            float preferred=Mathf.Max(0,1-gap/(body.PreferredMaxHeight-body.PreferredMinHeight));
            float clearance=Mathf.Min(ball.x,court.Width-ball.x,Mathf.Min(ball.z,court.Length-ball.z));
            return preferred+(body.WallClearance<=0?1:Mathf.Clamp01(clearance/body.WallClearance));
        }
    }
}
