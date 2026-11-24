using UnityEngine;

namespace SquashClash.Runtime.Ball
{
    /// <summary>Opacity of the ball's ground shadow as a function of the ball's height. Pure art: reads a height, owns no state.</summary>
    public static class BallShadowLook
    {
        public const float AlphaFar = .22f, AlphaNear = .72f, HalfHeight = .7f;

        /// <param name="height">Height of the ball's underside above the floor, in meters.</param>
        public static float Alpha(float height)
        {
            float h = Mathf.Max(0, height);
            float u = h / HalfHeight;
            // Zero slope on the floor, long tail: strictly falling for any finite height, never below AlphaFar.
            return AlphaFar + (AlphaNear - AlphaFar) / (1 + u * u);
        }
    }
}
