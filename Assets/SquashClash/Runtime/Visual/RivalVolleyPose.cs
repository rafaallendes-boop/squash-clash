namespace SquashClash.Runtime.Visual
{
    // DEV toggle «Pose de volea del rival» (2026-11-20). Cosmetic only: CharacterView is its one reader, so the rival's decisions,
    // timing and positions are the same on or off. Not persisted: every launch starts with the pose on.
    public static class RivalVolleyPose
    {
        public static bool Enabled=true;
    }
}
