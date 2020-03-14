namespace Untitled.Sexp.Formatting
{
    /// <summary>
    /// Specify how characters escaped in string and symbol.
    /// </summary>
    public enum EscapingStyle
    {
        /// <summary>
        /// Escaping style is unspecified.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Chararcter will be escape in R7RS '\x0000;' style.
        /// </summary>
        XStyle,

        /// <summary>
        /// Chararcter will be escape in Racket '\u0000' and '\U00000000' style.
        /// </summary>
        UStyle,
    }
}
