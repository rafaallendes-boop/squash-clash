using System;
using System.Collections.Generic;
using System.Linq;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    [Serializable] public struct CausalVector
    {
        public float DeltaHeight,DeltaLateral,DeltaSpeedFraction;public string Cause;
        public CausalVector(float height,float speed,string cause){DeltaHeight=height;DeltaLateral=0;DeltaSpeedFraction=speed;Cause=cause;}
    }
    public readonly struct CausalSettings
    {
        public static CausalSettings From(ParamSet p)=>new CausalSettings(
            (float)p.Get("causal.timingHeightPerM"),(float)p.Get("causal.timingSpeedPerM"),(float)p.Get("causal.stretchStart"),(float)p.Get("causal.jamDistance"),
            (float)p.Get("causal.stretchHeight"),(float)p.Get("causal.stretchSpeed"),(float)p.Get("causal.jamHeight"),(float)p.Get("causal.jamSpeed"),
            (float)p.Get("causal.capHeight"),(float)p.Get("causal.capSpeed"));
        public readonly float TimingHeight,TimingSpeed,StretchStart,JamDistance,StretchHeight,StretchSpeed,JamHeight,JamSpeed,CapHeight,CapSpeed;
        public CausalSettings(float timingHeight=.6f,float timingSpeed=-.25f,float stretchStart=.85f,float jamDistance=.35f,float stretchHeight=.4f,float stretchSpeed=-.2f,float jamHeight=.3f,float jamSpeed=-.15f,float capHeight=1,float capSpeed=-.4f)
        {TimingHeight=timingHeight;TimingSpeed=timingSpeed;StretchStart=stretchStart;JamDistance=jamDistance;StretchHeight=stretchHeight;StretchSpeed=stretchSpeed;JamHeight=jamHeight;JamSpeed=jamSpeed;CapHeight=capHeight;CapSpeed=capSpeed;}
    }
    // Owns only frozen values; evaluation has no access to an attempt, category or live configuration.
    public sealed class CausalReference
    {
        internal readonly struct Segment
        {
            internal readonly Vector3 A,B;
            internal Segment(Vector3 a,Vector3 b){A=a;B=b;}
        }
        private readonly Segment[] zone;
        private readonly BodyFrame[] frames;
        private readonly long firstStep;
        private readonly IReadOnlyList<long> perfect;
        public IReadOnlyList<long> PerfectSteps=>perfect;
        public double ArcRadius {get;}
        public double Tempo {get;}
        public TimingSettings Timing {get;}
        public int PlanVersion {get;}
        internal CausalReference(Segment[] zone,long[] perfect,double radius,double tempo,TimingSettings timing,int version,BodyFrame[] frames,long firstStep)
        {this.frames=frames;this.firstStep=firstStep;this.zone=zone;this.perfect=Array.AsReadOnly(perfect);ArcRadius=radius;Tempo=tempo;Timing=timing;PlanVersion=version;}
        public BodyFrame BodyAt(long step)=>frames[checked((int)(step-firstStep))];
        public double Distance(Vector3 point,BodyFrame body)
        {
            var q=point-new Vector3(body.PositionXZ.x,body.ReferenceHeight,body.PositionXZ.y);
            double best=double.PositiveInfinity;
            foreach(var line in zone)
            {
                double x=(double)line.B.x-line.A.x,y=(double)line.B.y-line.A.y,z=(double)line.B.z-line.A.z;
                double dx=(double)q.x-line.A.x,dy=(double)q.y-line.A.y,dz=(double)q.z-line.A.z;
                double norm=x*x+y*y+z*z;
                double t=norm==0?0:Math.Max(0,Math.Min(1,(dx*x+dy*y+dz*z)/norm));
                dx-=t*x;dy-=t*y;dz-=t*z;best=Math.Min(best,dx*dx+dy*dy+dz*dz);
            }
            return Math.Sqrt(best);
        }
    }
    public readonly struct CausalShotResult
    {
        public readonly CausalVector[] Vectors;public readonly CausalInput Input;public readonly ShotSolution Solution;
        public CausalShotResult(CausalVector[] vectors,CausalInput input,ShotSolution solution){Vectors=vectors;Input=input;Solution=solution;}
    }
    public readonly struct CausalInput
    {
        public readonly Vector2 Aim;public readonly float Speed;
        public CausalInput(Vector2 aim,float speed){Aim=aim;Speed=speed;}
    }
    public static class CausalQuality
    {
        public static CausalShotResult Solve(CausalReference reference,BodyFrame body,Vector3 contact,Vector2 aim,float power,EffectiveConfig config,IBallSimulationFactory factory)
        {
            var settings=CausalSettings.From(config.Source);var vectors=Evaluate(reference,body,contact,settings);
            float requested=(float)config.Source.Get("shot.vMin")+power*((float)config.Source.Get("shot.vMax")-(float)config.Source.Get("shot.vMin"));
            var input=Apply(aim,requested,vectors,settings);
            // The existing solver retains its feasibility raising/lowering, cap and verifier unchanged.
            // Equal endpoints convey the already composed total speed without clamping it back to vMin.
            var solution=ShotSolver.Solve(new ShotRequest(contact,input.Aim,power),input.Speed,input.Speed,(float)config.Source.Get("shot.vCap"),
                config.Physics.Gravity,config.Physics.BallRadius,config.Court,factory,config.Physics);
            return new CausalShotResult(vectors,input,solution);
        }
        public static CausalReference Prepare(Trajectory trajectory,ContactPlan plan,ClockMap clock,TimingSettings timing,double presented,double tempo,Func<long,BodyFrame> bodyAt=null)
        {
            if(trajectory==null||clock==null)throw new ArgumentNullException(nameof(trajectory));
            if(!plan.Exists||plan.TStarStep<0||trajectory.States.Count<2)throw new ArgumentException("A quantized reception plan is required.");
            var frozen=clock.Freeze();
            if(!frozen.TrySimToReal(plan.TStarStep,0,out double target))throw new ArgumentException("Reception target is unmapped.");
            // ContactContract is constant on open cells split by its timing, geometry, mapping and
            // quantization boundaries. Test every boundary too: ties and inclusive edges are real cells.
            var cuts=new SortedSet<double>{target-timing.AssistMs/1000,target,target+timing.AssistMs/1000,
                target-timing.ValidMs/1000,target+timing.ValidMs/1000,presented-timing.LatencyCompMs/1000};
            foreach(double t in frozen.ObservationTimes)cuts.Add(t);
            void Cut(double at)
            {var t=SimTime.FromSteps(at);if(frozen.TrySimToReal(t.Step,t.Fraction,out double real))cuts.Add(real);}
            Cut(plan.KIn.Steps);Cut(plan.KOut.Steps);
            foreach(var state in trajectory.States){Cut(state.Step);Cut(state.Step+.5);}
            foreach(var e in trajectory.Contacts)Cut(e.Step-1+(double)e.TimeInStep*plan.Legal.StepHz);
            double low=target-timing.AssistMs/1000,high=target+timing.AssistMs/1000;
            var times=cuts.Where(t=>t>=low&&t<=high).ToArray();
            var plans=new PresentedPlans();plans.Present(presented,plan,target);
            var perfect=new SortedSet<long>();
            void Probe(double corrected)
            {
                var d=ContactContract.Decide(corrected+timing.LatencyCompMs/1000,plans,frozen,trajectory,timing,false);
                if(d.Result==ShotResult.Perfect)perfect.Add(d.ContactStep);
            }
            for(int i=0;i<times.Length;i++)
            {
                Probe(times[i]);
                if(i+1<times.Length)Probe(times[i]+(times[i+1]-times[i])*.5);
            }
            long start=trajectory.States[0].Step;int n=trajectory.States.Count,star=checked((int)(plan.TStarStep-start));
            var q=new Vector3[n];var arc=new double[n];var frames=new BodyFrame[n];
            for(int i=0;i<n;i++)
            {
                var state=trajectory.States[i];if(state.Step!=start+i)throw new ArgumentException("Noncontiguous reception trajectory.");
                var frame=bodyAt==null?plan.Body:bodyAt(state.Step);frames[i]=frame;
                q[i]=state.Position-new Vector3(frame.PositionXZ.x,frame.ReferenceHeight,frame.PositionXZ.y);
                if(i>0)
                {
                    double dx=(double)q[i].x-q[i-1].x,dy=(double)q[i].y-q[i-1].y,dz=(double)q[i].z-q[i-1].z;
                    arc[i]=arc[i-1]+Math.Sqrt(dx*dx+dy*dy+dz*dz);
                }
            }
            double origin=arc[star],radius=0;
            for(int i=0;i<n;i++)arc[i]-=origin;
            foreach(long j in perfect)radius=Math.Max(radius,Math.Abs(arc[checked((int)(j-start))]));
            var zone=new List<CausalReference.Segment>();
            // Isolated nodes are retained, including L=0. Segments are clipped by both arc and K,
            // including fractional ends of K; no extrapolation past the legal reception interval.
            for(int i=0;i<n;i++)
            {
                if(start+i>=plan.KIn.Steps&&start+i<=plan.KOut.Steps&&Math.Abs(arc[i])<=radius)
                    zone.Add(new CausalReference.Segment(q[i],q[i]));
                if(i+1==n)continue;
                double lo=Math.Max(0,plan.KIn.Steps-(start+i)),hi=Math.Min(1,plan.KOut.Steps-(start+i));
                double span=arc[i+1]-arc[i];
                if(span>0){lo=Math.Max(lo,(-radius-arc[i])/span);hi=Math.Min(hi,(radius-arc[i])/span);}
                else if(Math.Abs(arc[i])>radius)continue;
                if(lo>hi)continue;
                zone.Add(new CausalReference.Segment(Vector3.LerpUnclamped(q[i],q[i+1],(float)lo),Vector3.LerpUnclamped(q[i],q[i+1],(float)hi)));
            }
            if(zone.Count==0)throw new InvalidOperationException("No geometric zone in reception K.");
            return new CausalReference(zone.ToArray(),perfect.ToArray(),radius,tempo,timing,plan.Version,frames,start);
        }
        public static CausalVector[] Evaluate(CausalReference reference,BodyFrame body,Vector3 contact,CausalSettings settings)
        {
            if(reference==null)throw new ArgumentNullException(nameof(reference));
            double d=reference.Distance(contact,body);
            var vectors=new List<CausalVector>(3){new CausalVector((float)Math.Min(settings.CapHeight,settings.TimingHeight*d),(float)Math.Max(settings.CapSpeed,settings.TimingSpeed*d),"timing")};
            double x=(double)contact.x-body.PositionXZ.x,y=(double)contact.y-body.ReferenceHeight,z=(double)contact.z-body.PositionXZ.y;
            double reachDistance=Math.Sqrt(x*x+y*y+z*z);
            // Linear onset from the configured thresholds; t=0 (no technique attenuation).
            double stretch=Math.Max(0,Math.Min(1,(reachDistance/body.Reach-settings.StretchStart)/(1-settings.StretchStart)));
            double jam=settings.JamDistance<=0?0:Math.Max(0,Math.Min(1,1-reachDistance/settings.JamDistance));
            if(stretch>0)vectors.Add(new CausalVector((float)(settings.StretchHeight*stretch),(float)(settings.StretchSpeed*stretch),"stretch"));
            if(jam>0)vectors.Add(new CausalVector((float)(settings.JamHeight*jam),(float)(settings.JamSpeed*jam),"jam"));
            return vectors.ToArray();
        }
        public static CausalInput Apply(Vector2 aim,float speed,IReadOnlyList<CausalVector> vectors,CausalSettings settings)
        {
            float height=0,loss=0;
            foreach(var vector in vectors){height+=vector.DeltaHeight;loss+=vector.DeltaSpeedFraction;}
            return new CausalInput(new Vector2(aim.x,aim.y+Mathf.Min(settings.CapHeight,height)),speed*(1+Mathf.Max(settings.CapSpeed,loss)));
        }
    }
}
