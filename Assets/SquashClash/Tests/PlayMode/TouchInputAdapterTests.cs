using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Phase=UnityEngine.InputSystem.TouchPhase;
using Object=UnityEngine.Object;
namespace SquashClash.Runtime.Tests
{
    public sealed class TouchInputAdapterTests : InputTestFixture
    {
        private GameObject root;private TouchInputAdapter adapter;private Touchscreen screen;
        private readonly List<TouchSample> received=new List<TouchSample>();
        [SetUp] public override void Setup()
        {
            base.Setup();received.Clear();screen=InputSystem.AddDevice<Touchscreen>();
            root=new GameObject("Touch adapter test");adapter=root.AddComponent<TouchInputAdapter>();
            adapter.InputAreaOverride=new Rect(0,0,1000,1000);adapter.ReadRealTime=()=>5;
            adapter.SampleReceived+=s=>received.Add(s);
        }
        [TearDown] public override void TearDown() {Object.DestroyImmediate(root);base.TearDown();}
        private void Queue(Phase phase,double t,float x=30,int id=1)=>InputSystem.QueueStateEvent(screen,new TouchState{touchId=id,phase=phase,position=new Vector2(x,80)},t);
        [Test] public void EventTimestamps_ArePreservedAsDouble()
        {Queue(Phase.Began,1.123456789);InputSystem.Update();Assert.That(received.Count,Is.EqualTo(1));Assert.That(received[0].TimeReal,Is.EqualTo(1.123456789).Within(1e-9));}
        [Test] public void MultipleEventsInOneFrame_YieldDistinctSamplesInOrder()
        {Queue(Phase.Began,1);Queue(Phase.Moved,1.01,31);Queue(Phase.Moved,1.02,32);Queue(Phase.Moved,1.03,33);Queue(Phase.Ended,1.04,34);InputSystem.Update();Assert.That(received.Count,Is.EqualTo(5));Assert.That(received[2].TimeReal,Is.EqualTo(1.02));Assert.That(received[4].Phase,Is.EqualTo(TouchPhaseKind.Ended));Assert.That(adapter.Stats.MaxSamplesPerFrame,Is.EqualTo(5));}
        [Test] public void NoEventsInFrame_DoesNotSynthesizeSamples()
        {Queue(Phase.Began,1);InputSystem.Update();long n=adapter.Stats.Samples;InputSystem.Update();Assert.That(received.Count,Is.EqualTo(1));Assert.That(adapter.Stats.Samples,Is.EqualTo(n));}
        [Test] public void TouchStartedOverUi_DoesNotStartGesture()
        {
            var g=new GestureTracker(3,250,6,1);adapter.SampleReceived+=s=>g.Push(s);
            adapter.BlockedByUi=true;Queue(Phase.Began,1);InputSystem.Update();adapter.BlockedByUi=false;
            Queue(Phase.Moved,1.02,90);Queue(Phase.Ended,1.03,95);InputSystem.Update();
            Assert.That(g.Active,Is.False);Assert.That(g.Outcome,Is.EqualTo(GestureOutcome.None));
        }
        [Test] public void ScreenMetrics_ZeroDpi_UsesDeclaredFallbackAndFlagsIt()
        {float mm=ScreenMetrics.MmPerPx(0,460,out var source);Assert.That(mm,Is.EqualTo(25.4f/460).Within(1e-7));Assert.That(source,Is.EqualTo(DpiSource.NominalFallback));}
        [TestCase(30)][TestCase(5000)] public void ScreenMetrics_ImplausiblePositiveDpi_UsesFallbackAndFlagsIt(float dpi)
        {Assert.That(ScreenMetrics.MmPerPx(dpi,460,out var source),Is.EqualTo(25.4f/460).Within(1e-7));Assert.That(source,Is.EqualTo(DpiSource.NominalFallback));}
        [Test] public void OnEvent_ReadsTouchStatePayload_NotStaleControls()
        {Queue(Phase.Began,1,111,4);InputSystem.Update();Assert.That(received.Count,Is.EqualTo(1));Assert.That(received[0].FingerId,Is.EqualTo(4));Assert.That(received[0].PositionPx.x,Is.EqualTo(111));Assert.That(received[0].Phase,Is.EqualTo(TouchPhaseKind.Began));}
        [Test] public void NonMonotonicEventTimes_AreKeptInSequenceAndCounted()
        {Queue(Phase.Began,1);Queue(Phase.Moved,1.02,31);Queue(Phase.Moved,1.01,32);InputSystem.Update();Assert.That(received.Count,Is.EqualTo(3));Assert.That(received[1].Sequence,Is.LessThan(received[2].Sequence));Assert.That(received[2].TimeReal,Is.EqualTo(1.01));Assert.That(adapter.Stats.OutOfOrder,Is.EqualTo(1));}
        [Test] public void SafeArea_ExcludesSystemEdges()
        {adapter.InputAreaOverride=new Rect(10,20,100,200);Assert.That(adapter.IsPointAllowed(new Vector2(40,80)),Is.True);Assert.That(adapter.IsPointAllowed(new Vector2(40,19)),Is.False);}
        [Test] public void LossOfFocus_CancelsAndOldEndCannotStartAgain()
        {
            int canceled=0;adapter.SystemCanceled+=()=>canceled++;Queue(Phase.Began,1);InputSystem.Update();
            root.SendMessage("OnApplicationFocus",false,SendMessageOptions.DontRequireReceiver);
            root.SendMessage("OnApplicationFocus",true,SendMessageOptions.DontRequireReceiver);
            Queue(Phase.Ended,1.03,90);InputSystem.Update();Assert.That(canceled,Is.GreaterThanOrEqualTo(1));Assert.That(received.Count,Is.EqualTo(1));
            Queue(Phase.Began,6,30,2);InputSystem.Update();Assert.That(received.Count,Is.EqualTo(2));
        }
    }
}
