using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    // Brief discreet flash at the delivery's contact position, Perfect with applied contact only. Pure view: no collider, no camera, clock or ball effect.
    public sealed class PerfectFlashView : MonoBehaviour
    {
        public const float DurationSeconds=.12f;
        private const float Radius=.09f;
        private AppBootstrap app;
        private ShotController controller;
        private Transform flash;
        private Mesh disc;
        private Material material;
        private float elapsed;
        public bool IsVisible {get;private set;}
        public int ShowCount {get;private set;}
        public Transform Flash=>flash;
        public void Initialize(AppBootstrap app,ShotController controller)
        {
            if(this.app!=null)return;this.app=app;this.controller=controller;
            disc=Disc();material=new Material(Resources.Load<Shader>("Legibility")){name="Perfect flash"};material.SetColor("_BaseColor",new Color(1,.95f,.65f,.9f));
            var go=new GameObject("Perfect flash",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);
            go.GetComponent<MeshFilter>().sharedMesh=disc;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
            flash=go.transform;flash.localScale=Vector3.one*Radius;go.SetActive(false);
            app.Host.Launched+=OnLaunched;controller.PresentationDelivered+=OnDelivered;
        }
        private void OnLaunched(SquashClash.Core.Config.EffectiveConfig cfg){Hide();ShowCount=0;}
        private void OnDelivered(ShotPresentation p)
        {
            if(p.Category!=PresentationCategory.Perfect||!p.HasContact||p.LaunchId!=app.Host.LaunchId)return;
            var cam=UnityEngine.Camera.main;flash.position=p.ContactPosition;flash.rotation=cam!=null?cam.transform.rotation:Quaternion.identity;
            elapsed=0;IsVisible=true;ShowCount++;flash.gameObject.SetActive(true);
        }
        private void Hide(){IsVisible=false;elapsed=0;if(flash!=null)flash.gameObject.SetActive(false);}
        public void Tick(double dt){if(!IsVisible)return;elapsed+=(float)dt;if(elapsed>=DurationSeconds)Hide();}
        private void Update(){Tick(Time.unscaledDeltaTime);}
        private static Mesh Disc()
        {
            const int sides=24;var vertices=new Vector3[sides+1];var colors=new Color[sides+1];var triangles=new int[sides*3];colors[0]=Color.white;
            for(int i=0;i<sides;i++)
            {
                float angle=2*Mathf.PI*i/sides;vertices[i+1]=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0);colors[i+1]=Color.white;
                triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%sides+1;
            }
            var mesh=new Mesh{name="Flash disc",vertices=vertices,colors=colors,triangles=triangles};mesh.RecalculateBounds();return mesh;
        }
        private void OnDestroy()
        {
            if(app!=null)app.Host.Launched-=OnLaunched;if(controller!=null)controller.PresentationDelivered-=OnDelivered;
            if(disc!=null)Destroy(disc);if(material!=null)Destroy(material);
        }
    }
}
