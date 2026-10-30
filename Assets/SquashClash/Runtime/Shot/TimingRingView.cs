using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    public sealed class TimingRingView : MonoBehaviour
    {
        private LineRenderer line;
        private Material material;
        private Transform ring;
        private ContactPlan plan;
        private double target,presented;
        public bool Visible=>line!=null&&line.enabled;
        public void Show(ContactPlan plan,double targetContactAtReal,bool visible)
        {
            if(line==null)
            {
                ring=new GameObject("First Shot timing ring").transform;ring.SetParent(transform,false);
                line=ring.gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=48;line.widthMultiplier=.035f;
                line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
                material=new Material(Resources.Load<Shader>("Legibility"));material.SetColor("_BaseColor",Color.white);line.sharedMaterial=material;
            }
            this.plan=plan;target=targetContactAtReal;presented=Time.realtimeSinceStartupAsDouble;
            line.enabled=visible&&plan.Exists;ring.position=plan.PStar;Tick(presented);
        }
        public void Tick(double now)
        {
            if(!Visible)return;
            var camera=UnityEngine.Camera.main;if(camera!=null)ring.rotation=camera.transform.rotation;
            float remaining=(float)System.Math.Max(0,target-now),total=(float)System.Math.Max(double.Epsilon,target-presented);
            float radius=.1f+.35f*Mathf.Clamp01(remaining/total);
            line.startColor=line.endColor=now<=target?Color.cyan:Color.yellow;
            for(int i=0;i<line.positionCount;i++){float angle=i*Mathf.PI*2/line.positionCount;line.SetPosition(i,new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius,0));}
        }
        private void OnDestroy(){if(ring!=null)Destroy(ring.gameObject);if(material!=null)Destroy(material);}
    }
}
