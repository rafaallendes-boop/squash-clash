using System;
using UnityEngine;
namespace SquashClash.Runtime.Rally
{
    // «vs» plate (front 4). RED skeleton: the API the tests need, with no behaviour yet.
    public static class VsPlate
    {
        // Real time source, replaceable by tests.
        public static Func<double> Now=()=>Time.unscaledTimeAsDouble;
        public static void Reset(){Now=()=>Time.unscaledTimeAsDouble;}
    }
}
