using UnityEngine;
namespace SquashClash.Runtime.Visual
{
    // Device robustness, block C: which hand the drawn player holds the racket in ("Mano: diestro / zurdo" in the menu).
    // Cosmetic only: CharacterView is its one reader. The gesture, the M2-4 aim, the physics, the timing and the rules
    // do not know it exists (the Sling control's own gesture.slingPreferLeft parameter is a different thing and is not touched).
    public static class Handedness
    {
        public const string Key="sc.hand";
        private static bool? left;
        // Remembered between launches. Read once, then cached: the figure asks every frame.
        public static bool Left
        {
            get{if(!left.HasValue)left=PlayerPrefs.GetInt(Key,0)==1;return left.Value;}
            set{left=value;PlayerPrefs.SetInt(Key,value?1:0);PlayerPrefs.Save();}
        }
    }
}
