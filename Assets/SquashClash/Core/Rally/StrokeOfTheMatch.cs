namespace SquashClash.Core.Rally
{
    // The player's best stroke of a match for the result card (spec 2026-11-25). Pure tally: the view feeds it each resolved stroke and each
    // finished point. A two-wall winner is a point the player won whose last applied stroke was a two-wall one.
    public sealed class StrokeOfTheMatch
    {
        public const int MinCount=3;
        public int Perfects {get;private set;}
        public int BoastWinners {get;private set;}
        public int Volleys {get;private set;}
        private bool pendingBoast;
        public void Reset(){Perfects=BoastWinners=Volleys=0;pendingBoast=false;}
        // applied: the stroke was Perfect or Valid and went out. Anything else leaves nothing pending.
        public void OnStroke(bool applied,bool perfect,bool volley,bool boast)
        {
            if(!applied){pendingBoast=false;return;}
            if(perfect)Perfects++;
            if(volley)Volleys++;
            pendingBoast=boast;
        }
        public void OnPointEnded(bool humanWon){if(humanWon&&pendingBoast)BoastWinners++;pendingBoast=false;}
        // The first kind, in this order, that reaches MinCount; "" when none does.
        public static string Line(int perfects,int boastWinners,int volleys)
        {
            if(perfects>=MinCount)return "Golpe del partido: "+perfects+" Perfectos";
            if(boastWinners>=MinCount)return "Golpe del partido: "+boastWinners+" dos paredes ganadores";
            if(volleys>=MinCount)return "Golpe del partido: "+volleys+" voleas";
            return "";
        }
        public string Line()=>Line(Perfects,BoastWinners,Volleys);
    }
}
