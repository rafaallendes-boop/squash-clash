using NUnit.Framework;
using SquashClash.Core.Rally;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class StretchedFilterTests
    {
        [TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)]
        public void StretchedFilter_Apply_RejectsUnreachable_IncludingBoast(int scenario)
        {
            double[] weights;StretchedEvaluation[] evaluations;int[] rejected;int expected;double best;
            switch(scenario)
            {
                case 1:
                    weights=new double[]{1,100};
                    evaluations=new[]{new StretchedEvaluation(true,true,3,false),new StretchedEvaluation(true,false,1,true)};
                    rejected=new[]{1};expected=-1;best=1;break;
                case 2:
                    weights=new double[]{1,100};
                    evaluations=new[]{new StretchedEvaluation(true,false,10,false),new StretchedEvaluation(true,true,1,true)};
                    rejected=new[]{0};expected=-1;best=100;break;
                case 3:
                    weights=new double[]{100,5,2};
                    evaluations=new[]{new StretchedEvaluation(true,false,1,true),new StretchedEvaluation(true,false,8,false),new StretchedEvaluation(true,false,5,false)};
                    rejected=new[]{0,1,2};expected=2;best=double.NegativeInfinity;break;
                default:
                    weights=new double[]{100,1};
                    evaluations=new[]{new StretchedEvaluation(false,true,0,false),new StretchedEvaluation(true,true,1,false)};
                    rejected=new[]{0};expected=-1;best=1;break;
            }
            int fallback=StretchedFilter.Apply(weights,evaluations,weights.Length,out double actualBest);
            foreach(int i in rejected)Assert.That(weights[i],Is.EqualTo(double.NegativeInfinity),"B3c case "+scenario+", rejected option "+i);
            Assert.That(fallback,Is.EqualTo(expected),"E1 must choose the least-late non-boast only");
            Assert.That(actualBest,Is.EqualTo(best));
        }
    }
}
