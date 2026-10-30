using SquashClash.Core.Tests.Config;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Core.Tests.Shot
{
    public sealed class M24ProjectionTests
    {
        private GameObject go;private Camera camera;private FrontisProjection projection;
        [SetUp] public void Setup()
        {
            go=new GameObject("Captured projection fixture");camera=go.AddComponent<Camera>();
            camera.transform.position=new Vector3(3.2f,8,14.75f);camera.transform.LookAt(new Vector3(3.2f,.4f,0));
            camera.fieldOfView=90;camera.aspect=1206f/2622;camera.pixelRect=new Rect(0,0,1206,2622);
            projection=new FrontisProjection(camera.cameraToWorldMatrix,camera.projectionMatrix,camera.pixelRect,new Rect(0,102,1206,2334),CourtDimensions.Standard(),10.025,7);
        }
        [TearDown] public void Cleanup()=>Object.DestroyImmediate(go);
        private Vector2 ScreenPoint(float x,float y)=>camera.WorldToScreenPoint(new Vector3(x,y,0));
        [Test] public void Began_MapsUsingCapturedProjection_AX1()
        {
            var left=ScreenPoint(4.77f,1.1355f);var right=ScreenPoint(1.63f,1.1355f);Assert.That(left.x,Is.LessThan(right.x));
            Assert.That(projection.TryMap(left,out var aim,out _),Is.True);Assert.That(aim.x,Is.EqualTo(4.77f).Within(.0001));Assert.That(aim.y,Is.EqualTo(1.1355f).Within(.0001));
            camera.transform.position+=Vector3.right*2;
            Assert.That(projection.TryMap(left,out var still,out _),Is.True);Assert.That(still,Is.EqualTo(aim));
            Assert.That(projection.ObservedAt,Is.EqualTo(10.025));Assert.That(projection.Frame,Is.EqualTo(7));
        }
        [TestCase(.48f)] [TestCase(4.57f)] [TestCase(0)] [TestCase(5)]
        public void TinOut_RemainsMappableButInvalid(float y)
        {Assert.That(projection.TryMap(ScreenPoint(3.2f,y),out var aim,out _),Is.True);Assert.That(projection.IsInvalid(aim),Is.True);}
        [Test] public void LegalPoint_IsNotInvalid()=>Assert.That(projection.IsInvalid(new Vector2(3.2f,2.2f)),Is.False);
        [Test] public void OutsideDomain_RejectsWithReason_WithoutLegalClamp()
        {
            Assert.That(projection.TryMap(ScreenPoint(-1,2),out _,out var reason),Is.False);Assert.That(reason,Is.EqualTo("outside-front-wall"));
            Assert.That(projection.TryMap(ScreenPoint(3,-1),out _,out reason),Is.False);Assert.That(reason,Is.EqualTo("outside-front-wall"));
            Assert.That(projection.TryMap(new Vector2(float.NaN,100),out _,out reason),Is.False);Assert.That(reason,Is.EqualTo("non-finite-point"));
            var singular=new FrontisProjection(Matrix4x4.zero,Matrix4x4.zero,new Rect(0,0,1206,2622),new Rect(0,0,1206,2622),CourtDimensions.Standard(),0,0);
            Assert.That(singular.TryMap(new Vector2(600,1300),out _,out reason),Is.False);Assert.That(reason,Is.EqualTo("invalid-ray"));
        }
        [Test] public void Selector_DefaultIsM24_AndReferenceControlsRemainSelectable()
        {Assert.That(ConfigFixtures.Default().With("gesture.control",0).GetInt("gesture.control"),Is.Zero);Assert.That(ConfigFixtures.Default().With("gesture.control",2).GetInt("gesture.control"),Is.EqualTo(2));}
        [Test] public void ExperimentalPresetAndStartupProjectionKeepDefaultsAndSimulationKey()
        {
            var defaults=ConfigFixtures.Default();var file=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"..","docs/validation/presets/S1-M2-4.json"));
            Assert.That(SquashClash.Core.Config.PresetCodec.TryParse(System.IO.File.ReadAllText(file),out var preset,out var error),Is.True,error);
            var p=SquashClash.Core.Config.PresetCodec.Overlay(defaults,preset,new System.Collections.Generic.List<string>());
            Assert.That(p.GetInt("gesture.control"),Is.EqualTo(2));Assert.That(p.Get("power.pullDeadZoneMm"),Is.EqualTo(2));Assert.That(p.Get("power.pullFullMm"),Is.EqualTo(35));
            Assert.That(p.Get("gesture.tapMaxMm"),Is.EqualTo(3));Assert.That(p.Get("gesture.tapMaxMs"),Is.EqualTo(250));
            Assert.That(defaults.Hash,Is.EqualTo("9af98129d12e"));Assert.That(defaults.Ordered.Count,Is.EqualTo(92));
            Assert.That(SquashClash.Core.Config.EffectiveConfig.From(p).SimulationKey,Is.EqualTo("166a96e319c7"));
            TestContext.WriteLine("M24_PRESET_HASH="+p.Hash+" P0_EXTENDED_HASH="+defaults.With("launcher.repeatSeconds",3).Hash);
        }
    }
}
