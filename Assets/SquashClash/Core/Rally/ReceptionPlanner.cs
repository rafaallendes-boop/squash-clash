using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    public readonly struct ReceptionPlan
    {
        public readonly bool Reachable;
        // Stance: where the body runs to. BodyAtContact: where it stands when the ball arrives (equal when reachable).
        public readonly Vector2 Stance,BodyAtContact;
        public readonly ContactPlan Contact;
        public readonly long ArrivalStep;
        public readonly double SlackS;
        // Arrival quality q in [0,1] (spec 9.1): the "position" source for later error layers.
        public readonly float Quality;
        public readonly string Reason;
        public ReceptionPlan(bool reachable,Vector2 stance,Vector2 bodyAtContact,ContactPlan contact,long arrivalStep,double slack,float quality,string reason)
        {Reachable=reachable;Stance=stance;BodyAtContact=bodyAtContact;Contact=contact;ArrivalStep=arrivalStep;SlackS=slack;Quality=quality;Reason=reason;}
    }
    // Contact planner for a moving character (spec 9.1). It picks where to stand; the existing ContactPlanner,
    // untouched, then decides K, T* and P* for a body standing there. It never picks a volley and never swings.
    public static class ReceptionPlanner
    {
        private readonly struct Candidate
        {
            public readonly long Step;public readonly Vector2 Stance;public readonly int Arrival;public readonly float Score;
            public Candidate(long step,Vector2 stance,int arrival,float score){Step=step;Stance=stance;Arrival=arrival;Score=score;}
        }
        public static ReceptionPlan Plan(RallyContext context,Trajectory trajectory,PointRules rulesAtStart,PlayerBody body,int delaySteps,int version,Vector2 stanceNoise=default)
        {
            if(context==null||trajectory==null||rulesAtStart==null||body==null)throw new ArgumentNullException(nameof(context));
            var p=context.Params;var legal=context.Legal;var court=context.Court;var frame=context.FrameAt(body.Position);
            ReceptionPlan None(string reason)=>new ReceptionPlan(false,body.Position,body.Position,
                new ContactPlan(false,default,default,0,default,default,version,frame,legal,court,reason),0,0,0,reason);
            if(trajectory.States.Count<2)return None("no-trajectory");
            long start=trajectory.States[0].Step;var who=rulesAtStart.ToHit;var rules=rulesAtStart.Clone();
            int interval=Math.Max(1,context.Steps(p.CandidateIntervalS)),next=0;
            var reachable=new List<Candidate>();Candidate? closest=null;int closestLate=int.MaxValue;bool window=false;
            for(int i=1;i<trajectory.States.Count&&!rules.Closed;i++)
            {
                var state=trajectory.States[i];
                while(next<trajectory.Contacts.Count&&trajectory.Contacts[next].Step<=state.Step)rules.OnBallContact(trajectory.Contacts[next++]);
                if(rules.Closed||!rules.CanHit(who)||state.Position.y<legal.MinHeight||state.Position.y>legal.MaxHeight)continue;
                window=true;
                if((state.Step-start)%interval!=0)continue;
                int available=(int)(state.Step-start);
                foreach(float side in Sides)
                {
                    var stance=context.ClampBody(new Vector2(state.Position.x+side*frame.IdealLateral,state.Position.z)+stanceNoise);
                    int arrival=body.StepsToReach(stance,delaySteps,out _,available+context.StepHz);
                    if(arrival<=available)reachable.Add(new Candidate(state.Step,stance,arrival,Score(context,frame,state.Position,(available-arrival)/(double)context.StepHz)));
                    else
                    {
                        int late=arrival==int.MaxValue?int.MaxValue-1:arrival-available;
                        if(late<closestLate){closestLate=late;closest=new Candidate(state.Step,stance,arrival,0);}
                    }
                }
            }
            if(!window)return None("no-legal-window");
            reachable.Sort((a,b)=>b.Score!=a.Score?b.Score.CompareTo(a.Score):a.Step!=b.Step?a.Step.CompareTo(b.Step):a.Stance.x.CompareTo(b.Stance.x));
            int attempts=Math.Min(reachable.Count,Math.Max(1,(int)p.PlanAttempts));
            for(int i=0;i<attempts;i++)
            {
                var c=reachable[i];var contact=ContactPlanner.Plan(trajectory,context.FrameAt(c.Stance),court,legal,version);
                long arrivalStep=start+c.Arrival;
                if(!contact.Exists||contact.TStarStep<0||arrivalStep>contact.TStarStep)continue;
                double slack=(contact.TStarStep-arrivalStep)/(double)context.StepHz;
                return new ReceptionPlan(true,c.Stance,c.Stance,contact,arrivalStep,slack,Mathf.Clamp01((float)(slack/p.SlackCapS)),"planned");
            }
            // Unreachable (contract case 7): run at the least-late candidate anyway. The contract then sees the body
            // where it really is when the ball passes; if the ball is within reach from there, that honest chance stays.
            var target=closest??reachable[0];
            var at=body.PositionAfter(target.Stance,delaySteps,(int)(target.Step-start));
            var effort=ContactPlanner.Plan(trajectory,context.FrameAt(at),court,legal,version);
            bool chance=effort.Exists&&effort.TStarStep>=0;
            return new ReceptionPlan(chance,target.Stance,at,effort,start+Math.Min(target.Arrival,int.MaxValue/2),0,0,chance?"stretched":"unreachable");
        }
        private static readonly float[] Sides={-1,1};
        private static float Score(RallyContext context,BodyFrame frame,Vector3 ball,double slack)
        {
            var p=context.Params;var court=context.Court;
            float gap=ball.y<frame.PreferredMinHeight?frame.PreferredMinHeight-ball.y:ball.y>frame.PreferredMaxHeight?ball.y-frame.PreferredMaxHeight:0;
            float height=Mathf.Max(0,1-gap/(frame.PreferredMaxHeight-frame.PreferredMinHeight));
            float clearance=Mathf.Min(Mathf.Min(ball.x,court.Width-ball.x),Mathf.Min(ball.z,court.Length-ball.z));
            float wall=frame.WallClearance<=0?1:Mathf.Clamp01(clearance/frame.WallClearance);
            return (float)(p.WeightSlack*Math.Min(slack,p.SlackCapS)/p.SlackCapS+p.WeightHeight*height+p.WeightWall*wall);
        }
    }
}
