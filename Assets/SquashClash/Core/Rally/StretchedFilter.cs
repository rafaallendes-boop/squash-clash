namespace SquashClash.Core.Rally
{
    // Facts about one allowed intention, in the same order as the decision's utility weights.
    public readonly struct StretchedEvaluation
    {
        public readonly bool Good,Reachable,IsBoast;
        public readonly long Delay;
        public StretchedEvaluation(bool good,bool reachable,long delay,bool isBoast)
        {Good=good;Reachable=reachable;Delay=delay;IsBoast=isBoast;}
    }
    public static class StretchedFilter
    {
        // RED skeleton required by B3c. No filtering has been implemented yet.
        public static int Apply(double[] weights,StretchedEvaluation[] evaluations,int count,out double best)
        {best=double.NegativeInfinity;return -1;}
    }
}
