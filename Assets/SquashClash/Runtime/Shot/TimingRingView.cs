using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    public sealed class TimingRingView : MonoBehaviour
    {
        // Option B: the converging ring fits the fixed triangle's incircle.
        public const float OuterRadius=.45f;
        private const float FitRadius=.1f,ClosedRadius=FitRadius*2, StrokePixels=2, FallbackWidth=.002f;
        // Look of the two reception rings (PO, 23 nov): the amber volley ring is drawn twice as thick as the white one and has a soft halo
        // at the same radius, so the two read apart at a glance. Only how they look; when each shows is untouched.
        private const float AmberStrokeFactor=2, HaloStrokeFactor=4, HaloAlpha=.3f;
        // White ring waiting at the outer radius: its alpha breathes between 1 and PulseLow at 2 Hz; steady once it converges.
        private const float PulseLow=.45f,PulsePeriod=.5f;
        private const float ValidAlpha=.35f, PerfectAlpha=.6f;
        private const int CircleSegments=96, TriangleVertices=3;
        private LineRenderer line,halo,triangle,validBand,perfectBand,lateValidBand,latePerfectBand;
        private Material material;
        private Transform ring;
        private TimingSettings timing;
        private bool shown,closing;
        private double closeFrom,closeUntil;
        private double target,presented;
        private int aidLevel,planVersion;
        public int AidLevel=>aidLevel;
        private Color tint=Color.white;
        // Colour of the cue (white by default; the volley opportunity is tinted).
        public Color Tint=>tint;
        public void SetTint(Color color){tint=color;}
        private bool Amber=>tint==ShotController.VolleyTint;
        // Where the cue is on the court (the plan's P*), for what is written next to it.
        public Vector3 CuePosition=>ring!=null?ring.position:Vector3.zero;
        // A new opportunity of the same reception (the bounce after the volley): the partial-aid cutoff applies to each one.
        public void BeginOpportunity(){CutoffLatched=false;CutoffTarget=0;CutoffPlanVersion=0;ClearClosingWindow();}
        public event System.Action CutoffActivated;
        public void BeginReception(int level)
        {
            aidLevel=level;CutoffLatched=false;CutoffTarget=0;CutoffPlanVersion=0;ClearClosingWindow();
            shown=false;if(ring!=null)Tick(Time.realtimeSinceStartupAsDouble);
        }
        // Presentation only (24 nov): the real-time tap window of the uncommitted amber offer. Over it the amber ring closes from the outer
        // radius to ClosedRadius, so the window running out is visible. No window (never set, until<=from or infinite) leaves the ring as the cue draws it.
        public void SetClosingWindow(double fromReal,double untilReal)
        {
            closing=untilReal>fromReal&&!double.IsInfinity(untilReal);closeFrom=fromReal;closeUntil=untilReal;
        }
        private void ClearClosingWindow(){closing=false;}
        public bool CutoffLatched {get;private set;}
        public double CutoffTarget {get;private set;}
        public int CutoffPlanVersion {get;private set;}
        public ImpactCueState CueState {get;private set;}
        public bool Visible=>line!=null&&line.enabled;
        public void Show(ContactPlan plan,double targetContactAtReal,bool visible,TimingSettings timing)
        {
            // Observe the old plan before replacing it: a replan cannot reopen a passed cutoff.
            Tick(Time.realtimeSinceStartupAsDouble);
            this.timing=timing;target=targetContactAtReal;planVersion=plan.Version;presented=Time.realtimeSinceStartupAsDouble;
            shown=visible&&plan.Exists;
            // Keep the hidden path free of GameObjects, LineRenderers, and materials.
            if(!shown||aidLevel==2||CutoffLatched){Tick(presented);return;}
            if(ring==null)
            {
                ring=new GameObject("First Shot timing cue").transform;ring.SetParent(transform,false);
                material=new Material(Resources.Load<Shader>("Legibility"));material.SetColor("_BaseColor",Color.white);
                line=CreateLine("Converging ring",CircleSegments);
                triangle=CreateLine("Fixed triangle",TriangleVertices);
                validBand=CreateLine("Valid band",CircleSegments);
                perfectBand=CreateLine("Perfect band",CircleSegments);
                lateValidBand=CreateLine("Late valid band",CircleSegments);
                latePerfectBand=CreateLine("Late perfect band",CircleSegments);
                DrawCircle(triangle,FitRadius*2,Mathf.PI/2);
            }
            ring.position=plan.PStar;Tick(presented);
        }
        private LineRenderer CreateLine(string name,int count)
        {
            var child=new GameObject(name);child.transform.SetParent(ring,false);
            var renderer=child.AddComponent<LineRenderer>();renderer.useWorldSpace=false;renderer.loop=true;renderer.positionCount=count;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            renderer.sharedMaterial=material;return renderer;
        }
        public void Tick(double now)
        {
            var state=ImpactCue.Evaluate(now,presented,target,timing,shown,!shown);CueState=state;
            if(aidLevel==1&&shown&&!CutoffLatched&&now>=target-.15)
            {CutoffLatched=true;CutoffTarget=target;CutoffPlanVersion=planVersion;CutoffActivated?.Invoke();}
            if(ring==null)return;
            if(aidLevel==2||CutoffLatched||state.Phase==ImpactCuePhase.Hidden||state.Phase==ImpactCuePhase.Closed)
            {if(halo!=null)halo.enabled=false;line.enabled=triangle.enabled=validBand.enabled=perfectBand.enabled=lateValidBand.enabled=latePerfectBand.enabled=false;return;}
            const float alpha=1;
            var camera=UnityEngine.Camera.main;
            float width=FallbackWidth;
            if(camera!=null)
            {
                ring.rotation=camera.transform.rotation;
                var screen=camera.WorldToScreenPoint(ring.position);
                if(screen.z>0)width=Vector3.Distance(camera.ScreenToWorldPoint(screen),
                    camera.ScreenToWorldPoint(screen+new Vector3(StrokePixels,0,0)));
            }
            // The halo exists only once the ring has been amber: a white-only cue keeps exactly the lines it always had.
            if(Amber&&halo==null){halo=CreateLine("Volley halo",CircleSegments);halo.sortingOrder=-1;}
            float lineAlpha=!Amber&&state.Approach>=1?1-(1-PulseLow)*(1-Mathf.Cos(2*Mathf.PI*(float)((now-presented)/PulsePeriod)))/2:alpha;
            SetStyle(line,lineAlpha,width*(Amber?AmberStrokeFactor:1));if(halo!=null)SetStyle(halo,Amber?alpha*HaloAlpha:0,width*HaloStrokeFactor);SetStyle(triangle,alpha,width);
            SetStyle(validBand,timing.ValidLimitOn?alpha*ValidAlpha:0,width);
            SetStyle(perfectBand,alpha*PerfectAlpha,width);
            SetStyle(lateValidBand,timing.ValidLimitOn?alpha*ValidAlpha:0,width);
            SetStyle(latePerfectBand,alpha*PerfectAlpha,width);
            float radius=Radius(state.Approach);
            // Float rounding must not visually snap to the fit radius before T*.
            if(!state.AtOrAfterIdeal&&radius<=FitRadius)radius=FitRadius+FitRadius*1.1920929e-7f;
            double duration=System.Math.Max(0,target-System.Math.Max(presented,target-ImpactCue.ApproachMs/1000))*1000;
            if(!state.AtOrAfterIdeal&&Amber&&closing)radius=Mathf.Min(radius,Mathf.Lerp(OuterRadius,ClosedRadius,Mathf.Clamp01((float)((now-closeFrom)/(closeUntil-closeFrom)))));
            if(state.AtOrAfterIdeal)radius=LateRadius(System.Math.Max(0,state.DeltaMs),duration);
            DrawCircle(line,radius);if(halo!=null)DrawCircle(halo,radius);
            DrawCircle(lateValidBand,LateRadius(timing.ValidMs,duration));
            DrawCircle(latePerfectBand,LateRadius(timing.AssistMs,duration));
            DrawCircle(validBand,WindowRadius(timing.ValidMs,duration));
            DrawCircle(perfectBand,WindowRadius(timing.AssistMs,duration));
        }
        // Continue inward after T*: the inner contours mark the inclusive late A/V edges.
        private static float LateRadius(double ms,double duration)=>FitRadius/(1+(OuterRadius/FitRadius-1)*(float)(ms/System.Math.Max(1,duration)));
        private static float Radius(float approach)=>FitRadius+(OuterRadius-FitRadius)*approach;
        private static float WindowRadius(double ms,double duration)=>Radius(duration>0?Mathf.Clamp01((float)(ms/duration)):1);
        private void SetStyle(LineRenderer renderer,float alpha,float width)
        {
            renderer.enabled=alpha>0;renderer.widthMultiplier=width;
            renderer.startColor=renderer.endColor=new Color(tint.r,tint.g,tint.b,alpha);
        }
        private static void DrawCircle(LineRenderer renderer,float radius,float offset=0)
        {
            for(int i=0;i<renderer.positionCount;i++)
            {
                float angle=offset+i*Mathf.PI*2/renderer.positionCount;
                renderer.SetPosition(i,new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius,0));
            }
        }
        private void OnDestroy(){if(ring!=null)Destroy(ring.gameObject);if(material!=null)Destroy(material);}
    }
}
