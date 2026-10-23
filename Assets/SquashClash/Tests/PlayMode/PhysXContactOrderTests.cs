using System;
using System.Collections.Generic;
using NUnit.Framework;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.Physics;
using UnityEngine;

namespace SquashClash.Runtime.Tests
{
    public sealed class PhysXContactOrderTests
    {
        private static ContactEvent Event(float time = 0, SurfaceId surface = SurfaceId.Floor, bool persistent = false,
            long step = 1, ContactTiming timing = ContactTiming.Estimated, Vector3 point = default,
            Vector2 uv = default, Vector3 normal = default, Vector3 approach = default) =>
            new ContactEvent(surface, point, uv, normal, step, time, timing, approach, persistent);
        [Test] public void Keys_AreTimeThenSurfaceThenPersistence_WithUnknownTimeLast()
        {
            var a = Event(.002f); var b = Event(.001f, SurfaceId.BackWall, true);
            var c = Event(float.NaN, SurfaceId.BackWall); var d = Event(float.NaN, persistent:true);
            var e = Event(float.NaN); var f = Event(.001f, SurfaceId.BackWall);
            var items = new List<ContactEvent> {c,a,b,d,f,e,e};
            items.Sort(PhysXContactOrder.Instance);
            Assert.That(items, Is.EqualTo(new[] { f,b,a,e,e,d,c }), "values and duplicate multiplicity preserved");
        }
        [Test] public void TieBreak_UsesEveryRemainingFieldIncludingSignedZeroAndNaNPayloads()
        {
            var a = Event();
            var variants = new[] { Event(step:2), Event(timing:ContactTiming.Exact),
                Event(time:BitConverter.Int32BitsToSingle(unchecked((int)0x80000000))),
                Event(point:Vector3.right),Event(point:Vector3.up),Event(point:Vector3.forward),
                Event(uv:Vector2.right),Event(uv:Vector2.up),
                Event(normal:Vector3.right),Event(normal:Vector3.up),Event(normal:Vector3.forward),
                Event(approach:Vector3.right),Event(approach:Vector3.up),Event(approach:Vector3.forward) };
            foreach (var b in variants) {
                Assert.That(PhysXContactOrder.Instance.Compare(a,b),Is.LessThan(0));
                Assert.That(PhysXContactOrder.Instance.Compare(b,a),Is.GreaterThan(0));
            }
            var n1=Event(time:BitConverter.Int32BitsToSingle(unchecked((int)0x7FC00001)));
            var n2=Event(time:BitConverter.Int32BitsToSingle(unchecked((int)0x7FC00002)));
            Assert.That(PhysXContactOrder.Instance.Compare(n1,n2),Is.LessThan(0));
            Assert.That(PhysXContactOrder.Instance.Compare(n1,n1),Is.Zero);
        }
        [Test] public void Ties_DoNotOverridePrimaryKeys_AndEveryPermutationHasSameOrder()
        {
            var samples=new[] {Event(float.NaN),Event(-0f,SurfaceId.LeftWall),Event(0,SurfaceId.Floor,true),Event(0),Event(.001f)};
            var expected=new List<ContactEvent>(samples);expected.Sort(PhysXContactOrder.Instance);
            int permutations=0;
            void Check(int start) {
                if(start==samples.Length) {var actual=new List<ContactEvent>(samples);actual.Sort(PhysXContactOrder.Instance);Assert.That(actual,Is.EqualTo(expected));permutations++;return;}
                for(int i=start;i<samples.Length;i++) {(samples[start],samples[i])=(samples[i],samples[start]);Check(start+1);(samples[start],samples[i])=(samples[i],samples[start]);}
            }
            Check(0);Assert.That(permutations,Is.EqualTo(120));
        }
    }
}
