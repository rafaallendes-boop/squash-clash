using System;
using System.Collections.Generic;
namespace SquashClash.Core.Shot
{
    // Observation map of the host's existing SimClock. Never advances or owns a simulation clock.
    public sealed class ClockMap
    {
        private readonly struct Segment
        {
            public readonly double Real,Position,Tempo,Slow;
            public readonly bool Paused,GapBefore,InvalidAtStart;
            public Segment(double real,double position,double tempo,double slow,bool paused,bool gap,bool invalid)
            {Real=real;Position=position;Tempo=tempo;Slow=slow;Paused=paused;GapBefore=gap;InvalidAtStart=invalid;}
        }
        private readonly List<Segment> segments=new List<Segment>(512);
        private int stepHz;
        public long LaunchId {get;private set;}
        public void Begin(long launchId,double tReal,long step,double tempo,double slowMotion,int stepHz)
        {
            Validate(tReal,step,0,tempo,slowMotion);
            if(launchId<0||stepHz<=0)throw new ArgumentOutOfRangeException(nameof(stepHz));
            this.stepHz=stepHz;LaunchId=launchId;segments.Clear();
            segments.Add(new Segment(tReal,step,tempo,slowMotion,false,false,slowMotion<1));
        }
        // droppedSteps is a fact supplied by SimClock; timestamp jitter is not a discard detector.
        public void Mark(double tReal,long step,double fraction,double tempo,double slowMotion,bool paused,bool droppedSteps=false)
        {
            Validate(tReal,step,fraction,tempo,slowMotion);
            if(segments.Count==0)throw new InvalidOperationException("Begin a launch before marking its clock.");
            var last=segments[segments.Count-1];double position=step+fraction;
            if(tReal<last.Real||position<last.Position)throw new ArgumentOutOfRangeException(nameof(tReal));
            bool invalid=droppedSteps||last.Tempo!=tempo||last.Slow!=slowMotion||last.Paused!=paused;
            if(tReal==last.Real)
            {
                segments[segments.Count-1]=new Segment(tReal,position,tempo,slowMotion,paused,last.GapBefore||droppedSteps,last.InvalidAtStart||invalid);
                return;
            }
            segments.Add(new Segment(tReal,position,tempo,slowMotion,paused,droppedSteps,invalid));
        }
        public bool TryRealToSim(long launchId,double tReal,out long step,out double fraction)
        {
            step=0;fraction=0;
            return launchId==LaunchId&&TryRealToSim(tReal,out step,out fraction);
        }
        public bool TryRealToSim(double tReal,out long step,out double fraction)
        {
            step=0;fraction=0;
            if(!Finite(tReal)||segments.Count==0||tReal<segments[0].Real)return false;
            int i=Find(tReal);var a=segments[i];double position;
            if(tReal==a.Real)position=a.Position;
            else if(i+1<segments.Count)
            {
                var b=segments[i+1];if(b.GapBefore)return false;
                // A paused single step is discrete, never a fabricated fractional advance.
                position=a.Paused?a.Position:a.Position+(b.Position-a.Position)*(tReal-a.Real)/(b.Real-a.Real);
            }
            else position=a.Position+(tReal-a.Real)*Rate(a);
            if(!Finite(position)||position<0||position>=long.MaxValue)return false;
            // Arithmetic such as (.5-.4)*120 may land a few ulps below an exact step.
            // Bound this normalization by double representation error, not a game-time tolerance.
            double nearest=Math.Round(position);
            if(Math.Abs(position-nearest)<=4*2.2204460492503131e-16*Math.Max(1,Math.Abs(position)))position=nearest;
            step=(long)Math.Floor(position);fraction=position-step;return true;
        }
        public bool TrySimToReal(long step,double fraction,out double tReal)
        {
            tReal=0;if(step<0||!Finite(fraction)||fraction<0||fraction>=1||segments.Count==0)return false;
            double target=step+fraction;
            for(int i=0;i<segments.Count;i++)
            {
                var a=segments[i];if(target==a.Position){tReal=a.Real;return true;}
                if(i+1<segments.Count)
                {
                    var b=segments[i+1];
                    if(!a.Paused&&!b.GapBefore&&target>a.Position&&target<=b.Position&&b.Position>a.Position)
                    {tReal=a.Real+(target-a.Position)*(b.Real-a.Real)/(b.Position-a.Position);return true;}
                }
                else if(target>a.Position&&Rate(a)>0)
                {tReal=a.Real+(target-a.Position)/Rate(a);return Finite(tReal);}
            }
            return false;
        }
        public bool InvalidSince(double tReal)
        {
            if(!Finite(tReal)||segments.Count==0||tReal<segments[0].Real)return true;
            for(int i=Find(tReal);i<segments.Count;i++)
            {
                var s=segments[i];double end=i+1<segments.Count?segments[i+1].Real:double.PositiveInfinity;
                if(s.InvalidAtStart&&s.Real>=tReal)return true;
                if((s.Paused||s.Slow<1)&&end>tReal)return true;
                if(i+1<segments.Count&&segments[i+1].GapBefore&&end>tReal)return true;
            }
            return false;
        }
        private double Rate(Segment s)=>s.Paused?0:s.Tempo*s.Slow*stepHz;
        private int Find(double time)
        {
            int low=0,high=segments.Count-1;
            while(low<high){int mid=low+(high-low+1)/2;if(segments[mid].Real<=time)low=mid;else high=mid-1;}
            return low;
        }
        private static bool Finite(double x)=>!double.IsNaN(x)&&!double.IsInfinity(x);
        private static void Validate(double time,long step,double fraction,double tempo,double slow)
        {
            if(!Finite(time)||time<0||step<0||!Finite(fraction)||fraction<0||fraction>=1||!Finite(tempo)||tempo<=0||!Finite(slow)||slow<=0||slow>1)
                throw new ArgumentOutOfRangeException(nameof(time));
        }
    }
}
