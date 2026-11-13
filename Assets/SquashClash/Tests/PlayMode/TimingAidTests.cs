using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Tests
{
    public sealed class TimingAidTests
    {
        private GameObject root;
        private TimingRingView view;
        private double target;
        private ContactPlan plan;
        [SetUp] public void Setup()
        {
            root=new GameObject("timing test");view=root.AddComponent<TimingRingView>();
            target=Time.realtimeSinceStartupAsDouble+1;
            var body=new BodyFrame(new Vector2(3.2f,5.5f),true,1.3f,.6f,.4f,1.1f,.3f);
            plan=new ContactPlan(true,new SimTime(100,0),new SimTime(200,0),150,new Vector3(2.6f,.6f,5.5f),StrokeSide.Drive,7,body,new LegalSettings(.05f,2,100,0),CourtDimensions.Standard());
        }
        [TearDown] public void Cleanup()=>Object.DestroyImmediate(root);
        [TestCase(-100.001)][TestCase(-100)][TestCase(-99.999)]
        [TestCase(-40.001)][TestCase(-40)][TestCase(-39.999)]
        [TestCase(-.001)][TestCase(0)][TestCase(.001)]
        [TestCase(39.999)][TestCase(40)][TestCase(40.001)]
        [TestCase(99.999)][TestCase(100)][TestCase(100.001)]
        public void FullAid_BoundariesMatchUnchangedContract(double delta)
        {
            var settings=new TimingSettings(40,true,100);
            double now=target+delta/1000;
            var clock=new ClockMap();clock.Begin(1,target-1.5,0,1,1,100);
            var trajectory=new Trajectory();for(int i=0;i<=230;i++)trajectory.States.Add(new BallState{Step=i,Position=plan.PStar});
            ContactEvent E(SurfaceId s,long step)=>new ContactEvent(s,plan.PStar-CourtDimensions.InwardNormal(s)*.02f,Vector2.zero,CourtDimensions.InwardNormal(s),step,0,ContactTiming.Exact,Vector3.zero,false);
            trajectory.Contacts.Add(E(SurfaceId.FrontWall,1));trajectory.Contacts.Add(E(SurfaceId.Floor,101));trajectory.Contacts.Add(E(SurfaceId.Floor,201));
            var plans=new PresentedPlans();plans.Present(target-1,plan,target);
            var decision=ContactContract.Decide(now,plans,clock,trajectory,settings,false);
            var state=ImpactCue.Evaluate(now,target-1,target,settings,true,false);
            bool contact=decision.Result==ShotResult.Perfect||decision.Result==ShotResult.Valid;
            Assert.That(state.InAssistWindow,Is.EqualTo(decision.Result==ShotResult.Perfect));
            Assert.That(state.InValidWindow,Is.EqualTo(contact));
            view.Show(plan,target,true,settings);view.Tick(now);
            Assert.That(view.Visible,Is.EqualTo(now<=target||contact),"inclusive late boundary must remain visible");
            Assert.That(root.GetComponentsInChildren<LineRenderer>().All(r=>r.enabled),Is.EqualTo(view.Visible));
        }
        [Test] public void FullAid_RepresentsBothEarlyAndLateWindowEdges()
        {
            view.Show(plan,target,true,new TimingSettings(40,true,100));view.Tick(target+.06);
            var lines=root.GetComponentsInChildren<LineRenderer>();
            LineRenderer Find(string name)=>lines.SingleOrDefault(r=>r.name==name);
            Assert.That(Find("Late perfect band"),Is.Not.Null);
            Assert.That(Find("Late valid band"),Is.Not.Null);
            float R(string name)=>Find(name).GetPosition(0).magnitude;
            Assert.That(R("Late valid band"),Is.LessThan(R("Converging ring")));
            Assert.That(R("Converging ring"),Is.LessThan(R("Late perfect band")));
            Assert.That(R("Late perfect band"),Is.LessThan(.1f));
        }
        [Test] public void AidPartial_HidesRingLast150ms()
        {
            view.BeginReception(1);view.Show(plan,target,true,new TimingSettings(40,true,100));
            view.Tick(target-.150001);Assert.That(view.Visible,Is.True);
            Assert.That(root.GetComponentsInChildren<LineRenderer>().All(r=>r.enabled),Is.True);
            view.Tick(target-.15);Assert.That(view.CutoffLatched,Is.True);
            Assert.That(root.GetComponentsInChildren<LineRenderer>().All(r=>!r.enabled),Is.True);
            Assert.That(view.CutoffTarget,Is.EqualTo(target));Assert.That(view.CutoffPlanVersion,Is.EqualTo(7));
            view.Show(plan,target+10,true,new TimingSettings(40,true,100));view.Tick(target);
            Assert.That(view.Visible,Is.False);Assert.That(view.CutoffTarget,Is.EqualTo(target));
            view.BeginReception(0);view.Show(plan,target+10,true,new TimingSettings(40,true,100));Assert.That(view.Visible,Is.True);
        }
        [Test] public void AidNone_ShowsOnlyBallAndShadow()
        {
            var unrelated=new GameObject("aim power feedback ball shadow");unrelated.transform.SetParent(root.transform);
            view.BeginReception(2);view.Show(plan,target,true,new TimingSettings(40,true,100));view.Tick(target);
            Assert.That(view.Visible,Is.False);Assert.That(root.GetComponentsInChildren<LineRenderer>().All(r=>!r.enabled),Is.True);
            Assert.That(view.CueState.Phase,Is.EqualTo(ImpactCuePhase.AtIdeal));Assert.That(unrelated.activeSelf,Is.True);
        }
        [Test] public void PureT3_HasNoValidBands()
        {
            view.Show(plan,target,true,new TimingSettings(40,false,100));view.Tick(target);
            foreach(var r in root.GetComponentsInChildren<LineRenderer>().Where(r=>r.name.ToLowerInvariant().Contains("valid")))Assert.That(r.enabled,Is.False);
        }
    }
}
