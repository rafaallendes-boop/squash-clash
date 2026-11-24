namespace SquashClash.Runtime.Shot
{
    // Single source of the reception rule text (24 nov), in two halves, one per ring. The tutorial (step 4) and the first-match hint (H1)
    // show Full; the caption on the white ring shows White, word for word.
    public static class ReceptionRule
    {
        public const string Amber="Anillo ámbar: toca y volea en el aire.";
        public const string White="Anillo blanco: espera el bote y golpea.";
        public const string Full=Amber+" "+White;
    }
}
