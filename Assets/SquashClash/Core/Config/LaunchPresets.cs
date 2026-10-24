using System;
using System.Collections.Generic;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Config
{
    public static class LaunchPresets
    {
        public sealed class Entry { public string Id, Label; public LaunchSpec Launch; public Vector2 HumanXZ, RivalXZ; }
        public static IReadOnlyList<Entry> All(CourtDimensions d, float gravity)
        {
            var human = new Vector2(d.Width / 2, 5.5f); var rival = new Vector2(4.5f, 6.5f);
            return Array.AsReadOnly(new[] {
                Aim("L1", "Drive largo", new Vector3(1,1.2f,7), 1,1.2f,30,human,rival,gravity),
                Aim("L2", "Baja junto a lateral", new Vector3(.3f,.35f,4), .3f,.35f,18,human,rival,gravity),
                Aim("L3", "Drop corto", new Vector3(2,1,3), 2,.7f,5,human,rival,gravity),
                Aim("L4", "Globo alto", new Vector3(d.Width/2,1,7), d.Width/2,4.2f,15,human,rival,gravity),
                Aim("L5", "Detrás del humano", new Vector3(d.Width/2,1.1f,5), d.Width/2,.9f,12,new Vector2(d.Width/2,2.5f),rival,gravity),
                Aim("L6", "Esquina trasera", new Vector3(.3f,1.3f,7), 3.64f,1.5f,30,human,new Vector2(d.Width-.3f,d.Length-.65f),gravity)
            });
        }
        private static Entry Aim(string id,string label,Vector3 start,float targetX,float targetY,float horizontalSpeed,
            Vector2 human,Vector2 rival,float gravity)
        {
            // Physical radius is invariant; visual scale never enters the ballistic solution.
            float radius = new PhysicsParams().BallRadius;
            var center = Ballistics.CenterForContact(SurfaceId.FrontWall,new Vector3(targetX,targetY,0),radius);
            var velocity = Ballistics.SolveCenterTo(start,center,horizontalSpeed,gravity);
            return new Entry { Id=id,Label=label,Launch=new LaunchSpec(start,velocity.normalized,velocity.magnitude),HumanXZ=human,RivalXZ=rival };
        }
    }
}
