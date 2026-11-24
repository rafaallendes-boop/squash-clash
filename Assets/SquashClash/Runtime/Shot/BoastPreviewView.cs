using System;
using System.Diagnostics;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    // Two-wall preview (design 2026-11-24-two-wall-preview-hint): while the finger rests on the side wall and pulls, a dashed oval on the
    // front wall around where the boast would meet it with the ideal timing (P*). Without the causal layer, so a zone and not a point: no
    // cross, no number, another colour. A separate view: it only reads the M2-4 gesture (GesturePreview.M24) and the controller's plan; it
    // writes neither, writes no metrics and never touches the authoritative solve (ShotController.Solve/ShowShot). It hides on release, on
    // cancel and once the attempt is consumed, so it never shows with the frozen reticle's cross. Recomputed on BoastPreviewSchedule.
    public sealed class BoastPreviewView : MonoBehaviour
    {
        // View-only geometry (metres on the front wall) and colour. Larger than the cross arms (0.12 m) and not white like the cross.
        // Discreet (SC-Lead, 24 nov): a hint, not a target that plays for the player, so thinner than the cross stroke (0.035 m) and
        // translucent (the Legibility shader blends alpha).
        public const int Dashes=12;
        private const float HalfWidthMeters=.30f,HalfHeightMeters=.36f,StrokeMeters=.022f,WallOffset=.03f;
        private static readonly Color ZoneTint=new Color(.45f,.82f,1f,.45f);
        public int DashCount=>Dashes;
        public float HalfHeight=>HalfHeightMeters;
        public float StrokeWidth=>StrokeMeters;
        public System.Collections.Generic.IReadOnlyList<LineRenderer> DashRenderers=>dashes;
        public Color Tint=>ZoneTint;
        public Func<double> Now=()=>Time.realtimeSinceStartupAsDouble;
        public bool Visible {get;private set;}
        public Vector2 Front {get;private set;}
        public Vector3 Center=>new Vector3(Front.x,Front.y,WallOffset);
        public int SolveCount=>schedule.SolveCount;
        public double LastCostMs {get;private set;}
        public double MaxCostMs {get;private set;}
        private AppBootstrap app;private GesturePreview gesture;private ShotController controller;
        private readonly BoastPreviewSchedule schedule=new BoastPreviewSchedule();
        private IBallSimulationFactory factory;
        private LineRenderer[] dashes;private Material material;private bool solved;private Vector2 drawn=new Vector2(float.NaN,float.NaN);
        public void Initialize(AppBootstrap app,GesturePreview gesture,ShotController controller)
        {
            if(this.app!=null)return;
            this.app=app;this.gesture=gesture;this.controller=controller;
            factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
        }
        // After the gesture read its input this frame.
        private void LateUpdate()=>Tick();
        public void Tick()
        {
            var m=gesture!=null?gesture.M24:null;
            bool held=app!=null&&controller!=null&&m!=null&&m.Active&&m.HasAim&&m.Side.Exists&&m.P>0&&controller.Available&&!controller.AttemptConsumed;
            var plan=held?(controller.VolleyCommitted?controller.Volley.Plan:controller.Plan):default;
            if(!held||!plan.Exists||plan.TStarStep<0){schedule.Reset();solved=false;Show(false);return;}
            if(schedule.ShouldSolve(Now(),app.Host.LaunchId,plan.PStar,m.Side,m.P))
            {
                long start=Stopwatch.GetTimestamp();
                var s=BoastPreview.Estimate(plan.PStar,m.Side,m.P,app.Host.ActiveConfig,factory,ShotIntentSolver.DefaultFootprintRadius,out var info);
                LastCostMs=(Stopwatch.GetTimestamp()-start)*1000d/Stopwatch.Frequency;MaxCostMs=Math.Max(MaxCostMs,LastCostMs);
                solved=s.Solved&&info.Solved;if(solved)Front=info.Front;
            }
            Show(solved);
        }
        private void Show(bool visible)
        {
            Visible=visible;
            if(!visible){if(dashes!=null)foreach(var d in dashes)d.enabled=false;return;}
            Ensure();
            if(Front!=drawn){Draw(Front);drawn=Front;}
            foreach(var d in dashes)d.enabled=true;
        }
        private void Ensure()
        {
            if(dashes!=null)return;
            material=new Material(Resources.Load<Shader>("Legibility"));material.SetColor("_BaseColor",Color.white);
            dashes=new LineRenderer[Dashes];
            for(int i=0;i<Dashes;i++)
            {
                var go=new GameObject("Boast preview dash "+i);go.transform.SetParent(transform,false);var d=go.AddComponent<LineRenderer>();
                d.useWorldSpace=true;d.loop=false;d.positionCount=4;d.widthMultiplier=StrokeMeters;d.sharedMaterial=material;d.startColor=d.endColor=ZoneTint;
                d.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;d.receiveShadows=false;d.enabled=false;dashes[i]=d;
            }
        }
        // Each dash covers the first half of its slot of the oval, so the gaps make it read as a zone, not a ring or a cross.
        private void Draw(Vector2 c)
        {
            float slot=Mathf.PI*2/Dashes;
            for(int i=0;i<Dashes;i++)
                for(int k=0;k<4;k++)
                {
                    float a=i*slot+k*(slot*.5f)/3;
                    dashes[i].SetPosition(k,new Vector3(c.x+Mathf.Cos(a)*HalfWidthMeters,c.y+Mathf.Sin(a)*HalfHeightMeters,WallOffset));
                }
        }
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
