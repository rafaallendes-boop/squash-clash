namespace SquashClash.Core.Shot
{
    // Raised once per accepted Swing order: the only source of the swing intention pose.
    public readonly struct SwingIntent
    {
        public readonly long LaunchId;
        public readonly string AttemptId;
        public readonly StrokeSide Side;
        // Volley v1: the order went to the volley opportunity, and the height of its planned contact (metres). Defaults change nothing.
        public readonly bool Volley;public readonly float ContactHeight;
        public SwingIntent(long launchId,string attemptId,StrokeSide side,bool volley=false,float contactHeight=0){LaunchId=launchId;AttemptId=attemptId;Side=side;Volley=volley;ContactHeight=contactHeight;}
    }
}
