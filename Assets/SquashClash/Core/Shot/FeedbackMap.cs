namespace SquashClash.Core.Shot
{
    public enum HapticStyle {Firm,Discreet}
    // Pure mapping from a final presentation category to its haptic style (PO decisions 5-7). Failed never vibrates.
    public static class FeedbackMap
    {
        public static bool TryHaptic(PresentationCategory category,bool enabled,out HapticStyle style)
        {
            style=HapticStyle.Firm;if(!enabled)return false;
            if(category==PresentationCategory.Perfect)return true;
            if(category==PresentationCategory.Valid){style=HapticStyle.Discreet;return true;}
            return false;
        }
    }
}
