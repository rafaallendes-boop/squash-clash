using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Rally;
using UnityEngine;
namespace SquashClash.Runtime.Visual
{
    // Rival serve ritual (spec 2026-11-24): draws a SEPARATE ball (same mesh, material and halo as the real one) while the pause before the rival's
    // serve plays. View only: no collider, no body, not a BallView, never reads or writes the host. The real ball is hidden meanwhile.
    public sealed class RivalServeRitualView : MonoBehaviour
    {
        private RallyDirector rally;private BallView ballView;private Transform halo;private UnityEngine.Camera view;
        public bool Showing {get;private set;}
        public Transform Ball {get;private set;}
        public void Initialize(AppBootstrap app,RallyDirector rally,BallView ballView,LegibilityAids aids)
        {
            this.rally=rally;this.ballView=ballView;
            var go=new GameObject("Serve ritual ball",typeof(MeshFilter),typeof(MeshRenderer));Ball=go.transform;
            var source=ballView.GetComponent<MeshRenderer>();
            go.GetComponent<MeshFilter>().sharedMesh=ballView.GetComponent<MeshFilter>().sharedMesh;
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=source.sharedMaterial;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            var sourceHalo=ballView.transform.Find("Halo");
            if(sourceHalo!=null)
            {
                var h=VisualKit.Part("Halo",Ball,sourceHalo.GetComponent<MeshFilter>().sharedMesh,sourceHalo.GetComponent<MeshRenderer>().sharedMaterial);
                halo=h.transform;halo.localScale=Vector3.one*BallView.OutlineScale;
            }
            go.SetActive(false);
            aids.StandIn=()=>rally.RivalServeRitualBall(out var b)?b:(Vector3?)null;
        }
        private void LateUpdate()=>Refresh();
        public void Refresh()
        {
            if(rally==null||PointEndFreeze.Active)return;
            Showing=rally.RivalServeRitualBall(out var p);
            if(Ball.gameObject.activeSelf!=Showing)Ball.gameObject.SetActive(Showing);
            ballView.SetHidden(Showing);
            if(!Showing)return;
            Ball.position=p;Ball.localScale=ballView.transform.lossyScale;
            if(halo!=null){if(view==null)view=UnityEngine.Camera.main;if(view!=null)halo.rotation=view.transform.rotation;}
        }
        private void OnDestroy(){if(Ball!=null)Destroy(Ball.gameObject);}
    }
}
