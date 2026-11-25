namespace SquashClash.Core.Rally
{
    // STUB (TDD red).
    public sealed class StrokeOfTheMatch
    {
        public const int MinCount=3;
        public int Perfects {get;private set;}
        public int BoastWinners {get;private set;}
        public int Volleys {get;private set;}
        public void Reset(){}
        public void OnStroke(bool applied,bool perfect,bool volley,bool boast){}
        public void OnPointEnded(bool humanWon){}
        public static string Line(int perfects,int boastWinners,int volleys)=>"";
        public string Line()=>"";
    }
}
