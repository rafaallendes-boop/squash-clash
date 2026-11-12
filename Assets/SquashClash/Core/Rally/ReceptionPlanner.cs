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
        private readonly struct Sample
        {
            public readonly long Step;public readonly Vector3 Position;public readonly float Feature;
            public Sample(long step,Vector3 position,float feature){Step=step;Position=position;Feature=feature;}
        }
        // elapsedSteps: steps already played since the trajectory start (the present can be past the opponent's contact).
        public static ReceptionPlan Plan(RallyContext context,Trajectory trajectory,PointRules rulesAtStart,PlayerBody body,int delaySteps,int version,Vector2 stanceNoise=default,int elapsedSteps=0)
        {
            if(context==null||trajectory==null||rulesAtStart==null||body==null)throw new ArgumentNullException(nameof(context));
            var p=context.Params;var legal=context.Legal;var court=context.Court;var frame=context.FrameAt(body.Position);
            ReceptionPlan None(string reason)=>new ReceptionPlan(false,body.Position,body.Position,
                new ContactPlan(false,default,default,0,default,default,version,frame,legal,court,reason),0,0,0,reason);
            if(trajectory.States.Count<2)return None("no-trajectory");
            long start=trajectory.States[0].Step;var who=rulesAtStart.ToHit;var rules=rulesAtStart.Clone();int next=0;
            // Legal window L (spec 7.1.1): after the front wall, one bounce, legal height. Every step, in order.
            var window=new List<Sample>(256);
            for(int i=1;i<trajectory.States.Count&&!rules.Closed;i++)
            {
                var state=trajectory.States[i];
                while(next<trajectory.Contacts.Count&&trajectory.Contacts[next].Step<=state.Step)rules.OnBallContact(trajectory.Contacts[next++]);
                if(rules.Closed||!rules.CanHit(who)||state.Position.y<legal.MinHeight||state.Position.y>legal.MaxHeight)continue;
                window.Add(new Sample(state.Step,state.Position,Feature(frame,court,state.Position)));
            }
            if(window.Count==0)return None("no-legal-window");
            int interval=Math.Max(1,context.Steps(p.CandidateIntervalS));float reach2=frame.Reach*frame.Reach;
            float marginCap=Math.Max(1,context.Steps(p.WindowMarginCapS));
            var reachable=new List<Candidate>();Candidate? closest=null;int closestLate=int.MaxValue;
            for(int index=0;index<window.Count;index++)
            {
                var sample=window[index];
                if((sample.Step-start)%interval!=0)continue;
                int available=(int)(sample.Step-start)-elapsedSteps;
                if(available<0)continue;
                foreach(float side in Sides)
                {
                    var stance=context.ClampBody(new Vector2(sample.Position.x+side*frame.IdealLateral,sample.Position.z)+stanceNoise);
                    bool In(int k){var d=window[k].Position-new Vector3(stance.x,frame.ReferenceHeight,stance.y);return d.sqrMagnitude<=reach2;}
                    if(!In(index))continue;
                    // The run of consecutive legal steps within reach from this stance, and the step the contact planner
                    // will prefer inside it (first best height/clearance). A stance is better when that step sits well
                    // inside the run: releasing a little early or late still finds the ball within reach.
                    int first=index,last=index;
                    while(first>0&&window[first-1].Step==window[first].Step-1&&In(first-1))first--;
                    while(last+1<window.Count&&window[last+1].Step==window[last].Step+1&&In(last+1))last++;
                    int star=first;for(int k=first+1;k<=last;k++)if(window[k].Feature>window[star].Feature)star=k;
                    int untilStar=(int)(window[star].Step-start)-elapsedSteps;
                    int arrival=body.StepsToReach(stance,delaySteps,out _,Math.Max(untilStar,available)+context.StepHz);
                    if(arrival<=untilStar)
                    {
                        double slack=(untilStar-arrival)/(double)context.StepHz;
                        float margin=Mathf.Min(star-first,last-star);
                        reachable.Add(new Candidate(sample.Step,stance,arrival,(float)(p.WeightSlack*Math.Min(slack,p.SlackCapS)/p.SlackCapS
                            +p.WeightHeight*Height(frame,window[star].Position)+p.WeightWall*Wall(frame,court,window[star].Position)+p.WeightWindow*Mathf.Min(1,margin/marginCap))));
                    }
                    else
                    {
                        int late=arrival==int.MaxValue?int.MaxValue-1:arrival-available;
                        if(late<closestLate){closestLate=late;closest=new Candidate(sample.Step,stance,arrival,0);}
                    }
                }
            }
            reachable.Sort((a,b)=>b.Score!=a.Score?b.Score.CompareTo(a.Score):a.Step!=b.Step?a.Step.CompareTo(b.Step):a.Stance.x.CompareTo(b.Stance.x));
            int attempts=Math.Min(reachable.Count,Math.Max(1,(int)p.PlanAttempts));
            for(int i=0;i<attempts;i++)
            {
                var c=reachable[i];var contact=ContactPlanner.Plan(trajectory,context.FrameAt(c.Stance),court,legal,version);
                long arrivalStep=start+elapsedSteps+c.Arrival;
                if(!contact.Exists||contact.TStarStep<0||arrivalStep>contact.TStarStep)continue;
                double slack=(contact.TStarStep-arrivalStep)/(double)context.StepHz;
                return new ReceptionPlan(true,c.Stance,c.Stance,contact,arrivalStep,slack,Mathf.Clamp01((float)(slack/p.SlackCapS)),"planned");
            }
            // Unreachable (contract case 7): run at the least-late candidate anyway. The contract then sees the body
            // where it really is when the ball passes; if the ball is within reach from there, that honest chance stays.
            Candidate target;
            if(closest.HasValue)target=closest.Value;
            else if(reachable.Count>0)target=reachable[0];
            else
            {
                var sample=window[window.Count/2];var stance=context.ClampBody(new Vector2(sample.Position.x+frame.IdealLateral,sample.Position.z));
                target=new Candidate(sample.Step,stance,body.StepsToReach(stance,delaySteps,out _),0);
            }
            var at=body.PositionAfter(target.Stance,delaySteps,Math.Max(0,(int)(target.Step-start)-elapsedSteps));
            var effort=ContactPlanner.Plan(trajectory,context.FrameAt(at),court,legal,version);
            bool chance=effort.Exists&&effort.TStarStep>=0;
            return new ReceptionPlan(chance,target.Stance,at,effort,start+elapsedSteps+Math.Min(target.Arrival,int.MaxValue/2),0,0,chance?"stretched":"unreachable");
        }
        private static readonly float[] Sides={-1,1};
        // Same preference the contact planner uses to pick T* inside a range.
        private static float Height(BodyFrame frame,Vector3 ball)
        {
            float gap=ball.y<frame.PreferredMinHeight?frame.PreferredMinHeight-ball.y:ball.y>frame.PreferredMaxHeight?ball.y-frame.PreferredMaxHeight:0;
            return Mathf.Max(0,1-gap/(frame.PreferredMaxHeight-frame.PreferredMinHeight));
        }
        private static float Wall(BodyFrame frame,CourtDimensions court,Vector3 ball)
        {
            float clearance=Mathf.Min(Mathf.Min(ball.x,court.Width-ball.x),Mathf.Min(ball.z,court.Length-ball.z));
            return frame.WallClearance<=0?1:Mathf.Clamp01(clearance/frame.WallClearance);
        }
        private static float Feature(BodyFrame frame,CourtDimensions court,Vector3 ball)=>Height(frame,ball)+Wall(frame,court,ball);
    }
}
