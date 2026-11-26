using System.Collections.Generic;
namespace SquashClash.Core.Rally
{
    // Simple referee (spec 2026-11-26): the call between points as text. Server's score first, like the WSF referee.
    public static class RefereeCall
    {
        public const string Tin="¡Tin!",MatchBall="Match ball";
        public static string Score(int server,int receiver)=>server+"–"+receiver;
        public static string Score(MatchScore m)=>m.Server==PlayerId.Human?Score(m.Human,m.Rival):Score(m.Rival,m.Human);
        public static string Call(MatchScore m,bool tin)
        {
            var parts=new List<string>(3);
            if(tin)parts.Add(Tin);
            if(m.MatchBall)parts.Add(MatchBall);
            parts.Add(Score(m));
            return string.Join(" · ",parts);
        }
    }
}
