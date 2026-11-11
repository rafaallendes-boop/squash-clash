using System;
using System.Collections.Generic;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    public enum ShotLine { Straight, Cross, Drop }
    public enum ShotHeight { Low, Mid, High }
    public enum ShotPower { Medium, Strong }
    public readonly struct ShotOption
    {
        public readonly ShotLine Line;public readonly ShotHeight Height;public readonly ShotPower Power;
        public readonly Vector2 Aim;public readonly float P;
        // Floor zone the shot is meant to reach (x, z): the "away from the opponent" feature.
        public readonly Vector2 Zone;
        public ShotOption(ShotLine line,ShotHeight height,ShotPower power,Vector2 aim,float p,Vector2 zone){Line=line;Height=height;Power=power;Aim=aim;P=p;Zone=zone;}
        public bool Long=>Line!=ShotLine.Drop&&Height!=ShotHeight.Low;
    }
    // Spec 11.3: {straight, cross, drop} x {low, mid, high} x {medium, strong}, as intentions (aim on the front wall, power).
    public static class ShotOptions
    {
        private const float Margin=.01f;
        public static List<ShotOption> Build(RallyContext context,Vector3 contact)
        {
            var p=context.Params;var court=context.Court;var options=new List<ShotOption>(18);
            float inset=(float)p.CornerInset;bool nearIsRight=contact.x<court.Width*.5f;
            float nearX=nearIsRight?inset:court.Width-inset,farX=nearIsRight?court.Width-inset:inset;
            // Mirror the back corner across the front wall: the straight line to it crosses the wall at the aim x.
            float Through(float cornerX)=>contact.x+(cornerX-contact.x)*contact.z/(contact.z+court.Length);
            var deep=court.Length-1f;
            foreach(ShotLine line in new[]{ShotLine.Straight,ShotLine.Cross,ShotLine.Drop})
                foreach(ShotHeight height in new[]{ShotHeight.Low,ShotHeight.Mid,ShotHeight.High})
                    foreach(ShotPower power in new[]{ShotPower.Medium,ShotPower.Strong})
                    {
                        Vector2 aim,zone;float strength;
                        if(line==ShotLine.Drop)
                        {
                            float x=Mathf.Lerp(contact.x,nearX,.7f);
                            aim=new Vector2(x,(float)p.DropHeight+(int)height*.22f);zone=new Vector2(x,1.5f);
                            strength=(float)p.PowerDrop+(power==ShotPower.Strong?.15f:0);
                        }
                        else
                        {
                            float corner=line==ShotLine.Straight?nearX:farX;
                            aim=new Vector2(Through(corner),(float)(height==ShotHeight.Low?p.AimLow:height==ShotHeight.Mid?p.AimMid:p.AimHigh));
                            zone=new Vector2(corner,deep);strength=(float)(power==ShotPower.Strong?p.PowerStrong:p.PowerMedium);
                        }
                        // Cap the launch elevation: the closer to the front wall, the lower the aim.
                        float distance=Mathf.Sqrt((aim.x-contact.x)*(aim.x-contact.x)+contact.z*contact.z);
                        aim.y=Mathf.Min(aim.y,contact.y+Mathf.Tan((float)p.MaxAimElevationDeg*Mathf.Deg2Rad)*distance);
                        options.Add(new ShotOption(line,height,power,ClampAim(context,aim),Mathf.Clamp01(strength),zone));
                    }
            return options;
        }
        // The shared solver only accepts aims between the tin and the out line; intentions are clipped to that domain.
        public static Vector2 ClampAim(RallyContext context,Vector2 aim)
        {
            var court=context.Court;float r=context.Physics.BallRadius+Margin;
            return new Vector2(Mathf.Clamp(aim.x,r,court.Width-r),Mathf.Clamp(aim.y,court.TinTop+r,court.FrontOutLower-r));
        }
    }
}
