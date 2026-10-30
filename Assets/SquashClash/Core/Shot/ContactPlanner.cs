using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    public readonly struct SimTime
    {
        public readonly long Step;public readonly double Fraction;
        public SimTime(long step,double fraction=0){Step=step;Fraction=fraction;}
        public double Steps=>Step+Fraction;
        public static SimTime FromSteps(double value){long s=(long)System.Math.Floor(value);return new SimTime(s,value-s);}
    }
    public readonly struct LegalSettings
    {
        public readonly float MinHeight,MaxHeight,Gravity,Radius;public readonly int StepHz;public readonly CourtDimensions Court;
        public LegalSettings(float min,float max,int hz=120,float gravity=9.81f,float radius=.02f,CourtDimensions court=null){MinHeight=min;MaxHeight=max;StepHz=hz;Gravity=gravity;Radius=radius;Court=court??CourtDimensions.Standard();}
    }
    public readonly struct ContactPlan
    {
        public readonly bool Exists;public readonly SimTime KIn,KOut;public readonly long TStarStep;public readonly Vector3 PStar;public readonly StrokeSide Side;public readonly int Version;
        public readonly BodyFrame Body;public readonly LegalSettings Legal;public readonly CourtDimensions Court;public readonly string Reason;
        public ContactPlan(bool exists,SimTime kin,SimTime kout,long star,Vector3 point,StrokeSide side,int version,BodyFrame body,LegalSettings legal,CourtDimensions court,string reason=null)
        {Exists=exists;KIn=kin;KOut=kout;TStarStep=star;PStar=point;Side=side;Version=version;Body=body;Legal=legal;Court=court;Reason=reason;}
    }
    public static class ContactPlanner
    {
        private readonly struct Range {public readonly double Start,End;public Range(double a,double b){Start=a;End=b;}}
        public static ContactPlan Plan(Trajectory t,BodyFrame body,CourtDimensions court,LegalSettings legal,int version)
        {
            if(t==null||t.States.Count<2)return new ContactPlan(false,default,default,0,default,default,version,body,legal,court,"no-trajectory");
            var query=new Query(t,body,legal,court);var ranges=query.Ranges();int bestRange=-1,cursor=0;long best=-1;float score=float.NegativeInfinity;
            foreach(var state in t.States)
            {
                while(cursor<ranges.Count&&ranges[cursor].End<state.Step)cursor++;
                if(cursor>=ranges.Count)break;
                if(state.Step<ranges[cursor].Start||!query.IsLegal(new SimTime(state.Step)))continue;
                float height=state.Position.y;
                float gap=height<body.PreferredMinHeight?body.PreferredMinHeight-height:height>body.PreferredMaxHeight?height-body.PreferredMaxHeight:0;
                float preferred=Mathf.Max(0,1-gap/(body.PreferredMaxHeight-body.PreferredMinHeight));
                float clearance=Mathf.Min(state.Position.x,court.Width-state.Position.x,Mathf.Min(state.Position.z,court.Length-state.Position.z));
                float value=preferred+(body.WallClearance<=0?1:Mathf.Clamp01(clearance/body.WallClearance));
                if(value>score){score=value;best=state.Step;bestRange=cursor;}
            }
            if(bestRange<0)
            {
                if(ranges.Count==0)return new ContactPlan(false,default,default,0,default,default,version,body,legal,court,"unreachable");
                return new ContactPlan(true,SimTime.FromSteps(ranges[0].Start),SimTime.FromSteps(ranges[0].End),-1,default,default,version,body,legal,court,"no-legal-quantized-step");
            }
            var range=ranges[bestRange];var point=t.States[(int)(best-t.States[0].Step)].Position;
            return new ContactPlan(true,SimTime.FromSteps(range.Start),SimTime.FromSteps(range.End),best,point,body.SideFor(point),version,body,legal,court);
        }
        public static bool IsLegalAt(Trajectory t,SimTime when,BodyFrame body,LegalSettings legal)
            =>t!=null&&t.States.Count>0&&new Query(t,body,legal,legal.Court??CourtDimensions.Standard()).IsLegal(when);
        public static bool IsLegalStep(Trajectory t,long step,BodyFrame body,LegalSettings legal)=>IsLegalAt(t,new SimTime(step),body,legal);
        private readonly struct Curve
        {
            public readonly double X,Y,Z,VX,VY,VZ,G;
            public Curve(Vector3 point,Vector3 velocity,double gravity,double offset)
            {X=point.x+velocity.x*offset;Y=point.y+velocity.y*offset-.5*gravity*offset*offset;Z=point.z+velocity.z*offset;VX=velocity.x;VY=velocity.y-gravity*offset;VZ=velocity.z;G=gravity;}
            public void At(double t,out double x,out double y,out double z){x=X+VX*t;y=Y+VY*t-.5*G*t*t;z=Z+VZ*t;}
        }
        private sealed class Query
        {
            private readonly Trajectory t;private readonly BodyFrame body;private readonly LegalSettings legal;private readonly CourtDimensions court;
            private readonly List<ContactEvent>[] events;private readonly bool[] front,invalid;private readonly int[] bounces;
            private readonly IReadOnlyList<LineBand> lines;private readonly List<LineId> touched=new List<LineId>(4);
            private readonly double[] cuts=new double[16],polynomial=new double[5],roots=new double[8];private readonly PolynomialRoots finder=new PolynomialRoots();
            public Query(Trajectory t,BodyFrame body,LegalSettings legal,CourtDimensions court)
            {
                if(legal.StepHz<=0||legal.MinHeight>=legal.MaxHeight||body.Reach<=0||body.PreferredMinHeight>=body.PreferredMaxHeight)throw new ArgumentOutOfRangeException(nameof(legal));
                this.t=t;this.body=body;this.legal=legal;this.court=court;lines=CourtLines.Build(court);
                int n=t.States.Count;events=new List<ContactEvent>[n];front=new bool[n];invalid=new bool[n];bounces=new int[n];
                foreach(var e in t.Contacts)
                {
                    long index=e.Step-1-t.States[0].Step;
                    if(index<0)continue;if(index>=n)continue;
                    if(events[(int)index]==null)events[(int)index]=new List<ContactEvent>(4);events[(int)index].Add(e);
                }
                bool f=false,bad=false;int bounce=0;
                for(int i=0;i<n;i++)
                {
                    if(t.States[i].Step!=t.States[0].Step+i)throw new ArgumentException("Trajectory steps must be contiguous.");
                    front[i]=f;invalid[i]=bad;bounces[i]=bounce;
                    if(events[i]!=null)foreach(var e in events[i])Update(e,ref f,ref bounce,ref bad);
                }
            }
            private double Offset(ContactEvent e)=>Math.Min(1,Math.Max(0,(double)e.TimeInStep*legal.StepHz));
            private void Update(ContactEvent e,ref bool f,ref int bounce,ref bool bad)
            {
                if(e.Surface==SurfaceId.FrontWall)f=true;
                if(e.Surface==SurfaceId.Floor&&!e.Persistent)bounce++;
                if(e.Surface==SurfaceId.FrontWall&&(e.Point.y<=court.TinTop||e.Point.y>=court.FrontOutLower))bad=true;
                if(e.Surface==SurfaceId.BackWall&&e.Point.y>=court.BackOutLower)bad=true;
                if((e.Surface==SurfaceId.LeftWall||e.Surface==SurfaceId.RightWall)&&e.Point.y>=Mathf.Lerp(court.FrontOutLower,court.BackOutLower,e.Point.z/court.Length))bad=true;
                LineContactClassifier.Overlapping(lines,e.Surface,court.ToSurfaceUV(e.Surface,e.Point),legal.Radius,touched);
                foreach(var line in touched)if(line==LineId.Tin||line==LineId.FrontOut||line==LineId.BackOut||line==LineId.LeftSideOut||line==LineId.RightSideOut)bad=true;
            }
            private bool Within(double x,double y,double z)
            {
                if(y<legal.MinHeight||y>legal.MaxHeight)return false;
                double dx=x-body.PositionXZ.x,dy=y-body.ReferenceHeight,dz=z-body.PositionXZ.y;
                return dx*dx+dy*dy+dz*dz<=(double)body.Reach*body.Reach;
            }
            public bool IsLegal(SimTime when)
            {
                long at=when.Step-t.States[0].Step;if(at<0||at>=t.States.Count||when.Fraction<0||when.Fraction>=1||double.IsNaN(when.Fraction))return false;
                int i=(int)at;bool f=front[i],bad=invalid[i];int bounce=bounces[i],part=0;ContactEvent? exact=null;
                var list=events[i];if(list!=null)foreach(var e in list)
                {
                    double offset=Offset(e);if(offset>when.Fraction)break;Update(e,ref f,ref bounce,ref bad);part++;
                    if(offset==when.Fraction)exact=e;
                }
                if(!f||bounce!=1||bad)return false;
                if(exact.HasValue){var e=exact.Value;var p=e.Point+e.Normal*legal.Radius;return Within(p.x,p.y,p.z);}
                if(when.Fraction==0){var p=t.States[i].Position;return Within(p.x,p.y,p.z);}
                if(i+1>=t.States.Count)return false;
                double start=part==0?0:Offset(list[part-1]);var curve=Segment(i,part,start);
                curve.At((when.Fraction-start)/legal.StepHz,out var x,out var y,out var z);return Within(x,y,z);
            }
            private Curve Segment(int i,int part,double start)
            {
                var list=events[i];if(part==0)return new Curve(t.States[i].Position,t.States[i].Velocity,legal.Gravity,start/legal.StepHz);
                if(list!=null&&part<list.Count)
                {
                    var next=list[part];return new Curve(next.Point+next.Normal*legal.Radius,next.ApproachVelocity,legal.Gravity,(start-Offset(next))/legal.StepHz);
                }
                var end=t.States[i+1];return new Curve(end.Position,end.Velocity,legal.Gravity,(start-1)/legal.StepHz);
            }
            public List<Range> Ranges()
            {
                var ranges=new List<Range>(t.States.Count);
                for(int i=0;i+1<t.States.Count;i++)
                {
                    bool f=front[i],bad=invalid[i];int bounce=bounces[i];var list=events[i];int count=list?.Count??0;double start=0;
                    for(int part=0;part<=count;part++)
                    {
                        double end=part<count?Offset(list[part]):1;
                        if(f&&bounce==1&&!bad&&end>start)AddIntervals(Segment(i,part,start),t.States[i].Step+start,(end-start)/legal.StepHz,ranges);
                        if(part<count){Update(list[part],ref f,ref bounce,ref bad);start=end;}
                    }
                }
                return ranges;
            }
            private void AddIntervals(Curve c,double start,double duration,List<Range> ranges)
            {
                int count=2;cuts[0]=0;cuts[1]=duration;
                Array.Clear(polynomial,0,5);polynomial[0]=c.Y-legal.MinHeight;polynomial[1]=c.VY;polynomial[2]=-.5*c.G;
                count=AppendRoots(polynomial,2,duration,count);
                polynomial[0]=c.Y-legal.MaxHeight;count=AppendRoots(polynomial,2,duration,count);
                double dx=c.X-body.PositionXZ.x,dy=c.Y-body.ReferenceHeight,dz=c.Z-body.PositionXZ.y,halfG=-.5*c.G;
                polynomial[0]=dx*dx+dy*dy+dz*dz-(double)body.Reach*body.Reach;
                polynomial[1]=2*(dx*c.VX+dy*c.VY+dz*c.VZ);
                polynomial[2]=c.VX*c.VX+c.VY*c.VY+c.VZ*c.VZ+2*dy*halfG;polynomial[3]=2*c.VY*halfG;polynomial[4]=halfG*halfG;
                count=AppendRoots(polynomial,4,duration,count);Array.Sort(cuts,0,count);
                for(int j=1;j<count;j++)
                {
                    double a=cuts[j-1],b=cuts[j];if(b<=a)continue;c.At((a+b)*.5,out var x,out var y,out var z);if(!Within(x,y,z))continue;
                    double from=start+a*legal.StepHz,to=start+b*legal.StepHz;
                    if(ranges.Count>0&&ranges[ranges.Count-1].End==from){var previous=ranges[ranges.Count-1];ranges[ranges.Count-1]=new Range(previous.Start,to);}
                    else ranges.Add(new Range(from,to));
                }
            }
            private int AppendRoots(double[] p,int degree,double duration,int count)
            {int n=finder.Find(p,degree,0,duration,roots);for(int i=0;i<n;i++)if(roots[i]>0&&roots[i]<duration)cuts[count++]=roots[i];return count;}
        }
        // Private bounded quartic root isolation for ballistic sphere/height boundaries; scratch allocated once per plan.
        private sealed class PolynomialRoots
        {
            private readonly double[][] derivatives={new double[5],new double[5],new double[5],new double[5],new double[5]};
            private readonly double[][] critical={new double[8],new double[8],new double[8],new double[8],new double[8]};
            public int Find(double[] p,int degree,double low,double high,double[] result)
            {
                while(degree>0&&p[degree]==0)degree--;if(degree==0)return 0;
                if(degree==1){double r=-p[0]/p[1];if(r>=low&&r<=high){result[0]=r;return 1;}return 0;}
                var derivative=derivatives[degree];for(int i=1;i<=degree;i++)derivative[i-1]=i*p[i];
                var points=critical[degree];int n=Find(derivative,degree-1,low,high,points),found=0;double a=low,fa=Value(p,degree,a);
                for(int i=0;i<=n;i++)
                {
                    double b=i<n?points[i]:high,fb=Value(p,degree,b);
                    if(fa==0&&(found==0||result[found-1]!=a))result[found++]=a;
                    if((fa<0&&fb>0)||(fa>0&&fb<0))
                    {
                        double left=a,right=b,leftValue=fa;
                        for(int k=0;k<64;k++)
                        {
                            double mid=(left+right)*.5;if(mid==left||mid==right)break;double value=Value(p,degree,mid);
                            if(value==0){left=right=mid;break;}
                            if((leftValue<0&&value>0)||(leftValue>0&&value<0))right=mid;else{left=mid;leftValue=value;}
                        }
                        result[found++]=(left+right)*.5;
                    }
                    if(i==n&&fb==0&&(found==0||result[found-1]!=b))result[found++]=b;
                    a=b;fa=fb;
                }
                return found;
            }
            private static double Value(double[] p,int degree,double x){double value=p[degree];for(int i=degree-1;i>=0;i--)value=value*x+p[i];return value;}
        }
    }
}
