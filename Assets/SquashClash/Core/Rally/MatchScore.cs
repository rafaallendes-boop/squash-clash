using System;
namespace SquashClash.Core.Rally
{
    // Right box: the one beside the right wall seen from behind the court (x near 0). The first serve of a hand
    // is from the right by default (spec 8.5).
    public enum ServiceBox { Right, Left }
    // Point-a-rally game to a target with a margin (spec 5.4: to 7, by 2, no cap) and serve rotation (spec 8.5).
    public sealed class MatchScore
    {
        private readonly int target,margin;
        public int Human {get;private set;}
        public int Rival {get;private set;}
        public PlayerId Server {get;private set;}
        public ServiceBox Box {get;private set;}
        public int PointsPlayed {get;private set;}
        public bool Finished=>Math.Max(Human,Rival)>=target&&Math.Abs(Human-Rival)>=margin;
        public PlayerId? Winner=>!Finished?(PlayerId?)null:Human>Rival?PlayerId.Human:PlayerId.Rival;
        public MatchScore(int target,int margin,PlayerId firstServer)
        {
            if(target<1||margin<1)throw new ArgumentOutOfRangeException(nameof(target));
            this.target=target;this.margin=margin;Server=firstServer;Box=ServiceBox.Right;
        }
        // False (and ignored) once the game is over: the score never changes after the end.
        public bool PointWon(PlayerId winner)
        {
            if(Finished)return false;
            if(winner==PlayerId.Human)Human++;else Rival++;
            PointsPlayed++;
            // The winner serves: the same server changes box, a new server starts from the right.
            if(winner==Server)Box=Box==ServiceBox.Right?ServiceBox.Left:ServiceBox.Right;
            else{Server=winner;Box=ServiceBox.Right;}
            return true;
        }
    }
}
