namespace SquashClash.Core.Rally
{
    // One-line HUD texts (R5). Pure strings: the scene only shows them.
    public static class RallyText
    {
        public static string Player(PlayerId who)=>who==PlayerId.Human?"Tú":"Rival";
        private static string Score(int human,int rival)=>"Tú "+human+" – "+rival+" Rival";
        public static string Live(int human,int rival,int shots,PlayerId toHit)=>Score(human,rival)+" · golpes "+shots+" · turno: "+Player(toHit);
        public static string Point(PlayerId? winner,PointEndCause cause,ReceptionResult? reception,int shots,int human,int rival)
            =>(winner.HasValue?"Punto "+Player(winner.Value):"Sin punto")+": "+Cause(cause,reception)+" · "+shots+(shots==1?" golpe":" golpes")+" · "+Score(human,rival);
        public static string Cause(PointEndCause cause,ReceptionResult? reception)
        {
            switch(cause)
            {
                case PointEndCause.DoubleBounce:return "doble bote"+(reception.HasValue?" ("+Reception(reception.Value)+")":"");
                case PointEndCause.Tin:return "tin";
                case PointEndCause.Out:return "fuera";
                case PointEndCause.NotUp:return "no llegó al frontis";
                case PointEndCause.LaunchFault:return "lanzamiento nulo";
                default:return "tope de golpes";
            }
        }
        private static string Reception(ReceptionResult result)
        {
            switch(result)
            {
                case ReceptionResult.Air:return "al aire";
                case ReceptionResult.Late:return "tarde";
                case ReceptionResult.Unreachable:return "no llega";
                case ReceptionResult.NoSwing:return "sin golpe";
                case ReceptionResult.SystemFault:return "falla del sistema";
                case ReceptionResult.NotEvaluable:return "no evaluable";
                default:return "contacto";
            }
        }
    }
}
