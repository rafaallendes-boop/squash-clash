namespace SquashClash.Core.Rally
{
    // One-line HUD texts (R5). Pure strings: the scene only shows them.
    public static class RallyText
    {
        public static string Player(PlayerId who)=>who==PlayerId.Human?"Tú":"Rival";
        // Rival styles (design 2026-11-19): shown at the start of a match and on the scoreboard.
        public static string RivalIntro(RivalStyleId style)=>"Rival: "+RivalStyle.Name(style);
        // «vs» plate (front 4, 22 nov rev 2): the first serve's flight is introduced in at most 1.5 s (in 0.18, the banner's own fade-in; hold 1.10; out 0.20).
        // Under «RIVAL» goes the style's own name: no invented names.
        public const double VsInSeconds=.18,VsHoldSeconds=1.10,VsOutSeconds=.20;
        public static string VsStyle(RivalStyleId style)=>RivalStyle.Name(style);
        private static string Score(int human,int rival)=>"Tú "+human+" – "+rival+" Rival";
        public static string Live(int human,int rival,int shots,PlayerId toHit)=>Score(human,rival)+" · golpes "+shots+" · turno: "+Player(toHit);
        public static string Point(PlayerId? winner,PointEndCause cause,ReceptionResult? reception,int shots,int human,int rival)
            =>(winner.HasValue?"Punto "+Player(winner.Value):"Sin punto")+": "+Cause(cause,reception)+" · "+shots+(shots==1?" golpe":" golpes")+" · "+Score(human,rival);
        // Serve kinds (design 2026-11-20-serve): the touch in the pause before the human's own serve.
        public const string ServeHint="Toca el frontis: arriba globo, abajo drive";
        public static string Box(ServiceBox box)=>box==ServiceBox.Right?"der.":"izq.";
        // Match line while a point is played: score, who serves and from where, strokes and turn.
        public static string LiveMatch(int human,int rival,int shots,PlayerId toHit,PlayerId server,ServiceBox box)
            =>Score(human,rival)+" · saca "+Player(server)+" ("+Box(box)+") · golpes "+shots+" · turno: "+Player(toHit);
        public static string MatchOver(PlayerId winner,int human,int rival)=>"Partido para "+Player(winner)+": "+Score(human,rival)+" · Revancha para jugar otro";
        public static string Cause(PointEndCause cause,ReceptionResult? reception)
        {
            switch(cause)
            {
                case PointEndCause.DoubleBounce:return "doble bote"+(reception.HasValue?" ("+Reception(reception.Value)+")":"");
                case PointEndCause.Tin:return "tin";
                case PointEndCause.Out:return "fuera";
                case PointEndCause.NotUp:return "no llegó al frontis";
                case PointEndCause.LaunchFault:return "lanzamiento nulo";
                case PointEndCause.ServiceFault:return "saque fallado";
                default:return "tope de golpes";
            }
        }
        private static string Reception(ReceptionResult result)
        {
            switch(result)
            {
                case ReceptionResult.Air:return "temprano";
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
