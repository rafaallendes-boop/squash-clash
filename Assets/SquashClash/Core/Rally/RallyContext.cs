using System;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    // Immutable bundle shared by both players: one court, one physics, one contract, one set of limits (spec 17.A.9).
    public sealed class RallyContext
    {
        public EffectiveConfig Config {get;}
        public RallyParams Params {get;}
        public CourtDimensions Court {get;}
        public PhysicsParams Physics {get;}
        public LegalSettings Legal {get;}
        public TimingSettings Timing {get;}
        public IBallSimulationFactory Factory {get;}
        public float VMin {get;}
        public float VMax {get;}
        public float VCap {get;}
        public ShotPaceSettings Pace {get;}
        public int StepHz=>Physics.StepHz;
        public double Tempo=>Config.Tempo;
        public Vector2 T {get;}
        public RallyContext(EffectiveConfig config,RallyParams parameters,IBallSimulationFactory factory)
        {
            Config=config??throw new ArgumentNullException(nameof(config));Params=parameters??throw new ArgumentNullException(nameof(parameters));
            Factory=factory??throw new ArgumentNullException(nameof(factory));
            if(config.Backend!="P2-Analytic")throw new InvalidOperationException("First Rally requires the P2-Analytic backend.");
            Court=config.Court;Physics=config.Physics;var source=config.Source;
            Legal=new LegalSettings((float)source.Get("legal.minHeight"),(float)source.Get("legal.maxHeight"),Physics.StepHz,Physics.Gravity,Physics.BallRadius,Court);
            Timing=TimingSettings.From(source);
            VMin=(float)source.Get("shot.vMin");VMax=(float)source.Get("shot.vMax");VCap=(float)source.Get("shot.vCap");Pace=ShotPaceSettings.From(source);
            T=new Vector2(Court.Width*.5f,Court.Length-Court.ShortLineNearBackEdgeFromBack-Court.LineWidth*.5f);
        }
        public int Steps(double simSeconds)=>(int)Math.Round(simSeconds*StepHz);
        public MoveSettings Move=>new MoveSettings((float)Params.Accel,(float)Params.MaxSpeed,(float)Params.Brake,(float)Params.ArriveEpsilon,StepHz);
        // The same body template for both players; only the position differs.
        public BodyFrame FrameAt(Vector2 position)=>BodyFrame.From(Config.Source,position);
        public Vector2 WaitPosition(PlayerId who)=>ClampBody(T+(who==PlayerId.Human
            ?new Vector2(-(float)Params.WaitLateralOffset,(float)Params.HumanWaitBehindT)
            :new Vector2((float)Params.WaitLateralOffset,(float)Params.RivalWaitBehindT)));
        public Vector2 ClampBody(Vector2 position)
        {
            float margin=(float)Params.BodyWallMargin;
            return new Vector2(Mathf.Clamp(position.x,margin,Court.Width-margin),Mathf.Clamp(position.y,margin,Court.Length-margin));
        }
        // Isolated prediction with the same P2 code and step as the game; it never touches the live ball.
        public Trajectory Predict(in BallState from,double horizonSeconds=10)
        {
            using(var simulation=Factory.Create(Physics,Court))
                return TrajectoryRunner.Run(simulation,from,Steps(horizonSeconds),StopCondition.SecondFloorBounce,Court);
        }
        private readonly BallState?[] serves=new BallState?[2];
        // The automatic serve from a box; found once per context.
        public BallState Serve(ServiceBox box)
        {
            if(!serves[(int)box].HasValue)serves[(int)box]=ServePlanner.Find(this,box);
            return serves[(int)box].Value;
        }
        public BallState NeutralLaunch(PlayerId receiver)
        {
            float side=receiver==PlayerId.Human?-1:1;
            var position=new Vector3(Court.Width*.5f+side*(float)Params.LaunchLateral,(float)Params.LaunchHeight,(float)Params.LaunchZ);
            var center=Ballistics.CenterForContact(SurfaceId.FrontWall,new Vector3(position.x,(float)Params.LaunchTargetHeight,0),Physics.BallRadius);
            return new BallState{Position=position,Velocity=Ballistics.SolveCenterTo(position,center,(float)Params.LaunchHorizontalSpeed,Physics.Gravity)};
        }
    }
}
