using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    public sealed class AimReticleView : MonoBehaviour
    {
        // View-only dimensions: >=60 px minor diameter and >=6 px stroke at the
        // approved default camera / iPhone 16 Pro viewport. No tuning keys.
        private const float RadiusMeters=.42f,StrokeMeters=.08f;
        private LineRenderer line;private Material material;
        private IReadOnlyList<LineBand> bands;private float tin,outLine,lineWidth,width;
        private readonly List<LineId> touched=new List<LineId>(2);
        public AimState State {get;private set;}
        public Vector2 Aim {get;private set;}
        public bool IsInvalid {get;private set;}
        public Color Tint {get;private set;}
        public void SetAim(Vector2 aim,AimState state,CourtDimensions court)
        {
            if(line==null)
            {
                line=gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.widthMultiplier=StrokeMeters;
                line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
                material=new Material(Resources.Load<Shader>("Legibility"));material.SetColor("_BaseColor",Color.white);line.sharedMaterial=material;
            }
            Aim=aim;State=state;transform.position=new Vector3(aim.x,aim.y,.035f);
            if(bands==null||tin!=court.TinTop||outLine!=court.FrontOutLower||lineWidth!=court.LineWidth||width!=court.Width)
            {bands=CourtLines.Build(court);tin=court.TinTop;outLine=court.FrontOutLower;lineWidth=court.LineWidth;width=court.Width;}
            LineContactClassifier.Overlapping(bands,SurfaceId.FrontWall,aim,.01f,touched);
            IsInvalid=aim.y<=court.TinTop||aim.y>=court.FrontOutLower||touched.Contains(LineId.Tin)||touched.Contains(LineId.FrontOut);
            Tint=IsInvalid?Color.red:state==AimState.Frozen?Color.yellow:Color.cyan;
            line.startColor=line.endColor=Tint;
            int count=state==AimState.Frozen?5:33;line.positionCount=count;
            for(int i=0;i<count;i++)
            {
                float angle=i*Mathf.PI*2/(count-1);
                line.SetPosition(i,new Vector3(Mathf.Cos(angle)*RadiusMeters,Mathf.Sin(angle)*RadiusMeters,0));
            }
        }
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
