using System.Collections.Generic;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Rally;
using UnityEngine;
namespace SquashClash.Runtime.Menu
{
    // What a finished menu match adds to the player's bests. View and persistence only: it reads the finished match,
    // never feeds anything back into the game, and only PlayerPrefs (sc.records.*) remembers it.
    public readonly struct MatchSummary
    {
        public readonly bool HumanWon;public readonly AiLevel Level;public readonly int Human,Rival,LongestRally;public readonly float Seconds;
        // Strokes of the match (spec 2026-11-25): shown on the card only; PersonalRecords.Apply never reads them.
        public readonly int Perfects,BoastWinners,Volleys;
        public MatchSummary(bool humanWon,AiLevel level,int human,int rival,int longestRally,float seconds,int perfects=0,int boastWinners=0,int volleys=0)
        {HumanWon=humanWon;Level=level;Human=human;Rival=rival;LongestRally=longestRally;Seconds=seconds;Perfects=perfects;BoastWinners=boastWinners;Volleys=volleys;}
        public string StrokeOfTheMatchLine=>StrokeOfTheMatch.Line(Perfects,BoastWinners,Volleys);
    }
    public static class PersonalRecords
    {
        public const string LongestRallyKey="sc.records.longestRally",BestStreakKey="sc.records.bestStreak",StreakKey="sc.records.streak",
            BestMarginKey="sc.records.bestMargin",FastestWinKey="sc.records.fastestWin",WinsKeyPrefix="sc.records.wins.";
        public static readonly string[] LevelKeys={"easy","medium","hard"};
        // DEV switch (default on). Off: nothing is written and the card shows no line. Not remembered.
        public static bool Enabled=true;
        public static string[] AllKeys()
        {
            var keys=new List<string>{LongestRallyKey,BestStreakKey,StreakKey,BestMarginKey,FastestWinKey};
            foreach(var level in LevelKeys)keys.Add(WinsKeyPrefix+level);
            return keys.ToArray();
        }
        public static int LongestRally=>PlayerPrefs.GetInt(LongestRallyKey,0);
        public static int BestStreak=>PlayerPrefs.GetInt(BestStreakKey,0);
        public static int Streak=>PlayerPrefs.GetInt(StreakKey,0);
        public static int BestMargin=>PlayerPrefs.GetInt(BestMarginKey,0);
        // Seconds of the fastest win; 0 while there is none.
        public static float FastestWin=>PlayerPrefs.GetFloat(FastestWinKey,0f);
        public static int Wins(AiLevel level)=>PlayerPrefs.GetInt(WinsKeyPrefix+LevelKeys[(int)level],0);
        public static void Clear(){foreach(var key in AllKeys())PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();}
        public static string StreakText(int wins)=>wins==1?"1 victoria":wins+" victorias";
        public static string PointsText(int points)=>points==1?"1 punto":points+" puntos";
        // Writes the finished match and returns one line per record it broke ("Rally más largo: 23 golpes").
        public static List<string> Apply(MatchSummary match)
        {
            var broken=new List<string>();
            if(!Enabled)return broken;
            if(match.LongestRally>LongestRally)
            {
                PlayerPrefs.SetInt(LongestRallyKey,match.LongestRally);broken.Add("Rally más largo: "+RallyHud.StrokesText(match.LongestRally));
            }
            if(match.HumanWon)
            {
                int streak=Streak+1,bestStreak=BestStreak;
                PlayerPrefs.SetInt(StreakKey,streak);
                if(streak>bestStreak)
                {
                    PlayerPrefs.SetInt(BestStreakKey,streak);
                    // A first win is a streak of one: not worth a celebration.
                    if(streak>=2)broken.Add("Mejor racha: "+StreakText(streak));
                }
                var winsKey=WinsKeyPrefix+LevelKeys[(int)match.Level];PlayerPrefs.SetInt(winsKey,PlayerPrefs.GetInt(winsKey,0)+1);
                int margin=match.Human-match.Rival;
                if(margin>BestMargin){PlayerPrefs.SetInt(BestMarginKey,margin);broken.Add("Mayor diferencia: "+PointsText(margin));}
                float fastest=FastestWin;
                if(match.Seconds>0&&(fastest<=0||match.Seconds<fastest)){PlayerPrefs.SetFloat(FastestWinKey,match.Seconds);broken.Add("Victoria más rápida: "+RallyHud.DurationText(match.Seconds));}
            }
            else PlayerPrefs.SetInt(StreakKey,0);
            PlayerPrefs.Save();
            return broken;
        }
        public static string CardLine(List<string> broken)=>broken==null||broken.Count==0?"":"¡Nuevo récord! "+string.Join(" · ",broken);
    }
}
