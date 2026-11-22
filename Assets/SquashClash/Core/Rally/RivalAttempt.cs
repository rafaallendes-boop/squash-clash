using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    // Visual attempt only: never changes a reception or the point's rules.
    public readonly struct RivalAttempt
    {
        public readonly long Step;
        public readonly StrokeSide Side;
        public RivalAttempt(long step,StrokeSide side){Step=step;Side=side;}
        public static RivalAttempt StepFor(RallyContext context,Trajectory trajectory,PointRules rulesAtStart,Vector2[] path)
        {
            if(context==null||trajectory==null||rulesAtStart==null||path==null)throw new ArgumentNullException(nameof(context));
            var attempt=new RivalAttempt(-1,default);
            var rules=rulesAtStart.Clone();int next=0;float closest=float.PositiveInfinity;
            for(int i=0;i<trajectory.States.Count&&i<path.Length&&!rules.Closed;i++)
            {
                var state=trajectory.States[i];
                while(next<trajectory.Contacts.Count&&trajectory.Contacts[next].Step<=state.Step&&!rules.Closed)
                    rules.OnBallContact(trajectory.Contacts[next++]);
                if(state.Position.y>context.Court.WallHeight&&!rules.Closed)rules.OnOverTop();
                // The terminal second-bounce step is included, so the view can attempt before EndPoint on that tick.
                if(rules.FloorBounces==0||(rules.Closed&&rules.Cause!=PointEndCause.DoubleBounce))continue;
                var frame=context.FrameAt(path[i]);
                float distance=(state.Position-new Vector3(path[i].x,frame.ReferenceHeight,path[i].y)).magnitude-frame.Reach;
                if(distance>=closest)continue;
                closest=distance;attempt=new RivalAttempt(state.Step,frame.SideFor(state.Position));
            }
            return attempt;
        }
    }
}
using System;
