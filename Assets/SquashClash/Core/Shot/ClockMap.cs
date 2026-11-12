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
        // Last supplied observation, including a coalesced endpoint. Passive metadata only:
        // preserves monotonic validation and the known prefix before a discard/discontinuity.
        private Segment observed;
        private int stepHz;
        public long LaunchId {get;private set;}
        public void Begin(long launchId,double tReal,long step,double tempo,double slowMotion,int stepHz)
        {
            Validate(tReal,step,0,tempo,slowMotion);
            if(launchId<0||stepHz<=0)throw new ArgumentOutOfRangeException(nameof(stepHz));
            this.stepHz=stepHz;LaunchId=launchId;segments.Clear();
            observed=new Segment(tReal,step,tempo,slowMotion,false,false,slowMotion<1);
            segments.Add(observed);
        }
        // droppedSteps is a fact supplied by SimClock; timestamp jitter is not a discard detector.
        public void Mark(double tReal,long step,double fraction,double tempo,double slowMotion,bool paused,bool droppedSteps=false)
        {
            Validate(tReal,step,fraction,tempo,slowMotion);
            if(segments.Count==0)throw new InvalidOperationException("Begin a launch before marking its clock.");
            var last=segments[segments.Count-1];double position=step+fraction;
            if(tReal<observed.Real||position<observed.Position)throw new ArgumentOutOfRangeException(nameof(tReal));
            bool invalid=droppedSteps||observed.Tempo!=tempo||observed.Slow!=slowMotion||observed.Paused!=paused;
            if(tReal==observed.Real)
            {
                if(observed.Real>last.Real)segments.Add(observed);
                observed=new Segment(tReal,position,tempo,slowMotion,paused,observed.GapBefore||droppedSteps,observed.InvalidAtStart||invalid);
                segments[segments.Count-1]=observed;
                return;
            }
            var next=new Segment(tReal,position,tempo,slowMotion,paused,droppedSteps,invalid);
            // Exact continuation only. No tolerance, pruning, time cap or second clock.
            if(!invalid&&position==NormalizePosition(observed.Position+(tReal-observed.Real)*Rate(observed),Math.Max(tReal,observed.Real)*Rate(observed)))
            {observed=next;return;}
            // Do not stretch a later gap or an inexact interval over an already known prefix.
            if(observed.Real>last.Real)segments.Add(observed);
            segments.Add(next);observed=next;
        }
        public bool TryRealToSim(long launchId,double tReal,out long step,out double fraction)
        {
            step=0;fraction=0;
            return launchId==LaunchId&&TryRealToSim(tReal,out step,out fraction);
        }
        public bool TryRealToSim(double tReal,out long step,out double fraction)
        {
            step=0;fraction=0;
            if(!Finite(tReal)||SegmentCount==0||tReal<At(0).Real)return false;
            int i=Find(tReal);var a=At(i);double position,timeScale=0;
            if(tReal==a.Real)position=a.Position;
            else if(i+1<SegmentCount)
            {
                var b=At(i+1);if(b.GapBefore)return false;
                // A paused single step is discrete, never a fabricated fractional advance.
                position=a.Paused?a.Position:a.Position+(b.Position-a.Position)*(tReal-a.Real)/(b.Real-a.Real);
                if(!a.Paused)timeScale=Math.Max(tReal,b.Real)*Math.Abs((b.Position-a.Position)/(b.Real-a.Real));
            }
            else
            {
                position=a.Position+(tReal-a.Real)*Rate(a);
                timeScale=Math.Max(tReal,a.Real)*Rate(a);
            }
            if(!Finite(position)||position<0||position>=long.MaxValue)return false;
            position=NormalizePosition(position,timeScale);
            step=(long)Math.Floor(position);fraction=position-step;return true;
        }
        public bool TrySimToReal(long step,double fraction,out double tReal)
        {
            tReal=0;if(step<0||!Finite(fraction)||fraction<0||fraction>=1||SegmentCount==0)return false;
            double target=step+fraction;
            for(int i=0;i<SegmentCount;i++)
            {
                var a=At(i);if(target==a.Position){tReal=a.Real;return true;}
                if(i+1<SegmentCount)
                {
                    var b=At(i+1);
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
            if(!Finite(tReal)||SegmentCount==0||tReal<At(0).Real)return true;
            for(int i=Find(tReal);i<SegmentCount;i++)
            {
                var s=At(i);double end=i+1<SegmentCount?At(i+1).Real:double.PositiveInfinity;
                if(s.InvalidAtStart&&s.Real>=tReal)return true;
                if((s.Paused||s.Slow<1)&&end>tReal)return true;
                if(i+1<SegmentCount&&At(i+1).GapBefore&&end>tReal)return true;
            }
            return false;
        }
        // The latest supplied endpoint remains queryable exactly without retaining every frame.
        private int SegmentCount=>segments.Count+(segments.Count>0&&observed.Real>segments[segments.Count-1].Real?1:0);
        private Segment At(int index)=>index<segments.Count?segments[index]:observed;
        private static double NormalizePosition(double position,double timeScale=0)
        {
            // Subtracting absolute double timestamps can lose ulps at their magnitude, even
            // when the resulting step position is small. Propagate that scale through the
            // local steps/second slope, including interpolation's timestamp denominator.
            // The existing four-epsilon envelope covers representation/arithmetic error;
            // it is not a game-time window. Exact observations have no timestamp arithmetic.
            double nearest=Math.Round(position);
            double scale=Math.Max(1,Math.Max(Math.Abs(position),timeScale));
            if(Math.Abs(position-nearest)<=4*2.2204460492503131e-16*scale)position=nearest;
            return position;
        }
        private double Rate(Segment s)=>s.Paused?0:s.Tempo*s.Slow*stepHz;
        private int Find(double time)
        {
            int low=0,high=SegmentCount-1;
            while(low<high){int mid=low+(high-low+1)/2;if(At(mid).Real<=time)low=mid;else high=mid-1;}
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
