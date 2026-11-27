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
        // RED stubs: the real bodies come in the next commit.
        public static int LeadSteps(RallyContext c)=>0;
        public static SwingReadKind Kind(RallyContext c,long contactStep,Vector3 contactPoint)=>SwingReadKind.Right;
        public static bool IsLong(in AiShot shot)=>shot.Option.Line!=ShotLine.Drop&&shot.Option.Line!=ShotLine.Boast&&shot.Option.Height!=ShotHeight.Low;
        public static Vector2? Stance(RallyContext c,Vector3 contact,in AiShot shot,SwingReadKind kind,PlayerBody rival,long contactStep,int stepsUntilContact)=>null;
    }
}
