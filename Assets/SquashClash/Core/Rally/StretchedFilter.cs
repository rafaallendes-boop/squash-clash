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
        // Safe nominal intentions keep their utilities. E1 is the least-late non-boast, with ties in input order.
        public static int Apply(double[] weights,StretchedEvaluation[] evaluations,int count,out double best)
        {
            best=double.NegativeInfinity;int leastLate=-1;long smallestDelay=long.MaxValue;bool reachable=false;
            for(int i=0;i<count;i++)
            {
                if(double.IsNegativeInfinity(weights[i]))continue;
                var evaluation=evaluations[i];
                if(!evaluation.IsBoast&&(leastLate<0||evaluation.Delay<smallestDelay))
                {leastLate=i;smallestDelay=evaluation.Delay;}
                if(evaluation.Good&&evaluation.Reachable)
                {reachable=true;if(weights[i]>best)best=weights[i];}
                else weights[i]=double.NegativeInfinity;
            }
            return reachable?-1:leastLate;
        }
    }
}
