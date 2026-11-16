using System;
namespace SquashClash.Core.Diagnostics
{
    public sealed class UserMetrics
    {
        public UserMetrics(Action<string,string> write,Func<double> readTime) { }
        public void StartSession() { }
        public void SetPaused(bool paused) { }
        public void SetFocused(bool focused) { }
        public void EndSession() { }
        public void Menu(string screen,string action) { }
        public void BeginTutorialStep(int step) { }
        public void EndTutorialStep(bool completed,bool skipped) { }
        public void BeginMatch(string difficulty,string hand) { }
        public void UpdateMatchScore(int human,int rival) { }
        public void EndMatch(bool abandoned) { }
        public void Pause(string reason) { }
    }
}
