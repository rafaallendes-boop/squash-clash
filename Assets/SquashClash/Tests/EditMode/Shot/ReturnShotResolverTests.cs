using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Core.Tests.Config;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class ReturnShotResolverTests
    {
        private EffectiveConfig cfg;
        private readonly Vector3 focal=new Vector3(2.5999999046325684f,.16269451379776f,1.1300088167190552f);
        [SetUp] public void Setup()=>cfg=EffectiveConfig.From(ConfigFixtures.Default());
        private ReturnShotResolver Resolver(bool enabled,float margin=.05f)=>new ReturnShotResolver(6,28,32,cfg.Court,cfg.Physics,new AnalyticBallSimulationFactory(.1f),new ReturnRuleSettings(enabled,margin));
        private ShotSolution Original(ShotRequest r)=>ShotSolver.Solve(r,6,28,32,cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court,new AnalyticBallSimulationFactory(.1f),cfg.Physics);
        private static void Same(ShotSolution a,ShotSolution b)
        {
            Assert.That(b.Solved,Is.EqualTo(a.Solved));Assert.That(b.Reason,Is.EqualTo(a.Reason));Assert.That(b.SpeedRaised,Is.EqualTo(a.SpeedRaised));Assert.That(b.TargetLowered,Is.EqualTo(a.TargetLowered));
            var av=new[]{a.Speed,a.Velocity.x,a.Velocity.y,a.Velocity.z,a.TargetUsed.x,a.TargetUsed.y};var bv=new[]{b.Speed,b.Velocity.x,b.Velocity.y,b.Velocity.z,b.TargetUsed.x,b.TargetUsed.y};
            for(int i=0;i<av.Length;i++)Assert.That(BitConverter.SingleToInt32Bits(bv[i]),Is.EqualTo(BitConverter.SingleToInt32Bits(av[i])));
        }
        [Test] public void Guard_PreservesValidShortBase_WithoutSearching()
        {var r=new ShotRequest(focal,new Vector2(3.2f,2.2f),.2f);var s=Resolver(true).Resolve(r);Assert.That(s.State,Is.EqualTo(ReturnRuleState.Preserved));Assert.That(s.SolverCalls,Is.EqualTo(1));Same(Original(r),s.Solution);Assert.That(s.AdjustedRequest.AimOnFrontWall,Is.EqualTo(r.AimOnFrontWall));}
        [Test] public void Changed_KeepsRequestedXAndP_AndMatchesMarginEvaluation()
        {var r=new ShotRequest(focal,new Vector2(3.2f,2.2f),1);var s=Resolver(true).Resolve(r);Assert.That(s.State,Is.EqualTo(ReturnRuleState.Changed));Assert.That(s.AdjustedRequest.AimOnFrontWall.x,Is.EqualTo(r.AimOnFrontWall.x));Assert.That(s.AdjustedRequest.P,Is.EqualTo(1));Assert.That(BitConverter.SingleToInt32Bits(s.AdjustedRequest.AimOnFrontWall.y),Is.EqualTo(BitConverter.SingleToInt32Bits(.65f)));Same(Original(s.AdjustedRequest),s.Solution);}
        [Test] public void Changed_ClearsTinPlusConfiguredMargin()
        {var s=Resolver(true).Resolve(new ShotRequest(focal,new Vector2(3.2f,2.2f),1));Assert.That(s.State,Is.EqualTo(ReturnRuleState.Changed));Assert.That((double)s.AdjustedRequest.AimOnFrontWall.y,Is.GreaterThan((double)cfg.Court.TinTop+.05f));}
        [Test] public void NoSolution_ConservesOriginalSolverResult_EvenWhenUnsolved()
        {var r=new ShotRequest(new Vector3(2.5999999046325684f,.3648533523082733f,6.3830108642578125f),new Vector2(6.38f,.5f),0);var s=Resolver(true).Resolve(r);Assert.That(s.State,Is.EqualTo(ReturnRuleState.NoSolution));Same(Original(r),s.Solution);Assert.That(s.AdjustedRequest.AimOnFrontWall,Is.EqualTo(r.AimOnFrontWall));}
        [Test] public void Disabled_IsOriginalSolutionBitForBit()
        {foreach(float p in new[]{0f,.2f,.6f,1f}){var r=new ShotRequest(focal,new Vector2(3.2f,2.2f),p);var s=Resolver(false).Resolve(r);Assert.That(s.State,Is.EqualTo(ReturnRuleState.Disabled));Assert.That(s.SolverCalls,Is.EqualTo(1));Same(Original(r),s.Solution);}}
        [Test] public void RepeatedExactRequest_UsesDeterministicCachedResolution()
        {var resolver=Resolver(true);var r=new ShotRequest(focal,new Vector2(3.2f,2.2f),1);var a=resolver.Resolve(r);var b=resolver.Resolve(r);Assert.That(a.State,Is.EqualTo(ReturnRuleState.Changed));Same(a.Solution,b.Solution);Assert.That(b.SolverCalls,Is.EqualTo(0));}
        [Test] public void InvalidOriginalAim_PreservesSolverFailure_WithoutSearching()
        {
            var r=new ShotRequest(focal,new Vector2(3.2f,6),1);var result=Resolver(true).Resolve(r);
            Assert.That(result.State,Is.EqualTo(ReturnRuleState.NoSolution));Assert.That(result.SolverCalls,Is.EqualTo(1));Same(Original(r),result.Solution);
        }
        [Test] public void NewKeys_DefaultOff_AndAll106PriorValuesAreUnchanged()
        {
            var p=ConfigFixtures.Default();foreach(string k in new[]{"shot.returnEnabled","shot.returnTinMargin","shot.returnSearchStep"})Assert.That(ParamCatalog.TryGet(k,out _),Is.True,k);
            Assert.That(p.GetBool("shot.returnEnabled"),Is.False);Assert.That(p.Get("shot.returnTinMargin"),Is.EqualTo(.05));Assert.That(p.Get("shot.returnSearchStep"),Is.EqualTo(.05));
            var old=JsonUtility.FromJson<PresetFile>(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Tests/Fixtures/pre-return-rule-defaults.json")));Assert.That(old.values.Count,Is.EqualTo(106));foreach(var v in old.values)Assert.That(p.Get(v.key),Is.EqualTo(v.value),v.key);
            TestContext.WriteLine("RETURN_DEFAULTS_HASH="+p.Hash+" P0="+p.With("launcher.repeatSeconds",3).Hash);Assert.That(p.Ordered.Count,Is.EqualTo(109));Assert.That(EffectiveConfig.From(p).SimulationKey,Is.EqualTo("166a96e319c7"));var legacy=PresetCodec.Overlay(p,old,new System.Collections.Generic.List<string>());Assert.That(ReturnRuleSettings.From(legacy).Enabled,Is.False);
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"..","docs/validation/presets/S2-return-rule.json"));
            Assert.That(PresetCodec.TryParse(File.ReadAllText(path),out var experimental,out var error),Is.True,error);
            var enabled=PresetCodec.Overlay(p,experimental,new System.Collections.Generic.List<string>());Assert.That(ReturnRuleSettings.From(enabled).Enabled,Is.True);
            foreach(var v in p.Ordered)if(v.Key!="shot.returnEnabled")Assert.That(enabled.Get(v.Key),Is.EqualTo(v.Value),v.Key);
            TestContext.WriteLine("RETURN_EXPERIMENT_HASH="+enabled.Hash);
        }
    }
}
