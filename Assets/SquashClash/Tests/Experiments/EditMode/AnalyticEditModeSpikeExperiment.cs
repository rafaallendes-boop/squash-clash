using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
namespace SquashClash.Experiments.EditMode
{
    public sealed class AnalyticEditModeSpikeExperiment
    {
        [UnityTest,Timeout(1800000)]
        public IEnumerator FullRun_SameCampaignWithoutPlayMode() => new AnalyticSpikeExperiment().FullRun_UsesSameScenariosAndCriteria_ReportsVerdictsAsData();
    }
}
