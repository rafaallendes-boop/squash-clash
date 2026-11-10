using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace SquashClash.AutoQA
{
    [Serializable] public sealed class TouchPoint {public string phase;public double t;public float x,y;}
    [Serializable] public sealed class TouchScript {public TouchPoint[] samples;}
}
