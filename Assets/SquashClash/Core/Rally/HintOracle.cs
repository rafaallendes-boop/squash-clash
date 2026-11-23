namespace SquashClash.Core.Rally
{
    // The two "was it really seen" oracles of the first-match hints (spec front 2, section 1.3). Nothing here reads a clock of the game.
    public static class HintOracle
    {
        // In the banner: the fade-in is complete (the HUD's own accumulated opacity, which clips a long frame), the banner and the
        // hint row are visible and the row has a resolved layout. Real time is not used: a long frame can pass 0.18 s with the
        // opacity still below 1, and then the hint is not seen.
        public static bool BannerSeen(float bannerOpacity, bool bannerVisible, bool rowVisible, float rowHeight)
            => bannerOpacity >= 1f && bannerVisible && rowVisible && rowHeight > 0f;
    }
    // In the serve guide, which has no entrance animation: real time visible. It sums the difference between consecutive frames that are
    // both visible with the game clock running; a long frame counts whole, a hidden or paused frame breaks the run.
    public sealed class GuideSeenClock
    {
        private const double Tolerance = 1e-9;
        private double last;
        private bool running;
        public double Seconds { get; private set; }
        public void Frame(double now, bool visible, bool gamePaused)
        {
            if (!visible || gamePaused) { running = false; return; }
            if (running) Seconds += now > last ? now - last : 0;
            last = now;
            running = true;
        }
        public bool Reached(double threshold) => Seconds >= threshold - Tolerance;
        public void Reset() { Seconds = 0; running = false; }
    }
}
