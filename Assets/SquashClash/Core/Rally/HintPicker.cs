namespace SquashClash.Core.Rally
{
    public enum HintId { Serve = 0, Volley = 1, Boast = 2, BoastCross = 3 }
    public enum HintState { Unseen = 0, Armed = 1, Seen = 2, Retired = 3 }
    public enum HintPause { Ordinary, HumanServe, MatchOver }
    public enum HintWhere { None, Banner, Guide }
    public sealed class HintPicker
    {
        public const int Count = 4;
        public HintPicker(int serve, int volley, int boast, int boastCross, bool windowClosed) { }
        public bool WindowClosed => false;
        public bool InMatch => false;
        public bool AllDone => false;
        public HintState State(HintId id) => HintState.Unseen;
        public int Stored(HintId id) => 0;
        public HintWhere SeenWhere(HintId id) => HintWhere.None;
        public bool BeginMatch() => false;
        public void EndMatch(bool normal) { }
        public void RivalServed() { }
        public void OnReceptionEnded(bool volleyShown, bool volleyApplied) { }
        public void OnStroke(bool applied, bool sideAimOpen, bool strokeBoast) { }
        public HintId? Choose(HintPause pause, bool serverIsRival) => null;
        public void EndPause() { }
        public bool MarkSeen(HintId id, HintWhere where) => false;
    }
}
