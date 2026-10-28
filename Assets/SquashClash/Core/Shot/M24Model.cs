using System;
using SquashClash.Core.Court;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    // Preview state only. Receives the existing touch stream and a captured view.
    public sealed class M24Model
    {
        private readonly SlingSettings settings;private readonly float scale;private int finger;private bool began;
        public readonly FrontisProjection Projection;
        public M24Model(SlingSettings settings,FrontisProjection projection,float mmPerPx)
        {
            if(projection==null||!Finite(mmPerPx)||mmPerPx<=0||settings.FullMm<=settings.DeadMm||settings.DeadMm<0)throw new ArgumentOutOfRangeException(nameof(mmPerPx));
            this.settings=settings;Projection=projection;scale=mmPerPx;
        }
        public bool Active {get;private set;}
        public bool HasAim {get;private set;}
        public bool Invalid {get;private set;}
        public bool EverOutsideDeadZone {get;private set;}
        public bool Conservative {get;private set;}
        public bool TapCandidate {get;private set;}
        public Vector2 Aim {get;private set;}
        public Vector2 Anchor {get;private set;}
        public float P {get;private set;}
        public float DistanceMm {get;private set;}
        public float MaxDistanceMm {get;private set;}
        public float MaxDisplacementMm {get;private set;}
        public float LateralMm {get;private set;}
        public double BeganAt {get;private set;}=double.NaN;
        public double ReleaseAt {get;private set;}=double.NaN;
        public double TerminalAt {get;private set;}=double.NaN;
        public GestureOutcome Outcome {get;private set;}
        public string Reason {get;private set;}="none";
        public string Phase=>!Active?Outcome.ToString():Invalid?"Inválida":DistanceMm>settings.DeadMm?"Tensando":EverOutsideDeadZone?"Retorno sin potencia":"Fijada";
        public void Push(in TouchSample sample)
        {
            if(Outcome!=GestureOutcome.None)return;
            if(sample.Phase==TouchPhaseKind.Began)
            {
                if(began)return;
                began=Active=true;finger=sample.FingerId;Anchor=sample.PositionPx;BeganAt=sample.TimeReal;
                if(!Projection.TryMap(Anchor,out var aim,out var reason))
                {Active=false;Outcome=GestureOutcome.Rejected;Reason=reason;TerminalAt=sample.TimeReal;return;}
                Aim=aim;HasAim=true;Invalid=Projection.IsInvalid(aim);Reason=Invalid?"invalid-aim":"began-fixed";
            }
            else if(!Active||sample.FingerId!=finger)return;
            DistanceMm=Mathf.Max(0,(Anchor.y-sample.PositionPx.y)*scale);
            LateralMm=(sample.PositionPx.x-Anchor.x)*scale;
            MaxDistanceMm=Mathf.Max(MaxDistanceMm,DistanceMm);
            MaxDisplacementMm=Mathf.Max(MaxDisplacementMm,Vector2.Distance(Anchor,sample.PositionPx)*scale);
            EverOutsideDeadZone|=DistanceMm>settings.DeadMm;
            P=Invalid?0:Mathf.Clamp01((DistanceMm-settings.DeadMm)/(settings.FullMm-settings.DeadMm));
            if(sample.Phase!=TouchPhaseKind.Ended)return;
            ReleaseAt=TerminalAt=sample.TimeReal;Active=false;
            double duration=(ReleaseAt-BeganAt)*1000;
            TapCandidate=MaxDisplacementMm<settings.TapMm&&duration>=0&&duration<settings.TapMs;
            Outcome=Invalid?GestureOutcome.InvalidAim:TapCandidate?GestureOutcome.Tap:
                DistanceMm<=settings.DeadMm?EverOutsideDeadZone?GestureOutcome.CanceledReturnToDeadZone:GestureOutcome.AbandonedNoPull:GestureOutcome.Swing;
            Reason=Outcome.ToString();
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
        public void Cancel(double now,string reason="context") { }
    }
}
