using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    // Volley v1 look (PO, 19 nov): instead of a «Volea · » note over the play, the amber volley ring pops at the contact point. It
    // starts at the cue's fitted radius, grows and fades out in a quarter of a second. Only an applied contact of the volley
    // opportunity shows it. Pure view: no collider, no clock, timing window, contact or ball effect.
    public sealed class VolleyBurstView : MonoBehaviour
    {
        public const float DurationSeconds=.25f,StartRadius=.1f,EndRadius=.24f;
        private const float StrokePixels=3,FallbackWidth=.003f;
        private const int Segments=64;
        private AppBootstrap app;
        private ShotController controller;
        private Transform burst;
        private LineRenderer line;
        private Material material;
        private float elapsed;
        public bool IsVisible {get;private set;}
        public int ShowCount {get;private set;}
        public float Radius {get;private set;}
        public float Alpha {get;private set;}
        public Color Tint {get;private set;}=ShotController.VolleyTint;
        public Transform Burst=>burst;
        public void Initialize(AppBootstrap app,ShotController controller)
        {
            if(this.app!=null)return;this.app=app;this.controller=controller;
            material=new Material(Resources.Load<Shader>("Legibility")){name="Volley burst"};material.SetColor("_BaseColor",Color.white);
            var go=new GameObject("Volley burst");go.transform.SetParent(transform,false);burst=go.transform;
            line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=Segments;
            line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;line.sharedMaterial=material;
            go.SetActive(false);
            app.Host.Launched+=OnLaunched;controller.PresentationDelivered+=OnDelivered;
        }
        private void OnLaunched(SquashClash.Core.Config.EffectiveConfig cfg){Hide();ShowCount=0;}
        private void OnDelivered(ShotPresentation p)
        {
            if(!p.HasContact||p.LaunchId!=app.Host.LaunchId||controller.LastOpportunity!=Opportunity.Volley)return;
            burst.position=p.ContactPosition;elapsed=0;IsVisible=true;ShowCount++;burst.gameObject.SetActive(true);Apply();
        }
        private void Hide(){IsVisible=false;elapsed=0;if(burst!=null)burst.gameObject.SetActive(false);}
        // Frozen under a pause, like the Perfect flash; real (unscaled) time otherwise.
        public void Tick(double dt)
        {
            if(!IsVisible||app.Clock.Paused)return;
            elapsed+=(float)System.Math.Max(0,dt);
            if(elapsed>=DurationSeconds){Hide();return;}
            Apply();
        }
        private void Update(){Tick(Time.unscaledDeltaTime);}
        private void Apply()
        {
            float k=Mathf.Clamp01(elapsed/DurationSeconds),ease=1-(1-k)*(1-k);
            Radius=StartRadius+(EndRadius-StartRadius)*ease;Alpha=1-k;
            var camera=UnityEngine.Camera.main;float width=FallbackWidth;
            if(camera!=null)
            {
                burst.rotation=camera.transform.rotation;
                var screen=camera.WorldToScreenPoint(burst.position);
                if(screen.z>0)width=Vector3.Distance(camera.ScreenToWorldPoint(screen),camera.ScreenToWorldPoint(screen+new Vector3(StrokePixels,0,0)));
            }
            line.widthMultiplier=width;line.startColor=line.endColor=new Color(Tint.r,Tint.g,Tint.b,Alpha);
            for(int i=0;i<Segments;i++)
            {
                float angle=i*Mathf.PI*2/Segments;
                line.SetPosition(i,new Vector3(Mathf.Cos(angle)*Radius,Mathf.Sin(angle)*Radius,0));
            }
        }
        private void OnDestroy()
        {
            if(app!=null)app.Host.Launched-=OnLaunched;if(controller!=null)controller.PresentationDelivered-=OnDelivered;
            if(material!=null)Destroy(material);
        }
    }
}
