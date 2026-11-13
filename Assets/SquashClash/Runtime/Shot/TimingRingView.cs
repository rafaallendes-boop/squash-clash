using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    public sealed class TimingRingView : MonoBehaviour
    {
        // Option B: the converging ring fits the fixed triangle's incircle.
        private const float FitRadius=.1f, OuterRadius=.45f, StrokePixels=2, FallbackWidth=.002f;
        private const float ValidAlpha=.35f, PerfectAlpha=.6f;
        private const int CircleSegments=96, TriangleVertices=3;
        private LineRenderer line,triangle,validBand,perfectBand,lateValidBand,latePerfectBand;
        private Material material;
        private Transform ring;
        private TimingSettings timing;
        private bool shown;
        private double target,presented;
        public bool Visible=>line!=null&&line.enabled;
        public void Show(ContactPlan plan,double targetContactAtReal,bool visible,TimingSettings timing)
        {
            this.timing=timing;target=targetContactAtReal;presented=Time.realtimeSinceStartupAsDouble;
            shown=visible&&plan.Exists;
            // Keep the hidden path free of GameObjects, LineRenderers, and materials.
            if(!shown){if(ring!=null)Tick(presented);return;}
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
            var state=ImpactCue.Evaluate(now,presented,target,timing,shown,!shown);
            if(ring==null)return;
            if(state.Phase==ImpactCuePhase.Hidden||state.Phase==ImpactCuePhase.Closed)
            {line.enabled=triangle.enabled=validBand.enabled=perfectBand.enabled=lateValidBand.enabled=latePerfectBand.enabled=false;return;}
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
            SetStyle(line,alpha,width);SetStyle(triangle,alpha,width);
            SetStyle(validBand,timing.ValidLimitOn?alpha*ValidAlpha:0,width);
            SetStyle(perfectBand,alpha*PerfectAlpha,width);
            SetStyle(lateValidBand,timing.ValidLimitOn?alpha*ValidAlpha:0,width);
            SetStyle(latePerfectBand,alpha*PerfectAlpha,width);
            float radius=Radius(state.Approach);
            // Float rounding must not visually snap to the fit radius before T*.
            if(!state.AtOrAfterIdeal&&radius<=FitRadius)radius=FitRadius+FitRadius*1.1920929e-7f;
            double duration=System.Math.Max(0,target-System.Math.Max(presented,target-ImpactCue.ApproachMs/1000))*1000;
            if(state.AtOrAfterIdeal)radius=LateRadius(System.Math.Max(0,state.DeltaMs),duration);
            DrawCircle(line,radius);
            DrawCircle(lateValidBand,LateRadius(timing.ValidMs,duration));
            DrawCircle(latePerfectBand,LateRadius(timing.AssistMs,duration));
            DrawCircle(validBand,WindowRadius(timing.ValidMs,duration));
            DrawCircle(perfectBand,WindowRadius(timing.AssistMs,duration));
        }
        // Continue inward after T*: the inner contours mark the inclusive late A/V edges.
        private static float LateRadius(double ms,double duration)=>FitRadius/(1+(OuterRadius/FitRadius-1)*(float)(ms/System.Math.Max(1,duration)));
        private static float Radius(float approach)=>FitRadius+(OuterRadius-FitRadius)*approach;
        private static float WindowRadius(double ms,double duration)=>Radius(duration>0?Mathf.Clamp01((float)(ms/duration)):1);
        private static void SetStyle(LineRenderer renderer,float alpha,float width)
        {
            renderer.enabled=alpha>0;renderer.widthMultiplier=width;
            renderer.startColor=renderer.endColor=new Color(1,1,1,alpha);
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
