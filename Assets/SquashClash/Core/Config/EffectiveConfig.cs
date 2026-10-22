using System;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;

namespace SquashClash.Core.Config
{
    // Immutable source; mutable geometry/physics DTOs are derived as independent copies.
    public sealed class EffectiveConfig
    {
        public ParamSet Source { get; }
        public string Hash => Source.Hash;
        public string SimulationKey { get; }
        public LaunchSpec Launch
        {
            get
            {
                double yaw = Source.Get("launcher.yawDeg") * Math.PI / 180, pitch = Source.Get("launcher.pitchDeg") * Math.PI / 180;
                // AX-1: the default rear camera projects -X toward screen right.
                var direction = new UnityEngine.Vector3((float)(-Math.Sin(yaw)*Math.Cos(pitch)), (float)Math.Sin(pitch), (float)(-Math.Cos(yaw)*Math.Cos(pitch)));
                return new LaunchSpec(new UnityEngine.Vector3((float)Source.Get("launcher.x"),(float)Source.Get("launcher.y"),(float)Source.Get("launcher.z")), direction, (float)Source.Get("launcher.speed"));
            }
        }
        public string Backend => Source.GetInt("physics.backend") == 0 ? "P1-PhysX" : throw new InvalidOperationException("Backend not authorized.");
        public double Tempo => Source.Get("pace.tempo");
        public double LaunchScale => Source.Get("pace.launchScale");
        public PhysicsParams Physics => new PhysicsParams {
            Gravity = (float)Source.Get("physics.gravity"), StepHz = Source.GetInt("physics.stepHz"),
            MaxImpactsPerStep = Source.GetInt("physics.maxImpactsPerStep"),
            Floor = Material("floor"), FrontWall = Material("front"), SideWalls = Material("side"), BackWall = Material("back")
        };
        // CourtDimensions is mutable for geometry authoring; consumers receive their own copy.
        public CourtDimensions Court => CourtDimensions.WithTinTop((float)Source.Get("court.tinTop"));

        private SurfaceMaterial Material(string surface) => new SurfaceMaterial {
            Restitution = (float)Source.Get("physics." + surface + ".restitution"),
            TangentialFriction = (float)Source.Get("physics." + surface + ".friction")
        };
        private EffectiveConfig(ParamSet source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            SimulationKey = source.HashOf("physics.", "court.");
        }
        public static EffectiveConfig From(ParamSet source) => new EffectiveConfig(source);
    }
}
