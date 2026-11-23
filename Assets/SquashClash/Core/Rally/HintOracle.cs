namespace SquashClash.Core.Rally
{
    // The two "was it really seen" oracles of the first-match hints (spec front 2, section 1.3).
    public static class HintOracle
    {
        public static bool BannerSeen(float bannerOpacity, bool bannerVisible, bool rowVisible, float rowHeight) => false;
    }
    public sealed class GuideSeenClock
    {
        public double Seconds => 0;
        public void Frame(double now, bool visible, bool gamePaused) { }
        public bool Reached(double threshold) => false;
        public void Reset() { }
    }
}
