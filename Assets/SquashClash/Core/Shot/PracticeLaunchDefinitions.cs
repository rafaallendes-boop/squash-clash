using System;
using System.Collections.Generic;
using UnityEngine;
using SquashClash.Core.Court;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
namespace SquashClash.Core.Shot
{
    public readonly struct PracticeLaunch
    {
        public readonly string Id;public readonly BallState Initial;public readonly BodyFrame Body;
        // The recipe offers the volley opportunity too (VL1 only).
        public readonly bool Volley;
        public PracticeLaunch(string id,BallState initial,BodyFrame body,bool volley=false){Id=id;Initial=initial;Body=body;Volley=volley;}
    }
    public static class PracticeLaunchDefinitions
    {
        // Frozen initial EXPERIMENTO recipes predeclared in the phase1 report before evaluation.
        // practice.launch index: SL1-SL6 (0-5) and VL1 (6), the volley recipe of the AutoQA volley suite (design 2026-11-18).
        public static PracticeLaunch At(EffectiveConfig config,int index)=>index==VolleyIndex?Volley(config):Create(config)[index];
        public const int VolleyIndex=6;
        // VL1: a high feed from the front court that passes the fixed body before bouncing deep and comes back after the bounce.
        public static PracticeLaunch Volley(EffectiveConfig config)
        {
            float mid=config.Court.Width*.5f;var position=new Vector3(mid-.6f,1.2f,5);
            var center=Ballistics.CenterForContact(SurfaceId.FrontWall,new Vector3(position.x,4.2f,0),config.Physics.BallRadius);
            return new PracticeLaunch("VL1",new BallState{Position=position,Velocity=Ballistics.SolveCenterTo(position,center,15,config.Physics.Gravity)},
                BodyFrame.From(config.Source,new Vector2(mid-1.2f,8.25f)),true);
        }
        public static IReadOnlyList<PracticeLaunch> Create(EffectiveConfig config)
        {
            float mid=config.Court.Width*.5f;
            PracticeLaunch Make(string id,float x,float y,float z,float targetY,float horizontal,Vector2 body)
            {
                var position=new Vector3(x,y,z);
                var center=Ballistics.CenterForContact(SurfaceId.FrontWall,new Vector3(x,targetY,0),config.Physics.BallRadius);
                return new PracticeLaunch(id,new BallState{Position=position,Velocity=Ballistics.SolveCenterTo(position,center,horizontal,config.Physics.Gravity)},BodyFrame.From(config.Source,body));
            }
            return Array.AsReadOnly(new[]{
                Make("SL1",mid-.6f,1.2f,7,2.2f,15,new Vector2(mid,5.5f)),
                Make("SL2",mid+.6f,1.2f,7,2.2f,15,new Vector2(mid,5.5f)),
                Make("SL3",.35f,1.3f,7,1.5f,18,new Vector2(1.3f,8.3f)),
                Make("SL4",config.Court.Width-.35f,1.3f,7,1.5f,18,new Vector2(config.Court.Width-1.3f,8.3f)),
                Make("SL5",mid-.6f,1,3,.7f,6,new Vector2(mid,1.6f)),
                Make("SL6",mid+.6f,1,3,.7f,6,new Vector2(mid,1.6f))});
        }
    }
}
