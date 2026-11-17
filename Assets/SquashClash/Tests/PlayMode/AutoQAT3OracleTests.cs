using NUnit.Framework;
using SquashClash.AutoQA;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Runtime.Tests
{
    public sealed class AutoQAT3OracleTests
    {
        private ParamSet Defaults=>PresetCodec.LoadDefault(Resources.Load<TextAsset>("DefaultPreset").text);
        [TestCase(-320,"Failed","Air",false,false)]
        [TestCase(-200,"Valid","None",true,false)]
        [TestCase(-10,"Perfect","None",true,true)]
        [TestCase(10,"Perfect","None",true,false)]
        [TestCase(170,"Valid","None",true,false)]
        [TestCase(320,"Failed","Late",false,false)]
        public void T3_UsesKBeforeAssistance(double delta,string result,string fail,bool applied,bool scheduled)
        {
            var p=Defaults.With("timing.validLimit",0);
            var e=Oracle.Expect(new Scenario{kind="swing",releaseOffsetMs=delta},p,-240,246);
            Assert.That(e.result,Is.EqualTo(result));Assert.That(e.fail,Is.EqualTo(fail));
            Assert.That(e.applied,Is.EqualTo(applied));Assert.That(e.scheduled,Is.EqualTo(scheduled));
        }
        [Test] public void T3_SL1Late170_RemainsLateOutsideK()
        {
            var e=Oracle.Expect(new Scenario{kind="swing",releaseOffsetMs=170},Defaults.With("timing.validLimit",0),-268,6);
            Assert.That(e.result,Is.EqualTo("Failed"));Assert.That(e.fail,Is.EqualTo("Late"));
        }
        [Test] public void AutoQASources_KeepTheAuthorityGuard()=>new AutoQAGuardTests().AutoQASources_RespectAuthorityContract();
        [Test] public void V_ExplicitConfigPreservesApprovedHashes()
        {
            var p=Defaults.With("timing.validLimit",1);
            Assert.That(p.Hash,Is.EqualTo("17d026fe010a"));
            Assert.That(p.With("practice.launch",4).With("human.z",(double)1.6f).Hash,Is.EqualTo("5f4ce0c83a9f"));
        }
        [Test] public void K_IsMeasuredFromRawPositionsAndCollisionHistory()
        {
            var p=Defaults.With("body.reach",1);
            var t=new Trajectory();
            for(int i=0;i<=4;i++)t.States.Add(new BallState{Step=i,Position=new Vector3((float)p.Get("human.x")+i-2,.9f,(float)p.Get("human.z"))});
            float radius=EffectiveConfig.From(p).Physics.BallRadius;
            foreach(var surface in new[]{SurfaceId.FrontWall,SurfaceId.Floor})
            {
                var normal=CourtDimensions.InwardNormal(surface);
                t.Contacts.Add(new ContactEvent(surface,t.States[0].Position-normal*radius,Vector2.zero,normal,1,0,ContactTiming.Exact,Vector3.zero,false));
            }
            var k=PhysicsWindow.Measure(t,p,2);double stepMs=1000/p.Get("physics.stepHz")/p.Get("pace.tempo");
            Assert.That(k.kInMs,Is.EqualTo(-stepMs).Within(stepMs/32));
            Assert.That(k.kOutMs,Is.EqualTo(stepMs).Within(stepMs/32));
            t.Contacts.Clear();Assert.That(PhysicsWindow.Measure(t,p,2),Is.Null,"no legal K without front and bounce");
        }
    }
}
