namespace SquashClash.Core.Shot
{
    // Raised once per accepted Swing order: the only source of the swing intention pose.
    public readonly struct SwingIntent
    {
        public readonly long LaunchId;
        public readonly string AttemptId;
        public readonly StrokeSide Side;
        public SwingIntent(long launchId,string attemptId,StrokeSide side){LaunchId=launchId;AttemptId=attemptId;Side=side;}
    }
}
