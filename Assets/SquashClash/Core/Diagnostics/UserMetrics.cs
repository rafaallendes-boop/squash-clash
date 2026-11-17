using System;
using UnityEngine;
namespace SquashClash.Core.Diagnostics
{
    // Product observation only. Its real-time durations never drive the simulation or the gesture.
    public sealed class UserMetrics
    {
        [Serializable] private sealed class SessionLine {public string action;public double durationSeconds,sessionDurationSeconds;}
        [Serializable] private sealed class MenuLine {public string screen,action;}
        [Serializable] private sealed class TutorialLine {public int step;public double durationSeconds;public bool completed,skipped;}
        [Serializable] private sealed class MatchLine {public string matchId,difficulty,hand,result;public int humanPoints,rivalPoints;public double durationSeconds;public bool abandoned;}
        [Serializable] private sealed class PauseLine {public string reason;}
        private readonly Action<string,string> write;private readonly Func<double> readTime;
        private bool sessionOpen,background,paused,focused=true,matchOpen;
        private double sessionAt,phaseAt,stepAt,matchAt;private int step,human,rival;
        private string matchId,difficulty,hand;
        public UserMetrics(Action<string,string> write,Func<double> readTime)
        {this.write=write??throw new ArgumentNullException(nameof(write));this.readTime=readTime??throw new ArgumentNullException(nameof(readTime));}
        private void Emit(string kind,object value)=>write(kind,JsonUtility.ToJson(value));
        private void Session(string action,double at,double duration)=>Emit("app-session",new SessionLine{action=action,durationSeconds=duration,sessionDurationSeconds=Math.Max(0,at-sessionAt)});
        public void StartSession()
        {
            if(sessionOpen)return;sessionOpen=true;sessionAt=phaseAt=readTime();background=paused||!focused;
            Session("start",sessionAt,0);
        }
        public void SetPaused(bool value){paused=value;ObserveBackground();}
        public void SetFocused(bool value){focused=value;ObserveBackground();}
        private void ObserveBackground()
        {
            bool next=paused||!focused;if(!sessionOpen||next==background)return;
            double at=readTime();Session(next?"background":"foreground",at,Math.Max(0,at-phaseAt));phaseAt=at;background=next;
        }
        public void EndSession()
        {
            EndTutorialStep(false,true);EndMatch(true);if(!sessionOpen)return;
            double at=readTime();Session("end",at,Math.Max(0,at-phaseAt));sessionOpen=false;
        }
        public void Menu(string screen,string action)=>Emit("menu",new MenuLine{screen=screen,action=action});
        public void BeginTutorialStep(int value){step=value;stepAt=readTime();}
        public void EndTutorialStep(bool completed,bool skipped)
        {
            if(step<=0)return;
            Emit("tutorial-step",new TutorialLine{step=step,durationSeconds=Math.Max(0,readTime()-stepAt),completed=completed,skipped=skipped});step=0;
        }
        public void BeginMatch(string level,string selectedHand)
        {
            EndMatch(true);matchOpen=true;matchId=Guid.NewGuid().ToString("D");difficulty=level;hand=selectedHand;human=rival=0;matchAt=readTime();
            Emit("match-start",Match("pending",false,0));
        }
        public void UpdateMatchScore(int humanPoints,int rivalPoints){if(matchOpen){human=humanPoints;rival=rivalPoints;}}
        private MatchLine Match(string result,bool abandoned,double duration)=>new MatchLine{matchId=matchId,difficulty=difficulty,hand=hand,result=result,humanPoints=human,rivalPoints=rival,durationSeconds=duration,abandoned=abandoned};
        public void EndMatch(bool abandoned)
        {
            if(!matchOpen)return;matchOpen=false;
            Emit("match-end",Match(abandoned?"abandoned":human>rival?"win":human<rival?"loss":"draw",abandoned,Math.Max(0,readTime()-matchAt)));
        }
        public void Pause(string reason)=>Emit("pause",new PauseLine{reason=reason});
    }
}
