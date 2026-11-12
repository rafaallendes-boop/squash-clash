using System;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    public enum BodyPhase { Waiting, Approaching, Set, Recovering }
    public readonly struct MoveSettings
    {
        public readonly float Accel,MaxSpeed,Brake,ArriveEpsilon;public readonly int StepHz;
        public MoveSettings(float accel,float maxSpeed,float brake,float arriveEpsilon,int stepHz)
        {
            if(!(accel>0)||!(maxSpeed>0)||!(brake>0)||!(arriveEpsilon>0)||stepHz<=0)throw new ArgumentOutOfRangeException(nameof(accel));
            Accel=accel;MaxSpeed=maxSpeed;Brake=brake;ArriveEpsilon=arriveEpsilon;StepHz=stepHz;
        }
    }
    // Kinematic character on the floor plane (x, z). It advances only by simulation steps: one clock for ball and bodies.
    // Read-only state for views: Position, Velocity, Speed, Phase, Target, Reacting.
    public sealed class PlayerBody
    {
        private readonly MoveSettings settings;
        private BodyPhase arrivedPhase,pendingMoving,pendingArrived;
        private Vector2 pendingTarget;
        private int pendingDelay;
        public PlayerId Id {get;}
        public Vector2 Position {get;private set;}
        public Vector2 Velocity {get;private set;}
        public BodyPhase Phase {get;private set;}
        public Vector2 Target {get;private set;}
        // True during the reaction delay: the new target is known but the body has not turned to it yet.
        public bool Reacting=>pendingDelay>0;
        public float Speed=>Velocity.magnitude;
        public PlayerBody(PlayerId id,Vector2 start,MoveSettings settings)
        {Id=id;this.settings=settings;Position=Target=start;Phase=arrivedPhase=BodyPhase.Waiting;}
        private PlayerBody(PlayerBody other)
        {
            Id=other.Id;settings=other.settings;Position=other.Position;Velocity=other.Velocity;Phase=other.Phase;Target=other.Target;
            arrivedPhase=other.arrivedPhase;pendingMoving=other.pendingMoving;pendingArrived=other.pendingArrived;pendingTarget=other.pendingTarget;pendingDelay=other.pendingDelay;
        }
        public void Teleport(Vector2 position)
        {Position=Target=position;Velocity=Vector2.zero;Phase=arrivedPhase=BodyPhase.Waiting;pendingDelay=0;}
        // After delaySteps the body turns to target. Until then it keeps whatever it was doing.
        public void MoveTo(Vector2 target,int delaySteps,BodyPhase moving,BodyPhase arrived)
        {
            if(delaySteps<0)throw new ArgumentOutOfRangeException(nameof(delaySteps));
            pendingTarget=target;pendingMoving=moving;pendingArrived=arrived;pendingDelay=delaySteps;
            if(delaySteps==0)Turn();
        }
        private void Turn(){Target=pendingTarget;Phase=pendingMoving;arrivedPhase=pendingArrived;pendingDelay=0;}
        public void Step()
        {
            if(pendingDelay>0&&--pendingDelay==0)Turn();
            var to=Target-Position;float distance=to.magnitude;
            if(distance<=settings.ArriveEpsilon){Position=Target;Velocity=Vector2.zero;Phase=arrivedPhase;return;}
            var direction=to/distance;float dt=1f/settings.StepHz;
            // Speed along the new heading only: a turn sheds lateral speed (no extra assist either way).
            float speed=Mathf.Max(0,Vector2.Dot(Velocity,direction));
            float desired=Mathf.Min(settings.MaxSpeed,Mathf.Sqrt(2*settings.Brake*distance));
            speed=speed<desired?Mathf.Min(desired,speed+settings.Accel*dt):Mathf.Max(desired,speed-settings.Brake*dt);
            float travel=speed*dt;
            if(travel>=distance){Position=Target;Velocity=Vector2.zero;Phase=arrivedPhase;return;}
            Position+=direction*travel;Velocity=direction*speed;
        }
        // Simulates a copy with the same code as Step: prediction and execution cannot diverge.
        public int StepsToReach(Vector2 target,int delaySteps,out Vector2 positionAtLimit,int limitSteps=int.MaxValue)
        {
            var copy=new PlayerBody(this);copy.MoveTo(target,delaySteps,BodyPhase.Approaching,BodyPhase.Set);
            int steps=0,cap=Math.Min(limitSteps,settings.StepHz*30);
            while(steps<cap&&!(copy.pendingDelay==0&&copy.Phase==BodyPhase.Set)){copy.Step();steps++;}
            positionAtLimit=copy.Position;
            return copy.pendingDelay==0&&copy.Phase==BodyPhase.Set?steps:int.MaxValue;
        }
        public Vector2 PositionAfter(Vector2 target,int delaySteps,int steps)
        {
            var copy=new PlayerBody(this);copy.MoveTo(target,delaySteps,BodyPhase.Approaching,BodyPhase.Set);
            for(int i=0;i<steps;i++)copy.Step();
            return copy.Position;
        }
    }
}
