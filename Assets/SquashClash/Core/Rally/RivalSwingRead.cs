using System;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Core.Rally
{
    // [EXPERIMENTO] set21 (27 nov; local branch, never shipped): the rival reads the human's swing and starts running before the contact.
    public enum SwingReadKind { Right=1, Direction=2, Length=3 }
    public readonly struct SwingRead
    {
        // Index in RallyOutcome.Receptions of the rival reception that follows the read human stroke.
        public readonly int RivalReception;public readonly SwingReadKind Kind;
        // Actual steps between the read and the human contact.
        public readonly int LeadSteps;public readonly long ReadStep,ContactStep;
        // False when the read shot could not be solved: the rival did not move.
        public readonly bool Planned;public readonly Vector2 Stance,BodyAtRead;
        public SwingRead(int rivalReception,SwingReadKind kind,int leadSteps,long readStep,long contactStep,bool planned,Vector2 stance,Vector2 bodyAtRead)
        {RivalReception=rivalReception;Kind=kind;LeadSteps=leadSteps;ReadStep=readStep;ContactStep=contactStep;Planned=planned;Stance=stance;BodyAtRead=bodyAtRead;}
    }
    public static class RivalSwingRead
    {
        public static int LeadSteps(RallyContext c)=>c.Params.RivalSwingReadLeadMs<=0?0:(int)Math.Round(c.Params.RivalSwingReadLeadMs/1000*c.Tempo*c.StepHz);
        private static int Bits(float f)=>BitConverter.ToInt32(BitConverter.GetBytes(f),0);
        // Own generator seeded by the contact, so no brain's RNG is ever consumed.
        public static SwingReadKind Kind(RallyContext c,long contactStep,Vector3 contactPoint)
        {
            long seed=unchecked(contactStep*0x9E3779B1L+Bits(contactPoint.x)*73856093L^Bits(contactPoint.y)*19349663L^Bits(contactPoint.z)*83492791L);
            var rng=new RallyRng(seed);
            if(rng.NextDouble()>=c.Params.RivalSwingMisreadP)return SwingReadKind.Right;
            return rng.NextDouble()<.5?SwingReadKind.Direction:SwingReadKind.Length;
        }
        public static bool IsLong(in AiShot shot)=>shot.Option.Line!=ShotLine.Drop&&shot.Option.Line!=ShotLine.Boast&&shot.Option.Height!=ShotHeight.Low;
        // Where the rival runs for the read intention; null when that shot cannot be solved. stepsUntilContact: steps left before the human contact.
        public static Vector2? Stance(RallyContext c,Vector3 contact,in AiShot shot,SwingReadKind kind,PlayerBody rival,long contactStep,int stepsUntilContact)
        {
            ShotSolution solution;
            if(kind==SwingReadKind.Length)
            {
                bool isLong=IsLong(shot);ShotOption? read=null;
                foreach(var o in ShotOptions.Build(c,contact))
                    if(isLong?o.Line==ShotLine.Drop&&o.Height==ShotHeight.Low&&o.Power==ShotPower.Medium:o.Line==ShotLine.Straight&&o.Height==ShotHeight.Mid&&o.Power==ShotPower.Strong){read=o;break;}
                if(!read.HasValue)return null;
                solution=RallyContact.SolveIntention(c,contact,read.Value.Aim,read.Value.P);
            }
            else solution=shot.Option.Side.Exists?RallyContact.SolveBoastIntention(c,contact,shot.Option.Side,shot.P,out _):RallyContact.SolveIntention(c,contact,shot.Aim,shot.P);
            if(!solution.Solved)return null;
            var t=c.Predict(new BallState{Position=contact,Velocity=solution.Velocity,Step=contactStep});
            var rules=c.NewRules();rules.Begin(PlayerId.Human,PlayerId.Rival);
            // A negative elapsedSteps is the time still left before the trajectory starts.
            var plan=ReceptionPlanner.Plan(c,t,rules,rival,0,0,default,-stepsUntilContact);
            var stance=plan.Stance;
            return kind==SwingReadKind.Direction?c.ClampBody(new Vector2(c.Court.Width-stance.x,stance.y)):stance;
        }
    }
}
