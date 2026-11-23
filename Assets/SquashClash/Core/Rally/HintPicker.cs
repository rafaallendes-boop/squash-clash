namespace SquashClash.Core.Rally
{
    public enum HintId { Serve = 0, Volley = 1, Boast = 2, BoastCross = 3 }
    // Unseen and Armed are in memory only; Seen (stored 1) and Retired (stored 2) are what the player's device remembers.
    public enum HintState { Unseen = 0, Armed = 1, Seen = 2, Retired = 3 }
    public enum HintPause { Ordinary, HumanServe, MatchOver }
    public enum HintWhere { None, Banner, Guide }
    // First-match hints (spec front 2, 2026-11-22, section 1.3): which hint a pause shows and how each one moves between states.
    // Pure: it reads facts the adapter gives it and never touches the game. Four hints, each seen once per install, shown one per pause.
    public sealed class HintPicker
    {
        public const int Count = 4;
        private static readonly HintId[] Priority = { HintId.Serve, HintId.Volley, HintId.Boast, HintId.BoastCross };
        private readonly HintState[] state = new HintState[Count];
        private readonly HintWhere[] seenWhere = new HintWhere[Count];
        // The form H0 is armed for (banner: before the rival's first serve; guide: the human's serve pause). One key, two forms.
        private HintWhere serveForm;
        private bool rivalServed, boastDone;
        public HintPicker(int serve, int volley, int boast, int boastCross, bool windowClosed)
        {
            state[(int)HintId.Serve] = FromStored(serve);
            state[(int)HintId.Volley] = FromStored(volley);
            state[(int)HintId.Boast] = FromStored(boast);
            state[(int)HintId.BoastCross] = FromStored(boastCross);
            WindowClosed = windowClosed;
        }
        private static HintState FromStored(int value) => value == 1 ? HintState.Seen : value == 2 ? HintState.Retired : HintState.Unseen;
        public bool WindowClosed { get; private set; }
        public bool InMatch { get; private set; }
        public bool AllDone
        {
            get
            {
                foreach (var s in state) if (s != HintState.Seen && s != HintState.Retired) return false;
                return true;
            }
        }
        public HintState State(HintId id) => state[(int)id];
        // What the device remembers: 1 seen, 2 retired, 0 anything else (armed is never stored).
        public int Stored(HintId id) => state[(int)id] == HintState.Seen ? 1 : state[(int)id] == HintState.Retired ? 2 : 0;
        public HintWhere SeenWhere(HintId id) => seenWhere[(int)id];
        // A match of the window begins. False when the window is closed or there is nothing left to show.
        public bool BeginMatch()
        {
            ClearArmed();
            rivalServed = boastDone = false;
            InMatch = !WindowClosed && !AllDone;
            return InMatch;
        }
        // Armed hints always go back to unseen. Only a normal end (win or loss) closes the window; abandoning does not.
        public void EndMatch(bool normal)
        {
            ClearArmed();
            if (InMatch && normal) WindowClosed = true;
            InMatch = false;
        }
        private void ClearArmed()
        {
            for (int i = 0; i < Count; i++) if (state[i] == HintState.Armed) state[i] = HintState.Unseen;
            serveForm = HintWhere.None;
        }
        private void Arm(HintId id)
        {
            if (state[(int)id] == HintState.Unseen) state[(int)id] = HintState.Armed;
        }
        private void CheckClose()
        {
            if (AllDone) WindowClosed = true;
        }
        // The rival has served in this match: the banner form of H0 cannot arm any more.
        public void RivalServed()
        {
            if (InMatch) rivalServed = true;
        }
        // The amber ring showed in a human reception that ended with no volley applied (a bounce stroke, a failure, or no stroke).
        public void OnReceptionEnded(bool volleyShown, bool volleyApplied)
        {
            if (InMatch && volleyShown && !volleyApplied) Arm(HintId.Volley);
        }
        // One resolved stroke of the human. strokeBoast means an applied two-wall shot that did not fall back to the front wall.
        public void OnStroke(bool applied, bool sideAimOpen, bool strokeBoast)
        {
            if (!InMatch || !applied) return;
            if (strokeBoast)
            {
                bool first = !boastDone;
                boastDone = true;
                // The player found the shot on their own: the hint to try it is retired, whether it was waiting or not yet armed.
                if (state[(int)HintId.Boast] == HintState.Unseen || state[(int)HintId.Boast] == HintState.Armed) state[(int)HintId.Boast] = HintState.Retired;
                if (first) Arm(HintId.BoastCross);
                CheckClose();
            }
            else if (sideAimOpen && !boastDone) Arm(HintId.Boast);
        }
        // At most one hint for this pause. The guide holds H0 only; an ordinary pause follows the order H0 (banner), H1, H2, H3.
        public HintId? Choose(HintPause pause, bool serverIsRival)
        {
            if (!InMatch || pause == HintPause.MatchOver) return null;
            if (pause == HintPause.HumanServe)
            {
                if (state[(int)HintId.Serve] == HintState.Unseen) { state[(int)HintId.Serve] = HintState.Armed; serveForm = HintWhere.Guide; }
                return state[(int)HintId.Serve] == HintState.Armed && serveForm == HintWhere.Guide ? HintId.Serve : (HintId?)null;
            }
            if (serverIsRival && !rivalServed && state[(int)HintId.Serve] == HintState.Unseen) { state[(int)HintId.Serve] = HintState.Armed; serveForm = HintWhere.Banner; }
            foreach (var id in Priority)
            {
                if (state[(int)id] != HintState.Armed) continue;
                if (id == HintId.Serve && serveForm != HintWhere.Banner) continue;
                return id;
            }
            return null;
        }
        // The pause is over. H0 only lives as long as its own pause; the others stay armed until the match ends.
        public void EndPause()
        {
            if (state[(int)HintId.Serve] == HintState.Armed) state[(int)HintId.Serve] = HintState.Unseen;
            serveForm = HintWhere.None;
        }
        // The only transition that depends on the player really seeing the hint. The guide holds H0, and only in its guide form.
        public bool MarkSeen(HintId id, HintWhere where)
        {
            if (!InMatch || state[(int)id] != HintState.Armed || where == HintWhere.None) return false;
            if (id == HintId.Serve ? where != serveForm : where != HintWhere.Banner) return false;
            state[(int)id] = HintState.Seen;
            seenWhere[(int)id] = where;
            CheckClose();
            return true;
        }
    }
}
