using System;
namespace SquashClash.AutoQA
{
    internal static class Determinism
    {
        internal static bool Equivalent(PocObservation left,PocObservation right)
        {
            if(left==null||right==null||left.exactShot!=right.exactShot||left.additional!=right.additional||left.relativeMs==null||right.relativeMs==null||left.relativeMs.Length!=right.relativeMs.Length)return false;
            for(int i=0;i<left.relativeMs.Length;i++)
                if(double.IsNaN(left.relativeMs[i])||double.IsInfinity(left.relativeMs[i])||double.IsNaN(right.relativeMs[i])||double.IsInfinity(right.relativeMs[i])||Math.Abs(left.relativeMs[i]-right.relativeMs[i])>PocObservation.TimeToleranceMs)return false;
            return true;
        }
    }
}
